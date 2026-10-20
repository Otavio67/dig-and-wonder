# Dig & Wonder — protótipo Unity 6

Este é o primeiro protótipo de combate automático lateral. O projeto Godot permanece no repositório como origem dos valores e de parte da arte. O fluxo Unity preserva a fantasia de escavação, a broca como personagem, inimigos, ouro por derrota, melhorias e chefes como condição de avanço.

No PC local (20/10/2026), Unity **6000.0.23f1** e o toolchain Android foram instalados. A preparação/importação real terminou e um APK ARM64 assinado com a chave de depuração foi gerado em `.tools/build/DigAndWonder-debug.apk`; `aapt` leu o pacote e `apksigner` confirmou a assinatura. O APK ainda não foi instalado em aparelho. Consulte o [estado local e próximos passos](../docs/UNITY-CONTEXTO-LOCAL.md).

Para continuar no seu PC, consulte [transferência e configuração local](../docs/CONTINUAR-UNITY-LOCAL.md), com mensagem pronta para a nova conversa Codex.

## O que está nesta amostra

- Em perfil novo, aparece **Iniciar escavação**. Em perfil Unity já salvo, a luta abre direto na última posição segura.
- A broca e os inimigos lutam automaticamente. A Pioneira faz quatro impactos somando os 32 de dano da fonte Godot. O boss é o único requisito para limpar a fase; ao vencê-lo, o perfil recebe o próximo estágio e os baús/chaves definidos pelo catálogo.
- Hub de acampamento separado do HUD de combate. O hub em português mostra a broca, Mina, Oficina, Garagem, Baús, Álbum e Perfil. A faixa de missão fica no HUD durante a escavação; a oficina exibe ATQ/VIDA com custo, máximo e melhorias futuras bloqueadas; a garagem agrupa brocas por fase; e Baús mostra a carteira e os módulos bloqueados por progresso. Dano e Vida são compráveis com ouro. Abertura de baús, equipamentos aplicáveis e álbum de descobertas ainda não estão implementados nesta amostra.
- A Oficina abre pelo hub e pela aba **Personagem** no combate. Tem vitrine da Pioneira, quatro slots de peças, módulos/passivos e navegação Oficina/Peças/Drones. Mostra ATQ/VIDA, ouro e a barra de XP individual da personagem equipada. As compras preservam os dados Godot. A tentativa pausa enquanto essa tela está aberta e volta ao mesmo estado ao fechar. Peças e Drones são consultas de conteúdo ainda em preparação. Arte e fontes em [OFICINA-PERSONAGEM.md](../docs/OFICINA-PERSONAGEM.md).
- XP pertence a cada broca por ID estável. Personagens novas começam no nível 0; a guardada conserva seu XP. Curva inicial: 10 XP por inimigo comum, 50 por chefe, requisito 20 + 10 × nível, teto configurável 50. Save esquema 2 migra a versão Unity 1 preservando economia/tentativa. Regras e 39 checks em [EXPERIENCIA-PERSONAGENS.md](../docs/EXPERIENCIA-PERSONAGENS.md).
- UI retrato, input de mouse/toque por botões, layout com CanvasScaler e ajuste de área segura.
- A interface de combate é uma composição provisória feita por código e texto TMP, com ícones provisórios; não é a arte/layout final. O cenário dá movimento de caverna por tween, separado da lógica de combate. A camada visual ainda não muda com a fase.
- `DrillSpriteSheetPlayer2D` procura a folha RGBA da Pioneira em `Resources/Art/Drill`, escolhe frames por eventos reais e mantém o ícone TMP se a importação ainda não estiver pronta. `ProceduralAnimator2D` anima escala somente no elemento visual e interrompe idle ao atacar/receber impacto/morrer. `GroundDrillMotion2D` cuida do deslocamento cosmético no chão e nunca move a posição funcional. Pools aquecidos de projéteis (24), dano (32), moedas (8) e impactos/partículas (32). Efeitos usam DOTween em tempo não escalado e são limpos em derrota, vitória e suspensão do app. A simulação não instancia objetos visuais por ataque.

A avaliação e o passo a passo para importar a folha de sprites da Broca Pioneira estão em [SPRITE-BROCA-UNITY.md](../docs/SPRITE-BROCA-UNITY.md).

## Abrir

1. Instale Unity **6000.0.23f1** com suporte Android (SDK, NDK e OpenJDK embutidos) se for compilar para celular.
2. Abra `UnityProject` no Unity Hub e deixe o Package Manager resolver `Packages/manifest.json`.
3. Ao abrir no Editor, o importador converte os JSON e o Theme Godot em ScriptableObjects e o preparador cria a cena vazia `Assets/_Game/Scenes/Prototype.unity`, define retrato/Input System e inclui a cena nos builds.
4. Entre em Play. Em perfil novo, toque/click **Iniciar escavação**. Feche e abra novamente para conferir a retomada; o perfil válido entra direto no combate.

O save fica em `Application.persistentDataPath`, não no cache temporário que o Android pode limpar. Ele grava em gatilhos de recompensa/melhoria/chefes, a cada 5 s e ao suspender/fechar; o arquivo é validado por checksum e recupera backup. Esse protótipo ainda não importa automaticamente o save Godot.

