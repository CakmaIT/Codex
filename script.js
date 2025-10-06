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

const teams = ["Sunshines", "Rainbows", "Stars", "Heroes"];

const flashcardCategory = document.getElementById("flashcard-category");
const flashcardWord = document.getElementById("flashcard-word");
const flashcardMeaning = document.getElementById("flashcard-meaning");
const spinBtn = document.getElementById("spin-btn");
const quizQuestion = document.getElementById("quiz-question");
const quizOptionsContainer = document.getElementById("quiz-options");
const quizFeedback = document.getElementById("quiz-feedback");
const newQuizBtn = document.getElementById("new-quiz-btn");
const challengeDisplay = document.getElementById("challenge-display");
const newChallengeBtn = document.getElementById("new-challenge-btn");
const scoreboardContainer = document.getElementById("scoreboard");
const scoreTeamSelect = document.getElementById("score-team");
const scoreButtons = document.querySelectorAll(".score-btn");
const resetScoresBtn = document.getElementById("reset-scores");

let lastWordIndex = null;
let currentQuizAnswer = null;
const scores = Object.fromEntries(teams.map((team) => [team, 0]));

function shuffle(array) {
  return array
    .map((item) => ({ sort: Math.random(), value: item }))
    .sort((a, b) => a.sort - b.sort)
    .map((item) => item.value);
}

function pickRandomWord() {
  if (vocabulary.length === 0) {
    return null;
  }

  let index;
  do {
    index = Math.floor(Math.random() * vocabulary.length);
  } while (index === lastWordIndex && vocabulary.length > 1);

  lastWordIndex = index;
  return vocabulary[index];
}

function updateFlashcard() {
  const word = pickRandomWord();
  if (!word) return;

  flashcardCategory.textContent = word.category;
  flashcardWord.textContent = word.word;
  flashcardMeaning.textContent = word.meaning;
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

function renderScoreboard() {
  scoreboardContainer.innerHTML = "";
  teams.forEach((team) => {
    const card = document.createElement("div");
    card.className = "score-card";
    card.innerHTML = `
      <h3>${team}</h3>
      <p>${scores[team]}</p>
    `;
    scoreboardContainer.appendChild(card);
  });
}

function changeScore(team, delta) {
  scores[team] = Math.max(0, scores[team] + delta);
  renderScoreboard();
}

spinBtn.addEventListener("click", updateFlashcard);
newQuizBtn.addEventListener("click", createQuizQuestion);
newChallengeBtn.addEventListener("click", updateChallenge);

scoreButtons.forEach((button) => {
  button.addEventListener("click", () => {
    const team = scoreTeamSelect.value;
    const delta = Number(button.dataset.change);
    changeScore(team, delta);
  });
});

resetScoresBtn.addEventListener("click", () => {
  teams.forEach((team) => (scores[team] = 0));
  renderScoreboard();
});

// Başlangıç durumunu hazırla
renderScoreboard();
updateChallenge();
createQuizQuestion();
