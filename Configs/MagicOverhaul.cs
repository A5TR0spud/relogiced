using System.ComponentModel;
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
}