# Dig & Wonder

**Projeto ativo: Unity 6**, com Android como destino inicial. Esta é a referência principal para abrir, testar e desenvolver o jogo. O projeto Godot na raiz foi preservado como origem de dados, arte e histórico; não é a versão em desenvolvimento agora.

## Protótipo atual

A amostra Unity é um jogo 2D retrato de combate automático com tema de escavação. A broca enfrenta inimigos, recebe ouro e permite comprar melhorias. Em um perfil novo, o jogador toca em **Iniciar escavação**; a luta e os ataques acontecem automaticamente.

Esta ainda é uma base de protótipo. A interface e os ícones são provisórios; abertura de baús, equipamentos, álbum de descobertas e novas regiões não estão implementados. O jogo ainda não passou por revisão visual, teste de toque em aparelho Android ou medição de desempenho.

A amostra visual da Broca Pioneira agora tem locomoção de chão separada da lógica: caminhada curta, avanço de ataque, reação a dano e derrota. Uma folha PNG transparente derivada da arte enviada já está no checkout; a importação e a ligação dos frames estão documentadas em [SPRITE-BROCA-UNITY.md](docs/SPRITE-BROCA-UNITY.md).

O Acampamento é uma tela de hub separada do HUD de combate: oficina, garagem, baús, álbum e bloqueios de fase ficam na base; a escavação mantém apenas informações e controles da tentativa.

A Oficina abre uma tela de personagem com a Pioneira no pedestal ciano, quatro slots de peças, módulos/passivos e abas Oficina, Peças e Drones. Mostra ATQ/VIDA reais, custos e reforços comprados. Pode ser acessada também pela aba **Personagem** no combate; a expedição pausa enquanto o menu está aberto. Direção visual, testes e limitações em [OFICINA-PERSONAGEM.md](docs/OFICINA-PERSONAGEM.md).

Um smoke test em Play Mode passou no Unity 6000.0.23f1: a luta simulou 10 segundos, com sete ataques automáticos, sete de ouro, 112/120 de vida e nenhum erro ou exceção de runtime. Isso não valida interação física, aparência final ou desempenho no Android; veja os detalhes em [revisão de combate e save](docs/UNITY-REVISAO-COMBATE-SAVE.md).

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
