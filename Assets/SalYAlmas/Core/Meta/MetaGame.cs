using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Progress;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Meta
{
    /// <summary>Resultado de una operación del meta-juego: éxito o el motivo por el que no se pudo.</summary>
    public readonly struct OpResult
    {
        public bool Ok { get; }
        public string Message { get; }

        private OpResult(bool ok, string message) { Ok = ok; Message = message ?? ""; }

        public static OpResult Success(string message = "") => new OpResult(true, message);
        public static OpResult Fail(string message) => new OpResult(false, message);
        public override string ToString() => (Ok ? "Ok" : "Error") + (Message.Length > 0 ? ": " + Message : "");
    }

    /// <summary>
    /// El bucle de progreso de la Fase 2 sobre un perfil: niveles y grados, Juicios, crafting, Salmueras, gacha y Fresno.
    /// Todo pasa por el libro contable y por un azar con semilla guardado en el perfil, así que es determinista.
    /// </summary>
    public sealed class MetaGame
    {
        public MetaDatabase Meta { get; }
        public GameDatabase Combat => Meta.Combat;
        public PlayerProfile Profile { get; }
        public SeededRandom Rng { get; }
        public Ledger Ledger => Profile.Ledger;

        public ProgressionService Progress { get; }
        public TrialService Trials { get; }
        public Forge Forge { get; }
        public Cauldron Cauldron { get; }
        public Termas Termas { get; }
        public FresnoService Fresno { get; }

        public MetaGame(MetaDatabase meta, PlayerProfile profile)
        {
            Meta = meta ?? throw new ArgumentNullException(nameof(meta));
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Profile.Ledger.Attach(meta.Catalog);
            Profile.Ledger.Day = Profile.Day;
            Rng = SeededRandom.FromState(Profile.Rng, Profile.Seed);
            Progress = new ProgressionService(this);
            Trials = new TrialService(this);
            Forge = new Forge(this);
            Cauldron = new Cauldron(this);
            Termas = new Termas(this);
            Fresno = new FresnoService(this);
        }

        public void SetDay(int day)
        {
            Profile.Day = day;
            Profile.Ledger.Day = day;
        }

        /// <summary>Agrega una bruja al aquelarre. Devuelve false si ya la tenías (no existen los duplicados).</summary>
        public bool AddWitch(string id)
        {
            if (Profile.Owns(id)) return false;
            if (!Meta.Roster.ContainsKey(id)) throw new KeyNotFoundException("Bruja desconocida: " + id);
            Profile.Witches[id] = new WitchProgress { Id = id, ObtainedDay = Profile.Day };
            return true;
        }

        // ------------------------------------------------------------------ Guardado

        public string ToJson()
        {
            Profile.Rng = Rng.GetState();
            return ProfileSerializer.ToJson(Profile);
        }

        public void Save(ISaveStore store, string slot) => store.Save(slot, ToJson());

        public static MetaGame Load(MetaDatabase meta, ISaveStore store, string slot)
        {
            var json = store.Load(slot);
            if (json == null) return null;
            return new MetaGame(meta, ProfileSerializer.FromJson(json));
        }

        // ------------------------------------------------------------------ Perfiles de inicio

        /// <summary>Cuenta nueva: estaciones de la Tortuga, recetas rústicas y forjadas, y las Sales de Baño del banner de inicio.</summary>
        public static PlayerProfile NewProfile(MetaDatabase meta, ulong seed, int day)
        {
            var p = new PlayerProfile { Seed = seed, Day = day };
            p.Rng = new SeededRandom(seed).GetState();
            p.Ledger.Attach(meta.Catalog);
            p.Ledger.Day = day;
            foreach (var s in meta.Stations) if (s.Base) p.Stations.Add(s.Id);
            foreach (var id in meta.RecipeOrder)
            {
                var r = meta.Recipes[id];
                var w = meta.Weapon(r.Output);
                if (r.Generated && w.Tier <= 2) p.Recipes.Add(id);
            }
            p.Recipes.Add("fusion_pendulo");
            var starter = meta.Gacha.Banners.Find(b => b.Type == BannerType.Inicio);
            if (starter != null)
            {
                p.Ledger.Apply(Res.SalesDeBano, (long)starter.MaxPulls * meta.Gacha.CostPerPull, "historia", "Bienvenida a Halia");
                p.Gacha.StarterChoice = starter.Choices.Count > 0 ? starter.Choices[0] : "";
            }
            return p;
        }

        /// <summary>
        /// Perfil de prueba para el Editor: las seis brujas con kit (cuatro en grado B y nivel de la Fase 1, Zarigüeya y Topo
        /// en grado D para subirlas por sus Juicios), todas las estaciones y recetas, y recursos marcados con la fuente "prueba".
        /// </summary>
        public static PlayerProfile TestProfile(MetaDatabase meta, ulong seed, int day)
        {
            var p = NewProfile(meta, seed, day);
            var game = new MetaGame(meta, p);
            foreach (var id in meta.Combat.WitchOrder)
            {
                game.AddWitch(id);
                var def = meta.Combat.Witch(id);
                var wp = p.Witches[id];
                if (id == "zarigueya") { wp.Level = 40; wp.Grade = Grade.D; }
                else if (id == "topo") { wp.Level = 38; wp.Grade = Grade.D; }
                else { wp.Level = def.Level; wp.Grade = Grade.B; wp.Via = def.Via; wp.ViaChosen = true; }
            }
            foreach (var s in meta.Stations) p.Stations.Add(s.Id);
            foreach (var id in meta.RecipeOrder) p.Recipes.Add(id);
            var l = p.Ledger;
            l.Apply(Res.Almitas, 1500000, "prueba");
            foreach (var planet in Planets.All)
            {
                l.Apply(Res.SalCruda(planet), 60, "prueba");
                l.Apply(Res.SalDestilada(planet), 40, "prueba");
                l.Apply(Res.Quintaesencia(planet), 30, "prueba");
            }
            l.Apply(Res.SalNegra, 10, "prueba");
            l.Apply(Res.Savia, 20000, "prueba");
            l.Apply(Res.SalesDeBano, 16000, "prueba");
            l.Apply(Res.Perlas, 1000, "prueba");
            foreach (var r in meta.Catalog.All())
                if (r.Family == "mat") l.Apply(r.Id, 30, "prueba");
            foreach (var w in new[] { "daga_1_a", "campana_2_b", "baculo_1_a", "grimorio_2_a", "incensario_1_a", "varita_1_a" })
                game.Forge.Grant(w);
            p.Rng = game.Rng.GetState();
            return p;
        }
    }
}
