# Dig & Wonder — identidade Toy-Mecha Subterrâneo

Versão 1.3 — 20/10/2026. Base: direção fornecida pelo usuário, referências visuais anexadas, análise do vídeo de gameplay e revisões do HUD. Este é o guia vigente para novas imagens e para a evolução visual do projeto Unity. Não afirma que estas regras já estejam implementadas.

**Decisão de fluxo de 21/10/2026:** o usuário substituiu o hub Acampamento pelo menu ilustrado com **Iniciar a mineração**. As referências ao hub neste guia são históricas. Depois de iniciar, as vitórias seguem para a próxima fase/mapa dentro do jogo. A logo v3 foi aprovada após os ajustes de fonte e tamanho do “&”; referência em `UnityProject/Assets/_Game/Resources/Art/StartMenu/dig-and-wonder-logo-v3.png`. O fundo do menu reúne Pioneira, duas variantes decorativas, inimigos e uma mina com casas, máquinas e cristais. Logo, botão e textos permanecem separados da ilustração.

## 1. Conceito e intenção

**Pequenas máquinas explorando um mundo subterrâneo cheio de descobertas.**

Pilares: metal pesado nas estruturas, calor e carisma nos personagens e luz de descoberta nos cristais. Leitura rápida de RPG mobile com ambientação autoral. Cartoon 2D mecânico de formas compactas e robustas, com inspiração dieselpunk.

A referência de Legend of Slime orienta ritmo, clareza de combate automático e acesso à evolução durante a partida. Não copiar personagens, silhuetas, ícones, enquadramentos, painéis ou composição de tela como um conjunto. As imagens de oficina enviadas são referências de aproximação, não uma aprovação de todos os detalhes.

HUD é a interface durante a partida; hub é o acampamento. Não confundir os termos em briefings.

## 2. Assinatura visual

- **Ponta de broca:** formas cônicas, setas angulares e cortes diagonais em progresso, divisórias e marcadores ativos.
- **Cristal encaixado:** energia ciano incrustada em molduras metálicas, medidores e núcleos; aparenta fazer parte da máquina.
- **Chapa de manutenção:** cantos chanfrados em 45° ou suavemente arredondados. Interior limpo; rebites, tubulações e desgaste concentrados nas bordas externas.

Engrenagens ficam reservadas a evolução mecânica e, se esse recurso for aprovado no futuro, sucata. Não usar engrenagem como símbolo de ouro, moldura de todos os slots ou decoração repetida em tudo. Esta regra não cria uma moeda de sucata no gameplay.

## 3. Paleta funcional

| Papel | Cores | Regra |
| --- | --- | --- |
| Estrutura e profundidade | Petróleo `#1B263B`, carvão `#22212C` | Fundos e bases |
| Superfície de leitura | Aço `#4B485C` | Preenchimento sólido e tranquilo atrás de texto |
| Ação disponível | Verde oficina `#1E6F5C` a `#2A9D8F` | Comprar, melhorar, equipar quando a ação estiver disponível |
| Valor e recompensa | Âmbar `#FFB703` | Ouro, custos em ouro, recompensas e marcos de evolução |
| Energia e tecnologia | Ciano `#00F5D4` | Cristais, seleção, núcleos, feixes, habilidades e progresso energético |
| Calor dos materiais | Cobre `#9A6642`, ferrugem `#B5651D` | Pequenos detalhes e conexões; desgaste localizado |
| Alerta e perigo | Coral `#FF595E` | Fechar, dano iminente e avisos |
| Texto e luz | Creme `#FFF8E7` | Leitura principal e reflexos |

Exceção explícita: títulos principais podem usar âmbar como assinatura de marca. O chassi pode ter latão/dourado como material. Isso não transforma toda moldura ou todo botão em dourado. Preços em cristais usam seu ícone e identificação próprios; não devem parecer custos em ouro.

Neutros e variações de luminosidade são permitidos para volume e contraste. Saturação alta fica concentrada em informação relevante. Alertas, seleção e disponibilidade não dependem somente de cor: usar também forma, ícone e texto.

