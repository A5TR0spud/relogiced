using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.Hardmode.LostWispLantern;

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
            if (proj.type == ModContent.ProjectileType<LostWisp>())
                SeekProjectiles.Add(proj);
        }
    }
}

public class LostWisp : ModProjectile
{    
    private static Asset<Texture2D> texAsset;
    
    public override void Load()
    {
        texAsset = ModContent.Request<Texture2D>(Texture);
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

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_TileUpdate)
        {
            Projectile.maxPenetrate = 1;
            Projectile.penetrate = 1;
            Projectile.timeLeft += 60;
        }
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

    private List<Node> _openNodes = null;
    private List<Node> _closedNodes = null;
    private List<Point> _path = null;
    private int _tries = 0;

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

        if (nearestEnemy.whoAmI != Target)
        {
            _path = null;
            _tries = 0;
            _openNodes = [];
            _closedNodes = [];
            Target = nearestEnemy.whoAmI;
        }

        bool pathfindingDisabled = Relogiced.ConfigPerformance.LostWispLanternMaximumFramesToCheck == 0 ||
                                   Relogiced.ConfigPerformance.LostWispLanternAbsoluteMaximumTileCheck == 0 ||
                                   Relogiced.ConfigPerformance.LostWispLanternMaximumTileCheck == 0;

        if (pathfindingDisabled)
        {
            Projectile.tileCollide = false;
        }
        if (pathfindingDisabled || Collision.CanHit(Projectile, nearestEnemy))
        {
            Projectile.velocity *= 0.98f;
            Projectile.velocity += 0.3f * (nearestEnemy.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
            return;
        }
        List<Point> steps = PathfindingHelper.AStarIterative(
            Projectile.Center.ToTileCoordinates(),
            nearestEnemy.Center.ToTileCoordinates(),
            ref _openNodes,
            ref _closedNodes,
            out bool complete,
            width: Projectile.width,
            height: Projectile.height,
            failureMode: PathfindingHelper.FailureMode.PathToBest,
            maxTilesToCheck: (int)Math.Min(Relogiced.ConfigPerformance.LostWispLanternMaximumTileCheck,
                Relogiced.ConfigPerformance.LostWispLanternAbsoluteMaximumTileCheck /
                SeekProjectileTicker.SeekProjectiles.Count),
            costFunction: (from, to) =>
            {
                bool fromSolid = Collision.SolidTiles(from.X, from.X, from.Y, from.Y);
                bool toSolid = Collision.SolidTiles(to.X, to.X, to.Y, to.Y);
                if (!fromSolid && toSolid)
                    return null;
                int num = 10;
                foreach (Projectile proj in SeekProjectileTicker.SeekProjectiles)
                {
                    if (proj.whoAmI == Projectile.whoAmI) continue;
                    if (to.ToWorldCoordinates().DistanceSQ(proj.Center) < 16 * 16)
                        num += 50;
                }

                if (!fromSolid)
                {
                    if (Collision.IsWorldPointSolid(to.ToWorldCoordinates(8, 24)))
                        num += 3;
                    if (Collision.IsWorldPointSolid(to.ToWorldCoordinates(24, 0)))
                        num += 3;
                    if (Collision.IsWorldPointSolid(to.ToWorldCoordinates(8, -8)))
                        num += 3;
                    if (Collision.IsWorldPointSolid(to.ToWorldCoordinates(-8, 8)))
                        num += 3;
                }

                if (toSolid)
                {
                    num += 15;
                }

                return num;
            }
        );

        bool totalFailure = _tries > Relogiced.ConfigPerformance.LostWispLanternMaximumFramesToCheck;

        if (complete || totalFailure)
        {
            _path = steps;
            _openNodes = null;
            _closedNodes = null;
            _tries = 0;
        }
        else
        {
            _tries++;
        }

        if (_path == null || _path.Count == 0)
        {
            Projectile.tileCollide = false;
            return;
        }

        /*foreach (Point p in _path)
        {
            Dust.NewDustPerfect(p.ToWorldCoordinates(), DustID.Clentaminator_Green, Vector2.Zero);
        }*/

        Projectile.tileCollide =
            !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);

        Point? nextStep = PathfindingHelper.NextStep(Projectile, _path);

        Projectile.velocity *= 0.98f;
        if (nextStep == null) return;

        Vector2 force =
            0.3f * (nextStep.Value.ToWorldCoordinates() - Projectile.Center).SafeNormalize(Vector2.Zero);
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