(function () {
  const teacherViews = {};
  const projectorViews = {};

  function $(selector, root = document) {
    return root.querySelector(selector);
  }

  function $all(selector, root = document) {
    return Array.from(root.querySelectorAll(selector));
  }

  function formatTimer(ms) {
    if (!ms) return "00:00";
    const total = Math.max(0, Math.floor(ms / 1000));
    const m = String(Math.floor(total / 60)).padStart(2, "0");
    const s = String(total % 60).padStart(2, "0");
    return `${m}:${s}`;
  }

  function getGroupList(cls) {
    return Object.entries(cls.roster.groups);
  }

  function renderGroups(container, cls) {
    if (!container) return;
    container.innerHTML = "";
    const template = document.getElementById("group-card-template");
    getGroupList(cls).forEach(([id, data]) => {
      const node = template.content.firstElementChild.cloneNode(true);
      node.dataset.group = id;
      node.querySelector('[data-field="name"]').textContent = `${data.name}`;
      node.querySelector('[data-field="score"]').textContent = data.score;
      node.querySelector('[data-field="behavior"]').textContent = data.behavior;
      node.querySelector('.avatar').textContent = data.avatar || "😀";
      container.appendChild(node);
    });
  }

  function renderHome(cls) {
    const topicEl = document.querySelector('[data-bind="topic"]');
    if (topicEl) {
      const unit = cls.units.find((u) => u.id === cls.current.unitId) || cls.units[0];
      topicEl.textContent = unit ? unit.title : "—";
    }
    renderGroups(document.getElementById("group-list"), cls);
  }

  function renderProjectorHome(cls) {
    const avatars = document.getElementById("projector-avatars");
    const scores = document.getElementById("projector-scores");
    if (avatars) {
      avatars.innerHTML = "";
      getGroupList(cls).forEach(([id, data]) => {
        const span = document.createElement("span");
        span.className = "projector-avatar";
        span.dataset.group = id;
        span.textContent = `${data.avatar || "😀"} ${id}`;
        avatars.appendChild(span);
      });
    }
    if (scores) {
      scores.innerHTML = "";
      getGroupList(cls).forEach(([id, data]) => {
        const span = document.createElement("span");
        span.className = "projector-score";
        span.dataset.group = id;
        span.textContent = `${id}: ${data.score}`;
        scores.appendChild(span);
      });
    }
  }

  function setActiveTab(tabId) {
    $all('#teacher-tabs .tab').forEach((btn) => {
      btn.classList.toggle("active", btn.dataset.tab === tabId);
    });
    $all('#view-container .view').forEach((view) => {
      view.classList.toggle("view-active", view.id === `view-${tabId}`);
    });
  }

  function toggleHeader(active) {
    const header = document.getElementById("main-header");
    const main = document.getElementById("main");
    if (!header || !main) return;
    header.classList.toggle("hidden", !active);
    main.classList.toggle("hidden", !active);
  }

  function showPinDialog() {
    const dialog = document.getElementById("pin-dialog");
    const input = document.getElementById("pin-input");
    if (!dialog || !input) return new Promise((resolve) => resolve(false));
    dialog.hidden = false;
    input.value = "";
    input.focus();
    return new Promise((resolve) => {
      function cleanup(result) {
        dialog.hidden = true;
        input.value = "";
        cancelBtn.removeEventListener("click", onCancel);
        confirmBtn.removeEventListener("click", onConfirm);
        dialog.removeEventListener("keydown", onKey);
        resolve(result);
      }
      function onCancel() {
        cleanup(false);
      }
      function onConfirm() {
        cleanup(input.value);
      }
      function onKey(event) {
        if (event.key === "Escape") {
          cleanup(false);
        } else if (event.key === "Enter") {
          cleanup(input.value);
        }
      }
      const cancelBtn = document.getElementById("pin-cancel");
      const confirmBtn = document.getElementById("pin-confirm");
      cancelBtn.addEventListener("click", onCancel);
      confirmBtn.addEventListener("click", onConfirm);
      dialog.addEventListener("keydown", onKey);
    });
  }

  function setProjectorSection(section) {
    const sections = $all('.projector-section');
    sections.forEach((node) => {
      node.classList.toggle("active", node.id === section);
    });
  }

  function setProjectorStatus(cls) {
    const header = document.querySelector('.projector-status');
    if (!header) return;
    const mode = cls.current.mode;
    const unit = cls.units.find((u) => u.id === cls.current.unitId) || { title: "—" };
    header.querySelector('[data-field="class"]').textContent = `Class ${cls.id}`;
    header.querySelector('[data-field="unit"]').textContent = unit.title;
    header.querySelector('[data-field="timer"]').textContent = formatTimer(Date.now() - (cls.current.timerStart || Date.now()));
    header.querySelector('[data-field="group"]').textContent = `Group ${cls.current.activeGroup}`;
    const modeHeader = document.querySelector('#projector-content [data-field="mode"]');
    if (modeHeader) {
      modeHeader.textContent = mode;
    }
  }

  function renderTimer(cls) {
    const timerEl = document.querySelector('[data-bind="timer"]');
    if (timerEl) {
      timerEl.textContent = formatTimer(Date.now() - (cls.current.timerStart || Date.now()));
    }
    if (document.body.dataset.role === "projector") {
      setProjectorStatus(cls);
    }
  }

  window.UI = {
    renderHome,
    renderGroups,
    renderProjectorHome,
    setActiveTab,
    toggleHeader,
    showPinDialog,
    setProjectorSection,
    setProjectorStatus,
    renderTimer
  };
})();
