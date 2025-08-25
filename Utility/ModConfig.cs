using BepInEx.Configuration;
using HarmonyLib;
using MoreEyes.Managers;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Utility;
//Client configuration items
internal class ModConfig
{
    internal static ConfigEntry<ModLogLevel> ClientLogLevel { get; private set; }
    internal static ConfigEntry<ModInMenuDisplay> ModNamesInMenu {  get; private set; }
    internal static ConfigEntry<MenuOrderBy> MenuListOrder { get; private set; }
    internal static ConfigEntry<float> MenuItemScrollSpeed { get; private set; }
    
    internal static ConfigFile ConfigFile { get; private set; }

    internal static void BindConfigItems(ConfigFile config)
    {
        ConfigFile = config;
        ClientLogLevel = config.Bind("General", "ClientLogLevel", ModLogLevel.Standard, "Set MoreEyes logging level");
        ModNamesInMenu = config.Bind("Menus", "Display Mod Names", ModInMenuDisplay.Duplicates, "Set when should mod names be displayed in the MoreEyes Menu");
        MenuListOrder = config.Bind("Menus", "Order By", MenuOrderBy.NameOnly, "Set pupil/iris ordering mode");
        MenuItemScrollSpeed = config.Bind("Menus", "Auto-scroll Speed", 14f, new ConfigDescription("Set how fast the name of a pupil or iris will scroll when the name doesn't fit the text box.", new AcceptableValueRange<float>(8f, 90f)));
    }

    internal static void GenerateConfigItems()
    {
        if (ConfigFile == null)
            return;

        CustomEyeManager.AllPupilTypes.Do(p =>
        {
            if (p.isVanilla)
                return;
            p.ConfigToggle = ConfigFile.Bind($"{p.ModName} Pupils", p.Name, true, $"Enable/Disable pupil - {p.Name}");
        });

        CustomEyeManager.AllIrisTypes.Do(i =>
        {
            if (i.isVanilla)
                return;
            i.ConfigToggle = ConfigFile.Bind($"{i.ModName} Irises", i.Name, true, $"Enable/Disable iris - {i.Name}");
        });
    }
}
