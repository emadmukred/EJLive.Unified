#!/bin/bash
# archive-originals.sh
# Copies tracked files into archive/originals preserving directory structure.
set -e
mkdir -p archive/originals
# use git ls-files to list tracked files and copy them
git ls-files | while read -r f; do
  mkdir -p "archive/originals/$(dirname "$f")"
  cp --parents "${f}" "archive/originals/" 2>/dev/null || cp "${f}" "archive/originals/${f}" 2>/dev/null || true
done

echo "Archive completed to archive/originals/"
