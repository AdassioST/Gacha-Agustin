using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public struct LogLine
    {
        public string Text;
        public Color Color;
    }

    /// <summary>Resumen de una Cadena jugada, para la pantalla final de la prueba con jugadores.</summary>
    public sealed class ChainRecord
    {
        public string Order;
        public int PredictedSparks;
        public int Sparks;
        public int Damage;
        public int BestPossibleSparks;
    }

    /// <summary>
    /// Reproduce los eventos del núcleo con su ritmo (frames de la Partitura, pausas de Quiebre y
    /// Ascensión) y lleva un "espejo" de los números que se animan. La presentación nunca decide reglas:
    /// solo manda órdenes al núcleo y consume sus eventos.
    /// </summary>
    public sealed class BattleDirector
    {
        public Battle Battle { get; }
        public GameDatabase Db { get; }
        public BattleSetup Setup { get; }
        public readonly Dictionary<int, UnitView> Views = new Dictionary<int, UnitView>();
        public readonly List<LogLine> Log = new List<LogLine>();
        public readonly List<ChainRecord> Chains = new List<ChainRecord>();
        public readonly CombatLogFormatter Formatter;
        private readonly CombatLogFormatter _hitFormatter;
        public readonly AutoPilot Pilot = new AutoPilot();

        /// <summary>Órdenes grabadas que se vuelven a jugar en vez de esperar al jugador (repetición).</summary>
        public readonly Queue<BattleCommand> Replay = new Queue<BattleCommand>();
        public bool IsReplay { get; private set; }

        public float Speed = 1f;
        public bool Auto;
        /// <summary>El jugador sostiene una carta: la pausa antes del turno enemigo espera a que suelte (Ascensión por arrastre).</summary>
        public bool HoldDecision;

        /// <summary>Se dispara cuando un evento se muestra en pantalla (después de su espera). Lo usan el sonido y otros oyentes.</summary>
        public event Action<BattleEvent> EventPresented;
        public bool ShowHits;

        // Espejo de lo que se ve en pantalla (avanza con los eventos, no con el estado final).
        public readonly Dictionary<int, int> Hp = new Dictionary<int, int>();
        public readonly Dictionary<int, double> Toughness = new Dictionary<int, double>();
        public readonly Dictionary<int, int> Salinity = new Dictionary<int, int>();
        public readonly Dictionary<int, int> Anima = new Dictionary<int, int>();
        public int Grains;
        public int Resonance;
        public int ActiveId = -1;

        // Partitura que se está reproduciendo.
        public ChainPreview PlayingChain { get; private set; }
        public int PlayheadFrame { get; private set; } = -1;
        public readonly HashSet<int> PlayedNotes = new HashSet<int>();

        public string Banner { get; private set; } = "";
        public Color BannerColor { get; private set; } = Color.white;
        public float BannerTime { get; private set; }
        public string Toast { get; private set; } = "";
        public float ToastTime { get; private set; }

        public float RealStartTime { get; }
        public float RealEndTime { get; private set; } = -1;
        public bool ResultReady => Battle.IsOver && !IsPlaying;

        private int _cursor;
        private float _wait;
        private bool _preDelayDone;
        private int _lastChainFrame;
        private float _decisionTimer;
        private ChainPreview _pendingChainPreview;
        private int _pendingBestSparks;
        private int _chainHitIndex;

        public BattleDirector(GameDatabase db, BattleSetup setup, IEnumerable<SavedPartitura> partituras, IEnumerable<BattleCommand> replay = null)
        {
            Db = db;
            Setup = setup;
            Battle = new Battle(db, setup);
            Formatter = new CombatLogFormatter(db) { IncludeHits = false };
            _hitFormatter = new CombatLogFormatter(db) { IncludeHits = true };
            if (partituras != null) Pilot.Partituras.AddRange(partituras);
            if (replay != null)
            {
                foreach (var c in replay) Replay.Enqueue(c);
                IsReplay = true;
            }
            Battle.Start();
            RealStartTime = Time.realtimeSinceStartup;
            SyncMirror();
        }

        public bool IsPlaying => _cursor < Battle.Events.Count || _wait > 0;

        /// <summary>El jugador puede elegir: es turno de bruja y ya se vio todo lo anterior.</summary>
        public bool AwaitingPlayer => !IsPlaying && !Auto && !IsReplay && Battle.Pending == DecisionKind.AllyTurn;

        public WitchUnit ActiveWitch => Battle.ActiveUnit as WitchUnit;

        // ------------------------------------------------------------------ Órdenes

        public CommandResult Submit(BattleCommand cmd)
        {
            if (cmd == null) return CommandResult.Fail("Sin orden.");
            if (cmd.Type == CommandType.OpenChain)
            {
                _pendingChainPreview = Battle.PreviewChain(cmd.Order, cmd.WithGuest);
                var best = AutoPilot.BestChain(Battle);
                _pendingBestSparks = best?.preview.SparkCount ?? 0;
            }
            var r = Battle.Execute(cmd);
            if (!r.Ok) ShowToast(r.Error);
            _decisionTimer = 0;
            return r;
        }

        public void ShowToast(string text)
        {
            Toast = text;
            ToastTime = 2.5f;
        }

        // ------------------------------------------------------------------ Bucle

        public void Tick(float dt)
        {
            if (BannerTime > 0) BannerTime -= dt;
            if (ToastTime > 0) ToastTime -= dt;

            float speed = Mathf.Max(0.25f, Speed);
            if (_wait > 0)
            {
                _wait -= dt * speed;
                return;
            }

            int processed = 0;
            while (_cursor < Battle.Events.Count && _wait <= 0 && processed < 64)
            {
                var e = Battle.Events[_cursor];
                if (!_preDelayDone)
                {
                    _preDelayDone = true;
                    float pre = PreDelay(e);
                    if (pre > 0)
                    {
                        _wait = pre;
                        return;
                    }
                }
                _cursor++;
                _preDelayDone = false;
                Apply(e);
                _wait = PostDelay(e);
                processed++;
            }

            if (IsPlaying) return;
            SyncMirror();
            if (Battle.IsOver)
            {
                if (RealEndTime < 0) RealEndTime = Time.realtimeSinceStartup;
                return;
            }

            _decisionTimer += dt * speed;
            if (IsReplay)
            {
                if (Replay.Count > 0 && _decisionTimer > 0.25f) Submit(Replay.Dequeue());
                return;
            }
            switch (Battle.Pending)
            {
                case DecisionKind.EnemyTurn:
                case DecisionKind.AutoTurn:
                    // Pausa corta: deja una ventana para lanzar una Ascensión antes del turno enemigo.
                    if (HoldDecision && !Auto) _decisionTimer = 0f;
                    if (_decisionTimer > (Auto ? 0.25f : 0.55f)) Submit(Auto ? (Pilot.Next(Battle) ?? BattleCommand.Advance()) : BattleCommand.Advance());
                    break;
                case DecisionKind.AllyTurn:
                    if (Auto && _decisionTimer > 0.3f)
                    {
                        var cmd = Pilot.Next(Battle);
                        var r = Submit(cmd);
                        if (!r.Ok) Submit(BattleCommand.Basic(Battle.ActiveUnit.Id, -1));
                    }
                    break;
            }
        }

        /// <summary>Espera antes de mostrar un evento: en la Cadena, los golpes caen en su frame (60 fps).</summary>
        private float PreDelay(BattleEvent e)
        {
            int frame;
            switch (e)
            {
                case HitLanded h when h.Source == HitSource.Chain || h.Source == HitSource.Reaction:
                    frame = h.Frame;
                    break;
                case ChainLinkStarted l:
                    frame = l.Frame;
                    break;
                default:
                    return 0f;
            }
            float delay = Mathf.Max(0, frame - _lastChainFrame) / 60f;
            _lastChainFrame = Mathf.Max(_lastChainFrame, frame);
            return delay;
        }

        private float PostDelay(BattleEvent e)
        {
            switch (e)
            {
                case BattleStarted _: return 0.6f;
                case TurnStarted _: return 0.3f;
                case ActionUsed a: return a.Action == ActionKind.Potion || a.Action == ActionKind.SelfCast ? 0.35f : 0.45f;
                case HitLanded h: return h.Source == HitSource.Chain || h.Source == HitSource.Reaction ? 0f : 0.12f;
                case BreakTriggered _: return 0.7f;
                case AspectFormed _: return 0.35f;
                case ReactionTriggered _: return 0.45f;
                case ChainStarted _: return 0.5f;
                case ChainEnded _: return 0.9f;
                case AscensionCast a: return a.Level == 1 ? 1.2f : a.Level == 2 ? 2.0f : 3.0f;
                case EcstasyStarted _: return 1.0f;
                case EnemyCharging _: return 0.8f;
                case AttackCanceled _: return 0.8f;
                case PhaseChanged _: return 1.2f;
                case UnitDefeated _: return 0.6f;
                case TurnSkipped _: return 0.5f;
                default: return 0.02f;
            }
        }

        // ------------------------------------------------------------------ Aplicar eventos a la vista

        private UnitView View(int id) => Views.TryGetValue(id, out var v) ? v : null;

        private void SetBanner(string text, Color color, float seconds = 1.4f)
        {
            Banner = text;
            BannerColor = color;
            BannerTime = seconds;
        }

        private void Apply(BattleEvent e)
        {
            EventPresented?.Invoke(e);
            AddLog(e);
            switch (e)
            {
                case TurnStarted t:
                    if (ActiveId >= 0) View(ActiveId)?.SetActive(false);
                    ActiveId = t.UnitId;
                    View(t.UnitId)?.SetActive(true);
                    BattleLighting.SetFocus(View(t.UnitId)?.transform);
                    break;

                case ActionUsed a:
                    if (a.Action != ActionKind.Potion) View(a.ActorId)?.Lunge();
                    if (a.Action == ActionKind.Spell || a.Action == ActionKind.SelfCast || a.Action == ActionKind.Ecstasy)
                    {
                        View(a.ActorId)?.Glow(0.8f);
                        CastLight(a.ActorId, 1.4f, 0.3f, 0.75f);
                    }
                    else if (a.Action == ActionKind.EnemySkill || a.Action == ActionKind.EnemyCharge) CastLight(a.ActorId, 1.1f, 0.28f, 0.6f);
                    if (a.Action == ActionKind.EnemyCharge)
                    {
                        View(a.ActorId)?.SetCharging(true);
                        SetBanner("¡" + a.MoveName + "!", Palette.Danger, 1.6f);
                    }
                    if (a.Action == ActionKind.EnemySkill) View(a.ActorId)?.SetCharging(false);
                    break;

                case HitLanded h:
                {
                    Hp[h.TargetId] = h.TargetHp;
                    if (Toughness.ContainsKey(h.TargetId)) Toughness[h.TargetId] = Math.Max(0, Toughness[h.TargetId] - h.ToughnessDamage);
                    var v = View(h.TargetId);
                    if (v != null)
                    {
                        v.Shake(h.IsCrit ? 0.3f : 0.15f);
                        v.Flash(h.IsSpark ? Palette.Spark : Color.white, 0.2f);
                        if (h.Damage > 0 || h.ShieldAbsorbed > 0)
                        {
                            Color c = h.IsSpark ? Palette.Spark : (h.TargetId >= Battle.FirstEnemyId ? Palette.Planet(h.Planet) : Palette.Danger);
                            string text = (h.Damage > 0 ? h.Damage.ToString("N0") : "0") + (h.IsCrit ? "!" : "");
                            v.AddFloat(text, c, h.IsSpark || h.IsCrit ? 38 : 30);
                        }
                        if (h.IsSpark) v.AddFloat("Spark", Palette.Spark, 26);
                    }
                    if (PlayingChain != null && (h.Source == HitSource.Chain || h.Source == HitSource.Reaction))
                    {
                        PlayheadFrame = h.Frame;
                        PlayedNotes.Add(h.Frame * 100 + h.AttackerId);
                        _chainHitIndex++;
                    }
                    break;
                }

                case DotTicked d:
                    Hp[d.UnitId] = d.Hp;
                    View(d.UnitId)?.AddFloat(d.Damage.ToString("N0"), Palette.Crimson, 26);
                    break;

                case Healed h:
                    Hp[h.UnitId] = h.Hp;
                    View(h.UnitId)?.AddFloat((h.Amount > 0 ? "+" : "") + h.Amount.ToString("N0"), h.Inverted ? Palette.Danger : Palette.Planet(Planet.Venus), 26);
                    break;

                case ShieldGained s:
                    View(s.UnitId)?.AddFloat("Escudo " + s.Amount.ToString("N0"), Palette.Planet(Planet.Luna), 24);
                    break;

                case SalinityChanged s:
                    Salinity[s.WitchId] = s.New;
                    break;

                case SalinityBandChanged b:
                    View(b.WitchId)?.AddFloat(SalinityBands.DisplayName(b.NewBand), Palette.Band(b.NewBand), 26);
                    break;

                case AnimaChanged a:
                    Anima[a.WitchId] = a.New;
                    break;

                case GrainsChanged g:
                    Grains = g.New;
                    break;

                case ResonanceChanged r:
                    Resonance = r.New;
                    break;

                case BreakTriggered b:
                    Toughness[b.TargetId] = 0;
                    View(b.TargetId)?.SetBroken(true);
                    View(b.TargetId)?.SetCharging(false);
                    View(b.TargetId)?.AddFloat("¡Quiebre! " + b.EffectName, Palette.Planet(b.Planet), 36);
                    if (View(b.TargetId) is UnitView broken) BattleLighting.Flash(broken.Head + Vector3.down * 0.6f, Palette.Planet(b.Planet), 1.8f, 0.34f, 0.8f); // TUNING
                    SetBanner("QUIEBRE · " + b.EffectName, Palette.Planet(b.Planet));
                    break;

                case ToughnessRestored t:
                    if (Battle.FindEnemy(t.UnitId) is EnemyUnit en) Toughness[t.UnitId] = en.MaxToughness;
                    View(t.UnitId)?.SetBroken(false);
                    break;

                case AspectFormed a:
                {
                    string reaction = a.Reaction != HybridReaction.None ? " → " + AspectRules.DisplayName(a.Reaction) : "";
                    SetBanner(AspectRules.DisplayName(a.Aspect) + reaction, Palette.GoldBright, 1.0f);
                    break;
                }

                case ReactionTriggered r:
                    SetBanner(AspectRules.DisplayName(r.Reaction), Palette.Spark, 1.2f);
                    break;

                case ChainStarted c:
                    PlayingChain = _pendingChainPreview;
                    PlayheadFrame = 0;
                    PlayedNotes.Clear();
                    _lastChainFrame = 0;
                    _chainHitIndex = 0;
                    SetBanner("CADENA", Palette.GoldBright, 1.0f);
                    Chains.Add(new ChainRecord
                    {
                        Order = string.Join(" → ", c.Names),
                        PredictedSparks = c.PredictedSparks,
                        BestPossibleSparks = _pendingBestSparks,
                    });
                    break;

                case ChainLinkStarted l:
                    PlayheadFrame = l.Frame;
                    View(l.WitchId)?.Lunge();
                    View(l.WitchId)?.Glow(0.6f);
                    break;

                case ChainEnded c:
                    if (Chains.Count > 0)
                    {
                        Chains[Chains.Count - 1].Sparks = c.Sparks;
                        Chains[Chains.Count - 1].Damage = c.TotalDamage;
                    }
                    SetBanner(c.Sparks + " Spark · " + c.TotalDamage.ToString("N0") + " de daño", Palette.Spark, 1.6f);
                    PlayingChain = null;
                    PlayheadFrame = -1;
                    break;

                case AscensionCast a:
                    View(a.WitchId)?.Glow(2.5f);
                    CastLight(a.WitchId, 1.6f + 0.4f * a.Level, 0.38f + 0.12f * a.Level, 1.2f + 0.5f * a.Level); // TUNING
                    SetBanner("ASCENSIÓN " + (a.Level == 1 ? "I" : a.Level == 2 ? "II" : "III") + " · " + a.MoveName, Palette.FiveStar, 1.8f);
                    break;

                case EcstasyStarted s:
                    View(s.WitchId)?.SetEcstasy(true);
                    SetBanner("ÉXTASIS ABISAL · " + s.WitchName, Palette.Band(SalinityBand.ExtasisAbisal), 1.6f);
                    break;

                case EcstasyEnded s:
                    View(s.WitchId)?.SetEcstasy(false);
                    break;

                case AttackCanceled c:
                    View(c.UnitId)?.SetCharging(false);
                    SetBanner(c.SkillName + " cancelado: " + c.Reason, Palette.Spark, 1.6f);
                    break;

                case PhaseChanged p:
                    SetBanner("SEGUNDA FASE", Palette.Danger, 1.6f);
                    break;

                case UnitDefeated u:
                    View(u.UnitId)?.Die();
                    if (View(u.UnitId) is UnitView fallen) BattleLighting.Flash(fallen.Head + Vector3.down * 0.8f, Palette.SaltWhite, 1.2f, 0.26f, 0.7f);
                    break;

                case BattleEnded b:
                    SetBanner(b.Outcome == BattleOutcome.Victory ? "VICTORIA" : "DERROTA",
                        b.Outcome == BattleOutcome.Victory ? Palette.FiveStar : Palette.Danger, 3f);
                    break;
            }
        }

        /// <summary>Luz del color del planeta de quien lanza: hechizos, habilidades enemigas y Ascensiones.</summary>
        private void CastLight(int unitId, float intensity, float radius, float life)
        {
            var v = View(unitId);
            if (v == null) return;
            Planet planet = Planet.Sol;
            if (Battle.FindWitch(unitId) is WitchUnit w) planet = w.Dominant;
            else if (Battle.FindEnemy(unitId) is EnemyUnit en) planet = en.Def.AttackPlanet;
            BattleLighting.Flash(v.Head + Vector3.down * 0.5f, Palette.Planet(planet), intensity, radius, life, v.transform);
        }

        private void AddLog(BattleEvent e)
        {
            if (e is HitLanded && !ShowHits) return;
            string line = e is HitLanded ? _hitFormatter.Format(e) : Formatter.Format(e);
            if (string.IsNullOrEmpty(line)) return;
            Log.Add(new LogLine { Text = line, Color = ColorFor(line) });
            if (Log.Count > 300) Log.RemoveRange(0, Log.Count - 300);
        }

        public static Color ColorFor(string line)
        {
            if (line.StartsWith("[Quiebre]")) return Palette.Planet(Planet.Sol);
            if (line.StartsWith("[Cadena]") || line.StartsWith("[Apertura]") || line.StartsWith("[Enlace]") || line.StartsWith("[Cierre]")) return Palette.GoldBright;
            if (line.StartsWith("[Aspecto]") || line.StartsWith("[Reacción]")) return Palette.Spark;
            if (line.StartsWith("[Ascensión")) return Palette.FiveStar;
            if (line.StartsWith("[Enemigo]") || line.StartsWith("[Amenaza]") || line.StartsWith("[Caída]") || line.StartsWith("[Fase]")) return Palette.Danger;
            if (line.StartsWith("[Salinidad]") || line.StartsWith("[Éxtasis")) return Palette.Band(SalinityBand.Sobresalada);
            if (line.StartsWith("[Turno") || line.StartsWith("[Ciclo")) return Palette.ParchmentShadow;
            if (line.StartsWith("[Cura]") || line.StartsWith("[Escudo]")) return Palette.Planet(Planet.Venus);
            return Palette.Parchment;
        }

        /// <summary>Copia el estado real al espejo cuando no queda nada por reproducir.</summary>
        public void SyncMirror()
        {
            foreach (var w in Battle.Witches)
            {
                Hp[w.Id] = w.Hp;
                Salinity[w.Id] = w.Salinity;
                Anima[w.Id] = w.Anima;
            }
            foreach (var e in Battle.Enemies)
            {
                Hp[e.Id] = e.Hp;
                Toughness[e.Id] = e.Toughness;
                View(e.Id)?.SetBroken(e.Broken);
                View(e.Id)?.SetCharging(e.IsCharging);
            }
            Grains = Battle.Grains;
            Resonance = Battle.Resonance;
            ActiveId = Battle.ActiveUnit?.Id ?? -1;
            foreach (var kv in Views) kv.Value.SetActive(kv.Key == ActiveId);
        }

        // ------------------------------------------------------------------ Registro para la prueba con jugadores

        public string ExportReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Sal y Almas · Fase 1 · registro de combate");
            sb.AppendLine("Encuentro: " + Battle.Encounter.Name);
            sb.AppendLine("Semilla: " + Setup.Seed);
            sb.AppendLine("Equipo: " + string.Join(", ", Setup.Team) + (Setup.GuestId != null ? " · Invitada: " + Setup.GuestId : ""));
            sb.AppendLine("Resultado: " + Battle.Outcome + " en " + Battle.TurnNumber + " turnos");
            float end = RealEndTime > 0 ? RealEndTime : Time.realtimeSinceStartup;
            sb.AppendLine("Tiempo real: " + FormatTime(end - RealStartTime));
            sb.AppendLine();
            sb.AppendLine("Cadenas:");
            foreach (var c in Chains) sb.AppendLine($"  {c.Order} · Spark {c.Sparks} (previstos {c.PredictedSparks}, mejor posible {c.BestPossibleSparks}) · daño {c.Damage}");
            sb.AppendLine();
            sb.AppendLine("Registro:");
            var fmt = new CombatLogFormatter(Db) { IncludeHits = true };
            foreach (var e in Battle.Events)
            {
                var line = fmt.Format(e);
                if (line != null) sb.AppendLine(line);
            }
            sb.AppendLine();
            sb.AppendLine("Órdenes (para repetir el combate):");
            foreach (var c in Battle.CommandLog) sb.AppendLine("  " + c);
            return sb.ToString();
        }

        /// <summary>Huella de todos los eventos, para comprobar que una repetición salió idéntica.</summary>
        public string EventFingerprint()
        {
            unchecked
            {
                ulong h = 1469598103934665603UL;
                foreach (var e in Battle.Events)
                    foreach (char ch in e.Serialize()) { h ^= ch; h *= 1099511628211UL; }
                return Battle.Events.Count + " eventos · " + h.ToString("x16");
            }
        }

        public static string FormatTime(float seconds)
        {
            int s = Mathf.Max(0, Mathf.RoundToInt(seconds));
            return (s / 60) + ":" + (s % 60).ToString("00");
        }
    }
}
