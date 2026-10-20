# Dig & Wonder — direção de arte

**Estado: direção aprovada pelo usuário em 20/10/2026; primeira integração em avaliação visual.**

Este documento orienta a próxima revisão artística. Não declara os desenhos atuais como arte final. O usuário autorizou continuar e depois refinou o estilo para fundos pintados, suaves, sem contorno e sem reflexo plástico.

## 1. Conceito e decisões principais

**Uma companheira de latão abre caminho por uma terra que guarda coisas inesperadas.**

A broca é o centro emocional e visual da experiência: uma máquina robusta, curiosa e um pouco teimosa, com um rosto expressivo integrado à cabine. O jogador deve reconhecer sua silhueta mesmo em miniatura. A mina transmite descoberta, calor e mistério; a interface parece equipamento dessa mesma expedição.

Decisões propostas:

1. **Azul-petróleo com destaques quentes.** Preservar a paleta existente; usar luz âmbar para aquecer superfícies e coral para ações principais.
2. **Fundos ilustrados em três planos.** Silhuetas distantes, estruturas intermediárias e moldura próxima, com parallax discreto ligado à descida. A qualidade vem da composição, das formas e da luz; o número de planos não substitui acabamento.
3. **Broca grande como mascote.** Lia e Bento saem da composição principal. Seus arquivos e identidades permanecem preservados para uso posterior. Sua reintrodução visual depende de uma decisão futura.

### Paleta completa

As sete cores principais mantêm os valores de `IDENTIDADE-VISUAL.md`. As demais são cores de apoio propostas para luz, sombra e materiais, sem substituir as cores já definidas nos catálogos do jogo.

| Cor | Hex | Uso |
| --- | --- | --- |
| Azul-petróleo | `#123448` | Fundo e identidade geral da mina |
| Tinta escura | `#17303D` | Contorno, juntas, texto em superfícies claras |
| Creme | `#FFF4D8` | Texto claro, placas de informação e luz refletida |
| Turquesa | `#39D4C1` | Energia, cristais e acentos da broca |
| Latão | `#F2B84B` | Metal, botões secundários e recompensas reais |
| Coral | `#F27C67` | Ações principais, avisos de perigo com símbolo e texto |
| Menta luminosa | `#B7FFF1` | Reflexos cristalinos e foco de controles |
| Azul de abismo | `#0B202D` | Sombras profundas e plano distante |
| Cobre escurecido | `#9D603D` | Sombra de latão, tubos e ferragens |
| Terra quente | `#704A38` | Raízes e fundo das camadas rasas |
| Luz de entardecer | `#E7A06A` | Céu e luz ambiente da superfície |
| Aço fosco | `#617A80` | Ponta, suportes e controles indisponíveis |
| Violeta Aurora | `#7966B2` | Apoio do ambiente especial existente |

Coral de ação e coral de perigo precisam de contextos distintos: perigo sempre acompanhado de sinal, nome ou forma própria. Não depender somente de cor para comunicar estado, material ou raridade.

### Acabamento da broca

- Corpo principal arredondado, centro de gravidade baixo e ponta grande voltada para baixo. A ponta deve parecer presa ao motor por um eixo e um suporte capazes de sustentar a peça.
- Cabine, carenagem, eixo, rodas/esteiras e ferramentas formam uma máquina integrada. Evitar peças soltas ou um rosto colado sobre uma caixa.
- Luz principal quente vindo de cima e da esquerda; luz secundária turquesa vindo do núcleo e dos cristais próximos. Essa lógica vale para máquina, terreno e interface.
- Usar massas de cor base e sombra bem desenhadas, sombra de contato nas junções, oclusão sob a cabine e pequenos reflexos nas quinas de metal. Degradês sutis são permitidos como apoio; não devem substituir o desenho dos volumes.
- Contorno com referência de aproximadamente 3 px no viewport de projeto, ajustado à escala. Linhas internas mais leves. Evitar a mesma espessura em cada parafuso, dobra e borda.
- Expressão por olhos, pálpebras, inclinação e reação da cabine: curiosidade em idle, esforço ao perfurar, surpresa numa descoberta. O movimento deve preservar a sensação de peso.
- Os seis modelos existentes devem ter diferenças de silhueta e equipamento coerentes com seus padrões já implementados: ponta central da Pioneira, conjunto triplo da Trinca, disposição em leque da Prisma, bobinas da Dínamo, emissores da Ecos e aletas/núcleo da Aurora. Nenhuma habilidade nova é criada por essa diferenciação artística.

