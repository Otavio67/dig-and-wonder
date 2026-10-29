# Revisão visual — 0.0.32

Atende aos recortes enviados pelo usuário: ícones das melhorias, faixa de combate, navegação e peças ao redor da broca na Oficina. A instrução nova substitui a preferência anterior pelos pictogramas v8 nessas áreas.

## Alterações

- 18 pictogramas Toy-Mecha vetoriais: aço, cobre, núcleo ciano, broca helicoidal e coração blindado. Uma única família compartilhada entre Mina e Oficina, com transparência e margens; sem recortar desenhos de uma tela pronta.
- Chapa chanfrada em nove partes para o HUD. Linhas de melhorias mais altas, nível em faixa própria, ícone centralizado, ação verde e custo separado.
- AUTO com estado legível; especial e quatro poderes em slots uniformes. Rank real nos poderes, cooldown no especial, sem cinco rótulos repetidos de abertura. Força usa visor simples.
- Oficina com fundo mais escuro na vitrine da máquina, módulos novos, ícones que acompanham cada página e títulos próprios para Poderes, Tesouro e Brocas. Última página não mostra uma melhoria inexistente. Compras indisponíveis ficam dessaturadas.
- Reinício após derrota ocupa uma ação na faixa de missão, com altura maior, sem apertar o cabeçalho das melhorias.
- Inimigos comuns usam barras; texto fica reservado a funções e preparação de ataque. A barra grande do chefe foi movida para baixo da placa de fase, que a ocultava; a barra local duplicada foi removida.

## Verificação

107 verificações de domínio passaram (economia, progressão, combate, persistência e HUD). Compilação C# com stubs passou. Revisão no Unity 6000.6.6f1 em 738×1599 e 720×1280, com perfis descartáveis separados do save do usuário: combate, compra de ATQ com atualização do saldo e nível, ajuda, rolagem, modos, álbum, quatro páginas de melhorias, duas páginas de poderes, tesouro e garagem. Capturas em `.tools`/diretório local de QA, fora do Git.

Derrota e vitória foram examinadas com estados de apresentação preparados no perfil descartável; não são evidência de vencer/perder essas partidas por gameplay real. Chefe e onda de cinco inimigos foram renderizados na simulação em execução. Não houve alerta de texto excedendo seus limites nas páginas da Oficina ensaiadas. A exceção preexistente de `UnityEditor.Search` apareceu durante a inicialização; os ensaios terminaram normalmente.

Este marco não comprova desempenho, toque ou safe area em aparelho Android. O projeto mantém economia e esquema de save. A aparência ainda pode receber ajustes após o teste do usuário.
