# Dig & Wonder — mapa de migração Godot → Unity

**Fase 0 de inventário; 20/10/2026.** Fonte inspecionada: branch `prototype/first-playable`, commit `f1db0df`, versão Godot 0.0.6. Este documento propõe a estrutura Unity 6; não inicia a conversão nem altera regras ou conteúdo.

## 1. Diagnóstico do projeto de origem

- Godot 4.7.2, GDScript, renderer Compatibility, viewport lógico 540×960, orientação retrato, limite configurado de 60 FPS.
- Entrada principal: `main.tscn` → superfície de área segura → `Game`. A cena de combate não usa árvore de controles Godot para desenhar sua HUD: grande parte da tela e dos botões é desenhada e hit-tested em `game.gd`.
- A direção jogável atual é defesa automática por camadas, documentada em `docs/DESENVOLVIMENTO.md` e `docs/MARCO2.md`. O antigo jogo de grade continua em scripts/dados de compatibilidade, mas não é a cena de entrada.
- Há **36 scripts GDScript do projeto**, **1 script GDScript de teste**, **27 scripts do plugin AdMob**, **2 cenas `.tscn`**, **1 recurso `.tres`** e **8 catálogos JSON**. Não foram encontrados arquivos C#, `.csproj`, `.asmdef`, `Assets/`, `Packages/` ou `ProjectSettings/` de Unity; o executável/editor Unity não está instalado neste ambiente.
- `project.godot` não declara autoloads. Scripts com `class_name` (por exemplo `DigEngine`, `Economy`, `DrillSpec`, `EquipmentSystem`, `PhaseSystem` e `SaveStore`) são classes globais de script, não singletons com estado global.
- Não há sistema de física/colisão 2D ativo. Combate e seleção de alvos são resolvidos pela simulação determinística; projéteis são registros de dados e desenhos, não `Area2D`/`RigidBody2D`.
- Os principais números ativos vêm de `content/defense/prototype.json`; a maior parte das regras está implementada em `defense_run.gd`. Os recursos `biomes`, `drills`, `equipment`, `phases` e parte do save pertencem ao modo antigo/propostas, não ao combate atual.
- Não há áudio ou fontes próprias em `assets/`. Os ativos encontrados são raster PNG e SVG. A proveniência conhecida está em `assets/LICENCAS.md`; os desenhos gerados para o projeto não recebem por isso uma licença geral de terceiros.

## 2. Configuração, cenas, recursos e serviços globais

| Origem Godot | Função encontrada | Destino proposto na Unity |
| --- | --- | --- |
| `project.godot` | Entrada `main.tscn`, viewport retrato, stretch, renderer Compatibility, 60 FPS, ícone e configurações Android. Sem `[autoload]`. | `ProjectSettings`, URP com Renderer 2D, Player Settings e `Packages/manifest.json`. Novo Input System com `InputActionAsset`. Configurar CanvasScaler retrato e limite de frame como meta; nenhuma equivalência de FPS é afirmada. Bootstrap scene substitui autoloads inexistentes. |
| `main.tscn` | Raiz `DigAndWonder` aplica área segura; filho `Game` contém entrada, estado, combate desenhado, eventos, UI e rigs. | `Scenes/Gameplay.unity`: `GameBootstrap` e `GameplayRoot`, Canvas uGUI/TMP, câmera Orthographic/Cinemachine 3 e mundo de combate. Raiz `SafeAreaRoot` dimensiona o Canvas. Separar controllers e visuais conforme a seção 4. |
| `tests/android_ads/ads_test.tscn` | Tela independente de rewarded de teste; não é a cena principal nem oferece recompensa ao jogo. | `Scenes/Development/RewardedAdsTest.unity`, fora das cenas do jogador e dos builds de produção. Usar adapter de SDK separado, sem migrar plugin Godot. |
| `assets/ui/expedition_theme.tres` | Theme com quatro StyleBoxFlat, paleta, estados de botão/painel, foco e fonte base 16. | `Assets/_Game/UI/Theme` com TMP Settings, cores/estilos uGUI e componentes de botão. Preservar foco/acessibilidade no prefab; não converter o `.tres` literalmente. |
| `combat_event` em `game.gd`; sinais do plugin | Comunicação por sinais Godot entre simulação, apresentação e AdMob. Não são autoloads. | Eventos C# (`event Action<CombatEvent>` ou interface/event bus tipado); `UnityEvent` somente onde a cena precisa de ligação editorial. Callbacks de anúncios permanecem isolados. |
| `class_name` em scripts de progressão/save | Nomes globais do editor, sem ciclo de vida singleton. | Classes C# namespaced, serviços explicitamente injetados pelo bootstrap e `ScriptableObject` apenas para dados editáveis. Não criar singleton só por causa do `class_name`. |

