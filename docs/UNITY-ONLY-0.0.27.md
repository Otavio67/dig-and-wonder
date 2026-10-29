# Checkpoint 0.0.27 — repositório Unity

29/10/2026. Limpeza solicitada pelo usuário e preparação do repositório público. Base: `f11ed03`, sem reescrever o histórico Git.

## Removido da árvore atual

262 arquivos da implementação antiga: `project.godot`, cenas e recursos Godot, GDScript e UIDs, plugin AdMob daquela engine, harnesses, scripts de instalação/exportação/captura Godot, workflow antigo, arte usada somente naquela implementação e seus documentos/capturas históricos.

O projeto ativo permanece em `UnityProject/`. `content/` conserva os catálogos JSON de combate e de design futuro; nenhum deles exige Godot. As fontes SVG atuais ficam em `assets/ui/gameplay/`. O estado anterior pode ser consultado no commit `f11ed03`, sem repor os arquivos no checkout.

## Dependências corrigidas

- `GodotCatalogImporter` virou `ContentCatalogImporter`, com o mesmo GUID, menu **Import content data** e os mesmos valores. O preparador usa o novo nome.
- Os checks localizam a raiz por `UnityProject/ProjectSettings/ProjectVersion.txt`, sem precisar de `project.godot`. O teste de paridade continua comparando os mesmos JSON.
- O exportador de ícones usa as fontes canônicas atuais, sem copiar SVGs da pasta antiga.
- README, instruções do agente e guias de entrada passaram a descrever apenas o fluxo Unity.
- Incluídos os 14 `.meta` atualizados pelo Editor na importação dos ícones. GUIDs e fontes gráficas preservados.

## Validação e limites

A limpeza não altera economia, schema de save, combate ou arte aprovada do Unity. Validação local em 29/10/2026: 85 checks de domínio aprovados, sem falhas; compilação com stubs e compilação contra DLLs reais do Unity 6000.6.6f1 concluídas sem erros (execução incremental). A sintaxe do exportador de ícones e `git diff --check` também passaram. Nenhum arquivo rastreado específico do Godot permanece na árvore atual. Não foi executado um novo Play Mode nesta limpeza. A revisão visual de barras, sprites e HUD e os testes Android continuam pendentes; este checkpoint não afirma correção desses problemas.

O repositório público deve abrir na branch Unity `prototype/first-playable`. A publicação não adiciona licença geral de código aberto nem muda os termos das dependências. O material retirado continua recuperável no histórico anterior.
