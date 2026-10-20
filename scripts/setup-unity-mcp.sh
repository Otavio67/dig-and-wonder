#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
checkout="$root/.tools/unity-mcp-v10"
revision="d49ae2953580f3481beb1e084a1da2682f0b5610"
command -v uv >/dev/null || { echo "Instale uv antes de preparar o servidor MCP." >&2; exit 127; }
if [[ ! -d "$checkout/.git" ]]; then
  git clone --depth 1 --branch v10.0.0 https://github.com/CoplayDev/unity-mcp.git "$checkout"
fi
if [[ "$(git -C "$checkout" rev-parse HEAD)" != "$revision" ]]; then
  echo "O checkout MCP difere da revisão fixada; não será substituído." >&2
  exit 1
fi
# Usa o lock upstream e os certificados do sistema, preservando o proxy.
UV_SYSTEM_CERTS=true UV_CACHE_DIR="$root/.tools/uv-cache" \
  uv sync --project "$checkout/Server" --frozen --no-dev --python "$(command -v python3)"
echo "Servidor Coplay MCP 10.0.0 preparado."
echo "Inicie com: bash scripts/unity-mcp-server.sh"
