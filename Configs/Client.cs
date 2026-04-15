using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class Client : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [Header("Experiments")]
    [ReloadRequired] [DefaultValue(false)]
    public bool LightingRework;
}