(function () {
  const DEFAULT_STOPWORDS = [
    "the",
    "and",
    "for",
    "with",
    "this",
    "that",
    "have",
    "from",
    "your",
    "about",
    "into",
    "after",
    "before",
    "because",
    "once",
    "where",
    "while",
    "which",
    "should",
    "would",
    "could"
  ];

  let stopwordList = [];
  let stopwordLoaded = false;
  let viewInitialized = false;
  let currentUnits = [];

  function $(selector, root = document) {
    return root.querySelector(selector);
  }

  function createView(container) {
    container.innerHTML = `
      <section class="content-import">
        <h2>Content Manager</h2>
        <p>Import PDFs or images. Files stay on this device.</p>
        <label class="ghost">
          <input type="file" id="content-input" accept="application/pdf,image/*" multiple hidden>
          <span>Choose Files</span>
        </label>
        <div id="content-drop" class="drop-zone" aria-label="Drop files here">Drop files here</div>
      </section>
      <section>
        <h3>Image Previews &amp; Crop</h3>
        <div id="content-preview" class="preview-grid"></div>
      </section>
      <section>
        <h3>Detected Units</h3>
        <div id="unit-table"></div>
      </section>
      <section>
        <h3>Unit → Mode Mapping</h3>
        <div id="unit-mapping"></div>
      </section>
      <section class="pack-actions">
        <button id="save-pack" class="primary">Save Unit Pack</button>
        <input id="load-pack" type="file" accept="application/json" hidden>
        <button id="load-pack-btn" class="ghost">Load Unit Pack</button>
      </section>
      <section>
        <h3>Stored Files</h3>
        <p class="caption">Imported PDFs and images are saved to this device for offline reuse.</p>
        <div id="stored-files" class="stored-files" aria-live="polite"></div>
      </section>
    `;

    const input = $("#content-input", container);
    const drop = $("#content-drop", container);
    const loadBtn = $("#load-pack-btn", container);
    const loadInput = $("#load-pack", container);

    input.addEventListener("change", async (event) => {
      const files = Array.from(event.target.files || []);
      await processFiles(files);
    });

    ;["dragenter", "dragover"].forEach((type) => {
      drop.addEventListener(type, (event) => {
        event.preventDefault();
        drop.classList.add("dragging");
      });
    });
    ;["dragleave", "drop"].forEach((type) => {
      drop.addEventListener(type, (event) => {
        event.preventDefault();
        drop.classList.remove("dragging");
      });
    });
    drop.addEventListener("drop", async (event) => {
      const files = Array.from(event.dataTransfer.files || []);
      await processFiles(files);
    });

    $("#save-pack", container).addEventListener("click", savePack);
    loadBtn.addEventListener("click", () => loadInput.click());
    loadInput.addEventListener("change", importPack);

    viewInitialized = true;
  }

  function render(container, cls) {
    if (!viewInitialized) {
      createView(container);
    }
    currentUnits = cls.units.slice();
    renderUnitTable(cls);
    renderMapping(cls);
    renderStoredFiles(cls);
  }

  async function ensureStopwords() {
    if (stopwordLoaded) return stopwordList;
    try {
      const response = await fetch("data/stopwords.json");
      if (!response.ok) throw new Error("stopwords fetch failed");
      stopwordList = await response.json();
    } catch (error) {
      console.warn("Falling back to default stopwords", error);
      stopwordList = DEFAULT_STOPWORDS.slice();
    }
    stopwordLoaded = true;
    return stopwordList;
  }

  async function processFiles(files) {
    if (!files.length) return;
    await ensureStopwords();
    const cls = State.getActiveClass();
    for (const file of files) {
      let text = "";
      if (file.type && file.type.startsWith('image/')) {
        text = await handleImageFile(file);
      } else {
        text = await OCR.run(file);
      }
      await ingestText(text);
      try {
        await ContentDB.storeImportedFile(cls.id, file, {
          textPreview: (text || "").slice(0, 280)
        });
      } catch (error) {
        console.warn("Failed to store file", error);
      }
    }
    cls.units = currentUnits;
    State.persist();
    renderUnitTable(cls);
    renderMapping(cls);
    renderStoredFiles(cls);
    Sync.emit("units", { classId: cls.id, units: currentUnits });
  }

  async function ingestText(text) {
    if (!text) return;
    await ensureStopwords();
    const units = parseUnits(text, stopwordList);
    mergeUnits(units);
  }

  function parseUnits(text, stopwords) {
    const blocks = text.split(/\n\s*/g);
    const units = [];
    let active = null;
    blocks.forEach((block) => {
      const clean = block.trim();
      if (!clean) return;
      if (/^(unit|lesson)\s+\d+/i.test(clean)) {
        if (active) {
          units.push(active);
        }
        active = {
          id: crypto.randomUUID(),
          title: clean,
          words: [],
          raw: ""
        };
        return;
      }
      if (!active) {
        active = {
          id: crypto.randomUUID(),
          title: "Unit",
          words: [],
          raw: ""
        };
      }
      active.raw += clean + "\n";
      const words = clean
        .toLowerCase()
        .replace(/[^a-z\s]/g, " ")
        .split(/\s+/)
        .filter((w) => w.length > 2 && !stopwords.includes(w));
      const freq = countFrequency(words);
      Object.entries(freq).forEach(([term, count]) => {
        if (!active.words.some((item) => item.term === term)) {
          active.words.push({ term, count });
        } else {
          const existing = active.words.find((item) => item.term === term);
          existing.count += count;
        }
      });
    });
    if (active) units.push(active);
    return units.map((u) => ({
      id: u.id,
      title: u.title,
      words: u.words
        .sort((a, b) => b.count - a.count)
        .slice(0, 15)
        .map(({ term, count }) => ({ term, definition: `Common classroom word (${count})` })),
      summary: createSummary(u.raw),
      mcqs: createMCQs(u.words)
    }));
  }

  async function handleImageFile(file) {
    const preview = document.getElementById("content-preview");
    if (!preview) return OCR.run(file);
    const wrapper = document.createElement("article");
    wrapper.className = "preview-card";
    const title = document.createElement("header");
    title.textContent = file.name;
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");
    const controls = document.createElement("div");
    controls.className = "crop-controls";
    const topRange = document.createElement("input");
    topRange.type = "range";
    topRange.min = 0;
    topRange.max = 40;
    topRange.value = 0;
    const bottomRange = document.createElement("input");
    bottomRange.type = "range";
    bottomRange.min = 0;
    bottomRange.max = 40;
    bottomRange.value = 0;
    const applyBtn = document.createElement("button");
    applyBtn.textContent = "Apply Crop OCR";
    applyBtn.className = "ghost";
    const output = document.createElement("pre");
    output.className = "ocr-output";
    const topLabel = document.createElement("label");
    topLabel.textContent = "Top";
    topLabel.appendChild(topRange);
    const bottomLabel = document.createElement("label");
    bottomLabel.textContent = "Bottom";
    bottomLabel.appendChild(bottomRange);
    controls.appendChild(topLabel);
    controls.appendChild(bottomLabel);
    controls.appendChild(applyBtn);
    wrapper.appendChild(title);
    wrapper.appendChild(canvas);
    wrapper.appendChild(controls);
    wrapper.appendChild(output);
    preview.appendChild(wrapper);

    const bitmap = await createImageBitmap(file);
    const scale = Math.min(400 / bitmap.width, 1);
    canvas.width = Math.round(bitmap.width * scale);
    canvas.height = Math.round(bitmap.height * scale);

    function drawOverlay() {
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(bitmap, 0, 0, canvas.width, canvas.height);
      const top = (Number(topRange.value) / 100) * canvas.height;
      const bottom = (Number(bottomRange.value) / 100) * canvas.height;
      ctx.fillStyle = "rgba(0,0,0,0.3)";
      if (top > 0) ctx.fillRect(0, 0, canvas.width, top);
      if (bottom > 0) ctx.fillRect(0, canvas.height - bottom, canvas.width, bottom);
    }

    topRange.addEventListener("input", drawOverlay);
    bottomRange.addEventListener("input", drawOverlay);

    async function runCrop() {
      const topPercent = Number(topRange.value) / 100;
      const bottomPercent = Number(bottomRange.value) / 100;
      const cropHeight = bitmap.height * (1 - topPercent - bottomPercent);
      const offsetY = bitmap.height * topPercent;
      const off = document.createElement("canvas");
      off.width = bitmap.width;
      off.height = Math.max(1, Math.round(cropHeight));
      const offCtx = off.getContext("2d");
      offCtx.drawImage(bitmap, 0, offsetY, bitmap.width, cropHeight, 0, 0, off.width, off.height);
      const blob = await new Promise((resolve) => off.toBlob(resolve, file.type || "image/png"));
      const croppedFile = new File([blob], file.name, { type: file.type || "image/png" });
      const text = await OCR.run(croppedFile);
      output.textContent = text || "(no text detected)";
      return text;
    }

    applyBtn.addEventListener("click", async () => {
      const text = await runCrop();
      await ingestText(text);
      const cls = State.getActiveClass();
      cls.units = currentUnits;
      State.persist();
      renderUnitTable(cls);
      renderMapping(cls);
    });

    drawOverlay();
    const initialText = await runCrop();
    return initialText;
  }

  function countFrequency(words) {
    return words.reduce((acc, word) => {
      acc[word] = (acc[word] || 0) + 1;
      return acc;
    }, {});
  }

  function createSummary(raw) {
    if (!raw) return ["Text not available"];
    const sentences = raw.split(/[.!?]/).map((s) => s.trim()).filter(Boolean);
    return sentences.slice(0, 3);
  }

  function difficultyFromWord(word) {
    if (word.length <= 4) return "easy";
    if (word.length <= 7) return "medium";
    return "hard";
  }

  function createMCQs(words) {
    const sorted = words.sort((a, b) => b.count - a.count).slice(0, 8);
    const mcqs = [];
    for (let i = 0; i < Math.min(5, sorted.length); i++) {
      const correct = sorted[i];
      const distractors = sorted
        .filter((item) => item.term !== correct.term)
        .slice(0, 3)
        .map((item) => item.term);
      const options = shuffle([correct.term, ...distractors]);
      mcqs.push({
        question: `Choose the word related to ${correct.term}`,
        options,
        answer: options.indexOf(correct.term),
        difficulty: difficultyFromWord(correct.term)
      });
    }
    return mcqs;
  }

  function shuffle(list) {
    const arr = list.slice();
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  function mergeUnits(units) {
    units.forEach((unit) => {
      const existing = currentUnits.find((u) => u.title === unit.title);
      if (existing) {
        existing.words = mergeWordLists(existing.words, unit.words);
        existing.summary = unit.summary;
        existing.mcqs = unit.mcqs;
      } else {
        currentUnits.push({
          ...unit,
          modeMap: ["QUIZ", "PUZZLE", "SPEAK", "STORY", "DRAW"]
        });
      }
    });
  }

  function mergeWordLists(a, b) {
    const map = new Map();
    a.forEach((item) => map.set(item.term, item));
    b.forEach((item) => {
      if (map.has(item.term)) {
        map.get(item.term).definition = item.definition;
      } else {
        map.set(item.term, item);
      }
    });
    return Array.from(map.values());
  }

  function renderUnitTable(cls) {
    const table = document.getElementById("unit-table");
    if (!table) return;
    table.innerHTML = "";
    if (!currentUnits.length) {
      table.innerHTML = `<p>No units detected yet.</p>`;
      return;
    }
    currentUnits.forEach((unit) => {
      const card = document.createElement("article");
      card.className = "unit-card";
      card.innerHTML = `
        <header>
          <h4 contenteditable="true" data-field="title">${unit.title}</h4>
          <button class="ghost" data-action="remove">Remove</button>
        </header>
        <p>${unit.summary.join(" • ")}</p>
        <ul class="word-list">
          ${unit.words.map((w) => `<li>${w.term} <span>${w.definition}</span></li>`).join("")}
        </ul>
      `;
      card.querySelector('[data-field="title"]').addEventListener("blur", (event) => {
        unit.title = event.target.textContent.trim();
        State.upsertUnit(unit);
      });
      card.querySelector('[data-action="remove"]').addEventListener("click", () => {
        currentUnits = currentUnits.filter((item) => item.id !== unit.id);
        const cls = State.getActiveClass();
        cls.units = currentUnits;
        State.persist();
        renderUnitTable(cls);
        renderMapping(cls);
      });
      table.appendChild(card);
    });
  }

  function renderMapping(cls) {
    const container = document.getElementById("unit-mapping");
    if (!container) return;
    container.innerHTML = "";
    const modes = ["QUIZ", "PUZZLE", "SPEAK", "STORY", "DRAW"];
    modes.forEach((mode) => {
      const section = document.createElement("section");
      section.className = "mode-chip-row";
      const title = document.createElement("h4");
      title.textContent = mode;
      section.appendChild(title);
      const row = document.createElement("div");
      row.className = "chip-row";
      currentUnits.forEach((unit) => {
        const chip = document.createElement("button");
        chip.className = "chip";
        chip.textContent = unit.title;
        const selected = (cls.unitSelections[mode] || []).includes(unit.id);
        chip.classList.toggle("active", selected);
        chip.addEventListener("click", () => {
          const list = cls.unitSelections[mode] || [];
          const idx = list.indexOf(unit.id);
          if (idx >= 0) {
            list.splice(idx, 1);
          } else {
            list.push(unit.id);
          }
          State.setUnitSelection(mode, list);
          renderMapping(cls);
        });
        row.appendChild(chip);
      });
      section.appendChild(row);
      container.appendChild(section);
    });
  }

  function savePack() {
    const cls = State.getActiveClass();
    const pack = {
      classId: cls.id,
      generatedAt: new Date().toISOString(),
      units: currentUnits
    };
    const blob = new Blob([JSON.stringify(pack, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `${cls.id}-unit-pack.json`;
    a.click();
    URL.revokeObjectURL(url);
  }

  async function importPack(event) {
    const file = event.target.files?.[0];
    if (!file) return;
    try {
      const json = JSON.parse(await file.text());
      currentUnits = json.units || [];
      const cls = State.getActiveClass();
      cls.units = currentUnits;
      State.persist();
      renderUnitTable(cls);
      renderMapping(cls);
    } catch (err) {
      alert("Unable to load pack");
    }
  }

  async function renderStoredFiles(cls) {
    const container = document.getElementById("stored-files");
    if (!container || !window.ContentDB) return;
    container.textContent = "Loading files…";
    try {
      const files = await ContentDB.listFiles(cls.id);
      if (!files.length) {
        container.textContent = "No stored files yet. Import PDFs or images to keep them here.";
        return;
      }
      container.innerHTML = "";
      files.forEach((file) => {
        container.appendChild(createFileCard(file));
      });
    } catch (error) {
      console.warn("Unable to list stored files", error);
      container.textContent = "Storage unavailable in this browser.";
    }
  }

  function createFileCard(file) {
    const article = document.createElement("article");
    article.className = "stored-file";
    const header = document.createElement("header");
    const title = document.createElement("h4");
    title.textContent = file.name;
    header.appendChild(title);
    const meta = document.createElement("p");
    meta.className = "meta";
    const sizeKb = Math.max(1, Math.round(file.size / 1024));
    const date = new Date(file.addedAt);
    meta.textContent = `${sizeKb} KB • ${date.toLocaleString()}`;
    const preview = document.createElement("p");
    preview.className = "preview";
    preview.textContent = file.textPreview ? `${file.textPreview.trim()}…` : "(No text preview captured)";
    const actions = document.createElement("div");
    actions.className = "actions";
    const download = document.createElement("button");
    download.className = "ghost";
    download.textContent = "Download";
    download.addEventListener("click", () => downloadFile(file.id, file.name));
    const remove = document.createElement("button");
    remove.className = "danger";
    remove.textContent = "Delete";
    remove.addEventListener("click", async () => {
      if (!confirm(`Remove ${file.name} from this device?`)) return;
      await ContentDB.deleteFile(file.id);
      const cls = State.getActiveClass();
      renderStoredFiles(cls);
    });
    actions.appendChild(download);
    actions.appendChild(remove);
    article.appendChild(header);
    article.appendChild(meta);
    article.appendChild(preview);
    article.appendChild(actions);
    return article;
  }

  async function downloadFile(id, name) {
    try {
      const record = await ContentDB.getFile(id);
      if (!record || !record.blob) {
        alert("File not found in storage.");
        return;
      }
      const url = URL.createObjectURL(record.blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = name;
      a.click();
      setTimeout(() => URL.revokeObjectURL(url), 2000);
    } catch (error) {
      console.warn("Download failed", error);
      alert("Could not download the stored file. Try again later.");
    }
  }

  window.ContentManager = {
    render
  };
})();
