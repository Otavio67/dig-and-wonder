# Dig & Wonder — assets separados do HUD de gameplay

Os elementos principais da composição final foram separados em dois atlases de UI e também exportados em PNGs individuais para facilitar a migração para prefabs no Unity.

## UI

- `UnityProject/Assets/_Game/Resources/Art/UI/dw_hud_top_atlas.png`: carteira de ouro, carteira de cristal, menu, placa de fase, missão e botão de melhoria.
- `UnityProject/Assets/_Game/Resources/Art/UI/dw_hud_combat_atlas.png`: missão compacta, AUTO, slot ativo, slot bloqueado, força e linha de melhoria.
- `UnityProject/Assets/_Game/Resources/Art/UI/dw_nav_icons_v2.png`: MINA, MODOS, OFICINA, PETS, BAÚS e LOJA.
- `UnityProject/Assets/_Game/Resources/Art/UI/Extracted/`: cópias individuais dos componentes acima, com transparência preservada.
- `UnityProject/Assets/_Game/Resources/Art/UI/Structure/`: estruturas reutilizáveis do HUD: painel de melhorias, trilho dos poderes, linha de upgrade, faixa de missão, base do menu inferior e trilho de rolagem.
- `UnityProject/Assets/_Game/Resources/Art/UI/dw_hud_structure_atlas.png`: atlas-fonte dessas chapas azuis de aço/concreto, usado para manter o acabamento consistente entre as telas.

`PrototypeGameUI` corta os atlases em sprites independentes durante a inicialização, mantendo textos, valores, estados de bloqueio e botões como elementos dinâmicos do jogo.

## Chão de combate

O `worldPanel` é a área exclusiva do cenário. A broca e os inimigos usam âncoras relativas a ele, com a linha de combate em aproximadamente 34–37% da altura a partir da base do cenário — sobre o caminho amarelo do fundo. O HUD inferior fica fora desse painel, portanto as unidades não são mais posicionadas em relação à borda da tela.

## Convenções

- azul-petróleo/carvão: estrutura e profundidade;
- âmbar: moeda, recompensa e evolução;
- ciano: cristal, energia, seleção e slots ativos;
- coral: dano, perigo e fechamento;
- contornos escuros, rebites e cortes de broca: leitura rápida em tela vertical.
