# Referências de design para o protótipo

Pesquisa em 19/10/2026. As referências servem para observar padrões de experiência; não autorizam copiar personagens, arte, texto ou conteúdo.

## Padrões observados e decisões

- O guia oficial de **Gold & Goblins** descreve um ciclo em etapas: quebrar rochas rende recursos e abre caminho; melhorias e um portão dão um próximo objetivo claro. Aproveitamos recompensas visíveis por célula e a meta contextual no HUD. Mantemos a escavação manual e a transformação da broca como diferencial; não adicionamos fusão de ajudantes, portões ou reset de mina neste marco. [Guia para iniciantes](https://appquantum.helpshift.com/hc/en/7-gold-and-goblins/faq/54-welcome-to-gold-and-goblins-beginner-s-guide/)
- O suporte oficial de **Idle Miner Tycoon** organiza objetivos de minas e melhorias, além de listar aparições opcionais de personagem recompensado por vídeo. Inspirou uma próxima meta sempre visível, sem adicionar uma árvore de gestão extensa. [Ajuda: jogabilidade geral](https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/section/24-general-gameplay/)
- **Deep Town** divulga artefatos descobertos durante a mineração e sistemas automatizados. Mantemos a descoberta única no álbum; automação fica para quando o núcleo manual estiver validado. [Página do jogo na Play Store](https://play.google.com/store/apps/details?id=com.rockbite.deeptown)
- A orientação do Google Play alerta que anúncios em tela cheia inesperados no começo de um nível ou enquanto a pessoa espera iniciar a jornada prejudicam a experiência; rewarded ads devem ser escolha explícita. Por isso, o protótipo não mostra anúncio automático. O ponto a testar futuramente é, depois de uma venda concluída, oferecer “assista para receber mais X moedas”, com a venda normal disponível mesmo sem anúncio. [Política de experiências de anúncios](https://support.google.com/googleplay/android-developer/answer/12271244)
- A documentação Android do AdMob exige IDs de teste durante desenvolvimento e a recompensa deve ser concedida no callback de recompensa, não apenas no fechamento do anúncio. Integração não entra antes de haver plugin Android compatível e teste em aparelho. [Rewarded ads para Android](https://developers.google.com/admob/android/rewarded)

## Progressão aplicada

1. Tela inicial: **Iniciar escavação** ou **Continuar**.
2. Primeira venda: bônus único de 8 moedas e mensagem de confirmação.
3. HUD aponta a próxima ação e o que falta para a Broca Prisma.
4. Camadas ficam mais duras e valiosas por profundidade, para a transformação alterar alcance e ritmo.
5. Broca Prisma abre um leque de três células; escavar até o veio brilhante libera a Semente lunar no álbum.
6. Depois da descoberta, o objetivo muda para reforçar a broca.

## Critérios para anúncios futuros

Sem anúncios em tela de início, durante mineração, nem para bloquear venda, melhoria ou descoberta. Primeiro candidato: bônus opcional pós-venda, em troca explicitamente descrita (adicionar uma quantia igual ao valor-base vendido, sem dobrar o bônus de primeira venda), limitado a uma oferta por venda e concedido somente após confirmação do SDK. Segundo candidato, apenas quando houver progresso offline real: opção para dobrar o ganho offline ao voltar. Não adicionar banner fixo no protótipo sem teste de layout em telefone.

## Baús e equipamentos (versão 0.0.1)

As decisões partem do ciclo de profundidade e das recompensas legíveis documentados acima. O padrão comum de “chave comprada ou espera” foi descartado para este protótipo: cada chave e baú é ganho cavando, surgem em marcos alternados (8 m para a chave e 16 m para o primeiro baú, depois a cada 24 m), e baús podem ficar guardados sem prazo. Isso evita um bloqueio de progressão e não cria moeda paga. Também não há anúncio para obter chave ou abrir baú.

A abertura custa uma chave. As chances começam em Ruim 28%, Boa 36%, Épica 22%, Lendária 9%, Única 3,5% e Mística 1,5%; as categorias altas ganham peso com a profundidade. O painel informa as chances iniciais e a proteção: cinco aberturas sem item Épico levam a um Épico garantido; doze sem Lendário levam a um Lendário garantido. Duplicatas pagam ouro fixo por raridade. As brocas mudam a forma da escavação, e equipamentos têm encaixes próprios com bônus que afetam poder ou valor por bloco.

Não há compra com dinheiro real, recompensa por anúncio ou conteúdo pago aleatório. Caso isso seja considerado no futuro, será necessário mostrar chances antes da compra, revisar classificação/privacidade e políticas vigentes da loja, e preservar uma rota de progressão sem gasto. O sistema atual não depende dessa monetização.

A pesquisa web externa não estava acessível durante esta revisão (proxy respondeu 403). As referências citadas na primeira parte são as já registradas no checkpoint anterior; não foram revalidadas nesta rodada. As porcentagens e regras acima vêm dos catálogos e do código desta versão, não de dados atribuídos a outros jogos.

## Companheira-broca e coleção (versão 0.0.2)

A broca foi escolhida como personagem para manter a leitura visual ligada à ação principal e evitar um avatar que não participa da escavação. O rosto deixa sua presença explícita. Os modelos mudam o padrão espacial de mineração; skins mudam cor e habilidade passiva, incluindo combinações com bônus e penalidades. Assim a escolha visual também influencia como jogar, mas o modelo inicial e a skin Pioneira permitem começar sem depender de baús.

Os seis níveis de raridade são compartilhados pelos equipamentos. Cada nível contém dez itens com nomes, encaixes, habilidade descrita e combinação de poder, valor, velocidade ou sorte. Três encaixes evitam somar o catálogo inteiro ao personagem; o painel mantém itens não encontrados visíveis como silhuetas com efeito e raridade, para dar metas de coleção. As probabilidades de baú e a conversão de duplicatas continuam explícitas e sem compra ou anúncio.

A pesquisa externa desta implementação também retornou 403 no proxy; portanto, esta classificação, as passivas e o tamanho de catálogo são decisões de design do projeto, não alegações sobre práticas pesquisadas em jogos concorrentes. A revisão manual em celular ainda deve confirmar se os nomes, efeitos, legibilidade e quantidade de opções cabem bem no controle por toque.

## Fases e chefes (versão 0.0.3)

A progressão foi alterada para dar um objetivo de conclusão sem exigir 100% da coleta: fases têm limite de profundidade, e apenas derrotar o chefe libera a próxima. O tamanho finito da escavação limita o máximo de minérios; as quantidades de chaves/baús são limitadas por dados por fase e não aparecem como checklist. O controle aproveita a broca já existente: toque ou clique sustentado no chefe ataca, com poder, nível e padrão da broca influenciando o dano.

Há três biomas regulares em ciclo, com materiais e chefes próprios. Uma rolagem de 22% ao vencer cada chefe pode abrir a Fenda Aurora, uma área surpresa mais rara com minério, descoberta, baús e guardiã próprios. A escolha é salva para que reiniciar não faça o evento rerrolar. Esses números são hipóteses do protótipo; ainda precisam de ajuste depois de testar a duração das fases e o ritmo dos chefes em aparelho e com jogadores.
