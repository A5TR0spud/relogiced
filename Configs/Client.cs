using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Relogiced.Configs;

public class Client : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [Header("Experiments")]
    [DefaultValue(false)]
    public bool LightingRework;
    [DefaultValue(false)]
    public bool LightingCompensation;

    [Header("Other")]
    [DefaultValue(false)]
    public bool DebugMode;
}