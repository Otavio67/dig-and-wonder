# Importação Unity local — 20/10/2026

## Cópia de trabalho

- Projeto: `C:\Users\otaau\OneDrive\Documents\ChatGPT\Dig & Wonder\Dig & Wonder\UnityProject`.
- Base: `c8d5d7f`, branch `prototype/first-playable`, jogo 0.0.22.
- Origem preservada: `C:\Users\otaau\.codex\.chatgpt-projects\Dig & Wonder`.
- Editor encontrado: `C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe`.
- O projeto usava 6000.0.23f1; a cópia de trabalho foi convertida pelo Editor instalado para 6000.6.6f1. Não abrir esta cópia convertida em um Editor anterior.

## Importação confirmada

Executado `DigAndWonder.Editor.PrototypeProjectSetup.PrepareForBatchMode` no Editor real. O log confirmou `[DigAndWonderSetup] Dados, cena e fontes preparados.`. Os pacotes resolveram, scripts e shaders foram compilados e 37 arquivos ScriptableObject estão em `Assets/_Game/Resources/Data`.

A cena `Assets/_Game/Scenes/Prototype.unity` está habilitada nos Build Settings. Fontes TMP, arte da broca, arte da oficina e dados de combate/progressão foram importados. A cena é intencionalmente mínima: `DigAndWonderBootstrap` cria os objetos do jogo quando entra em Play Mode.

O Unity atualizou automaticamente os pacotes para a linha compatível com o Editor: 2D Animation 16.0.1, Cinemachine 6.6.0, Input System 1.20.0, Newtonsoft JSON 3.2.2, URP 17.6.0, Test Framework 1.8.0 e uGUI 2.6.0. Manifesto, lockfile, settings e metadados de importação foram preservados na cópia de trabalho.

Coplay MCP foi obtido no commit já fixado no manifesto. A conexão do cliente com o Editor ainda não foi comprovada.

## Ajustes locais

- Convertido o PluginImporter de DOTween.dll.meta de versão 1 para 2, preservando o GUID e a disponibilidade no Editor/plataformas. O formato anterior produzia erro de versão mínima no Editor novo.
- O leitor numérico de `EconomyChecks.cs` passou a aceitar CRLF e LF. A expressão anterior falhava nos arquivos Windows antes de executar 16 cenários; após o ajuste, os 75 checks passaram.
- `UnityCompileCheck` compilou sem erros nem avisos (.NET 8.0.426, APIs simuladas). A importação real do Editor também compilou os scripts; esta verificação independente não substitui runtime.

## Evidências e limites

Logs na pasta superior à raiz desta cópia:

- `unity-import-6000.6.log`: importação e preparação concluídas.
- `unity-editor-6000.6.log`: abertura gráfica posterior; houve encerramento do Editor, sem causa confirmada nesta inspeção.
- `domain-checks-windows.log`: resultados dos checks.

Play Mode interativo, aparência, controles, conexão MCP e desempenho não foram confirmados nesta importação. A captura/ativação da janela por Computer Use expirou aguardando autorização do aplicativo. O log da abertura gráfica registra aviso de token de acesso da licença; a preparação batch reconheceu o entitlement e concluiu.

Foram encontrados módulos Windows e WebGL. Android Build Support, SDK/NDK/OpenJDK não estavam presentes em `PlaybackEngines` na inspeção. Instalar esses módulos antes de tentar gerar APK com 6000.6.6f1. Não usar as evidências de APK de outro ambiente como validação desta máquina.

Não houve commit, push, alteração de saves nem expansão de conteúdo nesta tarefa.
