using System.Collections.Generic;
using SalYAlmas.Core.Data;
using SalYAlmas.Core.Magic;

namespace SalYAlmas.Core.Combat
{
    public enum Side { Ally, Enemy }

    public abstract class Unit
    {
        public int Id { get; internal set; }
        public string Key { get; internal set; }
        public string Name { get; internal set; }
        public Side Side { get; internal set; }
        /// <summary>Posición en su bando (0 = primera). Sirve para desempatar.</summary>
        public int Slot { get; internal set; }
        public int Level { get; internal set; }
        public int MaxHp { get; internal set; }
        public int Hp { get; internal set; }
        public double BaseAtk { get; internal set; }
        public double BaseSpeed { get; internal set; }

        /// <summary>Distancia que le falta recorrer en la línea de turnos (empieza en 10 000). AV = Distance / Velocidad.</summary>
        public double Distance { get; internal set; }

        /// <summary>Avance o retraso pedido mientras la unidad actúa; se aplica cuando termina su turno.</summary>
        internal double PendingDistanceDelta;

        internal readonly List<Status> StatusList = new List<Status>();
        public IReadOnlyList<Status> Statuses => StatusList;

        public bool IsAlive => Hp > 0;

        public bool Has(StatusKind kind)
        {
            foreach (var s in StatusList) if (s.Kind == kind) return true;
            return false;
        }

        public Status Find(StatusKind kind)
        {
            foreach (var s in StatusList) if (s.Kind == kind) return s;
            return null;
        }

        public double Sum(StatusKind kind)
        {
            double total = 0;
            foreach (var s in StatusList) if (s.Kind == kind) total += s.Value;
            return total;
        }

        public double ShieldAmount => Sum(StatusKind.Shield);
    }

    public sealed class WitchUnit : Unit
    {
        public WitchDef Def { get; internal set; }
        public bool IsGuest { get; internal set; }
        public int Salinity { get; internal set; }
        public int Anima { get; internal set; }
        public Grade Grade { get; internal set; }
        public Via Via { get; internal set; }
        public bool VislumbreUsed { get; internal set; }
        public bool ReviveUsed { get; internal set; }
        public int FrameShift { get; internal set; }
        /// <summary>Arma, progreso y talentos de la Fase 2 (null = la bruja del JSON).</summary>
        public WitchLoadout Loadout { get; internal set; }
        /// <summary>Ataque básico con el patrón de golpes de su arma (null = el del kit).</summary>
        public MoveDef BasicOverride { get; internal set; }
        public MoveDef BasicMove => BasicOverride ?? Def.Basic;

        public Planet Dominant => Via == Via.Mayor ? Def.Primary : Def.Secondary;
        public Planet Recessive => Via == Via.Mayor ? Def.Secondary : Def.Primary;

        public bool InEcstasy => Has(StatusKind.Ecstasy);
        public bool IsDesalada => Has(StatusKind.Desalada);
        public bool InVislumbre => Has(StatusKind.Vislumbre);

        /// <summary>Grado efectivo: el siguiente mientras dura el Vislumbre.</summary>
        public Grade EffectiveGrade => InVislumbre ? PurityMath.Next(Grade) : Grade;

        /// <summary>Puede controlarla el jugador (ni en Éxtasis ni muerta).</summary>
        public bool IsControllable => IsAlive && !InEcstasy;

        /// <summary>Puede lanzar magia: Hechizo, Autohechizo, Vislumbre, Cadena o Ascensión.</summary>
        public bool CanCastMagic => IsControllable && !IsDesalada;
    }

    public sealed class EnemyUnit : Unit
    {
        public EnemyDef Def { get; internal set; }
        public double Toughness { get; internal set; }
        public double MaxToughness { get; internal set; }
        public bool Broken { get; internal set; }
        public int PatternIndex { get; internal set; }
        public bool InPhase2 { get; internal set; }
        public EnemySkillDef ChargedSkill { get; internal set; }
        public bool IsCharging => ChargedSkill != null;
        public bool IsBoss => Def.IsBoss;

        public IReadOnlyList<string> CurrentPattern => InPhase2 && Def.Phase2 != null ? Def.Phase2.Pattern : Def.Pattern;

        /// <summary>Siguiente habilidad que usará (para mostrar intención en la interfaz).</summary>
        public EnemySkillDef NextSkill
        {
            get
            {
                if (ChargedSkill != null) return ChargedSkill;
                var pattern = CurrentPattern;
                return Def.Skill(pattern[PatternIndex % pattern.Count]);
            }
        }

        public bool IsWeakTo(Planet p) => Def.Weaknesses.Contains(p);
    }
}
