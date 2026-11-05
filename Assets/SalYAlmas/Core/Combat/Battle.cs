using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    /// <summary>
    /// Simulación determinista de un combate. Recibe órdenes (BattleCommand) y produce eventos (BattleEvent).
    /// No depende de Unity: con la misma configuración, la misma semilla y las mismas órdenes da siempre los
    /// mismos eventos.
    /// </summary>
    public sealed partial class Battle
    {
        public const int GuestId = 9;
        public const int FirstEnemyId = 100;

        public GameDatabase Db { get; }
        public CombatRules Rules { get; }
        public BattleSetup Setup { get; }
        public EncounterDef Encounter { get; }

        private readonly IRandomSource _rng;
        private readonly List<BattleEvent> _events = new List<BattleEvent>();
        private readonly List<WitchUnit> _witches = new List<WitchUnit>();
        private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();
        private readonly List<BattleCommand> _commandLog = new List<BattleCommand>();
        private readonly List<BeltSlot> _belt = new List<BeltSlot>();
        private readonly HashSet<int> _deathAnnounced = new HashSet<int>();

        public IReadOnlyList<BattleEvent> Events => _events;
        public IReadOnlyList<WitchUnit> Witches => _witches;
        public IReadOnlyList<EnemyUnit> Enemies => _enemies;
        public IReadOnlyList<BattleCommand> CommandLog => _commandLog;
        public IReadOnlyList<BeltSlot> Belt => _belt;
        public WitchUnit Guest { get; }

        public int Grains { get; internal set; }
        public int Resonance { get; internal set; }
        public int BlackSalt { get; internal set; }
        public int Almitas { get; private set; }
        public int TotalSparks { get; private set; }
        public int TurnNumber { get; private set; }
        public int Cycle { get; private set; } = 1;
        public double ElapsedAv { get; private set; }
        public DecisionKind Pending { get; private set; } = DecisionKind.None;
        public Unit ActiveUnit { get; private set; }
        public BattleOutcome Outcome { get; private set; } = BattleOutcome.None;
        public bool GuestClosingUsed { get; private set; }
        public bool FeignDeathUsed { get; private set; }
        public bool IsOver => Pending == DecisionKind.BattleOver;
        public bool IsExtraTurn => _isExtraTurn;
        public bool PotionUsedThisTurn => _potionsThisTurn >= Rules.Potions.PerTurn;
        public bool SelfCastUsedThisTurn => _selfCastThisTurn;

        private bool _started;
        private int _potionsThisTurn;
        private bool _selfCastThisTurn;
        private bool _magicThisTurn;
        private bool _isExtraTurn;
        private bool _extraTurnRequested;
        private Unit _extraTurnUnit;

        // Ascensiones: Aspecto con la anterior de la misma ronda y Spark si se encima con otra.
        private int _lastAscensionCaster = -1;
        private int _lastAscensionCycle = -1;
        private readonly List<KeyValuePair<int, int>> _ascensionWindowHits = new List<KeyValuePair<int, int>>();
        private int _ascensionWindowSparkResonance;

        public Battle(GameDatabase db, BattleSetup setup, IRandomSource rng = null)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Setup = setup?.Clone() ?? throw new ArgumentNullException(nameof(setup));
            Rules = db.Rules;
            _rng = rng ?? new SeededRandom(setup.Seed);

            _mods = Setup.Modifiers ?? new StatBlock();

            var errors = new List<string>();
            if (Setup.Team.Count < 1 || Setup.Team.Count > 4) errors.Add("El equipo lleva de una a cuatro brujas.");
            if (Setup.CustomEncounter == null && !db.Encounters.ContainsKey(Setup.EncounterId ?? "")) errors.Add("Encuentro desconocido: " + Setup.EncounterId);
            if (Setup.CustomEncounter != null)
                foreach (var id in Setup.CustomEncounter.Enemies)
                    if (FindEnemyDef(id) == null) errors.Add("Enemigo desconocido: " + id);
            var seen = new HashSet<string>();
            foreach (var id in Setup.Team)
            {
                if (!db.Witches.ContainsKey(id)) errors.Add("Bruja desconocida: " + id);
                if (!seen.Add(id)) errors.Add("Bruja repetida en el equipo: " + id);
            }
            if (Setup.GuestId != null)
            {
                if (!db.Witches.ContainsKey(Setup.GuestId)) errors.Add("Invitada desconocida: " + Setup.GuestId);
                if (seen.Contains(Setup.GuestId)) errors.Add("La Invitada no puede estar también en el equipo.");
            }
            if (Setup.Belt.Count > BeltSlots) errors.Add($"El cinturón tiene {BeltSlots} ranuras.");
            foreach (var slot in Setup.Belt)
            {
                if (PotionDef(slot.PotionId) == null) errors.Add("Poción desconocida: " + slot.PotionId);
                if (slot.Charges < 0 || slot.Charges > Rules.Potions.MaxPerType) errors.Add($"Cada tipo de poción lleva de 0 a {Rules.Potions.MaxPerType}.");
            }
            if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

            Encounter = Setup.CustomEncounter ?? db.Encounter(Setup.EncounterId);
            for (int i = 0; i < Setup.Team.Count; i++)
                _witches.Add(CreateWitch(db.Witch(Setup.Team[i]), i + 1, i, false));
            if (Setup.GuestId != null)
                Guest = CreateWitch(db.Witch(Setup.GuestId), GuestId, 4, true);
            for (int i = 0; i < Encounter.Enemies.Count; i++)
                _enemies.Add(CreateEnemy(FindEnemyDef(Encounter.Enemies[i]), FirstEnemyId + i, i));
            foreach (var slot in Setup.Belt) _belt.Add(new BeltSlot(slot.PotionId, slot.Charges));
        }

        private WitchUnit CreateWitch(WitchDef def, int id, int slot, bool guest)
        {
            Setup.Loadouts.TryGetValue(def.Id, out var lo);
            var mods = lo?.Mods ?? new StatBlock();
            // Los modificadores de equipo (Injertos, Raíces) no tocan a la Invitada: es la bruja de otra persona.
            double teamAtk = guest ? 0 : _mods.Get(ModKeys.AtkPct);
            double teamHp = guest ? 0 : _mods.Get(ModKeys.HpPct);
            double teamSpeed = guest ? 0 : _mods.Get(ModKeys.SpeedFlat);
            int maxHp = def.MaxHp;
            if (lo != null || teamHp != 0)
                maxHp = Math.Max(1, (int)Math.Round(def.MaxHp * (lo?.HpMultiplier ?? 1.0) * (1.0 + mods.Get(ModKeys.HpPct) + teamHp), MidpointRounding.AwayFromZero));
            double atk = (def.Atk * (lo?.AtkMultiplier ?? 1.0) + mods.Get(ModKeys.FlatAtk)) * (1.0 + mods.Get(ModKeys.AtkPct) + teamAtk);
            double speed = def.Speed * (lo?.SpeedMultiplier ?? 1.0) + mods.Get(ModKeys.SpeedFlat) + teamSpeed;

            var w = new WitchUnit
            {
                Id = id,
                Key = def.Id,
                Name = string.IsNullOrEmpty(def.ShortName) ? def.Name : def.ShortName,
                Side = Side.Ally,
                Slot = slot,
                Level = lo?.Level ?? def.Level,
                MaxHp = maxHp,
                Hp = maxHp,
                BaseAtk = atk * def.PassiveAtkMultiplier,
                BaseSpeed = speed,
                Distance = Rules.TurnOrder.AvBase,
                Def = def,
                IsGuest = guest,
                Grade = Setup.GradeOverrides.TryGetValue(def.Id, out var g) ? g : def.Grade,
                Via = Setup.ViaOverrides.TryGetValue(def.Id, out var v) ? v : def.Via,
                FrameShift = (Setup.FrameShifts.TryGetValue(def.Id, out var fs) ? fs : 0) + (int)Math.Round(mods.Get(ModKeys.FrameShift)),
                Loadout = lo,
            };
            if (lo?.BasicHits != null && lo.BasicHits.Count > 0)
            {
                var basic = new MoveDef
                {
                    Id = def.Basic.Id, Name = def.Basic.Name, Kind = def.Basic.Kind, Target = def.Basic.Target,
                    Multiplier = def.Basic.Multiplier, Toughness = def.Basic.Toughness, ByAscendant = def.Basic.ByAscendant,
                    Effects = def.Basic.Effects, Description = def.Basic.Description,
                };
                foreach (var h in lo.BasicHits) basic.Hits.Add(new HitDef(h.Frame, h.Share));
                w.BasicOverride = basic;
            }
            if (!guest)
            {
                if (Setup.StartHp.TryGetValue(def.Id, out var frac))
                    w.Hp = Math.Max(1, Math.Min(w.MaxHp, (int)Math.Round(w.MaxHp * frac, MidpointRounding.AwayFromZero)));
                if (Setup.StartSalinity.TryGetValue(def.Id, out var sal))
                    w.Salinity = Math.Max(0, Math.Min(Rules.Salinity.ExtasisAt - 1, sal));
            }
            return w;
        }

        private EnemyUnit CreateEnemy(EnemyDef def, int id, int slot)
        {
            int hp = def.MaxHp;
            if (_mods.Has(ModKeys.EnemyHpPct)) hp = Math.Max(1, (int)Math.Round(hp * (1.0 + _mods.Get(ModKeys.EnemyHpPct)), MidpointRounding.AwayFromZero));
            double tough = def.Toughness * (1.0 + _mods.Get(ModKeys.EnemyToughnessPct));
            return new EnemyUnit
            {
                Id = id,
                Key = def.Id,
                Name = string.IsNullOrEmpty(def.ShortName) ? def.Name : def.ShortName,
                Side = Side.Enemy,
                Slot = slot,
                Level = def.Level,
                MaxHp = hp,
                Hp = hp,
                BaseAtk = def.Atk * (1.0 + _mods.Get(ModKeys.EnemyAtkPct)),
                BaseSpeed = def.Speed * (1.0 + _mods.Get(ModKeys.EnemySpeedPct)),
                Distance = Rules.TurnOrder.AvBase,
                Def = def,
                Toughness = tough,
                MaxToughness = tough,
            };
        }

        // ------------------------------------------------------------------ Fase 2: modificadores

        private readonly StatBlock _mods;

        /// <summary>Modificadores de todo el combate (Injertos, Mutaciones, maldiciones, Raíces).</summary>
        public StatBlock TeamModifiers => _mods;

        /// <summary>Ranuras del cinturón: las del GDD más las de las Raíces.</summary>
        public int BeltSlots => Rules.Potions.BeltSlots + (int)Math.Round(_mods.Get(ModKeys.ExtraBeltSlots));

        /// <summary>Valor de un modificador para una unidad: el del equipo más el de su arma, prefijo y progreso.</summary>
        public double Mod(Unit u, string key)
        {
            if (!(u is WitchUnit w)) return 0;
            double team = w.IsGuest ? 0 : _mods.Get(key);
            return team + (w.Loadout?.Mods.Get(key) ?? 0);
        }

        public EnemyDef FindEnemyDef(string id)
        {
            if (id == null) return null;
            foreach (var e in Setup.CustomEnemies) if (e.Id == id) return e;
            return Db.Enemies.TryGetValue(id, out var def) ? def : null;
        }

        public PotionDef PotionDef(string id)
        {
            if (id == null) return null;
            foreach (var p in Setup.CustomPotions) if (p.Id == id) return p;
            return Db.Potions.TryGetValue(id, out var def) ? def : null;
        }

        // ------------------------------------------------------------------ Consultas

        public Unit FindUnit(int id)
        {
            foreach (var w in _witches) if (w.Id == id) return w;
            foreach (var e in _enemies) if (e.Id == id) return e;
            if (Guest != null && Guest.Id == id) return Guest;
            return null;
        }

        public WitchUnit FindWitch(int id) => FindUnit(id) as WitchUnit;
        public EnemyUnit FindEnemy(int id) => FindUnit(id) as EnemyUnit;

        public IEnumerable<Unit> UnitsOnField()
        {
            foreach (var w in _witches) yield return w;
            foreach (var e in _enemies) yield return e;
        }

        public List<EnemyUnit> AliveEnemies()
        {
            var list = new List<EnemyUnit>();
            foreach (var e in _enemies) if (e.IsAlive) list.Add(e);
            return list;
        }

        public List<WitchUnit> AliveWitches()
        {
            var list = new List<WitchUnit>();
            foreach (var w in _witches) if (w.IsAlive) list.Add(w);
            return list;
        }

        public double Speed(Unit u)
        {
            double pct = u.Sum(StatusKind.SpeedPct);
            double flat = u.Sum(StatusKind.SpeedFlat);
            double baseSpeed = u.BaseSpeed * StatRatio(u);
            return Math.Max(1.0, baseSpeed * (1.0 + pct) + flat);
        }

        public double Atk(Unit u) => Math.Max(0.0, u.BaseAtk * StatRatio(u) * (1.0 + u.Sum(StatusKind.AtkPct)));

        /// <summary>AV que le falta a una unidad para actuar.</summary>
        public double ActionValue(Unit u)
        {
            double d = u.Distance;
            if (u == ActiveUnit) d = Rules.TurnOrder.AvBase + u.PendingDistanceDelta;
            return Math.Max(0.0, d) / Speed(u);
        }

        /// <summary>Multiplicador de estadísticas del Vislumbre (siguiente grado / grado actual).</summary>
        private double StatRatio(Unit u)
        {
            if (!(u is WitchUnit w) || !w.InVislumbre) return 1.0;
            var m = Rules.Purity.GradeStatMultiplier;
            int g = (int)w.Grade;
            int n = (int)PurityMath.Next(w.Grade);
            if (m == null || n >= m.Length || g >= m.Length || m[g] <= 0) return 1.0;
            return m[n] / m[g];
        }

        public double Purity(WitchUnit w) => Rules.Purity.PurityOf(w.EffectiveGrade);

        public SalinityBand Band(WitchUnit w) => SalinityBands.FromValue(w.Salinity, Rules.Salinity);

        public double CritRate(WitchUnit w)
        {
            double rate = w.Def.CritRate + w.Sum(StatusKind.CritRate) + Mod(w, ModKeys.CritRate);
            if (Band(w) == SalinityBand.Salobre) rate += Rules.Salinity.SalobreCritRate;
            return Math.Max(0.0, Math.Min(1.0, rate));
        }

        /// <summary>ΣB del atacante (sin la marca del objetivo).</summary>
        public double DamageBonus(Unit u, bool byAscendant)
        {
            double b = u.Sum(StatusKind.DamagePct);
            if (u is WitchUnit w)
            {
                var band = Band(w);
                if (band == SalinityBand.Sobresalada) b += Rules.Salinity.SobresaladaDamage;
                if (band == SalinityBand.CristalizacionTurbia && byAscendant) b += Rules.Salinity.CristalizacionAscendantDamage;
                if (Setup.PlanetaryDay.HasValue && Setup.PlanetaryDay.Value == w.Dominant) b += Rules.Combat.PlanetaryDayDamage;
                if (w.InVislumbre && w.Grade == Grade.SS) b += Rules.Vislumbre.ApoteosisDamage;
                b += Mod(w, ModKeys.DamagePct) + Mod(w, ModKeys.PlanetDamage(w.Dominant));
                if (byAscendant) b += Mod(w, ModKeys.AscendantDamagePct);
            }
            return b;
        }

        public double DefenseReduction(Unit u)
        {
            double r = u.Sum(StatusKind.DefenseDown) + Mod(u, ModKeys.DefenseDown);
            if (u is WitchUnit w && Band(w) == SalinityBand.Sobresalada) r += Rules.Salinity.SobresaladaDefenseDown;
            return Math.Max(0.0, Math.Min(1.0, r));
        }

        public double Res(Unit u, Planet p)
        {
            double r = u.Sum(StatusKind.ResAll);
            if (u is EnemyUnit e) r += e.Def.ResFor(p);
            return r;
        }

        public bool GrainBoostActive
        {
            get
            {
                foreach (var w in _witches) if (w.IsAlive && w.Has(StatusKind.GrainBoost)) return true;
                return false;
            }
        }

        /// <summary>Próximos turnos según el AV actual, para la línea de turnos. No cambia el estado.</summary>
        public List<TurnOrderEntry> TurnOrderPreview(int count)
        {
            var result = new List<TurnOrderEntry>();
            var units = new List<Unit>();
            var dist = new Dictionary<int, double>();
            foreach (var u in UnitsOnField())
            {
                if (!u.IsAlive) continue;
                units.Add(u);
                dist[u.Id] = u == ActiveUnit ? Rules.TurnOrder.AvBase + u.PendingDistanceDelta : u.Distance;
            }
            if (units.Count == 0) return result;
            double elapsed = 0;
            if (ActiveUnit != null && ActiveUnit.IsAlive)
            {
                result.Add(new TurnOrderEntry(ActiveUnit, 0));
                if (_extraTurnRequested) result.Add(new TurnOrderEntry(ActiveUnit, 0));
            }
            while (result.Count < count)
            {
                Unit best = null;
                double bestAv = 0;
                foreach (var u in units)
                {
                    double av = Math.Max(0.0, dist[u.Id]) / Speed(u);
                    if (best == null || CompareTurn(u, av, best, bestAv) < 0) { best = u; bestAv = av; }
                }
                elapsed += bestAv;
                foreach (var u in units) dist[u.Id] = Math.Max(0.0, dist[u.Id] - bestAv * Speed(u));
                dist[best.Id] = Rules.TurnOrder.AvBase;
                result.Add(new TurnOrderEntry(best, elapsed));
            }
            return result;
        }

        /// <summary>
        /// Orden de la línea de turnos. Menor AV primero. Empates (dentro de TieEpsilon): primero las brujas,
        /// luego la de mayor Velocidad y al final la de menor posición. // TUNING: el GDD no define el desempate.
        /// </summary>
        private int CompareTurn(Unit a, double avA, Unit b, double avB)
        {
            if (Math.Abs(avA - avB) > Rules.TurnOrder.TieEpsilon) return avA.CompareTo(avB);
            if (a.Side != b.Side) return a.Side == Side.Ally ? -1 : 1;
            int s = Speed(b).CompareTo(Speed(a));
            if (s != 0) return s;
            return a.Slot.CompareTo(b.Slot);
        }

        // ------------------------------------------------------------------ Ciclo de vida

        public void Start()
        {
            if (_started) throw new InvalidOperationException("El combate ya empezó.");
            _started = true;

            var teamNames = new List<string>();
            foreach (var w in _witches) teamNames.Add(w.Name);
            var enemyNames = new List<string>();
            foreach (var e in _enemies) enemyNames.Add(e.Name);
            Emit(new BattleStarted
            {
                Seed = Setup.Seed,
                EncounterName = Encounter.Name,
                Team = teamNames,
                Guest = Guest?.Name ?? "",
                Enemies = enemyNames,
                Regencia = Guest != null ? Guest.Def.Regencia.Name : "",
            });

            Grains = Math.Min(Rules.Grains.Max, Rules.Grains.Start);
            BlackSalt = (Setup.BlackSalt ?? Rules.Combat.StartingBlackSalt) + (int)Math.Round(_mods.Get(ModKeys.ExtraBlackSalt));
            ApplyRegencia();
            ApplyStartModifiers();
            AdvanceToNextTurn();
        }

        private void ApplyRegencia()
        {
            if (Guest == null) return;
            var reg = Guest.Def.Regencia;
            string name = "Regencia: " + reg.Name;
            foreach (var w in _witches)
            {
                if (reg.TeamHp > 0)
                {
                    w.MaxHp = (int)Math.Round(w.MaxHp * (1.0 + reg.TeamHp), MidpointRounding.AwayFromZero);
                    w.Hp = w.MaxHp;
                }
                if (reg.TeamAtk > 0) AddStatus(w, StatusKind.AtkPct, reg.TeamAtk, -1, name, isBuff: true, dispellable: false, source: Guest);
                if (reg.TeamSpeedFlat > 0) AddStatus(w, StatusKind.SpeedFlat, reg.TeamSpeedFlat, -1, name, isBuff: true, dispellable: false, source: Guest);
            }
            if (reg.StartGrains > 0) AddGrains(reg.StartGrains, name, allowBoost: false);
            if (reg.StartResonance > 0) ChangeResonance(reg.StartResonance, name);
            if (reg.StartEnemyDelay > 0)
                foreach (var e in _enemies) ShiftAction(e, -reg.StartEnemyDelay, name);
        }

        /// <summary>Efectos de inicio de la Fase 2: Granos, Resonancia, Ánima, escudos y adelantos de Injertos y armas.</summary>
        private void ApplyStartModifiers()
        {
            int grains = (int)Math.Round(_mods.Get(ModKeys.StartGrains));
            foreach (var w in _witches) grains += (int)Math.Round(w.Loadout?.Mods.Get(ModKeys.StartGrains) ?? 0);
            if (grains != 0) AddGrains(grains, "Injertos y armas", allowBoost: false);
            int res = (int)Math.Round(_mods.Get(ModKeys.StartResonance));
            if (res != 0) ChangeResonance(res, "Injertos");
            double shield = _mods.Get(ModKeys.StartShieldPct);
            double advance = _mods.Get(ModKeys.StartAdvance);
            foreach (var w in _witches)
            {
                int anima = (int)Math.Round(Mod(w, ModKeys.StartAnima));
                if (anima != 0) ChangeAnima(w, anima);
                if (shield > 0) AddShield(w, (int)Math.Round(w.MaxHp * shield, MidpointRounding.AwayFromZero), 2, "Espejo de Marea", w);
                if (advance > 0) ShiftAction(w, advance, "Mente Relámpago");
            }
        }

        private void AdvanceToNextTurn()
        {
            while (true)
            {
                if (CheckOutcome()) return;
                if (TurnNumber >= Rules.Combat.MaxTurns)
                {
                    EndBattle(BattleOutcome.Defeat, "Se agotó el límite de turnos");
                    return;
                }

                Unit next;
                bool extra = false;
                if (_extraTurnUnit != null && _extraTurnUnit.IsAlive)
                {
                    next = _extraTurnUnit;
                    extra = true;
                }
                else
                {
                    next = PickNext(out double elapsed);
                    PassTime(elapsed);
                    next.Distance = 0;
                }
                _extraTurnUnit = null;

                if (BeginTurn(next, extra)) return;
            }
        }

        private Unit PickNext(out double elapsed)
        {
            Unit best = null;
            double bestAv = 0;
            foreach (var u in UnitsOnField())
            {
                if (!u.IsAlive) continue;
                double av = Math.Max(0.0, u.Distance) / Speed(u);
                if (best == null || CompareTurn(u, av, best, bestAv) < 0) { best = u; bestAv = av; }
            }
            elapsed = bestAv;
            return best;
        }

        private void PassTime(double elapsed)
        {
            if (elapsed > 0)
            {
                foreach (var u in UnitsOnField())
                    if (u.IsAlive) u.Distance = Math.Max(0.0, u.Distance - elapsed * Speed(u));
                ElapsedAv += elapsed;
            }
            int cycle = CycleAt(ElapsedAv);
            if (cycle != Cycle)
            {
                Cycle = cycle;
                Emit(new CycleStarted { Cycle = cycle, ElapsedAv = ElapsedAv });
            }
        }

        /// <summary>Ciclo (ronda) al que pertenece un AV acumulado: el primero dura 150 y los siguientes 100 (GDD 9.2).</summary>
        public int CycleAt(double elapsedAv)
        {
            var r = Rules.TurnOrder;
            if (elapsedAv <= r.FirstCycleAv + r.TieEpsilon) return 1;
            return 2 + (int)Math.Floor((elapsedAv - r.FirstCycleAv - r.TieEpsilon) / r.CycleAv);
        }

        /// <summary>Empieza el turno. Devuelve false si el turno se resolvió solo (la unidad cayó o lo perdió).</summary>
        private bool BeginTurn(Unit u, bool extra)
        {
            TurnNumber++;
            ActiveUnit = u;
            _isExtraTurn = extra;
            _extraTurnRequested = false;
            if (!extra)
            {
                _potionsThisTurn = 0;
                _selfCastThisTurn = false;
                _magicThisTurn = false;
            }
            _ascensionWindowHits.Clear();
            _ascensionWindowSparkResonance = 0;

            Emit(new TurnStarted
            {
                UnitId = u.Id,
                UnitName = u.Name,
                Side = u.Side,
                TurnNumber = TurnNumber,
                Cycle = Cycle,
                ElapsedAv = ElapsedAv,
                IsExtra = extra,
            });

            if (!extra)
            {
                if (u is EnemyUnit e && e.Broken)
                {
                    e.Broken = false;
                    e.Toughness = e.MaxToughness;
                    Emit(new ToughnessRestored { UnitId = e.Id, UnitName = e.Name });
                }

                TickDots(u);
                if (!u.IsAlive)
                {
                    ActiveUnit = null;
                    return false;
                }

                if (u is WitchUnit w && Band(w) == SalinityBand.CristalizacionTurbia)
                {
                    int loss = (int)Math.Round(w.MaxHp * Rules.Salinity.CristalizacionHpLoss, MidpointRounding.AwayFromZero);
                    if (!Rules.Salinity.CristalizacionIsLethal) loss = Math.Min(loss, w.Hp - 1);
                    if (loss > 0)
                    {
                        w.Hp -= loss;
                        Emit(new DotTicked { UnitId = w.Id, UnitName = w.Name, StatusName = "Cristalización Turbia", Planet = w.Dominant, Damage = loss, Hp = w.Hp });
                        AnnounceDeath(w);
                    }
                    if (!w.IsAlive)
                    {
                        ActiveUnit = null;
                        return false;
                    }
                }

                var frozen = u.Find(StatusKind.Frozen);
                if (frozen != null)
                {
                    RemoveStatus(u, frozen, "consumido");
                    Emit(new TurnSkipped { UnitId = u.Id, UnitName = u.Name, Reason = "Helada Negra" });
                    EndTurn(u);
                    return false;
                }
            }

            if (u is WitchUnit witch)
                Pending = witch.InEcstasy ? DecisionKind.AutoTurn : DecisionKind.AllyTurn;
            else
                Pending = DecisionKind.EnemyTurn;
            return true;
        }

        private void EndTurn(Unit u)
        {
            if (u is WitchUnit w && !_isExtraTurn && w.IsAlive && !_magicThisTurn)
                ChangeSalinity(w, Rules.Salinity.TurnWithoutMagic, "turno sin magia");
            if (u is WitchUnit hw && !_isExtraTurn && hw.IsAlive)
            {
                double heal = Mod(hw, ModKeys.EndTurnHealPct);
                if (heal > 0) Heal(hw, (int)Math.Round(hw.MaxHp * heal, MidpointRounding.AwayFromZero), "Jardín Esmeralda");
            }

            if (!_isExtraTurn) TickStatuses(u);

            u.Distance = Math.Max(0.0, Rules.TurnOrder.AvBase + u.PendingDistanceDelta);
            u.PendingDistanceDelta = 0;

            if (_extraTurnRequested && u.IsAlive) _extraTurnUnit = u;
            _extraTurnRequested = false;
            ActiveUnit = null;
            Pending = DecisionKind.None;
        }

        /// <summary>Cierra el turno de la unidad activa y avanza al siguiente.</summary>
        private void FinishTurn()
        {
            if (CheckOutcome()) return;
            var u = ActiveUnit;
            if (u != null) EndTurn(u);
            AdvanceToNextTurn();
        }

        /// <summary>Después de una acción libre o una Ascensión: revisa si terminó el combate o si cayó la unidad activa.</summary>
        private void AfterFreeAction()
        {
            if (CheckOutcome()) return;
            if (ActiveUnit != null && !ActiveUnit.IsAlive)
            {
                ActiveUnit = null;
                AdvanceToNextTurn();
            }
        }

        private bool CheckOutcome()
        {
            if (Pending == DecisionKind.BattleOver) return true;
            bool anyEnemy = false, anyWitch = false;
            foreach (var e in _enemies) if (e.IsAlive) { anyEnemy = true; break; }
            foreach (var w in _witches) if (w.IsAlive) { anyWitch = true; break; }
            if (!anyEnemy) { EndBattle(BattleOutcome.Victory, "Todos los enemigos cayeron"); return true; }
            if (!anyWitch) { EndBattle(BattleOutcome.Defeat, "Cayó todo el aquelarre"); return true; }
            return false;
        }

        private void EndBattle(BattleOutcome outcome, string reason)
        {
            Outcome = outcome;
            Pending = DecisionKind.BattleOver;
            ActiveUnit = null;
            Emit(new BattleEnded
            {
                Outcome = outcome,
                Turns = TurnNumber,
                ElapsedAv = ElapsedAv,
                Sparks = TotalSparks,
                Almitas = Almitas,
                Reason = reason,
            });
        }

        internal void Emit(BattleEvent e)
        {
            e.Index = _events.Count;
            _events.Add(e);
        }

        private void AnnounceDeath(Unit u)
        {
            if (u.IsAlive || _deathAnnounced.Contains(u.Id)) return;
            _deathAnnounced.Add(u.Id);
            if (u is EnemyUnit e) e.ChargedSkill = null;
            Emit(new UnitDefeated { UnitId = u.Id, UnitName = u.Name, Side = u.Side });
        }

        // ------------------------------------------------------------------ Órdenes

        public CommandResult Execute(BattleCommand cmd)
        {
            if (cmd == null) return CommandResult.Fail("Orden vacía.");
            if (!_started) return CommandResult.Fail("El combate no ha empezado.");
            if (Pending == DecisionKind.BattleOver) return CommandResult.Fail("El combate terminó.");

            CommandResult result;
            switch (cmd.Type)
            {
                case CommandType.Advance: result = DoAdvance(); break;
                case CommandType.Ascension: result = DoAscension(cmd); break;
                case CommandType.Basic: result = DoBasic(cmd); break;
                case CommandType.Spell: result = DoSpell(cmd); break;
                case CommandType.SelfCast: result = DoSelfCast(cmd); break;
                case CommandType.Vislumbre: result = DoVislumbre(cmd); break;
                case CommandType.Potion: result = DoPotion(cmd); break;
                case CommandType.OpenChain: result = DoChain(cmd); break;
                default: result = CommandResult.Fail("Orden desconocida."); break;
            }
            if (result.Ok) _commandLog.Add(cmd);
            return result;
        }

        /// <summary>Vuelve a jugar un combate con la misma configuración y las mismas órdenes.</summary>
        public static Battle Replay(GameDatabase db, BattleSetup setup, IEnumerable<BattleCommand> commands)
        {
            var b = new Battle(db, setup);
            b.Start();
            foreach (var c in commands)
            {
                var r = b.Execute(c);
                if (!r.Ok) throw new InvalidOperationException("La repetición se desvió: " + c + " → " + r.Error);
            }
            return b;
        }

        private CommandResult RequireAllyTurn(int actorId, out WitchUnit actor)
        {
            actor = null;
            if (Pending != DecisionKind.AllyTurn) return CommandResult.Fail("No es el turno de una bruja.");
            if (ActiveUnit == null || ActiveUnit.Id != actorId) return CommandResult.Fail("Solo la bruja en turno puede hacer eso.");
            actor = (WitchUnit)ActiveUnit;
            if (!actor.IsAlive) return CommandResult.Fail(actor.Name + " no puede actuar.");
            return CommandResult.Success;
        }

        private CommandResult DoAdvance()
        {
            if (Pending == DecisionKind.EnemyTurn)
            {
                RunEnemyTurn((EnemyUnit)ActiveUnit);
                return CommandResult.Success;
            }
            if (Pending == DecisionKind.AutoTurn)
            {
                RunEcstasyTurn((WitchUnit)ActiveUnit);
                return CommandResult.Success;
            }
            return CommandResult.Fail("Es turno de una bruja: elige una acción.");
        }
    }
}
