# Dig & Wonder — desenvolvimento Unity

## Marco 0.0.28: descobertas e variações de combate

As ideias de coleção, variedade e clareza foram integradas: álbum com 20 achados garantidos nas fases 5/10 de cada capítulo; ágil, blindado e carregador introduzidos gradualmente; rajadas/especial procuram outro alvo depois de uma morte; treino selecionável sem perder o avanço da campanha. A aba ÁLBUM ocupa o acesso antes reservado a Pets. Veja [decisões, regras e testes](DESCOBERTAS-E-COMBATE-0.0.28.md).

## Capítulos e ondas

O usuário definiu uma campanha com **capítulos compostos de fases** (Capítulo 1 — Fase 1, Capítulo 1 — Fase 2, etc.). A proposta inicial é 10 fases por capítulo: fases 1–4 e 6–9 têm ondas comuns de **3 a 5 inimigos**, variando entre ondas; na fase 5 há um encontro de mini-chefe acompanhado de **3 ou 4 inimigos comuns**; na fase 10 o chefe aparece **sozinho**, sem inimigos comuns, com uma barra de vida grande, visível e ligada à sua vida real. Se houver ondas antes do mini-chefe, o encontro dele só começa quando elas forem eliminadas. Uma fase comum termina ao derrotar todas as ondas, não ao derrotar um chefe.

O HUD converte a numeração contínua de 1 a 100 em dez capítulos de dez fases. Em tentativas novas, as fases comuns têm três ondas sequenciais de 3 a 5 inimigos; a fase 5 tem duas ondas e então o mini-chefe com 3 ou 4 acompanhantes; a fase 10 começa com o chefe sozinho e barra de vida ampla. A fase termina quando todos os inimigos do encontro são derrotados. Cada onda só começa quando a anterior termina. Em encontros novos o mini-chefe usa a folha do mineiro e escala menor que o guardião; sua vida é 3 vezes a de um inimigo comum. Dano comum cai para 55% da curva anterior para acomodar grupos maiores; dano do mini-chefe é 1,2 vez o dano comum ajustado. Em encontros versão 2, o chefe final usa 90% da vida e 85% do dano anteriores; versões salvas 0/1 conservam seus valores.

Primeira vitória em cada fase concede 5 cristais; fases 5 e 10 acrescentam 20. Fases comuns concedem um bônus de ouro equivalente a dez inimigos comuns; mini-chefe e chefe concedem um baú e uma chave ao concluir a fase. Repetir o chefe isolado concede 15% do XP de chefe, sinalizado no HUD, para evitar que sua luta curta acelere desproporcionalmente o XP. O ouro de inimigos e as melhorias persistem após derrota. Brocas continuam liberadas a cada dez fases vencidas.

O save esquema 4 registra coleção e versão do encontro: uma tentativa antiga retoma as regras antigas e novas tentativas usam capítulos. A implementação passou nos testes de domínio e simulações, mas a interface, o ritmo, o equilíbrio e a compatibilidade em jogo precisam ser verificados no Editor e em aparelho. O tempo do botão AUTO manual e a aparência do mini-chefe continuam pendentes de revisão visual.

Referência atual após a retirada do projeto Godot em 29/10/2026. O histórico anterior permanece nos commits do Git. A limpeza não altera números, recompensas ou formato de save.

## Ciclo principal

Broca à esquerda, voltada para a direita; inimigos chegam pela direita. Combate automático lateral em retrato, com ouro por inimigo derrotado, melhorias durante a tentativa e progressão por fases. A apresentação anima eventos reais da simulação, sem calcular dano ou conceder recompensas por conta própria.

O menu inicial mostra a arte aprovada e prepara recursos antes de liberar **Iniciar mineração**. A ação começa ou retoma o perfil. Vitória avança para a próxima fase/mapa sem voltar ao menu inicial; derrota permite melhorar e recomeçar. A campanha termina na fase 100, com treino depois dela.

## Progressão e economia

- Sete melhorias compartilhadas: ATQ, VIDA, AUTORREPARO, CADÊNCIA, CHANCE CRÍTICA, DANO CRÍTICO e ESPECIAL.
- XP individual das brocas até nível 60 e quatro poderes por personagem. Novas brocas são concedidas ao vencer os marcos de dez fases, sempre no nível zero.
- Ouro, cristais, baús e chaves usam as regras existentes. A loja usa as transações do tesouro; isto não comprova compras com dinheiro real.
- AUTO alterna o ataque básico. Com AUTO desligado, tocar na broca solicita uma rajada com a mesma cadência; a implementação ainda precisa de teste no Editor e em aparelho. O especial mantém sua automação e permite toque quando pronto; os demais módulos abrem os poderes existentes na Oficina.
- Abrir a Oficina pausa a tentativa. Não mudar essa regra só por causa de um mockup.

Regras numéricas: `UnityProject/Assets/_Game/Resources/Data/CombatBalance.asset`, `Data/Economy/midgame.json` e os catálogos JSON em `content/`. O `ContentCatalogImporter` converte os dados de combate e fases em ScriptableObjects sem depender de outra engine. Consulte [economia](ECONOMIA-MIDGAME.md), [campanha](ENDGAME-100.md) e [brocas por fase](BROCAS-POR-FASE.md).

## Arquitetura e save

`IdleCombatSimulation` controla combate e estados terminais; `AutoBattleDriver` faz a ponte com Unity e persistência; `PrototypeGameUI` apresenta o HUD uGUI/TMP. Animações e efeitos acompanham IDs estáveis e eventos do simulador.

O perfil esquema 4 fica em `Application.persistentDataPath`. Gravação atômica, checksum, backups, migrações e retomada preservam progresso. Arquivo inválido não autoriza reset silencioso. Compras e combate pausam quando a gravação falha. Não apagar nem substituir perfis do usuário em testes.

## Arte e conteúdo

A identidade vigente é [Toy-Mecha Subterrâneo](IDENTIDADE-TOY-MECHA.md). Textos e valores permanecem no Unity; ícones têm SVG editável em `assets/ui/gameplay/`. O cenário, as unidades e as barras precisam compartilhar a linha de chão. A revisão visual final do HUD e o posicionamento de barras ainda têm pendências.

As onze brocas funcionam no domínio e usam a base da Pioneira com cores e módulos visuais de chassi/rajada/área. Folhas exclusivas ainda não foram produzidas. Pets, Torre/Profundezas, cinquenta inimigos futuros e arte exclusiva do elenco não estão concluídos. Catálogos de conteúdo futuro não são evidência de presença no jogo.

## Validar e continuar

Use os checks .NET e o CI Unity; depois confirme importação, Play Mode, botões, scroll, save/retomada e combate no Editor. Android exige APK, aparelho, toque, safe area e medição de desempenho. Não ampliar conteúdo durante uma tarefa de limpeza ou correção pontual.
