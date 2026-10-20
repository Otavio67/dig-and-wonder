# Dig & Wonder

Jogo casual de mineração cartoon 2D para Android. Engine escolhida: Godot 4, GDScript. Nome provisório: Dig & Wonder.

## Contexto recuperado em 19/10/2026

Fontes: conversas “Publicar jogo Android com anúncios” e “Branch · Publicar jogo Android com anúncios”. Após uma recuperação parcial, o usuário forneceu o texto completo da proposta aprovada neste chat.

Direção aprovada: escavar, coletar, vender, transformar a broca e descobrir mundos subterrâneos inesperados. Transformações devem mudar o padrão de escavação, além dos números. Coleção de descobertas e ajudantes automáticos complementam uma progressão simples.

## Documentos

- [Especificação e roteiro de desenvolvimento](docs/DESENVOLVIMENTO.md)
- [Continuidade na nuvem](docs/NUVEM.md)
- [Identidade visual e referências de arte](docs/IDENTIDADE-VISUAL.md)

## Executar e verificar

Abra `project.godot` no Godot 4.7.2 e execute com F6/F5. A versão `0.0.5` consolida a prontidão técnica Android e prepara uma prova isolada de anúncio recompensado de teste. Veja a [análise baseada na documentação oficial do Android](docs/REVISAO-ANDROID.md).

Em Linux x86_64:

```bash
bash setup-cloud.sh
bash scripts/setup-android.sh
bash check-project.sh
bash scripts/export-android-debug.sh
```

O APK de depuração sai em `builds/dig-and-wonder-android-debug.apk` (pasta ignorada pelo Git). Veja [configuração e limites do teste Android](docs/ANDROID.md). O preset é voltado a teste local em aparelhos ARM64; não é um pacote de publicação.

O instalador baixa a versão fixada do distribuidor oficial e confere o SHA-512. O workflow `.github/workflows/validate.yml` executa importação e inicialização no GitHub Actions. Esses checks não comprovam qualidade visual, 60 FPS, funcionamento em Android ou anúncios.

## Protótipo jogável

O marco atual mostra uma tela inicial com **Iniciar escavação** ou **Continuar** quando encontra progresso salvo. Toque e segure (ou clique e segure) no terreno para escavar; veios mais raros e profundos valem mais moedas. A primeira venda concede um bônus único de 8 moedas. O HUD mostra a meta atual. Com 30 moedas, transforme a Broca Pioneira em Broca Prisma: o padrão muda de uma célula para três células em leque. A mina acompanha o avanço em profundidade; o álbum fica no botão superior direito e a descoberta aparece a partir de 16 células. O progresso e dano parcial de pedras resistentes são gravados automaticamente em armazenamento persistente `user://` durante a escavação (intervalo de até um segundo), ao soltar o controle, vender, melhorar e perder foco/pausar/fechar. Há backup do save anterior válido para recuperação. Veja [referências e decisões de design](docs/REFERENCIAS-DE-DESIGN.md), inclusive os momentos planejados para rewarded ads. O SDK de anúncios fica isolado em um APK técnico de teste, não no jogo principal.

No editor, abra `project.godot` com Godot 4.7.2 e rode F6/F5. No desktop, use o mouse; o jogo está em retrato. Para testar a lógica e a persistência, rode:

```bash
GODOT_BIN=.tools/godot/Godot_v4.7.2-stable_linux.x86_64 bash check-project.sh
GODOT_BIN=.tools/godot/Godot_v4.7.2-stable_linux.x86_64 bash scripts/run-harnesses.sh
```

Em `0.0.2`, o HUD abre três menus: **Baús**, **Equipamentos** e **Personagem**. Os 60 equipamentos ficam em grupos de dez nas raridades Ruim, Boa, Épica, Lendária, Única e Mística. Baús também podem liberar brocas e skins. A broca é uma companheira com rosto, modelos de escavação próprios e skins equipáveis com habilidade inata; bônus e penalidades alteram força, velocidade, moedas por bloco e sorte. Cada encaixe aceita um equipamento. Baús e chaves continuam vindo da escavação, sem compra nem anúncio. O save esquema 3 migra os esquemas 1 e 2 e preserva itens já adquiridos.

Em `0.0.3`, cada fase tem 24 trechos de profundidade e terreno/minérios próprios. Há três ambientes regulares e uma Fenda Aurora surpresa com chance de 22% após vencer o chefe. Cada chefe bloqueia a escavação para baixo; toque e segure nele para atacar. Derrotá-lo abre a fase seguinte e paga moedas. Chaves e baús ficam em blocos marcados para exploração, com limite por fase (2 de cada nas fases regulares; 3 na Fenda); coletar todos é opcional. O álbum recebe uma descoberta própria por ambiente. O save esquema 5 migra versões anteriores e conserva fases futuras sorteadas.

