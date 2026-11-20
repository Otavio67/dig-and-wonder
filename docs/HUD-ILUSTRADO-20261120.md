# HUD ilustrado e build Windows — 20/11/2026

## Pedido
O usuário pediu que eu definisse e aplicasse uma melhoria definitiva no HUD. Pediu também um `.exe` para testar no PC antes da validação Android.

## Diagnóstico (captura real do Unity, 540 × 960)
- Os menus (perfil, loja e derrota) já usavam a arte cartoon ilustrada aprovada em 31/10. A metade inferior do HUD continuava com chapas planas escuras e parecia de outro jogo.
- Faltando ouro, os botões MELHORAR ficavam cinza-escuros e o preço aparecia em coral. Isso contraria a seção 9 do guia: o coral é reservado a perigo.
- FORÇA flutuava solta entre os poderes e o painel, desperdiçando altura. Só duas linhas de melhoria cabiam.
- As barras de vida de inimigos vizinhos se encostavam e pareciam uma faixa única. A vida da broca tinha barra e número pequenos.
- A aba ativa era indicada só por um traço ciano fino.

## O que mudou (somente código; nenhuma arte nova gerada)
- **Missão:** passou a usar a placa ilustrada `skin_stat`. A recompensa aparece em âmbar com contorno.
- **Painel de melhorias:** chapa escura com friso de latão e título em Titan One âmbar. FORÇA foi para o cabeçalho, à direita. O painel cresceu até logo abaixo dos poderes e agora mostra duas linhas inteiras e parte da terceira, como pede o guia.
- **Linhas de melhoria:** usam a moldura ilustrada `skin_id`, e o ícone com o nível fica numa caixa própria.
- **Ação:** verde ilustrado (`skin_action`) quando é possível comprar e aço azul (`skin_tab`) dessaturado quando falta ouro, com o preço ainda legível. No nível máximo usa ciano (`skin_tab_active`). Bloqueado por requisito fica mais escuro.
- **Navegação:** a aba ativa usa a placa ciano com cristal (`skin_tab_active`), a mesma da loja. O dock ganhou um friso de latão contínuo até as bordas.
- **Combate:** as barras de inimigos têm recuo lateral e ficaram mais altas. A vida da broca tem barra mais grossa e número maior com contorno. Os textos de status sobre o cenário ganharam contorno.
- **Derrota:** o retrato procurava o frame `hurt_00`, que não existe. Agora usa `action_03`.

Economia, save, regras de combate e catálogos não foram alterados.

## Segunda rodada — ícones, textos e botões
O usuário pediu mais ajustes em todas as áreas, principalmente ícones, textos fora do lugar e botões. A revisão usou capturas em 1080 × 1920 (`DW_UI_REVIEW_SUPERSIZE=2`), recortadas por região.
- **Topo:** o retrato mostra a broca inteira, e não mais cortada.
- **Cenário:** a vida da broca ganhou o ícone dedicado de coração ao lado do número.
- **Missão:** o texto começa depois da tampa dourada e ficou maior.
- **Poderes e AUTO:** o círculo mostra só o ícone ou o AUTO, centralizado e maior. O estado (ON/OFF, recarga, 0/5, NV.x) foi para uma linha própria abaixo, sem cruzar o aro.
- **Linhas de melhoria:** a caixa do ícone cobre a peça de latão que aparecia pela borda e o valor ficou maior e mais claro. "MELHORAR" e o preço ficam dentro do miolo do botão, sem encostar na moldura. Bloqueado e Máximo centralizam o texto.
- **Navegação:** a placa ciano inteira fica atrás do ícone, sem esticar em nove partes. O rótulo fica abaixo dela, em ciano na aba ativa, e todos os rótulos ganharam contorno.
- **Compilação com stubs:** recebeu as APIs usadas pelo build Windows e por `TMP margin`. Na primeira rodada esse check quebrava, o que teria falhado o CI.

## Terceira rodada — moldura, combate, navegação e outras telas
O usuário escolheu as quatro frentes: moldura das melhorias, combate no cenário, ícones da navegação e outras telas.
- **Moldura das melhorias:** chapa azul-aço com contorno fino de latão, no lugar das tampas douradas grandes da `skin_id`.
- **Combate:** barras de inimigos com contorno escuro e números de dano maiores (32) com contorno mais grosso. As mensagens de status ficam sobre uma faixa translúcida, que só aparece quando há texto.
- **Navegação:** área do ícone maior e escala óptica por ícone (picareta ×1,18, chave ×1,12, escudo e cadeado ×0,96), para que diagonais finas e formas cheias pareçam do mesmo tamanho.
- **Oficina** (abas Oficina, Poderes, Tesouro e Brocas):
  - Fundo ilustrado escurecido e placa de título igual à dos menus. O fechar usa o disco coral ilustrado.
  - Os botões usam a arte ilustrada por estado: verde para ação, aço para indisponível e ciano para selecionado.
  - Os painéis cinza-claros passaram a aço escuro translúcido, e as setas de página ficaram compactas.
  - A aba Oficina mostra as melhorias no espaço que antes ficava vazio. A mensagem de status ganhou uma faixa própria.
  - O cartão da garagem ficou mais alto, com o botão EQUIPAR legível.
