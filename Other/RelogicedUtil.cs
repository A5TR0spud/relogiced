using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Other;

public class RelogicedUtil : ModSystem
{
    public static bool ChangeItemTypeFromRMB(Item item, int newItemID, bool unlockSound = false)
    {
        if (!Main.mouseRightRelease || !ChangeItemType(item, newItemID)) return false;
        SoundEngine.PlaySound(unlockSound ? SoundID.Unlock : SoundID.Grab);
        Main.stackSplit = 30;
        Main.mouseRightRelease = false;
        return true;
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
