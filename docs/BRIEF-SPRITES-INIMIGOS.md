# Brief de sprites dos inimigos — Dig & Wonder

Documento preparado para enviar a uma IA de concept art, sprite sheet ou ilustração 2D. O objetivo é produzir uma família visual consistente para o combate lateral da broca.

## Direção artística obrigatória

**Estilo:** cartoon 2D chibi de aventura subterrânea, acabamento de ilustração para jogo mobile, formas arredondadas, materiais legíveis e humor visual. A broca protagonista é um veículo vivo: os inimigos devem parecer habitantes da mesma caverna, com escala menor ou próxima da broca e leitura imediata em uma tela de celular.

**Linha e volume:** contorno externo escuro azul-marinho quase preto, espessura visual de 6–10 px em uma tela de 1024 px; contornos internos mais finos; sombras em blocos suaves; brilho especular pequeno em metal, cristal e olhos. Não usar realismo, gore, sujeira excessiva ou textura fotográfica.

**Paleta de mundo:** azul petróleo `#1B263B`, teal `#1E6F5C`, âmbar `#FFB703`, ciano cristal `#00F5D4`, coral `#FF595E`, creme `#FFF8E7`, rocha `#4B485C` e carvão `#22212C`. Cada inimigo recebe uma cor de assinatura, mas o contorno e o creme dos olhos devem manter a família visual.

**Composição:** vista lateral 3/4 voltada para a direita, pés ou base apoiados no chão, corpo inteiro visível, silhueta separada do fundo, três quartos de frente suficientes para mostrar rosto e ataque. Entregar PNG transparente, sem cenário, sem texto, sem números, sem barra de vida e sem logotipo.

**Proporção sugerida:** canvas 512×512 px, personagem ocupando 78–86% da altura, margem transparente de pelo menos 8%, pivô lógico no centro da base. Para unidades grandes, preservar a mesma base e reduzir apenas a largura para não cortar a leitura do terreno.

**Camadas separadas:** `Body`, `HeadOrFace`, `Accessory`, `WeaponOrAttack`, `HitFlashMask`, `Shadow`. A sombra deve ser uma camada própria. Peças que giram, pulsam, abrem, fecham, saltam ou recuam não devem ser achatadas no mesmo desenho.

**Estados mínimos de animação:** `idle` (2–4 poses), `telegraph` (preparação), `attack`, `recover`, `hit`, `defeat`. O idle deve ter apenas uma parte respirando, piscando ou balançando; não fazer todos os elementos pulsarem juntos. Ataques precisam mostrar antecipação e recuperação.

**Regra de leitura:** o jogador deve saber onde o ataque vai acontecer antes do dano. O telegraph é um anel, faixa, sombra, partícula ou brilho com cor própria. Vermelho/coral indica dano iminente, amarelo indica preparação, ciano indica informação verdadeira e roxo indica distorção da Dimensão Fraturada.

## Prompt-base para a IA de arte

> Criar um sprite 2D cartoon chibi para o jogo mobile Dig & Wonder, aventura de escavação lateral. Vista 3/4 voltada para a direita, corpo inteiro, silhueta forte, contorno azul-marinho espesso, sombras em blocos suaves, cores âmbar/teal/ciano/creme, expressão simpática e levemente engraçada, acabamento de ilustração limpa para jogo, PNG com fundo transparente, canvas 512×512, personagem apoiado no chão, sem texto, sem interface, sem cenário, sem marca d’água. Respeitar a descrição específica abaixo. Separar visualmente corpo, rosto, acessório, arma e efeito de ataque para futura animação.

**Negative prompt comum:** realismo, horror, sangue, gore, anatomia humana realista, fundo completo, perspectiva frontal plana, personagem cortado, texto, letras, números, HUD, watermark, baixa resolução, ruído, excesso de detalhes minúsculos, olhos sem brilho, contorno inconsistente, pose flutuando.

## Inimigos por faixa de campanha

### Faixa 1 — fases 1–10: Terra de Entrada

#### 1. `slime_terra` — Gosma de Terra

- **Silhueta:** gota baixa e larga, topo irregular com duas pedrinhas, base achatada como uma poça; olhos creme grandes e boca pequena convencida.
- **Materiais e cores:** corpo marrom argiloso `#9A6642`, sombra terracota, pedrinhas cinza, brilho úmido teal bem discreto.
- **Acessório e personalidade:** uma folhinha presa no topo; postura desajeitada, sorriso de quem tenta parecer perigoso e escorrega.
- **Animação:** idle ondula só a borda; telegraph achata e cria uma poça escura; ataque salta em arco curto; recuperação balança e recolhe as pedras.
- **Efeito:** respingos de terra creme no pouso; telegraph coral pequeno sob o corpo; derrota vira três poças menores que evaporam.

#### 2. `morcego_cascalho_ii` — Morcego Cascudo

