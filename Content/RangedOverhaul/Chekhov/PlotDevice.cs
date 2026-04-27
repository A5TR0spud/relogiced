using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Chekhov;

public class PlotDevice : ModProjectile
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }

    public int Target => (int)Projectile.ai[0] - 1;

    public Vector2 Origin = Vector2.Zero;

    public float Delta
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public int CopyExtraUpdatesType => (int)Projectile.ai[2];

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
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        return target.whoAmI == Target;
    }

    public override void AI()
    {
        if (Target < 0 || Target >= Main.npc.Length)
        {
            Projectile.Kill();
            return;
        }
        
        if (Projectile.localAI[0] == 0)
        {
            SoundStyle sound = SoundID.Item11;
            SoundEngine.PlaySound(sound.WithPitchOffset(0.15f), Origin);
            SoundEngine.PlaySound(sound.WithPitchOffset(-0.15f), Origin);
            Projectile.localAI[0] = 1;
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
        //TODO: VFX
        for (int i = 0; i < speed; i++)
        {
            /*if (!Main.rand.NextBool(3))
                continue;*/
            Dust dust = Dust.NewDustPerfect(
                oldPos + i * dir,
                DustID.WhiteTorch,
                dir,
                Scale: 0.75f
            );
            dust.noGravity = true;
            dust.noLightEmittence = true;
        }
        
        Projectile.Center = newPos;
        Delta += speed;
        Projectile.rotation = (targetPos - Origin).SafeNormalize(-Vector2.UnitY).ToRotation();
        if (Delta >= dist)
        {
            Projectile.Center = targetPos;
            Projectile.Hitbox.Inflate(160, 160);
            Projectile.Damage();
            Projectile.Kill();
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
}