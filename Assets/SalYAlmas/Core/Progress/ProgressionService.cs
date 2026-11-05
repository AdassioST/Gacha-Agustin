using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Progress
{
    /// <summary>Lo que pide la siguiente subida de grado (GDD 12.3).</summary>
    public sealed class GradeRequirement
    {
        public GradeStep Step;
        public Bundle Cost = new Bundle();
        public bool LevelOk;
        public bool CanAfford;
        public string Missing = "";
        public List<TrialConditionDef> Conditions = new List<TrialConditionDef>();
        public bool IsMax => Step == null;
    }

    /// <summary>Niveles, grados, Vía, Dignidad y Talentos de Cadena. Todo se paga con el libro contable.</summary>
    public sealed class ProgressionService
    {
        private readonly MetaGame _g;

        public ProgressionService(MetaGame game) { _g = game; }

        private ProgressionRules Rules => _g.Meta.Progression;

        public WitchProgress Get(string id) =>
            _g.Profile.Witches.TryGetValue(id, out var w) ? w : throw new KeyNotFoundException("No tienes a " + id);

        // ------------------------------------------------------------------ Nivel (GDD 11.3)

        public long LevelUpCost(string id, int levels = 1)
        {
            var w = Get(id);
            int to = Math.Min(Rules.Level.Max, w.Level + Math.Max(1, levels));
            return Rules.LevelCostRange(w.Level, to);
        }

        public OpResult LevelUp(string id, int levels = 1)
        {
            var w = Get(id);
            if (w.Level >= Rules.Level.Max) return OpResult.Fail("Ya está en el nivel máximo.");
            int to = Math.Min(Rules.Level.Max, w.Level + Math.Max(1, levels));
            var cost = new Bundle().Add(Res.Almitas, Rules.LevelCostRange(w.Level, to));
            if (!_g.Ledger.TrySpend(cost, "nivel", $"{id} {w.Level} → {to}")) return OpResult.Fail(_g.Ledger.Missing(cost));
            w.Level = to;
            return OpResult.Success($"Nivel {to}");
        }

        /// <summary>Sube todos los niveles que alcancen las almitas.</summary>
        public OpResult LevelUpMax(string id)
        {
            var w = Get(id);
            int to = w.Level;
            long have = _g.Ledger.Balance(Res.Almitas);
            long spent = 0;
            while (to < Rules.Level.Max && spent + Rules.LevelCost(to + 1) <= have) { spent += Rules.LevelCost(to + 1); to++; }
            if (to == w.Level) return OpResult.Fail("No alcanzan las almitas.");
            return LevelUp(id, to - w.Level);
        }

        // ------------------------------------------------------------------ Grados (GDD 12.3)

        public GradeRequirement Requirement(string id)
        {
            var w = Get(id);
            var req = new GradeRequirement();
            if (w.Grade == Grade.SS) return req;
            var step = Rules.StepTo(PurityMath.Next(w.Grade));
            req.Step = step;
            req.Cost.Add(Res.Almitas, step.Almitas);
            if (step.SaltAmount > 0) req.Cost.Add(Res.Salt(DominantPlanet(id), step.SaltTier), step.SaltAmount);
            req.LevelOk = w.Level >= step.MinLevel;
            req.CanAfford = _g.Ledger.CanAfford(req.Cost);
            req.Missing = _g.Ledger.Missing(req.Cost);
            req.Conditions = Rules.ConditionsFor(id, step.Grade);
            return req;
        }

        public Planet DominantPlanet(string id)
        {
            var w = Get(id);
            var r = _g.Meta.Witch(id);
            return w.Via == Via.Mayor ? r.Primary : r.Secondary;
        }

        /// <summary>
        /// Sube el grado después de vencer el Juicio. Lo llama TrialService; aquí solo se cobra y se aplica.
        /// En B la bruja elige su Vía (GDD 7.2).
        /// </summary>
        internal OpResult ApplyGradeUp(string id, Via? via)
        {
            var w = Get(id);
            var req = Requirement(id);
            if (req.IsMax) return OpResult.Fail("Ya está en SS.");
            if (!req.LevelOk) return OpResult.Fail($"Necesita nivel {req.Step.MinLevel}.");
            if (!_g.Ledger.TrySpend(req.Cost, "destilacion", $"{id} → {req.Step.Grade}")) return OpResult.Fail(req.Missing);
            w.Grade = req.Step.Grade;
            if (w.Grade == Grade.B && !w.ViaChosen)
            {
                w.Via = via ?? Via.Mayor;
                w.ViaChosen = true;
            }
            w.Memories.Add(Rules.MemoryFor(id, w.Grade));
            return OpResult.Success("Grado " + w.Grade);
        }

        // ------------------------------------------------------------------ Vía (GDD 7.2)

        public long ViaChangeCost(string id)
        {
            var w = Get(id);
            return w.ViaChanges < Rules.ViaFreeChanges ? 0 : Rules.ViaChangeCost;
        }

        public OpResult ChangeVia(string id, Via via)
        {
            var w = Get(id);
            if (w.Grade < Grade.B || !w.ViaChosen) return OpResult.Fail("La Vía se elige al llegar al grado B.");
            if (w.Via == via) return OpResult.Fail("Ya sigue esa Vía.");
            long cost = ViaChangeCost(id);
            if (cost > 0 && !_g.Ledger.TrySpend(new Bundle().Add(Res.Almitas, cost), "via", id))
                return OpResult.Fail(_g.Ledger.Missing(new Bundle().Add(Res.Almitas, cost)));
            w.Via = via;
            w.ViaChanges++;
            return OpResult.Success(cost == 0 ? "Cambio gratis" : $"Cambio por {cost:N0} almitas");
        }

        // ------------------------------------------------------------------ Dignidad (GDD 8.1)

        public string DignityName(string id) => Rules.Dignity.Names[Get(id).Dignity];

        public OpResult DignityUp(string id)
        {
            var w = Get(id);
            if (w.Dignity >= Rules.Dignity.Max) return OpResult.Fail("Su Ascendente ya está en Domicilio.");
            var cost = new Bundle().Add(Res.Almitas, Rules.Dignity.Costs[w.Dignity + 1]);
            if (!_g.Ledger.TrySpend(cost, "dignidad", id)) return OpResult.Fail(_g.Ledger.Missing(cost));
            w.Dignity++;
            return OpResult.Success(Rules.Dignity.Names[w.Dignity]);
        }

        // ------------------------------------------------------------------ Talentos de Cadena (GDD 12.6)

        public Bundle ChainTalentCost(string id, ChainRole role)
        {
            var w = Get(id);
            int level = w.ChainTalents[(int)role];
            var cost = new Bundle().Add(Res.Almitas, Rules.ChainTalentCost(level));
            if (Rules.ChainTalents.MaterialPerLevel > 0 && Rules.ChainTalents.PlanetMaterials.TryGetValue(DominantPlanet(id), out var mat))
                cost.Add(Res.Mat(mat), Rules.ChainTalents.MaterialPerLevel * level);
            return cost;
        }

        public OpResult ChainTalentUp(string id, ChainRole role)
        {
            var w = Get(id);
            int i = (int)role;
            if (w.ChainTalents[i] >= Rules.ChainTalents.Max) return OpResult.Fail("Talento al máximo.");
            var cost = ChainTalentCost(id, role);
            if (!_g.Ledger.TrySpend(cost, "talentos", $"{id} {role}")) return OpResult.Fail(_g.Ledger.Missing(cost));
            w.ChainTalents[i]++;
            return OpResult.Success($"{role} {w.ChainTalents[i]}");
        }

        // ------------------------------------------------------------------ Loadout de combate

        /// <summary>Estadísticas, arma y bonos de una bruja para el combate (lo que el núcleo de la Fase 1 lee de BattleSetup).</summary>
        public WitchLoadout BuildLoadout(string id) => LoadoutBuilder.Build(_g.Meta, Get(id), _g.Profile);

        /// <summary>Configuración de combate con el progreso real de cada bruja del equipo.</summary>
        public BattleSetup BuildSetup(IReadOnlyList<string> team, ulong seed)
        {
            var setup = new BattleSetup { Seed = seed };
            setup.Modifiers.AddAll(_g.Story.CombatMods());
            foreach (var id in team)
            {
                var w = Get(id);
                if (!_g.Meta.HasKit(id)) throw new InvalidOperationException(id + " todavía no tiene kit de combate.");
                setup.Team.Add(id);
                setup.Loadouts[id] = BuildLoadout(id);
                setup.GradeOverrides[id] = w.Grade;
                setup.ViaOverrides[id] = w.Via;
            }
            return setup;
        }
    }

    /// <summary>Convierte el progreso de una bruja (nivel, grado, Dignidad, talentos, arma y prefijo) en un WitchLoadout.</summary>
    public static class LoadoutBuilder
    {
        public static WitchLoadout Build(MetaDatabase meta, WitchProgress w, PlayerProfile profile)
        {
            var rules = meta.Progression;
            var def = meta.Combat.Witch(w.Id);
            var stat = meta.Combat.Rules.Purity.GradeStatMultiplier;
            double gradeRatio = stat[(int)w.Grade] / stat[(int)def.Grade];
            double levelRatio = rules.LevelFactor(w.Level) / rules.LevelFactor(def.Level);
            var lo = new WitchLoadout
            {
                Level = w.Level,
                AtkMultiplier = levelRatio * gradeRatio,
                HpMultiplier = levelRatio * gradeRatio,
                SpeedMultiplier = 1.0,
            };

            foreach (var step in rules.Grades) if (step.Grade <= w.Grade) lo.Mods.AddAll(step.Mods);
            if (w.Dignity > 0 && w.Dignity < rules.Dignity.AscendantDamage.Count)
                lo.Mods.Add(ModKeys.AscendantDamagePct, rules.Dignity.AscendantDamage[w.Dignity]);
            for (int i = 0; i < 3; i++)
            {
                int extra = Math.Max(0, w.ChainTalents[i] - 1);
                if (extra > 0) lo.Mods.Add(ModKeys.ChainMove((ChainRole)i), extra * rules.ChainTalents.BonusPerLevel);
            }

            var weapon = w.WeaponUid != 0 ? profile.Weapon(w.WeaponUid) : null;
            if (weapon != null && meta.Weapons.TryGetValue(weapon.DefId, out var wd))
            {
                var fam = meta.Family(wd.Family);
                var prefix = meta.Prefix(weapon.Prefix);
                lo.WeaponName = (prefix != null ? prefix.Name + " · " : "") + wd.Name;
                lo.Mods.Add(ModKeys.FlatAtk, wd.Atk);
                lo.Mods.AddAll(wd.Mods);
                if (prefix != null) lo.Mods.AddAll(prefix.Mods);
                var dominant = w.Via == Via.Mayor ? def.Primary : def.Secondary;
                if (wd.Planet == dominant) lo.Mods.Add(ModKeys.DamagePct, meta.AffinityDamage);
                if (fam != null && fam.FrameShift != 0) lo.Mods.Add(ModKeys.FrameShift, fam.FrameShift);
                lo.BasicHits = new List<HitDef>(wd.Hits.Count > 0 ? wd.Hits : fam?.Hits ?? new List<HitDef>());
                lo.ChainEchoes = new List<ChainEcho>(wd.Echoes.Count > 0 ? wd.Echoes : fam?.Echoes ?? new List<ChainEcho>());
            }
            return lo;
        }
    }
}