- **Silhueta:** corpo oval de cascalho, asas curtas de couro, orelhas pontudas e cauda de pedrinha; deve continuar legível quando pequeno.
- **Materiais e cores:** pedras cinza `#77747A`, couro marrom escuro, olhos âmbar, duas marcas ciano nas asas.
- **Acessório e personalidade:** uma porca metálica como brinco; expressão brincalhona, faz pose antes de voar.
- **Animação:** idle asas alternadas; telegraph duas pedras orbitam o alvo; ataque sobe, faz curva irregular, mergulha e foge; recuperação sacode poeira.
- **Efeito:** rastro de pedrinhas e sombra circular no chão; impacto com três fragmentos, sem fumaça pesada.

#### 3. `toupeira_pavio` — Toupeira Pavio

- **Silhueta:** toupeira pequena e redonda, focinho grande, capacete torto e pavio visível no topo; mochila-bomba arredondada.
- **Materiais e cores:** pelo carvão `#403A3C`, macacão âmbar, bomba coral, pavio laranja vivo e olhos creme.
- **Acessório e personalidade:** capacete com três riscos de mineração; conta a própria contagem regressiva com sorriso nervoso.
- **Animação:** idle o pavio solta fumaça; telegraph pavio pisca mais rápido e o corpo inclina; ataque corre em linha curta e explode; recuperação deixa o capacete girando.
- **Efeito:** círculo coral no chão, faíscas âmbar e fumaça ciano curta; explosão cartunesca em estrela, sem fogo realista.

#### 4. `caramujo_miudo` — Caramujo Miúdo

- **Silhueta:** caramujo baixo com casco grande em forma de porca, duas antenas e placa frontal; base firme para transmitir peso.
- **Materiais e cores:** casco argila teal escuro, espiral âmbar, corpo creme alaranjado, borda metálica azul-marinho.
- **Acessório e personalidade:** pequena placa escrita apenas com um símbolo de escudo, nunca texto; educado, pede licença antes de bloquear.
- **Animação:** idle antenas em ritmos diferentes; telegraph fecha o casco e acende uma borda amarela; ataque rola devagar e freia com peso; recuperação abre o casco.
- **Efeito:** anel amarelo de redução de dano durante o casco; poeira baixa nas paradas.

#### 5. `cogumelo_po` — Cogumelo de Pó

- **Silhueta:** cogumelo curto, chapéu largo bege com manchas, corpo fino e dois bracinhos; não confundir com item coletável.
- **Materiais e cores:** chapéu creme sujo, manchas roxas suaves, caule verde-musgo, olhos ciano sonolentos.
- **Acessório e personalidade:** uma gravata de folha; trata a luta como teatro e faz reverência.
- **Animação:** idle chapéu inclina para um lado; telegraph infla e treme; ataque solta nuvem circular e se esconde; recuperação reaparece atrás da poeira.
- **Efeito:** partículas creme em espiral e ícones abstratos de poeira; área de redução de cadência marcada por círculos roxos translúcidos.

### Faixa 2 — fases 11–20: Argila e Primeiros Veios

#### 6. `aranha_argila` — Aranha de Argila

- **Silhueta:** oito pernas grossas de barro, corpo oval vermelho-terra, olhos em dois grupos e uma pequena mochila de fios.
- **Materiais e cores:** argila coral escura, juntas âmbar, teia creme com brilho ciano; contorno bem separado entre cada perna.
- **Acessório e personalidade:** carretel no abdômen; perfeccionista, organiza as teias por cor.
- **Animação:** idle pernas alternadas; telegraph marca quadrados no chão; ataque estica fios em grade e fecha; recuperação enrola a teia de volta.
- **Efeito:** grade creme/ciano no chão, linha de impacto coral apenas na faixa que será travada; fios devem ser camada separada.

#### 7. `goblin_picareta` — Goblin Picareta

- **Silhueta:** goblin baixo, cabeça grande, orelhas assimétricas, picareta maior que o corpo e mochila de moedas.
- **Materiais e cores:** pele verde-musgo, roupa roxa escura, metal âmbar, moedas douradas com brilho amarelo.
- **Acessório e personalidade:** capacete com lanterna apagada; malandro, rouba uma moeda e devolve uma parte como gorjeta.
- **Animação:** idle olha para a bolsa; telegraph esconde a picareta atrás do corpo; ataque avança, golpeia e salta; recuperação confere a mochila.
- **Efeito:** moedas fazem arco visual separado; telegraph pequeno brilho dourado no carrinho; impacto deixa duas lascas de rocha.

#### 8. `besouro_pedra` — Besouro Pedra

- **Silhueta:** besouro oval pesado, carapaça facetada, seis patas curtas e olhos quase escondidos; forma deve parecer uma pedra até se mover.
- **Materiais e cores:** rocha cinza-violeta, placas azul-petróleo, juntas coral, brilho branco só nas facetas.
- **Acessório e personalidade:** nenhuma arma; finge ser uma rocha quando é observado.
- **Animação:** idle só as antenas vibram; telegraph encolhe e fecha a carapaça; ataque avança pesado; recuperação reabre os olhos e sacode poeira.
- **Efeito:** carapaça recebe flash amarelo durante defesa; pequenos fragmentos caem ao receber impacto.

