using System;

namespace SalYAlmas.Core.Data
{
    /// <summary>Marca un valor que viene literal del GDD. Las pruebas comprueban que rules.json no lo cambie.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GddAttribute : Attribute
    {
        public string Section { get; }
        public GddAttribute(string section) { Section = section; }
    }

    /// <summary>Marca un valor propuesto que el GDD no fija (// TUNING).</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TuningAttribute : Attribute { }

    /// <summary>
    /// Todas las reglas numéricas del combate. Los valores por defecto son los del GDD y las propuestas
    /// marcadas con [Tuning]. Data/rules.json puede sobrescribirlos (ver RulesLoader).
    /// </summary>
    public sealed class CombatRules
    {
        public TurnOrderRules TurnOrder = new TurnOrderRules();
        public GrainRules Grains = new GrainRules();
        public SalinityRules Salinity = new SalinityRules();
        public PurityRules Purity = new PurityRules();
        public AnimaRules Anima = new AnimaRules();
        public ResonanceRules Resonance = new ResonanceRules();
        public ChainRules Chain = new ChainRules();
        public AspectTuning Aspects = new AspectTuning();
        public ReactionRules Reactions = new ReactionRules();
        public BreakRules Break = new BreakRules();
        public SelfCastRules SelfCast = new SelfCastRules();
        public VislumbreRules Vislumbre = new VislumbreRules();
        public MiscCombatRules Combat = new MiscCombatRules();
        public PotionRules Potions = new PotionRules();
    }

    public sealed class TurnOrderRules
    {
        [Gdd("9.2")] public double AvBase = 10000;
        [Gdd("9.2")] public double FirstCycleAv = 150;
        [Gdd("9.2")] public double CycleAv = 100;
        /// <summary>Dos AV a menos de esta distancia se consideran empate.</summary>
        public double TieEpsilon = 1e-6;
    }

    public sealed class GrainRules
    {
        [Gdd("9.3")] public int Max = 5;
        [Gdd("9.3")] public int Start = 3;
        [Gdd("9.3")] public int BasicGain = 1;
        [Gdd("9.3")] public int SpellCost = 1;
    }

    public sealed class SalinityRules
    {
        [Gdd("7.4")] public int Max = 150;
        [Gdd("7.4")] public int Spell = 10;
        [Gdd("7.4")] public int SelfCast = 25;
        [Gdd("7.4")] public int BlackSalt = 40;
        [Gdd("7.4")] public int Vislumbre = 30;
        [Gdd("7.4")] public int Basic = -5;
        [Gdd("7.4")] public int TurnWithoutMagic = -10;
        [Gdd("9.5")] public int ChainLink = 5;
        [Gdd("7.4")] public int SalobreAt = 40;
        [Gdd("7.4")] public int SobresaladaAt = 70;
        [Gdd("7.4")] public int CristalizacionAt = 100;
        [Gdd("7.4")] public int ExtasisAt = 150;
        [Gdd("7.4")] public double SalobreCritRate = 0.10;
        [Gdd("7.4")] public double SobresaladaDamage = 0.20;
        [Gdd("7.4")] public double SobresaladaDefenseDown = 0.15;
        [Gdd("7.4")] public double CristalizacionAscendantDamage = 0.40;
        [Gdd("7.4")] public double CristalizacionHpLoss = 0.05;
        [Gdd("7.4")] public int ExtasisTurns = 2;
        [Gdd("7.4")] public double ExtasisDamageMultiplier = 2.0;
        [Gdd("7.4")] public int ExtasisResetTo = 60;
        [Gdd("7.4")] public int DesaladaTurns = 1;
        /// <summary>Si la pérdida de vida de la Cristalización puede matar. Propuesta: no (deja 1 de vida).</summary>
        [Tuning] public bool CristalizacionIsLethal = false;
        /// <summary>Salinidad que suma una Ascensión. El GDD no lo dice.</summary>
        [Tuning] public int Ascension = 0;
    }

