(function () {
  let canvas, ctx, container;
  let drawing = false;
  let strokeStyle = "#0f6fd7";
  let lineWidth = 4;
  const history = [];
  let redoStack = [];
  let initialized = false;

  document.addEventListener("DOMContentLoaded", () => {
    window.OZGE.subscribe(() => refresh());
    document.addEventListener("ozge-draw-action", (event) => handleAction(event.detail.action));
  });

  function refresh() {
    const cls = window.OZGE.getClassState();
    const stage = document.getElementById("mode-stage");
    if (!stage) return;
    if (cls.mode !== "DRAW") {
      if (container) container.style.display = "none";
      return;
    }
    if (!initialized) {
      renderDrawUI(stage);
      initialized = true;
    }
    container.style.display = "block";
    populateWords();
    updateProjectorWord();
  }

  function renderDrawUI(stage) {
    container = document.createElement("div");
    container.className = "canvas-container";
    container.innerHTML = `
      <div class="canvas-toolbar">
        <button data-tool="pen" class="ghost">Pen</button>
        <button data-tool="eraser" class="ghost">Eraser</button>
        <button data-tool="undo" class="ghost">Undo</button>
        <button data-tool="redo" class="ghost">Redo</button>
        <button data-tool="clear" class="ghost">Clear</button>
        <button data-tool="save" class="primary">Save</button>
        <select id="word-picker"></select>
      </div>
      <canvas class="draw-surface" width="800" height="400" aria-label="Drawing canvas"></canvas>
    `;
    stage.innerHTML = "";
    stage.appendChild(container);
    canvas = container.querySelector("canvas");
    ctx = canvas.getContext("2d");
    ctx.lineCap = "round";
    bindCanvas();
    container.querySelectorAll("[data-tool]").forEach((btn) => btn.addEventListener("click", () => handleAction(btn.dataset.tool)));
    document.getElementById("word-picker").addEventListener("change", () => updateProjectorWord());
  }

  function populateWords() {
    const select = document.getElementById("word-picker");
    if (!select) return;
    const cls = window.OZGE.getClassState();
    const words = cls.activeUnits.flatMap((unit) => unit.vocabulary.map((item) => item.word));
    select.innerHTML = words.map((word) => `<option value="${word}">${word}</option>`).join("");
  }

  function bindCanvas() {
    const pointerDown = (event) => {
      drawing = true;
      ctx.beginPath();
      const pos = getPos(event);
      ctx.moveTo(pos.x, pos.y);
      history.push(canvas.toDataURL());
      redoStack = [];
    };
    const pointerMove = (event) => {
      if (!drawing) return;
      const pos = getPos(event);
      ctx.strokeStyle = strokeStyle;
      ctx.lineWidth = lineWidth;
      ctx.lineTo(pos.x, pos.y);
      ctx.stroke();
    };
    const pointerUp = () => {
      drawing = false;
    };
    canvas.addEventListener("pointerdown", pointerDown);
    canvas.addEventListener("pointermove", pointerMove);
    canvas.addEventListener("pointerup", pointerUp);
    canvas.addEventListener("pointerleave", pointerUp);
  }

  function getPos(event) {
    const rect = canvas.getBoundingClientRect();
    const x = (event.clientX - rect.left) * (canvas.width / rect.width);
    const y = (event.clientY - rect.top) * (canvas.height / rect.height);
    return { x, y };
  }

  function handleAction(action) {
    switch (action) {
      case "pen": strokeStyle = "#0f6fd7"; lineWidth = Math.min(12, lineWidth); break;
      case "eraser": strokeStyle = "#ffffff"; lineWidth = 20; break;
      case "undo": undo(); break;
      case "redo": redo(); break;
      case "clear": clearCanvas(); break;
      case "save": saveDrawing(); break;
      case "next": cycleWord(1); break;
      case "prev": cycleWord(-1); break;
      case "thicker": lineWidth = Math.min(24, lineWidth + 2); break;
      case "thinner": lineWidth = Math.max(2, lineWidth - 2); break;
    }
  }

  function undo() {
    const last = history.pop();
    if (!last) return;
    redoStack.push(canvas.toDataURL());
    redraw(last);
  }

  function redo() {
    const next = redoStack.pop();
    if (!next) return;
    history.push(canvas.toDataURL());
    redraw(next);
  }

  function redraw(dataUrl) {
    const img = new Image();
    img.onload = () => ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
    img.src = dataUrl;
  }

  function clearCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
  }

  function saveDrawing() {
    const cls = window.OZGE.getClassState();
    const word = document.getElementById("word-picker").value;
    window.OZGE.updateClass(cls.id, (current) => ({ drawGallery: [...current.drawGallery, { word, data: canvas.toDataURL(), time: Date.now() }] }));
    alert("Drawing saved to gallery");
  }

  function cycleWord(direction) {
    const select = document.getElementById("word-picker");
    if (!select) return;
    const next = (select.selectedIndex + direction + select.options.length) % select.options.length;
    select.selectedIndex = next;
    updateProjectorWord();
  }

  function updateProjectorWord() {
    const word = document.getElementById("word-picker")?.value || "";
    window.OZGE.updateModePayload({ word });
  }
})();
