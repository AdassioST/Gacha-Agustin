using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Fresno
{
    /// <summary>Cómo juega el piloto de runs. Los valores por defecto imitan a una jugadora prudente.</summary>
    public sealed class ClimberPolicy
    {
        /// <summary>Maldiciones que acepta en cada zona.</summary>
        public int CursesPerZone;
        /// <summary>Ranura de Eco donde guarda la build al terminar (−1 = no guarda).</summary>
        public int SaveEcoSlot = 0;
        /// <summary>Busca el nodo de la Mancha si existe una ruta hasta él.</summary>
        public bool SeekMancha = true;
        /// <summary>Se desvía a un Santuario cuando lleva esta cantidad de almitas sueltas.</summary>
        public long BankThreshold = 6000;
        /// <summary>Por debajo de esta vida media busca descanso y evita las Élites.</summary>
        public double LowHp = 0.55;
        /// <summary>Rama que prefiere al elegir Injertos (null = la que ya lleve más).</summary>
        public Planet? PreferredRama;
        /// <summary>Abandona al entrar a esta zona, antes de las maldiciones (−1 = sigue hasta el final). Sirve para el Ascenso corto.</summary>
        public int StopAtZone = -1;
    }

    /// <summary>Lo que dejó una run jugada por el piloto, para pruebas y balance.</summary>
    public sealed class ClimbReport
    {
        public RunOutcome Outcome;
        public string ReachedNode = "";
        public int Nodes;
        public int Combats;
        public int CombatsWon;
        public int Grafts;
        public int Fruits;
        public int Commands;
        public int AllyTurns;
        public int Chains;
        public long AlmitasEarned;
        public long AlmitasBanked;
        public long ManchaLeft;
        public long ManchaRecovered;
        public long SaviaEarned;
        public string EcoUsed = "";
        public readonly List<string> Log = new List<string>();
        public readonly List<string> Path = new List<string>();
        /// <summary>Huella de todos los combates de la run: sirve para comprobar el determinismo.</summary>
        public string Fingerprint = "";
    }

    /// <summary>
    /// Piloto de runs del Fresno: elige nodos, Injertos, eventos y descansos y pelea con el AutoPilot del combate.
    /// No tiene azar propio ni memoria fuera del estado, así que con la misma semilla juega la misma run.
    /// </summary>
    public sealed class AutoClimber
    {
        private readonly MetaGame _g;
        public readonly ClimberPolicy Policy;
        public readonly AutoPilot Pilot = new AutoPilot();

        /// <summary>Permite a una prueba alterar un combate antes de pelearlo (por ejemplo, para forzar una derrota).</summary>
        public Action<BattleSetup> BeforeBattle;

        public AutoClimber(MetaGame game, ClimberPolicy policy = null)
        {
            _g = game;
            Policy = policy ?? new ClimberPolicy();
        }

        private FresnoService F => _g.Fresno;
        private RunState Run => F.Run;

        /// <summary>Juega una run de principio a fin, incluida la decisión del Eco.</summary>
        public ClimbReport Climb(IReadOnlyList<string> team, int ecoSlot = -1, ulong? seed = null, IReadOnlyList<string> belt = null)
        {
            var started = F.StartRun(team, ecoSlot, seed, belt);
            if (!started.Ok) throw new InvalidOperationException(started.Message);
            return Continue();
        }

        /// <summary>Sigue una run ya empezada hasta el final (o hasta StopAtZone, donde la abandona).</summary>
        public ClimbReport Continue()
        {
            var report = new ClimbReport { EcoUsed = Run.EcoName };
            uint hash = 2166136261;
            int guard = 0;
            while (Run.Phase != RunPhase.EcoSave)
            {
                if (++guard > 2000) throw new InvalidOperationException("El piloto de runs no avanza en " + Run.NodeId + " (" + Run.Phase + ").");
                hash = Step(report, hash);
            }
            hash = Mix(hash, Run.Outcome.ToString());
            report.Outcome = Run.Outcome;
            report.ReachedNode = Run.NodeId;
            report.CombatsWon = Run.CombatsWon;
            report.Grafts = Run.Grafts.Count;
            foreach (var rama in _g.Meta.Fresno.Ramas) if (F.FruitActive(rama.Planet)) report.Fruits++;
            report.AlmitasEarned = Run.Earned;
            report.AlmitasBanked = Run.Banked;
            report.ManchaRecovered = Run.ManchaRecovered;
            report.SaviaEarned = Run.SaviaEarned;
            var mancha = _g.Profile.Fresno.Mancha;
            report.ManchaLeft = mancha != null && mancha.FromRun == Run.RunNumber ? mancha.Almitas : 0;
            report.Log.AddRange(Run.Log);
            report.Path.AddRange(Run.Path);
            report.Fingerprint = hash.ToString("x8");
            SaveEco();
            return report;
        }

        /// <summary>Da un solo paso de la run: una decisión o un combate. La interfaz lo usa para el botón de avance automático.</summary>
        public void StepOnce()
        {
            if (Run != null && Run.Phase != RunPhase.EcoSave) Step(new ClimbReport(), 0);
        }

        private uint Step(ClimbReport report, uint hash)
        {
            switch (Run.Phase)
            {
                case RunPhase.Curses:
                    if (Policy.StopAtZone >= 0 && Run.Zone >= Policy.StopAtZone) Require(F.Abandon());
                    else Require(F.ChooseCurses(PickCurses()));
                    break;
                case RunPhase.ChooseNode:
                    Require(F.EnterNode(PickNode().Lane));
                    report.Nodes++;
                    break;
                case RunPhase.Battle:
                    report.Combats++;
                    hash = Fight(report, hash);
                    break;
                case RunPhase.Choice:
                    ResolveChoice();
                    break;
            }
            return hash;
        }

        private static void Require(OpResult r)
        {
            if (!r.Ok) throw new InvalidOperationException("El piloto de runs hizo algo inválido: " + r.Message);
        }

        private static uint Mix(uint hash, string text)
        {
            foreach (char c in text) { hash ^= c; hash *= 16777619; }
            return hash;
        }

        private List<string> PickCurses()
        {
            var list = new List<string>();
            foreach (var c in _g.Meta.Fresno.Curses)
            {
                if (list.Count >= Policy.CursesPerZone) break;
                list.Add(c.Id);
            }
            return list;
        }

        // ------------------------------------------------------------------ Combate

        private uint Fight(ClimbReport report, uint hash)
        {
            var setup = F.PrepareBattle();
            BeforeBattle?.Invoke(setup);
            var battle = new Battle(_g.Combat, setup);
            battle.Start();
            report.Commands += Pilot.RunToEnd(battle);
            foreach (var e in battle.Events)
            {
                if (e is Events.TurnStarted t && t.Side == Side.Ally) report.AllyTurns++;
                if (e is Events.ChainStarted) report.Chains++;
            }
            if (!battle.IsOver) throw new InvalidOperationException("El combate del nodo " + Run.NodeId + " no terminó.");
            hash = Mix(hash, $"{Run.NodeId}:{battle.Outcome}:{battle.TurnNumber}:{battle.TotalSparks};");
            foreach (var w in battle.Witches) hash = Mix(hash, w.Hp + ",");
            Require(F.ResolveBattle(battle));
            return hash;
        }

        // ------------------------------------------------------------------ Mapa

        private double AverageHp()
        {
            double sum = 0;
            foreach (var id in Run.Team) sum += Run.Hp[id];
            return sum / Math.Max(1, Run.Team.Count);
        }

        private bool ReachesMancha(MapNode from)
        {
            if (Run.Mancha == null || Run.ManchaResolved || from.Zone != Run.Mancha.Zone) return false;
            if (from.HasMancha) return true;
            if (from.Floor >= Run.Mancha.Floor) return false;
            foreach (var lane in from.Next)
            {
                var next = F.Map.Node(from.Zone, from.Floor + 1, lane);
                if (next != null && ReachesMancha(next)) return true;
            }
            return false;
        }

        private MapNode PickNode()
        {
            MapNode best = null;
            double bestScore = double.MinValue;
            foreach (var n in F.AvailableNodes())
            {
                double s = Score(n);
                // A igual puntaje, el carril más bajo: determinista.
                if (s > bestScore) { bestScore = s; best = n; }
            }
            if (best == null) throw new InvalidOperationException("No hay nodos disponibles en " + Run.NodeId);
            return best;
        }

        private double Score(MapNode n)
        {
            double hp = AverageHp();
            bool low = hp < Policy.LowHp;
            double score;
            switch (n.Type)
            {
                case NodeType.Santuario: score = low ? 90 : Run.Loose >= Policy.BankThreshold ? 70 : 25; break;
                case NodeType.Combate: score = low ? 15 : 50; break;
                case NodeType.Elite: score = hp > 0.8 ? 55 : -20; break;
                case NodeType.Nido: score = 60; break;
                case NodeType.Evento: score = 45; break;
                case NodeType.Mercader: score = Run.Loose >= 2000 * (Run.Zone + 1) ? 48 : 20; break;
                case NodeType.Caldero: score = 35; break;
                default: score = 40; break;
            }
            if (Policy.SeekMancha && ReachesMancha(n)) score += 200;
            return score;
        }

        // ------------------------------------------------------------------ Decisiones

        private void ResolveChoice()
        {
            switch (Run.Pending.Kind)
            {
                case ChoiceKind.Graft:
                    Require(F.ChooseGraft(PickGraft(Run.Pending.Options)));
                    break;
                case ChoiceKind.Mutation:
                    Require(F.ChooseMutation(true));
                    break;
                case ChoiceKind.Event:
                    Require(F.ChooseEventOption(PickEventOption()));
                    break;
                case ChoiceKind.Santuario:
                    Santuario();
                    break;
                case ChoiceKind.Mercader:
                    Merchant();
                    break;
                case ChoiceKind.Caldero:
                    Caldero();
                    break;
                default:
                    throw new InvalidOperationException("Decisión desconocida: " + Run.Pending.Kind);
            }
        }

        private string PickGraft(List<string> options)
        {
            string best = options.Count > 0 ? options[0] : null;
            int bestScore = int.MinValue;
            var rules = _g.Meta.Fresno;
            foreach (var id in options)
            {
                var g = rules.Graft(id);
                int score = F.GraftsOf(g.Rama) * 10;
                if (Policy.PreferredRama == g.Rama) score += 100;
                if (score > bestScore) { bestScore = score; best = id; }
            }
            return best;
        }

        private int PickEventOption()
        {
            var ev = _g.Meta.Fresno.Event(Run.Pending.EventId);
            bool low = AverageHp() < Policy.LowHp;
            int best = -1;
            double bestScore = double.MinValue;
            for (int i = 0; i < ev.Options.Count; i++)
            {
                var opt = ev.Options[i];
                if (F.OptionBlocker(opt).Length > 0) continue;
                double score = -opt.CostAlmitas / 1000.0;
                foreach (var o in opt.Outcomes)
                {
                    switch (o.Type)
                    {
                        case EventOutcomeType.Heal: score += low ? 6 : 2; break;
                        case EventOutcomeType.Graft: score += 5; break;
                        case EventOutcomeType.Mutation: score += 3; break;
                        case EventOutcomeType.RunMods: score += 3; break;
                        case EventOutcomeType.Almitas: score += o.Value / 1000.0; break;
                        case EventOutcomeType.Savia: score += o.Value / 100.0; break;
                        case EventOutcomeType.BlackSalt: score += 1; break;
                        case EventOutcomeType.Curse: score -= 2; break;
                    }
                }
                if (score > bestScore) { bestScore = score; best = i; }
            }
            if (best < 0) throw new InvalidOperationException("El evento " + ev.Id + " no tiene opción disponible.");
            return best;
        }

        private void Santuario()
        {
            int salinity = 0;
            foreach (var id in Run.Team) salinity = Math.Max(salinity, Run.Salinity[id]);
            if (AverageHp() < 0.6 || salinity >= 70) { Require(F.SantuarioRest()); return; }
            foreach (var id in Run.Team)
            {
                foreach (ChainRole role in new[] { ChainRole.Cierre, ChainRole.Enlace, ChainRole.Apertura })
                {
                    if (Run.Trained.Exists(t => t.Witch == id && t.Role == role)) continue;
                    Require(F.SantuarioTrain(id, role));
                    return;
                }
            }
            Require(F.SantuarioRest());
        }

        private void Merchant()
        {
            for (int guard = 0; guard < 8; guard++)
            {
                int pick = -1;
                for (int i = 0; i < Run.Pending.Options.Count; i++)
                {
                    if (Run.Loose < Run.Pending.Prices[i]) continue;
                    string o = Run.Pending.Options[i];
                    bool wanted = o == "curar" ? AverageHp() < Policy.LowHp : o.StartsWith("injerto:", StringComparison.Ordinal);
                    if (wanted) { pick = i; break; }
                }
                if (pick < 0) break;
                Require(F.MerchantBuy(pick));
            }
            Require(F.LeaveChoice());
        }

        private void Caldero()
        {
            foreach (var option in Run.Pending.Options)
            {
                if (option == "turbia") continue;
                if (F.CalderoBrew(option).Ok) return;
            }
            Require(F.LeaveChoice());
        }

        // ------------------------------------------------------------------ Eco

        private void SaveEco()
        {
            if (Policy.SaveEcoSlot < 0 || !F.EcoSlotUnlocked(Policy.SaveEcoSlot) || Run.Grafts.Count == 0)
            {
                Require(F.SkipEco());
                return;
            }
            // Conserva los Injertos de la Rama con más avance: son los que acercan el Fruto en la siguiente run.
            var rules = _g.Meta.Fresno;
            var grafts = new List<string>(Run.Grafts);
            var count = new Dictionary<Planet, int>();
            foreach (var id in grafts) { var r = rules.Graft(id).Rama; count[r] = (count.TryGetValue(r, out var v) ? v : 0) + 1; }
            var order = new List<int>();
            for (int i = 0; i < grafts.Count; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                int byRama = count[rules.Graft(grafts[b]).Rama].CompareTo(count[rules.Graft(grafts[a]).Rama]);
                return byRama != 0 ? byRama : a.CompareTo(b);
            });
            var keep = new List<string>();
            for (int i = 0; i < order.Count && keep.Count < rules.EcoGrafts; i++) keep.Add(grafts[order[i]]);
            var trained = Run.Trained.Count > 0 ? Run.Trained[0] : null;
            Require(F.SaveEco(Policy.SaveEcoSlot, keep, trained));
        }
    }
}
