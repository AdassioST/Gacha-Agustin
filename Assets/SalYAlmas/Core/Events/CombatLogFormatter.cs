using System.Collections.Generic;
using System.Globalization;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Events
{
    /// <summary>
    /// Convierte eventos en líneas de registro estilo LitRPG, por ejemplo:
    /// "[Quiebre] Marisal quiebra al Guardián: Sangrado fuerte."
    /// Devuelve null para eventos que no merecen línea propia (cambios de recursos).
    /// </summary>
    public sealed class CombatLogFormatter
    {
        private readonly Dictionary<string, string> _articles = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _rituals = new Dictionary<string, string>();

        /// <summary>Si es true, también escribe una línea por cada golpe.</summary>
        public bool IncludeHits = true;

        public CombatLogFormatter(GameDatabase db = null)
        {
            if (db == null) return;
            foreach (var e in db.Enemies.Values)
            {
                string name = string.IsNullOrEmpty(e.ShortName) ? e.Name : e.ShortName;
                _articles[name] = e.Article ?? "";
            }
        }

        private static string N(int v) => v.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ');

        /// <summary>"a" + nombre con su artículo: "al Guardián", "a la Larva", "a Marisal".</summary>
        private string ToName(string name)
        {
            if (_articles.TryGetValue(name ?? "", out var art))
            {
                if (art == "el") return "al " + name;
                if (art.Length > 0) return "a " + art + " " + name;
            }
            return "a " + name;
        }

        private string TheName(string name)
        {
            if (_articles.TryGetValue(name ?? "", out var art) && art.Length > 0)
                return char.ToUpperInvariant(art[0]) + art.Substring(1) + " " + name;
            return name;
        }

        private static string Lower(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

        private static string Roman(int n) => n == 1 ? "I" : n == 2 ? "II" : "III";

        private static string RoleName(ChainRole r) => r == ChainRole.Apertura ? "Apertura" : r == ChainRole.Enlace ? "Enlace" : "Cierre";

        public string Format(BattleEvent ev)
        {
            switch (ev)
            {
                case BattleStarted e:
                {
                    string guest = string.IsNullOrEmpty(e.Guest) ? "" : $" · Invitada: {e.Guest} (Regencia: {e.Regencia})";
                    return $"[Libro Mayor] Comienza el combate: {e.EncounterName}. Aquelarre: {string.Join(", ", e.Team)}{guest}.";
                }
                case CycleStarted e:
                    return $"[Ciclo {e.Cycle}] Avanza la rueda (AV {e.ElapsedAv:0}).";
                case TurnStarted e:
                    return e.IsExtra ? $"[Turno {e.TurnNumber}] {e.UnitName} actúa de nuevo, movida por sus Hilos." : $"[Turno {e.TurnNumber}] {TheName(e.UnitName)}.";
                case ActionUsed e:
                    return FormatAction(e);
                case HitLanded e:
                {
                    if (!IncludeHits) return null;
                    string spark = e.IsSpark ? " ✦ Spark" : "";
                    string crit = e.IsCrit ? " ¡Crítico!" : "";
                    string shield = e.ShieldAbsorbed > 0 ? $" (escudo {N(e.ShieldAbsorbed)})" : "";
                    string tough = e.ToughnessDamage > 0 ? $" · Firmeza −{e.ToughnessDamage:0.#}" : "";
                    string frame = e.Source == HitSource.Chain || e.Source == HitSource.Reaction ? $" [f{e.Frame}]" : "";
                    return $"[Golpe] {e.AttackerName} → {e.TargetName}: {N(e.Damage)}{crit}{spark}{shield}{tough}{frame}";
                }
                case AspectFormed e:
                {
                    string reaction = e.Reaction != HybridReaction.None ? $" → {AspectRules.DisplayName(e.Reaction)}" : "";
                    return $"[Aspecto] {e.FirstName} ({Planets.DisplayName(e.FirstPlanet)}) y {e.SecondName} ({Planets.DisplayName(e.SecondPlanet)}) forman {AspectRules.DisplayName(e.Aspect)}{reaction}: {AspectRules.EffectSummary(e.Aspect, e.Reaction)}.";
                }
                case ReactionTriggered e:
                    return e.TargetId >= 0
                        ? $"[Reacción] {AspectRules.DisplayName(e.Reaction)}: {TheName(e.TargetName)} {e.Detail}."
                        : $"[Reacción] {AspectRules.DisplayName(e.Reaction)}: {e.Detail}.";
                case BreakTriggered e:
                    return $"[Quiebre] {e.AttackerName} quiebra {ToName(e.TargetName)}: {e.EffectName}.";
                case ToughnessRestored e:
                    return $"[Firmeza] {TheName(e.UnitName)} recupera su Firmeza.";
                case SalinityBandChanged e:
                    return $"[Salinidad] {e.WitchName} pasa a {SalinityBands.DisplayName(e.NewBand)} ({e.Value}/150): {SalinityBands.EffectSummary(e.NewBand)}.";
                case EcstasyStarted e:
                    return $"[Éxtasis Abisal] ¡El Ascendente de {e.WitchName} toma el control!";
                case EcstasyEnded e:
                    return $"[Éxtasis Abisal] {e.WitchName} vuelve en sí, Desalada (Salinidad {e.SalinityAfter}).";
                case StatusApplied e:
                    if (e.Status == StatusKind.Ecstasy) return null;
                    return e.Turns > 0
                        ? $"[Estado] {TheName(e.UnitName)}: {e.StatusName} ({e.Turns} {(e.Turns == 1 ? "turno" : "turnos")})."
                        : $"[Estado] {TheName(e.UnitName)}: {e.StatusName}.";
                case StatusRemoved e:
                    return e.Reason == "terminó" ? null : $"[Estado] {TheName(e.UnitName)} pierde {e.StatusName} ({e.Reason}).";
                case DotTicked e:
                    return $"[{e.StatusName}] {TheName(e.UnitName)} pierde {N(e.Damage)} de vida.";
                case Healed e:
                    return e.Inverted
                        ? $"[Curas invertidas] {e.UnitName} pierde {N(-e.Amount)} ({e.Reason})."
                        : $"[Cura] {e.UnitName} recupera {N(e.Amount)} ({e.Reason}).";
                case ShieldGained e:
                    return $"[Escudo] {e.UnitName} gana un escudo de {N(e.Amount)}.";
                case ActionShifted e:
                {
                    string verb = e.Percent > 0 ? "adelanta" : "se retrasa";
                    return $"[Línea de turnos] {TheName(e.UnitName)} {verb} {System.Math.Abs(e.Percent):P0} ({e.Reason}).";
                }
                case TurnSkipped e:
                    return $"[{e.Reason}] {TheName(e.UnitName)} pierde su turno.";
                case AttackCanceled e:
                    return $"[Cancelado] {e.SkillName} {DeName(e.UnitName)} se deshace: {e.Reason}.";
                case PhaseChanged e:
                    return $"[Fase] {TheName(e.UnitName)}: {e.Text}";
                case UnitDefeated e:
                    return e.Side == Side.Enemy ? $"[Caída] {TheName(e.UnitName)} se deshace en sal." : $"[Caída] {e.UnitName} cae.";
                case ChainStarted e:
                {
                    string order = string.Join(" → ", e.Names);
                    string guest = e.WithGuest ? " (cierra la Invitada)" : "";
                    return $"[Cadena] La Calculista ordena en silencio: {order}{guest}. Spark previstos: {e.PredictedSparks}.";
                }
                case ChainLinkStarted e:
                    return $"[{RoleName(e.Role)}] {e.WitchName}: {e.MoveName} [f{e.Frame}]";
                case ChainEnded e:
                    return $"[Cadena] Fin: {e.Sparks} Spark, {N(e.TotalDamage)} de daño, +{e.Almitas} almitas, +{e.ResonanceFromSparks} de Resonancia por Spark.";
                case AscensionCast e:
                    return $"[Ascensión {Roman(e.Level)}] {e.WitchName} canta: {e.MoveName} (−{e.AnimaSpent} Ánima).";
                case BattleEnded e:
                    return e.Outcome == BattleOutcome.Victory
                        ? $"[Fin] ¡Victoria! {e.Turns} turnos, {e.Sparks} Spark, {e.Almitas} almitas."
                        : $"[Fin] Derrota: {e.Reason}.";
                default:
                    return null;
            }
        }

        private string DeName(string name)
        {
            if (_articles.TryGetValue(name ?? "", out var art))
            {
                if (art == "el") return "del " + name;
                if (art.Length > 0) return "de " + art + " " + name;
            }
            return "de " + name;
        }

        private string FormatAction(ActionUsed e)
        {
            string target = string.IsNullOrEmpty(e.TargetName) ? "" : $" → {e.TargetName}";
            switch (e.Action)
            {
                case ActionKind.Basic:
                    return $"[Básico] {e.ActorName}: {e.MoveName}{target}.";
                case ActionKind.Spell:
                    return string.IsNullOrEmpty(e.Detail)
                        ? $"[Hechizo] {e.ActorName}: {e.MoveName}{target}."
                        : $"[Hechizo] {e.ActorName} {Lower(e.Detail)}: {e.MoveName}{target}.";
                case ActionKind.SelfCast:
                    return $"[Autohechizo] El Ascendente de {e.ActorName} la hechiza: {e.MoveName} ({e.Detail}).";
                case ActionKind.Vislumbre:
                    return $"[{e.MoveName}] {e.ActorName} se funde con su Ascendente ({e.Detail}).";
                case ActionKind.Potion:
                    return $"[Salmuera] {e.ActorName} usa {e.MoveName}{(e.TargetName == e.ActorName ? "" : " en " + e.TargetName)}.";
                case ActionKind.Ecstasy:
                    return $"[Éxtasis Abisal] El Ascendente de {e.ActorName} desata {e.MoveName}{target} (daño ×2).";
                case ActionKind.EnemyCharge:
                    return $"[Amenaza] {TheName(e.ActorName)} prepara {e.MoveName}. {e.Detail}";
                case ActionKind.EnemySkill:
                    return $"[Enemigo] {TheName(e.ActorName)} usa {e.MoveName}{target}.";
                default:
                    return $"[{e.Action}] {e.ActorName}: {e.MoveName}{target}.";
            }
        }
    }
}
