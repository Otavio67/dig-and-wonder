# Especificação inicial — Dig & Wonder

Consolidada em 19/10/2026 a partir das decisões do usuário e da proposta aprovada na conversa anterior. Quantidades e parâmetros de balanceamento são hipóteses para testar.

## Objetivo e público

Jogo Android de mineração cartoon 2D, fácil de entender nos primeiros segundos e interessante também para adolescentes e jovens adultos até aproximadamente 22 anos. Identidade divertida e expressiva, sem depender de uma apresentação exclusivamente infantil. Priorizar Android; considerar iOS depois de avaliar os resultados do lançamento.

## Promessa e ciclo principal

“Só mais uma melhoria para descobrir no que minha broca vai se transformar e o que existe mais fundo.”

Escavar → coletar → vender → melhorar/transformar → alcançar novas profundidades → descobrir e repetir.

Controle por toque ou arraste, utilizável com um dedo, com equivalente de mouse no computador. Testar a opção mais clara e confortável. Evitar decisões obrigatórias de “escavação inteligente”.

## Pilares

- Escavação satisfatória: resposta ao toque, fragmentos legíveis, som e movimento que reforcem cada quebra. Cavar precisa ser divertido antes dos upgrades.
- Transformações visíveis que mudem o padrão de destruição: broca tripla abre túneis simultâneos; elétrica encadeia destruição; laser atravessa uma linha. São exemplos propostos para testar.
- Descobertas: pistas do que vem a seguir, tesouros, fósseis, tecnologias e criaturas colecionáveis.
- Progressão simples: objetivo próximo claro, poucos menus e uma moeda principal na primeira versão.
- Conteúdo separado dos sistemas para permitir novas regiões, brocas e coleções.

## Escopo desejado para a primeira versão

- Três regiões: próxima da superfície, cavernas de cristais e uma região misteriosa mais profunda.
- Broca inicial e transformações com habilidades diferentes. Seis transformações foi um alvo proposto anteriormente, condicionado à validação do núcleo.
- Álbum de tesouros, fósseis e criaturas. Cerca de vinte descobertas é hipótese de conteúdo para a versão completa.
- Melhorias de velocidade, força e capacidade; valores a balancear.
- Ajudantes que geram moedas e uma base simples para melhorar.
- Progresso persistente e continuidade de escavação após as primeiras regiões. Geração repetível não deve ser anunciada como conteúdo inédito infinito.

Mundos de doces, cogumelos gigantes e dinossauros continuam possíveis em expansões ou surpresas. Não precisam dominar a identidade inicial. Sem multiplayer, árvore complexa de habilidades ou múltiplas moedas no início.

## Primeiro protótipo

Critérios reforçados pelo usuário: bonito, leve e jogável. Meta inicial de 60 FPS em Android intermediário, pendente de medição em aparelho real. Antes de ampliar conteúdo, gerar um APK mínimo e validar toque e anúncio recompensado de teste. Essa prova técnica ocorre separadamente do protótipo de escavação e não exige anúncios reais.

Construir uma fatia pequena: uma região, terreno escavável, controle por toque/mouse, coleta e venda, uma moeda, uma melhoria, broca básica e uma transformação perceptivelmente diferente, uma descoberta no álbum e progresso salvo. Interface inicial em português.

Critério de avanço: conseguir jogar e entender o controle, perceber a transformação e recuperar o save. Avaliar diversão com pessoas; aprovação técnica não comprova interesse do público.

## Organização técnica

Godot 4 com GDScript. Fixar a versão do editor e dos templates. Godot 4.7.2 foi escolhido nesta retomada; validar sua execução no ambiente de nuvem.

Estrutura sugerida:

```text
project.godot
scenes/                # jogo e interface
scripts/gameplay/      # terreno, broca, coleta, câmera
scripts/progression/   # melhorias, desbloqueios, economia
scripts/save/          # persistência e migração
content/biomes/
content/drills/
content/discoveries/
assets/                # arte, áudio, fontes e licenças
tests/
tools/                 # instalação/verificação reproduzíveis
docs/
```

Definir conteúdo em Resources ou formato validado com identificadores estáveis. Separar entrada, renderização, economia e persistência. Evitar preços e regras espalhados pela interface, assim como abstrações sem necessidade concreta.

Gerar terreno por trechos e descartar trechos distantes para limitar memória. Usar sementes fixas em testes reproduzíveis. Retrato é a hipótese inicial para controle com uma mão; validar durante o protótipo.

Persistir em `user://` com versão de esquema, validação e escrita que preserve um save anterior válido em caso de falha. Não versionar saves. Quando houver renda offline, limitar o intervalo, tratar relógio retrocedendo e evitar duplicação de recompensas.

## Visual e áudio

