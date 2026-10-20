# Marco 1 — estado e entrega

20/10/2026. **Concluído no desktop. Marco 2 posteriormente autorizado e implementado: ver MARCO2.md.**

## Resumo do ponto de partida

Base `3e7aea8`, Godot 4.7.2, GDScript, Compatibility, modo manual por células e save 5. Arte pintada, paleta, área segura, backup e ferramentas de captura eram aproveitáveis. Alterações preexistentes em `project.godot`, metadados de importação, SVG legado da broca e PNGs locais foram preservadas e ficam fora do commit do marco.

Foram consultados AGENTS, README, desenvolvimento, identidade/direção artística, versões, Android e scripts de validação. Histórico do modo substituído foi arquivado em `docs/historico/`.

## Decisões

- Entrada única de defesa, sem modo extra ou engine nova. Carrinho fixo, dois canhões, Pioneira descartável, um inimigo e seis camadas de vida compartilhada.
- Canhões escolhem ameaça preparando ataque ao carrinho, depois menor ID. Pioneira não é defensor e não transfere dano para outra camada já revelada.
- Encontro final de dois Torrões, não boss. A reserva final não é descartada por uma fila cheia. Fase seguinte registrada como desbloqueada, mas não apresentada como conteúdo jogável.
- Derrota limpa temporários. Reinício/reabertura começa do zero com vida e cooldown completos; ouro e compras ficam. Voltar à abertura também inicia uma nova tentativa ao continuar.
- Migração significativa exige confirmação dentro do jogo. Moedas + mochila em 1:1, reforços da broca para Dano até 10; nada de converter itens sem regra. Snapshot de bytes verificado antes de escrever perfil 6 separado; original intacto.

## Arquivos principais

| Responsabilidade | Arquivos |
| --- | --- |
| Dados implementados e propostas | `content/defense/prototype.json`, `abilities.json` |
| Combate e estados | `scripts/gameplay/defense_run.gd` |
| Interface, input e ciclo de vida | `scripts/gameplay/game.gd`, `defense_layout.gd`, `safe_surface.gd` |
| Perfil e importação | `scripts/save/defense_save.gd`; leitor legado preservado |
| Verificação | `check-project.ps1`, `scripts/run-harnesses.sh`, harnesses de defesa/save/gameplay/Android/rigs |
| Captura | `scripts/capture_defense.gd`; quatro entradas antigas encaminham ao novo jogo |
| Regras e planejamento | README, AGENTS, DESENVOLVIMENTO, CATALOGO-DEFESA, IDENTIDADE-VISUAL, DIRECAO-DE-ARTE |
| Histórico e limites | VERSOES, ANDROID, REVISAO-ANDROID, REFERENCIAS-DE-DESIGN, `docs/historico/`, LICENCAS |

## Verificação executada

`./check-project.ps1` passou com Godot Windows: importação, boot e sete harnesses (lógica legada, save legado, defesa, save de defesa, integração, Android lógico e rigs preservados). Casos adicionados depois foram novamente executados nos harnesses afetados: reabertura da cena principal, limite de 24 projéteis e reserva final com fila saturada. Nenhum erro foi ignorado pelo verificador.

Cobertura: seleção de alvo, cooldown e custo zero por lançamento; recompensa única por camada/mob; limites; atributos e compras; derrota terminal e limpeza; vitória finita; ouro/compras preservados; esquemas 1–5; snapshot antes da conversão; recuperação por backup/temporário; input transformado, toque único, pausa e área segura. Dados de teste ficam isolados em `.tools/check-windows/`; harnesses que gravam exigem `D_W_TESTING=1`.

Foi usado o check nativo Windows; o bloqueio do Git Bash havia sido registrado na revisão anterior. O check nativo executa as mesmas verificações de importação/boot e os harnesses. Não houve exportação de APK. O aviso de Android build-tools do editor não é um teste de Android.

## Evidências reais

Pasta local ignorada: `builds/review/marco1-20261020/`.

- `01-inicio.png`: instrução do ciclo.
- `02-lancamento.png`, `03-combate.png`, `04-melhorias.png`: mesma tentativa, lançamentos/compras reais do protótipo.
- `05-vitoria.png`: continuação da mesma tentativa por ações normais e compras acessíveis, sem dano/ouro injetado.
- `06-derrota.png`: outra tentativa chegou à derrota por inatividade; ouro/níveis preservados.
- `07-reinicio.png`: primeira camada, vida cheia e cooldown completo.
- `08-paisagem.png`: encaixe com área segura; retrato continua sendo o formato principal.
- `09-importacao-fixture.png`: prévia isolada da confirmação, com valores de exemplo e gravação bloqueada; não são dados do jogador.
- `sequencia/` e `marco1-defesa.gif`: 240 quadros, vinte segundos a 12 fps, velocidade normal.

Capturas são **execução desktop em viewport de celular**, não mockups nem validação Android. O script de captura bloqueia persistência para não gravar sobre o perfil do jogador.

## Limites e próximo marco

Canhões e Torrão são placeholders nativos 2D; o terreno reaproveita a arte anterior em uma faixa única. O tratamento artístico e o ritmo ainda precisam de avaliação humana. A antecipação de ataque e o texto dos controles podem ser refinados. Não foram verificados toque real, tamanho físico em dp, FPS, memória, consumo, anúncios ou APK.

Catálogo completo é planejamento claramente identificado em `CATALOGO-DEFESA.md`. Próximo Marco 2: clareza, balanceamento e somente uma unidade alternativa. Não implementar novos inimigos, fases ou Serpente de Sucata nesta entrega.
