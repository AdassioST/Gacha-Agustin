using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Fresno;

namespace SalYAlmas.Core.World
{
    // ------------------------------------------------------------------ Historia (GDD 6 y 13)

    public sealed class SignDef
    {
        public string Id;
        public string Name;
        public string Element;
        public string Phrase = "";
        /// <summary>Región donde empieza quien eligió este signo. Vacío si la región todavía no sale.</summary>
        public string Region = "";
        public CalculistaStat Bonus;
    }

    public sealed class SenderoDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        public StatBlock Mods = new StatBlock();
        /// <summary>Lo que el núcleo todavía no aplica de este Sendero (se reporta y se muestra).</summary>
        public string Pending = "";
    }

    public sealed class TitleDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        public StatBlock Mods = new StatBlock();
    }

    public enum BookBand { Bajo, Medio, Alto, Maximo }

    public sealed class StoryEffects
    {
        public readonly List<string> Marks = new List<string>();
        public string Title = "";
        public int Souls;
        public int Paralaje;
        public string Witch = "";
        public readonly List<KeyValuePair<string, long>> Rewards = new List<KeyValuePair<string, long>>();

        public bool IsEmpty => Marks.Count == 0 && Title.Length == 0 && Souls == 0 && Paralaje == 0 && Witch.Length == 0 && Rewards.Count == 0;
    }

    /// <summary>Condiciones de una variante de texto o de una opción. Todas deben cumplirse.</summary>
    public sealed class StoryCondition
    {
        public readonly List<string> Marks = new List<string>();
        public readonly List<string> NotMarks = new List<string>();
        public string Title = "";
        public string Witch = "";
        public int MinCorruption;
        public int MinSouls;
        public int MinParalaje;

        public bool IsEmpty => Marks.Count == 0 && NotMarks.Count == 0 && Title.Length == 0 && Witch.Length == 0 && MinCorruption == 0 && MinSouls == 0 && MinParalaje == 0;
    }

    public sealed class StoryVariant
    {
        public StoryCondition When = new StoryCondition();
        public string Text = "";
    }

    public sealed class StoryChoice
    {
        public string Text = "";
        /// <summary>Solo aparece si se cumple.</summary>
        public StoryCondition When = new StoryCondition();
        public CalculistaStat? Stat;
        public int Need;
        public StoryEffects Effects = new StoryEffects();
        public string Next = "";
    }

    public sealed class StoryBattle
    {
        public readonly List<string> Enemies = new List<string>();
        /// <summary>Equipo fijo (prólogo). Vacío: el equipo que elija la jugadora.</summary>
        public readonly List<string> Team = new List<string>();
        public double Strength = 1.0;
        public bool Boss;
        /// <summary>Empieza con la Resonancia llena y marca el primer combate guiado.</summary>
        public bool Tutorial;
        public string Next = "";
    }

    public sealed class StoryScene
    {
        public string Id;
        public string Speaker = "";
        /// <summary>ID de la bruja que se muestra en el retrato.</summary>
        public string Portrait = "";
        public string Text = "";
        public readonly List<StoryVariant> Variants = new List<StoryVariant>();
        public readonly List<StoryChoice> Choices = new List<StoryChoice>();
        public StoryBattle Battle;
        public StoryEffects Effects = new StoryEffects();
        public string Next = "";
    }

    public sealed class MissionDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        public string Start;
        public readonly Dictionary<string, StoryScene> Scenes = new Dictionary<string, StoryScene>();
        public StoryEffects Rewards = new StoryEffects();
    }

    public sealed class RegionDef
    {
        public string Id;
        public string Sign = "";
        public string Name;
        public string Theme = "";
        public string Echo = "";
        /// <summary>El prólogo no cuenta para los eclipses ni para el orden de regiones.</summary>
        public bool Prologue;
        public readonly List<MissionDef> Missions = new List<MissionDef>();
    }

    public sealed class StoryRules
    {
        public int StatMax = 20;
        public int StatBase = 4;
        public int WitchBonus = 2;
        public readonly List<string> Species = new List<string>();
        public readonly List<string> Pronouns = new List<string>();
        public readonly List<SignDef> Signs = new List<SignDef>();
        public int ParalajeMax = 100;
        public int[] ParalajeBands = { 25, 50, 80 };
        public int EclipseEvery = 2;
        public int MaxCorruption = 2;
        /// <summary>Las doce regiones del anillo, para el reloj de eclipses aunque aún no se puedan jugar.</summary>
        public readonly List<string> Ring = new List<string>();
        public readonly List<SenderoDef> Senderos = new List<SenderoDef>();
        public int SenderoMaxLevel = 10;
        public int SenderoXpPerLevel = 100;
        public int SenderoXpPerWin = 25;
        public double SenderoGrowth = 0.05;
        public readonly List<TitleDef> Titles = new List<TitleDef>();
        public readonly List<RegionDef> Regions = new List<RegionDef>();

        public RegionDef Region(string id) => Regions.Find(r => r.Id == id);
        public SenderoDef Sendero(string id) => Senderos.Find(s => s.Id == id);
        public TitleDef Title(string id) => Titles.Find(t => t.Id == id);
        public SignDef Sign(string id) => Signs.Find(s => s.Id == id);
    }

    // ------------------------------------------------------------------ Termas (propuesta de meta-juego, sección 10)

    public sealed class TermasRules
    {
        public int BathParalaje = -20;
        public int BathCooldownDays = 1;
        public int TalkTrust = 20;
        public int TrustPerLevel = 400;
        public int TrustMaxLevel = 10;
        public readonly List<string> GenericTalk = new List<string>();
        public readonly List<string> GenericTouch = new List<string>();
        public readonly Dictionary<string, List<string>> Talk = new Dictionary<string, List<string>>();
        public readonly Dictionary<string, string> Shake = new Dictionary<string, string>();
        public readonly Dictionary<string, string> PullLine = new Dictionary<string, string>();
        public string DefaultShake = "";
    }

    // ------------------------------------------------------------------ Tienda y cosméticos (GDD 16.4 y 16.5)

    public enum ProductKind { Perlas, Grimorio, Diezmo }

    public sealed class ProductDef
    {
        public string Id;
        public string Name;
        public ProductKind Kind;
        public double PriceUsd;
        public long Perlas;
        public long Bonus;
        public int DailySales;
        public int Days;
    }

    public sealed class CosmeticDef
    {
        public string Id;
        public string Name;
        /// <summary>atuendo, ascendente, color_sal, manantial, marco, decoracion.</summary>
        public string Kind;
        public string Witch = "";
        public string Description = "";
        public long Perlas;
        public long Polvo;
        /// <summary>Color en hexadecimal, para los manantiales y el color de sal de prueba.</summary>
        public string Color = "";
    }

    public sealed class PassTier
    {
        public int Level;
        public readonly List<KeyValuePair<string, long>> Free = new List<KeyValuePair<string, long>>();
        public readonly List<KeyValuePair<string, long>> Premium = new List<KeyValuePair<string, long>>();
        public string PremiumCosmetic = "";
    }

    public sealed class ShopRules
    {
        public readonly List<ProductDef> Products = new List<ProductDef>();
        public readonly List<CosmeticDef> Cosmetics = new List<CosmeticDef>();
        public readonly List<PassTier> Pass = new List<PassTier>();
        public int PassXpPerLevel = 1000;
        public int PassDays = 42;
        public readonly List<string> PaidGachaBlocked = new List<string>();

        public ProductDef Product(string id) => Products.Find(p => p.Id == id);
        public CosmeticDef Cosmetic(string id) => Cosmetics.Find(c => c.Id == id);
    }

    // ------------------------------------------------------------------ Localización (GDD 21.2)

    public sealed class LocTable
    {
        public readonly List<string> Languages = new List<string>();
        /// <summary>clave → idioma → texto.</summary>
        public readonly Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>();
        public string Fallback = "es";

        public string Get(string key, string language)
        {
            if (!Strings.TryGetValue(key, out var row)) return key;
            if (row.TryGetValue(language, out var s) && s.Length > 0) return s;
            return row.TryGetValue(Fallback, out var f) ? f : key;
        }
    }

    /// <summary>Todo el contenido de la Fase 3, ya validado.</summary>
    public sealed class WorldDatabase
    {
        public readonly StoryRules Story = new StoryRules();
        public readonly TermasRules Termas = new TermasRules();
        public readonly ShopRules Shop = new ShopRules();
        public readonly LocTable Loc = new LocTable();
    }
}
