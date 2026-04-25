using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Hardmode.LostWispLantern;

public class LostWispLantern : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    public override void SetDefaults()
    {
        Item.damage = 60;
        Item.crit = 9;
        Item.DamageType = DamageClass.Magic;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 5);
        Item.shoot = ModContent.ProjectileType<LostWisp>();
        Item.shootSpeed = 10f;
        Item.mana = 12;
        SoundStyle sound = SoundID.Item29;
        Item.UseSound = sound.WithPitchOffset(-0.3f);
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
        Item.holdStyle = ItemHoldStyleID.HoldLamp;
        Item.flame = true;
        Item.useTurn = false;
        Item.useTurnOnAnimationStart = true;
        Item.knockBack = 0;
        Item.scale = 0.7f;
        Item.autoReuse = true;
        Item.noMelee = true;
    }
    
    public override void AddRecipes()
    {
        Recipe.Create(Type)
            .AddRecipeGroup(RecipeGroupID.IronBar, 17)
            .AddIngredient(ItemID.SoulofFright, 9)
            .AddIngredient(ItemID.SoulofSight, 4)
            .AddIngredient(ItemID.SoulofNight, 13)
            .AddIngredient(ItemID.CursedFlame, 3)
            .AddCondition(Condition.InGraveyard)
            .AddDecraftCondition(Condition.CorruptWorld)
            .Register();
        Recipe.Create(Type)
            .AddRecipeGroup(RecipeGroupID.IronBar, 17)
            .AddIngredient(ItemID.SoulofFright, 9)
            .AddIngredient(ItemID.SoulofSight, 4)
            .AddIngredient(ItemID.SoulofNight, 13)
            .AddIngredient(ItemID.Ichor, 3)
            .AddCondition(Condition.InGraveyard)
            .AddDecraftCondition(Condition.CrimsonWorld)
            .Register();
    }

    public override void HoldItem(Player player)
    {
        if (player.netLifeTime % 6 == 0 && Main.rand.NextBool() || Main.rand.NextBool(4))
        {
            Vector2 pos = player.itemLocation + Item.scale * new Vector2((float)(13 * player.direction), -20f * player.gravDir);
            pos = player.RotatedRelativePoint(pos);
            pos += new Vector2(-5, player.gravDir < 0 ? -5 : 0);
            Dust d = Dust.NewDustDirect(
                pos,
                10, 10, DustID.CoralTorch);
            d.velocity *= 0.1f;
            d.noGravity = true;
            d.velocity.Y -= 2 * player.gravDir;
            d.position -= d.scale * new Vector2(3);
            d.fadeIn = 0.3f;
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage,
        ref float knockback)
    {
        float radius = 2f * velocity.Length() / 10f;
        velocity = 0.5f * velocity + Main.rand.NextVector2Circular(radius, radius);
    }
}

public class PathfindWeaponMagicDraw : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Player.HeldItem.type == ModContent.ItemType<LostWispLantern>())
            drawInfo.weaponDrawOrder = WeaponDrawOrder.BehindBackArm;
    }
}