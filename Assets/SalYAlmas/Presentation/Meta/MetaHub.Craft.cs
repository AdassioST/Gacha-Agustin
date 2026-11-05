using System.Collections.Generic;
using System.Linq;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Save;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    public sealed partial class MetaHub
    {
        // Yunque
        private int _selectedWeapon;
        private int _weaponPage;
        private int _recipePage;
        private int _recipeFilter; // 0 = se puede hacer, 1 a 5 = nivel, 6 = fusiones
        private Planet _refinePlanet = Planet.Sol;

        // Caldero
        private string _base = "agua_termal";
        private int _saltChoice; // 0 a 6 planeta, 7 Sal Negra, 8 almitas, 9 nada
        private int _purity;
        private string _reagent = "flor_de_sal";

        private void DrawForge(Rect a)
        {
            DrawWeapons(new Rect(a.x, a.y, 620, a.height));
            DrawRecipes(new Rect(a.x + 630, a.y, 760, a.height));
            DrawAltar(new Rect(a.x + 1400, a.y, a.width - 1400, a.height));
        }

        private void DrawWeapons(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 14, r.y + 6, r.width - 28, 36), "Tus armas", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            var weapons = Game.Profile.Weapons;
            _weaponPage = Paged(new Rect(r.x + 12, r.y + 46, r.width - 24, 480), weapons, _weaponPage, 48, (box, w) =>
            {
                string by = Game.Profile.EquippedBy(w.Uid);
                if (GuiKit.Button(box, Game.Forge.DisplayName(w) + (by != null ? $"  ·  {WitchName(by)}" : ""), true, 16)) _selectedWeapon = w.Uid;
                if (w.Uid == _selectedWeapon) GuiKit.Frame(box, Palette.GoldBright, 3f);
            });

            var sel = Game.Profile.Weapon(_selectedWeapon);
            float y = r.y + 534, x = r.x + 14, width = r.width - 28;
            if (sel == null)
            {
                GuiKit.Label(new Rect(x, y, width, 40), "Elige un arma para reforjarla o equiparla.", 18, Palette.ParchmentShadow);
                return;
            }
            var def = _meta.Weapon(sel.DefId);
            var family = _meta.Family(def.Family);
            var prefix = _meta.Prefix(sel.Prefix);
            var hits = def.Hits.Count > 0 ? def.Hits : family?.Hits ?? new List<HitDef>();
            int shift = prefix != null ? (int)prefix.Mods.Get(Core.Combat.ModKeys.FrameShift) : 0;
            string frames = string.Join(", ", hits.Select(h => (h.Frame + shift).ToString()));
            string info = $"<b>{Game.Forge.DisplayName(sel)}</b>\nATQ {def.Atk:0}  ·  {Planets.DisplayName(def.Planet)}  ·  {family?.Name}\nFrames del básico: {frames}{(shift != 0 ? $" ({shift:+0;-0})" : "")}"
                + (prefix != null ? $"\nPrefijo: {prefix.Mods}" : "\nSin prefijo");
            GuiKit.Label(new Rect(x, y, width, 112), info, 17, Palette.Parchment, TextAnchor.UpperLeft, false, true);
            y += 118;
            if (GuiKit.Button(new Rect(x, y, width, 56), $"Reforjar\n<size=14>{Game.Forge.ReforgeCost(sel.Uid).Describe(_meta.Catalog)}</size>", true, 20)) Apply(Game.Forge.Reforge(sel.Uid));
            y += 62;
            GuiKit.Label(new Rect(x, y, width, 26), "Equipar a:", 18, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 28;
            int k = 0;
            foreach (var id in _meta.RosterOrder)
            {
                if (!Game.Profile.Owns(id) || !_meta.HasKit(id)) continue;
                bool hasIt = Game.Profile.Witches[id].WeaponUid == sel.Uid;
                var box = new Rect(x + (k % 3) * (width / 3), y + (k / 3) * 44, width / 3 - 6, 40);
                if (GuiKit.Button(box, (hasIt ? "● " : "") + Game.Combat.Witch(id).ShortName, true, 16))
                {
                    if (hasIt) { Game.Forge.Unequip(id); Save(); }
                    else Apply(Game.Forge.Equip(id, sel.Uid));
                }
                k++;
            }
        }

        private void DrawRecipes(Rect r)
        {
            GuiKit.PanelBox(r);
            GuiKit.Label(new Rect(r.x + 14, r.y + 6, r.width - 28, 36), "Yunque Astral: recetas", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            string[] filters = { "Se pueden", "Nivel 1", "Nivel 2", "Nivel 3", "Nivel 4", "Nivel 5", "Fusiones" };
            for (int i = 0; i < filters.Length; i++)
            {
                var box = new Rect(r.x + 12 + i * 105, r.y + 46, 100, 40);
                if (GuiKit.Button(box, filters[i], true, 15)) { _recipeFilter = i; _recipePage = 0; }
                if (_recipeFilter == i) GuiKit.Frame(box, Palette.GoldBright, 2f);
            }
            IEnumerable<string> ids = _meta.RecipeOrder.Where(id => Game.Profile.Recipes.Contains(id));
            if (_recipeFilter == 0) ids = ids.Where(id => Game.Forge.Check(id).CanCraft);
            else if (_recipeFilter == 6) ids = ids.Where(id => _meta.Recipes[id].Kind == RecipeKind.Fusion);
            else ids = ids.Where(id => _meta.Recipes[id].Kind != RecipeKind.Fusion && _meta.Weapon(_meta.Recipes[id].Output).Tier == _recipeFilter);
            var list = ids.ToList();
            _recipePage = Paged(new Rect(r.x + 12, r.y + 94, r.width - 24, r.height - 104), list, _recipePage, 74, (box, id) =>
            {
                var check = Game.Forge.Check(id);
                var rec = check.Recipe;
                var parts = rec.Inputs.Select(i => Game.Forge.Describe(i));
                string stations = string.Join(" + ", rec.Stations.Select(s => _meta.Station(s)?.Name ?? s));
                string text = $"{_meta.Weapon(rec.Output).Name}  ·  {stations}\n<size=13>{string.Join(", ", parts)}</size>";
                if (GuiKit.Button(box, text, check.CanCraft, 17))
                {
                    var result = Game.Forge.Craft(id, out var made);
                    Apply(result);
                    if (made != null) _selectedWeapon = made.Uid;
                }
            });
            if (list.Count == 0) GuiKit.Label(new Rect(r.x + 14, r.y + 100, r.width - 28, 40), "Nada por aquí. Prueba otro filtro.", 18, Palette.ParchmentShadow);
        }

        private void DrawAltar(Rect r)
        {
            GuiKit.PanelBox(r);
            float x = r.x + 12, width = r.width - 24, y = r.y + 6;
            GuiKit.Label(new Rect(x, y, width, 36), "Altar del Árbol", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 42;
            for (int i = 0; i < Planets.All.Length; i++)
            {
                var p = Planets.All[i];
                var box = new Rect(x + (i % 3) * (width / 3), y + (i / 3) * 42, width / 3 - 4, 38);
                if (GuiKit.Button(box, Planets.DisplayName(p), true, 15)) _refinePlanet = p;
                if (_refinePlanet == p) GuiKit.Frame(box, Palette.GoldBright, 2f);
            }
            y += 132;
            var l = Game.Ledger;
            GuiKit.Label(new Rect(x, y, width, 84), $"Cruda {l.Balance(Res.SalCruda(_refinePlanet))}\nDestilada {l.Balance(Res.SalDestilada(_refinePlanet))}\nQuintaesencia {l.Balance(Res.Quintaesencia(_refinePlanet))}", 18, Palette.Parchment, TextAnchor.UpperLeft);
            y += 90;
            string[] names = { "", "Destilada", "Quintaesencia" };
            for (int tier = 1; tier <= 2; tier++)
            {
                if (GuiKit.Button(new Rect(x, y, width, 66), $"Refinar a {names[tier]}\n<size=13>{Game.Forge.RefineCost(_refinePlanet, tier).Describe(_meta.Catalog)}</size>", l.CanAfford(Game.Forge.RefineCost(_refinePlanet, tier)), 17))
                    Apply(Game.Forge.Refine(_refinePlanet, tier));
                y += 72;
            }
        }

        // ------------------------------------------------------------------ Caldero

        private BrewSalt CurrentSalt()
        {
            if (_saltChoice < 7) return BrewSalt.Of(Planets.All[_saltChoice], _purity);
            if (_saltChoice == 7) return BrewSalt.Black;
            return _saltChoice == 8 ? BrewSalt.Souls : BrewSalt.NoSalt;
        }

        private void DrawCauldron(Rect a)
        {
            var r = new Rect(a.x, a.y, 1180, a.height);
            GuiKit.PanelBox(r);
            float x = r.x + 16, width = r.width - 32, y = r.y + 6;
            var rules = _meta.Brewing;
            GuiKit.Label(new Rect(x, y, width, 36), "Caldero de Bruja: base + sal + reactivo + almitas", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 44;

            GuiKit.Label(new Rect(x, y, width, 26), "Base", 20, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 30;
            for (int i = 0; i < rules.Bases.Count; i++)
            {
                var b = rules.Bases[i];
                var box = new Rect(x + i * 260, y, 254, 44);
                if (GuiKit.Button(box, b.Name, true, 17)) _base = b.Id;
                if (_base == b.Id) GuiKit.Frame(box, Palette.GoldBright, 3f);
            }
            y += 54;

            GuiKit.Label(new Rect(x, y, width, 26), "Sal", 20, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 30;
            for (int i = 0; i < 10; i++)
            {
                string label = i < 7 ? Planets.DisplayName(Planets.All[i]) : i == 7 ? "Sal Negra" : i == 8 ? "Almitas" : "Sin sal";
                var box = new Rect(x + (i % 5) * 226, y + (i / 5) * 48, 220, 44);
                if (GuiKit.Button(box, label, true, 17)) _saltChoice = i;
                if (_saltChoice == i) GuiKit.Frame(box, Palette.GoldBright, 3f);
            }
            y += 104;
            string[] purity = { "Cruda (básica)", "Destilada (mejorada)", "Quintaesencia (maestra)" };
            for (int i = 0; i < 3; i++)
            {
                var box = new Rect(x + i * 300, y, 294, 44);
                if (GuiKit.Button(box, purity[i], _saltChoice < 7, 17)) _purity = i;
                if (_purity == i && _saltChoice < 7) GuiKit.Frame(box, Palette.GoldBright, 3f);
            }
            y += 54;

            GuiKit.Label(new Rect(x, y, width, 26), "Reactivo", 20, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            y += 30;
            for (int i = 0; i < rules.Reagents.Count; i++)
            {
                var re = rules.Reagents[i];
                var box = new Rect(x + i * 226, y, 220, 44);
                if (GuiKit.Button(box, re.Name, true, 16)) _reagent = re.Id;
                if (_reagent == re.Id) GuiKit.Frame(box, Palette.GoldBright, 3f);
            }
            y += 56;

            var salt = CurrentSalt();
            var cost = Game.Cauldron.Cost(_base, salt, _reagent);
            bool known = Game.Cauldron.Match(_base, salt, _reagent) is BrewRecipeDef match && Game.Profile.Recetario.Contains(match.Potion);
            string preview = known ? "Receta conocida: " + Game.Combat.Potion(Game.Cauldron.Match(_base, salt, _reagent).Potion).Name : "Resultado desconocido. Una mezcla sin receta da una Salmuera Turbia.";
            GuiKit.Label(new Rect(x, y, width, 30), preview, 19, Palette.Parchment);
            y += 32;
            GuiKit.Label(new Rect(x, y, width, 30), "Costo: " + cost.Describe(_meta.Catalog), 17, Game.Ledger.CanAfford(cost) ? Palette.Parchment : Palette.Danger);
            y += 38;
            if (GuiKit.Button(new Rect(x, y, 420, 70), "Preparar", Game.Ledger.CanAfford(cost), 30))
            {
                var result = Game.Cauldron.Brew(_base, salt, _reagent);
                Note(result.Message, result.Ok);
                if (result.Ok) Save();
            }

            var side = new Rect(r.xMax + 10, a.y, a.xMax - r.xMax - 10, a.height);
            GuiKit.PanelBox(side);
            float sx = side.x + 14, sw = side.width - 28, sy = side.y + 6;
            GuiKit.Label(new Rect(sx, sy, sw, 36), "Tu inventario", 26, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            sy += 42;
            foreach (var kv in Game.Profile.Potions.OrderBy(k => k.Key, System.StringComparer.Ordinal))
            {
                GuiKit.Label(new Rect(sx, sy, sw, 28), $"{Cauldron.NameOf(_meta, kv.Key)}  ×{kv.Value}", 17, Palette.Parchment);
                sy += 30;
            }
            sy += 14;
            GuiKit.Label(new Rect(sx, sy, sw, 36), $"Recetario ({Game.Profile.Recetario.Count}/{rules.Recipes.Count})", 24, Palette.GoldBright, TextAnchor.MiddleLeft, true);
            sy += 40;
            foreach (var rec in rules.Recipes)
            {
                bool found = Game.Profile.Recetario.Contains(rec.Potion);
                string text = found ? $"{Game.Combat.Potion(rec.Potion).Name}: {rules.Base(rec.Base).Name} + {rec.SaltKind} + {rules.Reagent(rec.Reagent).Name}" : "???";
                GuiKit.Label(new Rect(sx, sy, sw, 26), text, 14, found ? Palette.Parchment : Palette.ParchmentShadow);
                sy += 27;
            }
        }
    }
}
