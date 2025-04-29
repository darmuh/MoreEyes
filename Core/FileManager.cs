﻿using HarmonyLib;
using MoreEyes.EyeManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoreEyes.Core;

//save selections per steam id in this class
public class FileManager
{
    //cache this for reading/writing changes
    internal static Dictionary<string, string> playerSelections = [];

    public static void ReadTextFile()
    {
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");

        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        if (!File.Exists(filePath + "\\selections.txt"))
            Plugin.logger.LogWarning("Player selections could not be obtained!");
        else
        {
            string rawText = File.ReadAllText(filePath + "\\selections.txt");
            Plugin.logger.LogMessage($"Assigning saved player selections!");

            if (rawText.Length == 0)
                return;

            //re-using this from my purchasepack stuff in lethalcompany
            playerSelections = [];

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

            playerSelections = rawText
               .Split(';')
               .Select(item => item.Trim())
               .Select(item => item.Split(':'))
               .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
        }
    }

    public static void UpdatePlayerSelections()
    {
        CustomEyeManager.AllPlayerSelections.Do(a =>
        {
            string selections = $"pupilLeft={a.pupilLeft.Name},pupilRight={a.pupilRight.Name},irisLeft={a.irisLeft.Name},irisRight={a.irisRight.Name}";

            if (playerSelections.ContainsKey(a.playerID))
            {
                playerSelections[a.playerID] = selections;
                Plugin.Spam($"Updated {a.playerID} selections in FileManager");
            }
            else
            {
                playerSelections.Add(a.playerID, selections);
                Plugin.Spam($"Added {a.playerID} selections in FileManager");
            }   
        });
    }

    public static void WriteTextFile()
    {
        UpdatePlayerSelections();
        //use appdata location with folder name `moreEyes`
        string filePath = Path.Combine(@"%userprofile%\appdata\locallow\semiwork\Repo", "moreEyes");

        //expands the %userprofile% to the actual location on the machine
        filePath = Environment.ExpandEnvironmentVariables(filePath);

        string fileContents = string.Empty;

        playerSelections.Do(p =>
        {
            //reversing the dictionary back to one long string that can be parsed again
            fileContents += p.Key + ":";
            fileContents += p.Value + ";";
        });

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        File.WriteAllText(filePath + "\\selections.txt", fileContents);
    }
}
