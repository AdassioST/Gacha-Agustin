using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Fresno
{
    public enum NodeType { Combate, Elite, Evento, Santuario, Caldero, Mercader, Nido, Mancha, Jefe }

    public sealed class ZoneDef
    {
        public string Id;
        public string Name;
        public string Theme = "";
        public string Boss;
        public readonly List<string> Enemies = new List<string>();
        public double HpScale = 1;
        public double AtkScale = 1;
        public long Almitas;
        public long Savia;
    }

    public sealed class MapRules
    {
        public int Floors = 10;
        public int Lanes = 3;
        public int EliteFromFloor = 3;
        public readonly List<int> SanctuaryFloors = new List<int>();
        public readonly Dictionary<NodeType, int> Weights = new Dictionary<NodeType, int>();
    }

    public sealed class EncounterRules
    {
        public int[] EnemiesByFloor = { 2, 2, 2, 3, 3, 3, 3, 3, 3, 1 };
        public double HpPerFloor = 0.03;
        public double EliteHp = 2.2;
        public double EliteAtk = 1.25;
        public double LevelPerFloor = 0.3;
    }

    public sealed class RewardRules
    {
        public double CombatFloorBonus = 0.05;
        public double EliteMultiplier = 2.5;
        public double BossMultiplier = 6;
        public double NidoMultiplier = 2;
        public int EliteBlackSalt = 1;
        public int NidoSalt = 2;
        public long FullClearSavia = 3000;
    }

    public sealed class WeeklyRules
    {
        public long CapAlmitas = 300000;
        public double OverflowSaviaRate = 0.10;
        public long BathSaltsPerBoss = 125;
        public long BathSaltsMax = 500;
    }

    public sealed class GraftDef
    {
        public string Id;
        public string Name;
        public Planet Rama;
        public bool Pending;
        public StatBlock Mods = new StatBlock();
        public string Description => Pending ? "Pendiente: definido en datos, sin efecto todavía." : Mods.ToString();
    }

    public sealed class FruitDef
    {
        public string Name;
        public string Description = "";
        public StatBlock Mods = new StatBlock();
    }

    /// <summary>Una de las siete Ramas (GDD 10.3): 15 Injertos de un planeta y su Fruto.</summary>
    public sealed class RamaDef
    {
        public Planet Planet;
        public string Name;
        public int UnlockOfrenda;
        public FruitDef Fruit = new FruitDef();
        public readonly List<GraftDef> Grafts = new List<GraftDef>();
    }

    public sealed class MutationDef
    {
        public string Id;
        public string Name;
        public string Text = "";
        public StatBlock Mods = new StatBlock();
        public string Description => string.IsNullOrEmpty(Text) ? Mods.ToString() : Text;
    }

    public sealed class CurseDef
    {
        public string Id;
        public string Name;
        public StatBlock Mods = new StatBlock();
    }

    public sealed class RootDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        public StatBlock Mods = new StatBlock();
        public readonly List<long> Costs = new List<long>();
        public int Max => Costs.Count;
    }

    public sealed class OfrendaRules
    {
        public double LevelBase = 50000;
        public double LevelExponent = 1.5;
        public StatBlock PerLevel = new StatBlock();
        public StatBlock Caps = new StatBlock();
        public int DustEvery = 5;
        public long DustAmount = 50;
        public readonly SortedDictionary<int, string> Unlocks = new SortedDictionary<int, string>();

        /// <summary>Almitas ofrendadas en total para llegar al nivel n.</summary>
        public long Threshold(int level) => level <= 0 ? 0 : (long)System.Math.Round(LevelBase * System.Math.Pow(level, LevelExponent));

        public int LevelFor(long offered)
        {
            int level = 0;
            while (offered >= Threshold(level + 1)) level++;
            return level;
        }
    }

    public enum CalculistaStat { Calculo, Temple, Encanto, Oficio }

    public enum EventOutcomeType { Almitas, Graft, Mutation, BlackSalt, Salinity, Heal, RunMods, Curse, Savia }

    public sealed class EventOutcome
    {
        public EventOutcomeType Type;
        public double Value;
        public string Name = "";
        public StatBlock Mods = new StatBlock();
    }

    public sealed class EventOption
    {
        public string Text;
        public CalculistaStat? Stat;
        public int Need;
        public long CostAlmitas;
        public readonly List<EventOutcome> Outcomes = new List<EventOutcome>();
    }

    public sealed class FresnoEventDef
    {
        public string Id;
        public string Name;
        public string Text = "";
        public readonly List<EventOption> Options = new List<EventOption>();
    }

    public sealed class EcoUnlockRule
    {
        public int Slot;
        /// <summary>"inicio" o "jefe:&lt;zona&gt;".</summary>
        public string Rule;
    }

    /// <summary>Todas las reglas del Fresno (fresno.json).</summary>
    public sealed class FresnoRules
    {
        public readonly List<ZoneDef> Zones = new List<ZoneDef>();
        public MapRules Map = new MapRules();
        public EncounterRules Encounters = new EncounterRules();
        public RewardRules Rewards = new RewardRules();
        public double ManchaReturn = 0.30;
        public double ReviveHpAfterWin = 0.10;
        public WeeklyRules Weekly = new WeeklyRules();
        public int RestSalinity = -40;
        public double RestHeal = 0.30;
        public double TrainBonus = 0.25;
        public long MerchantGraftPrice = 2500;
        public long MerchantPotionPrice = 1500;
        public long MerchantHealPrice = 2000;
        public double MerchantHealAmount = 0.30;
        public int GraftChoices = 3;
        public int FruitAt = 3;
        public int MaxCursesPerZone = 5;
        public double CurseAlmitasMultiplier = 1.25;
        public readonly List<CurseDef> Curses = new List<CurseDef>();
        public int PariaThreeStarChoices = 1;
        public double PariaThreeStarAlmitas = 0.10;
        public double PariaFourStarAlmitas = 0.05;
        public int EcoSlots = 3;
        public int EcoGrafts = 3;
        public int EcoTrainedMoves = 1;
        public readonly List<EcoUnlockRule> EcoUnlocks = new List<EcoUnlockRule>();
        public readonly List<RamaDef> Ramas = new List<RamaDef>();
        public readonly List<MutationDef> Mutations = new List<MutationDef>();
        public readonly List<RootDef> Roots = new List<RootDef>();
        public OfrendaRules Ofrenda = new OfrendaRules();
        public int CalculistaBase = 10;
        public int CalculistaWitchBonus = 2;
        public readonly List<FresnoEventDef> Events = new List<FresnoEventDef>();

        public RamaDef Rama(Planet p) => Ramas.Find(r => r.Planet == p);

        public GraftDef Graft(string id)
        {
            foreach (var r in Ramas) foreach (var g in r.Grafts) if (g.Id == id) return g;
            return null;
        }

        public MutationDef Mutation(string id) => Mutations.Find(m => m.Id == id);
        public CurseDef Curse(string id) => Curses.Find(c => c.Id == id);
        public RootDef Root(string id) => Roots.Find(r => r.Id == id);
        public FresnoEventDef Event(string id) => Events.Find(e => e.Id == id);
        public int ZoneIndex(string id) => Zones.FindIndex(z => z.Id == id);
    }
}