### Cenas e prefabs planejados

| Origem/componente | Hierarquia-alvo planejada |
| --- | --- |
| `main.tscn` / `Game` | `GameplayRoot` (`GameBootstrap`, `CombatLoop`, `GameState`) → `World` (fundo, poço, terreno, unidade lógica) + `MainCamera` (`Camera`, `CinemachineCamera`) + `UIRoot` (`Canvas`, `SafeAreaFitter`) → HUD, cooldown, ouro, upgrades e painéis de início/resultado. |
| Carrinho atual | Se mantido no novo design: `CartRoot` (lógica/estado/collider somente se aprovado) → `Logic` + `Visual` (`SpriteRenderer`, Animator/DOTween e rigs separados para chassi/canhões). Hoje a posição é fixa e não há collider. |
| Broca Pioneira atual | `PioneerRoot` (lógica/vida/ataque/alvo e eventual collider) → `Logic` e `Visual` (carcaça, mola, ponta). Hoje a broca é uma unidade descartável de impacto, não o avatar persistente. |
| Inimigo atual | `EnemyRoot` (PV/temporização/alvo) → `Logic` + `Visual` (poses do Torrão). Identidade/forma das unidades do auto-battler precisam ser definidas. |
| Projéteis/efeitos | Prefabs registrados em `ObjectPool<T>` para projéteis, dano flutuante, moedas e partículas conforme o requisito. Na origem são registros de simulação/desenho, sem instâncias de física; não adicionar colisões que alterem a regra sem aprovação. |

## 3. Scripts Godot e equivalentes planejados

### Runtime e layout

| Arquivo | Responsabilidade atual | Equivalente Unity planejado |
| --- | --- | --- |
| `scripts/gameplay/game.gd` | Controlador da cena: tela/estado, InputEvent, loop fixo de 0,05 s, eventos, save, renderização da UI e do mundo, rigs. | Decompor em `GameBootstrap`, `GameFlowController`, `CombatLoop`, presenters e controladores uGUI. Evento de simulação alimenta feedback visual; UI não calcula combate. |
| `scripts/gameplay/defense_run.gd` | Simulação determinística de defesa, unidades, alvos por ID, cooldown, ondas, disparos, dano, ouro, melhorias e vitória/derrota. | `CombatSimulation`/`BattleController` em C#, com modelos/estados próprios e eventos. Manter ordem temporal e todos os valores da fonte escolhida; pooling apenas na representação visual. |
| `scripts/gameplay/defense_layout.gd` | Retângulos e hitboxes lógicas dos botões ativos. | Prefabs uGUI, `RectTransform`, layout groups, áreas de toque e `CanvasScaler`; números de posição são redesenhados para safe area, não copiados cegamente. |
| `scripts/gameplay/safe_surface.gd` | Ajusta viewport retrato à área segura Android e cancela input ao redimensionar/retomar. | `SafeAreaFitter` no Canvas, `Screen.safeArea`, pausa/cancelamento de `InputAction` e PlayMode/device tests. |
| `scripts/gameplay/ui_layout.gd` | Geometria da UI antiga de mineração, álbum, oficina, páginas e itens; não alimenta `main.tscn`. | Base de referência para Canvas/uGUI e TMP somente se a interface antiga for escolhida como base. |
| `scripts/gameplay/dig_engine.gd` | Grade lógica legada: durabilidade, camadas, padrão da broca, profundidade por fase e moedas. | `MiningSimulation` C# e definições de terreno/habilidade em `ScriptableObject`, somente se aprovado para o novo loop. |

### Progressão e saves