Confira o histórico e as notas em [VERSOES.md](docs/VERSOES.md). Conteúdo de brocas, equipamentos e região permanece separado dos sistemas para facilitar novas expansões.

## Direção visual em andamento

A interface está recebendo uma identidade própria de expedição subterrânea, com paleta azul-petróleo, turquesa, latão e coral, contornos marcados, cristais luminosos e personagens com silhuetas legíveis em celular. A tela inicial e a faixa de escavação agora apresentam a broca-companheira e os mineiros Lia e Bento; detalhes em [IDENTIDADE-VISUAL.md](docs/IDENTIDADE-VISUAL.md). Esta revisão visual ainda não recebeu novo número/tag de versão.

Atualização de 20/10/2026: o ambiente de nuvem tem Android SDK e templates Godot configurados; os APKs de jogo e de teste AdMob são exportáveis e passam em verificações estáticas. O ambiente não tem `/dev/kvm` nem aparelho conectado. A exportação do teste inclui somente IDs oficiais de demonstração e requer internet para baixar/ver o anúncio. Nenhum anúncio foi exibido em aparelho e 60 FPS não foi medido. Em `0.0.4`, houve revisão visual de capturas reais do Godot com Xvfb/Mesa por software; capturas desktop não substituem testes visuais em telefone.

## Diretrizes confirmadas pelo usuário

- Priorizar Android. Considerar iOS depois, conforme os resultados financeiros.
- Explorar monetização com anúncios; formato e frequência ainda não definidos.
- Usar visual cartoon 2D, com design atraente e identidade própria.
- Ter regras fáceis de entender e aprendizado durante o jogo.
- Atender também adolescentes e jovens até aproximadamente 22 anos; não restringir a proposta a um jogo infantil.
- Buscar diferenciação por uma combinação funcional de ideias e mecânicas.
- Explorar uma experiência contínua, com descobertas e progressão que sustentem o interesse.
- Começar com um escopo viável e permitir a adição de conteúdo depois.
- Evitar a complexidade da proposta anterior chamada de “escavação inteligente”.
- Há liberdade criativa para propor soluções e discordar quando houver bons motivos.

## Nome em avaliação

- Nome de trabalho: **Dig & Wonder**.
- Identificador sugerido para pastas e repositório: `dig-and-wonder`.
- Avaliação criativa: o nome comunica escavação, curiosidade e descoberta. Combina com uma direção cartoon e deixa espaço para diferentes tipos de achados e ambientes.
- Ponto a testar com o público brasileiro: facilidade de pronúncia, escrita e lembrança de “Wonder”.
- Busca preliminar na web por “Dig & Wonder”, “Dig and Wonder” e “digandwonder” em 19/10/2026: não foi identificado um jogo com correspondência exata nos resultados retornados. Disponibilidade comercial, de marca, domínio e perfis permanece não verificada.

## Configuração e pendências

