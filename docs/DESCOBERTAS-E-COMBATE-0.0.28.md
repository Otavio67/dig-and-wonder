# Descobertas, combate e progressão — 0.0.28

Implementado em 29/10/2026, após o pedido de integrar as propostas ao jogo. Projeto Unity; não há instalação de pacotes adicionais. A UI é criada pelo bootstrap existente, sem montagem manual de cena.

## Escolhas de design

As referências de mineração/exploração sugeriam devolver à campanha a sensação de descoberta. Isso cabe no combate automático como recompensa de um encontro, sem exigir escavação estratégica. Variedade de inimigos e ataques ajuda a distinguir fases; textos de objetivo e recompensa precisam refletir o estado real. As brocas já possuíam rajadas, estilhaços e especiais em área: esses sistemas foram aproveitados.

Não foram adicionados moedas, gacha, missões diárias, penalidades por ausência ou bônus de atributo por completar coleção. O álbum é uma motivação paralela à força da máquina. Pets, modos extras e arte exclusiva de todas as brocas continuam fora deste marco.

## Regras implementadas

- **20 descobertas:** fase 5 de cada capítulo concede um fóssil; fase 10 concede uma relíquia. A recompensa é garantida ao concluir o encontro. Matar o mini-chefe sem eliminar a escolta não concede a descoberta. Cada entrada existe uma só vez no perfil, sem sorteio ou duplicatas.
- **Álbum funcional:** acesso na navegação, dez páginas de capítulos, nomes/descrições dos achados e condição dos bloqueados. Ao ganhar uma entrada, abre após o passo da simulação e pausa o avanço até Continuar. O conteúdo é textual; não há vinte ilustrações exclusivas.
- **Perfis antigos:** a coleção começa vazia; é possível recuperar os achados de chefes já vencidos pelo treino. Ouro, cristais, XP, poderes, baús e fronteira da campanha são preservados.
- **Ondas preservadas:** três ondas de 3–5 inimigos; fase 5 encerra com mini-chefe e 3–4 acompanhantes; fase 10 contém só o guardião. Nenhuma descoberta cai em fase comum.
- **Inimigos:** comuns desde a fase 1; ágeis introduzidos na 2; blindados na 3; carregadores na 4. Distribuição determinística, sem nova rolagem ao retomar.
- **Ágil:** 70% da vida, 65% do dano e intervalo de ataque de 70% do comum.
- **Blindado:** 115% da vida, intervalo de 130%; recebe 75% do dano de tiros e estilhaços. O especial ignora essa resistência.
- **Carregador:** dano de 145%, intervalo de 150%, preparação mínima de 1,2 segundo. Guardiões também anunciam seus golpes por pelo menos 1,2 segundo. Eliminar um inimigo durante a preparação cancela seu golpe; projéteis já lançados continuam.
- **Ataques da broca:** tiros ainda não lançados e impactos seguintes do especial procuram um novo alvo quando o anterior morre. Projéteis que já estavam voando não mudam de alvo.
- **Guardiões:** encontros versão 2 usam 90% da vida e 85% do dano do guardião anterior. Mini-chefes mantêm 3× vida do comum e 1,2× dano comum ajustado. Não há exceção escondida para uma fase específica.
- **Treino:** qualquer fase de 1 até a última vencida pode ser selecionada em MODOS após terminar a tentativa atual. A vitória mantém a maior fase vencida e o bioma desbloqueado, e oferece retorno à fronteira da campanha. Não há avanço automático após treino. Cristais de primeira vitória não se repetem; chefe repetido continua com 15% do XP de chefe.
- **Apresentação:** mini-chefe usa a folha existente do mineiro; guardião fica maior e sozinho com sua barra grande. Tipos têm rótulos, formas distintas e aviso PREPARANDO. As brocas usam cores e módulos visuais associados a chassi, rajada e ataque em área/estilhaços, ainda sobre a folha base da Pioneira.
- **Clareza:** resumo de vitória apresenta a recompensa efetivamente concedida. Há dicas iniciais de inimigos e sugestão de melhoria que o jogador consegue comprar após derrota. O álbum substitui a antiga aba de Pets em preparação.

## Persistência

Save esquema **4**, com migração em memória e backup anterior preservado. `DiscoveryStages` identifica entradas por fase; tentativas guardam `IsTraining`, tipo do inimigo e preparação pendente dos ataques. Campos ausentes, duplicados ou inválidos são rejeitados sem reset silencioso. Regras de encontros 0/1 continuam disponíveis para tentativas antigas; novas tentativas usam versão 2.

A descoberta e os recursos são concedidos antes dos eventos de vitória. O driver continua gravando depois do passo completo; a UI não concede recursos nem calcula dano. Nenhum perfil real foi usado como fixture.

## Validação

- **107 checks de domínio aprovados**, incluindo coleção, repetição, escolta, migração 3→4, recuperação de save, entradas malformadas, treino, resistência, preparação/interrupção, troca de alvo e retomada determinística.
- **UnityCompileCheck** com stubs: zero erros e avisos.
- **UnityApiCompileCheck**: zero erros e avisos contra DLLs do Editor instalado **6000.0.23f1** e assemblies locais dos pacotes. É verificação C# adicional, não validação no Editor exigido **6000.6.6f1**.
- Nove campanhas simuladas, três estratégias × três sementes, chegaram à fase **100** com **20/20 descobertas**. Resultado final: **53,04–65,89 minutos simulados**, **112–120 tentativas**, sem anúncios, dinheiro real ou gasto de cristais.

Comparação: a base anterior terminou em 85,76–93,89 minutos e 126–145 tentativas. Antes do ajuste dos guardiões, a nova mecânica terminou em 55,92–69,45 minutos e 122–128 tentativas, mas acumulou 54 derrotas na fase 60 entre os nove ensaios. O ajuste reduziu esse total para 24. Ainda há concentração nas fases 45 e 55. Os números vêm de compradores automáticos; não medem diversão nem tempo de pessoas.

Relatórios finais: `docs/balance/*-0.0.28.csv`. A coluna `discoveries` permite conferir o álbum ao longo da campanha. A projeção otimista de XP até nível 60 ainda fica em cerca de 64,23 horas ativas no cenário de farm da fase 60, com equipamentos máximos desde o início; não é uma meta aprovada de duração nem um limite para outras estratégias. O pós-campanha merece teste com pessoas antes de ampliar conteúdo.

## Limites e próxima validação

O Windows bloqueou temporariamente a execução de um binário recompilado; os comandos normais executaram com sucesso após as alterações finais. Nenhuma política de segurança foi alterada. Somente o Editor 6000.0.23f1 consta na instalação local; o projeto exige 6000.6.6f1. Não foi aberto na versão errada.

Ainda falta abrir **6000.6.6f1** e verificar importação, Play Mode, álbum/pausa/retorno, troca de broca, tamanho dos textos e das barras em retrato, falha real de gravação e retomada. Depois, validar toque, safe area e FPS em Android. Os testes e a compilação não comprovam esses itens. A arte exclusiva do elenco e dos colecionáveis não está concluída; os módulos reutilizados precisam de revisão visual em movimento.
