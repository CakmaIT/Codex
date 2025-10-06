(function () {
  const CHANNEL = "ozge-sync";
  const STORAGE_KEY = "ozge-sync-snapshot";
  let bc = null;
  let pollInterval = null;

  document.addEventListener("DOMContentLoaded", () => {
    setupChannel();
    setupPolling();
  });

  function setupChannel() {
    try {
      bc = new BroadcastChannel(CHANNEL);
      window.OZGE.subscribe((state) => {
        bc.postMessage({ type: "state", state });
        saveSnapshot(state);
      });
      bc.onmessage = (event) => {
        if (!event.data) return;
        if (event.data.type === "state" && event.data.state) {
          reconcileState(event.data.state);
        }
      };
    } catch (err) {
      console.warn("BroadcastChannel not available", err);
    }
  }

  function setupPolling() {
    pollInterval = setInterval(() => {
      const snapshot = loadSnapshot();
      if (snapshot) reconcileState(snapshot);
    }, 3000);
  }

  function saveSnapshot(state) {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ state, updatedAt: Date.now() }));
    } catch (err) {
      console.warn("Could not save snapshot", err);
    }
  }

  function loadSnapshot() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw);
      return parsed.state;
    } catch (err) {
      return null;
    }
  }

  function reconcileState(snapshot) {
    if (!snapshot || !snapshot.classes) return;
    Object.keys(snapshot.classes).forEach((classId) => {
      const remote = snapshot.classes[classId];
      const local = window.OZGE.state.classes[classId];
      if (!local || !remote) return;
      if ((remote.lastSync || 0) > (local.lastSync || 0)) {
        window.OZGE.state.classes[classId] = { ...local, ...remote };
      }
    });
  }

  window.addEventListener("beforeunload", () => {
    if (pollInterval) clearInterval(pollInterval);
    if (bc) bc.close();
  });
})();
