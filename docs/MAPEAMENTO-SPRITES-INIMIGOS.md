# Correspondência das folhas de inimigos recebidas

Inspeção visual dos 16 arquivos em 20/10/2026. Origem preservada: `C:/Users/otaau/.codex/.chatgpt-projects/Dig & Wonder/assets/inimigos`.

Correspondência baseada em `BRIEF-SPRITES-INIMIGOS.md`, `content/enemies/campaign-tiers.json` e características vistas nas próprias imagens, não apenas no nome abreviado do arquivo.

| Arquivo recebido | ID do catálogo | Nome | Identificação visual |
| --- | --- | --- | --- |
| lama.jpeg | slime_terra | Gosma de Terra | Argila, folha e pedrinhas; salto e achatamento |
| morcego.jpeg | morcego_cascalho_ii | Morcego Cascudo | Corpo rochoso, asas com marcas ciano e porca na orelha |
| toupeira.jpeg | toupeira_pavio | Toupeira Pavio | Capacete, macacão âmbar, bomba e pavio |
| lesma.jpeg | caramujo_miudo | Caramujo Miúdo | Casco em porca verde, espiral âmbar e escudo |
| cogumelo.jpeg | cogumelo_po | Cogumelo de Pó | Chapéu creme manchado, corpo verde e gravata de folha |
| aranha.jpeg | aranha_argila | Aranha de Argila | Corpo terracota, carretel e teia em grade |
| goblin.jpeg | goblin_picareta | Goblin Picareta | Picareta, capacete e mochila de moedas |
| aranha-pedra.jpeg | besouro_pedra | Besouro Pedra | Carapaça facetada violeta, patas curtas e antenas; não é a aranha de argila |
| tronco.jpeg | xilo_rocha | Xilo-Rocha | Tronco, membros de pedra, folhas e cajado |
| lagarto.jpeg | lagarto_basaltico | Lagarto Basáltico | Basalto escuro, fissuras laranja e olhos ciano |
| aranha-robo.jpeg | aranha_ferro | Aranha de Ferro | Metal petróleo, pernas articuladas e ímã |
| golem.jpeg | golem_cascalho | Golem de Cascalho | Pedras, musgo e juntas âmbar |
| mineiro.jpeg | mineiro_sonolento | Mineiro Sonolento | Capacete, lampião ciano, bocejo e fumaça |
| eco.jpeg | eco_azul | Eco Azul | Corpo ciano com núcleo branco e sombra roxa deslocada |
| tatu.jpeg | tatu_fossil | Tatu Fóssil | Casco creme estrelado e focinho de broca |
| abelha.jpeg | abelha_ambar | Abelha de Âmbar | Abdômen âmbar e asas facetadas douradas; variação do brief que previa asas ciano |

## Diferenças importantes

- A toupeira recebida é Pavio. Não substituir automaticamente o Torrão existente: o conceito de HUD usava uma toupeira com pá, e o runtime tem a identidade `torrao`. São identidades distintas.
- A criatura baixa de rocha do conceito corresponde ao Besouro Pedra; usar a referência recebida em futuras revisões autorizadas. A criatura argilosa corresponde à Gosma de Terra, cuja folha define a versão com folhinha.
- O Cristal Vivo (`cristal_vivo`), décimo tipo do catálogo, não tem folha nesta pasta. Os arquivos cobrem 16 dos primeiros 17 tipos, não os 50 planejados.

## Preparação ainda necessária

Os arquivos são JPEGs com fundo branco, várias poses, efeitos e sombras embutidos. Não possuem transparência, camadas separadas, metadados de recorte, pivôs ou tempos de animação. As quantidades e posições de poses variam entre folhas; não presumir uma única grade. Verificar continuidade das peças, enquadramento e escala antes de montar clipes. Converter JPEG em PNG sozinho não remove o fundo nem recupera qualidade perdida.

Para uso no combate, a protagonista fica à esquerda olhando para a direita; inimigos chegam pela direita e devem olhar para a esquerda. Preparar orientação sem espelhar indevidamente assimetrias importantes.

Esta entrega apenas identifica e registra correspondências. Originais não renomeados, não recortados e não substituídos; nenhuma importação ou mudança de gameplay/Unity realizada. A procedência/licença das imagens recebidas não foi informada.