#### 9. `xilo_rocha` — Xilo-Rocha

- **Silhueta:** boneco de madeira curto com raízes de pedra nos pés e braços, rosto talhado e ombros largos.
- **Materiais e cores:** madeira marrom quente, pedra cinza, folhas teal, olhos âmbar em forma de sementes.
- **Acessório e personalidade:** galho usado como cajado; fala devagar e dá conselhos que ninguém pediu.
- **Animação:** idle raízes respiram alternadamente; telegraph planta os dois pés; ataque faz raízes crescerem; recuperação recolhe o cajado.
- **Efeito:** setas verdes desenham o caminho das raízes; aliado afetado recebe aura verde curta e legível.

#### 10. `cristal_vivo` — Cristal Vivo

- **Silhueta:** criatura de quatro pernas formada por um cristal central e pequenas lascas orbitais; núcleo deve ser a leitura principal.
- **Materiais e cores:** cristal ciano, bordas teal, base roxa escura, três facetas com amarelo, azul e magenta controlados.
- **Acessório e personalidade:** sem objeto externo; humor muda pela cor do núcleo.
- **Animação:** idle lascas orbitam em velocidades diferentes; telegraph três cores acendem em sequência; ataque pulsa três vezes; recuperação apaga o núcleo e volta ao ciano.
- **Efeito:** anéis prismáticos separados do corpo; terceiro pulso tem uma linha branca fina atravessando escudos.

### Faixa 3 — fases 21–30: Basalto e Fósseis

#### 11. `lagarto_basaltico` — Lagarto Basáltico

- **Silhueta:** lagarto baixo e atlético, cauda de pedra quente, quatro garras largas e cabeça inclinada para frente.
- **Materiais e cores:** basalto quase preto, barriga âmbar escura, rachaduras laranja, olhos ciano estreitos.
- **Acessório e personalidade:** faixa de minério na cauda; corre sem olhar para trás.
- **Animação:** idle cauda contrabalança; telegraph raspa uma garra na parede; ataque sobe, corre pela parede e desce; recuperação abana a poeira.
- **Efeito:** faíscas laranja na parede, rastro diagonal e marca coral no ponto de descida.

#### 12. `aranha_ferro` — Aranha de Ferro

- **Silhueta:** aranha azul metálica com pernas articuladas finas, corpo de bobina e uma garra magnética no abdômen.
- **Materiais e cores:** metal azul-petróleo, ímãs coral e âmbar, olhos creme, detalhes ciano.
- **Acessório e personalidade:** parafusos presos nas pernas; coleciona metal e chama tudo de família.
- **Animação:** idle pernas em pares opostos; telegraph ímã se solta e objetos tremem; ataque puxa um projétil; recuperação trava a garra.
- **Efeito:** linhas magnéticas curvas ciano, parafusos orbitando e pequeno estalo amarelo no fim do puxão.

#### 13. `golem_cascalho` — Golem de Cascalho

- **Silhueta:** montanha pequena de pedras empilhadas, ombro maior de um lado, braço-martelo e dois olhos em rochas claras.
- **Materiais e cores:** cascalho cinza, musgo teal, fissuras âmbar e olhos creme.
- **Acessório e personalidade:** nenhuma armadura; gentil até alguém mexer em sua pilha.
- **Animação:** idle pedras do ombro se acomodam; telegraph junta pedras no braço; ataque ergue e desaba o martelo; recuperação lenta, com peso.
- **Efeito:** onda de choque creme baixa no chão, três pedras saltam e o impacto usa squash moderado.

#### 14. `mineiro_sonolento` — Mineiro Sonolento

- **Silhueta:** mineiro humanoide pequeno, macacão largo, capacete grande e lampião azul; olhos quase fechados.
- **Materiais e cores:** roupa marrom, capacete amarelo gasto, lampião ciano, botas azul-marinho.
- **Acessório e personalidade:** picareta usada como bengala; ataca no sonho e acorda confuso.
- **Animação:** idle cabeça cai e volta; telegraph boceja e o lampião oscila; ataque sopra fumaça e ronca; recuperação ajeita o capacete.
- **Efeito:** letras abstratas em forma de “Z” sem texto legível, fumaça azul suave e área de sono roxa.

#### 15. `eco_azul` — Eco Azul

- **Silhueta:** silhueta humanoide translúcida com bordas quebradas, sem pernas claramente definidas, sombra atrasada.
- **Materiais e cores:** corpo azul ciano translúcido, núcleo branco, sombra roxa deslocada para trás.
- **Acessório e personalidade:** nenhum; não sabe se é cópia ou original.
- **Animação:** idle sombra imita a pose com atraso; telegraph a sombra faz o movimento antes do corpo; ataque repete a ação; recuperação dissolve as bordas.
- **Efeito:** duplicata visual sem colisão, linhas de atraso ciano e um eco roxo no ponto de impacto.

### Faixa 4 — fases 31–40: Fósseis e Musgo

