using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.EarlyGame;

public class EarlyGameMagic : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.EarlyGameWeapons;
    }

    public override void Load()
    {
        RelogicedUtil.ChangeItemSprite(ItemID.WandofSparking, "Content/MagicOverhaul/EarlyGame/WandOfSparking");
    }

    public override void Unload()
    {
        RelogicedUtil.RestoreItemSprite(ItemID.WandofSparking);
    }

    public override void AddRecipes()
    {
        Recipe.Create(ItemID.WandofSparking)
        .AddIngredient(ItemID.Torch, 99)
        .AddIngredient(ItemID.FallenStar, 5)
        .AddCondition(Condition.NotRemixWorld)
        .AddCondition(Condition.NotZenithWorld)
        .Register();
    }
}

public class EarlyGameMagicShop : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.EarlyGameWeapons || Relogiced.ConfigAssorted.ShopChanges;
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Clothier && shop.Name == "Shop")
        {
            shop.Add(ItemID.BookofSkulls, Condition.BloodMoonOrHardmode);
            shop.Add(ItemID.SkeletronHand, Condition.BloodMoonOrHardmode);
        }
    }
}