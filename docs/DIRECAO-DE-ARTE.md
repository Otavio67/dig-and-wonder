# Direção de arte — defesa com escavação

Atualização de 20/10/2026: estado atual e animações em [MARCO2.md](MARCO2.md); terreno/poço em [REVISAO-MARCO1.md](REVISAO-MARCO1.md). Essas revisões substituem as menções anteriores a dois espaços vazios ou impacto único da Pioneira.

Direção de jogo atual autorizada em 20/10/2026. A aprovação artística anterior continua orientando paleta, pintura, materiais e acabamento; a antiga restrição a alterações exclusivamente visuais foi substituída pelo briefing de defesa. Regras atuais em `DESENVOLVIMENTO.md`.

## Protagonista e leitura

Carrinho fixo no topo: esteiras, corpo central com rosto, ferragens de latão, esmalte turquesa e dois canhões. Brocas são unidades lançadas, não o mesmo objeto fixo. Lia e Bento ficam preservados e ocultos. O poço abaixo deixa clara a trajetória e mantém a frente próxima. A barra pertence à camada toda, não a cada desenho de fragmento.

## Acabamento

Pintura digital cartoon 2D, fundos sem contorno, bordas suaves, saturação moderada, textura sutil e composição assimétrica. Metal fosco, sombras de contato e reflexos pequenos; evitar plasticidade 3D, geometria sem desenho e ruído excessivo. Paleta completa de base em `IDENTIDADE-VISUAL.md`.

No Marco 1, reutilizar atlas, ícones e cenografia aprovados. Canhões e Torrão podem ser placeholders coerentes, claramente identificados como tal. Não gastar o marco produzindo arte de sete inimigos, sete habilidades ou um boss.

## Interface

Ouro, fase e vida do carrinho no alto. Resistência, material e contagem de camadas junto à frente. Pioneira com cooldown legível e estado pronto. Duas melhorias permanentes com custo, nível e atributo. Placas metálicas com rebites: coral para lançamento/reinício, latão para melhorias. Sem venda, chaves, baús, mochila ou controles antigos sem função no ciclo.

Áreas funcionais dos controles implementados têm no mínimo 64 unidades lógicas; a meta física de 48 dp depende de medição real depois do stretch e da densidade. Área segura e escala uniforme são preservadas. Cabeçalho e controles não tremem com dano.

## Movimento

- Canhões: recuo curto independente, mirando alvos da simulação, munição com trajetória clara.
- Carrinho: piscada discreta e reação visual de 0,25 s ao dano real.
- Pioneira: queda de 0,8 s com ponta cíclica; um único impacto.
- Torrão: preparo sinalizado por 0,65 s, projétil de terra visível, barra própria.
- Camada: fissura por resistência restante, reação curta de 0,18 s ao impacto real; nova faixa mantém a frente na mesma altura.
- Ouro: texto sobe somente após a simulação conceder o recurso, sem números decorativos de recompensa.
- Perda de foco: pausa e cancelamento do input; nenhuma descarga de ataques ao retomar.

## Ambientes e escopo

O acampamento pintado serve à única fase do Marco 1. Atlas de raízes, cobre, abismo e Aurora continuam preservados para fases posteriores; não representam regiões novas já jogáveis. Parallax continua somente na cenografia.

## Não fazer

Não retomar a grade manual, lançar por ouro, criar escudos com brocas descartáveis, desenhar unidades futuras como disponíveis, ocultar custos/cooldowns, copiar conteúdo das referências ou apresentar placeholders como arte final. Não alegar Android, FPS, anúncios ou APK sem evidência correspondente.

## Aprovação posterior

Primeiro validar ciclo, derrota, recuperação, alvos, cooldown e legibilidade do Marco 1 com capturas reais. Marco 2 depois de aprovação: clareza/balanceamento e apenas uma unidade alternativa. Boss, novas fases e catálogo completo ficam posteriores.
