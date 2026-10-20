# Dig & Wonder — especificação histórica Godot

> **Direção ativa: Unity 6.** O usuário confirmou em 20/10/2026 que o desenvolvimento deve continuar em Unity. Abra `UnityProject/` e siga `UnityProject/README.md`, `UNITY-CONTEXTO-LOCAL.md` e `UNITY-REVISAO-COMBATE-SAVE.md`. O restante deste documento preserva decisões e regras de uma versão anterior em Godot; não é instrução para instalar, abrir, validar ou retomar o jogo em Godot.

## Decisão e escopo

Esta especificação registrava a versão Godot de defesa com unidades lançáveis e progressão por fases. A implementação ativa foi transferida para Unity; estes valores e regras ficam como referência histórica e não devem direcionar mudanças no protótipo Unity sem pedido explícito.

Ciclo: **lançar unidades → escavar/combater → ganhar ouro → melhorar permanentemente → superar a fase**. Carrinho fixo, poço abaixo, frente de escavação em distância visual limitada. O jogador não seleciona células.

## Estado atual: Marco 2

O usuário aprovou a revisão visual e o Marco 2 em 20/10/2026. A especificação complementar atual é `MARCO2.md`: Pioneira divide os mesmos 32 de dano em quatro golpes a cada 0,7 s após voo de 0,8 s; quebra ao terminar. Um único apoio adicional, Robô Escavador, gratuito nesta fase de protótipo, sem compra ou campo permanente novo. Save 6 inalterado. A revisão adicionou somente o bônus inicial explicitamente pedido (`first_impact_gold: 2` por tentativa). Demais preços/recompensas/cadências preservados.

## Base do Marco 1 (valores preservados salvo as diferenças acima)

- Uma fase: Poço do Acampamento, seis camadas, sem boss. Materiais: terra, terra, argila, pedra, pedra e cristal. Uma única camada ativa, uma barra de resistência. Vida inicial 44 + 8 por camada; ouro 12 + 2 por camada. Vida compartilhada, sem blocos funcionais independentes.
- Carrinho: 120 PV; dois canhões, cada um com 6 de dano e intervalo de 1,25 s, com pequeno desfasamento. Atacam automaticamente; priorizam preparação de ataque ao carrinho e depois menor ID. Projétil não retargeteia e se dissipa se seu alvo morreu.
- Pioneira: 32 de dano, voo de 0,8 s, cooldown de 3 s, gratuita e descartável. Pode atingir uma toupeira na coluna central. Não bloqueia projéteis nem serve de escudo. O impacto pertence à camada registrada no lançamento; não transfere excesso para a próxima.
- Torrão: 48 PV, 8 de dano, primeiro ataque em 1,7 s, depois a cada 2,8 s; aviso de 0,65 s e projétil de terra com viagem de 0,65 s. Alvo é defensor vivo na frente; sem ele, carrinho. No Marco 1 não há defensor persistente disponível.
- Ondas por tempo a cada 10 s, no máximo 12; três primeiras com um Torrão, posteriores com dois. Quebras das camadas 2 e 4 adicionam uma toupeira. Após a camada 6, duas adicionais formam o encontro final e ondas temporizadas param. Vitória exige limpar todos os inimigos e a fila, incluindo a reserva final. Sem boss.
- Ouro de camada e mob concedido uma única vez. Ouro pertence ao perfil permanente; morrer não o retira. Compras não são devolvidas.
- Dano: custo 18 + 14 por nível anterior, até 10; canhões e Pioneira ganham +12% de suas bases por nível. Vida: custo 20 + 16 por nível anterior, até 10; +35 PV máximos por nível, adicionando os novos 35 PV à vida atual quando comprado durante tentativa. Cadência não é melhorável neste marco.
- Três espaços: Pioneira e dois vazios, sem seletor ou unidades futuras exibidas como disponíveis. Catálogo proposto em `CATALOGO-DEFESA.md`, dados de habilidade com indicador `implemented`.

## Limites e temporização

Máximo de quatro inimigos, duas unidades de impacto e 24 projéteis. Fila normal até oito aparecimentos; encontro final reserva mais dois sem descartá-los no limite. Efeitos visuais até 32. Nenhuma geração aleatória é necessária ao combate inicial.