**Critério de aprovação da arte:** broca reconhecível a 25% do tamanho de trabalho; encaixes convincentes; luz consistente; face legível; modelos distinguíveis sem ler o nome. SVG é uma opção de fonte, não um critério de qualidade.

## 2. Cinco ambientes de profundidade

São tratamentos visuais da descida e das regiões existentes, não novas regiões ou mudanças no conteúdo. Exceto a superfície de 0–40 m solicitada, as faixas abaixo são referências artísticas propostas. Seus limites finais precisam ser conciliados com a sequência atual de fases antes de implementar. A identidade de uma região existente tem precedência sobre uma simples troca de cor por metragem.

| Ambiente | Paleta e luz | Plano distante | Plano intermediário | Plano próximo | Clima |
| --- | --- | --- | --- | --- | --- |
| **Acampamento ao entardecer — 0–40 m** | Azul-petróleo, laranja suave, latão e creme. Luz quente superior esquerda. | Céu em degradê e montanhas com silhuetas suaves. | Acampamento ilustrado: tenda, guincho e iluminação de oficina, sem funções ou botões novos. | Travessas da entrada da mina, terra cortada e raízes no topo. | Poeira dourada esparsa. A grade continua subterrânea; o céu ocupa a abertura e a área acima dela. |
| **Terra das raízes — referência 40–96 m** | Terra quente, cobre, creme e sombra azul. | Massas de terra escuras e cavidades largas. | Raízes sinuosas, pedras incrustadas e estratos em curvas. | Fragmentos de rocha e raízes nas laterais, fora da leitura dos alvos. | Poeira fina; últimos reflexos quentes da superfície. |
| **Veios de cobre — referência 96–192 m** | Azul-petróleo, cobre e turquesa. | Caverna azul com formas grandes e poucos detalhes. | Veios de cobre atravessando a rocha; pequenos conjuntos de cristal. | Lajes fraturadas e cristais laterais com sombra de contato. | Névoa turquesa leve; cristais iluminam pequenas áreas próximas. |
| **Abismo luminoso — referência acima de 192 m** | Azul de abismo, petróleo, menta e latão pontual. | Espaço escuro com névoa em massas largas. | Cristais grandes, arcos de rocha e cavidades que sugerem escala. | Bordas de rocha quase em silhueta, com reflexos da ferramenta. | A luz da broca ganha importância. A escuridão nunca reduz a leitura de blocos, avisos ou controles. |
| **Fenda Aurora — quando a região especial existente estiver ativa** | Turquesa, menta e violeta; coral e latão permanecem na interface. | Luz em faixas curvas e profundas. | Formações minerais assimétricas e cristais facetados mais longos. | Arcos de pedra e reflexos saturados em áreas pequenas. | Névoa colorida lenta e poucos pontos luminosos. A surpresa vem da composição e da luz, sem novos materiais funcionais. |

### Movimento e transições

- Grade e áreas de toque permanecem estáveis. Apenas os fundos recebem parallax.
- Referência de deslocamento visual: distante 0,08×, intermediário 0,18× e próximo 0,30× do deslocamento de câmera, com limite e reaproveitamento dos planos. Ajustar após observar a descida real; evitar deslizamento que faça o terreno parecer solto.
- Transição de paleta e névoa em **1,5 s**, com easing `sine in/out`. Usar mistura entre os dois tratamentos; não trocar de imagem ou clima num corte seco.
- Ao atravessar rapidamente várias faixas, substituir o destino da transição em andamento, sem acumular efeitos.
- Na superfície, montanhas e céu são silenciosos; no abismo, poucos pontos de luz. O fundo não compete com blocos e broca.
- Não exigir novas fontes de luz por bloco. Preferir iluminação desenhada nos ativos e camadas discretas de névoa. Leveza é uma intenção a medir, não uma garantia de FPS em Android.

### Direção dos blocos

O bloco é um recorte de uma formação mineral, não uma ficha colorida repetida. A composição deve unir fragmentação, luz, material e variação sem esconder os limites tocáveis.