#### 16. `tatu_fossil` — Tatu Fóssil

- **Silhueta:** tatu baixo e largo, casco em placas de osso, focinho de broca e quatro patas robustas.
- **Materiais e cores:** osso creme, fossilização roxa, corpo marrom, luz âmbar nas juntas.
- **Acessório e personalidade:** pequenos fósseis em formato de estrela no casco; contador de histórias antigo.
- **Animação:** idle placas respiram em sequência; telegraph enrola e escuta o chão; ataque investe como broca; recuperação cava uma pequena marca.
- **Efeito:** ossos acendem em cadeia, rastro de poeira e marca de escavação creme no chão.

#### 17. `abelha_ambar` — Abelha de Âmbar

- **Silhueta:** abelha compacta com abdômen de âmbar facetado, asas cristalinas grandes e ferrão curto.
- **Materiais e cores:** amarelo âmbar, listras marrom, asas ciano translúcidas, olhos pretos com brilho creme.
- **Acessório e personalidade:** minúscula ferramenta de operária presa à cintura; trabalha demais e exige elogios.
- **Animação:** idle asas fora de sincronia; telegraph carrega o ferrão; ataque faz ziguezague e volta; recuperação limpa as asas.
- **Efeito:** risco dourado acompanha o trajeto; dois pontos de impacto separados indicam o ataque em sequência.

#### 18. `mumia_argila` — Múmia de Argila

- **Silhueta:** corpo pequeno enrolado em faixas largas, cabeça inclinada, mãos escondidas e pés de barro.
- **Materiais e cores:** argila bege, faixas coral desbotadas, amuletos teal, olhos ciano sonolentos.
- **Acessório e personalidade:** amuleto redondo no peito; só quer terminar a soneca de mil anos.
- **Animação:** idle faixa solta balança; telegraph desenrola uma faixa no chão; ataque puxa e amarra; recuperação enrola o corpo.
- **Efeito:** círculo de faixas creme, área de lentidão roxa e partículas de barro que caem no fim.

#### 19. `bruto_musgo` — Bruto de Musgo

- **Silhueta:** criatura grande de ombros redondos, chifres de raiz, pés largos e barriga coberta de musgo.
- **Materiais e cores:** pedra verde-musgo, raiz marrom, olhos creme, pequenas flores ciano.
- **Acessório e personalidade:** flor presa no chifre; chora quando quebra uma flor, não quando quebra rocha.
- **Animação:** idle respiração pesada; telegraph abaixa a cabeça e pisa forte; ataque corre e freia; recuperação balança o musgo.
- **Efeito:** poeira verde-bege, squash de impacto e anel coral de empurrão visual.

#### 20. `espelho_fossil` — Espelho Fóssil

- **Silhueta:** placa de fóssil flutuante com moldura irregular, um rosto refletido e uma cópia menor no interior.
- **Materiais e cores:** pedra violeta, moldura creme, reflexo ciano e rachaduras âmbar.
- **Acessório e personalidade:** nenhum; repete elogios e reclamações com a mesma expressão.
- **Animação:** idle reflexo pisca atrasado; telegraph reflexo pisca antes do corpo; ataque gera imagem falsa; recuperação racha e recompõe a placa.
- **Efeito:** cópia translúcida em camada separada, fragmentos de fóssil e brilho branco no reflexo verdadeiro.

### Faixa 5 — fases 41–50: Veios e Ferrugem

#### 21. `serpente_cristal` — Serpente Cristalina

- **Silhueta:** serpente formada por segmentos turquesa, cabeça triangular simpática e cauda com ponta de cristal.
- **Materiais e cores:** cristal ciano/teal, barriga creme, olhos âmbar, encaixes roxos.
- **Acessório e personalidade:** nenhum; fica ofendida quando a broca não segue sua trilha.
- **Animação:** idle segmentos ondulam em tempos diferentes; telegraph luz percorre a cauda; ataque espirala e avança; recuperação desenrola lentamente.
- **Efeito:** trilha luminosa separada, segmentos acendem da cauda à cabeça e o impacto deixa um arco ciano.

#### 22. `caranguejo_ferrugem` — Caranguejo de Ferrugem

- **Silhueta:** caranguejo baixo, carapaça quadrada, uma garra maior e pernas metálicas curtas.
- **Materiais e cores:** ferrugem laranja, metal marrom, detalhes teal, olhos creme em hastes.
- **Acessório e personalidade:** etiqueta de fiscal presa à carapaça, sem letras; cobra pedágio de tudo.
- **Animação:** idle garra maior abre e fecha devagar; telegraph garra levanta placas metálicas; ataque fecha em arco; recuperação esconde a garra.
- **Efeito:** moedas douradas drenadas em direção à garra; telegraph coral no recurso, sem texto.

#### 23. `guardiao_veio` — Guardião de Veio