## 4. Tipografia e hierarquia

**Exceção solicitada em 21/10/2026 para o início:** a ação **INICIAR MINERAÇÃO** usa Titan One branca, grossa e de tamanho moderado, sem placa ou fundo visível. O estado anterior mostra rótulo e barra brancos de carregamento. A logo aprovada fica preservada; o cenário ativo dessa tela passa à proposta cartoon v2, com formas mais simples e menos brilho. As demais ações continuam com Nunito conforme a tabela.

Apenas duas famílias: **Titan One** para impacto e **Nunito** para leitura/UI.

| Aplicação | Família/peso | Tratamento |
| --- | --- | --- |
| OFICINA, DESCOBERTA e grandes títulos | Titan One | Caixa alta, âmbar, contorno escuro e sombra curta dura |
| Abas e ações | Nunito ExtraBold | Creme, sem degradê; sombra simples somente se necessária |
| Ouro, dano, nível e preços | Nunito ExtraBold | Números destacados; contorno no cenário, leitura limpa em painel sólido |
| Nomes de peças e atributos | Nunito Bold | Caixa alta apenas em rótulos curtos, como ATQ e PV |
| Descrições e requisitos | Nunito Regular/SemiBold | Escrita normal, sem contorno pesado nem relevo |

Base inicial para layout lógico de 360 pontos de largura: títulos 26–32, valores destacados 20–24, ações 16–18, texto corrente 14–16 e rótulos secundários 12–14. São valores de composição para calibrar no aparelho, não pixels fixos para toda resolução. No Canvas de 1080 de largura, aplicar escala equivalente e conferir o tamanho físico.

Manter espaço para acentos e descendentes. Cobrir português, números, pontuação e sinal de porcentagem. Não usar contorno para disfarçar fonte pequena. Somente títulos recebem o acabamento mais expressivo; evitar sombra, brilho e relevo em todos os textos.

Produzir pesos reais da Nunito quando possível; não transformar o mesmo peso 800 em toda a hierarquia. A fonte existente `DW Rounded ExtraBold` deriva de Nunito 800 e não substitui os pesos Regular, SemiBold e Bold. Preservar licenças e regras de renomeação ao preparar as fontes.

## 5. Desenho, materiais e iluminação

- Silhuetas compactas e reconhecíveis em tamanho pequeno. Peças grandes e funcionais, não excesso de microdetalhes.
- Contornos escuros mais fortes na silhueta externa e mais leves no interior. Na UI, espessura consistente entre componentes equivalentes.
- Volume em poucos planos de luz/sombra, com cel shading e pequenos reflexos duros no metal. Gradientes localizados podem modelar volume; evitar acabamento fotorrealista e plástico brilhante.
- Cobre, aço, borracha e cristal precisam parecer materiais diferentes. Cristal facetado; esteiras opacas; metal com reflexos curtos; pintura com desgaste restrito às bordas.
- Oficina com luz ambiente quente e acentos ciano. Definir a direção da luz por cena/folha e mantê-la coerente entre peças.
- Fundos mais suaves, com menor contraste e menos detalhes do que unidades e objetos interativos. Não aplicar o mesmo contorno pesado a toda pedra distante.

## 6. Personagens e inimigos

A broca é a protagonista. O visor funciona como rosto; olhos, inclinação do chassi, suspensão e reação das esteiras demonstram intenção. Evitar expressividade feita apenas por números e partículas.

Na oficina, vista em três quartos para apresentar a máquina. No combate lateral, pose e orientação adequadas à leitura dos ataques. Mesma construção, paleta, materiais e identidade nas duas vistas. Não espelhar cegamente peças assimétricas importantes.

Cada inimigo precisa de silhueta, personalidade, material dominante e preparação de ataque reconhecíveis. Não depender apenas de recoloração para diferenciar funções. Telegraphs devem sobreviver à redução de tamanho e à sobreposição de efeitos.

## 7. Oficina e HUD

### Oficina