    public sealed class PurityRules
    {
        [Gdd("12.3")] public double D = 0.70;
        [Gdd("12.3")] public double C = 0.78;
        [Gdd("12.3")] public double B = 0.85;
        [Gdd("12.3")] public double A = 0.92;
        [Gdd("12.3")] public double S = 0.97;
        [Gdd("12.3")] public double SS = 0.999;
        [Gdd("7.2")] public double DominantSlope = 0.8;
        [Gdd("7.2")] public double Pivot = 0.7;
        /// <summary>Multiplicador de estadísticas por grado, relativo a D. Lo usa el Vislumbre.</summary>
        [Tuning] public double[] GradeStatMultiplier = { 1.00, 1.05, 1.10, 1.16, 1.22, 1.28 };

        public double PurityOf(Magic.Grade g)
        {
            switch (g)
            {
                case Magic.Grade.D: return D;
                case Magic.Grade.C: return C;
                case Magic.Grade.B: return B;
                case Magic.Grade.A: return A;
                case Magic.Grade.S: return S;
                default: return SS;
            }
        }
    }

    public sealed class AnimaRules
    {
        [Gdd("9.7")] public int Max = 300;
        [Gdd("9.3")] public int Basic = 20;
        [Gdd("9.3")] public int Spell = 30;
        [Gdd("9.6")] public int Spark = 2;
        [Gdd("9.7")] public int Level1Cost = 100;
        [Gdd("9.7")] public int Level2Cost = 200;
        [Gdd("9.7")] public int Level3Cost = 300;
        /// <summary>Ánima por eslabón de Cadena ("se llena al actuar").</summary>
        [Tuning] public int ChainLink = 10;
        /// <summary>Ánima por cada golpe recibido ("se llena al recibir daño").</summary>
        [Tuning] public int HitTaken = 8;
        /// <summary>Ánima por un turno de Éxtasis.</summary>
        [Tuning] public int Ecstasy = 30;
    }

    public sealed class ResonanceRules
    {
        [Gdd("9.5")] public int Max = 100;
        [Gdd("9.5")] public int Basic = 4;
        [Gdd("9.5")] public int Spell = 8;
        [Gdd("9.5")] public int Break = 15;
        [Gdd("9.5")] public int Spark = 10;
        [Gdd("9.6")] public int SparkCapPerChain = 20;
        [Gdd("9.3")] public int ChainCost = 100;
        /// <summary>Resonancia por un turno de Éxtasis (igual que un Hechizo).</summary>
        [Tuning] public int Ecstasy = 8;
    }

    public sealed class ChainRules
    {
        [Gdd("9.5")] public int MinWitches = 3;
        [Gdd("9.5")] public int MaxWitches = 4;
        [Gdd("9.5")] public int LinkSpacingFrames = 24;
        [Gdd("9.6")] public int SparkWindowFrames = 2;
        [Gdd("9.6")] public double SparkDamageBonus = 0.5;
        [Gdd("9.6")] public int SparkAlmitas = 1;
        /// <summary>1.8 s a 60 fps: el último golpe de un movimiento de Cadena.</summary>
        [Gdd("9.5")] public int MaxLinkFrames = 108;
        /// <summary>6 s a 60 fps: la Cadena completa.</summary>
        [Gdd("9.5")] public int MaxChainFrames = 360;
        /// <summary>Desfase de la segunda Ascensión cuando dos se lanzan encimadas (GDD 9.6).</summary>
        [Tuning] public int AscensionOverlayOffsetFrames = 0;
    }

    public sealed class AspectTuning
    {
        [Gdd("7.3")] public double ConjunctionDamage = 0.25;
        [Gdd("7.3")] public int OppositionSalinity = 20;
        [Gdd("7.3")] public double SextileAdvance = 0.15;
        [Gdd("7.3")] public int TrineGrains = 1;
        [Gdd("7.3")] public double TrineHeal = 0.05;
        [Gdd("7.3")] public double SquareToughness = 0.40;
        [Gdd("7.3")] public int SquareSalinity = 10;
    }

