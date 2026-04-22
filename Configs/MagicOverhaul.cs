using System.ComponentModel;
using Relogiced.Content.MagicOverhaul.Hardmode.PathfindWeapon;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class MagicOverhaul : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    [ReloadRequired] [DefaultValue(true)]
    public bool ManaRewrite;
    
    [Header("Items")]
    [ReloadRequired] [DefaultValue(true)]
    public bool EarlyGameWeapons;
    [ReloadRequired] [DefaultValue(true)]
    public bool AuxiliaryManaItems;
    [ReloadRequired] [DefaultValue(true)]
    public bool HardmodeWeapons;
    [DefaultValue(500)] [Range((uint)0, (uint)1000)] [Increment((uint)25)]
    public uint LostWispLanternMaximumTileCheck;
    [DefaultValue(3000)] [Range((uint)0, (uint)6000)] [Increment((uint)50)]
    public uint LostWispLanternAbsoluteMaximumTileCheck;
}