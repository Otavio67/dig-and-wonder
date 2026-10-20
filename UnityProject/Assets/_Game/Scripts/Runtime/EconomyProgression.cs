using System;
using System.Linq;

namespace DigAndWonder
{
    public enum AttackPowerKind { Serration, Resonance, BossBreaker, Ricochet }

    [Serializable]
    public sealed class EconomyUpgradeRule
    {
        public UpgradeKind Kind;
        public string Name;
        public string Description;
        public int BaseCost;
        public double CostGrowth;
        public int MaximumLevel;
        public int UnlockLevel;
        public double Bonus;
        public int Cost(int level) => EconomyRules.Scale(BaseCost, CostGrowth, level);
    }

    [Serializable]
    public sealed class AttackPowerRule
    {
        public string Id;
        public string Name;
        public string Description;
        public AttackPowerKind Kind;
        public int UnlockLevel;
        public int MaximumRank;
        public int BaseCost;
        public double CostGrowth;
        public double Bonus;
        public int Cost(int rank) => EconomyRules.Scale(BaseCost, CostGrowth, rank);
    }

    [Serializable]
    public sealed class ChestRewardRule
    {
        public string Name;
        public int Weight;
        public int GoldMultiplier;
        public int Crystals;
    }

    /// <summary>Tabela autoral da Unity. Valores históricos do Godot continuam preservados.</summary>
    [Serializable]
    public sealed class EconomyRules
    {
        public int MaximumStage;
        public double EnemyHealthBase, EnemyDamageBase, EnemyHealthGrowth, EnemyDamageGrowth;
        public int EndgameStartStage;
        public double EndgameHealthGrowth, EndgameDamageGrowth;
        public double BossHealthMultiplier, BossDamageMultiplier, GoldGrowth, ExperienceGrowth;
        public int EnemyGoldBase, BossGoldMultiplier;
        public int MinimumExperienceStep;
        public int WaveCount, LateWaveAfter, LateWaveSize;
        public float WaveInterval;
        public double CharacterDamagePerLevel, CharacterHealthPerLevel;
        public double AttackLevelMultiplier, HealthLevelMultiplier, BaseCriticalChance, BaseCriticalMultiplier;
        public int FirstClearCrystals, MilestoneInterval, MilestoneCrystals;
        public int KeyCrystalCost, GoldPackCrystalCost, GoldPackMultiplier;
        public CharacterExperienceRules Experience;
        public EconomyUpgradeRule[] Upgrades;
        public AttackPowerRule[] Powers;
        public ChestRewardRule[] ChestRewards;
        public CharacterCombatData[] CharacterRoster;
        public int DifficultyStepInterval;
        public double DifficultyHealthStep, DifficultyDamageStep;

