using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Tests
{
    /// <summary>
    /// Construye combates sintéticos para las pruebas. Estas pruebas corren igual con dotnet test
    /// (Tests/Core.Tests) y en el Test Runner de Unity (EditMode).
    /// </summary>
    public static class TestKit
    {
        private static GameDatabase _realDb;

        /// <summary>Carpeta Assets/SalYAlmas/Data, buscando hacia arriba desde el directorio actual o el de las pruebas.</summary>
        public static string DataDir()
        {
            foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                var dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    var candidate = Path.Combine(dir.FullName, "Assets", "SalYAlmas", "Data");
                    if (Directory.Exists(candidate)) return candidate;
                    dir = dir.Parent;
                }
            }
            throw new DirectoryNotFoundException("No encontré Assets/SalYAlmas/Data");
        }

        public static GameDatabase RealDb() => _realDb ?? (_realDb = DataLoader.LoadFromDirectory(DataDir()));

        public static string ReadData(string file) => File.ReadAllText(Path.Combine(DataDir(), file));

        // ------------------------------------------------------------------ Definiciones sintéticas

        public static MoveDef Move(string id, MoveKind kind, double multiplier, double toughness, params int[] frames)
        {
            var m = new MoveDef { Id = id, Name = id, Kind = kind, Multiplier = multiplier, Toughness = toughness, ByAscendant = kind != MoveKind.Basic };
            if (frames.Length == 0) frames = new[] { 0 };
            foreach (var f in frames) m.Hits.Add(new HitDef(f, 1.0 / frames.Length));
            return m;
        }

        public static WitchDef Witch(string id, Planet primary, Planet secondary, double speed = 100, double atk = 1000, int hp = 10000, int level = 50)
        {
            var w = new WitchDef
            {
                Id = id,
                Name = id,
                ShortName = id,
                Rarity = 4,
                Primary = primary,
                Secondary = secondary,
                Via = Via.Mayor,
                Grade = Grade.B,
                Level = level,
                MaxHp = hp,
                Atk = atk,
                Speed = speed,
                CritRate = 0,
                CritDamage = 0.5,
                SelfCast = SelfCastForm.None,
                SelfCastName = "Autohechizo",
                Basic = Move(id + "_basic", MoveKind.Basic, 1.0, 10, 0),
                Spell = Move(id + "_spell", MoveKind.Spell, 2.0, 20, 0),
                Opening = Move(id + "_open", MoveKind.Opening, 1.0, 10, 0),
                Link = Move(id + "_link", MoveKind.Link, 1.0, 10, 0),
                Closing = Move(id + "_close", MoveKind.Closing, 1.0, 10, 0),
                Regencia = new RegenciaDef { Name = "Regencia de " + id },
            };
            for (int i = 0; i < 3; i++) w.Ascensions[i] = Move(id + "_asc" + (i + 1), MoveKind.Ascension, 3.0 + i, 30, 0);
            return w;
        }

        public static void SetChainFrames(WitchDef w, int[] opening, int[] link, int[] closing)
        {
            w.Opening = Move(w.Id + "_open", MoveKind.Opening, 1.0, 10, opening);
            w.Link = Move(w.Id + "_link", MoveKind.Link, 1.0, 10, link);
            w.Closing = Move(w.Id + "_close", MoveKind.Closing, 1.0, 10, closing);
        }

        /// <summary>Enemigo inofensivo y muy resistente: sirve para probar reglas sin que el combate termine.</summary>
        public static EnemyDef Enemy(string id, double speed = 100, double toughness = 1000, int hp = 10000000, params Planet[] weaknesses)
        {
            var e = new EnemyDef
            {
                Id = id,
                Name = id,
                ShortName = id,
                Level = 50,
                MaxHp = hp,
                Atk = 1000,
                Speed = speed,
                Toughness = toughness,
                AttackPlanet = Planet.Saturno,
            };
            e.Weaknesses.AddRange(weaknesses.Length > 0 ? weaknesses : new[] { Planet.Sol, Planet.Luna });
            e.Skills.Add(new EnemySkillDef { Id = "esperar", Name = "Esperar", TargetRule = EnemyTargetRule.Self });
            e.Pattern.Add("esperar");
            return e;
        }

        public static EnemySkillDef AttackSkill(string id, double multiplier, EnemyTargetRule rule = EnemyTargetRule.Random, bool ultimate = false, bool charge = false)
        {
            var s = new EnemySkillDef { Id = id, Name = id, TargetRule = rule, Multiplier = multiplier, IsUltimate = ultimate, Charge = charge };
            s.Hits.Add(new HitDef(0, 1.0));
            return s;
        }

        public static GameDatabase Db(IEnumerable<WitchDef> witches, IEnumerable<EnemyDef> enemies, CombatRules rules = null)
        {
            var db = new GameDatabase(rules ?? new CombatRules());
            foreach (var w in witches) db.Add(w);
            var enc = new EncounterDef { Id = "test", Name = "Prueba" };
            foreach (var e in enemies)
            {
                db.Add(e);
                enc.Enemies.Add(e.Id);
            }
            db.Add(enc);
            foreach (var p in RealDb().Potions.Values) db.Add(p);
            return db;
        }

        public static Battle Start(GameDatabase db, IEnumerable<string> team, string guest = null, ulong seed = 1, IRandomSource rng = null,
            Action<BattleSetup> configure = null, string encounter = "test")
        {
            var setup = new BattleSetup { Team = team.ToList(), GuestId = guest, EncounterId = encounter, Seed = seed };
            configure?.Invoke(setup);
            var b = new Battle(db, setup, rng ?? new FixedRandom(0.99));
            b.Start();
            return b;
        }

        /// <summary>Resuelve turnos enemigos y automáticos hasta que le toque a una bruja.</summary>
        public static void AdvanceToAlly(Battle b)
        {
            int guard = 0;
            while (!b.IsOver && b.Pending != DecisionKind.AllyTurn && guard++ < 500)
            {
                var r = b.Execute(BattleCommand.Advance());
                if (!r.Ok) throw new InvalidOperationException(r.Error);
            }
        }

        /// <summary>Hace que le toque a la bruja indicada, pasando con ataques básicos de las demás.</summary>
        public static WitchUnit TurnOf(Battle b, string witchKey)
        {
            for (int guard = 0; guard < 200; guard++)
            {
                AdvanceToAlly(b);
                var w = (WitchUnit)b.ActiveUnit;
                if (w.Key == witchKey) return w;
                Ok(b.Execute(BattleCommand.Basic(w.Id, -1)));
            }
            throw new InvalidOperationException("Nunca le tocó a " + witchKey);
        }

        public static void Ok(CommandResult r)
        {
            if (!r.Ok) throw new InvalidOperationException("La orden falló: " + r.Error);
        }

        public static WitchUnit W(Battle b, string key) => b.Witches.First(w => w.Key == key);

        public static List<T> EventsSince<T>(Battle b, int index) where T : BattleEvent =>
            b.Events.Skip(index).OfType<T>().ToList();

        /// <summary>Ids en orden de las brujas indicadas.</summary>
        public static List<int> Ids(Battle b, params string[] keys) => keys.Select(k => W(b, k).Id).ToList();
    }
}
