#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

version="4.7.2"
archive="Godot_v${version}-stable_linux.x86_64.zip"
engine=".tools/godot/Godot_v${version}-stable_linux.x86_64"
release="https://github.com/godotengine/godot-builds/releases/download/${version}-stable"

if [[ "$(uname -s)" != "Linux" || "$(uname -m)" != "x86_64" ]]; then
  echo "Este instalador prepara Linux x86_64. Em outros sistemas, use o Godot 4.7.2 oficial."
  exit 1
fi

for dependency in curl unzip sha512sum; do
  command -v "$dependency" >/dev/null || { echo "Dependência ausente: $dependency"; exit 1; }
done

if [[ ! -x "$engine" ]]; then
  mkdir -p .tools/downloads .tools/godot
  curl --fail --location --retry 3 "$release/$archive" -o ".tools/downloads/$archive"
  curl --fail --location --retry 3 "$release/SHA512-SUMS.txt" -o .tools/downloads/SHA512-SUMS.txt
  (
    cd .tools/downloads
    awk -v filename="$archive" '$2 == filename || $2 == "*" filename { print; found=1 } END { if (!found) exit 1 }' SHA512-SUMS.txt > selected.sha512
    sha512sum --check selected.sha512
  )
  unzip -o ".tools/downloads/$archive" -d .tools/godot
  chmod +x "$engine"
fi

actual_version="$("$engine" --headless --version)"
[[ "$actual_version" == "$version.stable."* ]] || { echo "Versão inesperada: $actual_version"; exit 1; }
echo "Godot verificado: $actual_version"
