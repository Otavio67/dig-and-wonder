#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
server="$root/.tools/unity-mcp-v10/Server/.venv/bin/mcp-for-unity"
if [[ ! -x "$server" ]]; then
  echo "Prepare o servidor com: bash scripts/setup-unity-mcp.sh" >&2
  exit 127
fi
export UNITY_MCP_DISABLE_TELEMETRY=1
export UNITY_MCP_LOG_DIR="$root/.tools/logs/unity-mcp"
mkdir -p "$root/.tools/logs"
echo "$$" > "$root/.tools/unity-mcp-server.pid"
# Porta apenas local: o Editor e o cliente MCP ficam nesta mesma máquina.
exec "$server" --transport http --http-url http://127.0.0.1:8080 --http-host 127.0.0.1
