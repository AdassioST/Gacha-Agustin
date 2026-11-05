using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.World
{
    /// <summary>Lo que la presentación necesita para mostrar una opción de la escena actual.</summary>
    public sealed class ChoiceView
    {
        public int Index;
        public StoryChoice Def;
        public string Blocker = "";
        public string CheckLabel = "";
        public bool Available => Blocker.Length == 0;
    }

    /// <summary>
    /// Motor de historia LitRPG (GDD 6 y 13): regiones en orden libre, misiones, decisiones con umbrales fijos y sin dados,
    /// marcas de Destino, Títulos, Almas digeridas, Senderos, Relectura, reloj de eclipses y Paralaje. Todo sale de datos.
    /// </summary>
    public sealed class StoryService
    {
        private readonly MetaGame _g;

        public StoryService(MetaGame game) { _g = game; }

        private StoryRules R => _g.Meta.World.Story;
        public CalculistaState C => _g.Profile.Calculista;
        public bool Created => C.Created;

        // ------------------------------------------------------------------ Calculista

        /// <summary>Crea a la Calculista. El signo define la región inicial y da +1 a una estadística por elemento.</summary>
        public OpResult Create(string name, string pronouns, string species, string signId)
        {
            if (C.Created) return OpResult.Fail("Ya existe.");
            name = (name ?? "").Trim();
            if (name.Length < 2 || name.Length > 12) return OpResult.Fail("El nombre lleva de 2 a 12 caracteres.");
            if (!R.Pronouns.Contains(pronouns)) return OpResult.Fail("Elige tus pronombres.");
            if (!R.Species.Contains(species)) return OpResult.Fail("Elige una especie.");
            var sign = R.Sign(signId);
            if (sign == null) return OpResult.Fail("Elige un signo.");
            C.Created = true;
            C.Name = name;
            C.Pronouns = pronouns;
            C.Species = species;
            C.Sign = signId;
            C.Stats.Clear();
            foreach (CalculistaStat s in Enum.GetValues(typeof(CalculistaStat))) C.Stats[s.ToString()] = R.StatBase;
            C.Stats[sign.Bonus.ToString()] += 1;
            C.Sendero = R.Senderos.Count > 0 ? R.Senderos[0].Id : "";
            _g.Analytics.Track(AnalyticsEvents.TutorialStep, "calculista_creada");
            return OpResult.Success(name);
        }

        public string StartRegion => R.Sign(C.Sign)?.Region ?? "";

        /// <summary>Estadística para un chequeo: la base, más el bono de cada bruja afín del equipo (GDD 13.2).</summary>
        public int Stat(CalculistaStat stat, IReadOnlyList<string> team = null)
        {
            int v = C.Stats.TryGetValue(stat.ToString(), out var b) ? b : R.StatBase;
            if (team != null)
                foreach (var id in team)
                {
                    if (!_g.Profile.Witches.TryGetValue(id, out var wp)) continue;
                    var ro = _g.Meta.Roster[id];
                    var dominant = wp.Via == Via.Mayor ? ro.Primary : ro.Secondary;
                    if (FresnoService.StatFor(dominant) == stat) v += R.WitchBonus;
                }
            return Math.Min(R.StatMax, v);
        }

        // ------------------------------------------------------------------ Paralaje (GDD 6.5 y HUECOS H-15)

        public BookBand Band
        {
            get
            {
                var b = R.ParalajeBands;
                if (C.Paralaje >= b[2]) return BookBand.Maximo;
                if (C.Paralaje >= b[1]) return BookBand.Alto;
                if (C.Paralaje >= b[0]) return BookBand.Medio;
                return BookBand.Bajo;
            }
        }

        public void AddParalaje(int amount) => C.Paralaje = Math.Max(0, Math.Min(R.ParalajeMax, C.Paralaje + amount));

        // ------------------------------------------------------------------ Regiones y misiones

        public bool RegionDone(string id) => C.RegionsDone.Contains(id);

        /// <summary>Todas las regiones del lanzamiento terminadas: se abre la Relectura.</summary>
        public bool GameFinished
        {
            get
            {
                foreach (var r in R.Regions) if (!r.Prologue && !RegionDone(r.Id)) return false;
                return true;
            }
        }

        public bool MissionDone(string mission) => C.MissionsDone.Contains(mission);

        public int MissionIndex(RegionDef region)
        {
            for (int i = 0; i < region.Missions.Count; i++) if (!MissionDone(region.Missions[i].Id)) return i;
            return region.Missions.Count;
        }

        public bool PrologueDone
        {
            get
            {
                foreach (var r in R.Regions) if (r.Prologue && !RegionDone(r.Id)) return false;
                return true;
            }
        }

        /// <summary>Por qué no se puede empezar. El reloj de eclipses nunca bloquea; solo se piden las misiones anteriores.</summary>
        public string CanStart(string regionId, string missionId)
        {
            var region = R.Region(regionId);
            if (region == null) return "Región desconocida.";
            int idx = region.Missions.FindIndex(m => m.Id == missionId);
            if (idx < 0) return "Misión desconocida.";
            if (!region.Prologue && !PrologueDone) return "Termina primero el prólogo.";
            if (MissionDone(missionId)) return GameFinished ? "" : "La Relectura se abre al terminar el juego.";
            for (int i = 0; i < idx; i++) if (!MissionDone(region.Missions[i].Id)) return "Falta " + region.Missions[i].Name + ".";
            return "";
        }

        public bool InMission => C.Mission.Length > 0;

        public RegionDef CurrentRegion => R.Region(C.MissionRegion);
        public MissionDef CurrentMission => CurrentRegion?.Missions.Find(m => m.Id == C.Mission);
        public StoryScene CurrentScene => CurrentMission != null && CurrentMission.Scenes.TryGetValue(C.Scene, out var s) ? s : null;

        public OpResult StartMission(string regionId, string missionId)
        {
            if (!C.Created) return OpResult.Fail("Crea primero a tu Calculista.");
            if (InMission) return OpResult.Fail("Ya hay una misión en curso.");
            string why = CanStart(regionId, missionId);
            if (why.Length > 0) return OpResult.Fail(why);
            var region = R.Region(regionId);
            var m = region.Missions.Find(x => x.Id == missionId);
            C.Reread = MissionDone(missionId);
            C.Mission = m.Id;
            C.MissionRegion = region.Id;
            if (!region.Prologue && !C.RegionsVisited.Contains(region.Id)) C.RegionsVisited.Add(region.Id);
            EnterScene(m.Start);
            return OpResult.Success(m.Name);
        }

        private void EnterScene(string id)
        {
            C.Scene = id;
            var scene = CurrentScene;
            Apply(scene.Effects);
        }

        private bool Meets(StoryCondition c, IReadOnlyList<string> team)
        {
            foreach (var m in c.Marks) if (!C.Marks.Contains(m)) return false;
            foreach (var m in c.NotMarks) if (C.Marks.Contains(m)) return false;
            if (c.Title.Length > 0 && !C.Titles.Contains(c.Title)) return false;
            if (c.Witch.Length > 0 && (team == null || !((IList<string>)team).Contains(c.Witch)) && !_g.Profile.Owns(c.Witch)) return false;
            if (c.MinSouls > C.Souls || c.MinParalaje > C.Paralaje) return false;
            if (c.MinCorruption > 0 && CorruptionOf(C.MissionRegion) < c.MinCorruption) return false;
            return true;
        }

        /// <summary>El texto de la escena: la última variante que se cumple, o el texto base. {nombre} y {signo} se sustituyen.</summary>
        public string SceneText(IReadOnlyList<string> team = null)
        {
            var scene = CurrentScene;
            if (scene == null) return "";
            string text = scene.Text;
            foreach (var v in scene.Variants) if (Meets(v.When, team)) text = v.Text;
            return Fill(text);
        }

        public string Fill(string text) => (text ?? "").Replace("{nombre}", C.Name).Replace("{signo}", R.Sign(C.Sign)?.Name ?? "");

        public List<ChoiceView> Choices(IReadOnlyList<string> team = null)
        {
            var list = new List<ChoiceView>();
            var scene = CurrentScene;
            if (scene == null) return list;
            for (int i = 0; i < scene.Choices.Count; i++)
            {
                var c = scene.Choices[i];
                if (!Meets(c.When, team)) continue;
                var view = new ChoiceView { Index = i, Def = c };
                if (c.Stat.HasValue)
                {
                    int have = Stat(c.Stat.Value, team);
                    view.CheckLabel = $"{c.Stat.Value} {have}/{c.Need}";
                    if (have < c.Need) view.Blocker = $"Necesitas {c.Stat.Value} {c.Need} (tienes {have}).";
                }
                list.Add(view);
            }
            return list;
        }

        public OpResult Choose(int index, IReadOnlyList<string> team = null)
        {
            var scene = CurrentScene;
            if (scene == null || index < 0 || index >= scene.Choices.Count) return OpResult.Fail("Opción inválida.");
            var view = Choices(team).Find(v => v.Index == index);
            if (view == null) return OpResult.Fail("Esa opción no está disponible.");
            if (!view.Available) return OpResult.Fail(view.Blocker);
            Apply(view.Def.Effects);
            Go(view.Def.Next);
            return OpResult.Success();
        }

        /// <summary>Avanza una escena sin opciones ni combate.</summary>
        public OpResult Continue()
        {
            var scene = CurrentScene;
            if (scene == null) return OpResult.Fail("No hay escena.");
            if (scene.Choices.Count > 0) return OpResult.Fail("Elige una opción.");
            if (scene.Battle != null) return OpResult.Fail("Hay un combate.");
            Go(scene.Next);
            return OpResult.Success();
        }

        private void Go(string next)
        {
            if (string.IsNullOrEmpty(next)) FinishMission();
            else EnterScene(next);
        }

        /// <summary>
        /// Aplica marcas, Títulos, Almas y recompensas. En una Relectura las marcas se pueden reescribir, pero los recursos
        /// y las brujas no se vuelven a entregar.
        /// </summary>
        private void Apply(StoryEffects e)
        {
            foreach (var m in e.Marks) if (!C.Marks.Contains(m)) C.Marks.Add(m);
            if (e.Title.Length > 0 && !C.Titles.Contains(e.Title)) C.Titles.Add(e.Title);
            C.Souls = Math.Max(0, C.Souls + e.Souls);
            AddParalaje(e.Paralaje);
            if (C.Reread) return;
            foreach (var kv in e.Rewards) _g.Ledger.Apply(kv.Key, kv.Value, "historia", C.Mission);
            if (e.Witch.Length > 0) _g.AddWitch(e.Witch);
        }

        private void FinishMission()
        {
            var region = CurrentRegion;
            var mission = CurrentMission;
            Apply(mission.Rewards);
            if (!C.MissionsDone.Contains(mission.Id)) C.MissionsDone.Add(mission.Id);
            C.Mission = "";
            C.Scene = "";
            C.MissionRegion = "";
            C.Reread = false;

            bool all = region.Missions.TrueForAll(m => C.MissionsDone.Contains(m.Id));
            if (all && !C.RegionsDone.Contains(region.Id))
            {
                C.RegionsDone.Add(region.Id);
                _g.Analytics.Track(AnalyticsEvents.StoryRegionDone, region.Id);
                if (!region.Prologue)
                {
                    int done = 0;
                    foreach (var r in C.RegionsDone) if (!R.Region(r).Prologue) done++;
                    C.Marks.Add((done == 1 ? "primera_" : "despues_") + region.Id);
                    if (done % R.EclipseEvery == 0) Eclipse();
                }
            }
        }

        /// <summary>Abandona la misión: se vuelve a empezar desde su inicio. Lo ganado en sus escenas se queda.</summary>
        public OpResult LeaveMission()
        {
            if (!InMission) return OpResult.Fail("No hay misión.");
            C.Mission = "";
            C.Scene = "";
            C.MissionRegion = "";
            C.Reread = false;
            return OpResult.Success();
        }

        // ------------------------------------------------------------------ Eclipses (GDD 6, HUECOS H-16)

        public int CorruptionOf(string region) => C.Corruption.TryGetValue(region ?? "", out var v) ? v : 0;

        /// <summary>Cada dos regiones hay un eclipse: las regiones sin visitar se corrompen un poco más. Nunca bloquea.</summary>
        private void Eclipse()
        {
            C.Eclipses++;
            foreach (var region in R.Ring)
            {
                if (C.RegionsVisited.Contains(region)) continue;
                C.Corruption[region] = Math.Min(R.MaxCorruption, CorruptionOf(region) + 1);
            }
            AddParalaje(5);
            _g.Analytics.Track(AnalyticsEvents.StoryEclipse, C.Eclipses.ToString());
        }

        // ------------------------------------------------------------------ Combate de historia

        /// <summary>
        /// Configura el combate de la escena. Los enemigos escalan con el nivel medio del equipo: la historia se puede
        /// jugar con brujas recién obtenidas, y el Fresno es donde el nivel importa.
        /// </summary>
        public BattleSetup PrepareBattle(IReadOnlyList<string> team, ulong seed)
        {
            var battle = CurrentScene?.Battle ?? throw new InvalidOperationException("No hay combate en esta escena.");
            var ids = battle.Team.Count > 0 ? (IReadOnlyList<string>)battle.Team : team;
            BattleSetup setup;
            if (battle.Team.Count > 0)
            {
                setup = new BattleSetup { Seed = seed, Team = new List<string>(battle.Team) };
                foreach (var id in battle.Team)
                {
                    int lv = _g.Combat.Witch(id).Level;
                    setup.Loadouts[id] = new WitchLoadout { Level = lv };
                }
            }
            else setup = _g.Progress.BuildSetup(ids, seed);

            double avg = 0;
            foreach (var id in ids) avg += setup.Loadouts.TryGetValue(id, out var lo) && lo.Level.HasValue ? lo.Level.Value : _g.Combat.Witch(id).Level;
            avg /= Math.Max(1, ids.Count);

            var enc = new Data.EncounterDef { Id = "historia_" + C.Mission + "_" + C.Scene, Name = CurrentMission.Name };
            var rules = _g.Meta.Progression;
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                var src = _g.Meta.Enemy(battle.Enemies[i]);
                double scale = rules.LevelFactor((int)Math.Round(avg)) / rules.LevelFactor(src.Level) * battle.Strength;
                var def = FresnoService.Scaled(src, $"{src.Id}#h{i}", scale, scale, 0, "");
                def.Level = Math.Max(1, (int)Math.Round(avg));
                enc.Enemies.Add(def.Id);
                setup.CustomEnemies.Add(def);
            }
            setup.CustomEncounter = enc;
            if (battle.Tutorial) setup.Modifiers.Add(ModKeys.StartResonance, 100);
            return setup;
        }

        /// <summary>Si ganas, sigue la historia y el Sendero activo gana experiencia. Perder se reintenta sin costo.</summary>
        public OpResult ResolveBattle(Battle b)
        {
            var battle = CurrentScene?.Battle;
            if (battle == null) return OpResult.Fail("No hay combate en esta escena.");
            if (!b.IsOver) return OpResult.Fail("El combate no ha terminado.");
            if (b.Outcome != Events.BattleOutcome.Victory) return OpResult.Fail("Caíste. Puedes reintentar sin costo.");
            AddSenderoXp(R.SenderoXpPerWin);
            foreach (var w in b.Witches) C.Souls += w.IsAlive ? 1 : 0;
            Go(battle.Next);
            return OpResult.Success("Victoria");
        }

        // ------------------------------------------------------------------ Senderos (GDD 13.3, HUECOS H-17)

        public int SenderoLevel(string id)
        {
            int xp = C.SenderoXp.TryGetValue(id ?? "", out var v) ? v : 0;
            return Math.Min(R.SenderoMaxLevel, 1 + xp / R.SenderoXpPerLevel);
        }

        public void AddSenderoXp(int amount)
        {
            if (C.Sendero.Length == 0) return;
            C.SenderoXp[C.Sendero] = (C.SenderoXp.TryGetValue(C.Sendero, out var v) ? v : 0) + amount;
        }

        /// <summary>Se cambia gratis fuera de combate.</summary>
        public OpResult SetSendero(string id)
        {
            if (R.Sendero(id) == null) return OpResult.Fail("Sendero desconocido.");
            C.Sendero = id;
            return OpResult.Success(R.Sendero(id).Name);
        }

        /// <summary>Bonos de la Calculista para cualquier combate: su Sendero (crece con el nivel) y sus Títulos.</summary>
        public StatBlock CombatMods()
        {
            var b = new StatBlock();
            if (!C.Created) return b;
            var s = R.Sendero(C.Sendero);
            if (s != null)
            {
                double growth = 1.0 + R.SenderoGrowth * (SenderoLevel(C.Sendero) - 1);
                foreach (var key in s.Mods.Keys) b.Add(key, s.Mods.Get(key) * growth);
            }
            foreach (var id in C.Titles) { var t = R.Title(id); if (t != null) b.AddAll(t.Mods); }
            return b;
        }
    }
}
