using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Placas enemigas dentro del escenario, con seis puestos que nunca se superponen.</summary>
    public static class BattleHudLayout
    {
        // TUNING: reserva el lado izquierdo al aquelarre y la columna derecha a las órdenes.
        public const float PlateWidth = 252, PlateHeight = 154;

        public static Rect EnemyPlate(Vector2 head, IReadOnlyList<Rect> occupied)
        {
            var ideal = new Rect(Mathf.Clamp(head.x - PlateWidth / 2, 1032, 1310), Mathf.Clamp(head.y - PlateHeight, 238, 590), PlateWidth, PlateHeight);
            if (occupied.All(r => !Expanded(r).Overlaps(ideal))) return ideal;
            Rect best = default;
            float distance = float.MaxValue;
            foreach (float y in new[] { 238f, 410f, 582f })
                foreach (float x in new[] { 1032f, 1310f })
                {
                    var candidate = new Rect(x, y, PlateWidth, PlateHeight);
                    if (occupied.Any(r => Expanded(r).Overlaps(candidate))) continue;
                    float d = (candidate.center - ideal.center).sqrMagnitude;
                    if (d >= distance) continue;
                    distance = d;
                    best = candidate;
                }
            // Cuando el primer puesto libre es continuo, puede bloquear dos filas discretas. Recoloca esa
            // primera placa en su celda más próxima antes de insertar las siguientes (ResolveEnemyPlates).
            return distance < float.MaxValue ? best : ideal;
        }

        private static Rect Expanded(Rect r) => new Rect(r.x - 5, r.y - 5, r.width + 10, r.height + 10);

        public static List<Rect> ResolveEnemyPlates(IReadOnlyList<Vector2> heads)
        {
            var result = new List<Rect>();
            if (heads.Count <= 2)
            {
                foreach (var head in heads) result.Add(EnemyPlate(head, result));
                return result;
            }
            var free = new List<Rect>();
            foreach (float y in new[] { 238f, 410f, 582f })
                foreach (float x in new[] { 1032f, 1310f }) free.Add(new Rect(x, y, PlateWidth, PlateHeight));
            foreach (var head in heads.Take(6))
            {
                var chosen = free.OrderBy(r => (r.center - head).sqrMagnitude).First();
                result.Add(chosen);
                free.Remove(chosen);
            }
            return result;
        }
    }
}
