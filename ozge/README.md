# Özge – Classroom Edition

Offline-first classroom toolkit featuring a teacher dashboard, projector view, behavior tracking, content management with OCR, analytics, and deployment automation.

## Features

- **Dual-screen workflow** – Launch a synced Projector View (via `window.open`) with BroadcastChannel and localStorage fallback.
- **Mode hub** – Quiz, Puzzle, Speak, Story, Draw & Spell, Bonus Blitz, Calm Down overlay, and teacher-only tabs for content, analytics, logs, settings, and class admin.
- **Split reveal quiz** – Projector view shows questions while answer choices stay private on the teacher dashboard for guided pacing.
- **Behavior & discipline** – Group scoring, quick behavior buttons (good, warning, penalty), automatic Team Spirit bonus, calm overlay, attendance capture, and lesson log entries.
- **Draw & Spell** – Touch-friendly canvas with undo/redo, palette, PNG export, per-class gallery, and approve-to-projector with PIN gate.
- **Content Manager** – Import PDF/image files, crop images offline, perform heuristic OCR, auto-cluster into units, map units to modes, and manage portable unit packs.
- **AI Lesson Helper** – Rule-based summaries, vocabulary definitions, MCQs with difficulty tags per unit.
- **Speaking practice** – Web Speech API integration with offline scoring fallback, lesson log storage, manual bonus.
- **Weekly analytics** – Accuracy, difficult words, behavior trends, export to JSON/CSV, and print-ready view.
- **Automation** – `scripts/prepare_and_push.sh` prepares commits, pushes `main`, and publishes gh-pages; GitHub Actions workflow triggers on pushes, weekdays 05:00 Europe/Istanbul, or manual dispatch.

## Getting Started

1. Open `ozge/index.html` in a modern browser. The Teacher Dashboard runs fully offline.
2. Launch the Projector View from the dashboard header to open `projector.html` in a new window.
3. Switch among the four preset classes (5A–5D); each maintains isolated units, rosters, scores, logs, and snapshots.

## Content Management

- Drag-and-drop or select PDF/image files in the Content Manager tab.
- For images, adjust crop sliders and run offline OCR to extract clean text.
- Units auto-generate with summaries, vocabulary lists, and MCQs; map units to activity modes via chips.
- Export/import portable unit packs (`.json`) scoped per class.
- Imported files are saved to a local IndexedDB store per class. Review, download, or delete them from the **Stored Files** panel.

## Offline Stored Files

- Imported PDFs/images stay on the current device inside an IndexedDB database (no remote upload).
- Each class keeps its own library; switching classes refreshes the stored list.
- Download buttons rebuild the original file blob so you can archive or reimport later.
- Delete actions prompt for confirmation and immediately remove the file from storage.

## Desktop Packaging (EXE)

Package the web app as a desktop executable using the bundled Electron configuration:

```bash
cd ozge
./scripts/build_exe.sh win   # Windows 64-bit build
./scripts/build_exe.sh linux # Linux 64-bit build
```

- Requires Node.js 18+ with npm available on the packaging machine.
- The script installs dev dependencies on first run and caches them under `node_modules`.
- Windows builds output to `ozge/dist/Ozge Classroom-win32-x64/` with `Ozge Classroom.exe` ready for offline distribution.
- Linux builds emit an AppImage-style directory under `dist/`.
- You can launch a desktop preview via `npm start` (after `npm install`) which wraps `index.html` inside an Electron shell.

## Behavior & Moderation

- Behavior controls adjust scores and add log entries (penalties require PIN, default `1234`).
- Calm Down Mode activates during Emergency STOP, dimming screens and muting interactions.
- All sensitive actions prompt for PIN; update the code under **Settings**.

## Deployment Automation

### Required Secrets

Configure the following repository secrets so CI can publish automatically:

| Secret | Purpose |
| --- | --- |
| `GH_USERNAME` | Git username used for CI commits |
| `GH_EMAIL` | Email for CI commits |
| `GH_TOKEN` | Personal access token with `repo` scope |
| `GH_REPO` | Full HTTPS repository URL (e.g., `https://github.com/owner/ozge-classroom.git`) |

### Local Push Script

Run the helper script from the repository root:

```bash
GH_USERNAME="your-username" \
GH_EMAIL="you@example.com" \
GH_TOKEN="ghp_xxx" \
GH_REPO="https://github.com/owner/ozge-classroom.git" \
bash scripts/prepare_and_push.sh
```

The script commits all changes to `main`, pushes to the remote, then updates the `gh-pages` branch using the `ozge/` directory contents.

### GitHub Actions

Workflow file: `.github/workflows/deploy.yml`

- Triggers on push to `main`, scheduled weekdays at 05:00 Europe/Istanbul (`0 2 * * 1-5` UTC), and manual `workflow_dispatch`.
- `build` job checks out the repo and runs `scripts/prepare_and_push.sh` using repo secrets.
- `deploy-gh-pages` job ensures the `gh-pages` branch exists, pushes it with `GH_TOKEN`, and posts the site URL summary.

The published site lives at `https://<owner>.github.io/<repository>/` after gh-pages updates.

## Adjusting the Schedule

The cron line in the workflow can be edited to change timing. Remember Istanbul time is UTC+3 with no DST; adjust accordingly before converting to UTC.

## Safety & Accessibility

- Minimum touch target ≥56px, high-contrast gradient theme, ARIA labels, and keyboard shortcuts for drawing (N, C, S, [, ], E, P).
- Offline heuristics avoid external network calls; OCR and AI helpers run locally.
- No third-party CDNs or dynamic imports; all assets bundle inside `/ozge`.
