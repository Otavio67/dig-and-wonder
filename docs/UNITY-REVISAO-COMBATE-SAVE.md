# Checkpoint Unity — combate e save (20/10/2026)

## Escopo e estado

Correções sobre `0ffe683`, na branch `prototype/first-playable`. Baseline limpo, Unity 6000.0.23f1, dez checks existentes passando. Sem mudanças em catálogos, preços, dano, cadência, recompensas ou schema 1 do perfil Unity. A versão de jogo publicada continua 0.0.7; este é um checkpoint de correções de código, sem novo APK.

**Validação do jogo pelo Editor: bloqueada.** O código e os testes de domínio foram executados fora do Editor. Não houve Play Mode, revisão visual, vídeo ou medição Android.

## Correções

- Vitória interrompe imediatamente os ataques pendentes. Um projétil inimigo posterior no mesmo passo não pode transformar a vitória em derrota.
- Um especial que derrota o boss encerra o passo antes de outro disparo. Vitória e derrota limpam inimigos, projéteis, fila e contadores de ataques; reiniciar preserva o perfil e restaura vida/cooldown pelas regras existentes.
- Ouro, chaves, baús e próximo estágio do boss são aplicados antes dos eventos de recompensa. O observador não recebe uma transação parcial.
- Autosave por evento é solicitado e realizado ao terminar o passo completo da simulação. Compra, suspensão e encerramento continuam com gravação imediata. O timer é reiniciado após cada tentativa de escrita.
- Falha de escrita pausa a simulação e bloqueia compras/avanço/reinício; a UI informa o erro e há nova tentativa a cada intervalo de autosave (5 s). Ao conseguir gravar, o combate pode continuar. Esse fluxo de driver/UI foi compilado e inspecionado; sua interação no Editor ainda precisa ser executada.
- Conteúdo `null`, números não finitos, contadores inválidos e entradas de snapshot ausentes são rejeitados. O backup pode ser usado sem uma exceção de desserialização interromper a inicialização.
- O temporário participa da detecção de progresso e da recuperação. Ordem: principal válido, backup válido, temporário válido. Um temporário inválido sozinho bloqueia a criação silenciosa de um perfil novo.
- A escrita usa `Flush(true)` e valida o temporário antes de substituir o principal. Isso não comprova resistência a toda perda de energia no filesystem Android; esse teste exige aparelho.

## Evidências executadas

| Verificação | Resultado | Limite |
| --- | --- | --- |
| `dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj` | 20 PASS, 0 FAIL | Simulação C# e arquivos temporários isolados; sem lifecycle Unity |
| Novos testes antes da correção | Oito falhas reproduzidas | Duas outras verificações novas cobrem preservação/prioridade de arquivos |
| `dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj --no-restore` | 0 erros, 0 avisos | Stubs Unity/DOTween |
| `dotnet build UnityProject/Tools/UnityApiCompileCheck/UnityApiCompileCheck.csproj -p:UnityRoot="$UNITY_PATH" --no-restore` | 0 erros, 0 avisos | DLLs reais do Editor/template; não valida o grafo UPM do projeto |
| Editor com `-batchmode -nographics -quit -buildTarget Android` | Exit 1: `No valid Unity Editor license found` | Interrompe antes da importação/compilação do projeto |
| `git diff --check` | Sem problemas de whitespace | Inspeção de diff |

A retomada agora compara todos os timers, alvos e projéteis do snapshot, e depois compara mais cinco segundos de execução retomada com a execução contínua. Os checks de save usam diretórios temporários próprios; o perfil do jogador não foi alterado.

Logs locais ignorados pelo Git: `.tools/logs/unity-domain-before-fixes.log`, `unity-domain-final.log`, `unity-stub-final.log`, `unity-api-final.log`, `unity-validation-final.log`.

O workflow `.github/workflows/unity-code.yml` executa os checks de domínio e a compilação com stubs sem licença do Editor. A execução hospedada no GitHub deve ser conferida separadamente; os resultados acima são desta máquina remota.

## Paridade numérica preservada

| Número | Fonte Godot | Unity após correção |
| --- | --- | --- |
| Vida base | 120 | 120 |
| Disparo básico / intervalo / segundo tiro | 6 / 1,25 s / 0,28 s | Mesmos valores |
| Pioneira | 32, 4 golpes, 0,7 s, cooldown 3 s | Mesmos valores |
| Torrão: PV / dano / ouro | 48 / 8 / 7 | Mesmos valores |
| Custo Dano | 18 + 14 × nível | Mesma fórmula |
| Bônus Dano / limite | +12% / 10 | Mesmos valores |
| Custo Vida / bônus / limite | 20 + 16 × nível / +35 PV / 10 | Mesmos valores |

Nenhum arquivo de conteúdo ou importador de ScriptableObjects foi alterado. A importação dos assets e a tabela completa dos ScriptableObjects reais continuam pendentes do Editor.

## Roteiro pendente no Editor

1. Em perfil de teste novo, iniciar, receber ouro e comprar Dano/Vida. Encerrar e abrir para conferir retomada.
2. Derrotar o boss com projéteis em voo; conferir vitória única, baús/chaves e ausência de ações posteriores. Avançar e conferir a fase salva.
3. Perder com especial/projéteis ativos; reiniciar e conferir vida, cooldown, ouro e limpeza dos efeitos.
4. Em diretório de teste isolado, impedir a escrita do `.tmp`; conferir pausa, aviso e botões bloqueados. Restabelecer escrita e conferir gravação/retomada sem duplicar recompensa.
5. Suspender/retomar o app e conferir que não há tempo de combate acumulado. No Android, repetir com toque, interrupção do processo e pouco espaço em disco.

Não usar o save de uso normal para testes de corrupção. A recuperação automatizada já usa arquivos descartáveis; não há motivo para apagar o progresso do jogador.

## Bloqueio de infraestrutura

O modo **Acesso completo** permite ações do agente. As permissões de sites do **Computador na nuvem** tratam da navegação. Nos testes executados, nenhum desses controles modificou a política do proxy do terminal: ela permaneceu `restricted`, sem hosts adicionais, e os serviços Unity responderam 403 ao CONNECT.

Isso não comprova ausência de login no navegador do usuário. O fato confirmado é que o processo do Editor remoto não conseguiu obter token/licença. O projeto permanece disponível para edição e testes C# independentes.