- **Equipamentos e Loja:** o botão do cartão saiu de cima do aro dourado. O rodapé de Equipamentos fica numa faixa escura.
- **Revisão:** `UiVisualReview` com `DW_UI_REVIEW_EXTRA=1` também captura Modos, Equipamentos e as quatro páginas da Oficina.

## Quarta rodada — revisão completa de telas, textos, ícones e botões
O usuário relatou textos deformados, ícones mal feitos e telas erradas. Pediu revisão de tudo usando jogos do gênero como referência, adaptados à identidade Toy-Mecha. As regras foram registradas em `IDENTIDADE-TOY-MECHA.md` (seção de 20/11/2026).
- **Textos deformados (causa):** `Nunito-Reading.ttf` era uma fonte variável. O Unity a desenhava no peso ExtraLight padrão, sem as métricas corretas, e ainda havia negrito sintético sobre uma fonte ExtraBold. Correção: Nunito estática SemiBold e Bold, negrito sintético desligado e regra `UiTypography.Normalize` aplicada no HUD, nos menus e na Oficina.
- **Ícones:** a família Toy-Mecha v3 foi gerada por `scripts/export-mecha-icons-v3.cjs`, que usa o Edge headless e não depende de pacotes. Tem 24 ícones, incluindo cadeado, chave, cristal e moeda.
- **Garagem:** cada cartão mostra a broca real tingida com a cor dela. As bloqueadas aparecem como silhueta com cadeado. A equipada tem borda ciano e a selecionada, borda âmbar.
- **Tesouro e carteira:** vitrines com baú, chave e moeda acima dos botões. A carteira da Oficina virou duas pílulas com ícone, no lugar do texto "ouro/cristais".
- **Poderes:** cada linha ganhou o ícone do poder. **Modos:** setas e botão de treino com a arte ilustrada.
- **Ajuda e Álbum:** fundo ilustrado, placa de título, painel de leitura, abas que indicam a página ativa e cartões de descoberta com contorno de latão.
- **Configurações e Perfil:** blocos de leitura em chapa escura, botões de volume do tamanho certo para o toque e ID afastado das ferragens.
- **Cartões da Loja e de Equipamentos:** o botão ficou mais alto e o texto, legível.
- **Revisão:** capturas 2x de todas as telas, incluindo Ajuda e Álbum (`UiVisualReview` com `DW_UI_REVIEW_EXTRA=1`).

## Quinta rodada — vitória, chefe, derrota e feedback
O usuário escolheu esses dois blocos. Antes, vitória, chefe e broca nova eram só uma linha de texto de status, e os botões do HUD não reagiam ao toque.
- **Faixas de momento** (`PrototypeGameUI.Moments.cs`): placa de título, faixa escura com contorno na cor do momento e subtítulo. Entram em 0,28 s (escala elástica), ficam visíveis e saem. Uma fila evita sobreposição:
  - "GUARDIÃO!" ou "MINI-CHEFE!" em coral, com emblema;
  - "FASE CONCLUÍDA!" em âmbar, com o ouro ganho na fase e a próxima fase;
  - "NOVA BROCA!" em ciano, com a broca tingida na cor dela. Aparece depois da vitória.
- As faixas aparecem só depois do evento real da simulação e são limpas na derrota. Não alteram combate, recompensa nem save.
- **Barra do chefe:** chapa escura com contorno coral, emblema de perigo, barra grossa com reflexo e nome com vida. Pisca em creme durante a preparação do ataque. Fica abaixo da placa da fase.
- **Derrota:** o popup ilustrado foi mantido, agora com a tipografia normalizada e resposta ao toque.
- **Feedback:**
  - `UiPressFeedback` em todos os botões (HUD, menus, derrota e cartões da garagem): o botão afunda em 80 ms e volta com leve elasticidade.
  - O texto "MELHORAR" pulsa devagar enquanto a compra é possível.
  - Ao comprar, a linha pulsa e o nível acende em ciano.
  - Ajuda e Álbum entram com animação curta (0,2 s).
- **Verificação:** capturas reais de chefe, vitória e broca liberada (forçadas no perfil de revisão, sem mudar a fase salva) e reinício da derrota confirmado. As animações foram verificadas só pelo código e pela execução sem erros. Ainda não foram avaliadas em movimento por uma pessoa.

## Build Windows
Menu **Dig & Wonder → Build Windows (.exe)** ou linha de comando:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -batchmode -quit -projectPath UnityProject -buildTarget Win64 -executeMethod DigAndWonder.Editor.PrototypeProjectSetup.BuildWindowsPlayer -buildOutput "<pasta>\DigAndWonder.exe"
```

A janela abre em retrato 540 × 960 e pode ser redimensionada. O mouse substitui o toque. O build usa o save normal do jogador no PC (persistentDataPath do Windows), separado do aparelho Android. Para voltar ao Android, o build de APK troca a plataforma de novo.

## Verificação
- Compilação com stubs: 0 erros, 0 avisos. O Editor 6000.6.6f1 compilou sem erros.
- `UiVisualReview` rodou em Play Mode com o perfil de revisão separado. Ele capturou HUD, perfil, loja, configurações e derrota e confirmou o reinício após 10 s.
- **Não verificado:** toque real, desempenho e safe area em aparelho Android. Proporções diferentes de 9:16 também não foram testadas. A aprovação artística final é do usuário.