- **Silhueta:** sentinela humanoide robusta com linha de minério atravessando o peito, braços quadrados e pernas de trilho.
- **Materiais e cores:** rocha azul-escura, veio ciano brilhante, metal âmbar e olhos coral.
- **Acessório e personalidade:** placa frontal como escudo; protege uma pedra e esquece por quê.
- **Animação:** idle veio pulsa; telegraph marca uma linha no terreno; ataque divide a faixa; recuperação recompõe o peito.
- **Efeito:** linha brilhante separada do corpo, duas áreas alternadas e estilhaços ciano no fim.

#### 24. `mineiro_capataz` — Mineiro Capataz

- **Silhueta:** mineiro alto e fino, capacete com viseira, apito grande e prancheta angular.
- **Materiais e cores:** uniforme marrom/teal, capacete âmbar, apito coral e luvas creme.
- **Acessório e personalidade:** prancheta com símbolos geométricos, nunca texto; transforma tudo em turno de trabalho.
- **Animação:** idle bate o pé; telegraph apita e aponta para um aliado; ataque acelera o aliado; recuperação anota no ar.
- **Efeito:** carimbo vermelho abstrato no aliado e três linhas de velocidade âmbar.

#### 25. `broto_fossil` — Broto Fóssil

- **Silhueta:** broto pálido que nasce de um osso, duas folhas como braços e base de raízes.
- **Materiais e cores:** osso creme, caule verde pálido, folhas teal, espinhos âmbar.
- **Acessório e personalidade:** pequeno fragmento de fóssil na testa; parece tímido até ocupar a tela.
- **Animação:** idle folhas tremem; telegraph sementes saltam; ataque raiz cresce em arco; recuperação fecha o broto.
- **Efeito:** sementes brancas separadas, barreira de raízes com contorno ciano e poeira baixa.

### Faixa 6 — fases 51–60: Submundo Ferrífero

#### 26. `imp_faisca` — Imp Faísca

- **Silhueta:** imp pequeno com chifres curtos, mochila de faíscas, cauda enrolada e sorriso atrevido.
- **Materiais e cores:** pele coral/laranja, mochila carvão, faíscas amarelas, olhos ciano.
- **Acessório e personalidade:** óculos de proteção levantados na testa; ri antes da própria pegadinha.
- **Animação:** idle mochila pisca; telegraph acende um ponto no alvo; ataque dispara brasa e rola; recuperação sopra a fumaça.
- **Efeito:** projétil laranja com núcleo creme e pequenas brasas; queimadura indicada por três pontos coral.

#### 27. `hiena_cinza` — Hiena Cinza

- **Silhueta:** hiena magra de pernas fortes, orelhas grandes, focinho comprido e cauda de fumaça.
- **Materiais e cores:** cinza carvão, manchas roxas, coleira metálica âmbar, olhos amarelos.
- **Acessório e personalidade:** coleira com uma pequena placa sem texto; leva qualquer perseguição a sério demais.
- **Animação:** idle fareja em dois tempos; telegraph pegadas aparecem; ataque corre em linha e morde; recuperação rosna e volta à postura.
- **Efeito:** rastro de pegadas cinza, marca coral no alvo e leve tremor na coleira.

#### 28. `verme_magma` — Verme de Magma

- **Silhueta:** verme segmentado, cabeça arredondada, placas basálticas e boca circular sem dentes assustadores.
- **Materiais e cores:** vermelho magma, basalto carvão, brilho âmbar e olhos creme.
- **Acessório e personalidade:** sino pequeno preso a uma placa; não consegue ficar parado quando ouve um sino.
- **Animação:** idle segmentos comprimem em sequência; telegraph afunda com bolhas; ataque mergulha e surge; recuperação pinga calor.
- **Efeito:** bolhas laranja atravessam o chão, poça coral com borda amarela e fumaça baixa.

#### 29. `ferreiro_sombrio` — Ferreiro Sombrio

- **Silhueta:** figura baixa de avental grande, braços fortes, martelo azul e máscara com olhos estreitos.
- **Materiais e cores:** avental carvão, pele cinza, martelo azul-ciano, faíscas âmbar.
- **Acessório e personalidade:** cinto de ferramentas; conserta inimigos e cobra histórias.
- **Animação:** idle martelo balança; telegraph aquece a cabeça do martelo; ataque bate no aliado; recuperação sopra a brasa.
- **Efeito:** faíscas azuis, aura de armadura teal no aliado e pequeno som visual de solda em linhas curvas.

#### 30. `escudo_carvao` — Escudo de Carvão

- **Silhueta:** placa viva larga, um olho vermelho, duas pernas minúsculas e uma borda serrilhada.
- **Materiais e cores:** carvão quase preto, borda cinza, olho coral, pequenos rebites âmbar.
- **Acessório e personalidade:** nenhum; fica na frente de tudo, inclusive dos amigos.
- **Animação:** idle olho olha para os lados; telegraph abre a placa e trava no chão; ataque avança como parede; recuperação fecha a borda.
- **Efeito:** faixa preta cresce à frente, projétil bloqueado vira faísca branca e o escudo recebe flash amarelo.

### Faixa 7 — fases 61–70: Cinza e Fendas

#### 31. `corvo_carvao` — Corvo de Carvão

