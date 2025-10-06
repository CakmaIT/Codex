#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if ! command -v npm >/dev/null 2>&1; then
  echo "npm is required to package the desktop build." >&2
  exit 1
fi

cd "$ROOT_DIR"

if [ ! -d node_modules ]; then
  npm install
fi

TARGET=${1:-win}
case "$TARGET" in
  win)
    npm run package:win
    ;;
  linux)
    npm run package:linux
    ;;
  *)
    echo "Unknown target '$TARGET'. Use 'win' or 'linux'." >&2
    exit 1
    ;;
esac

