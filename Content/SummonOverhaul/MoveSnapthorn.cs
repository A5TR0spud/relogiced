using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.SummonOverhaul;

public class MoveSnapthornNPC : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigSummonOverhaul.MoveSnapthorn;
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.WitchDoctor && shop.Name == "Shop")
        {
            shop.InsertAfter(ItemID.Blowgun, ItemID.ThornWhip);
        }
    }
}

public class MoveSnapthornSystem : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigSummonOverhaul.MoveSnapthorn;
    }

    public override void PostAddRecipes()
    {
        for (int i = 0; i < Recipe.numRecipes; i++) {
            Recipe recipe = Main.recipe[i];
            if (recipe == null) continue;
            if (recipe.Disabled) continue;

            if (recipe.TryGetIngredient(ItemID.Stinger, out var _) &&
                recipe.TryGetIngredient(ItemID.Vine, out var _) &&
                recipe.TryGetIngredient(ItemID.JungleSpores, out var _) &&
                recipe.HasResult(ItemID.ThornWhip)
               )
            {
                recipe.DisableRecipe();
                return;
            }
        }
    }
}