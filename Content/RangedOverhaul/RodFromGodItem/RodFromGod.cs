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

namespace Relogiced.Content.RangedOverhaul.RodFromGodItem;

public class RodFromGod : ModProjectile
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return RodFromGodItem.IsEnabled;
    }

    public ref float HighestSpeed => ref Projectile.ai[0];
    public float SpeedScalar => Speed / HighestSpeed;

    public bool FadingOut => Projectile.timeLeft < 60 * (1 + Projectile.extraUpdates);
    public float Speed => Projectile.velocity.Length() * (1 + Projectile.extraUpdates);
    public bool TooSlow => Speed <= STOPPING_SPEED;
    private static Asset<Texture2D> texAsset;
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CanHitPastShimmer[Type] = true;
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 19;
        ProjectileID.Sets.NeedsUUID[Type] = true;
        ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        texAsset = ModContent.Request<Texture2D>(Texture);
    }

    public override void SetDefaults()
    {
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.extraUpdates = 8;
        Projectile.height = 16 * 15;
        Projectile.width = 16 * 5;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.timeLeft = 60 * 60 * (1 + Projectile.extraUpdates);
        Projectile.hide = true;
        Projectile.netImportant = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        HighestSpeed = Speed;
        Projectile.position.Y = 1;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers,
        List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        float xDist = Math.Max(Math.Abs(target.Center.X - Projectile.Center.X) - Projectile.width / 2, 1);
        float distScalar = 1f / xDist;
        modifiers.FinalDamage *= SpeedScalar;
        modifiers.Knockback *= SpeedScalar * distScalar;
        modifiers.SetCrit();
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        float xDist = Math.Max(Math.Abs(target.Center.X - Projectile.Center.X) - Projectile.width / 2, 1);
        float distScalar = 1f / xDist;
        modifiers.FinalDamage *= SpeedScalar;
        modifiers.Knockback *= SpeedScalar * distScalar;
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
    }

    private const float STOPPING_SPEED = 0.3f;
    private const int IGNORE_TILES = 4;
    private const float STOPPING_FRACTION = 0.8f;

    public override bool? CanDamage()
    {
        return !FadingOut && !TooSlow;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        int fireball = (int)(16 * 8 * SpeedScalar);
        hitbox.Inflate(fireball, fireball / 4);
    }

    public override bool? CanHitNPC(NPC target)
    {
        int realLife = target.realLife;
        if (realLife >= 0 && realLife < Main.maxNPCs && Projectile.localNPCImmunity[realLife] != 0)
        {
            return false;
        }
        return null;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        int realLife = target.realLife;
        if (realLife >= 0 && realLife < Main.maxNPCs)
        {
            Projectile.localNPCImmunity[realLife] = Projectile.localNPCHitCooldown;
        }
    }

    public float olderVelocity = 0;

    public override void AI()
    {
        HighestSpeed = Math.Max(HighestSpeed, Speed);
        if (FadingOut)
        {
            Projectile.Opacity = Projectile.timeLeft / (60f * (1 + Projectile.extraUpdates));
            Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.LunarOre,
                newColor: new Color(79, 79, 79)).velocity *= 0.1f;
        }
        
        Projectile.velocity *= 0.998f;
        Projectile.velocity.X *= 0.9f;
        Projectile.ArmorPenetration = (int)Speed;
        int collidingTiles = 0;
        int totalTiles = 0;
        int widthSteps = Projectile.width / 16;
        int heightSteps = Projectile.height / 32;
        int flipFlop = Projectile.timeLeft % 2;
        List<Point> collisionPoints = [];
        bool firstOrSecondFrameOfUpdates = Projectile.timeLeft % (Projectile.extraUpdates + 1) <= 1;
        for (int i = 0; i <= widthSteps; i++)
        {
            for (int j = 0; j < heightSteps; j++)
            {
                int weight = j / 3 + 1;
                totalTiles += weight;
                float u = Projectile.position.X + Projectile.width * (i / (float)widthSteps);
                float v = Projectile.position.Y + Projectile.height * (j / (float)heightSteps);
                if (i % 2 == flipFlop)
                    v += 16;
                int t = DustID.Clentaminator_Blue;
                if (WorldGen.InWorld((int)u / 16, (int)v / 16) && Collision.IsWorldPointSolid(new Vector2(u, v), true))
                {
                    collidingTiles += weight;
                    t = DustID.Clentaminator_Red;
                    collisionPoints.Add(new Point((int)u / 16, (int)v / 16));
                    if (firstOrSecondFrameOfUpdates)
                        //too dim to actually really see but prevents clipping
                        Lighting.AddLight(new Vector2(u, v), new Vector3(0.05f, 0.075f, 0.1f));
                }
                if (!RelogicedUtil.DEBUG_MODE || !Main.rand.NextBool(40))
                    continue;
                Dust.NewDustPerfect(new Vector2(u, v), t, Vector2.Zero);
            }
        }

        float collidingPercent = (collidingTiles - IGNORE_TILES) / (float)totalTiles;
        if (FadingOut)
            collidingPercent *= Projectile.Opacity;
        collidingPercent = Math.Clamp(collidingPercent / STOPPING_FRACTION, 0, 1);
        Projectile.velocity.Y += 0.05f * (1f - collidingPercent);
        Projectile.velocity *= 1f - collidingPercent;
        if (!FadingOut && TooSlow)
            Projectile.velocity = Vector2.Zero;

        //SoundStyle sound = SoundID.Item32;
        //SoundEngine.PlaySound(sound.WithVolumeScale((Projectile.oldVelocity - Projectile.velocity).LengthSquared()), Projectile.Center);

        if (!FadingOut)
            UpdateFX(
                Projectile.velocity.Y,
                Projectile.velocity.Y - Projectile.oldVelocity.Y,
                Projectile.velocity.Y - 2 * Projectile.oldVelocity.Y + olderVelocity,
                collisionPoints,
                collidingPercent
            );
        olderVelocity = Projectile.oldVelocity.Y;
    }

    //TODO: VFX
    private void UpdateFX(float speed, float acceleration, float jerk, List<Point> collisionPoints, float encumberance)
    {
        if (speed < 0) return;
        if (acceleration >= 0 && speed > 3f / 16f / 60f / 7f)
        {
            int consequent = 2 + (int)(600 / speed) - (int)(20 * speed);
            for (int i = 0; i < 6; i++)
                if (consequent < 1 || Main.rand.NextBool(consequent))
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex);
                    d.velocity *= 0.2f;
                    d.velocity.Y -= 0.25f * (0.5f + Main.rand.NextFloat() * Main.rand.NextFloat()) * speed;
                    d.velocity.X += Main.rand.NextFloat() * 5 * Math.Sign(d.position.X - Projectile.Center.X) * Speed / HighestSpeed;
                }
            return;
        }
        if (jerk >= 0) return;
        if (encumberance <= 0) return;
        foreach (Point p in collisionPoints)
        {
            if (Main.rand.NextBool(Projectile.extraUpdates + 2))
                Dust.NewDustDirect(p.ToWorldCoordinates(), 0, 0, DustID.Vortex, Scale: 0.5f)
                    .velocity.X *= 0.5f;
            int consequent = 60 - 5 * (int)encumberance + (int)jerk;
            if (consequent < 1 || Main.rand.NextBool(consequent))
                Gore.NewGore(Projectile.GetSource_FromThis(), p.ToWorldCoordinates(), Vector2.Zero,
                    Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Color alpha = Projectile.GetAlpha(lightColor);
        SpriteEffects spriteEffects = Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 offset = -Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        Rectangle sourceRect = texAsset.Value.Bounds;
        Vector2 origin = sourceRect.Size() / 2f;

        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Vector2 pos = Projectile.oldPos[i];
            float delta = 1f - (i == 0 ? 0 : i / ((float)Projectile.oldPos.Length - 1));
            Main.EntitySpriteDraw(
                texAsset.Value,
                pos + new Vector2(Projectile.width / 2, Projectile.height / 2 - Projectile.velocity.Y) + offset,
                sourceRect,
                alpha * (delta * delta),
                0,
                origin,
                new Vector2(Projectile.scale),
                spriteEffects
            );
        }
        
        Main.EntitySpriteDraw(
            texAsset.Value,
            Projectile.Center + offset,
            sourceRect,
            alpha,
            0,
            origin,
            new Vector2(Projectile.scale),
            spriteEffects
        );
        return false;
    }
}