O checkpoint de correções posterior à 0.0.7 limpa ações ao encerrar a luta, salva ao fim do passo e recupera também temporário válido quando principal/backup não podem ser lidos. Falha de escrita pausa o combate e tenta novamente. Veja [correções e roteiro pendente no Editor](../docs/UNITY-REVISAO-COMBATE-SAVE.md).

## Toolchain na nuvem

O Unity 6000.0.23f1 com módulo Android está instalado no ambiente remoto. Para reproduzir a instalação Linux x86_64 e habilitar os comandos:

```bash
python3 scripts/setup-unity-cloud.py
source scripts/unity-env.sh
unity -version
bash scripts/check-unity.sh
```

O Editor fornece a interface de linha de comando, incluindo `-batchmode`; não é necessário um CLI Unity separado. A tentativa real de importação parou por falta de licença Unity ativada nesta máquina. O registro `packages.unity.com` também está bloqueado pela política de rede. Detalhes, fontes e resultados em [UNITY-NUVEM.md](../docs/UNITY-NUVEM.md).

O comando `unity` criado por `scripts/unity-env.sh` é um wrapper do executável do Editor. Ele não é o produto Unity CLI e não aceita comandos como `unity auth login` ou `unity license activate`.

O servidor Coplay MCP 10.0.0 está preparado e registrado no Codex CLI remoto. O protocolo respondeu, mas a consulta encontrou zero Editores conectados. A ponte foi declarada no manifesto; sua importação e a ativação Personal aguardam autenticação/rede. Comandos e evidências em [UNITY-MCP.md](../docs/UNITY-MCP.md).

## Checagens que rodam sem Unity Editor

No checkout do repositório, com .NET 8:

```bash
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

`DomainChecks` executa verificações de saldo/preço, recompensas únicas, duração/alvo de projéteis, quatro golpes do especial, regra do boss, retomada e recuperação de backup. O teste de paridade lê os dados Godot atuais. `UnityCompileCheck` compila os scripts Runtime/Editor contra stubs mínimos Unity e APIs simuladas de DOTween; é verificação estática, **não** substitui importação, compilação batchmode ou teste em Play.

Com o toolchain de nuvem instalado, `dotnet build UnityProject/Tools/UnityApiCompileCheck/UnityApiCompileCheck.csproj` também compila os fontes contra DLLs reais do Editor e do template 2D. Essa checagem usa os pacotes disponíveis no template, incluindo Input System 1.9.0, e não confirma a resolução das versões declaradas no projeto. O comando completo `scripts/check-unity.sh` tenta a importação real depois dessas checagens e retorna falha se o Editor não concluir.

## Paridade observada nos dados

| Sistema | Godot | Unity | Resultado |
| --- | ---: | ---: | --- |
| Broca/ataque básico | 6 de dano por tiro | 6 | Igual |
| Cadência básica | 1,25 s; segundo tiro após 0,28 s | 1,25 s; intervalo 0,28 s | Igual |
| Vida base | 120 | 120 | Igual |
| Pioneira | 32 em 4 impactos; cooldown 3 s | 32 em 4 impactos; cooldown 3 s | Igual |
| Torrão | 48 PV; 8 de dano; 7 ouro | 48 PV; 8 de dano; 7 ouro | Igual |
| Upgrade Dano | custo 18 + 14 × nível; +12%; máximo 10 | Mesma curva | Igual |
| Upgrade Vida | custo 20 + 16 × nível; +35 PV; máximo 10 | Mesma curva | Igual |

Os custos de Crítico, Crítico+ e Especial não existiam no combate Godot de origem; não foram ativados nem tiveram números inventados. O catálogo antigo fornece PV de boss desproporcionais ao dano do combate automático, então esses valores foram preservados e ainda precisam de balanceamento antes de considerar o desafio finalizado.

## Limites deste checkpoint

- O ambiente remoto de nuvem foi instalado via GameCI, mas o Editor remoto segue sem licença e os pacotes são bloqueados pelo proxy. Isso não impede o Editor local instalado acima. O projeto local já importou e gerou APK; Play Mode e teste em aparelho continuam pendentes.
- URP 2D, Cinemachine 3, Input System, TMP, 2D Animation, Unity UI e Unity Test Framework estão declarados no Package Manager. O bootstrap usa Input System, UI e TMP; o pipeline URP/Renderer 2D e a câmera Cinemachine ainda precisam ser abertos e verificados no Editor.
- Recursos essenciais TMP são importados automaticamente pelo preparador quando o pacote local disponibiliza o `.unitypackage`; se a fonte aparecer sem glifos no primeiro Play, execute **Window → TextMeshPro → Import TMP Essential Resources**.
- Não foi feita revisão visual, gravação nem medição de desempenho; não há afirmação de 60 FPS. Instalação/toque real, safe area por aparelho, memória e desempenho aguardam um aparelho Android.
- Não há anúncios Unity ligados. A prova de rewarded da versão Godot é histórica e separada do gameplay atual; anúncios não interrompem nem são requisito desta amostra.
- Nesta fatia existe somente ouro e chaves/baús guardados; não há moeda premium, lootbox aberta, equipamento aplicável, discovery/álbum ou fase de escavação surpresa no gameplay Unity.

## Licenças

DOTween está copiado sem modificações do repositório oficial; origem, commit upstream, README e licença acompanham `Assets/_Game/ThirdParty/DOTween`. A licença permite redistribuição do pacote conforme seus termos. Pacotes Unity mantêm suas próprias licenças. O tema gráfico nesta amostra é composto por formas e cores provisórias.