| Arquivo | Responsabilidade atual | Equivalente Unity planejado |
| --- | --- | --- |
| `scripts/progression/drill.gd` | Leitor/equipador do catálogo antigo; padrões de escavação; transformação para Prisma. | `DrillDefinition : ScriptableObject` (identidade, padrão/skill, aparência, raridade, custo); runtime `DrillLoadout`. Padrões convertem como offsets sem mudar sua semântica. |
| `scripts/progression/economy.gd` | Venda simples, teste de saldo e compra. | `EconomyService` C# puro com testes EditMode. |
| `scripts/progression/equipment_system.gd` | Bônus, chaves/baús, raridade com sorte/proteção, duplicatas viram moedas. Pertence ao catálogo/sistema antigo. | `EquipmentDefinition`, `DrillSkinDefinition`, `LootTableDefinition` e `ChestDefinition` como `ScriptableObject`; `LootService` com RNG explícito e testes. Ativar somente após decidir se equipamento/baús ficam no escopo. |
| `scripts/progression/phase_system.gd` | Sequência normal/surpresa, indexação por profundidade e seleção determinística a partir de seed. Fases e boss não correspondem ao combate de entrada atual. | `PhaseDefinition`/`BiomeDefinition : ScriptableObject` e `ProgressionService`; preservar algoritmo/seed se escolhido. |
| `scripts/save/defense_save.gd` | Save ativo schema 6: ouro, Dano/Vida, vitórias/desbloqueio; backup/temporário/rename e importação confirmada de perfil antigo. | `SaveRepository` C#, DTO JSON versionado em `Application.persistentDataPath`, gravação atômica/backup e migração explícita. Não mapear o arquivo de save Godot como `ScriptableObject`. |
| `scripts/save/save_store.gd` | Leitor e migrações 1–5 do jogo de grade; guarda escavação, moedas, descoberta, equipamento, chaves/baús, fase/boss. | `LegacySaveImporter` de desenvolvimento para migração de dados, se o usuário quiser preservar saves. O runtime novo terá schema separado; não descartar original. |

### Apresentação/feel

| Arquivo | Responsabilidade atual | Equivalente Unity planejado |
| --- | --- | --- |
| `scripts/visual/art_direction.gd` | Paleta/fundos, mistura de 5 ambientes, parallax, névoa, placa e estados de botão. | Sprites e Materials URP 2D; `BackgroundPresenter`, `DOTween` e UI Theme/TMP. Parallax somente na cenografia. |
| `scripts/visual/terrain_art.gd` | Seleção de material/variante por coordenadas sem RNG e fissuras de dano. | `TerrainRenderer` por SpriteRenderer/Tilemap ou segmentos; mantém seleção determinística e `ScriptableObject` de material. |
| `scripts/visual/cart_rig.gd`, `scripts/visual/cannon_rig.gd` | Rig visual do carrinho, idle/derrota no AnimationPlayer, mira e Tween de recuo/clarão por canhão, hit flash. | Prefab `CartRoot/Logic/Visual`; `ProceduralAnimator2D`, child pivots para cada canhão e DOTween por evento real. |
| `scripts/visual/pioneer_rig.gd` | Rig visual da Pioneira; fase do ciclo deriva da idade da unidade/simulação; Tween do anel de impacto. | Prefab `PioneerRoot/Logic/Visual`; `ProceduralAnimator2D` + sequência DOTween/Animator sincronizada a eventos C#. |
| `scripts/visual/expedition_actor.gd` | Rigs por partes de Lia/Bento/broca para a interface anterior; idle, piscar e respostas de mineração. | Prefabs/modular sprites, rigs locais, DOTween; permanecer como recursos não usados até definição de personagem. |
| `scripts/visual/expedition_visuals.gd` | Posiciona equipe/broca em início, mineração, personagem e oficina; Lia/Bento escondidos. | `CharacterStagePresenter`/prefab de showcase; fora de cena se o jogo novo mantiver apenas a broca. |
| `scripts/visual/model_details.gd` | Silhuetas/acessórios decorativos por modelo de broca. | Prefab modular de acessórios ou `DrillVisualDefinition`; sem efeito em regras. |

### Testes/captura/dev tools

