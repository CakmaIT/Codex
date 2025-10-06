(function () {
  const Speech = {
    recognizer: null,
    active: false,
    init() {
      const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
      if (SpeechRecognition) {
        this.recognizer = new SpeechRecognition();
        this.recognizer.lang = "en-US";
        this.recognizer.interimResults = true;
        this.recognizer.onresult = (event) => {
          const transcript = Array.from(event.results).map((result) => result[0].transcript).join(" ");
          const score = this.score(transcript);
          document.dispatchEvent(new CustomEvent("ozge-speech-score", { detail: { transcript, score } }));
        };
        this.recognizer.onend = () => { this.active = false; };
      }
      window.OZGE.subscribe(renderSpeakMode);
      document.addEventListener("ozge-speech-start", () => this.start());
      document.addEventListener("ozge-speech-stop", () => this.stop());
      document.addEventListener("ozge-speech-score", onScore);
    },
    start() {
      if (this.recognizer) {
        this.active = true;
        this.recognizer.start();
      } else {
        this.simulate();
      }
    },
    stop() {
      if (this.recognizer && this.active) {
        this.recognizer.stop();
      }
    },
    simulate() {
      const phrases = ["I love learning", "Speaking clearly", "Team spirit", "Vocabulary champion"];
      const transcript = phrases[Math.floor(Math.random() * phrases.length)];
      const score = this.score(transcript);
      setTimeout(() => document.dispatchEvent(new CustomEvent("ozge-speech-score", { detail: { transcript, score } })), 1000);
    },
    score(transcript) {
      if (!transcript) return 0;
      const words = transcript.split(/\s+/).length;
      const clarity = Math.min(1, transcript.replace(/[^a-z]/gi, "").length / 30);
      return Math.min(100, Math.round(words * 10 * clarity + Math.random() * 10));
    }
  };

  function renderSpeakMode() {
    const cls = window.OZGE.getClassState();
    const stage = document.getElementById("mode-stage");
    if (!stage || cls.mode !== "SPEAK") return;
    const unit = cls.activeUnits[Math.floor(Math.random() * cls.activeUnits.length)] || {};
    const sentence = unit.sentences?.[Math.floor(Math.random() * (unit.sentences?.length || 1))] || "Describe your day.";
    stage.innerHTML = `
      <div class="quiz-card">
        <h3>Speaking Practice</h3>
        <p>${sentence}</p>
        <div class="behavior-actions">
          <button id="speak-start" class="primary">Start</button>
          <button id="speak-stop" class="ghost">Stop</button>
          <button id="speak-award" class="ghost">Award +5</button>
        </div>
        <div class="waveform" id="speak-wave" aria-live="polite">Ready</div>
        <div id="speak-score" class="timer-display">--</div>
      </div>
    `;
    stage.querySelector("#speak-start").addEventListener("click", () => document.dispatchEvent(new CustomEvent("ozge-speech-start")));
    stage.querySelector("#speak-stop").addEventListener("click", () => document.dispatchEvent(new CustomEvent("ozge-speech-stop")));
    stage.querySelector("#speak-award").addEventListener("click", () => {
      window.OZGE.adjustScore(cls.activeGroup || "A", 5, "Speaking award");
    });
    window.OZGE.updateModePayload({ sentence });
  }

  function onScore(event) {
    const { transcript, score } = event.detail;
    const stageScore = document.getElementById("speak-score");
    const wave = document.getElementById("speak-wave");
    if (stageScore) stageScore.textContent = score;
    if (wave) wave.textContent = transcript;
    const group = window.OZGE.getClassState().activeGroup || "A";
    window.OZGE.updateClass(window.OZGE.state.activeClassId, (cls) => ({ speakingResults: [...cls.speakingResults, { group, transcript, score, time: Date.now() }] }));
    window.OZGE.Analytics.record("SPEAK", { score });
  }

  document.addEventListener("DOMContentLoaded", () => Speech.init());

  window.OZGE = window.OZGE || {};
  window.OZGE.Speech = Speech;
})();
