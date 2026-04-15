using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MeleeOverhaul.EnchantedWeaponsRework;

public class EnchantedRework : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMeleeOverhaul.EnchantedSwordRework;
    }

    public override void PostAddRecipes()
    {
        for (int i = 0; i < Recipe.numRecipes; i++) {
            Recipe recipe = Main.recipe[i];
            if (recipe == null) continue;
            if (recipe.Disabled) continue;

            if (recipe.TryGetIngredient(ItemID.WoodenBoomerang, out var _) &&
                recipe.TryGetIngredient(ItemID.FallenStar, out var _) &&
                recipe.HasResult(ItemID.EnchantedBoomerang) &&
                recipe.RemoveIngredient(ItemID.FallenStar)
            )
            {
                recipe.AddIngredient(ItemID.FallenStar, 5);
                recipe.AddTile(TileID.WorkBenches);
                return;
            }
        }
    }
}