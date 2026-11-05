using System.Collections.Generic;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Data
{
    /// <summary>Un golpe de un movimiento: su frame (60 fps) y su parte del multiplicador total.</summary>
    public sealed class HitDef
    {
        public int Frame;
        public double Share;

        public HitDef() { }
        public HitDef(int frame, double share) { Frame = frame; Share = share; }
    }

    public enum MoveKind { Basic, Spell, Opening, Link, Closing, Ascension, EnemySkill }

    public enum TargetMode { Enemy, AllEnemies, Ally, AllAllies, Self, None }

    public enum ChainRole { Apertura, Enlace, Cierre }

    public enum EffectTiming { Start, End }

    public enum EffectType
    {
        Grains,          // Value: Granos (se duplica con la Excavación del Topo)
        Advance,         // Value: % de 10 000 que se adelanta
        Delay,           // Value: % de 10 000 que se retrasa
        Buff,            // Stat, Value, Turns
        Debuff,          // Stat, Value, Turns
        Dot,             // Dot, Value (coef. de ATQ), Turns
        Shield,          // Value: % de vida máxima, Turns
        Heal,            // Value: % de vida máxima
        Salinity,        // Value: puntos
        Anima,           // Value: puntos
        Resonance,       // Value: puntos
        GrainBoost,      // Turns: duplica la producción de Granos (Topo)
        FeignDeath,      // Muerte Fingida (Zarigüeya): cancela el siguiente ataque definitivo, una vez por combate
        Cleanse,         // Count: estados negativos que limpia
        Dispel,          // Count: mejoras que quita
        NextSpellDouble, // Elixir de Sal Negra
        InvertHealing,   // Turns
        FreeVislumbre,   // Filtro de Pureza
        NextToughness,   // Value: +% de Firmeza a la siguiente acción con golpes
        Paralaje,        // Fuera de combate (Agua de Olvido)
        Taunt,           // Turns: los enemigos la atacan a ella (Guardianas)
        MaxHpBurst,      // Value: % de la vida máxima del objetivo. Consume la intoxicación propia (Salitre)
        StealGrains,     // Value: Granos que se roba (enemigos)
    }

    public enum EffectTarget { Target, Self, AllAllies, OtherAllies, AllEnemies }

    public enum StatKind { Atk, Damage, Speed, SpeedFlat, CritRate, Res, DefenseDown, DamageTaken }

    public enum DotKind { Burn, Bleed, Poison, Shock }

    public sealed class EffectDef
    {
        public EffectType Type;
        public EffectTarget Target = EffectTarget.Target;
        public EffectTiming At = EffectTiming.End;
        public StatKind Stat;
        public DotKind Dot;
        public double Value;
        public int Turns;
        public int Count;
        public string Name;
    }

    public sealed class MoveDef
    {
        public string Id;
        public string Name;
        public MoveKind Kind;
        public TargetMode Target = TargetMode.Enemy;
        /// <summary>M_hab total del movimiento; se reparte entre los golpes según Share.</summary>
        public double Multiplier;
        /// <summary>Firmeza total que quita si el planeta es débil; se reparte entre los golpes.</summary>
        public double Toughness;
        /// <summary>Lo ejecuta el Ascendente (cuenta para el +40 % de la Cristalización Turbia).</summary>
        public bool ByAscendant;
        public List<HitDef> Hits = new List<HitDef>();
        public List<EffectDef> Effects = new List<EffectDef>();
        public string Description = "";

        public int LastHitFrame
        {
            get
            {
                int max = 0;
                foreach (var h in Hits) if (h.Frame > max) max = h.Frame;
                return max;
            }
        }
    }

    public enum SelfCastForm { None, AgujaYSello, Hilos, FrenesiSalNegra }

    /// <summary>Habilidad de líder que aporta la Invitada durante todo el combate (GDD 9.1).</summary>
    public sealed class RegenciaDef
    {
        public string Name = "";
        public string Description = "";
        public double TeamAtk;
        public double TeamSpeedFlat;
        public double TeamHp;
        public int StartGrains;
        public int StartResonance;
        public double StartEnemyDelay;
    }

    public sealed class WitchDef
    {
        public string Id;
        public string Name;
        /// <summary>Nombre corto para el registro y la interfaz ("Marisal").</summary>
        public string ShortName;
        public string Species = "";
        public int Rarity;
        public string Role = "";
        public string AscendantSign = "";
        public string AscendantName = "";
        public string Ritual = "";
        public Planet Primary;
        public Planet Secondary;
        /// <summary>Nombre de la mezcla turbia (GDD 7.2), por ejemplo "Escarcha Sangrienta".</summary>
        public string MixName = "";
        public Via Via = Via.Mayor;
        public Grade Grade = Grade.B;
        public int Level;
        public int MaxHp;
        public double Atk;
        public double Speed;
        public double CritRate;
        public double CritDamage;
        /// <summary>Multiplicador fijo de ATQ por una pasiva (Zarigüeya: su Ascendente se hace el muerto).</summary>
        public double PassiveAtkMultiplier = 1.0;
        public string PassiveName = "";
        public string PassiveDescription = "";
        /// <summary>Topo: sus golpes no hacen daño (sí quitan Firmeza y sí cuentan para el Spark).</summary>
        public bool DealsNoDamage;
        /// <summary>Axa: al caer revive una vez con este % de vida y la Salinidad en cero.</summary>
        public double ReviveOnceHp;
        /// <summary>Salitre: se intoxica al empezar el combate. Coeficiente del veneno sobre su ATQ.</summary>
        public double StartPoison;
        public SelfCastForm SelfCast;
        public string SelfCastName = "";
        public MoveDef Basic;
        public MoveDef Spell;
        public MoveDef Opening;
        public MoveDef Link;
        public MoveDef Closing;
        /// <summary>Ascensión I, II y III.</summary>
        public MoveDef[] Ascensions = new MoveDef[3];
        public RegenciaDef Regencia = new RegenciaDef();

        public Planet Dominant => Via == Via.Mayor ? Primary : Secondary;
        public Planet Recessive => Via == Via.Mayor ? Secondary : Primary;

        public MoveDef MoveFor(ChainRole role)
        {
            switch (role)
            {
                case ChainRole.Apertura: return Opening;
                case ChainRole.Enlace: return Link;
                default: return Closing;
            }
        }
    }

    public enum EnemyTargetRule { Random, LowestHp, HighestSalinity, All, Self }

    /// <summary>
    /// Mecánicas que vuelven distinto a un enemigo. Se combinan; las declara enemies.json o fresno_enemies.json.
    /// Con todo en cero, el enemigo se comporta como en la Fase 1.
    /// </summary>
    public sealed class EnemyTraits
    {
        /// <summary>Devuelve este % del daño directo a quien lo golpea (Espejos).</summary>
        public double ReflectPct;
        /// <summary>Los Sin Carta: los Aspectos no le afectan (GDD 6.5).</summary>
        public bool AspectImmune;
        /// <summary>Mientras viva, sus compañeros reciben este % menos de daño (hay que matarlo primero).</summary>
        public double ProtectAllies;
        /// <summary>Cuando alguien le pega con un Hechizo, gana este % de ataque por 2 turnos.</summary>
        public double SpellEnrage;
        /// <summary>Gana este % de ataque al final de cada turno que sigue viva (Soldada de Crisol).</summary>
        public double RampAtk;
        /// <summary>Al actuar, cura este % de su vida máxima a la aliada con menos vida (Novicia sin Alma).</summary>
        public double HealAllies;
        /// <summary>Efectos que suelta al caer. Golpean a las brujas aunque ya haya muerto.</summary>
        public readonly List<EffectDef> OnDeath = new List<EffectDef>();
        /// <summary>Forma de la silueta de prueba: cristal, bestia, ave, masa, torre, cangrejo, ojo, espejo, sombra, flor.</summary>
        public string Shape = "";
        /// <summary>Texto para la placa: qué hace y cómo se le gana.</summary>
        public string Hint = "";
    }

    public sealed class EnemySkillDef
    {
        public string Id;
        public string Name;
        public EnemyTargetRule TargetRule;
        public double Multiplier;
        public List<HitDef> Hits = new List<HitDef>();
        public List<EffectDef> Effects = new List<EffectDef>();
        /// <summary>Ataque definitivo: lo cancela la Muerte Fingida.</summary>
        public bool IsUltimate;
        /// <summary>Pasa un turno cargando antes de soltarlo.</summary>
        public bool Charge;
        public string ChargeText = "";
        public string Description = "";
    }

    public sealed class EnemyPhaseDef
    {
        public double HpBelow;
        public List<string> Pattern = new List<string>();
        public double SpeedBonus;
        public string Text = "";
    }

    public sealed class EnemyDef
    {
        public string Id;
        public string Name;
        /// <summary>Nombre corto para el registro ("Guardián").</summary>
        public string ShortName;
        /// <summary>Artículo para el registro: "el", "la" o vacío ("quiebra al Guardián").</summary>
        public string Article = "";
        public string Description = "";
        public int Level;
        public int MaxHp;
        public double Atk;
        public double Speed;
        public double Toughness;
        public List<Planet> Weaknesses = new List<Planet>();
        public Dictionary<Planet, double> Res = new Dictionary<Planet, double>();
        public Planet AttackPlanet;
        public bool IsBoss;
        /// <summary>Escala en el escenario (presentación). Sin dato: Medium, o Large si es jefe.</summary>
        public EnemySize Size = EnemySize.Medium;
        public EnemyTraits Traits = new EnemyTraits();
        /// <summary>Acciones por turno. Los jefes pueden actuar dos veces para no quedar abrumados por cuatro brujas.</summary>
        public int ActionsPerTurn = 1;
        public List<EnemySkillDef> Skills = new List<EnemySkillDef>();
        public List<string> Pattern = new List<string>();
        public EnemyPhaseDef Phase2;

        public EnemySkillDef Skill(string id)
        {
            foreach (var s in Skills) if (s.Id == id) return s;
            return null;
        }

        public double ResFor(Planet p) => Res.TryGetValue(p, out var v) ? v : 0.0;
    }

    public sealed class EncounterDef
    {
        public string Id;
        public string Name;
        public string Description = "";
        public List<string> Enemies = new List<string>();
    }

    public enum PotionTarget { Ally, AllAllies, Enemy, None }

    /// <summary>
    /// Tamaño de un enemigo en el escenario, de una gota de obsidiana a un cangrejo con un pueblo en el caparazón.
    /// Solo lo usa la presentación para la escala y el encuadre; no cambia ninguna regla.
    /// </summary>
    public enum EnemySize { Small, Medium, Large, Giant, Colossal }

    public sealed class PotionDef
    {
        public string Id;
        public string Name;
        public string Ingredients = "";
        public string Description = "";
        public PotionTarget Target;
        public bool UsableInCombat = true;
        public List<EffectDef> Effects = new List<EffectDef>();
    }
}
