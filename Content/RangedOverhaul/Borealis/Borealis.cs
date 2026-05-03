using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Borealis;

public class Borealis : ModItem
{
    private const int ITEM_TIME = 16;
    public override void SetDefaults()
    {
        Item.damage = 15000;
        Item.crit = 96;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = -1;
        Item.autoReuse = false;
        Item.useAnimation = Item.useTime = ITEM_TIME;
        Item.holdStyle = ItemHoldStyleID.HoldRadio;
        Item.width = 32;
        Item.height = 20;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.knockBack = 15;
        Item.shoot = ModContent.ProjectileType<RodFromGod>();
        SoundStyle sound = SoundID.Mech;
        sound = sound.WithPitchOffset(-0.4f);
        Item.UseSound = sound;
        Item.shootSpeed = 24f;
        Item.value = Item.sellPrice(0, 20);
        Item.rare = ItemRarityID.Red;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        RelogicedUtil.AppendTooltip(tooltips,
            RelogicedUtil.GetPriceTooltip(FirePrice, "Items.Borealis.FirePrice"));
    }

    public readonly long FirePrice = Item.buyPrice(platinum: 1);

    public override void HoldItem(Player player)
    {
        if (!RelogicedUtil.DEBUG_MODE) return;
        uint cd = BorealisCooldownSystem.BorealisCooldownTimer;
        if (cd == 0) return;
        if (cd % 60 != 0) return;
        int t = (int)(cd / 60);
        CombatText.NewText(new Rectangle((int)player.Top.X, (int)player.Top.Y, 0, 0),
            Color.White,
            t);
    }

    public override void UpdateInventory(Player player)
    {
        if (player.whoAmI == Main.myPlayer && BorealisCooldownSystem.PlayRefreshSoundThisTick)
        {
            SoundStyle sound = SoundID.Item35;
            sound = sound.WithPitchOffset(3f);
            SoundEngine.PlaySound(sound);
            sound = SoundID.Item35;
            sound = sound.WithPitchOffset(-0.5f);
            SoundEngine.PlaySound(sound);
            SoundEngine.PlaySound(SoundID.MaxMana);
            for (int i = 0; i < 5; i++)
            {
                int num = Dust.NewDust(player.position, player.width, player.height, 45, 0f, 0f, 255, default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
                Main.dust[num].noLight = true;
                Main.dust[num].noGravity = true;
                Dust obj = Main.dust[num];
                obj.velocity *= 0.5f;
            }
        }
        BorealisCooldownSystem.PlayRefreshSoundThisTick = false;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        //button depressed
        Player.CompositeArmStretchAmount stretchAmount = Player.CompositeArmStretchAmount.Quarter;
        float rot = -0.9f;
        int frame = 11;
        if (player.itemAnimation > player.itemAnimationMax / 2)
        {
            //button pressed
            stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
            rot = -0.8f;
            frame = 12;
        }
        player.bodyFrame.Y = 56 * frame;
        
        player.SetCompositeArmFront(true, stretchAmount, rot * player.direction);
    }

    public override bool CanShoot(Player player)
    {
        return true;
        return player.CanAfford(FirePrice) && BorealisCooldownSystem.IsRodFromGodAvailable() && player.ownedProjectileCounts[ModContent.ProjectileType<BorealisReticle>()] == 0;
    }

    public override void ModifyWeaponCrit(Player player, ref float crit)
    {
        crit = 100;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage,
        ref float knockback)
    {
        position = Main.MouseWorld;
        velocity = Vector2.UnitY * velocity.Length();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type,
        int damage, float knockback)
    {
        if (!player.BuyItem(FirePrice))
            return false;
        NetworkHelper.Borealis_PutOnCooldown();
        Projectile.NewProjectile(
            player.GetSource_ItemUse(Item),
            position, velocity, type, damage, knockback, player.whoAmI,
            ai0: player.GetWeaponAttackSpeed(Item) * ITEM_TIME / (float)Item.useTime
        );
        return false;
    }
}

public class BorealisCooldownSystem : ModSystem
{
    internal static uint BorealisCooldownTimer = 0;
    internal const uint BOREALIS_COOLDOWN = 60 * 90;

