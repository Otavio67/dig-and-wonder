# Dig & Wonder — instruções de desenvolvimento

Leia README.md e docs/DESENVOLVIMENTO.md antes de implementar. O usuário autorizou autonomia nas decisões técnicas e criativas reversíveis. Trabalhe em português e registre decisões relevantes. Não peça aprovação para cada detalhe.

## Continuidade local — 20/10/2026

A direção ativa é Unity 6, combate automático lateral e broca protagonista, na branch `prototype/first-playable`. O usuário confirmou em 20/10/2026 que devemos focar em Unity. Leia também `UnityProject/README.md`, `docs/CONTINUAR-UNITY-LOCAL.md` e `docs/UNITY-CONTEXTO-LOCAL.md`. Godot é somente histórico/origem preservada; não orientar configuração, testes ou desenvolvimento por esse caminho. O marco atual é preparar e validar o protótipo Unity existente neste PC, sem ampliar conteúdo.

## Identidade visual — leitura obrigatória antes de gerar imagens

Antes de gerar ou editar qualquer imagem de Dig & Wonder, leia integralmente `docs/IDENTIDADE-TOY-MECHA.md` e as instruções da skill de geração/edição aplicável. Execute a checagem da seção 13 antes de montar o prompt e revise o resultado pelos mesmos critérios antes de entregar. Informe brevemente ao usuário que está aplicando o guia.

Este guia é a referência vigente para novas imagens e revisões visuais Unity. Em caso de divergência, ele prevalece sobre paletas, tipografia, personagem protagonista e composição dos documentos históricos `docs/DIRECAO-DE-ARTE.md`, `docs/IDENTIDADE-VISUAL.md` e descrições visuais antigas de `docs/OFICINA-PERSONAGEM.md`. Preserve o histórico; regras de gameplay, economia e save não mudam por causa da direção de arte. Instruções novas do usuário têm precedência sobre o guia e devem ser registradas quando alterarem a identidade.

Use as referências aprovadas para manter o mesmo personagem e materiais. Não gere imagens sem uma tarefa autorizada. Conceitos devem ser identificados como conceitos; capturas reais, como capturas. Para UI funcional, texto e números ficam no Unity e recursos visuais são modulares. Ícones de UI devem ter fonte vetorial editável quando apropriado, sem emojis/glifos de recursos.

## Economia de créditos

O usuário quer economizar o máximo possível. Trabalhe por marcos pequenos, agrupe leituras/alterações e mantenha um resumo durável do estado para reduzir releituras. Use ferramentas diretas quando disponíveis, limite verificações ao que a mudança exige e evite consultas repetidas sem nova evidência. Salve commits em pontos úteis, sem necessidade de um commit por pequeno ajuste. Não inicie uma implementação grande quando o marco autorizado for apenas configuração. Registre os testes feitos para evitar repeti-los sem motivo.

Preferência de modelos: priorizar GPT-6 Luna com esforço leve em configuração, documentação e alterações simples; reservar Sol para arquitetura, depuração difícil ou integrações complexas quando necessário. Manter modo rápido desligado. O seletor da interface controla o modelo; não afirmar uma troca sem verificar. Atualizar o GitHub em lotes por marcos funcionais ou pontos de recuperação.

## Prioridades

- Bonito: cartoon 2D expressivo, identidade própria, interface limpa e efeitos legíveis.
- Leve: terreno por trechos, recursos otimizados, efeitos limitados e descarte de conteúdo distante.
- Jogável: controle preciso, escavação satisfatória antes de upgrades e progresso compreensível.
- Meta inicial: 60 FPS em Android intermediário. É uma meta pendente de medição em aparelho; não apresentar como resultado comprovado.

## Ambiente

Unity 6000.0.23f1, C#, Android como destino inicial. Abra `UnityProject/` pelo Unity Hub. Para checks .NET independentes do Editor, use os comandos registrados em `UnityProject/README.md`. O workflow `.github/workflows/validate.yml` valida a origem Godot e não substitui a validação do Unity.

Não versione `.tools/`, `UnityProject/Library/`, APKs, credenciais ou chaves. Registre licenças dos recursos externos.

## Ordem de trabalho

Direção principal autorizada em 20/10/2026: defesa com escavação por camadas, carrinho fixo, dois canhões automáticos e unidades lançáveis. O modo manual em grade foi substituído. Especificação atual em `docs/DESENVOLVIMENTO.md`; material antigo em `docs/historico/`.

1. Marco 1: uma fase curta, camada ativa, Pioneira com cooldown, Torrão, ouro, melhorias de dano/vida, derrota/reinício, perfil 6 e importação segura de legado.
2. Marco 2 autorizado em 20/10: Pioneira de quatro golpes, Torrão pintado, clareza e somente o Escavador adicional. Implementação atual em docs/MARCO2.md. Não avançar ao Marco 3 automaticamente.
3. Marco 3: fases/desbloqueios e novos inimigos um por vez.
4. Marco 4: primeiro boss. Validar APK/toque/desempenho e anúncios isolados antes de publicação; não tornar anúncio requisito de tentativa.

## Verificação

Execute checks adequados ao que mudou. Ao implementar economia/save/transformações, teste recursos insuficientes, padrões distintos de destruição e recuperação do progresso. Headless não comprova qualidade visual, diversão, desempenho em Android ou funcionamento de anúncios. Documente resultados e limitações sem inventar verificações.

O protótipo atual usa a entrada de defesa; não manter um segundo modo completo em paralelo. Não declarar APK, anúncios ou nuvem do Codex configurados sem evidência.
