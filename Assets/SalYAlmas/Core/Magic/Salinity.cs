namespace SalYAlmas.Core.Magic
{
    /// <summary>Bandas de Salinidad (GDD 7.4).</summary>
    public enum SalinityBand { Estable, Salobre, Sobresalada, CristalizacionTurbia, ExtasisAbisal }

    public static class SalinityBands
    {
        public static SalinityBand FromValue(int value, Data.SalinityRules r)
        {
            if (value >= r.ExtasisAt) return SalinityBand.ExtasisAbisal;
            if (value >= r.CristalizacionAt) return SalinityBand.CristalizacionTurbia;
            if (value >= r.SobresaladaAt) return SalinityBand.Sobresalada;
            if (value >= r.SalobreAt) return SalinityBand.Salobre;
            return SalinityBand.Estable;
        }

        public static string DisplayName(SalinityBand b)
        {
            switch (b)
            {
                case SalinityBand.Estable: return "Estable";
                case SalinityBand.Salobre: return "Salobre";
                case SalinityBand.Sobresalada: return "Sobresalada";
                case SalinityBand.CristalizacionTurbia: return "Cristalización Turbia";
                default: return "Éxtasis Abisal";
            }
        }

        public static string EffectSummary(SalinityBand b)
        {
            switch (b)
            {
                case SalinityBand.Estable: return "Sin efecto";
                case SalinityBand.Salobre: return "+10 % de probabilidad de crítico";
                case SalinityBand.Sobresalada: return "+20 % de daño, −15 % de defensa";
                case SalinityBand.CristalizacionTurbia: return "+40 % de daño del Ascendente; −5 % de vida por turno";
                default: return "El Ascendente toma el control dos turnos (daño ×2)";
            }
        }
    }
}
