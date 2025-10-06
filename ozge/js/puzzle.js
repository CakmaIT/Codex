(function () {
  document.addEventListener("DOMContentLoaded", () => window.OZGE.subscribe(render));
  let solution = "";

  function render() {
    const cls = window.OZGE.getClassState();
    const stage = document.getElementById("mode-stage");
    if (!stage || cls.mode !== "PUZZLE") return;
    const word = pickWord(cls);
    solution = word;
    stage.innerHTML = `
      <div class="puzzle-card" aria-label="Word puzzle">
        <h3>Arrange the tiles</h3>
        <p>Group ${cls.activeGroup || 'A'}, drag letters to spell the word.</p>
        <div class="tiles" id="tile-bank">${shuffle(word.split('')).map((letter, index) => `<button class="tile" draggable="true" data-index="${index}">${letter.toUpperCase()}</button>`).join('')}</div>
        <div class="tiles" id="tile-drop" aria-live="polite"></div>
      </div>
    `;
    setupDrag();
    window.OZGE.updateModePayload({ word });
  }

  function pickWord(cls) {
    const units = cls.activeUnits || [];
    const words = units.flatMap((unit) => unit.vocabulary.map((item) => item.word));
    return words[Math.floor(Math.random() * words.length)] || "puzzle";
  }

  function setupDrag() {
    const bank = document.getElementById("tile-bank");
    const drop = document.getElementById("tile-drop");
    let assembled = [];
    bank.querySelectorAll(".tile").forEach((tile) => {
      tile.addEventListener("click", () => {
        assembled.push(tile.textContent);
        tile.disabled = true;
        updateDrop(drop, assembled.join(""));
      });
    });
    drop.addEventListener("dblclick", () => {
      assembled = [];
      bank.querySelectorAll(".tile").forEach((tile) => (tile.disabled = false));
      updateDrop(drop, "");
    });
    const observer = new MutationObserver(() => {
      if (assembled.join("").toLowerCase() === solution.toLowerCase()) {
        celebrate();
      }
    });
    observer.observe(drop, { childList: true, subtree: true });
  }

  function updateDrop(drop, word) {
    drop.innerHTML = word.split("").map((letter) => `<span class="tile filled">${letter}</span>`).join("");
  }

  function celebrate() {
    const group = window.OZGE.getClassState().activeGroup || "A";
    window.OZGE.adjustScore(group, 8, "Puzzle solved");
    const confetti = document.createElement("div");
    confetti.className = "overlay";
    confetti.innerHTML = `<svg width="120" height="120"><use href="assets/icons.svg#confetti"></use></svg><h2>Great job!</h2>`;
    document.getElementById("overlays").appendChild(confetti);
    setTimeout(() => confetti.remove(), 1500);
    window.OZGE.Analytics.record("PUZZLE", { score: 100 });
  }

  function shuffle(array) {
    const copy = [...array];
    for (let i = copy.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [copy[i], copy[j]] = [copy[j], copy[i]];
    }
    return copy;
  }
})();
