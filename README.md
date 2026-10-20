# Dig & Wonder

Protótipo Godot 4.7.2 de **defesa com escavação por camadas**, para Android, desenvolvido em GDScript com renderer Compatibility. Esta é a direção principal autorizada em 20/10/2026. A mineração manual em grade foi substituída, não virou um segundo modo.

## Estado atual — Marco 2

Carrinho fixo no topo, dois canhões automáticos, seis camadas com vida compartilhada, Broca Pioneira com quatro golpes e cooldown de 3 s e Toupeira Torrão. Lance brocas, receba ouro por camada/mob e compre melhorias permanentes de dano e vida. Ao zerar a vida, a tentativa termina; reinicie com vida cheia, camada inicial e cooldown completo. Ouro já ganho e compras permanecem.

A fase termina após seis camadas e um encontro de toupeiras. Não há boss neste marco. A vitória registra o próximo desbloqueio, mas somente a fase 1 está implementada; a opção disponível é repetir. O Robô Escavador é o único apoio adicional: gratuito, recarga de 20 s e atuação de até 10 s. Demais habilidades, inimigos e fases continuam propostas.

Detalhes atuais: [Marco 2](docs/MARCO2.md), [revisão do Marco 1](docs/REVISAO-MARCO1.md), [continuidade na nuvem](docs/CONTINUAR-NA-NUVEM.md).

## Jogar no PC

Abra `project.godot` no **Godot 4.7.2** e use F5. No executável portátil local:

```powershell
& '.\.tools\godot-4.7.2-win64\Godot_v4.7.2-stable_win64_console.exe' --path .
```

Clique em **Iniciar tentativa**, lance a Pioneira quando ficar pronta e compre Dano/Vida com o ouro recebido. Não há mira manual, custo por lançamento, venda, mochila, chaves, baús ou anúncios no ciclo atual. Esc/Voltar retorna à abertura; iniciar novamente recomeça a tentativa, com cooldown completo.

## Verificar

Windows: `./check-project.ps1` usa Godot portátil, dados isolados em `.tools/check-windows/` e falha em erros de script/log ou ausência da confirmação de cada harness. Linux: `bash check-project.sh` e `bash scripts/run-harnesses.sh`, após `bash setup-cloud.sh`. Harnesses de catálogo/save antigos permanecem para validar os dados importáveis; gameplay, interação e combate têm cobertura nova. Logs locais em `.tools/logs/`.

## Save e importação

O perfil ativo usa esquema **6**, `user://dig_wonder.defense.v6.json`, com temporário validado e backup. Guarda ouro, níveis de Dano/Vida, três espaços, conclusão e desbloqueio. A tentativa não é retomada ao reabrir: vida, inimigos, brocas, tiros, camadas e cooldown reiniciam. Sem renda offline ou ataques acumulados.

Saves 1–5 são detectados pelo leitor legado, mas **não importados automaticamente**. A tela informa a regra e exige **Criar cópia e importar**: moedas + valor da mochila viram ouro em 1:1, sem bônus de primeira venda; `drill_level - 1` vira nível de dano limitado a 10. Itens, skins, chaves, baús, fases e excedentes ficam na cópia, sem valor de conversão inventado. Uma cópia de bytes idênticos, identificada por hash, é verificada antes da escrita do perfil 6. O arquivo original não é alterado. Perfil inválido sem recuperação disponível mostra erro, sem apagar silenciosamente o progresso.

## Arte e arquitetura reaproveitadas

Atlas pintados da máquina/terreno, fundos em camadas, paleta petróleo/turquesa/latão/coral, placas com rebites, ícones e superfície de área segura. Nesta amostra o carrinho tem pivô de chassi e dois canhões independentes; a Pioneira separa carcaça, mola e ponta. Animações cosméticas não alteram hit points, cooldown, dano ou alvos. Lia e Bento e seus rigs continuam preservados, fora da tela. Veja a [gravação curta da amostra](docs/evidencias/20261020/amostra-animacoes.mp4).

- [Desenvolvimento e regras atuais](docs/DESENVOLVIMENTO.md)
- [Catálogo: implementado versus proposto](docs/CATALOGO-DEFESA.md)
- [Identidade visual](docs/IDENTIDADE-VISUAL.md) e [direção de arte](docs/DIRECAO-DE-ARTE.md)
- [Histórico de versões](docs/VERSOES.md), [proveniência](assets/LICENCAS.md) e [Android](docs/ANDROID.md)
- Histórico do modo substituído em `docs/historico/`; a entrada principal não o executa.

A versão de protótipo atual é 0.0.6, checkpoint da amostra de animações. Marco 1 e Marco 2 foram checkpoints sem APK próprio; builds anteriores são do modo antigo.

## Evidências e limites

Capturas reais do Marco 1 em `builds/review/marco1-20261020/`, ignoradas pelo Git. `scripts/capture_defense.gd` registra vinte segundos em 12 fps e continua a mesma tentativa por comandos normais até vitória; também verifica derrota por inatividade e reinício. A persistência é desativada somente na instância de captura para não sobrescrever arquivos do jogador. As antigas entradas de captura encaminham para o protótipo atual.

Execução desktop em viewport de celular não comprova toque Android, FPS, memória, consumo, acessibilidade, anúncios ou APK. O protótipo não exige anúncio. SDK/prova AdMob continuam isolados e não foram alterados. Marco 2 autorizado e implementado: uma unidade adicional e animações. Não avançar ao Marco 3 sem novo pedido.
