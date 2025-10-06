const vocabulary = [
  {
    category: "People",
    word: "classmate",
    meaning: "sınıf arkadaşı"
  },
  {
    category: "People",
    word: "principal",
    meaning: "müdür"
  },
  {
    category: "People",
    word: "nurse",
    meaning: "hemşire"
  },
  {
    category: "Actions",
    word: "whisper",
    meaning: "fısıldamak"
  },
  {
    category: "Actions",
    word: "borrow",
    meaning: "ödünç almak"
  },
  {
    category: "Actions",
    word: "cheer",
    meaning: "tezahürat yapmak"
  },
  {
    category: "Places",
    word: "playground",
    meaning: "oyun alanı"
  },
  {
    category: "Places",
    word: "cafeteria",
    meaning: "yemekhane"
  },
  {
    category: "Places",
    word: "auditorium",
    meaning: "konferans salonu"
  },
  {
    category: "Feelings",
    word: "excited",
    meaning: "heyecanlı"
  },
  {
    category: "Feelings",
    word: "nervous",
    meaning: "güvensiz/gergin"
  },
  {
    category: "Feelings",
    word: "proud",
    meaning: "gururlu"
  },
  {
    category: "School Life",
    word: "attendance",
    meaning: "yoklama"
  },
  {
    category: "School Life",
    word: "experiment",
    meaning: "deney"
  },
  {
    category: "School Life",
    word: "art supplies",
    meaning: "resim malzemeleri"
  },
  {
    category: "Useful Phrases",
    word: "May I come in?",
    meaning: "İçeri girebilir miyim?"
  },
  {
    category: "Useful Phrases",
    word: "Can you repeat, please?",
    meaning: "Tekrar edebilir misiniz?"
  },
  {
    category: "Useful Phrases",
    word: "What's your favourite subject?",
    meaning: "En sevdiğin ders nedir?"
  }
];

const challenges = [
  "Takımınla ayağa kalk ve kelimeyi pantomimle anlat!",
  "Kelimeyi söylemeden çizerek arkadaşlarına anlat.",
  "Kelimeyi kullanarak kısa bir cümle kur ve sınıfla paylaş.",
  "Tüm sınıf kelimeyle ilgili bir ritimle alkış tutsun!",
  "Kelimeyi hızlıca hecele ve arkadaşına fısılda.",
  "Kelimeyle ilgili mini bir şarkı ya da tezahürat uydur.",
  "Kelimeyi drama yaparak canlandır.",
  "Kelimeyle ilgili üç ipucu düşün ve arkadaşlarının tahmin etmesini sağla.",
  "Sınıfa kelimeyi hatırlatacak bir hareket bulun ve herkes yapsın."
];

const prompts = [
  "Bugün kendini nasıl hissediyorsun? (feeling)",
  "En sevdiğin okul etkinliği ne?",
  "Okulda yardım istediğin biri kim olurdu?",
  "Zil çaldığında ne yaparsın?",
  "Takımınla birlikte en komik sınıf anınızı paylaşın."
];

const emojiStories = [
  {
    emojis: "🚀👩‍🚀🪐",
    prompt: "Bir uzay macerası anlat."
  },
  {
    emojis: "🍎📚🎒",
    prompt: "Okuldaki en eğlenceli günü hikâyeleştir."
  },
  {
    emojis: "🎤🎵🌟",
    prompt: "Bir yetenek gösterisi düşün ve anlat."
  },
  {
    emojis: "🏖️☀️🧃",
    prompt: "Yaz tatilindeki bir anıyı paylaş."
  },
  {
    emojis: "🐶🚲💨",
    prompt: "Hayvan arkadaşınla yaptığın bir macerayı anlat."
  },
  {
    emojis: "🎢🎠🍿",
    prompt: "Lunaparktaki en heyecanlı anı canlandır."
  },
  {
    emojis: "🏆⚽️🎯",
    prompt: "Takımınla kazandığın bir maçı anlat."
  }
];

const categoryIcons = {
  People: "🧑‍🤝‍🧑",
  Actions: "🏃",
  Places: "🏫",
  Feelings: "😊",
  "School Life": "📚",
  "Useful Phrases": "💬"
};

