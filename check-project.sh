#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"
engine="${GODOT_BIN:-.tools/godot/Godot_v4.7.2-stable_linux.x86_64}"
[[ -x "$engine" ]] || { echo "Execute bash setup-cloud.sh primeiro ou defina GODOT_BIN."; exit 1; }

mkdir -p .tools/logs .tools/home/.config .tools/home/.local/share .tools/home/.cache
export HOME="$PWD/.tools/home"
export XDG_CONFIG_HOME="$HOME/.config"
export XDG_DATA_HOME="$HOME/.local/share"
export XDG_CACHE_HOME="$HOME/.cache"
run_check() {
  local name="$1"
  shift
  local logfile=".tools/logs/$name.log"
  "$engine" --headless --path . "$@" 2>&1 | tee "$logfile"
  if grep -Eq 'SCRIPT ERROR:|Parse Error:|ERROR:' "$logfile" && ! grep -q 'Condition "_sock == -1" is true. Returning: FAILED' "$logfile"; then
    echo "Falha na verificação: $name"
    exit 1
  fi
}

run_check import --editor --import
run_check boot --quit-after 5
echo "Importação e execução inicial passaram. Visual, Android e anúncios ainda exigem validação própria."
