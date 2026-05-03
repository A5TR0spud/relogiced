using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Relogiced.Other;

public class RelogicedUtil : ModSystem
{
    public static bool DEBUG_MODE => Relogiced.ConfigClient.DebugMode;
    public static bool ChangeItemTypeFromRMB(Item item, int newItemID, bool unlockSound = false)
    {
        if (!Main.mouseRightRelease || !ChangeItemType(item, newItemID)) return false;
        SoundEngine.PlaySound(unlockSound ? SoundID.Unlock : SoundID.Grab);
        Main.stackSplit = 30;
        Main.mouseRightRelease = false;
        return true;
    }

    public static ItemTooltip GetPriceTooltip(long price, string tooltipKey)
    {
        string priceText = "";
		long copper = 0L;
		long silver = 0L;
		long gold = 0L;
		long platinum = 0L;
		if (price < 1)
		{
			price = 1L;
		}
		if (price >= 1000000)
		{
			copper = price / 1000000;
			price -= copper * 1000000;
		}
		if (price >= 10000)
		{
			silver = price / 10000;
			price -= silver * 10000;
		}
		if (price >= 100)
		{
			gold = price / 100;
			price -= gold * 100;
		}
		if (price >= 1)
		{
			platinum = price;
		}
		if (copper > 0)
		{
			priceText = priceText + copper + " " + Lang.inter[15].Value + " ";
		}
		if (silver > 0)
		{
			priceText = priceText + silver + " " + Lang.inter[16].Value + " ";
		}
		if (gold > 0)
		{
			priceText = priceText + gold + " " + Lang.inter[17].Value + " ";
		}
		if (platinum > 0)
		{
			priceText = priceText + platinum + " " + Lang.inter[18].Value + " ";
		}

		return ItemTooltip.FromLocalization(Relogiced.Instance.GetLocalization(tooltipKey).WithFormatArgs(priceText.Trim()));
    }

    public static void AppendTooltip(List<TooltipLine> tooltips, ItemTooltip toAdd, int insertionIndex = -1)
    {
	    for (int i = 0; i < toAdd.Lines; i++)
	    {
		    int idx = insertionIndex < 0 || insertionIndex > tooltips.Count ? -1 : insertionIndex++;
		    TooltipLine line = new TooltipLine(Relogiced.Instance, "Tooltip" + i, toAdd.GetLine(i));
		    if (idx < 0)
			    tooltips.Add(line);
		    else
			    tooltips.Insert(insertionIndex, line);
	    }
    }

    public static void ReplaceTooltip(List<TooltipLine> tooltips, string newTooltipKey)
    {
        bool delete = false;
        int firstIndex = 0;
        int indicesToDelete = 0;
        for (int i = 0; i < tooltips.Count; i++)
        {
            TooltipLine line = tooltips[i];
            if (line.Name.StartsWith("Tooltip"))
            {
                if (delete)
                {
                    indicesToDelete++;
                    continue;
                }
                ItemTooltip t = ItemTooltip.FromLocalization(Relogiced.Instance.GetLocalization(newTooltipKey));
                line.Text = t.GetLine(0);
                firstIndex = i;
                for (int j = 1; j < t.Lines; j++)
                {
                    firstIndex++;
                    tooltips.Insert(i + j, new TooltipLine(Relogiced.Instance, "Tooltip" + j, t.GetLine(j)));
                }
                delete = true;
            }
        }
        tooltips.RemoveRange(firstIndex, indicesToDelete);
    }

    public static bool ChangeItemType(Item item, int newItemID)
    {
        if (item.type == newItemID || item.IsAir || newItemID == 0) return false;
        item.ChangeItemType(newItemID);
        Recipe.FindRecipes();
        return true;
    }

    public static Asset<Texture2D> GetAsset(string suffix)
    {
        return Relogiced.Instance.Assets.Request<Texture2D>(suffix);
    }

    public static void ChangeItemSprite(int item, string suffix)
    {
        TextureAssets.Item[item] = Relogiced.Instance.Assets.Request<Texture2D>(suffix);
    }

    public static void RestoreItemSprite(int item)
    {
        TextureAssets.Item[item] = Main.Assets.Request<Texture2D>("Images/Item_" + item);
    }
}
