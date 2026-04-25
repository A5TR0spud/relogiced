using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Hardmode.HauntedBrazier;

public class HauntedBrazier : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<HauntedBrazierTile>());
        Item.rare = ItemRarityID.Pink;
    }
    
    public override void AddRecipes()
    {
        Recipe.Create(Type)
            .AddIngredient(ItemID.StoneBlock, 100)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(ItemID.SoulofNight, 5)
            .AddIngredient(ItemID.CursedFlame, 5)
            .AddTile(TileID.HeavyWorkBench)
            .AddCondition(Condition.InGraveyard)
            .AddDecraftCondition(Condition.CorruptWorld)
            .Register();
        Recipe.Create(Type)
            .AddIngredient(ItemID.StoneBlock, 100)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(ItemID.SoulofNight, 5)
            .AddIngredient(ItemID.Ichor, 5)
            .AddTile(TileID.HeavyWorkBench)
            .AddCondition(Condition.InGraveyard)
            .AddDecraftCondition(Condition.CrimsonWorld)
            .Register();
    }
}