using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace SalYAlmas.Presentation
{
    /// <summary>
    /// Recorrido de pantallas para revisión visual: con el argumento "-recorrido carpeta", visita cada pantalla del hub,
    /// guarda una captura y cierra el juego. Solo funciona en compilaciones de desarrollo y en el Editor.
    /// </summary>
    public sealed class ScreenWalk : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public MetaHub Hub;
        public string Folder;

        public static string Requested()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-recorrido") return args[i + 1];
            return null;
        }

        private IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            ScreenCapture.CaptureScreenshot(Path.Combine(Folder, name + ".png"));
            yield return new WaitForSecondsRealtime(0.4f);
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(Folder);
            Hub.DebugNewPlayer();
            yield return Shot("Creator");
            Hub.DebugPrologue();
            yield return Shot("Scene");
            Hub.DebugPullDemo();
            yield return new WaitForSecondsRealtime(2.4f);
            yield return Shot("Pull");
            Hub.DebugTestProfile();
            foreach (MetaHub.Screen s in Enum.GetValues(typeof(MetaHub.Screen)))
            {
                if (s == MetaHub.Screen.Creator || s == MetaHub.Screen.Scene) continue;
                Hub.DebugShow(s);
                yield return Shot(s.ToString());
            }
            Hub.DebugRun();
            yield return Shot("FresnoRun");
            Hub.DebugBattle();
            yield return new WaitForSecondsRealtime(2f);
            yield return Shot("Battle");
            Application.Quit();
        }
#endif
    }

    public sealed partial class MetaHub
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public void DebugTestProfile()
        {
            _pull = null;
            NewTestProfile();
        }

        public void DebugBattle()
        {
            F.EnterNode(F.AvailableNodes()[0].Lane);
            StartBattle?.Invoke(F.PrepareBattle(), b => F.ResolveBattle(b), "");
        }

        public void DebugShow(Screen s)
        {
            _witch = "marisal";
            Current = s;
        }

        public void DebugRun()
        {
            F.StartRun(Party(), -1, 7);
            F.ChooseCurses(new string[0]);
            Current = Screen.Fresno;
        }

        public void DebugNewPlayer()
        {
            _store.Delete(Slot);
            NewPlayer();
            _creatorName = "Noa";
        }

        public void DebugPrologue()
        {
            Game.Story.Create("Noa", "Elle", _meta.World.Story.Species[0], "aries");
            Game.Story.StartMission("vado", "prologo");
            Game.Story.Continue();
            Current = Screen.Scene;
        }

        public void DebugPullDemo()
        {
            Game.Story.LeaveMission();
            Game.Termas.SetStarterChoice("solenne");
            Current = Screen.Summon;
            Pull("inicio", 1);
        }
#endif
    }
}
