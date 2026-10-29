using System;
using System.Collections.Generic;

namespace DigAndWonder
{
    /// <summary>Recursos essenciais/volumosos carregados antes de liberar a entrada no combate.</summary>
    public static class StartupContent
    {
        public static IReadOnlyList<string> ResourcePaths { get; } = Array.AsReadOnly(new[]
        {
            "Art/StartMenu/mine-start-v2",
            "Art/StartMenu/dig-and-wonder-logo-v3",
            "Fonts/Workshop/TitanOne-Regular",
            "Data/CombatBalance",
            "Data/Economy",
            "Data/Progression",
            "Art/UI/dw_hud_top_atlas",
            "Art/UI/dw_hud_combat_atlas",
            "Art/UI/dw_hud_structure_atlas",
            "Art/UI/dw_nav_icons_v2",
            "Art/UI/Extracted/gold_coin",
            "Art/Drill/drill_pioneira",
            "Art/Projectiles/drill_projectile",
            "Art/UI/Icons/nav_album",
            "Art/Enemies/lama",
            "Art/Enemies/morcego",
            "Art/Enemies/aranha-pedra",
            "Art/Enemies/mineiro",
            "Art/Enemies/toupeira",
            "Art/Enemies/golem",
            "Art/Backgrounds/fundo-prado-cristalino-v1",
            "Art/Backgrounds/fundo-gruta-basaltica-v1",
            "Art/Backgrounds/fundo-jardim-fossil-v1",
            "Shaders/EnemyWhiteCutout"
        });
    }
}
