# Dig & Wonder

Jogo 2D em retrato de combate automático e exploração subterrânea, desenvolvido em **Unity 6** para Android. Identidade visual: Toy-Mecha Subterrâneo, com brocas mecânicas, cristais e oficina.

## Projeto atual

O projeto executável fica em `UnityProject/`. A branch ativa é **`prototype/first-playable`**. O checkpoint **0.0.27** remove a implementação antiga em Godot e mantém o projeto Unity, seus catálogos JSON e as fontes vetoriais dos ícones atuais. O histórico anterior permanece nos commits do Git.

A campanha vai até a fase 100, com ouro, cristais, XP individual das brocas, sete melhorias, quatro poderes por personagem, baús com chave e save esquema 3. A cada dez fases vencidas é liberada uma nova broca no nível zero. Depois da fase 100 há treino. Pets e Torre/Profundezas continuam como conteúdo futuro.

Alterações locais em desenvolvimento organizam as 100 fases em dez capítulos: ondas de 3–5 inimigos, mini-chefe acompanhado na fase 5 e chefe sozinho na fase 10. As recompensas e o dano das ondas foram ajustados; [regras e limites](docs/DESENVOLVIMENTO.md). Essa versão passou por testes de lógica, mas ainda exige Play Mode e revisão visual no Editor.

A tela inicial prepara recursos reais antes de liberar **Iniciar mineração**. A vitória segue para o próximo mapa; a derrota permite melhorar e recomeçar dentro da partida. Oficina, poderes, tesouro e coleção ficam na navegação. A arte exclusiva das onze brocas, o alinhamento visual final e a validação Android ainda precisam de trabalho.

## Abrir o jogo

1. Abra o Unity Hub, confira a licença e adicione a pasta `UnityProject`.
2. Use a versão indicada em [ProjectVersion.txt](UnityProject/ProjectSettings/ProjectVersion.txt): **6000.6.6f1** neste checkpoint.
3. Aguarde a importação, abra `Assets/_Game/Scenes/Prototype.unity` e pressione Play.
4. Clique em **Iniciar mineração** para iniciar ou retomar o perfil.

## Estrutura

| Pasta | Conteúdo |
| --- | --- |
| `UnityProject/` | Assets, pacotes, configurações e verificadores C# do jogo ativo |
| `content/` | Catálogos JSON independentes de engine; dados de combate e conteúdo futuro |
| `assets/ui/gameplay/` | Fontes SVG editáveis dos ícones atuais |
| `scripts/` | Exportação de assets e ferramentas de apoio Unity |
| `docs/` | Identidade, design, economia e evidências com seus limites |

## Verificação

Com .NET 8, a partir da raiz do repositório:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

O CI usa essas mesmas verificações. Elas não substituem importação e Play Mode no Editor nem testes de toque/desempenho em aparelho. Uma captura da 0.0.26 foi renderizada no Unity em 28/10/2026, com frames avançando e um inimigo em combate; mostrou problemas visuais ainda pendentes. Não há resultado de FPS Android comprovado.

## Documentação

- [Estado e limpeza Unity — 0.0.27](docs/UNITY-ONLY-0.0.27.md)
- [Guia do projeto Unity](UnityProject/README.md)
- [Regras atuais de desenvolvimento](docs/DESENVOLVIMENTO.md)
- [Identidade visual e checagem de imagens](docs/IDENTIDADE-TOY-MECHA.md)
- [Economia e progressão](docs/ECONOMIA-MIDGAME.md)
- [Campanha até a fase 100](docs/ENDGAME-100.md)
- [Proveniência dos assets](assets/LICENCAS.md)

Caches, ferramentas locais, saves de teste, capturas e APKs ficam fora do Git. Tornar o repositório público não altera as licenças das fontes ou dependências.