| Arquivos | O que cobrem hoje | Equivalente Unity planejado |
| --- | --- | --- |
| `scripts/gameplay/logic_harness.gd`, `defense_harness.gd`, `game_harness.gd`, `android_harness.gd` | Economia/padrões/save legados/loot; combate; integração e pausa; input transformado/safe area. | EditMode tests para regras/dados/save e PlayMode tests para cena/Input/safe area. Não portar harness GDScript como código runtime. |
| `scripts/save/save_harness.gd`, `defense_save_harness.gd` | Migração, validação, temporário, backup e recuperação dos dois formatos. | EditMode tests + fixtures JSON isoladas para `SaveRepository` e importer. |
| `scripts/visual/animation_harness.gd`, `characters_harness.gd` | Ordem/interrupção de animação, Tweens limitados, pivôs, pausa, RNG e rigs existentes. | PlayMode/EditMode tests de `ProceduralAnimator2D`, pool cleanup, pause/time-scale e eventos. |
| `scripts/capture_defense.gd`, `capture_marco2.gd`, `capture_review.gd`, `capture_characters.gd`, `capture_animation_sample.gd` | Gravações/capturas da cena principal com persistência desativada e fixtures declaradas. | Ferramentas Editor/PlayMode de screenshot/capture, nunca incluídas no build normal. |
| `scripts/capture_environments.gd`, `capture_priority.gd`, `capture_visual.gd` | Entradas de compatibilidade que encaminham para captura existente. | Não portar; remover depois de garantir que pipelines equivalentes foram substituídos. |
| `scripts/run-harnesses.sh`, `check-project.sh`, `check-project.ps1` | Checks de importação/boot, harnesses e isolamento de saves. | CI Unity batchmode + Unity Test Framework. O comando de compilação obrigatório depende de Unity Editor instalado. |
| `scripts/setup-android.sh`, `export-android-debug.sh`, `export-android-ad-test.sh`, `verify-android-apk.py`, `capture-animation-sample.sh`, `setup-cloud.sh` | Instalação/export Godot, APK de teste, checagem estática e ferramentas da nuvem. | Substituir por Build Profiles/Android Gradle de Unity, scripts de assinatura de debug e validação de artefato; não portar scripts Godot. |
| `tests/android_ads/ads_test.gd` | Carrega/exibe rewarded de teste e só conta callback de recompensa. | Componente de cena técnica `RewardedAdsTestPanel` e `IRewardedAdsService` isolado; usar IDs oficiais de teste. Sem integração ao loop do jogo nesta fase. |

### Plugin de terceiro

O diretório `addons/AdmobPlugin/` (plugin 7.0, MIT) contém `Admob.gd`, `AdmobPlugin.gd`, `InlineAdaptiveBanner.gd`, `config/{AdCache,AdmobAndroidExportConfig,AdmobExportConfig,AdmobIosExportConfig}.gd` e os DTOs `model/{AdError,AdInfo,AdSize,AdapterResponseInfo,AdapterStatus,AdmobConfig,AdmobSettings,ConsentRequestParameters,FormError,InitializationStatus,LoadAdError,LoadAdRequest,MediationNetwork,NetworkExtras,NetworkPrivacySettings,ResponseInfo,RewardItem,UserConsent}.gd`. `addons/GMPShared/` contém `GmpLogger.gd` e `SpmDependency.gd`; o plugin inclui AARs debug/release.

**Plano:** não traduzir 27 scripts/AARs para C#. Usar um SDK/adapter de anúncio compatível com Unity, manter rewarded em cena de teste isolada e obter/confirmar a versão/licença do pacote antes de incluir. A integração não pertence ao loop de combate. Cinemachine 3, Input System, TMP e URP 2D serão dependências pelo Package Manager; DOTween não está no repositório e requer fonte/importação/licença. Nenhuma dependência está instalada porque ainda não há projeto Unity.

## 4. Catálogos, recursos e conteúdo

Não há conjunto de `.tres` de balanceamento: o único `.tres` é o Theme de UI. Dados de gameplay ficam em oito JSON; partes dos defaults/equações ficam em GDScript. Proposta de conversão para `ScriptableObject`:

