# Dig & Wonder — desenvolvimento Unity

Referência atual após a retirada do projeto Godot em 29/10/2026. O histórico anterior permanece nos commits do Git. A limpeza não altera números, recompensas ou formato de save.

## Ciclo principal

Broca à esquerda, voltada para a direita; inimigos chegam pela direita. Combate automático lateral em retrato, com ouro por inimigo derrotado, melhorias durante a tentativa e progressão por fases. A apresentação anima eventos reais da simulação, sem calcular dano ou conceder recompensas por conta própria.

O menu inicial mostra a arte aprovada e prepara recursos antes de liberar **Iniciar mineração**. A ação começa ou retoma o perfil. Vitória avança para a próxima fase/mapa sem voltar ao menu inicial; derrota permite melhorar e recomeçar. A campanha termina na fase 100, com treino depois dela.

## Progressão e economia

- Sete melhorias compartilhadas: ATQ, VIDA, AUTORREPARO, CADÊNCIA, CHANCE CRÍTICA, DANO CRÍTICO e ESPECIAL.
- XP individual das brocas até nível 60 e quatro poderes por personagem. Novas brocas são concedidas ao vencer os marcos de dez fases, sempre no nível zero.
- Ouro, cristais, baús e chaves usam as regras existentes. A loja usa as transações do tesouro; isto não comprova compras com dinheiro real.
- AUTO alterna o ataque básico. O especial mantém sua automação e permite toque quando pronto; os demais módulos abrem os poderes existentes na Oficina.
- Abrir a Oficina pausa a tentativa. Não mudar essa regra só por causa de um mockup.

Regras numéricas: `UnityProject/Assets/_Game/Resources/Data/CombatBalance.asset`, `Data/Economy/midgame.json` e os catálogos JSON em `content/`. O `ContentCatalogImporter` converte os dados de combate e fases em ScriptableObjects sem depender de outra engine. Consulte [economia](ECONOMIA-MIDGAME.md), [campanha](ENDGAME-100.md) e [brocas por fase](BROCAS-POR-FASE.md).

## Arquitetura e save

`IdleCombatSimulation` controla combate e estados terminais; `AutoBattleDriver` faz a ponte com Unity e persistência; `PrototypeGameUI` apresenta o HUD uGUI/TMP. Animações e efeitos acompanham IDs estáveis e eventos do simulador.

O perfil esquema 3 fica em `Application.persistentDataPath`. Gravação atômica, checksum, backups, migrações e retomada preservam progresso. Arquivo inválido não autoriza reset silencioso. Compras e combate pausam quando a gravação falha. Não apagar nem substituir perfis do usuário em testes.

## Arte e conteúdo

A identidade vigente é [Toy-Mecha Subterrâneo](IDENTIDADE-TOY-MECHA.md). Textos e valores permanecem no Unity; ícones têm SVG editável em `assets/ui/gameplay/`. O cenário, as unidades e as barras precisam compartilhar a linha de chão. A revisão visual final do HUD e o posicionamento de barras ainda têm pendências.

As onze brocas funcionam no domínio, mas reutilizam provisoriamente a arte da Pioneira. Pets, Torre/Profundezas, cinquenta inimigos futuros e arte exclusiva do elenco não estão concluídos. Catálogos de conteúdo futuro não são evidência de presença no jogo.

## Validar e continuar

Use os checks .NET e o CI Unity; depois confirme importação, Play Mode, botões, scroll, save/retomada e combate no Editor. Android exige APK, aparelho, toque, safe area e medição de desempenho. Não ampliar conteúdo durante uma tarefa de limpeza ou correção pontual.
