using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.EarlyGame.MagicWand;

public class MagicWand : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.EarlyGameWeapons;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(gold: 2);
        Item.damage = 8;
        Item.DamageType = DamageClass.Magic;
        Item.knockBack = 1;
        Item.mana = 4;
        Item.crit = 6;
        Item.noMelee = true;
        Item.useTime = 10;
        Item.useAnimation = 30;
        Item.reuseDelay = 4;
        Item.shootSpeed = 7;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shoot = ModContent.ProjectileType<Suits>();
        Item.scale = 0.9f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
        int damage, float knockback)
    {
        Vector2 newPos = position + new Vector2(
            Main.rand.NextFloat(-10, 10),
            Main.rand.NextFloat(-10, 10)
        );
        Vector2 newVel = velocity.Length() * (Main.MouseWorld - newPos).SafeNormalize(velocity);
        bool useNewPos = Collision.CanHit(player.Center, 0, 0, newPos - new Vector2(9), 18, 18);
        Projectile.NewProjectile(source,
            useNewPos ? newPos : position,
            useNewPos ? newVel : position,
            type, damage, knockback, player.whoAmI, ai0: Main.rand.Next(4)
        );
        if (Main.rand.Next(100) < player.GetWeaponCrit(Item))
        {
            Projectile dove = Projectile.NewProjectileDirect(source, position, velocity, ProjectileID.ReleaseDoves, damage, 0, player.whoAmI);
            dove.friendly = true;
        }
        return false;
    }
}

public class MagicWandShop : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.EarlyGameWeapons;
    }
    
    public override void SetupTravelShop(int[] shop, ref int nextSlot)
    {
        int idx = -1;
        for (int i = 0; i < shop.Length; i++)
        {
            if (shop[i] == ItemID.MagicHat)
            {
                idx = i;
                break;
            }
        }

        if (idx <= -1 || idx + 1 > shop.Length) return;

        for (int i = shop.Length; i > idx; i--)
        {
            shop[i] = shop[i - 1];
        }

        shop[idx + 1] = ModContent.ItemType<MagicWand>();
        nextSlot++;
    }
}