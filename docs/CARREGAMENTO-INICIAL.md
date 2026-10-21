# Carregamento e entrada no combate — 0.0.24

O usuário pediu **Iniciar mineração** em branco, sem a placa verde, com letras grossas como a logo e tamanho moderado. Pediu também uma barra branca enquanto o jogo prepara a próxima tela, liberando a ação só quando puder entrar no combate.

## Comportamento implementado

- A tela usa o fundo cartoon v2 e a logo v3 aprovada. O recurso de fundo v1 foi substituído no Unity; o GUID foi preservado na troca, e a versão anterior continua no histórico Git.
- A ação é texto **INICIAR MINERAÇÃO**, branco puro, Titan One 42 no Canvas de referência 1080×1920. Não há fundo, borda ou placa. A área invisível de toque continua larga, independente do tamanho das letras; pressão move apenas o texto pelo componente já existente.
- Antes da preparação, a ação fica oculta e não recebe cliques. A tela exibe **CARREGANDO...**, trilho branco a 20% de opacidade e preenchimento branco puro.
- O primeiro frame pertence à tela de abertura. Em seguida, a coroutine usa `Resources.LoadAsync` para os caminhos de `StartupContent.ResourcePaths`: arte inicial/fonte, dados essenciais, atlas, sprites principais de combate, fundos e shader. O manifesto tem 18 caminhos e é validado contra os arquivos do repositório.
- A barra representa progresso por tarefas: recursos até 75%, perfil/driver até 83%, HUD/Oficina até 94%, pools de feedback até 100%. O progresso interno de cada solicitação assíncrona também é considerado. Os pesos representam etapas, não bytes ou estimativa de duração.
- Catálogos menores e leitura/validação do save continuam síncronos na etapa do driver; construção do HUD e aquecimento dos pools continuam síncronos em etapas separadas por frames. Não há promessa de ausência de travamentos nesses trabalhos sem medir no Editor/aparelho.
- Não há `WaitForSeconds`, tempo mínimo artificial, sorteio ou tween de progresso. Em cache, a abertura pode terminar rapidamente. A barra cheia é mostrada em um frame antes de liberar a ação.
- Só depois de todas as etapas aparece **INICIAR MINERAÇÃO**. O toque retoma a tentativa salva ou começa uma nova; em seguida abre o combate. Não é necessário um segundo menu ou toque.
- Durante a preparação e a espera pelo toque, o driver permanece pausado. Erro de recurso, preparação ou save impede a liberação da ação, mostra mensagem e registra o detalhe no Console. O progresso inválido não é substituído por perfil vazio.
- A preparação tem um único dono (`PrototypeGameUI`) e uma única coroutine. Inicialização repetida é ignorada; ao destruir a interface, a continuação é encerrada e a inscrição nos eventos é removida. Solicitações nativas já iniciadas podem terminar internamente sem callback para uma interface destruída.

Essa tela começa após o bootstrap da cena, já dentro do jogo. Ela não substitui a tela de lançamento do Android nem o tempo de inicialização da engine anterior ao primeiro frame.

## Arquivos principais

- `DigAndWonderBootstrap.cs`: monta câmera, Canvas e EventSystem; delega a preparação à interface, em vez de carregar o driver antes de haver tela.
- `PrototypeGameUI.cs`: monta a abertura, atualiza o progresso e prepara driver, HUD e pools antes de liberar a ação.
- `StartupContent.cs`: lista estável dos recursos carregados na preparação.
- `Art/StartMenu/mine-start-v2.png`: fundo cartoon; logo aprovada permanece em `dig-and-wonder-logo-v3.png`.
- `scripts/preview-start-menu.py`: prévias independentes dos estados pronto/carregando.

O save permanece no esquema 3. Recompensas, custos, dano, vida, XP, poderes, velocidades e regras de avanço da campanha não foram alterados neste marco. Os relatórios de economia agora usam `VERSION` para nomear seus arquivos.

## Verificações e limitações

| Verificação | Resultado / limite |
| --- | --- |
| DomainChecks | 79 aprovados: 78 anteriores e um novo check de existência/unicidade dos recursos e seus metadados |
| Compilação com stubs | Sem erros/avisos; não executa coroutines |
| Compilação contra APIs reais | Sem erros/avisos com as DLLs do Editor antigo/template; não confirma resolução/importação no 6000.6.6f1 |
| Composição | Estados pronto/carregando em Chromium; inspeção em 1080×1920 e 390×844, capturas adicionais 360×640 e 360×800 |
| Unity batch / Play Mode atual | Pendente: 6000.6.6f1 não está instalado na nuvem; ausência já registrada no marco anterior |
| APK, toque, FPS, duração do loading e fluidez da barra | Não medidos nesta versão; dependem do Editor vigente e de aparelho para cobertura Android |

Logs locais: `.tools/logs/start-loading-stubs.log`, `start-loading-real-api.log`, `start-loading-domain.log` e `start-loading-economy-report.log`.

Prévia completa: [pronto](design/start-menu-v3/preview-ready.png) e [carregando](design/start-menu-v3/preview-loading.png). São composições externas ao Unity.

## Roteiro no Editor/aparelho

1. Abrir o projeto com Unity 6000.6.6f1 e deixar importar. Entrar em Play na cena Prototype.
2. Confirmar loading branco antes da ação e ausência da placa verde. O carregamento rápido não deve ser retardado de propósito.
3. Clicar durante a preparação: não deve começar combate. Ao concluir, confirmar que a barra some e a ação aparece.
4. Tocar **INICIAR MINERAÇÃO** e confirmar a tela de combate. Com save de teste existente, esperar no menu e verificar que vida/tempo/recompensas não mudam; tocar e retomar.
5. Em cópia isolada, retirar temporariamente um recurso obrigatório: esperar mensagem de falha, ação indisponível e nenhum reset de save. Restaurar o arquivo.
6. Sair de Play durante o carregamento e entrar novamente; conferir ausência de callbacks, UI ou inscrições duplicadas. Testar pause/resume do aplicativo durante a preparação.
7. Em Android, medir primeira abertura e cache, toque e texto em telas diferentes. A prévia Chromium não comprova isso.
