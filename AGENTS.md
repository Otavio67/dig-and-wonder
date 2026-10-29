# Dig & Wonder — instruções de desenvolvimento

Leia `README.md`, `docs/DESENVOLVIMENTO.md` e `UnityProject/README.md` antes de implementar. Trabalhe em português. O usuário autorizou decisões técnicas reversíveis e atualização do GitHub por marcos; não peça aprovação para cada detalhe.

## Direção ativa

- Unity 6, C#, Android inicial, combate automático lateral e broca protagonista. Branch ativa: `prototype/first-playable`.
- Em 29/10/2026 o usuário pediu remover o material antigo do Godot. Não reintroduza projeto, scripts, plugins, builds ou testes daquela engine. O histórico permanece no Git; os JSON compartilhados em `content/` continuam como fontes numéricas e de design do Unity.
- A versão do Editor é a de `UnityProject/ProjectSettings/ProjectVersion.txt`. Abra `UnityProject/` pelo Hub. Se a licença não for reconhecida, confira a instalação MSIX do Hub e atualize as licenças antes de concluir que o Editor não funciona.
- O jogo abre na tela ilustrada com a logo v3 aprovada. Loading branco acompanha recursos reais; somente depois aparece **INICIAR MINERAÇÃO**, branca em Titan One, sem placa/fundo. Não reintroduza loading por tempo decorativo ou Acampamento como entrada.
- Vitória segue para o próximo mapa dentro do jogo. Derrota permite melhorar/recomeçar ali; a fase 100 encerra a campanha e permite treino. Oficina, poderes, tesouro e coleção ficam na navegação. Pets e Torre/Profundezas são futuros, sem promessa de implementação.

## Identidade visual

Antes de gerar ou editar qualquer imagem, leia integralmente `docs/IDENTIDADE-TOY-MECHA.md` e a skill aplicável. Execute a checagem da seção 13 antes do prompt e após o resultado; informe que está aplicando o guia. O guia prevalece sobre documentos históricos. Instruções novas do usuário têm precedência e devem ser registradas.

Preserve referências e a logo aprovada. Não gere arte sem tarefa autorizada. Texto e números funcionais ficam no Unity, com recursos modulares. Para ícones, prefira fontes SVG editáveis; não use emojis ou glifos como recursos. Conceito não é captura de gameplay.

## Fontes e integridade

- Preserve economia, balanceamento, catálogos e compatibilidade de save ao fazer alterações visuais ou limpeza. `ContentCatalogImporter` lê os JSON do repositório e mantém os ScriptableObjects.
- Preserve GUIDs ao renomear assets e não substitua progresso do usuário por fixtures. Use perfis de teste isolados para compras e ensaios.
- Não versione `.tools/`, `Library`, `Temp`, `UserSettings`, APKs, saves, credenciais ou chaves. Preserve licenças das fontes, DOTween e demais dependências.

## Verificação e entrega

Execute os checks adequados ao que mudou. A base é:

```powershell
dotnet run --project UnityProject/Tools/DomainChecks/DomainChecks.csproj
dotnet build UnityProject/Tools/UnityCompileCheck/UnityCompileCheck.csproj
```

Compilação com stubs não confirma importação nem renderização. Confirme erros no Editor quando possível; descreva exatamente o que foi testado. Meta de 60 FPS Android exige medição em aparelho. Não apresente screenshots conceituais, build técnico ou testes headless como aprovação visual, diversão ou desempenho.

Economize créditos: agrupe leituras e alterações, mantenha contexto durável e evite repetir testes sem nova evidência. Salve commits e atualize o GitHub por marcos úteis. Prefira modelos leves em tarefas simples quando disponíveis; o seletor controla o modelo, não afirme uma troca sem evidência. Mantenha o modo rápido desligado.
