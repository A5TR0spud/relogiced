using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class RangedOverhaul : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    [Header("Items")]
    [ReloadRequired] [DefaultValue(true)]
    public bool Chekhov;
    [ReloadRequired] [DefaultValue(true)]
    public bool RodFromGodItem;
}