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

O resultado fica em `builds/dig-and-wonder-android-debug.apk`, é assinado pela chave debug local e está excluído do Git. O preset é ARM64, pacote de teste `com.otavio67.digandwonder`, versão 0.0.4, API mínima 24 e API alvo 36. Não usa chave de publicação e não declara permissão de internet.

## Estado da validação

No ambiente de nuvem em 20/10/2026, Godot 4.7.2 exportou o APK sem erro, `apksigner verify` aprovou as assinaturas v2/v3 e `aapt` confirmou o identificador e as versões Android. Importação do projeto e boot headless também foram executados. Durante a inicialização do Godot houve aviso de conexão ao daemon ADB; uma chamada explícita a `adb devices -l` iniciou o daemon e listou zero dispositivos. A exportação não depende de aparelho conectado.

Ainda é necessário instalar o APK em um Android real e conferir abertura, proporções, toque, pausa/retorno e salvamento. Este ambiente remoto não vê as portas USB do computador do usuário. Para testar no PC local, instale as mesmas ferramentas ou transfira o APK e execute `adb install -r dig-and-wonder-android-debug.apk` com Depuração USB ativa. Nenhum anúncio foi integrado ou chamado; a permissão de internet fica desativada até existir uma ponte Android de anúncios e um anúncio recompensado de teste. Desempenho de 60 FPS não foi medido.

## Revisão 0.0.4

A análise das orientações oficiais, correções de input/área segura/ciclo de vida/save, resultados e pendências estão em [REVISAO-ANDROID.md](REVISAO-ANDROID.md). A exportação agora executa `scripts/verify-android-apk.py`: checa assinatura, integridade, manifesto, permissões, exclusão das ferramentas de desenvolvimento e alinhamentos ZIP/ELF de 16 KB.

`bash scripts/run-harnesses.sh` verifica a lógica e as regressões Android em desktop. Com um display X11 ou `xvfb-run`, `bash scripts/capture-visual.sh` gera imagens reais do Godot em `builds/review/`, usando dados de teste isolados. O processo visual é opcional e não mede FPS Android.

## Anúncios de teste

A exportação prepara a base Android, mas não instala um plugin de anúncios. A etapa seguinte deve escolher uma ponte Godot Android compatível com 4.7.2, habilitar build Gradle customizado, integrar apenas IDs oficiais de teste do Google Mobile Ads e confirmar a recompensa no callback do SDK. Não use IDs de produção para a validação técnica. A presença do SDK ou uma exportação bem-sucedida não prova que o anúncio foi servido nem que o ciclo de recompensa funciona.
