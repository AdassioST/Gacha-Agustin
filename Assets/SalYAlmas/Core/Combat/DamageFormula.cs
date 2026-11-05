using System;

namespace SalYAlmas.Core.Combat
{
    /// <summary>Entradas de la fórmula de daño (GDD 9.8), ya resueltas para un golpe y un planeta.</summary>
    public struct DamageInput
    {
        /// <summary>ATQ del atacante con sus mejoras de ataque.</summary>
        public double Atk;
        /// <summary>M_hab: multiplicador de la habilidad para este golpe.</summary>
        public double SkillMultiplier;
        /// <summary>ΣB: suma de bonos de daño.</summary>
        public double BonusSum;
        /// <summary>M_p: multiplicador de pureza de este planeta (GDD 7.2).</summary>
        public double PurityMultiplier;
        /// <summary>C: 1 o 1 + daño crítico.</summary>
        public double CritMultiplier;
        /// <summary>N_a.</summary>
        public int AttackerLevel;
        /// <summary>N_e.</summary>
        public int TargetLevel;
        /// <summary>R_DEF: reducción de defensa del objetivo.</summary>
        public double DefenseReduction;
        /// <summary>RES_e del objetivo contra este planeta.</summary>
        public double TargetRes;
        /// <summary>PEN del atacante.</summary>
        public double Penetration;
        /// <summary>V: vulnerabilidad por Quiebre (1.1 o 1).</summary>
        public double Vulnerability;
        /// <summary>S: Spark (1.5 o 1).</summary>
        public double Spark;
        /// <summary>A: bono de Aspecto.</summary>
        public double Aspect;

        public static DamageInput Neutral(double atk, double skillMultiplier, int attackerLevel, int targetLevel) => new DamageInput
        {
            Atk = atk,
            SkillMultiplier = skillMultiplier,
            BonusSum = 0,
            PurityMultiplier = 1,
            CritMultiplier = 1,
            AttackerLevel = attackerLevel,
            TargetLevel = targetLevel,
            DefenseReduction = 0,
            TargetRes = 0,
            Penetration = 0,
            Vulnerability = 1,
            Spark = 1,
            Aspect = 1,
        };
    }

    /// <summary>
    /// Daño = ATQ · M_hab · (1 + ΣB) · M_p · C · F_DEF · F_RES · V · S · A
    /// F_DEF = (N_a + 20) / ((N_e + 20)(1 − R_DEF) + N_a + 20)
    /// F_RES = 1 − (RES_e − PEN)
    /// </summary>
    public static class DamageFormula
    {
        public static double DefenseFactor(int attackerLevel, int targetLevel, double defenseReduction)
        {
            double rDef = Math.Max(0.0, Math.Min(1.0, defenseReduction));
            double na = attackerLevel + 20.0;
            double ne = targetLevel + 20.0;
            return na / (ne * (1.0 - rDef) + na);
        }

        public static double ResFactor(double targetRes, double penetration) => 1.0 - (targetRes - penetration);

        public static double Compute(DamageInput i)
        {
            double bonus = Math.Max(0.0, 1.0 + i.BonusSum);
            double raw = i.Atk * i.SkillMultiplier * bonus * i.PurityMultiplier * i.CritMultiplier
                         * DefenseFactor(i.AttackerLevel, i.TargetLevel, i.DefenseReduction)
                         * ResFactor(i.TargetRes, i.Penetration)
                         * i.Vulnerability * i.Spark * i.Aspect;
            return Math.Max(0.0, raw);
        }

        /// <summary>Redondeo del daño final de un golpe: al entero más cercano, medios hacia arriba.</summary>
        public static int Round(double damage) => (int)Math.Round(damage, MidpointRounding.AwayFromZero);
    }
}
