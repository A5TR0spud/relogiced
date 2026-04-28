using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Chekhov;

public class PlotDevice : ModProjectile
{
    private static Asset<Texture2D> texAsset;
    
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }
    
    public int Target
    {
        get => (int)Projectile.ai[0] - 1;
        set => Projectile.ai[0] = value + 1;
    }

    public Vector2 Origin = Vector2.Zero;

    public float Delta
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public int CopyExtraUpdatesType => (int)Projectile.ai[2];
    public bool DamageWindow = false;

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(Origin);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Origin = reader.ReadVector2();
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        texAsset = ModContent.Request<Texture2D>(Texture);
    }

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.penetrate = 1;
        Projectile.friendly = true;
        Projectile.width = Projectile.height = 16;
        Projectile.extraUpdates = 1;
        DamageWindow = false;
    }

    public override bool ShouldUpdatePosition()
    {
        return false;
    }

    public override bool CanHitPvp(Player target)
    {
        //this would be so broken lol
        return false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Origin = Projectile.Center;
        Delta = 0;
        if (CopyExtraUpdatesType != 0)
        {
            Projectile proj = new Projectile();
            proj.SetDefaults(CopyExtraUpdatesType);
            Projectile.extraUpdates += proj.extraUpdates;
            Projectile.maxPenetrate = proj.maxPenetrate;
            Projectile.penetrate = proj.penetrate;
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        return DamageWindow && target.whoAmI == Target;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        Vector2 searchOrigin = Projectile.owner >= 0 && Projectile.owner < Main.player.Length
            ? Main.player[Projectile.owner].Center
            : Origin;
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - searchOrigin.X);
        modifiers.ScalingArmorPenetration += 0.05f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        HitFX(target.Center, (target.Center - Origin).SafeNormalize(Vector2.Zero));
        Projectile.damage = (int)(Projectile.damage * 0.33f) + 1;
        Projectile.knockBack *= 0.5f;
        if (Projectile.damage <= 2)
        {
            Projectile.Kill();
            return;
        }
        Vector2 searchOrigin = Projectile.owner >= 0 && Projectile.owner < Main.player.Length
            ? Main.player[Projectile.owner].Center
            : Origin;
        Target = GetChekhovTarget(searchOrigin, plotDevice: Projectile)?.whoAmI ?? -1;
        Origin = Projectile.Center;
        Delta = 0;
        Projectile.extraUpdates++;
        Projectile.netUpdate = true;
        Projectile.localAI[0] = 1;
    }

    public override void AI()
    {
        if (Target < 0 || Target >= Main.npc.Length || Projectile.penetrate == 0)
        {
            Projectile.Kill();
            return;
        }

        NPC npc = Main.npc[Target];
        if (npc == null || !npc.active)
        {
            Projectile.Kill();
            return;
        }

        Vector2 targetPos = npc.Center;
        float dist = targetPos.Distance(Origin);
        float speed = Projectile.velocity.Length();
        Vector2 oldPos = Projectile.Center;
        Vector2 newPos = Delta / dist * targetPos + (1f - Delta / dist) * Origin;
        Vector2 dir = (newPos - oldPos).SafeNormalize(-Vector2.UnitY);
        
        if (Projectile.localAI[0] == 0)
        {
            SpawnFX(Origin);
        }
        else
        {
            if ((int)Projectile.localAI[0] == 1)
            {
                SoundStyle soundSantaMissile = SoundID.Item42;
                SoundEngine.PlaySound(soundSantaMissile.WithVolumeScale(0.5f).WithPitchOffset(-0.55f), Projectile.Center);
            }
            for (int i = 0; i < speed; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    oldPos + i * dir,
                    DustID.WhiteTorch,
                    dir,
                    Scale: 0.75f
                );
                dust.noGravity = true;
                dust.noLightEmittence = true;
            }
        }
        Projectile.localAI[0]++;
        
        Projectile.Center = newPos;
        Delta += speed;
        Projectile.rotation = (targetPos - Origin).SafeNormalize(-Vector2.UnitY).ToRotation();
        if (Delta >= dist)
        {
            Projectile.Center = targetPos;
            Projectile.Hitbox.Inflate(160, 160);
            DamageWindow = true;
            Projectile.Damage();
            DamageWindow = false;
        }
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

    public void SpawnFX(Vector2 origin)
    {
        SoundStyle soundBoomstick = SoundID.Item36;
        SoundStyle soundSniper = SoundID.Item40;
        SoundStyle soundRevolver = SoundID.Item41;
        SoundStyle soundGunFire = SoundID.Item11;
        SoundStyle soundSantaMissile = SoundID.Item42;
        SoundEngine.PlaySound(soundRevolver.WithVolumeScale(0.6f).WithPitchOffset(-0.15f), origin);
        SoundEngine.PlaySound(soundSniper.WithVolumeScale(0.5f), origin);
        SoundEngine.PlaySound(soundBoomstick.WithVolumeScale(0.3f).WithPitchOffset(0.25f), origin);
        SoundEngine.PlaySound(soundGunFire.WithVolumeScale(0.4f).WithPitchOffset(-0.15f), origin);
        SoundEngine.PlaySound(soundSantaMissile.WithVolumeScale(0.5f).WithPitchOffset(-0.55f), origin);
        Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
        for (int i = 0; i < 15; i++)
        {
            Vector2 vel = 16 * dir.RotatedByRandom(i * 0.025f) * Main.rand.NextFloat(0.7f + 0.025f * i, 1f + 0.025f * i);
            Dust d = Dust.NewDustDirect(
                origin + dir * 18f,
                0, 0,
                DustID.Torch,
                vel.X,
                vel.Y,
                Scale: 1.4f
            );
            d.noGravity = true;
        }
        for (int i = 0; i < 10; i++)
        {
            Vector2 vel = 2 * dir.RotatedByRandom(i * 0.05f) * Main.rand.NextFloat(0.7f + 0.05f * i, 1f + 0.05f * i);
            Dust.NewDustPerfect(
                origin + dir * 18f,
                DustID.Smoke,
                vel,
                Scale: 1.5f
            ).fadeIn = 0.3f;
        }
    }

    public void HitFX(Vector2 center, Vector2 oldDir)
    {
        SoundStyle soundBlastHit = SoundID.Item10;
        SoundStyle soundExplosion = SoundID.Item14;
        SoundEngine.PlaySound(soundExplosion.WithVolumeScale(0.375f), center);
        SoundEngine.PlaySound(soundBlastHit.WithVolumeScale(0.5f).WithPitchOffset(0.15f), center);
        for (int i = 0; i < Projectile.damage / 1000f * Projectile.velocity.Length() * (Projectile.extraUpdates + 1); i++)
        {
            Dust.NewDustDirect(
                center,
                0, 0,
                DustID.Torch
            ).velocity += i * oldDir.RotatedByRandom(0.5) * 0.125f;
        }
    }

    //TODO: VFX
    public override void PostDraw(Color lightColor)
    {
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    
    
    public static NPC GetChekhovTarget(Vector2 position, Player owner = null, Projectile plotDevice = null)
    {
        NPC nearest = null;
        float cost = 0;
        int damage = owner?.GetWeaponDamage(owner.GetModPlayer<ChekhovPlayer>().FirstChekhov) ?? plotDevice?.damage ?? 0;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(plotDevice))
                continue;
            if (npc.life <= 0)
                continue;
            if (plotDevice != null && plotDevice.localNPCImmunity[npc.whoAmI] != 0)
                continue;
            float newCost = npc.Distance(position);
            if (!Collision.CanHit((Entity)owner ?? (Entity)plotDevice, npc))
                newCost *= 2;
            if (newCost > 16 * 100) //100 tile range (40 tiles off-screen horizontally)
                continue;
            newCost -= 16f * 3f * (float)Math.Log10(npc.life);
            //zombie (40HP): 4.8 tiles closer
            //mimic (500HP): 8.1 tiles closer
            //wyvern (4000HP): 10.8 tiles closer
            //master destroyer (153000HP): 15.6 tiles closer
            if (npc.boss)
                newCost -= 16 * 10; //treat it 10 tiles closer if it's a boss
            if (npc.life < damage)
                newCost += 16 * 10; //if damage would be wasted, be harsh
            newCost += Main.rand.NextFloat(-64f, 64f + float.Epsilon); //where's the fun if there's no gambling?
            if (newCost > cost && nearest != null)
                continue;
            nearest = npc;
            cost = newCost;
        }

        return nearest;
    }
}