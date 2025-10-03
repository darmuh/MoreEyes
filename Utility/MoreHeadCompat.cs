using HarmonyLib;
using MoreEyes.Collections;
using MoreEyes.Components;
using MoreEyes.Core;
using MoreEyes.Managers;
using MoreHead;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoreEyes.Utility;
internal class MoreHeadCompat
{
    internal static int CurrentOutFit => ConfigManager.GetCurrentOutfitIndex();
    internal static int LastOutFit = -1;
    internal static Dictionary<int, string> OutFitToSelections = [];
    
    internal static void AssignCurrentOutFitToCurrentSelections()
    {
        int current = CurrentOutFit;
        string selection = FileManager.PlayerSelections[PatchedEyes.Local.playerID];
        if (OutFitToSelections[current] != selection)
            OutFitToSelections[current] = selection;
        Loggers.Debug($"Setting OutFitToSelections[{current}] to {selection}");
    }

    internal static void OutfitCheck()
    {
        if (CurrentOutFit == LastOutFit)
            return;

        Loggers.Debug($"OutfitCheck detected different outfit number!");

        if(OutFitToSelections.TryGetValue(CurrentOutFit, out string selections))
        {
            PatchedEyes.Local.CurrentSelections.SetSelectionsFromPairs(FileManager.GetPairsFromString(selections));
            PatchedEyes.Local.CurrentSelections.PlayerEyesSpawn();
        }  
        else
            OutFitToSelections[CurrentOutFit] = FileManager.PlayerSelections[PatchedEyes.Local.playerID];

        LastOutFit = CurrentOutFit;
        FileManager.WriteTextFile();
        MoreEyesNetwork.SyncMoreEyesChanges();
    }

    internal static void MenuCheck()
    {
        if (MoreHeadUI.decorationsPage != null)
        {
            if (MoreHeadUI.decorationsPage.menuPage.pageActive)
                OutfitCheck();
        }
    }


    internal static void ReadSaveFile()
    {
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");

        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        if (!File.Exists(filePath + "\\outfits.txt"))
            Loggers.Message("No saved player outfits file exists!");
        else
        {
            string rawText = File.ReadAllText(filePath + "\\outfits.txt");
            Loggers.Message($"Assigning saved player outfit selections!");

            if (rawText.Length == 0)
                return;

            OutFitToSelections = [];

            //remove trailing ";" to avoid dictionary indexing errors
            if (rawText.EndsWith(';'))
                rawText = rawText.TrimEnd(';');

            OutFitToSelections = rawText
               .Split(';')
               .Select(item => item.Trim())
               .Select(item => item.Split(':'))
               .ToDictionary(pair => Convert.ToInt32(pair[0].Trim()), pair => pair[1].Trim());
        }

        AssignCurrentOutFitToCurrentSelections();
    }

    internal static void WriteSaveFile()
    {
        Loggers.Message("Updating saved outfit selections!");
        OutFitToSelections[CurrentOutFit] = FileManager.PlayerSelections[PatchedEyes.Local.playerID];
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");
        Loggers.Debug($"Location: {filePath}");
        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        string fileContents = string.Empty;

        OutFitToSelections.Do(o =>
        {
            //reversing the dictionary back to one long string that can be parsed again
            fileContents += o.Key + ":";
            fileContents += o.Value + ";";
        });

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        File.WriteAllText(filePath + "\\outfits.txt", fileContents);
    }
}
