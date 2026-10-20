# Oficina da personagem — 0.0.15

## Resultado e acesso

A Oficina usa uma composição de interface, com peças independentes: placa de título, vitrine da Pioneira, pedestal, slots, cartucho de combate, melhorias e navegação em esteira. O Acampamento continua sendo o hub. A tela de personagem abre pelo cartão **Oficina** ou pela aba **Personagem** durante uma expedição; **Voltar** e o botão coral de fechar retornam à origem.

Ao abrir durante a luta, o driver grava o estado seguro e pausa a simulação. Fechar remove essa pausa e limpa o acumulador, sem simular o tempo gasto no menu. A tentativa não é recriada. Compras usam a mesma rotina de economia da base; falhas de escrita continuam bloqueando compras e a tentativa, com novas tentativas de gravação pelo driver. Pausa do aplicativo continua sendo uma condição independente.

## Identidade aplicada

| Papel | Cor |
| --- | --- |
| Fundo | `#1B263B` |
| Painéis internos | `#1E6F5C` |
| Melhorias e destaques | `#FFB703` |
| Seleção e pedestal | `#00F5D4` |
| Fechar | `#FF595E` |
| Texto | `#FFF8E7` |

Titan One nos títulos e botões, DW Rounded ExtraBold nos números e descrições. DW Rounded é uma instância estática de Nunito com peso 800, renomeada para cumprir a licença OFL. Licenças acompanham as fontes em `Resources/Fonts/Workshop/`.

O material TMP de cada uma das duas fontes tem contorno compartilhado; os títulos recebem sombra dura pelo underlay SDF. A largura do contorno depende da escala/renderização TMP e deve ser ajustada no aparelho; não se declara espessura física exata. Não há cópia de material para cada rótulo. As duas fontes e seus atlas são criados uma vez por tela e descartados com ela.

São 18 PNGs originais modulares em `Resources/Art/Workshop/`, gerados por `scripts/create-workshop-art.py`: moldura com rebites, botão com chanfro, slot hexagonal, pedestal, engrenagem, esteira e ícones de peças/ações. Moldura, botão e esteira usam bordas de sprite para escala em nove partes, evitando ampliar os cantos. Texturas de ícones mantêm proporção. A folha existente da Pioneira é reutilizada diretamente.

## Conteúdo real e prévias

- **ATQ e VIDA:** números, níveis, máximos e custos consultados no perfil/ScriptableObjects. A compra fica indisponível sem expedição, sem saldo, no máximo ou durante falha de save. O preço permanece visível.
- **Barra de reforços:** soma dos níveis de ATQ e VIDA, dividida pela soma de seus máximos. É progresso de melhorias compradas, não XP de personagem. Não foi criada uma regra de maestria.
- **Módulo Pioneira:** mostra o número de impactos e dano base do `specialAbility`, sem confundir dano base com dano após melhorias.
- **Broca, Chassi, Motor e Núcleo:** mostram a composição original da Pioneira e abrem explicações por toque. Cadeados sinalizam que troca de peças ainda está em preparação.
- **Passivos:** três encaixes escuros com cadeados, sem buffs fictícios.
- **Peças:** aba de consulta com as quatro peças originais; não equipa itens inexistentes.
- **Drones:** consulta o `unlockPhase` do recurso `drone` e informa que o projeto ainda está em preparação, inclusive quando a fase já foi alcançada.
- **Reparo/Crítico:** requisitos de nível acompanhados de “em preparação”. Atingir o nível não habilita uma habilidade não implementada.

Não há moeda premium nova. A engrenagem representa o ouro já salvo. A tela abandonou o antigo rótulo “Poder” que repetia dano por tiro como se fosse uma pontuação de poder/DPS.

## Hierarquia runtime

```text
Game UI / Safe Area / Background
├── Expedition hub
├── Progression screen [CharacterWorkshopView]
│   ├── Workshop sign / Close workshop
│   ├── Character showcase
│   │   ├── pedestal
│   │   ├── Text (fallback) / Drill Visual [Image, DrillSpriteSheetPlayer2D, ProceduralAnimator2D]
│   │   └── BROCA / CHASSI / MOTOR / NÚCLEO slots
│   ├── Reinforcement track / Energy tube / Energy fill
│   ├── Combat cartridge / Pioneer module / Locked passives
│   ├── Workshop tab 0 [ATQ / VIDA]
│   ├── Workshop tab 1 [Peças]
│   ├── Workshop tab 2 [Drones]
│   ├── Return from workshop
│   └── Navigation tread / Tab OFICINA / Tab PEÇAS / Tab DRONES
└── Combat screen
```

