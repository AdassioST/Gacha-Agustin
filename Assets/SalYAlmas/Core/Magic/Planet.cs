using System;

namespace SalYAlmas.Core.Magic
{
    /// <summary>Las siete sales planetarias (GDD 7.1).</summary>
    public enum Planet { Sol, Luna, Marte, Mercurio, Venus, Jupiter, Saturno }

    /// <summary>Secta helenística (GDD 7.1). Mercurio es variable.</summary>
    public enum Sect { Diurna, Nocturna, Variable }

    public static class Planets
    {
        public static readonly Planet[] All =
        {
            Planet.Sol, Planet.Luna, Planet.Marte, Planet.Mercurio, Planet.Venus, Planet.Jupiter, Planet.Saturno
        };

        public static Sect SectOf(Planet p)
        {
            switch (p)
            {
                case Planet.Sol:
                case Planet.Jupiter:
                case Planet.Saturno:
                    return Sect.Diurna;
                case Planet.Luna:
                case Planet.Marte:
                case Planet.Venus:
                    return Sect.Nocturna;
                default:
                    return Sect.Variable;
            }
        }

        /// <summary>Oposiciones de regentes (GDD 7.3): Sol o Luna con Saturno, Marte con Venus, Mercurio con Júpiter.</summary>
        public static bool AreOpposed(Planet a, Planet b)
        {
            return IsPair(a, b, Planet.Sol, Planet.Saturno)
                || IsPair(a, b, Planet.Luna, Planet.Saturno)
                || IsPair(a, b, Planet.Marte, Planet.Venus)
                || IsPair(a, b, Planet.Mercurio, Planet.Jupiter);
        }

        private static bool IsPair(Planet a, Planet b, Planet x, Planet y) => (a == x && b == y) || (a == y && b == x);

        /// <summary>Nombre visible en español.</summary>
        public static string DisplayName(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return "Sol";
                case Planet.Luna: return "Luna";
                case Planet.Marte: return "Marte";
                case Planet.Mercurio: return "Mercurio";
                case Planet.Venus: return "Venus";
                case Planet.Jupiter: return "Júpiter";
                default: return "Saturno";
            }
        }

        /// <summary>Nombre de la sal (GDD 7.1).</summary>
        public static string SaltName(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return "Sal Áurea";
                case Planet.Luna: return "Sal Argenta";
                case Planet.Marte: return "Sal Férrea";
                case Planet.Mercurio: return "Sal de Azogue";
                case Planet.Venus: return "Sal Cúprica";
                case Planet.Jupiter: return "Sal Estánnea";
                default: return "Sal Plúmbea";
            }
        }

        /// <summary>Identificador usado en los JSON.</summary>
        public static string Id(Planet p)
        {
            switch (p)
            {
                case Planet.Sol: return "sol";
                case Planet.Luna: return "luna";
                case Planet.Marte: return "marte";
                case Planet.Mercurio: return "mercurio";
                case Planet.Venus: return "venus";
                case Planet.Jupiter: return "jupiter";
                default: return "saturno";
            }
        }

        public static bool TryParse(string id, out Planet planet)
        {
            foreach (var p in All)
            {
                if (string.Equals(Id(p), id, StringComparison.OrdinalIgnoreCase))
                {
                    planet = p;
                    return true;
                }
            }
            planet = Planet.Sol;
            return false;
        }

        /// <summary>Planeta que rige cada día de la semana (GDD 7.7). 0 = domingo.</summary>
        public static Planet RulerOfWeekday(int dayOfWeek)
        {
            switch (((dayOfWeek % 7) + 7) % 7)
            {
                case 0: return Planet.Sol;
                case 1: return Planet.Luna;
                case 2: return Planet.Marte;
                case 3: return Planet.Mercurio;
                case 4: return Planet.Jupiter;
                case 5: return Planet.Venus;
                default: return Planet.Saturno;
            }
        }
    }
}
