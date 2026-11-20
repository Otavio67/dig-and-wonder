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
