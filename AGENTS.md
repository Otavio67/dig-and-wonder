# Dig & Wonder — instruções de desenvolvimento

Leia README.md e docs/DESENVOLVIMENTO.md antes de implementar. O usuário autorizou autonomia nas decisões técnicas e criativas reversíveis. Trabalhe em português e registre decisões relevantes. Não peça aprovação para cada detalhe.

## Economia de créditos

O usuário quer economizar o máximo possível. Trabalhe por marcos pequenos, agrupe leituras/alterações e mantenha um resumo durável do estado para reduzir releituras. Use ferramentas diretas quando disponíveis, limite verificações ao que a mudança exige e evite consultas repetidas sem nova evidência. Salve commits em pontos úteis, sem necessidade de um commit por pequeno ajuste. Não inicie uma implementação grande quando o marco autorizado for apenas configuração. Registre os testes feitos para evitar repeti-los sem motivo.

## Prioridades

- Bonito: cartoon 2D expressivo, identidade própria, interface limpa e efeitos legíveis.
- Leve: terreno por trechos, recursos otimizados, efeitos limitados e descarte de conteúdo distante.
- Jogável: controle preciso, escavação satisfatória antes de upgrades e progresso compreensível.
- Meta inicial: 60 FPS em Android intermediário. É uma meta pendente de medição em aparelho; não apresentar como resultado comprovado.

## Ambiente

Godot 4.7.2, GDScript, renderer Compatibility, Android como destino inicial. Desenvolvimento preferencial na nuvem. Instale com `bash setup-cloud.sh`; valide com `bash check-project.sh`. O workflow `.github/workflows/validate.yml` executa essas verificações no GitHub.

Não versione `.tools/`, `.godot/`, APKs, credenciais ou chaves. Recursos originais do jogo não recebem automaticamente a licença do Godot. Registre licenças dos recursos externos.

## Ordem de trabalho

Direção principal autorizada em 20/10/2026: defesa com escavação por camadas, carrinho fixo, dois canhões automáticos e unidades lançáveis. O modo manual em grade foi substituído. Especificação atual em `docs/DESENVOLVIMENTO.md`; material antigo em `docs/historico/`.

1. Marco 1: uma fase curta, camada ativa, Pioneira com cooldown, Torrão, ouro, melhorias de dano/vida, derrota/reinício, perfil 6 e importação segura de legado.
2. Marco 2 autorizado em 20/10: Pioneira de quatro golpes, Torrão pintado, clareza e somente o Escavador adicional. Implementação atual em docs/MARCO2.md. Não avançar ao Marco 3 automaticamente.
3. Marco 3: fases/desbloqueios e novos inimigos um por vez.
4. Marco 4: primeiro boss. Validar APK/toque/desempenho e anúncios isolados antes de publicação; não tornar anúncio requisito de tentativa.

## Verificação

Execute checks adequados ao que mudou. Ao implementar economia/save/transformações, teste recursos insuficientes, padrões distintos de destruição e recuperação do progresso. Headless não comprova qualidade visual, diversão, desempenho em Android ou funcionamento de anúncios. Documente resultados e limitações sem inventar verificações.

O protótipo atual usa a entrada de defesa; não manter um segundo modo completo em paralelo. Não declarar APK, anúncios ou nuvem do Codex configurados sem evidência.
