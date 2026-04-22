using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Hardmode.PathfindWeapon;

public class SeekProjectileTicker : ModSystem
{
    public static List<Projectile> SeekProjectiles = [];
    
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    public override void PreUpdateProjectiles()
    {
        SeekProjectiles.Clear();
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.type == ModContent.ProjectileType<SeekProjectile>())
                SeekProjectiles.Add(proj);
        }
    }
}

public class SeekProjectile : ModProjectile
{    
    private static Asset<Texture2D> texAsset;
    
    public override void Load()
    {
        texAsset = Relogiced.Instance.Assets.Request<Texture2D>("Content/MagicOverhaul/Hardmode/PathfindWeapon/SeekProjectile");
    }

    public int Target
    {
        get => (int)Projectile.ai[0] - 1;
        set => Projectile.ai[0] = value + 1;
    }

    public int FadeIn
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }
    
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
    }

    private const int FADE_IN_TIME = 30;
    private const int FRAMERATE = 12;
    private const int FADE_OUT_TIME = 30;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.penetrate = 4;
        Projectile.ArmorPenetration = 15;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.timeLeft = 60 * 10;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
        FadeIn = FADE_IN_TIME;
        Projectile.Opacity = 0;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }

    public override bool? CanDamage()
    {
        if (Projectile.timeLeft < FADE_OUT_TIME / 2)
            return false;
        return base.CanDamage();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        Color drawColor = Projectile.GetAlpha(lightColor);
        Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * texAsset.Height() / Main.projFrames[Type], texAsset.Width(), texAsset.Height() / Main.projFrames[Type] - 2);
        Vector2 origin = sourceRectangle.Size() / 2f;
        
        Main.EntitySpriteDraw(
            texture: texAsset.Value,
            position: Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY - 6),
            sourceRectangle: sourceRectangle,
            color: drawColor,
            rotation: Projectile.rotation,
            origin: origin,
            scale: Projectile.scale,
            effects: spriteEffects,
            worthless: 0
        );
        return false;
    }

    public override bool? CanCutTiles()
    {
        return false;
    }

    public override void AI()
    {
        Projectile.velocity *= 0.97f;
        Projectile.rotation = -Projectile.velocity.X * 0.1f;

        if (Projectile.damage <= 0 && Projectile.timeLeft > FADE_OUT_TIME && Projectile.penetrate <= 0)
            Projectile.timeLeft = FADE_OUT_TIME;

        Projectile.frame = (Projectile.frameCounter / 60) % Main.projFrames[Type];
        Projectile.spriteDirection = Projectile.frameCounter % (120 * Main.projFrames[Type]) - 60 * Main.projFrames[Type] >= 0 ? 1 : -1;
        Projectile.frameCounter += FRAMERATE;
        
        if (Projectile.timeLeft % 3 == 0 && Main.rand.NextBool() || Main.rand.NextBool())
        {
            Dust d = Dust.NewDustDirect(Projectile.position + new Vector2(0, -16),
                Projectile.width, Projectile.height + 16,
                DustID.CoralTorch, Alpha: Projectile.alpha);
            d.velocity *= 0.1f;
            d.noGravity = true;
            d.velocity.Y -= 2;
            d.position -= d.scale * new Vector2(3);
            d.fadeIn = 0.3f;
        }
        
        if (Projectile.timeLeft < FADE_OUT_TIME)
        {
            Projectile.tileCollide = false;
            Projectile.Opacity -= 1f / FADE_OUT_TIME;
            return;
        }
        if (FadeIn > 0)
        {
            Projectile.Opacity += 1f / FADE_IN_TIME;
        }
        FadeIn--;

        NPC nearestEnemy = Projectile.FindTargetWithinRange(16 * 200);

        if (nearestEnemy == null)
        {
            Target = -1;
            Projectile.tileCollide = false;
            return;
        }

        Target = nearestEnemy.whoAmI;

        List<Point> steps = PathfindingHelper.AStar(
            Projectile.Center.ToTileCoordinates(),
            nearestEnemy.Center.ToTileCoordinates(),
            width: Projectile.width,
            height: Projectile.height,
            maxTilesToCheck: (int)Math.Min(Relogiced.ConfigMagicOverhaul.LostWispLanternMaximumTileCheck, Relogiced.ConfigMagicOverhaul.LostWispLanternAbsoluteMaximumTileCheck / SeekProjectileTicker.SeekProjectiles.Count),
            costFunction: (from, to) =>
            {
                int num = 1;
                foreach (Projectile proj in SeekProjectileTicker.SeekProjectiles)
                {
                    if (proj.whoAmI == Projectile.whoAmI) continue;
                    if (to.ToWorldCoordinates().DistanceSQ(proj.Center) < 16 * 16)
                        num += 16;
                }

                return num;
            }
        );
        
        Projectile.tileCollide = steps.Count > 1 && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);

        Point nextStep = PathfindingHelper.NextStep(Projectile, steps);

        Projectile.velocity *= 0.98f;
        Vector2 force = 0.3f * (nextStep.ToWorldCoordinates() - Projectile.Center).SafeNormalize(Vector2.Zero);
        //if (Projectile.Center.DistanceSQ(nextStep.ToWorldCoordinates()) < 16 * 16 && Projectile.velocity.LengthSquared() > 4)
        //    Projectile.velocity *= 0.1f;
        if (Math.Sign(Projectile.velocity.X + force.X) != Math.Sign(force.X))
            Projectile.velocity.X *= 0.7f;
        if (Math.Sign(Projectile.velocity.Y + force.Y) != Math.Sign(force.Y))
            Projectile.velocity.Y *= 0.7f;
        Projectile.velocity += force;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
}