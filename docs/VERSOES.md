# Versões do Dig & Wonder

Versões seguem `0.0.patch` durante o protótipo. Cada checkpoint funcional recebe tag Git `v<versão>`; ajustes locais não devem trocar o número até outro marco verificável. O número também fica em `VERSION` e `project.godot`.

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
