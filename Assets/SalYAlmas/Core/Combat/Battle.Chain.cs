using System;
using System.Collections.Generic;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Events;

namespace SalYAlmas.Core.Combat
{
    public sealed partial class Battle
    {
        /// <summary>
        /// Calcula la Partitura de una Cadena sin tocar el estado del combate: los golpes, los Aspectos y los
        /// Spark previstos. order son ids de brujas en el campo; withGuest agrega a la Invitada como Cierre.
        /// </summary>
        public ChainPreview PreviewChain(IReadOnlyList<int> order, bool withGuest)
        {
            string error = ChainMembershipError(order, withGuest);
            if (error.Length > 0) return ChainPreview.Invalid(error);
            return ChainPlanner.Plan(BuildParticipants(order, withGuest), Rules);
        }

        /// <summary>Por qué no se puede abrir una Cadena ahora ("" si se puede).</summary>
        public string ChainBlocker()
        {
            if (Pending != DecisionKind.AllyTurn) return "Solo en el turno de una bruja.";
            if (Resonance < Rules.Resonance.ChainCost) return $"Falta Resonancia ({Resonance}/{Rules.Resonance.ChainCost}).";
            return "";
        }

        /// <summary>Tope de Resonancia por Spark en una Cadena: el del GDD más el del Fruto de Júpiter.</summary>
        public int SparkResonanceCap => Rules.Resonance.SparkCapPerChain + (int)Math.Round(_mods.Get(ModKeys.SparkResonanceCap));

        public bool CanUseGuestInChain => Guest != null && !GuestClosingUsed;

        /// <summary>Brujas que pueden entrar a una Cadena ahora.</summary>
        public List<WitchUnit> ChainEligibleWitches()
        {
            var list = new List<WitchUnit>();
            foreach (var w in _witches) if (w.CanCastMagic) list.Add(w);
            return list;
        }

        private string ChainMembershipError(IReadOnlyList<int> order, bool withGuest)
        {
            var c = Rules.Chain;
            if (order == null) return "Elige brujas.";
            if (order.Count < c.MinWitches || order.Count > c.MaxWitches) return $"La Cadena lleva de {c.MinWitches} a {c.MaxWitches} brujas.";
            var seen = new HashSet<int>();
            foreach (var id in order)
            {
                if (!seen.Add(id)) return "Una bruja no puede repetirse en la Cadena.";
                var w = FindWitch(id);
                if (w == null || w.IsGuest || !_witches.Contains(w)) return "La Cadena usa brujas del campo (la Invitada se agrega aparte).";
                if (!w.IsAlive) return w.Name + " cayó.";
                if (w.InEcstasy) return w.Name + " está en Éxtasis: su Ascendente no obedece.";
                if (w.IsDesalada) return w.Name + " está Desalada.";
            }
            if (withGuest)
            {
                if (Guest == null) return "No hay Invitada.";
                if (GuestClosingUsed) return "La Invitada ya cerró una Cadena en este combate.";
            }
            return "";
        }

        private List<ChainParticipant> BuildParticipants(IReadOnlyList<int> order, bool withGuest)
        {
            var list = new List<ChainParticipant>();
            foreach (var id in order)
            {
                var w = FindWitch(id);
                list.Add(new ChainParticipant { UnitId = w.Id, Name = w.Name, Def = w.Def, Dominant = w.Dominant, FrameShift = w.FrameShift, Echoes = w.Loadout?.ChainEchoes });
            }
            if (withGuest)
                list.Add(new ChainParticipant { UnitId = Guest.Id, Name = Guest.Name, Def = Guest.Def, Dominant = Guest.Dominant, FrameShift = Guest.FrameShift, Echoes = Guest.Loadout?.ChainEchoes, IsGuest = true });
            return list;
        }

        private struct TimelineEntry
        {
            public int Frame;
            public int Phase; // 0 inicio de eslabón, 1 golpe, 2 fin de eslabón
            public int Link;
            public ChainHitPlan Hit;
            public int Order;
        }

