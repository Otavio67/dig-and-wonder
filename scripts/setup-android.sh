#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
sdk="$root/.tools/android-sdk"
downloads="$root/.tools/downloads"
cmdline_version="16111833"
cmdline_sha1="e025545c62a8e64c7559119566a569fb1dec5f60"
godot_version="4.7.2"
template_name="Godot_v${godot_version}-stable_export_templates.tpz"
release_url="https://github.com/godotengine/godot-builds/releases/download/${godot_version}-stable"

if [[ "$(uname -s)" != "Linux" || "$(uname -m)" != "x86_64" ]]; then
  echo "Este instalador prepara Android para Linux x86_64."
  exit 1
fi

for dependency in curl unzip sha1sum sha512sum python3; do
  command -v "$dependency" >/dev/null || { echo "Dependência ausente: $dependency"; exit 1; }
done
command -v java >/dev/null || { echo "Instale OpenJDK 17 ou superior antes de continuar."; exit 1; }

java_bin="$(readlink -f "$(command -v java)")"
java_home="${JAVA_HOME:-$(dirname "$(dirname "$java_bin")")}"
java_major="$(java -version 2>&1 | sed -n '1s/.*version "\([0-9]*\).*/\1/p')"
if [[ -z "$java_major" || "$java_major" -lt 17 ]]; then
  echo "Godot Android precisa de OpenJDK 17 ou superior; encontrado: ${java_major:-desconhecido}."
  exit 1
fi

mkdir -p "$downloads" "$sdk/cmdline-tools/latest"
android="$sdk/cmdline-tools/latest/bin/android"
if [[ ! -x "$android" ]]; then
  archive="$downloads/commandlinetools-linux-${cmdline_version}_latest.zip"
  curl --fail --location --retry 3 \
    "https://dl.google.com/android/repository/commandlinetools-linux-${cmdline_version}_latest.zip" \
    -o "$archive"
  printf '%s  %s\n' "$cmdline_sha1" "$archive" | sha1sum --check
  unzip -q -o "$archive" -d "$sdk/cmdline-tools/latest"
  mv "$sdk/cmdline-tools/latest/cmdline-tools/"* "$sdk/cmdline-tools/latest/"
  rmdir "$sdk/cmdline-tools/latest/cmdline-tools"
fi

export ANDROID_HOME="$sdk"
export ANDROID_SDK_ROOT="$sdk"
export PATH="$sdk/cmdline-tools/latest/bin:$sdk/platform-tools:$PATH"
for package in \
  'platform-tools' \
  'platforms;android-36' \
  'build-tools;36.0.0' \
  'ndk;28.1.13356709' \
  'cmake;3.10.2.4988404'; do
  "$android" --sdk="$sdk" sdk install "$package"
done

template_dir="${XDG_DATA_HOME:-$HOME/.local/share}/godot/export_templates/${godot_version}.stable"
if [[ ! -f "$template_dir/android_debug.apk" || ! -f "$template_dir/android_source.zip" ]]; then
  mkdir -p "$template_dir"
  sums="$downloads/SHA512-SUMS.txt"
  template_archive="$downloads/$template_name"
  curl --fail --location --retry 3 "$release_url/SHA512-SUMS.txt" -o "$sums"
  curl --fail --location --retry 3 "$release_url/$template_name" -o "$template_archive"
  awk -v filename="$template_name" \
    '$2 == filename || $2 == "*" filename { print; found=1 } END { if (!found) exit 1 }' \
    "$sums" | (cd "$downloads" && sha512sum --check)
  unzip -j -o "$template_archive" 'templates/android_debug.apk' \
    'templates/android_release.apk' 'templates/android_source.zip' \
    'templates/version.txt' -d "$template_dir"
fi

env_file="$root/.tools/android-env.sh"
cat > "$env_file" <<'EOF'
#!/usr/bin/env bash
_DIG_WONDER_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export ANDROID_HOME="$_DIG_WONDER_ROOT/.tools/android-sdk"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$PATH"
unset _DIG_WONDER_ROOT
EOF

settings="${XDG_CONFIG_HOME:-$HOME/.config}/godot/editor_settings-4.7.tres"
mkdir -p "$(dirname "$settings")"
python3 - "$settings" "$sdk" "$java_home" <<'PY'
from pathlib import Path
import sys

settings = Path(sys.argv[1])
sdk_path, java_path = sys.argv[2:]
lines = settings.read_text().splitlines() if settings.exists() else [
    '[gd_resource type="EditorSettings" format=3]', '', '[resource]'
]
keys = ('export/android/android_sdk_path = ', 'export/android/java_sdk_path = ')
lines = [line for line in lines if not line.startswith(keys)]
lines.extend((
    f'export/android/android_sdk_path = "{sdk_path}"',
    f'export/android/java_sdk_path = "{java_path}"',
))
settings.write_text('\n'.join(lines) + '\n')
PY

echo "Android pronto para Godot $godot_version."
echo "SDK: $sdk"
echo "JDK: $java_home (OpenJDK $java_major)"
echo "Templates Android: $template_dir"
echo "Para exportar: bash scripts/export-android-debug.sh"
