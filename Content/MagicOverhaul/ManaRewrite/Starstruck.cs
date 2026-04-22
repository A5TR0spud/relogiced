using Terraria;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.ManaRewrite;

public class Starstruck : ModBuff
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite && Relogiced.ConfigMagicOverhaul.AuxiliaryManaItems;
    }

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<ManaRewritePlayer>().Starstruck = true;
        player.statDefense -= 4;
        player.tipsy = true;
    }
}