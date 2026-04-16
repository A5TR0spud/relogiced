using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class MagicOverhaul : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    [Header("Items")]
    [ReloadRequired] [DefaultValue(true)]
    public bool EarlyGameWeapons;
}