# Prompts — arte exclusiva das onze brocas

Criado em 20/11/2026 a pedido do usuário. Guia aplicado: [IDENTIDADE-TOY-MECHA.md](IDENTIDADE-TOY-MECHA.md) v1.3, incluindo a checagem da seção 13. Referência principal: a folha aprovada da Pioneira em `UnityProject/Assets/_Game/Resources/Art/Drill/drill_pioneira.png`. Ela também serve para conferir acabamento, contorno e proporção de todas as outras brocas.

Este documento **não gera imagens**. Ele reúne os prompts prontos para quando a produção for autorizada. Nenhuma arte aqui substitui validação no Unity.

## 1. Como usar

1. **Sempre anexe a folha da Pioneira** como referência de estilo e diga ao gerador que ela é a referência de qualidade e construção, não um modelo para copiar.
2. Gere **primeiro a folha de personagem** (seção 3, prompt A) de cada broca. Só depois de aprovada, gere a **folha de combate 4 × 3** (prompt B), anexando a folha de personagem aprovada da mesma broca.
3. O retrato de oficina (prompt C) é opcional. Ele serve para a vitrine/garagem e o cartão de desbloqueio.
4. Monte cada prompt assim: **Bloco de estilo comum** + **bloco da broca** + **bloco técnico do formato** + **negativos**.
5. Ordem sugerida, igual à de desbloqueio: Pioneira (refinamento) → Rebite → Centelha → Bastião → Prisma → Sísmica → Aurora → Cobalto → Nova → Titã → Singular. Revise as três primeiras juntas antes de produzir o resto, como recomenda a seção 14 do guia.

## 2. Contrato técnico com o Unity

O código atual (`DrillSpriteSheetPlayer2D`) lê uma folha de 12 frames com nomes fixos. Uma folha nova deve respeitar este formato para entrar sem refazer código:

| Item | Valor |
| --- | --- |
| Tela | 1680 × 960 px, PNG RGBA com transparência real fora da máquina |
| Grade | 4 colunas × 3 linhas, células de 420 × 320 px |
| Linha 1 | `idle_00`–`idle_03`: respiração da suspensão, cristal/visor pulsando |
| Linha 2 | `walk_00`–`walk_03`: esteiras em tração, poeira atrás (à esquerda), ponta girando |
| Linha 3 | `action_00` preparação · `action_01` ponta girando forte · `action_02` disparo do projétil · `action_03` recuperação/careta de impacto |
| Direção | Broca **virada para a direita** (avança para a direita; inimigos chegam pela direita) |
| Contato | Esteiras apoiadas na mesma linha de chão em todas as células, ~8% acima da borda inferior da célula (pivô X 0,50; Y 0,08) |
| Ocupação | A máquina ocupa ~80% da largura da célula, com margem para poeira e projétil na linha 3 |
| Arquivo | `UnityProject/Assets/_Game/Resources/Art/Drill/drill_<id>.png` (ids abaixo) |

Os ids de arquivo são `pioneer`, `rivet`, `spark`, `bastion`, `prism`, `seismic`, `aurora`, `cobalt`, `nova`, `titan` e `singularity`. Os nomes dos frames continuam `idle_00`…`action_03`. Para trocar a folha por broca, `DrillSpriteSheetPlayer2D` precisa receber o caminho por personagem, porque hoje ele está fixo em `drill_pioneira`. Essa mudança de código é pequena e deve ser feita quando as primeiras folhas chegarem.

## 3. Blocos reutilizáveis

### Bloco de estilo comum (colar em todos os prompts)

```text
Dig & Wonder "Toy-Mecha Subterranean" production game art. A small, compact,
charismatic tracked drilling machine that is the hero of a 2D side-view
auto-battler. Chunky toy-like proportions: wide low chassis on rubber tank
treads, a big conical drill bit pointing RIGHT, a rounded cabin with a dark
visor screen that works as the face (two glowing eyes, simple expressive
shapes), a faceted crystal socketed into the top of the machine.
Cartoon cel-shading with 2-3 light planes, short hard specular highlights on
metal, thick dark navy outer outline (#1B263B) and thinner interior lines.
Light from upper-left, warm ambient with cyan accent glow from the crystal.
Materials must read as distinct: painted metal plates, brass/copper fittings,
matte dark rubber treads, faceted glowing crystal. Wear and scratches only on
plate edges. Clean, readable silhouette at small mobile size; few large
functional parts instead of micro-detail. Match the construction, line weight
and finish quality of the attached reference sheet (Broca Pioneira) exactly.
```

