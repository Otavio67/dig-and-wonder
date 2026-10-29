# Dig & Wonder — Unity

Projeto ativo em C#, Unity 6, combate automático 2D em retrato e Android como destino inicial. Abra esta pasta no Unity Hub; mantenha a raiz do repositório junto dela, pois o importador lê `../content/`.

## Estado atual

O checkpoint **0.0.28** integra 20 descobertas, inimigos ágeis/blindados/carregadores, treino por fase, feedback de combate e migração de save. Catálogos, assets e fontes SVG permanecem no projeto. Detalhes em [descobertas e combate](../docs/DESCOBERTAS-E-COMBATE-0.0.28.md).

A campanha vai até a fase 100. Ouro/cristais, onze brocas com XP individual, sete melhorias, quatro poderes por personagem, baús/chaves e save esquema 4 já têm lógica e cobertura de domínio. Pets, Torre/Profundezas, arte exclusiva do elenco e ampliação visual do bestiário continuam futuros.

A 0.0.26 foi executada no Editor após atualizar as licenças no Hub MSIX. Uma captura de 28/10/2026 confirmou frames avançando e inimigo em combate, mas mostrou problemas de posicionamento de barras. Isso não representa aprovação visual final ou validação Android.

## Abrir

1. Use a versão de `ProjectSettings/ProjectVersion.txt`: **6000.6.6f1** neste checkpoint.
2. Abra esta pasta pelo Hub, aguarde os pacotes e a importação.
3. Abra `Assets/_Game/Scenes/Prototype.unity` e pressione Play.
4. Aguarde a preparação real e clique em **Iniciar mineração**.

O preparador mantém a cena nos builds, orientação retrato e Input System. O menu **Dig & Wonder → Import content data** reconverte os JSON compartilhados quando necessário. A abertura normal não exige instalação de Godot.

Vitória avança para o próximo mapa; derrota permite melhorar e recomeçar. Oficina, poderes, coleção e tesouro ficam na navegação. O save fica em `Application.persistentDataPath`; falhas de gravação pausam o combate e preservam os arquivos.

## Verificações sem Editor

Na raiz do repositório, com .NET 8:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

Os checks cobrem economia, recompensas, campanha, XP, poderes, retomada e recuperação de save. A raiz é localizada por `UnityProject/ProjectSettings/ProjectVersion.txt`. O workflow `.github/workflows/unity-code.yml` executa domínio e compilação com stubs; não há workflow Godot.

`UnityApiCompileCheck` também aceita DLLs reais do Editor e um caminho de assemblies dos pacotes. No PC atual:

```powershell
dotnet build UnityProject/Tools/UnityApiCompileCheck/UnityApiCompileCheck.csproj '-p:UnityRoot=C:/Program Files/Unity/Hub/Editor/6000.6.6f1' '-p:TemplateAssemblies=C:/Users/otaau/OneDrive/Documents/ChatGPT/Dig & Wonder/Dig & Wonder/UnityProject/Library/ScriptAssemblies'
```

Compilação C# não substitui resolução de pacotes, importação, renderização ou Play Mode. O projeto usa uGUI/TMP, Input System, DOTween e a ponte Coplay já declarada; não instalar um segundo provider MCP por padrão.

## Ferramentas e assets

- `scripts/export-hud-assets.ps1`: peças extraídas dos atlas com as coordenadas usadas pelo runtime.
- `scripts/export-gameplay-icons.cjs`: SVGs editáveis e PNGs dos pictogramas atuais; requer Node e Sharp.
- `scripts/plot-economy.py`: gráficos dos ensaios, com matplotlib.
- Scripts Unity de nuvem/MCP são apoio; confira seus parâmetros e a versão exigida antes de usá-los. Toolchain antigo de nuvem não comprova suporte ao Editor vigente.

Licenças e proveniência em [assets/LICENCAS.md](../assets/LICENCAS.md). Instruções atuais em [DESENVOLVIMENTO.md](../docs/DESENVOLVIMENTO.md), identidade em [IDENTIDADE-TOY-MECHA.md](../docs/IDENTIDADE-TOY-MECHA.md) e limites da limpeza em [UNITY-ONLY-0.0.27.md](../docs/UNITY-ONLY-0.0.27.md).

## Android

Instale Android Build Support compatível pelo Hub. O menu **Dig & Wonder → Build Android debug APK** gera o build técnico em `.tools/build/`. Um APK antigo não valida o checkpoint atual. Toque, áreas seguras e 60 FPS em aparelho continuam pendentes de medição.
