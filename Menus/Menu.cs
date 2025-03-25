using UnityEngine;
using BepInEx;
using MenuLib;
using MenuLib.MonoBehaviors;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace MoreEyes.Menus;

internal sealed class Menu
{
    internal static REPOButton clickedButton;

    private static readonly List<GameObject> placeholder = [];

    internal static void Initialize()
    {
        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, p, new Vector2(48.3f, 55.5f))); // Change these later
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, p, new Vector2(186f, 32)));
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, p, new Vector2(126f, 86f)));
    }
    private static void CreateElement()
    {
        clickedButton = null;

        var REPOPopup = MenuAPI.CreateREPOPopupPage("Mods", REPOPopupPage.PresetSide.Left, false, true);

        REPOPopup.AddElement(p => MenuAPI.CreateREPOButton("Back", () => {
            if (placeholder.Count == 0)
            {
                REPOPopup.ClosePage(true);
                return;
            }
        }, p, new Vector2(66f, 18f)));

        // Totally placeholder
    }
}
