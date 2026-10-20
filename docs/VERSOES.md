# Versões do Dig & Wonder

Versões seguem `0.0.patch` durante o protótipo. Cada checkpoint recebe tag Git `v<versão>`; ajustes locais são agrupados até outro marco verificável. Na direção Unity ativa, o número fica em `VERSION`, `UnityProject/GAME_VERSION.txt` e nas configurações de Player. `project.godot` permanece como histórico.

## 0.0.1 — Baús e oficina

- Chaves e baús grátis encontrados em marcos de profundidade, sem expiração.
- Catálogo de seis brocas com padrões diferentes e seis equipamentos passivos.
- Seis raridades, chance crescente com profundidade, proteção de sorte e conversão de repetidos em ouro.
- Oficina permite consultar efeitos e equipar achados.
- Save esquema 2 com migração da versão 1.
- Sem compras de baú ou anúncio para chaves/abertura.

Tag: `v0.0.1` (criada junto ao checkpoint Git deste marco).

## 0.0.2 — companheira-broca e coleção ampliada

- HUD separado em Baús, Equipamentos e Personagem.
- 60 equipamentos, dez em cada raridade: Ruim, Boa, Épica, Lendária, Única e Mística.
- Quatro atributos funcionais: poder, moedas por bloco, velocidade de escavação e sorte nos baús; itens podem ter contrapartidas.
- A broca é a personagem, com rosto, seis modelos/padrões de escavação e 13 skins com passivas próprias.
- Save esquema 3 guarda skins e migra os esquemas 1 e 2.
- Harnesses de catálogo, sorte, passivas, menus e migração executados com Godot 4.7.2.

Tag: `v0.0.2`.

## 0.0.3 — fases, minérios e chefes

- Fases finitas de 24 trechos com paleta, terreno e minérios exclusivos definidos em `content/phases/catalog.json`.
- Três regiões regulares em ciclo e Fenda Aurora surpresa (22% de chance ao derrotar um chefe); fase sorteada fica persistida.
- Um chefe diferente por fase bloqueia a descida; tocar e segurar ataca, e derrotá-lo paga moedas e abre a fase seguinte.
- Chaves e baús aparecem em blocos marcados para exploração, com limite por fase (2 de cada em regiões normais, 3 na Fenda), sem exigir coleta para vencer.
- Descobertas exclusivas por ambiente; save esquema 5 migra saves antigos para uma fase próxima da profundidade já alcançada.
- Harnesses verificam limite do terreno, limites de recompensas, combate, transição, sorteio e save.

Tag: `v0.0.3`.

## Build Android de teste — 0.0.3 (20/10/2026)

- Toolchain Linux preparada por `scripts/setup-android.sh`; exportação repetível por `scripts/export-android-debug.sh`.
- Preset `Android teste`, ícone provisório cartoon e compressão ETC2/ASTC para Android.
- APK ARM64 assinado para depuração, validado pelo `apksigner`; manifesto confirma API mínima 24 e alvo 36.
- O APK não integra anúncios. ADB funciona no container, mas não há dispositivo conectado; instalação, toque e desempenho seguem pendentes de aparelho.

## 0.0.4 — revisão Android e resposta ao toque

- Análise da documentação oficial Google/Android com decisões e pendências em `REVISAO-ANDROID.md`.
- Área segura e proporção preservada em resize, input convertido para o painel escalado e conteúdo recortado nas margens.
- Corrigidos Vender/Melhorar; controles maiores, equipamentos e modelos/skins paginados, painéis arredondados e texto centralizado/quebrado em linhas.
- Um toque ativo por vez, cancelamento por perda de foco, pausa/retomada e navegação Voltar/Escape.
- Autosave de escavação agrupado em até um segundo; liberação de toque e ações importantes gravam imediatamente. Save temporário validado e substituição sem remover antes o principal, com backup e recuperação de temporário.
- APK debug 0.0.4 (versionCode 4); verificador de manifesto, assinatura, integridade e alinhamento nativo/ZIP 16 KB. Harnesses e capturas fora do pacote.
- Importação/boot, harnesses e capturas reais desktop por Xvfb/Mesa revisados. Execução Android, acessibilidade completa, anúncios e FPS dependem de validação no aparelho.

