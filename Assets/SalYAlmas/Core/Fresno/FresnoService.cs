using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Fresno
{
    /// <summary>
    /// El Fresno (GDD 10): runs con mapa de nodos, Injertos, Frutos, Mutaciones, maldiciones, Bendición del Paria, almitas
    /// sueltas que se guardan en Santuarios, Mancha de Sal, Savia, Eco, Raíces, Ofrenda y tope semanal. No hay energía:
    /// las runs nunca se cortan; lo que se limita es la recompensa de la semana.
    /// </summary>
    public sealed class FresnoService
    {
        public const int BeltCharges = 3;

        private readonly MetaGame _g;
        private FresnoMap _map;
        private ulong _mapSeed;
        private ManchaState _mapMancha;

        public FresnoService(MetaGame game) { _g = game; }

        private FresnoRules R => _g.Meta.Fresno;
        private FresnoProfile F => _g.Profile.Fresno;
        public RunState Run => F.Run;
        public bool InRun => F.Run != null;

        /// <summary>Mapa de la run actual (se regenera con la semilla y la Mancha; no se guarda).</summary>
        public FresnoMap Map
        {
            get
            {
                if (Run == null) return null;
                if (_map == null || _mapSeed != Run.Seed || !ReferenceEquals(_mapMancha, Run.Mancha))
                {
                    _map = MapGenerator.Generate(R, Run.Seed, Run.Mancha);
                    _mapSeed = Run.Seed;
                    _mapMancha = Run.Mancha;
                }
                return _map;
            }
        }

        public ZoneDef Zone => Run != null ? R.Zones[Math.Min(Run.Zone, R.Zones.Count - 1)] : null;

        public MapNode CurrentNode => Run == null || Run.Floor < 0 ? null : Map.Node(Run.Zone, Run.Floor, Run.Lane);

        private void Log(string line) => Run?.Log.Add(line);

        private IRandomSource NextRng() => new SeededRandom(MapGenerator.Mix(Run.Seed, 0x5A17 + ++Run.Draws));

        // ================================================================== Progreso permanente

        public int OfrendaLevel => R.Ofrenda.LevelFor(F.OfrendaTotal);

        /// <summary>Bonos de la Ofrenda: pequeños y con tope (GDD 10.6 y 11.4).</summary>
        public StatBlock OfrendaBonus()
        {
            var b = new StatBlock();
            int level = OfrendaLevel;
            foreach (var key in R.Ofrenda.PerLevel.Keys)
            {
                double v = R.Ofrenda.PerLevel.Get(key) * level;
                double cap = R.Ofrenda.Caps.Get(key);
                b.Add(key, cap > 0 ? Math.Min(v, cap) : v);
            }
            return b;
        }

        public OpResult Offer(long almitas)
        {
            if (almitas <= 0) return OpResult.Fail("Ofrenda inválida.");
            var cost = new Bundle().Add(Res.Almitas, almitas);
            if (!_g.Ledger.TrySpend(cost, "ofrenda", "Ofrenda al Fresno")) return OpResult.Fail(_g.Ledger.Missing(cost));
            int before = OfrendaLevel;
            F.OfrendaTotal += almitas;
            int after = OfrendaLevel;
            var notes = new List<string>();
            for (int lvl = Math.Max(before, F.OfrendaLevelPaid) + 1; lvl <= after; lvl++)
            {
                if (lvl % R.Ofrenda.DustEvery == 0)
                {
                    _g.Ledger.Apply(Res.PolvoAstral, R.Ofrenda.DustAmount, "ofrenda.hito", "Ofrenda nivel " + lvl);
                    notes.Add($"+{R.Ofrenda.DustAmount} Polvo Astral");
                }
                if (R.Ofrenda.Unlocks.TryGetValue(lvl, out var text)) notes.Add(text);
            }
            F.OfrendaLevelPaid = Math.Max(F.OfrendaLevelPaid, after);
            _g.Report("ofrenda", almitas);
            return OpResult.Success($"El Fresno crece: nivel {after}" + (notes.Count > 0 ? ". " + string.Join(" ", notes) : ""));
        }

        public long NextOfrendaThreshold => R.Ofrenda.Threshold(OfrendaLevel + 1);

        public bool RamaUnlocked(Planet p) => R.Rama(p).UnlockOfrenda <= OfrendaLevel;

        public int RootLevel(string id) => F.Roots.TryGetValue(id, out var v) ? v : 0;

        public StatBlock RootMods()
        {
            var b = new StatBlock();
            foreach (var r in R.Roots) b.AddAll(r.Mods, RootLevel(r.Id));
            return b;
        }

        /// <summary>Raíces: talentos de cuenta con tope que se compran con Savia (GDD 10.6).</summary>
        public OpResult BuyRoot(string id)
        {
            var root = R.Root(id);
            if (root == null) return OpResult.Fail("Raíz desconocida.");
            int level = RootLevel(id);
            if (level >= root.Max) return OpResult.Fail("Esa Raíz ya llegó a su tope.");
            var cost = new Bundle().Add(Res.Savia, root.Costs[level]);
            if (!_g.Ledger.TrySpend(cost, "raices", root.Name)) return OpResult.Fail(_g.Ledger.Missing(cost));
            F.Roots[id] = level + 1;
            return OpResult.Success($"{root.Name} {level + 1}/{root.Max}");
        }

        public bool EcoSlotUnlocked(int slot)
        {
            foreach (var u in R.EcoUnlocks)
            {
                if (u.Slot != slot + 1) continue;
                if (u.Rule == "inicio") return true;
                if (u.Rule.StartsWith("jefe:", StringComparison.Ordinal)) return F.BossesDefeated.Contains(u.Rule.Substring(5));
            }
            return false;
        }

        // ------------------------------------------------------------------ Tope semanal (GDD 10.4)

        public int CurrentWeek => Math.Max(0, _g.Profile.Day) / 7;

        private void EnsureWeek()
        {
            if (F.Week == CurrentWeek) return;
            F.Week = CurrentWeek;
            F.WeekBanked = 0;
            F.WeekBathSalts = 0;
        }

        public long WeeklyRemaining
        {
            get
            {
                EnsureWeek();
                return Math.Max(0, R.Weekly.CapAlmitas - F.WeekBanked);
            }
        }

        /// <summary>
        /// Guarda almitas en la cartera respetando el tope semanal. Lo que pasa del tope se convierte en Savia a una tasa
        /// menor. Devuelve las almitas que entraron.
        /// </summary>
        public long BankAlmitas(long amount, string flow)
        {
            if (amount <= 0) return 0;
            EnsureWeek();
            long allowed = Math.Max(0, R.Weekly.CapAlmitas - F.WeekBanked);
            long banked = Math.Min(amount, allowed);
            if (banked > 0) _g.Ledger.Apply(Res.Almitas, banked, flow, Run != null ? "Run " + Run.RunNumber : "");
            F.WeekBanked += banked;
            long overflow = amount - banked;
            if (overflow > 0)
            {
                long savia = (long)Math.Round(overflow * R.Weekly.OverflowSaviaRate, MidpointRounding.AwayFromZero);
                if (savia > 0) _g.Ledger.Apply(Res.Savia, savia, "fresno.tope", $"{overflow:N0} almitas sobre el tope semanal");
                if (Run != null) { Run.SaviaEarned += savia; Log($"[Tope semanal] {overflow:N0} almitas se vuelven {savia:N0} de Savia."); }
            }
            if (Run != null) Run.Banked += banked;
            return banked;
        }

        // ================================================================== Run

        public string CanStart(IReadOnlyList<string> team)
        {
            if (InRun) return "Ya hay una run en curso.";
            if (team == null || team.Count < 1 || team.Count > 4) return "El equipo lleva de una a cuatro brujas.";
            var seen = new HashSet<string>();
            foreach (var id in team)
            {
                if (!seen.Add(id)) return "Bruja repetida.";
                if (!_g.Profile.Owns(id)) return "No tienes a " + id + ".";
                if (!_g.Meta.HasKit(id)) return _g.Meta.Witch(id).Name + " todavía no tiene kit de combate.";
            }
            return "";
        }

        /// <summary>
        /// Empieza una run. Puede llevar un Eco (sus Injertos y su movimiento entrenado) y Salmueras del inventario.
        /// La Mancha de la run anterior pasa al mapa de esta run: tienes un intento para recuperarla.
        /// </summary>
        public OpResult StartRun(IReadOnlyList<string> team, int ecoSlot = -1, ulong? seed = null, IReadOnlyList<string> belt = null)
        {
            string why = CanStart(team);
            if (why.Length > 0) return OpResult.Fail(why);
            if (ecoSlot >= 0 && (ecoSlot >= F.Eco.Count || F.Eco[ecoSlot] == null)) return OpResult.Fail("Esa ranura de Eco está vacía.");

            var run = new RunState
            {
                Seed = seed ?? _g.Rng.NextULong(),
                RunNumber = ++F.RunsStarted,
                Team = new List<string>(team),
                Phase = RunPhase.Curses,
                Zone = 0,
                Floor = -1,
            };
            for (int z = 0; z < R.Zones.Count; z++) run.Curses.Add(new List<string>());
            foreach (var id in team) { run.Hp[id] = 1.0; run.Salinity[id] = 0; }
            F.Run = run;

            if (belt != null)
            {
                int slots = BeltSlots;
                foreach (var pid in belt)
                {
                    if (run.Belt.Count >= slots) break;
                    int have = _g.Profile.Potions.TryGetValue(pid, out var n) ? n : 0;
                    int take = Math.Min(BeltCharges, have);
                    if (take <= 0) continue;
                    _g.Profile.Potions[pid] = have - take;
                    if (_g.Profile.Potions[pid] == 0) _g.Profile.Potions.Remove(pid);
                    run.Belt.Add(new BeltSlot(pid, take));
                }
            }

            if (ecoSlot >= 0)
            {
                var eco = F.Eco[ecoSlot];
                foreach (var g in eco.Grafts) if (R.Graft(g) != null && !run.Grafts.Contains(g)) run.Grafts.Add(g);
                foreach (var t in eco.Trained) if (run.Team.Contains(t.Witch)) run.Trained.Add(new TrainedMove(t.Witch, t.Role));
                run.EcoSlotUsed = ecoSlot;
                run.EcoName = eco.Name;
                Log($"[Eco] Empiezas con el Eco «{eco.Name}»: {eco.Grafts.Count} Injertos y {run.Trained.Count} movimiento entrenado.");
            }

            if (F.Mancha != null)
            {
                run.Mancha = F.Mancha;
                F.Mancha = null;
                Log($"[Mancha de Sal] Tus {run.Mancha.Almitas:N0} almitas perdidas esperan en el nodo {run.Mancha.Zone + 1}-{run.Mancha.Floor + 1}-{run.Mancha.Lane + 1}. Tienes un intento.");
            }
            _map = null;
            Log($"[Fresno] Run {run.RunNumber} con {string.Join(", ", NamesOf(team))}. Semilla {run.Seed}.");
            Log($"[Zona] {Zone.Name}: {Zone.Theme}. Puedes aceptar hasta {R.MaxCursesPerZone} maldiciones (×{R.CurseAlmitasMultiplier} almitas cada una).");
            return OpResult.Success("Run " + run.RunNumber);
        }

        private List<string> NamesOf(IEnumerable<string> ids)
        {
            var list = new List<string>();
            foreach (var id in ids) list.Add(_g.Combat.Witches.TryGetValue(id, out var w) ? w.ShortName : id);
            return list;
        }

        public int BeltSlots => _g.Combat.Rules.Potions.BeltSlots + (int)Math.Round(RootMods().Get(ModKeys.ExtraBeltSlots));

        // ------------------------------------------------------------------ Maldiciones (GDD 10.3)

        public OpResult ChooseCurses(IReadOnlyList<string> curses)
        {
            if (Run == null || Run.Phase != RunPhase.Curses) return OpResult.Fail("Las maldiciones se eligen al entrar a una zona.");
            curses = curses ?? Array.Empty<string>();
            if (curses.Count > R.MaxCursesPerZone) return OpResult.Fail($"Hasta {R.MaxCursesPerZone} maldiciones.");
            var list = new List<string>();
            foreach (var c in curses)
            {
                if (R.Curse(c) == null) return OpResult.Fail("Maldición desconocida: " + c);
                if (list.Contains(c)) return OpResult.Fail("Maldición repetida.");
                list.Add(c);
            }
            Run.Curses[Run.Zone] = list;
            Run.Phase = RunPhase.ChooseNode;
            if (list.Count > 0) Log($"[Maldición] Aceptas {list.Count}: almitas ×{CurseMultiplier(Run.Zone):0.###} en esta zona.");
            return OpResult.Success();
        }

        public double CurseMultiplier(int zone) => Math.Pow(R.CurseAlmitasMultiplier, Run.Curses[zone].Count);

        // ------------------------------------------------------------------ Paria, multiplicadores y modificadores

        public int CountRarity(int rarity)
        {
            int n = 0;
            foreach (var id in Run.Team) if (_g.Meta.Roster[id].Rarity == rarity) n++;
            return n;
        }

        /// <summary>Multiplicador de almitas de la zona actual: maldiciones × (1 + Paria + Mutaciones + Raíces + Ofrenda + eventos).</summary>
        public double AlmitasMultiplier()
        {
            double pct = CountRarity(3) * R.PariaThreeStarAlmitas + CountRarity(4) * R.PariaFourStarAlmitas;
            pct += RunModifiers().Get(ModKeys.AlmitasPct);
            return CurseMultiplier(Run.Zone) * (1.0 + pct);
        }

        public int GraftChoiceCount() =>
            R.GraftChoices + CountRarity(3) * R.PariaThreeStarChoices + (int)Math.Round(RunModifiers().Get(ModKeys.GraftChoices));

        public int GraftsOf(Planet rama)
        {
            int n = 0;
            foreach (var id in Run.Grafts) { var g = R.Graft(id); if (g != null && g.Rama == rama && !g.Pending) n++; }
            return n;
        }

        public bool FruitActive(Planet rama) => GraftsOf(rama) >= R.FruitAt;

        /// <summary>Todo lo que modifica los combates de la run: Injertos, Frutos, Mutaciones, maldiciones de la zona, Raíces, Ofrenda y eventos.</summary>
        public StatBlock RunModifiers()
        {
            var mods = new StatBlock();
            foreach (var id in Run.Grafts) { var g = R.Graft(id); if (g != null) mods.AddAll(g.Mods); }
            foreach (var rama in R.Ramas) if (FruitActive(rama.Planet)) mods.AddAll(rama.Fruit.Mods);
            foreach (var id in Run.Mutations) { var m = R.Mutation(id); if (m != null) mods.AddAll(m.Mods); }
            foreach (var id in Run.Curses[Math.Min(Run.Zone, Run.Curses.Count - 1)]) { var c = R.Curse(id); if (c != null) mods.AddAll(c.Mods); }
            mods.AddAll(RootMods());
            mods.AddAll(OfrendaBonus());
            mods.AddAll(_g.Story.CombatMods());
            foreach (var kv in Run.RunMods) mods.Add(kv.Key, kv.Value);
            return mods;
        }

        // ------------------------------------------------------------------ Mapa

        public List<MapNode> AvailableNodes()
        {
            var list = new List<MapNode>();
            if (Run == null || Run.Phase != RunPhase.ChooseNode) return list;
            if (Run.Floor < 0) { list.AddRange(Map.Floor(Run.Zone, 0)); return list; }
            var node = CurrentNode;
            if (node == null || Run.Floor + 1 >= R.Map.Floors) return list;
            foreach (var lane in node.Next) list.Add(Map.Node(Run.Zone, Run.Floor + 1, lane));
            return list;
        }

        public OpResult EnterNode(int lane)
        {
            if (Run == null || Run.Phase != RunPhase.ChooseNode) return OpResult.Fail("Ahora no puedes avanzar.");
            var target = AvailableNodes().Find(n => n.Lane == lane);
            if (target == null) return OpResult.Fail("Ese nodo no está conectado.");
            Run.Floor = target.Floor;
            Run.Lane = target.Lane;
            Run.Path.Add(target.Id);
            Log($"[Nodo {target.Id}] {NodeName(target.Type)}.");

            if (target.HasMancha && Run.Mancha != null && !Run.ManchaResolved)
            {
                Run.Loose += Run.Mancha.Almitas;
                Run.ManchaRecovered = Run.Mancha.Almitas;
                Run.ManchaResolved = true;
                Log($"[Mancha de Sal] Recuperas tus {Run.Mancha.Almitas:N0} almitas. Siguen sueltas: guárdalas en un Santuario.");
            }

            switch (target.Type)
            {
                case NodeType.Combate:
                case NodeType.Elite:
                case NodeType.Jefe:
                    Run.Phase = RunPhase.Battle;
                    break;
                case NodeType.Evento:
                    OpenChoice(ChoiceKind.Event, new List<string>(), target.EventId);
                    break;
                case NodeType.Santuario:
                {
                    long loose = Run.Loose;
                    Run.Loose = 0;
                    long banked = BankAlmitas(loose, "fresno.santuario");
                    Log($"[Santuario de Sal] Guardas {banked:N0} almitas. Descansa o entrena.");
                    OpenChoice(ChoiceKind.Santuario, new List<string> { "descansar", "entrenar" });
                    break;
                }
                case NodeType.Caldero:
                    OpenChoice(ChoiceKind.Caldero, CalderoOptions());
                    break;
                case NodeType.Mercader:
                    OpenMerchant();
                    break;
                case NodeType.Nido:
                    OpenNest();
                    AfterNode();
                    break;
                default:
                    AfterNode();
                    break;
            }
            return OpResult.Success(target.Id);
        }

        public static string NodeName(NodeType t)
        {
            switch (t)
            {
                case NodeType.Combate: return "Combate";
                case NodeType.Elite: return "Élite";
                case NodeType.Evento: return "Evento LitRPG";
                case NodeType.Santuario: return "Santuario de Sal";
                case NodeType.Caldero: return "Caldero";
                case NodeType.Mercader: return "Mercader de Almas";
                case NodeType.Nido: return "Nido de Almas";
                case NodeType.Mancha: return "Mancha de Sal";
                default: return "Jefe";
            }
        }

        private void OpenChoice(ChoiceKind kind, List<string> options, string eventId = "", List<long> prices = null)
        {
            Run.Pending = new PendingChoice { Kind = kind, Options = options, EventId = eventId ?? "", Prices = prices ?? new List<long>() };
            Run.Phase = RunPhase.Choice;
        }

        /// <summary>Después de resolver un nodo: el siguiente piso, la siguiente zona o el final de la run.</summary>
        private void AfterNode()
        {
            Run.Pending = new PendingChoice();
            var node = CurrentNode;
            if (node != null && node.Type == NodeType.Jefe)
            {
                if (Run.Zone + 1 >= R.Zones.Count) { EndRun(RunOutcome.Victory); return; }
                Run.Zone++;
                Run.Floor = -1;
                Run.Lane = R.Map.Lanes / 2;
                Run.Phase = RunPhase.Curses;
                Log($"[Zona] {Zone.Name}: {Zone.Theme}.");
                return;
            }
            Run.Phase = RunPhase.ChooseNode;
        }

        // ------------------------------------------------------------------ Combate

        /// <summary>Configuración del combate del nodo actual, con el progreso real de las brujas y todos los modificadores de la run.</summary>
        public BattleSetup PrepareBattle()
        {
            if (Run == null || Run.Phase != RunPhase.Battle) throw new InvalidOperationException("No hay combate en este nodo.");
            var node = CurrentNode;
            ulong seed = MapGenerator.Mix(Run.Seed, (ulong)(1000 + Run.Zone * 100 + node.Floor * 10 + node.Lane));
            var setup = _g.Progress.BuildSetup(Run.Team, seed);
            setup.Modifiers = RunModifiers();
            foreach (var t in Run.Trained)
                if (setup.Loadouts.TryGetValue(t.Witch, out var lo)) lo.Mods.Add(ModKeys.ChainMove(t.Role), R.TrainBonus);
            foreach (var id in Run.Team)
            {
                setup.StartHp[id] = Run.Hp[id];
                setup.StartSalinity[id] = Run.Salinity[id];
            }
            foreach (var slot in Run.Belt) setup.Belt.Add(new BeltSlot(slot.PotionId, slot.Charges));
            var ids = new List<string>();
            foreach (var s in Run.Belt) ids.Add(s.PotionId);
            setup.CustomPotions = Cauldron.CustomPotionsFor(_g.Meta, ids);
            BuildEncounter(setup, node, new SeededRandom(seed ^ 0xE11CUL));
            return setup;
        }

        private void BuildEncounter(BattleSetup setup, MapNode node, IRandomSource rng)
        {
            var zone = Zone;
            var enc = R.Encounters;
            var e = new EncounterDef { Id = $"fresno_{node.Id}", Name = $"{zone.Name} · {NodeName(node.Type)} {node.Id}" };
            double floorHp = 1.0 + enc.HpPerFloor * node.Floor;
            int levelAdd = (int)Math.Round(enc.LevelPerFloor * node.Floor);
            if (node.Type == NodeType.Jefe)
            {
                var boss = _g.Meta.Enemy(zone.Boss);
                Add(setup, e, Scaled(boss, $"{boss.Id}@{node.Id}", zone.HpScale, zone.AtkScale, 0, ""));
            }
            else
            {
                int count = enc.EnemiesByFloor[Math.Min(node.Floor, enc.EnemiesByFloor.Length - 1)];
                var pool = new List<string>(zone.Enemies);
                for (int i = 0; i < count; i++)
                {
                    if (pool.Count == 0) pool.AddRange(zone.Enemies);
                    string pick = pool[rng.NextInt(pool.Count)];
                    pool.Remove(pick);
                    var def = _g.Meta.Enemy(pick);
                    bool elite = node.Type == NodeType.Elite && i == 0;
                    Add(setup, e, Scaled(def, $"{def.Id}@{node.Id}#{i}", zone.HpScale * floorHp * (elite ? enc.EliteHp : 1.0),
                        zone.AtkScale * (elite ? enc.EliteAtk : 1.0), levelAdd, elite ? "Élite: " : ""));
                }
            }
            setup.CustomEncounter = e;
        }

        private static void Add(BattleSetup setup, EncounterDef e, EnemyDef def)
        {
            e.Enemies.Add(def.Id);
            setup.CustomEnemies.Add(def);
        }

        public static EnemyDef Scaled(EnemyDef src, string id, double hp, double atk, int levelAdd, string prefix)
        {
            var d = new EnemyDef
            {
                Id = id, Name = prefix + src.Name, ShortName = prefix + src.ShortName, Article = src.Article, Description = src.Description,
                Level = Math.Min(100, src.Level + levelAdd), MaxHp = Math.Max(1, (int)Math.Round(src.MaxHp * hp)), Atk = src.Atk * atk,
                Speed = src.Speed, Toughness = src.Toughness, AttackPlanet = src.AttackPlanet, IsBoss = src.IsBoss,
                ActionsPerTurn = src.ActionsPerTurn, Skills = src.Skills, Pattern = src.Pattern, Phase2 = src.Phase2,
            };
            d.Weaknesses.AddRange(src.Weaknesses);
            foreach (var kv in src.Res) d.Res[kv.Key] = kv.Value;
            d.Traits = src.Traits;
            return d;
        }

        /// <summary>Aplica el resultado de un combate terminado: vida y Salinidad que arrastra la run, cinturón, almitas y recompensas.</summary>
        public OpResult ResolveBattle(Battle battle)
        {
            if (Run == null || Run.Phase != RunPhase.Battle) return OpResult.Fail("No hay combate pendiente.");
            if (!battle.IsOver) return OpResult.Fail("El combate no ha terminado.");
            bool victory = battle.Outcome == BattleOutcome.Victory;
            if (victory) { _g.Quests.ReportBattle(battle); _g.Report("fresno_nodo"); }
            foreach (var w in battle.Witches)
            {
                double frac = w.MaxHp > 0 ? w.Hp / (double)w.MaxHp : 0;
                if (!w.IsAlive && victory) { frac = R.ReviveHpAfterWin; Log($"[Fresno] {w.Name} se levanta con {frac:P0} de vida."); }
                Run.Hp[w.Key] = frac;
                Run.Salinity[w.Key] = w.IsAlive ? w.Salinity : 0;
            }
            Run.Belt.Clear();
            foreach (var s in battle.Belt) if (s.Charges > 0) Run.Belt.Add(new BeltSlot(s.PotionId, s.Charges));
            if (battle.Almitas > 0)
            {
                Run.Loose += battle.Almitas;
                Run.Earned += battle.Almitas;
            }

            if (!victory)
            {
                Log($"[Fresno] Cae el aquelarre en el nodo {Run.NodeId}.");
                Die();
                return OpResult.Success("Derrota");
            }

            Run.CombatsWon++;
            var node = CurrentNode;
            var rw = R.Rewards;
            double mult = (1.0 + rw.CombatFloorBonus * node.Floor) * AlmitasMultiplier();
            if (node.Type == NodeType.Elite) mult *= rw.EliteMultiplier;
            if (node.Type == NodeType.Jefe) mult *= rw.BossMultiplier;
            long almitas = (long)Math.Round(Zone.Almitas * mult, MidpointRounding.AwayFromZero);
            Run.Loose += almitas;
            Run.Earned += almitas;
            AddPouch(Res.SalCruda(Planets.All[NextRng().NextInt(7)]), 1);
            Log($"[Victoria] +{almitas:N0} almitas sueltas (llevas {Run.Loose:N0} sin guardar).");

            switch (node.Type)
            {
                case NodeType.Combate:
                    OfferGrafts();
                    break;
                case NodeType.Elite:
                    if (rw.EliteBlackSalt > 0) _g.Ledger.Apply(Res.SalNegra, rw.EliteBlackSalt, "fresno.elite", "Élite " + node.Id);
                    OfferMutation();
                    break;
                case NodeType.Jefe:
                    ZoneCleared();
                    OfferGrafts();
                    break;
            }
            return OpResult.Success("Victoria");
        }

        private void ZoneCleared()
        {
            _g.Report("fresno_zona");
            var zone = Zone;
            double pct = RunModifiers().Get(ModKeys.SaviaPct);
            long savia = (long)Math.Round(zone.Savia * (1.0 + pct));
            if (savia > 0) _g.Ledger.Apply(Res.Savia, savia, "fresno.zona", zone.Name);
            Run.SaviaEarned += savia;
            if (!F.BossesDefeated.Contains(zone.Id)) F.BossesDefeated.Add(zone.Id);
            EnsureWeek();
            long salts = Math.Min(R.Weekly.BathSaltsPerBoss, R.Weekly.BathSaltsMax - F.WeekBathSalts);
            if (salts > 0)
            {
                _g.Ledger.Apply(Res.SalesDeBano, salts, "fresno.semanal", zone.Name);
                F.WeekBathSalts += salts;
            }
            Log($"[Zona superada] {zone.Name}: +{savia:N0} de Savia" + (salts > 0 ? $" y +{salts} Sales de Baño (semanal)." : "."));
        }

        // ------------------------------------------------------------------ Injertos y Mutaciones

        private void OfferGrafts()
        {
            var candidates = new List<string>();
            foreach (var rama in R.Ramas)
            {
                if (!RamaUnlocked(rama.Planet)) continue;
                foreach (var g in rama.Grafts) if (!g.Pending && !Run.Grafts.Contains(g.Id)) candidates.Add(g.Id);
            }
            var rng = NextRng();
            var options = new List<string>();
            int n = Math.Min(GraftChoiceCount(), candidates.Count);
            for (int i = 0; i < n; i++)
            {
                int k = rng.NextInt(candidates.Count);
                options.Add(candidates[k]);
                candidates.RemoveAt(k);
            }
            if (options.Count == 0) { AfterNode(); return; }
            OpenChoice(ChoiceKind.Graft, options);
        }

        public OpResult ChooseGraft(string id)
        {
            if (Run == null || Run.Phase != RunPhase.Choice || Run.Pending.Kind != ChoiceKind.Graft) return OpResult.Fail("No hay Injertos que elegir.");
            if (id != null)
            {
                if (!Run.Pending.Options.Contains(id)) return OpResult.Fail("Ese Injerto no está entre las opciones.");
                AddGraft(id);
            }
            AfterNode();
            return OpResult.Success(id ?? "sin Injerto");
        }

        private void AddGraft(string id)
        {
            var g = R.Graft(id);
            bool before = FruitActive(g.Rama);
            Run.Grafts.Add(id);
            Log($"[Injerto] {g.Name} ({R.Rama(g.Rama).Name}): {g.Description}.");
            if (!before && FruitActive(g.Rama))
            {
                var rama = R.Rama(g.Rama);
                Log($"[Fruto] ¡{rama.Fruit.Name}! {rama.Fruit.Description}");
            }
        }

        private void OfferMutation()
        {
            var candidates = new List<string>();
            foreach (var m in R.Mutations) if (!Run.Mutations.Contains(m.Id)) candidates.Add(m.Id);
            if (candidates.Count == 0) { AfterNode(); return; }
            OpenChoice(ChoiceKind.Mutation, new List<string> { candidates[NextRng().NextInt(candidates.Count)] });
        }

        public OpResult ChooseMutation(bool accept)
        {
            if (Run == null || Run.Phase != RunPhase.Choice || Run.Pending.Kind != ChoiceKind.Mutation) return OpResult.Fail("No hay Mutación que aceptar.");
            if (accept)
            {
                var m = R.Mutation(Run.Pending.Options[0]);
                Run.Mutations.Add(m.Id);
                Log($"[Mutación del Árbol] {m.Name}: {m.Description}.");
            }
            AfterNode();
            return OpResult.Success(accept ? "aceptada" : "rechazada");
        }

        // ------------------------------------------------------------------ Eventos LitRPG (GDD 13.2: umbrales fijos, sin dados)

        public static CalculistaStat StatFor(Planet p)
        {
            switch (p)
            {
                case Planet.Mercurio:
                case Planet.Jupiter: return CalculistaStat.Calculo;
                case Planet.Saturno:
                case Planet.Luna: return CalculistaStat.Temple;
                case Planet.Venus:
                case Planet.Sol: return CalculistaStat.Encanto;
                default: return CalculistaStat.Oficio;
            }
        }

        /// <summary>Estadística de la Calculista para un chequeo: su base más el rasgo de cada bruja del equipo.</summary>
        public int Stat(CalculistaStat stat)
        {
            if (_g.Story.Created) return _g.Story.Stat(stat, Run.Team);
            int v = R.CalculistaBase;
            foreach (var id in Run.Team)
            {
                var wp = _g.Profile.Witches[id];
                var r = _g.Meta.Roster[id];
                var dominant = wp.Via == Via.Mayor ? r.Primary : r.Secondary;
                if (StatFor(dominant) == stat) v += R.CalculistaWitchBonus;
            }
            return v;
        }

        public string OptionBlocker(EventOption opt)
        {
            if (opt.Stat.HasValue && Stat(opt.Stat.Value) < opt.Need) return $"Necesitas {opt.Stat.Value} {opt.Need} (tienes {Stat(opt.Stat.Value)}).";
            if (opt.CostAlmitas > Run.Loose) return $"Faltan almitas sueltas ({Run.Loose:N0}/{opt.CostAlmitas:N0}).";
            return "";
        }

        public OpResult ChooseEventOption(int index)
        {
            if (Run == null || Run.Phase != RunPhase.Choice || Run.Pending.Kind != ChoiceKind.Event) return OpResult.Fail("No hay evento.");
            var ev = R.Event(Run.Pending.EventId);
            if (ev == null || index < 0 || index >= ev.Options.Count) return OpResult.Fail("Opción inválida.");
            var opt = ev.Options[index];
            string blocker = OptionBlocker(opt);
            if (blocker.Length > 0) return OpResult.Fail(blocker);
            Run.Loose -= opt.CostAlmitas;
            Log($"[{ev.Name}] {opt.Text}.");
            Run.Pending = new PendingChoice();
            bool followUp = false;
            foreach (var o in opt.Outcomes)
            {
                switch (o.Type)
                {
                    case EventOutcomeType.Almitas:
                        Run.Loose += (long)o.Value;
                        Run.Earned += (long)o.Value;
                        break;
                    case EventOutcomeType.BlackSalt:
                        _g.Ledger.Apply(Res.SalNegra, (long)o.Value, "fresno.evento", ev.Name);
                        break;
                    case EventOutcomeType.Salinity:
                        foreach (var id in Run.Team) Run.Salinity[id] = Math.Max(0, Math.Min(149, Run.Salinity[id] + (int)o.Value));
                        break;
                    case EventOutcomeType.Heal:
                        foreach (var id in Run.Team) Run.Hp[id] = Math.Min(1.0, Run.Hp[id] + o.Value);
                        break;
                    case EventOutcomeType.RunMods:
                        foreach (var key in o.Mods.Keys) Run.RunMods[key] = (Run.RunMods.TryGetValue(key, out var v) ? v : 0) + o.Mods.Get(key);
                        Run.RunModNames.Add(o.Name);
                        break;
                    case EventOutcomeType.Curse:
                    {
                        var current = Run.Curses[Run.Zone];
                        var options = R.Curses.FindAll(c => !current.Contains(c.Id));
                        if (current.Count < R.MaxCursesPerZone && options.Count > 0)
                        {
                            var c = options[NextRng().NextInt(options.Count)];
                            current.Add(c.Id);
                            Log($"[Maldición] {c.Name}: almitas ×{CurseMultiplier(Run.Zone):0.###} en esta zona.");
                        }
                        break;
                    }
                    case EventOutcomeType.Savia:
                        _g.Ledger.Apply(Res.Savia, (long)o.Value, "fresno.zona", ev.Name);
                        break;
                    case EventOutcomeType.Graft:
                        followUp = true;
                        break;
                    case EventOutcomeType.Mutation:
                        followUp = true;
                        OfferMutation();
                        break;
                }
            }
            if (followUp && opt.Outcomes.Exists(o => o.Type == EventOutcomeType.Graft)) OfferGrafts();
            else if (!followUp) AfterNode();
            return OpResult.Success(opt.Text);
        }

        // ------------------------------------------------------------------ Santuario (GDD 10.2)

        public OpResult SantuarioRest()
        {
            if (Run == null || Run.Pending.Kind != ChoiceKind.Santuario) return OpResult.Fail("No estás en un Santuario.");
            double heal = R.RestHeal + RunModifiers().Get(ModKeys.SantuarioHealPct);
            foreach (var id in Run.Team)
            {
                Run.Hp[id] = Math.Min(1.0, Run.Hp[id] + heal);
                Run.Salinity[id] = Math.Max(0, Run.Salinity[id] + R.RestSalinity);
            }
            Log($"[Santuario] Descansan: {R.RestSalinity} de Salinidad y +{heal:P0} de vida a todas.");
            AfterNode();
            return OpResult.Success("descanso");
        }

        public OpResult SantuarioTrain(string witchId, ChainRole role)
        {
            if (Run == null || Run.Pending.Kind != ChoiceKind.Santuario) return OpResult.Fail("No estás en un Santuario.");
            if (!Run.Team.Contains(witchId)) return OpResult.Fail("Esa bruja no está en la run.");
            Run.Trained.Add(new TrainedMove(witchId, role));
            var def = _g.Combat.Witch(witchId);
            Log($"[Santuario] {def.ShortName} entrena su {role} ({def.MoveFor(role).Name}): +{R.TrainBonus:P0} hasta el final de la run.");
            AfterNode();
            return OpResult.Success("entrenamiento");
        }

        // ------------------------------------------------------------------ Mercader de Almas

        private void OpenMerchant()
        {
            var rng = NextRng();
            var options = new List<string>();
            var prices = new List<long>();
            var grafts = new List<string>();
            foreach (var rama in R.Ramas)
                if (RamaUnlocked(rama.Planet))
                    foreach (var g in rama.Grafts) if (!g.Pending && !Run.Grafts.Contains(g.Id)) grafts.Add(g.Id);
            for (int i = 0; i < 2 && grafts.Count > 0; i++)
            {
                int k = rng.NextInt(grafts.Count);
                options.Add("injerto:" + grafts[k]);
                prices.Add(R.MerchantGraftPrice * (Run.Zone + 1));
                grafts.RemoveAt(k);
            }
            var potions = new List<string>();
            foreach (var id in _g.Combat.PotionOrder) if (_g.Combat.Potion(id).UsableInCombat) potions.Add(id);
            options.Add("salmuera:" + potions[rng.NextInt(potions.Count)]);
            prices.Add(R.MerchantPotionPrice * (Run.Zone + 1));
            options.Add("curar");
            prices.Add(R.MerchantHealPrice * (Run.Zone + 1));
            OpenChoice(ChoiceKind.Mercader, options, "", prices);
        }

        public OpResult MerchantBuy(int index)
        {
            if (Run == null || Run.Pending.Kind != ChoiceKind.Mercader) return OpResult.Fail("No hay Mercader.");
            if (index < 0 || index >= Run.Pending.Options.Count) return OpResult.Fail("Opción inválida.");
            long price = Run.Pending.Prices[index];
            if (Run.Loose < price) return OpResult.Fail($"Faltan almitas sueltas ({Run.Loose:N0}/{price:N0}).");
            string option = Run.Pending.Options[index];
            if (option.StartsWith("salmuera:", StringComparison.Ordinal) && !AddToBelt(option.Substring(9)))
                return OpResult.Fail("El cinturón está lleno.");
            Run.Loose -= price;
            if (option.StartsWith("injerto:", StringComparison.Ordinal)) AddGraft(option.Substring(8));
            else if (option == "curar") foreach (var id in Run.Team) Run.Hp[id] = Math.Min(1.0, Run.Hp[id] + R.MerchantHealAmount);
            Log($"[Mercader] Compras {MerchantLabel(option)} por {price:N0} almitas sueltas.");
            Run.Pending.Options.RemoveAt(index);
            Run.Pending.Prices.RemoveAt(index);
            return OpResult.Success(option);
        }

        public string MerchantLabel(string option)
        {
            if (option.StartsWith("injerto:", StringComparison.Ordinal)) return "el Injerto " + R.Graft(option.Substring(8)).Name;
            if (option.StartsWith("salmuera:", StringComparison.Ordinal)) return Cauldron.NameOf(_g.Meta, option.Substring(9));
            return $"curación (+{R.MerchantHealAmount:P0} de vida a todas)";
        }

        public OpResult LeaveChoice()
        {
            if (Run == null || Run.Phase != RunPhase.Choice) return OpResult.Fail("No hay nada que dejar.");
            if (Run.Pending.Kind == ChoiceKind.Graft) return ChooseGraft(null);
            if (Run.Pending.Kind == ChoiceKind.Mutation) return ChooseMutation(false);
            if (Run.Pending.Kind == ChoiceKind.Santuario || Run.Pending.Kind == ChoiceKind.Event) return OpResult.Fail("Elige una opción.");
            AfterNode();
            return OpResult.Success();
        }

        // ------------------------------------------------------------------ Caldero y Nido

        private void AddPouch(string resource, int amount)
        {
            Run.Pouch[resource] = (Run.Pouch.TryGetValue(resource, out var v) ? v : 0) + amount;
        }

        /// <summary>Recetas que se pueden preparar con la bolsa de la run, más "turbia" (mezclar sin receta).</summary>
        public List<string> CalderoOptions()
        {
            var list = new List<string>();
            var src = new PouchSource(this);
            foreach (var r in _g.Meta.Brewing.Recipes)
            {
                if (!_g.Combat.Potion(r.Potion).UsableInCombat || r.SaltKind == BrewSaltKind.Refined) continue;
                var salt = SaltFor(r);
                var cost = _g.Cauldron.Cost(r.Base, salt, r.Reagent);
                if (src.CanAfford(cost)) list.Add(r.Potion);
            }
            list.Add("turbia");
            return list;
        }

        private static BrewSalt SaltFor(BrewRecipeDef r)
        {
            switch (r.SaltKind)
            {
                case BrewSaltKind.Planet: return BrewSalt.Of(r.Planet, 0);
                case BrewSaltKind.BlackSalt: return BrewSalt.Black;
                case BrewSaltKind.Almitas: return BrewSalt.Souls;
                default: return BrewSalt.NoSalt;
            }
        }

        public OpResult CalderoBrew(string option)
        {
            if (Run == null || Run.Pending.Kind != ChoiceKind.Caldero) return OpResult.Fail("No hay Caldero.");
            if (!Run.Pending.Options.Contains(option)) return OpResult.Fail("Esa preparación no está disponible.");
            if (FreeBeltSlotFor(null) < 0) return OpResult.Fail("El cinturón está lleno.");
            BrewResult result;
            var src = new PouchSource(this);
            if (option == "turbia")
            {
                string salt = null, reagent = null;
                foreach (var kv in Run.Pouch) if (kv.Value > 0 && kv.Key.StartsWith("sal_cruda.", StringComparison.Ordinal)) { salt = kv.Key; break; }
                foreach (var r in _g.Meta.Brewing.Reagents) if (src.Have(r.Resource) > 0) { reagent = r.Id; break; }
                if (salt == null || reagent == null) return OpResult.Fail("No hay ingredientes para mezclar.");
                Planets.TryParse(salt.Substring("sal_cruda.".Length), out var planet);
                result = _g.Cauldron.BrewWith(src, "agua_termal", BrewSalt.Of(planet, 0), reagent, "salmuera");
            }
            else
            {
                var r = _g.Meta.Brewing.Recipes.Find(x => x.Potion == option);
                result = _g.Cauldron.BrewWith(src, r.Base, SaltFor(r), r.Reagent, "salmuera");
            }
            if (!result.Ok) return OpResult.Fail(result.Message);
            AddToBelt(result.PotionId);
            Log($"[Caldero] {result.Message}");
            AfterNode();
            return OpResult.Success(result.PotionId);
        }

        private int FreeBeltSlotFor(string potionId)
        {
            for (int i = 0; i < Run.Belt.Count; i++)
                if (potionId != null && Run.Belt[i].PotionId == potionId && Run.Belt[i].Charges < BeltCharges) return i;
            return Run.Belt.Count < BeltSlots ? Run.Belt.Count : -1;
        }

        private bool AddToBelt(string potionId)
        {
            int i = FreeBeltSlotFor(potionId);
            if (i < 0) return false;
            if (i < Run.Belt.Count) Run.Belt[i].Charges++;
            else Run.Belt.Add(new BeltSlot(potionId, 1));
            return true;
        }

        private void OpenNest()
        {
            var rng = NextRng();
            long almitas = (long)Math.Round(Zone.Almitas * R.Rewards.NidoMultiplier * AlmitasMultiplier(), MidpointRounding.AwayFromZero);
            Run.Loose += almitas;
            Run.Earned += almitas;
            var planet = Planets.All[rng.NextInt(7)];
            if (R.Rewards.NidoSalt > 0) _g.Ledger.Apply(Res.SalCruda(planet), R.Rewards.NidoSalt, "fresno.nido", "Nido " + Run.NodeId);
            var reagents = _g.Meta.Brewing.Reagents;
            AddPouch(reagents[rng.NextInt(reagents.Count)].Resource, 1);
            AddPouch(Res.SalCruda(Planets.All[rng.NextInt(7)]), 1);
            AddPouch(Res.Mat(rng.NextDouble() < 0.5 ? "vinagre" : "leche_lunar"), 1);
            Log($"[Nido de Almas] +{almitas:N0} almitas sueltas, {R.Rewards.NidoSalt} {Planets.SaltName(planet)} y ingredientes para el Caldero.");
        }

        /// <summary>Ingredientes de una run: la bolsa para sales y materiales, las almitas sueltas para las almitas.</summary>
        private sealed class PouchSource : IIngredientSource
        {
            private readonly FresnoService _f;
            public PouchSource(FresnoService f) { _f = f; }

            public long Have(string resource)
            {
                if (resource == Res.Almitas) return _f.Run.Loose;
                if (_f.Run.Pouch.TryGetValue(resource, out var v)) return v;
                return resource == Res.Savia || resource == Res.SalNegra ? _f._g.Ledger.Balance(resource) : 0;
            }

            public bool CanAfford(Bundle cost)
            {
                foreach (var kv in cost.Items) if (Have(kv.Key) < kv.Value) return false;
                return true;
            }

            public bool TrySpend(Bundle cost, string flow, string note)
            {
                if (!CanAfford(cost)) return false;
                foreach (var kv in cost.Items)
                {
                    if (kv.Key == Res.Almitas) _f.Run.Loose -= kv.Value;
                    else if (_f.Run.Pouch.ContainsKey(kv.Key)) _f.Run.Pouch[kv.Key] -= (int)kv.Value;
                    else _f._g.Ledger.Apply(kv.Key, -kv.Value, flow, note);
                }
                return true;
            }
        }

        // ------------------------------------------------------------------ Muerte, final y Eco

        /// <summary>
        /// Cae todo el equipo (GDD 10.4): las almitas sueltas quedan como Mancha de Sal en este nodo. Si la Mancha de la
        /// run anterior no se recuperó, el Fresno la absorbe y devuelve el 30 % como Savia.
        /// </summary>
        private void Die()
        {
            long loose = Run.Loose;
            Run.Loose = 0;
            if (loose > 0)
            {
                F.Mancha = new ManchaState { Zone = Run.Zone, Floor = Math.Max(0, Run.Floor), Lane = Run.Lane, Almitas = loose, FromRun = Run.RunNumber };
                Log($"[Mancha de Sal] {loose:N0} almitas quedan en el nodo {Run.NodeId}. Tienes un intento para recuperarlas.");
            }
            EndRun(RunOutcome.Defeat);
        }

        /// <summary>Abandonar cuenta como caer: las almitas sueltas se quedan en el árbol.</summary>
        public OpResult Abandon()
        {
            if (Run == null || Run.Phase == RunPhase.EcoSave || Run.Phase == RunPhase.Finished) return OpResult.Fail("No hay run en curso.");
            Log("[Fresno] Abandonas la run.");
            long loose = Run.Loose;
            Run.Loose = 0;
            if (loose > 0)
                F.Mancha = new ManchaState { Zone = Run.Zone, Floor = Math.Max(0, Run.Floor), Lane = Run.Lane, Almitas = loose, FromRun = Run.RunNumber };
            EndRun(RunOutcome.Abandoned);
            return OpResult.Success();
        }

        private void EndRun(RunOutcome outcome)
        {
            Run.Outcome = outcome;
            if (outcome == RunOutcome.Victory) _g.Report("fresno_run");
            _g.Analytics.Track(World.AnalyticsEvents.FresnoRunEnd, outcome + ":" + Run.NodeId);
            if (Run.Mancha != null && !Run.ManchaResolved)
            {
                long savia = (long)Math.Round(Run.Mancha.Almitas * R.ManchaReturn, MidpointRounding.AwayFromZero);
                if (savia > 0) _g.Ledger.Apply(Res.Savia, savia, "fresno.mancha", $"Mancha de la run {Run.Mancha.FromRun}");
                Run.SaviaFromMancha = savia;
                Run.SaviaEarned += savia;
                Run.ManchaResolved = true;
                Log($"[Mancha absorbida] El Fresno se queda con tus {Run.Mancha.Almitas:N0} almitas y te devuelve {savia:N0} de Savia.");
            }
            if (outcome == RunOutcome.Victory)
            {
                long loose = Run.Loose;
                Run.Loose = 0;
                long banked = BankAlmitas(loose, "fresno.cima");
                long bonus = R.Rewards.FullClearSavia;
                if (bonus > 0) _g.Ledger.Apply(Res.Savia, bonus, "fresno.zona", "Copa Astral superada");
                Run.SaviaEarned += bonus;
                F.FullClears++;
                Log($"[La Copa] Vences a la Jardinera. Guardas {banked:N0} almitas y +{bonus:N0} de Savia.");
            }
            // Las Salmueras que sobran vuelven al inventario (máximo cinco de cada tipo).
            foreach (var slot in Run.Belt)
            {
                int have = _g.Profile.Potions.TryGetValue(slot.PotionId, out var n) ? n : 0;
                int back = Math.Min(_g.Combat.Rules.Potions.MaxPerType, have + slot.Charges);
                if (back > 0) _g.Profile.Potions[slot.PotionId] = back;
            }
            Run.Belt.Clear();
            F.History.Add(new RunSummary
            {
                Run = Run.RunNumber,
                Outcome = outcome.ToString(),
                ReachedNode = Run.NodeId,
                AlmitasBanked = Run.Banked,
                ManchaLeft = F.Mancha != null && F.Mancha.FromRun == Run.RunNumber ? F.Mancha.Almitas : 0,
                ManchaRecovered = Run.ManchaRecovered,
                SaviaFromMancha = Run.SaviaFromMancha,
                SaviaEarned = Run.SaviaEarned,
                EcoUsed = Run.EcoName,
                Grafts = new List<string>(Run.Grafts),
            });
            Run.Phase = RunPhase.EcoSave;
            Run.Pending = new PendingChoice();
        }

        public int EcoSlotCount => R.EcoSlots;

        /// <summary>Guarda un Eco: hasta tres Injertos y un movimiento entrenado de la run que terminó (GDD 10.5).</summary>
        public OpResult SaveEco(int slot, IReadOnlyList<string> grafts, TrainedMove trained, string name = null)
        {
            if (Run == null || Run.Phase != RunPhase.EcoSave) return OpResult.Fail("El Eco se guarda al terminar una run.");
            if (slot < 0 || slot >= R.EcoSlots) return OpResult.Fail("Ranura inválida.");
            if (!EcoSlotUnlocked(slot)) return OpResult.Fail("Esa ranura de Eco todavía no se gana.");
            grafts = grafts ?? Array.Empty<string>();
            if (grafts.Count > R.EcoGrafts) return OpResult.Fail($"Un Eco guarda {R.EcoGrafts} Injertos.");
            foreach (var g in grafts) if (!Run.Grafts.Contains(g)) return OpResult.Fail("Ese Injerto no es de esta run: " + g);
            var eco = new EcoSave { Name = name ?? $"Run {Run.RunNumber}", FromRun = Run.RunNumber };
            eco.Grafts.AddRange(grafts);
            if (trained != null)
            {
                if (!Run.Trained.Exists(t => t.Witch == trained.Witch && t.Role == trained.Role)) return OpResult.Fail("Ese movimiento no se entrenó en esta run.");
                eco.Trained.Add(new TrainedMove(trained.Witch, trained.Role));
            }
            while (F.Eco.Count < R.EcoSlots) F.Eco.Add(null);
            F.Eco[slot] = eco;
            FinishRun();
            return OpResult.Success("Eco guardado en la ranura " + (slot + 1));
        }

        public OpResult SkipEco()
        {
            if (Run == null || Run.Phase != RunPhase.EcoSave) return OpResult.Fail("No hay run terminada.");
            FinishRun();
            return OpResult.Success();
        }

        private void FinishRun()
        {
            Run.Phase = RunPhase.Finished;
            F.RunsFinished++;
            F.Run = null;
            _map = null;
        }
    }
}
