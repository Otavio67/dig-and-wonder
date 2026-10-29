# Proveniência e licenças dos assets atuais

## Arte do Unity

As referências enviadas pelo usuário orientam personagem, materiais e composição. Não foram copiadas interfaces ou personagens dos jogos citados como referência. PNGs gerados para o projeto ou derivados das referências do usuário não são descritos como ilustrações feitas à mão; esta declaração não concede exclusividade nem uma licença geral ao projeto.

A folha da Pioneira, os inimigos, projéteis, fundos e módulos visuais atuais ficam em `UnityProject/Assets/_Game/Resources/Art/`. Os PNGs da oficina foram construídos proceduralmente para o projeto com `scripts/create-workshop-art.py`. Sua integração e limitações estão documentadas nos guias de sprites, HUD e identidade em `docs/`.

## Ícones vetoriais atuais

Os SVGs em `assets/ui/gameplay/` são fontes editáveis de pictogramas construídos em código para Dig & Wonder, rasterizados por `scripts/export-gameplay-icons.cjs`. Mina e Baús foram reaproveitados da família vetorial anterior e agora têm fontes canônicas nessa mesma pasta. Não são extrações de interfaces de outros jogos.

## Fontes

- **Titan One Regular**: Google Fonts, SIL Open Font License 1.1. Fonte e licença em `UnityProject/Assets/_Game/Resources/Fonts/Workshop/TitanOne-Regular.ttf` e `TitanOne-OFL.txt`. Origem: https://github.com/google/fonts/tree/main/ofl/titanone.
- **DW Rounded ExtraBold**: derivação estática do Nunito variável, peso 800, instanciada com fontTools e renomeada para DW Rounded. Créditos e OFL preservados em `DWRounded-ExtraBold.ttf` e `Nunito-OFL.txt`. Origem: https://github.com/google/fonts/tree/main/ofl/nunito.
- **Nunito Reading**: arquivo variável original do Nunito, usando o peso padrão de leitura. Arquivo `Nunito-Reading.ttf`, mesma OFL integral preservada no projeto.

## Dependências e histórico

Licenças das dependências Unity, incluindo DOTween, permanecem nos diretórios correspondentes. O antigo plugin Godot AdMob e seus arquivos foram retirados; ele não é uma dependência do Unity.

Recursos usados somente na implementação anterior foram removidos do checkout em 29/10/2026. A proveniência detalhada daquele material permanece no histórico Git, antes da limpeza. Tornar o repositório público não altera os termos das fontes ou dependências e não cria uma licença de código aberto para todo o projeto.

O ícone `nav_album.svg` (29/10/2026) foi desenhado em código para o álbum: livro com cristal, paleta Toy-Mecha, sem texto embutido. PNG rasterizado com Sharp; fonte vetorial editável preservada.

Em 0.0.29, `hud-approved-v8.png` é uma cópia sem alteração da referência gerada `docs/design/gameplay-toy-mecha-v1/conceito-gameplay-hud-v8.png`. O runtime recorta apenas pictogramas selecionados; textos funcionais são TMP. Origem e limitações da referência permanecem as mesmas.

Em 0.0.32, a pedido do usuário, 18 pictogramas receberam desenho vetorial próprio Toy-Mecha v2. Fontes SVG em `assets/ui/gameplay/`, exportador `scripts/export-mecha-icons.cjs`, PNGs com transparência para Unity. A chapa `hud_panel` é um vetor próprio com bordas para escala em nove partes. Não usam recursos de outros jogos. O exportador anterior executa o novo ao terminar para preservar a direção vigente.
