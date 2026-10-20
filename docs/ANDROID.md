# Teste Android

## Preparar o ambiente Linux x86_64

Requer OpenJDK 17 ou superior, `curl`, `unzip`, `sha1sum`, `sha512sum` e Python 3. No repositório:

```bash
bash setup-cloud.sh
bash scripts/setup-android.sh
```

O instalador guarda o SDK em `.tools/android-sdk`, instala Platform Tools, Android API 36, Build Tools 36.0.0, NDK 28.1 e CMake 3.10.2.4988404. Baixa os templates Godot 4.7.2 do release oficial e valida a soma SHA-512 antes de instalar os arquivos Android. Os caminhos do JDK/SDK são escritos nas preferências locais do editor Godot; nenhum caminho de máquina ou SDK é adicionado ao Git.

Para gerar e validar um APK de depuração:

```bash
bash scripts/export-android-debug.sh
```

O resultado fica em `builds/dig-and-wonder-android-debug.apk`, é assinado pela chave debug local e está excluído do Git. O preset é ARM64, pacote de teste `com.otavio67.digandwonder`, versão 0.0.5, API mínima 24 e API alvo 36. Não usa chave de publicação e não declara permissão de internet. Ferramentas e plug-in de anúncios são excluídos deste pacote.

## Estado da validação

No ambiente de nuvem em 20/10/2026, Godot 4.7.2 exportou o APK sem erro, `apksigner verify` aprovou as assinaturas v2/v3 e `aapt` confirmou o identificador e as versões Android. Importação do projeto e boot headless também foram executados. Durante a inicialização do Godot houve aviso de conexão ao daemon ADB; uma chamada explícita a `adb devices -l` iniciou o daemon e listou zero dispositivos. A exportação não depende de aparelho conectado.

Ainda é necessário instalar os APKs em um Android real e conferir abertura, proporções, toque, pausa/retorno, salvamento e callback de anúncio. Este ambiente remoto não vê as portas USB do computador do usuário. Para testar no aparelho local, gere o APK, conecte um telefone com Depuração USB ativa e use `adb install -r builds/dig-and-wonder-android-debug.apk`. O teste de anúncios usa seu próprio APK técnico e requer internet/Play Services. Desempenho de 60 FPS não foi medido.

## Revisão 0.0.4

A análise das orientações oficiais, correções de input/área segura/ciclo de vida/save, resultados e pendências estão em [REVISAO-ANDROID.md](REVISAO-ANDROID.md). A exportação agora executa `scripts/verify-android-apk.py`: checa assinatura, integridade, manifesto, permissões, exclusão das ferramentas de desenvolvimento e alinhamentos ZIP/ELF de 16 KB.

`bash scripts/run-harnesses.sh` verifica a lógica e as regressões Android em desktop. Com um display X11 ou `xvfb-run`, `bash scripts/capture-visual.sh` gera imagens reais do Godot em `builds/review/`, usando dados de teste isolados. O processo visual é opcional e não mede FPS Android.

## Anúncios de teste

A prova fica isolada da cena e do APK principal. Foi fixado o plugin MIT [godot-sdk-integrations/godot-admob v7.0](https://github.com/godot-sdk-integrations/godot-admob/tree/v7.0), testado pelo autor com Godot 4.7; o pacote inclui SDK Google Mobile Ads 24.9.0. O build Gradle customizado usa JDK 17 ou superior.

Para gerar o APK técnico:

```bash
bash scripts/export-android-ad-test.sh
```

O APK `builds/dig-and-wonder-ad-test.apk` usa pacote `.adtest`, permissão de internet e os IDs públicos de demonstração do Google, não IDs de produção. A tela separada carrega/exibe o rewarded apenas após toque; a contagem aumenta quando o SDK emite `rewarded_ad_user_earned_reward` e não no fechamento. Baixar e compilar Gradle requer rede. Instalar/exibir o anúncio, retornar de tela cheia e confirmar os sinais ainda depende de Android com Google Play Services e internet.

Esta prova não integra oferta, frequência ou recompensa ao ciclo de venda do jogo. Isso fica para a definição de identidade/layout. Antes de distribuição, configurar consentimento UMP conforme a região, declaração de dados, público-alvo e SDKs usados; não usar configuração de teste para produção.
