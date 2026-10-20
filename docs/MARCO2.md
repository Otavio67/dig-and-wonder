# Marco 2 — clareza, animação e uma unidade adicional

20/10/2026. Autorizado após a revisão do Marco 1. Sem nova fase, inimigo, boss, moeda, anúncio, menu ou versão de save. Evidência desktop; Android físico pendente.

## Pioneira

- Voo de 0,8 s com aceleração. Aterrissa e golpeia em 0,8 / 1,5 / 2,2 / 2,9 s contados desde o lançamento. Quatro golpes de 8 na base: **mesmos 32 totais**, escalados pelo nível de dano existente. Recarga continua 3 s.
- Trabalha por 2,1 s entre o primeiro e último contato. Sua sequência visual é sincronizada pela idade da unidade: preparação, quatro compressões/impactos e recuperação. O dano continua emitido pela simulação nos quatro contatos, nunca pelo desenho. A ponta gira no voo; um anel de impacto responde ao evento real.
- A carcaça e a ponta têm pivôs visuais próprios, separados da raiz que acompanha o estado lógico. O AnimationPlayer em modo manual deriva a fase da simulação; Tween responde a impactos pontuais. Remover unidade, derrotar ou reiniciar cancela ring Tweens, animações e instâncias sem afetar dano/cooldown.
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

Ainda não verificados: Android real/toque, dp físicos, FPS, memória, consumo, anúncios e novo APK. Os canhões continuam em desenho procedural e a amostra não é arte final integral. Não avançar ao Marco 3 sem novo pedido.

### Amostra de animação: carrinho e Pioneira

O carrinho mantém sua ilustração como chassi visual independente, com pivô de suspensão sutil, dois canhões e tubos recuando individualmente. O AnimationPlayer controla idle/derrota; cada disparo real dispara um único Tween de recuo/clarão. Dano recebido usa outro Tween no brilho da carcaça; derrota interrompe ambos e toma prioridade. Reinício limpa e restaura os valores de repouso.

Gravação contínua do renderer desktop: [amostra de animações](evidencias/20261020/amostra-animacoes.mp4). Contém idle, tiros automáticos repetidos, quatro impactos da Pioneira, sobreposição de canhões e broca, derrota por projéteis e reinício limpo. O encontro da captura usa somente pacing/vida/dano acelerados no script de captura; dados de fase, dano, cooldown e save do jogo não mudam. O vídeo registra o jogo renderizado em 30 fps configurados para captura, sem medir desempenho. Para reproduzir, execute `bash scripts/capture-animation-sample.sh` com Godot 4.7.2, ffmpeg e X11/Xvfb.

Harness adicional `scripts/visual/animation_harness.gd`: AP de idle/derrota e ciclo manual da Pioneira; recuos substituídos em vez de empilhados; impacto substituído; interrupção na morte, reset e remoção. APK/toque/fluidez Android seguem pendentes de aparelho.

## Skills e procedência

Consultado o catálogo oficial pelo helper de skill-installer: nenhuma skill específica de animação Godot encontrada; nenhuma instalada. Imagegen já disponível foi usado para os atlas raster. Animação integrada em GDScript, sem Blender/3D ou pacotes externos. Fontes e licenças em `assets/LICENCAS.md`; prompts completos em `PROMPTS-MARCO2.md`.