| Recurso Godot | Destino Unity planejado | Estado na origem |
| --- | --- | --- |
| `content/defense/prototype.json` | `DefensePrototypeDefinition` com fase, terreno ativo, carrinho, canhões, Pioneira, Escavador, Torrão, upgrades/limites. | Fonte dos números do combate ativo. |
| `content/defense/abilities.json` | `AbilityDefinition` por habilidade, validando `implemented`. | Pioneira e Escavador ativos na fase atual; outras linhas são propostas e não devem virar jogáveis automaticamente. |
| `content/biomes/prado_cristalino.json` | `BiomeDefinition` + materiais/recompensa e descoberta. | Catálogo da mineração manual antiga. |
| `content/phases/catalog.json` | `PhaseDefinition`, `BiomeDefinition`, tabela de fase surpresa/encounter. | 3 fases normais, uma surpresa, inimigos e bosses catalogados; não são a fase ativa da cena principal. |
| `content/discoveries/catalog.json` | `DiscoveryDefinition`. | Catálogo pequeno do jogo antigo. |
| `content/drills/catalog.json` | `DrillDefinition`, com offsets de escavação, cor, custo e raridade. | 6 padrões diferentes; legado/inativo na cena de defesa. |
| `content/drills/skins.json` | `DrillSkinDefinition` + passivos/descrições. | 13 aparências (inicial + 12); catálogo antigo. |
| `content/equipment/catalog.json` | `EquipmentDefinition` e enums/valores de raridade/slot. | 60 itens (10 por raridade) em sistema antigo. |
| `assets/ui/expedition_theme.tres` | uGUI/TMP Theme + ScriptableObject de paleta se útil. | Estilo visual, não balanceamento. |

Conversão proposta é **dados fiéis com rastreio de origem**; ScriptableObjects novos ficam em pastas por domínio (`Assets/_Game/Data/Combat`, `Drills`, `Equipment`, `Phases`, `Visuals`). Não marcar conteúdo proposto como ativo sem decisão. JSON fonte/arquivo de save continuam preservados durante migração.

## 5. Ativos visuais inspecionados

| Pasta/arquivos | Conteúdo e estado | Importação Unity proposta |
| --- | --- | --- |
| `assets/ilustracoes/maquina-baus-atlas.png` | Atlas pintado com carrinho, broca/ponta e baú fechado/aberto. Usado por rigs atuais; atlas inteiro não é um rig articulado. | Manter como textura fonte; slice em Sprite Editor com pivôs revisados. Não inferir articulações inexistentes; separar/redesenhar peças para animação após aprovação. |
| `assets/terreno/materiais-atlas.png`, `assets/terreno/solo-torroes-raizes.png` | 12 variações de quatro famílias de material e faixa contínua de solo/raízes. Usados por poço/terreno atual ou helper. | Sprite atlas/textures com Compression/Filter e Pixels Per Unit a definir; URP 2D. |
| `assets/ambientes/ambientes-atlas.png`, `intermediario.png`, `proximo.png` | Cinco fundos em atlas e duas camadas transparentes, com centro de jogo livre. | Slices de background/SpriteRenderer, camadas de parallax e camera; perfil de textura móvel a definir. |
| `assets/characters/torrao-poses.png`, `escavador-poses.png` | Atlas com 4 poses de inimigo e 3 de unidade de apoio. Usados no runtime de defesa. | Slice Sprite Mode Multiple; clip Animator ou prefab de pose; `Visual` separado de `Logic`. |
| `assets/characters/drill_companion.png`, `miner_lia.png`, `miner_bento.png` | Ilustrações raster do sistema anterior/legadas; não são carregadas pela entrada ativa atual. | Manter como arquivo legado; só importar para catálogo de arte se a identidade/unidades forem confirmadas. |
| `assets/personagens/{lia_braco,lia_cabeca,lia_corpo,bento_braco,bento_cabeca,bento_corpo,broca_corpo,broca_nucleo,broca_ponta,chave,cristal}.svg` | 11 peças vetoriais modulares para rigs antigos. | SVG → Sprite/texture ou raster de alta resolução; pivôs e filtragem a definir; não converter para mesh sem razão. |
| `assets/ui/{album,bag,bussola,chest,coin,drill,gear,key,lock,picareta,up}.svg` | 11 ícones SVG. `coin`/`lock` aparecem no fluxo de defesa; outros pertencem à UI antiga. | UI Sprite Assets; mapeamento/seleção depende da UI escolhida. |
| `assets/dig_wonder_icon.svg` | Ícone de aplicação. | Ícone Android/Player Settings (slices raster por plataforma se exigido). |
| `assets/LICENCAS.md` | Proveniência dos ativos gerados e distinção de arquivos legados. | Preservar no Git; atribuições/licenças dos pacotes Unity serão adicionadas separadamente. |
| Arquivos `*.import` | Import settings gerados pelo Godot, ao lado dos ativos. | Não copiar como metadata Unity; Unity cria `.meta` próprios e deve manter GUIDs após importar. |

