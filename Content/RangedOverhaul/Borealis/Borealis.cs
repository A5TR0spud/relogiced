using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Relogiced.Content.RangedOverhaul.Borealis;

public class Borealis : ModItem
{
    public override void Load()
    {
        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += On_ItemSlotOnDraw_SpriteBatch_ItemArray_int_int_Vector2_Color;
    }

    public override void Unload()
    {
        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= On_ItemSlotOnDraw_SpriteBatch_ItemArray_int_int_Vector2_Color;
    }

    private void On_ItemSlotOnDraw_SpriteBatch_ItemArray_int_int_Vector2_Color(
        On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig,
        SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
    {
        orig(spriteBatch, inv, context, slot, position, lightColor);
        Player player = Main.LocalPlayer;
        Item item = inv[slot];
        bool canBeUsed = BorealisCooldownSystem.BorealisCanBeUsed;
        bool canAfford = player.CanAfford(FirePrice);
        if (context == 13 && !item.IsAir && item.type == Type && (!canBeUsed || !canAfford))
        {
            Texture2D value = TextureAssets.InventoryBack.Value;
            float inventoryScale = Main.inventoryScale;
            Color color = Color.White;
            if (lightColor != Color.Transparent)
            {
                color = lightColor;
            }

            float strengthOfNo = 0f;
            if (!canBeUsed)
            {
                strengthOfNo = BorealisCooldownSystem.BorealisCooldownTimer /
                               (float)BorealisCooldownSystem.BOREALIS_COOLDOWN;
            }
            if (!canAfford)
            {
                strengthOfNo = 1f;
            }
            //Vector2 vector = value.Size() * inventoryScale;
            float scale = Main.inventoryScale;//ItemSlot.DrawItemIcon(item, context, spriteBatch, position + vector / 2f, inventoryScale, 32f, color);
            Vector2 position2 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
            Color color3 = item.GetAlpha(color) * strengthOfNo;
            spriteBatch.Draw(TextureAssets.Cd.Value, position2, (Rectangle?)null, color3, 0f, default(Vector2), scale, (SpriteEffects)0, 0f);
        }
    }


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
        Item.shoot = ModContent.ProjectileType<BorealisReticle>();
        SoundStyle sound = SoundID.Mech;
        sound = sound.WithPitchOffset(-0.4f);
        sound = sound.WithVolumeScale(0.9f);
        Item.UseSound = sound;
        Item.shootSpeed = 24f;
        Item.value = Item.sellPrice(0, 20);
        Item.rare = ItemRarityID.Red;
    }

    public override void UseAnimation(Player player)
    {
        SoundStyle sound = SoundID.Item108;
        sound = sound.WithPitchOffset(1.5f).WithVolumeScale(0.15f);
        SoundEngine.PlaySound(sound, player.whoAmI == Main.myPlayer ? null : player.Center);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        RelogicedUtil.AppendTooltip(tooltips,
            RelogicedUtil.GetPriceTooltip(FirePrice, "Items.Borealis.FirePrice"));
    }

    public static readonly long FirePrice = Item.buyPrice(platinum: 1);

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
            sound = sound.WithPitchOffset(5f);
            SoundEngine.PlaySound(sound);
            sound = SoundID.Unlock;
            sound = sound.WithPitchOffset(0.5f);
            SoundEngine.PlaySound(sound);
            sound = SoundID.MaxMana;
            sound = sound.WithPitchOffset(-0.5f);
            SoundEngine.PlaySound(sound);
            for (int i = 0; i < 5; i++)
            {
                int num = Dust.NewDust(player.position, player.width, player.height, DustID.ManaRegeneration, 0f, 0f, 255, default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
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
        return player.CanAfford(FirePrice) && BorealisCooldownSystem.IsRodFromGodAvailable() && player.ownedProjectileCounts[Item.shoot] == 0;
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
            source,
            position, velocity, type, damage, knockback, player.whoAmI,
            ai0: player.GetWeaponAttackSpeed(Item) * ITEM_TIME / (float)Item.useTime
        );
        return false;
    }
}

public class BorealisPlayer : ModPlayer
{
    public override void PostItemCheck()
    {
        Item item = Player.HeldItem;
        if (Player.cursorItemIconEnabled || !string.IsNullOrEmpty(Player.cursorItemIconText) || item.IsAir || item.type != ModContent.ItemType<Borealis>()) return;
        bool canAfford = Player.CanAfford(Borealis.FirePrice);
        bool canUse = BorealisCooldownSystem.BorealisCanBeUsed;
        if (canAfford && canUse) return;
        Player.cursorItemIconEnabled = true;
        //Player.cursorItemIconPush = 0;
        int tMinus = (int)((BorealisCooldownSystem.BorealisCooldownTimer - 1) / 60 + 1);
        Player.cursorItemIconText = !canAfford
            ? RelogicedUtil.GetPriceTooltip(Borealis.FirePrice, "Items.Borealis.FirePrice").GetLine(0)
            : Relogiced.Instance.GetLocalization("Items.Borealis.OnCooldown").WithFormatArgs(tMinus).Value;
        Player.cursorItemIconID = -1;//ModContent.ItemType<CoolDownIcon>();
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