using UnityEngine;
using BepInEx;
using MenuLib;

namespace MoreEyes.Menus;

internal sealed class Menu
{

    internal static void Initialize()
    {
        MenuAPI.AddElementToMainMenu(parent => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, parent, new Vector2(48.3f, 55.5f))); // Change these later
        MenuAPI.AddElementToLobbyMenu(parent => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, parent, new Vector2(186f, 32)));
        MenuAPI.AddElementToEscapeMenu(parent => MenuAPI.CreateREPOButton("MoreEyes", CreateElement, parent, new Vector2(126f, 86f)));
    }
    private static void CreateElement()
    {
        // Open popup with MenuAPI.CreateREPOPopupPage()
        // Create a back button for this popup page with MENUAPI.CreateREPOButton()
    }
}
