using HarmonyLib;
using MoreEyes.Collections;
using MoreEyes.Core;
using MoreEyes.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Managers;
internal class FileManager
{
    //cache this for reading/writing changes
    internal static Dictionary<string, string> PlayerSelections { get; private set; } = [];
    internal static bool UpdateWrite { get; set; } = false;

    internal static void ReadTextFile()
    {
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");

        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        if (!File.Exists(filePath + "\\selections.txt"))
            Loggers.Message("No saved player selections exist!");
        else
        {
            string rawText = File.ReadAllText(filePath + "\\selections.txt");
            Loggers.Message($"Assigning saved player selections!");

            if (rawText.Length == 0)
                return;

            //re-using this from my purchasepack stuff in lethalcompany
            PlayerSelections = [];

            //creates a dictionary based on the following pattern
            //123xyz:bo,ba,be;456abc:fo,fa,fe
            //the keys will be 123xyz and 456abc as they are the first strings in the pair split between the :
            //the rest of the string will be one singular value, so bo,ba,be is one singular string matching the key
            //we can parse this value later to our actual class data
            //this is only the initial piece of the puzzle, it will be a bit more complicated
            //my idea is to do something like
            //12341234:pupilLeft=standard,pupilRight=cross,irisLeft=none,irisRight=cat;124432542:pupilLeft=standard,pupilRight=cross,irisLeft=none,irisRight=cat

            //remove trailing ";" to avoid dictionary indexing errors
            if (rawText.EndsWith(';'))
                rawText = rawText.TrimEnd(';');

            PlayerSelections = rawText
               .Split(';')
               .Select(item => item.Trim())
               .Select(item => item.Split(':'))
               .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
        }
    }

    public static string GetSelectionsText(string playerID)
    {
        if(PlayerSelections.TryGetValue(playerID, out var selection))
            return selection;

        return string.Empty;
    }

    public static string GetSelectionsText(PlayerEyeSelection player)
    {
        if(player == null) return string.Empty;

        return $"pupilLeft={player.pupilLeft.UID},pupilLeftColor={ColorUtility.ToHtmlStringRGB(player.PupilLeftColor)},pupilRight={player.pupilRight.UID},pupilRightColor={ColorUtility.ToHtmlStringRGB(player.PupilRightColor)},irisLeft={player.irisLeft.UID},irisLeftColor={ColorUtility.ToHtmlStringRGB(player.IrisLeftColor)},irisRight={player.irisRight.UID},irisRightColor={ColorUtility.ToHtmlStringRGB(player.IrisRightColor)}";
    }

    private static void UpdatePlayerSelections()
    {
        CustomEyeManager.AllPlayerSelections.Do(a =>
        {
            string selections = GetSelectionsText(a);

            if (PlayerSelections.ContainsKey(a.playerID))
            {
                PlayerSelections[a.playerID] = selections;
            }
            else
            {
                PlayerSelections.Add(a.playerID, selections);
            }   
        });
    }

    internal static Dictionary<string, string> GetPairsFromString(string selections)
    {
        return selections.Split(',')
                .Select(item => item.Trim())
                .Select(item => item.Split('='))
                .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
    }

    internal static Dictionary<string, string> GetSelectionPairsFromFile(string playerID)
    {
        Dictionary<string, string> pairs = [];
        if (!CustomEyeManager.VanillaPupilsExist)
            return pairs;

        string selections = GetPlayerSelections(playerID);
        
        if (string.IsNullOrEmpty(selections))
        {
            Loggers.Info($"Unable to get saved selection for [ {playerID} ]");
            return pairs;
        }

        return GetPairsFromString(selections);

    }
    private static string GetPlayerSelections(string steamID)
    {
        if (PlayerSelections.ContainsKey(steamID))
            return PlayerSelections[steamID];
        else
            return "";
    }

    internal static void WriteTextFile()
    {
        Loggers.Message("Updating saved selections!");
        UpdateWrite = false;
        UpdatePlayerSelections();
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");
        Loggers.Debug($"Location: {filePath}");
        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        string fileContents = string.Empty;

        PlayerSelections.Do(p =>
        {
            //reversing the dictionary back to one long string that can be parsed again
            fileContents += p.Key + ":";
            fileContents += p.Value + ";";
        });

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        File.WriteAllText(filePath + "\\selections.txt", fileContents);

        if (ModCompats.IsMoreHeadPresent())
            MoreHeadCompat.WriteSaveFile();
    }
}
