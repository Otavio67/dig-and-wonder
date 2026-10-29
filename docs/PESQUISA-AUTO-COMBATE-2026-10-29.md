# Pesquisa e aplicação — combate automático

Pesquisa em 29/10/2026; descrições oficiais, não avaliação de partidas próprias nem medição de retenção.

- [Idle Battlegrounds](https://play.google.com/store/apps/details?id=com.nightskygames.idlebattlegrounds): combina combate automático, ouro, melhorias e personagens com habilidades. Aplicação proposta: brocas com padrões perceptivelmente diferentes e melhorias cujo efeito seja claro.
- [Nomad Idle, página do desenvolvedor](https://store.steampowered.com/developer/thefoxknocks/): usa ondas automáticas, atributos, equipamentos e habilidades passivas. Aplicação proposta: escolhas simples e uma sequência de encontros compreensível, sem introduzir todos os sistemas de uma vez.
- [Kin and Quarry, página do desenvolvedor](https://store.steampowered.com/developer/thefoxknocks/): mineração, exploração, melhoria, achados e ajudantes. Aplicação proposta: recuperar descobertas subterrâneas como motivação futura; ainda não estão implementadas neste projeto.
- [Unity: economia equilibrada](https://unity.com/how-to/design-balanced-in-game-economy-guide-part-3): recomenda considerar diferentes comportamentos e verificar curvas de fontes e gastos em cenários distintos. Aplicação: simulações com três estratégias e sementes, sem tomar seus resultados como tempo ou diversão de pessoas.

## Aplicado neste marco

Na campanha atual, o guardião de uma nova tentativa espera até os inimigos comuns e a fila acabarem; assim ele aparece sozinho. Partidas antigas que já haviam gerado um chefe continuam com o estado salvo. Uma barra ampla exibe vida atual e máxima do guardião. O teste de domínio verifica o surgimento sem inimigos e fila; a barra compilou com stubs, mas posicionamento e leitura ainda exigem o Editor.

Os nove cenários automáticos anteriores foram repetidos após essa alteração: todos alcançaram a fase 100 em 132,15–143,01 minutos simulados e 112–124 tentativas. As derrotas concentraram-se principalmente nas fases 50 e 60. São cenários controlados com compra automática e não demonstram ritmo humano. A projeção otimista de XP na fase 60 é 66,415 horas por broca ao nível 60, com equipamento máximo desde o início.

## Atualização posterior: capítulos implementados

Os encontros por capítulo e as recompensas foram implementados depois dos resultados históricos acima. Consulte [desenvolvimento](DESENVOLVIMENTO.md) e [revisão de gameplay](REVISAO-GAMEPLAY-2026-10-29.md) para regras e simulações atuais. Revisão visual, interação de toque e desempenho Android continuam pendentes.
