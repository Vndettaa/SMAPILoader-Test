﻿using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using HarmonyLib;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SMAPIGameLoader;
internal static class EntryGame
{
    public static void LaunchGameActivity(Activity launcherActivity)
    {
        TaskTool.Run(launcherActivity, () =>
        {
            ////debug
            //ToastNotifyTool.Notify("Start sleep 8 sec");
            //Thread.Sleep(8000);
            //ToastNotifyTool.Notify("Done sleep.");

            LaunchGameActivityInternal(launcherActivity);
        });
    }

    static void LaunchGameActivityInternal(Activity launcherActivity)
    {
        ToastNotifyTool.Notify("Starting Game..");
        //check game it's can launch with version
        if (StardewApkTool.IsGameVersionSupport == false)
        {
            ToastNotifyTool.Notify("Not support game version: " + StardewApkTool.CurrentGameVersion + ", please update game");
            return;
        }

        try
        {
            //clone game assets
            TaskTool.AddNewLine("Try clone game assets");
            GameAssetManager.VerifyAssets();
            TaskTool.AddNewLine("Done verify asset");
            GameAssemblyManager.VerifyAssemblies();
            TaskTool.AddNewLine("Done verify assemblies");

            //Load MonoGame.Framework.dll into reference
            GameAssemblyManager.LoadAssembly(GameAssemblyManager.MonoGameFrameworkDllFileName);

            //patch rewrite StardewValley.dll & Load
            TaskTool.AddNewLine("Try rewrite StardewValley.dll");
            var stardewDllFilePath = GameAssemblyManager.StardewValleyFilePath;
            StardewGameRewriter.Rewrite(stardewDllFilePath, out var isRewrite);
            TaskTool.AddNewLine("Done rewriter");
            Assembly.LoadFrom(stardewDllFilePath);

#if DEBUG
            ToastNotifyTool.Notify("Error can't start game on Debug Mode");
            return;
#endif

            var intent = new Intent(launcherActivity, typeof(SMAPIActivity));
            launcherActivity.StartActivity(intent);
            launcherActivity.Finish();
        }
        catch (Exception ex)
        {
            ToastNotifyTool.Notify("Error:LaunchGameActivity: " + ex.ToString());
        }
    }
}