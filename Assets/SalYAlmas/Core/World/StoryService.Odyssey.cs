using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;

namespace SalYAlmas.Core.World
{
    /// <summary>
    /// La Odisea (docs/propuestas/ODISEA.md): el lugar lo eliges tú y el cielo avanza solo. Las regiones se abren por el
    /// anillo desde la región de tu signo; su nivel y su capa de horror dependen de cuántas regiones llevabas al llegar.
    /// Dentro de una región, cada combate de historia es una cadena de secciones que arrastra vida y Salinidad, con botín.
    /// Las zonas terminadas se repiten como Expediciones con la Marea que elijas.
    /// </summary>
    public sealed partial class StoryService
    {
        private OdysseyRules O => R.Odyssey;
        public bool OdysseyEnabled => O.Enabled;

        // ------------------------------------------------------------------ Anillo y Marea

        /// <summary>Las regiones construidas, en el orden del anillo (zodiaco). El anillo salta las que no existen.</summary>
        public List<RegionDef> BuiltRing
        {
            get
            {
                var list = new List<RegionDef>();
                foreach (var id in R.Ring)
                {
                    var reg = R.Region(id);
                    if (reg != null && !reg.Prologue) list.Add(reg);
                }
                return list;
            }
        }

        public int RegionsDoneCount
        {
            get
            {
                int d = 0;
                foreach (var id in C.RegionsDone) { var reg = R.Region(id); if (reg != null && !reg.Prologue) d++; }
                return d;
            }
        }

        /// <summary>n: regiones terminadas cuando llegaste. Si todavía no llegas, las que llevas ahora.</summary>
        public int ArrivalIndex(string region) => C.Arrival.TryGetValue(region ?? "", out var n) ? n : RegionsDoneCount;

        /// <summary>La Marea de Sal de hoy: sube con cada eclipse.</summary>
        public int CurrentTide => Math.Min(O.MaxTide, C.Eclipses);

        /// <summary>La Marea de una región: la de cuando llegaste (lo visitado ya no se corrompe, GDD 6).</summary>
        public int Tide(string region) => Math.Min(O.MaxTide, CorruptionOf(region));

        public static SkyLayer LayerOf(int tide) => tide >= 4 ? SkyLayer.Abismo : tide >= 2 ? SkyLayer.Grieta : SkyLayer.Superficie;
        public SkyLayer Layer(string region) => LayerOf(Tide(region));

        public static string LayerName(SkyLayer l) => l == SkyLayer.Abismo ? "Abismo" : l == SkyLayer.Grieta ? "Grieta" : "Superficie";

        public int RegionLevel(string region) => Math.Min(80, O.LevelBase + O.LevelStep * ArrivalIndex(region));

        /// <summary>Nivel de una zona. Una Expedición con más Marea que la de llegada sube al nivel de esa Marea.</summary>
        public int ZoneLevel(RegionDef region, ZoneDef zone, int tide = -1)
        {
            int lv = RegionLevel(region.Id);
            if (tide > Tide(region.Id)) lv = Math.Max(lv, O.LevelBase + O.LevelStep * 2 * tide);
            int add = zone == null ? 0 : O.ZoneAdd[(int)zone.Kind];
            return Math.Min(80, lv + add);
        }

        /// <summary>
        /// Por qué no se puede entrar a una región nueva. La primera es la de tu signo; después, cualquier vecina de una
        /// región terminada (el arco crece por sus dos puntas, en sentido Directo o Retrógrado).
        /// </summary>
        public string RingBlock(string regionId)
        {
            if (!O.Enabled) return "";
            var region = R.Region(regionId);
            if (region == null || region.Prologue || C.RegionsVisited.Contains(regionId)) return "";
            if (regionId == StartRegion) return "";
            var ring = BuiltRing;
            int i = ring.FindIndex(x => x.Id == regionId);
            if (i < 0) return "Esta región todavía no se puede visitar.";
            if (RegionsDoneCount == 0)
                return "La Tortuga atraca primero en tu región: " + (R.Region(StartRegion)?.Name ?? "la de tu signo") + ".";
            var prev = ring[(i - 1 + ring.Count) % ring.Count];
            var next = ring[(i + 1) % ring.Count];
            if (RegionDone(prev.Id) || RegionDone(next.Id)) return "";
            return "La Tortuga llega por el anillo: termina una región vecina (" + prev.Name + " o " + next.Name + ").";
        }