    public sealed class ReactionRules
    {
        [Gdd("7.3")] public int EclipseDispel = 2;
        [Gdd("7.3")] public double EclipseDelay = 0.30;
        [Gdd("7.3")] public double HeladaBossSpeedDown = 0.50;
        [Gdd("7.3")] public int HeladaBossTurns = 1;
        [Gdd("7.3")] public double EspinasHealShare = 0.30;
        [Gdd("7.3")] public int TempestadExtraHits = 2;
        [Tuning] public double EspinasBleedCoef = 0.6;
        [Tuning] public int EspinasTurns = 2;
        /// <summary>Frames de los golpes extra de la Tempestad, relativos al inicio del eslabón que la provoca.</summary>
        [Tuning] public int[] TempestadHitFrames = { 10, 24 };
        [Tuning] public double TempestadCoef = 0.5;
        [Tuning] public double TempestadToughness = 10;
    }

    public sealed class BreakRules
    {
        [Gdd("9.4")] public double Delay = 0.25;
        [Gdd("9.4")] public double Vulnerability = 0.10;
        [Tuning] public int DotTurns = 2;
        [Tuning] public double SolBurnCoef = 0.5;
        [Tuning] public double LunaExtraDelay = 0.20;
        [Tuning] public double LunaSpeedDown = 0.10;
        [Tuning] public double MarteBleedCoef = 0.8;
        [Tuning] public double MercurioPoisonCoef = 0.4;
        [Tuning] public double MercurioExtraDelay = 0.15;
        [Tuning] public double VenusCharmDamageDown = 0.40;
        [Tuning] public double JupiterShockCoef = 0.4;
        [Tuning] public double JupiterJumpShare = 0.5;
        [Tuning] public double SaturnoExtraDelay = 0.35;
        /// <summary>Quebrar a un enemigo que carga un ataque definitivo se lo cancela.</summary>
        [Tuning] public bool CancelsCharge = true;
    }

    public sealed class SelfCastRules
    {
        [Gdd("8.2")] public double AgujaAtk = 0.30;
        [Gdd("8.2")] public int AgujaTurns = 2;
        [Gdd("8.2")] public int AgujaGrains = 2;
        [Gdd("8.2")] public double HilosHpCost = 0.10;
        [Gdd("8.2")] public double FrenesiDamage = 1.0;
        [Gdd("8.2")] public double FrenesiDefenseDown = 0.5;
        [Gdd("8.2")] public int FrenesiTurns = 1;
        /// <summary>El Frenesí "traga Sal Negra": consume una del inventario del combate.</summary>
        [Tuning] public int FrenesiBlackSalt = 1;
    }

    public sealed class VislumbreRules
    {
        [Gdd("8.2")] public int Turns = 2;
        [Gdd("8.2")] public int BlackSaltCost = 1;
        /// <summary>El grado A "mejora el Enlace y el Cierre" (GDD 12.3): bono al multiplicador.</summary>
        [Tuning] public double ChainMoveBonus = 0.15;
        [Tuning] public double ApoteosisDamage = 0.10;
    }

    public sealed class MiscCombatRules
    {
        [Tuning] public double BaseCritRate = 0.05;
        [Tuning] public double BaseCritDamage = 0.50;
        [Tuning] public double EnemyCritRate = 0.0;
        [Tuning] public int StartingBlackSalt = 2;
        [Tuning] public int MaxTurns = 400;
        [Gdd("7.7")] public double PlanetaryDayDamage = 0.15;
    }

    public sealed class PotionRules
    {
        [Gdd("14")] public int BeltSlots = 3;
        [Gdd("14")] public int PerTurn = 1;
        [Gdd("14.3")] public int MaxPerType = 5;
    }
}
