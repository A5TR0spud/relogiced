using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.EnchantedSwordRework;

public class EnchantedSwordProjectile : GlobalProjectile
{
    public static Asset<Texture2D> texAsset;
    public override void Load()
    {
        texAsset = TextureAssets.Projectile[ProjectileID.EnchantedBeam];
    }

    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.Config_Reworks.EnchantedSwordRework;
    }

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.type == ProjectileID.EnchantedBeam;
    }

    public override void SetDefaults(Projectile proj)
    {
        proj.timeLeft = 55;
        proj.penetrate = 3;
        proj.usesLocalNPCImmunity = true;
        proj.localNPCHitCooldown = -1;
    }

    public override void OnSpawn(Projectile proj, IEntitySource source)
    {
        float rot = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X);
        float projDir = Math.Sign(proj.velocity.X);
        rot -= projDir * MathHelper.PiOver2;
        proj.rotation = rot;
        proj.ai[2] = projDir;
        proj.ai[1] = Main.rand.Next(0, 2);
        if (proj.ai[1] == 0)
        {
            proj.maxPenetrate++;
            proj.penetrate++;
        }
    }

    public override bool? CanHitNPC(Projectile proj, NPC target)
    {
        if (proj.ai[1] == 0 && proj.penetrate == 1 && proj.ai[0] <= 35)
        {
            return false;
        }
        return base.CanHitNPC(proj, target);
    }

    public override void ModifyDamageHitbox(Projectile proj, ref Rectangle hitbox)
    {
        bool spinAttack = proj.ai[0] >= 35 && proj.ai[1] == 0;
        bool lungeAttack = proj.ai[0] >= 35 && proj.ai[1] != 0;
        float offSize = lungeAttack ? 24f : 16f;
        Vector2 off = proj.rotation.ToRotationVector2() * offSize;
        hitbox.X += (int)off.X;
        hitbox.Y += (int)off.Y;
        int bloat = spinAttack ? 8 : lungeAttack ? 0 : 4;
        hitbox.Inflate(bloat, bloat);
    }

    public override bool PreDraw(Projectile proj, ref Color lightColor)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (proj.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        Color drawColor = proj.GetAlpha(lightColor);
        Rectangle sourceRectangle = new Rectangle(0, 0, texAsset.Width(), texAsset.Height());
        Vector2 origin = sourceRectangle.Size() / 2f;
        Vector2 off = proj.rotation.ToRotationVector2() * 16f;
        
        Main.EntitySpriteDraw(
            texture: texAsset.Value,
            position: proj.Center - Main.screenPosition + new Vector2(0f, proj.gfxOffY) + off,
            sourceRectangle: sourceRectangle,
            color: drawColor,
            rotation: proj.rotation + MathHelper.PiOver4,
            origin: origin,
            scale: 1,
            effects: spriteEffects,
            worthless: 0
        );
        return false;
    }

    public override bool PreAI(Projectile proj)
    {
        float projDir = Math.Sign(proj.velocity.X);
        if (proj.ai[0] > 5)
        {
            proj.rotation += proj.ai[2] * 0.4f;
            proj.ai[2] *= 0.9f;
            if (proj.velocity.Length() > 0.1f)
                proj.velocity *= 0.9f;
            if (Math.Abs(proj.ai[2]) < 0.05f)
                proj.ai[2] = 0;
        }

        if (proj.ai[0] == 20)
        {
            proj.ResetLocalNPCHitImmunity();
            proj.ai[2] = -projDir;
            proj.velocity = proj.velocity.SafeNormalize(proj.rotation.ToRotationVector2()) * 8f;
            proj.rotation = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) + projDir * MathHelper.PiOver2;
            SoundStyle s = SoundID.Item1;
            s = s.WithPitchOffset(Main.rand.NextFloat(-0.2f, 0.2f)).WithVolumeScale(0.5f);
            SoundEngine.PlaySound(s, proj.Center);
        }

        if (proj.ai[0] == 35)
        {
            proj.ResetLocalNPCHitImmunity();
            SoundStyle s1 = SoundID.Item7;
            s1 = s1.WithPitchOffset(Main.rand.NextFloat(-0.2f, 0.2f)).WithVolumeScale(0.5f);
            SoundEngine.PlaySound(s1, proj.Center);
            if (proj.ai[1] != 0)
            {
                proj.ai[2] = 0.15f * projDir;
                proj.velocity = proj.velocity.SafeNormalize(proj.rotation.ToRotationVector2()) * 16f;
                proj.rotation = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) - 0.5f * projDir * MathHelper.PiOver4;
                if (proj.penetrate == proj.maxPenetrate)
                {
                    proj.CritChance = 300;
                    proj.penetrate = 1;
                }
                
                SoundStyle s = SoundID.Item105;
                s = s.WithVolumeScale(0.3f).WithPitchOffset(Main.rand.NextFloat(0.15f, 0.25f));
                SoundEngine.PlaySound(s, proj.Center);
            }
            else
            {
                proj.ai[2] = 2.5f * projDir;
                proj.penetrate = proj.maxPenetrate - 1;
                SoundStyle s = SoundID.Item15;
                s = s.WithVolumeScale(0.6f).WithPitchOffset(Main.rand.NextFloat(0.4f, 0.5f));
                SoundEngine.PlaySound(s, proj.Center);
            }
        }
        
        if (proj.localAI[1] > 7f)
        {
            int randDusti = Main.rand.Next(3);
            Dust.NewDustDirect(proj.Center - new Vector2(4), 8, 8, randDusti switch
            {
                0 => DustID.MagicMirror,
                1 => DustID.Enchanted_Gold,
                _ => DustID.Enchanted_Pink,
            }, 0f, 0f, 100, default(Color), 1.25f).velocity *= 0.1f;
        }
        if (proj.localAI[1] < 15f)
        {
            proj.localAI[1] += 1f;
        }
        else
        {
            if (proj.localAI[0] == 0f)
            {
                proj.scale -= 0.02f;
                proj.alpha += 30;
                if (proj.alpha >= 250)
                {
                    proj.alpha = 255;
                    proj.localAI[0] = 1f;
                }
            }
            else if (proj.localAI[0] == 1f)
            {
                proj.scale += 0.02f;
                proj.alpha -= 30;
                if (proj.alpha <= 0)
                {
                    proj.alpha = 0;
                    proj.localAI[0] = 0f;
                }
            }
        }
        if (proj.velocity.Length() > 16f)
        {
            proj.velocity = proj.velocity.SafeNormalize(proj.rotation.ToRotationVector2()) * 16f;
        }

        proj.ai[0]++;
        return false;
    }
}