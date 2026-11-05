using System;
using System.Collections.Generic;
using SalYAlmas.Core.Chain;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Dibuja la Partitura como un pentagrama: cada eslabón es una línea, cada golpe una nota en su frame, y
    /// los Spark previstos se encienden en dorado (GDD 9.5 y 19.4).
    /// </summary>
    public static class PartituraDrawer
    {
        public static void Draw(Rect area, ChainPreview preview, Func<int, Planet> planetOf, bool compact,
            int playhead = -1, HashSet<int> played = null, int sparkWindow = 2)
        {
            if (preview == null || !preview.IsValid) return;
            float labelW = compact ? 0 : 170;
            var staff = new Rect(area.x + labelW, area.y + (compact ? 8 : 28), area.width - labelW - 10, area.height - (compact ? 16 : 56));
            int lastStart = (preview.Links.Count - 1) * 24;
            float maxFrame = Mathf.Max(preview.TotalFrames, lastStart + 24) + 10;
            float sx = staff.width / maxFrame;
            float rowH = staff.height / 4f;

            // Cinco líneas del pentagrama.
            for (int i = 0; i < 5; i++)
                GuiKit.Fill(new Rect(staff.x, staff.y + i * rowH - 1, staff.width, 2), Palette.WithAlpha(Palette.ParchmentShadow, i < preview.Links.Count ? 0.45f : 0.15f));

            // Barras al inicio de cada eslabón, con su papel.
            foreach (var link in preview.Links)
            {
                float x = staff.x + link.StartFrame * sx;
                GuiKit.Fill(new Rect(x - 1, staff.y - 10, 2, staff.height + 20), Palette.WithAlpha(Palette.Gold, 0.7f));
                if (!compact)
                {
                    GuiKit.Label(new Rect(x + 4, staff.y - 30, 120, 20), "f" + link.StartFrame, 15, Palette.Gold);
                    float y = staff.y + link.Index * rowH;
                    string role = link.Role == ChainRole.Apertura ? "Apertura" : link.Role == ChainRole.Enlace ? "Enlace" : "Cierre";
                    GuiKit.Fill(new Rect(area.x, y - 18, 8, 36), Palette.Planet(link.Planet));
                    GuiKit.Label(new Rect(area.x + 14, y - 22, labelW - 16, 22), link.Name + (link.IsGuest ? " (Inv.)" : ""), 19, Palette.Parchment, TextAnchor.MiddleLeft, true);
                    GuiKit.Label(new Rect(area.x + 14, y + 0, labelW - 16, 20), role + " · " + Planets.DisplayName(link.Planet), 15, Palette.ParchmentShadow);
                }
            }

            // Uniones doradas entre golpes que hacen Spark.
            var hits = preview.Hits;
            for (int i = 0; i < hits.Count; i++)
            {
                for (int j = i + 1; j < hits.Count; j++)
                {
                    if (hits[i].OwnerId == hits[j].OwnerId || Math.Abs(hits[i].Frame - hits[j].Frame) > sparkWindow) continue;
                    var a = NotePos(staff, sx, rowH, hits[i]);
                    var b = NotePos(staff, sx, rowH, hits[j]);
                    GuiKit.Line(a, b, compact ? 3 : 5, Palette.WithAlpha(Palette.GoldBright, 0.85f));
                }
            }

            // Notas.
            float r = compact ? 6 : 10;
            foreach (var h in hits)
            {
                var p = NotePos(staff, sx, rowH, h);
                bool isPlayed = played != null && played.Contains(h.Frame * 100 + h.OwnerId);
                bool future = played != null && !isPlayed;
                float alpha = future ? 0.45f : 1f;
                Color c = Palette.Planet(planetOf(h.OwnerId));
                if (h.IsReaction) c = Palette.Planet(Planet.Jupiter);
                if (h.IsSpark)
                {
                    GuiKit.Dot(p, r + (isPlayed ? 10 : 6), Palette.WithAlpha(Palette.GoldBright, 0.55f * alpha));
                    c = Palette.Spark;
                }
                // Plica de la nota.
                if (!compact) GuiKit.Fill(new Rect(p.x + r - 2, p.y - 30, 2, 30), Palette.WithAlpha(c, alpha * 0.8f));
                if (h.IsReaction) GuiKit.Diamond(p, r + 2, Palette.WithAlpha(c, alpha));
                else GuiKit.Dot(p, isPlayed ? r + 2 : r, Palette.WithAlpha(c, alpha));
            }

            if (playhead >= 0)
            {
                float x = staff.x + playhead * sx;
                GuiKit.Fill(new Rect(x - 1.5f, staff.y - 14, 3, staff.height + 28), Palette.SaltWhite);
            }

            if (!compact)
            {
                for (int f = 0; f <= maxFrame; f += 12)
                {
                    float x = staff.x + f * sx;
                    GuiKit.Fill(new Rect(x, staff.yMax + 14, 1, f % 24 == 0 ? 10 : 5), Palette.ParchmentShadow);
                    if (f % 24 == 0) GuiKit.Label(new Rect(x - 20, staff.yMax + 24, 40, 18), f.ToString(), 13, Palette.ParchmentShadow, TextAnchor.MiddleCenter);
                }
            }
        }

        private static Vector2 NotePos(Rect staff, float sx, float rowH, ChainHitPlan h)
        {
            // Los golpes de reacción van entre líneas, a media altura del eslabón que la provoca.
            float y = staff.y + h.LinkIndex * rowH - (h.IsReaction ? rowH * 0.5f : 0);
            return new Vector2(staff.x + h.Frame * sx, y);
        }
    }
}
