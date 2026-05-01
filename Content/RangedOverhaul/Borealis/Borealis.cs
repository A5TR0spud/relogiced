using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Borealis;

public class Borealis : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 10000;
        Item.crit = 96;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.autoReuse = false;
        Item.useAnimation = 32;
        Item.holdStyle = ItemHoldStyleID.HoldRadio;
        Item.useTime = 32;
        Item.width = 32;
        Item.height = 20;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<RodFromGod>();
        SoundStyle sound = SoundID.Item35;
        sound = sound.WithPitchOffset(-0.2f);
        Item.UseSound = sound;
        Item.shootSpeed = 24f;
        Item.value = Item.buyPrice(0, 10);
        Item.rare = ItemRarityID.Orange;
    }

    public override bool CanUseItem(Player player)
    {
        return true;//BorealisCooldownSystem.BorealisIsOffCooldown;
    }

    public override void ModifyWeaponCrit(Player player, ref float crit)
    {
        crit = 1;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage,
        ref float knockback)
    {
        position = Main.MouseWorld;
        velocity = Vector2.UnitY * velocity.Length();
    }

    /*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
        int damage, float knockback)
    {
        if (BorealisCooldownSystem.BorealisIsOnCooldown)
            return false;
        BorealisCooldownSystem.BorealisIsOnCooldown = true;
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }*/
}

public class BorealisCooldownSystem : ModSystem
{
    private static uint _borealisCooldownTimer = 0;
    public const uint BOREALIS_COOLDOWN = 60 * 90;
    public static bool BorealisIsOnCooldown
    {
        get => _borealisCooldownTimer == 0;
        set
        {
            if (value)
                _borealisCooldownTimer = BOREALIS_COOLDOWN;
            else
                _borealisCooldownTimer = 0;
        }
    }

    private bool needsToSync = false;
    private uint lastTimer = 0;

    public static bool BorealisIsOffCooldown
    {
        get => !BorealisIsOnCooldown;
        set => BorealisIsOnCooldown = !value;
    }

    public override void PreUpdatePlayers()
    {
        if (_borealisCooldownTimer > 0)
        {
            _borealisCooldownTimer--;
        }

        if (_borealisCooldownTimer != lastTimer && _borealisCooldownTimer == 0)
        {
            needsToSync = true;
        }

        lastTimer = _borealisCooldownTimer;
    }
}

public class BorealisDrawPlayer : PlayerDrawLayer
{
    private static Asset<Texture2D> texAsset;

    public override void SetStaticDefaults()
    {
        texAsset = ModContent.Request<Texture2D>("Relogiced/Content/RangedOverhaul/Borealis/Borealis_Held");
    }

    public override Position GetDefaultPosition()
    {
        return PlayerDrawLayers.JimsDroneRadio.GetDefaultPosition();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<Borealis>())// && drawInfo.drawPlayer.itemAnimation == 0)
        {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            Texture2D value = texAsset.Value;
            DrawData item = new DrawData(
                value,
                new Vector2((float)((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2)) + drawInfo.drawPlayer.direction * 2),
                    (float)(int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.bodyFrame.Height + 4f + 14f)) + drawInfo.drawPlayer.bodyPosition + new Vector2((float)(drawInfo.drawPlayer.bodyFrame.Width / 2),
                    (float)(drawInfo.drawPlayer.bodyFrame.Height / 2)), bodyFrame, drawInfo.colorArmorLegs, drawInfo.drawPlayer.legRotation,
                drawInfo.legVect,
                1f,
                drawInfo.playerEffect);
            item.shader = drawInfo.cWaist;
            drawInfo.DrawDataCache.Add(item);
        }
    }
}