### Negativos (colar em todos os prompts)

```text
No text, no letters, no numbers, no logos, no UI, no frames, no background
scenery, no ground plane, no drop shadow blob, no cast shadow on background.
No humans, no pilot visible, no arms, no legs, no wings. No gears used as
decoration (at most one small functional gear). No photorealism, no glossy
plastic, no 3D render look, no pixel art, no anime. No emoji-like icons.
Do not mirror the machine: drill bit always on the RIGHT side.
Do not copy characters or designs from existing games.
```

### Bloco técnico A — folha de personagem (aprovação de design)

```text
Character design sheet on a fully transparent background, 2048x1024.
Left: the machine in clean side view facing right, neutral idle pose.
Right: the same machine in three-quarter view (front-right), same parts and
colors. Below each, small callouts of the crystal, visor face and drill bit
as isolated detail drawings. Consistent scale between the two views.
```

### Bloco técnico B — folha de combate 4 × 3

```text
Production sprite sheet, 1680x960, fully transparent background (real alpha),
exactly 4 columns x 3 rows of equal 420x320 cells, one pose per cell, nothing
crossing cell borders. Side view, machine faces RIGHT in every cell, treads
on the same ground line in every cell (8% above cell bottom), identical scale.
Row 1 (idle, 4 frames): subtle suspension bob, crystal glow pulsing, eyes blink
on frame 3.
Row 2 (walk, 4 frames): treads in traction, small dust puffs trailing to the
LEFT behind the machine, drill bit spinning with motion lines.
Row 3 (action, 4 frames): frame 1 wind-up (chassis leans back, eyes narrow);
frame 2 drill bit spinning hard with strong motion rings; frame 3 fires its
projectile to the right with a short muzzle flash (projectile described in the
drill block); frame 4 recovery, squinting ">_<" style eyes made of simple shapes.
Use the attached approved character sheet of THIS drill for design consistency
and the attached Broca Pioneira sheet for layout, poses and timing.
```

### Bloco técnico C — retrato de oficina (opcional)

```text
Single isolated illustration on a fully transparent background, 1024x1024,
three-quarter view facing front-right, slight low angle, the machine standing
proudly on its treads, crystal glowing, confident expression on the visor.
Leave ~10% empty margin on all sides. No platform, no scenery.
```

## 4. Uma broca por vez

Cada bloco mantém a **cor de identificação** que o jogo já usa (`TintR/G/B` em `Economy/midgame.json`), para que a arte nova continue reconhecível nas telas atuais. A evolução de raridade aparece no tamanho do cristal, na complexidade da silhueta e na qualidade dos materiais. Simples e Boa são utilitárias, Épica e Lendária têm peças assinatura e Única e Mística têm energia visível, sem nunca perder a leitura em tamanho pequeno.

### 1. Broca Pioneira — `pioneer` · Simples · inicial
Função: dois impactos concentrados, começo fraco e confiável. Arte atual aprovada; o objetivo é **refinar, não redesenhar**.

```text
Broca Pioneira: the starter. Mustard-yellow painted body with forest-green
panels, brass trims, red round tail lights and side lamps, small teal exhaust
canister at the back. Single medium cyan crystal in a brass crown socket.
Medium drill bit in worn steel with bronze grooves. Humble, eager, friendly
expression. Projectile: a small spinning steel drill tip with a yellow spark trail.
Keep this design faithful to the attached sheet; only clean up line weight,
consistent ground contact and alignment between frames.
```

### 2. Broca Rebite — `rivet` · Boa · fase 10
Função: dois impactos reforçados, a primeira troca segura. Tinta do jogo: creme quente (1, 0,87, 0,68).

