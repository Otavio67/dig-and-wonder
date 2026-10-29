# Revisão de gameplay e progressão — 29/10/2026

Base `0c2de1f`, Unity 6000.6.6f1. Verificações de domínio e compilação com stubs passaram. O relatório em `docs/balance/*-0.0.27.csv` foi gerado pelo núcleo de combate, sem anúncios, dinheiro real ou gasto de cristais.

O comprador automático foi corrigido para selecionar apenas melhorias e poderes que consegue pagar; antes, uma escolha preferida cara impedia a compra de alternativas acessíveis. Um teste cobre esse cenário. A alteração afeta o ensaio, não o balanceamento nem o save do jogo.

Foi implementada uma opção de ataque manual: ao desligar AUTO, o jogador pode tocar na broca para iniciar uma rajada, respeitando alvo e cadência. Uma rajada iniciada termina mesmo se AUTO for desligado. Em nova tentativa, o conjunto completo de 87 checks de domínio passou, incluindo alvo, rajada e cadência do ataque manual; compilação com stubs sem erros ou avisos. O toque e a apresentação ainda não foram verificados no Unity. Os relatórios foram regenerados com essa versão do combate.

Depois da introdução dos capítulos, passaram **92 checks** de domínio, incluindo composição das ondas, conclusão e recompensas, XP de treino, retomada do mini-chefe e preservação de tentativa com regras antigas. O build de compilação com stubs passou com zero erros e avisos. O teste de estimativa antiga de 60–100 horas foi retirado porque calculava ondas que já não existem; o relatório de simulação passa a registrar a hipótese de farm, sem provar um limite para todas as estratégias.

Na inspeção, `AutoBattleDriver.IsReady` já bloqueia reinício, treino, compras, baús e alterações de personagem durante falha de gravação. O cenário de falha ainda precisa de teste no Editor.

Com os capítulos e novo balanceamento, nove cenários (três estratégias e três sementes) completaram as 100 fases em 85,76–93,89 minutos simulados, com 126–145 tentativas. Os picos de derrotas ficaram principalmente nas fases 35, 45 e 55. Isso indica pontos prioritários para testar e avaliar curva e sensação de repetição, mas os compradores automáticos e suas prioridades não modelam jogadores reais.

O cálculo otimista de farm na fase 60 projeta 68,013 horas ativas até nível 60 de uma broca, com melhorias e poderes máximos desde o início e XP de chefe repetido a 15%. Não representa tempo real de jogo. Outros cenários de farm podem ser mais rápidos; as curvas merecem avaliação com pessoas.

Ainda necessários: abrir a versão atual no Editor exigido, conferir visual e interface, jogar e testar retomada/controles, e validar toque, desempenho e dificuldade em aparelho. Testes de domínio não demonstram diversão nem qualidade visual.
