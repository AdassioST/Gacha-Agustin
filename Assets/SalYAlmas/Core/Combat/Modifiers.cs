using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    /// <summary>
    /// Claves de los modificadores que entiende el combate. Los Injertos, Mutaciones, maldiciones, Raíces, armas
    /// y prefijos se escriben en datos con estas claves; el cargador rechaza cualquier otra.
    /// Todos los valores por defecto son neutros (0), así que un combate sin modificadores es el de la Fase 1.
    /// </summary>
    public static class ModKeys
    {
        // Bruja
        public const string AtkPct = "atkPct";
        public const string FlatAtk = "flatAtk";
        public const string HpPct = "hpPct";
        public const string SpeedFlat = "speedFlat";
        public const string CritRate = "critRate";
        public const string CritDamage = "critDamage";
        public const string DamagePct = "damagePct";
        public const string AscendantDamagePct = "ascendantDamagePct";
        public const string Penetration = "penetration";
        public const string DefenseDown = "defenseDown";
        public const string AnimaGainPct = "animaGainPct";
        public const string AspectBonus = "aspectBonus";
        public const string AreaDamagePct = "areaDamagePct";
        public const string PotionPotencyPct = "potionPotencyPct";
        public const string DelayPct = "delayPct";
        public const string StartGrains = "startGrains";
        public const string SalinityPerSpell = "salinityPerSpell";
        public const string HilosCostPct = "hilosCostPct";
        public const string FrameShift = "frameShift";
        public const string SparkDamageBonus = "sparkDamageBonus";
        public const string ChainDamagePct = "chainDamagePct";
        public const string AscensionDamagePct = "ascensionDamagePct";
        public const string SpellDamagePct = "spellDamagePct";
        public const string BasicDamagePct = "basicDamagePct";
        public const string ChainMoveApertura = "chainMove.apertura";
        public const string ChainMoveEnlace = "chainMove.enlace";
        public const string ChainMoveCierre = "chainMove.cierre";
        public const string StartAnima = "startAnima";
        public const string ToughnessPct = "toughnessPct";
        public const string DotPct = "dotPct";

        // Equipo
        public const string HealingPct = "healingPct";
        public const string ShieldPct = "shieldPct";
        public const string StartResonance = "startResonance";
        public const string ExtraBlackSalt = "extraBlackSalt";
        public const string ExtraBeltSlots = "extraBeltSlots";
        public const string BreakVulnerability = "breakVulnerability";
        public const string SalinityGainPct = "salinityGainPct";
        public const string ResonanceGainPct = "resonanceGainPct";
        public const string SparkResonanceCap = "sparkResonanceCap";

        // Enemigos
        public const string EnemyHpPct = "enemyHpPct";
        public const string EnemyAtkPct = "enemyAtkPct";
        public const string EnemySpeedPct = "enemySpeedPct";
        public const string EnemyToughnessPct = "enemyToughnessPct";

        // Frutos de las Ramas (efectos especiales)
        public const string DelayDamageCoef = "delayDamageCoef";
        public const string BurningTargetDamage = "burningTargetDamage";
        public const string BleedingTargetCrit = "bleedingTargetCrit";
        public const string StartShieldPct = "startShieldPct";
        public const string StartAdvance = "startAdvance";
        public const string EndTurnHealPct = "endTurnHealPct";

        // Solo del Fresno: el combate los ignora
        public const string AlmitasPct = "almitasPct";
        public const string GraftChoices = "graftChoices";
        public const string SantuarioHealPct = "santuarioHealPct";
        public const string SaviaPct = "saviaPct";

        /// <summary>Prefijo de las claves por planeta: "damagePct.marte" suma daño a las brujas cuyo planeta dominante es Marte.</summary>
        public const string PlanetDamagePrefix = "damagePct.";

        public static string PlanetDamage(Planet p) => PlanetDamagePrefix + Planets.Id(p);

        private static readonly HashSet<string> Known = new HashSet<string>
        {
            AtkPct, FlatAtk, HpPct, SpeedFlat, CritRate, CritDamage, DamagePct, AscendantDamagePct, Penetration, DefenseDown,
            AnimaGainPct, AspectBonus, AreaDamagePct, PotionPotencyPct, DelayPct, StartGrains, SalinityPerSpell, HilosCostPct,
            FrameShift, SparkDamageBonus, ChainDamagePct, AscensionDamagePct, SpellDamagePct, BasicDamagePct,
            ChainMoveApertura, ChainMoveEnlace, ChainMoveCierre, StartAnima, ToughnessPct, DotPct,
            HealingPct, ShieldPct, StartResonance, ExtraBlackSalt, ExtraBeltSlots, BreakVulnerability, SalinityGainPct,
            ResonanceGainPct, SparkResonanceCap,
            EnemyHpPct, EnemyAtkPct, EnemySpeedPct, EnemyToughnessPct,
            DelayDamageCoef, BurningTargetDamage, BleedingTargetCrit, StartShieldPct, StartAdvance, EndTurnHealPct,
            AlmitasPct, GraftChoices, SantuarioHealPct, SaviaPct,
        };

        public static bool IsKnown(string key)
        {
            if (key == null) return false;
            if (Known.Contains(key)) return true;
            if (key.StartsWith(PlanetDamagePrefix, StringComparison.Ordinal))
                return Planets.TryParse(key.Substring(PlanetDamagePrefix.Length), out _);
            return false;
        }

        public static string ChainMove(ChainRole role)
        {
            switch (role)
            {
                case ChainRole.Apertura: return ChainMoveApertura;
                case ChainRole.Enlace: return ChainMoveEnlace;
                default: return ChainMoveCierre;
            }
        }

        /// <summary>Texto corto para la interfaz y el registro.</summary>
        public static string Describe(string key, double v)
        {
            string pct = (v >= 0 ? "+" : "−") + Math.Abs(v * 100).ToString("0.#") + " %";
            string num = (v >= 0 ? "+" : "−") + Math.Abs(v).ToString("0.#");
            if (key.StartsWith(PlanetDamagePrefix, StringComparison.Ordinal) && Planets.TryParse(key.Substring(PlanetDamagePrefix.Length), out var p))
                return $"{pct} de daño a las brujas de {Planets.DisplayName(p)}";
            switch (key)
            {
                case AtkPct: return pct + " de ataque";
                case FlatAtk: return num + " de ataque";
                case HpPct: return pct + " de vida máxima";
                case SpeedFlat: return num + " de Velocidad";
                case CritRate: return pct + " de probabilidad de crítico";
                case CritDamage: return pct + " de daño crítico";
                case DamagePct: return pct + " de daño";
                case AscendantDamagePct: return pct + " de daño del Ascendente";
                case Penetration: return pct + " de penetración de resistencia";
                case DefenseDown: return (v >= 0 ? "−" : "+") + Math.Abs(v * 100).ToString("0.#") + " % de defensa";
                case AnimaGainPct: return pct + " de Ánima ganada";
                case AspectBonus: return pct + " al bono de Aspecto";
                case AreaDamagePct: return pct + " de daño en área";
                case PotionPotencyPct: return pct + " de potencia de Salmueras";
                case DelayPct: return pct + " a los retrasos que causa";
                case StartGrains: return num + " Grano de Sal al iniciar el combate";
                case SalinityPerSpell: return num + " de Salinidad por Hechizo";
                case HilosCostPct: return pct + " al costo de los Hilos";
                case FrameShift: return (v < 0 ? "golpes " + Math.Abs(v) + " frames antes" : "golpes " + v + " frames después");
                case SparkDamageBonus: return pct + " de daño de Spark";
                case ChainDamagePct: return pct + " de daño en la Cadena";
                case AscensionDamagePct: return pct + " de daño de Ascensión";
                case SpellDamagePct: return pct + " de daño de Hechizo";
                case BasicDamagePct: return pct + " de daño del ataque básico";
                case ChainMoveApertura: return pct + " a la Apertura";
                case ChainMoveEnlace: return pct + " al Enlace";
                case ChainMoveCierre: return pct + " al Cierre";
                case StartAnima: return num + " de Ánima al iniciar";
                case ToughnessPct: return pct + " de Firmeza quitada";
                case DotPct: return pct + " de daño por turno";
                case HealingPct: return pct + " de curación";
                case ShieldPct: return pct + " de escudos";
                case StartResonance: return num + " de Resonancia al iniciar";
                case ExtraBlackSalt: return num + " Sal Negra por combate";
                case ExtraBeltSlots: return num + " ranura de poción";
                case BreakVulnerability: return pct + " de daño a enemigos quebrados";
                case SalinityGainPct: return pct + " de Salinidad ganada";
                case ResonanceGainPct: return pct + " de Resonancia ganada";
                case SparkResonanceCap: return num + " al tope de Resonancia por Spark";
                case EnemyHpPct: return pct + " de vida enemiga";
                case EnemyAtkPct: return pct + " de ataque enemigo";
                case EnemySpeedPct: return pct + " de Velocidad enemiga";
                case EnemyToughnessPct: return pct + " de Firmeza enemiga";
                case DelayDamageCoef: return "cada retraso que causas hace daño igual al " + (v * 100).ToString("0.#") + " % de tu ataque";
                case BurningTargetDamage: return pct + " de daño contra enemigos con Quemadura";
                case BleedingTargetCrit: return pct + " de crítico contra enemigos con Sangrado";
                case StartShieldPct: return "escudo del " + (v * 100).ToString("0.#") + " % de la vida al iniciar";
                case StartAdvance: return "adelanta " + (v * 100).ToString("0.#") + " % a todas al iniciar";
                case EndTurnHealPct: return "cura " + (v * 100).ToString("0.#") + " % al terminar cada turno";
                case AlmitasPct: return pct + " de almitas";
                case GraftChoices: return num + " opción al elegir Injerto";
                case SantuarioHealPct: return pct + " de vida al descansar en un Santuario";
                case SaviaPct: return pct + " de Savia";
                default: return key + " " + num;
            }
        }
    }

    /// <summary>Suma de modificadores con clave (ver ModKeys). Las claves que faltan valen 0.</summary>
    public sealed class StatBlock
    {
        private readonly Dictionary<string, double> _values = new Dictionary<string, double>();
        private readonly List<string> _order = new List<string>();

        public IReadOnlyList<string> Keys => _order;
        public int Count => _order.Count;

        public double Get(string key) => key != null && _values.TryGetValue(key, out var v) ? v : 0.0;

        public bool Has(string key) => Math.Abs(Get(key)) > 1e-12;

        public void Add(string key, double value)
        {
            if (value == 0 || key == null) return;
            if (_values.TryGetValue(key, out var old)) _values[key] = old + value;
            else { _values[key] = value; _order.Add(key); }
        }

        public void Set(string key, double value)
        {
            if (!_values.ContainsKey(key)) _order.Add(key);
            _values[key] = value;
        }

        public void AddAll(StatBlock other, double scale = 1.0)
        {
            if (other == null) return;
            foreach (var k in other._order) Add(k, other._values[k] * scale);
        }

        public StatBlock Clone()
        {
            var c = new StatBlock();
            c.AddAll(this);
            return c;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            foreach (var k in _order) parts.Add(ModKeys.Describe(k, _values[k]));
            return string.Join("; ", parts);
        }
    }

    /// <summary>Golpe extra que un arma agrega a cada golpe de los movimientos de Cadena (familias de muchos golpes).</summary>
    public sealed class ChainEcho
    {
        public int Offset;
        public double Share;

        public ChainEcho() { }
        public ChainEcho(int offset, double share) { Offset = offset; Share = share; }
    }

    /// <summary>
    /// Lo que la Fase 2 cambia de una bruja concreta en un combate: nivel, estadísticas por nivel y grado, arma,
    /// prefijo, Dignidad y Talentos de Cadena. Lo arma el progreso (Core.Progress.LoadoutBuilder).
    /// </summary>
    public sealed class WitchLoadout
    {
        /// <summary>Nivel de la bruja (entra en la fórmula de daño). Null = el del JSON.</summary>
        public int? Level;
        /// <summary>Multiplicadores de nivel y grado sobre las estadísticas del JSON.</summary>
        public double AtkMultiplier = 1.0;
        public double HpMultiplier = 1.0;
        public double SpeedMultiplier = 1.0;
        /// <summary>Patrón de golpes del ataque básico según la familia de arma. Null = el del kit.</summary>
        public List<HitDef> BasicHits;
        /// <summary>Golpes extra de la familia en la Cadena (relativos a cada golpe).</summary>
        public List<ChainEcho> ChainEchoes = new List<ChainEcho>();
        public string WeaponName = "";
        public StatBlock Mods = new StatBlock();

        public WitchLoadout Clone()
        {
            var c = (WitchLoadout)MemberwiseClone();
            c.BasicHits = BasicHits != null ? new List<HitDef>(BasicHits) : null;
            c.ChainEchoes = new List<ChainEcho>(ChainEchoes);
            c.Mods = Mods.Clone();
            return c;
        }
    }
}
