(function () {
  const { state, updateClass, CLASSES } = window.OZGE;
  let timerInterval = null;

  document.addEventListener("DOMContentLoaded", () => {
    preloadDemoData();
    setupKeyboardShortcuts();
    initTimers();
  });

  function preloadDemoData() {
    Promise.all([
      fetch("data/demo-units.json").then((res) => res.json()).catch(() => []),
      fetch("data/demo-rosters.json").then((res) => res.json()).catch(() => ({}))
    ]).then(([units, rosters]) => {
      CLASSES.forEach((id) => {
        updateClass(id, (cls) => ({
          roster: rosters[id]?.students || cls.roster,
          groups: rosters[id]?.groups || cls.groups,
          schedule: rosters[id]?.schedule || cls.schedule,
          activeUnits: units,
          topic: units[0]?.title || cls.topic
        }));
      });
    });
  }

  function setupKeyboardShortcuts() {
    document.addEventListener("keydown", (event) => {
      const { key } = event;
      if (document.body.dataset.screen !== "teacher") return;
      if (key === " ") { event.preventDefault(); togglePause(); }
      if (key === "ArrowRight") nextMode();
      if (key === "ArrowLeft") previousMode();
      if (key === "n" || key === "N") triggerDrawAction("next");
      if (key === "c" || key === "C") triggerDrawAction("clear");
      if (key === "s" || key === "S") triggerDrawAction("save");
      if (key === "[") triggerDrawAction("thinner");
      if (key === "]") triggerDrawAction("thicker");
      if (key === "e" || key === "E") triggerDrawAction("eraser");
      if (key === "p" || key === "P") triggerDrawAction("pen");
    });
  }

  function togglePause() {
    window.OZGE.setMode("HOME", { summary: "Paused" });
  }

  function nextMode() {
    cycleMode(1);
  }

  function previousMode() {
    cycleMode(-1);
  }

  function cycleMode(direction) {
    const modes = window.OZGE.MODES;
    const cls = window.OZGE.getClassState();
    const index = modes.indexOf(cls.mode);
    const next = modes[(index + direction + modes.length) % modes.length];
    window.OZGE.setMode(next, {});
  }

  function triggerDrawAction(action) {
    document.dispatchEvent(new CustomEvent("ozge-draw-action", { detail: { action } }));
  }

  function initTimers() {
    timerInterval = setInterval(() => {
      const cls = window.OZGE.getClassState();
      if (cls.mode === "BONUS") {
        const payload = cls.activePayload || {};
        const startedAt = payload.startedAt || Date.now();
        const duration = payload.duration || 30;
        const end = startedAt + duration * 1000;
        if (Date.now() >= end) {
          window.OZGE.setMode("RESULT", { summary: "Bonus finished", accuracy: Math.round(Math.random() * 100) });
        } else {
          window.OZGE.updateModePayload({ ...payload, startedAt, duration, remaining: Math.ceil((end - Date.now()) / 1000) });
        }
      }
    }, 1000);
  }

  window.addEventListener("beforeunload", () => {
    if (timerInterval) clearInterval(timerInterval);
  });

  window.OZGE.Core = { preloadDemoData };
})();
