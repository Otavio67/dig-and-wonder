# Dig & Wonder

Jogo casual de mineração cartoon 2D para Android. Engine escolhida: Godot 4, GDScript. Nome provisório: Dig & Wonder.

## Contexto recuperado em 19/10/2026

Fontes: conversas “Publicar jogo Android com anúncios” e “Branch · Publicar jogo Android com anúncios”. Após uma recuperação parcial, o usuário forneceu o texto completo da proposta aprovada neste chat.

Direção aprovada: escavar, coletar, vender, transformar a broca e descobrir mundos subterrâneos inesperados. Transformações devem mudar o padrão de escavação, além dos números. Coleção de descobertas e ajudantes automáticos complementam uma progressão simples.

## Documentos

- [Especificação e roteiro de desenvolvimento](docs/DESENVOLVIMENTO.md)
- [Continuidade na nuvem](docs/NUVEM.md)

## Executar e verificar

Abra `project.godot` no Godot 4.7.2 e execute com F6/F5. A base atual contém uma tela de abertura estática; ainda não há escavação jogável.

Em Linux x86_64:

```bash
bash setup-cloud.sh
bash check-project.sh
```

O instalador baixa a versão fixada do distribuidor oficial e confere o SHA-512. O workflow `.github/workflows/validate.yml` executa importação e inicialização no GitHub Actions. Esses checks não comprovam qualidade visual, 60 FPS, funcionamento em Android ou anúncios.

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
