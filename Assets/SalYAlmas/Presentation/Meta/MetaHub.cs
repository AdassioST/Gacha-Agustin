using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Interfaz de prueba de la Fase 2 (IMGUI y figuras simples, como la Fase 1). Solo manda órdenes al núcleo y dibuja
    /// su estado: las reglas viven en Core. Las pestañas están en los otros archivos de esta clase.
    /// </summary>
    public sealed partial class MetaHub
    {
        private enum Tab { Fresno, Grados, Yunque, Caldero, Termas, Libro }

        private const string Slot = "perfil";
        private static readonly DateTime Epoch = new DateTime(2026, 1, 1);

        private readonly MetaDatabase _meta;
        private readonly ISaveStore _store;
        private Tab _tab = Tab.Fresno;
        private string _note = "";
        private Color _noteColor = Palette.Parchment;

        /// <summary>Pide a la escena un combate con el HUD real; al terminar devuelve el Battle.</summary>
        public Action<BattleSetup, Action<Battle>> StartBattle;
        public Action OpenFormation;

        public MetaGame Game { get; private set; }

        public MetaHub(MetaDatabase meta, ISaveStore store)
        {
            _meta = meta;
            _store = store;
            LoadOrCreate();
        }

        /// <summary>El reloj real solo se lee aquí, en la presentación; el núcleo recibe el día como entrada.</summary>
        private static int Today() => (DateTime.Now.Date - Epoch).Days;

        private void LoadOrCreate()
        {
            try
            {
                Game = _store.Exists(Slot) ? MetaGame.Load(_meta, _store, Slot) : null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo cargar el perfil: " + ex.Message);
                Game = null;
            }
            if (Game == null) NewProfile();
            else Game.SetDay(Today());
        }

        private void NewProfile()
        {
            var seed = (ulong)(DateTime.Now.Ticks % 1000000007L);
            Game = new MetaGame(_meta, MetaGame.TestProfile(_meta, seed, Today()));
            Save();
            ResetSelections();
        }

        private void Save()
        {
            try { Game.Save(_store, Slot); }
            catch (Exception ex) { Debug.LogException(ex); Note("No se pudo guardar: " + ex.Message, false); }
        }

        private void Note(string text, bool ok = true)
        {
            _note = text ?? "";
            _noteColor = ok ? Palette.Parchment : Palette.Danger;
        }

        private void Apply(OpResult r)
        {
            Note(r.Message, r.Ok);
            if (r.Ok) Save();
        }

        private void ResetSelections()
        {
            _curses.Clear();
            _ecoGrafts.Clear();
            _ecoTrained = -1;
            _team.Clear();
            _trialResult = null;
            _lastPull.Clear();
            _selectedWeapon = 0;
        }

        // ------------------------------------------------------------------ Marco

        public void Draw()
        {
            GuiKit.Fill(new Rect(0, 0, GuiKit.Width, GuiKit.Height), Palette.Abyss);
            DrawHeader();
            var area = new Rect(16, 96, GuiKit.Width - 32, GuiKit.Height - 150);
            switch (_tab)
            {
                case Tab.Fresno: DrawFresno(area); break;
                case Tab.Grados: DrawGrades(area); break;
                case Tab.Yunque: DrawForge(area); break;
                case Tab.Caldero: DrawCauldron(area); break;
                case Tab.Termas: DrawTermas(area); break;
                case Tab.Libro: DrawLedger(area); break;
            }
            GuiKit.Label(new Rect(20, GuiKit.Height - 46, GuiKit.Width - 40, 40), _note, 20, _noteColor);
        }

        private void DrawHeader()
        {
            var names = new[] { "Fresno", "Grados", "Yunque", "Caldero", "Termas", "Libro" };
            for (int i = 0; i < names.Length; i++)
            {
                var r = new Rect(16 + i * 142, 14, 134, 66);
                if (GuiKit.Button(r, names[i], true, 24)) { _tab = (Tab)i; _note = ""; }
                if ((int)_tab == i) GuiKit.Frame(r, Palette.GoldBright, 3f);
            }
            var l = Game.Ledger;
            string wallet = $"Almitas {l.Balance(Res.Almitas):N0}   ·   Savia {l.Balance(Res.Savia):N0}   ·   Sales de Baño {l.Balance(Res.SalesDeBano):N0}   ·   Polvo Astral {l.Balance(Res.PolvoAstral):N0}";
            GuiKit.Label(new Rect(880, 14, 700, 66), wallet, 18, Palette.Parchment, TextAnchor.MiddleLeft, false, true);
            if (GuiKit.Button(new Rect(1590, 14, 150, 66), "Combate libre\n<size=13>Fase 1</size>", !Game.Fresno.InRun, 18)) OpenFormation?.Invoke();
            if (GuiKit.Button(new Rect(1748, 14, 156, 66), "Perfil nuevo\n<size=13>de prueba</size>", true, 18))
            {
                _store.Delete(Slot);
                NewProfile();
                Note("Perfil de prueba nuevo.");
            }
        }

        // ------------------------------------------------------------------ Ayudas de dibujo

        private static string Stars(int rarity) => new string('★', rarity);

        private string WitchName(string id) => _meta.Roster.TryGetValue(id, out var r) ? r.Name : id;

        /// <summary>Lista paginada: dibuja una fila por elemento y los botones de página. Devuelve la página corregida.</summary>
        private static int Paged<T>(Rect area, IList<T> items, int page, float rowHeight, Action<Rect, T> row)
        {
            int perPage = Mathf.Max(1, Mathf.FloorToInt((area.height - 40) / rowHeight));
            int pages = Mathf.Max(1, Mathf.CeilToInt(items.Count / (float)perPage));
            page = Mathf.Clamp(page, 0, pages - 1);
            for (int i = 0; i < perPage && page * perPage + i < items.Count; i++)
                row(new Rect(area.x, area.y + i * rowHeight, area.width, rowHeight - 4), items[page * perPage + i]);
            if (pages > 1)
            {
                float y = area.yMax - 36;
                if (GuiKit.Button(new Rect(area.x, y, 70, 34), "◀", page > 0, 18)) page--;
                GuiKit.Label(new Rect(area.x + 76, y, 120, 34), $"{page + 1} / {pages}", 18, Palette.Parchment, TextAnchor.MiddleCenter);
                if (GuiKit.Button(new Rect(area.x + 200, y, 70, 34), "▶", page < pages - 1, 18)) page++;
            }
            return page;
        }
    }
}
