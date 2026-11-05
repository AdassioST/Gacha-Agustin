using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Placas enemigas compactas sobre las cabezas, dentro de la mitad derecha del escenario y sin encimarse.
    /// Con el HUD de esquinas ya no hay columna de órdenes: las placas pueden llegar hasta el borde derecho.
    /// Los jefes y los gigantes no llevan placa sobre la cabeza (sobre un coloso no cabe): su vida va en una barra arriba.
    /// </summary>
    public static class BattleHudLayout
    {
        // TUNING: placa de 220×78 (nombre, vida, Firmeza, debilidades e intención) y zona libre del escenario.
        public const float PlateWidth = 220, PlateHeight = 78;
        public const float MinX = 980, MaxX = 1900, MinY = 112, MaxY = 640;
        private static readonly float[] Cols = { 990, 1222, 1454, 1680 };
        private static readonly float[] Rows = { 118, 206, 294, 382 };

        // TUNING: barra de jefe bajo la Resonancia, entre la línea de turnos (x < 520) y los controles (x > 1450).
        // Termina antes de BattleStage.HeadLimit (186), así que ninguna cabeza de gigante queda tapada.
        public const float BossBarY = 120, BossBarHeight = 58, BossBarLeft = 560, BossBarRight = 1360, BossBarGap = 12;
        /// <summary>Caben dos barras lado a lado; un tercer jefe o gigante usa una placa normal.</summary>
        public const int MaxBossBars = 2;

        /// <summary>Barras de jefe para <paramref name="count"/> jefes o gigantes, repartidas en la franja superior.</summary>
        public static List<Rect> BossBars(int count)
        {
            var result = new List<Rect>();
            count = Mathf.Clamp(count, 0, MaxBossBars);
            if (count == 0) return result;
            float w = (BossBarRight - BossBarLeft - BossBarGap * (count - 1)) / count;
            for (int i = 0; i < count; i++) result.Add(new Rect(BossBarLeft + i * (w + BossBarGap), BossBarY, w, BossBarHeight));
            return result;
        }

        /// <summary>Lo más alto que puede ir una placa: con barra de jefe, debajo de ella.</summary>
        public static float PlatesTop(bool bossBars) => bossBars ? BossBarY + BossBarHeight + 8 : MinY;

        public static Rect EnemyPlate(Vector2 head, IReadOnlyList<Rect> occupied, float top = MinY)
        {
            var ideal = new Rect(Mathf.Clamp(head.x - PlateWidth / 2, MinX, MaxX - PlateWidth), Mathf.Clamp(head.y - PlateHeight - 6, top, MaxY - PlateHeight), PlateWidth, PlateHeight);
            if (occupied.All(r => !Expanded(r).Overlaps(ideal))) return ideal;
            Rect best = default;
            float distance = float.MaxValue;
            foreach (var candidate in Grid(top))
            {
                if (occupied.Any(r => Expanded(r).Overlaps(candidate))) continue;
                float d = (candidate.center - ideal.center).sqrMagnitude;
                if (d >= distance) continue;
                distance = d;
                best = candidate;
            }
            return distance < float.MaxValue ? best : ideal;
        }

        private static IEnumerable<Rect> Grid(float top)
        {
            // La rejilla entera baja si la barra de jefe ocupa la franja de arriba.
            float shift = Mathf.Max(0f, top - MinY);
            foreach (float y in Rows)
                foreach (float x in Cols)
                    yield return new Rect(x, y + shift, PlateWidth, PlateHeight);
        }

        private static Rect Expanded(Rect r) => new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);

        public static List<Rect> ResolveEnemyPlates(IReadOnlyList<Vector2> heads, float top = MinY)
        {
            var result = new List<Rect>();
            if (heads.Count <= 2)
            {
                foreach (var head in heads) result.Add(EnemyPlate(head, result, top));
                return result;
            }
            // Con tres o más, cada placa toma la celda libre más cercana a su cabeza: nunca se enciman.
            var free = Grid(top).ToList();
            foreach (var head in heads.Take(free.Count))
            {
                var target = new Vector2(Mathf.Clamp(head.x, MinX, MaxX), Mathf.Clamp(head.y - PlateHeight, top, MaxY));
                var chosen = free.OrderBy(r => (r.center - target).sqrMagnitude).First();
                result.Add(chosen);
                free.Remove(chosen);
            }
            return result;
        }
    }
}
