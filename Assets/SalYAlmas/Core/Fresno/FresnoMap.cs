using System;
using System.Collections.Generic;
using System.Text;
using SalYAlmas.Core.Combat;

namespace SalYAlmas.Core.Fresno
{
    public sealed class MapNode
    {
        public int Zone;
        public int Floor;
        public int Lane;
        public NodeType Type;
        /// <summary>La Mancha de Sal de la run anterior está en este nodo.</summary>
        public bool HasMancha;
        /// <summary>Carriles del piso siguiente a los que lleva este nodo.</summary>
        public readonly List<int> Next = new List<int>();
        public string EventId = "";

        public string Id => $"{Zone + 1}-{Floor + 1}-{Lane + 1}";
    }

    /// <summary>Mapa de una run: cuatro zonas de diez pisos con rutas que se bifurcan (GDD 10).</summary>
    public sealed class FresnoMap
    {
        public ulong Seed;
        /// <summary>[zona][piso] → nodos del piso (uno o tres).</summary>
        public readonly List<List<List<MapNode>>> Zones = new List<List<List<MapNode>>>();

        public List<MapNode> Floor(int zone, int floor) => Zones[zone][floor];

        public MapNode Node(int zone, int floor, int lane)
        {
            foreach (var n in Zones[zone][floor]) if (n.Lane == lane) return n;
            return null;
        }

        public IEnumerable<MapNode> AllNodes()
        {
            foreach (var z in Zones) foreach (var f in z) foreach (var n in f) yield return n;
        }

        /// <summary>Huella del mapa (tipos y conexiones) para comparar generaciones.</summary>
        public string Fingerprint()
        {
            var sb = new StringBuilder();
            foreach (var n in AllNodes())
            {
                sb.Append(n.Id).Append(':').Append(n.Type.ToString()[0]).Append(n.Type.ToString()[1]);
                if (n.HasMancha) sb.Append('*');
                sb.Append('>');
                foreach (var x in n.Next) sb.Append(x);
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Genera el mapa con semilla. Reglas: el piso 1 de cada zona es un Combate de entrada y el 10 es el Jefe, ambos en el
    /// carril del centro; los pisos 2 a 9 tienen tres carriles, cada nodo lleva a uno o dos del piso siguiente y todo nodo
    /// es alcanzable. El piso 9 es todo Santuario y el 5 tiene al menos uno; las Élites aparecen desde el piso 3.
    /// La misma semilla y la misma Mancha dan siempre el mismo mapa.
    /// </summary>
    public static class MapGenerator
    {
        public static FresnoMap Generate(FresnoRules rules, ulong seed, Save.ManchaState mancha = null)
        {
            var map = new FresnoMap { Seed = seed };
            var mr = rules.Map;
            int center = mr.Lanes / 2;
            for (int z = 0; z < rules.Zones.Count; z++)
            {
                var rng = new SeededRandom(Mix(seed, (ulong)z + 1));
                var floors = new List<List<MapNode>>();
                for (int f = 0; f < mr.Floors; f++)
                {
                    var nodes = new List<MapNode>();
                    bool single = f == 0 || f == mr.Floors - 1;
                    int count = single ? 1 : mr.Lanes;
                    for (int i = 0; i < count; i++)
                    {
                        var node = new MapNode { Zone = z, Floor = f, Lane = single ? center : i };
                        if (f == 0) node.Type = NodeType.Combate;
                        else if (f == mr.Floors - 1) node.Type = NodeType.Jefe;
                        else if (mr.SanctuaryFloors.Contains(f + 1) && f + 1 == mr.SanctuaryFloors[mr.SanctuaryFloors.Count - 1]) node.Type = NodeType.Santuario;
                        else node.Type = PickType(rng, mr, f);
                        if (node.Type == NodeType.Evento && rules.Events.Count > 0) node.EventId = rules.Events[rng.NextInt(rules.Events.Count)].Id;
                        nodes.Add(node);
                    }
                    // Pisos de Santuario intermedios: al menos uno.
                    if (!single && mr.SanctuaryFloors.Contains(f + 1) && !nodes.Exists(n => n.Type == NodeType.Santuario))
                        nodes[rng.NextInt(nodes.Count)].Type = NodeType.Santuario;
                    floors.Add(nodes);
                }

                for (int f = 0; f < mr.Floors - 1; f++) Connect(rng, floors[f], floors[f + 1]);
                map.Zones.Add(floors);
            }

            if (mancha != null && mancha.Almitas > 0 && mancha.Zone < map.Zones.Count)
            {
                var node = map.Node(mancha.Zone, mancha.Floor, mancha.Lane) ?? map.Floor(mancha.Zone, mancha.Floor)[0];
                node.HasMancha = true;
                if (node.Type != NodeType.Jefe && node.Floor != 0) node.Type = NodeType.Mancha;
            }
            return map;
        }

        private static NodeType PickType(IRandomSource rng, MapRules mr, int floor)
        {
            int total = 0;
            foreach (var kv in mr.Weights)
                if (Allowed(kv.Key, mr, floor)) total += kv.Value;
            int roll = rng.NextInt(Math.Max(1, total));
            // Orden fijo para que la misma semilla dé lo mismo en cualquier runtime.
            foreach (NodeType t in Enum.GetValues(typeof(NodeType)))
            {
                if (!mr.Weights.TryGetValue(t, out var w) || !Allowed(t, mr, floor)) continue;
                if (roll < w) return t;
                roll -= w;
            }
            return NodeType.Combate;
        }

        private static bool Allowed(NodeType t, MapRules mr, int floor) => !(t == NodeType.Elite && floor + 1 < mr.EliteFromFloor);

        private static void Connect(IRandomSource rng, List<MapNode> from, List<MapNode> to)
        {
            if (from.Count == 1 || to.Count == 1)
            {
                foreach (var a in from) foreach (var b in to) a.Next.Add(b.Lane);
                return;
            }
            foreach (var a in from)
            {
                var candidates = new List<int>();
                foreach (var b in to) if (Math.Abs(b.Lane - a.Lane) <= 1) candidates.Add(b.Lane);
                int first = candidates[rng.NextInt(candidates.Count)];
                a.Next.Add(first);
                // Bifurcación: la mitad de las veces abre un segundo camino.
                if (candidates.Count > 1 && rng.NextDouble() < 0.5)
                {
                    candidates.Remove(first);
                    a.Next.Add(candidates[rng.NextInt(candidates.Count)]);
                }
            }
            foreach (var b in to)
            {
                bool reached = from.Exists(a => a.Next.Contains(b.Lane));
                if (reached) continue;
                MapNode nearest = from[0];
                foreach (var a in from) if (Math.Abs(a.Lane - b.Lane) < Math.Abs(nearest.Lane - b.Lane)) nearest = a;
                nearest.Next.Add(b.Lane);
            }
            foreach (var a in from) a.Next.Sort();
        }

        /// <summary>Mezcla dos números en una semilla nueva (SplitMix64).</summary>
        public static ulong Mix(ulong a, ulong b)
        {
            ulong z = a + 0x9E3779B97F4A7C15UL * (b + 1);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
