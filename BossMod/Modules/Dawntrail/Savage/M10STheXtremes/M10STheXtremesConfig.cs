using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Savage.M10STheXtremes;

public enum Strategy
{
    [PropertyDisplay("Hector (NA)")]
    Hector,

    [PropertyDisplay("game8 (JP)")]
    game8,
}

[ConfigDisplay(Order = 0x150, Parent = typeof(DawntrailConfig))]
public class M10STheXtremesConfig : ConfigNode
{
    [PropertyDisplay("Select which strat to use for hints")]
    public Strategy HintOption = Strategy.Hector;

    // NA does NW - S - NW - N, drops puddle along voidzone in L-shape
    // JP does NE - NW - NE - NW, drops puddle along voidzone + wall in L-shape
    [PropertyDisplay("Show spots for Flame Floater")]
    public bool ShowFlameFloaterHints = false;

    [PropertyDisplay("Show spots for Double/Reverse Alley-Oop")]
    public bool ShowWaterAlleyOopHints = false;

    //[GroupPreset("TTHH/MMRR", [0, 1, 2, 3, 4, 5, 6, 7])]
    // index is the role assignment, value is index of GroupDetails
    // Hector (NA) and game8 (JP) use same boss-relative positions for 1st Deep Blue only one
    // different positions for watersnaking (3-1 instead of 2-2)
    // same spots during split arena
    // same spots after IA2
    [PropertyDisplay("Order for Double/Reverse Alley-Oop (boss relative)")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WaterAlleyOopAssignment = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    [PropertyDisplay("Show spots for Flame Alley-Oop")]
    public bool ShowFireAlleyOopHints = false;

    [PropertyDisplay("Show Deep Varial cleave early")]
    public bool ShowDeepVarialEarly = false;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        var needAssignments = ShowWaterAlleyOopHints || ShowFireAlleyOopHints;
        if (needAssignments)
        {
            var partyConfig = Service.Config.Get<PartyRolesConfig>();
            var playerAssignment = partyConfig[ws.Party.Members[PartyState.PlayerSlot].ContentId];

            if (playerAssignment == PartyRolesConfig.Assignment.Unassigned)
            {
                ImGui.TextColoredWrapped(Colors.TextColor2, "Set player role under Party Roles Assignment for selected option(s) to work!");
            }
        }

        if (!WaterAlleyOopAssignment.Validate())
        {
            ImGui.TextColoredWrapped(Colors.TextColor2, "Invalid role assignments for Double/Reverse Alley-Oop!");
        }
    }
}
