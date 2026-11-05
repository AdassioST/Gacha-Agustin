using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Crafting
{
    public sealed class StationDef
    {
        public string Id;
        public string Name;
        public bool Base;
        public string Region = "";
        public string Description = "";
    }

    /// <summary>Uno de los cinco niveles (GDD 15.2).</summary>
    public sealed class WeaponTierDef
    {
        public int Tier;
        public string Name;
        public double Atk;
        public bool Regional;
        public long Almitas;
        /// <summary>Pureza de la sal de la receta: 0 Cruda, 1 Destilada, 2 Quintaesencia.</summary>
        public int SaltTier;
        public int SaltAmount;
        public int Material;
    }

    /// <summary>Familia de armas: su patrón de golpes y su fortaleza (GDD 15.1).</summary>
    public sealed class WeaponFamilyDef
    {
        public string Id;
        public string Name;
        public string Pattern = "";
        public string Strength = "";
        public List<HitDef> Hits = new List<HitDef>();
        public List<ChainEcho> Echoes = new List<ChainEcho>();
        public int FrameShift;
        public Planet[] Planets = new Planet[2];
        public string Station;
        public string Material;
        public StatBlock Mods = new StatBlock();
    }

    public sealed class WeaponDef
    {
        public string Id;
        public string Name;
        public string Family;
        /// <summary>1 a 5; 6 para las legendarias.</summary>
        public int Tier;
        /// <summary>0 o 1 en las 120 armas normales; −1 en las especiales.</summary>
        public int Variant = -1;
        public Planet Planet;
        public double Atk;
        public bool Legendary;
        public bool Pending;
        public string Description = "";
        /// <summary>Golpes propios (legendarias); si está vacío usa los de la familia.</summary>
        public List<HitDef> Hits = new List<HitDef>();
        public List<ChainEcho> Echoes = new List<ChainEcho>();
        public StatBlock Mods = new StatBlock();
    }

    public enum RecipeKind { Forge, Fusion }

    /// <summary>Un ingrediente de receta: un arma concreta, cualquier arma de un nivel y planeta, o un recurso.</summary>
    public sealed class RecipeInput
    {
        public string WeaponId;
        public int? Tier;
        public Planet? Planet;
        public string Resource;
        public long Amount;

        public bool IsWeapon => WeaponId != null || Tier.HasValue;
    }

    public sealed class RecipeDef
    {
        public string Id;
        public string Output;
        public RecipeKind Kind;
        public List<string> Stations = new List<string>();
        public List<RecipeInput> Inputs = new List<RecipeInput>();
        public string Description = "";
        /// <summary>Recetas generadas de las 120 armas (las de nivel 1 se conocen desde el inicio).</summary>
        public bool Generated;
    }

    public sealed class PrefixDef
    {
        public string Id;
        public string Name;
        public int Weight;
        public StatBlock Mods = new StatBlock();
    }

    // ------------------------------------------------------------------ Salmueras

    public sealed class BrewBaseDef
    {
        public string Id;
        public string Name;
        public Bundle Cost = new Bundle();
    }

    public sealed class ReagentDef
    {
        public string Id;
        public string Name;
        public string Resource => Res.Mat(Id);
    }

    public enum BrewSaltKind { Planet, BlackSalt, Almitas, Refined, None }

    /// <summary>La sal elegida para una Salmuera: un planeta con su pureza, Sal Negra, almitas concentradas o nada.</summary>
    public struct BrewSalt
    {
        public BrewSaltKind Kind;
        public Planet Planet;
        /// <summary>0 Cruda, 1 Destilada, 2 Quintaesencia.</summary>
        public int Purity;

        public static BrewSalt Of(Planet p, int purity) => new BrewSalt { Kind = BrewSaltKind.Planet, Planet = p, Purity = purity };
        public static BrewSalt Black => new BrewSalt { Kind = BrewSaltKind.BlackSalt };
        public static BrewSalt Souls => new BrewSalt { Kind = BrewSaltKind.Almitas };
        public static BrewSalt NoSalt => new BrewSalt { Kind = BrewSaltKind.None };
    }

    public sealed class BrewRecipeDef
    {
        public string Potion;
        public string Base;
        /// <summary>Planeta, "sal_negra", "almitas", "refinada" o "ninguna".</summary>
        public BrewSaltKind SaltKind;
        public Planet Planet;
        public string Reagent;
    }

    public sealed class PotencyDef
    {
        public int Tier;
        public string Name;
        public double Scale;
    }

    public sealed class TurbidDef
    {
        public string Name;
        public string Description = "";
        public List<EffectDef> Effects = new List<EffectDef>();
    }

    public sealed class BrewingRules
    {
        public readonly List<BrewBaseDef> Bases = new List<BrewBaseDef>();
        public readonly List<ReagentDef> Reagents = new List<ReagentDef>();
        public readonly List<BrewRecipeDef> Recipes = new List<BrewRecipeDef>();
        public readonly List<PotencyDef> Potency = new List<PotencyDef>();
        public readonly List<TurbidDef> Turbid = new List<TurbidDef>();
        public int SaltAmount = 1;
        public long AlmitasPerBrew = 500;
        public long AlmitasConcentradas = 3000;

        public BrewBaseDef Base(string id) => Bases.Find(b => b.Id == id);
        public ReagentDef Reagent(string id) => Reagents.Find(r => r.Id == id);
    }
}
