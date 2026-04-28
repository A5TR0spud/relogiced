using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Chekhov;

public class ChekhovVisualizer : ModProjectile
{
    public override string Texture => "Relogiced/Content/RangedOverhaul/Chekhov/Chekhov";
    private static Asset<Texture2D> texAsset;

    public override void SetStaticDefaults()
    {
        texAsset = ModContent.Request<Texture2D>(Texture);
    }

    public override void SetDefaults()
    {
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.Opacity = 0;
        Projectile.timeLeft = LIFETIME;
        Projectile.width = Projectile.height = 16;
        Projectile.hide = true;
    }

    public override bool? CanDamage()
    {
        return false;
    }
    
    private const float MAX_OPACITY = 0.7f;
    private const int LIFETIME = 70;

    public override void AI()
    {
        Projectile.Opacity = MAX_OPACITY * (float)Math.Sin(MathHelper.Pi * ((float)Projectile.timeLeft / LIFETIME));

        if (Projectile.timeLeft < 40)
        {
            Projectile.velocity.Y *= 0.98f;
            Projectile.velocity.X *= 0.9f;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers,
        List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        Color drawColor = Projectile.GetAlpha(lightColor);
        Rectangle sourceRectangle = new Rectangle(0, 0, texAsset.Width(), texAsset.Height());
        Vector2 origin = sourceRectangle.Size() * 0.5f;
        
        Main.EntitySpriteDraw(
            texture: texAsset.Value,
            position: Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            sourceRectangle: sourceRectangle,
            color: drawColor,
            rotation: Projectile.rotation,
            origin: origin,
            scale: 1,
            effects: spriteEffects,
            worthless: 0
        );
        return false;
    }

}