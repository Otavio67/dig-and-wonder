# Revisão de interface — 30/10/2026

Implementação na cópia local fora do OneDrive.

- Mina: área de cenário reduzida, missão destacada, poderes separados e força centralizada; lista com linhas mais altas e maior espaçamento, preço coral sem saldo, bloqueio escuro.
- Navegação: Mina, Modos, Oficina, Equipamentos e Loja. Álbum/baús saíram da barra principal.
- Perfil: retrato sem nível, XP preservado, nome local editável, ID local copiável e resumo de poderes.
- Oficina: poderes, melhorias e arsenal usam páginas próprias, sem o showcase sobre elas. Tesouro saiu da navegação da oficina. Arsenal permite selecionar as brocas e usa os ícones existentes enquanto aguarda as skins.
- Loja: página dedicada, suprimentos, cristais, pacotes, baús e loot. Ofertas em dinheiro são prévias indisponíveis. Não há cobrança nem gateway de pagamento.
- Caixas com cristais: preço usa GoldPackCrystalCost do catálogo vigente; sorteio e recompensas reutilizam ChestRewards. Não consomem baús/chaves guardados. Recompensa é concedida e salva antes da animação. Há tela de probabilidades.
- Animações: abertura/fechamento dos menus, transição de página, baú pulsante e revelação da recompensa.

Verificação: testes de domínio existentes, compilação com stubs e com DLLs reais do Unity. A geração do APK importa/compila pelo Editor. Essas verificações não comprovam composição visual, toque ou desempenho em aparelho.

Limites: equipamentos exibem os sistemas atuais e encaminham à oficina; inventário/troca de peças e skins exclusivas permanecem futuros. As ofertas de cristais/pacotes são somente interface. Não há validação visual em aparelho registrada nesta revisão.