(function () {
  let canvas, ctx, drawing = false;
  let strokeColor = '#0f1a2a';
  let strokeWidth = 4;
  let history = [];
  let redoStack = [];
  let wordIndex = 0;
  let words = [];

  function render(container, cls) {
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Draw &amp; Spell</h2>
          <p>Sketch the word while students spell it aloud.</p>
        </div>
        <div class="draw-words">
          <button id="draw-prev" aria-label="Previous word">Prev</button>
          <span id="draw-current"></span>
          <button id="draw-next" aria-label="Next word">Next</button>
          <button id="draw-shuffle" class="ghost">Shuffle</button>
        </div>
      </header>
      <section>
        <div class="draw-toolbar" role="toolbar" aria-label="Drawing controls">
          <div class="color-row">
            ${['#0f1a2a', '#3c50ff', '#f58b4c', '#4ad66d', '#ffffff'].map((color) => `<button class="color-swatch" data-color="${color}" style="background:${color}"></button>`).join('')}
          </div>
          <select id="draw-size" aria-label="Brush size">
            <option value="3">Fine</option>
            <option value="6" selected>Medium</option>
            <option value="10">Bold</option>
          </select>
          <button id="draw-undo">Undo</button>
          <button id="draw-redo">Redo</button>
          <button id="draw-clear" class="ghost">Clear</button>
          <button id="draw-save" class="primary">Save Snapshot</button>
          <button id="draw-approve" class="ghost">Approve to Projector</button>
        </div>
        <canvas id="draw-canvas" class="draw-canvas" aria-label="Drawing canvas"></canvas>
      </section>
      <section>
        <h3>Gallery</h3>
        <div id="draw-gallery" class="gallery-grid"></div>
      </section>
    `;

    canvas = container.querySelector('#draw-canvas');
    ctx = canvas.getContext('2d');
    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);
    setupDrawing();

    container.querySelectorAll('.color-swatch').forEach((button) => {
      button.addEventListener('click', () => {
        strokeColor = button.dataset.color;
        container.querySelectorAll('.color-swatch').forEach((b) => b.classList.toggle('active', b === button));
      });
    });
    container.querySelector('#draw-size').addEventListener('change', (event) => {
      strokeWidth = Number(event.target.value);
    });
    container.querySelector('#draw-undo').addEventListener('click', undo);
    container.querySelector('#draw-redo').addEventListener('click', redo);
    container.querySelector('#draw-clear').addEventListener('click', () => confirmAction(() => clearCanvas()));
    container.querySelector('#draw-save').addEventListener('click', () => saveSnapshot(cls));
    container.querySelector('#draw-approve').addEventListener('click', () => approveDisplay(cls));
    container.querySelector('#draw-prev').addEventListener('click', () => changeWord(-1, cls));
    container.querySelector('#draw-next').addEventListener('click', () => changeWord(1, cls));
    container.querySelector('#draw-shuffle').addEventListener('click', () => shuffleWords(cls));

    initWordList(cls);
  }

  function initWordList(cls) {
    const selected = cls.unitSelections.DRAW;
    const pool = [];
    cls.units.forEach((unit) => {
      if (!selected.length || selected.includes(unit.id)) {
        unit.words.forEach((word) => pool.push(word.term));
      }
    });
    words = pool.length ? pool : ['pencil', 'classroom', 'learn'];
    wordIndex = 0;
    updateCurrentWord();
  }

  function updateCurrentWord() {
    const current = document.getElementById('draw-current');
    if (current) {
      current.textContent = words[wordIndex % words.length];
    }
  }

  function changeWord(delta, cls) {
    wordIndex = (wordIndex + delta + words.length) % words.length;
    updateCurrentWord();
    clearCanvas();
  }

  function shuffleWords(cls) {
    words = shuffle(words);
    wordIndex = 0;
    updateCurrentWord();
    clearCanvas();
  }

  function resizeCanvas() {
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * window.devicePixelRatio;
    canvas.height = rect.height * window.devicePixelRatio;
    ctx.scale(window.devicePixelRatio, window.devicePixelRatio);
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = strokeWidth;
  }

  function setupDrawing() {
    const start = (event) => {
      drawing = true;
      redoStack = [];
      ctx.beginPath();
      const { x, y } = position(event);
      ctx.moveTo(x, y);
      history.push(canvas.toDataURL());
    };
    const move = (event) => {
      if (!drawing) return;
      const { x, y } = position(event);
      ctx.strokeStyle = strokeColor;
      ctx.lineWidth = strokeWidth;
      ctx.lineTo(x, y);
      ctx.stroke();
    };
    const end = () => {
      drawing = false;
      ctx.closePath();
    };
    canvas.addEventListener('mousedown', start);
    canvas.addEventListener('mousemove', move);
    window.addEventListener('mouseup', end);
    canvas.addEventListener('touchstart', (event) => {
      event.preventDefault();
      start(event.touches[0]);
    });
    canvas.addEventListener('touchmove', (event) => {
      event.preventDefault();
      move(event.touches[0]);
    });
    canvas.addEventListener('touchend', end);
  }

  function position(event) {
    const rect = canvas.getBoundingClientRect();
    return {
      x: (event.clientX - rect.left),
      y: (event.clientY - rect.top)
    };
  }

  function undo() {
    if (!history.length) return;
    const last = history.pop();
    redoStack.push(canvas.toDataURL());
    restoreFromDataUrl(last);
  }

  function redo() {
    if (!redoStack.length) return;
    const next = redoStack.pop();
    history.push(canvas.toDataURL());
    restoreFromDataUrl(next);
  }

  function restoreFromDataUrl(dataUrl) {
    const img = new Image();
    img.onload = () => {
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(img, 0, 0);
    };
    img.src = dataUrl;
  }

  function clearCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    history = [];
    redoStack = [];
  }

  function confirmAction(action) {
    if (confirm('Are you sure?')) action();
  }

  function saveSnapshot(cls) {
    const dataUrl = canvas.toDataURL('image/png');
    const gallery = document.getElementById('draw-gallery');
    const img = document.createElement('img');
    img.src = dataUrl;
    img.alt = `Drawing of ${words[wordIndex]}`;
    gallery.appendChild(img);
    State.addSnapshot({ type: 'draw', word: words[wordIndex], dataUrl });
  }

  async function approveDisplay(cls) {
    const valid = await requirePin();
    if (!valid) return;
    Sync.emit('draw-approval', { classId: cls.id, dataUrl: canvas.toDataURL('image/png') });
  }

  async function requirePin() {
    const input = await UI.showPinDialog();
    const pin = State.store.settings.pin;
    if (input === pin) return true;
    alert('Incorrect PIN');
    return false;
  }

  function shuffle(array) {
    const arr = array.slice();
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  window.Draw = {
    render,
    shuffle
  };
})();
