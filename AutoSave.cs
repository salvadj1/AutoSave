using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Fougerite;
using UnityEngine;
using RustPP;

using Fougerite.Events;
using UnityEngine.Events;
using System.Timers;

namespace AutoSave
{
    public class AutoSave : Fougerite.Module
    {
        public override string Name { get { return "AutoSave"; } }
        public override string Author { get { return "Salva/juli"; } }
        public override string Description { get { return "AutoSave"; } }
        public override Version Version { get { return new Version("1.3"); } }
        public static string version { get { return "1.3"; } }
        
        private string red = "[color #FF0000]";
        private string blue = "[color #81F7F3]";
        private string green = "[color #82FA58]";
        private string yellow = "[color #F4FA58]";
        private string orange = "[color #FF8000]";

        public static int count = 0;

        public Mono MonoClass;
        public GameObject monoClassLoad;

        public static BackgroundWorker saveA;
        
        public override void Initialize()
        {     
            Hooks.OnServerInit += OnServerInit;
            Hooks.OnConsoleReceived += OnConsoleReceived;

            monoClassLoad = new GameObject();
            MonoClass = monoClassLoad.AddComponent<Mono>();
            UnityEngine.Object.DontDestroyOnLoad(monoClassLoad);
        }
        public override void DeInitialize()
        {
            Hooks.OnServerInit -= OnServerInit;
            Hooks.OnConsoleReceived -= OnConsoleReceived;
            saveA.DoWork -= new DoWorkEventHandler(GuardarMapaA);

            if (monoClassLoad != null) UnityEngine.Object.DestroyImmediate(monoClassLoad);
        }
        public void OnServerInit()
        {
            ConsoleSystem.Run("save.autosavetime 999999999", false); //disable Native Server Save
        }
        public void OnConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            if (arg.Class == "autosave" && arg.Function == "status" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                ConsoleSystem.LogError("AutoSave is Bussy?: " + saveA.IsBusy.ToString());
            }
        }
        public static void ContarObjectos()
        {
            if (Util.GetUtil().CurrentWorkingThreadID != Util.GetUtil().MainThreadID)
            {
                Loom.QueueOnMainThread(() =>
                {
                    ContarObjectos();
                });
                return;
            }
            try
            {
                count = World.GetWorld().Entities.Count;
            }
            catch (Exception ex)
            {
                Logger.Log("Error Autosave" + ex.ToString());
            }
            return;
        }
        public static void GuardarMapaA(object sender, DoWorkEventArgs e)
        {
            var StartTime = DateTime.Now;

            ConsoleSystem.Run("save.all", false);

            var EndTime = DateTime.Now;
            TimeSpan DifTime = EndTime.Subtract(StartTime);

            Logger.Log("[AutoSave v." + version + "] " + count + " Object(s). Took " + DifTime.Seconds.ToString() + " seconds save them in the Background " + "(total ms." + DifTime.Milliseconds.ToString() + ")");
            ConsoleSystem.Print("[AutoSave v." + version + "] " + count + " Object(s). Took " + DifTime.Seconds.ToString() + " seconds save them in the Background " + "(total ms." + DifTime.Milliseconds.ToString() + ")");
            Server.GetServer().BroadcastFrom("[AutoSave v." + version + "] ", count + " Object(s). Took " + DifTime.Seconds.ToString() + " seconds save them in the Background");

            saveA.CancelAsync();
        }     
    }
}
