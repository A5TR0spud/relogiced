using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Other;

public class TransmuteHelper : ModSystem
{
    private static Dictionary<int, List<int>> _shimmerMap;
    private static Dictionary<int, int> _shimmerIndices;

    public override void Load()
    {
        _shimmerMap = [];
        _shimmerIndices = [];
        On_Item.GetShimmered += On_ItemOnGetShimmered;
    }

    public override void Unload()
    {
        On_Item.GetShimmered -= On_ItemOnGetShimmered;
        _shimmerMap = [];
        _shimmerIndices = [];
    }

    private void On_ItemOnGetShimmered(On_Item.orig_GetShimmered orig, Item self)
    {
        ItemID.Sets.ShimmerTransformToItem[self.type] = GetShimmer(self.type);
        orig(self);
    }

    private static int GetShimmer(int inID)
    {
        int actuallyItem = ItemID.Sets.ShimmerCountsAsItem[inID];
        if (actuallyItem != -1)
            inID = actuallyItem;
        int currentResult = ItemID.Sets.ShimmerTransformToItem[inID];
        
        RegisterShimmer(inID, currentResult);
        
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
        RegisterShimmer(inID, outID);
        for (int i = 0; i < _shimmerMap[inID].Count; i++)
        {
            if (_shimmerMap[inID][i] == expectedOutID)
                _shimmerMap[inID][i] = outID;
        }
        return true;
    }

    public static void RegisterShimmer(int inID, int outID, bool reversible = false)
    {
        if (!_shimmerMap.ContainsKey(inID))
        {
            _shimmerMap.Add(inID, [outID]);
            _shimmerIndices.Add(inID, 0);
        }
        if (!_shimmerMap[inID].Contains(outID))
            _shimmerMap[inID].Add(outID);
        
        if (reversible)
            RegisterShimmer(outID, inID);
    }
}