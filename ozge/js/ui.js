(function () {
  const { subscribe, setActiveClass, setMode, updateClass, updateBehaviorLog, adjustScore, CLASSES, DEFAULT_AVATARS } = window.OZGE;
  let unsubscribe = null;
  let projectorWin = null;
  let calmTimeout = null;
  const projectorChannelName = "ozge-sync";

  document.addEventListener("DOMContentLoaded", () => {
    const screen = document.body.dataset.screen;
    if (screen === "teacher") initTeacher();
    if (screen === "projector") initProjector();
  });

  function initTeacher() {
    bindTeacherControls();
    unsubscribe = subscribe(renderTeacher);
    const bc = new BroadcastChannel(projectorChannelName);
    bc.onmessage = (event) => {
      if (event.data === "projector-ready") {
        projectorWin = projectorWin || window.open("projector.html", "ozge-projector");
      }
    };
  }

  function initProjector() {
    unsubscribe = subscribe(renderProjector);
    const bc = new BroadcastChannel(projectorChannelName);
    bc.postMessage("projector-ready");
  }

  function bindTeacherControls() {
    const openProjector = document.getElementById("open-projector");
    if (openProjector) {
      openProjector.addEventListener("click", () => {
        projectorWin = window.open("projector.html", "ozge-projector", "noopener=yes,width=1280,height=720");
      });
    }

    const tabs = document.querySelectorAll(".tablist .tab");
    tabs.forEach((tab) => {
      tab.addEventListener("click", () => {
        selectTab(tab.dataset.tab);
      });
    });

    const modeButtons = document.querySelectorAll(".mode-btn");
    modeButtons.forEach((btn) => {
      btn.addEventListener("click", () => {
        setMode(btn.dataset.mode, { selectedMode: btn.dataset.mode });
        window.OZGE.pushLessonEntry({ mode: btn.dataset.mode, action: "start" });
      });
    });

    document.getElementById("toggle-freeze")?.addEventListener("click", () => {
      if (!requirePin()) return;
      showOverlay({ id: "freeze", title: "Inputs frozen", description: "All controls are locked." });
    });

    document.getElementById("emergency-stop")?.addEventListener("click", () => {
      if (!requirePin()) return;
      triggerEmergencyStop();
    });

    document.getElementById("calm-mode")?.addEventListener("click", () => {
      if (!requirePin()) return;
      startCalmDown(90);
    });

    document.querySelectorAll("[data-behavior]").forEach((btn) => {
      btn.addEventListener("click", () => {
        const group = getActiveGroup();
        if (!group) return;
        updateBehaviorLog(group, btn.dataset.behavior, `${btn.dataset.behavior.toUpperCase()} recorded`);
        if (btn.dataset.behavior === "good") {
          maybeTriggerAutoBonus();
        }
      });
    });

    document.getElementById("start-bonus")?.addEventListener("click", () => {
      const duration = Number(document.getElementById("bonus-duration").value) || 30;
      setMode("BONUS", { duration });
    });

    document.getElementById("auto-bonus")?.addEventListener("change", (event) => {
      updateClass(window.OZGE.state.activeClassId, (cls) => ({ settings: { ...cls.settings, autoBonus: event.target.checked } }));
    });
  }

  function renderTeacher(state) {
    const cls = state.classes[state.activeClassId];
    document.getElementById("active-class-label").textContent = `Class: ${cls.id}`;
    document.getElementById("status-topic").textContent = cls.topic || "--";
    document.getElementById("status-mode").textContent = cls.mode;
    document.getElementById("status-group").textContent = getActiveGroup() || "A";
    document.getElementById("status-timer").textContent = formatTimer(cls.lastModeChange);
    renderScoreboard(cls);
    renderGroups(cls);
    renderBehavior(cls);
    renderTabs(cls, state);
    syncProjector(state, cls);
    checkCalmDown(cls);
  }

  function renderProjector(state) {
    const cls = state.classes[state.activeClassId];
    document.getElementById("projector-class").textContent = `Class ${cls.id}`;
    document.getElementById("projector-topic").textContent = cls.topic;
    document.getElementById("projector-mode").textContent = cls.mode;
    document.getElementById("projector-timer").textContent = formatTimer(cls.lastModeChange);
    document.getElementById("projector-group-name").textContent = getActiveGroup() || "A";
    renderProjectorStage(cls);
    renderProjectorScores(cls);
    renderProjectorBehavior(cls);
    checkCalmDown(cls);
  }

  function renderProjectorStage(cls) {
    const stage = document.getElementById("projector-stage");
    if (!stage) return;
    const payload = cls.activePayload || {};
    switch (cls.mode) {
      case "QUIZ":
        stage.innerHTML = `<div class="quiz-card"><h2>${payload.question || "Quiz time"}</h2><p>${payload.prompt || "Answer together!"}</p></div>`;
        break;
      case "PUZZLE":
        stage.innerHTML = `<div class="puzzle-card"><h2>Puzzle Mode</h2><p>${payload.word || "Arrange the tiles"}</p></div>`;
        break;
      case "SPEAK":
        stage.innerHTML = `<div class="quiz-card"><h2>Speaking Practice</h2><p>${payload.sentence || "Speak clearly"}</p><div class="timer-display">${payload.score ?? "--"}</div></div>`;
        break;
      case "STORY":
        stage.innerHTML = `<div class="story-card"><p>${payload.panel || "Story time"}</p></div>`;
        break;
      case "DRAW":
        stage.innerHTML = `<div class="story-card"><h2>Draw & Spell</h2><p>${payload.word || "Waiting for approval"}</p></div>`;
        break;
      case "BONUS":
        stage.innerHTML = `<div class="quiz-card"><h2>Bonus Blitz!</h2><p>${payload.prompt || "Get ready for double points"}</p><div class="timer-display">${payload.remaining ?? payload.duration ?? 30}</div></div>`;
        break;
      case "RESULT":
        stage.innerHTML = `<div class="quiz-card"><h2>Results</h2><p>${payload.summary || "Great work today!"}</p></div>`;
        break;
      default:
        stage.innerHTML = `<div class="quiz-card"><h2>${cls.topic}</h2><p>Select a mode to begin.</p></div>`;
    }
  }

  function renderScoreboard(cls) {
    const container = document.querySelector(".scoreboard");
    if (!container) return;
    container.innerHTML = "";
    Object.entries(cls.behaviorScore).forEach(([group, score]) => {
      if (!cls.groups[group]) return;
      const card = document.createElement("div");
      card.className = "score-card";
      card.innerHTML = `<strong>${group}</strong><span>${score}</span>`;
      container.appendChild(card);
    });
  }

  function renderGroups(cls) {
    const list = document.getElementById("group-list");
    if (!list) return;
    list.innerHTML = "";
    Object.keys(cls.groups).forEach((group) => {
      const li = document.createElement("li");
      li.className = "avatar-item" + (cls.avatars[group]?.dimmed ? " dimmed" : "");
      li.setAttribute("tabindex", "0");
      li.dataset.group = group;
      const avatar = cls.avatars[group] || DEFAULT_AVATARS[group] || { name: group, badge: "" };
      const svgMarkup = `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 80 80'><rect width='80' height='80' rx='24' fill='%230f6fd7'/><text x='50%' y='55%' dominant-baseline='middle' text-anchor='middle' font-size='32' fill='white'>${avatar.badge || group}</text></svg>`;
      const svgData = `data:image/svg+xml;utf8,${encodeURIComponent(svgMarkup)}`;
      li.innerHTML = `<img alt="${avatar.name}" src="${svgData}"><span>${group} · ${avatar.name}</span><span class="badge-small">Lvl ${avatar.badgeLevel ?? 0}</span>`;
      li.addEventListener("click", () => {
        updateClass(window.OZGE.state.activeClassId, (cls) => ({ activeGroup: group }));
      });
      list.appendChild(li);
    });
  }

  function renderBehavior(cls) {
    const log = document.getElementById("behavior-log");
    if (!log) return;
    log.innerHTML = cls.behavior.slice(0, 10).map((entry) => {
      const label = entry.type === "good" ? "🟢" : entry.type === "warning" ? "🟠" : entry.type === "penalty" ? "🔴" : "⭐";
      return `<div class="behavior-entry">${label} <strong>${entry.group}</strong> · ${new Date(entry.time).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}<br>${entry.note || ""}</div>`;
    }).join("");
  }

  function renderTabs(cls, state) {
    renderContentManager(cls);
    renderAnalytics(cls);
    renderLessonLog(cls);
    renderSettings(cls);
    renderClassTab(cls, state);
  }

  function renderContentManager(cls) {
    const container = document.getElementById("tab-CONTENT_MANAGER");
    if (!container) return;
    const unitChips = (cls.activeUnits || []).map((u) => `<span class="unit-chip selected" data-unit="${u.id}">${u.title}</span>`).join("");
    container.innerHTML = `
      <div class="file-drop" id="file-drop" tabindex="0" aria-label="Import PDF or images">
        <p>Drop PDF or image files here, or <button id="select-files" class="ghost" type="button">Browse</button></p>
        <input type="file" id="hidden-file-input" accept="application/pdf,image/*" multiple style="display:none">
      </div>
      <div class="unit-section">
        <h3>Active Units</h3>
        <div>${unitChips || '<p>No units selected.</p>'}</div>
        <button id="generate-helper" class="primary" type="button">AI Lesson Helper</button>
        <div id="lesson-helper"></div>
      </div>
    `;
    wireContentManager(container, cls);
  }

  function renderAnalytics(cls) {
    const container = document.getElementById("tab-ANALYTICS");
    if (!container) return;
    const modeEntries = Object.entries(cls.weeklyAnalytics.accuracyByMode || {});
    const bars = modeEntries.map(([mode, value], index) => `<div class="chart-bar" style="left:${index * 60}px;height:${Math.round(value)}%" aria-label="${mode} accuracy ${value}%"></div>`).join("");
    container.innerHTML = `
      <div class="analytics-card">
        <h3>Weekly Accuracy</h3>
        <div class="analytics-chart" role="img" aria-label="Weekly accuracy chart">${bars || '<p>No data yet.</p>'}</div>
        <h3>Behavior Timeline</h3>
        <div class="timeline">${cls.weeklyAnalytics.behaviorTimeline.slice(-6).map(item => `<div class="timeline-item">${new Date(item.time).toLocaleString()} · ${item.group} · ${item.type}</div>`).join('')}</div>
        <button class="ghost" id="export-analytics-json">Export JSON</button>
        <button class="ghost" id="export-analytics-csv">Export CSV</button>
      </div>
    `;
    container.querySelector("#export-analytics-json").addEventListener("click", () => exportData(cls.weeklyAnalytics, `analytics-${cls.id}.json`));
    container.querySelector("#export-analytics-csv").addEventListener("click", () => exportData(toCsv(cls.weeklyAnalytics), `analytics-${cls.id}.csv`));
  }

  function renderLessonLog(cls) {
    const container = document.getElementById("tab-LOG");
    if (!container) return;
    container.innerHTML = `
      <div class="analytics-card">
        <h3>Lesson Log</h3>
        <div class="timeline">${cls.lessonLog.slice(-20).reverse().map(entry => `<div class="timeline-item">${entry.timestamp || new Date().toISOString()} · ${entry.mode || entry.action} · ${entry.note || ''}</div>`).join('')}</div>
        <textarea id="manual-note" rows="3" placeholder="Add manual note"></textarea>
        <div class="behavior-actions">
          <button class="primary" id="add-note">Add Note</button>
          <button class="ghost" id="export-log">Export JSON</button>
        </div>
      </div>
    `;
    container.querySelector("#add-note").addEventListener("click", () => {
      const note = container.querySelector("#manual-note").value.trim();
      if (!note) return;
      window.OZGE.pushLessonEntry({ mode: "NOTE", note });
      container.querySelector("#manual-note").value = "";
    });
    container.querySelector("#export-log").addEventListener("click", () => exportData(cls.lessonLog, `lesson-log-${cls.id}.json`));
  }

  function renderSettings(cls) {
    const container = document.getElementById("tab-SETTINGS");
    if (!container) return;
    container.innerHTML = `
      <div class="settings-grid">
        <label>PIN <input id="pin-input" type="password" value="${cls.settings.pin}" aria-label="Security PIN"></label>
        <label>Bonus Duration <input id="settings-bonus" type="number" value="${cls.bonus.duration}" min="20" max="90"></label>
        <button id="save-settings" class="primary">Save Settings</button>
        <button id="export-class" class="ghost">Export Class Pack</button>
        <input type="file" id="import-class" accept="application/json" style="display:none">
        <button id="import-class-btn" class="ghost">Import Class Pack</button>
      </div>
    `;
    container.querySelector("#save-settings").addEventListener("click", () => {
      const pin = container.querySelector("#pin-input").value.trim() || "1234";
      const duration = Number(container.querySelector("#settings-bonus").value) || 30;
      updateClass(cls.id, (current) => ({ settings: { ...current.settings, pin }, bonus: { ...current.bonus, duration } }));
      alert("Settings saved");
    });
    container.querySelector("#export-class").addEventListener("click", () => {
      exportData(cls, `ozge-class-${cls.id}.json`);
    });
    const importInput = container.querySelector("#import-class");
    container.querySelector("#import-class-btn").addEventListener("click", () => importInput.click());
    importInput.addEventListener("change", async (event) => {
      const file = event.target.files?.[0];
      if (!file) return;
      const text = await file.text();
      try {
        const data = JSON.parse(text);
        window.OZGE.importClassPack(cls.id, data);
      } catch (err) {
        alert("Invalid class pack");
      }
    });
  }

  function renderClassTab(cls, state) {
    const container = document.getElementById("tab-CLASS");
    if (!container) return;
    const schedule = (cls.schedule || []).map((slot) => `<li>${slot}</li>`).join("");
    container.innerHTML = `
      <div class="class-switcher">
        ${CLASSES.map((id) => `<button data-class="${id}" class="${id === state.activeClassId ? 'primary' : 'ghost'}">${id}</button>`).join('')}
      </div>
      <h3>Roster</h3>
      <p>${cls.roster.length ? cls.roster.join(', ') : 'No students loaded.'}</p>
      <h3>Schedule</h3>
      <ul>${schedule}</ul>
      <div class="behavior-actions">
        <button class="primary" id="start-session">Start Session</button>
        <button class="ghost" id="mark-attendance">Mark Attendance</button>
      </div>
      <div class="file-drop" id="roster-drop">
        <p>Drop roster CSV/JSON here</p>
        <input type="file" id="roster-input" accept=".csv,application/json" style="display:none">
        <button id="browse-roster" class="ghost">Browse</button>
      </div>
    `;
    container.querySelectorAll("[data-class]").forEach((btn) => {
      btn.addEventListener("click", () => setActiveClass(btn.dataset.class));
    });
    container.querySelector("#start-session").addEventListener("click", () => {
      window.OZGE.pushLessonEntry({ mode: "SESSION", note: `Session started for ${cls.id}` });
      alert(`Session started for ${cls.id}`);
    });
    container.querySelector("#mark-attendance").addEventListener("click", () => {
      const attendance = cls.roster.reduce((acc, name) => {
        acc[name] = confirm(`Is ${name} present?`);
        return acc;
      }, {});
      updateClass(cls.id, () => ({ attendance, lastAttendanceDate: new Date().toISOString() }));
    });
    const rosterDrop = container.querySelector("#roster-drop");
    const browseRoster = container.querySelector("#browse-roster");
    const rosterInput = container.querySelector("#roster-input");
    browseRoster.addEventListener("click", () => rosterInput.click());
    rosterInput.addEventListener("change", (event) => {
      const file = event.target.files?.[0];
      if (!file) return;
      handleRosterFile(file, cls.id);
    });
    ;['dragenter','dragover'].forEach((evt) => rosterDrop.addEventListener(evt, (e) => { e.preventDefault(); rosterDrop.classList.add('drag'); }));
    ;['dragleave','drop'].forEach((evt) => rosterDrop.addEventListener(evt, (e) => { e.preventDefault(); rosterDrop.classList.remove('drag'); }));
    rosterDrop.addEventListener("drop", (event) => {
      const file = event.dataTransfer?.files?.[0];
      if (file) handleRosterFile(file, cls.id);
    });
  }

  function renderProjectorScores(cls) {
    const container = document.getElementById("projector-scores");
    if (!container) return;
    container.innerHTML = Object.entries(cls.behaviorScore).map(([group, score]) => `<div class="score-card"><strong>${group}</strong><span>${score}</span></div>`).join("");
  }

  function renderProjectorBehavior(cls) {
    const banner = document.getElementById("projector-behavior");
    if (!banner) return;
    const latest = cls.behavior[0];
    if (!latest) { banner.textContent = ""; return; }
    const symbol = latest.type === "good" ? "🟢" : latest.type === "warning" ? "🟠" : latest.type === "penalty" ? "🔴" : "⭐";
    banner.textContent = `${symbol} Group ${latest.group}: ${latest.note}`;
  }

  function formatTimer(start) {
    if (!start) return "00:00";
    const diff = Math.max(0, Date.now() - start);
    const minutes = String(Math.floor(diff / 60000)).padStart(2, "0");
    const seconds = String(Math.floor((diff % 60000) / 1000)).padStart(2, "0");
    return `${minutes}:${seconds}`;
  }

  function selectTab(tabId) {
    document.querySelectorAll(".tab").forEach((t) => t.classList.toggle("active", t.dataset.tab === tabId));
    document.querySelectorAll(".tab-panel").forEach((panel) => {
      panel.classList.toggle("active", panel.id === `tab-${tabId}`);
    });
  }

  function handleRosterFile(file, classId) {
    const reader = new FileReader();
    reader.onload = () => {
      const text = reader.result;
      if (file.type === "application/json" || file.name.endsWith(".json")) {
        try {
          const data = JSON.parse(text);
          updateClass(classId, () => ({ roster: data.students || [], groups: data.groups || {} }));
        } catch (err) {
          alert("Invalid JSON roster");
        }
      } else {
        const students = text.split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
        updateClass(classId, (cls) => ({ roster: students, groups: autoGroup(students, cls.groups) }));
      }
    };
    reader.readAsText(file);
  }

  function autoGroup(list, existing) {
    const groups = { ...existing };
    const groupKeys = Object.keys(groups);
    groupKeys.forEach((key) => groups[key] = []);
    list.forEach((student, index) => {
      const target = groupKeys[index % groupKeys.length];
      groups[target].push(student);
    });
    return groups;
  }

  function getActiveGroup() {
    const { activeClassId, classes } = window.OZGE.state;
    const cls = classes[activeClassId];
    return cls.activeGroup || "A";
  }

  function maybeTriggerAutoBonus() {
    const cls = window.OZGE.getClassState();
    if (!cls.settings.autoBonus) return;
    const last = cls.behavior.find((entry) => entry.type !== "score");
    if (!last) return;
    const elapsed = Date.now() - new Date(last.time).getTime();
    if (elapsed > 10 * 60 * 1000) {
      adjustScore(getActiveGroup(), 10, "Team Spirit Bonus");
      setMode("BONUS", { duration: cls.bonus.duration, prompt: "⭐ Team Spirit Bonus!" });
    }
  }

  function syncProjector(state, cls) {
    if (!projectorWin || projectorWin.closed) return;
    projectorWin.postMessage({ type: "state", state, cls }, "*");
  }

  window.addEventListener("message", (event) => {
    if (!event.data || event.data.type !== "state") return;
    // projector receives updates through subscribe, but keep fallback for other windows
  });

 function showOverlay({ id, title, description }) {
    const overlay = document.createElement("div");
    overlay.className = "overlay";
    overlay.dataset.id = id;
    overlay.innerHTML = `<h2>${title}</h2><p>${description}</p><button class="ghost" type="button">Close</button>`;
    overlay.querySelector("button").addEventListener("click", () => overlay.remove());
    const host = document.getElementById("overlays") || document.getElementById("projector-overlays");
    host?.appendChild(overlay);
  }

  function triggerEmergencyStop() {
    showOverlay({ id: "emergency", title: "Emergency STOP", description: "All tools frozen, audio muted, display hidden." });
    document.body.classList.add("emergency");
    setMode("HOME", { summary: "Emergency stop triggered" });
  }

  function requirePin() {
    const cls = window.OZGE.getClassState();
    const input = prompt("Enter PIN", "");
    return input === cls.settings.pin;
  }

  function startCalmDown(duration) {
    window.OZGE.updateClass(window.OZGE.state.activeClassId, (cls) => ({ calmUntil: Date.now() + duration * 1000 }));
    const overlay = document.createElement("div");
    overlay.className = "overlay calm-overlay";
    overlay.innerHTML = `<h2>Calm Down Mode</h2><p>Breath in, breath out...</p><audio id="calm-audio" loop></audio><div class="timer-display" id="calm-timer">${duration}</div>`;
    const host = document.getElementById("overlays") || document.getElementById("projector-overlays");
    host?.appendChild(overlay);
    playSound("soft-chime");
    let remaining = duration;
    calmTimeout && clearInterval(calmTimeout);
    calmTimeout = setInterval(() => {
      remaining -= 1;
      const timer = document.getElementById("calm-timer");
      if (timer) timer.textContent = remaining;
      if (remaining <= 0) {
        overlay.remove();
        clearInterval(calmTimeout);
      }
    }, 1000);
  }

  function checkCalmDown(cls) {
    if (cls.calmUntil && Date.now() < cls.calmUntil) {
      if (!document.querySelector(".calm-overlay")) {
        startCalmDown(Math.floor((cls.calmUntil - Date.now()) / 1000));
      }
    }
  }

  function playSound(name) {
    const base64 = document.querySelector(`script[data-sound='${name}']`);
    if (base64) return;
    fetch(`assets/sounds/${name}.wav.base64`).then((res) => res.text()).then((data) => {
      const audio = new Audio(`data:audio/wav;base64,${data.trim()}`);
      audio.play();
    }).catch(() => {});
  }

  function wireContentManager(container, cls) {
    const drop = container.querySelector("#file-drop");
    const hiddenInput = container.querySelector("#hidden-file-input");
    const browseBtn = container.querySelector("#select-files");
    browseBtn.addEventListener("click", () => hiddenInput.click());
    hiddenInput.addEventListener("change", (event) => handleFiles(event.target.files));
    ;['dragenter','dragover'].forEach((evt) => drop.addEventListener(evt, (e) => { e.preventDefault(); drop.classList.add('drag'); }));
    ;['dragleave','drop'].forEach((evt) => drop.addEventListener(evt, (e) => { e.preventDefault(); drop.classList.remove('drag'); }));
    drop.addEventListener("drop", (event) => {
      event.preventDefault();
      handleFiles(event.dataTransfer.files);
    });
    container.querySelector("#generate-helper").addEventListener("click", () => {
      const helper = window.OZGE.AILessonHelper.generate(cls.activeUnits || []);
      container.querySelector("#lesson-helper").innerHTML = helper.map(renderHelper).join("");
    });
  }

  function renderHelper(unit) {
    const mcqs = unit.mcqs.map((item) => `<li>${item.question} <em>${item.options.join(', ')}</em></li>`).join('');
    return `<div class="analytics-card"><h4>${unit.title}</h4><p>${unit.summary}</p><h5>Words</h5><ul>${unit.words.map(w => `<li>${w.word} - ${w.definition} (${w.difficulty})</li>`).join('')}</ul><h5>MCQs</h5><ol>${mcqs}</ol></div>`;
  }

  function handleFiles(fileList) {
    Array.from(fileList).forEach(async (file) => {
      const info = await window.OZGE.ContentManager.ingestFile(file);
      if (!info || !info.length) return;
      updateClass(window.OZGE.state.activeClassId, (cls) => ({ activeUnits: [...cls.activeUnits, ...info] }));
    });
  }

  function exportData(data, filename) {
    const blob = new Blob([typeof data === "string" ? data : JSON.stringify(data, null, 2)], { type: "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  function toCsv(analytics) {
    const rows = ["mode,accuracy"]; // simple csv
    Object.entries(analytics.accuracyByMode || {}).forEach(([mode, value]) => rows.push(`${mode},${value}`));
    return rows.join("\n");
  }
})();