Tag: `v0.0.4` no checkpoint desta revisão.

## 0.0.5 — preparação técnica e prova de anúncio

- APK principal 0.0.5 continua offline, sem permissão de internet e sem dependência de anúncios.
- Plug-in Android AdMob MIT v7.0 separado em `addons/`; teste isolado usa app/unit IDs oficiais de demonstração do Google.
- Cena técnica com recompensa concedida somente pelo callback do SDK, sem alterar a economia, menus ou fluxo principal.
- APK ad-test separado, com Gradle customizado, permissão de internet e verificações de assinatura, alinhamento, manifesto, SDK e integridade.
- Teste de importação, boot e harnesses preservados; exportações estáticas executadas. Sem aparelho nem KVM, exibição de anúncio, interação Android e FPS não foram medidos.

Tag: `v0.0.5` no checkpoint deste marco.

## Revisão local de sensação — após 0.0.5

- Lascas geradas ao atingir ou quebrar uma célula herdam a cor do material, giram durante a queda e continuam limitadas a 26 partículas ativas; a broca tem um recuo visual breve. Dano, intervalo de escavação, economia e save não mudaram.
- Capturas reais do Godot 4.7.2 em Windows: início, escavação e menus em `builds/review/` (saída local ignorada pelo Git), mais uma captura durante a quebra de bloco.
- Importação, boot e harnesses de lógica, gameplay e save passaram. O harness Android falhou na asserção de toque em `y=229`: a grade está em `y=300` após a alteração local de layout. FPS Android, toque em aparelho e acessibilidade não foram medidos.

Revisão visual/feedback sem nova versão ou tag.

## Revisão dos mineiros — 20/10/2026

- Lia e Bento redesenhados com proporções compactas, rostos maiores, contornos escuros e roupas simplificadas para os tamanhos usados no jogo. Mantidos os caminhos `assets/characters/miner_lia.png` e `assets/characters/miner_bento.png` e a transparência PNG.
- Gerados com a ferramenta integrada de imagens. Direção dos prompts: cartoon 2D com aparência vetorial, cabeça grande, cores sólidas, sombras definidas, poucos detalhes e fundo transparente; Lia segura um cristal, Bento acena e segura uma chave de oficina. Os originais anteriores ficam apenas no backup local ignorado em `.tools/art-backup/miners-20261020/`.
- Importação e sete capturas reais no Godot 4.7.2 Windows concluídas; início, escavação e personagem inspecionados. A revisão é visual, sem nova versão/tag. Não foram medidos FPS ou toque em Android.
- Os arquivos continuam sendo poses estáticas inteiras. Animação por esqueleto no Blender depende de preparar peças separadas ou um modelo; ainda não existe rig ou animação exportada.


## Etapa visual 1 — equipe animada (20/10/2026)

Lia, Bento e a broca-companheira usam 11 peças SVG originais em `assets/personagens/`, com silhuetas arredondadas, base/sombra, contorno escuro e reflexos pequenos. Os PNGs anteriores ficam como legado; não são carregados pelo jogo.

Os rigs `Node2D` em `scripts/visual/` usam somente Tween: respiração, piscadas em intervalos distintos, balanço da broca, braço/ferramenta sincronizado ao dano real, ponta em movimento durante a escavação e squash/stretch somente na quebra. A animação atua em pivôs locais, pausa sem foco e não consome aleatoriedade da progressão. Início, mina e personagem reutilizam a mesma equipe.

