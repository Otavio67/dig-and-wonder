# Sprite da Broca Pioneira — integração Unity

## Avaliação da amostra enviada

A folha tem uma leitura forte para celular: a silhueta é larga e baixa, as esteiras deixam o contato com o chão claro, a ponta aponta para a direção de avanço e o cristal ciano cria um ponto de atenção. As 12 poses também já separam três linguagens úteis: repouso, deslocamento com poeira e ação da broca. O acabamento cartoon, a paleta de latão/verde e o contorno escuro combinam com a interface provisória atual.

O arquivo da imagem ainda não está no checkout remoto. A imagem anexada à conversa mostra fundo branco; para entrar no jogo, o PNG precisa ser enviado ao repositório com transparência real fora da broca. Não foi criado um arquivo substituto nem usado o branco como fundo jogável.

## Como o protótipo usa o movimento agora

`GroundDrillMotion2D` foi adicionado à broca provisória. Ele controla apenas o elemento visual:

- caminhada curta horizontal, com balanço vertical de poucos pixels para manter as esteiras no chão;
- antecipação, avanço e recuperação em ataques reais da simulação;
- reação curta a dano e descida na derrota;
- todos os tweens são cancelados ao reiniciar, morrer, desabilitar ou remover a unidade;
- a raiz lógica, os atributos, o cooldown, o dano e o save não são animados.

`ProceduralAnimator2D` continua sendo o único dono da escala de idle/impacto/ataque. A locomoção não cria uma segunda máquina de estados de combate nem disputa a mesma propriedade de escala.

Durante esta etapa, o componente está ligado ao rótulo `Drill` criado pelo HUD para tornar o comportamento verificável mesmo sem o PNG. Quando a arte chegar, a hierarquia alvo é:

```text
DrillRoot (Logic: posição funcional e colisor)
└── Visual (SpriteRenderer)
    ├── GroundDrillMotion2D (posição visual)
    └── ProceduralAnimator2D (escala e respostas)
```

## Importação recomendada do PNG

1. Coloque o arquivo transparente em `UnityProject/Assets/_Game/Art/Drill/drill_pioneira.png`.
2. No Inspector: `Texture Type = Sprite (2D and UI)`, `Sprite Mode = Multiple`, mipmaps desligados e compressão adequada ao Android depois de conferir a borda.
3. Abra o Sprite Editor e divida em **4 colunas × 3 linhas**, células de 320 × 240 para a imagem de 1280 × 720.
4. Use pivô `Custom`, inicialmente próximo do centro das esteiras (X 0,50; Y 0,08). Ajuste olhando a linha de contato em uma tela retrato; o pivô não deve mudar o colisor.
5. Renomeie as células por linha para facilitar a montagem do Animator:
   - `idle_00`–`idle_03`: primeira linha;
   - `walk_00`–`walk_03`: segunda linha, com poeira;
   - `attack_00`–`attack_02` e `recover_00`: terceira linha.

Essa divisão é uma hipótese de leitura baseada na folha enviada e precisa de confirmação visual no Editor. A pose com a ponta lançada deve ser usada como ação especial somente depois de ligar o frame ao evento real correspondente.

## Validação pendente

O código foi preparado e pode ser compilado pelos checks do projeto, mas a importação do PNG, o recorte, a troca de frames, a leitura de transparência e a sensação de contato com o chão ainda exigem o arquivo no checkout e uma abertura no Unity Editor. A gravação no PC comprovou apenas o combate anterior; não há medição de FPS, toque ou fluidez em Android.
