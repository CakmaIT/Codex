(function () {
  let container = null;
  let interval = null;
  let timeLeft = 0;
  let currentQuestion = null;
  let streak = 0;

  function render(view, cls) {
    container = view;
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Bonus Blitz</h2>
          <p>Rapid-fire questions. Double points for correct answers!</p>
        </div>
        <button id="bonus-start" class="primary">Start Blitz</button>
      </header>
      <section class="bonus-body">
        <div class="bonus-status">Time left: <span data-field="time">0</span>s • Streak: <span data-field="streak">0</span></div>
        <div class="bonus-question" data-field="question">Press start to begin.</div>
        <div class="bonus-options" data-field="options"></div>
        <div class="bonus-group">
          <label for="bonus-group-select">Answering group</label>
          <select id="bonus-group-select"></select>
        </div>
      </section>
    `;
    container.querySelector('#bonus-start').addEventListener('click', () => start(cls));
    populateGroupSelect(cls);
  }

  function populateGroupSelect(cls) {
    const select = container.querySelector('#bonus-group-select');
    select.innerHTML = '';
    Object.keys(cls.roster.groups).forEach((group) => {
      const option = document.createElement('option');
      option.value = group;
      option.textContent = group;
      if (group === cls.current.activeGroup) option.selected = true;
      select.appendChild(option);
    });
  }

  function start(cls) {
    clearInterval(interval);
    timeLeft = (State.store.settings.bonusDuration || 30);
    streak = 0;
    updateTime();
    nextQuestion(cls);
    interval = setInterval(() => {
      timeLeft -= 1;
      updateTime();
      if (timeLeft <= 0) {
        clearInterval(interval);
        container.querySelector('[data-field="question"]').textContent = 'Bonus round finished!';
        container.querySelector('[data-field="options"]').innerHTML = '';
      }
    }, 1000);
  }

  function updateTime() {
    const timeEl = container.querySelector('[data-field="time"]');
    if (timeEl) timeEl.textContent = timeLeft;
    const streakEl = container.querySelector('[data-field="streak"]');
    if (streakEl) streakEl.textContent = streak;
  }

  function nextQuestion(cls) {
    const pool = buildQuestions(cls);
    currentQuestion = pool[Math.floor(Math.random() * pool.length)];
    const questionEl = container.querySelector('[data-field="question"]');
    const optionsEl = container.querySelector('[data-field="options"]');
    questionEl.textContent = currentQuestion.prompt;
    optionsEl.innerHTML = '';
    currentQuestion.options.forEach((option, index) => {
      const button = document.createElement('button');
      button.textContent = option;
      button.addEventListener('click', () => handleAnswer(index, cls));
      optionsEl.appendChild(button);
    });
  }

  function handleAnswer(index, cls) {
    const correct = index === currentQuestion.answer;
    if (correct) {
      streak += 1;
      const group = container.querySelector('#bonus-group-select').value;
      awardPoints(cls, group, 20 + streak * 2);
      nextQuestion(cls);
    } else {
      streak = 0;
      updateTime();
    }
  }

  function buildQuestions(cls) {
    const units = cls.units;
    if (!units.length) {
      return [
        { prompt: 'Spell the word for a writing tool.', options: ['pencil', 'glue', 'scissors'], answer: 0 }
      ];
    }
    const questions = [];
    units.forEach((unit) => {
      (unit.words || []).forEach((word) => {
        const options = shuffle([word.term, ...randomWords(units, 2, word.term)]);
        questions.push({
          prompt: `Which word fits: ${word.definition}?`,
          options,
          answer: Math.max(0, options.indexOf(word.term))
        });
      });
    });
    return questions.slice(0, 60);
  }

  function randomWords(units, count, exclude) {
    const pool = [];
    units.forEach((unit) => {
      unit.words.forEach((word) => {
        if (word.term !== exclude) pool.push(word.term);
      });
    });
    return shuffle(pool).slice(0, count);
  }

  function awardPoints(cls, group, points) {
    const g = cls.roster.groups[group];
    if (g) {
      g.score += points;
      State.persist();
      UI.renderGroups(document.getElementById('group-list'), cls);
      Sync.emit('score', { classId: cls.id, group, score: g.score });
    }
  }

  function shuffle(list) {
    const arr = list.slice();
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  window.Bonus = {
    render
  };
})();
