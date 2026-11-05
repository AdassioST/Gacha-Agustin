using System;
using System.Collections.Generic;

namespace SalYAlmas.Core.Data
{
    public sealed class DataValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public DataValidationException(IReadOnlyList<string> errors)
            : base("Datos inválidos:\n - " + string.Join("\n - ", errors))
        {
            Errors = errors;
        }
    }

    /// <summary>Todo lo que el combate necesita leer de los datos, ya validado.</summary>
    public sealed class GameDatabase
    {
        public CombatRules Rules { get; }
        public IReadOnlyDictionary<string, WitchDef> Witches => _witches;
        public IReadOnlyDictionary<string, EnemyDef> Enemies => _enemies;
        public IReadOnlyDictionary<string, EncounterDef> Encounters => _encounters;
        public IReadOnlyDictionary<string, PotionDef> Potions => _potions;

        /// <summary>Orden en que aparecen en los JSON (para menús deterministas).</summary>
        public IReadOnlyList<string> WitchOrder => _witchOrder;
        public IReadOnlyList<string> EncounterOrder => _encounterOrder;
        public IReadOnlyList<string> PotionOrder => _potionOrder;

        private readonly Dictionary<string, WitchDef> _witches = new Dictionary<string, WitchDef>();
        private readonly Dictionary<string, EnemyDef> _enemies = new Dictionary<string, EnemyDef>();
        private readonly Dictionary<string, EncounterDef> _encounters = new Dictionary<string, EncounterDef>();
        private readonly Dictionary<string, PotionDef> _potions = new Dictionary<string, PotionDef>();
        private readonly List<string> _witchOrder = new List<string>();
        private readonly List<string> _encounterOrder = new List<string>();
        private readonly List<string> _potionOrder = new List<string>();

        public GameDatabase(CombatRules rules)
        {
            Rules = rules ?? new CombatRules();
        }

        public void Add(WitchDef w) { _witches.Add(w.Id, w); _witchOrder.Add(w.Id); }
        public void Add(EnemyDef e) { _enemies.Add(e.Id, e); }
        public void Add(EncounterDef e) { _encounters.Add(e.Id, e); _encounterOrder.Add(e.Id); }
        public void Add(PotionDef p) { _potions.Add(p.Id, p); _potionOrder.Add(p.Id); }

        public WitchDef Witch(string id) => _witches.TryGetValue(id, out var w) ? w : throw new KeyNotFoundException("Bruja desconocida: " + id);
        public EnemyDef Enemy(string id) => _enemies.TryGetValue(id, out var e) ? e : throw new KeyNotFoundException("Enemigo desconocido: " + id);
        public EncounterDef Encounter(string id) => _encounters.TryGetValue(id, out var e) ? e : throw new KeyNotFoundException("Encuentro desconocido: " + id);
        public PotionDef Potion(string id) => _potions.TryGetValue(id, out var p) ? p : throw new KeyNotFoundException("Poción desconocida: " + id);
    }
}
