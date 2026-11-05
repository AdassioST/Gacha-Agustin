using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Crafting
{
    /// <summary>Qué hace falta para una receta: estaciones, armas y recursos.</summary>
    public sealed class RecipeCheck
    {
        public RecipeDef Recipe;
        public bool Known;
        public readonly List<string> MissingStations = new List<string>();
        public readonly List<string> MissingWeapons = new List<string>();
        public string MissingResources = "";
        /// <summary>Ejemplares de arma que se consumirían, en el orden de los ingredientes.</summary>
        public readonly List<int> WeaponUids = new List<int>();
        public Bundle Resources = new Bundle();

        public bool CanCraft => Known && MissingStations.Count == 0 && MissingWeapons.Count == 0 && MissingResources.Length == 0;

        public string Why()
        {
            var parts = new List<string>();
            if (!Known) parts.Add("receta desconocida");
            if (MissingStations.Count > 0) parts.Add("falta estar junto a: " + string.Join(" y ", MissingStations));
            if (MissingWeapons.Count > 0) parts.Add("faltan armas: " + string.Join(", ", MissingWeapons));
            if (MissingResources.Length > 0) parts.Add(MissingResources);
            return string.Join("; ", parts);
        }
    }

    /// <summary>
    /// Yunque Astral y Altar del Árbol (GDD 15): fabricar armas en estaciones (algunas recetas piden dos a la vez),
    /// fusionar, reforjar prefijos, equipar y refinar sal.
    /// </summary>
    public sealed class Forge
    {
        private readonly MetaGame _g;

        public Forge(MetaGame game) { _g = game; }

        private MetaDatabase Meta => _g.Meta;
        private PlayerProfile Profile => _g.Profile;

        /// <summary>Agrega un arma al inventario sin receta (botín, recompensas, perfil de prueba).</summary>
        public WeaponInstance Grant(string defId, string prefix = "")
        {
            if (!Meta.Weapons.ContainsKey(defId)) throw new KeyNotFoundException("Arma desconocida: " + defId);
            var w = new WeaponInstance { Uid = Profile.NextWeaponUid++, DefId = defId, Prefix = prefix ?? "" };
            Profile.Weapons.Add(w);
            return w;
        }

        public string DisplayName(WeaponInstance w)
        {
            var def = Meta.Weapon(w.DefId);
            var px = Meta.Prefix(w.Prefix);
            return (px != null ? px.Name + " " : "") + def.Name;
        }

        public void LearnRecipe(string id) => Profile.Recipes.Add(id);

        // ------------------------------------------------------------------ Recetas

        public RecipeCheck Check(string recipeId)
        {
            var rec = Meta.Recipes[recipeId];
            var check = new RecipeCheck { Recipe = rec, Known = Profile.Recipes.Contains(recipeId) };
            foreach (var s in rec.Stations)
                if (!Profile.Stations.Contains(s)) check.MissingStations.Add(Meta.Station(s)?.Name ?? s);

            var used = new HashSet<int>();
            foreach (var input in rec.Inputs)
            {
                if (input.IsWeapon)
                {
                    var found = FindWeapon(input, used);
                    if (found == null) check.MissingWeapons.Add(Describe(input));
                    else { used.Add(found.Uid); check.WeaponUids.Add(found.Uid); }
                }
                else check.Resources.Add(input.Resource, input.Amount);
            }
            check.MissingResources = _g.Ledger.Missing(check.Resources);
            return check;
        }

        public string Describe(RecipeInput input)
        {
            if (input.WeaponId != null) return Meta.Weapon(input.WeaponId).Name;
            if (input.Tier.HasValue)
            {
                string tier = Meta.Tier(input.Tier.Value)?.Name ?? ("nivel " + input.Tier.Value);
                return input.Planet.HasValue ? $"un arma {tier} de {Planets.DisplayName(input.Planet.Value)}" : "un arma " + tier;
            }
            return $"{input.Amount:N0} {_g.Meta.Catalog.NameOf(input.Resource)}";
        }

        /// <summary>Busca un ejemplar que sirva: primero uno sin equipar y sin prefijo.</summary>
        private WeaponInstance FindWeapon(RecipeInput input, HashSet<int> used)
        {
            WeaponInstance best = null;
            foreach (var w in Profile.Weapons)
            {
                if (used.Contains(w.Uid) || Profile.EquippedBy(w.Uid) != null) continue;
                var def = Meta.Weapon(w.DefId);
                bool match = input.WeaponId != null
                    ? def.Id == input.WeaponId
                    : def.Tier == input.Tier && (!input.Planet.HasValue || def.Planet == input.Planet.Value);
                if (!match) continue;
                if (best == null || (best.Prefix.Length > 0 && w.Prefix.Length == 0)) best = w;
            }
            return best;
        }

        public OpResult Craft(string recipeId, out WeaponInstance created)
        {
            created = null;
            if (!Meta.Recipes.ContainsKey(recipeId)) return OpResult.Fail("Receta desconocida.");
            var check = Check(recipeId);
            if (!check.CanCraft) return OpResult.Fail(check.Why());
            var rec = check.Recipe;
            string flow = rec.Kind == RecipeKind.Fusion ? "fusion" : "forja";
            if (!_g.Ledger.TrySpend(check.Resources, flow, rec.Id)) return OpResult.Fail(_g.Ledger.Missing(check.Resources));
            foreach (var uid in check.WeaponUids) Profile.Weapons.RemoveAll(w => w.Uid == uid);
            created = Grant(rec.Output);
            return OpResult.Success(Meta.Weapon(rec.Output).Name);
        }

        /// <summary>Recetas conocidas que se pueden hacer ahora mismo.</summary>
        public List<string> Craftable()
        {
            var list = new List<string>();
            foreach (var id in Meta.RecipeOrder) if (Profile.Recipes.Contains(id) && Check(id).CanCraft) list.Add(id);
            return list;
        }

        /// <summary>La Herbolaria (GDD 15.5): recetas que usan un arma o un recurso.</summary>
        public List<string> RecipesUsing(string weaponOrResource)
        {
            var list = new List<string>();
            foreach (var id in Meta.RecipeOrder)
                foreach (var i in Meta.Recipes[id].Inputs)
                    if (i.WeaponId == weaponOrResource || i.Resource == weaponOrResource) { list.Add(id); break; }
            return list;
        }

        // ------------------------------------------------------------------ Reforja (GDD 15.4)

        public Bundle ReforgeCost(int uid)
        {
            var w = Profile.Weapon(uid) ?? throw new KeyNotFoundException("Arma desconocida");
            var def = Meta.Weapon(w.DefId);
            int tier = Math.Min(5, def.Tier);
            return new Bundle()
                .Add(Res.Almitas, Meta.ReforgeAlmitasPerTier * tier)
                .Add(Res.SalCruda(def.Planet), Meta.ReforgeSaltPerTier * tier);
        }

        /// <summary>Vuelve a tirar el prefijo con almitas y sal, al azar según el peso de cada prefijo.</summary>
        public OpResult Reforge(int uid)
        {
            var w = Profile.Weapon(uid);
            if (w == null) return OpResult.Fail("Arma desconocida.");
            if (!Profile.Stations.Contains("yunque")) return OpResult.Fail("Hace falta el Yunque Astral.");
            var cost = ReforgeCost(uid);
            if (!_g.Ledger.TrySpend(cost, "reforja", DisplayName(w))) return OpResult.Fail(_g.Ledger.Missing(cost));
            w.Prefix = RollPrefix(_g.Rng).Id;
            return OpResult.Success(DisplayName(w));
        }

        public PrefixDef RollPrefix(IRandomSource rng)
        {
            int total = 0;
            foreach (var p in Meta.Prefixes) total += p.Weight;
            int roll = rng.NextInt(total);
            foreach (var p in Meta.Prefixes)
            {
                if (roll < p.Weight) return p;
                roll -= p.Weight;
            }
            return Meta.Prefixes[0];
        }

        // ------------------------------------------------------------------ Equipar

        public OpResult Equip(string witchId, int uid)
        {
            if (!Profile.Witches.TryGetValue(witchId, out var wp)) return OpResult.Fail("No tienes a esa bruja.");
            if (Profile.Weapon(uid) == null) return OpResult.Fail("Arma desconocida.");
            var other = Profile.EquippedBy(uid);
            if (other != null) Profile.Witches[other].WeaponUid = 0;
            wp.WeaponUid = uid;
            return OpResult.Success(DisplayName(Profile.Weapon(uid)));
        }

        public void Unequip(string witchId)
        {
            if (Profile.Witches.TryGetValue(witchId, out var wp)) wp.WeaponUid = 0;
        }

        // ------------------------------------------------------------------ Altar del Árbol: refinar sal

        public Bundle RefineCost(Planet planet, int toTier, int count = 1)
        {
            if (toTier < 1 || toTier > 2) throw new ArgumentOutOfRangeException(nameof(toTier));
            return new Bundle()
                .Add(Res.Salt(planet, toTier - 1), (long)Meta.RefineFrom[toTier] * count)
                .Add(Res.Almitas, Meta.RefineAlmitas[toTier] * count);
        }

        public OpResult Refine(Planet planet, int toTier, int count = 1)
        {
            if (!Profile.Stations.Contains("altar")) return OpResult.Fail("Hace falta el Altar del Árbol.");
            if (count < 1) return OpResult.Fail("Cantidad inválida.");
            var cost = RefineCost(planet, toTier, count);
            string note = $"{count} × {Planets.SaltName(planet)} → {(toTier == 1 ? "Destilada" : "Quintaesencia")}";
            if (!_g.Ledger.TrySpend(cost, "altar.refinar", note)) return OpResult.Fail(_g.Ledger.Missing(cost));
            _g.Ledger.Apply(Res.Salt(planet, toTier), count, "altar.refinar", note);
            return OpResult.Success(note);
        }
    }
}
