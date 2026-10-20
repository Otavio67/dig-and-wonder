# Unity no ambiente remoto — 20/10/2026

## Estado verificado

O Editor **6000.0.23f1**, revisão **1c4764c07fb4**, está instalado em `.tools/unity/6000.0.23f1`. A interface CLI faz parte do executável `Editor/Unity`. O comando `-version` executou e o log de inicialização confirmou a revisão. As bibliotecas nativas exigidas pelo executável foram encontradas nesta instância Debian 13.

A instalação ocupa aproximadamente 13 GiB. Reserve pelo menos 16 GiB livres para instalar e espaço adicional para `Library`, pacotes, Gradle e builds. Os binários, certificados de sessão e logs locais ficam em `.tools/`, ignorada pelo Git; os scripts de preparação ficam versionados.

| Componente | Resultado executado |
| --- | --- |
| Unity Editor/CLI 6000.0.23f1 | Instalado; comando de versão e início real do processo batch executados |
| .NET SDK 8.0.413 | Instalado; já existia fora do PATH; habilitado por `scripts/unity-env.sh` |
| OpenJDK 17.0.9 | `java -version` executou |
| Android NDK 23.1.7779620, r23b | `clang --version` executou e compilou arquivo C de prova para ARM64 |
| SDK Android 33, 34 e 35 / Build Tools 34.0.0 | Presentes no módulo; `aapt2 version` executou |
| ADB 34.0.5 | `adb version` executou; não houve teste em aparelho |
| DOTween | DLL e módulos do projeto disponíveis; incluídos na checagem C# contra DLLs reais |
| uGUI 2.0 / TextMeshPro | Presentes nos pacotes embutidos do Editor; TMP vem de `com.unity.ugui` |
| Demais pacotes UPM do manifesto | Resolução/importação pendente: licença não ativada e registro Unity bloqueado |
| Unity MCP | Nenhum bridge configurado ou ferramenta MCP Unity exposta nesta sessão |

## Reproduzir

Na raiz do repositório:

```bash
python3 scripts/setup-unity-cloud.py
source scripts/unity-env.sh
unity -version
bash scripts/check-unity.sh
```

O instalador recusa substituir outra instalação desconhecida ou instalar uma versão diferente da configurada em `ProjectVersion.txt`. Ele reutiliza uma instalação já verificada. A segunda execução passou sem reinstalar o Editor. Para usar outro executável já instalado, informe `UNITY_EDITOR=/caminho/Editor/Unity` antes de carregar o ambiente.

O ambiente mantém os proxies e a validação TLS. O Package Manager recebe o conjunto de certificados do sistema, e o Java usa uma cópia local do cacerts com a CA pública da sessão. Os comprovantes de licença Android previamente aceitos no SDK desta cópia são reaproveitados, quando presentes. Isso não ativa uma licença Unity.

## Fonte e integridade

