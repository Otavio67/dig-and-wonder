using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DigAndWonder
{
    public sealed class AtomicSaveRepository
    {
        private const int CurrentSchemaVersion = 1;
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

        private static void Validate(GameSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.SchemaVersion != CurrentSchemaVersion) throw new InvalidDataException("Versão de save incompatível.");
            if (data.Gold < 0 || data.Keys < 0 || data.Chests < 0 || data.AttackLevel < 0 || data.HealthLevel < 0 ||
                data.CriticalChanceLevel < 0 || data.CriticalDamageLevel < 0)
                throw new InvalidDataException("O save contém valores negativos.");
            if (string.IsNullOrWhiteSpace(data.UnlockedStageId)) throw new InvalidDataException("O perfil não tem fase desbloqueada válida.");
            if (data.ActiveRun == null) return;
            ActiveRunSnapshot run = data.ActiveRun;
            if (string.IsNullOrWhiteSpace(run.StageId)) throw new InvalidDataException("A tentativa salva não tem uma fase válida.");
            if (run.IsPlayerDefeated && run.IsStageCleared) throw new InvalidDataException("A tentativa tem resultados incompatíveis.");
            RequireFinite(run.ElapsedSeconds, run.PlayerHealth, run.AttackTimer, run.AttackHitTimer,
                run.SpecialCooldown, run.SpecialTravelTimer, run.SpecialHitTimer, run.NextWaveTimer);
            if (run.ElapsedSeconds < 0f || run.PlayerHealth < 0f || run.AttackHitsRemaining < 0 || run.SpecialHitsRemaining < 0 ||
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
                    if (enemy.Id < 1 || enemy.Id >= run.NextEntityId || !ids.Add(enemy.Id) || enemy.Health <= 0f || enemy.GoldReward < 0)
                        throw new InvalidDataException("Inimigo inválido no snapshot.");
                }
            }
            if (run.PendingHits != null)
            {
                foreach (PendingHitSnapshot hit in run.PendingHits)
                {
                    if (hit == null) throw new InvalidDataException("Projétil ausente no snapshot.");
                    RequireFinite(hit.Damage, hit.RemainingTravel);
                    if (hit.Damage < 0f || hit.SourceEntityId < 0 || hit.TargetEntityId < 0)
                        throw new InvalidDataException("Projétil inválido no snapshot.");
                }
            }
        }

        private static void RequireFinite(params float[] values)
        {
            foreach (float value in values)
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidDataException("O save contém números não finitos.");
        }

        private static bool TryRead(string path, out GameSaveData data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                var envelope = JsonConvert.DeserializeObject<SaveEnvelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || envelope.SchemaVersion != CurrentSchemaVersion || string.IsNullOrEmpty(envelope.Payload)) return false;
                if (!string.Equals(envelope.Sha256, ComputeHash(envelope.Payload), StringComparison.OrdinalIgnoreCase)) return false;
                data = JsonConvert.DeserializeObject<GameSaveData>(envelope.Payload);
                if (data == null) return false;
                Validate(data);
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
