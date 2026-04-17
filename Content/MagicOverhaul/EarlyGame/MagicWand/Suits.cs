using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.EarlyGame.MagicWand;

public class Suits : ModProjectile
{
    public int Suit
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    public int FadeInTimer
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public bool isSpades => Suit == 0;
    public bool isClubs => Suit == 1;
    public bool isHearts => Suit == 2;
    public bool isDiamonds => Suit == 3;
    public const int FadeIn = 10;
    public const int FadeOut = 10;
    
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 60;
        Projectile.Opacity = 0;
        Projectile.height = 18;
        Projectile.width = 18;
        Projectile.frame = 0;
        FadeInTimer = FadeIn;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (isDiamonds)
        {
            Projectile.ai[1] = Projectile.velocity.Length() * 2;
            Projectile.velocity *= 0.5f;
            Projectile.maxPenetrate++;
            Projectile.penetrate++;
        }

        if (isSpades)
        {
            Projectile.velocity *= 1.2f;
        }

        if (isHearts)
        {
            Projectile.velocity *= 0.8f;
            Projectile.velocity.Y += 0.8f;
        }

        if (isClubs && Projectile.TryGetOwner(out Player owner))
        {
            Projectile.ai[1] = Main.rand.NextFloat(-0.02f, 0.02f);
            if (owner.RollLuck(6) == 0)
            {
                Projectile.damage /= 2;
            }
            if (owner.RollLuck(8) > 1)
            {
                Projectile.extraUpdates++;
                Projectile.velocity *= 0.75f;
            }
            if (owner.RollLuck(5) > 1)
            {
                Projectile.maxPenetrate++;
                Projectile.penetrate++;
            }
            if (owner.RollLuck(10) > 1)
            {
                Projectile.ArmorPenetration += owner.RollLuck(10) > 1 ? 10 : 5;
            }
        }

        Projectile.frame = Suit;
        Projectile.netUpdate = true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (isDiamonds)
        {
            Projectile.velocity = oldVelocity;
            Projectile.tileCollide = false;
        }
        if (isHearts || isDiamonds)
        {
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, FadeOut);
        }
        if (isSpades)
        {
            Projectile.velocity = oldVelocity * 0.5f;
            Collision.HitTiles(Projectile.position + oldVelocity, oldVelocity, Projectile.width, Projectile.height);
            SoundStyle sound = SoundID.WormDigQuiet;
            SoundEngine.PlaySound(sound.WithPitchOffset(Main.rand.NextFloat(-0.065f, 0.125f)), Projectile.position);
            Projectile.tileCollide = false;
        }
        if (isHearts || isDiamonds || isClubs)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;
        }
        return false;
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (target.friendly && isHearts)
        {
            if (Projectile.damage > 0 && target.Hitbox.Intersects(Projectile.Hitbox))
            {
                target.AddBuff(BuffID.Lovestruck, 600);
            }
        }
        return base.CanHitNPC(target);
    }

    public override void AI()
    {
        if (isDiamonds)
        {
            if (Projectile.velocity.Length() < Projectile.ai[1])
                Projectile.velocity *= 1.05f;
            else if (Projectile.velocity.Length() > Projectile.ai[1])
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.ai[1];
            Projectile.scale = 0.5f + 0.5f * Projectile.velocity.Length() / Projectile.ai[1];
        }

        if (isClubs)
        {
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]);
            if (Projectile.timeLeft == 30)
            {
                Projectile.ai[1] = Main.rand.NextFloat(-0.04f, 0.04f);
                Projectile.netUpdate = true;
            }
        }
        if (isHearts)
        {
            Projectile.velocity.Y += 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Projectile.scale = 1f + 0.1f * (float)Math.Sin(Projectile.timeLeft * 0.04f);
        }
        else
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        
        if (Projectile.penetrate <= 0 && Projectile.damage <= 0)
        {
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, FadeOut);
        } 

        if (Projectile.timeLeft < FadeOut)
        {
            Projectile.Opacity -= 1f / FadeOut;
            if (Projectile.Opacity <= 0)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.timeLeft < FadeOut / 2)
            {
                Projectile.damage = 0;
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
            }
        }
        else if (FadeInTimer > 0)
        {
            Projectile.Opacity += 1f / FadeIn;
        }
        else if (isDiamonds && Projectile.timeLeft % 2 == 0)
        {
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GemDiamond, 0, 0,
                    120, default, 0.5f)
                .noGravity = true;
        }
        else if (isHearts && Projectile.timeLeft % 15 == 0)
        {
            Vector2 vector2 = new(Main.rand.Next(-10, 11), Main.rand.Next(-10, 11));
            vector2.SafeNormalize(-Vector2.UnitY);
            vector2.X *= 0.66f;
            Gore heart = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), 
                Projectile.position + new Vector2(
                    Main.rand.Next(Projectile.width + 1),
                    Main.rand.Next(Projectile.height + 1)
                    ),
                vector2 * Main.rand.Next(3, 6) * 0.33f,
                331,
                Main.rand.Next(40, 121) * 0.01f);
            heart.sticky = false;
            heart.velocity *= 0.4f;
            heart.velocity.Y -= 0.6f;
            heart.velocity *= 0.2f;
        }

        FadeInTimer--;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (isSpades)
        {
            modifiers.FinalDamage *= 1.5f;
        }
        if (isClubs)
        {
            const int numerator = 1;
            const int denominator = 2;
            if (Projectile.TryGetOwner(out Player player) && player.RollLuck(denominator) < numerator)
                modifiers.SetCrit();
            else
                modifiers.DisableCrit();
        }
        if (isHearts)
        {
            modifiers.DisableCrit();
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (isHearts)
        {
            target.AddBuff(BuffID.Lovestruck, 30 + 3 * damageDone);
        }

        if (isDiamonds)
        {
            target.AddBuff(BuffID.Midas, 30 + 3 * damageDone);
        }

        Projectile.damage = (int)(Projectile.damage * 0.85f + 1);
    }
}