        /// <summary>Sentido en que la Tortuga llegaría a la región desde una terminada: Directo, Retrógrado o vacío.</summary>
        public string Direction(string regionId)
        {
            var ring = BuiltRing;
            int i = ring.FindIndex(x => x.Id == regionId);
            if (i < 0 || ring.Count < 2) return "";
            if (RegionDone(ring[(i - 1 + ring.Count) % ring.Count].Id)) return "Directo";
            if (RegionDone(ring[(i + 1) % ring.Count].Id)) return "Retrógrado";
            return "";
        }

        /// <summary>La región opuesta a tu signo en el anillo completo: tu Descendente (ODISEA 2).</summary>
        public string Descendant
        {
            get
            {
                int i = R.Ring.IndexOf(StartRegion);
                return i < 0 || R.Ring.Count != 12 ? "" : R.Ring[(i + 6) % 12];
            }
        }

        // ------------------------------------------------------------------ Secciones de historia

        private bool OdysseyBattle(StoryBattle battle) =>
            O.Enabled && battle != null && battle.Team.Count == 0 && CurrentRegion != null && CurrentRegion.Zones.Count > 0;

        private int SectionsOf(StoryBattle battle)
        {
            if (!OdysseyBattle(battle) || battle.Tutorial) return 0;
            return CurrentRegion.ZoneOfMission(C.Mission)?.Sections ?? 0;
        }

        /// <summary>Secciones del combate en curso, contando el combate de historia que lo cierra.</summary>
        public int SectionCount => SectionsOf(CurrentScene?.Battle) + 1;
        /// <summary>Sección actual, desde 1.</summary>
        public int SectionNumber => C.Section + 1;

        public ZoneDef CurrentZone => CurrentRegion?.ZoneOfMission(C.Mission);

        private BattleSetup PrepareOdysseyBattle(StoryBattle battle, IReadOnlyList<string> team, ulong seed)
        {
            var region = CurrentRegion;
            var zone = region.ZoneOfMission(C.Mission);
            var setup = _g.Progress.BuildSetup(team, seed);
            bool wave = C.Section < SectionsOf(battle);
            int tide = Tide(region.Id);
            int level = ZoneLevel(region, zone) + (!wave && battle.Boss ? O.BossAdd : 0);
            var ids = wave ? Wave(zone, tide, Hash(C.Mission + "/" + C.Scene, (ulong)C.Section)) : new List<string>(battle.Enemies);
            FillEncounter(setup, ids, level, wave ? O.WaveStrength : battle.Strength, tide,
                $"historia_{C.Mission}_{C.Scene}_{C.Section}", CurrentMission.Name);
            ApplyCarry(setup, C.CarryHp, C.CarrySalinity);
            if (battle.Tutorial) setup.Modifiers.Add(ModKeys.StartResonance, 100);
            return setup;
        }

        /// <summary>Gana una sección: botín, y si no era la última, se queda en la misma escena con la vida arrastrada.</summary>
        private bool ResolveOdysseySection(StoryBattle battle, Battle b)
        {
            var region = CurrentRegion;
            var zone = region.ZoneOfMission(C.Mission);
            int sections = SectionsOf(battle);
            bool wave = C.Section < sections;
            int level = ZoneLevel(region, zone) + (!wave && battle.Boss ? O.BossAdd : 0);
            Loot(level, region, Tide(region.Id), 1.0, !wave && battle.Boss);
            C.SectionsWon++;
            if (!wave && battle.Boss) C.BossesWon++;
            SaveCarry(b, C.CarryHp, C.CarrySalinity);
            if (!wave) { C.Section = 0; return false; }
            C.Section++;
            return true;
        }

