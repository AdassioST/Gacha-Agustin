using System.Collections.Generic;
using System.Text;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    public sealed class BeltSlot
    {
        public string PotionId;
        public int Charges;

        public BeltSlot() { }
        public BeltSlot(string potionId, int charges) { PotionId = potionId; Charges = charges; }
    }

    /// <summary>Todo lo que define un combate antes de empezar. Con la misma configuración y las mismas órdenes, el combate se repite igual.</summary>
    public sealed class BattleSetup
    {
        /// <summary>De una a cuatro brujas en el campo, en orden de posición.</summary>
        public List<string> Team = new List<string>();
        /// <summary>Invitada: aporta su Regencia y puede cerrar una Cadena por combate. Puede ser null.</summary>
        public string GuestId;
        public string EncounterId;
        /// <summary>Cinturón de hasta tres ranuras (GDD 14).</summary>
        public List<BeltSlot> Belt = new List<BeltSlot>();
        /// <summary>Sal Negra disponible en el combate. Null = valor de las reglas.</summary>
        public int? BlackSalt;
        public ulong Seed = 1;
        /// <summary>Día planetario (GDD 7.7). Null = sin bono. Es una entrada para mantener el determinismo.</summary>
        public Planet? PlanetaryDay;
        /// <summary>Vía elegida en el grado B (por bruja). Si falta, la del JSON.</summary>
        public Dictionary<string, Via> ViaOverrides = new Dictionary<string, Via>();
        /// <summary>Grado por bruja. Si falta, el del JSON (B).</summary>
        public Dictionary<string, Grade> GradeOverrides = new Dictionary<string, Grade>();
        /// <summary>Desfase de frames por bruja (Fase 2: armas Rítmico y Pausado).</summary>
        public Dictionary<string, int> FrameShifts = new Dictionary<string, int>();

        public BattleSetup Clone()
        {
            var c = (BattleSetup)MemberwiseClone();
            c.Team = new List<string>(Team);
            c.Belt = new List<BeltSlot>();
            foreach (var b in Belt) c.Belt.Add(new BeltSlot(b.PotionId, b.Charges));
            c.ViaOverrides = new Dictionary<string, Via>(ViaOverrides);
            c.GradeOverrides = new Dictionary<string, Grade>(GradeOverrides);
            c.FrameShifts = new Dictionary<string, int>(FrameShifts);
            return c;
        }
    }

    public enum CommandType { Basic, Spell, SelfCast, Vislumbre, Potion, OpenChain, Ascension, Advance }

    /// <summary>Una orden del jugador (o de la IA automática). El combate solo cambia con órdenes.</summary>
    public sealed class BattleCommand
    {
        public CommandType Type;
        public int ActorId = -1;
        public int TargetId = -1;
        public int Slot = -1;
        public int Level;
        public List<int> Order = new List<int>();
        public bool WithGuest;

        public static BattleCommand Basic(int actor, int target) => new BattleCommand { Type = CommandType.Basic, ActorId = actor, TargetId = target };
        public static BattleCommand Spell(int actor, int target) => new BattleCommand { Type = CommandType.Spell, ActorId = actor, TargetId = target };
        public static BattleCommand SelfCast(int actor) => new BattleCommand { Type = CommandType.SelfCast, ActorId = actor };
        public static BattleCommand Vislumbre(int actor) => new BattleCommand { Type = CommandType.Vislumbre, ActorId = actor };
        public static BattleCommand Potion(int actor, int slot, int target) => new BattleCommand { Type = CommandType.Potion, ActorId = actor, Slot = slot, TargetId = target };
        public static BattleCommand Ascension(int caster, int level, int target) => new BattleCommand { Type = CommandType.Ascension, ActorId = caster, Level = level, TargetId = target };
        public static BattleCommand Advance() => new BattleCommand { Type = CommandType.Advance };

        public static BattleCommand Chain(int opener, IEnumerable<int> order, bool withGuest, int target) =>
            new BattleCommand { Type = CommandType.OpenChain, ActorId = opener, Order = new List<int>(order), WithGuest = withGuest, TargetId = target };

        public override string ToString()
        {
            var sb = new StringBuilder(Type.ToString());
            sb.Append(" actor=").Append(ActorId).Append(" target=").Append(TargetId);
            if (Type == CommandType.Potion) sb.Append(" slot=").Append(Slot);
            if (Type == CommandType.Ascension) sb.Append(" level=").Append(Level);
            if (Type == CommandType.OpenChain) sb.Append(" order=").Append(string.Join(",", Order)).Append(" guest=").Append(WithGuest);
            return sb.ToString();
        }
    }

    public readonly struct CommandResult
    {
        public bool Ok { get; }
        public string Error { get; }

        private CommandResult(bool ok, string error) { Ok = ok; Error = error; }

        public static CommandResult Success => new CommandResult(true, "");
        public static CommandResult Fail(string error) => new CommandResult(false, error);
        public override string ToString() => Ok ? "Ok" : Error;
    }

    /// <summary>Qué espera el combate.</summary>
    public enum DecisionKind
    {
        None,
        /// <summary>Turno de una bruja: el jugador elige.</summary>
        AllyTurn,
        /// <summary>Turno de un enemigo: se resuelve con Advance (antes se pueden lanzar Ascensiones).</summary>
        EnemyTurn,
        /// <summary>Turno de una bruja en Éxtasis: lo controla su Ascendente; se resuelve con Advance.</summary>
        AutoTurn,
        BattleOver,
    }

    public readonly struct TurnOrderEntry
    {
        public Unit Unit { get; }
        public double AvFromNow { get; }
        public TurnOrderEntry(Unit unit, double av) { Unit = unit; AvFromNow = av; }
    }
}