- **Silhueta:** corvo pequeno com asas angulares, bico curto e penas que terminam em brasas.
- **Materiais e cores:** carvão, cinza, brasa coral e olhos ciano.
- **Acessório e personalidade:** uma chave pendurada na pata; guarda segredos e troca por aplausos.
- **Animação:** idle asas não sincronizadas; telegraph penas se abrem em leque; ataque lança três penas; recuperação pousa torto.
- **Efeito:** três rastros vermelhos separados, cada impacto com um brilho âmbar curto.

#### 32. `gato_fenda` — Gato da Fenda

- **Silhueta:** gato roxo baixo, cauda dividida em duas, uma metade do corpo com atraso visual e olhos grandes.
- **Materiais e cores:** roxo profundo, ciano nas bordas, creme no focinho, coral nas patas.
- **Acessório e personalidade:** coleira com uma pequena fenda luminosa; só aparece quando ninguém olha diretamente.
- **Animação:** idle metade do corpo pisca atrasada; telegraph dois pares de olhos surgem; ataque some e troca de posição; recuperação boceja.
- **Efeito:** duplicata roxa sem colisão, rasgo ciano curto e indicação de prioridade trocada.

#### 33. `mago_fumaca` — Mago de Fumaça

- **Silhueta:** mago pequeno com chapéu triangular enorme, bastão fino e túnica que termina em fumaça.
- **Materiais e cores:** túnica cinza-fumaça, chapéu roxo, runas ciano, fivela âmbar.
- **Acessório e personalidade:** chapéu com uma pena chamuscada; explica cada feitiço com palavras demais.
- **Animação:** idle fumaça sai em duas direções; telegraph ergue o bastão e desenha; ataque envolve um aliado; recuperação recolhe o chapéu.
- **Efeito:** runas cinza em espiral, aura de ocultação roxa e um brilho ciano mostrando o aliado verdadeiro.

#### 34. `cavaleiro_obsidiana` — Cavaleiro de Obsidiana

- **Silhueta:** cavaleiro curto e largo, escudo angular, espada curta e visor fechado.
- **Materiais e cores:** obsidiana preta, bordas roxas, visor coral, rebites âmbar.
- **Acessório e personalidade:** capa curta de poeira; segue regras que inventou.
- **Animação:** idle escudo pesa para um lado; telegraph abaixa o visor; ataque carrega com escudo à frente; recuperação faz uma saudação rígida.
- **Efeito:** cone frontal coral, faísca no contra-ataque e brilho roxo no visor durante a defesa.

#### 35. `broca_fantasma` — Broca Fantasma

- **Silhueta:** mini broca translúcida com corpo arredondado, ponta cônica, esteiras fantasma e olhos ciano.
- **Materiais e cores:** azul espectral, ciano, violeta e branco translúcido; não copiar exatamente a protagonista.
- **Acessório e personalidade:** uma pequena sombra atrasada; admira a protagonista e tenta imitá-la.
- **Animação:** idle ponta gira em baixa velocidade; telegraph rastro aparece ao contrário; ataque atravessa e retorna; recuperação perde opacidade.
- **Efeito:** perfuração reversa ciano, trilha roxa e cópia reduzida do padrão básico do jogador.

### Faixa 8 — fases 71–80: Céu Subterrâneo

#### 36. `larva_estrela` — Larva Estelar

- **Silhueta:** larva flutuante arredondada, pequenos pés de luz, antenas como estrelas e barriga segmentada.
- **Materiais e cores:** azul noite, pontos ciano, barriga violeta e olhos creme.
- **Acessório e personalidade:** constelação de três pontos no dorso; acha que cada pedra é um planeta.
- **Animação:** idle flutua alternando altura; telegraph estrelas ligam o caminho; ataque orbita e cai; recuperação salta uma vez.
- **Efeito:** pontos estelares conectados, arco azul e pequeno impacto em estrela de quatro pontas.

#### 37. `sentinela_pulsar` — Sentinela Pulsar

- **Silhueta:** torre baixa com três pernas mecânicas, núcleo ciano central e placas que abrem como pétalas.
- **Materiais e cores:** metal azul-petróleo, núcleo ciano, bordas âmbar e visor coral.
- **Acessório e personalidade:** contador de impactos representado por três luzes, sem números; fica mais animada com combos.
- **Animação:** idle núcleo pulsa em ritmo lento; telegraph pétalas abrem e carregam; ataque pulsa três vezes; recuperação fecha as placas.
- **Efeito:** círculos ciano crescentes; terceiro pulso tem linha branca e mais contraste.

#### 38. `aracnideo_escuro` — Aracnídeo Escuro

- **Silhueta:** aranha violeta alongada, oito pernas que desaparecem nas pontas e corpo com máscara lisa.
- **Materiais e cores:** violeta escuro, roxo, ciano mínimo nas juntas e olhos creme.
- **Acessório e personalidade:** nenhum; muda os móveis de lugar e culpa a dimensão.
- **Animação:** idle pernas somem e voltam; telegraph linhas roxas são desenhadas e apagadas; ataque prende e salta; recuperação reaparece em outra posição.
- **Efeito:** teia sem chão em roxo translúcido, duas posições falsas e contorno ciano indicando a real.

