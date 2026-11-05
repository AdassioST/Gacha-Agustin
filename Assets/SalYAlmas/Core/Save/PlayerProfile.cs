using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Economy;
using SalYAlmas.Core.Fresno;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Save
{
    /// <summary>Progreso de una bruja obtenida (GDD 12). No existen los duplicados: hay a lo más una por id.</summary>
    public sealed class WitchProgress
    {
        public string Id;
        public int Level = 1;
        public Grade Grade = Grade.D;
        public Via Via = Via.Mayor;
        /// <summary>Ya eligió Vía al llegar a B.</summary>
        public bool ViaChosen;
        /// <summary>Cambios de Vía hechos después de elegirla (el primero es gratis).</summary>
        public int ViaChanges;
        /// <summary>0 Peregrina … 4 Domicilio (GDD 8.1).</summary>
        public int Dignity;
        /// <summary>Apertura, Enlace y Cierre, del 1 al 10 (GDD 12.6).</summary>
        public int[] ChainTalents = { 1, 1, 1 };
        /// <summary>Arma equipada (0 = ninguna).</summary>
        public int WeaponUid;
        public int TrialAttempts;
        public List<string> Memories = new List<string>();
        public int ObtainedDay;
    }

    /// <summary>Un arma fabricada. El prefijo es por ejemplar (se cambia en la reforja).</summary>
    public sealed class WeaponInstance
    {
        public int Uid;
        public string DefId;
        public string Prefix = "";
    }

    /// <summary>Contador de pity de un tipo de banner. Nunca expira (GDD 16.2).</summary>
    public sealed class GachaPity
    {
        public int Since5;
        public int Since4;
        /// <summary>Perdió el 75/25: la siguiente 5★ es la destacada.</summary>
        public bool Guaranteed;
        public int Total;
    }

    public sealed class GachaHistoryEntry
    {
        public long Seq;
        public int Day;
        public string Banner;
        public int Rarity;
        /// <summary>"bruja", "polvo" o "receta".</summary>
        public string Kind;
        public string Id;
        public long Amount;
        /// <summary>Tirada del contador de 5★ en que salió.</summary>
        public int Pity;
        public bool Featured;
        public bool LostFeatured;
    }

    public sealed class GachaState
    {
        public Dictionary<string, GachaPity> Pity = new Dictionary<string, GachaPity>();
        public int StandardPulls;
        public bool SelectorUsed;
        public int StarterPulls;
        public string StarterChoice = "";
        /// <summary>Ya salió la Regente del banner de inicio.</summary>
        public bool StarterFiveStar;
        public List<GachaHistoryEntry> History = new List<GachaHistoryEntry>();
        public long NextSeq = 1;
    }

    /// <summary>Un movimiento de Cadena entrenado en un Santuario (GDD 10.2).</summary>
    public sealed class TrainedMove
    {
        public string Witch;
        public ChainRole Role;

        public TrainedMove() { }
        public TrainedMove(string witch, ChainRole role) { Witch = witch; Role = role; }
    }

    /// <summary>Eco: tres Injertos y un movimiento entrenado guardados al terminar una run (GDD 10.5).</summary>
    public sealed class EcoSave
    {
        public string Name = "";
        public List<string> Grafts = new List<string>();
        public List<TrainedMove> Trained = new List<TrainedMove>();
        public int FromRun;
    }

    /// <summary>Almitas sueltas que quedaron donde cayó el aquelarre (GDD 10.4).</summary>
    public sealed class ManchaState
    {
        public int Zone;
        public int Floor;
        public int Lane;
        public long Almitas;
        public int FromRun;
    }

    public sealed class FresnoProfile
    {
        public Dictionary<string, int> Roots = new Dictionary<string, int>();
        public long OfrendaTotal;
        /// <summary>Último nivel de Ofrenda cuyos hitos ya se pagaron.</summary>
        public int OfrendaLevelPaid;
        /// <summary>Ranuras de Eco (null = vacía).</summary>
        public List<EcoSave> Eco = new List<EcoSave> { null, null, null };
        /// <summary>Zonas cuyo jefe se ha vencido alguna vez.</summary>
        public List<string> BossesDefeated = new List<string>();
        public ManchaState Mancha;
        public int RunsStarted;
        public int RunsFinished;
        public int FullClears;
        public int Week = -1;
        public long WeekBanked;
        public long WeekBathSalts;
        public RunState Run;
        /// <summary>Resumen de las runs terminadas, para el mapa y las pruebas de aceptación.</summary>
        public List<RunSummary> History = new List<RunSummary>();
    }

    public sealed class RunSummary
    {
        public int Run;
        public string Outcome;
        public string ReachedNode;
        public long AlmitasBanked;
        public long ManchaLeft;
        public long ManchaRecovered;
        public long SaviaFromMancha;
        public long SaviaEarned;
        public string EcoUsed = "";
        public List<string> Grafts = new List<string>();
    }

    /// <summary>La o el Calculista: la protagonista, su historia y lo que ha visto (GDD 6 y 13).</summary>
    public sealed class CalculistaState
    {
        public bool Created;
        public string Name = "";
        public string Pronouns = "";
        public string Species = "";
        public string Sign = "";
        public Dictionary<string, int> Stats = new Dictionary<string, int>();
        public string Sendero = "";
        public Dictionary<string, int> SenderoXp = new Dictionary<string, int>();
        public List<string> Marks = new List<string>();
        public List<string> Titles = new List<string>();
        public int Souls;
        public int Paralaje;
        public int Eclipses;
        public List<string> RegionsVisited = new List<string>();
        public List<string> RegionsDone = new List<string>();
        public List<string> MissionsDone = new List<string>();
        public Dictionary<string, int> Corruption = new Dictionary<string, int>();
        public string Mission = "";
        public string MissionRegion = "";
        public string Scene = "";
        /// <summary>La misión en curso es una Relectura: no vuelve a entregar recursos.</summary>
        public bool Reread;

        // Odisea (docs/propuestas/ODISEA.md)
        /// <summary>Regiones terminadas cuando llegaste a cada región: fija su nivel.</summary>
        public Dictionary<string, int> Arrival = new Dictionary<string, int>();
        public List<string> ZonesDone = new List<string>();
        /// <summary>Sección del combate en curso (0 = la primera).</summary>
        public int Section;
        /// <summary>Vida (fracción) y Salinidad que se arrastran dentro de la misión o la Expedición.</summary>
        public Dictionary<string, double> CarryHp = new Dictionary<string, double>();
        public Dictionary<string, int> CarrySalinity = new Dictionary<string, int>();
        /// <summary>Expedición en curso: región, zona y Marea elegida. Vacío si no hay.</summary>
        public string ExpRegion = "";
        public string ExpZone = "";
        public int ExpTide;
        /// <summary>La Expedición lleva su propia sección y su propio arrastre: la misión en pausa no se pierde.</summary>
        public int ExpSection;
        public Dictionary<string, double> ExpHp = new Dictionary<string, double>();
        public Dictionary<string, int> ExpSalinity = new Dictionary<string, int>();
        public int DailyDay = -1;
        public int DailySections;
        /// <summary>Botín de la última sección, para la pantalla.</summary>
        public List<string> LastLoot = new List<string>();
        public int SectionsWon;
        public int BossesWon;
    }

    public sealed class TermasState
    {
        public string Favorite = "";
        public Dictionary<string, int> Trust = new Dictionary<string, int>();
        public Dictionary<string, int> TalkedDay = new Dictionary<string, int>();
        public int LastBathDay = -1;
    }

    public sealed class PurchaseRecord
    {
        public string Receipt;
        public string Product;
        public double Usd;
        public int Day;
        public long Perlas;
        public bool Refunded;
    }

    public sealed class StoreState
    {
        public List<PurchaseRecord> Purchases = new List<PurchaseRecord>();
        public double SpendLimitUsd;
        /// <summary>Sales de Baño que salieron de Perlas compradas (HUECOS C-25).</summary>
        public long PurchasedSales;
        public long PerlasDebt;
        public bool GrimorioPremium;
        public int GrimorioXp;
        public List<int> PassFreeClaimed = new List<int>();
        public List<int> PassPremiumClaimed = new List<int>();
        public int DiezmoUntilDay;
        public int DiezmoDaily;
        public int DiezmoClaimedDay = -1;
        public HashSet<string> Cosmetics = new HashSet<string>();
        public Dictionary<string, string> Equipped = new Dictionary<string, string>();
        public bool FirstFiveDone;
        public bool FirstChainDone;
    }

    /// <summary>Ajustes y accesibilidad (GDD 23.3 y HUECOS S-17 y S-18).</summary>
    public sealed class SettingsState
    {
        public string Language = "es";
        public string Country = "MX";
        public bool ModestMode;
        public bool LargeText;
        public bool ColorSafe;
        public bool SkipCinematics;
        public bool ReduceFlashes;
        public bool BatterySaver;
        public bool HighQuality = true;
        public float MusicVolume = 0.7f;
    }

    /// <summary>
    /// Todo el estado persistente de una cuenta. Se guarda en JSON detrás de ISaveStore (local hoy; servidor en la Fase 3).
    /// </summary>
    public sealed class PlayerProfile
    {
        public const int CurrentVersion = 2;

        public CalculistaState Calculista = new CalculistaState();
        public TermasState TermasBase = new TermasState();
        public StoreState Store = new StoreState();
        public SettingsState Settings = new SettingsState();

        public int Version = CurrentVersion;
        public string Name = "Calculista";
        /// <summary>Día actual (días desde el 1 de enero de 2026). Es una entrada: el núcleo no lee el reloj.</summary>
        public int Day;
        public Ledger Ledger = new Ledger();
        public Dictionary<string, WitchProgress> Witches = new Dictionary<string, WitchProgress>();
        public List<WeaponInstance> Weapons = new List<WeaponInstance>();
        public int NextWeaponUid = 1;
        /// <summary>Equipo del home: se usa en la historia y en el Fresno. La primera es la favorita.</summary>
        public List<string> Party = new List<string>();
        /// <summary>Recetas de armas conocidas.</summary>
        public HashSet<string> Recipes = new HashSet<string>();
        /// <summary>Recetario de Salmueras descubiertas (GDD 14.1).</summary>
        public HashSet<string> Recetario = new HashSet<string>();
        /// <summary>Salmueras en el inventario (máximo cinco de cada tipo, GDD 14.3).</summary>
        public Dictionary<string, int> Potions = new Dictionary<string, int>();
        public HashSet<string> Stations = new HashSet<string>();
        public GachaState Gacha = new GachaState();
        public FresnoProfile Fresno = new FresnoProfile();
        /// <summary>Estado del azar del meta-juego (reforja, Salmuera Turbia, gacha local, semillas de runs).</summary>
        public ulong[] Rng = new ulong[4];
        public ulong Seed = 1;

        public bool Owns(string witchId) => Witches.ContainsKey(witchId);

        public WeaponInstance Weapon(int uid) => Weapons.Find(w => w.Uid == uid);

        public string EquippedBy(int uid)
        {
            foreach (var w in Witches.Values) if (w.WeaponUid == uid) return w.Id;
            return null;
        }
    }
}
