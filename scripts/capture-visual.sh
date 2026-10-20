#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
[[ -n "${DISPLAY:-}" ]] || { echo "Precisa de um display X11, por exemplo com xvfb-run."; exit 1; }
mkdir -p "$root/builds/review" "$root/.tools/visual/userdata" "$root/.tools/visual/config"
XDG_DATA_HOME="$root/.tools/visual/userdata" \
XDG_CONFIG_HOME="$root/.tools/visual/config" \
LIBGL_ALWAYS_SOFTWARE=1 \
"$root/.tools/godot/Godot_v4.7.2-stable_linux.x86_64" \
  --path "$root" --display-driver x11 --rendering-method gl_compatibility \
  --audio-driver Dummy --script res://scripts/capture_visual.gd
