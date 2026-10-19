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
