using System;
using System.Collections.Generic;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Economy
{
    /// <summary>Un recurso de la sección 11.1 con sus fuentes y sumideros declarados.</summary>
    public sealed class ResourceDef
    {
        public string Id;
        public string Name;
        public bool Sold;
        /// <summary>Familia del recurso ("sal_cruda" para "sal_cruda.marte").</summary>
        public string Family;
        public Planet? Planet;
        public string Region = "";
        public readonly HashSet<string> Sources = new HashSet<string>();
        public readonly HashSet<string> Sinks = new HashSet<string>();
    }

    /// <summary>Todos los recursos y flujos de la economía, ya expandidos por planeta y por material.</summary>
    public sealed class ResourceCatalog
    {
        private readonly Dictionary<string, ResourceDef> _resources = new Dictionary<string, ResourceDef>();
        private readonly List<string> _order = new List<string>();
        private readonly Dictionary<string, string> _flows = new Dictionary<string, string>();

        public IReadOnlyList<string> Order => _order;
        public IReadOnlyDictionary<string, string> Flows => _flows;
        /// <summary>Familias en el orden del JSON (las filas de la tabla 11.1).</summary>
        public readonly List<string> Families = new List<string>();
        public readonly Dictionary<string, string> FamilyNames = new Dictionary<string, string>();

        public void AddFlow(string id, string name) => _flows[id] = name;

        public void Add(ResourceDef r)
        {
            _resources.Add(r.Id, r);
            _order.Add(r.Id);
        }

        public bool Has(string id) => id != null && _resources.ContainsKey(id);

        public ResourceDef Get(string id) =>
            id != null && _resources.TryGetValue(id, out var r) ? r : throw new KeyNotFoundException("Recurso desconocido: " + id);

        public string FlowName(string flow) => flow != null && _flows.TryGetValue(flow, out var n) ? n : flow;

        public string NameOf(string id) => Has(id) ? Get(id).Name : id;

        public IEnumerable<ResourceDef> All()
        {
            foreach (var id in _order) yield return _resources[id];
        }
    }

    /// <summary>Claves de recurso. Las sales llevan el planeta: "sal_cruda.marte". Los materiales: "mat.obsidiana".</summary>
    public static class Res
    {
        public const string Almitas = "almitas";
        public const string SalNegra = "sal_negra";
        public const string Savia = "savia";
        public const string SalesDeBano = "sales_de_bano";
        public const string Perlas = "perlas";
        public const string PolvoAstral = "polvo_astral";

        public static string SalCruda(Planet p) => "sal_cruda." + Planets.Id(p);
        public static string SalDestilada(Planet p) => "sal_destilada." + Planets.Id(p);
        public static string Quintaesencia(Planet p) => "quintaesencia." + Planets.Id(p);
        public static string Mat(string id) => "mat." + id;

        /// <summary>Sal de un planeta según su pureza: 0 Cruda, 1 Destilada, 2 Quintaesencia.</summary>
        public static string Salt(Planet p, int purityTier)
        {
            switch (purityTier)
            {
                case 0: return SalCruda(p);
                case 1: return SalDestilada(p);
                default: return Quintaesencia(p);
            }
        }
    }

    /// <summary>Un movimiento del libro contable: siempre con su fuente (si suma) o su sumidero (si resta).</summary>
    public sealed class LedgerEntry
    {
        public long Seq;
        public int Day;
        public string Resource;
        public long Delta;
        public long Balance;
        public string Flow;
        public string Note = "";
    }

    /// <summary>Un costo o una recompensa de varios recursos.</summary>
    public sealed class Bundle
    {
        public readonly List<KeyValuePair<string, long>> Items = new List<KeyValuePair<string, long>>();

        public Bundle Add(string resource, long amount)
        {
            if (amount == 0) return this;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Key != resource) continue;
                Items[i] = new KeyValuePair<string, long>(resource, Items[i].Value + amount);
                return this;
            }
            Items.Add(new KeyValuePair<string, long>(resource, amount));
            return this;
        }

        public long Get(string resource)
        {
            foreach (var kv in Items) if (kv.Key == resource) return kv.Value;
            return 0;
        }

        public bool IsEmpty => Items.Count == 0;

        public string Describe(ResourceCatalog catalog)
        {
            var parts = new List<string>();
            foreach (var kv in Items) parts.Add($"{kv.Value:N0} {(catalog != null ? catalog.NameOf(kv.Key) : kv.Key)}");
            return parts.Count == 0 ? "gratis" : string.Join(", ", parts);
        }
    }

    public sealed class LedgerException : InvalidOperationException
    {
        public LedgerException(string message) : base(message) { }
    }

    /// <summary>
    /// Libro contable: saldos de todos los recursos y el registro de cada movimiento con su fuente o su sumidero
    /// (GDD 11: "cada recurso tiene una fuente clara y un sumidero claro"). Nada cambia un saldo sin pasar por aquí.
    /// Se guarda con el perfil; el catálogo se vuelve a enganchar al cargar.
    /// </summary>
    public sealed class Ledger
    {
        /// <summary>Movimientos que se conservan en el guardado; los totales por flujo nunca se pierden.</summary>
        public const int MaxEntries = 1000;

        public Dictionary<string, long> Balances = new Dictionary<string, long>();
        public List<LedgerEntry> Entries = new List<LedgerEntry>();
        /// <summary>Total por "recurso|flujo" de todo lo que entró y salió.</summary>
        public Dictionary<string, long> TotalsIn = new Dictionary<string, long>();
        public Dictionary<string, long> TotalsOut = new Dictionary<string, long>();
        public long NextSeq = 1;
        public int Day;

        private ResourceCatalog _catalog;

        public ResourceCatalog Catalog => _catalog;

        public void Attach(ResourceCatalog catalog) => _catalog = catalog;

        public long Balance(string resource) => Balances.TryGetValue(resource, out var v) ? v : 0;

        public bool CanAfford(Bundle cost)
        {
            foreach (var kv in cost.Items) if (Balance(kv.Key) < kv.Value) return false;
            return true;
        }

        /// <summary>Qué falta para pagar un costo ("" si alcanza).</summary>
        public string Missing(Bundle cost)
        {
            var parts = new List<string>();
            foreach (var kv in cost.Items)
            {
                long have = Balance(kv.Key);
                if (have < kv.Value) parts.Add($"{kv.Value - have:N0} {(_catalog != null ? _catalog.NameOf(kv.Key) : kv.Key)}");
            }
            return parts.Count == 0 ? "" : "Falta: " + string.Join(", ", parts);
        }

        /// <summary>Suma (delta &gt; 0, con una fuente) o resta (delta &lt; 0, con un sumidero) un recurso.</summary>
        public LedgerEntry Apply(string resource, long delta, string flow, string note = "")
        {
            if (delta == 0) return null;
            if (_catalog != null)
            {
                if (!_catalog.Has(resource)) throw new LedgerException("Recurso desconocido: " + resource);
                var def = _catalog.Get(resource);
                if (delta > 0 && !def.Sources.Contains(flow))
                    throw new LedgerException($"{def.Name} no tiene la fuente '{flow}'. Fuentes: {string.Join(", ", def.Sources)}");
                if (delta < 0 && !def.Sinks.Contains(flow))
                    throw new LedgerException($"{def.Name} no tiene el sumidero '{flow}'. Sumideros: {string.Join(", ", def.Sinks)}");
            }
            long balance = Balance(resource) + delta;
            if (balance < 0) throw new LedgerException($"Saldo insuficiente de {resource}: {Balance(resource)} < {-delta}");
            Balances[resource] = balance;
            var entry = new LedgerEntry { Seq = NextSeq++, Day = Day, Resource = resource, Delta = delta, Balance = balance, Flow = flow, Note = note ?? "" };
            Entries.Add(entry);
            if (Entries.Count > MaxEntries) Entries.RemoveRange(0, Entries.Count - MaxEntries);
            var totals = delta > 0 ? TotalsIn : TotalsOut;
            string key = resource + "|" + flow;
            totals[key] = (totals.TryGetValue(key, out var t) ? t : 0) + Math.Abs(delta);
            return entry;
        }

        public void Grant(Bundle reward, string flow, string note = "")
        {
            foreach (var kv in reward.Items) Apply(kv.Key, kv.Value, flow, note);
        }

        /// <summary>Paga un costo completo o nada: primero comprueba que alcance.</summary>
        public bool TrySpend(Bundle cost, string flow, string note = "")
        {
            if (!CanAfford(cost)) return false;
            foreach (var kv in cost.Items) Apply(kv.Key, -kv.Value, flow, note);
            return true;
        }

        public long TotalIn(string resource, string flow) => TotalsIn.TryGetValue(resource + "|" + flow, out var v) ? v : 0;
        public long TotalOut(string resource, string flow) => TotalsOut.TryGetValue(resource + "|" + flow, out var v) ? v : 0;
    }
}
