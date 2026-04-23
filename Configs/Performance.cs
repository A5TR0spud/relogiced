using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class Performance : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    [Header("Items")]
    
    [DefaultValue(200)] [Range((uint)0, (uint)1000)] [Increment((uint)25)]
    public uint LostWispLanternMaximumTileCheck;
    [DefaultValue(600)] [Range((uint)0, (uint)6000)] [Increment((uint)50)]
    public uint LostWispLanternAbsoluteMaximumTileCheck;
    [DefaultValue(15)] [Range((uint)0, (uint)30)] [Increment((uint)1)]
    public uint LostWispLanternMaximumFramesToCheck;
}