        private void OnOdysseyMissionFinished(RegionDef region, MissionDef mission)
        {
            C.Section = 0;
            C.CarryHp.Clear();
            C.CarrySalinity.Clear();
            if (!O.Enabled || region.Zones.Count == 0) return;
            var zone = region.ZoneOfMission(mission.Id);
            if (zone != null && !C.ZonesDone.Contains(zone.Id) && zone.Missions.TrueForAll(MissionDone))
            {
                C.ZonesDone.Add(zone.Id);
                Give(Res.SalesDeBano, O.ZoneSales, "historia");
            }
            if (RegionDone(region.Id) && region.Station.Length > 0) _g.Profile.Stations.Add(region.Station);
        }

        // ------------------------------------------------------------------ Expediciones

        public bool InExpedition => C.ExpZone.Length > 0;
        public RegionDef ExpeditionRegion => R.Region(C.ExpRegion);
        public ZoneDef ExpeditionZone => ExpeditionRegion?.Zone(C.ExpZone);

        /// <summary>Secciones antes del jefe de una Expedición.</summary>
        public int ExpeditionSections(ZoneDef z) => z.IsEx ? Math.Max(1, z.Sections) : O.ExpeditionSections;

        public int ExpeditionSectionCount => ExpeditionZone == null ? 0 : ExpeditionSections(ExpeditionZone) + 1;

        /// <summary>Mareas que se pueden elegir en una región: de la de llegada a la de hoy.</summary>
        public int MinTide(string regionId) => Tide(regionId);
        public int MaxTideFor(string regionId) => Math.Max(Tide(regionId), CurrentTide);

        public string CanExpedition(string regionId, string zoneId, int tide)
        {
            if (!O.Enabled) return "La Odisea no está activa.";
            if (InExpedition) return "Ya hay una Expedición en curso.";
            var region = R.Region(regionId);
            var zone = region?.Zone(zoneId);
            if (zone == null) return "Zona desconocida.";
            if (zone.IsEx && !RegionDone(regionId)) return "La zona EX se abre al terminar la región.";
            if (!zone.IsEx && !C.ZonesDone.Contains(zone.Id)) return "Termina primero la historia de la zona.";
            if (tide < MinTide(regionId) || tide > MaxTideFor(regionId)) return "Esa Marea no está disponible aquí.";
            return "";
        }

        public OpResult StartExpedition(string regionId, string zoneId, int tide)
        {
            string why = CanExpedition(regionId, zoneId, tide);
            if (why.Length > 0) return OpResult.Fail(why);
            C.ExpRegion = regionId;
            C.ExpZone = zoneId;
            C.ExpTide = tide;
            C.ExpSection = 0;
            C.ExpHp.Clear();
            C.ExpSalinity.Clear();
            return OpResult.Success(R.Region(regionId).Zone(zoneId).Name);
        }

        /// <summary>La zona EX muestra siempre una capa más que la región (ODISEA 4).</summary>
        private int ExpeditionWaveTide => Math.Min(O.MaxTide, C.ExpTide + (ExpeditionZone != null && ExpeditionZone.IsEx ? 2 : 0));

        public bool ExpeditionBossNext => InExpedition && C.ExpSection >= ExpeditionSections(ExpeditionZone);
        /// <summary>Sección actual de la Expedición, desde 1.</summary>
        public int ExpeditionSectionNumber => C.ExpSection + 1;

        private int ExpeditionLevel => ZoneLevel(ExpeditionRegion, ExpeditionZone, C.ExpTide) + (ExpeditionBossNext ? O.BossAdd : 0);

