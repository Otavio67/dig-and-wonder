using System;
using System.Collections.Generic;

namespace DigAndWonder
{
    // Pixel coordinates use Unity's bottom-left origin. Baked in the Editor;
    // no texture readbacks, flood fills or frame-bound searches on Android.
    public sealed class EnemySheetPresentation
    {
        public int Revision = 1;
        public string SourceHash;
        public float ReferenceWidth;
        public float ReferenceHeight;
        public EnemyFrameBounds[] Frames;
    }

    public sealed class EnemyFrameBounds
    {
        public int Row, Column, X, Y, Width, Height;
    }

    /// <summary>Coverage of the sheet's border-connected white matte, preserving enclosed eyes/highlights.</summary>
    public static class SpriteMatteCoverage
    {
        public static byte[] FromRgb(byte[] rgb, int width, int height)
        {
            if (width <= 0 || height <= 0 || rgb == null || rgb.Length != checked(width * height * 3))
                throw new ArgumentException("Dimensões RGB inválidas.");
            int count = width * height;
            var outside = new bool[count];
            var queue = new int[count];
            int head = 0, tail = 0;
            void Visit(int p)
            {
                if (outside[p]) return;
                int i = p * 3;
                int low = Math.Min(rgb[i], Math.Min(rgb[i + 1], rgb[i + 2]));
                int high = Math.Max(rgb[i], Math.Max(rgb[i + 1], rgb[i + 2]));
                if (low < 180 || high - low > 30) return;
                outside[p] = true;
                queue[tail++] = p;
            }
            for (int x = 0; x < width; x++) { Visit(x); Visit((height - 1) * width + x); }
            for (int y = 0; y < height; y++) { Visit(y * width); Visit(y * width + width - 1); }
            while (head < tail)
            {
                int p = queue[head++], x = p % width, y = p / width;
                if (x > 0) Visit(p - 1);
                if (x < width - 1) Visit(p + 1);
                if (y > 0) Visit(p - width);
                if (y < height - 1) Visit(p + width);
            }
            var alpha = new byte[count];
            for (int p = 0; p < count; p++)
            {
                if (outside[p]) continue;
                int i = p * 3;
                int low = Math.Min(rgb[i], Math.Min(rgb[i + 1], rgb[i + 2]));
                bool edge = p % width > 0 && outside[p - 1] || p % width < width - 1 && outside[p + 1]
                    || p >= width && outside[p - width] || p < count - width && outside[p + width];
                alpha[p] = edge ? (byte)Math.Min(255, (255 - low) * 255 / 100) : (byte)255;
            }
            return alpha;
        }

        // Discard isolated printed shadows and neighbouring debris in each source
        // cell. The connected creature keeps its enclosed light-coloured areas.
        public static EnemyFrameBounds KeepCreature(byte[] alpha, int width, int height,
            int left, int bottom, int cellWidth, int cellHeight, int row, int column)
        {
            var seen = new HashSet<int>();
            var queue = new Queue<int>();
            var largest = new List<int>();
            for (int y = bottom; y < bottom + cellHeight; y++)
            for (int x = left; x < left + cellWidth; x++)
            {
                int start = y * width + x;
                if (alpha[start] < 16 || !seen.Add(start)) continue;
                var island = new List<int>();
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int p = queue.Dequeue(), px = p % width, py = p / width;
                    island.Add(p);
                    void Visit(int next)
                    {
                        if (alpha[next] >= 16 && seen.Add(next)) queue.Enqueue(next);
                    }
                    if (px > left) Visit(p - 1);
                    if (px < left + cellWidth - 1) Visit(p + 1);
                    if (py > bottom) Visit(p - width);
                    if (py < bottom + cellHeight - 1) Visit(p + width);
                }
                if (island.Count > largest.Count) largest = island;
            }
            var keep = new HashSet<int>(largest);
            int x0 = left + cellWidth - 1, x1 = left, y0 = bottom + cellHeight - 1, y1 = bottom;
            for (int y = bottom; y < bottom + cellHeight; y++)
            for (int x = left; x < left + cellWidth; x++)
            {
                int p = y * width + x;
                if (!keep.Contains(p)) { alpha[p] = 0; continue; }
                x0 = Math.Min(x0, x); x1 = Math.Max(x1, x);
                y0 = Math.Min(y0, y); y1 = Math.Max(y1, y);
            }
            if (largest.Count == 0) throw new InvalidOperationException("Célula de criatura vazia.");
            x0 = Math.Max(left, x0 - 2); y0 = Math.Max(bottom, y0 - 2);
            x1 = Math.Min(left + cellWidth - 1, x1 + 2); y1 = Math.Min(bottom + cellHeight - 1, y1 + 2);
            return new EnemyFrameBounds { Row = row, Column = column, X = x0, Y = y0, Width = x1 - x0 + 1, Height = y1 - y0 + 1 };
        }
    }
}
