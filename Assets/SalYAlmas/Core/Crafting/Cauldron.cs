using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;

namespace SalYAlmas.Core.Crafting
{
    /// <summary>De dónde salen los ingredientes: el libro contable del perfil o la bolsa de una run del Fresno.</summary>
    public interface IIngredientSource
    {
        long Have(string resource);
        bool TrySpend(Bundle cost, string flow, string note);
    }

    public sealed class LedgerSource : IIngredientSource
    {
        private readonly Ledger _ledger;
        public LedgerSource(Ledger ledger) { _ledger = ledger; }
        public long Have(string resource) => _ledger.Balance(resource);
        public bool TrySpend(Bundle cost, string flow, string note) => _ledger.TrySpend(cost, flow, note);
    }

    public sealed class BrewResult
    {
        public bool Ok;
        public string Message = "";
        /// <summary>Id de la Salmuera resultante: "salmuera_aurea", "salmuera_aurea@mejorada", "salmuera_turbia@3".</summary>
        public string PotionId;
        public bool Turbid;
        public bool Discovered;
        public string Potency = "";
    }

    /// <summary>
    /// Caldero de Bruja (GDD 14): Salmuera = base + sal + reactivo + almitas. La pureza de la sal fija la potencia y una
    /// mezcla sin receta da una Salmuera Turbia con un efecto al azar. Las recetas nuevas se anotan en el Recetario.
    /// </summary>
    public sealed class Cauldron
    {
        public const string TurbidId = "salmuera_turbia";

        private readonly MetaGame _g;

        public Cauldron(MetaGame game) { _g = game; }

        private BrewingRules Rules => _g.Meta.Brewing;

        /// <summary>Costo completo de una preparación.</summary>
        public Bundle Cost(string baseId, BrewSalt salt, string reagentId)
        {
            var cost = new Bundle();
            var b = Rules.Base(baseId);
            if (b != null) foreach (var kv in b.Cost.Items) cost.Add(kv.Key, kv.Value);
            switch (salt.Kind)
            {
                case BrewSaltKind.Planet:
                case BrewSaltKind.Refined:
                    cost.Add(Res.Salt(salt.Planet, salt.Purity), Rules.SaltAmount);
                    break;
                case BrewSaltKind.BlackSalt:
                    cost.Add(Res.SalNegra, Rules.SaltAmount);
                    break;
                case BrewSaltKind.Almitas:
                    cost.Add(Res.Almitas, Rules.AlmitasConcentradas);
                    break;
            }
            var r = Rules.Reagent(reagentId);
            if (r != null) cost.Add(r.Resource, 1);
            cost.Add(Res.Almitas, Rules.AlmitasPerBrew);
            return cost;
        }

        /// <summary>La receta que corresponde a los ingredientes, o null (Salmuera Turbia).</summary>
        public BrewRecipeDef Match(string baseId, BrewSalt salt, string reagentId)
        {
            BrewRecipeDef refined = null;
            foreach (var r in Rules.Recipes)
            {
                if (r.Base != baseId || r.Reagent != reagentId) continue;
                switch (r.SaltKind)
                {
                    case BrewSaltKind.Planet:
                        if (salt.Kind == BrewSaltKind.Planet && salt.Planet == r.Planet) return r;
                        break;
                    case BrewSaltKind.Refined:
                        if ((salt.Kind == BrewSaltKind.Planet || salt.Kind == BrewSaltKind.Refined) && salt.Purity >= 1) refined = r;
                        break;
                    default:
                        if (salt.Kind == r.SaltKind) return r;
                        break;
                }
            }
            return refined;
        }

        /// <summary>Prepara en el Caldero de las Termas con el inventario del perfil (máximo cinco de cada Salmuera).</summary>
        public BrewResult Brew(string baseId, BrewSalt salt, string reagentId)
        {
            if (!_g.Profile.Stations.Contains("caldero")) return new BrewResult { Message = "Hace falta el Caldero de Bruja." };
            var preview = Match(baseId, salt, reagentId);
            if (preview != null)
            {
                string id = VariantId(preview.Potion, PotencyOf(preview, salt));
                int have = _g.Profile.Potions.TryGetValue(id, out var n) ? n : 0;
                if (have >= _g.Combat.Rules.Potions.MaxPerType)
                    return new BrewResult { Message = $"Ya tienes {have}: máximo cinco de cada tipo (GDD 14.3)." };
            }
            var result = BrewWith(new LedgerSource(_g.Ledger), baseId, salt, reagentId, "salmuera");
            if (result.Ok)
            {
                _g.Profile.Potions.TryGetValue(result.PotionId, out var count);
                _g.Profile.Potions[result.PotionId] = Math.Min(_g.Combat.Rules.Potions.MaxPerType, count + 1);
            }
            return result;
        }

