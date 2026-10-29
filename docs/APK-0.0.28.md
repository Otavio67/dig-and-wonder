# APK de teste 0.0.28 — 29/10/2026

Unity 6000.6.6f1 instalado localmente, com Android Build Support, NDK r27c, SDK e OpenJDK 17.0.18+8. Instalação dos módulos concluiu com código zero. O build Android terminou com `Succeeded`.

APK local (fora do Git): `.tools/build/DigAndWonder-0.0.28.apk`, 62.412.064 bytes (~62,4 MB). Aplicativo `com.DigAndWonder.DigWonder`, versão 0.0.28/código 28, Android mínimo API 26 (Android 8), target API 36, ARM64. Assinatura de teste verificada com `apksigner`, esquema v2. SHA-256: `44e54bca1002c93bd9f03424501fbde1fdedbb6837b486bf5728f6cd821c2505`.

## Correção necessária no ambiente de build

A importação na pasta principal falhou com `MaterialReferenceChanger` ausente: dois scripts do SRP Core 17.6.0 não tinham os respectivos `.meta` e eram ignorados em pacote imutável. Há [investigação da equipe Unity sobre esse erro e caminhos longos no Windows](https://discussions.unity.com/t/errors-when-creating-a-new-unity-6-3-urp-project-the-type-or-namespace-name-materialreferencechanger-could-not-be-found/1701487/28).

Foi criada uma cópia isolada em `C:/Users/pctri/dw-apk-0028/U`, contendo Assets, Packages e ProjectSettings atuais; `content` fica ao lado de `U`. O SRP Core 17.6.0 foi copiado do cache para `U/Packages/com.unity.render-pipelines.core`, permitindo ao Editor gerar os metadados ausentes. Os testes internos desse pacote, que exigem `Unity.TestUtilities` não distribuído no projeto, foram preservados em `C:/Users/pctri/dw-apk-0028/CorePackageTests`, fora da importação. Nenhum código runtime do pacote foi modificado. As dependências do repositório principal permanecem intactas; para repetir este build use a cópia preparada. Não importar a cópia temporária sobre saves ou sobre o projeto principal.

Comando utilizado: Editor 6000.6.6f1 em `-batchmode -nographics`, `-projectPath C:/Users/pctri/dw-apk-0028/U`, `-buildTarget Android`, `-executeMethod DigAndWonder.Editor.PrototypeProjectSetup.BuildAndroidDebugApk` e `-buildOutput` apontando para o APK acima. Log completo em `.tools/logs/android-0.0.28-build-final.log`; procedência e hash em `.tools/build/DigAndWonder-0.0.28.provenance.json`.

## Evidência e limites

Scripts, recursos e aplicativo compilaram no Editor exigido. O APK passou na leitura de manifest com AAPT e na verificação de assinatura. Isso amplia a evidência anterior de compilação com stubs. O build ainda apresenta avisos de APIs obsoletas e ferramentas; sucesso de build não equivale a ausência de avisos.

Não houve aparelho conectado no ADB: instalação, abertura, visual, toque, save em Android e desempenho ainda precisam ser verificados. A cópia do build inclui o asset TMP fallback local que já estava modificado antes desta tarefa; ele não foi incluído no commit. Para teste no aparelho, transferir o APK e abrir o arquivo no Android, permitindo a instalação pelo aplicativo usado para abri-lo quando solicitado. Não apagar o progresso anterior para resolver eventual conflito de assinatura; nesse caso, preservar o save e investigar antes.
