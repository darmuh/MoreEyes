using MenuLib;
using MenuLib.MonoBehaviors;
using UnityEngine;

// If we want to push an update before this is finished we can just not initialize it our plugin
namespace MoreEyes.Utility;
internal sealed class ExpressionMenu
{
    internal static REPOPopupPage EmoteMenu = new();
    internal static REPOAvatarPreview AvatarPreview;

    internal static REPOLabel zoomTip;
    internal static REPOLabel pages;

    internal static REPOButton num5;
    internal static REPOButton num6;
    internal static REPOButton num7;
    internal static REPOButton num8;
    internal static REPOButton num9;

    internal static readonly Vector2 buttonOffset = new(21f, 35f);
    internal static readonly Vector3 headerOffset = new(0f, 10f, 0f);

    internal static readonly Vector2 anchorOut = new(250f, 50f);
    internal static readonly Vector2 anchorIn = new(225f, 100f);
    internal static readonly Vector3 scaleOut = new(0.75f, 0.75f, 0.75f);
    internal static readonly Vector3 scaleIn = new(1.5f, 1.5f, 1.5f);
    internal static readonly Vector3 posOut = new(0f, -0.75f, 0f);
    internal static readonly Vector3 posIn = new(0f, -3f, 0f);

    internal static bool toggledBinding;

    internal static void Initialize()
    {
        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("Emotes", CreatePopupMenu, p, EyeSelectionMenu.buttonPosMain + buttonOffset));
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("Emotes", CreatePopupMenu, p, EyeSelectionMenu.buttonPosLobby + buttonOffset));
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("Emotes", CreatePopupMenu, p, EyeSelectionMenu.buttonPosEsc + buttonOffset));
    }
    private static void CreatePopupMenu()
    {
        if (EmoteMenu.menuPage != null) return;

        EmoteMenu = MenuAPI.CreateREPOPopupPage(MenuUtils.ApplyGradient("Facial Expressions"), false, true, 10f, new Vector2(-125f, 5f));

        EmoteMenu.AddElement(e =>
            MenuAPI.CreateREPOButton("Back", BackButton, EmoteMenu.transform, new Vector2(315f, 30f))
        );
        EmoteMenu.rectTransform.sizeDelta = new Vector2(400f, 100f);

        EmoteMenu.headerTMP.transform.position = EmoteMenu.headerTMP.transform.position + headerOffset;

        AvatarPreview = MenuAPI.CreateREPOAvatarPreview(EmoteMenu.transform, new Vector2(250f, 50f) , false);
        AvatarPreview.rigTransform.parent.localScale = scaleOut;
        AvatarPreview.rigTransform.parent.localPosition = posOut;

        MenuUtils.zoomLevel = 0f;

        MenuUtils.HandleScrollZoom(AvatarPreview, anchorIn, anchorOut, scaleIn, scaleOut, posIn, posOut);

        zoomTip = MenuAPI.CreateREPOLabel("! Scroll to zoom", EmoteMenu.transform, new Vector2(240, 0));

        MenuUtils.SetTipTextStyling(zoomTip);

        num5 = MenuAPI.CreateREPOButton("5", ToggleBinding, EmoteMenu.transform, new Vector2(75f, 270f));
        num6 = MenuAPI.CreateREPOButton("6", ToggleBinding, EmoteMenu.transform, new Vector2(75f, 220f));
        num7 = MenuAPI.CreateREPOButton("7", ToggleBinding, EmoteMenu.transform, new Vector2(75f, 170f));
        num8 = MenuAPI.CreateREPOButton("8", ToggleBinding, EmoteMenu.transform, new Vector2(75f, 120f));
        num9 = MenuAPI.CreateREPOButton("9", ToggleBinding, EmoteMenu.transform, new Vector2(75f, 70f));

        REPOButton[] numButtons = [num5, num6, num7, num8, num9];

        float offsetX = 50f;

        for (int i = 0; i < numButtons.Length; i++)
        {
            REPOButton numButton = numButtons[i];

            Vector2 pos = new(
                numButton.rectTransform.anchoredPosition.x + offsetX,
                numButton.rectTransform.anchoredPosition.y
            );

            int emoteIndex = i + 1;
            MenuAPI.CreateREPOButton($"Emote{emoteIndex}", CheckoutExpression, numButton.transform.parent, pos);
        }

        int num = 1; // placeholder rn

        pages = MenuAPI.CreateREPOLabel($"Page [{num}]", EmoteMenu.transform, new Vector2(450, 30));

        MenuUtils.SetHeaderTextStyling([pages]);

        EmoteMenu.AddElement(e => MenuAPI.CreateREPOButton("<", PreviousPage, pages.transform, new Vector2(50f, -3f)));
        EmoteMenu.AddElement(e => MenuAPI.CreateREPOButton(">", NextPage, pages.transform, new Vector2(120f, -3f)));

        int emoteCount = 7;
        float startY = 285f;
        float endY = 70f;
        float xPos = 512f;

        float stepY = (startY - endY) / (emoteCount - 1);

        for (int i = 0; i < emoteCount; i++)
        {
            float y = startY - i * stepY;
            Vector2 pos = new(xPos, y);
            int emoteIndex = i + 1;

            EmoteMenu.AddElement(e =>
                MenuAPI.CreateREPOButton($"Emote{emoteIndex}", BindOrCheckout, EmoteMenu.transform, pos)
            );
        }


        EmoteMenu.OpenPage(false);

        EmoteMenu.onEscapePressed += ShouldCloseMenu;
    }

    private static bool ShouldCloseMenu()
    {
        BackButton();
        return true;
    }

    private static void BackButton()
    {
        EmoteMenu.ClosePage(true);

        MenuPageEsc.instance?.ButtonEventContinue();
    }

    private static void PreviousPage()
    {

    }

    private static void NextPage()
    {

    }

    private static void ToggleBinding()
    {
        if (toggledBinding == false)
        {
            toggledBinding = true;
        }
        else
        {
            toggledBinding = false;
        }
    }
    private static void CheckoutExpression()
    {

    }
    private static void BindOrCheckout()
    {
        if (toggledBinding == true)
        {
            // swap for clicked button, if you click 5 and click on an expression on the right you'll change the info on the left box next to 5
            toggledBinding = false;
        }
        else { CheckoutExpression(); }
    }
}
