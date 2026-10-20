# Continuar o Dig & Wonder no PC

## O que transferir

Repositório: https://github.com/Otavio67/dig-and-wonder

Branch: **`prototype/first-playable`**. Checkpoint de código de origem deste guia: **`2089c71`**. A versão de jogo publicada continua 0.0.7, com checkpoints posteriores de preparação/correções.

Clone ou baixe **o repositório inteiro**. A pasta `UnityProject` depende dos JSON de `content/`, na raiz do repositório, para a primeira importação. Copiar somente `UnityProject` não transfere essa fonte.

No PowerShell, em uma pasta onde ainda não exista uma cópia do projeto:

```powershell
git clone --branch prototype/first-playable https://github.com/Otavio67/dig-and-wonder.git
cd dig-and-wonder
git status
```

Se já houver uma cópia com trabalho local, preserve as alterações antes de trocar a branch ou sincronizar. Não use `reset --hard` ou extraia um ZIP por cima de arquivos modificados.

## Abrir uma conversa com acesso local

Abra a pasta **`dig-and-wonder`** no Codex local, ou no editor com a extensão Codex. O workspace deve incluir `UnityProject`, `content`, `docs` e `AGENTS.md`.

Uma conversa comum do ChatGPT pode orientar, mas não ganha acesso ao disco nem ao Unity por receber um link. O histórico desta conversa também não é transferido automaticamente; use a mensagem pronta ao final deste guia.

## Preparar o Unity no Windows

1. Instale/abra o **Unity Hub**, entre na sua conta e ative a **Unity Personal** pelo fluxo do Hub. Não envie senha, token ou arquivo de licença para a conversa.
2. Pelo Hub/arquivo de versões oficial, instale o Editor **6000.0.23f1**, igual a `UnityProject/ProjectSettings/ProjectVersion.txt`. Inclua **Android Build Support**, **Android SDK & NDK Tools** e **OpenJDK** se for gerar APK. Se essa versão não estiver disponível, registre isso antes de escolher uma atualização.
3. Tenha Git disponível no PATH: o manifesto usa um pacote MCP via URL Git. Instale o **SDK .NET 8** se quiser executar os checks de domínio fora do Editor; ele não substitui o compilador do Unity.
4. No Hub, adicione/abra a pasta **`dig-and-wonder/UnityProject`** e aguarde a resolução dos pacotes. TMP vem do uGUI 2.0; DOTween já acompanha o repositório. Evite importar cópias duplicadas.
5. O importador deve criar os ScriptableObjects em `Assets/_Game/Resources/Data`; o preparador deve criar/incluir `Assets/_Game/Scenes/Prototype.unity` e ajustar retrato/Input System. Confira o resultado real e a Console. Comandos manuais, se necessários: **Dig & Wonder → Import Godot data** e **Dig & Wonder → Prepare prototype scene**.
6. Só depois de corrigir os erros de importação/compilação, entre em Play e confira o fluxo de iniciar/retomar, combate, melhorias e encerramento de fase.

Os pacotes URP 2D, Input System e Cinemachine estão declarados no manifesto. Isso não comprova que todos os assets/configurações da pipeline renderizem corretamente: essa configuração ainda precisa ser verificada no Editor.

O toolchain Linux em `.tools/` não está no Git e não precisa ser copiado. Os scripts de instalação da nuvem destinam-se a Linux; não execute o instalador GameCI para configurar um PC Windows.

## Conectar o Unity ao Codex local

A ponte escolhida é **CoplayDev/unity-mcp 10.0.0**, fixada no manifesto no commit `d49ae2953580f3481beb1e084a1da2682f0b5610`. Não adicione outro provider por padrão.

Depois de importar sem erros, leia a documentação do pacote instalado para preparar o servidor no Windows e use a janela **Window → MCP for Unity** para configurar/iniciar a sessão. O servidor e a configuração MCP precisam existir **no PC**: o endereço `127.0.0.1` usado na nuvem aponta para a própria máquina de quem o acessa.

Confirme a integração por uma instância do projeto local e uma leitura real de Console/hierarquia. Apenas instalar o plugin Codex ou iniciar o servidor não comprova que o Editor está conectado. Referência da preparação remota: [UNITY-MCP.md](UNITY-MCP.md).

## Estado já verificado na nuvem

- 20 checks de domínio passaram: economia, recompensas, quatro golpes da Pioneira, vitória/derrota terminais, retomada determinística e recuperação/preservação de arquivos.
- Compilação com stubs e compilação contra DLLs reais do Editor/template passaram com zero erros/avisos. Esta última usa as DLLs disponíveis no template, não a resolução final dos pacotes do projeto.
- O processo real do Editor iniciou, mas parou antes da importação: `No valid Unity Editor license found`. O proxy remoto também bloqueou autenticação/licença/pacotes da Unity.
- Não houve Play Mode Unity, revisão visual, vídeo, APK Unity ou medição Android. O login no navegador do usuário não comprovou a licença do processo remoto.
- As correções de save/combate estão em [UNITY-REVISAO-COMBATE-SAVE.md](UNITY-REVISAO-COMBATE-SAVE.md), com roteiro de testes pendentes. A pausa/retomada após falha de escrita foi compilada e inspecionada; ainda requer exercício real do driver/UI no Editor.

Na raiz do repositório, com SDK .NET 8:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

Esses comandos são independentes do Editor. `UnityApiCompileCheck` usa caminhos da instalação Linux GameCI e não deve ser tratado como um check Windows pronto.

Para a compilação real pelo Editor, depois de ativar a licença e fechar o projeto de forma normal, adapte o caminho do Editor se necessário:

```powershell
$projectRoot = (Get-Location).Path
New-Item -ItemType Directory -Force .tools/logs | Out-Null
& 'C:\Program Files\Unity\Hub\Editor\6000.0.23f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath "$projectRoot\UnityProject" -buildTarget Android -logFile "$projectRoot\.tools\logs\unity-local-import.log"
```

Leia o log e o código de saída; esse comando não gera APK. Importação/compilação bem-sucedida ainda deve ser seguida por Play Mode e pelos testes relevantes em aparelho.

## Mensagem para colar na nova conversa

> Continue o Dig & Wonder neste PC, no repositório Otavio67/dig-and-wonder, branch prototype/first-playable. Leia AGENTS.md, README.md, UnityProject/README.md, docs/CONTINUAR-UNITY-LOCAL.md e docs/UNITY-REVISAO-COMBATE-SAVE.md. A direção posterior autorizada é Unity 6, combate automático lateral com a broca protagonista; os documentos Godot antigos são origem e histórico, não uma instrução para voltar de engine. Primeiro configure e valide o ambiente local: Unity 6000.0.23f1, licença Personal pelo fluxo oficial, pacotes, importação, compilação real e Play Mode. Depois conecte a ponte Coplay MCP já declarada e confirme o projeto por Console/hierarquia. Preserve trabalho local e saves. Não invente números de balanceamento nem alegue qualidade visual/desempenho sem executar as verificações. Há 20 checks C# passando na nuvem; o Editor remoto ficou bloqueado por licença/proxy, sem APK Unity ou revisão visual. Priorize desbloquear e testar o protótipo existente antes de ampliar conteúdo. Use autonomia para ajustes reversíveis, economize créditos e faça checkpoints no GitHub por marcos. Não peça senhas/tokens pelo chat.
