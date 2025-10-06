#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

: "${GH_USERNAME?Need GH_USERNAME}"
: "${GH_EMAIL?Need GH_EMAIL}"
: "${GH_TOKEN?Need GH_TOKEN}"
: "${GH_REPO?Need GH_REPO}"

REMOTE_URL="https://${GH_TOKEN}@${GH_REPO#https://}"

if [ ! -d .git ]; then
  git init
fi

git config user.name "$GH_USERNAME"
git config user.email "$GH_EMAIL"

git checkout -B main

git add -A
if git diff --cached --quiet; then
  echo "No changes to commit on main"
else
  git commit -m "feat: initial Özge app build"
fi

if git remote | grep -q '^origin$'; then
  git remote set-url origin "$REMOTE_URL"
else
  git remote add origin "$REMOTE_URL"
fi

git push --set-upstream origin main

# Deploy ozge/ contents to gh-pages
WORKTREE_DIR="$(mktemp -d)"
cleanup() {
  git worktree remove "$WORKTREE_DIR" --force 2>/dev/null || true
  rm -rf "$WORKTREE_DIR"
}
trap cleanup EXIT

if git ls-remote --heads origin gh-pages >/dev/null 2>&1 && git ls-remote --heads origin gh-pages | grep -q gh-pages; then
  git worktree add "$WORKTREE_DIR" gh-pages
else
  git worktree add -B gh-pages "$WORKTREE_DIR"
fi

rsync -av --delete ozge/ "$WORKTREE_DIR"/ --exclude .git >/dev/null
cd "$WORKTREE_DIR"

git config user.name "$GH_USERNAME"
git config user.email "$GH_EMAIL"

git add -A
if git diff --cached --quiet; then
  echo "gh-pages is up to date"
else
  git commit -m "chore: update gh-pages"
  git push origin gh-pages
fi
