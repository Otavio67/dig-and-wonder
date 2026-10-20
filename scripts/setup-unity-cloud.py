#!/usr/bin/env python3
"""Instala o toolchain Unity em Linux x86_64 sem root e sem alterar o projeto."""

import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import tarfile
import time


ROOT = Path(__file__).resolve().parent.parent
TOOLS = ROOT / ".tools"
DOWNLOADS = TOOLS / "unity-downloads"
UNITY_VERSION = "6000.0.23f1"
IMAGE = "registry-1.docker.io/unityci/editor@sha256:32f907942bf246c2ae431c30208701e7365dcfe7a68ef79382444f9b2e0bb2e3"
EDITOR_LAYER = "sha256:587635c77ea92d6cd08abc4052628b8ca1805848120dd12fe8aeb32a0812ed5d"
CRANE_VERSION = "0.20.3"
CRANE_SHA256 = "36c67a932f489b3f2724b64af90b599a8ef2aa7b004872597373c0ad694dc059"
DOTNET_VERSION = "8.0.413"
DOTNET_SHA512 = "f173346751b2922887bb2fb1eb48fc88df9ac62cb7e8eac697f9a4cde17ad84f926dd02fba896968f1425b566fba3127b781dabeb74a088ed29a951d427e178a"


def checked_archive(name, url, expected_hash, algorithm="sha256"):
    archive = DOWNLOADS / name
    if not archive.exists():
        # Mantém proxy e certificados do ambiente; nunca desabilita TLS.
        subprocess.run(["curl", "--fail", "--location", "--show-error", "--retry", "2",
                        "--output", str(archive) + ".part", url], check=True)
        Path(str(archive) + ".part").rename(archive)
    digest = hashlib.new(algorithm)
    with archive.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    if digest.hexdigest() != expected_hash:
        raise RuntimeError(f"Checksum inválido: {archive}. Arquivo preservado para diagnóstico.")
    return archive


def ensure_dotnet():
    executable = TOOLS / "dotnet/dotnet"
    if executable.is_file():
        version = subprocess.check_output([str(executable), "--version"], text=True).strip()
        if version == DOTNET_VERSION:
            print(f".NET {version} já instalado.", flush=True)
            return
        raise RuntimeError(f".NET existente é {version}; não será substituído automaticamente.")
    archive = checked_archive("dotnet-sdk.tar.gz",
        f"https://builds.dotnet.microsoft.com/dotnet/Sdk/{DOTNET_VERSION}/dotnet-sdk-{DOTNET_VERSION}-linux-x64.tar.gz",
        DOTNET_SHA512, "sha512")
    executable.parent.mkdir(parents=True, exist_ok=True)
    with tarfile.open(archive) as package:
        package.extractall(executable.parent, filter="data")


def ensure_crane():
    archive = checked_archive("crane.tar.gz",
        f"https://github.com/google/go-containerregistry/releases/download/v{CRANE_VERSION}/go-containerregistry_Linux_x86_64.tar.gz",
        CRANE_SHA256)
    binary_directory = TOOLS / "bin"
    binary_directory.mkdir(exist_ok=True)
    with tarfile.open(archive) as package:
        package.extract("crane", binary_directory, filter="data")
    return binary_directory / "crane"


def portable_link(member, staging):
    # O NDK r23b inclui links absolutos do servidor de build do Android. A
    # correspondência é específica: todos continuam dentro do mesmo NDK local.
    ndk_prefix = "/buildbot/src/android/ndk-release-r23/out/linux/android-ndk-r23b/"
    if (member.issym() or member.islnk()) and member.linkname.startswith(ndk_prefix):
        member.linkname = "/opt/unity/Editor/Data/PlaybackEngines/AndroidPlayer/NDK/" + member.linkname[len(ndk_prefix):]
    if (member.issym() or member.islnk()) and member.linkname.startswith("/opt/unity/"):
        target = member.linkname.lstrip("/")
        member.linkname = os.path.relpath(staging / target, (staging / member.name).parent) if member.issym() else target
        return True
    return False