Validação no Godot 4.7.2 Windows: importação, boot e harnesses de lógica, gameplay, save, interface Android e personagens passaram. Os testes de toque agora usam `Layout.WORLD_TOP`, sem coordenada antiga fixa. O novo harness verifica eventos reais, célula vazia, dano versus quebra, pivôs, pausa, quantidade limitada de Tweens, RNG e esquema de save. O Git Bash foi bloqueado pelo sandbox (`NtCreateDirectoryObject`, 0xC0000022); os comandos equivalentes do `check-project.sh` e os harnesses foram executados diretamente com o Godot Windows.

Evidências locais ignoradas pelo Git: `builds/review/etapa1-20261020/antes/`, `depois/`, `depois/sequencia/` (96 quadros reais, 12 fps), `depois/personagens-animados.gif` e `depois/personagens-detalhe.gif`. A captura é desktop em viewport 540×960; não mede FPS, toque ou memória em Android. Para reproduzir a sequência, execute Godot com `--path . --fixed-fps 12 --audio-driver Dummy --script res://scripts/capture_characters.gd` usando dados de usuário isolados.

Esta etapa não redesenha blocos nem interface. A aparência destes elementos ainda limita a unidade visual. Próxima etapa, após aprovação: materiais dos blocos (etapa 2). A versão publicada continua 0.0.5, sem novo APK ou tag.


## Direção de arte aprovada — integração de 20/10/2026

A direção em `docs/DIRECAO-DE-ARTE.md` foi aprovada pelo usuário e recebeu uma correção de estilo durante a execução: fundos pintados, sem contorno, menos saturados, com textura sutil de pincel e sem aparência plástica. A revisão é uma integração artística para avaliação no jogo, sem nova versão/tag ou APK.

- Broca como único protagonista visível; Lia e Bento continuam nos arquivos/rigs, ocultos e sem receber eventos de escavação. A cabine pintada tem visor animado, ponta independente e silhuetas decorativas dos seis modelos existentes.
- Cinco tratamentos de ambiente em um atlas de fundo, mais duas camadas transparentes ilustradas. Parallax só na cenografia; transição de 1,5 s entre tratamentos e névoa com borda suave. As faixas visuais usam profundidade já existente; Fenda Aurora acompanha a região especial existente. Não são regiões novas.
- Terreno ilustrado com três variantes por família de material, escolha determinística por coordenada, cores moduladas pelo catálogo existente e fissuras sobre o dano real. Nenhum consumo adicional do RNG de geração/recompensa.
- Placas de metal com rebites e relevo, ações principais coral, secundárias latão, estados pressionado/desabilitado e ícones próprios. Telas existentes reorganizadas; controles continuam nas mesmas rotinas, com os harnesses usando o layout vigente.
- Perfuração, impacto visual curto, partículas de material, número da venda calculado a partir do valor real, reação de melhoria e poses fechada/aberta do baú. Confete limitado a 16 traços para Lendária/Única/Mística após abertura real. Falas ocasionais por texto de rádio, sem renderizar os mineiros e com intervalo de 30 s.

### Evidência e validação

Capturas reais: `builds/review/aprovada-20261020/antes/` e `depois/`; GIF `depois/jogo-em-movimento.gif`, com 144 quadros em 12 fps. A gravação demonstra compra real da Prisma e abertura real de um baú. As imagens `ambiente-*.png` são fixtures visuais isoladas das cinco faixas; não comprovam progressão conquistada pelo jogador. Tudo é **execução desktop em viewport de celular** (540×960), além da captura em paisagem com área segura preservada. As saídas ficam locais, ignoradas pelo Git e excluídas do APK.

Importação, boot e harnesses de lógica, gameplay, save, interface e personagens executados no Godot 4.7.2 Windows. O harness de personagens foi adequado à retirada dos mineiros da tela e verifica que permanecem ocultos, além de eventos reais, RNG, pivôs, foco e limite de Tweens. O Bash permaneceu indisponível pelo bloqueio de sandbox já registrado; os comandos equivalentes foram executados diretamente no Windows.

### Limites desta revisão

