using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Other.Lighting;

public class LightingRebalanceGlobalProj : GlobalProjectile
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.LightingRebalance;
    }

    public override void SetDefaults(Projectile proj)
    {
        if (proj.type == ProjectileID.FairyGlowstick)
        {
            proj.light *= 1.37245911661f;
        }
    }
}