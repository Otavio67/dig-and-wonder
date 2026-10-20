# Campanha final até a fase 100 — 0.0.19

Este marco transforma a campanha de 60 para 100 fases. A fase da campanha e o nível da broca continuam sendo progressos separados: a vitória de uma fase libera a recompensa correspondente, mas a broca recebida começa com XP 0, nível 0 e poderes vazios.

O plano de regiões, inimigos, pets, brocas futuras e modos adicionais está em [CONTENT-BIBLE-ENDGAME.md](CONTENT-BIBLE-ENDGAME.md). A quantidade progressiva de encontros e os 50 tipos planejados estão em [`content/enemies/campaign-tiers.json`](../content/enemies/campaign-tiers.json). Os catálogos ainda não são carregados pela cena Unity.

## Recompensas de fase

| Vitória | Broca | ATQ base | PV base | Padrão principal |
| ---: | --- | ---: | ---: | --- |
| 10 | Rebite | 1,45× | 1,4× | Dois impactos reforçados |
| 20 | Centelha | 1,9× | 2× | Três tiros leves, cadência +15% |
| 30 | Bastião | 2,75× | 3× | Dois impactos de 120%, cadência -5% |
| 40 | Prisma | 4,5× | 4,7× | Estilhaço inato de 20% |
| 50 | Sísmica | 6,8× | 8× | Especial em até três alvos, tiros +15% |
| 60 | Aurora | 11,5× | 14× | Três tiros de 85%, especial em dois alvos |
| 70 | Cobalto | 16,5× | 26× | Dois tiros perfurantes de 125%, cadência +5% |
| 80 | Nova | 28× | 48× | Três tiros, estilhaço inato de 25% |
| 90 | Titã | 42× | 90× | Dois tiros de 140%, especial em três alvos, cadência -10% |
| 100 | Singular | 70× | 180× | Três tiros, especial em três alvos, estilhaço inato de 30% |

As quatro recompensas novas reutilizam provisoriamente a folha visual da Pioneira com tintas de identificação. Elas já têm comportamento funcional distinto no núcleo de combate; sprites, rigs e animações exclusivos ficam para uma etapa visual posterior.

## Curva de campanha

O crescimento continua aplicado por fase, com um degrau a cada cinco fases:

```text
degrau = floor(fase / 5)
PV comum = 24 × 1,115^(fase−1) × 1,12^degrau
dano comum = 4 × 1,10^(fase−1) × 1,08^degrau
PV chefe = 8 × PV comum
dano chefe = 1,5 × dano comum
```

Até a fase 60, a parte contínua usa os fatores 1,115 (PV) e 1,10 (dano). A partir da fase 61, o end game comprime o crescimento entre degraus para 1,045 (PV) e 1,04 (dano). Os degraus de 5 fases continuam valendo integralmente. Essa compressão foi necessária porque a curva original fazia a simulação travar antes da recompensa da fase 100; ela mantém a pressão crescente sem tornar o último prêmio inalcançável dentro do combate atual.

O chefe permanece sendo a única condição de avanço. O número de ondas, a regra de ouro, o baú e a chave por vitória permanecem os mesmos; o chefe final entrega também a Singular e os prêmios normais da fase 100. Os biomas atuais continuam alternando, portanto este marco amplia a progressão numérica sem fingir que existem 40 cenários artísticos novos.

XP de inimigo continua estritamente crescente por fase. A referência de longo prazo do nível 60 individual permanece a fase 60; as fases 61–100 dão uma recuperação mais rápida para brocas recebidas tarde na campanha. Isso evita exigir que a Pioneira seja levada ao nível máximo antes de trocar e mantém a troca como recompensa de exploração.

## Fim da campanha e treino

Depois de vencer a fase 100:

- `HighestClearedStage` fica em 100 e `CampaignStage` não passa desse valor;
- a Singular é adicionada uma única vez, no nível 0;
- o botão de treino repete a fase 100 entre tentativas;
- repetir a fase 100 continua concedendo ouro, XP, baú e chave, mas não duplica os cristais de primeira vitória nem a Singular;
- a equipagem continua permitida somente entre tentativas e não altera uma rajada em andamento.

Não há fase 101 neste marco. O espaço de 100 fases é um ponto de recuperação para o próximo conteúdo de end game, que poderá receber desafios, regiões e equipamentos sem alterar o save atual.

## Compatibilidade e validação

O save permanece no esquema 3. Perfis que já venceram fases até 60 recebem as recompensas 10–60 por reconciliação; as quatro novas recompensas só aparecem quando `HighestClearedStage` alcançar 70, 80, 90 ou 100. Nenhum XP, poder, moeda ou baú existente é resetado.

O catálogo ativo agora tem onze `CharacterDefinition` e a fábrica rejeita conteúdo incompleto. Foram aprovados **75 checks de domínio**, incluindo limite na fase 100, catálogo 10–100, bases progressivas, padrão final da Singular, XP individual, save atômico e retomada de projéteis. A compilação contra stubs e DLLs reais do Unity continua obrigatória.

Nove simulações determinísticas do núcleo C# (três políticas de compra × três sementes, sem anúncios ou cristais) chegaram à fase 100: ataque **101,22–105,95 min**, equilíbrio **104,38–110,47 min** e sobrevivência **113,62–119,75 min**. A Titã foi a broca que concluiu essas passagens e a Singular ficou disponível como próxima recompensa. Esses são tempos simulados por compradores automáticos; não são uma promessa de ritmo humano ou de desempenho no Android.

Ainda pendente: importação/Play Mode no Editor remoto por falta de licença Unity, revisão visual no runtime, APK desta versão, teste em aparelho e medição de 60 FPS. Os números acima são regras e simulações do núcleo C#, não uma alegação de desempenho ou diversão humana.
