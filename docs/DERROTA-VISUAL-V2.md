# Tela de derrota: estatísticas e iconografia — 31/10/2026

Pedido: melhorar diagramação e substituir os pictogramas planos das estatísticas por ícones cartoon com volume, alinhados ao acabamento ilustrado do jogo.

Ícones criados com image_gen integrada, usando products.png e o ícone próprio da Pioneira como referências de material/volume. Atlas 2x2 transparente, sem números ou rótulos: cronômetro de latão/teal com mostrador creme; projétil helicoidal com rastro ciano/âmbar; cabeça de inimigo mineiro de pedra; grupo de três criaturas representando ondas. Não são novos inimigos de gameplay.

Prompt do atlas: production four-icon atlas, square two-by-two equal cells, each prop within its own cell, transparent background, polished 2.5D toy-mecha cartoon props rather than flat diagrams or sketchy drawings. Stop watch, drill projectile, helmeted stone-mole enemy, and group of three stone creatures. Crisp navy outlines, brass/teal metal, cyan energy and cream highlights, no words or numbers, matching Pioneira and reward-art references.

O primeiro fundo de estatística era baixo demais para duas linhas. Foi substituído por cartão compacto mais alto, com área de leitura ampla, medallion à esquerda e bordas menos invasivas. Prompt final: redesign the statistic card to a taller compact 2:1 plaque, preserve cartoon steel/teal/brass materials, left empty circular socket, right 65% clean reading surface occupying at least 70% of card height, thinner rim, corner details only, no text/icons and transparent exterior.

Rótulos, valores e ícones continuam separados e funcionais no Unity. Mantidos os contadores reais, o bloqueio de anúncio futuro e o reinício de fase após 10 segundos. O popup inclui a broca no cabeçalho em vez do ícone de chave genérico. Capturas com UiVisualReview usam perfil isolado, sem modificar o save do usuário.
Assets finais: defeat_icons_v2.png (quatro ícones isolados em atlas) e defeat_stat_card_v3.png (moldura alta). Os sprites são registrados em BuildIllustratedMenuArt e usados exclusivamente no resumo de derrota.

Validação: compilação pelo Unity 6000.6.6f1, Play Mode em 540x960, captura real derrota.png e teste de reinício automático após 10 segundos aprovados em death-review-final.log ([UIReview] Capturas e reinício após 10s verificados). Sem alterações na simulação, contadores, economia ou save nesta revisão visual.

APK: C:\Users\otaau\Projects\Dig-and-Wonder\DigAndWonder-derrota-20261031.apk. Build Android concluído e assinatura v2 verificada com apksigner. Tamanho do arquivo: 91.467.348 bytes. SHA256: C11847C12246D492E454C0E96C136D3906843D4EC39CDA154DA588EF60E86A2C. Testado em Play Mode; instalação física não executada nesta revisão.
