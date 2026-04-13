using Relogiced.Configs;
using Terraria.ModLoader;

namespace Relogiced;
public class Relogiced : Mod
{
	public static Reworks Config_Reworks => ModContent.GetInstance<Reworks>();
	public static Mod Instance => ModContent.GetInstance<Relogiced>();
}

