using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Crafting;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Progress;

namespace SalYAlmas.Core.Meta
{
    /// <summary>
    /// Carga y valida los JSON de la Fase 2 contra los de la Fase 1. Junta todos los errores en una
    /// DataValidationException, igual que el cargador del combate.
    /// </summary>
    public static class MetaLoader
    {
        public static MetaDatabase Load(MetaDataSources src, GameDatabase combat)
        {
            var r = new Reader();
            var db = new MetaDatabase(combat);
            r.Guard("economy.json", () => LoadEconomy(Json.Parse(src.Economy, "economy.json"), db, r));
            r.Guard("roster.json", () => LoadRoster(Json.Parse(src.Roster, "roster.json"), db, r));
            r.Guard("fresno_enemies.json", () => DataLoader.ReadEnemies(Json.Parse(src.FresnoEnemies, "fresno_enemies.json"), r,
                id => db.FresnoEnemies.ContainsKey(id) || combat.Enemies.ContainsKey(id), e => db.FresnoEnemies.Add(e.Id, e)));
            r.Guard("progression.json", () => LoadProgression(Json.Parse(src.Progression, "progression.json"), db, r));
            r.Guard("weapons.json", () => LoadWeapons(Json.Parse(src.Weapons, "weapons.json"), db, r));
            r.Guard("brewing.json", () => LoadBrewing(Json.Parse(src.Brewing, "brewing.json"), db, r));
            r.Guard("gacha.json", () => LoadGacha(Json.Parse(src.Gacha, "gacha.json"), db, r));
            r.Guard("fresno.json", () => LoadFresno(Json.Parse(src.Fresno, "fresno.json"), db, r));
            if (r.Errors.Count > 0) throw new DataValidationException(r.Errors);
            return db;
        }

        public static MetaDatabase LoadFromDirectory(string directory, GameDatabase combat) =>
            Load(MetaDataSources.FromDirectory(directory), combat);

        // ------------------------------------------------------------------ Utilidades

        private static StatBlock ReadMods(Reader r, JsonNode parent, string key = "mods")
        {
            var block = new StatBlock();
            var n = parent?.Get(key);
            if (n == null) return block;
            if (n.Kind != JsonKind.Object) { r.Error(n, $"'{key}' debe ser un objeto"); return block; }
            foreach (var kv in n.Properties)
            {
                if (!ModKeys.IsKnown(kv.Key)) { r.Error(kv.Value, "modificador desconocido: " + kv.Key); continue; }
                if (kv.Value.Kind != JsonKind.Number) { r.Error(kv.Value, "el modificador debe ser un número"); continue; }
                block.Add(kv.Key, kv.Value.AsNumber);
            }
            return block;
        }

        private static Planet ParsePlanet(Reader r, JsonNode n)
        {
            if (n == null || n.Kind != JsonKind.String || !Planets.TryParse(n.AsString, out var p)) { r.Error(n, "planeta inválido"); return Planet.Sol; }
            return p;
        }

        private static Bundle ReadCost(Reader r, JsonNode parent, string key, MetaDatabase db)
        {
            var b = new Bundle();
            var n = parent?.Get(key);
            if (n == null) return b;
            foreach (var kv in n.Properties)
            {
                if (!db.Catalog.Has(kv.Key)) r.Error(kv.Value, "recurso desconocido: " + kv.Key);
                b.Add(kv.Key, (long)Math.Round(kv.Value.AsNumber));
            }
            return b;
        }

        private static List<ChainEcho> ReadEchoes(Reader r, JsonNode parent)
        {
            var list = new List<ChainEcho>();
            foreach (var e in r.Arr(parent, "echoes", required: false))
            {
                if (e.Kind != JsonKind.Array || e.Items.Count != 2) { r.Error(e, "un eco es [desfase, parte]"); continue; }
                list.Add(new ChainEcho((int)Math.Round(e.Items[0].AsNumber), e.Items[1].AsNumber));
            }
            return list;
        }

        // ------------------------------------------------------------------ Economía

        private static void LoadEconomy(JsonNode root, MetaDatabase db, Reader r)
        {
            var flows = r.Obj(root, "flows");
            if (flows != null) foreach (var kv in flows.Properties) db.Catalog.AddFlow(kv.Key, kv.Value.AsString);

            foreach (var n in r.Arr(root, "resources"))
            {
                string family = r.Str(n, "id");
                string name = r.Str(n, "name");
                bool sold = r.Bool(n, "sold", false);
                db.Catalog.Families.Add(family);
                db.Catalog.FamilyNames[family] = name;
                var sources = new List<string>();
                var sinks = new List<string>();
                foreach (var s in r.Arr(n, "sources"))
                {
                    if (!db.Catalog.Flows.ContainsKey(s.AsString)) r.Error(s, "flujo no declarado: " + s.AsString);
                    sources.Add(s.AsString);
                }
                foreach (var s in r.Arr(n, "sinks"))
                {
                    if (!db.Catalog.Flows.ContainsKey(s.AsString)) r.Error(s, "flujo no declarado: " + s.AsString);
                    sinks.Add(s.AsString);
                }
                if (sources.Count == 0 || sinks.Count == 0) r.Error(n, family + ": cada recurso tiene fuente y sumidero (GDD 11)");

                void Add(string id, string display, Planet? planet, string region)
                {
                    var def = new ResourceDef { Id = id, Name = display, Sold = sold, Family = family, Planet = planet, Region = region ?? "" };
                    foreach (var s in sources) def.Sources.Add(s);
                    foreach (var s in sinks) def.Sinks.Add(s);
                    db.Catalog.Add(def);
                }

                if (r.Bool(n, "perPlanet", false))
                {
                    foreach (var p in Planets.All) Add(family + "." + Planets.Id(p), name + " (" + Planets.DisplayName(p) + ")", p, null);
                }
                else if (n.Has("materials"))
                {
                    foreach (var m in r.Arr(n, "materials"))
                        Add(family + "." + r.Str(m, "id"), r.Str(m, "name"), null, r.Str(m, "region", required: false));
                }
                else Add(family, name, null, null);
            }
        }

