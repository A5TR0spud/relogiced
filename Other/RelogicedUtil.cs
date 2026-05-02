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