```text
Broca Rebite: a sturdier, riveted upgrade of the starter. Warm cream and
sand painted armor plates held by big visible rivet rows, copper banding,
dark steel treads with reinforced side skirts. Thicker, heavier drill bit
with two copper collar rings. Crystal slightly larger, cyan, protected by a
riveted cage. Square-ish, dependable silhouette, slightly taller cabin than
the Pioneira. Determined, steady eyes. Projectile: a heavy riveted drill
tip leaving a short copper-orange spark trail.
```

### 3. Broca Centelha — `spark` · Boa · fase 20
Função: três tiros leves e rápidos, identidade de cadência. Tinta: azul-céu claro (0,65, 0,95, 1).

```text
Broca Centelha: light and nimble. Pale sky-blue painted body with white
accent stripes and small copper coils, lower and sleeker than the others,
thinner treads with a forward lean. Slim, long drill bit with spiral cyan
grooves that glow faintly. Two small exposed tesla-coil antennas behind the
cabin crackling with tiny cyan sparks. Small bright cyan crystal. Quick,
playful, eager expression. Projectile: three thin glowing cyan needle
bolts fired in a quick burst, each with an electric spark tail.
```

### 4. Broca Bastião — `bastion` · Épica · fase 30
Função: dois tiros pesados, blindagem e impacto. Tinta: lavanda-aço (0,85, 0,78, 1).

```text
Broca Bastião: a walking fortress. Heavy lavender-steel armor plates with
45-degree chamfered corners, thick dark gunmetal frame, a front shield
plate around the base of the drill, extra-wide treads with armored skirts.
Short, very thick drill bit like a battering ram with deep grooves. Crystal
recessed behind a slotted armored hatch, glowing violet-cyan through slits.
Visor narrower, like a helmet slit, calm and stoic eyes. Projectile: a
large blunt armored drill shell with a heavy violet shockwave ring on hit.
```

### 5. Broca Prisma — `prism` · Épica · fase 40
Função: estilhaço que atinge um alvo secundário, dano espalhado. Tinta: verde-menta (0,72, 1, 0,8).

```text
Broca Prisma: crystal-focused. Mint-green painted body with silver trims,
the drill bit itself is partly made of faceted translucent mint crystal
set in a steel spiral frame. Three small crystal prisms socketed along the
back like a crest, refracting little rainbow glints. Medium treads. Curious,
bright expression. Projectile: a faceted crystal shard that, on hit, splits
into one smaller glowing shard flying off at an angle (show the split in
action frame 4 as small fragments, not a big explosion).
```

### 6. Broca Sísmica — `seismic` · Lendária · fase 50
Função: especial em três alvos, entrada no Submundo. Tinta: terracota-salmão (1, 0,72, 0,62).

```text
Broca Sísmica: earth-shaker. Terracotta and rust-orange painted heavy body
with dark basalt-grey plates, glowing amber seams like cooling lava along
panel lines, two piston-hammers on the sides of the chassis. Wide, thick
drill bit with stepped ridges. Crystal is amber-orange with a cyan core.
Very wide, low, grounded silhouette, treads with spiked grips. Fierce,
focused expression. Projectile: a heavy stepped drill tip; special attack
reads as a ground shockwave arc that would cover three targets (show the
shockwave starting at the bit in action frame 3).
```

### 7. Broca Aurora — `aurora` · Lendária · fase 60
Função: três tiros, especial duplo e ricochete, luz concentrada. Tinta: azul-lavanda claro (0,84, 0,88, 1).

```text
Broca Aurora: luminous and elegant. Pale periwinkle and pearl-white painted
body with brass trims, translucent light-guide fins along the top that
glow with soft aurora gradients (teal to violet to pink, kept small and
localized). Smooth tapered drill bit with a ribbon of light spiraling
along it. Large clear crystal cluster on top emitting soft light. Serene,
confident expression. Projectile: three slender beams of aurora light,
one of them bending to ricochet (show a small bounce glint).
```

### 8. Broca Cobalto — `cobalt` · Lendária · fase 70
Função: tiros perfurantes, minério cristalino. Tinta: azul-cobalto (0,42, 0,72, 1).

