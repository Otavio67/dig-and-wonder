# XP individual por personagem — 0.0.16

> Atualização 0.0.18: havia sete brocas jogáveis, seis concedidas por vitórias nas fases 10–60, começando zeradas. No checkpoint 0.0.19, o catálogo passou a onze e a campanha vai até 100; veja [ENDGAME-100.md](ENDGAME-100.md) para as quatro recompensas finais. A regra de XP individual permanece a mesma.

> Histórico da 0.0.16. A partir da **0.0.17**, a curva ativa vai até 60, há bônus de atributos por nível e poderes individuais. Os totais de XP são preservados; o nível é recalculado pela curva nova. Consulte [ECONOMIA-MIDGAME.md](ECONOMIA-MIDGAME.md) para os números atuais e o save esquema 3.

## Regra de jogo

Cada personagem possuída tem um ID estável e seu próprio XP acumulado. A Pioneira (`pioneer`) começa no nível 0. Uma personagem concedida pela primeira vez começa com zero XP e nível 0; receber novamente o mesmo ID conserva seu progresso. Trocar de broca e voltar não transfere nem apaga XP.

Só a personagem registrada no início da tentativa recebe XP por inimigo derrotado. Disparar, causar dano, abrir menus, reabrir o app e passar tempo fora do jogo não concedem XP. A troca de personagem exige posse e é permitida somente entre tentativas, conservando a autoria dos ataques/projéteis pendentes. Derrota e reinício preservam XP.

O nível individual é separado dos reforços de ATQ/VIDA comprados com ouro. Esses reforços existentes continuam no perfil compartilhado, com seus valores preservados. Esta etapa não adiciona bônus automáticos de atributos por nível de personagem nem altera o dano/cooldown do combate.

## Curva inicial configurável

Estes são números **novos de protótipo**, criados para o pedido de XP; não são conversão de balanceamento Godot.

| Parâmetro | Valor inicial |
| --- | ---: |
| XP por inimigo comum derrotado | 10 |
| XP por chefe derrotado | 50 |
| XP para nível 0 → 1 | 20 |
| Aumento do requisito por nível | 10 |
| Nível máximo inicial | 50 |

Para subir a partir do nível `n`, exige-se `20 + 10 × n` XP. O excedente continua no nível seguinte. Exemplos: 10 XP total = nível 0, 10/20; 35 total = nível 1, 15/30; 50 total = nível 2, 0/40. O teto inicial é 13.250 XP acumulado; a barra exibe máximo ao chegar ao nível 50. Uma futura redução da curva não reduz XP já armazenado.

A curva está em `Resources/Data/CharacterExperience.asset`. Cada `CharacterDefinition` referencia uma curva por ScriptableObject; personagens futuras podem compartilhar a curva ou usar outro recurso. IDs, nomes e curvas não dependem de nome de GameObject nem de `GetInstanceID`.

## Arquivos e integração

- `SaveData.cs`: esquema 2, `EquippedCharacterId`, coleção `Characters` com `CharacterId`/`TotalExperience` e `CharacterId` na tentativa. O nível é calculado a partir do XP, evitando guardar dois valores que possam divergir.
- `CharacterProgression.cs`: posse, desbloqueio inicial, seleção na base, curva, níveis e concessão de XP. C# puro, sem relógio de UI ou RNG.
- `CharacterDefinition.cs` e `CharacterExperienceDefinition.cs`: conteúdo editável no Editor. Catálogo inicial real: apenas a Pioneira.
- `CombatConfigurationFactory.cs`: carrega os personagens implementados e suas curvas para a simulação.
- `IdleCombatSimulation.cs`: aplica XP dentro da transação de derrota do inimigo, antes dos eventos de recompensa. Emite `CharacterExperienceAwarded` e `CharacterLeveledUp`; a UI não concede XP.
- `AutoBattleDriver.cs`: encaminha autosave ao fim do passo e expõe pontos de entrada para futuras recompensas/seleção.
- `CharacterWorkshopView.cs`: nome da personagem equipada, nível individual e barra de XP. ATQ/VIDA permanecem nas linhas de reforço.
- `PrototypeGameUI.cs`: nível junto à vida, indicação de XP/subida de nível no combate e nível no Acampamento.

