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
        return item.type is ItemID.EnchantedBoomerang or ItemID.EnchantedSword;
    }

    public override void SetDefaults(Item item)
    {
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