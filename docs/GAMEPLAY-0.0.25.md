# Gameplay principal — revisão 0.0.25

Revisão de 25/10/2026 sobre o checkpoint `efc949a` (0.0.24), autorizada para completar a integração da tela de combate apontada na revisão geral. Não altera schema de save, custos, curva de XP, atributos de catálogo nem recompensas.

## Integração entregue

- O HUD lê dano, vida, reparo por segundo, cadência, chance/dano crítico e multiplicador do especial do simulador. ATQ mostra dano por disparo comum, incluindo multiplicador da personagem e Dentes de titânio; dano crítico e bônus contra chefes permanecem condicionais. Valores pequenos conservam sua escala (6 de dano, não 0,006).
- Sete linhas na ordem ATQ, VIDA, AUTORREPARO, CADÊNCIA, CHANCE CRÍTICA, DANO CRÍTICO e ESPECIAL. Todos mostram nível e usam os custos, requisitos e limites da economia existente. Saldo insuficiente, requisito e máximo desabilitam a compra. Após derrota, a lista permanece disponível para melhorar e recomeçar.
- FORÇA deixa de ser 128 fixo: indicador comparativo `ceil(vida máxima + 10 × DPS básico esperado)`, incluindo chance crítica média e quantidade/cadência dos disparos. O indicador não participa do balanceamento.
- AUTO tem raycast habilitado e alterna os disparos básicos reais. O especial preserva a automação anterior e aceita toque somente quando pronto, com alvo e sem uma execução em curso.
- Cinco espaços: especial com recarga e os quatro poderes existentes, mostrando nível exigido ou rank. Toque nos poderes abre a página correspondente na Oficina. Não foram inventados cinco novos poderes ativos.
- Menu superior abre informações da expedição; cristal/+ e LOJA levam ao tesouro existente, onde há as transações reais de chaves e ouro. A seleção das abas acompanha a navegação.
- Ondas e recompensa da primeira vitória usam dados reais. Não há promessa fixa de 20 cristais em todas as fases.

## Arte e apresentação

- Identificado `nPOTScale: 1` nos atlas: a importação podia redimensionar texturas, invalidando coordenadas de recortes em pixels. Corrigido para `0`, mipmaps desligados e bordas em Clamp; PNGs individuais importados como sprites.
- Carteiras, menu, faixa de missão, poderes e força tiveram recortes revisados contra os atlas originais. As peças individuais foram exportadas novamente, preservando GUIDs existentes; moeda recebeu PNG próprio.
- Reexportar com `scripts/export-hud-assets.ps1`: o script lê as coordenadas do runtime e conserva os pixels/transparência, sem gerar uma arte diferente.
- Cenário ocupa a área atrás do HUD superior. Três cópias alternadas/espelhadas da ilustração produzem deslocamento contínuo para a esquerda; vento tem velocidade própria. As construções presentes na ilustração passam com ela. Não são novas casas independentes ou camadas de arte inéditas.
- Linha de chão por bioma, imagens apoiadas pela base e barras de vida acima das unidades. Cinco espaços visuais comportam quatro criaturas mais um guardião. IDs permanecem ligados aos espaços durante a vida da unidade.
- Projéteis, números, moedas e impactos recebem pontos da broca/alvo. Projéteis destinados a alvo morto são retirados; efeitos são limpos ao abrir Oficina, recomeçar e trocar de fase.
- Folhas de inimigos usam seus formatos observados (4, 5 ou 6 colunas; Goblin com quatro linhas; lesma/besouro com linhas de larguras diferentes). Idle, preparação de ataque, impacto e derrota usam poses; animação não calcula dano.
- Chefes usam folhas substitutas distintas por bioma (tatu, golem, besouro, eco), com escala maior. Torrão ainda usa a Toupeira Pavio como substituto. Sprites exclusivos desses personagens permanecem pendentes; não são tratados como arte final.

## Validação

| Verificação | Evidência |
| --- | --- |
| DomainChecks | 85 aprovados: 79 anteriores + atributos do HUD, elegibilidade das sete melhorias, números, especial manual, AUTO e limites/importação dos atlas |
| UnityCompileCheck | Compilação C# com stubs: zero erros e avisos |
| UnityApiCompileCheck | Compilação contra DLLs do Unity 6000.6.6f1 e assemblies de pacotes já disponíveis em `Library/ScriptAssemblies`: zero erros; dois avisos anteriores de APIs obsoletas no bootstrap/preparador TMP |
| Editor real | Tentativa em batch com o Editor 6000.6.6f1 instalado: encerramento 198. A abertura normal também encerrou com `No valid Unity Editor license found. Please activate your license.` |
| Inspeção visual | Atlas, fundos e folhas originais examinados; não houve renderização desta revisão dentro do Unity |
| Android / FPS | Não executados nem medidos |

O verificador de APIs agora aceita o executável Windows e o caminho de assemblies importados via `-p:TemplateAssemblies=...`. Isso não substitui a resolução de pacotes ou execução da engine. Logs locais do bloqueio: `.tools/gameplay-editor-check.log` e `.tools/gameplay-editor-interactive-check.log` (ignorados pelo Git).

## Próxima validação necessária

Ativar a licença do Editor pelo Unity Hub e abrir a cena Prototype. Conferir início/carregamento, AUTO, sete melhorias com saldo insuficiente e desbloqueio, toque/scroll, páginas dos poderes, derrota/melhoria/recomeço, Oficina e retorno, passagem de mapas, alinhamento de tiros e morte dos inimigos. Usar um perfil de teste para compras. Depois gerar APK e medir toque, áreas seguras e FPS em aparelho.

Pets, Torre Invertida, arte exclusiva das onze brocas e expansão para os cinquenta inimigos continuam como conteúdo futuro. Esta revisão fecha as conexões identificadas da gameplay principal no código; não declara o jogo inteiro finalizado ou visualmente aprovado.
