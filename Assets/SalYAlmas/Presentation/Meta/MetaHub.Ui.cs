using System;

namespace SalYAlmas.Presentation
{
    /// <summary>Enganche con la capa nueva de interfaz (Presentation/Ui): lo que esa capa necesita para abrir pantallas y guardar.</summary>
    public sealed partial class MetaHub
    {
        /// <summary>Si responde true para la pantalla actual, el hub no dibuja nada: manda la capa uGUI.</summary>
        public Func<Screen, bool> ExternalScreen;

        /// <summary>Si responde true, la capa uGUI pone el fondo, la barra superior y la navegación; el hub solo dibuja su contenido.</summary>
        public Func<Screen, bool> ExternalChrome;

        /// <summary>Abre la ficha completa de una bruja.</summary>
        public void OpenWitch(string id)
        {
            _witch = id;
            _trialResult = null;
            Go(Screen.Witch);
        }

        public void SaveNow() => Save();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public void DebugHome() { Current = Screen.Home; }
#endif
    }
}
