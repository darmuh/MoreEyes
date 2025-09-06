using UnityEngine;

namespace MoreEyes.Utility;
public class Enums
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
    public enum EyePart
    {
        Pupil,
        Iris
    }
    //from Menu.cs
    public enum EyeSide
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

    //for safety meassure, we are creating our own keycode enum
    public enum NumpadKey
    {
        Keypad0 = KeyCode.Keypad0,
        Keypad1 = KeyCode.Keypad1,
        Keypad2 = KeyCode.Keypad2,
        Keypad3 = KeyCode.Keypad3,
        Keypad4 = KeyCode.Keypad4,
        Keypad5 = KeyCode.Keypad5,
        Keypad6 = KeyCode.Keypad6,
        Keypad7 = KeyCode.Keypad7,
        Keypad8 = KeyCode.Keypad8,
        Keypad9 = KeyCode.Keypad9
    }
}
