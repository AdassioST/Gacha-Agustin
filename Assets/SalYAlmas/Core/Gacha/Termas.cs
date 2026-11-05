using System;
using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Magic;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Gacha
{
    /// <summary>Lo que salió en una tirada.</summary>
    public sealed class PullResult
    {
        public int Rarity;
        /// <summary>"bruja", "polvo" o "receta".</summary>
        public string Kind;
        public string WitchId;
        public long Dust;
        public string RecipeId;
        public Planet? SaltPlanet;
        public long SaltAmount;
        public bool Featured;
        public bool LostFeatured;
        public int PityAt;

        /// <summary>Color del vapor (GDD 16.1): blanco 3★, violeta 4★, dorado 5★.</summary>
        public string Vapor => Rarity == 5 ? "dorado" : Rarity == 4 ? "violeta" : "blanco";
    }

    /// <summary>Resultado de la parte aleatoria de una tirada, sin elegir bruja. Lo usan las Termas y la simulación.</summary>
    public struct RarityRoll
    {
        public int Rarity;
        public int PityAt;
        /// <summary>Solo en banners limitados: la 5★ fue la destacada (por 75 % o por garantía).</summary>
        public bool Featured;
        public bool LostFeatured;
    }

    /// <summary>
    /// Gacha de las Termas (GDD 12.2 y 16). 0.6 % de 5★, pity suave desde la tirada 60 (+7 % por tirada), pity duro en 70;
    /// 4★ 6 % con garantía cada 10; destacada 75/25 con garantía; contador compartido por tipo de banner que no expira.
    /// Cero duplicados: una bruja que ya tienes no puede salir. El azar corre en el núcleo con semilla; en la Fase 3 la
    /// misma lógica corre en el servidor.
    /// </summary>
    public sealed class Termas
    {
        private readonly MetaGame _g;

        public Termas(MetaGame game) { _g = game; }

        private GachaRules Rules => _g.Meta.Gacha;
        private GachaState State => _g.Profile.Gacha;

        public GachaPity Pity(BannerDef banner)
        {
            if (!State.Pity.TryGetValue(banner.PityKey, out var p)) State.Pity[banner.PityKey] = p = new GachaPity();
            return p;
        }

        public GachaPity Pity(string bannerId) => Pity(Rules.Banner(bannerId));

        /// <summary>El termómetro (GDD 16.1): 0 a 1 hasta el pity duro; hierve desde el pity suave.</summary>
        public double Thermometer(string bannerId) => Math.Min(1.0, Pity(bannerId).Since5 / (double)Rules.HardPity);

        public bool Boiling(string bannerId) => Pity(bannerId).Since5 + 1 >= Rules.SoftPityStart;

        public bool SelectorAvailable => State.StandardPulls >= Rules.SelectorAt && !State.SelectorUsed;

        public int StarterPullsLeft(BannerDef b) => b.Type == BannerType.Inicio ? Math.Max(0, b.MaxPulls - State.StarterPulls) : int.MaxValue;

        public string CanPull(string bannerId, int count)
        {
            var b = Rules.Banner(bannerId);
            if (b == null) return "Banner desconocido.";
            if (count < 1) return "Cantidad inválida.";
            if (b.Type == BannerType.Inicio && count > StarterPullsLeft(b)) return $"Al banner de inicio le quedan {StarterPullsLeft(b)} tiradas.";
            if (b.Type == BannerType.Inicio && !b.Choices.Contains(State.StarterChoice)) return "Elige tu Regente del banner de inicio.";
            long cost = (long)Rules.CostPerPull * count;
            if (_g.Ledger.Balance(Res.SalesDeBano) < cost) return $"Faltan Sales de Baño ({_g.Ledger.Balance(Res.SalesDeBano):N0}/{cost:N0}).";
            return "";
        }

        public OpResult SetStarterChoice(string witchId)
        {
            var b = Rules.Banners.Find(x => x.Type == BannerType.Inicio);
            if (b == null || !b.Choices.Contains(witchId)) return OpResult.Fail("Esa Regente no está en el banner de inicio.");
            if (State.StarterFiveStar) return OpResult.Fail("Ya obtuviste la Regente del banner de inicio.");
            State.StarterChoice = witchId;
            return OpResult.Success(_g.Meta.Witch(witchId).Name);
        }

        public OpResult Pull(string bannerId, int count, out List<PullResult> results)
        {
            results = new List<PullResult>();
            string why = CanPull(bannerId, count);
            if (why.Length > 0) return OpResult.Fail(why);
            var b = Rules.Banner(bannerId);
            _g.Ledger.Apply(Res.SalesDeBano, -(long)Rules.CostPerPull * count, "tirada", $"{b.Name} ×{count}");
            for (int i = 0; i < count; i++) results.Add(PullOne(b));
            return OpResult.Success($"{count} tiradas");
        }

        private PullResult PullOne(BannerDef b)
        {
            var pity = Pity(b);
            bool forceFive = b.Type == BannerType.Inicio && !State.StarterFiveStar && State.StarterPulls + 1 >= b.MaxPulls;
            var roll = Roll(Rules, pity, _g.Rng, b.Type == BannerType.Limitado && !_g.Profile.Owns(b.Featured), forceFive);
            if (b.Type == BannerType.Inicio) State.StarterPulls++;
            if (b.Type == BannerType.Estandar) State.StandardPulls++;

            var result = new PullResult { Rarity = roll.Rarity, PityAt = roll.PityAt, Featured = roll.Featured, LostFeatured = roll.LostFeatured };
            string witch = null;
            if (roll.Rarity == 5)
            {
                if (b.Type == BannerType.Inicio)
                {
                    witch = !_g.Profile.Owns(State.StarterChoice) ? State.StarterChoice : PickUnowned(b.Choices);
                    State.StarterFiveStar = true;
                }
                else if (b.Type == BannerType.Limitado && roll.Featured) witch = b.Featured;
                else witch = PickUnowned(StandardFiveStars());
            }
            else witch = PickUnowned(PoolOf(roll.Rarity));

            if (witch != null)
            {
                _g.AddWitch(witch);
                result.Kind = "bruja";
                result.WitchId = witch;
            }
            else if (roll.Rarity >= 4)
            {
                // GDD 12.2: si ya tienes a todas las brujas posibles, el resultado se convierte en Polvo Astral.
                result.Kind = "polvo";
                result.Dust = roll.Rarity == 5 ? Rules.Dust5 : Rules.Dust4;
                _g.Ledger.Apply(Res.PolvoAstral, result.Dust, "gacha.polvo", b.Name);
            }
            else
            {
                // GDD 12.2: con las 3★ completas, recetas de armas y Sal Cruda. Nada del gacha sube grados.
                result.Kind = "receta";
                result.RecipeId = PickUnknownRecipe();
                if (result.RecipeId != null) _g.Profile.Recipes.Add(result.RecipeId);
                result.SaltPlanet = Planets.All[_g.Rng.NextInt(Planets.All.Length)];
                result.SaltAmount = Rules.ThreeStarSalt;
                _g.Ledger.Apply(Res.SalCruda(result.SaltPlanet.Value), result.SaltAmount, "gacha.sal", b.Name);
            }

            State.History.Add(new GachaHistoryEntry
            {
                Seq = State.NextSeq++, Day = _g.Profile.Day, Banner = b.Id, Rarity = result.Rarity, Kind = result.Kind,
                Id = result.WitchId ?? result.RecipeId ?? "", Amount = result.Dust + result.SaltAmount, Pity = result.PityAt,
                Featured = result.Featured, LostFeatured = result.LostFeatured,
            });
            if (State.History.Count > Rules.HistoryMax) State.History.RemoveRange(0, State.History.Count - Rules.HistoryMax);
            return result;
        }

        /// <summary>
        /// La parte aleatoria de una tirada: rareza y, en banners limitados, si la 5★ es la destacada.
        /// Actualiza el contador. Es la misma función que usa la simulación de un millón de tiradas.
        /// </summary>
        public static RarityRoll Roll(GachaRules rules, GachaPity pity, IRandomSource rng, bool featuredBanner, bool forceFive = false)
        {
            pity.Since5++;
            pity.Since4++;
            pity.Total++;
            var roll = new RarityRoll { PityAt = pity.Since5 };
            double u = rng.NextDouble();
            double p5 = rules.Rate5(pity.Since5);
            if (forceFive || u < p5)
            {
                roll.Rarity = 5;
                pity.Since5 = 0;
                pity.Since4 = 0;
                if (featuredBanner)
                {
                    bool win = pity.Guaranteed || rng.NextDouble() < rules.FeaturedRate;
                    roll.Featured = win;
                    roll.LostFeatured = !win;
                    pity.Guaranteed = !win;
                }
                return roll;
            }
            if (pity.Since4 >= rules.Guarantee4Every || u < p5 + rules.Base4)
            {
                roll.Rarity = 4;
                pity.Since4 = 0;
                return roll;
            }
            roll.Rarity = 3;
            return roll;
        }

        // ------------------------------------------------------------------ Pools con exclusión (GDD 12.2)

        public List<string> StandardFiveStars()
        {
            var list = new List<string>();
            foreach (var id in _g.Meta.RosterOrder)
            {
                var w = _g.Meta.Roster[id];
                if (w.Rarity == 5 && !w.Limited) list.Add(id);
            }
            return list;
        }

        public List<string> PoolOf(int rarity)
        {
            var list = new List<string>();
            foreach (var id in _g.Meta.RosterOrder) if (_g.Meta.Roster[id].Rarity == rarity) list.Add(id);
            return list;
        }

        /// <summary>Una bruja del pool que todavía no tienes; null si ya las tienes todas.</summary>
        private string PickUnowned(List<string> pool)
        {
            var candidates = new List<string>();
            foreach (var id in pool) if (!_g.Profile.Owns(id)) candidates.Add(id);
            return candidates.Count == 0 ? null : candidates[_g.Rng.NextInt(candidates.Count)];
        }

        private string PickUnknownRecipe()
        {
            var candidates = new List<string>();
            foreach (var id in _g.Meta.RecipeOrder) if (!_g.Profile.Recipes.Contains(id)) candidates.Add(id);
            return candidates.Count == 0 ? null : candidates[_g.Rng.NextInt(candidates.Count)];
        }

        // ------------------------------------------------------------------ Selector del estándar (GDD 16.2)

        public OpResult ClaimSelector(string witchId)
        {
            if (!SelectorAvailable) return OpResult.Fail($"El selector se abre a las {Rules.SelectorAt} tiradas del estándar ({State.StandardPulls}).");
            if (!StandardFiveStars().Contains(witchId)) return OpResult.Fail("Solo Regentes del banner estándar.");
            if (_g.Profile.Owns(witchId)) return OpResult.Fail("Ya la tienes: no existen los duplicados.");
            _g.AddWitch(witchId);
            State.SelectorUsed = true;
            State.History.Add(new GachaHistoryEntry
            {
                Seq = State.NextSeq++, Day = _g.Profile.Day, Banner = "selector", Rarity = 5, Kind = "bruja", Id = witchId,
            });
            return OpResult.Success(_g.Meta.Witch(witchId).Name);
        }
    }

    /// <summary>Simulación de la curva del gacha (GDD 16.2: "los promedios salen de simular nuestra curva").</summary>
    public sealed class GachaSimulation
    {
        public long Pulls;
        public long FiveStars;
        public long FourStars;
        public long Featured;
        public long LostFeatured;
        public int MaxPullsForFive;
        public int MaxPullsForFeatured;
        public int MaxGapFourStar;

        public double AveragePerFive => FiveStars == 0 ? 0 : Pulls / (double)FiveStars;
        public double AveragePerFeatured => Featured == 0 ? 0 : Pulls / (double)Featured;
        public double ConsolidatedRate => Pulls == 0 ? 0 : FiveStars / (double)Pulls;
        public double FourStarRate => Pulls == 0 ? 0 : FourStars / (double)Pulls;

        public static GachaSimulation Run(GachaRules rules, long pulls, ulong seed)
        {
            var sim = new GachaSimulation();
            var rng = new SeededRandom(seed);
            var pity = new GachaPity();
            int sinceFeatured = 0, since4 = 0;
            for (long i = 0; i < pulls; i++)
            {
                var roll = Termas.Roll(rules, pity, rng, featuredBanner: true);
                sim.Pulls++;
                sinceFeatured++;
                since4++;
                if (roll.Rarity >= 4)
                {
                    sim.MaxGapFourStar = Math.Max(sim.MaxGapFourStar, since4);
                    since4 = 0;
                }
                if (roll.Rarity == 4) sim.FourStars++;
                if (roll.Rarity != 5) continue;
                sim.FiveStars++;
                sim.MaxPullsForFive = Math.Max(sim.MaxPullsForFive, roll.PityAt);
                if (roll.LostFeatured) sim.LostFeatured++;
                if (roll.Featured)
                {
                    sim.Featured++;
                    sim.MaxPullsForFeatured = Math.Max(sim.MaxPullsForFeatured, sinceFeatured);
                    sinceFeatured = 0;
                }
            }
            return sim;
        }
    }
}
