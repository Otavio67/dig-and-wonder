# Continuar o Dig & Wonder neste PC

Abra a raiz completa do repositório e a branch `prototype/first-playable`. O projeto executável é `UnityProject/`; catálogos em `content/` continuam necessários ao importador. A implementação antiga do Godot foi retirada em 29/10/2026; não execute seus comandos de instalação, testes ou exportação a partir do histórico.

Use o Unity Hub, inclusive a instalação Windows/MSIX, e confira a licença Personal antes de abrir o Editor. A versão exigida está em `UnityProject/ProjectSettings/ProjectVersion.txt` (6000.6.6f1 neste checkpoint). No teste local anterior, abrir o Hub e atualizar as licenças resolveu a ausência de licença reconhecida pelo Editor.

Abra `Assets/_Game/Scenes/Prototype.unity`, entre em Play, aguarde o loading real e toque em **Iniciar mineração**. Save e perfil não devem ser substituídos por fixtures. A captura real de 28/10/2026 mostrou um inimigo em combate, com problemas visuais ainda pendentes de revisão.

Com .NET 8, execute os checks descritos em [UnityProject/README](../UnityProject/README.md). Importação e Play Mode não são comprovados por compilação com stubs. Para Android, instale os módulos correspondentes e valide APK, toque, áreas seguras e FPS em aparelho; não há resultado atual comprovado desses testes.

A ponte existente é CoplayDev/unity-mcp, declarada no manifesto. Use e valide essa ponte; não instale outro provider por padrão. Configuração e evidências históricas em [UNITY-MCP.md](UNITY-MCP.md).

Estado e limites atuais: [README](../README.md), [desenvolvimento](DESENVOLVIMENTO.md) e [limpeza 0.0.27](UNITY-ONLY-0.0.27.md).