| Material existente | Linguagem artística |
| --- | --- |
| Terra macia | Torrões de tamanhos diferentes, grãos discretos, bordas gastas e pequenas inclusões. Luz difusa, sem brilho metálico. |
| Argila | Estratos em curvas comprimidas, pequenas fissuras e áreas mais lisas. Evitar desenhar a mesma curva central em todas as células. |
| Pedra e rochas das demais regiões | Facetas largas e irregulares, fraturas com direção, sombra nas frestas e bordas de contato. As variantes aproveitam a identidade de cada região já existente. |
| Veios e minérios existentes | Veios embutidos na rocha, com espessura variável e reflexo restrito ao mineral. O minério precisa parecer parte da formação. |
| Cristais existentes | Agrupamentos com tamanhos distintos, facetas claras e escuras, base presa à rocha e brilho localizado. Não espalhar o mesmo losango por toda a grade. |

Preparar poucas variantes artísticas por família existente, escolhidas por coordenada ou hash visual independente. Não consumir o RNG do mundo/recompensa. Preservar célula, dureza, danos, recompensas e probabilidades. Fissuras de dano refletem o estado real; seleção, perigo e marcadores continuam legíveis por cima da arte.

## 3. Interface e botões com personalidade

### Hierarquia da tela

1. **Área de jogo:** maior massa visual, com broca e terreno como protagonistas.
2. **Informação imediata:** profundidade, objetivo e recursos em um cabeçalho compacto.
3. **Ações:** vender e melhorar com nome, custo/valor real e estado compreensível.
4. **Telas existentes:** baús, equipamentos, personagem e álbum com acabamento da mesma oficina. Melhorias são apresentadas em cartões com ilustração, raridade e situação real; não acrescentar níveis, compras ou sistemas inexistentes.

Lia e Bento não ocupam a faixa da broca. Remover sua presença da tela na próxima implementação, preservando arquivos e rigs. O espaço recuperado deve beneficiar o jogo, sem virar uma faixa vazia de decoração.

### Construção dos botões

- **Placas de metal com rebites**, cantos chanfrados ou bordas dobradas, espessura aparente, sombra curta e reflexo superior coerente com a luz da cena.
- Ação principal: esmalte **coral** sobre a placa, texto em tinta escura e pequeno brilho quente. Aplicar a vender/melhorar e à ação de escavar somente onde esse controle já existir. Não criar um botão de escavar além do toque atual na grade.
- Secundários: **latão** com texto escuro. Navegação e voltar têm menor peso visual, mantendo leitura e área de toque.
- Indicadores de recursos: pequenos mostradores de instrumento, sem parecer botões quando não forem interativos.
- Cartões de equipamento: moldura metálica e encaixe próprio para o retrato do item. Estado equipado usa marca e texto; indisponível usa cadeado e explicação.
- Baú: objeto ilustrado com tampa, dobradiças, fecho e materiais reconhecíveis. A abertura deve revelar o item que o sistema realmente sorteou.

| Estado | Aparência | Movimento |
| --- | --- | --- |
| Normal | Metal e esmalte com volume; rebites discretos. | Nenhum movimento obrigatório. |
| Pressionado | Placa afundada, sombra menor, reflexo reduzido. | Escala 1 → 0,97 em 70 ms; retorno em 110 ms, `quad out`. A ação responde imediatamente. |
| Desabilitado | Aço acinzentado, baixo brilho, texto ainda legível e razão visível quando necessário. | Sem pulso nem resposta visual de ação bem-sucedida. Manter os avisos funcionais já existentes. |
| Destaque | Filete claro e brilho localizado na ação disponível mais relevante. | Pulso de brilho em 2,4 s, `sine in/out`; sem piscar toda a placa. |
| Foco | Contorno menta externo e contínuo, distinguível do destaque. | Estático. A implementação precisa conectar o estado ao foco real, não apenas guardar um estilo sem uso. |

### Tamanho e área segura

**Meta de usabilidade: mínimo de 48 dp em Android.** A base atual usa viewport lógico de 540 × 960 com ajuste de área segura e escala uniforme. Unidades do Godot não equivalem automaticamente a dp.

Projetar controles com pelo menos 64 × 64 unidades lógicas e separação visível, ampliando a área interativa quando necessário. Verificar em aparelho o tamanho efetivo depois de stretch, densidade e área segura; ajustar para atingir 48 dp reais. Não declarar esse requisito como validado antes dessa medição. Não reduzir texto para compensar uma composição que não cabe.

## 4. Sistema de ícones

**Vocabulário:** picareta, moeda, mochila, bússola, baú, chave, broca, equipamento, álbum e cadeado. Usar cada ícone somente em funções existentes. A bússola pode acompanhar a informação de expedição; não cria navegação ou recurso novo.

