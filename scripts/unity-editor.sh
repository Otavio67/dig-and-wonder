#!/usr/bin/env bash
set -euo pipefail
source "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")/unity-env.sh"
if [[ ! -x "$UNITY_EDITOR" ]]; then
  echo "Unity Editor ausente: $UNITY_EDITOR" >&2
  echo "Prepare com: python3 scripts/setup-unity-cloud.py" >&2
  exit 127
fi
exec "$UNITY_EDITOR" "$@"
