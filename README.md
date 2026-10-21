# Dig & Wonder

**Projeto ativo: Unity 6**, com Android como destino inicial. Esta é a referência principal para abrir, testar e desenvolver o jogo. O projeto Godot na raiz foi preservado como origem de dados, arte e histórico; não é a versão em desenvolvimento agora.

## Protótipo atual

A amostra Unity é um jogo 2D retrato de combate automático com tema de escavação. A broca enfrenta inimigos, recebe ouro e permite comprar melhorias. A versão **0.0.24** abre com arte cartoon e logo aprovada. Uma barra branca acompanha a preparação; ao concluir, aparece **INICIAR MINERAÇÃO** em branco, sem placa. O botão inicia ou retoma o perfil; vitórias seguem automaticamente para o próximo mapa, sem retornar ao título. Arte, fluxo e limitações estão em [MENU-INICIAL.md](docs/MENU-INICIAL.md) e [CARREGAMENTO-INICIAL.md](docs/CARREGAMENTO-INICIAL.md).

O runtime do checkpoint **0.0.19** estende a campanha jogável até a fase **100**. Vencer as fases **10, 20, 30, 40, 50, 60, 70, 80, 90 e 100** concede novas brocas, sempre no nível 0; a fase 100 é o fim da campanha atual e depois dela fica disponível o treino. O checkpoint de conteúdo **0.0.22** mantém a bíblia e as faixas progressivas com cinco inimigos novos a cada dez fases, e acrescenta um briefing visual detalhado para criar os sprites dos 50 inimigos. Esse conteúdo futuro ainda não aparece na cena. Ouro/cristais, XP individual até nível 60, sete melhorias, quatro poderes por personagem, baús com chave e save esquema 3 continuam compatíveis. A dificuldade sobe em degraus a cada cinco fases. Regras e conteúdo do end game estão em [ENDGAME-100.md](docs/ENDGAME-100.md), [CONTENT-BIBLE-ENDGAME.md](docs/CONTENT-BIBLE-ENDGAME.md) e [BRIEF-SPRITES-INIMIGOS.md](docs/BRIEF-SPRITES-INIMIGOS.md). A interface ainda precisa de revisão no Unity e em aparelho Android.

A amostra visual da Broca Pioneira agora tem locomoção de chão separada da lógica: caminhada curta, avanço de ataque, reação a dano e derrota. Uma folha PNG transparente derivada da arte enviada já está no checkout; a importação e a ligação dos frames estão documentadas em [SPRITE-BROCA-UNITY.md](docs/SPRITE-BROCA-UNITY.md).

O menu inicial substituiu o Acampamento. Oficina, coleção de brocas e tesouro ficam na navegação durante a partida. A derrota permite melhorar e recomeçar ali mesmo; a vitória da fase 100 encerra a campanha sem avançar além do conteúdo disponível.

A Oficina abre uma tela de personagem com vitrine e abas **Oficina, Poderes, Tesouro e Brocas**. Brocas bloqueadas mostram a fase necessária e as possuídas podem ser equipadas entre tentativas. Existem onze brocas funcionais, com arte provisória da Pioneira reutilizada em cores distintas. XP e poderes são individuais; melhorias da oficina são compartilhadas. Save Unity esquema 3 preserva a coleção e migra esquemas 1/2 com backup.

O teste de Play Mode de 20/10/2026 está em [PLAYMODE-LOCAL-2026-10-20.md](docs/PLAYMODE-LOCAL-2026-10-20.md) e é anterior ao menu novo. A **0.0.24** passou em **79 checks de domínio**, compilação com stubs e contra DLLs reais do Editor antigo. Importação e Play Mode desta versão estão pendentes: a nuvem não tem o Editor 6000.6.6f1 vigente instalado. Há uma prévia de composição fora do Unity; não há medição Android.

## Abrir e executar

1. Abra a pasta `UnityProject` no Unity Hub com a versão de `ProjectVersion.txt` (**6000.6.6f1** neste checkpoint).
2. Aguarde a importação e a resolução dos pacotes.
3. Abra `Assets/_Game/Scenes/Prototype.unity` e pressione **Play**.
4. Toque ou clique em **Iniciar mineração**, inclusive para retomar uma tentativa salva.

Mais detalhes de configuração, save, APK e limitações estão em [UnityProject/README.md](UnityProject/README.md), [contexto local](docs/UNITY-CONTEXTO-LOCAL.md) e [revisão de combate e save](docs/UNITY-REVISAO-COMBATE-SAVE.md).

## Verificações disponíveis

Com .NET 8, os checks de domínio e compilação dos scripts podem ser executados sem abrir o Editor:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

Esses checks não substituem Play Mode, inspeção visual ou teste no Android. Os resultados e seus limites estão registrados nos documentos acima. APKs e arquivos locais de ferramentas ficam em `.tools/` e não devem ser enviados ao Git.

## Histórico Godot

Os documentos da versão anterior Godot permanecem disponíveis para referência e preservação de conteúdo. Para evitar confusão, use os documentos `UnityProject/` e `docs/UNITY-*.md` para a direção atual do projeto.
