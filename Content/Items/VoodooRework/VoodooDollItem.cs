using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public abstract class VoodooDollItem : ModItem
{
    public abstract int AssociatedNPC();
    public abstract int OtherVariant();
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.Config_Reworks.VoodooRework;// && !GetType().Name.Equals("VoodooDollItem");
    }

    public override LocalizedText Tooltip => AssociatedNPC() == 0
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
}