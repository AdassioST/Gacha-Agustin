using System;
using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>Antes del combate: equipo de hasta cuatro brujas, Invitada, Vía, cinturón, encuentro y semilla.</summary>
    public sealed class FormationScreen
    {
        private readonly GameDatabase _db;
        private readonly List<string> _team = new List<string> { "marisal", "selenia", "solenne", "tormentina" };
        private string _guest = "zarigueya";
        private string _encounter = "guardian";
        private readonly Dictionary<string, Via> _via = new Dictionary<string, Via>();
        private readonly string[] _belt = { "balsamo_cuprico", "leche_luna", "destilado_rocio" };
        private string _seedText;
        private bool _planetaryDay;

        public event Action<BattleSetup> OnStart;

        public FormationScreen(GameDatabase db)
        {
            _db = db;
            _seedText = NewSeed().ToString();
            _team.RemoveAll(id => !db.Witches.ContainsKey(id));
            if (!db.Witches.ContainsKey(_guest)) _guest = null;
            if (!db.Encounters.ContainsKey(_encounter)) _encounter = db.EncounterOrder.FirstOrDefault();
        }

        public static ulong NewSeed() => (ulong)(DateTime.Now.Ticks % 1000000007L);

        public BattleSetup BuildSetup(ulong seed)
        {
            var setup = new BattleSetup
            {
                Team = new List<string>(_team),
                GuestId = _guest,
                EncounterId = _encounter,
                Seed = seed,
                PlanetaryDay = _planetaryDay ? Planets.RulerOfWeekday((int)DateTime.Now.DayOfWeek) : (Planet?)null,
            };
            foreach (var kv in _via) setup.ViaOverrides[kv.Key] = kv.Value;
            foreach (var p in _belt)
                if (!string.IsNullOrEmpty(p) && _db.Potions.ContainsKey(p)) setup.Belt.Add(new BeltSlot(p, 3));
            return setup;
        }

        public void Draw()
        {
            GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.Abyss);
            GuiKit.Label(new Rect(60, 30, 1400, 60), "Sal y Almas · El Aquelarre (prototipo de combate)", 40, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(60, 86, 1600, 30), "Elige hasta cuatro brujas y una Invitada. La Invitada aporta su Regencia todo el combate y puede cerrar una Cadena.", 20, Palette.ParchmentShadow);

            float x = 60, y = 140;
            int i = 0;
            foreach (var id in _db.WitchOrder)
            {
                var w = _db.Witch(id);
                DrawCard(w, new Rect(x + (i % 3) * 600, y + (i / 3) * 300, 580, 284));
                i++;
            }

            DrawSide(new Rect(60, 760, 1800, 300));
        }

        private void DrawCard(WitchDef w, Rect r)
        {
            bool inTeam = _team.Contains(w.Id);
            bool isGuest = _guest == w.Id;
            GuiKit.PanelBox(r, 0.95f);
            var via = _via.TryGetValue(w.Id, out var v) ? v : w.Via;
            var dominant = via == Via.Mayor ? w.Primary : w.Secondary;
            var recessive = via == Via.Mayor ? w.Secondary : w.Primary;
            GuiKit.Fill(new Rect(r.x, r.y, r.width, 6), Palette.Planet(dominant));
            string rank = w.Rarity == 5 ? "Regente 5★" : w.Rarity == 4 ? "Oficiala 4★" : "Aprendiza 3★";
            GuiKit.Label(new Rect(r.x + 16, r.y + 12, 380, 34), w.Name, 28, Palette.Parchment, TextAnchor.MiddleLeft, true);
            GuiKit.Label(new Rect(r.x + 16, r.y + 46, 540, 22), $"{rank} · {w.Species} · {w.Role} · Asc. {w.AscendantSign}", 16, Palette.ParchmentShadow);
            GuiKit.PlanetPill(new Rect(r.x + 16, r.y + 76, 140, 26), dominant, true);
            GuiKit.PlanetPill(new Rect(r.x + 162, r.y + 76, 130, 26), recessive);
            GuiKit.Label(new Rect(r.x + 300, r.y + 76, 270, 26), w.MixName + " · 85 / 15", 16, Palette.Parchment);
            string self = w.SelfCast == SelfCastForm.None ? "Sin Autohechizo" : "Autohechizo: " + w.SelfCastName;
            GuiKit.Label(new Rect(r.x + 16, r.y + 108, 540, 22), $"ATQ {w.Atk * w.PassiveAtkMultiplier:0} · VIDA {w.MaxHp} · VEL {w.Speed:0} · {self}", 16, Palette.Parchment);
            string passive = string.IsNullOrEmpty(w.PassiveDescription) ? "" : w.PassiveDescription + " ";
            GuiKit.Label(new Rect(r.x + 16, r.y + 132, 548, 46), passive + "Regencia: " + w.Regencia.Name + ". " + w.Regencia.Description, 15, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
            GuiKit.Label(new Rect(r.x + 16, r.y + 180, 548, 40), $"Cadena: {w.Opening.Name} · {w.Link.Name} · {w.Closing.Name}", 15, Palette.GoldBright, TextAnchor.UpperLeft, false, true);

            float by = r.yMax - 56;
            if (GuiKit.Button(new Rect(r.x + 16, by, 180, 44), inTeam ? "<b>En el equipo</b>" : "Al equipo", !isGuest && (inTeam || _team.Count < 4), 18))
            {
                if (inTeam) _team.Remove(w.Id);
                else _team.Add(w.Id);
            }
            if (GuiKit.Button(new Rect(r.x + 204, by, 170, 44), isGuest ? "<b>Invitada</b>" : "Invitada", !inTeam, 18))
                _guest = isGuest ? null : w.Id;
            if (GuiKit.Button(new Rect(r.x + 382, by, 182, 44), via == Via.Mayor ? "Vía Mayor" : "Vía Menor", true, 18))
                _via[w.Id] = via == Via.Mayor ? Via.Menor : Via.Mayor;
            if (inTeam) GuiKit.Frame(r, Palette.GoldBright, 3);
            else if (isGuest) GuiKit.Frame(r, Palette.Planet(Planet.Venus), 3);
        }

        private void DrawSide(Rect r)
        {
            GuiKit.PanelBox(r, 0.95f);
            GuiKit.Label(new Rect(r.x + 16, r.y + 10, 300, 28), "Encuentro", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            float ey = r.y + 44;
            foreach (var id in _db.EncounterOrder)
            {
                var enc = _db.Encounter(id);
                if (GuiKit.Button(new Rect(r.x + 16, ey, 520, 52), (id == _encounter ? "<b>" : "") + enc.Name + (id == _encounter ? "</b>" : ""), true, 18)) _encounter = id;
                if (id == _encounter) GuiKit.Frame(new Rect(r.x + 16, ey, 520, 52), Palette.GoldBright, 2);
                ey += 58;
            }
            if (_encounter != null)
                GuiKit.Label(new Rect(r.x + 16, ey, 520, 60), _db.Encounter(_encounter).Description, 15, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);

            GuiKit.Label(new Rect(r.x + 570, r.y + 10, 400, 28), "Cinturón (toca para cambiar)", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var usable = _db.PotionOrder.Where(p => _db.Potion(p).UsableInCombat).ToList();
            for (int s = 0; s < _belt.Length; s++)
            {
                var def = _db.Potions.TryGetValue(_belt[s] ?? "", out var d) ? d : null;
                var rect = new Rect(r.x + 570, r.y + 44 + s * 62, 520, 56);
                if (GuiKit.Button(rect, def != null ? $"<b>{def.Name}</b> ×3\n<size=13>{def.Description}</size>" : "vacía", true, 16))
                {
                    int idx = usable.IndexOf(_belt[s]);
                    _belt[s] = usable[(idx + 1) % usable.Count];
                }
            }

            GuiKit.Label(new Rect(r.x + 1120, r.y + 10, 300, 28), "Semilla", 22, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            _seedText = GUI.TextField(new Rect(r.x + 1120, r.y + 44, 260, 40), _seedText, 20, GuiKit.ButtonStyle(20));
            if (GuiKit.Button(new Rect(r.x + 1390, r.y + 44, 120, 40), "Nueva", true, 18)) _seedText = NewSeed().ToString();
            GuiKit.Label(new Rect(r.x + 1120, r.y + 88, 660, 40), "La misma semilla con las mismas decisiones repite el combate evento por evento.", 15, Palette.ParchmentShadow, TextAnchor.UpperLeft, false, true);
            string day = Planets.DisplayName(Planets.RulerOfWeekday((int)DateTime.Now.DayOfWeek));
            if (GuiKit.Button(new Rect(r.x + 1120, r.y + 130, 390, 40), (_planetaryDay ? "<b>Día planetario: " : "Día planetario: ") + day + (_planetaryDay ? " (+15 %)</b>" : " (apagado)"), true, 16))
                _planetaryDay = !_planetaryDay;

            bool ok = _team.Count >= 1 && _encounter != null && ulong.TryParse(_seedText, out _);
            string summary = $"Equipo: {string.Join(", ", _team.Select(id => _db.Witch(id).ShortName))}" + (_guest != null ? $" · Invitada: {_db.Witch(_guest).ShortName}" : "");
            GuiKit.Label(new Rect(r.x + 1120, r.y + 178, 660, 30), summary, 16, Palette.Parchment);
            if (GuiKit.Button(new Rect(r.x + 1120, r.y + 214, 660, 70), "<b>Comenzar combate</b>", ok, 28) && ulong.TryParse(_seedText, out var seed))
                OnStart?.Invoke(BuildSetup(seed));
        }
    }
}