        public BattleSetup PrepareExpeditionBattle(IReadOnlyList<string> team, ulong seed)
        {
            if (!InExpedition) throw new InvalidOperationException("No hay Expedición en curso.");
            var region = ExpeditionRegion;
            var zone = ExpeditionZone;
            var setup = _g.Progress.BuildSetup(team, seed);
            List<string> ids;
            if (ExpeditionBossNext)
            {
                ids = new List<string> { zone.Boss.Length > 0 ? zone.Boss : region.Native };
                ids.Add(zone.Pool[(int)(Hash(zone.Id, (ulong)C.SectionsWon) % (ulong)zone.Pool.Count)]);
            }
            else ids = Wave(zone, ExpeditionWaveTide, Hash(zone.Id + "/exp", (ulong)(C.SectionsWon * 7 + C.ExpSection)));
            double strength = ExpeditionBossNext ? (zone.Boss.Length > 0 ? 1.0 : 1.5) : O.WaveStrength;
            FillEncounter(setup, ids, ExpeditionLevel, strength, ExpeditionWaveTide, $"exp_{zone.Id}_{C.ExpSection}", zone.Name);
            ApplyCarry(setup, C.ExpHp, C.ExpSalinity);
            return setup;
        }

        public OpResult ResolveExpeditionBattle(Battle b)
        {
            if (!InExpedition) return OpResult.Fail("No hay Expedición en curso.");
            if (!b.IsOver) return OpResult.Fail("El combate no ha terminado.");
            if (b.Outcome != Events.BattleOutcome.Victory) return OpResult.Fail("Caíste. Puedes reintentar la sección sin costo.");
            AddSenderoXp(R.SenderoXpPerWin);
            if (C.DailyDay != _g.Profile.Day) { C.DailyDay = _g.Profile.Day; C.DailySections = 0; }
            C.DailySections++;
            double factor = C.DailySections <= O.DailyFull ? 1.0 : O.AfterCap;
            bool boss = ExpeditionBossNext;
            var region = ExpeditionRegion;
            var zone = ExpeditionZone;
            Loot(ExpeditionLevel, region, ExpeditionWaveTide, factor, boss, true);
            SaveCarry(b, C.ExpHp, C.ExpSalinity);
            C.SectionsWon++;
            if (!boss)
            {
                C.ExpSection++;
                return OpResult.Success($"Sección {C.ExpSection} de {ExpeditionSections(zone) + 1}");
            }
            C.BossesWon++;
            if (zone.IsEx && !C.ZonesDone.Contains(zone.Id))
            {
                C.ZonesDone.Add(zone.Id);
                Give(Res.SalesDeBano, O.ZoneSales, "historia");
            }
            EndExpedition();
            return OpResult.Success("Expedición completa");
        }

        public OpResult LeaveExpedition()
        {
            if (!InExpedition) return OpResult.Fail("No hay Expedición.");
            EndExpedition();
            return OpResult.Success();
        }

        private void EndExpedition()
        {
            C.ExpRegion = "";
            C.ExpZone = "";
            C.ExpTide = 0;
            C.ExpSection = 0;
            C.ExpHp.Clear();
            C.ExpSalinity.Clear();
        }

        // ------------------------------------------------------------------ Enemigos, botín y arrastre

        /// <summary>Dos o tres enemigos de la zona. Desde la Grieta entran Cristalizados; en el Abismo, los Sin Carta.</summary>
        private List<string> Wave(ZoneDef zone, int tide, ulong h)
        {
            var list = new List<string>();
            int count = 2 + (int)(h % 2);
            for (int i = 0; i < count; i++) list.Add(zone.Pool[(int)((h >> (8 * (i + 1))) % (ulong)zone.Pool.Count)]);
            var layer = LayerOf(tide);
            if (layer >= SkyLayer.Grieta && O.GrietaInvaders.Count > 0 && ((h >> 40) & 1) == 0)
                list[list.Count - 1] = O.GrietaInvaders[(int)((h >> 44) % (ulong)O.GrietaInvaders.Count)];
            if (layer >= SkyLayer.Abismo && O.AbismoInvaders.Count > 0 && ((h >> 48) & 1) == 0)
                list[0] = O.AbismoInvaders[(int)((h >> 52) % (ulong)O.AbismoInvaders.Count)];
            return list;
        }

