using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Events
{
    /// <summary>
    /// Evento del combate. El núcleo los emite en orden y la presentación solo los reproduce (GDD 21.3).
    /// </summary>
    public abstract class BattleEvent
    {
        /// <summary>Posición en la lista de eventos del combate.</summary>
        public int Index { get; internal set; }

        public string Kind => GetType().Name;

        private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>Serialización estable (cultura invariante, orden de declaración). La usan las pruebas de determinismo.</summary>
        public string Serialize()
        {
            var t = GetType();
            PropertyInfo[] props;
            lock (PropertyCache)
            {
                if (!PropertyCache.TryGetValue(t, out props))
                {
                    props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.Name != nameof(Kind))
                        .OrderBy(p => p.MetadataToken)
                        .ToArray();
                    PropertyCache[t] = props;
                }
            }
            var sb = new StringBuilder(t.Name);
            foreach (var p in props)
            {
                sb.Append(' ').Append(p.Name).Append('=');
                AppendValue(sb, p.GetValue(this));
            }
            return sb.ToString();
        }

        private static void AppendValue(StringBuilder sb, object v)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case string s: sb.Append('"').Append(s).Append('"'); break;
                case double d: sb.Append(d.ToString("0.######", CultureInfo.InvariantCulture)); break;
                case float f: sb.Append(f.ToString("0.######", CultureInfo.InvariantCulture)); break;
                case IFormattable fmt: sb.Append(fmt.ToString(null, CultureInfo.InvariantCulture)); break;
                case IEnumerable e:
                    sb.Append('[');
                    bool first = true;
                    foreach (var item in e)
                    {
                        if (!first) sb.Append(',');
                        AppendValue(sb, item);
                        first = false;
                    }
                    sb.Append(']');
                    break;
                default: sb.Append(v); break;
            }
        }

        public override string ToString() => Serialize();
    }

    public enum ActionKind
    {
        Basic, Spell, SelfCast, Vislumbre, Potion, ChainLink, Ascension, Ecstasy, EnemySkill, EnemyCharge
    }

    public enum HitSource { Action, Chain, Ascension, Reaction, Ecstasy, Enemy }

    public enum BattleOutcome { None, Victory, Defeat }

    public enum BreakEffect { Quemadura, Encierro, SangradoFuerte, VenenoYRetraso, Encanto, Electrocucion, Gravedad }

    public sealed class BattleStarted : BattleEvent
    {
        public ulong Seed { get; internal set; }
        public string EncounterName { get; internal set; }
        public List<string> Team { get; internal set; }
        public string Guest { get; internal set; }
        public List<string> Enemies { get; internal set; }
        public string Regencia { get; internal set; }
    }

    public sealed class CycleStarted : BattleEvent
    {
        public int Cycle { get; internal set; }
        public double ElapsedAv { get; internal set; }
    }

    public sealed class TurnStarted : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public Side Side { get; internal set; }
        public int TurnNumber { get; internal set; }
        public int Cycle { get; internal set; }
        public double ElapsedAv { get; internal set; }
        public bool IsExtra { get; internal set; }
    }

    public sealed class ActionUsed : BattleEvent
    {
        public int ActorId { get; internal set; }
        public string ActorName { get; internal set; }
        public ActionKind Action { get; internal set; }
        public string MoveId { get; internal set; }
        public string MoveName { get; internal set; }
        public int TargetId { get; internal set; }
        public string TargetName { get; internal set; }
        public string Detail { get; internal set; }
    }

    public sealed class HitLanded : BattleEvent
    {
        public int AttackerId { get; internal set; }
        public string AttackerName { get; internal set; }
        public int TargetId { get; internal set; }
        public string TargetName { get; internal set; }
        public HitSource Source { get; internal set; }
        /// <summary>Frame del golpe (60 fps). En la Cadena es el frame absoluto desde el primer eslabón.</summary>
        public int Frame { get; internal set; }
        public int Damage { get; internal set; }
        public bool IsSpark { get; internal set; }
        public bool IsCrit { get; internal set; }
        public Planet Planet { get; internal set; }
        public int DominantDamage { get; internal set; }
        public int SecondaryDamage { get; internal set; }
        public double ToughnessDamage { get; internal set; }
        public int ShieldAbsorbed { get; internal set; }
        public int TargetHp { get; internal set; }
        public int ChainLink { get; internal set; }
    }

    public sealed class AspectFormed : BattleEvent
    {
        public int FirstId { get; internal set; }
        public string FirstName { get; internal set; }
        public int SecondId { get; internal set; }
        public string SecondName { get; internal set; }
        public Planet FirstPlanet { get; internal set; }
        public Planet SecondPlanet { get; internal set; }
        public Aspect Aspect { get; internal set; }
        public HybridReaction Reaction { get; internal set; }
        /// <summary>"Cadena" o "Ascensión".</summary>
        public string Context { get; internal set; }
        public int Frame { get; internal set; }
    }

    public sealed class ReactionTriggered : BattleEvent
    {
        public HybridReaction Reaction { get; internal set; }
        public int TargetId { get; internal set; }
        public string TargetName { get; internal set; }
        public string Detail { get; internal set; }
    }

    public sealed class BreakTriggered : BattleEvent
    {
        public int AttackerId { get; internal set; }
        public string AttackerName { get; internal set; }
        public int TargetId { get; internal set; }
        public string TargetName { get; internal set; }
        public Planet Planet { get; internal set; }
        public BreakEffect Effect { get; internal set; }
        public string EffectName { get; internal set; }
    }

    public sealed class ToughnessRestored : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
    }

    public sealed class SalinityChanged : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public int Old { get; internal set; }
        public int New { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class SalinityBandChanged : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public SalinityBand OldBand { get; internal set; }
        public SalinityBand NewBand { get; internal set; }
        public int Value { get; internal set; }
    }

    public sealed class EcstasyStarted : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
    }

    public sealed class EcstasyEnded : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public int SalinityAfter { get; internal set; }
    }

    public sealed class GrainsChanged : BattleEvent
    {
        public int Old { get; internal set; }
        public int New { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class ResonanceChanged : BattleEvent
    {
        public int Old { get; internal set; }
        public int New { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class AnimaChanged : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public int Old { get; internal set; }
        public int New { get; internal set; }
    }

    public sealed class StatusApplied : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public StatusKind Status { get; internal set; }
        public string StatusName { get; internal set; }
        public double Value { get; internal set; }
        public int Turns { get; internal set; }
        public bool IsBuff { get; internal set; }
    }

    public sealed class StatusRemoved : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public StatusKind Status { get; internal set; }
        public string StatusName { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class DotTicked : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public string StatusName { get; internal set; }
        public Planet Planet { get; internal set; }
        public int Damage { get; internal set; }
        public int Hp { get; internal set; }
    }

    public sealed class Healed : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public int Amount { get; internal set; }
        public int Hp { get; internal set; }
        public bool Inverted { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class ShieldGained : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public int Amount { get; internal set; }
    }

    public sealed class ActionShifted : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        /// <summary>Positivo = adelanta; negativo = retrasa. En fracción de 10 000.</summary>
        public double Percent { get; internal set; }
        public double NewAv { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class TurnSkipped : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class EnemyCharging : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public string SkillName { get; internal set; }
        public string Text { get; internal set; }
    }

    public sealed class AttackCanceled : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public string SkillName { get; internal set; }
        public string Reason { get; internal set; }
    }

    public sealed class PhaseChanged : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public string Text { get; internal set; }
    }

    public sealed class UnitDefeated : BattleEvent
    {
        public int UnitId { get; internal set; }
        public string UnitName { get; internal set; }
        public Side Side { get; internal set; }
    }

    public sealed class ChainStarted : BattleEvent
    {
        public int OpenerId { get; internal set; }
        public string OpenerName { get; internal set; }
        public List<int> Order { get; internal set; }
        public List<string> Names { get; internal set; }
        public bool WithGuest { get; internal set; }
        public int TargetId { get; internal set; }
        public int PredictedSparks { get; internal set; }
    }

    public sealed class ChainLinkStarted : BattleEvent
    {
        public int LinkIndex { get; internal set; }
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public ChainRole Role { get; internal set; }
        public string MoveName { get; internal set; }
        public int Frame { get; internal set; }
    }

    public sealed class ChainEnded : BattleEvent
    {
        public int Sparks { get; internal set; }
        public int TotalDamage { get; internal set; }
        public int Almitas { get; internal set; }
        public int ResonanceFromSparks { get; internal set; }
    }

    public sealed class AscensionCast : BattleEvent
    {
        public int WitchId { get; internal set; }
        public string WitchName { get; internal set; }
        public int Level { get; internal set; }
        public string MoveName { get; internal set; }
        public int AnimaSpent { get; internal set; }
        public int TargetId { get; internal set; }
    }

    public sealed class BattleEnded : BattleEvent
    {
        public BattleOutcome Outcome { get; internal set; }
        public int Turns { get; internal set; }
        public double ElapsedAv { get; internal set; }
        public int Sparks { get; internal set; }
        public int Almitas { get; internal set; }
        public string Reason { get; internal set; }
    }
}
