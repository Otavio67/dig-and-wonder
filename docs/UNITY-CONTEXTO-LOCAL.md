# Estado local do Dig & Wonder — 20/10/2026

## Estado confirmado

- Unity **6000.0.23f1_1c4764c07fb4** instalado; instalador terminou com saída 0.
- Executável: `%LOCALAPPDATA%/Unity/Editors/6000.0.23f1/Editor/Unity.exe`. A Unity CLI registra o caminho do executável completo, e não somente a pasta.
- Unity Hub **3.22.3 x64** instalado; Unity CLI **1.0.0-beta.13** disponível.
- **Unity Personal ativada**, confirmação real pela Unity CLI com saída 0. O Editor executou a preparação local com sucesso. Avisos de atualização de sessão/validação do cliente de licença constam no log, mas não bloquearam a execução batch; não foram desativadas verificações de assinatura.
- Projeto `UnityProject/` importado e compilado no Editor real. Cena `Assets/_Game/Scenes/Prototype.unity`, **24 ScriptableObjects** em `Assets/_Game/Resources/Data/` e `Assets/TextMesh Pro/Resources/TMP Settings.asset` confirmados em disco.
- Android Support instalado; Java 17.0.9, NDK 23.1.7779620, SDK API 33/34/35, Build Tools 34.0.0, CMake 3.22.1 e ADB conferidos. As licenças SDK foram aceitas e `sdkmanager --licenses` retornou 0.
- Importação real com `-buildTarget Android`: saída 0. APK ARM64 de depuração gerado em `.tools/build/DigAndWonder-debug.apk` (**36.972.192 bytes**); `aapt dump badging` leu o pacote (`com.DigAndWonder.DigWonder`, mínimo API 23, alvo API 35). SHA-256: `992D4200FCB4130E6B2E4F3806BFE8260E3D935883EB43B9A5DC42BD21AFA6D2`.
- `adb devices` não encontrou aparelho conectado. Instalação/toque real, Play Mode visual, anúncios e desempenho em Android continuam pendentes; a meta de 60 FPS não foi medida.

## Próxima etapa

Falta instalar o APK em um aparelho Android e conferir toque, safe area e aparência. Depois disso, seguir os checks existentes de Play Mode/save e decidir quando conectar o Coplay MCP. O protótipo atual é uma amostra Unity de combate automático; não confundir o APK técnico com a direção de mineração Dig & Wonder já discutida. Não expandir gameplay neste marco.

## Origem, Git e preservação

Fonte: `Otavio67/dig-and-wonder`, branch **prototype/first-playable**, commit `7b3518fa7549daa40f7f9d70c38a7c508fb28a67`. Versão 0.0.7 com checkpoints posteriores. ZIP completo autenticado pelo navegador; **338 arquivos originais conferidos individualmente** contra os hashes Git remotos, zero divergências antes dos ajustes locais.

A base anterior está em `.tools/backups/antes-download-20261020/`; Godot portátil existente foi preservado. Nenhum save do jogador foi alterado. A fonte Godot e os JSON de `content/` devem acompanhar UnityProject; são origem/importação, não orientação para retornar de engine.

O Git local continua em `main` sem commits e com origin HTTPS configurada. O conector GitHub lê o repositório privado, mas o Git do terminal não tem autenticação utilizável. O ZIP **não equivale a clone com histórico**. Não criar/enviar um commit inicial desses arquivos como projeto novo; sincronizar com a branch de origem preservando os ajustes locais. Nada foi publicado no GitHub nesta etapa.

## Ajustes de configuração feitos aqui

- `Packages/manifest.json`: **2D Animation 10.0.0 → 10.1.4**, preservando as outras sete dependências diretas. O original não compilava com Collections 2.5.1. Par final confirmado: Animation 10.1.4 e Common 9.0.7. Manifesto/lock foram gerados pela API UPM em projeto temporário e conferidos antes de aplicar. As tentativas intermediárias não são a configuração final.
- `Packages/packages-lock.json`: resolução real do Editor, manter para reprodutibilidade.
- `PrototypeProjectSetup.cs`: resolve os recursos TMP pelo caminho físico registrado do uGUI e disponibiliza `PrepareForBatchMode`. A preparação espera a importação assíncrona das fontes, verifica dados/cena/TMP carregáveis e encerra por conta própria, com timeout de 180 s. **Não usar -quit nesse método.**
- `PrototypeProjectSetup.cs`: inclui `BuildAndroidDebugApk`, acessível pelo menu **Dig & Wonder → Build Android debug APK** ou por batch mode. O build usa as cenas habilitadas e retorna código de saída conforme `BuildReport`.
- `UnityProject/VERSION` virou `UnityProject/GAME_VERSION.txt`: no Windows, o nome original colidia sem distinção de caixa com o cabeçalho C++ padrão `<version>`, causando falha de IL2CPP no primeiro APK. Depois da correção, o build Android completo terminou com saída 0.
- `UnityApiStubs.cs`: superfície mínima das APIs de Editor usadas na preparação, para preservar o check C# existente. Não substitui validação real Unity.
- Assets, `.meta` e configurações gerados pela primeira importação permanecem no projeto; revisar e versionar os recursos necessários com seus `.meta` quando o Git estiver sincronizado. Manter licenças do TMP/DOTween.
- AGENTS.md preserva a preferência de modelos econômicos e esclarece a continuidade Unity posterior aos documentos Godot.

