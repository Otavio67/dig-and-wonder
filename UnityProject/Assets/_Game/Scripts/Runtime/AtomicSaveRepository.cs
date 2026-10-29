using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigAndWonder
{
    public sealed class AtomicSaveRepository
    {
        private const int CurrentSchemaVersion = GameSaveData.CurrentSchemaVersion;
        private readonly string primaryPath;
        private readonly string backupPath;
        private readonly string temporaryPath;

        public AtomicSaveRepository(string directoryPath, string fileName = "dig-and-wonder.save.json")
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("Diretório do save inválido.", nameof(directoryPath));
            Directory.CreateDirectory(directoryPath);
            primaryPath = Path.Combine(directoryPath, fileName);
            backupPath = primaryPath + ".bak";
            temporaryPath = primaryPath + ".tmp";
        }

        public bool HasAnySaveFile => File.Exists(primaryPath) || File.Exists(backupPath) || File.Exists(temporaryPath);

        public bool TryLoad(out GameSaveData data, out bool recoveredBackup)
        {
            recoveredBackup = false;
            // Um app antigo não deve substituir um save íntegro de uma versão futura pelo seu backup antigo.
            if (HasVerifiedFutureVersion(primaryPath)) { data = null; return false; }
            if (TryRead(primaryPath, out data)) return true;
            string recoveryPath = backupPath;
            if (!TryRead(backupPath, out data))
            {
                // O temporário só é usado quando não há uma geração já confirmada válida.
                recoveryPath = temporaryPath;
                if (!TryRead(temporaryPath, out data))
                {
                    data = null;
                    return false;
                }
            }

            recoveredBackup = recoveryPath == backupPath;
            try
            {
                if (recoveryPath != temporaryPath) File.Copy(recoveryPath, temporaryPath, true);
                ReplacePrimary(temporaryPath, primaryPath, null);
            }
            catch (IOException)
            {
                // O backup já foi validado e permanece disponível mesmo se a restauração falhar.
            }
            catch (UnauthorizedAccessException)
            {
                // O perfil recuperado continua utilizável em memória nesta sessão.
            }
            return true;
        }

        public void Save(GameSaveData data)
        {
            Validate(data);
            if (HasVerifiedFutureVersion(primaryPath))
                throw new InvalidDataException("O save pertence a uma versão futura e foi preservado.");
            string payload = JsonConvert.SerializeObject(data, Formatting.None);
            var envelope = new SaveEnvelope
            {
                SchemaVersion = CurrentSchemaVersion,
                Payload = payload,
                Sha256 = ComputeHash(payload)
            };

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(envelope, Formatting.None));
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (!TryRead(temporaryPath, out _)) throw new InvalidDataException("O temporário não passou pela validação após a escrita.");
            bool keepCurrentAsBackup = TryRead(primaryPath, out _);
            ReplacePrimary(temporaryPath, primaryPath, keepCurrentAsBackup ? backupPath : null);
        }

        private static void Validate(GameSaveData data, bool allowLegacy = false)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            bool legacy = allowLegacy && (data.SchemaVersion >= 1 && data.SchemaVersion < CurrentSchemaVersion);
            if (data.SchemaVersion != CurrentSchemaVersion && !legacy) throw new InvalidDataException("Versão de save incompatível.");
            if (data.Gold < 0 || data.Keys < 0 || data.Chests < 0 || data.AttackLevel < 0 || data.HealthLevel < 0 ||
                data.CriticalChanceLevel < 0 || data.CriticalDamageLevel < 0 || data.SpecialLevel < 0 ||
                data.AttackSpeedLevel < 0 || data.RepairLevel < 0 || data.Crystals < 0 ||
                data.CampaignStage < 1 || data.HighestClearedStage < 0)
                throw new InvalidDataException("O save contém valores negativos.");
            if (string.IsNullOrWhiteSpace(data.UnlockedStageId)) throw new InvalidDataException("O perfil não tem fase desbloqueada válida.");
            if (data.SchemaVersion >= 3)
            {
                if (data.ChestStacks == null) throw new InvalidDataException("Procedência dos baús ausente.");
                var stages = new HashSet<int>(); long tracked = 0;
                foreach (ChestStackData stack in data.ChestStacks)
                {
                    if (stack == null || stack.Stage < 1 || stack.Count < 1 || !stages.Add(stack.Stage))
                        throw new InvalidDataException("Procedência dos baús inválida.");
                    tracked += stack.Count;
                }
                if (tracked > data.Chests) throw new InvalidDataException("Inventário de baús não confere.");
            }
            if (data.SchemaVersion >= 2)
            {
                if (data.Characters == null || data.Characters.Count == 0 || string.IsNullOrWhiteSpace(data.EquippedCharacterId))
                    throw new InvalidDataException("O perfil não contém personagens válidas.");
                var characterIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (CharacterProgressData character in data.Characters)
                {
                    if (character == null || string.IsNullOrWhiteSpace(character.CharacterId) || character.TotalExperience < 0 ||
                        !characterIds.Add(character.CharacterId))
                        throw new InvalidDataException("O perfil contém XP inválido ou personagens duplicadas.");
                    if (data.SchemaVersion >= 3)
                    {
                        if (character.Powers == null) throw new InvalidDataException("Poderes ausentes.");
                        var powerIds = new HashSet<string>(StringComparer.Ordinal);
                        foreach (PowerProgressData power in character.Powers)
                            if (power == null || string.IsNullOrWhiteSpace(power.PowerId) || power.Rank < 1 || !powerIds.Add(power.PowerId))
                                throw new InvalidDataException("Poderes inválidos ou duplicados.");
                    }
                }
                if (!characterIds.Contains(data.EquippedCharacterId))
                    throw new InvalidDataException("A personagem equipada não pertence ao perfil.");
                if (data.ActiveRun != null && data.ActiveRun.CharacterId != data.EquippedCharacterId)
                    throw new InvalidDataException("A personagem da tentativa diverge da personagem equipada.");
            }
            if (data.SchemaVersion >= 4)
            {
                if (data.DiscoveryStages == null) throw new InvalidDataException("Coleção ausente.");
                var discoveries = new HashSet<int>();
                foreach (int stage in data.DiscoveryStages)
                    if (DiscoveryCatalog.Find(stage) == null || stage > data.HighestClearedStage || !discoveries.Add(stage))
                        throw new InvalidDataException("Descoberta inválida ou duplicada.");
            }
            if (data.ActiveRun == null) return;
            ActiveRunSnapshot run = data.ActiveRun;
            if (!run.IsStageCleared && !run.IsPlayerDefeated && run.CampaignStage != data.CampaignStage)
                throw new InvalidDataException("A fase da tentativa diverge da campanha salva.");
            if (string.IsNullOrWhiteSpace(run.StageId)) throw new InvalidDataException("A tentativa salva não tem uma fase válida.");
            if (run.IsPlayerDefeated && run.IsStageCleared) throw new InvalidDataException("A tentativa tem resultados incompatíveis.");
            RequireFinite(run.ElapsedSeconds, run.PlayerHealth, run.AttackTimer, run.AttackHitTimer,
                run.SpecialCooldown, run.SpecialTravelTimer, run.SpecialHitTimer, run.NextWaveTimer);
            if (run.EncounterVersion < 0 || run.EncounterVersion > 2 || run.CampaignStage < 1 || run.ElapsedSeconds < 0f || run.PlayerHealth < 0f || run.AttackHitsRemaining < 0 || run.SpecialHitsRemaining < 0 ||
                run.WavesSpawned < 0 || run.PendingSpawns < 0 || run.NextEntityId < 1 || run.AttackTargetId < 0 || run.SpecialTargetId < 0 ||
                (!run.IsPlayerDefeated && !run.IsStageCleared && run.PlayerHealth <= 0f))
                throw new InvalidDataException("A tentativa tem contadores ou vida inválidos.");
            var ids = new HashSet<int>();
            if (run.Enemies != null)
            {
                foreach (EnemySnapshot enemy in run.Enemies)
                {
                    if (enemy == null) throw new InvalidDataException("Inimigo ausente no snapshot.");
                    RequireFinite(enemy.Health, enemy.AttackTimer);
                    if (enemy.Id < 1 || enemy.Id >= run.NextEntityId || !ids.Add(enemy.Id) || enemy.Health <= 0f || enemy.GoldReward < 0 || (int)enemy.Archetype < 0 || (int)enemy.Archetype > 3 || enemy.IsBoss && enemy.Archetype != EnemyArchetype.Regular)
                        throw new InvalidDataException("Inimigo inválido no snapshot.");
                }
            }
            if (run.PendingHits != null)
            {
                foreach (PendingHitSnapshot hit in run.PendingHits)
                {
                    if (hit == null) throw new InvalidDataException("Projétil ausente no snapshot.");
                    RequireFinite(hit.Damage, hit.RemainingTravel, hit.TelegraphRemaining);
                    if (hit.TelegraphRemaining < 0f || hit.TelegraphRemaining > System.Math.Max(0f, hit.RemainingTravel) + .001f || hit.Damage < 0f || hit.SourceEntityId < 0 || hit.TargetEntityId < 0)
                        throw new InvalidDataException("Projétil inválido no snapshot.");
                }
            }
        }

        private static void RequireFinite(params float[] values)
        {
            foreach (float value in values)
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidDataException("O save contém números não finitos.");
        }

        private static bool HasVerifiedFutureVersion(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                SaveEnvelope envelope = JsonConvert.DeserializeObject<SaveEnvelope>(File.ReadAllText(path, Encoding.UTF8));
                return envelope != null && envelope.SchemaVersion > CurrentSchemaVersion && !string.IsNullOrEmpty(envelope.Payload) &&
                    string.Equals(envelope.Sha256, ComputeHash(envelope.Payload), StringComparison.OrdinalIgnoreCase);
            }
            catch (JsonException) { return false; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        private static bool TryRead(string path, out GameSaveData data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || envelope.SchemaVersion < 1 || envelope.SchemaVersion > CurrentSchemaVersion || string.IsNullOrEmpty(envelope.Payload)) return false;
                if (!string.Equals(envelope.Sha256, ComputeHash(envelope.Payload), StringComparison.OrdinalIgnoreCase)) return false;
                JObject payload = JObject.Parse(envelope.Payload);
                if (payload["SchemaVersion"]?.Type != JTokenType.Integer ||
                    payload["SchemaVersion"].ToString() != envelope.SchemaVersion.ToString()) return false;
                // No esquema novo, campos ausentes não podem reiniciar XP silenciosamente pelos defaults do DTO.
                if (envelope.SchemaVersion >= 2)
                {
                    if (payload["Characters"] == null || payload["EquippedCharacterId"] == null) return false;
                    if (payload["Characters"] is JArray characters)
                        foreach (JToken character in characters)
                            if (!(character is JObject item) || item["CharacterId"] == null || item["TotalExperience"] == null) return false;
                    if (payload["ActiveRun"] is JObject activeRun && activeRun["CharacterId"] == null) return false;
                }
                if (envelope.SchemaVersion >= 3)
                {
                    string[] required = { "Crystals", "CampaignStage", "HighestClearedStage", "LootRandomState", "SpecialLevel", "AttackSpeedLevel", "RepairLevel" };
                    foreach (string field in required)
                        if (payload[field]?.Type != JTokenType.Integer) return false;
                    if (!(payload["Characters"] is JArray savedCharacters)) return false;
                    if (!(payload["ChestStacks"] is JArray)) return false;
                    foreach (JToken character in savedCharacters)
                        if (!(character["Powers"] is JArray)) return false;
                    if (payload["ActiveRun"] is JObject active && active["CampaignStage"]?.Type != JTokenType.Integer) return false;
                    if (payload["ActiveRun"] is JObject snapshot && snapshot["PendingHits"] is JArray hits)
                        foreach (JToken hit in hits)
                            if (!(hit is JObject projectile) || projectile["IsBasic"]?.Type != JTokenType.Boolean ||
                                projectile["IsCritical"]?.Type != JTokenType.Boolean) return false;
                }
                if (envelope.SchemaVersion >= 4)
                {
                    if (!(payload["DiscoveryStages"] is JArray)) return false;
                    if (payload["ActiveRun"] is JObject newRun)
                    {
                        if (newRun["IsTraining"]?.Type != JTokenType.Boolean || newRun["EncounterVersion"]?.Type != JTokenType.Integer) return false;
                        if (newRun["Enemies"] is JArray newEnemies)
                            foreach (JToken enemy in newEnemies)
                                if (!(enemy is JObject savedEnemy) || savedEnemy["Archetype"]?.Type != JTokenType.Integer) return false;
                        if (newRun["PendingHits"] is JArray newHits)
                            foreach (JToken hit in newHits)
                                if (!(hit is JObject savedHit) || savedHit["TelegraphRemaining"] == null) return false;
                    }
                }
                data = JsonConvert.DeserializeObject<GameSaveData>(envelope.Payload);
                if (data == null) return false;
                Validate(data, allowLegacy: true);
                if (data.SchemaVersion < CurrentSchemaVersion)
                {
                    // Migração apenas em memória. Os bytes originais viram backup na primeira gravação confirmada.
                    int originalVersion = data.SchemaVersion;
                    data.SchemaVersion = CurrentSchemaVersion;
                    if (originalVersion == 1)
                    {
                        data.EquippedCharacterId = CharacterProgression.StarterCharacterId;
                        data.Characters = new List<CharacterProgressData>
                        {
                            new CharacterProgressData { CharacterId = CharacterProgression.StarterCharacterId }
                        };
                        if (data.ActiveRun != null) data.ActiveRun.CharacterId = CharacterProgression.StarterCharacterId;
                    }
                    if (originalVersion < 3)
                    {
                        data.CampaignStage = 1;
                        data.HighestClearedStage = 0;
                        data.Crystals = data.SpecialLevel = data.AttackSpeedLevel = data.RepairLevel = 0;
                        data.LootRandomState = 0xA341316Cu;
                        data.ChestStacks = new List<ChestStackData>();
                        foreach (CharacterProgressData character in data.Characters) character.Powers = new List<PowerProgressData>();
                        if (data.ActiveRun != null) data.ActiveRun.CampaignStage = 1;
                    }
                    data.DiscoveryStages = new List<int>();
                    Validate(data);
                }
                return data != null;
            }
            catch (JsonException) { return false; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (InvalidDataException) { return false; }
        }

        private static void ReplacePrimary(string sourcePath, string destinationPath, string backupDestination)
        {
            if (!File.Exists(destinationPath))
            {
                File.Move(sourcePath, destinationPath);
                return;
            }

            if (backupDestination != null)
            {
                try
                {
                    File.Replace(sourcePath, destinationPath, backupDestination, true);
                    return;
                }
                catch (PlatformNotSupportedException) { }
                catch (IOException) { }
            }

            if (backupDestination != null) File.Copy(destinationPath, backupDestination, true);
            File.Delete(destinationPath);
            File.Move(sourcePath, destinationPath);
        }

        private static string ComputeHash(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte item in hash) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }

        [Serializable]
        private sealed class SaveEnvelope
        {
            public int SchemaVersion;
            public string Payload;
            public string Sha256;
        }
    }
}
