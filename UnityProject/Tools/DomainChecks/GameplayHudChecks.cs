using DigAndWonder;

internal static class GameplayHudChecks
{
    public static IEnumerable<(string Name, Action Test)> All()
    {
        yield return ("HUD: atributos acompanham compra e poder sem valores fixos", LiveStats);
        yield return ("HUD: sete melhorias respeitam requisito, saldo e máximo", UpgradeEligibility);
        yield return ("HUD: números pequenos não viram frações artificiais", NumberFormatting);
        yield return ("combate: especial manual respeita cooldown e alvo", ManualSpecial);
        yield return ("combate: AUTO desligado suspende disparos básicos", AutoToggle);
        yield return ("HUD: recortes cabem nos atlas e importação preserva pixels", AtlasRectangles);
    }

    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private static void LiveStats()
    {
        EconomyRules rules = EconomyChecks.Rules();
        var save = new GameSaveData { Gold = 1000000 };
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(rules), save, "prado_cristalino", 7);
        CharacterCombatData character = rules.CharacterRoster[0];
        float before = GameplayHudModel.ShotDamage(sim, character);
        double power = GameplayHudModel.PowerRating(sim, character);
        Require(sim.TryPurchaseUpgrade(UpgradeKind.Attack), "Compra básica recusada");
        Require(GameplayHudModel.ShotDamage(sim, character) > before, "ATQ desatualizado");
        Require(GameplayHudModel.PowerRating(sim, character) > power, "Força desatualizada");
        before = GameplayHudModel.ShotDamage(sim, character);
        save.Characters[0].Powers.Add(new PowerProgressData { PowerId = "serration", Rank = 1 });
        Require(GameplayHudModel.ShotDamage(sim, character) > before, "Poder não refletido no dano mostrado");
        Require(GameplayHudModel.UpgradeValue(UpgradeKind.Attack, sim, character).Contains(GameplayHudModel.Number(GameplayHudModel.ShotDamage(sim, character))), "Rótulo divergente");
    }

    private static void UpgradeEligibility()
    {
        EconomyRules rules = EconomyChecks.Rules();
        var save = new GameSaveData { Gold = 1000000 };
        Require(GameplayHudModel.UpgradeOrder.Distinct().Count() == 7, "Lista incompleta");
        foreach (UpgradeKind kind in GameplayHudModel.UpgradeOrder)
        {
            EconomyUpgradeRule rule = rules.Upgrade(kind);
            Require(GameplayHudModel.UpgradeUnlocked(save, rules, kind) == (rule.UnlockLevel == 0), "Requisito inventado");
        }
        save.Gold = 0;
        Require(!GameplayHudModel.CanBuyUpgrade(save, rules, UpgradeKind.Attack), "Botão habilitado sem ouro");
        save.Gold = int.MaxValue; save.AttackLevel = rules.Upgrade(UpgradeKind.Attack).MaximumLevel;
        Require(!GameplayHudModel.CanBuyUpgrade(save, rules, UpgradeKind.Attack), "Compra acima do teto");
        save.Characters[0].TotalExperience = rules.Experience.MaximumExperience;
        Require(GameplayHudModel.UpgradeUnlocked(save, rules, UpgradeKind.Repair), "Autorreparo nunca desbloqueia");
    }

    private static void NumberFormatting()
    {
        Require(GameplayHudModel.Number(6) == "6", "Escala de ATQ alterada");
        Require(GameplayHudModel.Number(6.5) == "6,5", "Separador incorreto");
        Require(GameplayHudModel.Number(15000) == "15K", "Abreviação incorreta");
        Require(GameplayHudModel.Number(double.NaN) == "—", "Valor não finito exposto");
    }

    private static void ManualSpecial()
    {
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(), new GameSaveData(), "prado_cristalino", 8);
        Require(!sim.TryActivateSpecial(), "Especial sem alvo/recarga");
        sim.Step(0.05f);
        sim.Run.SpecialCooldown = 0;
        Require(sim.TryActivateSpecial(), "Especial pronto recusado");
        Require(!sim.TryActivateSpecial(), "Toque repetido duplicou especial");
        Require(sim.Run.SpecialHitsRemaining == 4, "Padrão de quatro golpes alterado");
    }

    private static void AutoToggle()
    {
        var sim = new IdleCombatSimulation(EconomyChecks.Configuration(), new GameSaveData(), "prado_cristalino", 9);
        int shots = 0;
        sim.EventRaised += e => { if (e.Type == CombatEventType.DrillAttack) shots++; };
        sim.AutoAttackEnabled = false;
        for (int i = 0; i < 20; i++) sim.Step(0.05f);
        Require(shots == 0, "AUTO OFF dispara tiros básicos");
        sim.AutoAttackEnabled = true; sim.Step(0.05f);
        Require(shots > 0, "AUTO ON não retoma");
    }

    private static void AtlasRectangles()
    {
        string root = EconomyChecks.Root;
        string source = File.ReadAllText(Path.Combine(root, "UnityProject/Assets/_Game/Scripts/Runtime/PrototypeGameUI.cs"));
        string pattern = "SliceUiAtlas\\(\"Art/UI/([^\"]+)\", new\\[\\]\\s*\\{(.*?)\\}, new\\[\\]\\s*\\{(.*?)\\}\\);";
        var blocks = System.Text.RegularExpressions.Regex.Matches(source, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        Require(blocks.Count == 3, "Manifesto dos atlas não encontrado");
        foreach (System.Text.RegularExpressions.Match block in blocks)
        {
            string path = Path.Combine(root, "UnityProject/Assets/_Game/Resources/Art/UI", block.Groups[1].Value + ".png");
            byte[] bytes = File.ReadAllBytes(path);
            int width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
            int height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
            Require(File.ReadAllText(path + ".meta").Contains("nPOTScale: 0"), "Atlas redimensionado na importação: " + path);
            var rectangles = System.Text.RegularExpressions.Regex.Matches(block.Groups[3].Value, @"new RectInt\((\d+), (\d+), (\d+), (\d+)\)");
            foreach (System.Text.RegularExpressions.Match rectangle in rectangles)
            {
                int x = int.Parse(rectangle.Groups[1].Value), y = int.Parse(rectangle.Groups[2].Value);
                int w = int.Parse(rectangle.Groups[3].Value), h = int.Parse(rectangle.Groups[4].Value);
                Require(w > 0 && h > 0 && x + w <= width && y + h <= height, "Recorte fora da imagem: " + path);
            }
        }
    }
}
