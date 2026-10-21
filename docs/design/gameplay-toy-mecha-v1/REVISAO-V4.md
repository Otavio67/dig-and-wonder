# Revisão v4 — HUD, modos e avanço lateral

20/10/2026. Conceitos criados com image_gen integrado após leitura do guia de identidade. Sem implementação Unity nesta tarefa.

## Imagens

- conceito-gameplay-hud-v4.png: AUTO maior, ícones pequenos coloridos, acesso Modos, lista de melhorias com scrollbar e próxima linha parcialmente visível. Broca à direita avançando para a esquerda; inimigos à esquerda. Abrigo de mineiros, carrinho, tecido e poeira contextualizam o cenário.
- conceito-menu-modos-v1.png: proposta de acesso Campanha e Profundezas (subtítulo Torre invertida). Profundezas é um nome sugerido, não aprovado definitivamente; botão Em preparação evita anunciar disponibilidade atual.

## Melhorias e movimento

Autorreparo significa recuperar PV automaticamente. A apresentação final deve consultar a tabela real de melhorias/requisitos. Ordem proposta para a lista: Ataque, Vida, Autorreparo, Cadência, Chance crítica, Dano crítico e Especial. Valores/requisitos da referência externa não foram importados para a economia.

O scroll está representado visualmente: faixa rolável limitada às melhorias, scrollbar e linha parcial, mantendo poderes, missão e navegação fixos. Arrasto e desbloqueios reais ainda dependem de implementação.

Para a futura animação, propõe-se mundo em camadas passando à direita quando a broca avança à esquerda, variação de velocidade por distância, elementos de mineração em planos intermediários, vento/tecidos/poeira sutis e desaceleração ao enfrentar grupos. Nenhuma animação foi gerada ou testada nesta entrega estática.

## Revisão

Conferidos cinco slots de poder, AUTO legível, seis abas completas, ícones com cor preservando silhuetas simples, indicação de rolagem e direção de combate invertida. A iconografia de VIDA/Autorreparo e pesos de fonte ainda devem ser refinados nos componentes reais; esta imagem não substitui os ícones vetoriais/TMP nem teste de contraste e toque.

## Prompt HUD