        // ------------------------------------------------------------------ Elenco

        private static void LoadRoster(JsonNode root, MetaDatabase db, Reader r)
        {
            foreach (var n in r.Arr(root, "witches"))
            {
                var e = new RosterEntry
                {
                    Id = r.Str(n, "id"),
                    Name = r.Str(n, "name"),
                    Rarity = r.Int(n, "rarity", 3, 5),
                    Species = r.Str(n, "species", required: false),
                    Primary = r.PlanetOf(n, "primary"),
                    Secondary = r.PlanetOf(n, "secondary"),
                    Role = r.Str(n, "role", required: false),
                    Limited = r.Bool(n, "limited", false),
                    HasKit = r.Bool(n, "kit", false),
                };
                if (db.Roster.ContainsKey(e.Id)) { r.Error(n, "bruja repetida: " + e.Id); continue; }
                if (e.HasKit && !db.Combat.Witches.ContainsKey(e.Id)) r.Error(n, e.Id + ": dice tener kit pero no está en witches.json");
                if (e.HasKit && db.Combat.Witches.TryGetValue(e.Id, out var kit) && kit.Rarity != e.Rarity) r.Error(n, e.Id + ": la rareza no coincide con witches.json");
                db.Roster.Add(e.Id, e);
                db.RosterOrder.Add(e.Id);
            }
            foreach (var id in db.Combat.WitchOrder)
                if (!db.Roster.TryGetValue(id, out var e) || !e.HasKit) r.Error(root, id + ": toda bruja con kit debe estar en el elenco con \"kit\": true");
        }

        // ------------------------------------------------------------------ Progreso