- Mesma família de desenho: volumes compactos, cantos suavizados e espessura óptica de contorno equivalente.
- Referência: contorno de aproximadamente 3 px quando o ícone estiver a 48 px na composição; ajustar detalhes internos para sobreviver à redução.
- Uma cor base, uma sombra e reflexo pequeno. Moeda parece metal; mochila parece tecido; cristal parece translúcido. Não aplicar o mesmo brilho plástico a todos os objetos.
- Luz superior esquerda, sombra curta inferior direita. Não usar sombra preta borrada que suje os tamanhos pequenos.
- Reconhecíveis em 24–32 px; nenhuma informação importante depende de detalhes abaixo disso. Ícone pequeno pode ter uma versão simplificada própria.
- Ícone acompanha texto e estado; não substituir nomes necessários por símbolos ambíguos ou emojis de aparência variável.

## 5. Animações e resposta visual

Animação valoriza o desenho pronto. Primeiro aprovar proporção, cor, luz e encaixes; depois ajustar a intensidade do movimento.

| Elemento / evento real | Movimento | Duração e easing | Limite |
| --- | --- | --- | --- |
| Broca em idle | Respiração mecânica discreta, cabine com peso, olhos atentos. | Ciclo de 2,8 s, `sine in/out`, amplitude de 1–1,5%. | Somente pivôs visuais. |
| Piscada | Pálpebras fecham e abrem; expressão ocasional de curiosidade. | 80 ms para fechar, 120 ms para abrir; intervalos de 3–6 s. | Temporização visual independente do RNG da partida. |
| Perfuração efetiva | Ponta gira/cicla, suporte vibra e corpo reage com inércia. | Ciclo da ponta de 120–180 ms; reação do corpo de 160–220 ms. | Começa com dano real; célula vazia não simula trabalho. |
| Quebra de bloco | Fragmentos do material, pequena compressão e expansão da broca. | 180–260 ms, `quad out`. | Reaproveitar partículas existentes; máximo atual de 26 partículas e descarte por vida útil. |
| Impacto ao escavar | Tremor curto na camada visual do mundo. | 80–120 ms, decaimento até zero; referência de 1–2 px lógicos. | HUD e cálculo de toque permanecem estáveis; não somar tremores sem limite. |
| Venda confirmada | Número sobe próximo à ação e se dissipa. | 650–850 ms, `cubic out`, deslocamento de 24–36 unidades. | Mostrar a diferença real de moedas, incluindo eventual bônus já existente. Nunca inventar recompensa. |
| Melhoria confirmada | Breve luz no núcleo e assentamento da nova peça; nome do resultado. | 450–650 ms, `quad out`. | Somente após sucesso; não atrasar comando, cobrança ou save. |
| Mudança de ambiente | Mistura de fundo e névoa. | 1,5 s, `sine in/out`. | Uma transição ativa, destino substituível. |
| Botão tocado | Placa afunda e retorna com reflexo curto. | 70 + 110 ms, `quad out`. | Mesmo retângulo funcional e resposta imediata. |
| Abertura de painel existente | Painel sobe ligeiramente e ganha opacidade. | 180–240 ms, `cubic out`. | Não bloquear toques nem criar novos menus. |
| Baú aberto | Fecho solta, tampa abre e item sorteado aparece. | 450–650 ms, `cubic out`. | Recompensa continua sendo concedida pelo fluxo atual. |
| Baú de raridade alta | Flash localizado e confete discreto nas cores da raridade. | Flash de 100–160 ms; confete de 600–900 ms. | Proposta: Lendária, Única e Mística; até 16 fragmentos, sem flash de tela inteira. |
| Lia/Bento, uso futuro | Respiração e piscada no retrato/rig preservado. | Mesma linguagem temporal da broca. | Não renderizar na tela principal nesta revisão. |

Pausar ao perder foco. Reutilizar ou substituir o Tween de uma ação em vez de criar filas ilimitadas. Descartar fragmentos, flashes e números ao terminar. Não acrescentar paralelamente outro sistema de animação sem necessidade.

## 6. Personalidade e falas

Lia observa e se encanta; Bento é prático e gosta de máquinas. A broca comunica emoção pelo próprio desenho, sem ocupar a tela com explicações constantes.

**Nesta revisão, nenhum mineiro aparece fisicamente na tela.** Se as falas forem aprovadas, usar uma pequena placa de rádio com o nome do emissor, sem retrato, nos momentos já existentes de descoberta ou mudança de camada. Não criar um sistema de ajudantes, missão ou tutorial novo.