const students = [];
const scores = {};

let lastWordIndex = null;
let currentQuizAnswer = null;
let currentScrambleWord = null;
let lastPickedStudent = null;

const flashcardCategory = document.getElementById("flashcard-category");
const flashcardWord = document.getElementById("flashcard-word");
const flashcardMeaning = document.getElementById("flashcard-meaning");
const flashcardStudent = document.getElementById("flashcard-student");
const flashcardStudentBtn = document.getElementById("flashcard-student-btn");
const spinBtn = document.getElementById("spin-btn");
const wheel = document.getElementById("wheel");

const quizQuestion = document.getElementById("quiz-question");
const quizOptionsContainer = document.getElementById("quiz-options");
const quizFeedback = document.getElementById("quiz-feedback");
const newQuizBtn = document.getElementById("new-quiz-btn");
const quizStudent = document.getElementById("quiz-student");
const quizStudentBtn = document.getElementById("quiz-student-btn");

const challengeDisplay = document.getElementById("challenge-display");
const newChallengeBtn = document.getElementById("new-challenge-btn");
const challengeStudent = document.getElementById("challenge-student");
const challengeStudentBtn = document.getElementById("challenge-student-btn");

const scrambleWord = document.getElementById("scramble-word");
const scrambleHint = document.getElementById("scramble-hint");
const scrambleAnswer = document.getElementById("scramble-answer");
const newScrambleBtn = document.getElementById("new-scramble-btn");
const scrambleRevealBtn = document.getElementById("scramble-reveal-btn");

const emojiSet = document.getElementById("emoji-set");
const emojiPrompt = document.getElementById("emoji-prompt");
const newEmojiBtn = document.getElementById("new-emoji-btn");
const emojiStudent = document.getElementById("emoji-student");
const emojiStudentBtn = document.getElementById("emoji-student-btn");

const scoreboardContainer = document.getElementById("scoreboard");
const scoreSelect = document.getElementById("score-student");
const scoreButtons = document.querySelectorAll(".score-btn");
const resetScoresBtn = document.getElementById("reset-scores");

const studentForm = document.getElementById("student-form");
const studentInput = document.getElementById("student-name");
const studentTags = document.getElementById("student-tags");
const randomStudentBtn = document.getElementById("random-student");
const selectedStudentDisplay = document.getElementById("selected-student");

const menuButtons = document.querySelectorAll(".menu-btn");
const panels = document.querySelectorAll(".panel");

const wheelSegments = Array.from(
  new Set(vocabulary.map((item) => item.category))
);
const wheelColors = [
  "#ff8fab",
  "#b388eb",
  "#8ecae6",
  "#a7f3d0",
  "#ffcf99",
  "#f497b5",
  "#c4a7e7",
  "#90dbf4"
];
const spinDuration = 3600;

let isSpinning = false;
let currentWheelRotation = 0;

function shuffle(array) {
  return array
    .map((item) => ({ value: item, sort: Math.random() }))
    .sort((a, b) => a.sort - b.sort)
    .map((item) => item.value);
}

function pickRandomWord() {
  if (vocabulary.length === 0) return null;

  let index;
  do {
    index = Math.floor(Math.random() * vocabulary.length);
  } while (index === lastWordIndex && vocabulary.length > 1);

  lastWordIndex = index;
  return vocabulary[index];
}

function pickWordByCategory(category) {
  if (!category) return pickRandomWord();

  const options = vocabulary.filter((item) => item.category === category);
  if (options.length === 0) {
    return pickRandomWord();
  }

  return options[Math.floor(Math.random() * options.length)];
}

function updateFlashcard(word = null) {
  const selection = word ?? pickRandomWord();
  if (!selection) return;

  const wordIndex = vocabulary.indexOf(selection);
  if (wordIndex !== -1) {
    lastWordIndex = wordIndex;
  }

  flashcardCategory.textContent = selection.category;
  flashcardWord.textContent = selection.word;
  flashcardMeaning.textContent = selection.meaning;
}

