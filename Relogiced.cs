using System.IO;
using Relogiced.Configs;
using Relogiced.Other;
using Terraria.ModLoader;

namespace Relogiced;
public class Relogiced : Mod
{
	public static Assorted ConfigAssorted => ModContent.GetInstance<Assorted>();
	public static MeleeOverhaul ConfigMeleeOverhaul => ModContent.GetInstance<MeleeOverhaul>();
	public static MagicOverhaul ConfigMagicOverhaul => ModContent.GetInstance<MagicOverhaul>();
	public static RangedOverhaul ConfigRangedOverhaul => ModContent.GetInstance<RangedOverhaul>();
	public static SummonOverhaul ConfigSummonOverhaul => ModContent.GetInstance<SummonOverhaul>();
	public static Client ConfigClient => ModContent.GetInstance<Client>();
	public static Performance ConfigPerformance => ModContent.GetInstance<Performance>();
	public static Mod Instance => ModContent.GetInstance<Relogiced>();
	
	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		NetworkHelper.HandlePacket(reader, whoAmI);
	}
}

