# Polimento do HUD — 0.0.29

Rodada de 29/10/2026 orientada pela captura do usuário no Android: nível flutuando sobre a broca, pictogramas divergentes da referência, textos sobrepostos e upgrades substituídos por instruções.

## Comportamento implementado

- O nível e o XP da broca ficam no cartão superior esquerdo. O cenário mostra somente vida atual/máxima e uma barra junto à altura real do sprite, calculada por largura e proporção da folha.
- O botão superior de menu abre **Como jogar** em um painel separado. A lista de melhorias permanece visível por baixo; fechar devolve o combate ao estado anterior. O avanço automático aguarda o fechamento. O painel antigo de instruções não substitui mais os upgrades.
- A aba Mina mantém as sete melhorias na lista rolável, inclusive após derrota. Falha de save continua impedindo compras. Título, valor, nível, ação e preço têm regiões próprias. Requisitos, máximo e ouro insuficiente mantêm suas regras existentes.
- Pictogramas selecionados de Mina, Modos, Oficina, Baús, Loja, missão, ATQ, VIDA, AUTORREPARO e CADÊNCIA vêm da referência v8 existente. `hud-approved-v8.png` é cópia integral e inalterada do conceito; `BuildApprovedHudIcons` define os recortes de desenho sem textos funcionais e tem prioridade sobre substitutos em `Art/UI/Icons`. Álbum e ícones sem equivalente conservam seus recursos existentes. Não há arte nova gerada nesta rodada.
- Capítulo e onda recebem margens internas; missão e recompensa usam áreas separadas sem texto sobre cristal. Nas melhorias, moeda e preço deixam de compartilhar a linha de ação. Hierarquia de valores e níveis foi ajustada.
- O corte retangular do sombreado superior e as continuações espelhadas do cenário foram retirados. Há um movimento pequeno de câmera dentro da margem da imagem. O fundo sólido também cobre o exterior da safe area; os controles continuam dentro dela.
- Avisos curtos de ataque evitam a palavra PREPARANDO invadir o inimigo vizinho. A barra do blindado usa âmbar para não se confundir com o trilho vazio.

Esta rodada não altera economia, atributos, recompensas ou formato do save. Identificação Android atualizada para 0.0.29 / código 29.

## Verificação

107 verificações de domínio passaram após a integração do novo recurso de abertura; `UnityCompileCheck` passou sem erros ou avisos. Foram feitas verificações adicionais em Play Mode no Unity **6000.6.6f1**, usando a cópia curta de build já descrita em [APK 0.0.28](APK-0.0.28.md).

Um perfil isolado em `C:/Users/pctri/dw-apk-0028/HudQaSave` forneceu capítulo 3/fase 9 e broca nível 18. Nenhum perfil real foi editado. A instrumentação temporária invocou os botões de início, compra de ATQ e ajuda, confirmou aumento de nível/débito de ouro, verificou que a ajuda não desativou a lista, fechou o painel e rolou até as últimas melhorias. O contador de frames e o tempo da simulação avançaram, demonstrando execução efetiva. Essa verificação não é um teste de toque físico.

Capturas reais do Editor foram inspecionadas em **738×1599**, **720×1280**, **720×1600** e, na primeira iteração, **1080×2160**. Foram corrigidos os problemas encontrados durante essa inspeção. As capturas ficam fora do Git em `C:/Users/pctri/dw-apk-0028/HudQa/`; os logs de Play Mode em `C:/Users/pctri/dw-apk-0028/hud-qa-*.log`. A leitura de `isTextOverflowing` não apontou estouro nos rótulos ativos do ensaio.

O Editor registrou uma exceção em seu indexador `UnityEditor.Search.SearchDatabase` durante inicialização; a exceção não veio dos scripts de gameplay e não interrompeu o ensaio. Não se declara Console sem avisos/erros. A compilação do projeto e as interações do ensaio concluíram sem erro próprio de UI registrado.

A instrumentação e o redirecionamento de save foram retirados da cópia antes do APK; os scripts de teste ficaram preservados fora de Assets em `C:/Users/pctri/dw-apk-0028/HudQaSources`. A cópia voltou a usar o `AutoBattleDriver` do repositório. O APK não inclui o perfil do ensaio.

## Limites

A validação visual foi no Editor; safe area real, toque e desempenho precisam ser conferidos novamente no aparelho. A arte dos inimigos, contornos claros das folhas, identidade de mineração e expansão do polimento além do HUD ainda pedem trabalho. Este marco corrige os problemas de interface indicados, sem representar aprovação visual final de todo o jogo.

## Ajuste adicional da navegação

Após o usuário apontar novamente os ícones deslocados, foi identificada a causa estrutural: as abas eram distribuídas em sextos da largura total, mas a arte `bottom_nav_structure` inclui duas tampas laterais e seis células internas com outras posições. As âncoras de cada botão agora usam os limites das células na imagem original de 1484 px (80–300, 318–518, 539–731, 751–944, 966–1156 e 1179–1402). Ícone e rótulo compartilham o centro da respectiva célula e têm faixas verticais distintas. A correção não se limita a deslocar desenhos dentro de botões antigos: as áreas de toque também acompanham os encaixes. Nova captura em `hud-qa-nav-final.log`, perfil isolado `HudQaNavSave`.
