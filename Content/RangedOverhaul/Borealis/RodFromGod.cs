using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Borealis;

public class RodFromGod : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CanHitPastShimmer[Type] = true;
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
        Projectile.timeLeft *= 1 + Projectile.extraUpdates;
        Projectile.hide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.position.Y = 1;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers,
        List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage *= Projectile.velocity.Length() / 16f;
        modifiers.SetCrit();
    }

    private const float STOPPING_SPEED = 0.3f;
    private const int IGNORE_TILES = 4;
    private const float STOPPING_FRACTION = 0.8f;

    public override bool? CanDamage()
    {
        return !FadingOut() && !TooSlow();
    }

    public bool FadingOut() => Projectile.timeLeft < 60 * (1 + Projectile.extraUpdates);
    public float Speed() => Projectile.velocity.Length() * (1 + Projectile.extraUpdates);
    public bool TooSlow() => Speed() <= STOPPING_SPEED;

    public override void AI()
    {
        if (FadingOut())
        {
            Projectile.Opacity = Projectile.timeLeft / (60f * (1 + Projectile.extraUpdates));
            Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.LunarOre,
                newColor: Color.DarkSlateGray).velocity *= 0.1f;
        }
        
        Projectile.velocity *= 0.998f;
        Projectile.velocity.X *= 0.9f;
        Projectile.ArmorPenetration = (int)Speed();
        int collidingTiles = 0;
        int totalTiles = 0;
        int widthSteps = Projectile.width / 16;
        int heightSteps = Projectile.height / 32;
        int flipFlop = Projectile.timeLeft % 2;
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
                }
                if (!Main.rand.NextBool(20))
                    continue;
                Dust.NewDustPerfect(new Vector2(u, v), t, Vector2.Zero);
            }
        }

        float collidingPercent = (collidingTiles - IGNORE_TILES) / (float)totalTiles;
        if (FadingOut())
            collidingPercent *= Projectile.Opacity;
        collidingPercent = Math.Clamp(collidingPercent / STOPPING_FRACTION, 0, 1);
        Projectile.velocity.Y += 0.05f * (1f - collidingPercent);
        Projectile.velocity *= 1f - collidingPercent;
        if (!FadingOut() && TooSlow())
            Projectile.velocity = Vector2.Zero;

        //SoundStyle sound = SoundID.Item32;
        //SoundEngine.PlaySound(sound.WithVolumeScale((Projectile.oldVelocity - Projectile.velocity).LengthSquared()), Projectile.Center);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        int fireball = (int)Projectile.velocity.Length();
        hitbox.Inflate(fireball, fireball / 4);
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
}