Hierarquia: título → broca/plataforma → nível e XP → informações e melhorias → navegação. Reservar respiro ao redor da máquina. Slots laterais têm menos brilho/saturação do que ela.

Plataforma de manutenção com sinalização industrial localizada e luz ciano. Ferramentas e tubos contextualizam a cena sem atravessar textos nem criar falsos botões.

### Combate

A batalha lateral precisa continuar visível e compreensível: chão de apoio, plano dos personagens, inimigos, ataques e recompensas. Carteira como visor mecânico; habilidades como módulos encaixados; upgrades em chapas compactas.

Usar o vídeo como referência de continuidade entre batalha e evolução. A regra atual do Unity pausa o combate na Oficina; uma mudança para combate contínuo durante menus exige decisão de gameplay separada. A arte não deve pressupor que essa mudança já aconteceu.

Não copiar a divisão exata da tela de referência. Definir proporções após um estudo conjunto de combate e painel de melhorias, conferindo a menor tela suportada. Evitar cenário comprimido por barras, menus e decoração.

Hub/acampamento, combate e oficina compartilham materiais, ícones e linguagem de botões, com densidade adequada a cada função.

### Ajustes solicitados pelo usuário na revisão do HUD

- Navegação inferior contínua até as duas bordas laterais e o limite inferior da tela. Evitar bandeja flutuante, margens externas e grandes molduras individuais. A base decorativa pode chegar à borda; os alvos de toque continuam respeitando a safe area real.
- Abas base: **MINA, OFICINA, PETS, BAÚS, LOJA**. A revisão v4 propõe um sexto acesso **MODOS**, ao lado de Mina, para atender ao pedido de acesso à torre invertida. O nome sugerido para esse modo é **Profundezas**, ainda sujeito à aprovação. A aba Brocas foi substituída por Pets neste conceito de navegação. Ícones pequenos, simples e consistentes, com rótulos curtos; reduzir bastante a altura e o peso visual do dock anterior. Evitar miniaturas de cenário, retratos e volumes em perspectivas diferentes nos ícones; usar uma família de pictogramas de leitura imediata. O usuário aprovou as formas simplificadas da v3 e pediu cor: preservar silhuetas e acrescentar preenchimentos coerentes com a paleta.
- Exibir até **cinco espaços de poderes**, além de um controle separado com o rótulo curto **AUTO** (não escrever AUTO-ATAQUE na interface). Este é um botão liga/desliga do ataque básico automático, com estados ON/OFF reconhecíveis. Não significa autoativação dos poderes nem pausa global.
- O texto AUTO deve ser grande e legível; não tratá-lo como legenda minúscula. O estado ON/OFF permanece visível no controle.
- Broca e inimigos menores e mais afastados, com faixa de chão livre entre a protagonista e o grupo adversário. Preservar a leitura das barras e valorizar a profundidade e o espaço do cenário. A referência de enquadramento é a vista ampla do Jardim Fóssil enviada pelo usuário.
- Refinamento posterior: reduzir novamente as unidades em relação ao conceito v2 e afastar um pouco mais o enquadramento do mundo, preservando o tamanho e a divisão do HUD. A proporção final permanece em revisão pelo usuário.
- O restante do conceito v1 foi considerado adequado pelo usuário; preservar carteira, progresso de fase, missão e melhorias salvo novos pedidos.
- Melhorias formam uma lista com rolagem vertical por arrasto apenas dentro do painel de atributos. Missão, poderes, carteira e navegação ficam fixos. Mostrar trilho de rolagem e parte da próxima linha para revelar conteúdo adicional; manter cadeados e requisitos legíveis nos desbloqueios posteriores. Não copiar números/requisitos da referência externa. Autorreparo é o nome descritivo proposto para recuperação automática de PV.
- Direção corrigida pelo usuário: broca à esquerda avançando para a direita; inimigos chegam pela direita. A direção invertida do conceito v4 foi corrigida. Não espelhar o HUD ou seus textos ao mudar a direção dos sprites.
- Organização dos atributos: um único ícone por linha, com o nível (ex.: NV. 0) numa faixa própria abaixo do desenho, inclusive em melhorias futuras. Reservar padding em todas as bordas, sem espremer nível e ícone. Na coluna seguinte, nome do atributo em cima como destaque principal (Nunito ExtraBold) e valor menor e mais leve abaixo (Nunito Regular/SemiBold, ex.: ATQ / 6 de ATQ, VIDA / 120 de vida). Esta regra específica prevalece sobre o peso genérico dos números na seção 4. Não repetir ícones pequenos junto dos valores nem deixar o valor isolado no meio da linha. Botão de melhoria e custo ficam à direita. Se necessário aumentar as linhas dentro da área rolável, não comprimir o cenário. Todos os itens seguem o mesmo padrão; a última linha pode aparecer parcialmente para sinalizar rolagem.
- O exemplo de formato 0.006 de ATQ foi levantado pelo usuário para discussão, não define conversão de escala. Não transformar dano 6 em 0,006 sem decisão explícita sobre unidades. A convenção de abreviações/decimais permanece pendente.
- Refinamento de Pets na v7: cabeça mecânica mais gráfica, contorno firme, poucas áreas de cor, materiais foscos e expressão simples. Evitar brilho plástico e microdetalhes. Variante em avaliação.
- Refinamento de espaçamento: centralizar verticalmente o conjunto ícone/rótulo nas abas inferiores, equilibrando respiro superior e inferior e evitando faixa vazia excessiva sob o dock. Separar visualmente as molduras douradas consecutivas dos atributos com intervalos escuros regulares; desenhos internos ligeiramente menores, sem reduzir a leitura do nível nem encostá-lo na borda.
- Estudos de ícones na v5: Modos usa camadas de rocha com cristal, sem elevador/seta; Pets testa a cabeça de um companheiro mecânico com visor ciano. São propostas em avaliação, não aprovação definitiva.