        public void Validate()
        {
            if (MaximumStage < 1 || MaximumStage > 200 || EnemyGoldBase < 1 || BossGoldMultiplier < 1 ||
                WaveCount < 2 || WaveCount > 30 || LateWaveAfter < 0 || LateWaveSize < 1 || LateWaveSize > 3 ||
                float.IsNaN(WaveInterval) || WaveInterval < 1 || WaveInterval > 30 || MilestoneInterval < 1 ||
                FirstClearCrystals < 0 || MilestoneCrystals < 0 || KeyCrystalCost < 1 || GoldPackCrystalCost < 1 || GoldPackMultiplier < 1)
                throw new ArgumentException("Contadores inválidos na economia.");
            RequireRange(EnemyHealthBase, 1, 10000); RequireRange(EnemyDamageBase, 0.1, 1000);
            RequireRange(EnemyHealthGrowth, 1, 1.2); RequireRange(EnemyDamageGrowth, 1, 1.2);
            if (EndgameStartStage < 2 || EndgameStartStage > MaximumStage + 1)
                throw new ArgumentException("Início do end game inválido.");
            RequireRange(EndgameHealthGrowth, 1, 1.2); RequireRange(EndgameDamageGrowth, 1, 1.2);
            if (MinimumExperienceStep < 1 || MinimumExperienceStep > 10) throw new ArgumentException("Passo mínimo de XP inválido.");
            RequireRange(GoldGrowth, 1, 1.2); RequireRange(ExperienceGrowth, 1, 1.2);
            RequireRange(BossHealthMultiplier, 1, 20); RequireRange(BossDamageMultiplier, 1, 10);
            RequireRange(CharacterDamagePerLevel, 0, 0.1); RequireRange(CharacterHealthPerLevel, 0, 0.1);
            RequireRange(AttackLevelMultiplier, 1, 1.2); RequireRange(HealthLevelMultiplier, 1, 1.2);
            RequireRange(BaseCriticalChance, 0, 0.5); RequireRange(BaseCriticalMultiplier, 1, 5);
            if (DifficultyStepInterval < 1 || DifficultyStepInterval > MaximumStage) throw new ArgumentException("Degrau de dificuldade inválido.");
            RequireRange(DifficultyHealthStep, 1, 1.5); RequireRange(DifficultyDamageStep, 1, 1.5);
            if (CharacterRoster == null || CharacterRoster.Length == 0 || CharacterRoster.Any(x => x == null) ||
                CharacterRoster.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != CharacterRoster.Length)
                throw new ArgumentException("Catálogo de brocas vazio ou duplicado.");
            foreach (CharacterCombatData character in CharacterRoster)
            {
                if (string.IsNullOrWhiteSpace(character.Id) || string.IsNullOrWhiteSpace(character.DisplayName) || character.UnlockStage < 0 ||
                    character.UnlockStage > MaximumStage || character.BasicHitCount < 0 || character.BasicHitCount > 3 ||
                    character.SpecialTargetCount < 1 || character.SpecialTargetCount > 3)
                    throw new ArgumentException("Broca inválida no catálogo.");
                RequireRange(character.AttackMultiplier, .1, 100); RequireRange(character.HealthMultiplier, .1, 200);
                RequireRange(character.SpeedMultiplier, .5, 1.5); RequireRange(character.ProjectileDamageMultiplier, .5, 2);
                RequireRange(character.InnateRicochet, 0, .75);
                RequireRange(character.TintR, 0, 1); RequireRange(character.TintG, 0, 1); RequireRange(character.TintB, 0, 1);
            }
            if (!CharacterRoster.Any(x => x.Id == CharacterProgression.StarterCharacterId && x.UnlockStage == 0))
                throw new ArgumentException("Broca inicial ausente.");
            if (Experience == null || Upgrades == null || Powers == null || ChestRewards == null ||
                Upgrades.Length != 7 || Powers.Length != 4 || ChestRewards.Length < 1)
                throw new ArgumentException("Economia sem tabelas completas.");
            Experience.Validate();
            Experience.RequiredForNextLevel(Experience.MaximumLevel - 1);
            if (Experience.MaximumLevel > 100) throw new ArgumentException("Curva excede o marco de mid-game.");
            if (Upgrades.Any(x => x == null) || Powers.Any(x => x == null)) throw new ArgumentException("Entrada de conteúdo ausente.");
            if (Upgrades.Select(x => x.Kind).Distinct().Count() != Upgrades.Length ||
                Powers.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != Powers.Length ||
                Powers.Select(x => x.Kind).Distinct().Count() != Powers.Length)
                throw new ArgumentException("IDs duplicados na economia.");
            foreach (EconomyUpgradeRule rule in Upgrades)
            {
                if (rule == null || !Enum.IsDefined(typeof(UpgradeKind), rule.Kind) || string.IsNullOrWhiteSpace(rule.Name) ||
                    rule.BaseCost < 1 || rule.MaximumLevel < 1 || rule.MaximumLevel > 100 || rule.UnlockLevel < 0 || rule.UnlockLevel > Experience.MaximumLevel)
                    throw new ArgumentException("Melhoria inválida.");
                RequireRange(rule.CostGrowth, 1, 2); RequireRange(rule.Bonus, 0, 1);
                if (rule.Kind == UpgradeKind.CriticalChance && BaseCriticalChance + rule.Bonus * rule.MaximumLevel > 0.75)
                    throw new ArgumentException("Chance crítica excede 75%.");
                rule.Cost(rule.MaximumLevel - 1);
            }
            foreach (AttackPowerRule rule in Powers)
            {
                if (rule == null || !Enum.IsDefined(typeof(AttackPowerKind), rule.Kind) || string.IsNullOrWhiteSpace(rule.Id) ||
                    string.IsNullOrWhiteSpace(rule.Name) || rule.BaseCost < 1 || rule.MaximumRank < 1 || rule.MaximumRank > 10 ||
                    rule.UnlockLevel < 0 || rule.UnlockLevel > Experience.MaximumLevel)
                    throw new ArgumentException("Poder inválido.");
                RequireRange(rule.CostGrowth, 1, 3); RequireRange(rule.Bonus, 0, 1);
                rule.Cost(rule.MaximumRank - 1);
            }
            int totalWeight = 0;
            foreach (ChestRewardRule reward in ChestRewards)
            {
                if (reward == null || reward.Weight < 1 || reward.GoldMultiplier < 1 || reward.GoldMultiplier > 100 ||
                    reward.Crystals < 0 || string.IsNullOrWhiteSpace(reward.Name)) throw new ArgumentException("Baú inválido.");
                totalWeight = checked(totalWeight + reward.Weight);
            }
            if (totalWeight != 10000) throw new ArgumentException("Probabilidades dos baús devem somar 100%.");
            // A fase final também precisa caber nos tipos usados no combate e na carteira.
            RequireRange(EnemyHealth(MaximumStage) * BossHealthMultiplier, 1, 1e9);
            RequireRange(EnemyDamage(MaximumStage) * BossDamageMultiplier, 0, 1e8);
            if ((long)NormalGold(MaximumStage) * Math.Max(BossGoldMultiplier, Math.Max(GoldPackMultiplier, ChestRewards.Max(x => x.GoldMultiplier))) > int.MaxValue)
                throw new ArgumentException("Recompensa excede o limite da carteira.");
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind)))
                if (Upgrade(kind) == null) throw new ArgumentException("Tipo de melhoria ausente.");
            if (Math.Abs(Upgrade(UpgradeKind.Attack).Bonus - (AttackLevelMultiplier - 1)) > 1e-9 ||
                Math.Abs(Upgrade(UpgradeKind.Health).Bonus - (HealthLevelMultiplier - 1)) > 1e-9)
                throw new ArgumentException("Percentuais de ataque/vida divergem dos multiplicadores.");
        }

        private static void RequireRange(double value, double minimum, double maximum)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < minimum || value > maximum)
                throw new ArgumentException("Percentual ou curva fora do intervalo permitido.");
        }

        public static int Scale(int basis, double growth, int index)
        {
            double value = Math.Ceiling(basis * Math.Pow(growth, Math.Max(0, index)));
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > int.MaxValue)
                throw new OverflowException("Curva econômica excedeu o limite da carteira.");
            return (int)value;
        }

        public int NormalGold(int stage) => Scale(EnemyGoldBase, GoldGrowth, StageIndex(stage));
        public int ExperienceReward(int stage, bool boss, CharacterExperienceRules characterExperience = null)
        {
            CharacterExperienceRules xp = characterExperience ?? Experience;
            int basis = boss ? xp.BossExperience : xp.RegularEnemyExperience;
            return Math.Max(Scale(basis, ExperienceGrowth, StageIndex(stage)), checked(basis + MinimumExperienceStep * StageIndex(stage)));
        }
        public int StageIndex(int stage) => Math.Max(0, Math.Min(MaximumStage, stage) - 1);
        public int DifficultyTier(int stage) => Math.Max(1, Math.Min(MaximumStage, stage)) / DifficultyStepInterval;
        private int EarlyStageIndex(int stage) => Math.Min(StageIndex(stage), EndgameStartStage - 2);
        private int EndgameStageIndex(int stage) => Math.Max(0, StageIndex(stage) - (EndgameStartStage - 2));
        public double EnemyHealth(int stage) => EnemyHealthBase * Math.Pow(EnemyHealthGrowth, EarlyStageIndex(stage)) * Math.Pow(EndgameHealthGrowth, EndgameStageIndex(stage)) * Math.Pow(DifficultyHealthStep, DifficultyTier(stage));
        public double EnemyDamage(int stage) => EnemyDamageBase * Math.Pow(EnemyDamageGrowth, EarlyStageIndex(stage)) * Math.Pow(EndgameDamageGrowth, EndgameStageIndex(stage)) * Math.Pow(DifficultyDamageStep, DifficultyTier(stage));
        public EconomyUpgradeRule Upgrade(UpgradeKind kind) => Upgrades.FirstOrDefault(x => x.Kind == kind);
        public AttackPowerRule Power(string id) => Powers.FirstOrDefault(x => x.Id == id);
    }

    public readonly struct DrillStats
    {
        public readonly float AttackDamage, MaximumHealth, AttackSpeed, SpecialMultiplier, CriticalChance, CriticalMultiplier, RepairPerSecond;
        public DrillStats(float attack, float health, float speed, float special, float chance, float critical, float repair)
        {
            AttackDamage = attack; MaximumHealth = health; AttackSpeed = speed; SpecialMultiplier = special;
            CriticalChance = chance; CriticalMultiplier = critical; RepairPerSecond = repair;
        }
    }

    public readonly struct ChestOpening
    {
        public readonly string Rarity;
        public readonly int Gold, Crystals;
        public ChestOpening(string rarity, int gold, int crystals) { Rarity = rarity; Gold = gold; Crystals = crystals; }
    }

    /// <summary>Transações locais indivisíveis. UI não altera saldo, nível ou sorteio diretamente.</summary>
    public static class EconomyProgression
    {
        public static int Level(GameSaveData save, UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Attack: return save.AttackLevel;
                case UpgradeKind.Health: return save.HealthLevel;
                case UpgradeKind.Special: return save.SpecialLevel;
                case UpgradeKind.AttackSpeed: return save.AttackSpeedLevel;
                case UpgradeKind.CriticalChance: return save.CriticalChanceLevel;
                case UpgradeKind.CriticalDamage: return save.CriticalDamageLevel;
                case UpgradeKind.Repair: return save.RepairLevel;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public static int PowerRank(GameSaveData save, string characterId, string powerId)
            => CharacterProgression.Find(save, characterId)?.Powers?.FirstOrDefault(x => x.PowerId == powerId)?.Rank ?? 0;

        public static int WorkshopLevel(GameSaveData save, CharacterExperienceRules xp)
        {
            int maximum = 0;
            foreach (CharacterProgressData character in save.Characters)
                maximum = Math.Max(maximum, CharacterProgression.GetLevel(character, xp).Level);
            return maximum;
        }

        public static float PowerBonus(GameSaveData save, EconomyRules rules, string characterId, AttackPowerKind kind)
        {
            AttackPowerRule rule = rules.Powers.First(x => x.Kind == kind);
            return (float)(PowerRank(save, characterId, rule.Id) * rule.Bonus);
        }

        public static DrillStats Stats(GameSaveData save, EconomyRules rules, string characterId, CharacterExperienceRules xp,
            float baseDamage = 6f, float baseHealth = 120f, CharacterCombatData definition = null)
        {
            if (definition == null) definition = rules.CharacterRoster.FirstOrDefault(x => x.Id == characterId);
            int level = CharacterProgression.GetLevel(CharacterProgression.Find(save, characterId), xp).Level;
            float attack = (float)(baseDamage * Math.Pow(rules.AttackLevelMultiplier, save.AttackLevel) * (1 + rules.CharacterDamagePerLevel * level) * (definition?.AttackMultiplier ?? 1));
            float health = (float)(baseHealth * Math.Pow(rules.HealthLevelMultiplier, save.HealthLevel) * (1 + rules.CharacterHealthPerLevel * level) * (definition?.HealthMultiplier ?? 1));
            return new DrillStats(attack, health,
                (float)(1 + save.AttackSpeedLevel * rules.Upgrade(UpgradeKind.AttackSpeed).Bonus) * (definition?.SpeedMultiplier ?? 1),
                (float)(1 + save.SpecialLevel * rules.Upgrade(UpgradeKind.Special).Bonus),
                (float)(rules.BaseCriticalChance + save.CriticalChanceLevel * rules.Upgrade(UpgradeKind.CriticalChance).Bonus),
                (float)(rules.BaseCriticalMultiplier + save.CriticalDamageLevel * rules.Upgrade(UpgradeKind.CriticalDamage).Bonus),
                (float)(save.RepairLevel * rules.Upgrade(UpgradeKind.Repair).Bonus));
        }

        public static bool TryBuyUpgrade(GameSaveData save, EconomyRules rules, CharacterExperienceRules xp, UpgradeKind kind)
        {
            EconomyUpgradeRule rule = rules.Upgrade(kind);
            if (rule == null) return false;
            int level = Level(save, kind);
            if (level >= rule.MaximumLevel || WorkshopLevel(save, xp) < rule.UnlockLevel)
                return false;
            int cost = rule.Cost(level);
            if (save.Gold < cost) return false;
            save.Gold -= cost;
            switch (kind)
            {
                case UpgradeKind.Attack: save.AttackLevel++; break;
                case UpgradeKind.Health: save.HealthLevel++; break;
                case UpgradeKind.Special: save.SpecialLevel++; break;
                case UpgradeKind.AttackSpeed: save.AttackSpeedLevel++; break;
                case UpgradeKind.CriticalChance: save.CriticalChanceLevel++; break;
                case UpgradeKind.CriticalDamage: save.CriticalDamageLevel++; break;
                case UpgradeKind.Repair: save.RepairLevel++; break;
            }
            return true;
        }

        public static bool TryBuyPower(GameSaveData save, EconomyRules rules, CharacterExperienceRules xp, string id)
        {
            AttackPowerRule rule = rules.Power(id);
            CharacterProgressData character = CharacterProgression.Find(save, save.EquippedCharacterId);
            if (rule == null || character == null || character.Powers == null) return false;
            int rank = PowerRank(save, character.CharacterId, id);
            if (rank >= rule.MaximumRank || CharacterProgression.GetLevel(character, xp).Level < rule.UnlockLevel) return false;
            int cost = rule.Cost(rank);
            if (save.Gold < cost) return false;
            var progress = character.Powers.FirstOrDefault(x => x.PowerId == id);
            if (progress == null)
            {
                progress = new PowerProgressData { PowerId = id };
                character.Powers.Add(progress);
            }
            save.Gold -= cost;
            progress.Rank++;
            return true;
        }

        public static bool TryBuyKey(GameSaveData save, EconomyRules rules)
        {
            if (save.Crystals < rules.KeyCrystalCost || save.Keys == int.MaxValue) return false;
            save.Crystals -= rules.KeyCrystalCost; save.Keys++; return true;
        }

        public static bool TrySelectTrainingStage(GameSaveData save)
        {
            if (save.HighestClearedStage < 1 || (save.ActiveRun != null && !save.ActiveRun.IsPlayerDefeated && !save.ActiveRun.IsStageCleared)) return false;
            save.CampaignStage = save.HighestClearedStage;
            return true;
        }

        public static bool TryBuyGoldPack(GameSaveData save, EconomyRules rules)
        {
            long reward = (long)rules.NormalGold(Math.Max(1, save.HighestClearedStage)) * rules.GoldPackMultiplier;
            if (save.Crystals < rules.GoldPackCrystalCost || reward > int.MaxValue - save.Gold) return false;
            save.Crystals -= rules.GoldPackCrystalCost; save.Gold += (int)reward; return true;
        }

        public static bool TryOpenChest(GameSaveData save, EconomyRules rules, out ChestOpening opening)
        {
            opening = default;
            if (save.Chests < 1 || save.Keys < 1) return false;
            uint nextState = NextRandom(save.LootRandomState);
            int roll = (int)(nextState % 10000u);
            ChestRewardRule reward = rules.ChestRewards[rules.ChestRewards.Length - 1];
            foreach (ChestRewardRule candidate in rules.ChestRewards)
            {
                if (roll < candidate.Weight) { reward = candidate; break; }
                roll -= candidate.Weight;
            }
            long tracked = 0;
            foreach (ChestStackData stack in save.ChestStacks) tracked += stack.Count;
            // Baús legados sem procedência usam a fase 1. Guardar um baú novo não aumenta seu valor.
            ChestStackData selectedStack = tracked == save.Chests ? save.ChestStacks.FirstOrDefault() : null;
            int rewardStage = selectedStack?.Stage ?? 1;
            long gold = (long)rules.NormalGold(rewardStage) * reward.GoldMultiplier;
            if (gold > int.MaxValue - save.Gold || reward.Crystals > int.MaxValue - save.Crystals) return false;
            save.Chests--; save.Keys--; save.Gold += (int)gold; save.Crystals += reward.Crystals;
            if (selectedStack != null && --selectedStack.Count == 0) save.ChestStacks.Remove(selectedStack);
            save.LootRandomState = nextState;
            opening = new ChestOpening(reward.Name, (int)gold, reward.Crystals);
            return true;
        }

        public static void RecordChestStage(GameSaveData save, int stage, int count)
        {
            if (stage < 1 || count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return;
            ChestStackData stack = save.ChestStacks.FirstOrDefault(x => x.Stage == stage);
            if (stack == null)
            {
                stack = new ChestStackData { Stage = stage };
                save.ChestStacks.Add(stack);
            }
            stack.Count = checked(stack.Count + count);
        }

        public static uint NextRandom(uint state)
        {
            uint value = state == 0 ? 0xA341316Cu : state;
            value ^= value << 13; value ^= value >> 17; value ^= value << 5;
            return value;
        }

        public static int Credit(int current, long amount)
        {
            if (current < 0 || amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            return (int)Math.Min(int.MaxValue, current + Math.Min(amount, int.MaxValue));
        }
    }
}
