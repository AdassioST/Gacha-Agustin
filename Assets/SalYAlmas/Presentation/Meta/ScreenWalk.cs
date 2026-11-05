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
            // La Odisea: Aries terminada, el mapa con Cáncer y Capricornio abiertos, encargos, una sección y una Expedición.
            Hub.DebugOdyssey();
            Hub.DebugShow(MetaHub.Screen.Story);
            yield return Shot("Odisea");
            Hub.DebugShow(MetaHub.Screen.Quests);
            yield return Shot("Encargos");
            Hub.DebugOdysseyScene();
            yield return Shot("OdiseaEscena");
            Hub.DebugExpedition();
            yield return Shot("Expedicion");
            Hub.DebugOdysseyBattle();
            yield return new WaitForSecondsRealtime(2f);
            yield return Shot("OdiseaCombate");
            Hub.DebugEndBattle();
            yield return new WaitForSecondsRealtime(0.5f);
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

        public void DebugOdyssey()
        {
            var s = _meta.World.Story;
            var c = Game.Profile.Calculista;
            if (!c.Created) Game.Story.Create("Noa", "Elle", s.Species[0], "aries");
            foreach (var id in new[] { "vado", "aries" })
            {
                var region = s.Region(id);
                foreach (var m in region.Missions) if (!c.MissionsDone.Contains(m.Id)) c.MissionsDone.Add(m.Id);
                if (!c.RegionsDone.Contains(id)) c.RegionsDone.Add(id);
                if (!region.Prologue && !c.RegionsVisited.Contains(id)) { c.RegionsVisited.Add(id); c.Arrival[id] = 0; }
                foreach (var z in region.Zones) if (!z.IsEx && !c.ZonesDone.Contains(z.Id)) c.ZonesDone.Add(z.Id);
            }
            Game.Report("cadena");
            Game.Report("seccion", 4);
            _region = "cancer";
            _zone = "";
        }

        public void DebugOdysseyScene()
        {
            Game.Story.StartMission("cancer", "can_1");
            Game.Story.Continue();
            Current = Screen.Scene;
        }

        public void DebugExpedition()
        {
            Game.Story.StartExpedition("aries", "aries_cielo", 0);
            Current = Screen.Expedition;
        }

        public void DebugOdysseyBattle()
        {
            StartBattle?.Invoke(Game.Story.PrepareExpeditionBattle(Party(), 7), b => Game.Story.ResolveExpeditionBattle(b), "");
        }

        /// <summary>Termina el combate de prueba sin jugarlo, para seguir el recorrido.</summary>
        public System.Action EndBattleRequested;
        public void DebugEndBattle() => EndBattleRequested?.Invoke();

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