Essas decisões orientam imagens e futura implementação. Pets, Loja, cinco poderes e alternância de auto-ataque no conceito não comprovam funcionalidades implementadas nem autorizam alterar economia ou regras de combate por conta própria.

## 8. Layout, contraste e leitura

- Grade de espaçamento inicial: 4, 8, 12, 16 e 24 pontos lógicos. Padding interno de cartões e botões coerente, sem texto encostando em rebites ou molduras.
- Área de toque como alvo inicial de 48 × 48 pontos lógicos, verificada por aparelho. Ícone visível pode ser menor; áreas de interação não se sobrepõem.
- Respeitar safe area e gestos do sistema. Testar retrato 16:9, 19,5:9 e 20:9; definir adaptação para telas maiores antes de expandir a composição.
- Textos explicativos longos podem rolar. Não encolher automaticamente tudo para caber. Títulos, preço e ação principal precisam permanecer claros.
- Alvos de contraste do projeto: 4,5:1 para texto corrente e 3:1 para texto grande/elementos importantes, medidos nas combinações finais. Não declarar acessibilidade validada apenas pela escolha da paleta.
- Fundo ciano/âmbar claro pede texto escuro quando creme perder contraste. Legibilidade tem precedência sobre a preferência de cor do rótulo.
- Formatação numérica consistente. Abreviações de grandes valores precisam de uma convenção documentada; permitir consultar valores detalhados quando necessário. Não alternar vírgula/ponto sem critério.

## 9. Estados de botões e feedback

| Estado | Aparência e informação |
| --- | --- |
| Disponível | Verde oficina, face clara, base escura; ação e preço legíveis |
| Pressionado | Face desce discretamente; hitbox e layout permanecem estáveis |
| Selecionado | Cristal/encaixe ciano e marcador de broca; não depender só de brilho |
| Ouro insuficiente | Face dessaturada, preço visível e indicação clara do recurso faltante; sem cadeado de desbloqueio |
| Bloqueado por requisito | Cadeado dedicado e requisito exato, como “Disponível na fase 10” |
| Nível máximo | Indicar “Máximo”; não continuar mostrando uma compra possível |
| Em preparação | Identificar conteúdo futuro; nunca confundir com recompensa já disponível |
| Compra concluída | Atualizar saldo, nível e atributo juntos; feedback curto ancorado ao item |
| Falha de gravação | Mensagem persistente e legível, respeitando o bloqueio real de compras/combate |

