using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

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

    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.EarlyGameWeapons;
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 60;
        Projectile.Opacity = 0;
        Projectile.height = 18;
        Projectile.width = 18;
        Projectile.frame = 0;
        Projectile.DamageType = DamageClass.Magic;
        FadeInTimer = FadeIn;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
    }

    private void SetFrame()
    {
        Projectile.frame = Suit % Main.projFrames[Type];
    }

    public override void OnSpawn(IEntitySource source)
    {
        SetFrame();
        
        if (isDiamonds)
        {
            Projectile.ai[1] = Projectile.velocity.Length() * 2;
            Projectile.velocity *= 0.5f;
            Projectile.maxPenetrate++;
            Projectile.penetrate++;
        }
        else if (isSpades)
        {
            Projectile.velocity *= 1.2f;
        }
        else if (isHearts)
        {
            Projectile.velocity *= 0.8f;
            Projectile.velocity.Y += 0.8f;
        }
        else if (isClubs) //should always be true by now but who knows
        {
            Projectile.ai[1] = Main.rand.NextFloat(-0.02f, 0.02f);
            Projectile.ai[2] = Main.rand.NextFloat(-0.04f, 0.04f);
            if (Projectile.TryGetOwner(out Player owner))
            {
                if (owner.RollLuck(5) < 1)
                {
                    Projectile.maxPenetrate++;
                    Projectile.penetrate++;
                }

                if (owner.RollLuck(5) < 1)
                {
                    Projectile.ArmorPenetration += 5;
                    if (owner.RollLuck(10) < 1)
                    {
                        Projectile.ArmorPenetration += 5;
                    }
                }
                
                if (owner.RollLuck(6) > 4)
                    Projectile.damage /= 2;
                if (owner.luck > 0 && Main.rand.NextFloat() < owner.luck)
                    Projectile.velocity *= Main.rand.NextFloat(1f, 1.5f + 0.5f * owner.luck);
                else if (owner.luck < 0 && Main.rand.NextFloat() < -owner.luck)
                    Projectile.velocity *= Main.rand.NextFloat(0.5f, 1f);
            }
        }

        while (Math.Abs(Projectile.velocity.X) >= Math.Max(Projectile.width, 16) || Math.Abs(Projectile.velocity.Y) >= Math.Max(Projectile.height, 16))
        {
            Projectile.velocity *= (1f + Projectile.extraUpdates) / (2f + Projectile.extraUpdates);
            Projectile.extraUpdates++;
        }
    }

    public override bool? CanCutTiles()
    {
        return isDiamonds || isSpades;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (isDiamonds)
        {
            SoundStyle sound = SoundID.Dig;
            sound = sound.WithVolumeScale(0.4f);
            SoundEngine.PlaySound(sound, Projectile.position);
            Collision.HitTiles(Projectile.position + oldVelocity, oldVelocity, Projectile.width, Projectile.height);
            Projectile.Kill();
            return false;
        }
        if (isHearts)
        {
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, FadeOut);
        }
        if (isSpades)
        {
            Projectile.velocity = oldVelocity * 0.5f;
            Collision.HitTiles(Projectile.position + oldVelocity, oldVelocity, Projectile.width, Projectile.height);
            SoundStyle sound = SoundID.WormDigQuiet;
            sound = sound.WithPitchOffset(Main.rand.NextFloat(-0.065f, 0.125f));
            SoundEngine.PlaySound(sound, Projectile.position);
            Projectile.tileCollide = false;
        }
        if (isHearts || isClubs)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            SoundStyle sound = SoundID.Item56;
            sound = sound.WithPitchOffset(Main.rand.NextFloat(-0.4f, -0.2f));
            sound = sound.WithVolumeScale(0.5f);
            SoundEngine.PlaySound(sound, Projectile.position);
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
        SetFrame();
        
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
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.timeLeft < 30 ? Projectile.ai[2] : Projectile.ai[1]);
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