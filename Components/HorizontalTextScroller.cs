using MoreEyes.Utility;
using TMPro;
using UnityEngine;

namespace MoreEyes.Components
{
    internal class HorizontalTextScroller : MonoBehaviour
    {
        internal float widthOfText = 180f; //text area to scroll
        //ScrollSpeed replaced with config item - ModConfig.MenuItemScrollSpeed.Value

        internal TextMeshProUGUI textMesh;
        private RectTransform textRect;

        private float scrollPos = 0f;
        private float xOffset = 0f;
        internal Vector3 startPos = Vector3.zero;
        private bool ready = false;
        private bool shouldScroll = false;

        //MenuButtonRef
        private MenuButton _button = null!;

        void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            textRect = GetComponent<RectTransform>();
        }

        void Start()
        {
            xOffset = textRect.localPosition.x * -1f;
            scrollPos = textRect.localPosition.x - xOffset;
            ready = true;

        }


        void LateUpdate()
        {
            if (textMesh == null || !ready)
                return;

            shouldScroll = textMesh.preferredWidth > 85f; //if text is large enough, make it scroll

            if (shouldScroll && _button != null) //added so that text only scrolls if the button is hovered
                shouldScroll = _button.hovering;

            if (!shouldScroll)
            {
                textRect.localPosition = startPos;
                scrollPos = textRect.localPosition.x - xOffset;
                textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
                return;
            }

            textRect.localPosition = new(scrollPos % WidthMultiplier() + xOffset, startPos.y, startPos.z);
            scrollPos -= ModConfig.MenuItemScrollSpeed.Value * Time.deltaTime;
        }

        private float WidthMultiplier()
        {
            return textMesh.preferredWidth * 1.5f;
        }

        internal void SetButtonRef(MenuButton button)
        {
            _button = button;
        }
    }
}
