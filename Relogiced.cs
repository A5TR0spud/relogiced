using Relogiced.Configs;
using Terraria.ModLoader;

namespace Relogiced;
public class Relogiced : Mod
{
	public static Assorted ConfigAssorted => ModContent.GetInstance<Assorted>();
	public static MeleeOverhaul ConfigMeleeOverhaul => ModContent.GetInstance<MeleeOverhaul>();
	public static MagicOverhaul ConfigMagicOverhaul => ModContent.GetInstance<MagicOverhaul>();
	public static Client ConfigClient => ModContent.GetInstance<Client>();
	public static Mod Instance => ModContent.GetInstance<Relogiced>();
}

