using BepInEx.Configuration;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Utility
{
    //Client configuration items
    internal class ModConfig
    {
        internal static ConfigEntry<ModLogLevel> ClientLogLevel { get; set; }
        internal static ConfigEntry<ModInMenuDisplay> ModNamesInMenu {  get; set; }
        internal static ConfigEntry<float> MenuItemScrollSpeed { get; set; }
        internal static ConfigFile ConfigFile { get; set; }

        internal static void BindConfigItems(ConfigFile config)
        {
            ConfigFile = config;
            ClientLogLevel = config.Bind("General", "ClientLogLevel", ModLogLevel.Standard, "Set MoreEyes logging level");
            ModNamesInMenu = config.Bind("Menus", "Display Mod Names", ModInMenuDisplay.Duplicates, "Set when should mod names be displayed in the MoreEyes Menu");
            MenuItemScrollSpeed = config.Bind("Menus", "Auto-scroll Speed", 14f, new ConfigDescription("Set how fast the name of a pupil or iris will scroll when the name doesn't fit the text box.", new AcceptableValueRange<float>(8f, 90f)));
        }
    }
}
