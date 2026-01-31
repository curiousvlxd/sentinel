#!/usr/bin/env bash
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
VOLUMES_DIR="$REPO_ROOT/volumes"

for name in ground-db satellite-db-airbus-sentinel-1 satellite-db-airbus-sentinel-2 satellite-db-airbus-sentinel-3; do
  d="$VOLUMES_DIR/$name"
  if [ -d "$d" ]; then
    rm -rf "$d"/*
    echo "Cleared: $d"
  fi
done

if [ -d "$VOLUMES_DIR" ]; then
  chmod -R a+rwx "$VOLUMES_DIR"
  echo "chmod a+rwx on $VOLUMES_DIR"
fi

echo "Done. Start AppHost again for fresh DB init."