def ensure_editor(crane):
    destination = TOOLS / "unity" / UNITY_VERSION
    marker = destination / ".gameci-source.json"
    if destination.exists():
        if marker.exists():
            source = json.loads(marker.read_text())
            if source.get("layer") == EDITOR_LAYER and source.get("verified") and (destination / "Editor/Unity").is_file():
                print(f"Unity {UNITY_VERSION} já instalado, com camada verificada.", flush=True)
                return
        raise RuntimeError(f"Instalação existente não reconhecida em {destination}; não será sobrescrita.")

    staging = destination.with_name(UNITY_VERSION + ".download")
    staging.mkdir(parents=True, exist_ok=True)
    process = subprocess.Popen([str(crane), "blob", "registry-1.docker.io/unityci/editor@" + EDITOR_LAYER], stdout=subprocess.PIPE)
    digest = hashlib.sha256()

    class DigestReader:
        def read(self, size=-1):
            data = process.stdout.read(size)
            digest.update(data)
            return data

    reader = DigestReader()
    total = 0
    rebased_links = 0
    last_report = time.monotonic()
    try:
        # A imagem fixa contém o Editor + Android nesta camada. Não duplica os
        # 6,6 GiB comprimidos em disco, nem instala o sistema operacional da imagem.
        with tarfile.open(fileobj=reader, mode="r|gz", bufsize=1024 * 1024) as package:
            for member in package:
                if not member.name.startswith("opt/unity/"):
                    continue
                if member.size > shutil.disk_usage(staging).free - 512 * 1024**2:
                    raise RuntimeError("Espaço insuficiente; extração parcial preservada para diagnóstico.")
                rebased_links += portable_link(member, staging)
                try:
                    package.extract(member, staging, filter="data")
                except tarfile.FilterError as error:
                    raise RuntimeError(f"Link não portátil: {member.name} => {member.linkname!r}") from error
                total += member.size
                if time.monotonic() - last_report > 15:
                    print(f"Extraídos {total / 1024**3:.2f} GiB; livres {shutil.disk_usage(staging).free / 1024**3:.2f} GiB.", flush=True)
                    last_report = time.monotonic()
        while reader.read(1024 * 1024):
            pass
        if process.wait() != 0:
            raise RuntimeError("Download da camada Unity falhou.")
        if "sha256:" + digest.hexdigest() != EDITOR_LAYER:
            raise RuntimeError("SHA-256 da camada Unity não confere; instalação não ativada.")
        (staging / "opt/unity").rename(destination)
        marker.write_text(json.dumps({"image": IMAGE, "layer": EDITOR_LAYER, "verified": True,
            "extractedBytes": total, "rebasedAbsoluteSymlinks": rebased_links}, indent=2) + "\n")
        shutil.rmtree(staging)
        print(f"Unity instalado: {destination}", flush=True)
    finally:
        if process.poll() is None:
            process.terminate()
            process.wait()


def prepare_android():
    android = TOOLS / "unity" / UNITY_VERSION / "Editor/Data/PlaybackEngines/AndroidPlayer"
    accepted_licenses = TOOLS / "android-sdk/licenses"
    if accepted_licenses.is_dir():
        for source in accepted_licenses.iterdir():
            target = android / "SDK/licenses" / source.name
            if source.is_file() and not target.exists():
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)

    # O JDK embutido mantém suas raízes normais e recebe a CA pública da sessão
    # numa cópia local. O certificado não entra no Git nem no conteúdo do jogo.
    proxy_ca = os.environ.get("CODEX_PROXY_CERT")
    if proxy_ca and Path(proxy_ca).is_file():
        trust_directory = TOOLS / "unity/trust"
        trust_directory.mkdir(exist_ok=True)
        fingerprint = hashlib.sha256(Path(proxy_ca).read_bytes()).hexdigest()
        marker = trust_directory / "proxy-ca.sha256"
        if not marker.exists() or marker.read_text() != fingerprint:
            trust_store = trust_directory / "cacerts"
            shutil.copyfile(android / "OpenJDK/lib/security/cacerts", trust_store)
            subprocess.run([str(android / "OpenJDK/bin/keytool"), "-importcert", "-noprompt",
                "-alias", "dig-and-wonder-cloud-proxy", "-file", proxy_ca,
                "-keystore", str(trust_store), "-storepass", "changeit"], check=True)
            marker.write_text(fingerprint)


def main():
    if platform.system() != "Linux" or platform.machine() not in ("x86_64", "AMD64"):
        raise RuntimeError("Este instalador prepara Linux x86_64.")
    if not hasattr(tarfile, "data_filter"):
        raise RuntimeError("Python com tarfile.data_filter é necessário (Python 3.12+ recomendado).")
    version_file = ROOT / "UnityProject/ProjectSettings/ProjectVersion.txt"
    version = next(line.split(":", 1)[1].strip() for line in version_file.read_text().splitlines()
                   if line.startswith("m_EditorVersion:"))
    if version != UNITY_VERSION:
        raise RuntimeError(f"Projeto usa Unity {version}; este instalador fixa {UNITY_VERSION}.")
    DOWNLOADS.mkdir(parents=True, exist_ok=True)
    ensure_dotnet()
    ensure_editor(ensure_crane())
    prepare_android()
    launcher = TOOLS / "bin/unity"
    if not launcher.exists() and not launcher.is_symlink():
        launcher.symlink_to(ROOT / "scripts/unity-editor.sh")
    (ROOT / "scripts/unity-editor.sh").chmod(0o755)
    print("Carregue o ambiente: source scripts/unity-env.sh")
    print("Confira o executável: bash scripts/unity-editor.sh -version")
    print("Licença Unity e acesso ao registro de pacotes são verificados separadamente.")


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, subprocess.CalledProcessError) as error:
        print(f"ERRO: {error}", file=sys.stderr)
        sys.exit(1)
