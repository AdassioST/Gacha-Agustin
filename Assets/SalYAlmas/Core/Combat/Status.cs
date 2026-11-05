using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    public enum StatusKind
    {
        AtkPct, DamagePct, SpeedPct, SpeedFlat, CritRate, ResAll, DefenseDown, DamageTaken,
        Shield,
        Burn, Bleed, Poison, Shock,
        Charm, Frozen, FeignDeath, InvertHealing, NextSpellDouble, FreeVislumbre, NextToughness,
        GrainBoost, Desalada, Ecstasy, Vislumbre,
    }

    /// <summary>
    /// Estado sobre una unidad. La duración se cuenta en turnos de quien lo lleva y baja al final de cada
    /// uno de sus turnos. TurnsLeft = −1 significa "hasta que se consuma" o permanente.
    /// </summary>
    public sealed class Status
    {
        public StatusKind Kind;
        public string Name = "";
        public int SourceId = -1;
        public double Value;
        public int TurnsLeft = -1;
        public bool IsBuff;
        public bool IsDebuff;
        public bool Dispellable = true;
        /// <summary>Aplicado durante el turno de quien lo lleva y ese turno no cuenta (Éxtasis, Desalada).</summary>
        public bool SkipFirstTick;

        // Daño por turno
        public Planet Planet;
        public double SnapshotAtk;
        public int AttackerLevel;
        /// <summary>Espinas Rojas: parte del daño que cura al equipo.</summary>
        public double HealShare;
        /// <summary>Electrocución: parte del daño que salta a los demás enemigos.</summary>
        public double JumpShare;

        public bool IsDot => Kind == StatusKind.Burn || Kind == StatusKind.Bleed || Kind == StatusKind.Poison || Kind == StatusKind.Shock;

        public Status Clone() => (Status)MemberwiseClone();

        public static StatusKind KindForStat(StatKind stat)
        {
            switch (stat)
            {
                case StatKind.Atk: return StatusKind.AtkPct;
                case StatKind.Damage: return StatusKind.DamagePct;
                case StatKind.Speed: return StatusKind.SpeedPct;
                case StatKind.SpeedFlat: return StatusKind.SpeedFlat;
                case StatKind.CritRate: return StatusKind.CritRate;
                case StatKind.Res: return StatusKind.ResAll;
                case StatKind.DefenseDown: return StatusKind.DefenseDown;
                default: return StatusKind.DamageTaken;
            }
        }

        /// <summary>Signo del valor: en penalizaciones casi todo resta, salvo DefenseDown y DamageTaken que suman.</summary>
        public static double SignedValue(StatKind stat, double value, bool isDebuff)
        {
            bool positiveIsBad = stat == StatKind.DefenseDown || stat == StatKind.DamageTaken;
            if (positiveIsBad) return isDebuff ? value : -value;
            return isDebuff ? -value : value;
        }

        public static StatusKind KindForDot(DotKind d)
        {
            switch (d)
            {
                case DotKind.Burn: return StatusKind.Burn;
                case DotKind.Bleed: return StatusKind.Bleed;
                case DotKind.Poison: return StatusKind.Poison;
                default: return StatusKind.Shock;
            }
        }

        public static Planet PlanetForDot(DotKind d)
        {
            switch (d)
            {
                case DotKind.Burn: return Planet.Sol;
                case DotKind.Bleed: return Planet.Marte;
                case DotKind.Poison: return Planet.Mercurio;
                default: return Planet.Jupiter;
            }
        }

        public static string DefaultName(StatusKind k)
        {
            switch (k)
            {
                case StatusKind.AtkPct: return "Ataque";
                case StatusKind.DamagePct: return "Daño";
                case StatusKind.SpeedPct: return "Velocidad";
                case StatusKind.SpeedFlat: return "Velocidad";
                case StatusKind.CritRate: return "Crítico";
                case StatusKind.ResAll: return "Resistencia";
                case StatusKind.DefenseDown: return "Defensa rota";
                case StatusKind.DamageTaken: return "Marca";
                case StatusKind.Shield: return "Escudo";
                case StatusKind.Burn: return "Quemadura";
                case StatusKind.Bleed: return "Sangrado";
                case StatusKind.Poison: return "Veneno";
                case StatusKind.Shock: return "Electrocución";
                case StatusKind.Charm: return "Encanto";
                case StatusKind.Frozen: return "Helada Negra";
                case StatusKind.FeignDeath: return "Muerte Fingida";
                case StatusKind.InvertHealing: return "Curas invertidas";
                case StatusKind.NextSpellDouble: return "Elixir de Sal Negra";
                case StatusKind.FreeVislumbre: return "Filtro de Pureza";
                case StatusKind.NextToughness: return "Vinagre de Marte";
                case StatusKind.GrainBoost: return "Excavación";
                case StatusKind.Desalada: return "Desalada";
                case StatusKind.Ecstasy: return "Éxtasis Abisal";
                default: return "Vislumbre";
            }
        }
    }
}