    internal static bool BorealisCanBeUsed = Main.netMode != NetmodeID.MultiplayerClient;
    public static bool PlayRefreshSoundThisTick = false;

    public static bool IsRodFromGodAvailable()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return BorealisCooldownTimer == 0;
        return BorealisCanBeUsed;
    }

    internal static void SetState(bool usable)
    {
        PlayRefreshSoundThisTick = usable;
        BorealisCanBeUsed = usable;
        if (!usable)
            BorealisCooldownTimer = BOREALIS_COOLDOWN;
    }

    public override void PreUpdatePlayers()
    {
        //if (Main.netMode == NetmodeID.Server && BorealisCooldownTimer != 0 && BorealisCooldownTimer % 60 == 0)
        //    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(((int)(BorealisCooldownTimer / 60)).ToString()), Color.White);
        if (BorealisCooldownTimer > 0)
        {
            BorealisCooldownTimer--;
            if (BorealisCooldownTimer == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NetworkHelper.Borealis_CoolOff();
            }
        }
    }

    public override void PostUpdatePlayers()
    {
        PlayRefreshSoundThisTick = false;
    }
}

public class BorealisDrawPlayer : PlayerDrawLayer
{
    private static Asset<Texture2D> texAsset;
    private static Asset<Texture2D> texAssetGlow;

    public override void SetStaticDefaults()
    {
        texAsset = ModContent.Request<Texture2D>("Relogiced/Content/RangedOverhaul/Borealis/Borealis_Held");
        texAssetGlow = ModContent.Request<Texture2D>("Relogiced/Content/RangedOverhaul/Borealis/Borealis_Held_Glow");
    }

    public override Position GetDefaultPosition()
    {
        return new Multiple()
        {
            { new Between(PlayerDrawLayers.JimsDroneRadio, PlayerDrawLayers.Shield), drawinfo => drawinfo.drawPlayer.itemAnimation == 0 },
            
            { new Between(PlayerDrawLayers.ProjectileOverArm, PlayerDrawLayers.FrozenOrWebbedDebuff), drawinfo => drawinfo.drawPlayer.itemAnimation != 0 }
        };
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<Borealis>())// && drawInfo.drawPlayer.itemAnimation == 0)
        {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            Texture2D value = texAsset.Value;
            Vector2 pos = new Vector2(
                              (float)((int)(drawInfo.Position.X - Main.screenPosition.X -
                                          (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                                          (float)(drawInfo.drawPlayer.width / 2)) +
                                      drawInfo.drawPlayer.direction * 2),
                              (float)(int)(drawInfo.Position.Y - Main.screenPosition.Y +
                                  (float)drawInfo.drawPlayer.height -
                                  (float)drawInfo.drawPlayer.bodyFrame.Height + 4f + 14f)) +
                          drawInfo.drawPlayer.bodyPosition +
                          new Vector2((float)(drawInfo.drawPlayer.bodyFrame.Width / 2),
                              (float)(drawInfo.drawPlayer.bodyFrame.Height / 2));
            DrawData item = new DrawData(
                value,
                pos,
                bodyFrame,
                drawInfo.colorArmorLegs,
                drawInfo.drawPlayer.legRotation,
                drawInfo.legVect,
                1f,
                drawInfo.playerEffect);
            item.shader = drawInfo.cWaist;
            drawInfo.DrawDataCache.Add(item);
            value = texAssetGlow.Value;
            DrawData item2 = new DrawData(
                value,
                pos,
                bodyFrame,
                Color.White,
                drawInfo.drawPlayer.legRotation,
                drawInfo.legVect,
                1f,
                drawInfo.playerEffect);
            item2.shader = drawInfo.cWaist;
            drawInfo.DrawDataCache.Add(item2);
        }
    }
}