#!/usr/bin/env python3
"""Usa o cliente oficial Unity com autenticação fornecida como segredo do ambiente."""

import os
from pathlib import Path
import subprocess
import sys


def main():
    root = Path(__file__).resolve().parent.parent
    version = next(line.split(":", 1)[1].strip() for line in
                   (root / "UnityProject/ProjectSettings/ProjectVersion.txt").read_text().splitlines()
                   if line.startswith("m_EditorVersion:"))
    editor = Path(os.environ.get("UNITY_EDITOR", root / ".tools/unity" / version / "Editor/Unity"))
    client = editor.parent / "Data/Resources/Licensing/Client/Unity.Licensing.Client"
    if not client.is_file():
        print("Cliente de licença ausente; prepare o Editor com scripts/setup-unity-cloud.py.", file=sys.stderr)
        return 127
    token = os.environ.get("UNITY_ACCESS_TOKEN", "").strip()
    if not token:
        print("Ativação pendente: falta autenticação Unity. Configure UNITY_ACCESS_TOKEN como segredo do ambiente pelo fluxo oficial da sua conta. Não inclua o token em arquivos do projeto.", file=sys.stderr)
        return 2

    # Opções confirmadas no --help do cliente incluído no Unity 6000.0.23f1.
    # Personal é o fallback gratuito; não compra planos nem cria uma conta.
    try:
        result = subprocess.run([str(client), "--activate-all", "--include-personal",
            "--accessToken", token], capture_output=True, text=True, timeout=60)
    except subprocess.TimeoutExpired:
        print("Ativação expirou; verifique o acesso aos serviços oficiais Unity.", file=sys.stderr)
        return 1
    # O token não é escrito nos logs deste auxiliar.
    for output in (result.stdout, result.stderr):
        if output:
            print(output.replace(token, "[redigido]"), end="")
    if result.returncode == 0:
        print("Cliente de ativação concluiu. Confirme a licença e a importação com bash scripts/check-unity.sh.")
    return result.returncode


if __name__ == "__main__":
    sys.exit(main())
