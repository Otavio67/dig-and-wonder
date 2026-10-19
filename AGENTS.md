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

1. Validar importação/execução da base e preparar exportação Android.
2. Gerar APK mínimo; testar toque e anúncio recompensado de teste em Android antes de expandir o jogo.
3. Implementar uma região, escavação/coleta/venda, uma melhoria, uma transformação distinta, descoberta e save.
4. Revisar visual e sensação de jogo antes de ampliar para três regiões, álbum e ajudantes.

## Verificação

Execute checks adequados ao que mudou. Ao implementar economia/save/transformações, teste recursos insuficientes, padrões distintos de destruição e recuperação do progresso. Headless não comprova qualidade visual, diversão, desempenho em Android ou funcionamento de anúncios. Documente resultados e limitações sem inventar verificações.

O estado inicial contém somente uma tela de abertura estática; ela não é um protótipo jogável. Não declarar APK, anúncios ou nuvem do Codex configurados sem evidência.
