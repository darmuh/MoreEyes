﻿namespace MoreEyes.Utility;
internal class Enums
{
    internal enum ModLogLevel
    {
        WarningsOnly,
        Standard,
        Debug
    }

    internal enum ModInMenuDisplay
    {
        Never,
        Duplicates,
        Always
    }
    internal enum MenuOrderBy
    {
        NameOnly,
        ModNameOnly,
        ModNameAndName,
        None
    }

    //from Menu.cs
    internal enum EyePart
    {
        Pupil,
        Iris
    }
    //from Menu.cs
    internal enum EyeSide
    {
        Left,
        Right
    }

    //from CustomEyeManager.cs
    internal enum PrefabSide
    {
        Left,
        Right,
        Both
    }
}
