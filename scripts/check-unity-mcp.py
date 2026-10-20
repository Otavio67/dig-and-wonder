#!/usr/bin/env python3
"""Verifica o protocolo MCP e a presença real de um Editor conectado, sem editar cenas."""

import asyncio
import json
import sys

from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client


async def main():
    async with streamablehttp_client("http://127.0.0.1:8080/mcp") as (reader, writer, _):
        async with ClientSession(reader, writer) as session:
            initialized = await session.initialize()
            tools = await session.list_tools()
            result = await session.read_resource("mcpforunity://instances")
            payload = json.loads(next(item.text for item in result.contents if hasattr(item, "text")))
            data = payload.get("data", payload)
            count = data.get("instance_count", 0)
            print(json.dumps({"mcp_connected": True, "protocol": initialized.protocolVersion,
                "server": initialized.serverInfo.name, "tool_count": len(tools.tools),
                "editor_connected": count > 0, "instance_count": count,
                "instances": data.get("instances", [])}, ensure_ascii=False, indent=2))
            return 0 if count > 0 else 2


if __name__ == "__main__":
    try:
        sys.exit(asyncio.run(main()))
    except Exception as error:
        print(json.dumps({"mcp_connected": False, "error": str(error)}, ensure_ascii=False))
        sys.exit(1)
