#!/usr/bin/env bash
# Publish the prebuilt Build/Web to a gh-pages branch (root) for GitHub Pages.
# Build first with tools/build-web.sh. Usage: tools/deploy-pages.sh [remote] [branch]
set -euo pipefail

REPO="$(cd "$(dirname "$0")/.." && pwd)"
BUILD="$REPO/Build/Web"
REMOTE="${1:-$(git -C "$REPO" remote | grep -qx origin && echo origin || git -C "$REPO" remote | head -1)}"
BRANCH="${2:-gh-pages}"
WORKTREE="$(mktemp -d)"

[ -f "$BUILD/index.html" ] || { echo "No build at $BUILD. Run tools/build-web.sh first."; exit 1; }
[ -n "$REMOTE" ] || { echo "No git remote found."; exit 1; }

cleanup() {
  git -C "$REPO" worktree remove --force "$WORKTREE" 2>/dev/null || rm -rf "$WORKTREE"
  git -C "$REPO" branch -D "$BRANCH" 2>/dev/null || true
}
trap cleanup EXIT

git -C "$REPO" branch -D "$BRANCH" 2>/dev/null || true
git -C "$REPO" worktree add --orphan -b "$BRANCH" "$WORKTREE"

cp -R "$BUILD/." "$WORKTREE/"
touch "$WORKTREE/.nojekyll" # Jekyll would drop TemplateData and other files it doesn't understand
git -C "$WORKTREE" add -A
git -C "$WORKTREE" commit -q -m "Deploy WebGL build $(date -u +%Y-%m-%dT%H:%M:%SZ)"
git -C "$WORKTREE" push -f "$REMOTE" "HEAD:$BRANCH"

URL="$(git -C "$REPO" remote get-url "$REMOTE" | sed -E 's#(git@github.com:|https://github.com/)##; s#\.git$##')"
OWNER="${URL%%/*}"; NAME="${URL##*/}"
echo "Pushed to $REMOTE/$BRANCH. Set Pages source to $BRANCH / (root) in repo settings."
echo "Site: https://${OWNER}.github.io/${NAME}/"