#### 39. `cartografo_perdido` — Cartógrafo Perdido

- **Silhueta:** mineiro dividido em três posições, cada uma com mochila e mapa flutuante; a posição real tem sombra mais forte.
- **Materiais e cores:** uniforme azul, mapas creme, tinta roxa e lampião ciano.
- **Acessório e personalidade:** três mapas orbitais; está sempre certo sobre o caminho errado.
- **Animação:** idle cópias atrasam movimentos; telegraph mapa dobra; ataque troca prioridade e posição; recuperação junta os três corpos.
- **Efeito:** mapa holográfico cobre o fundo apenas dentro de uma área, setas roxas e sombra ciano no corpo verdadeiro.

#### 40. `rei_miniatura` — Rei em Miniatura

- **Silhueta:** rei pequeno com coroa grande, capa de poeira, martelo curto e barriga arredondada.
- **Materiais e cores:** coroa âmbar, capa roxa, armadura de pedra, olhos creme e selo ciano.
- **Acessório e personalidade:** cetro pequeno demais; exige reverência mesmo quando ninguém o reconhece.
- **Animação:** idle coroa balança; telegraph ergue o cetro; ataque bate o decreto no chão; recuperação descansa sobre o martelo.
- **Efeito:** selo dourado no chão, onda curta coral e um brilho de resistência usado apenas uma vez por fase.

### Faixa 9 — fases 81–90: Dimensão Fraturada

#### 41. `fragmento_vivo` — Fragmento Vivo

- **Silhueta:** lasca de cristal com duas pernas de luz, uma ponta maior e pequenas cópias orbitando.
- **Materiais e cores:** cristal ciano, bordas brancas, sombra roxa e reflexo âmbar.
- **Acessório e personalidade:** nenhum; imita a última emoção que viu.
- **Animação:** idle cópias orbitam fora de sincronia; telegraph divide em duas formas; ataque toca e reagrupa; recuperação apaga a cópia falsa.
- **Efeito:** duas cópias sem colisão, uma mais opaca; impacto ciano com fragmentos creme.

#### 42. `eco_nulo` — Eco Nulo

- **Silhueta:** silhueta escura sem boca, olhos ciano separados e braços longos que terminam em ruído de pixels suaves.
- **Materiais e cores:** corpo preto-violeta, olhos ciano, bordas roxas e núcleo branco pequeno.
- **Acessório e personalidade:** nenhum; repete as frases da broca com atraso irritante, sem texto visível no sprite.
- **Animação:** idle cabeça move depois do corpo; telegraph escuta; ataque repete o impacto; recuperação silencia e encolhe.
- **Efeito:** som visual aparece antes do clarão, anel roxo atrasado e repetição parcial do último efeito.

#### 43. `dobra_lagarto` — Lagarto Dobra

- **Silhueta:** lagarto estreito com duas caudas em tempos diferentes, cabeça duplicada por uma borda deslocada.
- **Materiais e cores:** verde petróleo, violeta, olhos ciano e barriga creme fragmentada.
- **Acessório e personalidade:** nenhum; chega antes de sair.
- **Animação:** idle caudas com atraso; telegraph duas sombras se alinham; ataque morde no primeiro tempo e desfaz no segundo; recuperação fecha a dobra.
- **Efeito:** duas mordidas desenhadas em pontos distintos, segunda com transparência menor e cor roxa.

#### 44. `olho_violeta` — Olho Violeta

- **Silhueta:** olho flutuante grande dentro de um aro metálico, três pequenas peças orbitais e sem corpo humanoide.
- **Materiais e cores:** íris violeta, esclera creme, aro carvão, rebites âmbar e brilho ciano mínimo.
- **Acessório e personalidade:** nenhum; observa tudo e julga a pontaria.
- **Animação:** idle íris segue a broca; telegraph abre muito e foca; ataque fecha o olho em cone; recuperação pisca duas vezes.
- **Efeito:** cone violeta no chão, borda coral na área de inversão e reflexo branco na íris.

#### 45. `sombra_portal` — Sombra do Portal

- **Silhueta:** silhueta humanoide recortada por um aro de portal ciano, mãos e pés levemente desalinhados.
- **Materiais e cores:** interior preto-violeta, borda ciano, faíscas magenta e olhos creme quase invisíveis.
- **Acessório e personalidade:** nenhum; quer ser protagonista, mas só aparece no fundo.
- **Animação:** idle portal gira devagar; telegraph abre uma porta; ataque troca de lugar e golpeia; recuperação fecha o aro.
- **Efeito:** portal é uma camada separada, com pedaços de caverna girando ao contrário; troca protegida por brilho ciano.

### Faixa 10 — fases 91–100: Herdeiros da Fenda

#### 46. `guardiao_invertido` — Guardião Invertido

