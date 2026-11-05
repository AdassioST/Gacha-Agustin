using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Data
{
    /// <summary>Textos de los cinco JSON de datos.</summary>
    public sealed class DataSources
    {
        public string Rules;
        public string Witches;
        public string Enemies;
        public string Encounters;
        public string Potions;

        public static readonly string[] FileNames = { "rules.json", "witches.json", "enemies.json", "encounters.json", "potions.json" };

        /// <summary>Lee los JSON de una carpeta (Assets/SalYAlmas/Data o una copia).</summary>
        public static DataSources FromDirectory(string directory)
        {
            string Read(string name) => File.ReadAllText(Path.Combine(directory, name));
            return new DataSources
            {
                Rules = Read("rules.json"),
                Witches = Read("witches.json"),
                Enemies = Read("enemies.json"),
                Encounters = Read("encounters.json"),
                Potions = Read("potions.json"),
            };
        }
    }

    /// <summary>
    /// Convierte los JSON en definiciones y las valida. Junta todos los errores y los lanza juntos en una
    /// DataValidationException, para que diseño vea todo lo que está mal de una vez.
    /// </summary>
    public static class DataLoader
    {
        public static GameDatabase Load(DataSources src)
        {
            var r = new Reader();
            var rules = new CombatRules();
            if (!string.IsNullOrEmpty(src.Rules)) r.Guard("rules.json", () => RulesLoader.Apply(Json.Parse(src.Rules, "rules.json"), rules, r));
            var db = new GameDatabase(rules);

            r.Guard("witches.json", () => LoadWitches(Json.Parse(src.Witches, "witches.json"), db, r));
            r.Guard("enemies.json", () => LoadEnemies(Json.Parse(src.Enemies, "enemies.json"), db, r));
            r.Guard("encounters.json", () => LoadEncounters(Json.Parse(src.Encounters, "encounters.json"), db, r));
            r.Guard("potions.json", () => LoadPotions(Json.Parse(src.Potions, "potions.json"), db, r));

            if (r.Errors.Count > 0) throw new DataValidationException(r.Errors);
            return db;
        }

        public static GameDatabase LoadFromDirectory(string directory) => Load(DataSources.FromDirectory(directory));

        // ------------------------------------------------------------------ Brujas

        /// <summary>Suma brujas de otro archivo a una base ya cargada (el resto del elenco, GDD 18).</summary>
        internal static void AddWitches(JsonNode root, GameDatabase db, Reader r) => LoadWitches(root, db, r);

        private static void LoadWitches(JsonNode root, GameDatabase db, Reader r)
        {
            foreach (var n in r.Arr(root, "witches"))
            {
                var w = new WitchDef();
                w.Id = r.Str(n, "id");
                w.Name = r.Str(n, "name");
                w.ShortName = r.Str(n, "shortName", required: false, def: w.Name);
                w.Species = r.Str(n, "species", required: false);
                w.Rarity = r.Int(n, "rarity", 3, 5);
                w.Role = r.Str(n, "role", required: false);
                w.Ritual = r.Str(n, "ritual", required: false);

                var asc = r.Obj(n, "ascendant");
                if (asc != null)
                {
                    w.AscendantSign = r.Str(asc, "sign");
                    w.AscendantName = r.Str(asc, "name");
                }

                var mix = r.Obj(n, "mix");
                if (mix != null)
                {
                    w.Primary = r.PlanetOf(mix, "primary");
                    w.Secondary = r.PlanetOf(mix, "secondary");
                    w.MixName = r.Str(mix, "name", required: false);
                    if (w.Primary == w.Secondary) r.Error(mix, "la mezcla necesita dos planetas distintos (GDD 7.2)");
                }

                string via = r.Str(n, "via", required: false, def: "mayor");
                if (via == "mayor") w.Via = Via.Mayor;
                else if (via == "menor") w.Via = Via.Menor;
                else r.Error(n, "via debe ser 'mayor' o 'menor'");

                string grade = r.Str(n, "grade", required: false, def: "B");
                if (!PurityMath.TryParseGrade(grade, out w.Grade)) r.Error(n, "grado inválido: " + grade);

                var st = r.Obj(n, "stats");
                if (st != null)
                {
                    w.Level = r.Int(st, "level", 1, 80);
                    w.MaxHp = r.Int(st, "hp", 1, 1000000);
                    w.Atk = r.Num(st, "atk", 0, 100000);
                    w.Speed = r.Num(st, "spd", 1, 400);
                    w.CritRate = r.Num(st, "critRate", 0, 1, required: false, def: db.Rules.Combat.BaseCritRate);
                    w.CritDamage = r.Num(st, "critDmg", 0, 5, required: false, def: db.Rules.Combat.BaseCritDamage);
                }

                var passive = n.Get("passive");
                if (passive != null)
                {
                    w.PassiveName = r.Str(passive, "name", required: false);
                    w.PassiveDescription = r.Str(passive, "description", required: false);
                    w.PassiveAtkMultiplier = r.Num(passive, "atkMultiplier", 0, 5, required: false, def: 1.0);
                    w.DealsNoDamage = r.Bool(passive, "dealsNoDamage", false);
                    w.ReviveOnceHp = r.Num(passive, "reviveOnce", 0, 1, required: false);
                    w.StartPoison = r.Num(passive, "startPoison", 0, 5, required: false);
                }

                var sc = r.Obj(n, "selfCast");
                if (sc != null)
                {
                    string form = r.Str(sc, "form");
                    switch (form)
                    {
                        case "none": w.SelfCast = SelfCastForm.None; break;
                        case "agujaYSello": w.SelfCast = SelfCastForm.AgujaYSello; break;
                        case "hilos": w.SelfCast = SelfCastForm.Hilos; break;
                        case "frenesiSalNegra": w.SelfCast = SelfCastForm.FrenesiSalNegra; break;
                        default: r.Error(sc, "forma de Autohechizo desconocida: " + form); break;
                    }
                    w.SelfCastName = r.Str(sc, "name", required: w.SelfCast != SelfCastForm.None);
                    if (w.Rarity == 3 && w.SelfCast != SelfCastForm.None)
                        r.Error(sc, "las Aprendizas 3★ no tienen Autohechizo (GDD 12.1: es parte del kit de 4★ y 5★)");
                    if (w.Rarity >= 4 && w.SelfCast == SelfCastForm.None)
                        r.Error(sc, "las 4★ y 5★ tienen Autohechizo (GDD 12.1)");
                }

                var moves = r.Obj(n, "moves");
                if (moves != null)
                {
                    w.Basic = ReadMove(r, moves, "basic", MoveKind.Basic, db.Rules);
                    w.Spell = ReadMove(r, moves, "spell", MoveKind.Spell, db.Rules);
                    w.Opening = ReadMove(r, moves, "opening", MoveKind.Opening, db.Rules);
                    w.Link = ReadMove(r, moves, "link", MoveKind.Link, db.Rules);
                    w.Closing = ReadMove(r, moves, "closing", MoveKind.Closing, db.Rules);
                    for (int i = 0; i < 3; i++)
                        w.Ascensions[i] = ReadMove(r, moves, "ascension" + (i + 1), MoveKind.Ascension, db.Rules);
                    if (w.Basic != null && w.Basic.Target != TargetMode.Enemy && w.Basic.Target != TargetMode.AllEnemies)
                        r.Error(moves, "el ataque básico debe apuntar a enemigos");
                }

                var reg = r.Obj(n, "regencia");
                if (reg != null)
                {
                    w.Regencia = new RegenciaDef
                    {
                        Name = r.Str(reg, "name"),
                        Description = r.Str(reg, "description"),
                        TeamAtk = r.Num(reg, "teamAtk", 0, 1, required: false),
                        TeamSpeedFlat = r.Num(reg, "teamSpdFlat", 0, 100, required: false),
                        TeamHp = r.Num(reg, "teamHp", 0, 1, required: false),
                        StartGrains = r.Int(reg, "startGrains", 0, 5, required: false),
                        StartResonance = r.Int(reg, "startResonance", 0, 100, required: false),
                        StartEnemyDelay = r.Num(reg, "startEnemyDelay", 0, 1, required: false),
                    };
                }

                if (string.IsNullOrEmpty(w.Id)) continue;
                if (db.Witches.ContainsKey(w.Id)) { r.Error(n, "id de bruja repetido: " + w.Id); continue; }
                db.Add(w);
            }
            if (db.Witches.Count == 0) r.Error(root, "no hay brujas");
        }

        private static MoveDef ReadMove(Reader r, JsonNode parent, string key, MoveKind kind, CombatRules rules)
        {
            var n = r.Obj(parent, key);
            if (n == null) return null;
            var m = new MoveDef
            {
                Id = r.Str(n, "id"),
                Name = r.Str(n, "name"),
                Kind = kind,
                Multiplier = r.Num(n, "multiplier", 0, 50, required: false),
                Toughness = r.Num(n, "toughness", 0, 500, required: false),
                ByAscendant = r.Bool(n, "byAscendant", kind != MoveKind.Basic),
                Description = r.Str(n, "description", required: false),
            };
            m.Target = r.TargetModeOf(n, "target", TargetMode.Enemy);
            m.Hits = ReadHits(r, n, required: kind != MoveKind.Spell);
            m.Effects = ReadEffects(r, n);

            if ((m.Multiplier > 0 || m.Toughness > 0) && m.Hits.Count == 0)
                r.Error(n, "un movimiento con multiplicador o Firmeza necesita golpes");
            if (m.Hits.Count > 0 && m.Target != TargetMode.Enemy && m.Target != TargetMode.AllEnemies)
                r.Error(n, "solo los movimientos contra enemigos pueden tener golpes");
            bool chainMove = kind == MoveKind.Opening || kind == MoveKind.Link || kind == MoveKind.Closing;
            if (chainMove && m.Hits.Count == 0)
                r.Error(n, "los movimientos de Cadena declaran sus frames de golpe (GDD 9.5)");
            if (chainMove && m.LastHitFrame > rules.Chain.MaxLinkFrames)
                r.Error(n, $"un eslabón dura máximo {rules.Chain.MaxLinkFrames} frames (1.8 s, GDD 9.5)");
            if (chainMove && m.Target != TargetMode.Enemy && m.Target != TargetMode.AllEnemies)
                r.Error(n, "los movimientos de Cadena apuntan a enemigos");
            return m;
        }

        internal static List<HitDef> ReadHits(Reader r, JsonNode n, bool required)
        {
            var hits = new List<HitDef>();
            var arr = n.Get("hits");
            if (arr == null)
            {
                if (required) r.Error(n, "falta 'hits'");
                return hits;
            }
            if (arr.Kind != JsonKind.Array) { r.Error(arr, "'hits' debe ser un arreglo"); return hits; }
            bool explicitShares = false;
            foreach (var h in arr.Items)
            {
                if (h.Kind == JsonKind.Number) hits.Add(new HitDef((int)h.AsNumber, 0));
                else if (h.Kind == JsonKind.Array && h.Items.Count == 2 && h.Items[0].Kind == JsonKind.Number && h.Items[1].Kind == JsonKind.Number)
                {
                    hits.Add(new HitDef((int)h.Items[0].AsNumber, h.Items[1].AsNumber));
                    explicitShares = true;
                }
                else r.Error(h, "cada golpe es un frame o un par [frame, parte]");
            }
            if (hits.Count == 0) return hits;
            if (!explicitShares)
            {
                foreach (var h in hits) h.Share = 1.0 / hits.Count;
            }
            else
            {
                double sum = 0;
                foreach (var h in hits)
                {
                    if (h.Share <= 0) r.Error(arr, "la parte de cada golpe debe ser positiva");
                    sum += h.Share;
                }
                if (Math.Abs(sum - 1.0) > 0.001) r.Error(arr, $"las partes de los golpes deben sumar 1 (suman {sum:0.###})");
            }
            int prev = -1;
            foreach (var h in hits)
            {
                if (h.Frame < 0) r.Error(arr, "los frames no pueden ser negativos");
                if (h.Frame < prev) r.Error(arr, "los frames deben ir en orden");
                prev = h.Frame;
            }
            return hits;
        }

        internal static List<EffectDef> ReadEffects(Reader r, JsonNode n, string key = "effects")
        {
            var list = new List<EffectDef>();
            var arr = n.Get(key);
            if (arr == null) return list;
            if (arr.Kind != JsonKind.Array) { r.Error(arr, "'effects' debe ser un arreglo"); return list; }
            foreach (var e in arr.Items)
            {
                var fx = new EffectDef();
                string type = r.Str(e, "type");
                if (!Reader.EffectTypes.TryGetValue(type ?? "", out fx.Type)) { r.Error(e, "tipo de efecto desconocido: " + type); continue; }
                fx.Target = r.MapOf(e, "target", Reader.EffectTargets, EffectTarget.Target);
                fx.At = r.MapOf(e, "at", Reader.Timings, EffectTiming.End);
                fx.Value = r.Num(e, "value", -1000, 1000, required: false);
                fx.Turns = r.Int(e, "turns", 0, 20, required: false);
                fx.Count = r.Int(e, "count", 0, 20, required: false);
                fx.Name = r.Str(e, "name", required: false);
                if (fx.Type == EffectType.Buff || fx.Type == EffectType.Debuff)
                {
                    fx.Stat = r.MapOf(e, "stat", Reader.Stats, StatKind.Atk, required: true);
                    if (fx.Turns <= 0) r.Error(e, "las mejoras y penalizaciones necesitan 'turns'");
                }
                if (fx.Type == EffectType.Dot)
                {
                    fx.Dot = r.MapOf(e, "dot", Reader.Dots, DotKind.Burn, required: true);
                    if (fx.Turns <= 0) r.Error(e, "los daños por turno necesitan 'turns'");
                }
                if ((fx.Type == EffectType.Cleanse || fx.Type == EffectType.Dispel) && fx.Count <= 0) fx.Count = 1;
                list.Add(fx);
            }
            return list;
        }

        // ------------------------------------------------------------------ Enemigos

        private static void LoadEnemies(JsonNode root, GameDatabase db, Reader r) =>
            ReadEnemies(root, r, id => db.Enemies.ContainsKey(id), db.Add);

        /// <summary>Lee y valida enemigos con el formato de enemies.json (también los del Fresno).</summary>
        internal static void ReadEnemies(JsonNode root, Reader r, Func<string, bool> exists, Action<EnemyDef> add)
        {
            foreach (var n in r.Arr(root, "enemies"))
            {
                var e = new EnemyDef
                {
                    Id = r.Str(n, "id"),
                    Name = r.Str(n, "name"),
                    Description = r.Str(n, "description", required: false),
                    Article = r.Str(n, "article", required: false),
                    Level = r.Int(n, "level", 1, 100),
                    MaxHp = r.Int(n, "hp", 1, 100000000),
                    Atk = r.Num(n, "atk", 0, 100000),
                    Speed = r.Num(n, "spd", 1, 400),
                    Toughness = r.Num(n, "toughness", 1, 10000),
                    AttackPlanet = r.PlanetOf(n, "attackPlanet"),
                    IsBoss = r.Bool(n, "boss", false),
                };
                e.ShortName = r.Str(n, "shortName", required: false, def: e.Name);
                e.ActionsPerTurn = r.Int(n, "actionsPerTurn", 1, 3, required: false, def: 1);

                foreach (var w in r.Arr(n, "weaknesses"))
                {
                    if (w.Kind != JsonKind.String || !Planets.TryParse(w.AsString, out var p)) { r.Error(w, "planeta inválido"); continue; }
                    if (e.Weaknesses.Contains(p)) r.Error(w, "debilidad repetida: " + w.AsString);
                    else e.Weaknesses.Add(p);
                }
                if (e.Weaknesses.Count < 2 || e.Weaknesses.Count > 3)
                    r.Error(n, $"{e.Id}: cada enemigo tiene de dos a tres planetas débiles (GDD 9.4)");

                var res = n.Get("res");
                if (res != null)
                {
                    foreach (var kv in res.Properties)
                    {
                        if (!Planets.TryParse(kv.Key, out var p)) { r.Error(kv.Value, "planeta inválido en res: " + kv.Key); continue; }
                        if (kv.Value.Kind != JsonKind.Number || kv.Value.AsNumber < -1 || kv.Value.AsNumber > 1) { r.Error(kv.Value, "res entre −1 y 1"); continue; }
                        e.Res[p] = kv.Value.AsNumber;
                    }
                }

                foreach (var s in r.Arr(n, "skills"))
                {
                    var sk = new EnemySkillDef
                    {
                        Id = r.Str(s, "id"),
                        Name = r.Str(s, "name"),
                        TargetRule = r.MapOf(s, "target", Reader.EnemyTargets, EnemyTargetRule.Random, required: true),
                        Multiplier = r.Num(s, "multiplier", 0, 50, required: false),
                        IsUltimate = r.Bool(s, "ultimate", false),
                        Charge = r.Bool(s, "charge", false),
                        ChargeText = r.Str(s, "chargeText", required: false),
                        Description = r.Str(s, "description", required: false),
                    };
                    sk.Hits = ReadHits(r, s, required: false);
                    sk.Effects = ReadEffects(r, s);
                    if (sk.Multiplier > 0 && sk.Hits.Count == 0) r.Error(s, "una habilidad con daño necesita golpes");
                    if (e.Skill(sk.Id) != null) r.Error(s, "habilidad repetida: " + sk.Id);
                    else e.Skills.Add(sk);
                }

                foreach (var p in r.Arr(n, "pattern"))
                {
                    if (p.Kind != JsonKind.String || e.Skill(p.AsString) == null) r.Error(p, "el patrón usa una habilidad que no existe");
                    else e.Pattern.Add(p.AsString);
                }
                if (e.Pattern.Count == 0) r.Error(n, $"{e.Id}: la IA necesita un patrón");

                var ph = n.Get("phase2");
                if (ph != null)
                {
                    e.Phase2 = new EnemyPhaseDef
                    {
                        HpBelow = r.Num(ph, "hpBelow", 0.01, 0.99),
                        SpeedBonus = r.Num(ph, "spdBonus", 0, 200, required: false),
                        Text = r.Str(ph, "text", required: false),
                    };
                    foreach (var p in r.Arr(ph, "pattern"))
                    {
                        if (p.Kind != JsonKind.String || e.Skill(p.AsString) == null) r.Error(p, "la fase 2 usa una habilidad que no existe");
                        else e.Phase2.Pattern.Add(p.AsString);
                    }
                }

                var tr = n.Get("traits");
                if (tr != null)
                {
                    e.Traits.ReflectPct = r.Num(tr, "reflect", 0, 1, required: false);
                    e.Traits.AspectImmune = r.Bool(tr, "aspectImmune", false);
                    e.Traits.ProtectAllies = r.Num(tr, "protectAllies", 0, 0.9, required: false);
                    e.Traits.SpellEnrage = r.Num(tr, "spellEnrage", 0, 2, required: false);
                    e.Traits.RampAtk = r.Num(tr, "rampAtk", 0, 1, required: false);
                    e.Traits.HealAllies = r.Num(tr, "healAllies", 0, 1, required: false);
                    e.Traits.Shape = r.Str(tr, "shape", required: false);
                    e.Traits.Hint = r.Str(tr, "hint", required: false);
                    e.Traits.OnDeath.AddRange(ReadEffects(r, tr, "onDeath"));
                }

                if (string.IsNullOrEmpty(e.Id)) continue;
                if (exists(e.Id)) { r.Error(n, "id de enemigo repetido: " + e.Id); continue; }
                add(e);
            }
        }

        private static void LoadEncounters(JsonNode root, GameDatabase db, Reader r)
        {
            foreach (var n in r.Arr(root, "encounters"))
            {
                var enc = new EncounterDef
                {
                    Id = r.Str(n, "id"),
                    Name = r.Str(n, "name"),
                    Description = r.Str(n, "description", required: false),
                };
                foreach (var e in r.Arr(n, "enemies"))
                {
                    if (e.Kind != JsonKind.String || !db.Enemies.ContainsKey(e.AsString)) r.Error(e, "enemigo desconocido en el encuentro");
                    else enc.Enemies.Add(e.AsString);
                }
                if (enc.Enemies.Count < 1 || enc.Enemies.Count > 5) r.Error(n, "un encuentro tiene de 1 a 5 enemigos");
                if (string.IsNullOrEmpty(enc.Id)) continue;
                if (db.Encounters.ContainsKey(enc.Id)) { r.Error(n, "encuentro repetido: " + enc.Id); continue; }
                db.Add(enc);
            }
        }

        private static void LoadPotions(JsonNode root, GameDatabase db, Reader r)
        {
            foreach (var n in r.Arr(root, "potions"))
            {
                var p = new PotionDef
                {
                    Id = r.Str(n, "id"),
                    Name = r.Str(n, "name"),
                    Ingredients = r.Str(n, "ingredients", required: false),
                    Description = r.Str(n, "description"),
                    Target = r.MapOf(n, "target", Reader.PotionTargets, PotionTarget.Ally, required: true),
                    UsableInCombat = r.Bool(n, "inCombat", true),
                };
                p.Effects = ReadEffects(r, n);
                if (p.Effects.Count == 0) r.Error(n, "una Salmuera necesita al menos un efecto");
                if (string.IsNullOrEmpty(p.Id)) continue;
                if (db.Potions.ContainsKey(p.Id)) { r.Error(n, "poción repetida: " + p.Id); continue; }
                db.Add(p);
            }
        }
    }

    /// <summary>Sobrescribe CombatRules con rules.json usando los nombres de campo (sin distinguir mayúsculas).</summary>
    public static class RulesLoader
    {
        internal static void Apply(JsonNode node, object target, Reader r)
        {
            if (node.Kind != JsonKind.Object) { r.Error(node, "se esperaba un objeto"); return; }
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var kv in node.Properties)
            {
                FieldInfo field = null;
                foreach (var f in fields)
                    if (string.Equals(f.Name, kv.Key, StringComparison.OrdinalIgnoreCase)) { field = f; break; }
                if (field == null) { r.Error(kv.Value, "regla desconocida: " + kv.Key); continue; }

                var t = field.FieldType;
                var v = kv.Value;
                if (t == typeof(double))
                {
                    if (v.Kind != JsonKind.Number) r.Error(v, "se esperaba un número");
                    else field.SetValue(target, v.AsNumber);
                }
                else if (t == typeof(int))
                {
                    if (v.Kind != JsonKind.Number || Math.Abs(v.AsNumber - Math.Round(v.AsNumber)) > 1e-9) r.Error(v, "se esperaba un entero");
                    else field.SetValue(target, (int)Math.Round(v.AsNumber));
                }
                else if (t == typeof(bool))
                {
                    if (v.Kind != JsonKind.Bool) r.Error(v, "se esperaba true o false");
                    else field.SetValue(target, v.AsBool);
                }
                else if (t == typeof(double[]) || t == typeof(int[]))
                {
                    if (v.Kind != JsonKind.Array) { r.Error(v, "se esperaba un arreglo"); continue; }
                    var items = v.Items;
                    if (t == typeof(double[]))
                    {
                        var arr = new double[items.Count];
                        for (int i = 0; i < arr.Length; i++) arr[i] = items[i].AsNumber;
                        field.SetValue(target, arr);
                    }
                    else
                    {
                        var arr = new int[items.Count];
                        for (int i = 0; i < arr.Length; i++) arr[i] = (int)Math.Round(items[i].AsNumber);
                        field.SetValue(target, arr);
                    }
                }
                else if (t.IsClass)
                {
                    var sub = field.GetValue(target);
                    if (sub == null) { sub = Activator.CreateInstance(t); field.SetValue(target, sub); }
                    Apply(v, sub, r);
                }
                else r.Error(v, "tipo de regla no soportado: " + t.Name);
            }
        }

        /// <summary>Carga solo las reglas (sin brujas ni enemigos).</summary>
        public static CombatRules Load(string json)
        {
            var r = new Reader();
            var rules = new CombatRules();
            r.Guard("rules.json", () => Apply(Json.Parse(json, "rules.json"), rules, r));
            if (r.Errors.Count > 0) throw new DataValidationException(r.Errors);
            return rules;
        }
    }

    /// <summary>Lectura tipada con acumulación de errores.</summary>
    internal sealed class Reader
    {
        public readonly List<string> Errors = new List<string>();

        public static readonly Dictionary<string, EffectType> EffectTypes = new Dictionary<string, EffectType>
        {
            { "grains", EffectType.Grains }, { "advance", EffectType.Advance }, { "delay", EffectType.Delay },
            { "buff", EffectType.Buff }, { "debuff", EffectType.Debuff }, { "dot", EffectType.Dot },
            { "shield", EffectType.Shield }, { "heal", EffectType.Heal }, { "salinity", EffectType.Salinity },
            { "anima", EffectType.Anima }, { "resonance", EffectType.Resonance }, { "grainBoost", EffectType.GrainBoost },
            { "feignDeath", EffectType.FeignDeath }, { "cleanse", EffectType.Cleanse }, { "dispel", EffectType.Dispel },
            { "nextSpellDouble", EffectType.NextSpellDouble }, { "invertHealing", EffectType.InvertHealing },
            { "freeVislumbre", EffectType.FreeVislumbre }, { "nextToughness", EffectType.NextToughness },
            { "paralaje", EffectType.Paralaje }, { "taunt", EffectType.Taunt },
            { "maxHpBurst", EffectType.MaxHpBurst }, { "stealGrains", EffectType.StealGrains },
        };

        public static readonly Dictionary<string, EffectTarget> EffectTargets = new Dictionary<string, EffectTarget>
        {
            { "target", EffectTarget.Target }, { "self", EffectTarget.Self }, { "allAllies", EffectTarget.AllAllies },
            { "otherAllies", EffectTarget.OtherAllies }, { "allEnemies", EffectTarget.AllEnemies },
        };

        public static readonly Dictionary<string, EffectTiming> Timings = new Dictionary<string, EffectTiming>
        {
            { "start", EffectTiming.Start }, { "end", EffectTiming.End },
        };

        public static readonly Dictionary<string, StatKind> Stats = new Dictionary<string, StatKind>
        {
            { "atk", StatKind.Atk }, { "damage", StatKind.Damage }, { "speed", StatKind.Speed },
            { "speedFlat", StatKind.SpeedFlat }, { "critRate", StatKind.CritRate }, { "res", StatKind.Res },
            { "defenseDown", StatKind.DefenseDown }, { "damageTaken", StatKind.DamageTaken },
        };

        public static readonly Dictionary<string, DotKind> Dots = new Dictionary<string, DotKind>
        {
            { "burn", DotKind.Burn }, { "bleed", DotKind.Bleed }, { "poison", DotKind.Poison }, { "shock", DotKind.Shock },
        };

        public static readonly Dictionary<string, TargetMode> TargetModes = new Dictionary<string, TargetMode>
        {
            { "enemy", TargetMode.Enemy }, { "allEnemies", TargetMode.AllEnemies }, { "ally", TargetMode.Ally },
            { "allAllies", TargetMode.AllAllies }, { "self", TargetMode.Self }, { "none", TargetMode.None },
        };

        public static readonly Dictionary<string, EnemyTargetRule> EnemyTargets = new Dictionary<string, EnemyTargetRule>
        {
            { "random", EnemyTargetRule.Random }, { "lowestHp", EnemyTargetRule.LowestHp },
            { "highestSalinity", EnemyTargetRule.HighestSalinity }, { "all", EnemyTargetRule.All }, { "self", EnemyTargetRule.Self },
        };

        public static readonly Dictionary<string, PotionTarget> PotionTargets = new Dictionary<string, PotionTarget>
        {
            { "ally", PotionTarget.Ally }, { "allAllies", PotionTarget.AllAllies }, { "enemy", PotionTarget.Enemy }, { "none", PotionTarget.None },
        };

        public void Error(JsonNode at, string msg)
        {
            Errors.Add(at != null ? $"{at.Path} (línea {at.Line}): {msg}" : msg);
        }

        public void Guard(string source, Action action)
        {
            try { action(); }
            catch (JsonParseException ex) { Errors.Add(ex.Message); }
            catch (ArgumentNullException) { Errors.Add(source + ": falta el archivo"); }
        }

        public JsonNode Obj(JsonNode parent, string key, bool required = true)
        {
            var n = parent?.Get(key);
            if (n == null) { if (required) Error(parent, $"falta '{key}'"); return null; }
            if (n.Kind != JsonKind.Object) { Error(n, $"'{key}' debe ser un objeto"); return null; }
            return n;
        }

        public IReadOnlyList<JsonNode> Arr(JsonNode parent, string key, bool required = true)
        {
            var n = parent?.Get(key);
            if (n == null) { if (required) Error(parent, $"falta '{key}'"); return Array.Empty<JsonNode>(); }
            if (n.Kind != JsonKind.Array) { Error(n, $"'{key}' debe ser un arreglo"); return Array.Empty<JsonNode>(); }
            return n.Items;
        }

        public string Str(JsonNode parent, string key, bool required = true, string def = "")
        {
            var n = parent?.Get(key);
            if (n == null) { if (required) Error(parent, $"falta '{key}'"); return def; }
            if (n.Kind != JsonKind.String) { Error(n, $"'{key}' debe ser texto"); return def; }
            if (required && n.AsString.Length == 0) Error(n, $"'{key}' no puede estar vacío");
            return n.AsString;
        }

        public double Num(JsonNode parent, string key, double min, double max, bool required = true, double def = 0)
        {
            var n = parent?.Get(key);
            if (n == null) { if (required) Error(parent, $"falta '{key}'"); return def; }
            if (n.Kind != JsonKind.Number) { Error(n, $"'{key}' debe ser un número"); return def; }
            if (n.AsNumber < min || n.AsNumber > max) Error(n, $"'{key}' fuera de rango [{min}, {max}]: {n.AsNumber}");
            return n.AsNumber;
        }

        public int Int(JsonNode parent, string key, int min, int max, bool required = true, int def = 0)
        {
            double v = Num(parent, key, min, max, required, def);
            if (Math.Abs(v - Math.Round(v)) > 1e-9) Error(parent?.Get(key), $"'{key}' debe ser entero");
            return (int)Math.Round(v);
        }

        public bool Bool(JsonNode parent, string key, bool def)
        {
            var n = parent?.Get(key);
            if (n == null) return def;
            if (n.Kind != JsonKind.Bool) { Error(n, $"'{key}' debe ser true o false"); return def; }
            return n.AsBool;
        }

        public Planet PlanetOf(JsonNode parent, string key)
        {
            string s = Str(parent, key);
            if (!Planets.TryParse(s, out var p) && s.Length > 0) Error(parent.Get(key), "planeta desconocido: " + s);
            return p;
        }

        public TargetMode TargetModeOf(JsonNode parent, string key, TargetMode def) => MapOf(parent, key, TargetModes, def);

        public T MapOf<T>(JsonNode parent, string key, Dictionary<string, T> map, T def, bool required = false)
        {
            var n = parent?.Get(key);
            if (n == null) { if (required) Error(parent, $"falta '{key}'"); return def; }
            if (n.Kind != JsonKind.String || !map.TryGetValue(n.AsString, out var v))
            {
                Error(n, $"valor inválido para '{key}'. Opciones: {string.Join(", ", map.Keys)}");
                return def;
            }
            return v;
        }
    }
}
