(function () {
  const CHANNEL_NAME = "ozge-sync";
  const SNAPSHOT_KEY = "ozge-sync-snapshot";
  const listeners = new Set();
  let channel = null;
  let lastLocalEmit = 0;

  function initChannel() {
    try {
      channel = new BroadcastChannel(CHANNEL_NAME);
      channel.onmessage = (event) => {
        if (!event || !event.data) return;
        dispatch(event.data);
      };
    } catch (err) {
      console.warn("BroadcastChannel unavailable", err);
    }
  }

  function dispatch(message) {
    listeners.forEach((handler) => {
      try {
        handler(message);
      } catch (err) {
        console.error("Sync handler error", err);
      }
    });
  }

  function emit(type, payload) {
    const message = { type, payload, timestamp: Date.now() };
    lastLocalEmit = message.timestamp;
    if (channel) {
      channel.postMessage(message);
    }
    localStorage.setItem("ozge-sync-last", JSON.stringify(message));
    dispatch(message);
  }

  function pollFallback() {
    window.addEventListener("storage", (event) => {
      if (event.key !== "ozge-sync-last" || !event.newValue) return;
      try {
        const parsed = JSON.parse(event.newValue);
        if (parsed.timestamp <= lastLocalEmit) return;
        dispatch(parsed);
      } catch (err) {
        console.warn("Sync fallback parse error", err);
      }
    });
  }

  function saveSnapshot(state) {
    localStorage.setItem(SNAPSHOT_KEY, JSON.stringify(state));
  }

  function loadSnapshot() {
    try {
      const raw = localStorage.getItem(SNAPSHOT_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch (err) {
      console.warn("Snapshot load failed", err);
      return null;
    }
  }

  initChannel();
  pollFallback();

  window.Sync = {
    on(handler) {
      listeners.add(handler);
      return () => listeners.delete(handler);
    },
    emit,
    saveSnapshot,
    loadSnapshot,
    CHANNEL_NAME
  };
})();
