using System.Collections.Generic;
using SalYAlmas.Core.Combat;
using SalYAlmas.Core.Save;

namespace SalYAlmas.Core.Fresno
{
    public enum RunPhase
    {
        /// <summary>Al entrar a una zona: elegir hasta cinco maldiciones.</summary>
        Curses,
        /// <summary>Elegir el siguiente nodo.</summary>
        ChooseNode,
        /// <summary>Hay un combate preparado en el nodo actual.</summary>
        Battle,
        /// <summary>Hay una decisión pendiente (Injerto, Mutación, evento, Santuario, Mercader o Caldero).</summary>
        Choice,
        /// <summary>La run terminó: falta decidir el Eco.</summary>
        EcoSave,
        Finished,
    }

    public enum ChoiceKind { None, Graft, Mutation, Event, Santuario, Mercader, Caldero }

    public enum RunOutcome { None, Victory, Defeat, Abandoned }

    /// <summary>Una decisión pendiente. Las opciones son ids (Injertos, Mutaciones, recetas) o índices de evento.</summary>
    public sealed class PendingChoice
    {
        public ChoiceKind Kind;
        public List<string> Options = new List<string>();
        /// <summary>Precio de cada opción (Mercader).</summary>
        public List<long> Prices = new List<long>();
        public string EventId = "";
        public string Note = "";
    }

    /// <summary>
    /// Estado de una run del Fresno. Se guarda con el perfil: la run se puede pausar en cualquier nodo (GDD 10.1).
    /// El mapa no se guarda: se vuelve a generar con la semilla y la Mancha.
    /// </summary>
    public sealed class RunState
    {
        public ulong Seed;
        public int RunNumber;
        public List<string> Team = new List<string>();
        public RunPhase Phase;
        public RunOutcome Outcome;
        public int Zone;
        /// <summary>Piso del nodo actual (0 a 9); −1 al entrar a una zona.</summary>
        public int Floor = -1;
        public int Lane = 1;
        public List<string> Path = new List<string>();

        /// <summary>Almitas sueltas: se pierden en la Mancha si cae el aquelarre antes de un Santuario (GDD 10.4).</summary>
        public long Loose;
        public long Banked;
        public long Earned;
        public long SaviaEarned;
        public List<string> Grafts = new List<string>();
        public List<string> Mutations = new List<string>();
        public List<TrainedMove> Trained = new List<TrainedMove>();
        public List<List<string>> Curses = new List<List<string>>();
        public Dictionary<string, double> RunMods = new Dictionary<string, double>();
        public List<string> RunModNames = new List<string>();
        public Dictionary<string, double> Hp = new Dictionary<string, double>();
        public Dictionary<string, int> Salinity = new Dictionary<string, int>();
        public List<BeltSlot> Belt = new List<BeltSlot>();
        /// <summary>Ingredientes de la run para los nodos de Caldero.</summary>
        public Dictionary<string, int> Pouch = new Dictionary<string, int>();
        public PendingChoice Pending = new PendingChoice();
        public int EcoSlotUsed = -1;
        public string EcoName = "";

        /// <summary>Mancha de la run anterior: dónde está y cuánto lleva. Se recupera al llegar a ese nodo.</summary>
        public ManchaState Mancha;
        public bool ManchaResolved;
        public long ManchaRecovered;
        public long SaviaFromMancha;

        public ulong Draws;
        public int CombatsWon;
        public List<string> Log = new List<string>();

        public string NodeId => Floor < 0 ? $"{Zone + 1}-entrada" : $"{Zone + 1}-{Floor + 1}-{Lane + 1}";
    }
}
