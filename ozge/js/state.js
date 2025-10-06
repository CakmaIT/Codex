(function () {
  const CLASSES = ["5A", "5B", "5C", "5D"];
  const MODES = ["SPLASH", "HOME", "QUIZ", "PUZZLE", "SPEAK", "STORY", "DRAW", "BONUS", "RESULT"];
  const TEACHER_TABS = ["CONTENT_MANAGER", "ANALYTICS", "LOG", "SETTINGS", "CLASS"];
  const STORAGE_KEY = "ozge-classroom-state-v1";

  const DEFAULT_AVATARS = {
    A: { name: "Wizard of Words", badge: "🌟" },
    B: { name: "Grammar Guardian", badge: "📘" },
    C: { name: "Story Sage", badge: "📖" },
    D: { name: "Sound Sprinter", badge: "🎙️" },
    E: { name: "Puzzle Pilot", badge: "🧩" },
    F: { name: "Bonus Bandit", badge: "⚡" },
    G: { name: "Calm Captain", badge: "🕊️" },
    H: { name: "Quiz Quester", badge: "❓" }
  };

  function createDefaultClassState(id) {
    return {
      id,
      roster: [],
      groups: { A: [], B: [], C: [], D: [], E: [], F: [], G: [], H: [] },
      avatars: Object.keys(DEFAULT_AVATARS).reduce((acc, key) => {
        acc[key] = { ...DEFAULT_AVATARS[key], dimmed: false, badgeLevel: 0 };
        return acc;
      }, {}),
      attendance: {},
      behavior: [],
      behaviorScore: { A: 0, B: 0, C: 0, D: 0, E: 0, F: 0, G: 0, H: 0 },
      behaviorTimer: 0,
      calmUntil: 0,
      topic: "Welcome Pack",
      mode: "HOME",
      selectedMode: null,
      unitPacks: {},
      activeUnits: [],
      quizProgress: { index: 0, correct: 0, total: 0 },
      puzzleProgress: { solved: 0 },
      speakingResults: [],
      storyProgress: { index: 0 },
      drawGallery: [],
      bonus: { streak: 0, lastTriggered: 0, duration: 30 },
      lessonLog: [],
      weeklyAnalytics: {
        accuracyByMode: {},
        mostMissed: [],
        behaviorTimeline: [],
        speakingScores: []
      },
      settings: {
        pin: "1234",
        autoBonus: false,
        projectorWindowId: null
      },
      schedule: [],
      sessionSnapshots: [],
      lastSync: Date.now(),
      lastAttendanceDate: null
    };
  }

  function loadStoredState() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw);
      if (!parsed || typeof parsed !== "object") return null;
      return parsed;
    } catch (err) {
      console.warn("Failed to load stored state", err);
      return null;
    }
  }

  function buildInitialState() {
    const fromStorage = loadStoredState();
    const base = {
      classes: {},
      activeClassId: CLASSES[0],
      splashShown: false,
      projectorConnected: false
    };
    CLASSES.forEach((id) => {
      base.classes[id] = createDefaultClassState(id);
    });
    const merged = fromStorage ? mergeState(base, fromStorage) : base;
    merged.version = "1.0.0";
    return merged;
  }

  function mergeState(base, saved) {
    const result = JSON.parse(JSON.stringify(base));
    if (!saved || typeof saved !== "object") return result;
    Object.keys(result.classes).forEach((id) => {
      if (saved.classes && saved.classes[id]) {
        result.classes[id] = { ...result.classes[id], ...saved.classes[id] };
      }
    });
    if (saved.activeClassId && result.classes[saved.activeClassId]) {
      result.activeClassId = saved.activeClassId;
    }
    result.splashShown = saved.splashShown ?? false;
    result.projectorConnected = saved.projectorConnected ?? false;
    return result;
  }

  const state = buildInitialState();
  const subscribers = new Set();

  function getClassState(id) {
    return state.classes[id || state.activeClassId];
  }

  function setActiveClass(id) {
    if (!state.classes[id]) return;
    state.activeClassId = id;
    emit();
  }

  function updateClass(id, updater) {
    const classState = getClassState(id);
    if (!classState) return;
    const next = updater({ ...classState });
    if (!next) return;
    state.classes[id] = { ...classState, ...next, lastSync: Date.now() };
    emit();
  }

  function pushLessonEntry(entry) {
    const id = state.activeClassId;
    updateClass(id, (cls) => {
      const lessonLog = [...(cls.lessonLog || [])];
      lessonLog.push({ ...entry, timestamp: new Date().toISOString() });
      return { lessonLog };
    });
  }

  function updateBehaviorLog(group, type, note) {
    const id = state.activeClassId;
    updateClass(id, (cls) => {
      const history = [...(cls.behavior || [])];
      history.unshift({ group, type, note, time: new Date().toISOString() });
      const scoreDelta = type === "good" ? 5 : type === "warning" ? 0 : -10;
      const behaviorScore = { ...cls.behaviorScore };
      behaviorScore[group] = (behaviorScore[group] || 0) + scoreDelta;
      const avatars = { ...cls.avatars };
      if (type === "penalty") {
        avatars[group] = { ...avatars[group], dimmed: true };
      }
      if (type === "good") {
        avatars[group] = { ...avatars[group], dimmed: false, badgeLevel: avatars[group].badgeLevel + 1 };
      }
      const behaviorTimeline = [...(cls.weeklyAnalytics.behaviorTimeline || [])];
      behaviorTimeline.push({ time: Date.now(), group, type });
      const weeklyAnalytics = { ...cls.weeklyAnalytics, behaviorTimeline };
      return { behavior: history.slice(0, 80), behaviorScore, avatars, weeklyAnalytics };
    });
  }

  function adjustScore(group, delta, reason) {
    const id = state.activeClassId;
    updateClass(id, (cls) => {
      const behaviorScore = { ...cls.behaviorScore };
      behaviorScore[group] = (behaviorScore[group] || 0) + delta;
      const behavior = [...(cls.behavior || [])];
      behavior.unshift({ group, type: "score", note: reason, delta, time: new Date().toISOString() });
      return { behaviorScore, behavior: behavior.slice(0, 80) };
    });
  }

  function setMode(mode, payload = {}) {
    const id = state.activeClassId;
    if (!MODES.includes(mode)) return;
    updateClass(id, (cls) => {
      return {
        mode,
        selectedMode: payload.selectedMode || cls.selectedMode,
        lastModeChange: Date.now(),
        activePayload: payload
      };
    });
  }

  function updateModePayload(patch = {}) {
    const id = state.activeClassId;
    updateClass(id, (cls) => ({ activePayload: { ...cls.activePayload, ...patch } }));
  }

  function updateUnits(units) {
    const id = state.activeClassId;
    updateClass(id, (cls) => ({ activeUnits: units }));
  }

  function setLessonSummary(summary) {
    const id = state.activeClassId;
    updateClass(id, (cls) => {
      const weeklyAnalytics = {
        ...cls.weeklyAnalytics,
        accuracyByMode: {
          ...cls.weeklyAnalytics.accuracyByMode,
          [summary.mode]: summary.accuracy
        }
      };
      const lessonLog = [...(cls.lessonLog || [])];
      lessonLog.push(summary);
      return { weeklyAnalytics, lessonLog };
    });
  }

  function saveState() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch (err) {
      console.warn("Failed to persist state", err);
    }
  }

  function emit() {
    saveState();
    subscribers.forEach((fn) => {
      try { fn(state); } catch (err) { console.error(err); }
    });
  }

  function subscribe(fn) {
    subscribers.add(fn);
    fn(state);
    return () => subscribers.delete(fn);
  }

  function importClassPack(id, pack) {
    updateClass(id, () => ({ ...pack }));
  }

  window.OZGE = {
    CLASSES,
    MODES,
    TEACHER_TABS,
    DEFAULT_AVATARS,
    state,
    subscribe,
    getClassState,
    setActiveClass,
    updateClass,
    setMode,
    updateModePayload,
    updateUnits,
    adjustScore,
    updateBehaviorLog,
    pushLessonEntry,
    setLessonSummary,
    importClassPack
  };
})();
