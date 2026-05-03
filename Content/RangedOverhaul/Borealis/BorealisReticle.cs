using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.RangedOverhaul.Borealis;

public class BorealisNoDupeSystem : ModSystem
{
    internal static bool HasUpdated = false;
    public override void PreUpdateProjectiles()
    {
        HasUpdated = false;
    }

    public static bool GetHasUpdated() => HasUpdated;
}

public class BorealisReticle : ModProjectile
{
    private static Asset<Texture2D> texAsset;

    public override void SetStaticDefaults()
    {
        texAsset = ModContent.Request<Texture2D>(Texture);
    }

    private const int TIME = 60 * 30;

    public ref float AttackSpeed => ref Projectile.ai[0];

    public int ActualTime
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.timeLeft = TIME + 5;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.width = Projectile.height = 14;
        Projectile.extraUpdates = 0;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return new Color(255, 255, 255, 180);
    }

    public override bool ShouldUpdatePosition()
    {
        return false;
    }

    //TODO: WHY WONT IT SPAWN IN MP??
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 16; i++)
        {
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Vortex,
                Scale: 0.5f
            );
            dust.velocity = Vector2.Zero;
            dust.noGravity = true;
        }

        for (int i = 0; i < 64; i++)
        {
            int radius = Main.rand.NextBool() ? 33 : 59;
            Dust dust = Dust.NewDustPerfect(
                Projectile.Center + Main.rand.NextVector2CircularEdge(radius, radius),
                DustID.Vortex,
                Vector2.Zero
            );
            dust.noGravity = true;
        }
        if (timeLeft == 0 && Main.myPlayer == Projectile.owner)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("recognized dead, make proj"), Color.White);
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                Vector2.UnitY * Projectile.velocity.Length(),
                ModContent.ProjectileType<RodFromGod>(),
                Projectile.damage,
                Projectile.knockBack,
                Owner: Projectile.owner
            );
        }
    }

    public override bool PreAI()
    {
        if (BorealisNoDupeSystem.HasUpdated && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.timeLeft = 60;
            Projectile.Kill();
            return false;
        }
        BorealisNoDupeSystem.HasUpdated = true;
        return true;
    }

    public override void AI()
    {
        if ((int)Projectile.localAI[0] == 5 && Projectile.owner == Main.myPlayer)
        {
            SoundStyle sound = SoundID.Item35;
            sound = sound.WithPitchOffset(3f);
            SoundEngine.PlaySound(sound);
            sound = SoundID.Item35;
            sound = sound.WithPitchOffset(-0.5f);
            SoundEngine.PlaySound(sound);
            if (AttackSpeed != 0)
            {
                ActualTime = (int)(Projectile.timeLeft / AttackSpeed);
                Projectile.timeLeft = ActualTime;
            }
        }

        if (RelogicedUtil.DEBUG_MODE && Projectile.timeLeft % 60 == 0)
        {
            int textIdx = CombatText.NewText(
                new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y + 4, 0, 0),
                new Color(167, 245, 227),
                Projectile.timeLeft / 60
            );
            if (textIdx is >= 0 and < Main.maxCombatText)
            {
                CombatText text = Main.combatText[textIdx];
                text.velocity = Vector2.Zero;
                text.lifeTime = 60;
                text.alphaDir = 0;
                text.alpha = 0.99609375f;
            }
        }

        int radius = Main.rand.NextBool() ? 33 : 59;
        Dust dust = Dust.NewDustPerfect(
            Projectile.Center + Main.rand.NextVector2CircularEdge(radius, radius),
            DustID.Vortex,
            Vector2.Zero
        );
        dust.noGravity = true;
        dust.noLightEmittence = true;

        float delta = 1f - Projectile.timeLeft / (float)ActualTime;
        Projectile.rotation += AttackSpeed * AttackSpeed * (0.01f + 0.04f * delta * delta);

        Projectile.localAI[0]++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.timeLeft > ActualTime)
            return false;
        Color alpha = GetAlpha(lightColor).GetValueOrDefault(lightColor);
        const int MAX_COUNT = 12;
        int count = Math.Min((MAX_COUNT + 1) * Projectile.timeLeft / ActualTime, MAX_COUNT);
        float fadeOut = (MAX_COUNT + 1) * Projectile.timeLeft / (float)ActualTime - count;
        SpriteEffects spriteEffects = Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        for (int i = 0; i < count; i++)
        {
            //used to stagger the effect
            //works because 5 is a number which shares no factors with 12 and is close to its half. 7 could also be used.
            int fakeI = i * 5 % MAX_COUNT;
            float iDelta = count == 1 ? 0 : fakeI / (float)MAX_COUNT;
            float dir = Projectile.rotation - MathHelper.PiOver2 - MathHelper.TwoPi * iDelta;
            float dist = 0;
            Color alf = alpha;
            if (fadeOut < 0.5f && i == count - 1)
            {
                float fadeOutDelta = 1f - fadeOut * 2f;
                dist -= 16 * fadeOutDelta * fadeOutDelta;
                alf *= 1f - fadeOutDelta;
            }
            Vector2 pos = Projectile.Center + Projectile.scale * dist * dir.ToRotationVector2()
                - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Rectangle frame = texAsset.Frame(2, 1, 1, sizeOffsetX: -4);
            Vector2 origin = frame.Size() / 2f;
        
            Main.EntitySpriteDraw(
                texAsset.Value,
                pos,
                frame,
                alf,
                dir + MathHelper.PiOver2,
                origin,
                new Vector2(Projectile.scale),
                spriteEffects
            );
        }
        Vector2 cen = Projectile.Center
            - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        Rectangle sourceRect = texAsset.Frame(2, 1, 0, sizeOffsetX: -2);
        Vector2 cenOrigin = sourceRect.Size() / 2f;
        
        Main.EntitySpriteDraw(
            texAsset.Value,
            cen,
            sourceRect,
            alpha,
            0,
            cenOrigin,
            new Vector2(Projectile.scale),
            spriteEffects
        );
        return false;
    }
}