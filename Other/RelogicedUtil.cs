using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Relogiced.Other;

public class RelogicedUtil
{
    public static void ChangeItemTypeFromRMB(Item item, int newItemID, bool unlockSound = false)
    {
        if (!ChangeItemType(item, newItemID) || !Main.mouseRightRelease) return;
        SoundEngine.PlaySound(unlockSound ? SoundID.Unlock : SoundID.Grab);
        Main.stackSplit = 30;
        Main.mouseRightRelease = false;
    }

    public static bool ChangeItemType(Item item, int newItemID)
    {
        if (item.type == newItemID || item.IsAir || newItemID == 0) return false;
        item.ChangeItemType(newItemID);
        Recipe.FindRecipes();
        return true;
    }
}