using System.Collections.Generic;
using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Relogiced.Content.Items.VoodooRework;

public class GlobalVoodooItem : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type is ItemID.GuideVoodooDoll or ItemID.ClothierVoodooDoll;
    }

    public override void UpdateInventory(Item item, Player player)
    {
        if (item.type == ItemID.GuideVoodooDoll)
            player.killGuide = true;
        else
            player.killClothier = true;
    }

    public override bool CanRightClick(Item item)
    {
        return true;
    }

    public override void RightClick(Item item, Player player)
    {
        if (item.type == ItemID.GuideVoodooDoll)
        {
            RelogicedUtil.ChangeItemTypeFromRMB(item, ModContent.ItemType<GuideVoodooDoll_Off>());
        }
        else
        {
            RelogicedUtil.ChangeItemTypeFromRMB(item, ModContent.ItemType<ClothierVoodooDoll_Off>());
        }
    }

    public override void SetDefaults(Item item)
    {
        VoodooDollItem.DefaultToVoodooDoll(item);
        item.ResetPrefix();
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        return false;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        for (int i = 0; i < tooltips.Count; i++)
        {
            TooltipLine line = tooltips[i];
            if (line.Name == "Tooltip0" && line.Mod == "Terraria")
            {
                line.Text = ItemTooltip.FromLocalization(Mod.GetLocalization("Items.VoodooActiveTip")).GetLine(0);
                ItemTooltip t = ItemTooltip.FromLocalization(Mod.GetLocalization("Items.VoodooActiveTip"));
                for (int j = 1; j < t.Lines; j++)
                {
                    tooltips.Insert(i + j, new TooltipLine(Relogiced.Instance, "Tooltip" + j, t.GetLine(j)));
                }
                break;
            }
        }
    }
}