Cartoon 2D vibrante, silhuetas claras, máquinas expressivas e transformações perceptíveis em celular. Regiões com cores, formas e descobertas distintas. Efeitos devem reforçar a ação sem ocultar terreno ou interface.

Começar com arte provisória consistente. Registrar origem e licença de recursos externos. Referências de outros jogos na conversa não autorizam copiar seus recursos. Prever controles de áudio e redução de vibração/movimento; evitar tutoriais longos.

## Monetização

Anúncios recompensados opcionais sem bloquear o progresso normal. Fazer uma prova técnica antecipada com anúncio de teste para verificar a integração Android. Validar o núcleo sem anúncios reais e incorporar a experiência definitiva de anúncios depois, com confirmação correta da recompensa.

Antes de integrar/publicar, verificar políticas oficiais vigentes sobre público declarado, classificação, privacidade e anúncios. O público descrito aqui não determina por si só a classificação do aplicativo.

## Etapas

1. Repositório e ambiente reproduzível de Godot na nuvem; APK mínimo e prova de anúncio recompensado de teste em aparelho.
2. Protótipo de escavação, transformação e save, respeitando os critérios bonito, leve e jogável.
3. Refinamento do controle e da sensação de escavar.
4. Ampliação para três regiões, álbum e ajudantes.
5. APK e avaliação em Android real.
6. Arte/áudio finais, balanceamento, anúncios de teste e preparação da loja.

Eventos, robôs colecionáveis e expedições secretas ficam como possibilidades de atualizações.

## Autonomia

O usuário deu liberdade criativa para combinar ideias, resolver detalhes e discordar com bons motivos. Avançar em escolhas reversíveis sem pedir aprovação de cada detalhe. Registrar decisões relevantes e limitações.

Não inventar pesquisa, testes ou resultados comerciais. Números de concorrentes da conversa anterior não foram revalidados nesta retomada; não são premissas de receita ou alcance.

## Checkpoint do primeiro protótipo (19/10/2026)

Implementada uma região, mineração por células agrupadas em trechos de 12, venda por uma moeda, Broca Pioneira e Broca Prisma (padrão de três células), descoberta Semente lunar, álbum e save versionado com backup e recuperação. Conteúdo da região, catálogo de brocas e descobertas ficam em JSON separado dos sistemas. Arte provisória é desenhada com primitivas e paleta cartoon, sem recursos externos.

Verificações executadas com Godot 4.7.2: importação e boot headless; economia (saldo insuficiente, exato e venda); padrão básico versus prisma; save válido, rejeição de esquema inválido e recuperação do backup após corrupção. Não houve teste de APK, Android, toque real, anúncio ou FPS. Não há método de captura visual funcional no container; revisão visual manual continua necessária.

## Revisão incremental (19/10/2026)

Corrigidos input que minerava ao tocar fora da área de terreno, alinhamento de hit test com a grade, retomada da profundidade e nível salvo da broca, scroll que tornava a descoberta inalcançável e valor dos veios ignorado na venda. Harnesses confirmam coordenada central, HUD não minera, descoberta/scroll, valor maior do padrão prisma e nível da broca salvo. Continuam pendentes contraste/tipografia vistos em celular, validação visual, Android, anúncios e FPS.

Adicionada tela inicial com Iniciar/Continuar. O autosave persiste também dano parcial em pedra, além de moedas, mochila, profundidade, broca, células quebradas e descobertas. Mantido em `user://` persistente com backup: cache do Android pode ser limpo pelo sistema e não deve ser o único armazenamento de progresso.

Pesquisa de design resumida em `docs/REFERENCIAS-DE-DESIGN.md`. Aplicados objetivo dinâmico no HUD, bônus único de primeira venda, camadas com dureza/valor progressivos e descoberta que conduz ao próximo reforço. A oferta recompensada pós-venda está apenas planejada: requer integração Android e recompensa confirmada pelo SDK.

Camadas mostradas no terreno e camadas usadas na escavação agora compartilham a mesma consulta determinística por célula. O verificador de boot ignora somente as duas mensagens conhecidas de socket bloqueado do container; outros erros no log continuam falhando.

## Verificação

- Importação e análise de scripts no editor fixado em modo headless.
- Ciclo de coletar/vender/comprar e recursos insuficientes.
- Transformação altera o padrão de escavação.
- Descobertas não duplicam entradas no álbum.
- Salvar/reabrir, save inválido e versões do esquema.
- Limites de renda offline quando implementada.
- Revisão visual da interface e dos efeitos; headless não substitui esse teste.
- Toque, proporções de tela, pausa/retomada, memória e desempenho em Android real.

Registrar quais verificações foram executadas e os resultados. O primeiro marco exige protótipo executável e testes do seu escopo; publicação exige também validação no aparelho e preparação da loja.