Não transformar falta de dinheiro em alerta coral persistente. Coral é reservado a fechar, perigo e avisos relevantes. Um ponto de notificação precisa indicar uma ação real e desaparecer quando resolvida.

## 10. Movimento e efeitos

Movimento pesado e elástico de máquina compacta: suspensão curta, esteira com tração, recuo e recuperação. A ponta da broca deve se relacionar ao alvo e ao chão.

Toda animação de combate segue eventos reais da simulação. Antecipação, contato e recuperação devem respeitar a cadência existente. A arte não muda dano, cooldown ou momento de recompensa.

Pontos de partida para calibragem: pressão de botão em 80–120 ms e transições de painel em 140–220 ms. Efeitos mais expressivos ficam em habilidades, chefes e descobertas, com leitura preservada nos ataques frequentes.

HUD e texto não tremem junto com o cenário. Limitar flashes, partículas e números simultâneos. Prever redução de tremor/brilho. Recompensas visuais aparecem após concessão real; moedas decorativas não prometem valores inexistentes.

## 11. Ambientação

O subterrâneo não deve ser uniformemente escuro. Alternar cavernas minerais, raízes e terra, ruínas mecânicas e regiões estranhas de acordo com o conteúdo aprovado.

Separar planos: primeiro plano discreto, chão de combate legível, cenário intermediário e fundo atmosférico. Contraste diminui à distância. Parallax e partículas ambientais dão profundidade sem competir com projéteis e inimigos.

Cada região pode ter uma cor de ambiente dominante. Cores semânticas do HUD continuam estáveis. Ciano de seleção e coral de perigo precisam permanecer reconhecíveis mesmo em regiões com essas tonalidades.

### Cenário acompanhando a exploração

O usuário pediu cenário animado acompanhando o avanço: abrigos/casas de mineiros, postes, ferramentas, carrinhos e outras estruturas passam ao fundo, com vento visível e leve. Para deslocamento à direita, os elementos do mundo passam para a esquerda em relação à câmera. Plano distante se move menos; elementos próximos se movem mais. Evitar sensação de cenário em esteira durante golpes: propõe-se desacelerar o deslocamento durante confrontos, sujeito à validação de gameplay.

Vento e poeira devem ter movimento próprio sutil, sem linhas sobre textos ou excesso de partículas. Tecidos e lanternas podem reagir discretamente. Separar camadas de céu/caverna distante, cenário intermediário, chão e elementos próximos na futura produção; os mockups gerados continuam sendo imagens estáticas. Parallax não deve alterar posições lógicas, alcance ou temporização de combate.

## 12. Iconografia e produção de assets

Proibido usar emojis/glifos de fonte como substitutos de ícones de recursos, cadeados, peças e atributos. Usar ícones dedicados com silhueta consistente: moeda de ouro, cristal, coração blindado, broca de ataque e chave de reparo.

Preferir fontes vetoriais editáveis para iconografia de UI. A implementação pode usar sprites rasterizados e atlases apropriados ao Unity; não pressupor suporte SVG no runtime. Não gerar por IA um substituto de ícone vetorial estabelecido quando editar o vetor for o caminho adequado.

Para menus funcionais, produzir fundo, molduras, botões, ícones, personagem e efeitos separados. Texto final é renderizado no Unity; não embutir preços, níveis ou rótulos nos PNGs de produção.

Mockups conceituais podem conter texto ilustrativo, mas devem ser identificados como conceitos. Não apresentá-los como captura funcional do jogo.

