(function () {
  document.addEventListener("DOMContentLoaded", () => {
    document.addEventListener("ozge-bonus-answer", (event) => {
      const detail = event.detail || {};
      const group = detail.group || window.OZGE.state.classes[window.OZGE.state.activeClassId].activeGroup || "A";
      const correct = detail.correct ?? Math.random() > 0.3;
      const delta = correct ? 10 : -5;
      window.OZGE.adjustScore(group, delta, correct ? "Bonus correct" : "Bonus miss");
      if (correct) {
        window.OZGE.updateClass(window.OZGE.state.activeClassId, (cls) => ({ bonus: { ...cls.bonus, streak: (cls.bonus.streak || 0) + 1 } }));
      }
    });
  });
})();
