# Bíblia de conteúdo — profundidade, inimigos, brocas e pets — 0.0.20

Este documento transforma a próxima expansão em conteúdo consultável por dados. Os catálogos em `content/regions`, `content/enemies`, `content/drills`, `content/pets` e `content/modes` são separados da simulação Unity. IDs são estáveis para que um save futuro não dependa do nome traduzido.

## A viagem do jogador

### Fases 1–50: superfície e grutas

O jogador começa no acampamento, aprende o ritmo de avanço e encontra inimigos legíveis. Até a fase 20, o foco é aprender ouro, upgrades e a primeira troca de broca. Entre 21 e 50, a caverna fica mais pesada: basalto, fósseis, vapor e mineiros de apoio criam decisões de prioridade.

### Depois da fase 50: Submundo Ferrífero

Ao derrotar o chefe da fase 50, o piso abre uma passagem. A tela de vitória mostra a broca olhando para baixo, uma escada de lava azul e o texto **“O Submundo ouviu sua broca”**. A fase 51 começa com uma apresentação curta do novo ambiente.

O Submundo deve parecer mais profundo e industrial: magma frio, raízes negras, trilhos quebrados e máquinas de uma civilização mineradora. Mineiros perdidos podem aparecer como encontros opcionais. Resgatá-los não bloqueia o boss; concede atalhos, histórias e materiais.

### Depois da fase 80: Dimensão Fraturada

O chefe da fase 80 deixa um portal ciano flutuando sobre a arena. O portal tem borda de engrenagem, interior com estrelas e pequenos pedaços da própria caverna girando ao contrário. O jogador toca nele para atravessar; a transição dura poucos segundos e não deve parecer um loading vazio.

Na Dimensão Fraturada, o cenário dobra, os veios ficam suspensos e alguns inimigos usam cópias falsas. Os sinais de ataque precisam continuar claros: um brilho ciano indica informação verdadeira, roxo indica distorção e vermelho indica dano iminente.

## Inimigos e leitura de combate

O catálogo define quatro funções: básico, ágil, suporte/controle e tanque. Cada inimigo tem preparação, ação e recuperação. O jogador deve entender o próximo ataque pelo movimento e pelo som, sem depender de texto.

- **Torrão:** inimigo inicial, arremessa cascalho depois de levantar a pá.
- **Morcego de Cascalho:** mergulho irregular e cópia de alvo uma vez.
- **Caramujo Mineral:** fecha o casco antes de rolar e reduz dano durante a preparação.
- **Mineiro Aprendiz:** chama reforço e aumenta a cadência de um aliado.
- **Aranha Basáltica:** desenha teias no chão e reduz velocidade.
- **Golem de Veio:** martelo pesado e onda de choque.
- **Imp de Brasa:** projétil com queimadura curta.
- **Cão de Cinza:** marca um alvo e fica mais perigoso se não for interrompido.
- **Mineiro Perdido:** troca posições e pode abrir um atalho opcional.
- **Sentinela Obsidiana:** barreira frontal e contra-ataque.
- **Parasita Prismático:** cópia falsa, com o brilho revelando o alvo real.
- **Eco do Vazio:** repete uma parte do último dano recebido.
- **Cartógrafo Fraturado:** inverte a prioridade de alvo por um ciclo.

Os bosses seguem três momentos visuais: preparação reconhecível, ação com impacto e recuperação vulnerável. O **Rei Carvão** é o boss do Submundo; o **Oráculo Fraturado** fecha a Dimensão Fraturada. Nenhum deles está conectado ao combate atual ainda.

## As 11 brocas atuais

Pioneira, Rebite, Centelha, Bastião, Prisma, Sísmica, Aurora, Cobalto, Nova, Titã e Singular já estão no catálogo Unity 0.0.19. A aquisição das 10 primeiras recompensas é por vitória de fase; a Singular é a recompensa da fase 100. Cada uma tem XP e poderes próprios e começa no nível 0 quando é recebida.

## Brocas futuras

O catálogo futuro já reserva IDs para brocas de baús, eventos, álbum e pacotes cosméticos: Geodo, Forja, Abissal, Cartógrafa, Braseiro, Fenda, Oficina, Cometa, Coroa e Mosaico.

As regras de design são:

- baús podem entregar uma broca ou fragmentos, mas duplicatas nunca apagam progresso;
- eventos entregam sidegrades com identidade de padrão, não apenas números maiores;
- itens pagos não são necessários para terminar a campanha;
- toda broca paga precisa de uma alternativa não paga com fantasia e desempenho comparáveis;
- skins podem mudar cor, animação e efeitos, mas não podem esconder sinais de ataque.

## Pets

Pets são acompanhantes, não unidades de combate independentes. Na primeira versão, um pet ativo altera eventos específicos: início de fase, abertura de baú, minério raro, combo, dano letal ou leitura de boss. Isso mantém o custo de performance baixo e evita uma segunda linha de personagens para balancear.

Os primeiros nomes são Carvõezinho, Morceguinho Mapa, Geodinha, Fagulha, Reloginho Torto, Golem de Bolso, Vaga-lume Nulo, Chaveiro Falante, Orbe Aurora e Parafuso Real. Cada um tem personalidade, visual, raridade e uma passiva com gatilho explícito no catálogo.

## Modos que valem o próximo ciclo

1. **Escavação Profunda:** torre invertida com andares de chão quebrável, duas rotas e checkpoints. É o modo mais alinhado à fantasia original e deve ser o primeiro modo adicional.
2. **Caçada às Fendas:** fases curtas com dois modificadores e boss no fim, liberadas depois da fase 80.
3. **Galeria dos Guardiões:** sequência curta de bosses para treinar padrões.
4. **Corrida do Baú:** tentativa de 90 segundos com escolhas de risco e recompensa.
5. **Expedição dos Pets:** missões curtas com progresso offline limitado e resgate de histórias.
6. **Profundidade da Semana:** uma semente semanal com regra mutada e recompensas cosméticas.

## Prioridade de implementação

O próximo trabalho técnico deve ser: (1) ligar as regiões 51+ ao visual e ao portal, (2) criar um inimigo novo de cada função com telegraph real, (3) implementar o primeiro andar da Escavação Profunda, (4) adicionar um pet passivo simples, e só depois (5) conectar baús a fragmentos e brocas futuras.

Os catálogos desta entrega são planejamento estruturado. Eles não afirmam que inimigos, pets, portal, equipamentos ou modos futuros já aparecem na cena jogável.
