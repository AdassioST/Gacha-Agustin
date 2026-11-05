using System;

namespace SalYAlmas.Core.Magic
{
    /// <summary>Grados de Destilación (GDD 12.3).</summary>
    public enum Grade { D, C, B, A, S, SS }

    /// <summary>Ruta de purificación elegida en el grado B (GDD 7.2).</summary>
    public enum Via { Mayor, Menor }

    /// <summary>Reparto de daño entre los dos planetas de una mezcla (GDD 7.2).</summary>
    public static class PurityMath
    {
        /// <summary>D_dom = D · p · (1 + slope·(p − pivot)). Devuelve el factor que multiplica a D.</summary>
        public static double DominantShare(double p, double slope, double pivot) => p * (1.0 + slope * (p - pivot));

        /// <summary>D_sec = D · (1 − p).</summary>
        public static double SecondaryShare(double p) => 1.0 - p;

        /// <summary>Multiplicador del planeta dominante: 1 + slope·(p − pivot). Vale 1.0 en p = 0.7 y 1.24 en p = 0.999.</summary>
        public static double DominantMultiplier(double p, double slope, double pivot) => 1.0 + slope * (p - pivot);

        public static Grade Next(Grade g) => g == Grade.SS ? Grade.SS : (Grade)((int)g + 1);

        public static bool TryParseGrade(string s, out Grade grade)
        {
            foreach (Grade g in Enum.GetValues(typeof(Grade)))
            {
                if (string.Equals(g.ToString(), s, StringComparison.OrdinalIgnoreCase))
                {
                    grade = g;
                    return true;
                }
            }
            grade = Grade.D;
            return false;
        }
    }
}
