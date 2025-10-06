(function () {
  let containerRef = null;

  function render(container, cls) {
    containerRef = container;
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Story Panels</h2>
          <p>Use the mini comic prompts for reading and comprehension.</p>
        </div>
        <button id="story-new" class="primary">New Story</button>
      </header>
      <section class="story-body">
        <div class="story-panels" aria-live="polite"></div>
        <div class="story-question"></div>
        <div class="story-options"></div>
      </section>
    `;
    container.querySelector('#story-new').addEventListener('click', () => loadStory(cls));
    loadStory(cls);
  }

  function loadStory(cls) {
    const pool = buildStoryPool(cls);
    const story = pool[Math.floor(Math.random() * pool.length)];
    const panels = containerRef.querySelector('.story-panels');
    const questionEl = containerRef.querySelector('.story-question');
    const optionsEl = containerRef.querySelector('.story-options');
    panels.innerHTML = story.panels.map((text, index) => `<article class="story-panel"><span>${index + 1}</span><p>${text}</p></article>`).join('');
    questionEl.textContent = story.question;
    optionsEl.innerHTML = '';
    story.options.forEach((option, index) => {
      const button = document.createElement('button');
      button.textContent = option;
      button.addEventListener('click', () => {
        const correct = index === story.answer;
        if (correct) {
          awardPoints(cls, 8);
        }
        alert(correct ? 'Correct! +8 points' : 'Try again next time.');
      });
      optionsEl.appendChild(button);
    });
  }

  function buildStoryPool(cls) {
    const selected = cls.unitSelections.STORY;
    const units = cls.units.filter((unit) => !selected.length || selected.includes(unit.id));
    if (!units.length) {
      return [
        {
          panels: ['The classroom is quiet.', 'The teacher writes on the board.', 'Students raise their hands.'],
          question: 'What are the students doing?',
          options: ['Sleeping', 'Raising their hands', 'Leaving'],
          answer: 1
        }
      ];
    }
    return units.map((unit) => {
      const words = unit.words.slice(0, 4).map((w) => w.term);
      if (!words.length) {
        words.push('classroom');
      }
      const options = shuffle(words);
      return {
        panels: [
          `${unit.title} begins with a warm-up activity.`,
          `Students practice words like ${words.join(', ')}.`,
          `Everyone shares a sentence using the new words.`
        ],
        question: `What is one of the focus words in ${unit.title}?`,
        options,
        answer: Math.max(0, options.indexOf(words[0]))
      };
    });
  }

  function awardPoints(cls, points) {
    const group = cls.current.activeGroup;
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

  window.Story = {
    render
  };
})();
