#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
engine="$root/.tools/godot/Godot_v4.7.2-stable_linux.x86_64"
env_file="$root/.tools/android-env.sh"
test_project="$root/.tools/android-ad-test-project"
test_apk="$root/builds/dig-and-wonder-ad-test.apk"

[[ -x "$engine" ]] || { echo "Este exportador de anúncios é legado da versão Godot. O projeto ativo usa Unity 6; consulte UnityProject/README.md."; exit 1; }
[[ -f "$env_file" ]] || { echo "Execute bash scripts/setup-android.sh primeiro."; exit 1; }
source "$env_file"
mkdir -p "$root/.tools/ad-test-config/godot"
cat > "$root/.tools/ad-test-config/godot/editor_settings-4.7.tres" <<EOF
[gd_resource type="EditorSettings" format=3]

[resource]
export/android/android_sdk_path = "$ANDROID_HOME"
export/android/java_sdk_path = "${JAVA_HOME:-$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")}"
EOF
export XDG_CONFIG_HOME="$root/.tools/ad-test-config"

rm -rf "$test_project"
mkdir -p "$test_project"
tar -C "$root" --exclude=.git --exclude=.tools --exclude=.godot --exclude=builds \
  -cf - . | tar -C "$test_project" -xf -
python3 - "$test_project" <<'PY'
from pathlib import Path
import sys

project = Path(sys.argv[1])
settings = project / "project.godot"
source = settings.read_text()
old = 'run/main_scene="res://main.tscn"'
assert old in source
source = source.replace(old, 'run/main_scene="res://tests/android_ads/ads_test.tscn"', 1)
source += '\n[editor_plugins]\nenabled=PackedStringArray("res://addons/AdmobPlugin/plugin.cfg")\n'
settings.write_text(source)

preset_file = project / "export_presets.cfg"
preset = preset_file.read_text()
changes = {
    'export_path="builds/dig-and-wonder-android-debug.apk"': 'export_path="builds/dig-and-wonder-ad-test.apk"',
    'version/name="0.0.5"': 'version/name="0.0.5-adtest"',
    'package/unique_name="com.otavio67.digandwonder"': 'package/unique_name="com.otavio67.digandwonder.adtest"',
    'gradle_build/use_gradle_build=false': 'gradle_build/use_gradle_build=true',
    'permissions/internet=false': 'permissions/internet=true',
}
for old_value, new_value in changes.items():
    assert old_value in preset, f"Configuração Android não encontrada: {old_value}"
    preset = preset.replace(old_value, new_value, 1)
assert 'version/code=5' in preset, "Version code Android não sincronizado"
preset_file.write_text(preset)

# Este projeto temporário precisa do plugin e da cena técnica que o APK normal exclui.
if 'addons/AdmobPlugin/*' in preset:
    for development_path in ('addons/AdmobPlugin/*,', 'addons/GMPShared/*,', 'tests/android_ads/*'):
        preset = preset.replace(development_path, '')
    preset_file.write_text(preset)
PY

"$engine" --headless --editor --path "$test_project" --import
export JAVA_HOME="${JAVA_HOME:-$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")}"
"$engine" --headless --path "$test_project" --install-android-build-template \
  --export-debug 'Android teste' "$test_apk"
python3 - "$test_apk" "$ANDROID_HOME" <<'PY'
from pathlib import Path
import re
import subprocess
import sys
import zipfile

apk, sdk = Path(sys.argv[1]), Path(sys.argv[2])
tools = sdk / "build-tools/36.0.0"
def command(*args):
    return subprocess.check_output([str(a) for a in args], text=True, stderr=subprocess.STDOUT)

command(tools / "apksigner", "verify", "--verbose", apk)
command(tools / "zipalign", "-c", "-P", "16", "4", apk)
badging = command(tools / "aapt", "dump", "badging", apk)
manifest = command(tools / "aapt", "dump", "xmltree", apk, "AndroidManifest.xml")
assert "name='com.otavio67.digandwonder.adtest'" in badging
assert "versionName='0.0.5-adtest'" in badging
assert re.search(r"android.permission.INTERNET", command(tools / "aapt", "dump", "permissions", apk))
assert "ca-app-pub-3940256099942544~3347511713" in manifest, "App ID oficial de demonstração ausente"
assert "ca-app-pub-3940256099942544/5224354917" not in manifest, "Unit ID não deve ser metadado de manifesto"
with zipfile.ZipFile(apk) as archive:
    assert archive.testzip() is None
    names = archive.namelist()
    dex = b"".join(archive.read(name) for name in names if name.endswith(".dex"))
    assert b"org/godotengine/plugin/admob/AdmobPlugin" in dex, "Código nativo do plugin AdMob não foi integrado"
    assert not any("harness" in name for name in names)
print(f"AD_TEST_APK_OK signed=true api_target=36 test_ids_only=true internet_permission=true bytes={apk.stat().st_size}")
PY
echo "APK técnico de anúncio: $test_apk"
