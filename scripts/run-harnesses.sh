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
"$engine" --headless --path . --script res://scripts/gameplay/logic_harness.gd
"$engine" --headless --path . --script res://scripts/gameplay/game_harness.gd
"$engine" --headless --path . --script res://scripts/save/save_harness.gd