Brief de cada asset: uso, dimensão/proporção, vista/perspectiva, direção da luz, materiais, paleta, transparência, contorno e referências aprovadas. Registrar nome/versão e proveniência. Botões/painéis devem permitir escala em nove partes; folhas de animação precisam de pivô, escala e enquadramento consistentes.

## 13. Checagem obrigatória antes e depois de gerar imagens

Antes de qualquer geração ou edição de imagem do projeto, ler este arquivo integralmente e as instruções da skill aplicável. Em seguida, conferir:

1. Qual asset/tela foi pedido e qual parte do guia se aplica?
2. Há referência aprovada para o mesmo personagem, material ou componente? Reutilizá-la para manter consistência.
3. É conceito, recurso de produção, folha de animação ou ícone? Escolher o formato adequado; não gerar raster quando vetor/código for mais apropriado.
4. Paleta funcional, famílias tipográficas e hierarquia foram respeitadas?
5. Há protagonismo da broca, interior limpo nos painéis e detalhes nas bordas?
6. Não há engrenagens excessivas, emojis de recursos nem cópia direta do jogo de referência?
7. O prompt especifica enquadramento, iluminação, materiais e transparência corretos?
8. A composição não anuncia sistemas/recompensas ainda não implementados como funcionais?

Após gerar, inspecionar a imagem e avaliar esses mesmos pontos, além de anatomia mecânica, peças duplicadas, texto incorreto, margens e legibilidade em tamanho mobile. Corrigir desvios relevantes antes de entregar. A checagem é revisão de arte; ela não executa nem valida o gameplay.

Não gerar imagens automaticamente só porque este guia existe. A geração exige uma tarefa autorizada pelo usuário. Não implementar novas mecânicas ou alterar economia/save a partir de um mockup.

## 14. Próximas definições

Permanecem em discussão: composição final do HUD, primeira região a receber arte final, elenco inicial de inimigos a produzir, intensidade de desgaste dos materiais, ícone definitivo do ouro e eventual continuidade de combate ao abrir menus. Decidir com estudos pequenos e comparáveis.

Sequência recomendada: uma tela conjunta de combate + HUD → uma oficina coerente → uma folha com protagonista/inimigo/ícones → integração e revisão no Unity. Evitar produzir o catálogo completo antes de validar essa pequena amostra.

## Implementação de 29/10/2026 — coleção

O pedido de integrar as sugestões inclui o álbum. A navegação usa ÁLBUM no lugar da aba de Pets ainda não implementada. Ícone vetorial de livro com cristal, contorno petróleo e preenchimentos creme/aço/ciano, 256 × 256 transparente; sem texto embutido. Revisão do ícone realizada, incluindo margens e silhueta. O álbum usa cartões com nomes e descrições; ilustrações exclusivas dos achados não foram produzidas. Brocas reutilizam módulos e cores; inimigos usam folhas existentes com funções distintas. Isso não equivale a aprovação visual no Editor ou aparelho.

## Polimento solicitado em 29/10/2026 — HUD 0.0.29

O usuário pediu retirar o nível da broca no cenário, recuperar os ícones definidos, corrigir textos e devolver os upgrades. Nível e XP ficam no cartão superior da broca; vida acompanha a altura renderizada da máquina. O menu de ajuda é modal e mantém a lista de melhorias por baixo. A navegação permanece funcional, com álbum no lugar de Pets.

Pictogramas de Mina, Modos, Oficina, Baús, Loja, ATQ, VIDA, AUTORREPARO e CADÊNCIA são recortes explícitos da referência v8 existente, sem gerar outra família. `hud-approved-v8.png` preserva o arquivo de referência inteiro; somente as áreas de desenho são renderizadas como sprites. Nenhum texto, preço ou nível do conceito é usado como informação funcional. Álbum e ícones sem equivalente na referência mantêm seus recursos existentes. Essa integração é uma etapa de produção; fontes vetoriais anteriores permanecem disponíveis.

A revisão adicional da navegação mede as seis células internas da moldura para alinhar os botões. Não dividir a largura total do atlas em seis, pois isso inclui as tampas laterais e desloca ícones e textos.
