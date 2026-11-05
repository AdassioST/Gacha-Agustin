using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Meta;

namespace SalYAlmas.Core.World
{
    /// <summary>Carga y valida story.json, services.json y loc.json. Junta todos los errores, como los otros cargadores.</summary>
    public static class WorldLoader
    {
        public static WorldDatabase Load(string story, string services, string loc, MetaDatabase meta)
        {
            var r = new Reader();
            var w = new WorldDatabase();
            r.Guard("story.json", () => LoadStory(Json.Parse(story, "story.json"), w.Story, meta, r));
            r.Guard("services.json", () => LoadServices(Json.Parse(services, "services.json"), w, meta, r));
            r.Guard("loc.json", () => LoadLoc(Json.Parse(loc, "loc.json"), w.Loc, r));
            if (r.Errors.Count > 0) throw new DataValidationException(r.Errors);
            return w;
        }

        private static CalculistaStat ParseStat(Reader r, JsonNode at, string s)
        {
            switch (s)
            {
                case "calculo": return CalculistaStat.Calculo;
                case "temple": return CalculistaStat.Temple;
                case "encanto": return CalculistaStat.Encanto;
                case "oficio": return CalculistaStat.Oficio;
            }
            r.Error(at, "estadística desconocida: " + s);
            return CalculistaStat.Calculo;
        }

        private static StatBlock Mods(Reader r, JsonNode parent)
        {
            var b = new StatBlock();
            var n = parent.Get("mods");
            if (n == null) return b;
            foreach (var kv in n.Properties)
            {
                if (!ModKeys.IsKnown(kv.Key)) { r.Error(kv.Value, "modificador desconocido: " + kv.Key); continue; }
                b.Add(kv.Key, kv.Value.AsNumber);
            }
            return b;
        }

        private static void Strings(JsonNode parent, string key, List<string> into)
        {
            var n = parent.Get(key);
            if (n == null) return;
            if (n.Kind == JsonKind.String) { into.Add(n.AsString); return; }
            foreach (var i in n.Items) into.Add(i.AsString);
        }

        private static void Bundle(Reader r, MetaDatabase meta, JsonNode node, List<KeyValuePair<string, long>> into)
        {
            if (node == null) return;
            foreach (var kv in node.Properties)
            {
                if (!meta.Catalog.Has(kv.Key)) { r.Error(kv.Value, "recurso desconocido: " + kv.Key); continue; }
                into.Add(new KeyValuePair<string, long>(kv.Key, (long)kv.Value.AsNumber));
            }
        }

        // ------------------------------------------------------------------ Historia

        private static void LoadStory(JsonNode root, StoryRules s, MetaDatabase meta, Reader r)
        {
            var c = r.Obj(root, "calculista");
            if (c != null)
            {
                s.StatMax = r.Int(c, "statMax", 1, 100);
                s.StatBase = r.Int(c, "statBase", 0, 100);
                s.WitchBonus = r.Int(c, "witchBonus", 0, 100);
                Strings(c, "species", s.Species);
                Strings(c, "pronouns", s.Pronouns);
                foreach (var n in r.Arr(c, "signs"))
                    s.Signs.Add(new SignDef
                    {
                        Id = r.Str(n, "id"), Name = r.Str(n, "name"), Element = r.Str(n, "element"), Phrase = r.Str(n, "phrase", false),
                        Region = r.Str(n, "region", false), Bonus = ParseStat(r, n, r.Str(n, "bonus")),
                    });
            }

            var p = r.Obj(root, "paralaje");
            if (p != null)
            {
                s.ParalajeMax = r.Int(p, "max", 1, 1000);
                var bands = r.Arr(p, "bands");
                if (bands.Count != 3) r.Error(p, "'bands' lleva tres umbrales");
                else for (int i = 0; i < 3; i++) s.ParalajeBands[i] = (int)bands[i].AsNumber;
            }
            var e = r.Obj(root, "eclipse");
            if (e != null)
            {
                s.EclipseEvery = r.Int(e, "everyRegions", 1, 12);
                s.MaxCorruption = r.Int(e, "maxCorruption", 0, 10);
                Strings(e, "ring", s.Ring);
            }

            var sen = r.Obj(root, "sendero");
            if (sen != null)
            {
                s.SenderoMaxLevel = r.Int(sen, "maxLevel", 1, 100);
                s.SenderoXpPerLevel = r.Int(sen, "xpPerLevel", 1, 100000);
                s.SenderoXpPerWin = r.Int(sen, "xpPerWin", 0, 100000);
                s.SenderoGrowth = r.Num(sen, "growthPerLevel", 0, 1);
                foreach (var n in r.Arr(sen, "list"))
                    s.Senderos.Add(new SenderoDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Description = r.Str(n, "description"), Mods = Mods(r, n), Pending = r.Str(n, "pending", false) });
            }