Para integrar uma personagem futura: adicionar um `CharacterDefinition` com ID único em `Resources/Data/Characters/`, vincular sua curva e marcar como implementada quando a integração de personagem/arte/combate estiver pronta. Uma recompensa real chama `driver.TryUnlockCharacter(id)`; a seleção na base chama `driver.TryEquipCharacter(id)`. O driver rejeita IDs ausentes ou não implementados. Desbloquear não equipa automaticamente.

Essas APIs persistem a alteração pelo mesmo repositório atômico. Retorno positivo significa alteração aceita em memória; uma falha de gravação permanece visível em `IsSaveBlocked`, pausa o fluxo e tenta novamente. Não foram adicionados uma segunda broca jogável, recompensa fictícia ou um botão de teste que conceda personagem ao jogador. Os testes usam um segundo ID de fixture que não existe no catálogo do jogo.

## Migração e recuperação

O leitor aceita o esquema Unity 1, valida checksum e dados antigos e cria o esquema 2 em memória. A Pioneira recebe XP 0, pois não havia histórico confiável de XP. Ouro, chaves, baús, níveis comprados, fase e tentativa são preservados; a tentativa antiga passa a pertencer à Pioneira.

Apenas carregar não altera os bytes originais. Na primeira gravação confirmada, o principal passa ao esquema 2 e os bytes anteriores são preservados no backup rotativo. Saves novos exigem explicitamente a coleção, o ID equipado e os campos de XP; campos ausentes não são aceitos como XP zero. A desserialização substitui a coleção padrão em vez de adicionar uma segunda Pioneira.

Validação rejeita XP negativo, IDs vazios/duplicados, personagem equipada não possuída e tentativa de outra personagem. IDs futuros guardados na coleção são preservados; se a equipada estiver ausente no catálogo desta instalação, o jogo bloqueia o início sem zerar o arquivo. Um principal íntegro de esquema futuro também é preservado, mesmo existindo backup antigo.

## Verificações executadas

- **39 checks de domínio aprovados**: os 21 anteriores, mais casos de nível zero, isolamento, duplicatas, troca, autoria da tentativa, XP por morte única, recompensa atômica de chefe, excedente, limite, derrota, retomada de projéteis/XP a partir de arquivo real, migração, backup e dados inválidos/futuros. Arquivos sintéticos isolados; nenhum save real do jogador foi modificado.
- **Compilação com stubs e contra DLLs reais Unity/TMP/uGUI/DOTween**: zero erros e avisos. Não substitui importação do Editor ou Play Mode.
- **Referências dos novos assets e `.meta`**: GUIDs únicos e referências locais existentes.
- **Tentativa real do Editor batch**: saída 1, interrompida antes da importação por `No valid Unity Editor license found`. Log local ignorado: `.tools/logs/unity-character-xp-0.0.16.log`.

O dano base 6, vida 120, custos/limites de ATQ/VIDA e especial de 32 em quatro golpes continuam com os valores anteriores. O esquema de persistência foi migrado deliberadamente de 1 para 2.

## Validação pendente no Editor/aparelho

1. Abrir `Prototype` e conferir **Personagem**: Pioneira nível 0, XP 0/20 em perfil novo.
2. Derrotar um inimigo comum: XP 10/20. Derrotar o segundo: nível 1, XP 0/30. Verificar indicação da subida de nível.
3. Abrir o menu durante a tentativa, aguardar e fechar: XP/combate devem permanecer pausados, sem avanço pelo tempo do menu.
4. Fechar/reabrir e comparar nível, XP, ouro e estado da luta. Repetir após derrota e reinício.
5. Usar cópia isolada de um save Unity 1 para conferir migração e backup, sem sobrescrever o perfil pessoal durante a revisão.
6. Personagens adicionais devem ser validadas na interface de seleção quando houver outra personagem real integrada. A independência dos registros já foi testada em C# com duas fixtures.

Não houve nova captura/gravação do jogo ou APK. O PNG da Oficina 0.0.15 continua sendo uma prévia externa histórica; ainda exibe a antiga barra de reforços. Toque e desempenho Android continuam pendentes de medição em aparelho.
