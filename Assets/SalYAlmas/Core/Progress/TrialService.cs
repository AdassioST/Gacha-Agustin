using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Progress
{
    /// <summary>Un Juicio listo para pelearse: el combate generado y las condiciones que hay que cumplir.</summary>
    public sealed class TrialPlan
    {
        public string WitchId;
        public string WitchName;
        public GradeStep Step;
        public Grade Target => Step.Grade;
        /// <summary>"Juicio I" … "Juicio V".</summary>
        public string Title;
        public List<TrialConditionDef> Conditions = new List<TrialConditionDef>();
        public EnemyDef Ascendant;
        public BattleSetup Setup;
        /// <summary>Vía elegida si el Juicio lleva al grado B.</summary>
        public Via? Via;
    }

    public sealed class ConditionResult
    {
        public TrialConditionDef Def;
        public string Text;
        public bool Passed;
        public string Detail;
    }

    public sealed class TrialResult
    {
        public bool Victory;
        public bool Passed;
        public List<ConditionResult> Conditions = new List<ConditionResult>();
        public OpResult GradeUp;
        public string Memory = "";
    }

    /// <summary>
    /// Juicio de Pureza (GDD 12.4): la bruja y hasta tres aliadas contra su propio Ascendente, que usa el kit de ella.
    /// Desde el Juicio II se suma una condición LitRPG. Los intentos son ilimitados y perder no cuesta nada: el costo del
    /// grado se cobra solo al ganar.
    /// </summary>
    public sealed class TrialService
    {
        private static readonly string[] Roman = { "I", "II", "III", "IV", "V" };
        private readonly MetaGame _g;

        public TrialService(MetaGame game) { _g = game; }

        public OpResult Prepare(string witchId, IReadOnlyList<string> allies, ulong seed, out TrialPlan plan, Via? via = null)
        {
            plan = null;
            var profile = _g.Profile;
            if (!profile.Owns(witchId)) return OpResult.Fail("No tienes a esa bruja.");
            if (!_g.Meta.HasKit(witchId)) return OpResult.Fail("Su kit de combate llega en la Fase 3.");
            var req = _g.Progress.Requirement(witchId);
            if (req.IsMax) return OpResult.Fail("Ya está en SS.");
            if (!req.LevelOk) return OpResult.Fail($"Necesita nivel {req.Step.MinLevel} para su Juicio.");
            if (!req.CanAfford) return OpResult.Fail("Para destilarla después del Juicio. " + req.Missing);
            var rules = _g.Meta.Progression.Trial;
            allies = allies ?? Array.Empty<string>();
            if (allies.Count > rules.MaxAllies) return OpResult.Fail($"Hasta {rules.MaxAllies} aliadas.");
            var team = new List<string> { witchId };
            foreach (var a in allies)
            {
                if (a == witchId || team.Contains(a)) return OpResult.Fail("Aliada repetida.");
                if (!profile.Owns(a)) return OpResult.Fail("No tienes a " + a + ".");
                if (!_g.Meta.HasKit(a)) return OpResult.Fail(a + " todavía no tiene kit de combate.");
                team.Add(a);
            }

            var wp = profile.Witches[witchId];
            var def = _g.Combat.Witch(witchId);
            var asc = BuildAscendant(_g.Meta, wp, req.Step);
            var setup = _g.Progress.BuildSetup(team, seed);
            setup.CustomEncounter = new EncounterDef
            {
                Id = "juicio_" + witchId + "_" + req.Step.Grade,
                Name = $"Juicio {Roman[req.Step.Index]}: {def.Name} contra {asc.Name}",
                Description = req.Step.Unlock,
            };
            setup.CustomEncounter.Enemies.Add(asc.Id);
            setup.CustomEnemies.Add(asc);
            wp.TrialAttempts++;

            plan = new TrialPlan
            {
                WitchId = witchId,
                WitchName = string.IsNullOrEmpty(def.ShortName) ? def.Name : def.ShortName,
                Step = req.Step,
                Title = "Juicio " + Roman[req.Step.Index],
                Conditions = req.Conditions,
                Ascendant = asc,
                Setup = setup,
                Via = via,
            };
            return OpResult.Success(plan.Title);
        }

        /// <summary>Evalúa el combate terminado. Si ganó y cumplió las condiciones, cobra y sube el grado.</summary>
        public TrialResult Resolve(TrialPlan plan, Battle battle)
        {
            var result = new TrialResult { Victory = battle.Outcome == BattleOutcome.Victory };
            result.Conditions = Evaluate(plan.Conditions, battle, plan.WitchId, plan.WitchName);
            result.Passed = result.Victory && result.Conditions.TrueForAll(c => c.Passed);
            _g.Report("juicio_intento");
            if (result.Victory) _g.Quests.ReportBattle(battle);
            if (!result.Passed)
            {
                result.GradeUp = OpResult.Fail(result.Victory ? "No cumpliste la condición. Puedes reintentar sin costo." : "El Ascendente ganó. Puedes reintentar sin costo.");
                return result;
            }
            result.GradeUp = _g.Progress.ApplyGradeUp(plan.WitchId, plan.Via);
            if (result.GradeUp.Ok) { result.Memory = _g.Meta.Progression.MemoryFor(plan.WitchId, plan.Target); _g.Report("juicio"); _g.Report("grado"); }
            return result;
        }

        // ------------------------------------------------------------------ El Ascendente

        /// <summary>Genera al Ascendente de la bruja con su propio kit: Golpe Astral, Cierre, Autohechizo y Ascensión I telegrafiada.</summary>
        public static EnemyDef BuildAscendant(MetaDatabase meta, WitchProgress wp, GradeStep step)
        {
            var rules = meta.Progression;
            var tr = rules.Trial;
            var def = meta.Combat.Witch(wp.Id);
            var stat = meta.Combat.Rules.Purity.GradeStatMultiplier;
            int level = step.TrialLevel;
            double gradeRatio = stat[(int)wp.Grade] / stat[(int)def.Grade];
            var dominant = wp.Via == Via.Mayor ? def.Primary : def.Secondary;
            var recessive = wp.Via == Via.Mayor ? def.Secondary : def.Primary;
            string name = string.IsNullOrEmpty(def.AscendantName) ? "Ascendente de " + def.ShortName : def.AscendantName;

            var e = new EnemyDef
            {
                Id = $"ascendente_{wp.Id}_{step.Grade}",
                Name = name,
                ShortName = name,
                Article = "",
                Description = $"El Ascendente de {def.Name} ({def.AscendantSign}) usa el kit de ella en su contra.",
                Level = level,
                MaxHp = (int)Math.Round(tr.HpAtReference * rules.LevelFactor(level) / rules.LevelFactor(tr.ReferenceLevel)),
                Atk = def.Atk * def.PassiveAtkMultiplier * rules.LevelFactor(level) / rules.LevelFactor(def.Level) * gradeRatio * tr.AtkFactor,
                Speed = def.Speed,
                Toughness = tr.Toughness,
                AttackPlanet = dominant,
                IsBoss = true,
                // Un espíritu astral que pelea detrás de su bruja: en el escenario la sobrepasa.
                Size = EnemySize.Giant,
                ActionsPerTurn = tr.ActionsPerTurn,
            };
            // La Zarigüeya: su Ascendente se hace el muerto. Al usarlo en su contra recupera el ataque completo.
            if (def.PassiveAtkMultiplier < 1) e.Atk /= def.PassiveAtkMultiplier;
            foreach (var p in Planets.All)
                if ((Planets.AreOpposed(p, dominant) || Planets.AreOpposed(p, recessive)) && e.Weaknesses.Count < 3) e.Weaknesses.Add(p);
            foreach (var p in Planets.All)
                if (e.Weaknesses.Count < 2 && p != dominant && !e.Weaknesses.Contains(p)) e.Weaknesses.Add(p);
            e.Res[dominant] = tr.OwnPlanetRes;

            var spell = def.Spell.Multiplier > 0 && def.Spell.Hits.Count > 0 ? def.Spell : def.Basic;
            e.Skills.Add(FromMove("golpe_astral", spell, EnemyTargetRule.Random, 1.0, "Su Golpe Astral, devuelto."));
            e.Skills.Add(FromMove("cierre", def.Closing, EnemyTargetRule.LowestHp, 1.0, "Su propio Cierre."));
            var self = new EnemySkillDef { Id = "autohechizo", TargetRule = EnemyTargetRule.Self };
            if (def.SelfCast != SelfCastForm.None)
            {
                self.Name = def.SelfCastName;
                self.Description = "Se hechiza a sí mismo como ella: +30 % de ataque.";
                self.Effects.Add(new EffectDef { Type = EffectType.Buff, Stat = StatKind.Atk, Value = 0.30, Turns = 2, Target = EffectTarget.Self, Name = def.SelfCastName });
            }
            else
            {
                self.Name = "Ascendente Escondido";
                self.Description = "Se esconde tras su bruja: +20 % de resistencia.";
                self.Effects.Add(new EffectDef { Type = EffectType.Buff, Stat = StatKind.Res, Value = 0.20, Turns = 2, Target = EffectTarget.Self, Name = "Ascendente Escondido" });
            }
            e.Skills.Add(self);
            var asc = FromMove("ascension", def.Ascensions[0], EnemyTargetRule.All, 0.7, "");
            asc.IsUltimate = true;
            asc.Charge = true;
            asc.ChargeText = $"{name} se manifiesta completo. Quiébralo antes de que suelte {def.Ascensions[0].Name}.";
            e.Skills.Add(asc);
            e.Pattern.AddRange(new[] { "golpe_astral", "cierre", "autohechizo", "golpe_astral", "ascension" });
            e.Phase2 = new EnemyPhaseDef { HpBelow = 0.5, SpeedBonus = 10, Text = $"{name} deja de imitarla: pelea como quiere ser." };
            e.Phase2.Pattern.AddRange(new[] { "cierre", "golpe_astral", "ascension" });
            return e;
        }

        private static EnemySkillDef FromMove(string id, MoveDef move, EnemyTargetRule target, double scale, string description)
        {
            var s = new EnemySkillDef
            {
                Id = id,
                Name = move.Name,
                TargetRule = target,
                Multiplier = Math.Max(0.5, move.Multiplier) * scale,
                Description = description,
            };
            foreach (var h in move.Hits) s.Hits.Add(new HitDef(h.Frame, h.Share));
            if (s.Hits.Count == 0) s.Hits.Add(new HitDef(12, 1.0));
            foreach (var fx in move.Effects)
            {
                // Solo los efectos que caen sobre su objetivo (ahora una bruja): daños por turno, penalizaciones y retrasos.
                if (fx.Target != EffectTarget.Target) continue;
                if (fx.Type == EffectType.Dot || fx.Type == EffectType.Debuff || fx.Type == EffectType.Delay || fx.Type == EffectType.Salinity)
                    s.Effects.Add(fx);
            }
            return s;
        }

        // ------------------------------------------------------------------ Condiciones LitRPG

        public static List<ConditionResult> Evaluate(IReadOnlyList<TrialConditionDef> conditions, Battle battle, string witchId, string witchName)
        {
            var list = new List<ConditionResult>();
            foreach (var c in conditions)
            {
                var r = new ConditionResult { Def = c, Text = c.TextFor(witchName) };
                switch (c.Type)
                {
                    case TrialConditionType.SalinityAbove:
                    {
                        WitchUnit w = null;
                        foreach (var x in battle.Witches) if (x.Key == witchId) w = x;
                        int s = w?.Salinity ?? 0;
                        r.Passed = w != null && w.IsAlive && s > c.Value;
                        r.Detail = $"Salinidad final {s}";
                        break;
                    }
                    case TrialConditionType.NoAscensions:
                    {
                        int n = Count<AscensionCast>(battle);
                        r.Passed = n == 0;
                        r.Detail = $"{n} Ascensiones";
                        break;
                    }
                    case TrialConditionType.AlliesRarity:
                    {
                        r.Passed = true;
                        foreach (var x in battle.Witches) if (x.Key != witchId && x.Def.Rarity != c.Value) r.Passed = false;
                        r.Detail = r.Passed ? "aliadas válidas" : "hay aliadas de otra rareza";
                        break;
                    }
                    case TrialConditionType.MaxCycles:
                        r.Passed = battle.Cycle <= c.Value;
                        r.Detail = $"{battle.Cycle} rondas";
                        break;
                    case TrialConditionType.NoFalls:
                    {
                        int falls = 0;
                        foreach (var e in battle.Events) if (e is UnitDefeated d && d.Side == Side.Ally) falls++;
                        r.Passed = falls == 0;
                        r.Detail = $"{falls} caídas";
                        break;
                    }
                    case TrialConditionType.MinChains:
                    {
                        int n = Count<ChainStarted>(battle);
                        r.Passed = n >= c.Value;
                        r.Detail = $"{n} Cadenas";
                        break;
                    }
                    case TrialConditionType.NoPotions:
                    {
                        int n = 0;
                        foreach (var e in battle.Events) if (e is ActionUsed a && a.Action == ActionKind.Potion) n++;
                        r.Passed = n == 0;
                        r.Detail = $"{n} Salmueras";
                        break;
                    }
                    case TrialConditionType.MaxTeam:
                        r.Passed = battle.Witches.Count <= c.Value;
                        r.Detail = $"{battle.Witches.Count} brujas";
                        break;
                    case TrialConditionType.MinSparks:
                        r.Passed = battle.TotalSparks >= c.Value;
                        r.Detail = $"{battle.TotalSparks} Spark";
                        break;
                }
                list.Add(r);
            }
            return list;
        }

        private static int Count<T>(Battle b) where T : BattleEvent
        {
            int n = 0;
            foreach (var e in b.Events) if (e is T) n++;
            return n;
        }
    }
}