A avaliação artística final depende da revisão das capturas pelo usuário. Os fundos intermediário/próximo são reutilizados entre faixas; as seis brocas ainda compartilham a carenagem principal e recebem acessórios próprios. Os cartões pequenos podem exigir mais simplificação para melhorar a leitura. O foco está definido no Theme, mas navegação por teclado ainda requer integração própria. Tamanho em dp, toque, FPS, memória, consumo, anúncios e APK não foram validados em aparelho. Nenhuma alteração de balanceamento, formato de save ou fluxo de anúncios foi feita.

## Marco 1 — defesa com escavação (20/10/2026)

- Mudança de direção autorizada: grade manual substituída na entrada principal por carrinho fixo, dois canhões automáticos, uma camada compartilhada e Pioneira por cooldown, sem custo por lançamento. Seis camadas e encontro final de Torrões; nenhum boss.
- Ouro por evento único; melhorias permanentes limitadas de dano/vida; derrota terminal com limpeza completa e reinício preservando carteira/compras. Cooldown integral ao recomeçar ou reabrir, sem renda offline.
- Perfil esquema 6 em arquivo separado. Importação dos esquemas 1–5 depende de confirmação; cópia integral verificada antes da conversão de moedas+mochila 1:1 e reforços para Dano até nível 10. Demais itens ficam arquivados, sem conversão inventada, e o original permanece intacto.
- Arte, paleta, atlas, área segura, placas e infraestrutura Godot reaproveitados. Canhões e Torrão são placeholders coerentes; catálogos completos e boss permanecem planejamento identificado.
- Check Windows nativo, importação/boot e harnesses de combate, save novo/antigo, integração, input e rigs preservados. Ferramentas de captura antigas encaminham para o jogo atual. Documentação antiga arquivada e direção atual consolidada em DESENVOLVIMENTO/README.
- Capturas e gravação reais desktop em `builds/review/marco1-20261020/`; vinte segundos em 12 fps, execução normal. A mesma tentativa vence por lançamentos/compras acessíveis, outra perde por inatividade e reinicia sem retirar o permanente. Captura não grava no perfil do jogador.

Checkpoint sem nova versão/tag/APK. Não foram medidos toque, dp, FPS, memória, anúncios ou execução Android. Próximo marco, após aprovação: clareza/balanceamento e uma unidade alternativa.

## 20/10/2026 — revisão do Marco 1 e Marco 2
Solo/poço, feedback de Torrão/ouro, comparação de melhorias, espaços bloqueados. Exceção solicitada de 2 ouro no primeiro impacto por tentativa. Pioneira de quatro golpes com mesmos 32 totais, salto e quebra; Torrão em quatro poses; somente o Escavador adicional, gratuito nesta fase de protótipo para manter save/economia. Sem novo número de versão, APK ou tag. Ver MARCO2.md e CONTINUAR-NA-NUVEM.md.

## 0.0.6 — amostra de animações do carrinho e da Pioneira (20/10/2026)

- Chassi com idle sutil, resposta ao dano e pose de derrota prioritária; dois canhões com tubos/pivôs e recuos/clarões independentes ligados aos disparos reais.
- Pioneira separada em carcaça, mola e ponta; giro no voo, ciclo de preparação/impacto/recuperação sincronizado à idade da unidade e anel de impacto ligado aos contatos da simulação.
- Interrupção e limpeza em morte, reinício, pausa e remoção. Nenhuma alteração de dano, cooldown, colisão, save ou comportamento de combate.
- Harnesses e gravação contínua desktop de 9,6 s em `docs/evidencias/20261020/`. Gravação acelerada somente por fixture local; não mede desempenho. Sem teste em aparelho ou novo APK.

Tag: `v0.0.6` no checkpoint deste marco.

## 0.0.7 — início do protótipo Unity 6 (20/10/2026)

- Adicionados importador dos dados Godot e ScriptableObjects para combate automático por fases, upgrades, inimigos e save Unity com checksum/backup/retomada.
- HUD móvel provisória em português com iniciar/retomar, tabs inferiores, compras Dano/Vida, caverna rolando e feedback pooled com DOTween.
- Valores de combate com equivalência direta preservados; melhorias sem fonte Godot continuam desligadas.
- Checks de domínio e compilação contra stubs Unity passam. Não houve Unity Editor, importação real, gravação, APK, revisão visual ou medição Android neste checkpoint.

