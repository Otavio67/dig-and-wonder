# Validação local — 20/10/2026

## Godot 4.7.2 no Windows

O import headless (`--editor --import`) e a inicialização (`--quit-after 5`) passaram. Também passaram os harnesses `logic_harness.gd`, `save_harness.gd`, `defense_harness.gd`, `defense_save_harness.gd`, `game_harness.gd`, `android_harness.gd` e `characters_harness.gd`, todos com código de saída 0 e seus marcadores `*_OK`.

O jogo foi aberto em janela desktop a partir da cena principal e continuou executando. O log de runtime não registrou erros de script ou de recurso. O processo recebeu diretórios de dados de teste isolados e `D_W_TESTING=1`.

Logs locais estão em `.tools/logs/runcheck-godot-*.log` e o log do jogo em `.tools/check-windows/Roaming/Godot/app_userdata/Dig & Wonder/logs/godot.log`; essas pastas são temporárias e ignoradas pelo Git.

## Correção de importação

Na primeira importação local, o Godot também percorreu `UnityProject/Library` e outros arquivos gerados pelo Unity, tratando caches e meshes binárias como assets Godot. O import concluiu, mas emitiu erros que não pertenciam ao jogo. `UnityProject/.gdignore` agora mantém esse projeto separado do scanner Godot. A importação repetida passou sem erros.

## Limites

O script Windows `check-project.ps1` não pôde ser usado diretamente: o caminho padrão do executável não existe neste checkout, e o executável `Godot_v4.7.2-stable_win64_console.exe` disponível retorna erro 193. A suíte foi executada com o Editor gráfico 4.7.2 em modo headless, capturando os códigos de saída, logs e marcadores de cada teste.

Não foi validado toque em aparelho Android, exportação Godot para Android, desempenho em aparelho ou revisão visual detalhada. A janela desktop ficou aberta para teste manual.
