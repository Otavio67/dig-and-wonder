# Dig & Wonder — protótipo Unity 6

Este é o primeiro protótipo de combate automático lateral. O projeto Godot permanece no repositório como origem dos valores e de parte da arte. O fluxo Unity preserva a fantasia de escavação, a broca como personagem, inimigos, ouro por derrota, melhorias e chefes como condição de avanço.

## O que está nesta amostra

- Em perfil novo, aparece **Iniciar escavação**. Em perfil Unity já salvo, a luta abre direto na última posição segura.
- A broca e os inimigos lutam automaticamente. A Pioneira faz quatro impactos somando os 32 de dano da fonte Godot. O boss é o único requisito para limpar a fase; ao vencê-lo, o perfil recebe o próximo estágio e os baús/chaves definidos pelo catálogo.
- Menus em português para Mina, Melhorias, Brocas, Baús e Perfil. Dano e Vida são compráveis com ouro. Brocas e Baús mostram o estado atual do catálogo; abertura de baús e equipamentos ainda não estão implementados nesta amostra.
- UI retrato, input de mouse/toque por botões, layout com CanvasScaler e ajuste de área segura.
- A interface de combate é uma composição provisória feita por código e texto TMP, com ícones provisórios; não é a arte/layout final. O cenário dá movimento de caverna por tween, separado da lógica de combate. A camada visual ainda não muda com a fase.
- `ProceduralAnimator2D` anima escala somente no elemento visual e interrompe idle ao atacar/receber impacto/morrer. Pools aquecidos de projéteis (24), dano (32), moedas (8) e impactos/partículas (32). Efeitos usam DOTween em tempo não escalado e são limpos em derrota, vitória e suspensão do app. A simulação não instancia objetos visuais por ataque.

## Abrir

1. Instale Unity **6000.0.23f1** com suporte Android (SDK, NDK e OpenJDK embutidos) se for compilar para celular.
2. Abra `UnityProject` no Unity Hub e deixe o Package Manager resolver `Packages/manifest.json`.
3. Ao abrir no Editor, o importador converte os JSON e o Theme Godot em ScriptableObjects e o preparador cria a cena vazia `Assets/_Game/Scenes/Prototype.unity`, define retrato/Input System e inclui a cena nos builds.
4. Entre em Play. Em perfil novo, toque/click **Iniciar escavação**. Feche e abra novamente para conferir a retomada; o perfil válido entra direto no combate.

O save fica em `Application.persistentDataPath`, não no cache temporário que o Android pode limpar. Ele grava em gatilhos de recompensa/melhoria/chefes, a cada 5 s e ao suspender/fechar; o arquivo é validado por checksum e recupera backup. Esse protótipo ainda não importa automaticamente o save Godot.

## Toolchain na nuvem

O Unity 6000.0.23f1 com módulo Android está instalado no ambiente remoto. Para reproduzir a instalação Linux x86_64 e habilitar os comandos:

```bash
python3 scripts/setup-unity-cloud.py
source scripts/unity-env.sh
unity -version
bash scripts/check-unity.sh
```

O Editor fornece a interface de linha de comando, incluindo `-batchmode`; não é necessário um CLI Unity separado. A tentativa real de importação parou por falta de licença Unity ativada nesta máquina. O registro `packages.unity.com` também está bloqueado pela política de rede. Detalhes, fontes e resultados em [UNITY-NUVEM.md](../docs/UNITY-NUVEM.md).

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

Os custos de Crítico, Crítico+ e Especial não existem no combate Godot ativo; não foram ativados nem tiveram números inventados. O catálogo antigo fornece PV de boss desproporcionais ao dano do combate automático, então esses valores foram preservados e ainda precisam de balanceamento antes de considerar o desafio finalizado.

## Limites deste checkpoint

- Editor/CLI e Android instalados via GameCI com SHA-256 verificado. O processo batch real iniciou e foi interrompido pela ausência de licença Unity. Importação, compilação pelo Editor, Play Mode e APK continuam bloqueados; `UnityApiCompileCheck` e `UnityCompileCheck` passaram sem erros/avisos, e os dez checks de domínio passaram.
- URP 2D, Cinemachine 3, Input System, TMP, 2D Animation, Unity UI e Unity Test Framework estão declarados no Package Manager. O bootstrap usa Input System, UI e TMP; o pipeline URP/Renderer 2D e a câmera Cinemachine ainda precisam ser abertos e verificados no Editor.
- Recursos essenciais TMP são importados automaticamente pelo preparador quando o pacote local disponibiliza o `.unitypackage`; se a fonte aparecer sem glifos no primeiro Play, execute **Window → TextMeshPro → Import TMP Essential Resources**.
- Não foi feita revisão visual, gravação nem medição de desempenho; não há afirmação de 60 FPS. APK, instalação/toque real, safe area por aparelho, memória e desempenho aguardam Editor/Android e aparelho.
- Não há anúncios Unity ligados. A prova de rewarded existente é separada do gameplay Godot; anúncios não interrompem nem são requisito desta amostra.
- Nesta fatia existe somente ouro e chaves/baús guardados; não há moeda premium, lootbox aberta, equipamento aplicável, discovery/álbum ou fase de escavação surpresa no gameplay Unity.

## Licenças

DOTween está copiado sem modificações do repositório oficial; origem, commit upstream, README e licença acompanham `Assets/_Game/ThirdParty/DOTween`. A licença permite redistribuição do pacote conforme seus termos. Pacotes Unity mantêm suas próprias licenças. O tema gráfico nesta amostra é composto por formas e cores provisórias.
