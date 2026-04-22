using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class Assorted : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    
    [Header("Items")]
    [ReloadRequired] [DefaultValue(true)]
    public bool AntiGravityHookRework;
    
    [ReloadRequired] [DefaultValue(true)]
    public bool VoodooRework;
    
    [ReloadRequired] [DefaultValue(true)]
    public bool ShopChanges;
    
    [ReloadRequired] [DefaultValue(true)]
    public bool RarityChanges;

    [Header("Other")]
    [ReloadRequired] [DefaultValue(true)]
    public bool BuffStationsSave;
}