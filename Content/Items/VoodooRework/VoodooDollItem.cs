using System.Collections.Generic;
using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public abstract class VoodooDollItem : ModItem
{
    public abstract List<int> AssociatedNPCs();
    public abstract int OtherVariant();
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    public override LocalizedText Tooltip => AssociatedNPCs().TrueForAll(i => i == 0)
        ? Mod.GetLocalization("Items.VoodooInactiveTip")
        : Mod.GetLocalization("Items.VoodooActiveTip");

    public override bool CanRightClick()
    {
        return true;
    }

    public override void RightClick(Player player)
    {
        RelogicedUtil.ChangeItemTypeFromRMB(Item, OtherVariant());
    }

    public override bool ConsumeItem(Player player)
    {
        return false;
    }

    public static void DefaultToVoodooDoll(Item item)
    {
        item.accessory = false;
        item.maxStack = 1;
        item.value = Item.buyPrice(silver: 50);
    }

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.Blue;
    }

    public override void UpdateInventory(Player player)
    {
        ModVoodooPlayer plr = player.GetModPlayer<ModVoodooPlayer>();
        foreach (int associatedNPC in AssociatedNPCs())
        {
            if (associatedNPC == 0 || plr.KillableNPCs.Contains(associatedNPC))
                continue;
            plr.KillableNPCs.Add(associatedNPC);
        }
    }
}