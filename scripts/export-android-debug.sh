#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
env_file="$root/.tools/android-env.sh"
engine="$root/.tools/godot/Godot_v4.7.2-stable_linux.x86_64"
apk="$root/builds/dig-and-wonder-android-debug.apk"

[[ -f "$env_file" ]] || { echo "Execute bash scripts/setup-android.sh primeiro."; exit 1; }
[[ -x "$engine" ]] || { echo "Este exportador é legado e gera um APK Godot. O projeto ativo usa Unity 6; consulte docs/UNITY-CONTEXTO-LOCAL.md."; exit 1; }
source "$env_file"
export JAVA_HOME="${JAVA_HOME:-$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")}"

mkdir -p "$root/builds"
"$engine" --headless --path "$root" --export-debug \
  'Android teste' "$apk"
python3 "$root/scripts/verify-android-apk.py" "$apk" --sdk "$ANDROID_HOME"
echo "APK de teste gerado: $apk"
