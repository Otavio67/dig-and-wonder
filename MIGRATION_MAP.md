# Dig & Wonder — mapa de migração Godot → Unity

**Inventário Fase 0 concluído; protótipo Fases 1–3 em andamento; 20/10/2026.** Fonte inspecionada: branch `prototype/first-playable`, commit de origem documentado na Fase 0, versão Godot 0.0.6. O usuário autorizou continuar após o inventário. Este mapa foi atualizado para registrar a conversão feita em `UnityProject/`.

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

Paridade conferida pelos testes de domínio e pela leitura dos catálogos Godot. A compilação real/importação Unity continua pendente porque o Editor não está instalado no ambiente remoto.

## 8. Decisões e pendências após a aprovação do inventário

1. **Fonte dos valores:** números com equivalente direto foram importados de Godot. Crítico, Crítico+ e custo de melhoria do Especial não existem no combate Godot ativo; ficam desativados. PV/recompensas dos bosses foram preservados, mas não estão balanceados para o novo loop.
2. **Broca/carrinho:** direção respondida pelo usuário — a broca é a protagonista persistente que avança sozinha; o carrinho não conduz o loop. O visual subterrâneo/paleta continua como inspiração. As sprites finais não foram montadas na UI Unity provisória.
3. **Progressão:** luta automática em fases, ouro por inimigo e boss como condição de avanço. Baús/chaves são concedidos pelo boss, mas a abertura e o catálogo de equipamentos ainda não estão no protótipo Unity. As abas estão preparadas como navegação inicial.
4. **Offline/save:** sem progresso de combate offline. O perfil Unity usa `Application.persistentDataPath`, arquivo temporário, checksum e backup. Uma tentativa ativa volta do último snapshot salvo. Não foi feita migração automática do save Godot.
5. **Alvos/colisores:** combate segue por IDs/eventos tipados, sem física determinar acertos; pooling é somente visual.
6. **Validação:** DOTween está versionado verbatim com licença. Unity Editor/CLI continua ausente e download oficial bloqueado por resposta 403; por isso batchmode, importação real, Play e APK permanecem pendentes.
7. **Android/configuração Unity:** retrato, Input System novo e cena de entrada são preparados pelo script do Editor. URP Renderer 2D e Cinemachine 3 estão no Package Manager, mas ainda não foram ativados/validados no Editor. APK e identidade final aguardam ferramenta e revisão.

### Direção de jogo definida pelo pedido após o inventário

O vídeo enviado (`WhatsApp Video 2026-10-20 at 00.22.57.mp4`) é uma **referência visual/de ritmo**, não documentação de regras para copiar. Nele se vê jogo mobile em retrato, combate e avanço automáticos em uma arena lateral, barras/indicadores de progresso, recompensas e uma área inferior dedicada a atributos e melhorias. A solicitação do usuário define a adaptação para Dig & Wonder:

