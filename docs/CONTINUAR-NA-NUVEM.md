# Continuidade — Dig & Wonder

## Estado para retomar

Repositório `Otavio67/dig-and-wonder`, branch `prototype/first-playable`. Godot 4.7.2, Compatibility, GDScript; entrada `main.tscn`. Leia `AGENTS.md`, `README.md`, `DESENVOLVIMENTO.md`, `REVISAO-MARCO1.md`, `MARCO2.md` e veja `evidencias/20261020/` antes de alterar.

Marco 1 e revisão concluídos. Marco 2: Pioneira de quatro golpes e quebra, Torrão pintado, poço/solo, melhorias legíveis e um Escavador temporário. Save 6 intacto, importação legada sempre confirmada com snapshot. Primeiro impacto concede 2 ouro por tentativa, explicitamente solicitado; demais valores econômicos preservados. Só uma fase e um inimigo. Não começar Marco 3, novos inimigos, boss ou nova economia sem pedido.

## Preparação Linux / Codex Cloud

1. Conecte o repositório no ambiente Codex Cloud e selecione **prototype/first-playable**, não uma branch antiga.
2. Setup: `bash setup-cloud.sh` (baixa Godot oficial e verifica checksum).
3. Verificação: `bash check-project.sh && bash scripts/run-harnesses.sh`.
4. Não comitar `.tools/`, `.godot/`, saves, credenciais, APK ou caches. Evidências selecionadas estão em docs; capturas brutas em builds são locais e reproduzíveis pelos scripts.

Verificações de Linux/nuvem devem ser relatadas somente depois de executadas lá. Resultado obtido nesta sessão: check nativo Windows, sete harnesses e capturas desktop. Android físico não foi validado.

## Prompt para continuar depois

> Continue o Dig & Wonder na branch prototype/first-playable. Leia AGENTS.md e docs/CONTINUAR-NA-NUVEM.md e examine docs/evidencias/20261020/. Os Marcos 1 e 2 estão implementados. Primeiro confirme o checkout e resuma o estado e as limitações. Aguarde meu próximo objetivo antes de iniciar o Marco 3. Preserve save/economia, artes pintadas e as animações existentes. Não substitua o jogo por uma página HTML.

## Tentativa de envio direto

Em 20/10/2026, `codex cloud list --json` respondeu sem tarefas. O seletor de ambientes mostrou apenas a opção global, sem ambiente de projeto. O navegador disponível abriu a página pública do Codex e ofereceu Entrar. Nenhuma tarefa cloud foi criada, nem foi alegado que o código executou na nuvem. É necessário entrar e disponibilizar um ambiente do repositório para submeter uma tarefa com o prompt acima.

O snapshot de publicação também inclui os arquivos locais preexistentes do projeto: configuração do editor em project.godot, carenagem SVG da broca, drill_companion.png e metadados de importação. Não foram apagados ou substituídos nesta revisão. Binários locais, caches e saves permanecem fora do Git.
