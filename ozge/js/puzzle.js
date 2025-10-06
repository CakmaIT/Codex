(function () {
  let container = null;
  let currentWord = null;

  function render(view, cls) {
    container = view;
    if (!container) return;
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Puzzle Tiles</h2>
          <p>Drag the letters into place to spell the word.</p>
        </div>
        <button id="puzzle-new" class="primary">New Word</button>
      </header>
      <section class="puzzle-body">
        <div class="puzzle-target" aria-live="polite"></div>
        <div class="puzzle-tiles" aria-label="Letter tiles"></div>
        <div class="puzzle-feedback" data-field="feedback"></div>
      </section>
    `;
    container.querySelector('#puzzle-new').addEventListener('click', () => nextWord(cls));
    nextWord(cls);
  }

  function nextWord(cls) {
    const units = getWordPool(cls);
    if (!units.length) {
      container.querySelector('.puzzle-target').textContent = 'No words available.';
      container.querySelector('.puzzle-tiles').innerHTML = '';
      return;
    }
    const word = units[Math.floor(Math.random() * units.length)];
    currentWord = word;
    const target = container.querySelector('.puzzle-target');
    target.innerHTML = '<span class="puzzle-slot" data-slot></span>'.repeat(word.length);
    renderTiles(word.split(''));
    container.querySelector('[data-field="feedback"]').textContent = '';
  }

  function renderTiles(letters) {
    const area = container.querySelector('.puzzle-tiles');
    area.innerHTML = '';
    shuffle(letters).forEach((letter) => {
      const tile = document.createElement('button');
      tile.className = 'puzzle-tile';
      tile.textContent = letter.toUpperCase();
      tile.draggable = true;
      tile.addEventListener('dragstart', (event) => {
        event.dataTransfer.setData('text/plain', letter);
      });
      tile.addEventListener('click', () => placeLetter(letter));
      area.appendChild(tile);
    });
    container.querySelectorAll('[data-slot]').forEach((slot, index) => {
      slot.addEventListener('dragover', (event) => event.preventDefault());
      slot.addEventListener('drop', (event) => {
        event.preventDefault();
        placeLetter(event.dataTransfer.getData('text/plain'), index);
      });
      slot.addEventListener('click', () => placeLetterPrompt(index));
    });
  }

  function placeLetter(letter, index) {
    const slots = Array.from(container.querySelectorAll('[data-slot]'));
    if (typeof index !== 'number') {
      index = slots.findIndex((slot) => !slot.textContent);
    }
    if (index < 0 || index >= slots.length) return;
    const slot = slots[index];
    if (slot.textContent) return;
    slot.textContent = letter.toUpperCase();
    validate();
  }

  function placeLetterPrompt(index) {
    const letter = prompt('Type letter');
    if (letter) {
      placeLetter(letter[0], index);
    }
  }

  function validate() {
    const slots = Array.from(container.querySelectorAll('[data-slot]'));
    const attempt = slots.map((slot) => slot.textContent.toLowerCase()).join('');
    if (attempt.length !== currentWord.length) return;
    const feedback = container.querySelector('[data-field="feedback"]');
    if (attempt === currentWord) {
      feedback.textContent = 'Great job! +5 points';
      awardPoints(5);
      confetti();
    } else {
      feedback.textContent = 'Try again!';
    }
  }

  function awardPoints(points) {
    const cls = State.getActiveClass();
    const group = cls.current.activeGroup;
    const g = cls.roster.groups[group];
    if (g) {
      g.score += points;
      State.persist();
      UI.renderGroups(document.getElementById('group-list'), cls);
      Sync.emit('score', { classId: cls.id, group, score: g.score });
    }
  }

  function confetti() {
    const overlay = document.createElement('div');
    overlay.className = 'confetti';
    overlay.style.position = 'fixed';
    overlay.style.inset = '0';
    overlay.style.pointerEvents = 'none';
    overlay.style.background = 'repeating-linear-gradient(90deg, rgba(255,255,255,0), rgba(255,255,255,0) 10px, rgba(79,109,245,0.4) 10px, rgba(79,109,245,0.4) 20px)';
    document.body.appendChild(overlay);
    setTimeout(() => overlay.remove(), 600);
  }

  function getWordPool(cls) {
    const selected = cls.unitSelections.PUZZLE;
    const words = [];
    cls.units.forEach((unit) => {
      if (!selected.length || selected.includes(unit.id)) {
        unit.words.forEach((word) => words.push(word.term.toLowerCase()));
      }
    });
    return words.filter((w) => w.length >= 3).slice(0, 50);
  }

  function shuffle(list) {
    const arr = list.slice();
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  window.Puzzle = {
    render
  };
})();
