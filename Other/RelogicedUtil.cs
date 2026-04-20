using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Other;

public class RelogicedUtil : ModSystem
{
    public static void ChangeItemTypeFromRMB(Item item, int newItemID, bool unlockSound = false)
    {
        if (!Main.mouseRightRelease || !ChangeItemType(item, newItemID)) return;
        SoundEngine.PlaySound(unlockSound ? SoundID.Unlock : SoundID.Grab);
        Main.stackSplit = 30;
        Main.mouseRightRelease = false;
    }

    private static List<int>[] _shimmerMap = ItemID.Sets.Factory.CreateCustomSet<List<int>>(null);
    private static int[] _shimmerIndices = ItemID.Sets.Factory.CreateIntSet(0);

    public override void Load()
    {
        for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++)
        {
            _shimmerMap[i] = [ItemID.Sets.ShimmerTransformToItem[i]];
        }
        On_Item.GetShimmered += On_ItemOnGetShimmered;
    }

    public override void Unload()
    {
        On_Item.GetShimmered -= On_ItemOnGetShimmered;
        _shimmerMap = ItemID.Sets.Factory.CreateCustomSet<List<int>>(null);
        _shimmerIndices = ItemID.Sets.Factory.CreateIntSet(0);
    }

    private void On_ItemOnGetShimmered(On_Item.orig_GetShimmered orig, Item self)
    {
        ItemID.Sets.ShimmerTransformToItem[self.type] = GetShimmer(self.type);
        orig(self);
    }

    private static int GetShimmer(int inID)
    {
        int currentResult = ItemID.Sets.ShimmerTransformToItem[inID];
        if (!_shimmerMap[inID].Contains(currentResult))
        {
            RegisterShimmer(inID, currentResult);
        }
        
        for (int i = _shimmerIndices[inID]; i < _shimmerIndices[inID] + _shimmerMap[inID].Count; i++)
        {
            int idx = i % _shimmerMap[inID].Count;
            int ret = _shimmerMap[inID][idx];
            _shimmerIndices[inID] = (_shimmerIndices[inID] + 1) % _shimmerMap[inID].Count;
            //if the result is to decraft it, but there is no decraft, skip it
            if (ret <= 0 && ShimmerTransforms.GetDecraftingRecipeIndex(inID) == -1) continue;
            return ret;
        }

        return currentResult;
    }

    public static bool OverrideShimmer(int inID, int outID, int expectedOutID = -1,
        bool doNothingIfUnexpected = false)
    {
        if (ItemID.Sets.ShimmerTransformToItem[inID] != expectedOutID)
        {
            if (doNothingIfUnexpected)
                return false;
            RegisterShimmer(inID, outID);
            return true;
        }

        ItemID.Sets.ShimmerTransformToItem[inID] = outID;
        for (int i = 0; i < _shimmerMap[inID].Count; i++)
        {
            if (_shimmerMap[inID][i] == expectedOutID)
                _shimmerMap[inID][i] = outID;
        }
        return true;
    }

    public static void RegisterShimmer(int inID, int outID, bool reversible = false)
    {
        if (!_shimmerMap[inID].Contains(outID))
            _shimmerMap[inID].Add(outID);
        
        if (reversible)
            RegisterShimmer(outID, inID);
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
