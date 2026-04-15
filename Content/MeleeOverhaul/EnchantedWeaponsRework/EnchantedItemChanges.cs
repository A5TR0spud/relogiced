using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MeleeOverhaul.EnchantedWeaponsRework;

public class EnchantedItemChanges : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMeleeOverhaul.EnchantedSwordRework;
    }

    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        return item.type is ItemID.Trimarang or ItemID.EnchantedBoomerang or ItemID.EnchantedSword;
    }

    public override void SetDefaults(Item item)
    {
        if (item.type == ItemID.Trimarang)
        {
            item.damage += 3;
            item.crit += 2;
            item.useTime -= 1;
            item.useAnimation -= 1;
            return;
        }

        if (item.type == ItemID.EnchantedBoomerang)
        {
            item.damage -= 1;
            return;
        }

        if (item.type == ItemID.EnchantedSword)
        {
            item.damage -= 2;
            return;
        }
    }
}