| Emissor | Momento | Exemplo de texto proposto |
| --- | --- | --- |
| Lia | Primeiro cristal relevante encontrado na camada | “Olha esse brilho. Tem mais coisa aqui.” |
| Bento | Transição para rocha mais profunda | “Rocha nova. Vamos com calma.” |
| Lia | Entrada na Fenda Aurora existente | “Isso não parece uma caverna comum...” |
| Bento | Transformação de broca concluída | “Agora sim. Encaixe perfeito!” |

Placa com até duas linhas, referência de **18–20 unidades lógicas** para a fala e **12–14** para o nome, com contraste creme/petróleo. Exibição por 2,5–3 s, aparecimento de 180 ms e saída de 200 ms. Posicionar fora de controles e alvos; no máximo uma fala por vez e intervalo mínimo de 30 s. Avisos funcionais têm prioridade.

Esses textos são propostas editoriais. O projeto já possui avisos/toasts; não foi estabelecido nesta direção que exista um sistema completo de diálogos de Lia e Bento. Antes de implementar, conferir os eventos disponíveis e aproveitar o canal existente. Não adicionar falas repetidas a cada bloco ou recompensa.

## 7. O que não fazer

- Não considerar uma ilustração finalizada apenas porque está em SVG, tem contorno ou usa mais detalhes.
- Não apresentar placeholders e capturas de conceito como prova de arte implementada ou acabamento final.
- Não montar a broca como peças desconectadas, com escalas incompatíveis, sombra sem origem ou rosto sobreposto sem integração.
- Não repetir a mesma forma geométrica com outra cor para representar todos os materiais, itens ou botões.
- Não transformar toda a interface em placas iguais. Ação, navegação, recurso e coleção precisam ter hierarquia e construção próprias dentro da mesma oficina.
- Não copiar personagens, marcas, logotipos, recursos exclusivos ou telas das referências. Adaptar princípios de composição, clareza e acabamento.
- Não usar pixel art, 3D, realismo fotográfico, brilhos excessivos ou texturas ruidosas.
- Não usar contornos pesados em todos os detalhes internos; não reduzir o jogo a uma grade de molduras.
- Não esconder a grade com personagens, névoa, confete, textos ou efeitos de impacto.
- Não simular escavação automática, dano, recompensa, compra ou evolução inexistente só para parecer mais dinâmico.
- Não modificar regiões, inimigos, habilidades, menus, dureza, velocidade, custos, probabilidades, save ou anúncios.
- Não apagar Lia e Bento ou seus ativos; sua retirada é da composição visível.
- Não declarar 60 FPS, 48 dp efetivos, desempenho, consumo ou funcionamento Android como verificados sem teste correspondente em aparelho.

## 8. Relação com a identidade existente e aprovação

Esta proposta detalha o cartoon 2D de aparência vetorial de `docs/IDENTIDADE-VISUAL.md`: preserva a paleta principal e os conceitos de exploração, ferramentas artesanais e companheira curiosa. Resolve a falta de acabamento exigindo materiais, composição, silhueta e iluminação coerentes. Aparência vetorial significa clareza de formas e bordas; não limita os ativos finais a SVG.

A principal mudança de composição proposta é a broca assumir sozinha o protagonismo visual, com Lia e Bento preservados fora da tela. Botões principais passam a coral; latão organiza as ações secundárias e os materiais da oficina. Os ambientes em profundidade são cenografia para conteúdo existente.

### Critérios para a implementação posterior

1. Aprovar esta direção antes de produzir novos assets ou alterar cenas/scripts.
2. Começar pelo acabamento de uma broca, uma composição de terreno com materiais existentes e um conjunto pequeno de controles em tamanho real. Avaliar o conjunto, não os formatos dos arquivos.
3. Conferir a hierarquia em 540 × 960 e em outra proporção suportada, preservando área segura, texto e toque.
4. Produzir capturas reais do Godot antes/depois e sequência de animação; identificar como execução desktop em viewport de celular quando for esse o caso.
5. Validar regressões de gameplay/save e limites de efeitos. Desempenho Android fica pendente até medição em aparelho.
6. Atualizar identidade e proveniência junto da implementação aprovada, sem classificar referências como ativos reutilizáveis.

**Execução autorizada:** integração e limites registrados no README, seção Direção de arte aprovada. O acabamento segue em avaliação pelas capturas reais; não presumir aprovação artística final.