Tag: `v0.0.7`. O projeto Unity e seus limites estão em [UnityProject/README.md](../UnityProject/README.md).

## Preparação da nuvem — após 0.0.7 (20/10/2026)

- Unity Editor/CLI 6000.0.23f1 + Android instalados na instância remota a partir da imagem GameCI fixada por digest, com SHA-256 validado. .NET 8, Java, ADB, aapt2 e compilação C para ARM64 verificados.
- Instalador e comandos de ambiente/check versionados. TMP/uGUI, versão de 2D Animation, referências de assembly e configuração do Input System ajustados à API do Unity 6.
- Dez checks de domínio passaram; compilação C# com stubs e com DLLs reais passou sem erros/avisos. A checagem com DLLs não substitui o Editor e usa os pacotes disponíveis no template.
- A tentativa real de importação Android foi bloqueada por licença Unity não ativada. Registro de pacotes Unity bloqueado pela política de rede. Sem Play Mode, APK, vídeo ou conexão Unity MCP confirmada.

Checkpoint de preparação, sem nova versão/tag de jogo. Instruções e evidências em [UNITY-NUVEM.md](UNITY-NUVEM.md).

## Preparação MCP e ativação Personal — 20/10/2026

- Coplay MCP 10.0.0 preparado com revisão fixa, dependências Python instaladas pelo lock upstream e cliente HTTP registrado no Codex CLI remoto.
- Handshake real MCP passou e anunciou 29 ferramentas; consulta de instâncias retornou zero Editores. Ponte declarada no manifesto, com importação ainda pendente.
- Método Personal confirmado no cliente oficial. Ativação exige autenticação da conta; login/licença/registro de pacotes estão bloqueados pela política de rede atual. Auxiliar de ativação preparado para segredo do ambiente, sem credenciais versionadas.
- Não houve edição do gameplay, Play Mode, hierarquia pelo Editor, APK ou ativação de licença confirmada. Sem nova versão/tag de jogo.

Instruções e bloqueios: [UNITY-MCP.md](UNITY-MCP.md).

## Correções de combate/save Unity — após 0.0.7 (20/10/2026)

- Oito falhas reproduzidas e corrigidas: ações após vitória/derrota, recompensa parcial de boss e validação/recuperação de save. Temporários válidos podem ser recuperados; arquivos inválidos são preservados.
- Autosave solicitado por evento é feito após o passo completo. Falha de escrita pausa combate e compras, informa o erro e tenta novamente. Esse fluxo de UI/lifecycle ainda depende de validação no Editor.
- Vinte checks C# passaram; compilação com stubs e DLLs reais passou com zero erros/avisos. Adicionado workflow de checks C# independente de licença.
- A tentativa batch Android continua bloqueada pela licença. Sem novo APK, tag de jogo, Play Mode, vídeo ou alegação de desempenho. Números e schema preservados.

Relatório e passos de teste: [UNITY-REVISAO-COMBATE-SAVE.md](UNITY-REVISAO-COMBATE-SAVE.md).

## 0.0.8 — movimento de chão da Broca Pioneira (20/10/2026)

- Adicionado `GroundDrillMotion2D`, uma camada visual reutilizável para caminhada, antecipação, avanço, impacto e derrota.
- O movimento usa DOTween em tempo não escalado e cancela tweens pendentes ao reiniciar, morrer, desabilitar ou remover a unidade.
- A posição funcional, o colisor, os valores de combate, a economia e o save permanecem fora da animação.
- A amostra de sprite enviada foi avaliada e documentada; a folha PNG ainda estava pendente de integração no checkout.

Checks estáticos serão executados neste checkpoint. O Editor remoto não tem licença ativa; não há nova gravação, teste de aparelho ou medição Android.