1. Usuário escolheu continuar pela nuvem e autorizou criar um repositório GitHub na conta `Otavio67`.
2. Repositório criado na conta correta e com visibilidade privada: [Otavio67/dig-and-wonder](https://github.com/Otavio67/dig-and-wonder).
3. Preparar ambiente de nuvem com Godot 4.7.2 e incluir o repositório.
4. Implementar e testar o primeiro protótipo da especificação.

## Estado local

O projeto está jogável em Godot 4.7.2, com APK debug, prova AdMob isolada, SDK Android, testes da lógica/save e capturas visuais reproduzíveis. O checkpoint atual é `0.0.5`. Permanecem pendentes execução em Android real, métricas de desempenho, acessibilidade completa, integração do momento de anúncio à economia do jogo e preparação da loja. Os detalhes estão em [REVISAO-ANDROID.md](docs/REVISAO-ANDROID.md).

## Revisão de sensação após 0.0.5

Lascas da escavação agora usam a cor do material, giram durante a queda e a broca recua levemente ao atingir ou quebrar uma célula. Capturas reais do Godot 4.7.2 em Windows ficam em `builds/review/` (saída local ignorada pelo Git). Importação, boot e harnesses de lógica, gameplay e save passaram; a falha histórica do harness Android em `y=229` foi corrigida na etapa visual 1 usando `Layout.WORLD_TOP`; FPS e toque em Android real continuam sem medição.


## Etapa visual 1 — equipe animada (20/10/2026)

Lia, Bento e a broca-companheira usam 11 peças SVG originais em `assets/personagens/`, com silhuetas arredondadas, base/sombra, contorno escuro e reflexos pequenos. Os PNGs anteriores ficam como legado; não são carregados pelo jogo.

Os rigs `Node2D` em `scripts/visual/` usam somente Tween: respiração, piscadas em intervalos distintos, balanço da broca, braço/ferramenta sincronizado ao dano real, ponta em movimento durante a escavação e squash/stretch somente na quebra. A animação atua em pivôs locais, pausa sem foco e não consome aleatoriedade da progressão. Início, mina e personagem reutilizam a mesma equipe.

Validação no Godot 4.7.2 Windows: importação, boot e harnesses de lógica, gameplay, save, interface Android e personagens passaram. Os testes de toque agora usam `Layout.WORLD_TOP`, sem coordenada antiga fixa. O novo harness verifica eventos reais, célula vazia, dano versus quebra, pivôs, pausa, quantidade limitada de Tweens, RNG e esquema de save. O Git Bash foi bloqueado pelo sandbox (`NtCreateDirectoryObject`, 0xC0000022); os comandos equivalentes do `check-project.sh` e os harnesses foram executados diretamente com o Godot Windows.

Evidências locais ignoradas pelo Git: `builds/review/etapa1-20261020/antes/`, `depois/`, `depois/sequencia/` (96 quadros reais, 12 fps), `depois/personagens-animados.gif` e `depois/personagens-detalhe.gif`. A captura é desktop em viewport 540×960; não mede FPS, toque ou memória em Android. Para reproduzir a sequência, execute Godot com `--path . --fixed-fps 12 --audio-driver Dummy --script res://scripts/capture_characters.gd` usando dados de usuário isolados.

Esta etapa não redesenha blocos nem interface. A aparência destes elementos ainda limita a unidade visual. Próxima etapa, após aprovação: materiais dos blocos (etapa 2). A versão publicada continua 0.0.5, sem novo APK ou tag.


## Direção de arte aprovada — integração de 20/10/2026

A direção em `docs/DIRECAO-DE-ARTE.md` foi aprovada pelo usuário e recebeu uma correção de estilo durante a execução: fundos pintados, sem contorno, menos saturados, com textura sutil de pincel e sem aparência plástica. A revisão é uma integração artística para avaliação no jogo, sem nova versão/tag ou APK.

- Broca como único protagonista visível; Lia e Bento continuam nos arquivos/rigs, ocultos e sem receber eventos de escavação. A cabine pintada tem visor animado, ponta independente e silhuetas decorativas dos seis modelos existentes.
- Cinco tratamentos de ambiente em um atlas de fundo, mais duas camadas transparentes ilustradas. Parallax só na cenografia; transição de 1,5 s entre tratamentos e névoa com borda suave. As faixas visuais usam profundidade já existente; Fenda Aurora acompanha a região especial existente. Não são regiões novas.
- Terreno ilustrado com três variantes por família de material, escolha determinística por coordenada, cores moduladas pelo catálogo existente e fissuras sobre o dano real. Nenhum consumo adicional do RNG de geração/recompensa.
- Placas de metal com rebites e relevo, ações principais coral, secundárias latão, estados pressionado/desabilitado e ícones próprios. Telas existentes reorganizadas; controles continuam nas mesmas rotinas, com os harnesses usando o layout vigente.
- Perfuração, impacto visual curto, partículas de material, número da venda calculado a partir do valor real, reação de melhoria e poses fechada/aberta do baú. Confete limitado a 16 traços para Lendária/Única/Mística após abertura real. Falas ocasionais por texto de rádio, sem renderizar os mineiros e com intervalo de 30 s.

### Evidência e validação

Capturas reais: `builds/review/aprovada-20261020/antes/` e `depois/`; GIF `depois/jogo-em-movimento.gif`, com 144 quadros em 12 fps. A gravação demonstra compra real da Prisma e abertura real de um baú. As imagens `ambiente-*.png` são fixtures visuais isoladas das cinco faixas; não comprovam progressão conquistada pelo jogador. Tudo é **execução desktop em viewport de celular** (540×960), além da captura em paisagem com área segura preservada. As saídas ficam locais, ignoradas pelo Git e excluídas do APK.

Importação, boot e harnesses de lógica, gameplay, save, interface e personagens executados no Godot 4.7.2 Windows. O harness de personagens foi adequado à retirada dos mineiros da tela e verifica que permanecem ocultos, além de eventos reais, RNG, pivôs, foco e limite de Tweens. O Bash permaneceu indisponível pelo bloqueio de sandbox já registrado; os comandos equivalentes foram executados diretamente no Windows.

### Limites desta revisão

A avaliação artística final depende da revisão das capturas pelo usuário. Os fundos intermediário/próximo são reutilizados entre faixas; as seis brocas ainda compartilham a carenagem principal e recebem acessórios próprios. Os cartões pequenos podem exigir mais simplificação para melhorar a leitura. O foco está definido no Theme, mas navegação por teclado ainda requer integração própria. Tamanho em dp, toque, FPS, memória, consumo, anúncios e APK não foram validados em aparelho. Nenhuma alteração de balanceamento, formato de save ou fluxo de anúncios foi feita.
