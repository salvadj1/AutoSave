using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using Fougerite.Events;
using Fougerite;
using UnityEngine;

using RustProto;
using RustProto.Helpers;
using System.IO;

namespace AutoSave
{
    public class AutoSave : Fougerite.Module
    {
        public override string Name { get { return "AutoSave"; } }
        public override string Author { get { return "Salva/juli"; } }
        public override string Description { get { return "AutoSave "; } }
        public override Version Version { get { return new Version("1.5"); } }

        private static string red = "[color #FF0000]";
        private static string blue = "[color #81F7F3]";
        private static string green = "[color #82FA58]";
        private static string yellow = "[color #F4FA58]";
        private static string orange = "[color #FF8000]";
        private static string white = "[color #FFFFFF]";

        public Mono MonoClass;
        public GameObject monoClassLoad;
        public IniParser Settings;

        public static DateTime SaveStartTime = DateTime.Now;
        public static DateTime SaveEndTime = DateTime.Now;
        public static int ObjectsCount = 0;

        public static int MinsToSave = 10;
        public static bool SaveInBackground = false;
        public static bool ShowObjectCount = true;

        public static BackgroundWorker BGW;

        public override void Initialize()
        {
            Hooks.OnServerLoaded += OnServerLoaded;
            Hooks.OnConsoleReceived += OnConsoleReceived;
            Hooks.OnServerInit += OnServerInit;
            ReloadConfig();
        }
        public override void DeInitialize()
        {
            Hooks.OnServerLoaded -= OnServerLoaded;
            Hooks.OnConsoleReceived -= OnConsoleReceived;
            Hooks.OnServerInit -= OnServerInit;
            if (monoClassLoad != null) UnityEngine.Object.DestroyImmediate(monoClassLoad);
        }
        public void OnServerInit()
        {
            //ConsoleSystem.Run("save.autosavetime 999999999", false); //disable Native Server Save
            ConsoleSystem.Run("save.autosavetime " + int.MaxValue, false);//dretax way
        }
        public void OnServerLoaded()
        {
            monoClassLoad = new GameObject();
            MonoClass = monoClassLoad.AddComponent<Mono>();
            UnityEngine.Object.DontDestroyOnLoad(monoClassLoad);
        }
        public void OnConsoleReceived(ref ConsoleSystem.Arg arg, bool external)
        {
            if (arg.Class == "help" && arg.Function == "help" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                Logger.LogError("PLUGIN: " + Name + " " + Version);
                Logger.Log("asave.save - SAVE ALL MAP");
                Logger.Log("asave.reload - RELOAD CONFIGURATION");
                Logger.LogError("");
            }
            if (arg.Class == "asave" && arg.Function == "save" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                CallServerSave();
                Logger.Log("Done!");
            }
            if (arg.Class == "asave" && arg.Function == "reload" && ((arg.argUser != null && arg.argUser.admin) || arg.argUser == null))
            {
                ReloadConfig();
                Logger.Log("Done!");
            }
        }
        private void ReloadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "MinsToSave", "10");
                Settings.AddSetting("Settings", "SaveInBackground", "false");
                Settings.AddSetting("Settings", "ShowObjectCount", "true");
                Settings.Save();
                Logger.Log(Name + " Plugin: New Settings File Created!");
                ReloadConfig();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                if (Settings.ContainsSetting("Settings", "MinsToSave") &&
                    Settings.ContainsSetting("Settings", "SaveInBackground") &&
                    Settings.ContainsSetting("Settings", "ShowObjectCount"))
                {

                    try
                    {
                        MinsToSave = int.Parse(Settings.GetSetting("Settings", "MinsToSave"));
                        SaveInBackground = Settings.GetBoolSetting("Settings", "SaveInBackground");
                        ShowObjectCount = Settings.GetBoolSetting("Settings", "ShowObjectCount");
                        Logger.Log(Name + " Plugin: Settings file Loaded!");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(Name + " Plugin: Detected a problem in the configuration");
                        Logger.Log("ERROR -->" + ex.Message);
                        File.Delete(Path.Combine(ModuleFolder, "Settings.ini"));
                        Logger.Log(Name + " Plugin: Deleted the old configuration file");
                        ReloadConfig();
                    }
                }
                else
                {
                    Logger.LogError(Name + " Plugin: Detected a problem in the configuration (lost key)");
                    File.Delete(Path.Combine(ModuleFolder, "Settings.ini"));
                    Logger.LogError(Name + " Plugin: Deleted the old configuration file");
                    ReloadConfig();
                }
                return;
            }
            return;
        }
        public static void CallServerSave()
        {
            Loom.QueueOnMainThread(() =>
            {
                SaveStartTime = DateTime.Now;

                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\save\server_data\BackUpSaves\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\save\server_data\BackUpSaves\");
                }
                try
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + @"\save\server_data\rust_island_2013.sav"))
                    {
                        string d = System.DateTime.Now.Day.ToString();
                        string h = System.DateTime.Now.Hour.ToString();
                        string m = System.DateTime.Now.Minute.ToString();
                        string s = System.DateTime.Now.Second.ToString();
                        string date = "Day " + d + " Hour " + h + "-" + m + "-" + s;
                        string name = "rust_island_2013 " + date + ".sav";
                        File.Copy(Directory.GetCurrentDirectory() + @"\save\server_data\rust_island_2013.sav", Directory.GetCurrentDirectory() + @"\save\server_data\BackUpSaves\" + name, true);
                    }
                }
                catch (Exception ex)
                {
                }


                if (SaveInBackground)
                {
                    BackgroundSave();
                }
                else
                {
                    NormalSave();
                }
            });
        }

        public static void NormalSave()
        {
            Loom.QueueOnMainThread(() =>
            {
                try
                {
                    AvatarSaveProc.SaveAll(); //???????
                }
                catch (Exception ex)
                {
                }
                
                WorldSave fsave;
                using (Recycler<WorldSave, WorldSave.Builder> recycler = WorldSave.Recycler())
                {
                    WorldSave.Builder builder = recycler.OpenBuilder();
                    ServerSaveManager.Get(false).DoSave(ref builder);
                    fsave = builder.Build();
                }

                if (ShowObjectCount)
                {
                    ObjectsCount = fsave.SceneObjectCount + fsave.InstanceObjectCount;
                }

                FileStream stream2 = File.Open(Directory.GetCurrentDirectory() + @"\save\server_data\rust_island_2013.sav", FileMode.Create, FileAccess.Write);
                fsave.WriteTo(stream2);
                stream2.Flush();
                
                stream2.Dispose(); //??

                SaveEndTime = DateTime.Now;
                AnnounceResults();
            }); 
        }

        public static void BackgroundSave()
        {
            //BackgroundWorker BGW = new BackgroundWorker();
            BGW = new BackgroundWorker();
            BGW.DoWork += new DoWorkEventHandler(SaveBW);
            BGW.RunWorkerAsync();
        }
        public static void SaveBW(object sender, DoWorkEventArgs e)
        {
            AvatarSaveProc.SaveAll(); //???????
            WorldSave fsave;
            using (Recycler<WorldSave, WorldSave.Builder> recycler = WorldSave.Recycler())
            {
                WorldSave.Builder builder = recycler.OpenBuilder();
                ServerSaveManager.Get(false).DoSave(ref builder);
                fsave = builder.Build();
            }

            if (ShowObjectCount)
            {
                ObjectsCount = fsave.SceneObjectCount + fsave.InstanceObjectCount;
            }

            FileStream stream2 = File.Open(Directory.GetCurrentDirectory() + @"\save\server_data\rust_island_2013.sav", FileMode.Create, FileAccess.Write);
            fsave.WriteTo(stream2);
            stream2.Flush();
            stream2.Dispose(); //??

            SaveEndTime = DateTime.Now;
            AnnounceResults();
            BGW.Dispose();
        }
        public static void AnnounceResults()
        {
            string savetype = " ";
            if (SaveInBackground)
            {
                savetype = "in Background";
            }
            Loom.QueueOnMainThread(() =>
            {
                TimeSpan DifTime = SaveEndTime.Subtract(SaveStartTime);

                if (ShowObjectCount)
                {
                    Logger.Log(" ");
                    Logger.Log("Server Saved " + ObjectsCount + " Object(s). Took " + DifTime.Seconds + "." + DifTime.Milliseconds + " seconds to save them " + savetype);
                    Logger.Log(@"BACKUP file has been created in ...\save\server_data\BackUpSave");
                    Logger.Log(" ");

                    Server.GetServer().BroadcastFrom("AutoSave", "Server Saved " + yellow + ObjectsCount + white +
                        " Object(s). Took " + yellow + DifTime.Seconds + "." + DifTime.Milliseconds + white + " seconds " + savetype);
                }
                else
                {
                    Logger.Log(" ");
                    Logger.Log("Server Saved Took " + DifTime.Seconds + "." + DifTime.Milliseconds + " seconds " + savetype);
                    Logger.Log(@"BACKUP file has been created in ...\save\server_data\BackUpSave");
                    Logger.Log(" ");

                    Server.GetServer().BroadcastFrom("AutoSave", "Server Saved Took " +
                        yellow + DifTime.Seconds + "." + DifTime.Milliseconds + white + " seconds " + savetype);
                }
            });
            return;
        }
        /*
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
        }*/
    }
}