- Loop principal: a broca caminha/avança sozinha; inimigos surgem e são combatidos automaticamente. O jogador investe recursos em atributos e equipamentos entre ações, sem controle de movimento manual.
- Fluxo de entrada: em um perfil novo, o botão Iniciar leva a broca para dentro da caverna. Se já houver progresso salvo, abrir o jogo retoma diretamente o último ponto seguro, conforme pedido anterior do usuário.
- Recompensas do loop: cada inimigo derrotado concede ouro; concluir uma fase concede um baú. A fonte Godot fornece valores reaproveitáveis quando houver equivalência, mas a tabela de inimigos/fases do novo loop ainda exige balanceamento rastreável. Como baús já foram definidos com chave, cada fase concluída deve prover ou acompanhar a chave necessária para não transformar a recompensa garantida em bloqueio arbitrário; drops extras podem ser opcionais.
- A interface reserva a parte inferior para menus e melhorias da broca, incluindo Vida, Dano, Especial e Crítico. Ataque rápido, defesa e recarga de especial são candidatos adicionais, sujeitos à paridade com os dados e à aprovação de balanceamento.
- A progressão principal é por fases/biomas, com inimigos progressivamente mais fortes e um boss como requisito para avançar. Coletáveis opcionais não bloqueiam o boss.
- Escavação fica como evento/fase especial, com padrão de perfuração e coleta de minério próprios; não substitui o loop principal de combate.
- Ideias para diferenciar: módulos instalados na própria broca (ponta, motor, núcleo), que mudam comportamento/ataque além de apenas aumentar número; eventos de escavação com objetivo curto; recompensas de baú por chaves obtidas jogando, com duplicatas convertidas em material de melhoria. São propostas, ainda não implementação ou balanceamento aprovado.
- Monetização, se adicionada depois, deve ser voluntária e fora do combate: por exemplo, recompensa opcional ao final de uma fase ou bônus de retorno offline. Nenhum anúncio interrompe luta ou é necessário para vencer. A viabilidade SDK/teste continua pendente da configuração Unity.
- Progresso offline é uma decisão separada: não aparece como regra confirmada do vídeo nem existe no combate Godot atual. Definir limite e fórmula antes de implementá-lo.
- Save/retomada: progresso usa armazenamento persistente do aplicativo, não cache temporário do sistema, com arquivo temporário, validação e backup. Salvar em compras, recompensa, equipamento, conclusão/checkpoint de fase e suspensão/retorno do app. O combate deve retomar de um checkpoint seguro sem duplicar ouro ou baús; renda offline continua opcional e ainda sem fórmula definida.
- Animação — recomendação técnica inicial: testar primeiro o pacote Unity 2D Animation (ossos, Sprite Skin e Animator) para o rig modular da broca, com DOTween somente para respostas pontuais de game feel. O projeto tem atlas raster e peças SVG, mas não um rig de personagem completo; as partes que se deformam/rodam precisam ser separadas e preparadas como arte antes de qualquer middleware.
- Spine 2D é uma opção se o padrão visual exigir deformação de malha, transições complexas e muitas skins/peças trocáveis. Para uma única protagonista mecânica, pode ser custo e ferramenta extras antes de sabermos que o rig nativo não basta. Se for escolhido, requer licença adequada para autoria/uso comercial e compatibilidade entre editor, export e runtime Unity; verificar termos/preço vigentes antes da compra. Nem Spine nem o pacote nativo resolvem arte sem peças/pivôs adequados.
- Amostra visual a comparar futuramente: movimento contínuo para a direita com rotação visível da ponta/esteira, idle sem pulsação geral, antecipação/ataque/impacto e recuperação, dano, derrota/vitória; `Logic`/colisor não se move por Tween. Fundo da caverna pode rolar em parallax enquanto a autoridade de posição permanece na lógica.

O jogo de referência não determina valores, taxas de loot, duração de fase ou frequência de anúncios. Esses dados serão rastreados à origem Godot ou explicitamente marcados como novos quando aprovados.

## Estado atual da migração

- **Fase 0:** inventário feito; mapa aprovado para prosseguir em conversa posterior.
- **Fase 1:** ScriptableObjects/importador, simulação, recompensas, upgrades e save com recuperação implementados. Dez verificações de domínio passam; fonte de paridade é testada.
- **Fase 2:** feedback base em DOTween e quatro pools pré-aquecidos; efeitos cosméticos não mudam a simulação e são cancelados em terminais.
- **Fase 3:** bootstrap retrato e HUD provisória em código, Input System/UI/TMP, área segura, menus, botão de início/retomada e compras Dano/Vida. Cena vazia e settings são gerados pelo Editor ao abrir.
- A compilação local contra stubs mínimos passou sem avisos, mas **não é importação/compilação Unity real**. Não houve revisão visual, gravação, execução no Unity nem teste Android nesta nuvem. Ainda falta ativar/configurar URP 2D e validar a câmera Cinemachine no Editor.
