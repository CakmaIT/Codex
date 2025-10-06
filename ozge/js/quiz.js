(function () {
  let initialized = false;
  let currentQuestion = null;

  document.addEventListener("DOMContentLoaded", () => {
    window.OZGE.subscribe(render);
  });

  function render() {
    const cls = window.OZGE.getClassState();
    const stage = document.getElementById("mode-stage");
    if (!stage || cls.mode !== "QUIZ") return;
    if (!initialized) {
      initialized = true;
    }
    currentQuestion = buildQuestion(cls);
    stage.innerHTML = renderQuestion(currentQuestion, cls);
    stage.querySelectorAll(".quiz-options button").forEach((btn) => btn.addEventListener("click", () => selectAnswer(btn.dataset.answer === "true")));
    window.OZGE.updateModePayload({ question: currentQuestion.prompt, prompt: `Choose the meaning of ${currentQuestion.word}` });
  }

  function buildQuestion(cls) {
    const unit = cls.activeUnits[Math.floor(Math.random() * cls.activeUnits.length)] || {};
    const entry = (unit.vocabulary || [])[Math.floor(Math.random() * (unit.vocabulary?.length || 1))] || { word: "Word", definition: "Definition" };
    const distractors = (cls.activeUnits.flatMap((u) => u.vocabulary.map((v) => v.definition)).filter((def) => def !== entry.definition).sort(() => 0.5 - Math.random()).slice(0, 3)) || [];
    const answers = shuffle([entry.definition, ...distractors]).map((definition) => ({ definition, correct: definition === entry.definition }));
    return {
      word: entry.word,
      prompt: entry.definition,
      answers,
      unitTitle: unit.title || "Unit"
    };
  }

  function renderQuestion(question, cls) {
    return `
      <div class="quiz-card">
        <h3>${question.unitTitle} · Vocabulary Quiz</h3>
        <p>What does <strong>${question.word}</strong> mean?</p>
        <div class="quiz-options">
          ${question.answers.map((answer) => `<button data-answer="${answer.correct}">${answer.definition}</button>`).join('')}
        </div>
      </div>
    `;
  }

  function selectAnswer(correct) {
    const buttons = document.querySelectorAll(".quiz-options button");
    buttons.forEach((btn) => {
      const isCorrect = btn.dataset.answer === "true";
      btn.classList.add(isCorrect ? "correct" : "incorrect");
    });
    const group = window.OZGE.getClassState().activeGroup || "A";
    window.OZGE.adjustScore(group, correct ? 5 : -2, correct ? "Quiz correct" : "Quiz miss");
    window.OZGE.Analytics.record("QUIZ", { score: correct ? 100 : 40, missed: correct ? [] : [currentQuestion.word] });
    setTimeout(() => window.OZGE.setMode("RESULT", { summary: correct ? "Great answer!" : "Review this word", accuracy: correct ? 100 : 40 }), 1200);
  }

  function shuffle(array) {
    const copy = [...array];
    for (let i = copy.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
  }
})();