            foreach (var n in r.Arr(root, "titles"))
                s.Titles.Add(new TitleDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Description = r.Str(n, "description", false), Mods = Mods(r, n) });

            foreach (var n in r.Arr(root, "regions"))
            {
                var region = new RegionDef
                {
                    Id = r.Str(n, "id"), Sign = r.Str(n, "sign", false), Name = r.Str(n, "name"), Theme = r.Str(n, "theme", false),
                    Echo = r.Str(n, "echo", false), Prologue = r.Bool(n, "prologue", false),
                };
                foreach (var mn in r.Arr(n, "missions")) region.Missions.Add(ReadMission(mn, s, meta, r));
                s.Regions.Add(region);
            }

            foreach (var sign in s.Signs)
                if (sign.Region.Length > 0 && s.Region(sign.Region) == null) r.Error(root, "signo con región desconocida: " + sign.Region);
        }

        private static MissionDef ReadMission(JsonNode n, StoryRules s, MetaDatabase meta, Reader r)
        {
            var m = new MissionDef { Id = r.Str(n, "id"), Name = r.Str(n, "name"), Description = r.Str(n, "description", false), Start = r.Str(n, "start") };
            m.Rewards = ReadEffects(n.Get("rewards"), s, meta, r);
            foreach (var sn in r.Arr(n, "scenes"))
            {
                var scene = new StoryScene
                {
                    Id = r.Str(sn, "id"), Speaker = r.Str(sn, "speaker", false), Portrait = r.Str(sn, "portrait", false), Text = r.Str(sn, "text", false), Next = r.Str(sn, "next", false),
                    Effects = ReadEffects(sn.Get("effects"), s, meta, r),
                };
                if (scene.Portrait.Length > 0 && !meta.Roster.ContainsKey(scene.Portrait)) r.Error(sn, "retrato desconocido: " + scene.Portrait);
                foreach (var vn in r.Arr(sn, "variants", false))
                    scene.Variants.Add(new StoryVariant { When = ReadCondition(vn.Get("when"), r), Text = r.Str(vn, "text") });
                foreach (var cn in r.Arr(sn, "choices", false))
                {
                    var choice = new StoryChoice { Text = r.Str(cn, "text"), When = ReadCondition(cn.Get("when"), r), Next = r.Str(cn, "next", false), Effects = ReadEffects(cn.Get("effects"), s, meta, r) };
                    if (cn.Has("stat")) { choice.Stat = ParseStat(r, cn, r.Str(cn, "stat")); choice.Need = r.Int(cn, "need", 0, 100); }
                    scene.Choices.Add(choice);
                }
                var bn = sn.Get("battle");
                if (bn != null)
                {
                    var b = new StoryBattle { Strength = r.Num(bn, "strength", 0.01, 100, false, 1.0), Boss = r.Bool(bn, "boss", false), Tutorial = r.Bool(bn, "tutorial", false), Next = r.Str(bn, "next", false) };
                    Strings(bn, "enemies", b.Enemies);
                    Strings(bn, "team", b.Team);
                    foreach (var id in b.Enemies) if (meta.Enemy(id) == null) r.Error(bn, "enemigo desconocido: " + id);
                    foreach (var id in b.Team) if (!meta.HasKit(id)) r.Error(bn, "bruja sin kit en el combate: " + id);
                    if (b.Enemies.Count == 0) r.Error(bn, "un combate lleva enemigos");
                    scene.Battle = b;
                }
                if (m.Scenes.ContainsKey(scene.Id)) r.Error(sn, "escena repetida: " + scene.Id);
                else m.Scenes[scene.Id] = scene;
            }
            if (!m.Scenes.ContainsKey(m.Start ?? "")) r.Error(n, "la escena inicial no existe: " + m.Start);
            foreach (var scene in m.Scenes.Values)
            {
                Check(r, n, m, scene.Next);
                if (scene.Battle != null) Check(r, n, m, scene.Battle.Next);
                foreach (var choice in scene.Choices) Check(r, n, m, choice.Next);
            }
            return m;
        }

        private static void Check(Reader r, JsonNode at, MissionDef m, string next)
        {
            if (!string.IsNullOrEmpty(next) && !m.Scenes.ContainsKey(next)) r.Error(at, $"{m.Id}: la escena '{next}' no existe");
        }

        private static StoryCondition ReadCondition(JsonNode n, Reader r)
        {
            var c = new StoryCondition();
            if (n == null) return c;
            Strings(n, "marks", c.Marks);
            Strings(n, "notMarks", c.NotMarks);
            c.Title = r.Str(n, "title", false);
            c.Witch = r.Str(n, "witch", false);
            c.MinCorruption = r.Int(n, "minCorruption", 0, 10, false);
            c.MinSouls = r.Int(n, "minSouls", 0, 1000000, false);
            c.MinParalaje = r.Int(n, "minParalaje", 0, 1000, false);
            return c;
        }

        private static StoryEffects ReadEffects(JsonNode n, StoryRules s, MetaDatabase meta, Reader r)
        {
            var e = new StoryEffects();
            if (n == null) return e;
            Strings(n, "marks", e.Marks);
            e.Title = r.Str(n, "title", false);
            if (e.Title.Length > 0 && s.Title(e.Title) == null) r.Error(n, "título desconocido: " + e.Title);
            e.Souls = r.Int(n, "souls", -100000, 100000, false);
            e.Paralaje = r.Int(n, "paralaje", -1000, 1000, false);
            e.Witch = r.Str(n, "witch", false);
            if (e.Witch.Length > 0 && !meta.Roster.ContainsKey(e.Witch)) r.Error(n, "bruja desconocida: " + e.Witch);
            Bundle(r, meta, n.Get("res"), e.Rewards);
            return e;
        }

        // ------------------------------------------------------------------ Termas y tienda

        private static void LoadServices(JsonNode root, WorldDatabase w, MetaDatabase meta, Reader r)
        {
            var t = r.Obj(root, "termas");
            if (t != null)
            {
                var d = w.Termas;
                d.BathParalaje = r.Int(t, "bathParalaje", -1000, 0);
                d.BathCooldownDays = r.Int(t, "bathCooldownDays", 0, 100);
                d.TalkTrust = r.Int(t, "talkTrust", 0, 10000);
                d.TrustPerLevel = r.Int(t, "trustPerLevel", 1, 100000);
                d.TrustMaxLevel = r.Int(t, "trustMaxLevel", 1, 100);
                d.DefaultShake = r.Str(t, "defaultShake");
                Strings(t, "genericTalk", d.GenericTalk);
                Strings(t, "genericTouch", d.GenericTouch);
                var talk = t.Get("talk");
                if (talk != null)
                    foreach (var kv in talk.Properties)
                    {
                        if (!meta.Roster.ContainsKey(kv.Key)) r.Error(kv.Value, "bruja desconocida: " + kv.Key);
                        var list = new List<string>();
                        foreach (var i in kv.Value.Items) list.Add(i.AsString);
                        d.Talk[kv.Key] = list;
                    }
                ReadMap(t.Get("shake"), d.Shake);
                ReadMap(t.Get("pullLine"), d.PullLine);
            }

            var s = r.Obj(root, "shop");
            if (s == null) return;
            var shop = w.Shop;
            foreach (var n in r.Arr(s, "products"))
            {
                string kind = r.Str(n, "kind");
                shop.Products.Add(new ProductDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), PriceUsd = r.Num(n, "usd", 0, 1000), Perlas = (long)r.Num(n, "perlas", 0, 1000000, false),
                    Bonus = (long)r.Num(n, "bonus", 0, 1000000, false), DailySales = r.Int(n, "dailySales", 0, 100000, false), Days = r.Int(n, "days", 0, 1000, false),
                    Kind = kind == "grimorio" ? ProductKind.Grimorio : kind == "diezmo" ? ProductKind.Diezmo : ProductKind.Perlas,
                });
            }
            foreach (var n in r.Arr(s, "cosmetics"))
            {
                // Ningún cosmético da estadísticas (GDD 16.5): el cargador lo rechaza.
                if (n.Has("mods")) r.Error(n, "un cosmético no puede dar estadísticas");
                var c = new CosmeticDef
                {
                    Id = r.Str(n, "id"), Name = r.Str(n, "name"), Kind = r.Str(n, "kind"), Witch = r.Str(n, "witch", false), Description = r.Str(n, "description", false),
                    Perlas = (long)r.Num(n, "perlas", 0, 1000000, false), Polvo = (long)r.Num(n, "polvo", 0, 1000000, false), Color = r.Str(n, "color", false),
                };
                if (c.Witch.Length > 0 && !meta.Roster.ContainsKey(c.Witch)) r.Error(n, "bruja desconocida: " + c.Witch);
                shop.Cosmetics.Add(c);
            }
            var pass = r.Obj(s, "grimorio");
            if (pass != null)
            {
                shop.PassXpPerLevel = r.Int(pass, "xpPerLevel", 1, 1000000);
                shop.PassDays = r.Int(pass, "days", 1, 1000);
                foreach (var n in r.Arr(pass, "tiers"))
                {
                    var tier = new PassTier { Level = r.Int(n, "level", 1, 1000), PremiumCosmetic = r.Str(n, "premiumCosmetic", false) };
                    Bundle(r, meta, n.Get("free"), tier.Free);
                    Bundle(r, meta, n.Get("premium"), tier.Premium);
                    if (tier.PremiumCosmetic.Length > 0 && shop.Cosmetic(tier.PremiumCosmetic) == null) r.Error(n, "cosmético desconocido: " + tier.PremiumCosmetic);
                    shop.Pass.Add(tier);
                }
            }
            Strings(s, "paidGachaBlocked", shop.PaidGachaBlocked);
        }

        private static void ReadMap(JsonNode n, Dictionary<string, string> into)
        {
            if (n == null) return;
            foreach (var kv in n.Properties) into[kv.Key] = kv.Value.AsString;
        }

        private static void LoadLoc(JsonNode root, LocTable loc, Reader r)
        {
            Strings(root, "languages", loc.Languages);
            loc.Fallback = r.Str(root, "fallback");
            var strings = r.Obj(root, "strings");
            if (strings == null) return;
            foreach (var kv in strings.Properties)
            {
                var row = new Dictionary<string, string>();
                foreach (var t in kv.Value.Properties)
                {
                    if (!loc.Languages.Contains(t.Key)) r.Error(t.Value, "idioma desconocido: " + t.Key);
                    row[t.Key] = t.Value.AsString;
                }
                loc.Strings[kv.Key] = row;
            }
        }
    }
}
