using System;
using System.Collections.Generic;

namespace DigAndWonder
{
    public sealed class DiscoveryDefinition
    {
        public readonly int Stage;
        public readonly string Name, Description;
        public bool IsRelic => Stage % 10 == 0;
        public DiscoveryDefinition(int stage, string name, string description)
        { Stage = stage; Name = name; Description = description; }
    }

    /// <summary>Achados garantidos por encontro concluído, sem sorteio, moeda ou bônus de poder.</summary>
    public static class DiscoveryCatalog
    {
        private static readonly DiscoveryDefinition[] entries = {
            new DiscoveryDefinition(5, "Folha de cristal", "Uma folha preservada em quartzo. A luz revela cada nervura."),
            new DiscoveryDefinition(10, "Bússola do guardião", "A agulha aponta para baixo. Ainda há muito para descobrir."),
            new DiscoveryDefinition(15, "Pegada de basalto", "Uma pequena criatura atravessou a lava antes que ela esfriasse."),
            new DiscoveryDefinition(20, "Forja de bolso", "Um núcleo antigo que continua morno, mesmo longe da fornalha."),
            new DiscoveryDefinition(25, "Semente ancestral", "Esta semente de pedra guarda o desenho de uma floresta perdida."),
            new DiscoveryDefinition(30, "Coração do jardim", "Raízes minerais envolvem uma esfera que brilha no escuro."),
            new DiscoveryDefinition(35, "Asa de âmbar", "Um inseto minúsculo deixou sua asa marcada para sempre."),
            new DiscoveryDefinition(40, "Prisma de ecos", "Cada face devolve uma cor diferente das profundezas."),
            new DiscoveryDefinition(45, "Concha espiral", "O formato da concha lembra a ponta de uma broca."),
            new DiscoveryDefinition(50, "Mapa das raízes", "Linhas gravadas mostram túneis muito mais antigos que a mina."),
            new DiscoveryDefinition(55, "Dente de titã", "Pequeno para seu antigo dono. Enorme para a coleção."),
            new DiscoveryDefinition(60, "Motor esquecido", "Engrenagens seladas contam a história de outra expedição."),
            new DiscoveryDefinition(65, "Flor de cobalto", "Pétalas metálicas cresceram ao redor de um fóssil vegetal."),
            new DiscoveryDefinition(70, "Lente da aurora", "Uma lente que separa a luz em faixas luminosas."),
            new DiscoveryDefinition(75, "Ovo geodo", "Por fora, pedra; por dentro, um pequeno céu de cristais."),
            new DiscoveryDefinition(80, "Estrela subterrânea", "Um fragmento de metal que parece ter vindo de muito longe."),
            new DiscoveryDefinition(85, "Espiral gigante", "A marca de uma criatura que escavava antes das máquinas."),
            new DiscoveryDefinition(90, "Chave das profundezas", "Seu mecanismo não abre portas: ele revela caminhos."),
            new DiscoveryDefinition(95, "Primeiro cristal", "Camadas incontáveis cercam o menor cristal da coleção."),
            new DiscoveryDefinition(100, "Núcleo Wonder", "O último guardião protegia uma luz. A expedição agora faz parte da sua história.")
        };
        public static IReadOnlyList<DiscoveryDefinition> Entries { get; } = Array.AsReadOnly(entries);
        public static DiscoveryDefinition Find(int stage) => Array.Find(entries, item => item.Stage == stage);
        public static bool TryAward(GameSaveData profile, int clearedStage)
        {
            if (Find(clearedStage) == null || profile.HighestClearedStage < clearedStage ||
                profile.DiscoveryStages.Contains(clearedStage)) return false;
            profile.DiscoveryStages.Add(clearedStage);
            profile.DiscoveryStages.Sort();
            return true;
        }
    }
}
