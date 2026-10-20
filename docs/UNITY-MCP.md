# Conexão Unity MCP e licença Personal — 20/10/2026

## Resultado atual

- Servidor **Coplay MCP for Unity 10.0.0** instalado e em execução local em `http://127.0.0.1:8080/mcp`.
- Cliente `UnityMCP` registrado e habilitado no Codex CLI deste ambiente.
- Handshake MCP real passou com protocolo `2025-11-25`; `tools/list` anunciou **29 ferramentas** no conjunto padrão.
- A leitura real de `mcpforunity://instances` retornou **zero instâncias**. O Editor ainda não está conectado; leitura de hierarquia/Console e controle de cenas pelo Editor permanecem pendentes.
- Ponte Unity declarada em `UnityProject/Packages/manifest.json`, fixada no commit `d49ae2953580f3481beb1e084a1da2682f0b5610` do release v10.0.0. A importação desse pacote pelo Editor aguarda a licença e o registro de pacotes.

O provider [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp/tree/v10.0.0) é MIT, independente da Unity Technologies, aceita Unity 2021.3–6.x e fornece a ponte sem assinatura Unity AI. É o único provider configurado. Seus processos de servidor são ferramentas de desenvolvimento nesta máquina, sem mudança do ciclo de combate.

## Reproduzir a preparação

```bash
bash scripts/setup-unity-mcp.sh
bash scripts/unity-mcp-server.sh
```

O segundo comando mantém o servidor em primeiro plano. A instância atual é mantida numa sessão de execução remota, com log `.tools/logs/unity-mcp-server.log` e PID em `.tools/unity-mcp-server.pid`. Antes de iniciar outra, confira a porta 8080. Reiniciar o ambiente pode exigir iniciar o servidor novamente.

O instalador verifica a revisão Git e usa `Server/uv.lock` upstream com `uv sync --frozen --no-dev`. Telemetria Python desativada e endereço de escuta limitado a loopback. Nenhum segredo é necessário para esse servidor local.

Configuração equivalente do cliente:

```bash
codex mcp add UnityMCP --url http://127.0.0.1:8080/mcp
```

O registro já existe nesta instância; o comando não precisa ser repetido. O registro no Codex CLI não injeta ferramentas automaticamente na lista de conectores de uma conversa ChatGPT já aberta. O teste nesta conversa usa um cliente do SDK MCP para acessar diretamente o servidor.

Para verificar cliente, servidor e presença real do Editor:

```bash
.tools/unity-mcp-v10/Server/.venv/bin/python scripts/check-unity-mcp.py
```

Retornos: `0` = servidor e pelo menos um Editor conectado; `2` = MCP respondeu, porém nenhum Editor está conectado; `1` = falha na consulta. Resultado observado nesta preparação: **2**. Log: `.tools/logs/unity-mcp-check.log`.

Depois da ativação e importação bem-sucedidas, abrir `UnityProject` no Editor e usar **Window → MCP for Unity** para iniciar a sessão HTTP local apontando para `http://127.0.0.1:8080`. A conexão completa será confirmada por uma instância dessa cópia do projeto e uma consulta de hierarquia/Console, antes de executar edições.

## Ativação Personal pendente

O usuário informou Unity Personal. O ambiente não tem sessão Unity, token, serial ou arquivo de licença configurados. A tentativa com o cliente oficial instalado, usando `--activate-all --include-personal`, retornou:

```text
Usage: --activate-all should be used with --accessToken or --username and --password
```

O login da conta Unity é uma etapa necessária. A opção `--include-personal` é descrita pelo `--help` do cliente como fallback Personal quando a conta não tem seats atribuídos. A autenticação deve vir do fluxo oficial da conta; a instalação do Editor ou do plugin ChatGPT não fornece essa identidade.

O auxiliar `scripts/activate-unity-personal.py` está pronto para usar `UNITY_ACCESS_TOKEN` fornecido como **segredo do ambiente**, obtido de uma autenticação Unity válida. Esse auxiliar não realiza o login OAuth nem obtém o token. Ele chama o cliente oficial, limita a duração e remove o valor do token do próprio output. Sem autenticação, retorna `2` e informa a pendência. Não inclui credenciais nos arquivos versionados.

Após configurar a autenticação e a rede:

```bash
python3 scripts/activate-unity-personal.py
bash scripts/check-unity.sh
```

Sucesso do cliente de ativação ainda precisa ser confirmado por uma inicialização/importação do Editor. A verificação batch anterior registrou `No valid Unity Editor license found. Please activate your license.`

## Rede do ambiente

Na configuração de rede do ambiente remoto, os hosts `id.unity.com`, `license.unity3d.com` e `packages.unity.com` precisam de acesso: os três tiveram bloqueio explícito **403 ao CONNECT pelo proxy**. A mudança é feita pelo fluxo de configuração do ambiente; nenhuma regra/proxy foi alterada por scripts. O acesso à autenticação e licença é indispensável para ativar a conta; o registro de pacotes é necessário para importar o projeto e a ponte.

Logs de diagnóstico em `.tools/logs/unity-personal-activation.log` e `unity-license-help.log`. A ativação da licença, a importação da ponte, um Editor conectado e testes em Play Mode continuam **pendentes**. Nenhum APK novo foi produzido nesta preparação.
