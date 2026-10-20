# Continuidade na nuvem

O projeto está em desenvolvimento neste ambiente Linux x86_64 remoto e no repositório privado [Otavio67/dig-and-wonder](https://github.com/Otavio67/dig-and-wonder). A branch de trabalho é `prototype/first-playable`; as versões funcionais recebem tags `v0.0.x`. O checkpoint `v0.0.5` prepara os APKs de jogo e de teste AdMob.

## Configuração

1. Repositório GitHub conectado e checkpoint `v0.0.5` publicado.
2. Godot 4.7.2, JDK, SDK Android, templates, Build Tools e NDK disponíveis em `.tools/` ou no ambiente do usuário; APKs permanecem fora do Git.
3. Para retomar, rode `bash setup-cloud.sh`, `bash scripts/setup-android.sh`, `bash check-project.sh` e `bash scripts/run-harnesses.sh`.
4. Antes de alterar identidade visual/layout, aguarda-se validação de APKs em aparelho. KVM e dispositivo USB não estão expostos a este container.

Fonte: [documentação oficial de ambientes do Codex Cloud](https://learn.chatgpt.com/docs/environments/cloud-environments).

## Android e revisão visual

Código e verificações podem rodar em Linux na nuvem. Gerar APK exige configurar JDK, Android SDK, templates e exportação, conforme a [documentação do Godot](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html). Não versionar chaves/senhas nem criar chave definitiva de publicação sem armazenamento e recuperação definidos.

Capturas foram revisadas por Xvfb/Mesa; elas não comprovam toque, desempenho ou anúncios em Android. A prova de anúncio recompensado usa `scripts/export-android-ad-test.sh` e apenas IDs de demonstração. A exibição real ainda requer aparelho com Play Services e internet. A API GitHub Actions/Release devolveu `Forbidden` ao tentar consultar resultados e anexar APK; `git push` confirmou branch e tags no remoto.

## Prompt de continuidade

```text
Continue o Dig & Wonder em Godot 4/GDScript, para Android. Leia README.md e
docs/DESENVOLVIMENTO.md. A direção aprovada é mineração cartoon 2D, controle
simples, brocas que transformam o padrão de escavação, mundos inesperados,
álbum e tycoon simples. O público inclui jovens até aproximadamente 22 anos.

Configure e valide o ambiente com Godot 4.7.2. Implemente o primeiro protótipo:
uma região, escavação, coleta/venda, melhoria, uma transformação distinta,
uma descoberta e progresso salvo. Separe conteúdo dos sistemas principais.

Resolva detalhes reversíveis com autonomia. Use arte provisória consistente.
Não integre anúncios reais antes de validar o núcleo. Execute os testes
adequados e documente resultados, instruções reproduzíveis e o que exige
revisão visual ou Android real. Conclua esse marco antes de ampliar o conteúdo.
```
