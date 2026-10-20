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

## Incremento 0.0.1 — baús e equipamento

Os baús fechados são achados a cada 24 m; as chaves aparecem 8 m antes de cada baú. A primeira dupla fica em 8/16 m. Ambos são gratuitos e não expiram. Um baú consome uma chave e gera uma recompensa de catálogo. Probabilidades iniciais: Ruim 28%, Boa 36%, Épica 22%, Lendária 9%, Única 3,5%, Mística 1,5%; peso de raridades altas cresce gradualmente com profundidade. O quinto baú sem Épico garante um; o décimo segundo sem Lendário garante um. Repetidos convertem em ouro (5, 9, 18, 34, 60 ou 100 conforme a raridade).

O catálogo traz cinco brocas além da inicial, com padrões de escavação próprios, e seis equipamentos para três encaixes. Os bônus afetam poder e valor por bloco. Save esquema 2 persiste inventário, equipamentos, chaves, baús, garantias e último achado; saves versão 1 migram preservando moedas, escavação e broca Prisma.

A interface oferece a Oficina de Achados no botão de baús do HUD. A oferta mostra raridade, descrição, estado encontrado/equipado, chances iniciais e garantias. Nenhum baú/chave é vendido ou ligado a anúncio. Detalhes de regras e limitação da pesquisa web estão em [REFERENCIAS-DE-DESIGN.md](REFERENCIAS-DE-DESIGN.md).

## Incremento 0.0.2 — companheira-broca e menus

O HUD agora separa Baús, Equipamentos e Personagem; a oficina reaproveita os mesmos três botões como navegação interna. O catálogo contém exatamente dez equipamentos em cada uma das seis raridades já definidas (60 no total), com três encaixes e quatro atributos aplicados à escavação: poder, moedas por bloco, velocidade e sorte para baús. A sorte desloca o peso de raridade; os valores dos itens e skins são limitados por raridade e mostram seus efeitos e contrapartidas.

A broca ganha rosto e passa a ser apresentada como companheira. Seis modelos mantêm seus padrões próprios. Treze skins (uma inicial e doze encontráveis em baús, duas por raridade) alteram a cor e aplicam uma habilidade passiva feita dos mesmos atributos, incluindo bônus e penalidades. O save esquema 3 registra coleção/equipamento de skins e migra esquemas 1 e 2, preservando brocas, equipamentos e progresso existentes.

## Fontes e limitações de pesquisa

Esta rodada tentou consulta web, mas o proxy do ambiente devolveu 403. Não atribuí regras novas a outros jogos. A expansão segue as referências já registradas e adota como decisões próprias efeitos legíveis, catálogo separado, skins que mudam números além da aparência e proteção contra duplicatas/sorte ruim. Retomar uma pesquisa atualizada de modelos de coleção e regras de divulgação antes de qualquer monetização com itens aleatórios.

## Incremento 0.0.3 — fases e chefes

As regiões são dados independentes em `content/phases/catalog.json`. Cada fase ocupa 24 trechos verticais (48 m), com paleta, materiais de rocha, minérios, uma descoberta, posições marcadas de chaves/baús e um chefe próprio. O mapa tem largura finita e não permite cavar além do chefe. Segurar o toque/clique sobre o chefe causa dano baseado no nível, poder e padrão da broca. Derrotá-lo concede moedas e abre imediatamente o próximo trecho.

O ciclo normal alterna Prado Cristalino, Gruta Basáltica e Jardim Fóssil. Ao derrotar o chefe, uma rolagem determinística tem 22% de chance de escolher a Fenda Aurora; o resultado entra no save e não se altera após reiniciar. A Fenda tem minério, descoberta, chefe e três pares de chaves/baús marcados no terreno; regiões regulares oferecem até dois pares. Os ícones revelam onde escavar, mas não há checklist nem coleta obrigatória. Somente o chefe abre a descida. Os blocos de cada mapa têm composição determinística, então o máximo de materiais é limitado pelo tamanho da fase.

O save esquema 5 grava a sequência de fases, chefe ativo, chefes vencidos e marcadores já recolhidos. Saves 1–4 são migrados para uma fase que corresponde aproximadamente à profundidade atual, sem remover moedas, chaves ou baús antigos. Harnesses cobrem o limite de mineração, quantidade máxima de achados, dano do chefe, escolha surpresa e retomada da fase salva.

## Ambiente Android — 20/10/2026

Preparado ambiente Linux x86_64 com OpenJDK 21, Android Platform Tools, SDK/API 36, Build Tools 36.0.0, NDK 28.1 e CMake 3.10.2.4988404. Os templates de exportação Godot 4.7.2 vieram do release oficial e passaram na checagem SHA-512. A configuração e o preset ficam documentados em `docs/ANDROID.md`; arquivos grandes do SDK, do Godot e APKs ficam fora do Git em `.tools/` e `builds/`.

O preset de teste produz APK debug assinado para ARM64, API mínima 24/alvo 36, pacote `com.otavio67.digandwonder` e sem permissão de internet. A exportação headless foi concluída, `apksigner verify` aprovou assinatura v2/v3 e `aapt` confirmou pacote/versões. Não havia aparelho conectado; logo, instalação, controles por toque, anúncios e desempenho continuam pendentes de validação física. O APK também não inclui integração de anúncios.
