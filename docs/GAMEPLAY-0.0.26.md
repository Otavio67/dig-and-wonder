# Gameplay — checkpoint 0.0.26

26/10/2026. Continuação de `dc60ebe`, preservando economia e schema de save.

- Unity Hub 3.22.3 localizado como pacote Windows/MSIX. O Hub reconheceu a licença Personal existente; após atualizar as licenças e manter o Hub aberto, o Editor 6000.6.6f1 abriu o projeto normalmente.
- Antes dos últimos ajustes visuais, o Play Mode carregou o menu e iniciou a mineração pelo botão real. Foram observados frames 5210, 11720 e 15806, com driver pronto, carregamento concluído e combate ativo. Duas leituras do Console retornaram zero erros/avisos. Isso confirma execução, sem comprovar toda a gameplay ou Android.
- A captura real revelou excesso de subsolo, pouco espaço para o cenário, títulos próximos às molduras e estrutura duplicada atrás dos poderes. Foram ajustadas as proporções do HUD, o recorte do fundo e a linha de apoio das unidades, removendo a estrutura redundante. Navegação inferior chega às bordas; AUTO tem texto escuro sobre a placa clara.
- Quatorze ícones em SVG editável e PNG transparente, com pictogramas simples para navegação e atributos. Fontes em `assets/ui/gameplay`, exportação em `scripts/export-gameplay-icons.cjs`; Mina e Baús reutilizam vetores anteriores do projeto. PNGs configurados como sprites individuais.
- Nunito oficial adicionado para valores secundários, com o peso padrão de leitura da fonte variável. Origem: `https://github.com/google/fonts/tree/main/ofl/nunito`; licença OFL já preservada em `UnityProject/Assets/_Game/Resources/Fonts/Workshop/Nunito-OFL.txt`.
- Saves de teste e capturas ficam somente em `.tools/`, ignorado pelo Git. Instrumentação temporária de inicialização da ponte removida do projeto.
- Antes do envio: 85 testes de domínio aprovados e compilação C# contra as DLLs instaladas do Unity concluída com zero erros e dois avisos anteriores de APIs obsoletas.

O controle do computador foi interrompido por Esc durante a conferência dos últimos ajustes. A composição final, o alinhamento em todos os biomas, o uso dos novos ícones/fontes em runtime e os testes de toque/Android continuam pendentes. Este envio é um checkpoint de desenvolvimento, sem aprovação visual final ou tag de release.
