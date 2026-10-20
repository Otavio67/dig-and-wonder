# Catálogo de defesa — implementação e propostas

20/10/2026. **Somente Pioneira, Toupeira Torrão e fase 1 pertencem ao Marco 1.** Valores das demais linhas são planejamento, não conteúdo disponível. Uma futura fase precisa ser testada com as ferramentas realmente desbloqueadas.

## Inimigos

Vida relativa usa Torrão = 48 PV (1×). Recompensas são ouro; ninguém retira recursos permanentes do jogador.

| Nome / estado | Aparência e estreia | Aparição | Vida | Dano / intervalo | Alvo | Habilidade e antecipação | Fraqueza | Ouro |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| **Toupeira Torrão — implementada** | Pequena toupeira de terra, focinho coral e garras creme; fase 1. Desenho provisório nativo 2D. | Ondas por tempo, após camadas 2/4 e encontro final. | 48 / 1× | 8 / 2,8 s; primeiro ataque em 1,7 s | Defensor vivo na frente; caso contrário, carrinho. Pioneira não é defensor. | Aviso de arremesso por 0,65 s; projétil de terra viaja por 0,65 s. | Canhões básicos, impacto da Pioneira se estiver no trajeto. | 7 |
| Morcego de Cristal — proposto | Asas assimétricas e fragmentos foscos; fase 2. | Pequena onda lateral, até 2. | 0,65× | 6 / 3 s | Carrinho | Rajada após abrir as asas por 0,8 s. | Canhões já disponíveis; drone é alternativa posterior. | 6 |
| Verme Blindado — proposto | Segmentos de pedra e juntas macias; fase 3. | Lateral de camada de pedra, um por vez. | 2,2× | 14 / 4,2 s | Defensor ou carrinho | Armadura reduz 25% do dano; abre por 1,2 s antes do bote. | Janela vulnerável e broca pesada; dano básico continua eficaz. | 15 |
| Esporo de Gás — proposto | Bolsa orgânica âmbar com fendas turquesa; fase 3. | Revelado por camada indicada. | 1,2× | 5 / 3,5 s | Unidades na frente; sem elas, carrinho | Infla por 1 s, cria área de lentidão por 3 s, sem acumular duração. | Bomba em área ou canhões comuns. | 10 |
| Aranha de Quartzo — proposta | Pernas finas e abdômen mineral; fase 4. | Lateral do poço. | 1,5× | 10 / 3 s | Defensor ou carrinho | Após 4 s sem dano, regenera no máximo 15% da vida uma única vez; brilho de 0,8 s antes. | Pressão contínua dos canhões; regeneração finita. | 12 |
| Golem de Cascalho — proposto | Pedras empilhadas de forma irregular, núcleo cobre; fase 5. | Revelado em camada sinalizada. | 3× | 20 / 5 s | Defensor ou carrinho | Escudo de camada com absorção finita de 30 PV, uma vez; vulnerável por 1,5 s após golpe avisado. | Janela vulnerável; básicos podem esgotar o escudo. | 22 |
| Sentinela Enferrujada — proposta | Drone de sucata e uma lente; fase 4 ou posterior, condicionado à defesa adequada. | Lateral superior. | 1,1× | 9 / 3 s | Carrinho | Mira de 1 s antes de disparar. | Canhões e drone do jogador; não exigir mira manual. | 12 |

## Habilidades

Os dados preparados estão em `content/defense/abilities.json`, com `implemented: false` para as seis propostas. Não há botões, lojas nem lançamentos falsos para elas. Todos os limites e valores futuros exigem revisão ao implementar.

| Habilidade / estado | Aparência e animação propostas | Compra / fase | Dano, vida, duração | Alvo e diferença | Cooldown / limite | Melhorias e limites |
| --- | --- | --- | --- | --- | --- | --- |
| **Pioneira — implementada** | Miniatura do conjunto mecânico atual, ponta em movimento durante a queda e impacto de material. | Grátis / 1 | 32 de impacto; voo 0,8 s; sem vida persistente. | Coluna central: menor ID terrestre em ±38 unidades do centro; se não houver alvo válido no impacto, atinge a camada marcada no lançamento. É consumida uma vez, nunca escudo. | 3 s / 2 | Dano compartilhado +12% da base por nível, até 10; cooldown fixo neste marco. |
| Pesada — proposta | Carenagem larga, ponta maciça, queda com peso e forte assentamento. | 120 / 2 | 105; voo 1,5 s | Impacto lento, eficiente contra armadura, sem atravessar encontros. | 8 s / 1 | Dano até nível 10; cooldown mínimo 5 s. |
| Veloz — proposta | Corpo fino, aletas curtas, queda veloz e rastro breve. | 100 / 2 | 24; voo 0,35 s | Resposta rápida e dano inferior por lançamento. | 4 s / 2 | Dano até 10; cooldown mínimo 2,5 s. |
| Bomba-Broca — proposta | Cápsula coral, expansão curta no impacto e fragmentos contidos. | 240 / 3 | 70 em área; voo 1 s | Inimigos na frente e somente camada ativa; não destrói três camadas. | 12 s / 1 | Dano até 10, raio limitado; cooldown mínimo 8 s. |
| Robô Escavador — proposto | Pernas curtas e braço-ferramenta com ciclo distinto de combate. | 150 / 2 | Dano 9, vida 45, duração 10 s | Terrestres próximos antes de minerar; primeiro defensor persistente. | 20 s / 1 | Vida/dano até 10; duração máxima 16 s. |
| Drone Atirador — proposto | Duas hélices e arma leve com recuo; pairar discreto. | 220 / 3 | Dano 8, vida 25, duração 8 s | Aéreos ameaçando o carrinho antes de terrestres; não minera. | 15 s / 1 | Dano até 10; duração máxima 14 s. |
| Robô Guardião — proposto | Escudo articulado e sinal de interceptação. | 300 / 4 | Dano 4, vida 70, duração 10 s | Intercepta até 3 ataques ao carrinho; não bloqueia passagem. | 18 s / 1 | Vida até 10; no máximo 5 interceptações e duração 15 s. |

### Regras comuns

- Lançamento não cobra ouro. Ouro compra/desbloqueia/melhora; cooldown limita uso.
- Três espaços: Pioneira e dois vazios no Marco 1. Não há seletor sem escolhas implementadas.
- Não há dano atravessando camadas no Marco 1. A broca grava o índice da camada no lançamento; se outra broca já a quebrou, o excesso é descartado. Não antecipa recompensa ou pula encontro.
- O alvo de canhão é o inimigo preparando ataque ao carrinho; empate pelo menor ID de aparecimento. Projétil lançado não troca de alvo se ele morrer.
- A implementação futura de um defensor precisa respeitar vida positiva e presença na frente. A Pioneira de impacto não se qualifica. O ramo de prioridade já é verificado com fixture, sem robô disponível no jogo.
- Boss planejado: **Serpente de Sucata**, encontro final de fases futuras, com duas fases de comportamento, aviso e janela de núcleo vulnerável. Sem módulo ou arte de boss no Marco 1.
