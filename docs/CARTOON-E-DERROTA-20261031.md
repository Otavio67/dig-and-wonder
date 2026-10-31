# Acabamento ilustrado e popup de derrota — 31/10/2026

## Causas corrigidas
O sprite hud_panel tem o centro branco e depende da cor aplicada no Image. Os popups usavam Color.white. Foram definidos preenchimentos escuros e adicionados fundos ilustrados. A navegação de cinco itens usava a arte de uma bandeja de seis células; agora as regiões de toque e os ícones seguem cinco colunas independentes. Missão e poderes perderam a moldura compartilhada; os módulos usam círculos.

## Arte
Ferramenta: image_gen integrada. Guia IDENTIDADE-TOY-MECHA aplicado antes/depois. Referências próprias: ícone Pioneira e baú ilustrado extraído do HUD aprovado. Os arquivos de produção ficam em UnityProject/Assets/_Game/Resources/Art/UI/Illustrated/: products.png (atlas 2x2), menu_background.png, offer_frame.png, menu_avatar.png (cópia do ícone próprio). Fontes/pesos funcionais continuam no Unity. Não há texto embutido nos PNGs.

Prompts finais usados:
1. Atlas: production game art asset sheet, one square 2 by 2 atlas with four isolated richly illustrated shop products, transparent background, one per equal quadrant, no text/cards/numbers; top left dark teal miner pouch with cyan crystals and brass clasp; top right open brass/steel chest with cyan crystals; bottom left rounded toy-mecha safe; bottom right mining cart with crystals and gold. Cartoon painted cel shading, thick navy outlines, hard highlights, teal/brass/copper and cyan discovery glow, matching Pioneira reference quality, no robot, not flat SVG.
2. Ambiente: production portrait menu background, 1024x1536, no UI/text/buttons/characters; subterranean merchant workshop, brass arch and cyan crystal on top, dark blue steel columns, copper pipes and amber lanterns at edges, teal energy tubes below, clean calm dark petroleum central 75%, crisp painted cartoon metal and subtle atmospheric light, artwork full bleed.
3. Cartão: isolated portrait 2:3 shop offer frame, transparent exterior and opaque dark navy teal interior, no text/product/icons; rounded chamfered corners, dark outline, wide brass rim and cream edge highlight, copper under-rim, corner bolts, one tiny cyan diamond in upper rim, spacious clean center, matching reference chest's material quality.

Inspeção: quatro produtos distintos e separados por quadrantes, contornos e paleta coerentes, leitura preservada; integração revisada em capturas reais do Unity a 540x960. Pesquisas visuais: https://forum.gamer.com.tw/C.php?bsn=76028&snA=2 ; https://about.my.games/blog/398-offerwalls-a-guide-to-everything-you-ever-wanted-to-know . Referências orientam hierarquia, cartões e materiais; não foram copiadas para o jogo.

## Derrota
Popup modal com tempo real da tentativa, projéteis da broca lançados, inimigos derrotados e ondas concluídas. Estatísticas ficam em ActiveRunSnapshot e são clonadas na retomada; saves anteriores recebem contadores zero. O botão Morrer reinicia a mesma fase preservando a economia/XP existentes. Após 10 segundos de tempo ativo de aplicativo, ocorre o mesmo reinício. Com gravação bloqueada, o reinício aguarda e o erro aparece. Continuar está indisponível, identificado como anúncio futuro; nenhuma chamada de anúncios foi adicionada.

## Verificação
108 checks de domínio passaram, incluindo teste novo que compara estatísticas com eventos reais e testa persistência/retomada. Compilação com stubs e bibliotecas Unity passou. UiVisualReview entrou em Play Mode com perfil separado, capturou perfil, loja, configurações, HUD e derrota, e confirmou o reinício automático após 10 segundos. Os testes usam DW_UI_REVIEW_SAVE apenas no Editor; não substituem o save do usuário. Build Android executado depois desse ensaio. Desempenho e interação em aparelho não medidos.
## Botões e campos ilustrados
Após as capturas iniciais, o usuário pediu botões com acabamento próprio e campos de perfil que não fossem caixas pretas. Foram gerados buttons.png e profile_fields.png com a ferramenta image_gen integrada. Os textos são renderizados no Unity, sem rótulos embutidos.

Prompt de botões: production UI button skin atlas, wide 2:1, two columns and two rows; normal blue-steel tab, selected emerald/cyan tab, available workshop-green action button, and round coral close disk. Transparent background, no text or symbols, chamfered rounded forms, dark navy outline, brass corner plates and rivets, handpainted metal bevels, cream top highlights and short hard lower shadow. Reference: offer_frame.png, same cartoon material quality.

Prompt de campos: profile UI atlas, wide 2:1, four separate assets; square portrait frame with truly transparent center and a cyan top diamond, sculpted steel/brass name input field, teal stat information plate with asymmetric copper/gold cap, and recessed account ID screen. Dark clean interiors, details only around edges, transparent exterior, no words or icons, no gears, matching offer_frame.png materials.

Recortes foram ajustados à posição real dos desenhos. Abas, ações e fechamento têm recursos distintos. Campos usam bordas de nove partes; retrato e botão redondo mantêm a proporção. Capturas reais do Unity foram usadas para revisar margens, bordas e leitura.
A loja distribui suas cinco abas em duas linhas (3+2), preservando a largura visual dos botões e seus alvos de toque. O ID e as estatísticas receberam padding para não atravessar ferragens.
