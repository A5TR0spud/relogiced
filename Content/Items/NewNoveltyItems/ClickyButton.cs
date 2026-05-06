using Relogiced.Content.Items.Abstracts;
using Relogiced.Content.RangedOverhaul.RodFromGodItem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.NewNoveltyItems;

public class ClickyButton : HandheldButtonItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.NewNoveltyItems || RodFromGodItem.IsEnabled;
    }

    public override bool UseHeavySound() => false;
    public override bool UseGlowMask() => false;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = ItemRarityID.Green;
    }

    public override void AddRecipes()
    {
        Recipe.Create(Type)
            .AddRecipeGroup(RecipeGroupID.IronBar, 2)
            .AddRecipeGroup(RecipeGroupID.PressurePlate)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}