## 0.0.9 — folha visual da Broca Pioneira (20/10/2026)

- Gerada uma derivação transparente da referência enviada, preservando a broca, a paleta e as 12 poses em uma folha RGBA de 1680 × 960.
- A folha foi normalizada em uma grade de 4 × 3 células de 420 × 320 e recebeu pivô provisório próximo ao contato das esteiras.
- A folha está versionada; a ligação ao HUD e a revisão de importação continuam para o Editor local.

Tag: `v0.0.9`. A imagem foi revisada visualmente fora do Unity; não há validação de frames, toque ou desempenho Android.

## 0.0.10 — folha conectada ao HUD (20/10/2026)

- A folha foi movida para `Resources/Art/Drill` para ser carregada pelo protótipo sem configuração manual de prefab.
- Adicionado `DrillSpriteSheetPlayer2D`, que resolve os 12 frames nomeados, alterna idle/caminhada/ataque/impacto e usa o ícone TMP como fallback se o Unity ainda não gerar os sub-sprites.
- O visual da broca agora é um filho separado do rótulo, mantendo escala, frames e movimento fora da posição funcional.

Tag: `v0.0.10`. Compilação contra stubs e DLLs reais passou; importação pelo Editor, gravação e aparelho Android continuam pendentes.

## 0.0.11 — hub de progressão e bloqueios (20/10/2026)

- O hub ganhou uma faixa de missão com o número da fase, o objetivo do guardião e o estado da tentativa.
- A oficina da broca exibe ATQ e VIDA com nível, custo em ouro, estado de máximo e botões desabilitados quando o limite é atingido.
- Recuperação, Crítico, Dano Crítico e Especial aparecem como melhorias futuras, usando os limites de nível e as fases já presentes nos dados; itens sem balanceamento continuam claramente marcados como “em balanceamento”.
- A garagem agrupa as brocas disponíveis e os desbloqueios por fase 2, 3 e 4. O menu de baús mostra quantidade de baús/chaves e sinaliza Equipamentos e Álbum de Descobertas como bloqueados até a fase 2.
- O layout foi reorganizado para comportar as informações no retrato, mantendo a broca e sua animação visual em um filho separado da posição funcional.

Esta versão melhora a apresentação e a leitura do progresso. Não adiciona abertura de baús, equipamento aplicável, álbum jogável ou uma nova fase; esses itens continuam sendo prévias bloqueadas no protótipo.

Checks de domínio, compilação dos scripts contra stubs e compilação contra DLLs reais do Editor passaram sem erros/avisos. Editor Play Mode, revisão visual, APK, toque e desempenho Android continuam pendentes de execução no ambiente licenciado/aparelho.

Tag: `v0.0.11`.

## 0.0.12 — hub separado do HUD (20/10/2026)

- Corrigida a separação conceitual e estrutural entre as telas: o **Acampamento** é o hub entre tentativas; o `Combat screen` contém apenas a apresentação da escavação e seus controles de combate.
- O hub agora apresenta a Broca Pioneira, oficina, garagem, baús, álbum e status da expedição em cartões próprios. Garagem, equipamentos e álbum exibem bloqueios de fase sem fingir que os sistemas ainda não implementados estão disponíveis.
- Uma tentativa salva continua podendo entrar diretamente no combate. Depois de derrota ou vitória, o botão **Acampamento** leva à base; dali o jogador inicia uma nova tentativa ou continua quando aplicável.
- O Canvas runtime foi renomeado para `Game UI` para não confundir o contêiner da interface com o HUD de combate.

Checks de domínio, compilação contra stubs e compilação contra DLLs reais do Unity passaram. A tela ainda precisa de revisão no Editor licenciado e teste em aparelho Android; nenhuma alegação de FPS foi feita.

Tag: `v0.0.12`.

## 0.0.13 — tela de personagem e oficina (20/10/2026)

