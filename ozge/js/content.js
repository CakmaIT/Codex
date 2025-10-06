(function () {
  const stopwords = new Set();
  fetch("data/stopwords.json").then((res) => res.json()).then((words) => words.forEach((w) => stopwords.add(w)));

  const ContentManager = {
    async ingestFile(file) {
      const rawText = await window.OZGE.OCR.extractText(file);
      if (!rawText) return [];
      const clean = normalize(rawText);
      const units = clusterUnits(clean);
      if (!units.length) {
        units.push(buildUnit("Imported", clean));
      }
      return units.map((unit) => ({ ...unit, sourceName: file.name }));
    }
  };

  function normalize(text) {
    return text.replace(/\r/g, " ").replace(/\n+/g, " \n ").replace(/[^A-Za-z0-9\n\s]/g, " ").toLowerCase();
  }

  function clusterUnits(text) {
    const lines = text.split(/\n/).map((line) => line.trim()).filter(Boolean);
    const units = [];
    let current = [];
    let currentTitle = "";
    lines.forEach((line) => {
      if (/\b(unit|lesson)\b/.test(line) && line.length < 80) {
        if (current.length) {
          units.push(buildUnit(currentTitle || `Unit ${units.length + 1}`, current.join(" ")));
        }
        current = [];
        currentTitle = capitalize(line);
      } else {
        current.push(line);
      }
    });
    if (current.length) {
      units.push(buildUnit(currentTitle || `Unit ${units.length + 1}`, current.join(" ")));
    }
    return units;
  }

  function buildUnit(title, text) {
    const words = text.split(/\s+/).filter(Boolean);
    const freq = {};
    words.forEach((word) => {
      if (stopwords.has(word) || word.length < 3) return;
      freq[word] = (freq[word] || 0) + 1;
    });
    const sorted = Object.entries(freq).sort((a, b) => b[1] - a[1]).slice(0, 10);
    const vocabulary = sorted.map(([word, count]) => ({
      word: capitalize(word),
      definition: simpleDefinition(word),
      difficulty: difficultyTag(count, word)
    }));
    const sentences = chunkSentences(text);
    const storyPanels = sentences.slice(0, 3).map(capitalize);
    const mcqBase = vocabulary.slice(0, 3);
    const mcqs = mcqBase.map((item) => ({
      question: `What best defines "${item.word}"?`,
      answer: item.definition,
      options: shuffle([item.definition, ...vocabulary.filter((v) => v.word !== item.word).slice(0, 3).map((v) => v.definition)])
    }));
    return {
      id: `${title.toLowerCase().replace(/[^a-z0-9]+/g, '-')}-${Date.now()}`,
      title: capitalize(title),
      vocabulary,
      sentences,
      story: {
        panels: storyPanels,
        question: `What happens in ${title}?`,
        options: storyPanels.slice(0, 4),
        answer: storyPanels[0] || ""
      },
      modeMap: { QUIZ: true, DRAW: true, SPEAK: true, STORY: true, PUZZLE: true, BONUS: true },
      mcqs
    };
  }

  function simpleDefinition(word) {
    return `Meaning of ${word}`;
  }

  function difficultyTag(count, word) {
    if (word.length > 8 || count <= 1) return "hard";
    if (count === 2) return "medium";
    return "easy";
  }

  function chunkSentences(text) {
    return text.split(/[.!?]/).map((s) => s.trim()).filter((s) => s.length > 0).map(capitalize);
  }

  function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  function shuffle(array) {
    const copy = [...array];
    for (let i = copy.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
  }

  const AILessonHelper = {
    generate(units) {
      return units.map((unit) => ({
        id: unit.id,
        title: unit.title,
        summary: buildSummary(unit),
        words: unit.vocabulary.map((word) => ({ ...word, synonyms: suggestSynonyms(word.word) })),
        mcqs: buildMCQs(unit)
      }));
    }
  };

  function buildSummary(unit) {
    const words = unit.vocabulary.slice(0, 3).map((w) => w.word).join(", ");
    return `Focus on ${words}. Practice with ${unit.sentences.length} speaking prompts.`;
  }

  function buildMCQs(unit) {
    if (unit.mcqs && unit.mcqs.length) return unit.mcqs;
    return unit.vocabulary.slice(0, 3).map((word) => ({
      question: `Choose the best meaning of ${word.word}`,
      options: shuffle([word.definition, ...unit.vocabulary.filter((w) => w.word !== word.word).slice(0, 3).map((w) => w.definition)]),
      answer: word.definition,
      difficulty: word.difficulty
    }));
  }

  function suggestSynonyms(word) {
    const endings = { ing: "doing", ed: "finished", ly: "manner" };
    const suffix = Object.keys(endings).find((suf) => word.endsWith(suf));
    return suffix ? [endings[suffix]] : [word + "er"];
  }

  window.OZGE = window.OZGE || {};
  window.OZGE.ContentManager = ContentManager;
  window.OZGE.AILessonHelper = AILessonHelper;
})();
