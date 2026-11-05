using System.Collections.Generic;

namespace SalYAlmas.Core.Gacha
{
    public enum BannerType { Inicio, Estandar, Limitado }

    public sealed class BannerDef
    {
        public string Id;
        public string Name;
        public BannerType Type;
        public string Featured;
        public int MaxPulls;
        public readonly List<string> Choices = new List<string>();
        public string Description = "";

        /// <summary>Contador de pity compartido entre banners del mismo tipo (GDD 16.2).</summary>
        public string PityKey => Type.ToString().ToLowerInvariant();
    }

    /// <summary>Tasas y reglas del gacha (GDD 16.2).</summary>
    public sealed class GachaRules
    {
        public int CostPerPull = 160;
        public double Base5 = 0.006;
        public int SoftPityStart = 60;
        public double SoftPityStep = 0.07;
        public int HardPity = 70;
        public double Base4 = 0.06;
        public int Guarantee4Every = 10;
        public double FeaturedRate = 0.75;
        public int SelectorAt = 300;
        public int HistoryMax = 2000;
        public int Dust5 = 40;
        public int Dust4 = 8;
        public int ThreeStarSalt = 3;
        public readonly List<BannerDef> Banners = new List<BannerDef>();

        public BannerDef Banner(string id) => Banners.Find(b => b.Id == id);

        /// <summary>
        /// Probabilidad de 5★ en la tirada número n desde la última 5★ (n empieza en 1). La tirada 60 ya lleva +7 %:
        /// con esa lectura el promedio es 52.9 tiradas y la tasa consolidada 1.89 %, los números del GDD.
        /// </summary>
        public double Rate5(int n)
        {
            if (n >= HardPity) return 1.0;
            if (n < SoftPityStart) return Base5;
            return System.Math.Min(1.0, Base5 + SoftPityStep * (n - SoftPityStart + 1));
        }
    }
}
