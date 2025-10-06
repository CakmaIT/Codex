(function () {
  let recognition = null;
  let available = false;

  function init() {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (SpeechRecognition) {
      recognition = new SpeechRecognition();
      recognition.lang = "en-US";
      recognition.continuous = false;
      recognition.interimResults = false;
      available = true;
    }
  }

  function startListening(onResult, onError) {
    if (!available) {
      setTimeout(() => onResult({ transcript: "", score: simulateScore() }), 500);
      return () => {};
    }
    recognition.start();
    recognition.onresult = (event) => {
      const transcript = Array.from(event.results)
        .map((result) => result[0])
        .map((result) => result.transcript)
        .join(" ");
      onResult({ transcript, score: evaluateTranscript(transcript) });
    };
    recognition.onerror = (event) => {
      onError(event.error);
    };
    return () => recognition.stop();
  }

  function evaluateTranscript(transcript) {
    const words = transcript.trim().split(/\s+/).filter(Boolean);
    if (!words.length) return 40;
    const averageLength = words.reduce((acc, w) => acc + w.length, 0) / words.length;
    const clarity = Math.min(1, words.length / 10);
    const score = Math.round(50 + clarity * 30 + Math.min(10, averageLength * 2));
    return Math.min(100, Math.max(0, score));
  }

  function simulateScore() {
    return 60 + Math.round(Math.random() * 40);
  }

  init();

  window.Speaking = {
    startListening,
    available: () => available
  };
})();
