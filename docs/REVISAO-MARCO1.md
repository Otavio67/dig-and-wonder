# Revisão visual do Marco 1 — 20/10/2026

Solicitada pelo usuário antes de avançar ao Marco 2. Faixa própria de solo pintado, com torrões, raízes e pedras, preservando a camada funcional única. Poço com paredes em perspectiva e escurecimento vertical; resistência sobre placa para não disputar com a Toupeira. Aparição mantém a regra original: destaque no poço e nome temporário. Recompensa do mob mostra `+7 OURO / TORRÃO` na posição da derrota.

Melhorias mostram nível, atual → próximo, custo em OURO e saldo. Espaços indisponíveis dizem BLOQUEADO, sem prometer fases ainda não jogáveis.

Exceção econômica explicitamente pedida: `first_impact_gold: 2` em `content/defense/prototype.json`. O primeiro dano válido de camada de cada tentativa concede 2 ouro uma vez. Outros impactos não repetem. Reiniciar repõe esse bônus junto da tentativa, assim como permite repetir recompensas normais de camada. Não é bônus por instalação; não adiciona campo permanente ao save. Recompensas, preços e demais valores anteriores inalterados. Dois ouros mostram progresso inicial; a melhoria de dano ainda custa 18.

Captura desktop nova, retrato 540×960, pela mesma sequência de input, sem persistência: `builds/review/revisao-marco1/antes/` e `depois/`. `01-inicio`, `02-primeiro-impacto`, `03-torrao-apareceu` (aparição natural aos 10 s), `04-recompensa-torrao` (abatida pelos canhões). Não houve injeção de inimigo ou ouro nessas capturas. Script `scripts/capture_review.gd`.

Verificação: check nativo Windows e inspeção das capturas; testes do bônus único por tentativa. Sem aparelho: toque real, tamanho físico, desempenho/FPS, consumo, APK e anúncios não verificados. A revisão não altera esquema de save nem migração.

Arte criada com a ferramenta integrada imagegen, skill imagegen; asset `assets/terreno/solo-torroes-raizes.png`. Prompt final: “Production 2D game terrain sprite for Dig & Wonder. A single horizontal cross section strip of EARTH SOIL, wide 3:1 composition, spanning the full width, no separate tiles. Hand-painted digital cartoon illustration with soft painterly brush texture, muted warm umber soil, clustered crumbly earth clods of varied asymmetrical sizes, embedded irregular grey stones and small pebbles, a few fine branching roots near the uneven top edge. Recognizable granular soil, absolutely no wood grain, no timber, no trunk, no plank. Top surface has gentle warm upper-left highlights, underside gets darker brown. Integrated volume and natural overlapping lumps, no thick black outlines, no glossy plastic, no 3D render. Ground strip fills image width and central height, irregular upper and lower silhouette. Actual transparent background outside earth silhouette. No text, characters, UI, logos, background or sky.”

Marco 2 autorizado pelo usuário em seguida: uma unidade alternativa, clareza/ritmo e nova animação da Pioneira com golpes repetidos e quebra final. Essas alterações de comportamento pertencem ao próximo checkpoint.
