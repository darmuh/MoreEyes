# MoreEyes

**MoreEyes** is a mod for **R.E.P.O** that expands character eye customization.  
This is an ***API*** that lets modders create **custom irises and pupils**, but also includes a basic package.

---

## ✨ Features

- **Custom Menu**  
  - Select irises and pupils for **left and right eyes** independently.  
  - Color each eye separately.  
  - Zoom in/out on your player avatar.  
  - Accessible from **Main Menu**, **Lobby Menu**, and **Escape Menu**.  
  - Change your eye selections **mid-game**.  
  - Menu text uses your chosen player’s color with a gradient shift (darker for bright colors, brighter for dark colors).  
  - Color sliders use a complementary color.

- **Configuration Options**  
  - **Menu prefix display:** Always, Duplicates, Never.  
  - **Alphabetical sorting:** None, Name Only, Mod Name Only, Mod Name & Name.  
  - **Auto-scroll speed:** Horizontally scrolls text (useful when prefix is present).  
  - **Client log level:** Standard, Debug, Warnings Only.  
  - **Generated config:** Toggle pupil/iris pairs per mod (supports left/right selections or individual toggles).

<details>
<summary> <b>📸 Previews</b> </summary>

![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acbb0426608489f9368483/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acbb5b7d9cf554dd7a0a06/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acbbccce267045733f2537/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acbc669b0ad9775274934b/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acc1567c4581e6f8eb353f/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acc211f5aae2a0fd59e653/download/image.png)  
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68acc2431a3572cd2eeb5f8f/download/image.png)
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68ada3be923441525c3ed32d/download/image.png)
![](https://trello.com/1/cards/68acbaf47857addc263fe80c/attachments/68ada3cc37eb097b738d685f/download/image.png)

</details>

---

## 📚 For Developers

Want to make your own iris or pupil mods?  
Check out the **MoreEyes SDK** for detailed documentation, setup, and examples:

👉 [Learn more in the SDK](https://github.com/darmuh/MoreEyesSDK)

### Known little problems

- Spamming mousewheel to scroll back and forth to zoom in and out on the playeravatar is a bit iffy, it'll need some more work but its not a big issue
- On the right pupil's next button `>` specifically on standard pupil on the first try it'll not do anything, just on the second click, this is a weird small issue - will look into it
- Interacting with valuables where you eyeselections' sizes are overridden will make them clip thru eyelids, this is something thats a very specific scenario, very small issue but we'd like to fix this as well