```text
Broca Cobalto: armored in crystalline ore. Deep cobalt-blue painted body
with dark navy steel and silver edges, natural cobalt ore crystals growing
out of armor plates in clean faceted clusters (shoulder and rear). Long,
sharp, narrow drill bit with a polished cobalt-crystal tip, built to pierce.
Crystal on top is deep blue with a white-hot center. Sharp, intense eyes.
Projectile: a long piercing cobalt spike that passes through the target,
leaving a straight blue streak.
```

### 9. Broca Nova — `nova` · Única · fase 80
Função: rajada tripla e estilhaço, portal para outra dimensão. Tinta: laranja-estelar (1, 0,55, 0,3).

```text
Broca Nova: touched by another dimension. Coral-orange and deep-plum painted
body with brass trims, a small floating ring around the rear housing that
shows a swirl of starry violet space inside (portal motif, kept small and
clean). Drill bit split into three rotating segments around a hot white-orange
core. Crystal is a burning orange-gold star shape set in a dark socket.
Two tiny crystal shards orbit the machine. Bold, excited expression.
Projectile: three small fiery star-cores fired in a burst; on hit one bursts
into small shards.
```

### 10. Broca Titã — `titan` · Única · fase 90
Função: impactos pesados e especial amplo, força para a reta final. Tinta: bronze-acinzentado (0,72, 0,68, 0,62).

```text
Broca Titã: colossal and battle-worn. The biggest silhouette of the roster
(about 15% larger than the others in the cell, still inside margins).
Weathered bronze and dark gunmetal heavy plates, huge exhaust stacks, double
stacked treads, hydraulic pistons supporting a massive twin-grooved drill
bit. Crystal is a large cyan block embedded in the chest like a reactor core.
Scratches and dents only on plate edges. Stern, unstoppable expression.
Projectile: a massive drill warhead with heavy motion trail; special shows a
wide sweeping shockwave arc.
```

### 11. Broca Singular — `singularity` · Mística · fase 100
Função: três tiros, especial triplo e estilhaço, forma final da campanha. Tinta: verde-água claro (0,7, 0,95, 0,9).

```text
Broca Singular: the final, mythic form. Pearl-teal and polished white-gold
body, sleek but still a compact toy-mecha (not humanoid), floating rings of
faceted crystal around the drill bit, a tiny dark singularity orb with a
cyan event-horizon halo set in the crown instead of a regular crystal.
The drill bit is crystal and white-gold, with energy lines flowing from the
core to its tip. Treads glide with a faint cyan hover glow under them but
still touch the ground line. Calm, wise, powerful expression.
Projectile: three crystal-light bolts in a burst; on hit they shatter into
small shards. Keep effects localized and readable at mobile size.
```

## 5. Checagem antes e depois de cada geração (seção 13 do guia)

Antes:
- [ ] Prompt montado com os quatro blocos e a referência da Pioneira anexada.
- [ ] Broca virada para a direita, sem texto, com fundo transparente e grade correta (para o prompt B).
- [ ] Sem engrenagens decorativas, sem emojis e sem cópia de jogo de referência.

Depois:
- [ ] A silhueta se distingue das outras dez em tamanho pequeno (teste reduzindo para 96 px de largura).
- [ ] Mesmas peças nas duas vistas e entre frames, sem peça duplicada ou faltando, e as esteiras na mesma linha.
- [ ] Transparência real (sem fundo xadrez "pintado"), nada atravessando a borda das células.
- [ ] Cor de identificação coerente com a tinta do jogo e raridade legível na complexidade.
- [ ] Registrar em `assets/LICENCAS.md`: ferramenta, data, prompt final e referências usadas.

## 6. Pendências de código quando as folhas chegarem

- `DrillSpriteSheetPlayer2D.ResourcePath` está fixo em `drill_pioneira`. Ele precisa aceitar `drill_<id>` da broca equipada e cair na Pioneira se a folha não existir.
- `PrototypeGameUI` (avatar, tela de derrota, menus) também procura `drill_pioneira` diretamente. Esses pontos precisam usar a broca equipada.
- A tela de derrota procura o frame `hurt_00`, que não existe na folha atual. Use `action_03` (careta de impacto) ou acrescente o frame.
- `StartupContent` pré-carrega somente a folha da Pioneira. Inclua as folhas das brocas possuídas.