Inspeção visual: paleta petróleo/turquesa/latão/coral; arte pintada com textura e contornos; atlas de máquina e inimigos são raster. Os desenhos não separam todas as articulações necessárias para uma animação convincente. Conversão de engine preserva pixels, não melhora a arte automaticamente.

## 6. Sinais, tempo, pooling, câmera e colisões

- `defense_run.gd` usa `step(0.05)` com acumulador máximo; eventos incluem launch, cannon, hit, strike, gold, purchase, defeat/victory. C# pode publicar eventos tipados e UI/Visual assinar sem passar autoridade de gameplay aos Tweens.
- Input hoje é toque/mouse/tecla e cálculo de retângulos manual. `InputActionAsset` e EventSystem/`InputSystemUIInputModule` são a equivalência prevista; cancelar toque, múltiplos toques e área segura têm cobertura lógica existente.
- Tween Godot responde a eventos; não altera estado lógico. Em Unity, `ProceduralAnimator2D` atua no `Visual`; feedback que deve sobreviver a `Time.timeScale` usa `SetUpdate(true)`. Combate ainda não tem hit stop nem `Time.timeScale`; a política e a escala da pausa devem ser escolhidas no desenho do loop.
- Não existem colisores Godot nem resolução física. Lógica usa IDs, posições e janela horizontal. Campo `Logic/Collider` pedido para Unity ainda não tem formato/regra equivalente: decidir se colliders serão apenas sensores/apresentação ou se substituirão testes geométricos da simulação.
- A mira/câmera atuais são fixas, sem camera follow. Cinemachine 3 pode controlar framing/impulse, mas deve ficar desacoplado de alvos e valores de combate.
- Projéteis, dano, ouro e partículas serão instanciados de pools `UnityEngine.Pool.ObjectPool<T>`; reutilização fica no lado visual e resultados de dano/recompensa continuam derivados da simulação. Limites ativos Godot: 24 projéteis, 32 efeitos visuais, 4 inimigos e 2 unidades.

## 7. Fontes numéricas e paridade a preservar

Esta é uma **referência de origem para a futura Fase 1**, não um resultado de paridade (Unity ainda não existe). Para evitar misturar conteúdo ativo com legado:

| Número/regra de origem | Fonte e valor lido | Estado Unity na Fase 0 |
| --- | --- | --- |
| Fase/solo ativos | `prototype.json`: 6 camadas; vida `44 + 8 × índice`; ouro `12 + 2 × índice`; materiais terra/terra/argila/pedra/pedra/cristal; bônus inicial por tentativa 2 ouro. | — não implementado; a base de gameplay ainda precisa ser escolhida. |
| Carrinho/canhões | 120 PV; 2 canhões, 6 dano cada e 1,25 s de intervalo; timer inicial do lado direito 0,28 s. | — |
| Pioneira | 32 dano, voo 0,8 s, cooldown 3 s, 4 golpes, 0,7 s entre contatos, máximo 2 simultâneas; não defende. | — |
| Escavador | 9 dano, 45 PV, voo 0,8 s, duração ativa 10 s, cooldown 20 s, ataque a cada 1 s, limite 1. | — |
| Torrão | 48 PV, 8 dano, primeiro ataque em 1,7 s, intervalo 2,8 s, telegraph 0,65 s, projétil 0,65 s, 7 ouro. | — |
| Upgrades/limites | Dano: custo `18 + 14 × nível anterior`, +12% base por nível, máximo 10. Vida: `20 + 16 × nível anterior`, +35 PV por nível, máximo 10. 4 inimigos, 24 projéteis, 2 unidades, fila 8. Simulação 0,05 s. | — |
| Economia antiga | `economy.gd`, bioma e catálogos: moedas por célula/material; padrões de 1/3/5/8 offsets; baú consome 1 chave e 1 baú, taxas base [28,36,22,9,3.5,1.5], garantia de épico após 4 falhas e lendário após 11; duplicata converte por raridade. | — legado/inativo no gameplay atual; não combinar sem decisão. |
| Fases/bosses antigos | `phases/catalog.json`: ciclos, chance surpresa 22%, materiais/vida/recompensas, posições de chave/baú e bosses com PV/ouro próprios. Não corresponde à fase ativa de defesa. | — dados catalogados, implementação atual não ativa essas fases/bosses. |

