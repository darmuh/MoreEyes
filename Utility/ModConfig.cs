using BepInEx.Configuration;
using MoreEyes.Managers;
using System.Linq;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Utility;
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

        foreach (var group in CustomEyeManager.AllPupilTypes
          .Where(p => !p.isVanilla)
          .GroupBy(p => p.PairName))
        {
            var config = ConfigFile.Bind($"{group.First().ModName} Pupils",
                                         group.Key,
                                         true,
                                         $"Enable/Disable pupil{(group.Count() > 1 ? " pair" : "")} - {group.Key}");

            foreach (var pupil in group)
                pupil.ConfigToggle = config;
        }

        foreach (var group in CustomEyeManager.AllIrisTypes
            .Where(i => !i.isVanilla)
            .GroupBy(i => i.PairName))
        {
            var config = ConfigFile.Bind($"{group.First().ModName} Irises",
                                         group.Key,
                                         true,
                                         $"Enable/Disable iris{(group.Count() > 1 ? " pair" : "")} - {group.Key}");

            foreach (var iris in group)
                iris.ConfigToggle = config;
        }
    }
}