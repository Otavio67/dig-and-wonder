# Dig & Wonder

Jogo casual de mineração cartoon 2D para Android. Engine escolhida: Godot 4, GDScript. Nome provisório: Dig & Wonder.

## Contexto recuperado em 19/10/2026

Fontes: conversas “Publicar jogo Android com anúncios” e “Branch · Publicar jogo Android com anúncios”. Após uma recuperação parcial, o usuário forneceu o texto completo da proposta aprovada neste chat.

Direção aprovada: escavar, coletar, vender, transformar a broca e descobrir mundos subterrâneos inesperados. Transformações devem mudar o padrão de escavação, além dos números. Coleção de descobertas e ajudantes automáticos complementam uma progressão simples.

## Documentos

- [Especificação e roteiro de desenvolvimento](docs/DESENVOLVIMENTO.md)
- [Continuidade na nuvem](docs/NUVEM.md)

## Executar e verificar

Abra `project.godot` no Godot 4.7.2 e execute com F6/F5. A versão `0.0.3` adiciona fases finitas e chefes à mineração.

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

O marco atual mostra uma tela inicial com **Iniciar escavação** ou **Continuar** quando encontra progresso salvo. Toque e segure (ou clique e segure) no terreno para escavar; veios mais raros e profundos valem mais moedas. A primeira venda concede um bônus único de 8 moedas. O HUD mostra a meta atual. Com 30 moedas, transforme a Broca Pioneira em Broca Prisma: o padrão muda de uma célula para três células em leque. A mina acompanha o avanço em profundidade; o álbum fica no botão superior direito e a descoberta aparece a partir de 16 células. O progresso e dano parcial de pedras resistentes são gravados automaticamente em armazenamento persistente `user://` ao escavar, vender, melhorar e pausar/fechar. Há backup do save anterior válido para recuperação. Veja [referências e decisões de design](docs/REFERENCIAS-DE-DESIGN.md), inclusive os momentos planejados para rewarded ads; anúncios ainda não estão integrados.

No editor, abra `project.godot` com Godot 4.7.2 e rode F6/F5. No desktop, use o mouse; o jogo está em retrato. Para testar a lógica e a persistência, rode:

```bash
GODOT_BIN=.tools/godot/Godot_v4.7.2-stable_linux.x86_64 bash check-project.sh
GODOT_BIN=.tools/godot/Godot_v4.7.2-stable_linux.x86_64 bash scripts/run-harnesses.sh
```

Em `0.0.2`, o HUD abre três menus: **Baús**, **Equipamentos** e **Personagem**. Os 60 equipamentos ficam em grupos de dez nas raridades Ruim, Boa, Épica, Lendária, Única e Mística. Baús também podem liberar brocas e skins. A broca é uma companheira com rosto, modelos de escavação próprios e skins equipáveis com habilidade inata; bônus e penalidades alteram força, velocidade, moedas por bloco e sorte. Cada encaixe aceita um equipamento. Baús e chaves continuam vindo da escavação, sem compra nem anúncio. O save esquema 3 migra os esquemas 1 e 2 e preserva itens já adquiridos.

Em `0.0.3`, cada fase tem 24 trechos de profundidade e terreno/minérios próprios. Há três ambientes regulares e uma Fenda Aurora surpresa com chance de 22% após vencer o chefe. Cada chefe bloqueia a escavação para baixo; toque e segure nele para atacar. Derrotá-lo abre a fase seguinte e paga moedas. Chaves e baús ficam em blocos marcados para exploração, com limite por fase (2 de cada nas fases regulares; 3 na Fenda); coletar todos é opcional. O álbum recebe uma descoberta própria por ambiente. O save esquema 5 migra versões anteriores e conserva fases futuras sorteadas.

Confira o histórico e as notas em [VERSOES.md](docs/VERSOES.md). Conteúdo de brocas, equipamentos e região permanece separado dos sistemas para facilitar novas expansões.

Atualização de 20/10/2026: o ambiente de nuvem tem Android SDK, ADB e templates Android do Godot configurados; a exportação e a assinatura do APK de depuração foram verificadas. Não foi integrado plugin de anúncios, nem exibido rewarded ad de teste. Validar instalação, toque, execução, anúncio e FPS depende de aparelho Android. A revisão visual automática não ficou disponível: o modo de captura do Godot abortou no container sem backend de janela.

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

Foram criados o projeto Godot, uma cena de abertura, instruções para desenvolvimento autônomo e scripts de verificação. A importação local foi executada com Godot 4.7.2. Ainda não há mecânica de jogo, APK ou anúncios integrados. O ambiente do Codex Cloud permanece pendente de configuração; o workflow do GitHub é uma verificação separada. A meta de 60 FPS em Android intermediário ainda precisa ser medida em aparelho.
