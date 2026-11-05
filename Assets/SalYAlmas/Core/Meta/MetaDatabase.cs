using System.Collections.Generic;
using System.IO;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Progress;

namespace SalYAlmas.Core.Meta
{
    /// <summary>Textos de los JSON de la Fase 2.</summary>
    public sealed class MetaDataSources
    {
        public string Economy;
        public string Roster;
        public string Progression;
        public string Weapons;
        public string Brewing;
        public string Gacha;
        public string Fresno;
        public string FresnoEnemies;
        public string Story;
        public string Services;
        public string Loc;
        /// <summary>Kits de las 18 brujas que no están en witches.json (opcional: sin él, solo pelean las seis de la Fase 1).</summary>
        public string WitchesExt;
        /// <summary>Zonas, secciones, botín y enemigos de la Odisea (opcional).</summary>
        public string Odyssey;
        /// <summary>Encargos, logros y Altura (opcional).</summary>
        public string Quests;

        public static readonly string[] FileNames =
        {
            "economy.json", "roster.json", "progression.json", "weapons.json", "brewing.json", "gacha.json", "fresno.json", "fresno_enemies.json", "story.json", "services.json", "loc.json",
            "witches_ext.json", "odyssey.json", "quests.json",
        };

        public static MetaDataSources FromDirectory(string directory)
        {
            string Read(string name) => File.ReadAllText(Path.Combine(directory, name));
            string Optional(string name) => File.Exists(Path.Combine(directory, name)) ? Read(name) : null;
            return new MetaDataSources
            {
                Economy = Read("economy.json"),
                Roster = Read("roster.json"),
                Progression = Read("progression.json"),
                Weapons = Read("weapons.json"),
                Brewing = Read("brewing.json"),
                Gacha = Read("gacha.json"),
                Fresno = Read("fresno.json"),
                FresnoEnemies = Read("fresno_enemies.json"),
                Story = Read("story.json"),
                Services = Read("services.json"),
                Loc = Read("loc.json"),
                WitchesExt = Optional("witches_ext.json"),
                Odyssey = Optional("odyssey.json"),
                Quests = Optional("quests.json"),
            };
        }
    }

    /// <summary>Todo lo que el bucle de progreso necesita leer de los datos, ya validado y con referencias resueltas.</summary>
    public sealed class MetaDatabase
    {
        public GameDatabase Combat { get; }
        public ResourceCatalog Catalog { get; } = new ResourceCatalog();
        public ProgressionRules Progression { get; } = new ProgressionRules();
        public BrewingRules Brewing { get; } = new BrewingRules();
        public GachaRules Gacha { get; } = new GachaRules();
        public FresnoRules Fresno { get; } = new FresnoRules();
        /// <summary>Contenido de la Fase 3: historia, Termas, tienda y localización.</summary>
        public World.WorldDatabase World { get; internal set; } = new World.WorldDatabase();

        public readonly Dictionary<string, RosterEntry> Roster = new Dictionary<string, RosterEntry>();
        public readonly List<string> RosterOrder = new List<string>();

        public readonly List<StationDef> Stations = new List<StationDef>();
        public readonly List<WeaponTierDef> Tiers = new List<WeaponTierDef>();
        public readonly List<WeaponFamilyDef> Families = new List<WeaponFamilyDef>();
        public readonly Dictionary<string, WeaponDef> Weapons = new Dictionary<string, WeaponDef>();
        public readonly List<string> WeaponOrder = new List<string>();
        public readonly Dictionary<string, RecipeDef> Recipes = new Dictionary<string, RecipeDef>();
        public readonly List<string> RecipeOrder = new List<string>();
        public readonly List<PrefixDef> Prefixes = new List<PrefixDef>();
        public double AffinityDamage = 0.08;
        public long ReforgeAlmitasPerTier = 3000;
        public int ReforgeSaltPerTier = 2;
        /// <summary>Altar del Árbol: sal del grado anterior y almitas por cada unidad refinada (índice 1 Destilada, 2 Quintaesencia).</summary>
        public readonly int[] RefineFrom = { 0, 5, 5 };
        public readonly long[] RefineAlmitas = { 0, 1000, 5000 };

        public readonly Dictionary<string, EnemyDef> FresnoEnemies = new Dictionary<string, EnemyDef>();
        /// <summary>Enemigos de las regiones de la Odisea (odyssey.json).</summary>
        public readonly Dictionary<string, EnemyDef> RegionEnemies = new Dictionary<string, EnemyDef>();

        public MetaDatabase(GameDatabase combat) { Combat = combat; }

        public RosterEntry Witch(string id) => Roster.TryGetValue(id, out var r) ? r : throw new KeyNotFoundException("Bruja desconocida: " + id);
        public WeaponDef Weapon(string id) => Weapons.TryGetValue(id, out var w) ? w : throw new KeyNotFoundException("Arma desconocida: " + id);
        public WeaponFamilyDef Family(string id) => Families.Find(f => f.Id == id);
        public WeaponTierDef Tier(int tier) => Tiers.Find(t => t.Tier == tier);
        public StationDef Station(string id) => Stations.Find(s => s.Id == id);
        public PrefixDef Prefix(string id) => string.IsNullOrEmpty(id) ? null : Prefixes.Find(p => p.Id == id);

        public RecipeDef RecipeFor(string weaponId)
        {
            foreach (var id in RecipeOrder) if (Recipes[id].Output == weaponId) return Recipes[id];
            return null;
        }

        /// <summary>Busca un enemigo del Fresno o, si no, de enemies.json.</summary>
        public EnemyDef Enemy(string id)
        {
            if (FresnoEnemies.TryGetValue(id, out var e)) return e;
            if (RegionEnemies.TryGetValue(id, out var re)) return re;
            return Combat.Enemies.TryGetValue(id, out var c) ? c : null;
        }

        public bool HasKit(string witchId) => Combat.Witches.ContainsKey(witchId);
    }
}
