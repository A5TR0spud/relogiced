using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.MeleeOverhaul.EnchantedWeaponsRework;

public class EnchantedBoomerangeProjectile : GlobalProjectile
{
    private const int ReboundTime = 30;
    private const int TargetCount = 3;
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMeleeOverhaul.EnchantedSwordRework;
    }

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        return entity.type == ProjectileID.EnchantedBoomerang;
    }

    public override void SetDefaults(Projectile entity)
    {
        entity.usesLocalNPCImmunity = true;
        entity.localNPCHitCooldown = -1;
        entity.penetrate = -1;
        //entity.stopsDealingDamageAfterPenetrateHits = true;
        //entity.timeLeft = 180;
        entity.aiStyle = 0;
        entity.tileCollide = true;
    }

    public override void OnSpawn(Projectile proj, IEntitySource source)
    {
        proj.ai[0] = ReboundTime;
        proj.ai[1] = -1;
        proj.ai[2] = TargetCount;
        proj.localAI[0] = (proj.velocity.X < 0 ? -1 : 1) * proj.velocity.Length();
    }

    public override bool PreAI(Projectile proj)
    {
        for (int i = -1; i <= 1; i += 2)
        {
            if (Main.rand.NextBool(4))
            {
                int randDusti = Main.rand.Next(3);
                Dust.NewDustDirect(proj.Center + new Vector2(12, 12 * i).RotatedBy(proj.rotation), 4, 4, randDusti switch
                {
                    0 => DustID.MagicMirror,
                    1 => DustID.Enchanted_Gold,
                    _ => DustID.Enchanted_Pink,
                }, proj.velocity.X * 0.5f, proj.velocity.Y * 0.5f, 125, default(Color), 0.8f).velocity *= 0.2f;
            }
        }
        
        if (!proj.TryGetOwner(out Player owner))
        {
            proj.Kill();
            return false;
        }

        if (proj.ai[0] == 0)
        {
            proj.ai[1] = -2;
            proj.ResetLocalNPCHitImmunity();
        }

        proj.rotation += 0.05f * proj.localAI[0];
        
        if ((int)proj.ai[1] == -2)
        {
            proj.tileCollide = false;
            Vector2 dir = proj.DirectionTo(owner.Center);
            Vector2 crossDir = (proj.Center + proj.velocity * proj.Distance(owner.Center)).ClosestPointOnLine(
                owner.Center + dir.RotatedBy(MathHelper.PiOver2),
                owner.Center - dir.RotatedBy(MathHelper.PiOver2)
            ) - owner.Center;
            proj.velocity *= 0.98f;
            proj.velocity += dir - 0.5f * crossDir.SafeNormalize(Vector2.Zero);
            if (proj.Colliding(proj.Hitbox, owner.Hitbox))
            {
                proj.Kill();
            }
        }
        
        if (proj.velocity.Length() > Math.Abs(proj.localAI[0]))
        {
            proj.velocity = proj.velocity.SafeNormalize(-Vector2.UnitY) * Math.Abs(proj.localAI[0]);
        }
        
        proj.ai[0]--;
        return false;
    }

    public override bool OnTileCollide(Projectile proj, Vector2 oldVelocity)
    {
        proj.velocity = oldVelocity;
        SoundEngine.PlaySound(SoundID.Dig, proj.position);
        Collision.HitTiles(proj.position, proj.velocity, proj.width, proj.height);
        Ricochet(proj);
        return false;
    }

    public override void ModifyHitNPC(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if ((int)proj.ai[2] == 1) modifiers.SetCrit();
    }

    public override void OnHitNPC(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        proj.damage = (int)(proj.damage * 0.8f + 1);
        Ricochet(proj);
    }

    public static void Ricochet(Projectile proj)
    {
        if (!proj.TryGetOwner(out Player owner))
        {
            proj.Kill();
            return;
        }
        if ((int)proj.ai[1] == -2) return;
        proj.ai[2]--;
        proj.netUpdate = true;
        if (proj.ai[2] == 0)
        {
            proj.velocity *= -1;
            proj.ai[1] = -2;
            return;
        }
        proj.ai[0] = ReboundTime;
        float dist = -1;
        float dist2 = -1;
        NPC nearest = null;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (Collision.CanHit(proj, npc) && npc.CanBeChasedBy(proj) && proj.localNPCImmunity[npc.whoAmI] == 0)
            {
                dist2 = owner.DistanceSQ(npc.Center);
                if (proj.DistanceSQ(npc.Center) > proj.velocity.LengthSquared() * ReboundTime * ReboundTime) continue;
                if (dist < 0 || dist2 < dist)
                {
                    dist = dist2;
                    nearest = npc;
                }
            }
        }

        if (dist < 0 || nearest == null)
        {
            proj.velocity *= -1;
            proj.ai[1] = -2;
            return;
        }

        proj.velocity = proj.DirectionTo(nearest.Center) * proj.velocity.Length();
        proj.ai[1] = nearest.whoAmI + 1;
    }
}