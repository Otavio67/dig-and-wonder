# Continuidade na nuvem

Em 19/10/2026, o usuário pediu desenvolvimento pela nuvem e autorizou criar um repositório na conta GitHub `Otavio67`. O repositório [Otavio67/dig-and-wonder](https://github.com/Otavio67/dig-and-wonder) foi criado pelo navegador, com proprietário e visibilidade privada confirmados. A execução na nuvem ainda não foi configurada.

## Configuração

1. Concluído: entrar como Otavio67 e criar o repositório privado. O navegador está autenticado; confirmar separadamente o acesso da integração do Codex ao novo repositório.
2. Versionar o README, a especificação e os arquivos de projeto; excluir `.tools/`, `.godot/`, credenciais e builds.
3. No Codex, escolher **Work in > Cloud**, criar/selecionar um ambiente e incluir o repositório.
4. Instalar Godot Linux 4.7.2 e fixar templates da mesma versão. Permitir acesso aos distribuidores oficiais necessários.
5. Verificar `godot --version`; após existir `project.godot`, executar importação headless e testes.
6. Revisar o relatório e publicar o ambiente; iniciar o desenvolvimento no ambiente publicado.
7. Manter commits e documentação atualizados para tornar o repositório a fonte durável do projeto.

Fonte: [documentação oficial de ambientes do Codex Cloud](https://learn.chatgpt.com/docs/environments/cloud-environments).

## Android e revisão visual

Código e verificações podem rodar em Linux na nuvem. Gerar APK exige configurar JDK, Android SDK, templates e exportação, conforme a [documentação do Godot](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html). Não versionar chaves/senhas nem criar chave definitiva de publicação sem armazenamento e recuperação definidos.

Headless não comprova qualidade visual. O ambiente de nuvem consultado documenta limitações para controle de computador/navegador; definir um método viável de captura antes de prometer revisão visual automática. Validar toque, desempenho e anúncios em Android real.

## Prompt de continuidade

```text
Continue o Dig & Wonder em Godot 4/GDScript, para Android. Leia README.md e
docs/DESENVOLVIMENTO.md. A direção aprovada é mineração cartoon 2D, controle
simples, brocas que transformam o padrão de escavação, mundos inesperados,
álbum e tycoon simples. O público inclui jovens até aproximadamente 22 anos.

Configure e valide o ambiente com Godot 4.7.2. Implemente o primeiro protótipo:
uma região, escavação, coleta/venda, melhoria, uma transformação distinta,
uma descoberta e progresso salvo. Separe conteúdo dos sistemas principais.

Resolva detalhes reversíveis com autonomia. Use arte provisória consistente.
Não integre anúncios reais antes de validar o núcleo. Execute os testes
adequados e documente resultados, instruções reproduzíveis e o que exige
revisão visual ou Android real. Conclua esse marco antes de ampliar o conteúdo.
```