        /// <summary>Enemigos al nivel de la zona. Ya no escalan con el equipo: subir de nivel sí sirve.</summary>
        private void FillEncounter(BattleSetup setup, List<string> ids, int level, double strength, int tide, string encId, string name)
        {
            var enc = new EncounterDef { Id = encId, Name = name };
            var rules = _g.Meta.Progression;
            for (int i = 0; i < ids.Count; i++)
            {
                var src = _g.Meta.Enemy(ids[i]);
                double scale = rules.LevelFactor(level) / rules.LevelFactor(src.Level) * strength;
                double hp = scale * (1.0 + O.TideHp * tide);
                var def = FresnoService.Scaled(src, $"{src.Id}#o{i}", hp, scale, 0, "");
                def.Level = level;
                enc.Enemies.Add(def.Id);
                setup.CustomEnemies.Add(def);
            }
            setup.CustomEncounter = enc;
        }

        private static void ApplyCarry(BattleSetup setup, Dictionary<string, double> hp, Dictionary<string, int> salinity)
        {
            foreach (var id in setup.Team)
            {
                if (hp.TryGetValue(id, out var h)) setup.StartHp[id] = h;
                if (salinity.TryGetValue(id, out var s)) setup.StartSalinity[id] = s;
            }
        }

        private void SaveCarry(Battle b, Dictionary<string, double> hp, Dictionary<string, int> salinity)
        {
            foreach (var w in b.Witches)
            {
                if (w.IsGuest) continue;
                hp[w.Key] = w.IsAlive ? Math.Max(O.FallenHp, w.Hp / (double)w.MaxHp) : O.FallenHp;
                salinity[w.Key] = w.IsAlive ? w.Salinity : 0;
            }
        }

        /// <summary>
        /// Botín de una sección: almitas que escalan con el nivel, el material de la región (Yunque y Talentos), sal cruda de
        /// su planeta (Altar y Caldero) y a veces un tesoro que depende de la capa. Los jefes dan el doble.
        /// </summary>
        private void Loot(int level, RegionDef region, int tide, double factor, bool boss, bool expedition = false)
        {
            C.LastLoot.Clear();
            if (C.Reread && !expedition) return;
            double f = factor * (boss ? 2.0 : 1.0);
            Give(Res.Almitas, (long)Math.Round(O.SectionAlmitas * Math.Pow(level, O.SectionExponent) * f), "historia");
            if (region.Material.Length > 0 && _g.Rng.NextDouble() < Math.Min(1.0, O.MaterialChance * (boss ? 1.5 : 1.0)))
            {
                int amount = O.MaterialMin + _g.Rng.NextInt(Math.Max(1, O.MaterialMax - O.MaterialMin + 1));
                Give(Res.Mat(region.Material), Math.Max(1, (long)Math.Round(amount * Math.Max(1.0, f))), "region");
            }
            if (_g.Rng.NextDouble() < O.SaltChance * factor) Give(Res.SalCruda(region.Planet), 1 + _g.Rng.NextInt(2), "historia");
            if (_g.Rng.NextDouble() < O.TreasureChance * (boss ? 2.0 : 1.0) * factor)
            {
                switch (LayerOf(tide))
                {
                    case SkyLayer.Superficie:
                        if (O.TradeMaterials.Count > 0) Give(Res.Mat(O.TradeMaterials[_g.Rng.NextInt(O.TradeMaterials.Count)]), 2, "region");
                        else Give(Res.SalCruda(Planets.All[_g.Rng.NextInt(7)]), 3, "historia");
                        break;
                    case SkyLayer.Grieta: Give(Res.SalDestilada(region.Planet), 1, "historia"); break;
                    default: Give(Res.SalNegra, 1, "historia"); Give(Res.Quintaesencia(region.Planet), 1, "historia"); break;
                }
            }
        }

        private void Give(string res, long amount, string source)
        {
            if (amount <= 0) return;
            _g.Ledger.Apply(res, amount, source, C.ExpZone.Length > 0 ? C.ExpZone : C.Mission);
            C.LastLoot.Add($"+{amount:N0} {_g.Meta.Catalog.NameOf(res)}");
        }

        private static ulong Hash(string s, ulong salt)
        {
            ulong h = 14695981039346656037UL;
            foreach (char ch in s) { h ^= ch; h *= 1099511628211UL; }
            return MapGenerator.Mix(h, salt + 0x9E37);
        }
    }
}
