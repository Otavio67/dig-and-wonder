# Continuar o Dig & Wonder no PC

## O que transferir

Repositório: https://github.com/Otavio67/dig-and-wonder

Branch de origem: **`prototype/first-playable`**, commit **`7b3518fa7549daa40f7f9d70c38a7c508fb28a67`**. Versão de jogo 0.0.7, com checkpoints posteriores de preparação/correções.

O repositório inteiro já está nesta pasta; os arquivos da árvore remota foram conferidos antes das alterações locais. `UnityProject` depende dos JSON em `content/`, portanto mantenha a raiz completa junto do projeto Unity. A pasta anterior e o Godot portátil foram preservados em `.tools/backups/` e `.tools/godot/`.

Esta cópia foi expandida de um ZIP autenticado e o Git local ainda não tem os commits da branch. O `origin` aponta para o repositório correto, mas o Git do terminal não tem credencial funcional. Não extraia outro ZIP por cima nem crie um commit inicial como se fosse um repositório novo. Para atualizar/publicar um marco depois, primeiro recupere a relação Git com a branch remota mantendo os ajustes locais.

O caminho do projeto nesta máquina é `C:\Users\pctri\OneDrive\Documentos\ChatGPT\Dig & Wonder\UnityProject`.

## Abrir uma conversa com acesso local

Abra a pasta **`dig-and-wonder`** no Codex local, ou no editor com a extensão Codex. O workspace deve incluir `UnityProject`, `content`, `docs` e `AGENTS.md`.

Uma conversa comum do ChatGPT pode orientar, mas não ganha acesso ao disco nem ao Unity por receber um link. O histórico desta conversa também não é transferido automaticamente; use a mensagem pronta ao final deste guia.

## Ambiente local já preparado

- Unity Hub **3.22.3 x64**, Editor **6000.0.23f1** e Unity Personal estão instalados/ativados.
- Android Build Support, OpenJDK 17.0.9, SDK API 33/34/35, NDK 23.1.7779620 e Build Tools 34.0.0 estão instalados. As licenças SDK estão aceitas.
- O projeto importou e compilou no Editor real. A preparação criou a cena e os recursos de dados/TMP.
- APK de depuração criado em `.tools/build/DigAndWonder-debug.apk`. O pacote ARM64 foi lido pelo `aapt`: `com.DigAndWonder.DigWonder`, mínimo API 23, alvo API 35. Não há aparelho Android conectado; instalação, toque, visual e desempenho continuam sem validação.
- Ao abrir, use o Unity Hub para adicionar a pasta local `UnityProject`. Para rebuild, use o menu **Dig & Wonder → Build Android debug APK**. O primeiro build completo levou cerca de 12 minutos; builds seguintes aproveitam o cache.
- Unity CLI fica em `%LOCALAPPDATA%\Unity\bin\unity.exe`; SDK .NET 8.0.426 está em `.tools/dotnet/`. O pacote do Coplay está declarado, mas ainda não foi conectado a um Editor local.

Os pacotes URP 2D, Input System e Cinemachine estão declarados no manifesto. Isso não comprova que todos os assets/configurações da pipeline renderizem corretamente: essa configuração ainda precisa ser verificada no Editor.

O toolchain Linux em `.tools/` não está no Git e não precisa ser copiado. Os scripts de instalação da nuvem destinam-se a Linux; não execute o instalador GameCI para configurar um PC Windows.

## Conectar o Unity ao Codex local

A ponte escolhida é **CoplayDev/unity-mcp 10.0.0**, fixada no manifesto no commit `d49ae2953580f3481beb1e084a1da2682f0b5610`. Não adicione outro provider por padrão.

Depois de importar sem erros, leia a documentação do pacote instalado para preparar o servidor no Windows e use a janela **Window → MCP for Unity** para configurar/iniciar a sessão. O servidor e a configuração MCP precisam existir **no PC**: o endereço `127.0.0.1` usado na nuvem aponta para a própria máquina de quem o acessa.

Confirme a integração por uma instância do projeto local e uma leitura real de Console/hierarquia. Apenas instalar o plugin Codex ou iniciar o servidor não comprova que o Editor está conectado. Referência da preparação remota: [UNITY-MCP.md](UNITY-MCP.md).

## Verificações anteriores na nuvem

- 20 checks de domínio passaram: economia, recompensas, quatro golpes da Pioneira, vitória/derrota terminais, retomada determinística e recuperação/preservação de arquivos.
- Compilação com stubs e compilação contra DLLs reais do Editor/template passaram com zero erros/avisos. Esta última usa as DLLs disponíveis no template, não a resolução final dos pacotes do projeto.
- Na época, o processo remoto do Editor parou antes da importação por falta de licença, e o proxy bloqueou autenticação/pacotes da Unity. Essa restrição não se aplica ao Editor local já instalado.
- Não houve Play Mode Unity, revisão visual, instalação/toque em aparelho, anúncio ou medição Android. O APK gerado confirma o build técnico, não a experiência em aparelho.
- As correções de save/combate estão em [UNITY-REVISAO-COMBATE-SAVE.md](UNITY-REVISAO-COMBATE-SAVE.md), com roteiro de testes pendentes. A pausa/retomada após falha de escrita foi compilada e inspecionada; ainda requer exercício real do driver/UI no Editor.

Na raiz do repositório, com SDK .NET 8:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

Esses comandos são independentes do Editor. `UnityApiCompileCheck` usa caminhos da instalação Linux GameCI e não deve ser tratado como um check Windows pronto.

Para preparar assets/dados pelo Editor em batch mode, adapte o caminho do Editor se necessário:

```powershell
$projectRoot = (Get-Location).Path
New-Item -ItemType Directory -Force .tools/logs | Out-Null
& "$env:LOCALAPPDATA\Unity\Editors\6000.0.23f1\Editor\Unity.exe" -batchmode -nographics -projectPath "$projectRoot\UnityProject" -buildTarget Android -executeMethod DigAndWonder.Editor.PrototypeProjectSetup.PrepareForBatchMode -logFile "$projectRoot\.tools\logs\unity-local-prepare.log"
```

Esse método encerra sozinho após confirmar a importação; não acrescente `-quit`. Para APK, use o menu descrito acima. Leia o log e o código de saída; importação/build bem-sucedidos ainda devem ser seguidos por Play Mode e pelos testes relevantes em aparelho.

## Mensagem para colar na nova conversa

> Continue o Dig & Wonder nesta pasta e leia AGENTS.md, README.md, UnityProject/README.md, docs/CONTINUAR-UNITY-LOCAL.md e docs/UNITY-REVISAO-COMBATE-SAVE.md. A direção atual é Unity 6; a origem Godot foi preservada. Unity 6000.0.23f1 e Android estão instalados; a preparação real e o APK técnico passaram. Abra a pasta `UnityProject` pelo Hub, confira Play Mode/Console e, quando houver aparelho, instale o APK para testar toque, safe area e aparência. Não alegue desempenho visual ou 60 FPS sem medir em aparelho. O Git local contém a árvore de arquivos mas ainda não o histórico da branch remota; mantenha as alterações e não faça commit inicial artificial. Use autonomia para ajustes reversíveis, economize créditos e atualize o GitHub em marcos. Não peça senhas/tokens pelo chat.