- **Silhueta:** robô de cabeça para baixo, pernas apontando para cima, braços pesados e núcleo vermelho no peito invertido.
- **Materiais e cores:** metal azul-petróleo, núcleo coral, juntas âmbar e brilho roxo nas bordas.
- **Acessório e personalidade:** placa de identificação sem letras; faz tudo ao contrário e acha isso estratégia.
- **Animação:** idle partículas sobem; telegraph trava e se desprende do chão; ataque cai para cima; recuperação retorna com giro lento.
- **Efeito:** pedras sobem, sombra fica acima do personagem e barreira coral que exige impacto especial.

#### 47. `cometa_devora` — Cometa Devorador

- **Silhueta:** cometa pequeno, corpo arredondado flamejante, boca cristalina grande e cauda longa em arco.
- **Materiais e cores:** núcleo laranja, bordas âmbar, boca ciano e cauda roxa.
- **Acessório e personalidade:** nenhum; come partículas e pede sobremesa.
- **Animação:** idle cauda ondula; telegraph aponta e comprime; ataque mergulha em curva; recuperação cospe uma faísca.
- **Efeito:** arco laranja forte, partículas positivas sendo puxadas para a boca e rastro roxo curto.

#### 48. `relogio_quebrado` — Relógio Quebrado

- **Silhueta:** relógio flutuante sem ponteiros, corpo redondo rachado, quatro engrenagens fora do encaixe e duas perninhas.
- **Materiais e cores:** metal carvão, borda âmbar, rachaduras ciano e interior violeta.
- **Acessório e personalidade:** ponteiro solto orbitando; vive cinco segundos atrasado e acha elegante.
- **Animação:** idle engrenagens em ritmos diferentes; telegraph tudo para por um instante; ataque salta no tempo; recuperação recoloca uma engrenagem.
- **Efeito:** círculos temporais no chão, ponteiro fantasma e deslocamento visual curto, sem congelar a interface inteira.

#### 49. `duplo_impossivel` — Duplo Impossível

- **Silhueta:** duas metades de uma criatura arredondada, cada uma com um olho, separadas por um espaço que não mostra fundo.
- **Materiais e cores:** metade âmbar/teal e metade roxa/ciano, bordas espelhadas e olhos creme.
- **Acessório e personalidade:** nenhuma; discute consigo sobre quem atacou primeiro.
- **Animação:** idle metades respiram fora de sincronia; telegraph cruzam em X; ataque impacta simultaneamente; recuperação funde as bordas.
- **Efeito:** X de linhas ciano e coral; em cada batida uma metade recebe o flash e a outra fica opaca.

#### 50. `herdeiro_fenda` — Herdeiro da Fenda

- **Silhueta:** príncipe pequeno feito de luz roxa, capa curta, coroa de três portais e corpo com parafusos flutuantes.
- **Materiais e cores:** luz violeta, coroa ciano, parafusos âmbar, olhos creme e sombra azul-petróleo.
- **Acessório e personalidade:** três miniportais acima da cabeça; acredita que a broca deve herdar a dimensão.
- **Animação:** idle portais giram em ritmos distintos; telegraph escolhe um alvo e abre coroas; ataque fortalece o inimigo fraco e troca posições; recuperação fecha dois portais, deixando um ativo.
- **Efeito:** três coroas ciano no alto dos alvos, linhas roxas de troca e aura âmbar no inimigo fortalecido.

## Entrega recomendada para a IA de arte

Para cada inimigo, gerar primeiro uma prancha de exploração com quatro poses: idle, telegraph, ataque e derrota. Depois fechar o sprite principal com fundo transparente e separar as camadas `Body`, `HeadOrFace`, `Accessory`, `WeaponOrAttack`, `HitFlashMask` e `Shadow`.

Usar o mesmo contorno, escala de base, direção para a direita e tratamento de olhos em toda a coleção. A personalidade deve aparecer na pose e no rosto, não em texto. A cor do telegraph deve ser reproduzível em animação: coral para dano, amarelo para preparação, ciano para informação verdadeira e roxo para distorção.

## Ordem de produção

1. `slime_terra`, `morcego_cascalho_ii`, `toupeira_pavio`, `caramujo_miudo` e `cogumelo_po` — validar a linguagem da primeira região.
2. `aranha_argila`, `goblin_picareta`, `besouro_pedra`, `xilo_rocha` e `cristal_vivo` — validar controle, tanque, suporte e elite.
3. Um inimigo de cada função das faixas 3–5 — testar o loop de telegraph e pool.
4. Submundo, Dimensão Fraturada e faixa 10 — só depois que a silhueta e a legibilidade dos primeiros dez estiverem aprovadas.

## Limites de implementação

Este documento é um briefing visual e não afirma que os sprites ou ataques já existem no Unity. O catálogo de fases e IDs está em [`content/enemies/campaign-tiers.json`](../content/enemies/campaign-tiers.json). A integração futura deve manter a raiz lógica separada do nó visual, usar pools para projéteis e partículas e não alterar a posição funcional durante animações.