No fechamento da Fase 1 a tabela exigida deve trazer, para cada valor efetivamente ativado, `origem Godot`, `Unity`, `igual/diferente` e justificativa. Nesta fase nenhum valor foi convertido ou conferido em runtime.

## 8. Ambiguidades para aprovação antes da Fase 1

1. **Qual gameplay e números são a fonte de verdade?** A entrada Godot 0.0.6 é defesa por camadas/carrinho/Pioneira/Torrão. O modo de escavação contínua, padrões de broca, equipamento, chaves/baús e fases catalogadas são legados/inativos. O objetivo novo “Idle RPG / Auto-Battler” pede um loop diferente. A Fase 1 deve partir dos números de `prototype.json`, dos valores antigos de mineração, ou de uma lista sua de quais grupos permanecem ativos?
2. **Papel da broca:** na defesa atual a Pioneira é um projétil/unidade descartável com 4 impactos; no jogo antigo ela é protagonista persistente de uma grade. No Auto-Battler ela deve ser o avatar persistente que ataca automaticamente? O carrinho permanece, vira base fora da arena ou sai por completo?
3. **Escopo visual/unidades:** “manter apenas a broca e o visual” significa manter a identidade (paleta/fundos/terreno/UI/atlas) e substituir personagens, ou manter também carrinho, Torrão e Escavador? `Lia/Bento` estão ocultos; sprites legacy diferem dos atlas pintados atuais.
4. **Progressão ativa:** a entrada atual tem ouro e upgrades permanentes de Dano/Vida; sistema antigo tem moedas, 60 equipamentos, skins, 6 brocas, chaves/baús e descoberta. Quais desses sistemas entram no novo loop e quais devem ser apenas dados disponíveis?
5. **Inatividade/offline:** “Idle RPG” pode significar luta automática apenas com app aberto ou avanço/recompensa offline. Godot atual não calcula renda/combate offline. Definir isso antes de criar valores, save e comportamento de retomada.
6. **Save dos jogadores:** preservar/importar o perfil de defesa schema 6, o save de mineração schemas 1–5, ambos, ou começar um save Unity vazio? Os saves reais do usuário não estão nesta cópia do repositório.
7. **Colisores:** nenhum collider existe na origem; alvos usam posição/IDs. Confirmar se os colliders Unity necessários serão hurtboxes para consultas/FX sem governar o combate, ou se física passa a determinar hits.
8. **Ambiente de compilação:** o projeto remoto não tem Unity Editor/CLI e não tem projeto Unity. Como a validação exige Unity 6 em batchmode a cada fase, o editor precisa estar disponível neste ambiente ou deve ser compilado no PC do usuário/runner. Também será necessário obter/importar DOTween; ele não está versionado.
9. **Alvo Android:** preservar pacote `com.otavio67.digandwonder`, ARM64, retrato, API mínima 24/alvo 36 e modo offline do preset atual? Unity/URP e SDKs podem alterar manifesto e requisitos; resolver isso antes da Fase 3.

## Situação da Fase 0

Leituras feitas: `project.godot`, as duas cenas, o único `.tres`, todos os JSON de conteúdo, scripts do projeto/teste, inventário da dependência AdMob, documentação de regras/proveniência e ativos visuais principais. Nenhum `.cs` foi criado, nenhum número foi alterado, nenhuma versão/save foi convertido, nenhum Unity batchmode foi executado. A Fase 1 depende da aprovação deste mapa e das decisões acima.