Os botões têm raiz de interação e face separada. `WorkshopButtonFeedback` abaixa a face em 5 unidades do Canvas em 0,09 s, usando tempo não escalado; limpa seu Tween anterior e cancela ao desabilitar/remover. A aba selecionada desloca apenas sua própria raiz em 5 unidades; a resposta do botão pertence à face filha. A broca usa somente animação de idle na vitrine; o pedestal permanece fixo. Não há rotação 3D simulada.

## Validação deste checkpoint

| Verificação executada | Resultado e alcance |
| --- | --- |
| Compilação C# contra DLLs reais Unity/TMP/uGUI/DOTween | Zero erros e avisos; não executa lifecycle/importação |
| Compilação C# contra stubs | Zero erros e avisos; não executa UI |
| DomainChecks | 21 verificações aprovadas de economia, combate e save |
| Editor `-batchmode -nographics -quit` | Bloqueado antes da importação por licença: `No valid Unity Editor license found` |
| Prévia externa da composição | HTML renderizado em Chromium/Playwright com arte e fontes locais; revisada fora do Unity |
| Play Mode da nova tela, toque, gravação e desempenho Android | Pendentes; nenhum APK novo gerado |

O log da tentativa real de Editor fica em `.tools/logs/unity-workshop-0.0.15.log`, ignorado pelo Git. O smoke test anterior executado no PC não valida esta tela.

### Paridade preservada

| Valor | Origem Godot | Unity nesta versão |
| --- | --- | --- |
| Dano por tiro | 6 | 6 |
| Vida base | 120 | 120 |
| Upgrade ATQ | custo 18 + 14 × nível; +12%; máximo 10 | Mesmos dados |
| Upgrade VIDA | custo 20 + 16 × nível; +35 PV; máximo 10 | Mesmos dados |
| Especial Pioneira | 32 de dano em 4 impactos; cooldown 3 s | Mesmos dados |
| Esquema/caminho/checksum de save | Save Unity existente | Sem alteração de formato |

### Prévia de design

`scripts/preview-workshop.py --capture` gera uma composição de design autossuficiente em `.tools/review/workshop-0.0.15/`. A captura é feita em Chromium/Playwright com conteúdo local em memória; não abre sites. Requer Pillow para gerar a arte e Playwright/Chromium apenas para renderizar a prévia. Essas ferramentas não são dependências do jogo.

A prévia tem tipografia e posicionamento aproximados. Ela não executa C#, não demonstra importação dos sprites, resposta ao toque, fluidez nem aparência final do Unity. Não deve ser apresentada como screenshot do jogo. A composição inspecionada está em [design/oficina-0.0.15.png](design/oficina-0.0.15.png).

## Roteiro pendente no Editor/aparelho

1. Atualizar a branch e abrir `UnityProject` com Unity 6000.0.23f1. Aguardar importação de fontes/PNG; abrir `Prototype`.
2. Em perfil novo: Acampamento → Oficina; conferir valores base, botões indisponíveis e retorno. Peças/Drones devem abrir suas páginas de consulta.
3. Durante uma tentativa: Personagem → aguardar alguns segundos → Voltar. Conferir vida/inimigos preservados durante o menu e ausência de rajada de ações atrasadas ao retornar.
4. Com saldo suficiente: comprar ATQ e VIDA; conferir preço, ouro restante, atributo e barra de reforços. Na reabertura do app, comparar com o save.
5. Repetir depois de derrota/vitória; conferir que compra na base não inicia combate e que a caminhada volta na tentativa seguinte.
6. Alternar abas e abrir/fechar repetidamente; conferir seleção, feedback de pressão e ausência de animação duplicada. Verificar TMP acentuado e mensagens longas em retrato 16:9 e 20:9, recortes e área segura.
7. Gravar execução real antes de concluir qualidade visual/fluidez. Toque, alvos físicos, contraste no aparelho, memória, draw calls e FPS permanecem pendentes de medição.
