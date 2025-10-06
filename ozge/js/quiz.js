(function () {
  let view = null;
  let state = {
    questions: [],
    index: 0,
    timer: null,
    endTime: null,
    selectedUnitIds: [],
    correct: 0,
    missedWords: []
  };

  function render(container, cls) {
    view = container;
    if (!view) return;
    view.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Quiz Mode</h2>
          <p>Timed multiple-choice questions with instant feedback.</p>
        </div>
        <div class="status-pill">
          <span>Progress</span>
          <span data-field="progress">0 / 0</span>
        </div>
      </header>
      <section class="quiz-body">
        <div class="quiz-question" data-field="question">Select "Start Quiz" to begin.</div>
        <div class="quiz-options" data-field="options"></div>
        <div class="quiz-actions">
          <button id="quiz-start" class="primary">Start Quiz</button>
          <button id="quiz-skip" class="ghost">Skip</button>
        </div>
      </section>
    `;
    view.querySelector('#quiz-start').addEventListener('click', () => startQuiz(cls));
    view.querySelector('#quiz-skip').addEventListener('click', () => nextQuestion(cls));
    loadQuestions(cls);
  }

  function loadQuestions(cls) {
    const selected = cls.unitSelections.QUIZ;
    const units = cls.units.filter((unit) => !selected.length || selected.includes(unit.id));
    const questions = [];
    units.forEach((unit) => {
      (unit.mcqs || []).forEach((mcq) => {
        questions.push({ ...mcq, unitId: unit.id, unitTitle: unit.title });
      });
    });
    state.questions = shuffle(questions).slice(0, 10);
    state.index = 0;
    updateProgress();
  }

  function startQuiz(cls) {
    if (!state.questions.length) {
      alert('No questions available. Add units in Content Manager.');
      return;
    }
    state.index = 0;
    state.endTime = Date.now() + 60000;
    state.correct = 0;
    state.missedWords = [];
    renderQuestion(cls);
    tickTimer(cls);
  }

  function renderQuestion(cls) {
    const question = state.questions[state.index];
    const questionEl = view.querySelector('[data-field="question"]');
    const optionsEl = view.querySelector('[data-field="options"]');
    if (!question) {
      questionEl.textContent = 'Quiz complete!';
      optionsEl.innerHTML = '';
      emitProjectorQuestion(null, cls);
      State.addLessonLog({
        mode: 'QUIZ',
        correct: state.correct,
        total: state.questions.length,
        unitId: cls.current.unitId,
        missedWords: state.missedWords
      });
      return;
    }
    questionEl.textContent = `${question.unitTitle}: ${question.question}`;
    optionsEl.innerHTML = '';
    question.options.forEach((option, index) => {
      const button = document.createElement('button');
      button.textContent = option;
      button.addEventListener('click', () => handleAnswer(cls, index));
      optionsEl.appendChild(button);
    });
    emitProjectorQuestion({
      unitTitle: question.unitTitle,
      prompt: question.question,
      index: state.index + 1,
      total: state.questions.length
    }, cls);
    updateProgress();
  }

  function handleAnswer(cls, index) {
    const question = state.questions[state.index];
    if (!question) return;
    const correct = index === question.answer;
    if (correct) {
      state.correct += 1;
      awardPoints(cls.current.activeGroup, 10);
    } else {
      state.missedWords.push(question.options[question.answer]);
    }
    nextQuestion(cls);
  }

  function nextQuestion(cls) {
    if (state.index < state.questions.length - 1) {
      state.index += 1;
      renderQuestion(cls);
    } else {
      state.index = state.questions.length;
      renderQuestion(cls);
    }
  }

  function awardPoints(group, points) {
    const cls = State.getActiveClass();
    const g = cls.roster.groups[group];
    if (g) {
      g.score += points;
      State.persist();
      UI.renderGroups(document.getElementById('group-list'), cls);
      Sync.emit('score', { classId: cls.id, group, score: g.score });
    }
  }

  function updateProgress() {
    if (!view) return;
    const display = view.querySelector('[data-field="progress"]');
    if (display) {
      display.textContent = `${Math.min(state.index + 1, state.questions.length)} / ${state.questions.length}`;
    }
  }

  function tickTimer(cls) {
    if (!state.endTime) return;
    const remaining = Math.max(0, state.endTime - Date.now());
    cls.current.timerStart = Date.now() - (60000 - remaining);
    UI.renderTimer(cls);
    if (remaining <= 0) {
      state.endTime = null;
      renderQuestion(cls);
      return;
    }
    requestAnimationFrame(() => tickTimer(cls));
  }

  function shuffle(list) {
    const arr = list.slice();
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  function emitProjectorQuestion(payload, cls) {
    Sync.emit('quiz-question', {
      classId: cls.id,
      question: payload
    });
  }

  window.Quiz = {
    render
  };
})();
