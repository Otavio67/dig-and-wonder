#!/usr/bin/env bash
# Use source para habilitar o toolchain desta cópia do projeto.
_DW_TOOLCHAIN_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
_DW_UNITY_VERSION="$(awk '/^m_EditorVersion:/ {print $2}' "$_DW_TOOLCHAIN_ROOT/UnityProject/ProjectSettings/ProjectVersion.txt")"
export UNITY_EDITOR="${UNITY_EDITOR:-$_DW_TOOLCHAIN_ROOT/.tools/unity/$_DW_UNITY_VERSION/Editor/Unity}"
export UNITY_PATH="$(dirname "$(dirname "$UNITY_EDITOR")")"
if [[ -n "${CODEX_PROXY_CERT:-}" && -f /etc/ssl/certs/ca-certificates.crt ]]; then
  export UPM_CACERTS="${UPM_CACERTS:-/etc/ssl/certs/ca-certificates.crt}"
fi
export PATH="$_DW_TOOLCHAIN_ROOT/.tools/bin:$PATH"
if [[ -x "$_DW_TOOLCHAIN_ROOT/.tools/dotnet/dotnet" ]]; then
  export DOTNET_ROOT="$_DW_TOOLCHAIN_ROOT/.tools/dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi
_DW_ANDROID_PLAYER="$UNITY_PATH/Editor/Data/PlaybackEngines/AndroidPlayer"
if [[ -d "$_DW_ANDROID_PLAYER/SDK" ]]; then
  export ANDROID_HOME="$_DW_ANDROID_PLAYER/SDK"
  export ANDROID_SDK_ROOT="$ANDROID_HOME"
  export PATH="$ANDROID_HOME/platform-tools:$PATH"
fi
if [[ -d "$_DW_ANDROID_PLAYER/NDK" ]]; then
  export ANDROID_NDK_ROOT="$_DW_ANDROID_PLAYER/NDK"
fi
if [[ -x "$_DW_ANDROID_PLAYER/OpenJDK/bin/java" ]]; then
  export JAVA_HOME="$_DW_ANDROID_PLAYER/OpenJDK"
  export PATH="$JAVA_HOME/bin:$PATH"
fi
if [[ -f "$_DW_TOOLCHAIN_ROOT/.tools/unity/trust/cacerts" && "${JAVA_TOOL_OPTIONS:-}" != *-Djavax.net.ssl.trustStore=* ]]; then
  export JAVA_TOOL_OPTIONS="${JAVA_TOOL_OPTIONS:+$JAVA_TOOL_OPTIONS }-Djavax.net.ssl.trustStore=\"$_DW_TOOLCHAIN_ROOT/.tools/unity/trust/cacerts\" -Djavax.net.ssl.trustStorePassword=changeit"
fi
unset _DW_TOOLCHAIN_ROOT _DW_UNITY_VERSION _DW_ANDROID_PLAYER