- A Oficina do Acampamento ganhou uma tela própria inspirada na referência: Broca Pioneira, poder total, linhas de ATQ/PV, nível, valor atual, custo em engrenagens e botões verdes de melhoria.
- Recuperação de PV, Chance de Crítico, Dano Crítico e Especial aparecem em linhas escurecidas com cadeado e requisitos legíveis. Como esses atributos continuam sem balanceamento funcional, permanecem bloqueados.
- Melhorias de ATQ e PV podem ser compradas na base entre tentativas usando o mesmo ouro e as mesmas curvas já existentes. O save continua sendo feito pelo repositório atômico.
- O ícone de engrenagem é somente a linguagem visual do ouro; nenhuma moeda premium foi adicionada.

Checks de economia/save, compilação contra stubs e compilação contra DLLs reais do Unity passaram. A revisão visual dentro do Editor e o teste em aparelho Android continuam pendentes.

Tag: `v0.0.13`.

## 0.0.14 — prévia da oficina no primeiro acesso (20/10/2026)

- A Oficina pode ser aberta no Acampamento mesmo antes da primeira tentativa, permitindo revisar a estrutura de atributos e bloqueios.
- Em perfil sem expedição, os valores e botões ficam explicitamente desabilitados com a instrução para iniciar; nenhuma compra ou saldo é criado.

Checks de domínio, compilação dos scripts e compilação contra DLLs reais do Unity passaram.

Tag: `v0.0.14`.

## 0.0.15 — identidade visual da personagem

- Nova composição de Oficina com a paleta petróleo/teal/âmbar/ciano/coral/creme enviada pelo usuário, vitrine da Pioneira, pedestal, quatro slots hexagonais e módulos/passivos em cartucho.
- 18 peças gráficas modulares originais, fontes Titan One e DW Rounded ExtraBold (derivação OFL de Nunito), contorno e sombra compartilhados nos materiais TMP.
- Abas Oficina, Peças e Drones funcionam como navegação de consulta. ATQ/VIDA continuam compráveis; saldo insuficiente, máximo e falha de save ficam visíveis. Peças, passivos e drones não implementados continuam identificados como conteúdo em preparação.
- Barra de reforços usa a soma das melhorias compradas, sem inventar XP/maestria. Removido o rótulo que confundia dano por tiro com poder total.
- Acesso pela aba Personagem no combate: pausa e snapshot seguros, retorno à mesma tentativa sem simular o tempo do menu. Novo feedback visual abaixa a face do botão sem mover a área de toque.
- Cartão da Pioneira no hub consulta atributos atuais. Reinício restaura também o movimento de chão após derrota.
- 21 checks de domínio aprovados; compilação com stubs e DLLs reais sem erros/avisos. Editor batch novamente bloqueado por licença antes da importação. Prévia de composição revisada em Chromium, explicitamente fora do Unity.

Detalhes e roteiro pendente de Play Mode/aparelho: [OFICINA-PERSONAGEM.md](OFICINA-PERSONAGEM.md). Nenhum APK novo ou medição de desempenho Android.

Tag: `v0.0.15`.

## 0.0.16 — XP individual por personagem

- Pioneira começa no nível 0. Cada ID de personagem tem XP próprio; desbloqueios novos começam zerados, duplicatas não resetam e trocar/voltar preserva o progresso anterior.
- XP por inimigo derrotado vinculado à personagem que iniciou a tentativa, com eventos de XP/subida de nível e preservação na derrota/reinício/retomada. Curva nova configurável por ScriptableObject: 10 por inimigo comum, 50 por chefe, requisito 20 + 10 × nível e máximo inicial 50.
- Barra da Oficina mostra nível e XP reais; HUD e Acampamento mostram nível. Reforços de ATQ/VIDA permanecem separados, com valores preservados.
- Save Unity esquema 2, migração de 1 em memória, primeiro backup com os bytes anteriores e rejeição de XP inválido/coleções duplicadas/campos ausentes. Versões futuras íntegras não são substituídas por backup antigo.
- Catálogo inicial só com a Pioneira; APIs de concessão/seleção preparadas para personagens futuras. Dois IDs de fixture foram usados para testar independência, sem adicionar personagem fictícia jogável.
- 39 checks de domínio passaram, compilação com stubs/DLLs reais sem erros/avisos. Editor batch novamente bloqueado por licença antes da importação. Sem novo APK, Play Mode desta versão ou medição Android.

