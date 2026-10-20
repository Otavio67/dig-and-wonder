using System;
using System.IO;
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

        public bool HasAnySaveFile => File.Exists(primaryPath) || File.Exists(backupPath);

        public bool TryLoad(out GameSaveData data, out bool recoveredBackup)
        {
            recoveredBackup = false;
            if (TryRead(primaryPath, out data)) return true;
            if (!TryRead(backupPath, out data))
            {
                data = null;
                return false;
            }

            recoveredBackup = true;
            try
            {
                File.Copy(backupPath, temporaryPath, true);
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
                stream.Flush();
            }

            bool keepCurrentAsBackup = TryRead(primaryPath, out _);
            ReplacePrimary(temporaryPath, primaryPath, keepCurrentAsBackup ? backupPath : null);
        }

        private static void Validate(GameSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.SchemaVersion != CurrentSchemaVersion) throw new InvalidDataException("Versão de save incompatível.");
            if (data.Gold < 0 || data.Keys < 0 || data.Chests < 0 || data.AttackLevel < 0 || data.HealthLevel < 0)
                throw new InvalidDataException("O save contém valores negativos.");
            if (data.ActiveRun != null && string.IsNullOrWhiteSpace(data.ActiveRun.StageId))
                throw new InvalidDataException("A tentativa salva não tem uma fase válida.");
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
