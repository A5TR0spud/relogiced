using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Hardmode.LostWispLantern;

public class NightglowGlowItem : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        return item.type == ItemID.FairyQueenMagicItem;
    }

    public override Color? GetAlpha(Item item, Color lightColor)
    {
        return Color.White;
    }
}