        private CommandResult DoChain(BattleCommand cmd)
        {
            var r = RequireAllyTurn(cmd.ActorId, out var opener);
            if (!r.Ok) return r;
            string blocker = ChainBlocker();
            if (blocker.Length > 0) return CommandResult.Fail(blocker);
            string error = ChainMembershipError(cmd.Order, cmd.WithGuest);
            if (error.Length > 0) return CommandResult.Fail(error);
            var firstTarget = ResolveEnemyTarget(cmd.TargetId);
            if (firstTarget == null) return CommandResult.Fail("Objetivo inválido.");

            var plan = ChainPlanner.Plan(BuildParticipants(cmd.Order, cmd.WithGuest), Rules);

            ChangeResonance(-Rules.Resonance.ChainCost, "Cadena");
            if (cmd.WithGuest) GuestClosingUsed = true;
            var names = new List<string>();
            foreach (var l in plan.Links) names.Add(l.Name);
            Emit(new ChainStarted
            {
                OpenerId = opener.Id,
                OpenerName = opener.Name,
                Order = new List<int>(cmd.Order),
                Names = names,
                WithGuest = cmd.WithGuest,
                TargetId = firstTarget.Id,
                PredictedSparks = plan.SparkCount,
            });

            // Línea de tiempo: inicios de eslabón, golpes y finales, en orden de frame.
            var entries = new List<TimelineEntry>();
            int seq = 0;
            foreach (var link in plan.Links)
            {
                entries.Add(new TimelineEntry { Frame = link.StartFrame, Phase = 0, Link = link.Index, Order = seq++ });
                int last = link.StartFrame;
                foreach (var h in plan.Hits) if (h.LinkIndex == link.Index && !h.IsReaction) last = Math.Max(last, h.Frame);
                entries.Add(new TimelineEntry { Frame = last, Phase = 2, Link = link.Index, Order = seq++ });
            }
            foreach (var h in plan.Hits) entries.Add(new TimelineEntry { Frame = h.Frame, Phase = 1, Link = h.LinkIndex, Hit = h, Order = seq++ });
            entries.Sort((a, b) =>
            {
                int c = a.Frame.CompareTo(b.Frame);
                if (c != 0) return c;
                c = a.Phase.CompareTo(b.Phase);
                if (c != 0) return c;
                if (a.Phase == 1) return ChainPlanner.CompareHits(a.Hit, b.Hit);
                c = a.Link.CompareTo(b.Link);
                return c != 0 ? c : a.Order.CompareTo(b.Order);
            });

            int n = plan.Links.Count;
            var apps = new AspectApplication[n];
            for (int i = 0; i < n; i++) apps[i] = new AspectApplication();
            var current = firstTarget;
            int sparkResonance = 0, chainSparks = 0, chainAlmitas = 0, chainDamage = 0;
            bool openerJoined = false;

            foreach (var entry in entries)
            {
                if (AliveEnemies().Count == 0) break;
                var link = plan.Links[entry.Link];
                var witch = link.IsGuest ? Guest : FindWitch(link.UnitId);
                if (witch == opener) openerJoined = true;
                current = Retarget(current);

                if (entry.Phase == 0)
                {
                    Emit(new ChainLinkStarted
                    {
                        LinkIndex = link.Index, WitchId = witch.Id, WitchName = witch.Name, Role = link.Role,
                        MoveName = link.Move.Name, Frame = link.StartFrame,
                    });
                    if (link.Index > 0)
                    {
                        var asp = plan.Aspects[link.Index - 1];
                        var prevLink = plan.Links[link.Index - 1];
                        var prev = prevLink.IsGuest ? Guest : FindWitch(prevLink.UnitId);
                        Emit(new AspectFormed
                        {
                            FirstId = prev.Id, FirstName = prev.Name, SecondId = witch.Id, SecondName = witch.Name,
                            FirstPlanet = asp.FromPlanet, SecondPlanet = asp.ToPlanet, Aspect = asp.Aspect, Reaction = asp.Reaction,
                            Context = "Cadena", Frame = link.StartFrame,
                        });
                        apps[link.Index] = ApplyAspect(asp.Aspect, asp.Reaction, prev, witch, current);
                    }
                    ApplyEffects(link.Move.Effects, EffectTiming.Start, witch, current, link.Move.Name);
                    ChangeSalinity(witch, Rules.Salinity.ChainLink, "eslabón de Cadena");
                    ChangeAnima(witch, Rules.Anima.ChainLink);
                }
                else if (entry.Phase == 1)
                {
                    var hit = entry.Hit;
                    var app = apps[link.Index];
                    HitContext ctx;
                    double mHab, tough;
                    if (hit.IsReaction)
                    {
                        ctx = ReactionContext(hit.Frame, link.Index);
                        mHab = Rules.Reactions.TempestadCoef;
                        tough = Rules.Reactions.TempestadToughness;
                    }
                    else
                    {
                        ctx = HitContext.For(HitSource.Chain, hit.Frame);
                        ctx.ByAscendant = link.Move.ByAscendant;
                        ctx.ChainLink = link.Index;
                        ctx.Kind = link.Move.Kind;
                        ctx.IsArea = link.Move.Target == TargetMode.AllEnemies;
                        mHab = link.Move.Multiplier * hit.Share * (1.0 + Mod(witch, ModKeys.ChainMove(link.Role)));
                        tough = link.Move.Toughness * hit.Share;
                        if (witch.InVislumbre && (link.Role == ChainRole.Enlace || link.Role == ChainRole.Cierre))
                            ctx.SkillScale = 1.0 + Rules.Vislumbre.ChainMoveBonus;
                    }
                    ctx.IsSpark = hit.IsSpark;
                    ctx.Aspect = app.DamageMultiplier + (link.Index > 0 ? Mod(witch, ModKeys.AspectBonus) : 0);
                    ctx.ToughnessMultiplier = app.ToughnessMultiplier;

                    if (hit.IsReaction || link.Move.Target == TargetMode.AllEnemies)
                    {
                        foreach (var e in AliveEnemies()) chainDamage += ResolveWitchHit(witch, e, mHab, tough, ctx).Damage;
                    }
                    else if (current != null)
                    {
                        chainDamage += ResolveWitchHit(witch, current, mHab, tough, ctx).Damage;
                    }

                    if (hit.IsSpark)
                    {
                        chainSparks++;
                        TotalSparks++;
                        chainAlmitas += Rules.Chain.SparkAlmitas;
                        Almitas += Rules.Chain.SparkAlmitas;
                        ChangeAnima(witch, Rules.Anima.Spark);
                        int res = Math.Min(Rules.Resonance.Spark, SparkResonanceCap - sparkResonance);
                        if (res > 0)
                        {
                            sparkResonance += res;
                            ChangeResonance(res, "Spark");
                        }
                    }
                }
                else
                {
                    ApplyEffects(link.Move.Effects, EffectTiming.End, witch, current, link.Move.Name);
                    if (link.Move.Hits.Count > 0) ConsumeNextToughness(witch);
                }
            }

            foreach (var app in apps)
                if (app.SextileWitch != null) ShiftAction(app.SextileWitch, Rules.Aspects.SextileAdvance, "Sextil");

            Emit(new ChainEnded { Sparks = chainSparks, TotalDamage = chainDamage, Almitas = chainAlmitas, ResonanceFromSparks = sparkResonance });
            if (openerJoined) _magicThisTurn = true;
            FinishTurn();
            return CommandResult.Success;
        }
    }
}
