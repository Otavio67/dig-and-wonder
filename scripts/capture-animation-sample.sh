#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
engine="$root/.tools/godot/Godot_v4.7.2-stable_linux.x86_64"
output="$root/docs/evidencias/20261020/amostra-animacoes.mp4"

[[ -x "$engine" ]] || { echo "Execute bash setup-cloud.sh primeiro."; exit 1; }
command -v ffmpeg >/dev/null || { echo "ffmpeg não encontrado."; exit 1; }
mkdir -p "$(dirname "$output")" "$root/.tools/logs"

if [[ -z "${DISPLAY:-}" ]]; then
  if [[ -x "$root/.tools/xvfb-root/usr/bin/xvfb-run" ]]; then
    export PATH="$root/.tools/xvfb-root/usr/bin:$PATH"
  fi
  command -v xvfb-run >/dev/null || { echo "A gravação requer X11 ou xvfb-run."; exit 1; }
  cd "$root"
  exec xvfb-run -a -s "-screen 0 640x1000x24 +extension GLX" \
    env DIG_ANIMATION_CAPTURE_XVFB=1 bash "$root/scripts/capture-animation-sample.sh"
fi

export LIBGL_ALWAYS_SOFTWARE=1
ffmpeg -hide_banner -loglevel error -y -f x11grab -framerate 30 \
  -video_size 540x960 -i "$DISPLAY.0+0,0" -an -c:v libx264 \
  -r 30 -fps_mode cfr -preset ultrafast -crf 23 -pix_fmt yuv420p -movflags +faststart \
  "$output" > "$root/.tools/logs/animation-capture-ffmpeg.log" 2>&1 &
recorder_pid=$!
finish_recording() {
  kill -INT "$recorder_pid" 2>/dev/null || true
  wait "$recorder_pid" 2>/dev/null || true
}
trap finish_recording EXIT

"$engine" --path "$root" --rendering-method gl_compatibility --audio-driver Dummy \
  --resolution 540x960 --position 0,0 \
  --script res://scripts/visual/capture_animation_sample.gd 2>&1 | tee "$root/.tools/logs/animation-capture-godot.log"
finish_recording
trap - EXIT
ffprobe -v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate \
  -of csv=p=0 "$output"
echo "Gravação salva: $output"
