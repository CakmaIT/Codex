(function () {
  document.addEventListener("DOMContentLoaded", () => window.OZGE.subscribe(render));

  function render() {
    const cls = window.OZGE.getClassState();
    const stage = document.getElementById("mode-stage");
    if (!stage || cls.mode !== "STORY") return;
    const unit = cls.activeUnits[Math.floor(Math.random() * cls.activeUnits.length)] || {};
    const story = unit.story || { panels: ["Story"], question: "What happened?", options: ["Option"], answer: "Option" };
    stage.innerHTML = `
      <div class="story-card">
        <div class="panels">${story.panels.map((panel, index) => `<div class="panel">${index + 1}. ${panel}</div>`).join('')}</div>
        <p>${story.question}</p>
        <div class="quiz-options">
          ${story.options.map((option) => `<button data-answer="${option === story.answer}">${option}</button>`).join('')}
        </div>
      </div>
    `;
    stage.querySelectorAll("button").forEach((btn) => btn.addEventListener("click", () => handleAnswer(btn.dataset.answer === "true", story)));
    window.OZGE.updateModePayload({ panel: story.panels[0], question: story.question });
  }

  function handleAnswer(correct, story) {
    const group = window.OZGE.getClassState().activeGroup || "A";
    window.OZGE.adjustScore(group, correct ? 6 : -3, correct ? "Story win" : "Story miss");
    window.OZGE.Analytics.record("STORY", { score: correct ? 100 : 40, missed: correct ? [] : [story.answer] });
    setTimeout(() => window.OZGE.setMode("RESULT", { summary: correct ? "Story mastered" : "Revisit the story", accuracy: correct ? 100 : 40 }), 1000);
  }
})();
