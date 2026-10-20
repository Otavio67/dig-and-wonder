# Dig & Wonder

**Projeto ativo: Unity 6**, com Android como destino inicial. Esta é a referência principal para abrir, testar e desenvolver o jogo. O projeto Godot na raiz foi preservado como origem de dados, arte e histórico; não é a versão em desenvolvimento agora.

## Protótipo atual

A amostra Unity é um jogo 2D retrato de combate automático com tema de escavação. A broca enfrenta inimigos, recebe ouro e permite comprar melhorias. Em um perfil novo, o jogador toca em **Iniciar escavação**; a luta e os ataques acontecem automaticamente.

O runtime do checkpoint **0.0.19** estende a campanha jogável até a fase **100**. Vencer as fases **10, 20, 30, 40, 50, 60, 70, 80, 90 e 100** concede novas brocas, sempre no nível 0; a fase 100 é o fim da campanha atual e depois dela fica disponível o treino. O checkpoint de conteúdo **0.0.22** mantém a bíblia e as faixas progressivas com cinco inimigos novos a cada dez fases, e acrescenta um briefing visual detalhado para criar os sprites dos 50 inimigos. Esse conteúdo futuro ainda não aparece na cena. Ouro/cristais, XP individual até nível 60, sete melhorias, quatro poderes por personagem, baús com chave e save esquema 3 continuam compatíveis. A dificuldade sobe em degraus a cada cinco fases. Regras e conteúdo do end game estão em [ENDGAME-100.md](docs/ENDGAME-100.md), [CONTENT-BIBLE-ENDGAME.md](docs/CONTENT-BIBLE-ENDGAME.md) e [BRIEF-SPRITES-INIMIGOS.md](docs/BRIEF-SPRITES-INIMIGOS.md). A interface ainda precisa de revisão no Unity e em aparelho Android.

A amostra visual da Broca Pioneira agora tem locomoção de chão separada da lógica: caminhada curta, avanço de ataque, reação a dano e derrota. Uma folha PNG transparente derivada da arte enviada já está no checkout; a importação e a ligação dos frames estão documentadas em [SPRITE-BROCA-UNITY.md](docs/SPRITE-BROCA-UNITY.md).

O Acampamento é uma tela de hub separada do HUD de combate: oficina, garagem, baús, álbum e bloqueios de fase ficam na base; a escavação mantém apenas informações e controles da tentativa.

A Oficina abre uma tela de personagem com vitrine e abas **Oficina, Poderes, Tesouro e Brocas**. A **Garagem** do hub abre a coleção; brocas bloqueadas mostram a fase necessária e as possuídas podem ser equipadas entre tentativas. Existem onze brocas funcionais, com arte provisória da Pioneira reutilizada em cores distintas. XP e poderes são individuais; melhorias da oficina são compartilhadas. Save Unity esquema 3 preserva a coleção e migra esquemas 1/2 com backup.

O smoke test histórico em Play Mode (sete ataques/10 segundos, sete de ouro e 112/120 PV) pertence ao protótipo anterior ao novo balanceamento. A 0.0.19 passou em **75 checks de domínio** e compilação C# contra DLLs reais; importação e Play Mode desta versão estão pendentes por falta de licença do Editor na nuvem. Não há medição Android.

## Abrir e executar

1. Abra a pasta `UnityProject` no Unity Hub com Unity **6000.0.23f1**.
2. Aguarde a importação e a resolução dos pacotes.
3. Abra `Assets/_Game/Scenes/Prototype.unity` e pressione **Play**.
4. Em um perfil novo, toque ou clique em **Iniciar escavação**.

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
