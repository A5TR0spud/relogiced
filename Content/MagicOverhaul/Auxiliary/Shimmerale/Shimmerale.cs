using Microsoft.Xna.Framework;
using Relogiced.Content.MagicOverhaul.ManaRewrite;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Auxiliary.Shimmerale;

public class Shimmerale : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite && Relogiced.ConfigMagicOverhaul.AuxiliaryManaItems;
    }

    public override void SetStaticDefaults()
    {
        Item ale = new Item(ItemID.Ale);
        Item.ResearchUnlockCount = ale.ResearchUnlockCount;
        ItemID.Sets.IsFood[Type] = true;
        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(242, 182, 248),
            new Color(178, 153, 245),
            new Color(139, 82, 210)
        ];
        TransmuteHelper.OverrideShimmer(ItemID.Ale, Type);
        TransmuteHelper.OverrideShimmer(ItemID.Mug, Type);
        
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(-1, 3)
        {
            NotActuallyAnimating = true
        });
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(22, 22, ModContent.BuffType<Starstruck>(), 7200, useGulpSound: true);
        Item.rare = ItemRarityID.LightPurple;
        Item.value = 100;
        Item.holdStyle = ItemHoldStyleID.HoldFront;
    }
}