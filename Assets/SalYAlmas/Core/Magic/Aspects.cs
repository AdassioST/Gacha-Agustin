namespace SalYAlmas.Core.Magic
{
    /// <summary>Aspectos en orden de prioridad (GDD 7.3: "si se cumplen varias condiciones, gana la de arriba").</summary>
    public enum Aspect { Conjuncion, Oposicion, Sextil, Trigono, Cuadratura }

    /// <summary>Reacciones Híbridas que dispara una Oposición (GDD 7.3).</summary>
    public enum HybridReaction { None, Eclipse, HeladaNegra, EspinasRojas, Tempestad }

    public static class AspectRules
    {
        /// <summary>
        /// Resuelve el Aspecto entre los planetas dominantes de dos brujas seguidas.
        /// Prioridad: Conjunción, Oposición, Sextil, Trígono, Cuadratura.
        /// </summary>
        public static Aspect Resolve(Planet first, Planet second)
        {
            if (first == second) return Aspect.Conjuncion;
            if (Planets.AreOpposed(first, second)) return Aspect.Oposicion;
            if (first == Planet.Mercurio || second == Planet.Mercurio) return Aspect.Sextil;
            // Mercurio (secta variable) ya quedó resuelto arriba: nunca llega aquí.
            return Planets.SectOf(first) == Planets.SectOf(second) ? Aspect.Trigono : Aspect.Cuadratura;
        }

        public static HybridReaction ReactionFor(Planet a, Planet b)
        {
            if (!Planets.AreOpposed(a, b)) return HybridReaction.None;
            bool Has(Planet x) => a == x || b == x;
            if (Has(Planet.Saturno) && Has(Planet.Sol)) return HybridReaction.Eclipse;
            if (Has(Planet.Saturno) && Has(Planet.Luna)) return HybridReaction.HeladaNegra;
            if (Has(Planet.Marte) && Has(Planet.Venus)) return HybridReaction.EspinasRojas;
            return HybridReaction.Tempestad;
        }

        public static string DisplayName(Aspect a)
        {
            switch (a)
            {
                case Aspect.Conjuncion: return "Conjunción";
                case Aspect.Oposicion: return "Oposición";
                case Aspect.Sextil: return "Sextil";
                case Aspect.Trigono: return "Trígono";
                default: return "Cuadratura";
            }
        }

        public static string DisplayName(HybridReaction r)
        {
            switch (r)
            {
                case HybridReaction.Eclipse: return "Eclipse";
                case HybridReaction.HeladaNegra: return "Helada Negra";
                case HybridReaction.EspinasRojas: return "Espinas Rojas";
                case HybridReaction.Tempestad: return "Tempestad";
                case HybridReaction.None: return "Sin carta";
                default: return "";
            }
        }

        /// <summary>Texto corto del efecto, para la Partitura y el registro.</summary>
        public static string EffectSummary(Aspect a, HybridReaction r)
        {
            switch (a)
            {
                case Aspect.Conjuncion: return "+25 % de daño a este eslabón";
                case Aspect.Oposicion:
                    switch (r)
                    {
                        case HybridReaction.Eclipse: return "quita 2 mejoras y retrasa 30 %";
                        case HybridReaction.HeladaNegra: return "pierde su turno (jefe: −50 % de Velocidad)";
                        case HybridReaction.EspinasRojas: return "sangrado que cura al equipo";
                        case HybridReaction.Tempestad: return "rayo a todos con 2 golpes extra";
                        default: return "Reacción Híbrida";
                    }
                case Aspect.Sextil: return "Adelanta 15 % a la segunda bruja";
                case Aspect.Trigono: return "+1 Grano y cura 5 %";
                default: return "+40 % de daño a la Firmeza";
            }
        }
    }
}
