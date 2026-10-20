# Identidade visual — Dig & Wonder

Direção visual inicial aplicada ao protótipo em 20/10/2026. A proposta combina as referências enviadas pelo usuário com observações de jogos mobile atuais; personagens, telas e elementos de interface são originais do projeto.

## Ideia central

Uma pequena equipe de exploração avança por uma mina viva, guiada por uma broca-companheira curiosa. O jogador deve perceber a máquina, a equipe, o objetivo imediato e o próximo trecho de terreno sem procurar pela tela.

**Tom:** aventura otimista, engenhocas artesanais e mistério subterrâneo. A identidade tem energia e contraste, mas evita aparência infantilizada ou excesso de informação.

## Linguagem visual

- Cartoon 2D com aparência vetorial, base e sombra em dois tons, reflexos brancos pequenos, contorno escuro de aproximadamente 3 px no tamanho de referência e silhuetas reconhecíveis em telas pequenas.
- Máquinas de latão e esmalte verde-azulado, com núcleo de cristal turquesa; formas arredondadas, peças aparentes e pequenos detalhes de oficina.
- Mineiros com equipamento útil, rostos expressivos e cores de roupa distintas. Lia coleta amostras; Bento cuida das ferramentas.
- Mina em camadas: parede escura nas laterais, rocha colorida no plano de escavação e cristais luminosos para destacar achados.
- Cartões e botões com cantos arredondados, contorno azul-petróleo e sombra curta. A cor dourada marca recompensa e ação; turquesa marca energia e melhoria.
- Uma única mensagem de objetivo por vez. Mineração manual e alvo tocável continuam no centro do fluxo.

## Paleta inicial

| Uso | Cor | Hex |
| --- | --- | --- |
| Fundo profundo | Azul-petróleo | `#123448` |
| Contorno e texto | Tinta escura | `#17303D` |
| Painel claro | Creme | `#FFF4D8` |
| Núcleo/energia | Turquesa | `#39D4C1` |
| Recompensa/ação principal | Latão dourado | `#F2B84B` |
| Ênfase e risco | Coral | `#F27C67` |
| Cristal | Menta luminosa | `#B7FFF1` |

As cores de rocha, minério e chefe continuam definidas por região em `content/phases/catalog.json`; a paleta acima organiza interface, personagens e efeitos comuns.

## Composição de jogo

1. Cabeçalho compacto para moedas, álbum, região, profundidade e objetivo.
2. Faixa mecânica com broca e mineiros como silhuetas de identificação.
3. Campo vertical de escavação com células, veios e laterais de caverna contrastantes.
4. Barra inferior persistente para mochila, venda e melhoria.

O enquadramento vertical das imagens de referência orienta a hierarquia visual, não o conteúdo específico. O jogo mantém a fantasia de escavar à mão e transformar a broca. Não adota a fusão de mineiros, personagens, máquinas, textos ou telas de outros jogos.

## Pesquisa de referências

Consulta feita em 20/10/2026 a páginas oficiais do Google Play e ao guia oficial de *Gold & Goblins*:

- [Gold and Goblins: Mining Games — Google Play](https://play.google.com/store/apps/details?id=com.redcell.goldandgoblins): a página descreve mineiros goblins, quebra de rochas, recompensas, regiões e melhoria. O guia oficial apresenta um ciclo repetido de trabalhadores, rochas e abertura de novas áreas. A referência serve para observar leitura da equipe e avanço por trechos; o Dig & Wonder mantém a broca-companheira e a ação manual.
- [Idle Miner Tycoon — Google Play](https://play.google.com/store/apps/details?id=com.fluffyfairygames.idleminertycoon): a página atual descreve upgrades e biomas. Serve como referência para variar a leitura das regiões conforme o progresso; a gestão de minas e automação não são parte da fatia visual atual.
- [My Little Universe — Google Play](https://play.google.com/store/apps/details?id=com.savetheworld.game): a página destaca mineração, exploração casual, combate e ambientes variados em uma apresentação estilizada. Serve para observar como personagem e mundo sustentam uma aventura visual contínua.
- [Guia oficial para iniciantes — Gold & Goblins](https://appquantum.helpshift.com/hc/en/7-gold-and-goblins/faq/54-welcome-to-gold-and-goblins-beginner-s-guide/): consultado para confirmar o ciclo de jogo descrito pela própria equipe.

Os três títulos foram usados como referências de categoria e apresentação. Não são uma avaliação comparativa de qualidade nem autorização para reutilizar seus ativos. As três imagens anexadas pelo usuário são a referência mais direta para composição vertical, máquinas centrais, pequenos mineiros, contraste e energia dos botões.

## Ativos e animação — etapa 1

Ativos ativos: `assets/personagens/lia_{cabeca,corpo,braco}.svg`, `bento_{cabeca,corpo,braco}.svg`, `cristal.svg`, `chave.svg`, `broca_corpo.svg`, `broca_ponta.svg` e `broca_nucleo.svg`. As formas são originais, escritas em SVG para este projeto a partir da direção existente. A importação em 2× e o filtro linear evitam aparência de pixel art nas peças.

Lia mantém o cristal, capacete dourado e lenço coral; Bento mantém a chave, capacete turquesa e lenço dourado. A broca tem corpo de latão, núcleo turquesa, rosto curioso e ponta separada. Modelos existentes mantêm seus identificadores e acentos; Prisma/Trinca exibem pontas adicionais. Nenhuma habilidade foi adicionada.

`scripts/visual/expedition_actor.gd` monta pivôs locais com Tween para respiração, piscada, braço, balanço, ponta e impacto. `expedition_visuals.gd` reutiliza os três rigs no início, na mina e no menu existente. Dano real dispara braço/ponta; quebra real dispara squash/stretch. Célula vazia não dispara animação. Os rigs não alteram colisão, posição lógica, alcance, velocidade, economia, save ou anúncios.

Os PNGs em `assets/characters/` são a direção anterior e ficam como legado, fora do render ativo. Blocos, tema e polimento continuam pendentes nas etapas 2–4; esta etapa não promete a transformação completa da interface.

Proveniência em `assets/LICENCAS.md`. Evidências reais e limitações em README, seção Etapa visual 1.