Os servidores `download.unity3d.com` e `packages.unity.com` responderam **403 ao CONNECT pelo proxy**, sob política restrita. Nenhuma regra da rede ou proxy foi alterada. O Editor foi obtido da distribuição pública [GameCI Docker](https://github.com/game-ci/docker), pelo registro Docker permitido, sem instalar o sistema operacional da imagem.

- Imagem de origem: `unityci/editor:ubuntu-6000.0.23f1-android-3`.
- Manifesto Linux amd64 fixado: `sha256:32f907942bf246c2ae431c30208701e7365dcfe7a68ef79382444f9b2e0bb2e3`.
- Camada contendo o Editor/Android: `sha256:587635c77ea92d6cd08abc4052628b8ca1805848120dd12fe8aeb32a0812ed5d`.
- A camada de aproximadamente 6,6 GiB comprimidos foi extraída por streaming e seu SHA-256 completo foi validado. Quatro links absolutos internos do NDK foram convertidos para links relativos ao mesmo conteúdo local; a compilação ARM64 confirmou o uso do toolchain.
- O downloader `crane` 0.20.3 vem do [go-containerregistry](https://github.com/google/go-containerregistry/releases/tag/v0.20.3), com SHA-256 fixado e verificado. O SDK .NET tem SHA-512 fixado, conferido contra os metadados oficiais de lançamento da Microsoft.

Editor e módulos mantêm os termos Unity/Android correspondentes. A imagem não fornece uma licença Unity ativada. Não são redistribuídos no repositório.

## Ajustes de compatibilidade

Sem alterar números de combate, economia ou save:

- Removida a dependência TMP 3.2 antiga: o Editor instalado inclui TMP no uGUI 2.0 e informa que o pacote TMP separado é um shim descontinuado. O caminho do importador de recursos essenciais passou para `Packages/com.unity.ugui/Package Resources/`.
- 2D Animation 12.0.0 exige Unity 6000.2. O manifesto foi ajustado para 10.0.0, cujo mínimo declarado é 2023.1. A resolução real pelo UPM ainda precisa ocorrer. Metadados consultados no [espelho público do pacote](https://github.com/needle-mirror/com.unity.2d.animation).
- A referência de assembly uGUI foi corrigida para `UnityEngine.UI`, nome encontrado no Editor. Referências a `Unity.Newtonsoft.Json` foram removidas dos asmdefs: o pacote fornece `Newtonsoft.Json.dll` com referência automática, preservada por `overrideReferences: false`.
- `PlayerSettings.activeInputHandler` não existe como setter público neste Editor. A compilação contra a DLL real reproduziu CS0117; o preparador agora configura a propriedade serializada pelo `SerializedObject`. A mesma compilação passou após a correção.
- A quebra de linhas TMP usa `textWrappingMode`, API vigente, mantendo a quebra normal do texto.

## Validação e bloqueios

| Verificação | Resultado e limite |
| --- | --- |
| `DomainChecks` | 10 passaram: economia, recompensas, projéteis, quatro impactos, boss, retomada e recuperação de backup/paridade |
| `UnityCompileCheck` | Compilação contra stubs: zero erros e zero avisos; o stub falso do setter de entrada foi removido |
| `UnityApiCompileCheck` | Todos os fontes Runtime/Editor e módulos DOTween compilaram contra DLLs reais disponíveis: zero erros e zero avisos |
| Importação real pelo Editor, alvo Android | **Bloqueada** antes da importação: `No valid Unity Editor license found. Please activate your license.` |
| Resolução de todas as dependências do manifesto | **Não executada** pelo Editor; registro `packages.unity.com` bloqueado |
| Play Mode, cenas/prefabs pelo Editor, vídeo e APK | **Não executados**, dependem da importação e licença |
| Aparelho, toque, desempenho Android e anúncios | **Não executados** |

`UnityApiCompileCheck` é uma checagem C# com .NET 8, não a compilação do projeto pelo Editor. Usa DLLs do template 2D fornecido na distribuição, incluindo Input System 1.9.0, além de módulos do Editor e DOTween. Não valida o grafo do Package Manager, asmdefs em execução, importadores, serialização, shaders nem as versões exatas de todos os pacotes declarados, como Input System 1.11.2. A primeira tentativa revelou um erro que os stubs não detectaram; por isso ambas as checagens e seus limites estão mantidos.

Logs locais: `.tools/logs/unity-install.log`, `unity-check.log` e `unity-import.log`. A prova ARM64 fica em `.tools/logs/unity-android-probe.o`.

Para continuar a importação e preparar APK, é necessário ativar uma licença Unity válida nesta máquina pelo método oficial adequado à conta e liberar o registro de pacotes no fluxo de configuração do ambiente. Não é necessário aparelho para essas duas etapas. Depois, executar `bash scripts/check-unity.sh` e corrigir os erros reais eventualmente registrados pelo Editor.

Unity Essentials é o pacote de orientações disponível nesta conversa. A conexão MCP exige um bridge adicional com o Editor ativo; o Editor instalado e a checagem CLI não comprovam essa conexão. Nenhum segundo bridge foi adicionado nesta preparação.