        private static void LoadProgression(JsonNode root, MetaDatabase db, Reader r)
        {
            var p = db.Progression;
            var lv = r.Obj(root, "level");
            p.Level.Max = r.Int(lv, "max", 1, 200);
            p.Level.CostBase = r.Num(lv, "costBase", 0, 1e9);
            p.Level.CostExponent = r.Num(lv, "costExponent", 0, 5);
            p.Level.StatGrowth = r.Num(lv, "statGrowth", 0, 1);

            var gr = r.Obj(root, "grades");
            p.GradeCostBase = r.Num(gr, "costBase", 0, 1e9);
            p.GradeCostFactor = r.Num(gr, "costFactor", 1, 10);
            int k = 0;
            foreach (var n in r.Arr(gr, "steps"))
            {
                string gs = r.Str(n, "grade");
                if (!PurityMath.TryParseGrade(gs, out var g) || g == Grade.D) { r.Error(n, "grado inválido: " + gs); continue; }
                var step = new GradeStep
                {
                    Grade = g,
                    Index = k,
                    Almitas = p.GradeCost(k),
                    MinLevel = r.Int(n, "minLevel", 1, 200),
                    TrialLevel = r.Int(n, "trialLevel", 1, 200),
                    SaltTier = r.Int(n, "saltTier", 0, 2),
                    SaltAmount = r.Int(n, "saltAmount", 0, 1000),
                    Unlock = r.Str(n, "unlock", required: false),
                    Mods = ReadMods(r, n),
                };
                if ((int)g != k + 1) r.Error(n, "los grados van en orden C, B, A, S, SS");
                p.Grades.Add(step);
                k++;
            }
            if (p.Grades.Count != 5) r.Error(gr, "se esperan los grados C, B, A, S y SS");

            var via = r.Obj(root, "via");
            p.ViaFreeChanges = r.Int(via, "freeChanges", 0, 10);
            p.ViaChangeCost = (long)r.Num(via, "changeCost", 0, 1e9);

            var dg = r.Obj(root, "dignity");
            foreach (var x in r.Arr(dg, "names")) p.Dignity.Names.Add(x.AsString);
            foreach (var x in r.Arr(dg, "costs")) p.Dignity.Costs.Add((long)x.AsNumber);
            foreach (var x in r.Arr(dg, "ascendantDamage")) p.Dignity.AscendantDamage.Add(x.AsNumber);
            if (p.Dignity.Names.Count != 5) r.Error(dg, "la Dignidad tiene cinco niveles (GDD 8.1)");
            if (p.Dignity.Costs.Count != p.Dignity.Names.Count || p.Dignity.AscendantDamage.Count != p.Dignity.Names.Count)
                r.Error(dg, "costos y bonos deben tener un valor por nivel de Dignidad");

            var ct = r.Obj(root, "chainTalents");
            p.ChainTalents.Max = r.Int(ct, "max", 1, 50);
            p.ChainTalents.BonusPerLevel = r.Num(ct, "bonusPerLevel", 0, 1);
            p.ChainTalents.AlmitasBase = r.Num(ct, "almitasBase", 0, 1e9);
            p.ChainTalents.AlmitasExponent = r.Num(ct, "almitasExponent", 0, 5);
            p.ChainTalents.MaterialPerLevel = r.Int(ct, "materialPerLevel", 0, 100);
            var pm = r.Obj(ct, "planetMaterials");
            if (pm != null)
                foreach (var kv in pm.Properties)
                {
                    if (!Planets.TryParse(kv.Key, out var planet)) { r.Error(kv.Value, "planeta inválido"); continue; }
                    if (!db.Catalog.Has(Res.Mat(kv.Value.AsString))) r.Error(kv.Value, "material desconocido: " + kv.Value.AsString);
                    p.ChainTalents.PlanetMaterials[planet] = kv.Value.AsString;
                }
            if (p.ChainTalents.PlanetMaterials.Count != 7) r.Error(ct, "falta el material de algún planeta");

            var tr = r.Obj(root, "trial");
            p.Trial.HpAtReference = r.Num(tr, "hpAtReference", 1, 1e8);
            p.Trial.AtkFactor = r.Num(tr, "atkFactor", 0, 10);
            p.Trial.Toughness = r.Num(tr, "toughness", 1, 10000);
            p.Trial.ActionsPerTurn = r.Int(tr, "actionsPerTurn", 1, 3);
            p.Trial.OwnPlanetRes = r.Num(tr, "ownPlanetRes", -1, 1);
            p.Trial.MaxAllies = r.Int(tr, "maxAllies", 0, 3);

            var conds = r.Obj(root, "conditions");
            if (conds != null)
                foreach (var kv in conds.Properties)
                {
                    var c = new TrialConditionDef
                    {
                        Id = kv.Key,
                        Text = r.Str(kv.Value, "text"),
                        Value = r.Int(kv.Value, "value", 0, 1000, required: false),
                    };
                    string type = r.Str(kv.Value, "type");
                    if (!Enum.TryParse(type, true, out c.Type)) r.Error(kv.Value, "tipo de condición desconocido: " + type);
                    p.Conditions[kv.Key] = c;
                }

            var tc = r.Obj(root, "trialConditions");
            if (tc != null)
                foreach (var kv in tc.Properties)
                {
                    if (kv.Key != "default" && !db.Roster.ContainsKey(kv.Key)) r.Error(kv.Value, "bruja desconocida: " + kv.Key);
                    var byGrade = new Dictionary<Grade, List<string>>();
                    foreach (var g in kv.Value.Properties)
                    {
                        if (!PurityMath.TryParseGrade(g.Key, out var grade)) { r.Error(g.Value, "grado inválido: " + g.Key); continue; }
                        var ids = new List<string>();
                        foreach (var id in g.Value.Items)
                        {
                            if (!p.Conditions.ContainsKey(id.AsString)) r.Error(id, "condición desconocida: " + id.AsString);
                            ids.Add(id.AsString);
                        }
                        byGrade[grade] = ids;
                    }
                    p.TrialConditions[kv.Key] = byGrade;
                }
            if (p.TrialConditions.TryGetValue("default", out var def))
            {
                // GDD 12.3: Juicio I sin condición; II, III y IV con una; V con dos.
                int[] expected = { 0, 1, 1, 1, 2 };
                for (int i = 0; i < 5; i++)
                {
                    var g = (Grade)(i + 1);
                    int count = def.TryGetValue(g, out var l) ? l.Count : 0;
                    if (count != expected[i]) r.Error(tc, $"el Juicio de {g} lleva {expected[i]} condiciones (GDD 12.3)");
                }
            }
            else r.Error(tc, "faltan las condiciones por defecto");

            var mem = r.Obj(root, "memories", required: false);
            if (mem != null)
                foreach (var kv in mem.Properties)
                {
                    var byGrade = new Dictionary<Grade, string>();
                    foreach (var g in kv.Value.Properties)
                        if (PurityMath.TryParseGrade(g.Key, out var grade)) byGrade[grade] = g.Value.AsString;
                    p.Memories[kv.Key] = byGrade;
                }
        }

        // ------------------------------------------------------------------ Armas

        private static void LoadWeapons(JsonNode root, MetaDatabase db, Reader r)
        {
            foreach (var n in r.Arr(root, "stations"))
                db.Stations.Add(new StationDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), Base = r.Bool(n, "base", false),
                    Region = r.Str(n, "region", required: false), Description = r.Str(n, "description", required: false),
                });

            foreach (var n in r.Arr(root, "tiers"))
            {
                var t = new WeaponTierDef
                {
                    Tier = r.Int(n, "tier", 1, 5), Name = r.Str(n, "name"), Atk = r.Num(n, "atk", 0, 10000),
                    Regional = r.Bool(n, "regional", false), Almitas = (long)r.Num(n, "almitas", 0, 1e9),
                    Material = r.Int(n, "material", 0, 1000),
                };
                var salt = r.Arr(n, "salt");
                if (salt.Count == 2) { t.SaltTier = (int)salt[0].AsNumber; t.SaltAmount = (int)salt[1].AsNumber; }
                else r.Error(n, "salt es [pureza, cantidad]");
                db.Tiers.Add(t);
            }
            if (db.Tiers.Count != 5) r.Error(root, "se esperan cinco niveles (GDD 15.2)");
            db.AffinityDamage = r.Num(root, "affinityDamage", 0, 1, required: false, def: 0.08);