Use case: precise-object-edit. Edit the attached latest full portrait Dig & Wonder CONCEPT. Preserve art identity, dark steel/copper UI, golden Pioneira robot, small distant units, broad environment, top wallet values 1.240 gold/120 cyan crystals and stage plaque, mission 01, exactly FIVE ability slots, and clean chamfered panels. Tall full portrait screen with complete uncropped bottom labels.
Requested changes:
1. Make heading 'AUTO' appreciably LARGER and bolder, almost as readable as 'ATQ', within its existing left switch area. Keep ON/OFF physical switch visible, currently ON; do not rename it AUTO-ATAQUE. No overlap.
2. Keep the approved SMALL SIMPLE bottom icon SHAPES, but color their fills while keeping unified outline/scale: MINA pickaxe copper handle/pale steel head; OFICINA wrench workshop teal/pale steel; PETS paw warm copper pads/teal enamel; BAÚS chest amber gold/brown; LOJA bag muted coral with copper tag. Add ONE compact tab 'MODOS' beside MINA with an elevator/lift and down-arrow icon (cyan/steel). EXACT six tabs left to right 'MINA', 'MODOS', 'OFICINA', 'PETS', 'BAÚS', 'LOJA'. Fullwidth low dock flush edges, small icons not miniature scenes. Keep cyan underline for MINA only.
3. Make the UPGRADE ROW AREA a clearly vertically scrollable independent panel, between fixed ability row and fixed bottom nav. Add a slim visible scrollbar at its right edge, thumb near top. Three readable rows shown with a fourth only partly visible at the bottom under a fade/mask, so it is obvious there is MORE below. Fixed top currency, mission, ability strip and bottom nav do not scroll. Match approximate structure of reference idle RPG stat rows but retain original industrial plate system.
Visible row1 exact labels 'ATQ', 'NV. 0', '6', 'Melhorar', coin '18'.
Row2 'VIDA', 'NV. 0', '120', 'Melhorar', coin '20'.
Row3 'AUTORREPARO', short explanatory text 'Recupera vida', dim button 'Em preparação', armored heart with small spanner icon (not a generic plus).
Partial row4 'CADÊNCIA', lock and short hint 'Desbloqueia depois', dim treatment. All text legible, Nunito-like weight. Do not copy reference gigantic numeric balance.
4. REVERSAL OF TRAVEL DIRECTION only in battle (not top avatar): Pioneira on RIGHT side x83% facing LEFT with drill pointing left, ALL THREE existing small enemies on LEFT x10%,22%,34% facing right and approaching from the left. Retain their SMALL v3 scale and generous empty ground gap. Same level horizontal ground. Keep subtle damage bars and restrained projectiles moving leftward. No overlap or enlarged units.
5. In existing cavern midground, add one small weathered miner shelter with warm window and one ore cart/rail marker, peripheral not central obstacle, with simple copper/wood construction matching cavern. Add a few very faint horizontal dust/wind wisps moving to the right, and hanging cloth fluttering; do not cover combat HUD with smoke. Background implies traveling through a populated mining world while remaining underground. Preserve layered fossil arches, warm light and cyan crystals. This is a STATIC concept representing future parallax, do not add movement arrows/technical labels in the artwork.
Colors functional: petrol #1B263B/charcoal, clean steel #4B485C, emerald #1E6F5C–#2A9D8F buttons, amber gold, cyan selection/energy, localized copper/rust, cream type. Avoid repeated gears and emoji. No phone frame/watermark/collage. Small units, landscape visible, navigation and scroll must fit fully inside canvas. No cropped text.

## Prompt modos

Use case: precise-object-edit. Image 1 is the latest Dig & Wonder portrait gameplay screen. Create its MODE SELECTOR state as ONE full portrait screen. Preserve the exact entire upper battlefield, top wallet 1.240/120, small left-facing golden drill on right, small enemies left, shelter/orecart, cyan crystals, warm cavern, world scale. Keep fullwidth six-tab bottom dock MINA/MODOS/OFICINA/PETS/BAÚS/LOJA and its newly colored simple icon shapes. Now mark MODOS active with cyan inset/underline instead of MINA.
Change only the lower panel BETWEEN the cave's ground cutaway and the bottom navigation: replace mission/abilities/upgrades there with a clean mechanical MODE SELECTION panel; do not place a full-screen pop-up over the cave.
Header 'MODOS DE EXPLORAÇÃO', amber Titan-style modest title on dark steel plate, small coral close button upper right. Subtitle 'Escolha sua próxima expedição' in clean cream Nunito.
Two wide, generously padded original illustrated mode cards stacked:
Card 1: 'CAMPANHA' with compact horizontal mine-track symbol and small local vignette of a tunnel path. Description 'Avance pela mina e enfrente guardiões.' Green button 'Continuar'. Tiny label 'Modo atual'.
Card 2: 'PROFUNDEZAS' with compact mine elevator and downward-arrow emblem, small cutaway vignette showing stacked underground floors receding downward. Subtitle 'Torre invertida'. Description 'Desça andar por andar.' Dim button 'Em preparação'. Inset cyan crystal accents, NO fantasy castle/tower rising into sky, no invented reward numbers. This mode is a future concept not claimed functional.
Chopped-corner steel panels #4B485C, petrol #1B263B base, copper edges with few rivets, creme #FFF8E7 text, emerald available action, cyan energy, amber title accent. Match existing chunky Toy-Mecha 2D cartoon, same type hierarchy, inner text areas plain, icons clean. Avoid visual clutter, no extra currencies or modes, no emojis, no gear excess. All text verbatim Portuguese and fully visible. No phone frame, annotations, collage, watermark.

