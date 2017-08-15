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


namespace AutoSave
{
    public class AutoSave_v101 : Fougerite.Module
    {
        public override string Name
        {
            get { return "AutoSave"; }
        }
        public override string Author
        {
            get { return "Salva/juli"; }
        }
        public override string Description
        {
            get { return "AutoSave"; }
        }
        public override Version Version
        {
            get { return new Version("1.0"); }
        }
        private string red = "[color #FF0000]";
        private string blue = "[color #81F7F3]";
        private string green = "[color #82FA58]";
        private string yellow = "[color #F4FA58]";
        private string orange = "[color #FF8000]";
        private static BackgroundWorker saveA = new BackgroundWorker();
        
        public override void Initialize()
        {     
               
            Hooks.OnServerInit += OnServerInit;
            Hooks.OnCommand += OnCommand;
            Hooks.OnConsoleReceived += OnConsoleReceived;
            saveA.DoWork += new DoWorkEventHandler(GuardarMapaA);
            saveA.WorkerSupportsCancellation = true;
        }
        public override void DeInitialize()
        {
                     
            Hooks.OnServerInit -= OnServerInit;
            Hooks.OnCommand -= OnCommand;
            Hooks.OnConsoleReceived -= OnConsoleReceived;
            saveA.DoWork -= new DoWorkEventHandler(GuardarMapaA);
        }
        public void OnServerInit()
        {
            Timer1(600000, null).Start();
            ConsoleSystem.Run("save.autosavetime 999999999", false);
            
        }
        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "autosave")
            {
                if (!player.Admin && !player.Moderator) return;
                player.MessageFrom("AutoSave", "AutoSave By " + Author + " " + Version.ToString());
                player.MessageFrom("AutoSave", "/autosave status - Know if Work in backGround is Bussy or no");
                player.MessageFrom("AutoSave", "/autosave reload - Reload and Relaunch Timers");

                if (args[0] == "status")
                {
                    player.MessageFrom("AutoSave","AutoSave is Bussy?: " + saveA.IsBusy.ToString());
                }
                else if (args[0] == "reload")
                {
                    if (saveA.IsBusy == true)
                    {                       
                        saveA.CancelAsync();
                        saveA.Dispose();
                        saveA.RunWorkerAsync();
                        player.MessageFrom("AutoSave", "Cancelled and Reloaded!");
                    }
                    else
                    {
                        saveA.RunWorkerAsync();
                        player.MessageFrom("AutoSave", "Reloaded!");
                    }
                }
            }       
        }
        public void OnConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            if (arg.Class == "autosave" && arg.Function == "status" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                ConsoleSystem.LogError("AutoSave is Bussy?: " + saveA.IsBusy.ToString());
            }
            if (arg.Class == "autosave" && arg.Function == "reload" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                if (saveA.IsBusy == true)
                {
                    saveA.CancelAsync();
                    saveA.Dispose();
                    ConsoleSystem.LogError("AutoSave BackGound Disposed!");
                }
                saveA.RunWorkerAsync();
                ConsoleSystem.LogError("AutoSave Reloaded!");
            }
        }
        public void GuardarMapaA(object sender, DoWorkEventArgs e)
        {
            /*
            Loom.QueueOnMainThread(() =>
            {
                Timer1(600000, null).Start();
            });
             * */
            try
            {
                Logger.Log("2/4 Saving...");
                ConsoleSystem.Print("2/4 Saving...");
                

                ConsoleSystem.Run("save.all", false);
                //RustPP.Helper.CreateSaves();

                Logger.Log("3/4 Done!!");
                ConsoleSystem.Print("3/4 Done!!");            
            }
            catch (Exception ex)
            {
                Logger.Log("ERROR ON SAVE: " + ex.ToString());
                ConsoleSystem.Print("ERROR ON SAVE: " + ex.ToString());
            }
            finally
            {

                Logger.Log("4/4 Finishing & Disposing!");
                Logger.Log("____________________________");
                Logger.Log("");
                ConsoleSystem.Print("4/4 Finishing & Disposing!");
                ConsoleSystem.Print("____________________________");

                saveA.CancelAsync();
                saveA.Dispose();
            }  
        }      
        public TimedEvent Timer1(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += CallBack1;
            return timedEvent;
        }
        public void CallBack1(TimedEvent e)
        {
            //var dict = e.Args;
            e.Kill();

            Timer1(600000, null).Start();
            Logger.Log("");
            Logger.Log("");
            Logger.Log("________AUTOSAVE________");
            Logger.Log("1/4 New Timer Created");
            ConsoleSystem.Print("________AUTOSAVE________");
            ConsoleSystem.Print("1/4 New Timer Created");

            if (saveA.IsBusy == true)
            {
                try
                {
                    saveA.CancelAsync();
                    saveA.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Log("ERROR ON DISPOSE: " + ex.ToString());
                    ConsoleSystem.PrintError("ERROR ON DISPOSE: " + ex.ToString());
                }
                finally
                {
                    saveA.RunWorkerAsync();
                }
            }
            else
            {
                saveA.RunWorkerAsync();
            } 
        }
    }
}
