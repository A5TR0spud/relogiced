using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Content.RangedOverhaul.RodFromGodItem;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.Abstracts;

public abstract class HandheldButtonItem : ModItem
{
    public abstract bool UseHeavySound();

    public abstract bool UseGlowMask();

    internal static Dictionary<int, Asset<Texture2D>> TexturesHeld = [];
    internal static Dictionary<int, Asset<Texture2D>> TexturesHeldGlow = [];

    public override void SetStaticDefaults()
    {
        if (!TexturesHeld.ContainsKey(Type))
            TexturesHeld.Add(Type, ModContent.Request<Texture2D>(Texture + "_Held"));
        if (!TexturesHeldGlow.ContainsKey(Type) && UseGlowMask())
            TexturesHeldGlow.Add(Type, ModContent.Request<Texture2D>(Texture + "_Held_Glow"));
    }

    public override void SetDefaults()
    {
        Item.useStyle = -1;
        Item.autoReuse = true;
        Item.useAnimation = Item.useTime = 16;
        Item.holdStyle = ItemHoldStyleID.HoldRadio;
        Item.width = 16;
        Item.height = 16;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        SoundStyle sound = SoundID.Mech;
        if (UseHeavySound())
        {
            sound = sound.WithPitchOffset(-0.4f);
            sound = sound.WithVolumeScale(0.9f);
        }
        Item.UseSound = sound;
    }

    public override void UseAnimation(Player player)
    {
        if (!UseHeavySound()) return;
        SoundStyle sound = SoundID.Item108;
        sound = sound.WithPitchOffset(1.5f).WithVolumeScale(0.15f);
        SoundEngine.PlaySound(sound, player.whoAmI == Main.myPlayer ? null : player.Center);
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
}

public class HandheldButtonItemDrawPlayer : PlayerDrawLayer
{
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
        if (drawInfo.drawPlayer.HeldItem.ModItem is HandheldButtonItem buttonItem)
        {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
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
                HandheldButtonItem.TexturesHeld[buttonItem.Type].Value,
                pos,
                bodyFrame,
                drawInfo.colorArmorLegs,
                drawInfo.drawPlayer.legRotation,
                drawInfo.legVect,
                1f,
                drawInfo.playerEffect);
            item.shader = drawInfo.cWaist;
            drawInfo.DrawDataCache.Add(item);
            if (buttonItem.UseGlowMask())
            {
                DrawData item2 = new DrawData(
                    HandheldButtonItem.TexturesHeldGlow[buttonItem.Type].Value,
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
}