Simulação em passos de 0,05 s; um frame acumula no máximo 0,25 s. Perder foco pausa o jogo e limpa acumulador/input; retomar não dispara ataques offline. Renderização não concede ouro ou calcula dano. Alvos usam IDs estáveis e cada objeto é removido após seu evento terminal.

## Tentativa, derrota e vitória

Ao morrer, nenhum ataque/lançamento/dano de camada continua: inimigos, unidades, projéteis e fila são limpos. O resultado mostra que o permanente foi preservado. Reiniciar volta à primeira camada, restaura vida e impõe cooldown completo de 3 s. Abertura/reabertura e saída por Voltar seguem a mesma regra, sem refazer cooldown por cliques repetidos durante uma tentativa ativa.

O perfil grava ouro, níveis, espaços, vitórias e desbloqueio. Não grava o estado temporário da tentativa. Fechar/reabrir reinicia a tentativa; não restaura unidades nem concede ouro por tempo fechado. A fase 2 é registrada como desbloqueável pela vitória, mas não está jogável neste marco; não há botão de avanço para conteúdo ausente.

## Persistência e compatibilidade

Leitor legado conserva o esquema 5 e suas migrações de 1–4. Novo perfil é esquema 6 em arquivo separado. Detectar legado abre confirmação com valores exatos; nada é convertido em disco ao apenas carregar.

Após confirmação: copiar bytes do candidato válido para snapshot identificado por hash, verificar identidade e somente então escrever perfil novo validado via temporário e rename, com backup anterior válido. Regra: moedas e valor já acumulado da mochila em 1:1 para ouro; sem conceder o bônus antigo de primeira venda. Níveis antigos da broca menos um mapeiam para Dano, limitado a dez; outros itens e excedentes ficam intactos no snapshot, sem conversão monetária. O arquivo original continua intacto. Repetir confirmação não substitui um perfil novo válido. Arquivos inválidos sem recuperação produzem erro visível em vez de reset silencioso.

A confirmação de importação é uma decisão necessária do jogador, não um segundo modo. Não há perda de ouro/níveis de defesa ao reiniciar. Falha de escrita pausa o combate e informa o problema.

## Organização

- `content/defense/prototype.json`: fase, camada, carrinho, Torrão, limites e melhorias implementadas.
- `content/defense/abilities.json`: dados da Pioneira e propostas explicitamente indisponíveis.
- `scripts/gameplay/defense_run.gd`: simulação, alvos, recompensas e estados terminais.
- `scripts/gameplay/game.gd`: apresentação, input, ciclo de vida e ligação com persistência.
- `scripts/save/defense_save.gd`: perfil 6, recuperação e importação confirmada.
- `safe_surface.gd`, atlas, ícones, paleta e ferramentas Godot são reaproveitados.
- Sistemas antigos de grade/equipamento não integram a entrada principal. Permanecem como legado, histórico e suporte a compatibilidade/testes, sem duas interfaces jogáveis.

## Roteiro autorizado

1. **Marco 1:** ciclo descrito acima, testes, capturas, documentação e um commit.
2. **Marco 2, após aprovação:** clareza, recuperação, ritmo, apresentação e somente uma unidade que ofereça escolha diferente.
3. **Marco 3:** fases adicionais e desbloqueios graduais, um material/inimigo por vez.
4. **Marco 4:** primeiro boss, Serpente de Sucata, com duas fases e antecipação legível.

Não implementar os quatro juntos. Não adicionar moedas, energia, combustível, chaves, oferta de anúncio ou catálogos jogáveis completos neste marco. A meta de 60 FPS em Android exige medição em aparelho.

## Verificação

Check-project importa e inicializa a entrada atual. Harnesses de defesa cobrem alvo estável, telegraph, cooldown, impacto descartável, recompensa única, capacidades, derrota terminal, limpeza, melhorias e fim de fase. Save: confirmação, cópia antes da conversão, esquemas 1–5, perfil 6, validação, backup/temporário e reabertura. Integração: input, compra, reinício e pausa. Android lógico: transformação, um toque ativo, cancelamento e encaixe de área segura; não é toque em aparelho.

Ferramentas antigas de catálogo/save mantêm cobertura de dados legados. Testes de grade/manual foram substituídos por cenários do novo fluxo; o histórico Git e `docs/historico/` registram o comportamento retirado. Capturas reais separam demonstração normal de fixtures de teste. Sem validação Android, anúncios ou APK neste marco.
