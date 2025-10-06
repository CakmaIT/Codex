#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || git init

git config user.name "${GH_USERNAME:-ozge-bot}"
git config user.email "${GH_EMAIL:-ozge-bot@example.com}"

git add -A
if git diff --cached --quiet; then
  echo "No changes to commit"
else
  git commit -m "feat: initial Özge app build" || true
fi

git branch -M main

if [ -n "${GH_REPO:-}" ]; then
  REMOTE_NAME="origin"
  if ! git remote | grep -q "^${REMOTE_NAME}$"; then
    REMOTE_URL="https://${GH_REPO#https://}"
    if [ -n "${GH_TOKEN:-}" ]; then
      REMOTE_URL="https://${GH_TOKEN}@${GH_REPO#https://}"
    fi
    git remote add "$REMOTE_NAME" "$REMOTE_URL"
  fi
  ensure_repo_exists() {
    if [ -z "${GH_TOKEN:-}" ]; then
      return
    fi
    local slug="${GH_REPO#https://github.com/}"
    slug="${slug%.git}"
    local api="https://api.github.com/repos/${slug}"
    if curl -fs -H "Authorization: token ${GH_TOKEN}" "$api" >/dev/null; then
      return
    fi
    local name="${slug##*/}"
    local owner="${slug%/*}"
    curl -fs -X POST \
      -H "Authorization: token ${GH_TOKEN}" \
      -H "Content-Type: application/json" \
      -d "{\"name\":\"${name}\",\"private\":false}" \
      https://api.github.com/user/repos >/dev/null
  }
  ensure_repo_exists
  if [ -n "${GH_TOKEN:-}" ]; then
    git push "https://${GH_TOKEN}@${GH_REPO#https://}" main || true
  else
    git push origin main || true
  fi
  if git rev-parse --verify main >/dev/null 2>&1; then
    SHA=$(git subtree split --prefix=ozge main || true)
    if [ -n "$SHA" ]; then
      if [ -n "${GH_TOKEN:-}" ]; then
        git push "https://${GH_TOKEN}@${GH_REPO#https://}" "$SHA:gh-pages" --force || true
      else
        git push origin "$SHA:gh-pages" --force || true
      fi
    fi
  fi
fi
