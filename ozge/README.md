# Özge – Classroom Edition

An offline-first classroom orchestration app built with pure HTML, CSS, and vanilla JavaScript. The experience ships with a Teacher Dashboard and a Projector View that stay in sync for dual-screen classrooms. Özge covers behavior tracking, Draw & Spell, quiz and puzzle game modes, speaking practice, calm-down sequences, AI-assisted lesson helpers, and automated GitHub publishing.

## Features

- **Dual-screen flow** – Launch `projector.html` from the Teacher Dashboard for a clean student-facing view. Synchronization uses `BroadcastChannel` with a localStorage snapshot fallback.
- **Four-class management** – Built-in classes (5A–5D) with isolated rosters, groups (A–H), attendance, unit packs, behavior history, lesson logs, and analytics.
- **Behavior and discipline suite** – Good/Warning/Penalty scoring, calm-down overlays with soft chime, emergency stop, freeze, privacy utilities, and auto bonus triggers after extended positive streaks.
- **Rich learning modes** – Quiz, Puzzle, Speaking, Story, Draw & Spell, Bonus Blitz, Calm Down, and Result states. Draw & Spell includes undo/redo, brush sizes, eraser, palette controls, word shuffling, and gallery snapshots.
- **Content Manager** – Import PDF or image files via drag-and-drop. Lightweight heuristics and offline OCR parsing cluster content into units, vocabulary lists, story panels, MCQs, and tags. Save and load portable class packs.
- **AI Lesson Helper** – Offline heuristics supply summaries, word definitions, synonym hints, and MCQ suggestions per unit.
- **Speaking practice** – Web Speech API integration (with simulated fallback) delivers transcript scoring, waveform feedback, and quick teacher awards.
- **Analytics** – Weekly accuracy charts, behavior timeline, speaking score history, export to JSON/CSV, and PDF-ready print sections.
- **Bonus mini-game** – Configurable blitz timer, streak bonuses, and group score integration.
- **Offline assets** – Local fonts, SVG icons, mini sound assets, and demo data ensure the app works out of the box without any CDN.
- **Automated publishing** – `/scripts/prepare_and_push.sh` handles committing, pushing to `main`, and mirroring `ozge/` onto `gh-pages`. GitHub Actions run the script on push, manual dispatch, and at 05:00 (GMT+3) weekdays.

## Project structure

```
/ozge/
  index.html              # Teacher dashboard launcher
  projector.html          # Projector / student view
  /css/styles.css         # Shared styles
  /js/                    # Modular vanilla JS feature files
  /assets/                # Inlineable icons and offline sounds
  /data/                  # Demo rosters, stopwords, unit packs
  /packs/                 # Import/export drop zone
  /scripts/prepare_and_push.sh
  /.github/workflows/deploy.yml
  README.md
```

## Getting started

1. Open `ozge/index.html` locally for the Teacher Dashboard.
2. Use the **Open Projector** button to spawn the student-facing window (`projector.html`). The BroadcastChannel and snapshot fallback maintain sync even when windows are refreshed.
3. Switch between classes, manage rosters, and import additional unit packs through the Content Manager.

Keyboard shortcuts on the Teacher Dashboard:

- `Space` – Pause to HOME
- `→ / ←` – Cycle modes
- `N` – Next Draw & Spell word (`P` for pen, `E` for eraser, `[` / `]` brush size, `S` save, `C` clear)

## Offline OCR & AI helper

- PDF parsing favors embedded text; otherwise, a lightweight brightness-based OCR fallback labels images. No external network calls.
- AI lesson helper applies frequency heuristics, stopword filtering, and rule-based difficulty tags to produce summaries, vocabulary hints, and MCQs entirely in-browser.

## GitHub auto-publish

### Required repository secrets

Set the following secrets (repository → Settings → Secrets and variables → Actions):

| Secret | Purpose |
| ------ | ------- |
| `GH_USERNAME` | Commit author name (e.g., `yasar-abravaya`) |
| `GH_EMAIL` | Commit author email |
| `GH_TOKEN` | Personal access token (repo scope) used by the script/workflow |
| `GH_REPO` | Target repository URL (`https://github.com/<owner>/<repo>.git`) |

### Local publishing

Run the helper script from the repository root:

```bash
./ozge/scripts/prepare_and_push.sh
```

The script will:

1. Configure git using `GH_USERNAME` / `GH_EMAIL`.
2. Commit pending changes with `feat: initial Özge app build` (if any).
3. Push to the `main` branch (creating the GitHub repo via API if required).
4. Publish the `/ozge` subtree to `gh-pages`.

Re-running the script is safe; it is idempotent and skips commits when no changes are staged.

### Continuous deployment

The GitHub Actions workflow performs the same script on:

- Every push to `main`
- Manual `workflow_dispatch`
- Weekdays at 05:00 Europe/Istanbul (02:00 UTC)

`deploy-gh-pages` posts the live preview URL in the job summary (e.g., `https://<owner>.github.io/<repo>/`).

To adjust the schedule time, edit the cron line inside `.github/workflows/deploy.yml`. Note: Istanbul is UTC+3, so 05:00 local corresponds to `0 2 * * 1-5` in UTC.

## Safety & moderation tools

- PIN-protected freeze, calm down, and emergency stop controls (default `1234`).
- Projector respects privacy states (blackout, freeze) and only shows approved Draw & Spell canvases.
- Emergency Stop engages freeze, mutes bonus audio, and displays a dark overlay.

## Data exports

- Behavior logs, analytics, and lesson notes export to JSON/CSV via the Teacher tabs.
- Class packs and lesson logs can be saved and re-imported per class.

Enjoy using Özge for engaging, offline-ready classroom sessions!
