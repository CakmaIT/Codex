(function () {
  const bodyRole = document.body.dataset.role || 'teacher';
  let projectorWindow = null;
  let autoBonusTimer = null;
  let calmOverlay = null;

  function initTeacher() {
    UI.toggleHeader(false);
    document.getElementById('enter-app').addEventListener('click', enterDashboard);
    document.getElementById('start-projector').addEventListener('click', openProjector);
    document.getElementById('emergency-stop').addEventListener('click', emergencyStop);
    setupTabs();
    setupHomeActions();
    setupBehaviorControls();
    setupPinDialog();
    setInterval(updateTimer, 1000);
  }

  function initProjector() {
    UI.setProjectorSection('projector-home');
    Sync.on(handleProjectorMessage);
    const snapshot = Sync.loadSnapshot();
    if (snapshot) {
      applySnapshot(snapshot);
    }
  }

  function enterDashboard() {
    UI.toggleHeader(true);
    document.getElementById('splash').classList.remove('view-active');
    document.getElementById('splash').setAttribute('aria-hidden', 'true');
    document.getElementById('main').focus();
    State.ready.then(() => {
      const cls = State.getActiveClass();
      if (!cls.lastPenalty) cls.lastPenalty = Date.now();
      updateActiveClass(cls.id);
      renderActiveTab('HOME');
      scheduleAutoBonus(cls);
    });
  }

  function updateActiveClass(id) {
    const header = document.querySelector('.active-class strong');
    if (header) header.textContent = id;
    document.querySelectorAll('.class-chip').forEach((chip) => {
      chip.classList.toggle('active', chip.dataset.class === id);
      chip.addEventListener('click', () => {
        State.setActiveClass(chip.dataset.class);
        const cls = State.getActiveClass();
        UI.renderHome(cls);
        renderActiveTab(currentTab);
        Sync.emit('class-change', { id: cls.id });
        scheduleAutoBonus(cls);
      });
    });
    const cls = State.getActiveClass();
    if (!cls.current.unitId && cls.units.length) {
      cls.current.unitId = cls.units[0].id;
    }
    UI.renderHome(cls);
    UI.renderProjectorHome(cls);
  }

  let currentTab = 'HOME';

  function setupTabs() {
    document.querySelectorAll('#teacher-tabs .tab').forEach((tab) => {
      tab.addEventListener('click', () => {
        const next = tab.dataset.tab;
        renderActiveTab(next);
      });
    });
  }

  function renderActiveTab(tabId) {
    currentTab = tabId;
    UI.setActiveTab(tabId);
    const cls = State.getActiveClass();
    switch (tabId) {
      case 'HOME':
        UI.renderHome(cls);
        break;
      case 'QUIZ':
        Quiz.render(document.getElementById('view-QUIZ'), cls);
        break;
      case 'PUZZLE':
        Puzzle.render(document.getElementById('view-PUZZLE'), cls);
        break;
      case 'SPEAK':
        renderSpeak(cls);
        break;
      case 'STORY':
        Story.render(document.getElementById('view-STORY'), cls);
        break;
      case 'DRAW':
        Draw.render(document.getElementById('view-DRAW'), cls);
        break;
      case 'BONUS':
        Bonus.render(document.getElementById('view-BONUS'), cls);
        break;
      case 'CONTENT_MANAGER':
        ContentManager.render(document.getElementById('view-CONTENT_MANAGER'), cls);
        break;
      case 'ANALYTICS':
        Analytics.render(document.getElementById('view-ANALYTICS'), cls);
        break;
      case 'LOG':
        renderLog(cls);
        break;
      case 'SETTINGS':
        renderSettings();
        break;
      case 'CLASS':
        renderClassAdmin(cls);
        break;
    }
    cls.current.mode = tabId;
    if (['HOME', 'QUIZ', 'PUZZLE', 'SPEAK', 'STORY', 'DRAW', 'BONUS', 'RESULT'].includes(tabId)) {
      Sync.emit('mode-change', { mode: tabId });
    }
    Sync.saveSnapshot(buildSnapshot());
  }

  function renderSpeak(cls) {
    const container = document.getElementById('view-SPEAK');
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Speaking Spotlight</h2>
          <p>Prompt students to repeat and score automatically.</p>
        </div>
        <button id="speak-start" class="primary">Start Practice</button>
      </header>
      <section class="speak-body">
        <label>
          Sentence prompt
          <input id="speak-prompt" type="text" value="Please describe your classroom." />
        </label>
        <div class="speak-status">
          <span>Speech engine: ${Speaking.available() ? 'Web Speech' : 'Simulated'}</span>
          <span>Score: <strong data-field="score">0</strong></span>
        </div>
        <div class="speak-wave" aria-hidden="true"></div>
        <button id="speak-award" class="ghost">Award +5</button>
      </section>
    `;
    container.querySelector('#speak-start').addEventListener('click', () => startSpeaking(cls));
    container.querySelector('#speak-award').addEventListener('click', () => awardSpeaking(cls));
  }

  function startSpeaking(cls) {
    const prompt = document.getElementById('speak-prompt').value;
    const scoreEl = document.querySelector('[data-field="score"]');
    Speaking.startListening((result) => {
      const score = result.score;
      scoreEl.textContent = score;
      State.addLessonLog({ mode: 'SPEAK', score, prompt });
      if (score > 70) {
        awardPoints(cls.current.activeGroup, 6);
      }
    }, (error) => {
      console.warn('speech error', error);
    });
  }

  function awardSpeaking(cls) {
    awardPoints(cls.current.activeGroup, 5);
    alert('Bonus points added!');
  }

  function awardPoints(group, points) {
    const cls = State.getActiveClass();
    const g = cls.roster.groups[group];
    if (g) {
      g.score += points;
      State.persist();
      UI.renderGroups(document.getElementById('group-list'), cls);
      UI.renderProjectorHome(cls);
      Sync.emit('score', { classId: cls.id, group, score: g.score });
    }
  }

  function renderLog(cls) {
    const container = document.getElementById('view-LOG');
    container.innerHTML = `
      <header class="mode-header">
        <div>
          <h2>Lesson Log</h2>
          <p>Auto-tracked entries plus manual notes.</p>
        </div>
        <button id="log-export" class="ghost">Export JSON</button>
      </header>
      <section>
        <textarea id="log-note" rows="4" placeholder="Add note"></textarea>
        <button id="log-add" class="primary">Add Note</button>
      </section>
      <section class="log-entries"></section>
    `;
    container.querySelector('#log-add').addEventListener('click', () => {
      const text = document.getElementById('log-note').value.trim();
      if (!text) return;
      State.addLessonLog({ mode: 'NOTE', note: text });
      document.getElementById('log-note').value = '';
      renderLog(State.getActiveClass());
    });
    container.querySelector('#log-export').addEventListener('click', () => {
      const blob = new Blob([JSON.stringify(cls.lessonLog, null, 2)], { type: 'application/json' });
      download(blob, `${cls.id}-lesson-log.json`);
    });
    const list = container.querySelector('.log-entries');
    list.innerHTML = cls.lessonLog.slice().reverse().map((entry) => `
      <article class="log-entry">
        <header>${new Date(entry.timestamp || Date.now()).toLocaleString()} – ${entry.mode}</header>
        <pre>${JSON.stringify(entry, null, 2)}</pre>
      </article>
    `).join('');
  }

  function renderSettings() {
    const container = document.getElementById('view-SETTINGS');
    const settings = State.store.settings;
    container.innerHTML = `
      <h2>Settings</h2>
      <label>Change PIN <input id="settings-pin" type="password" value="${settings.pin}" maxlength="4"></label>
      <label>Bonus Duration (seconds) <input id="settings-bonus" type="number" value="${settings.bonusDuration}"></label>
      <label>Auto Bonus Minutes <input id="settings-auto" type="number" value="${settings.autoBonusMinutes}"></label>
      <button id="settings-save" class="primary">Save Settings</button>
    `;
    container.querySelector('#settings-save').addEventListener('click', () => {
      settings.pin = document.getElementById('settings-pin').value || State.DEFAULT_PIN;
      settings.bonusDuration = Number(document.getElementById('settings-bonus').value) || 30;
      settings.autoBonusMinutes = Number(document.getElementById('settings-auto').value) || 10;
      State.persist();
      alert('Settings saved');
    });
  }

  function renderClassAdmin(cls) {
    const container = document.getElementById('view-CLASS');
    container.innerHTML = `
      <h2>Class Administration (${cls.id})</h2>
      <section>
        <h3>Roster Import</h3>
        <textarea id="class-roster" rows="4" placeholder="CSV: Group, Name"></textarea>
        <button id="class-import" class="primary">Import Roster</button>
      </section>
      <section>
        <h3>Schedule</h3>
        <ul>${(cls.schedule || []).map((item) => `<li>${item}</li>`).join('')}</ul>
        <input id="class-schedule" placeholder="e.g., Monday 09:00">
        <button id="class-add-schedule" class="ghost">Add Time</button>
      </section>
      <section>
        <h3>Attendance</h3>
        <button id="class-start-session" class="primary">Start Session</button>
      </section>
    `;
    container.querySelector('#class-import').addEventListener('click', () => importRoster(cls));
    container.querySelector('#class-add-schedule').addEventListener('click', () => {
      const value = document.getElementById('class-schedule').value;
      if (!value) return;
      cls.schedule.push(value);
      State.persist();
      renderClassAdmin(cls);
    });
    container.querySelector('#class-start-session').addEventListener('click', () => {
      const now = new Date().toLocaleString();
      State.addLessonLog({ mode: 'SESSION', startedAt: now });
      alert('Session started!');
    });
  }

  function importRoster(cls) {
    const text = document.getElementById('class-roster').value.trim();
    if (!text) return;
    text.split(/\n/).forEach((line) => {
      const [group, name] = line.split(',').map((part) => part.trim());
      if (!group || !name) return;
      if (!cls.roster.groups[group]) {
        cls.roster.groups[group] = { name: `Group ${group}`, avatar: '😀', score: 0, behavior: 'Balanced' };
      }
      const list = cls.roster.groups[group].members || [];
      list.push(name);
      cls.roster.groups[group].members = list;
    });
    State.persist();
    UI.renderGroups(document.getElementById('group-list'), cls);
    alert('Roster imported');
  }

  function setupHomeActions() {
    document.querySelectorAll('.home-card').forEach((card) => {
      card.addEventListener('click', () => {
        renderActiveTab(card.dataset.launch);
        State.getActiveClass().current.mode = card.dataset.launch;
        Sync.emit('mode-change', { mode: card.dataset.launch });
      });
    });
  }

  function setupBehaviorControls() {
    document.getElementById('group-list').addEventListener('click', async (event) => {
      const action = event.target.dataset.action;
      if (!action) {
        const card = event.target.closest('.group-card');
        if (card) {
          const cls = State.getActiveClass();
          cls.current.activeGroup = card.dataset.group;
          State.persist();
          Sync.emit('group-change', { group: cls.current.activeGroup });
        }
        return;
      }
      const card = event.target.closest('.group-card');
      const group = card.dataset.group;
      if (['penalty', 'attendance'].includes(action)) {
        const ok = await requirePin();
        if (!ok) return;
      }
      const cls = State.getActiveClass();
      if (action === 'attendance') {
        const present = prompt('Mark attendance (comma separated)');
        if (present) {
          cls.attendanceHistory.push({ date: new Date().toISOString(), group, present: present.split(',').map((p) => p.trim()) });
          State.persist();
          alert('Attendance recorded');
        }
        return;
      }
      const value = action === 'positive' ? 5 : action === 'warning' ? 0 : -10;
      State.recordBehavior(group, action, value);
      UI.renderGroups(document.getElementById('group-list'), cls);
      UI.renderProjectorHome(cls);
      Sync.emit('behavior', { group, action, value });
      if (action === 'penalty') {
        cls.lastPenalty = Date.now();
        scheduleAutoBonus(cls);
      }
    });
  }

  function setupPinDialog() {
    const dialog = document.getElementById('pin-dialog');
    if (dialog) {
      dialog.setAttribute('role', 'dialog');
    }
  }

  async function requirePin() {
    const input = await UI.showPinDialog();
    return input === State.store.settings.pin;
  }

  function openProjector() {
    projectorWindow = window.open('projector.html', 'ozge-projector', 'width=1280,height=720');
    Sync.emit('snapshot', buildSnapshot());
  }

  function buildSnapshot() {
    const cls = State.getActiveClass();
    return {
      classId: cls.id,
      mode: currentTab,
      scores: cls.roster.groups,
      unit: cls.current.unitId,
      timerStart: cls.current.timerStart,
      activeGroup: cls.current.activeGroup
    };
  }

  function updateTimer() {
    const cls = State.getActiveClass();
    UI.renderTimer(cls);
  }

  function emergencyStop() {
    State.store.projectorApproval = false;
    State.store.blackout = true;
    calmDown(120);
    Sync.emit('emergency', {});
  }

  function calmDown(duration) {
    if (calmOverlay) calmOverlay.remove();
    calmOverlay = document.createElement('div');
    calmOverlay.className = 'calm-overlay';
    calmOverlay.innerHTML = '<p>Calm Down Mode</p><p>Take deep breaths…</p>';
    document.body.appendChild(calmOverlay);
    setTimeout(() => {
      if (calmOverlay) calmOverlay.remove();
      calmOverlay = null;
    }, duration * 1000);
  }

  function scheduleAutoBonus(cls) {
    if (autoBonusTimer) clearInterval(autoBonusTimer);
    autoBonusTimer = setInterval(() => {
      const minutes = State.store.settings.autoBonusMinutes;
      const lastPenalty = cls.lastPenalty || 0;
      if (Date.now() - lastPenalty > minutes * 60000) {
        clearInterval(autoBonusTimer);
        if (confirm('⭐ Team Spirit Bonus! Start bonus game?')) {
          renderActiveTab('BONUS');
        }
      }
    }, 60000);
  }

  function download(blob, filename) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  function handleProjectorMessage(message) {
    switch (message.type) {
      case 'snapshot':
        applySnapshot(message.payload);
        break;
      case 'score':
        updateProjectorScores(message.payload);
        break;
      case 'mode-change':
        showMode(message.payload.mode);
        break;
      case 'behavior':
        displayBehavior(message.payload);
        break;
      case 'draw-approval':
        approveDrawing(message.payload);
        break;
      case 'group-change':
        setActiveGroupDisplay(message.payload.group);
        break;
      case 'emergency':
        blackout();
        break;
    }
  }

  function applySnapshot(snapshot) {
    if (!snapshot) return;
    document.querySelector('.projector-status [data-field="class"]').textContent = `Class ${snapshot.classId}`;
    setActiveGroupDisplay(snapshot.activeGroup);
    UI.renderProjectorHome(State.ensureClass(snapshot.classId));
    showMode(snapshot.mode || 'HOME');
  }

  function setActiveGroupDisplay(group) {
    const el = document.querySelector('.projector-status [data-field="group"]');
    if (el) {
      el.textContent = `Group ${group}`;
    }
  }

  function updateProjectorScores(payload) {
    const scores = document.getElementById('projector-scores');
    if (!scores) return;
    let span = scores.querySelector(`[data-group="${payload.group}"]`);
    if (!span) {
      span = document.createElement('span');
      span.dataset.group = payload.group;
      span.className = 'projector-score';
      scores.appendChild(span);
    }
    span.textContent = `${payload.group}: ${payload.score}`;
  }

  function showMode(mode) {
    if (mode && mode !== 'HOME') {
      UI.setProjectorSection('projector-content');
      document.querySelector('#projector-content [data-field="mode"]').textContent = mode;
      const body = document.getElementById('projector-content-body');
      if (body) {
        body.innerHTML = `<p class="projector-instruction">Stay focused! The teacher is guiding the <strong>${mode}</strong> activity.</p>`;
      }
    } else {
      UI.setProjectorSection('projector-home');
    }
  }

  function displayBehavior(payload) {
    const banner = document.createElement('div');
    banner.className = 'projector-banner';
    banner.textContent = `${payload.group} ${payload.action === 'positive' ? '👍' : payload.action === 'penalty' ? '⚠️' : '⚠️'}`;
    document.body.appendChild(banner);
    setTimeout(() => banner.remove(), 3000);
  }

  function approveDrawing(payload) {
    UI.setProjectorSection('projector-approval');
    const body = document.querySelector('#projector-approval .projector-await');
    body.innerHTML = `<img src="${payload.dataUrl}" alt="Approved drawing" style="max-width:90%;border-radius:20px;">`;
  }

  function blackout() {
    UI.setProjectorSection('projector-blackout');
  }

  if (bodyRole === 'teacher') {
    initTeacher();
    State.ready.then(() => updateActiveClass(State.getActiveClass().id));
  } else {
    initProjector();
  }
})();
