#!/usr/bin/env python3
"""Verificação estática do APK; não substitui execução num Android 4/16 KB."""
import argparse
import json
import re
import struct
import subprocess
import zipfile
from pathlib import Path


def command(*args):
    return subprocess.check_output([str(arg) for arg in args], text=True, stderr=subprocess.STDOUT)


def verify(apk, sdk):
    tools = sdk / "build-tools/36.0.0"
    command(tools / "apksigner", "verify", "--verbose", apk)
    command(tools / "zipalign", "-c", "-P", "16", "4", apk)
    badging = command(tools / "aapt", "dump", "badging", apk)
    permissions = command(tools / "aapt", "dump", "permissions", apk)
    manifest = command(tools / "aapt", "dump", "xmltree", apk, "AndroidManifest.xml")
    assert "name='com.otavio67.digandwonder'" in badging, "Pacote Android inesperado"
    version = (Path(__file__).resolve().parents[1] / "VERSION").read_text().strip()
    assert f"versionName='{version}'" in badging, "Versão do APK não corresponde a VERSION"
    target = int(re.search(r"targetSdkVersion:'(\d+)'", badging)[1])
    minimum = int(re.search(r"sdkVersion:'(\d+)'", badging)[1])
    assert target >= 36 and minimum == 24, "API alvo/mínima inesperada"
    assert "uses-permission" not in permissions, "Protótipo offline não deve pedir permissões"
    assert re.search(r"android:appCategory.*0x0\b", manifest), "App deve declarar categoria game"
    assert re.search(r"android:allowBackup.*0x0\b", manifest), "Backup de sistema não foi validado"
    native = []
    with zipfile.ZipFile(apk) as archive:
        assert archive.testzip() is None, "CRC inválido no APK"
        dev_paths = ("harness", "capture_visual", "revisao-android", "builds/review", "AdmobPlugin", "GMPShared", "android_ads")
        assert not any(any(fragment in name for fragment in dev_paths) for name in archive.namelist()), "APK contém ferramentas, anúncios de teste ou capturas de desenvolvimento"
        for name in archive.namelist():
            if not name.endswith(".so"):
                continue
            assert name.startswith("lib/arm64-v8a/"), f"ABI inesperada: {name}"
            data = archive.read(name)
            assert data[:6] == b"\x7fELF\x02\x01", f"ELF64 little-endian esperado: {name}"
            assert struct.unpack_from("<H", data, 18)[0] == 183, "Biblioteca não é AArch64"
            offset = struct.unpack_from("<Q", data, 32)[0]
            entry_size, count = struct.unpack_from("<HH", data, 54)
            loads = 0
            for index in range(count):
                entry = offset + index * entry_size
                if struct.unpack_from("<I", data, entry)[0] != 1:
                    continue
                file_offset, address = struct.unpack_from("<QQ", data, entry + 8)
                alignment = struct.unpack_from("<Q", data, entry + 48)[0]
                assert alignment >= 16384 and (address - file_offset) % 16384 == 0, f"PT_LOAD incompatível com 16 KB: {name}"
                loads += 1
            assert loads, f"ELF sem PT_LOAD: {name}"
            native.append({"file": name, "load_segments": loads, "alignment_16kb": True})
    assert native, "APK sem bibliotecas nativas"
    return {"version": version, "min_sdk": minimum, "target_sdk": target,
            "signature": "verified", "zip_alignment_16kb": True, "permissions": [],
            "native_libraries": native, "bytes": apk.stat().st_size,
            "runtime_android_tested": False}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", type=Path)
    parser.add_argument("--sdk", required=True, type=Path)
    args = parser.parse_args()
    print(json.dumps(verify(args.apk.resolve(), args.sdk.resolve()), indent=2, ensure_ascii=False))
