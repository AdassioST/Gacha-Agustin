using System;
using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Chain
{
    /// <summary>Una bruja dentro de una Cadena propuesta.</summary>
    public struct ChainParticipant
    {
        public int UnitId;
        public string Name;
        public WitchDef Def;
        public Planet Dominant;
        /// <summary>Desfase de frames por arma (Fase 2: prefijos Rítmico y Pausado).</summary>
        public int FrameShift;
        public bool IsGuest;
    }

    public sealed class ChainLinkPlan
    {
        public int Index;
        public int UnitId;
        public string Name;
        public bool IsGuest;
        public ChainRole Role;
        public MoveDef Move;
        public Planet Planet;
        public int StartFrame;
    }

    public sealed class ChainHitPlan
    {
        public int LinkIndex;
        public int OwnerId;
        public string OwnerName;
        /// <summary>Índice del golpe dentro de su movimiento; −1 en los golpes de una reacción.</summary>
        public int HitIndex;
        public int Frame;
        public double Share;
        public bool IsSpark;
        public bool IsReaction;
    }

    public sealed class ChainAspectPlan
    {
        public int FromLink;
        public int ToLink;
        public Planet FromPlanet;
        public Planet ToPlanet;
        public Aspect Aspect;
        public HybridReaction Reaction;
    }

    /// <summary>La Partitura: lo que pasará en una Cadena con un orden dado, calculado sin tocar el combate.</summary>
    public sealed class ChainPreview
    {
        public bool IsValid = true;
        public string Error = "";
        public List<ChainLinkPlan> Links = new List<ChainLinkPlan>();
        /// <summary>Golpes ordenados por frame (y luego por eslabón y golpe).</summary>
        public List<ChainHitPlan> Hits = new List<ChainHitPlan>();
        public List<ChainAspectPlan> Aspects = new List<ChainAspectPlan>();
        public int SparkCount;
        public int TotalFrames;

        public static ChainPreview Invalid(string error) => new ChainPreview { IsValid = false, Error = error };
    }

    /// <summary>
    /// Arma la Partitura de una Cadena. Es una función pura de los datos y el orden: no usa azar ni estado,
    /// así que lo que se previsualiza es exactamente lo que se ejecuta.
    /// </summary>
    public static class ChainPlanner
    {
        public static ChainRole RoleFor(int index, int count)
        {
            if (index == 0) return ChainRole.Apertura;
            if (index == count - 1) return ChainRole.Cierre;
            return ChainRole.Enlace;
        }

        public static ChainPreview Plan(IReadOnlyList<ChainParticipant> participants, CombatRules rules)
        {
            var preview = new ChainPreview();
            int count = participants.Count;
            for (int i = 0; i < count; i++)
            {
                var p = participants[i];
                var role = RoleFor(i, count);
                var move = p.Def.MoveFor(role);
                preview.Links.Add(new ChainLinkPlan
                {
                    Index = i,
                    UnitId = p.UnitId,
                    Name = p.Name,
                    IsGuest = p.IsGuest,
                    Role = role,
                    Move = move,
                    Planet = p.Dominant,
                    StartFrame = i * rules.Chain.LinkSpacingFrames,
                });
            }

            // Aspectos entre eslabones seguidos.
            for (int i = 1; i < count; i++)
            {
                var a = preview.Links[i - 1].Planet;
                var b = preview.Links[i].Planet;
                var aspect = AspectRules.Resolve(a, b);
                preview.Aspects.Add(new ChainAspectPlan
                {
                    FromLink = i - 1,
                    ToLink = i,
                    FromPlanet = a,
                    ToPlanet = b,
                    Aspect = aspect,
                    Reaction = aspect == Aspect.Oposicion ? AspectRules.ReactionFor(a, b) : HybridReaction.None,
                });
            }

            // Golpes de cada movimiento.
            for (int i = 0; i < count; i++)
            {
                var link = preview.Links[i];
                var shift = participants[i].FrameShift;
                for (int h = 0; h < link.Move.Hits.Count; h++)
                {
                    var hit = link.Move.Hits[h];
                    preview.Hits.Add(new ChainHitPlan
                    {
                        LinkIndex = i,
                        OwnerId = link.UnitId,
                        OwnerName = link.Name,
                        HitIndex = h,
                        Frame = Math.Max(0, link.StartFrame + hit.Frame + shift),
                        Share = hit.Share,
                    });
                }
            }

            // Tempestad: dos golpes extra del eslabón que la provoca (GDD 7.3: "facilita el Spark").
            foreach (var asp in preview.Aspects)
            {
                if (asp.Reaction != HybridReaction.Tempestad) continue;
                var link = preview.Links[asp.ToLink];
                int extra = Math.Min(rules.Reactions.TempestadExtraHits, rules.Reactions.TempestadHitFrames.Length);
                for (int k = 0; k < extra; k++)
                {
                    preview.Hits.Add(new ChainHitPlan
                    {
                        LinkIndex = asp.ToLink,
                        OwnerId = link.UnitId,
                        OwnerName = link.Name,
                        HitIndex = -1 - k,
                        Frame = link.StartFrame + rules.Reactions.TempestadHitFrames[k],
                        Share = 1.0,
                        IsReaction = true,
                    });
                }
            }

            preview.Hits.Sort(CompareHits);
            preview.SparkCount = MarkSparks(preview.Hits, rules.Chain.SparkWindowFrames);
            int last = 0;
            foreach (var h in preview.Hits) last = Math.Max(last, h.Frame);
            preview.TotalFrames = last;
            return preview;
        }

        internal static int CompareHits(ChainHitPlan a, ChainHitPlan b)
        {
            int c = a.Frame.CompareTo(b.Frame);
            if (c != 0) return c;
            c = a.LinkIndex.CompareTo(b.LinkIndex);
            if (c != 0) return c;
            // Los golpes de reacción (índice negativo) van después de los del movimiento en el mismo frame.
            int ai = a.HitIndex < 0 ? 1000 - a.HitIndex : a.HitIndex;
            int bi = b.HitIndex < 0 ? 1000 - b.HitIndex : b.HitIndex;
            return ai.CompareTo(bi);
        }

        /// <summary>
        /// Un golpe es Spark si otro golpe de otra bruja cae a una distancia de ±window frames (GDD 9.6).
        /// Devuelve cuántos golpes son Spark.
        /// </summary>
        public static int MarkSparks(List<ChainHitPlan> hits, int window)
        {
            int sparks = 0;
            for (int i = 0; i < hits.Count; i++)
            {
                bool spark = false;
                for (int j = 0; j < hits.Count && !spark; j++)
                {
                    if (i == j || hits[i].OwnerId == hits[j].OwnerId) continue;
                    if (Math.Abs(hits[i].Frame - hits[j].Frame) <= window) spark = true;
                }
                hits[i].IsSpark = spark;
                if (spark) sparks++;
            }
            return sparks;
        }
    }

    /// <summary>Un orden de Cadena guardado por el jugador (GDD 9.5: Partituras guardadas).</summary>
    public sealed class SavedPartitura
    {
        public string Name = "";
        public List<string> WitchIds = new List<string>();
        public bool WithGuest;
    }
}
