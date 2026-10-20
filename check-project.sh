#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"
engine="${GODOT_BIN:-.tools/godot/Godot_v4.7.2-stable_linux.x86_64}"
[[ -x "$engine" ]] || { echo "Execute bash setup-cloud.sh primeiro ou defina GODOT_BIN."; exit 1; }

mkdir -p .tools/logs .tools/home/.config .tools/home/.local/share .tools/home/.cache
task_home="$PWD/.tools/home"
export XDG_CONFIG_HOME="$task_home/.config"
export XDG_DATA_HOME="$task_home/.local/share"
export XDG_CACHE_HOME="$task_home/.cache"
if [[ -x .tools/android-sdk/platform-tools/adb ]] && command -v java >/dev/null; then
  export ANDROID_HOME="$PWD/.tools/android-sdk"
  export ANDROID_SDK_ROOT="$ANDROID_HOME"
  export JAVA_HOME="${JAVA_HOME:-$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")}"
  mkdir -p "$XDG_CONFIG_HOME/godot"
  cat > "$XDG_CONFIG_HOME/godot/editor_settings-4.7.tres" <<EOF
[gd_resource type="EditorSettings" format=3]

[resource]
export/android/android_sdk_path = "$ANDROID_HOME"
export/android/java_sdk_path = "$JAVA_HOME"
EOF
fi
run_check() {
  local name="$1"
  shift
  local logfile=".tools/logs/$name.log"
  "$engine" --headless --path . "$@" 2>&1 | tee "$logfile"
  if grep -Eq 'SCRIPT ERROR:|Parse Error:' "$logfile" || \
    grep 'ERROR:' "$logfile" | \
      grep -Fv 'Condition "_sock == -1" is true. Returning: FAILED' | \
      grep -Fv 'Condition "err != OK" is true. Returning: ERR_CANT_CREATE' | \
      grep -q .; then
    echo "Falha na verificação: $name"
    exit 1
  fi
}

run_check import --editor --import
run_check boot --quit-after 5
echo "Importação e execução inicial passaram. Visual Android, anúncio servido e desempenho ainda exigem validação em aparelho."