        /// <summary>Prepara con cualquier fuente de ingredientes (también la bolsa de una run). No toca el inventario.</summary>
        public BrewResult BrewWith(IIngredientSource source, string baseId, BrewSalt salt, string reagentId, string flow)
        {
            if (Rules.Base(baseId) == null) return new BrewResult { Message = "Base desconocida." };
            if (Rules.Reagent(reagentId) == null) return new BrewResult { Message = "Reactivo desconocido." };
            var cost = Cost(baseId, salt, reagentId);
            var recipe = Match(baseId, salt, reagentId);
            string note = recipe != null ? recipe.Potion : TurbidId;
            if (!source.TrySpend(cost, flow, note)) return new BrewResult { Message = "Faltan ingredientes: " + cost.Describe(_g.Meta.Catalog) };

            if (recipe == null)
            {
                int k = _g.Rng.NextInt(Rules.Turbid.Count);
                return new BrewResult
                {
                    Ok = true, Turbid = true, PotionId = TurbidId + "@" + k, Potency = "turbia",
                    Message = "La mezcla se enturbia: " + Rules.Turbid[k].Name + ". " + Rules.Turbid[k].Description,
                };
            }
            int tier = PotencyOf(recipe, salt);
            bool discovered = _g.Profile.Recetario.Add(recipe.Potion);
            var potion = _g.Combat.Potion(recipe.Potion);
            return new BrewResult
            {
                Ok = true, PotionId = VariantId(recipe.Potion, tier), Discovered = discovered, Potency = Rules.Potency[tier].Name,
                Message = $"{potion.Name} ({Rules.Potency[tier].Name})" + (discovered ? ". ¡Receta nueva en el Recetario!" : ""),
            };
        }

        /// <summary>Potencia según la pureza de la sal (GDD 14.1). Sal Negra, almitas y agua sola dan la básica.</summary>
        public static int PotencyOf(BrewRecipeDef recipe, BrewSalt salt)
        {
            if (salt.Kind != BrewSaltKind.Planet && salt.Kind != BrewSaltKind.Refined) return 0;
            if (recipe.SaltKind != BrewSaltKind.Planet && recipe.SaltKind != BrewSaltKind.Refined) return 0;
            return Math.Max(0, Math.Min(2, salt.Purity));
        }

        public string VariantId(string potionId, int tier) =>
            tier <= 0 ? potionId : potionId + "@" + Rules.Potency[Math.Min(tier, Rules.Potency.Count - 1)].Name;

        // ------------------------------------------------------------------ Definiciones para el combate

        /// <summary>
        /// Definición de una Salmuera con potencia o Turbia para BattleSetup.CustomPotions. Las básicas son las de
        /// potions.json (devuelve null: el combate ya las conoce).
        /// </summary>
        public static PotionDef VariantDef(MetaDatabase meta, string id)
        {
            if (id == null || !id.Contains("@")) return null;
            int at = id.IndexOf('@');
            string baseId = id.Substring(0, at);
            string suffix = id.Substring(at + 1);
            var rules = meta.Brewing;
            if (baseId == TurbidId)
            {
                if (!int.TryParse(suffix, out int k) || k < 0 || k >= rules.Turbid.Count) return null;
                var t = rules.Turbid[k];
                var def = new PotionDef
                {
                    Id = id, Name = "Salmuera Turbia: " + t.Name, Description = t.Description, Ingredients = "Una mezcla sin receta",
                    Target = PotionTarget.Ally, UsableInCombat = true,
                };
                def.Effects.AddRange(t.Effects);
                return def;
            }
            if (!meta.Combat.Potions.TryGetValue(baseId, out var basic)) return null;
            var potency = rules.Potency.Find(p => p.Name == suffix);
            if (potency == null) return null;
            var v = new PotionDef
            {
                Id = id, Name = $"{basic.Name} ({potency.Name})", Ingredients = basic.Ingredients,
                Description = $"{basic.Description} Potencia ×{potency.Scale:0.#}.", Target = basic.Target, UsableInCombat = basic.UsableInCombat,
            };
            foreach (var fx in basic.Effects) v.Effects.Add(Battle.ScaleEffect(fx, potency.Scale));
            return v;
        }

        public static List<PotionDef> CustomPotionsFor(MetaDatabase meta, IEnumerable<string> ids)
        {
            var list = new List<PotionDef>();
            var seen = new HashSet<string>();
            foreach (var id in ids)
            {
                if (!seen.Add(id)) continue;
                var def = VariantDef(meta, id);
                if (def != null) list.Add(def);
            }
            return list;
        }

        public static string NameOf(MetaDatabase meta, string id)
        {
            var v = VariantDef(meta, id);
            if (v != null) return v.Name;
            return meta.Combat.Potions.TryGetValue(id ?? "", out var p) ? p.Name : id;
        }
    }
}
