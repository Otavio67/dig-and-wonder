# Sprite da Broca Pioneira — integração Unity

## Avaliação da amostra enviada

A folha tem uma leitura forte para celular: a silhueta é larga e baixa, as esteiras deixam o contato com o chão claro, a ponta aponta para a direção de avanço e o cristal ciano cria um ponto de atenção. As 12 poses também já separam três linguagens úteis: repouso, deslocamento com poeira e ação da broca. O acabamento cartoon, a paleta de latão/verde e o contorno escuro combinam com a interface provisória atual.

Uma versão provisória foi gerada a partir da imagem anexada e está em `UnityProject/Assets/_Game/Resources/Art/Drill/drill_pioneira.png`. Ela tem transparência real fora da broca e uma tela normalizada de 1680 × 960, em 4 × 3 células de 420 × 320. É uma derivação de produção para o protótipo, não uma cópia do arquivo original nem uma promessa de arte final.

## Como o protótipo usa o movimento agora

`GroundDrillMotion2D` foi adicionado à broca provisória. Ele controla apenas o elemento visual:

- caminhada curta horizontal, com balanço vertical de poucos pixels para manter as esteiras no chão;
- antecipação, avanço e recuperação em ataques reais da simulação;
- reação curta a dano e descida na derrota;
- todos os tweens são cancelados ao reiniciar, morrer, desabilitar ou remover a unidade;
- a raiz lógica, os atributos, o cooldown, o dano e o save não são animados.

`ProceduralAnimator2D` continua sendo o único dono da escala de idle/impacto/ataque. A locomoção não cria uma segunda máquina de estados de combate nem disputa a mesma propriedade de escala.

Durante esta etapa, o componente está em um filho `Drill Visual` criado pelo HUD sob o rótulo `Drill`; isso permite usar a arte real sem misturar o desenho com o texto de fallback. A hierarquia alvo para a arte importada é:

```text
DrillRoot (Logic: posição funcional e colisor)
└── Visual (SpriteRenderer)
    ├── DrillSpriteSheetPlayer2D (frames)
    ├── GroundDrillMotion2D (posição visual)
    └── ProceduralAnimator2D (escala e respostas)
```

## Importação recomendada do PNG

1. Se o Unity ainda não tiver importado o arquivo, reabra o projeto e selecione `UnityProject/Assets/_Game/Resources/Art/Drill/drill_pioneira.png`.
2. No Inspector: `Texture Type = Sprite (2D and UI)`, `Sprite Mode = Multiple`, mipmaps desligados e compressão adequada ao Android depois de conferir a borda.
3. Abra o Sprite Editor e divida em **4 colunas × 3 linhas**, células de 320 × 240 para a imagem de 1280 × 720.
4. Use pivô `Custom`, inicialmente próximo do centro das esteiras (X 0,50; Y 0,08). Ajuste olhando a linha de contato em uma tela retrato; o pivô não deve mudar o colisor.
5. Renomeie as células por linha para facilitar a montagem do Animator:
   - `idle_00`–`idle_03`: primeira linha;
   - `walk_00`–`walk_03`: segunda linha, com poeira;
   - `attack_00`–`attack_02` e `recover_00`: terceira linha.

Essa divisão é uma hipótese de leitura baseada na folha enviada e precisa de confirmação visual no Editor. A pose com a ponta lançada deve ser usada como ação especial somente depois de ligar o frame ao evento real correspondente.

## Validação pendente

O PNG está versionado e foi revisado visualmente fora do Unity: transparência RGBA e grade 4 × 3 conferidas. `DrillSpriteSheetPlayer2D` já procura os 12 nomes de frame pela pasta `Resources` e mantém o ícone TMP como fallback. A importação pelo Unity, a criação dos sub-sprites e a sensação de contato com o chão ainda exigem uma abertura no Editor local. A gravação no PC comprovou apenas o combate anterior; não há medição de FPS, toque ou fluidez em Android.