function setupWheel() {
  if (!wheel) return;

  if (wheelSegments.length === 0) {
    wheel.style.setProperty("--wheel-gradient", "#f3e7ff 0deg 360deg");
    wheel.innerHTML = "";
    wheel.style.transform = "rotate(0deg)";
    updateWheelLabelPositions();
    return;
  }

  const segmentAngle = 360 / wheelSegments.length;
  const gradient = wheelSegments
    .map((_, index) => {
      const start = index * segmentAngle;
      const end = start + segmentAngle;
      const color = wheelColors[index % wheelColors.length];
      return `${color} ${start}deg ${end}deg`;
    })
    .join(", ");

  wheel.style.setProperty("--wheel-gradient", gradient);
  wheel.style.setProperty("--spin-duration", `${spinDuration}ms`);
  wheel.innerHTML = "";

  wheelSegments.forEach((category, index) => {
    const label = document.createElement("span");
    label.className = "wheel-label";
    label.style.setProperty("--angle", index * segmentAngle);
    const emoji = categoryIcons[category] ?? "🎯";
    label.innerHTML = `<span><span class="wheel-icon">${emoji}</span>${category}</span>`;
    wheel.appendChild(label);
  });

  currentWheelRotation = 0;
  wheel.style.transform = "rotate(0deg)";
  updateWheelLabelPositions();
}

function updateWheelLabelPositions() {
  if (!wheel) return;

  const boundingBox = wheel.getBoundingClientRect();
  const radius = Math.min(boundingBox.width, boundingBox.height) / 2;
  const labelDistance = Math.max(radius - 42, 70);
  wheel.style.setProperty("--label-distance", `${labelDistance}px`);

  wheel.querySelectorAll(".wheel-label").forEach((label) => {
    label.style.setProperty("--distance", `${labelDistance}px`);
  });
}

function spinWheel() {
  if (isSpinning || !wheel) return;

  const segmentAngle = 360 / wheelSegments.length;
  if (!Number.isFinite(segmentAngle)) {
    updateFlashcard();
    return;
  }

  isSpinning = true;
  spinBtn.disabled = true;
  flashcardCategory.textContent = "Çark dönüyor";
  flashcardWord.textContent = "🎡";
  flashcardMeaning.textContent = "Kategori seçiliyor...";
  wheel.classList.add("spinning");

  const selectedIndex = Math.floor(Math.random() * wheelSegments.length);
  const fullRotations = Math.floor(Math.random() * 3) + 4;
  const targetRotation =
    fullRotations * 360 + selectedIndex * segmentAngle + segmentAngle / 2;

  currentWheelRotation += targetRotation;

  // Force layout recalculation to ensure transition runs
  void wheel.offsetWidth;
  wheel.style.transform = `rotate(${currentWheelRotation}deg)`;

  const handleTransitionEnd = () => {
    clearTimeout(fallbackTimeout);
    const category = wheelSegments[selectedIndex];
    const word = pickWordByCategory(category);
    updateFlashcard(word);

    isSpinning = false;
    spinBtn.disabled = false;
    wheel.classList.remove("spinning");

    wheel.removeEventListener("transitionend", handleTransitionEnd);
  };

  const fallbackTimeout = setTimeout(handleTransitionEnd, spinDuration + 120);

  wheel.addEventListener("transitionend", handleTransitionEnd);
}

function createQuizQuestion() {
  const correctWord = pickRandomWord();
  if (!correctWord) return;

  quizQuestion.textContent = `"${correctWord.word}" kelimesinin Türkçesi nedir?`;
  currentQuizAnswer = correctWord.meaning;

  const options = new Set([correctWord.meaning]);
  while (options.size < 4 && options.size < vocabulary.length) {
    const randomMeaning =
      vocabulary[Math.floor(Math.random() * vocabulary.length)].meaning;
    options.add(randomMeaning);
  }

  quizOptionsContainer.innerHTML = "";
  shuffle([...options]).forEach((option) => {
    const button = document.createElement("button");
    button.className = "option-btn";
    button.textContent = option;
    button.addEventListener("click", () => handleQuizAnswer(button, option));
    quizOptionsContainer.appendChild(button);
  });

  quizFeedback.innerHTML = "&nbsp;";
}