## Checks executados e limites

| Verificação | Resultado |
| --- | --- |
| Integridade do ZIP | 338 arquivos idênticos à árvore remota |
| Checks de domínio C# | 20 PASS na transferência inicial; simulação/save não foram alterados depois |
| Compilação com stubs após o método de APK | Saída 0; zero erros e avisos |
| Preparação real Unity final | Saída 0; marcador `[DigAndWonderSetup] Dados, cena e fontes preparados.` |
| Conferência dos arquivos e log final | Cena, CombatBalance e TMP Settings presentes; 24 assets; zero erros C# ou exceções |
| SDK Android e licenças | Executáveis verificados; `sdkmanager --licenses` saída 0 |
| Importação do Editor no alvo Android | Saída 0; cena, dados e TMP carregados |
| APK de depuração ARM64 | Build real do Editor saída 0; pacote identificado pelo `aapt`; sem instalação em aparelho |
| Play Mode visual, toque em aparelho, anúncios e FPS | Pendentes; não inferir do build headless |

Logs: `.tools/logs/unity-domain-local.log`, `.tools/logs/unity-compile-final.log`, `.tools/logs/unity-local-prepare-verified.log`, `.tools/logs/unity-android-prepare.log` e `.tools/logs/unity-android-build.log`. Não repetir checks sem novas alterações/falhas. O Windows bloqueou o apphost do teste .NET; a mesma DLL executou pelo runtime instalado, sem alterar políticas de segurança.

Ferramentas: Unity CLI **1.0.0-beta.13** em `%LOCALAPPDATA%/Unity/bin/unity.exe`; SDK .NET **8.0.426** em `.tools/dotnet/`. Para repetir a preparação quando necessário:

```powershell
$repoRoot = (Get-Location).Path
& "$env:LOCALAPPDATA\Unity\Editors\6000.0.23f1\Editor\Unity.exe" -batchmode -nographics -projectPath "$repoRoot\UnityProject" -executeMethod DigAndWonder.Editor.PrototypeProjectSetup.PrepareForBatchMode -logFile "$repoRoot\.tools\logs\unity-local-prepare.log"
```

Aguardar o processo, verificar código de saída, marcador final, erros/exceções e os recursos; somente saída 0 não é prova suficiente. Não iniciar duas instâncias do mesmo projeto. Em wrappers PowerShell, aguardar o processo principal com `WaitForExit()`, pois `Start-Process -Wait` pode continuar esperando subprocessos residentes após o Editor encerrar.

Para gerar um APK de depuração depois de uma alteração, abra o projeto no Hub e use **Dig & Wonder → Build Android debug APK**. O batch method padrão grava em `.tools/build/DigAndWonder-debug.apk`; a saída é ignorada pelo Git. O build inicial completo levou cerca de 12 minutos nesta máquina; o Unity mantém caches locais para builds seguintes.

## Downloads e arquitetura

Instaladores em `.tools/downloads/`: Editor (4.033.958.912 bytes), Hub, suporte Android, OpenJDK e nove componentes SDK/NDK. Editor/suporte Android: MD5 e assinatura conferidos; Hub: SHA512 e assinatura; ZIPs abertos e checksum SHA384 disponível conferido. Não repetir downloads completos sem motivo. Nada de `.tools/`, Library, Temp, APKs, credenciais ou chaves deve ser versionado.

URP 17.0.3, Input System 1.11.2, uGUI 2.0/TMP, Cinemachine 3.1.2 e DOTween. Coplay MCP fixado no commit d49ae2953580f3481beb1e084a1da2682f0b5610; não adicionar outro provider por padrão. Assemblies `DigAndWonder.Runtime` e `DigAndWonder.Editor`; simulação, eventos, persistência e apresentação separados. `GodotCatalogImporter` converte content/ para Resources/Data; `DigAndWonderBootstrap` cria câmera/EventSystem/HUD/AutoBattleDriver em runtime.

O Unity Hub agora pode abrir `UnityProject/` pelo projeto local. A ponte Coplay MCP ainda não foi conectada ao Editor local. Priorizar o protótipo existente; sem ampliar fases, balanceamento ou monetização neste marco.