            foreach (var n in r.Arr(root, "families"))
            {
                var f = new WeaponFamilyDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), Pattern = r.Str(n, "pattern", required: false),
                    Strength = r.Str(n, "strength", required: false), Station = r.Str(n, "station"), Material = r.Str(n, "material"),
                    FrameShift = r.Int(n, "frameShift", -30, 30, required: false), Mods = ReadMods(r, n),
                };
                f.Hits = DataLoader.ReadHits(r, n, required: true);
                f.Echoes = ReadEchoes(r, n);
                var planets = r.Arr(n, "planets");
                if (planets.Count == 2) { f.Planets[0] = ParsePlanet(r, planets[0]); f.Planets[1] = ParsePlanet(r, planets[1]); }
                else r.Error(n, "cada familia tiene dos planetas, uno por variante");
                if (db.Station(f.Station) == null) r.Error(n, "estación desconocida: " + f.Station);
                if (!db.Catalog.Has(Res.Mat(f.Material))) r.Error(n, "material desconocido: " + f.Material);
                db.Families.Add(f);

                var names = r.Arr(n, "names");
                if (names.Count != 5) { r.Error(n, f.Id + ": cinco niveles de nombres"); continue; }
                for (int t = 1; t <= 5; t++)
                {
                    var pair = names[t - 1];
                    if (pair.Items.Count != 2) { r.Error(pair, f.Id + ": dos variantes por nivel"); continue; }
                    var tier = db.Tier(t);
                    for (int v = 0; v < 2; v++)
                    {
                        var w = new WeaponDef
                        {
                            Id = $"{f.Id}_{t}_{(v == 0 ? 'a' : 'b')}", Name = pair.Items[v].AsString, Family = f.Id, Tier = t, Variant = v,
                            Planet = f.Planets[v], Atk = tier?.Atk ?? 0, Mods = f.Mods.Clone(),
                        };
                        AddWeapon(db, r, n, w);
                        AddForgeRecipe(db, f, tier, w);
                    }
                }
            }
            if (db.Families.Count != 12) r.Error(root, "se esperan doce familias (GDD 15.1)");

            foreach (var n in r.Arr(root, "specials"))
            {
                var w = new WeaponDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), Family = r.Str(n, "family"), Tier = r.Int(n, "tier", 1, 6),
                    Planet = r.PlanetOf(n, "planet"), Atk = r.Num(n, "atk", 0, 10000), Legendary = r.Bool(n, "legendary", false),
                    Pending = r.Bool(n, "pending", false), Description = r.Str(n, "description", required: false), Mods = ReadMods(r, n),
                };
                if (n.Has("hits")) w.Hits = DataLoader.ReadHits(r, n, required: true);
                w.Echoes = ReadEchoes(r, n);
                var fam = db.Family(w.Family);
                if (fam == null) r.Error(n, "familia desconocida: " + w.Family);
                else
                {
                    var merged = fam.Mods.Clone();
                    merged.AddAll(w.Mods);
                    w.Mods = merged;
                }
                AddWeapon(db, r, n, w);
            }

            foreach (var n in r.Arr(root, "recipes"))
            {
                var rec = new RecipeDef
                {
                    Id = r.Str(n, "id"), Output = r.Str(n, "output"), Description = r.Str(n, "description", required: false),
                    Kind = r.Str(n, "kind") == "fusion" ? RecipeKind.Fusion : RecipeKind.Forge,
                };
                foreach (var s in r.Arr(n, "stations"))
                {
                    if (db.Station(s.AsString) == null) r.Error(s, "estación desconocida: " + s.AsString);
                    rec.Stations.Add(s.AsString);
                }
                if (!db.Weapons.ContainsKey(rec.Output)) r.Error(n, "la receta produce un arma desconocida: " + rec.Output);
                foreach (var i in r.Arr(n, "inputs"))
                {
                    var input = new RecipeInput();
                    if (i.Has("weapon"))
                    {
                        input.WeaponId = r.Str(i, "weapon");
                        if (!db.Weapons.ContainsKey(input.WeaponId)) r.Error(i, "arma desconocida: " + input.WeaponId);
                    }
                    else if (i.Has("tier"))
                    {
                        input.Tier = r.Int(i, "tier", 1, 6);
                        if (i.Has("planet")) input.Planet = r.PlanetOf(i, "planet");
                    }
                    else
                    {
                        input.Resource = r.Str(i, "res");
                        input.Amount = (long)r.Num(i, "amount", 1, 1e9);
                        if (!db.Catalog.Has(input.Resource)) r.Error(i, "recurso desconocido: " + input.Resource);
                    }
                    rec.Inputs.Add(input);
                }
                if (db.Recipes.ContainsKey(rec.Id)) { r.Error(n, "receta repetida: " + rec.Id); continue; }
                db.Recipes.Add(rec.Id, rec);
                db.RecipeOrder.Add(rec.Id);
            }

            var refine = r.Obj(root, "refining");
            var dest = r.Obj(refine, "destilada");
            var quin = r.Obj(refine, "quintaesencia");
            db.RefineFrom[1] = r.Int(dest, "from", 1, 1000);
            db.RefineAlmitas[1] = (long)r.Num(dest, "almitas", 0, 1e9);
            db.RefineFrom[2] = r.Int(quin, "from", 1, 1000);
            db.RefineAlmitas[2] = (long)r.Num(quin, "almitas", 0, 1e9);

            var rf = r.Obj(root, "reforge");
            db.ReforgeAlmitasPerTier = (long)r.Num(rf, "almitasPerTier", 0, 1e9);
            db.ReforgeSaltPerTier = r.Int(rf, "saltPerTier", 0, 1000);

            foreach (var n in r.Arr(root, "prefixes"))
            {
                var px = new PrefixDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Weight = r.Int(n, "weight", 1, 1000), Mods = ReadMods(r, n) };
                if (db.Prefixes.Exists(x => x.Id == px.Id)) r.Error(n, "prefijo repetido: " + px.Id);
                db.Prefixes.Add(px);
            }
            foreach (var required in new[] { "turbio", "salino", "astral", "desquiciado", "ritmico", "pausado", "legendario" })
                if (db.Prefix(required) == null) r.Error(root, "falta el prefijo del GDD 15.4: " + required);
        }

        private static void AddWeapon(MetaDatabase db, Reader r, JsonNode at, WeaponDef w)
        {
            if (db.Weapons.ContainsKey(w.Id)) { r.Error(at, "arma repetida: " + w.Id); return; }
            db.Weapons.Add(w.Id, w);
            db.WeaponOrder.Add(w.Id);
        }

        /// <summary>Receta de cada una de las 120 armas: el arma del nivel anterior, almitas, sal y material regional.</summary>
        private static void AddForgeRecipe(MetaDatabase db, WeaponFamilyDef f, WeaponTierDef tier, WeaponDef w)
        {
            if (tier == null) return;
            var rec = new RecipeDef { Id = "forja_" + w.Id, Output = w.Id, Kind = RecipeKind.Forge, Generated = true };
            rec.Stations.Add("yunque");
            if (tier.Regional) rec.Stations.Add(f.Station);
            if (w.Tier > 1) rec.Inputs.Add(new RecipeInput { WeaponId = $"{f.Id}_{w.Tier - 1}_{(w.Variant == 0 ? 'a' : 'b')}" });
            rec.Inputs.Add(new RecipeInput { Resource = Res.Almitas, Amount = tier.Almitas });
            if (tier.SaltAmount > 0) rec.Inputs.Add(new RecipeInput { Resource = Res.Salt(w.Planet, tier.SaltTier), Amount = tier.SaltAmount });
            if (tier.Material > 0) rec.Inputs.Add(new RecipeInput { Resource = Res.Mat(f.Material), Amount = tier.Material });
            rec.Description = $"{tier.Name}: Yunque Astral" + (tier.Regional ? " junto a " + (db.Station(f.Station)?.Name ?? f.Station) : "");
            db.Recipes.Add(rec.Id, rec);
            db.RecipeOrder.Add(rec.Id);
        }

        // ------------------------------------------------------------------ Salmueras

        private static void LoadBrewing(JsonNode root, MetaDatabase db, Reader r)
        {
            var b = db.Brewing;
            foreach (var n in r.Arr(root, "bases"))
                b.Bases.Add(new BrewBaseDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Cost = ReadCost(r, n, "cost", db) });
            foreach (var n in r.Arr(root, "reagents"))
            {
                var re = new ReagentDef { Id = r.Str(n, "id"), Name = r.Str(n, "name") };
                if (!db.Catalog.Has(re.Resource)) r.Error(n, "el reactivo no es un material del libro contable: " + re.Id);
                b.Reagents.Add(re);
            }
            b.SaltAmount = r.Int(root, "saltAmount", 1, 100);
            b.AlmitasPerBrew = (long)r.Num(root, "almitasPerBrew", 0, 1e9);
            b.AlmitasConcentradas = (long)r.Num(root, "almitasConcentradas", 0, 1e9);
            foreach (var n in r.Arr(root, "potency"))
                b.Potency.Add(new PotencyDef { Tier = r.Int(n, "tier", 0, 2), Name = r.Str(n, "name"), Scale = r.Num(n, "scale", 0, 10) });
            if (b.Potency.Count != 3) r.Error(root, "tres niveles de potencia: básica, mejorada y maestra (GDD 14.1)");

            var seen = new HashSet<string>();
            foreach (var n in r.Arr(root, "recipes"))
            {
                var rec = new BrewRecipeDef { Potion = r.Str(n, "potion"), Base = r.Str(n, "base"), Reagent = r.Str(n, "reagent") };
                string salt = r.Str(n, "salt");
                switch (salt)
                {
                    case "sal_negra": rec.SaltKind = BrewSaltKind.BlackSalt; break;
                    case "almitas": rec.SaltKind = BrewSaltKind.Almitas; break;
                    case "refinada": rec.SaltKind = BrewSaltKind.Refined; break;
                    case "ninguna": rec.SaltKind = BrewSaltKind.None; break;
                    default:
                        rec.SaltKind = BrewSaltKind.Planet;
                        if (!Planets.TryParse(salt, out rec.Planet)) r.Error(n, "sal desconocida: " + salt);
                        break;
                }
                if (!db.Combat.Potions.ContainsKey(rec.Potion)) r.Error(n, "Salmuera desconocida: " + rec.Potion);
                if (b.Base(rec.Base) == null) r.Error(n, "base desconocida: " + rec.Base);
                if (b.Reagent(rec.Reagent) == null) r.Error(n, "reactivo desconocido: " + rec.Reagent);
                string key = rec.Base + "|" + salt + "|" + rec.Reagent;
                if (!seen.Add(key)) r.Error(n, "dos recetas con los mismos ingredientes: " + key);
                b.Recipes.Add(rec);
            }
            foreach (var id in db.Combat.PotionOrder)
                if (!b.Recipes.Exists(x => x.Potion == id)) r.Error(root, "la Salmuera " + id + " no tiene receta");

            foreach (var n in r.Arr(root, "turbid"))
            {
                var t = new TurbidDef { Name = r.Str(n, "name"), Description = r.Str(n, "description", required: false) };
                t.Effects = DataLoader.ReadEffects(r, n);
                b.Turbid.Add(t);
            }
            if (b.Turbid.Count == 0) r.Error(root, "falta al menos un efecto de Salmuera Turbia");
        }

        // ------------------------------------------------------------------ Gacha

        private static void LoadGacha(JsonNode root, MetaDatabase db, Reader r)
        {
            var g = db.Gacha;
            g.CostPerPull = r.Int(root, "costPerPull", 1, 100000);
            g.Base5 = r.Num(root, "base5", 0, 1);
            g.SoftPityStart = r.Int(root, "softPityStart", 1, 1000);
            g.SoftPityStep = r.Num(root, "softPityStep", 0, 1);
            g.HardPity = r.Int(root, "hardPity", 1, 1000);
            g.Base4 = r.Num(root, "base4", 0, 1);
            g.Guarantee4Every = r.Int(root, "guarantee4Every", 1, 1000);
            g.FeaturedRate = r.Num(root, "featuredRate", 0, 1);
            g.SelectorAt = r.Int(root, "selectorAt", 1, 100000);
            g.HistoryMax = r.Int(root, "historyMax", 1, 1000000);
            var dust = r.Obj(root, "dust");
            g.Dust5 = r.Int(dust, "5", 0, 100000);
            g.Dust4 = r.Int(dust, "4", 0, 100000);
            g.ThreeStarSalt = r.Int(root, "threeStarSalt", 0, 1000);
            foreach (var n in r.Arr(root, "banners"))
            {
                var b = new BannerDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Description = r.Str(n, "description", required: false) };
                string type = r.Str(n, "type");
                if (!Enum.TryParse(type, true, out b.Type)) r.Error(n, "tipo de banner desconocido: " + type);
                b.Featured = r.Str(n, "featured", required: false, def: null);
                b.MaxPulls = r.Int(n, "maxPulls", 0, 100000, required: false);
                foreach (var c in r.Arr(n, "choices", required: false)) b.Choices.Add(c.AsString);
                if (b.Type == BannerType.Limitado && (b.Featured == null || !db.Roster.ContainsKey(b.Featured) || db.Roster[b.Featured].Rarity != 5))
                    r.Error(n, "un banner limitado destaca a una Regente del elenco");
                if (b.Type == BannerType.Inicio && (b.Choices.Count != 4 || b.MaxPulls <= 0))
                    r.Error(n, "el banner de inicio tiene 20 tiradas y cuatro Regentes a elegir (GDD 16.2)");
                foreach (var c in b.Choices)
                    if (!db.Roster.TryGetValue(c, out var e) || e.Rarity != 5 || e.Limited) r.Error(n, "opción inválida (Regente estándar): " + c);
                g.Banners.Add(b);
            }
        }

        // ------------------------------------------------------------------ Fresno

        private static void LoadFresno(JsonNode root, MetaDatabase db, Reader r)
        {
            var f = db.Fresno;
            foreach (var n in r.Arr(root, "zones"))
            {
                var z = new ZoneDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), Theme = r.Str(n, "theme", required: false), Boss = r.Str(n, "boss"),
                    HpScale = r.Num(n, "hpScale", 0.01, 100), AtkScale = r.Num(n, "atkScale", 0.01, 100),
                    Almitas = (long)r.Num(n, "almitas", 0, 1e9), Savia = (long)r.Num(n, "savia", 0, 1e9),
                };
                foreach (var e in r.Arr(n, "enemies"))
                {
                    if (db.Enemy(e.AsString) == null) r.Error(e, "enemigo desconocido: " + e.AsString);
                    z.Enemies.Add(e.AsString);
                }
                var boss = db.Enemy(z.Boss);
                if (boss == null || !boss.IsBoss) r.Error(n, "el jefe de zona debe ser un enemigo jefe: " + z.Boss);
                f.Zones.Add(z);
            }
            if (f.Zones.Count != 4) r.Error(root, "el Fresno tiene cuatro zonas (GDD 10.1)");

            var map = r.Obj(root, "map");
            f.Map.Floors = r.Int(map, "floors", 2, 50);
            f.Map.Lanes = r.Int(map, "lanes", 1, 6);
            f.Map.EliteFromFloor = r.Int(map, "eliteFromFloor", 1, 50);
            foreach (var x in r.Arr(map, "sanctuaryFloors")) f.Map.SanctuaryFloors.Add((int)x.AsNumber);
            var weights = r.Obj(map, "weights");
            if (weights != null)
                foreach (var kv in weights.Properties)
                {
                    if (!Enum.TryParse(kv.Key, true, out NodeType t) || t == NodeType.Jefe || t == NodeType.Mancha) { r.Error(kv.Value, "tipo de nodo inválido: " + kv.Key); continue; }
                    f.Map.Weights[t] = (int)kv.Value.AsNumber;
                }
            if (f.Map.Floors != 10) r.Error(map, "cada zona tiene diez nodos (GDD 10)");

            var enc = r.Obj(root, "encounters");
            var byFloor = r.Arr(enc, "enemiesByFloor");
            f.Encounters.EnemiesByFloor = new int[byFloor.Count];
            for (int i = 0; i < byFloor.Count; i++) f.Encounters.EnemiesByFloor[i] = (int)byFloor[i].AsNumber;
            if (byFloor.Count != f.Map.Floors) r.Error(enc, "enemiesByFloor necesita un valor por piso");
            f.Encounters.HpPerFloor = r.Num(enc, "hpPerFloor", 0, 1);
            f.Encounters.EliteHp = r.Num(enc, "eliteHp", 1, 10);
            f.Encounters.EliteAtk = r.Num(enc, "eliteAtk", 1, 10);
            f.Encounters.LevelPerFloor = r.Num(enc, "levelPerFloor", 0, 5);

            var rw = r.Obj(root, "rewards");
            f.Rewards.CombatFloorBonus = r.Num(rw, "combatFloorBonus", 0, 1);
            f.Rewards.EliteMultiplier = r.Num(rw, "eliteMultiplier", 0, 100);
            f.Rewards.BossMultiplier = r.Num(rw, "bossMultiplier", 0, 100);
            f.Rewards.NidoMultiplier = r.Num(rw, "nidoMultiplier", 0, 100);
            f.Rewards.EliteBlackSalt = r.Int(rw, "eliteBlackSalt", 0, 100);
            f.Rewards.NidoSalt = r.Int(rw, "nidoSalt", 0, 100);
            f.Rewards.FullClearSavia = (long)r.Num(rw, "fullClearSavia", 0, 1e9);

            var souls = r.Obj(root, "souls");
            f.ManchaReturn = r.Num(souls, "manchaReturn", 0, 1);
            f.ReviveHpAfterWin = r.Num(souls, "reviveHpAfterWin", 0.01, 1);

            var wk = r.Obj(root, "weekly");
            f.Weekly.CapAlmitas = (long)r.Num(wk, "capAlmitas", 0, 1e12);
            f.Weekly.OverflowSaviaRate = r.Num(wk, "overflowSaviaRate", 0, 1);
            f.Weekly.BathSaltsPerBoss = (long)r.Num(wk, "bathSaltsPerBoss", 0, 1e9);
            f.Weekly.BathSaltsMax = (long)r.Num(wk, "bathSaltsMax", 0, 1e9);
            if (f.Weekly.OverflowSaviaRate >= f.ManchaReturn) r.Error(wk, "la tasa del excedente debe ser menor que la de la Mancha (GDD 10.4: \"a una tasa menor\")");

            var sa = r.Obj(root, "santuario");
            f.RestSalinity = r.Int(sa, "restSalinity", -150, 0);
            f.RestHeal = r.Num(sa, "restHeal", 0, 1);
            f.TrainBonus = r.Num(sa, "trainBonus", 0, 5);

            var me = r.Obj(root, "merchant");
            f.MerchantGraftPrice = (long)r.Num(me, "graftPrice", 0, 1e9);
            f.MerchantPotionPrice = (long)r.Num(me, "potionPrice", 0, 1e9);
            f.MerchantHealPrice = (long)r.Num(me, "healPrice", 0, 1e9);
            f.MerchantHealAmount = r.Num(me, "healAmount", 0, 1);

            f.GraftChoices = r.Int(root, "graftChoices", 1, 10);
            f.FruitAt = r.Int(root, "fruitAt", 1, 15);

            var cu = r.Obj(root, "curses");
            f.MaxCursesPerZone = r.Int(cu, "maxPerZone", 0, 20);
            f.CurseAlmitasMultiplier = r.Num(cu, "almitasMultiplier", 1, 10);
            foreach (var n in r.Arr(cu, "list"))
                f.Curses.Add(new CurseDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Mods = ReadMods(r, n) });
            if (f.Curses.Count < f.MaxCursesPerZone) r.Error(cu, "hacen falta al menos tantas maldiciones como el máximo por zona");

            var pa = r.Obj(root, "paria");
            f.PariaThreeStarChoices = r.Int(pa, "threeStarChoices", 0, 10);
            f.PariaThreeStarAlmitas = r.Num(pa, "threeStarAlmitas", 0, 10);
            f.PariaFourStarAlmitas = r.Num(pa, "fourStarAlmitas", 0, 10);

            var eco = r.Obj(root, "eco");
            f.EcoSlots = r.Int(eco, "slots", 1, 10);
            f.EcoGrafts = r.Int(eco, "grafts", 0, 20);
            f.EcoTrainedMoves = r.Int(eco, "trainedMoves", 0, 10);
            foreach (var n in r.Arr(eco, "unlocks"))
            {
                var rule = new EcoUnlockRule { Slot = r.Int(n, "slot", 1, 10), Rule = r.Str(n, "rule") };
                if (rule.Rule != "inicio" && !(rule.Rule.StartsWith("jefe:", StringComparison.Ordinal) && f.ZoneIndex(rule.Rule.Substring(5)) >= 0))
                    r.Error(n, "regla de ranura de Eco desconocida: " + rule.Rule);
                f.EcoUnlocks.Add(rule);
            }

            var seenGrafts = new HashSet<string>();
            foreach (var n in r.Arr(root, "ramas"))
            {
                var rama = new RamaDef { Planet = r.PlanetOf(n, "planet"), Name = r.Str(n, "name"), UnlockOfrenda = r.Int(n, "unlockOfrenda", 0, 1000) };
                var fruit = r.Obj(n, "fruit");
                rama.Fruit = new FruitDef { Name = r.Str(fruit, "name"), Description = r.Str(fruit, "description", required: false), Mods = ReadMods(r, fruit) };
                foreach (var g in r.Arr(n, "grafts"))
                {
                    var graft = new GraftDef { Id = r.Str(g, "id"), Name = r.Str(g, "name"), Rama = rama.Planet, Pending = r.Bool(g, "pending", false), Mods = ReadMods(r, g) };
                    if (!seenGrafts.Add(graft.Id)) r.Error(g, "Injerto repetido: " + graft.Id);
                    if (!graft.Pending && graft.Mods.Count == 0) r.Error(g, graft.Id + ": un Injerto activo necesita efecto");
                    if (graft.Pending && graft.Mods.Count > 0) r.Error(g, graft.Id + ": un Injerto pendiente no tiene efecto todavía");
                    rama.Grafts.Add(graft);
                }
                if (rama.Grafts.Count != 15) r.Error(n, rama.Name + ": cada Rama tiene 15 Injertos (GDD 10.3)");
                if (f.Rama(rama.Planet) != null) r.Error(n, "Rama repetida: " + rama.Name);
                f.Ramas.Add(rama);
            }
            if (f.Ramas.Count != 7) r.Error(root, "siete Ramas, una por planeta (GDD 10.3)");

            foreach (var n in r.Arr(root, "mutations"))
                f.Mutations.Add(new MutationDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Text = r.Str(n, "description", required: false), Mods = ReadMods(r, n) });

            foreach (var n in r.Arr(root, "roots"))
            {
                var root2 = new RootDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Description = r.Str(n, "description", required: false), Mods = ReadMods(r, n) };
                foreach (var c in r.Arr(n, "costs")) root2.Costs.Add((long)c.AsNumber);
                if (root2.Costs.Count == 0) r.Error(n, "una Raíz necesita al menos un nivel (tope)");
                f.Roots.Add(root2);
            }

            var of = r.Obj(root, "ofrenda");
            f.Ofrenda.LevelBase = r.Num(of, "levelBase", 1, 1e12);
            f.Ofrenda.LevelExponent = r.Num(of, "levelExponent", 0.1, 5);
            f.Ofrenda.PerLevel = ReadMods(r, of, "perLevel");
            f.Ofrenda.Caps = ReadMods(r, of, "caps");
            f.Ofrenda.DustEvery = r.Int(of, "dustEvery", 1, 1000);
            f.Ofrenda.DustAmount = (long)r.Num(of, "dustAmount", 0, 1e9);
            foreach (var n in r.Arr(of, "unlocks")) f.Ofrenda.Unlocks[r.Int(n, "level", 1, 10000)] = r.Str(n, "text");

            var calc = r.Obj(root, "calculista");
            f.CalculistaBase = r.Int(calc, "base", 0, 100);
            f.CalculistaWitchBonus = r.Int(calc, "witchBonus", 0, 100);

            foreach (var n in r.Arr(root, "events"))
            {
                var ev = new FresnoEventDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Text = r.Str(n, "text", required: false) };
                foreach (var o in r.Arr(n, "options"))
                {
                    var opt = new EventOption { Text = r.Str(o, "text"), CostAlmitas = (long)r.Num(o, "costAlmitas", 0, 1e9, required: false) };
                    if (o.Has("stat"))
                    {
                        string stat = r.Str(o, "stat");
                        if (Enum.TryParse(stat, true, out CalculistaStat cs)) opt.Stat = cs;
                        else r.Error(o, "estadística desconocida: " + stat);
                        opt.Need = r.Int(o, "need", 0, 1000);
                    }
                    foreach (var oc in r.Arr(o, "outcomes"))
                    {
                        var outcome = new EventOutcome { Value = r.Num(oc, "value", -1e9, 1e9, required: false), Name = r.Str(oc, "name", required: false), Mods = ReadMods(r, oc) };
                        string type = r.Str(oc, "type");
                        if (!Enum.TryParse(type, true, out outcome.Type)) r.Error(oc, "resultado desconocido: " + type);
                        opt.Outcomes.Add(outcome);
                    }
                    ev.Options.Add(opt);
                }
                if (ev.Options.Count < 2) r.Error(n, "un evento ofrece al menos dos decisiones");
                f.Events.Add(ev);
            }
        }
    }
}
