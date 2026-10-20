#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$root/scripts/unity-env.sh"
cd "$root"
command -v dotnet >/dev/null || { echo "Prepare o .NET com scripts/setup-unity-cloud.py" >&2; exit 127; }
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
dotnet build UnityProject/Tools/UnityApiCompileCheck/UnityApiCompileCheck.csproj -p:UnityRoot="$UNITY_PATH"
echo "Checks .NET concluídos. Eles não substituem a importação no Unity."
mkdir -p .tools/logs
log="$root/.tools/logs/unity-import.log"
if ! bash scripts/unity-editor.sh -batchmode -nographics -quit \
    -projectPath "$root/UnityProject" -buildTarget Android -logFile "$log"; then
  echo "Unity não concluiu a importação. Consulte: $log" >&2
  exit 1
fi
if rg -q 'error CS[0-9]+|Scripts have compiler errors|Aborting batchmode|Exception:' "$log"; then
  echo "Unity registrou erros. Consulte: $log" >&2
  exit 1
fi
echo "Importação Unity para Android concluída. Log: $log"
