# Marco 2 — clareza, animação e uma unidade adicional

20/10/2026. Autorizado após a revisão do Marco 1. Sem nova fase, inimigo, boss, moeda, anúncio, menu ou versão de save. Evidência desktop; Android físico pendente.

## Pioneira

- Voo de 0,8 s com aceleração. Aterrissa e golpeia em 0,8 / 1,5 / 2,2 / 2,9 s contados desde o lançamento. Quatro golpes de 8 na base: **mesmos 32 totais**, escalados pelo nível de dano existente. Recarga continua 3 s.
- Trabalha por 2,1 s entre o primeiro e último contato; salta 31 unidades visuais, comprime a mola ao tocar e deixa poeira. Dano é emitido pela simulação nos contatos, nunca pelo desenho. O último golpe causa quebra com fragmentos da carenagem e descarte dos efeitos em 0,65 s.
- Não é defensor. Alvo central e índice de camada marcados no lançamento; se a camada mudar e não restar o inimigo marcado, quebra sem danificar a próxima. Sem transbordamento ou recompensa duplicada. Limite global continua duas unidades.
- Pausa congela animação e combate. Fim de tentativa limpa unidades/tiros e impede novos golpes. Efeitos estritamente cosméticos, no máximo 24 grupos de nove partículas; nada persiste no perfil.

## Toupeira Torrão

Regra de aparição, 48 PV, dano 8, cadência e ouro 7 inalterados. Atlas de quatro poses pintadas: repouso, preparação, arremesso e reação. Emergência em aproximadamente 0,33 s, respiração discreta, preparação durante os mesmos 0,65 s da regra, arremesso visível por 0,3 s e reação por 0,22 s. A derrota deixa uma imagem em dissipação e o texto do ouro por 1,8 s. Feedback não concede recursos.

## Única unidade adicional: Robô Escavador

Escolha distinta: Pioneira concentra dano de escavação; Escavador permanece e recebe ataques enquanto luta/minera. Atlas de três poses: repouso, golpe e defesa. Prioriza inimigos por ID/ameaça via seletor existente; sem inimigos, golpeia a camada atual. Dano 9 a cada 1 s, 45 PV, até 10 s após voo de 0,8 s, recarga 20 s, um Escavador por vez e limite global de duas unidades.

Decisão de escopo: apoio gratuito e disponível no segundo espaço desta fase de protótipo, sem compra, desbloqueio permanente ou melhoria própria. A proposta de preço 150/fase 2 foi adiada: fase 2 ainda não existe e o usuário pediu preservar economia/save. O terceiro espaço segue BLOQUEADO. Dados reais em `prototype.json`; catálogo marca apenas Pioneira/Escavador como implementados.

Enquanto voa, não protege. Vivo na frente, recebe o projétil de terra destinado a ele. Se morrer/expirar antes da chegada, o ataque atinge o carrinho conforme a regra existente. Expira ou é destruído; não se multiplica nem deixa renda offline. Reinício impõe recargas completas (3 s e 20 s).

## Verificação e evidência

Check nativo Windows: importação, boot e sete harnesses. Defesa verifica quatro contatos discretos, 32 totais, quebra única, nenhuma passagem de dano entre camadas, defesa só após pouso, vida do apoio, expiração, recarga de reinício e morte terminal. Integração verifica botões, compra, pausa/reabertura; save 6 e migração permanecem cobertos.

`scripts/capture_marco2.gd` registra 336 quadros de execução desktop em viewport retrato, 28 s de tempo simulado a 12 quadros/s, acionando botões e compras acessíveis. Continua a mesma tentativa até vitória, depois derrota por inatividade e reinício. Persistência desativada antes de instanciar a entrada. Não injeta ouro/inimigos nem grava sobre o progresso do jogador. Originais em `builds/review/marco2-20261020/`; seleção versionada em `docs/evidencias/20261020/`.

Ainda não verificados: Android real/toque, dp físicos, FPS, memória, consumo, anúncios e novo APK. Canhões ainda usam desenho nativo anterior; isso não é uma declaração de arte final integral. Não avançar ao Marco 3 sem novo pedido.

## Skills e procedência

Consultado o catálogo oficial pelo helper de skill-installer: nenhuma skill específica de animação Godot encontrada; nenhuma instalada. Imagegen já disponível foi usado para os atlas raster. Animação integrada em GDScript, sem Blender/3D ou pacotes externos. Fontes e licenças em `assets/LICENCAS.md`; prompts completos em `PROMPTS-MARCO2.md`.
