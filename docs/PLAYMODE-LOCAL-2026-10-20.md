# Validação de Play Mode — 20/10/2026

## Resultado

Smoke test de runtime aprovado com limitações, no Unity 6000.6.6f1, em Windows. O Editor real entrou e saiu de Play Mode em batch com gráficos; não foi apenas compilação nem execução da simulação por .NET.

A validação usou uma cópia isolada em `../.tools/playmode-20261020/UnityProject` relativa à pasta superior ao repositório. Perfil isolado: `Application.persistentDataPath` terminou em `DigAndWonderValidation/PlayModeSmoke20261020`. Os saves do jogador não foram usados. O preparador automático de identidade foi desabilitado somente nessa cópia, para impedir que o reload do Editor voltasse ao nome do produto original.

## Cenário exercitado

1. Abrir `Assets/_Game/Scenes/Prototype.unity` e entrar em Play Mode.
2. Confirmar bootstrap, driver pronto, onze brocas, interface e EventSystem.
3. Invocar o listener do botão ativo `Hub enter cave`.
4. Aguardar os Updates reais do driver, sem chamar manualmente `Simulation.Step`.
5. Confirmar avanço de frames/tempo, ataques, impactos, derrotas e ouro.
6. Pausar pelo método de pausa de menu do driver durante um segundo; confirmar tempo de combate parado.
7. Reler o save de teste pelo repositório real e conferir ouro e tentativa ativa.
8. Retomar o driver e confirmar novo avanço; sair de Play Mode.

## Evidências

- 15 verificações aprovadas no roteiro.
- Frames: 3 → 24469. Este número é do loop de teste batch e não representa FPS em aparelho.
- Tempo da simulação: 0 → 16,15004 s.
- 12 eventos de ataque, 10 impactos, 3 inimigos derrotados.
- Ouro: 0 → 24; vida: 120 → 119,6.
- Nenhum erro/exceção capturado durante o roteiro.
- Pausa/retomada e escrita/releitura do save aprovadas.

Relatório JSON: `C:\Users\otaau\OneDrive\Documents\ChatGPT\Dig & Wonder\.tools\playmode-20261020\playmode-report.json`.

Log completo: `C:\Users\otaau\OneDrive\Documents\ChatGPT\Dig & Wonder\.tools\playmode-20261020\unity-playmode.log`.

## Achados e limites

- Dois avisos no runtime: glifos de cadeado U+1F512 e círculo U+25C9 não encontrados em LiberationSans SDF/fallbacks; substituídos por quadrados nos objetos `Tab info` e `Label`.
- A importação registrou avisos de variantes de shader sem suporte. O roteiro não comprova qualidade de renderização.
- O log completo contém uma `ArgumentOutOfRangeException` na inicialização do indexador `UnityEditor.Search.SearchDatabase`/`SearchInit.IndexationOnStartup`, anterior à coleta do roteiro. A pilha aponta ao Editor, sem código do jogo. O teste prosseguiu e passou; portanto, o log completo não está livre de exceções, embora nenhuma tenha sido capturada durante o cenário de combate.
- A janela original estava em `Untitled` com Console inicialmente em 1 informação, 0 avisos e 0 erros. Ao tentar carregar a cena pela interface, o controle da janela falhou repetidamente com `failed to activate captured window`; não foi possível concluir a revisão visual interativa.
- Interação validada por listener real de Button, sem mouse/toque físico ou raycast de entrada.
- Pausa validada pela API do driver; não foi exercitado o fechamento visual da Oficina.
- Vitória, derrota, reinício, reabertura completa do aplicativo, compras, seleção de outras brocas, safe area, Android e desempenho não foram validados por este smoke test.
- Não houve alteração em scripts de produção, pacotes ou assets do projeto principal durante esta validação. O harness permanece somente na cópia isolada.

Próxima ação: corrigir os glifos ausentes e revisar o layout/interação na Game View, depois testar em Android quando o módulo estiver instalado.
