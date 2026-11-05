using System;
using System.Collections.Generic;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Gacha;
using SalYAlmas.Core.Meta;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.World
{
    // ------------------------------------------------------------------ Reloj y analítica

    /// <summary>El tiempo real entra por aquí; el núcleo nunca lo lee directo.</summary>
    public interface IClock
    {
        /// <summary>Días desde el 1 de enero de 2026.</summary>
        int Day { get; }
    }

    public sealed class FixedClock : IClock
    {
        public int Day { get; set; }
        public FixedClock(int day) { Day = day; }
    }

    public static class AnalyticsEvents
    {
        public const string TutorialStep = "tutorial_paso";
        public const string FirstChain = "primera_cadena";
        public const string FirstFiveStar = "primera_5";
        public const string FresnoRunEnd = "fresno_run";
        public const string Pull = "tirada";
        public const string Purchase = "compra";
        public const string Refund = "reembolso";
        public const string StoryRegionDone = "region_terminada";
        public const string StoryEclipse = "eclipse";
    }

    public interface IAnalytics
    {
        void Track(string name, string detail = "");
    }

    /// <summary>Implementación local: guarda los eventos en memoria para pruebas y la pantalla de desarrollo.</summary>
    public sealed class LocalAnalytics : IAnalytics
    {
        public readonly List<KeyValuePair<string, string>> Events = new List<KeyValuePair<string, string>>();
        public void Track(string name, string detail = "") => Events.Add(new KeyValuePair<string, string>(name, detail ?? ""));
    }

    // ------------------------------------------------------------------ Configuración remota

    public interface IRemoteConfig
    {
        bool PaidGachaAllowed(string country);
        bool BannerEnabled(string bannerId);
    }

    /// <summary>Valores de los datos locales. En producción apunta a Remote Config sin tocar el juego.</summary>
    public sealed class LocalRemoteConfig : IRemoteConfig
    {
        private readonly ShopRules _shop;
        public readonly HashSet<string> DisabledBanners = new HashSet<string>();
        public LocalRemoteConfig(ShopRules shop) { _shop = shop; }
        public bool PaidGachaAllowed(string country) => !_shop.PaidGachaBlocked.Contains(country ?? "");
        public bool BannerEnabled(string bannerId) => !DisabledBanners.Contains(bannerId);
    }

    // ------------------------------------------------------------------ Compras y servidor

    /// <summary>Lo que la tienda de la plataforma entrega al pagar.</summary>
    public sealed class Receipt
    {
        public string Id;
        public string Product;
        public string Signature;
    }

    public interface IStorePlatform
    {
        /// <summary>Cobra en la plataforma y devuelve el recibo. Null si la compra se canceló.</summary>
        Receipt Charge(string productId);
    }

    /// <summary>Tienda en modo de pruebas: firma recibos que solo el backend falso sabe validar.</summary>
    public sealed class FakeStorePlatform : IStorePlatform
    {
        public const string Secret = "tienda-de-pruebas";
        private int _next = 1;
        public bool Cancel;

        public Receipt Charge(string productId)
        {
            if (Cancel) return null;
            string id = "r" + _next++;
            return new Receipt { Id = id, Product = productId, Signature = FakeBackend.Sign(id, productId) };
        }
    }

    /// <summary>
    /// El servidor autoritativo detrás de una interfaz (GDD 21.2). Valida recibos y registra compras y tiradas.
    /// La implementación de Unity Gaming Services (decisión D-04) se conecta aquí cuando exista la cuenta.
    /// </summary>
    public interface IBackend
    {
        bool ValidateReceipt(Receipt receipt, out string error);
        void RecordPurchase(Receipt receipt, double usd, int day);
        bool Refund(string receiptId);
        void RecordPull(GachaHistoryEntry entry);
        IReadOnlyList<ServerPurchase> Purchases { get; }
        IReadOnlyList<GachaHistoryEntry> Pulls { get; }
    }

    public sealed class ServerPurchase
    {
        public string Receipt;
        public string Product;
        public double Usd;
        public int Day;
        public bool Refunded;
    }

    /// <summary>Backend falso en memoria, con las mismas reglas que el real: un recibo vale una sola vez y debe venir firmado.</summary>
    public sealed class FakeBackend : IBackend
    {
        private readonly List<ServerPurchase> _purchases = new List<ServerPurchase>();
        private readonly List<GachaHistoryEntry> _pulls = new List<GachaHistoryEntry>();

        public IReadOnlyList<ServerPurchase> Purchases => _purchases;
        public IReadOnlyList<GachaHistoryEntry> Pulls => _pulls;

        public static string Sign(string id, string product)
        {
            uint h = 2166136261;
            foreach (char c in FakeStorePlatform.Secret + "|" + id + "|" + product) { h ^= c; h *= 16777619; }
            return h.ToString("x8");
        }

        public bool ValidateReceipt(Receipt receipt, out string error)
        {
            error = "";
            if (receipt == null || receipt.Signature != Sign(receipt.Id, receipt.Product)) { error = "Recibo inválido."; return false; }
            if (_purchases.Exists(p => p.Receipt == receipt.Id)) { error = "Ese recibo ya se usó."; return false; }
            return true;
        }

        public void RecordPurchase(Receipt receipt, double usd, int day) =>
            _purchases.Add(new ServerPurchase { Receipt = receipt.Id, Product = receipt.Product, Usd = usd, Day = day });

        public bool Refund(string receiptId)
        {
            var p = _purchases.Find(x => x.Receipt == receiptId);
            if (p == null || p.Refunded) return false;
            p.Refunded = true;
            return true;
        }

        public void RecordPull(GachaHistoryEntry entry) => _pulls.Add(entry);
    }

    /// <summary>
    /// La tienda (GDD 16.4): Perlas, Grimorio, Diezmo de Sal y cosméticos. Solo se vende lo que la sección 16.4 permite:
    /// una prueba comprueba que ningún producto entrega recursos de progreso.
    /// </summary>
    public sealed class StoreService
    {
        private readonly MetaGame _g;
        public IStorePlatform Platform;
        public IBackend Backend;

        public StoreService(MetaGame game)
        {
            _g = game;
            Platform = new FakeStorePlatform();
            Backend = new FakeBackend();
        }

        private ShopRules Shop => _g.Meta.World.Shop;
        private StoreState S => _g.Profile.Store;

        // ------------------------------------------------------------------ Compras con dinero

        public double SpentThisMonth()
        {
            double total = 0;
            int month = _g.Profile.Day / 30;
            foreach (var p in S.Purchases) if (!p.Refunded && p.Day / 30 == month) total += p.Usd;
            return total;
        }

        /// <summary>Por qué no se puede comprar ("" si se puede). El límite de gasto es opcional (GDD 16.6).</summary>
        public string CanBuy(ProductDef product)
        {
            if (product == null) return "Producto desconocido.";
            if (S.SpendLimitUsd > 0 && SpentThisMonth() + product.PriceUsd > S.SpendLimitUsd)
                return $"Pasarías tu límite mensual de {S.SpendLimitUsd:0.00} USD.";
            if (product.Kind == ProductKind.Grimorio && S.GrimorioPremium) return "Ya tienes el Grimorio premium.";
            return "";
        }

        public OpResult Buy(string productId)
        {
            var product = Shop.Product(productId);
            string why = CanBuy(product);
            if (why.Length > 0) return OpResult.Fail(why);
            var receipt = Platform.Charge(productId);
            if (receipt == null) return OpResult.Fail("Compra cancelada.");
            return Redeem(receipt);
        }

        /// <summary>El servidor valida el recibo y solo entonces se entrega el producto.</summary>
        public OpResult Redeem(Receipt receipt)
        {
            if (!Backend.ValidateReceipt(receipt, out var error)) return OpResult.Fail(error);
            var product = Shop.Product(receipt.Product);
            if (product == null) return OpResult.Fail("Producto desconocido.");
            Backend.RecordPurchase(receipt, product.PriceUsd, _g.Profile.Day);
            long perlas = product.Perlas + product.Bonus;
            long payDebt = Math.Min(S.PerlasDebt, perlas);
            S.PerlasDebt -= payDebt;
            if (perlas - payDebt > 0) _g.Ledger.Apply(Res.Perlas, perlas - payDebt, "compra", product.Id);
            switch (product.Kind)
            {
                case ProductKind.Grimorio:
                    S.GrimorioPremium = true;
                    break;
                case ProductKind.Diezmo:
                    S.DiezmoUntilDay = Math.Max(S.DiezmoUntilDay, _g.Profile.Day) + product.Days;
                    S.DiezmoDaily = product.DailySales;
                    break;
            }
            S.Purchases.Add(new PurchaseRecord { Receipt = receipt.Id, Product = product.Id, Usd = product.PriceUsd, Day = _g.Profile.Day, Perlas = perlas });
            _g.Analytics.Track(AnalyticsEvents.Purchase, product.Id);
            return OpResult.Success(product.Name);
        }

        /// <summary>Un reembolso retira lo entregado; si las Perlas ya se gastaron, el saldo queda como deuda que descuenta las siguientes.</summary>
        public OpResult Refund(string receiptId)
        {
            var record = S.Purchases.Find(p => p.Receipt == receiptId);
            if (record == null || record.Refunded) return OpResult.Fail("Compra desconocida.");
            if (!Backend.Refund(receiptId)) return OpResult.Fail("El servidor rechazó el reembolso.");
            record.Refunded = true;
            long take = Math.Min(record.Perlas, _g.Ledger.Balance(Res.Perlas));
            if (take > 0) _g.Ledger.Apply(Res.Perlas, -take, "reembolso", receiptId);
            S.PerlasDebt += record.Perlas - take;
            var product = Shop.Product(record.Product);
            if (product?.Kind == ProductKind.Grimorio) S.GrimorioPremium = false;
            if (product?.Kind == ProductKind.Diezmo) S.DiezmoUntilDay = _g.Profile.Day;
            _g.Analytics.Track(AnalyticsEvents.Refund, record.Product);
            return OpResult.Success();
        }

        public OpResult SetSpendLimit(double usd)
        {
            if (usd < 0) return OpResult.Fail("Límite inválido.");
            S.SpendLimitUsd = usd;
            return OpResult.Success();
        }

        // ------------------------------------------------------------------ Conversión y moneda comprada (HUECOS C-25)

        /// <summary>Perlas a Sales de Baño, 1 a 1 (GDD 11.1). Las Sales compradas se cuentan aparte para la versión de Bélgica.</summary>
        public OpResult ConvertPerlas(long amount)
        {
            if (amount <= 0) return OpResult.Fail("Cantidad inválida.");
            if (_g.Ledger.Balance(Res.Perlas) < amount) return OpResult.Fail("Faltan Perlas.");
            _g.Ledger.Apply(Res.Perlas, -amount, "conversion", "");
            _g.Ledger.Apply(Res.SalesDeBano, amount, "perlas", "");
            S.PurchasedSales += amount;
            return OpResult.Success();
        }

        public long EarnedSales => Math.Max(0, _g.Ledger.Balance(Res.SalesDeBano) - S.PurchasedSales);

        // ------------------------------------------------------------------ Diezmo y Grimorio

        public bool DiezmoActive => _g.Profile.Day < S.DiezmoUntilDay;

        public OpResult ClaimDiezmo()
        {
            if (!DiezmoActive) return OpResult.Fail("No tienes un Diezmo de Sal activo.");
            if (S.DiezmoClaimedDay == _g.Profile.Day) return OpResult.Fail("Ya lo cobraste hoy.");
            S.DiezmoClaimedDay = _g.Profile.Day;
            _g.Ledger.Apply(Res.SalesDeBano, S.DiezmoDaily, "pase", "Diezmo de Sal");
            return OpResult.Success($"+{S.DiezmoDaily} Sales de Baño");
        }

        public int GrimorioLevel => 1 + S.GrimorioXp / Shop.PassXpPerLevel;

        public void AddGrimorioXp(int xp) => S.GrimorioXp += xp;

        /// <summary>Cobra un nivel del Grimorio: el carril gratis siempre, el premium si lo compraste.</summary>
        public OpResult ClaimTier(int level)
        {
            var tier = Shop.Pass.Find(t => t.Level == level);
            if (tier == null) return OpResult.Fail("Nivel desconocido.");
            if (GrimorioLevel < level) return OpResult.Fail($"Llega al nivel {level}.");
            bool any = false;
            if (!S.PassFreeClaimed.Contains(level))
            {
                S.PassFreeClaimed.Add(level);
                foreach (var kv in tier.Free) _g.Ledger.Apply(kv.Key, kv.Value, "pase", "Grimorio " + level);
                any = true;
            }
            if (S.GrimorioPremium && !S.PassPremiumClaimed.Contains(level))
            {
                S.PassPremiumClaimed.Add(level);
                foreach (var kv in tier.Premium) _g.Ledger.Apply(kv.Key, kv.Value, "pase", "Grimorio premium " + level);
                if (tier.PremiumCosmetic.Length > 0) S.Cosmetics.Add(tier.PremiumCosmetic);
                any = true;
            }
            return any ? OpResult.Success("Cobrado") : OpResult.Fail("Ya cobraste este nivel.");
        }

        // ------------------------------------------------------------------ Cosméticos (GDD 16.5)

        public bool OwnsCosmetic(string id) => S.Cosmetics.Contains(id);

        public OpResult BuyCosmetic(string id, bool withDust = false)
        {
            var c = Shop.Cosmetic(id);
            if (c == null) return OpResult.Fail("Cosmético desconocido.");
            if (OwnsCosmetic(id)) return OpResult.Fail("Ya lo tienes.");
            string resource = withDust ? Res.PolvoAstral : Res.Perlas;
            long price = withDust ? c.Polvo : c.Perlas;
            if (price <= 0) return OpResult.Fail(withDust ? "No se vende por Polvo Astral." : "No se vende por Perlas.");
            var cost = new Bundle().Add(resource, price);
            if (!_g.Ledger.TrySpend(cost, withDust ? "tienda_polvo" : "cosmeticos", c.Id)) return OpResult.Fail(_g.Ledger.Missing(cost));
            S.Cosmetics.Add(id);
            return OpResult.Success(c.Name);
        }

        /// <summary>Equipa por ranura: "manantial", "marco", "color_sal" o "atuendo:marisal". Sin id, vuelve al de serie.</summary>
        public OpResult Equip(string id)
        {
            var c = Shop.Cosmetic(id);
            if (c == null) return OpResult.Fail("Cosmético desconocido.");
            if (!OwnsCosmetic(id)) return OpResult.Fail("No lo tienes.");
            S.Equipped[SlotOf(c)] = id;
            return OpResult.Success(c.Name);
        }

        public void Unequip(CosmeticDef c) => S.Equipped.Remove(SlotOf(c));

        public static string SlotOf(CosmeticDef c) => c.Witch.Length > 0 ? c.Kind + ":" + c.Witch : c.Kind;

        public string Equipped(string slot) => S.Equipped.TryGetValue(slot, out var v) ? v : "";
    }

    // ------------------------------------------------------------------ Gacha detrás de una interfaz

    /// <summary>La presentación pide tiradas aquí: la animación recibe el resultado y nunca lo decide.</summary>
    public interface IGachaService
    {
        string CanPull(string bannerId, int count);
        OpResult Pull(string bannerId, int count, out List<PullResult> results);
    }

    /// <summary>
    /// Implementación local con la lógica del núcleo. Aplica el cumplimiento (país, banner apagado), registra cada tirada
    /// en el backend y marca la primera 5★ para la analítica.
    /// </summary>
    public sealed class LocalGachaService : IGachaService
    {
        private readonly MetaGame _g;
        public IRemoteConfig Remote;

        public LocalGachaService(MetaGame game)
        {
            _g = game;
            Remote = new LocalRemoteConfig(game.Meta.World.Shop);
        }

        public bool PaidAllowed => Remote.PaidGachaAllowed(_g.Profile.Settings.Country);

        public string CanPull(string bannerId, int count)
        {
            if (!Remote.BannerEnabled(bannerId)) return "Este banner no está disponible.";
            string why = _g.Termas.CanPull(bannerId, count);
            if (why.Length > 0) return why;
            // Bélgica: solo con moneda ganada jugando (GDD 2, 16.6 y HUECOS C-25).
            long cost = (long)_g.Meta.Gacha.CostPerPull * count;
            if (!PaidAllowed && _g.Store.EarnedSales < cost) return "En tu país las tiradas solo usan Sales de Baño ganadas jugando.";
            return "";
        }

        public OpResult Pull(string bannerId, int count, out List<PullResult> results)
        {
            results = new List<PullResult>();
            string why = CanPull(bannerId, count);
            if (why.Length > 0) return OpResult.Fail(why);
            int before = _g.Profile.Gacha.History.Count;
            long seqBefore = _g.Profile.Gacha.NextSeq;
            var r = _g.Termas.Pull(bannerId, count, out results);
            if (!r.Ok) return r;

            // Primero se gastan las Sales ganadas: lo comprado solo baja cuando el saldo ya no alcanza.
            var s = _g.Profile.Store;
            s.PurchasedSales = Math.Min(s.PurchasedSales, _g.Ledger.Balance(Res.SalesDeBano));

            foreach (var h in _g.Profile.Gacha.History)
                if (h.Seq >= seqBefore) _g.Store.Backend.RecordPull(h);
            foreach (var p in results)
            {
                _g.Analytics.Track(AnalyticsEvents.Pull, bannerId + ":" + p.Rarity);
                if (p.Rarity == 5 && !s.FirstFiveDone)
                {
                    s.FirstFiveDone = true;
                    _g.Analytics.Track(AnalyticsEvents.FirstFiveStar, p.WitchId ?? "");
                }
            }
            _g.Store.AddGrimorioXp(count * 10);
            return r;
        }

        // ------------------------------------------------------------------ Cumplimiento (GDD 16.6)

        /// <summary>Filas de la página de probabilidades de un banner: etiqueta y valor.</summary>
        public List<KeyValuePair<string, string>> OddsPage(string bannerId)
        {
            var g = _g.Meta.Gacha;
            var b = g.Banner(bannerId);
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Costo por tirada", g.CostPerPull + " Sales de Baño"),
                new KeyValuePair<string, string>("5★ base", (g.Base5 * 100).ToString("0.0") + " %"),
                new KeyValuePair<string, string>("Pity suave", $"Desde la tirada {g.SoftPityStart}, +{g.SoftPityStep * 100:0} % por tirada"),
                new KeyValuePair<string, string>("Pity duro", g.HardPity + " tiradas"),
                new KeyValuePair<string, string>("Promedio por 5★", "53 tiradas (tasa consolidada de 1.9 %)"),
                new KeyValuePair<string, string>("4★", $"{g.Base4 * 100:0} %, garantizada cada {g.Guarantee4Every} tiradas"),
                new KeyValuePair<string, string>("Duplicados", "No existen: una bruja que ya tienes no puede salir"),
                new KeyValuePair<string, string>("Tu contador", _g.Termas.Pity(b).Since5 + " tiradas sin 5★. Nunca expira"),
            };
            if (b.Type == BannerType.Limitado)
            {
                list.Add(new KeyValuePair<string, string>("Destacada", $"{g.FeaturedRate * 100:0}/{(1 - g.FeaturedRate) * 100:0}; si pierdes, la siguiente 5★ es la destacada"));
                list.Add(new KeyValuePair<string, string>("Promedio por la destacada", "66 tiradas; peor caso, 140"));
            }
            if (b.Type == BannerType.Inicio)
                list.Add(new KeyValuePair<string, string>("Garantía", $"Una 5★ a elegir a más tardar en la tirada {b.MaxPulls}. Si sale antes, las tiradas que quedan ya no dan otra"));
            if (b.Type == BannerType.Estandar)
                list.Add(new KeyValuePair<string, string>("Selector", $"A las {g.SelectorAt} tiradas eliges una 5★"));
            return list;
        }

        /// <summary>Historial de un año (GDD 16.6), de la tirada más nueva a la más vieja.</summary>
        public List<GachaHistoryEntry> YearHistory()
        {
            var list = new List<GachaHistoryEntry>();
            var h = _g.Profile.Gacha.History;
            for (int i = h.Count - 1; i >= 0; i--) if (_g.Profile.Day - h[i].Day <= 365) list.Add(h[i]);
            return list;
        }
    }

    // ------------------------------------------------------------------ Termas (propuesta de meta-juego, sección 10)

    /// <summary>La base sobre la Tortuga: reposo, tocar, conversar y baño. Es zona segura y ninguna recompensa da estadísticas.</summary>
    public sealed class TermasBase
    {
        private readonly MetaGame _g;
        public TermasBase(MetaGame game) { _g = game; }

        private TermasRules R => _g.Meta.World.Termas;
        private TermasState S => _g.Profile.TermasBase;

        public int TrustOf(string id) => S.Trust.TryGetValue(id, out var v) ? v : 0;
        public int TrustLevel(string id) => Math.Min(R.TrustMaxLevel, 1 + TrustOf(id) / R.TrustPerLevel);

        /// <summary>Hasta cuatro brujas en reposo; rotan cada día. Determinista con el día.</summary>
        public List<string> Resting()
        {
            var owned = new List<string>();
            foreach (var id in _g.Meta.RosterOrder) if (_g.Profile.Owns(id)) owned.Add(id);
            var list = new List<string>();
            if (S.Favorite.Length > 0 && owned.Contains(S.Favorite)) { list.Add(S.Favorite); owned.Remove(S.Favorite); }
            int day = Math.Max(0, _g.Profile.Day);
            for (int i = 0; i < owned.Count && list.Count < 4; i++) list.Add(owned[(day + i) % owned.Count]);
            return list;
        }

        public OpResult SetFavorite(string id)
        {
            if (!_g.Profile.Owns(id)) return OpResult.Fail("No la tienes.");
            S.Favorite = id;
            return OpResult.Success(_g.Meta.Witch(id).Name);
        }

        public bool Talked(string id) => S.TalkedDay.TryGetValue(id, out var d) && d == _g.Profile.Day;

        /// <summary>Una charla corta por bruja al día. La frase cambia con el día, no con el azar.</summary>
        public OpResult Talk(string id)
        {
            if (!_g.Profile.Owns(id)) return OpResult.Fail("No la tienes.");
            var lines = R.Talk.TryGetValue(id, out var own) && own.Count > 0 ? own : R.GenericTalk;
            string line = lines.Count > 0 ? lines[Math.Abs(_g.Profile.Day + id.Length) % lines.Count] : "";
            if (!Talked(id))
            {
                S.TalkedDay[id] = _g.Profile.Day;
                S.Trust[id] = TrustOf(id) + R.TalkTrust;
                _g.Store.AddGrimorioXp(5);
            }
            return OpResult.Success(line);
        }

        public string Touch(string id, int touches)
        {
            var list = R.GenericTouch;
            return list.Count == 0 ? "" : list[Math.Abs(touches + id.Length) % list.Count];
        }

        public string ShakeOf(string id) => R.Shake.TryGetValue(id, out var s) ? s : R.DefaultShake;
        public string PullLineOf(string id) => R.PullLine.TryGetValue(id, out var s) ? s : "";

        public bool CanBathe => _g.Profile.Day - S.LastBathDay >= R.BathCooldownDays || S.LastBathDay < 0;

        /// <summary>Bañarse baja el Paralaje: el agua hace olvidar (GDD 6.5).</summary>
        public OpResult Bathe()
        {
            if (!CanBathe) return OpResult.Fail("El agua ya te hizo olvidar hoy. Vuelve mañana.");
            S.LastBathDay = _g.Profile.Day;
            _g.Story.AddParalaje(R.BathParalaje);
            return OpResult.Success($"Paralaje {_g.Profile.Calculista.Paralaje}");
        }
    }
}
