(function () {
  const CLASSES = ["5A", "5B", "5C", "5D"];
  const DEFAULT_PIN = "1234";
  const STORAGE_KEY = "ozge-state-v1";

  const baseState = {
    activeClass: "5A",
    projectorApproval: false,
    blackout: false,
    calmModeUntil: null,
    settings: {
      pin: DEFAULT_PIN,
      bonusDuration: 30,
      autoBonusMinutes: 10
    }
  };

  let store = null;

  function createEmptyClassState(id) {
    const groups = ["A", "B", "C", "D", "E", "F", "G", "H"].reduce((acc, label) => {
      acc[label] = {
        name: `Group ${label}`,
        avatar: "😀",
        score: 0,
        behavior: "Balanced",
        badges: [],
        penalties: 0,
        attendance: {}
      };
      return acc;
    }, {});

    return {
      id,
      units: [],
      roster: { groups },
      schedule: [],
      attendanceHistory: [],
      unitSelections: {
        QUIZ: [],
        PUZZLE: [],
        SPEAK: [],
        STORY: [],
        DRAW: []
      },
      behaviorLog: [],
      lessonLog: [],
      sessionSnapshots: [],
      contentPacks: [],
      approvals: {
        pending: false,
        snapshot: null
      },
      current: {
        mode: "SPLASH",
        unitId: null,
        timerStart: null,
        activeGroup: "A",
        streak: 0
      },
      weekly: {
        analytics: [],
        lastUpdated: null
      }
    };
  }

  function loadSavedState() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw);
      if (!parsed || typeof parsed !== "object") return null;
      return parsed;
    } catch (err) {
      console.warn("State restore failed", err);
      return null;
    }
  }

  function persist() {
    if (!store) return;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(store));
  }

  async function loadDemoData() {
    const [unitsRes, rosterRes] = await Promise.all([
      fetch("data/demo-units.json").then((r) => r.json()),
      fetch("data/demo-rosters.json").then((r) => r.json())
    ]);
    CLASSES.forEach((id) => {
      if (!store.classes[id]) {
        store.classes[id] = createEmptyClassState(id);
      }
      const classState = store.classes[id];
      const roster = rosterRes[id];
      if (roster) {
        Object.entries(roster.groups || {}).forEach(([group, names]) => {
          if (classState.roster.groups[group]) {
            classState.roster.groups[group].members = names;
          }
        });
        classState.schedule = roster.schedule || [];
      }
      const demoUnits = unitsRes[id] || [];
      if (!classState.units.length) {
        classState.units = demoUnits;
      }
    });
  }

  function initStore() {
    const saved = loadSavedState();
    if (saved) {
      store = saved;
      CLASSES.forEach((id) => {
        if (!store.classes[id]) {
          store.classes[id] = createEmptyClassState(id);
        }
      });
    } else {
      store = {
        ...baseState,
        classes: {}
      };
      CLASSES.forEach((id) => {
        store.classes[id] = createEmptyClassState(id);
      });
    }
  }

  function ensureClass(id) {
    if (!store.classes[id]) {
      store.classes[id] = createEmptyClassState(id);
    }
    return store.classes[id];
  }

  function getActiveClass() {
    return ensureClass(store.activeClass);
  }

  function update(fn) {
    const result = fn(store, getActiveClass());
    persist();
    return result;
  }

  function setActiveClass(id) {
    if (!CLASSES.includes(id)) return;
    store.activeClass = id;
    persist();
    window.dispatchEvent(new CustomEvent("ozge:class-change", { detail: { id } }));
  }

  function recordBehavior(group, action, value) {
    const cls = getActiveClass();
    const entry = {
      timestamp: Date.now(),
      group,
      action,
      value
    };
    cls.behaviorLog.push(entry);
    const g = cls.roster.groups[group];
    if (g) {
      if (action === "positive") {
        g.score += value;
        g.behavior = "Excellent";
      } else if (action === "warning") {
        g.behavior = "Warning";
      } else if (action === "penalty") {
        g.score = Math.max(0, g.score - Math.abs(value));
        g.behavior = "Penalty";
        g.penalties += 1;
      }
    }
    persist();
    return entry;
  }

  function addLessonLog(entry) {
    const cls = getActiveClass();
    cls.lessonLog.push({ ...entry, timestamp: Date.now() });
    persist();
  }

  function addSnapshot(snapshot) {
    const cls = getActiveClass();
    cls.sessionSnapshots.push({ ...snapshot, timestamp: Date.now(), id: crypto.randomUUID() });
    persist();
  }

  function upsertUnit(unit) {
    const cls = getActiveClass();
    const index = cls.units.findIndex((u) => u.id === unit.id);
    if (index >= 0) {
      cls.units[index] = unit;
    } else {
      cls.units.push(unit);
    }
    persist();
  }

  function setUnitSelection(mode, ids) {
    const cls = getActiveClass();
    cls.unitSelections[mode] = Array.from(new Set(ids));
    persist();
  }

  function resetScores() {
    const cls = getActiveClass();
    Object.values(cls.roster.groups).forEach((group) => {
      group.score = 0;
      group.behavior = "Balanced";
      group.penalties = 0;
    });
    persist();
  }

  initStore();

  const ready = loadDemoData().then(() => {
    persist();
    return store;
  });

  window.State = {
    ready,
    CLASSES,
    get store() {
      return store;
    },
    getActiveClass,
    setActiveClass,
    update,
    recordBehavior,
    addLessonLog,
    addSnapshot,
    upsertUnit,
    setUnitSelection,
    resetScores,
    persist,
    ensureClass,
    DEFAULT_PIN
  };
})();