function handleQuizAnswer(button, answer) {
  const optionButtons = quizOptionsContainer.querySelectorAll(".option-btn");
  optionButtons.forEach((btn) => (btn.disabled = true));

  if (answer === currentQuizAnswer) {
    button.classList.add("correct");
    quizFeedback.textContent = "Harika! Doğru cevap 🎉";
  } else {
    button.classList.add("wrong");
    quizFeedback.textContent = `Yaklaştın! Doğru cevap: ${currentQuizAnswer}`;
  }
}

function updateChallenge() {
  const challenge = challenges[Math.floor(Math.random() * challenges.length)];
  const prompt = prompts[Math.floor(Math.random() * prompts.length)];

  challengeDisplay.innerHTML = `
    <h3>${challenge}</h3>
    <p>${prompt}</p>
  `;
}

function scrambleWordText(word) {
  const letters = word.replace(/\s+/g, "").split("");
  return shuffle(letters)
    .map((letter) => letter.toUpperCase())
    .join(" ");
}

function updateScramble() {
  const selection = pickRandomWord();
  if (!selection) return;

  currentScrambleWord = selection.word;
  const scrambled = scrambleWordText(selection.word);
  scrambleWord.textContent = scrambled;
  scrambleHint.textContent = `İpucu: ${selection.category}`;
  scrambleAnswer.textContent = "Hazır olduğunda cevabı görmek için butona bas.";
}

function revealScramble() {
  if (!currentScrambleWord) return;
  scrambleAnswer.textContent = `Cevap: ${currentScrambleWord}`;
}

function updateEmojiStory() {
  const story = emojiStories[Math.floor(Math.random() * emojiStories.length)];
  emojiSet.textContent = story.emojis;
  emojiPrompt.textContent = story.prompt;
}

function renderScoreboard() {
  scoreboardContainer.innerHTML = "";

  if (students.length === 0) {
    const empty = document.createElement("p");
    empty.className = "empty-state";
    empty.textContent = "Önce öğrenci ekleyerek puan toplamaya başlayın.";
    scoreboardContainer.appendChild(empty);
    return;
  }

  students.forEach((student) => {
    const card = document.createElement("div");
    card.className = "score-card";
    card.innerHTML = `
      <h3>${student}</h3>
      <p>${scores[student] ?? 0}</p>
    `;
    scoreboardContainer.appendChild(card);
  });
}

function updateScoreSelect() {
  scoreSelect.innerHTML = "";

  students.forEach((student) => {
    const option = document.createElement("option");
    option.value = student;
    option.textContent = student;
    scoreSelect.appendChild(option);
  });

  scoreSelect.disabled = students.length === 0;
  scoreButtons.forEach((button) => {
    button.disabled = students.length === 0;
  });
  resetScoresBtn.disabled = students.length === 0;

  if (students.length > 0) {
    scoreSelect.value = students[students.length - 1];
  }
}

function changeScore(student, delta) {
  if (!student) return;
  scores[student] = Math.max(0, (scores[student] ?? 0) + delta);
  renderScoreboard();
}

function resetScores() {
  students.forEach((student) => {
    scores[student] = 0;
  });
  renderScoreboard();
}

function renderStudents() {
  studentTags.innerHTML = "";

  if (students.length === 0) {
    const info = document.createElement("p");
    info.className = "empty-state";
    info.textContent = "Sahneye çıkacak ilk öğrenciyi ekleyin.";
    studentTags.appendChild(info);
    return;
  }

  students.forEach((student) => {
    const tag = document.createElement("span");
    tag.className = "student-tag";
    tag.innerHTML = `
      ${student}
      <button type="button" class="remove" data-remove="${student}">×</button>
    `;
    studentTags.appendChild(tag);
  });
}

