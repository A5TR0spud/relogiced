using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.ConditionFillers;

public class ConditionFillers : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.ConditionFillers;
    }

    public override void SetStaticDefaults()
    {
        TileID.Sets.CountsAsHoneySource[TileID.HoneyDispenser] = true;
        TileID.Sets.CountsAsLavaSource[TileID.LavafishBowl] = true;
    }
}