Regras, exemplos, detalhes da migração e pendências: [EXPERIENCIA-PERSONAGENS.md](EXPERIENCIA-PERSONAGENS.md).

Tag: `v0.0.16`.

## 0.0.17 — economia e progressão até o mid-game

- Duas moedas obtidas jogando: ouro e cristais. Custos exponenciais, sete melhorias reais (ataque, blindagem, especial, críticos, cadência e reparo), quatro poderes individuais com cinco ranks e requisitos de nível.
- XP individual até 60 com curva longa, recalibrada após o usuário considerar o avanço rápido: `ceil((20 + 12 × nível) × 1,125^nível)`. A referência é 60–100 horas ativas por broca; o cenário artificialmente favorável de farm projetou 66,5 h por ciclos medidos, sem afirmar tempo humano real.
- Campanha numérica até fase 60, usando os biomas existentes. Nove simulações terminaram a campanha em 159–188 min simulados, com personagem nível 32. Fase da campanha e nível individual são progressos diferentes.
- Tesouro abre baús consumindo chave, com categorias Comum/Raro/Épico/Lendário (70/23/6/1%). Procedência por fase impede inflação ao guardar baús. Cristais de primeira vitória e marcos não se repetem no treino.
- Treino na última fase vencida entre tentativas. Aba Oficina/Poderes/Tesouro substitui as consultas provisórias Peças/Drones. Novos personagens, coleção de peças, álbum, anúncios, XP offline e dinheiro real não foram implementados.
- Save Unity 3 migra versões 1/2 em memória, preserva quantidades/XP, mantém os bytes antigos no backup e salva estado do sorteio, campanha, poderes e pilhas de baús. Nível antigo é recalculado pela nova curva sem apagar XP.
- 61 checks de domínio, compilação C# com stubs e contra DLLs reais; Editor batch bloqueado por licença antes da importação. Sem Play Mode desta versão, APK novo, revisão visual no Unity ou medição Android.

Regras, pesquisa, CSV, gráfico e instruções: [ECONOMIA-MIDGAME.md](ECONOMIA-MIDGAME.md).

Tag: `v0.0.17`.

## 0.0.18 — brocas por vitória de fase

- Regra esclarecida: vencer fases 10/20/30/40/50/60 concede Rebite, Centelha, Bastião, Prisma, Sísmica e Aurora. A inicial é Pioneira. Não há desbloqueio por nível da personagem nem novo nível geral de jogador.
- Recompensas começam com XP/nível/poderes zerados; anteriores são preservadas. Equipagem entre tentativas, garagem paginada e identificação na vitrine/HUD. A arte ainda reutiliza a folha da Pioneira com cores distintas.
- Bases melhores e padrões reais: três tiros leves, disparos pesados, estilhaço inato, especial em até três alvos e combinação desses efeitos. Sem novo sistema de estados ou alteração de posições lógicas.
- Degraus extras de dificuldade nas fases 5/10/15… e XP com aumento mínimo de um ponto por fase; requisitos de XP individual continuam crescendo até 60.
- Save esquema 3 mantido: perfil guarda personagens/XP/poderes/fase; reconciliação concede marcos já vencidos sem reset. Prêmios completos antes dos eventos e da gravação.
- 74 checks aprovados; C# com stubs/DLLs reais sem erros/avisos. Nove ensaios até fase 60 com trocas e treino passaram. Importação/Play Mode bloqueados por licença nesta nuvem; sem APK novo ou medição Android.

Catálogo, cálculos, resultados e roteiro: [BROCAS-POR-FASE.md](BROCAS-POR-FASE.md).

Tag: `v0.0.18`.
