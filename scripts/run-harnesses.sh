#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."
engine="${GODOT_BIN:-.tools/godot/Godot_v4.7.2-stable_linux.x86_64}"
[[ -x "$engine" ]] || { echo "Execute bash setup-cloud.sh primeiro ou defina GODOT_BIN."; exit 1; }
mkdir -p .tools/home/.config .tools/home/.local/share .tools/home/.cache
export HOME="$PWD/.tools/home"
export XDG_CONFIG_HOME="$HOME/.config"
export XDG_DATA_HOME="$HOME/.local/share"
export XDG_CACHE_HOME="$HOME/.cache"
mkdir -p .tools/logs
run_harness() {
  local name="$1"
  local script="$2"
  local logfile=".tools/logs/$name.log"
  timeout 30s "$engine" --headless --path . --script "$script" 2>&1 | tee "$logfile"
  if grep -Eq 'SCRIPT ERROR:|Parse Error:|Assertion failed|ERROR:' "$logfile"; then
    echo "Falha no harness: $name"
    exit 1
  fi
}
run_harness logic res://scripts/gameplay/logic_harness.gd
run_harness gameplay res://scripts/gameplay/game_harness.gd
run_harness save res://scripts/save/save_harness.gd
run_harness android res://scripts/gameplay/android_harness.gd
