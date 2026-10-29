using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace DigAndWonder.Editor
{
    /// <summary>Bakes coverage and frame bounds; preserves the supplied JPEG artwork.</summary>
    public static class EnemyPresentationBaker
    {
        private const string Root = "Assets/_Game/Resources/Art/Enemies";
        private const string Output = Root + "/Presentation";

        [MenuItem("Dig & Wonder/Bake enemy presentation")]
        public static void Bake()
        {
            Directory.CreateDirectory(Output);
            foreach (string path in Directory.GetFiles(Root, "*.jpeg")) BakeSheet(path);
            AssetDatabase.Refresh();
            foreach (string path in Directory.GetFiles(Output, "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                if (importer == null) continue;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }

        private static void BakeSheet(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            string jsonPath = Output + "/" + id + ".json";
            byte[] source = File.ReadAllBytes(path);
            string hash;
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(source)).Replace("-", "");
            if (File.Exists(jsonPath) && File.Exists(Output + "/" + id + ".png"))
            {
                var saved = JsonConvert.DeserializeObject<EnemySheetPresentation>(File.ReadAllText(jsonPath));
                if (saved.Revision == 1 && saved.SourceHash == hash) return;
            }
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var mask = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, source)) throw new InvalidOperationException("Folha inválida: " + path);
                Color32[] pixels = texture.GetPixels32();
                var rgb = new byte[pixels.Length * 3];
                for (int p = 0; p < pixels.Length; p++)
                { rgb[p * 3] = pixels[p].r; rgb[p * 3 + 1] = pixels[p].g; rgb[p * 3 + 2] = pixels[p].b; }
                byte[] alpha = SpriteMatteCoverage.FromRgb(rgb, texture.width, texture.height);
                int rows = id == "goblin" ? 4 : 3;
                var frames = new List<EnemyFrameBounds>();
                for (int row = 0; row < rows; row++)
                {
                    int columns = id == "toupeira" && row < 2 ? 5 : id == "aranha-pedra" && row < 2 || id == "lesma" && row == 2 ? 6 : 4;
                    // Goblin's last row contains two poses followed by two empty cells.
                    int count = id == "goblin" && row == 3 ? 2 : columns;
                    for (int col = 0; col < count; col++)
                    {
                        int x = col * texture.width / columns, right = (col + 1) * texture.width / columns;
                        int y = texture.height - (row + 1) * texture.height / rows;
                        int top = texture.height - row * texture.height / rows;
                        frames.Add(SpriteMatteCoverage.KeepCreature(alpha, texture.width, texture.height, x, y, right - x, top - y, row, col));
                    }
                }
                // Empty cells must stay transparent, too.
                if (id == "goblin")
                    for (int y = 0; y < texture.height / 4; y++)
                    for (int x = texture.width / 2; x < texture.width; x++) alpha[y * texture.width + x] = 0;
                var data = new EnemySheetPresentation { SourceHash = hash, Frames = frames.ToArray(),
                    ReferenceWidth = frames.Where(f => f.Row == 0).Max(f => f.Width),
                    ReferenceHeight = frames.Where(f => f.Row == 0).Max(f => f.Height) };
                mask.Reinitialize(texture.width, texture.height);
                for (int p = 0; p < pixels.Length; p++) pixels[p] = new Color32(alpha[p], alpha[p], alpha[p], 255);
                mask.SetPixels32(pixels); mask.Apply();
                File.WriteAllBytes(Output + "/" + id + ".png", mask.EncodeToPNG());
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(mask); }
        }
    }
}