function addStudent(name) {
  const trimmed = name.trim();
  if (!trimmed) return;

  const formatted =
    trimmed.charAt(0).toUpperCase() + trimmed.slice(1).toLowerCase();
  if (students.includes(formatted)) {
    studentInput.value = "";
    studentInput.focus();
    return;
  }

  students.push(formatted);
  scores[formatted] = scores[formatted] ?? 0;
  renderStudents();
  renderScoreboard();
  updateScoreSelect();
  studentInput.value = "";
  studentInput.focus();
}

function removeStudent(name) {
  const index = students.indexOf(name);
  if (index === -1) return;

  students.splice(index, 1);
  delete scores[name];

  if (lastPickedStudent === name) {
    lastPickedStudent = null;
  }

  renderStudents();
  renderScoreboard();
  updateScoreSelect();
  selectedStudentDisplay.textContent = students.length
    ? "Hazır olan var mı?"
    : "Öğrenci ekleyerek başlayın!";

  if (students.length === 0) {
    flashcardStudent.textContent = "Öğrenci seçmek için listeden ekleme yap.";
    quizStudent.textContent = "Bir öğrenci seçmek için listeden ekleme yap.";
    challengeStudent.textContent = "Göreve lider olacak öğrenciyi seçelim!";
    emojiStudent.textContent = "Emojilere can katacak öğrenciyi seç!";
  }
}

function pickRandomStudent() {
  if (students.length === 0) {
    return null;
  }

  let student;
  do {
    student = students[Math.floor(Math.random() * students.length)];
  } while (students.length > 1 && student === lastPickedStudent);

  lastPickedStudent = student;
  return student;
}

function announceRandomStudent(targetElement) {
  const student = pickRandomStudent();
  if (!student) {
    targetElement.textContent = "Öğrenci ekleyerek seçim yapabilirsiniz.";
    return;
  }

  targetElement.textContent = `${student}! Hazırsan sahne senin ✨`;
}

function handleMenuClick(event) {
  const button = event.currentTarget;
  const target = button.dataset.target;

  menuButtons.forEach((btn) => btn.classList.remove("active"));
  panels.forEach((panel) => panel.classList.remove("active"));

  button.classList.add("active");
  document.getElementById(target).classList.add("active");
}

studentForm.addEventListener("submit", (event) => {
  event.preventDefault();
  addStudent(studentInput.value);
});

studentTags.addEventListener("click", (event) => {
  const button = event.target;
  if (button.matches("[data-remove]")) {
    removeStudent(button.dataset.remove);
  }
});

randomStudentBtn.addEventListener("click", () => {
  const student = pickRandomStudent();
  selectedStudentDisplay.textContent = student
    ? `${student}! Bugünün yıldızı sensin!`
    : "Öğrenci ekleyerek seçim yapabilirsiniz.";
});

flashcardStudentBtn.addEventListener("click", () =>
  announceRandomStudent(flashcardStudent)
);

quizStudentBtn.addEventListener("click", () =>
  announceRandomStudent(quizStudent)
);

challengeStudentBtn.addEventListener("click", () =>
  announceRandomStudent(challengeStudent)
);

emojiStudentBtn.addEventListener("click", () =>
  announceRandomStudent(emojiStudent)
);

spinBtn.addEventListener("click", spinWheel);
newQuizBtn.addEventListener("click", createQuizQuestion);
newChallengeBtn.addEventListener("click", updateChallenge);
newScrambleBtn.addEventListener("click", updateScramble);
scrambleRevealBtn.addEventListener("click", revealScramble);
newEmojiBtn.addEventListener("click", updateEmojiStory);

scoreButtons.forEach((button) => {
  button.addEventListener("click", () => {
    changeScore(scoreSelect.value, Number(button.dataset.change));
  });
});

resetScoresBtn.addEventListener("click", resetScores);

menuButtons.forEach((button) => {
  button.addEventListener("click", handleMenuClick);
});

let resizeTimeoutId;
window.addEventListener("resize", () => {
  clearTimeout(resizeTimeoutId);
  resizeTimeoutId = setTimeout(updateWheelLabelPositions, 120);
});

setupWheel();
renderStudents();
renderScoreboard();
updateScoreSelect();
updateFlashcard();
createQuizQuestion();
updateChallenge();
updateScramble();
updateEmojiStory();
