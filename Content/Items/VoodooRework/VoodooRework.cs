using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Relogiced.Other;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class VoodooRework : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    public override void Load()
    {
        RelogicedUtil.ChangeItemSprite(ItemID.GuideVoodooDoll, "Content/Items/VoodooRework/GuideVoodooDoll_On");
        RelogicedUtil.ChangeItemSprite(ItemID.ClothierVoodooDoll, "Content/Items/VoodooRework/ClothierVoodooDoll_On");
        IL_ShopHelper.ProcessMood += IL_ShopHelperOnProcessMood;
    }

    public override void Unload()
    {
        RelogicedUtil.RestoreItemSprite(ItemID.GuideVoodooDoll);
        RelogicedUtil.RestoreItemSprite(ItemID.ClothierVoodooDoll);
        IL_ShopHelper.ProcessMood -= IL_ShopHelperOnProcessMood;
    }

    private void IL_ShopHelperOnProcessMood(ILContext il)
    {
        try
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(i => i.MatchLdstr("Content"));
            c.GotoNext(i => i.MatchLdarg0());
            c.GotoNext(i => i.MatchLdarg0());
            c.GotoNext(i => i.MatchLdarg0());
            c.GotoNext(i => i.MatchLdfld<ShopHelper>("_currentPriceAdjustment"));
            c.GotoNext(i => i.MatchCall<ShopHelper>("LimitAndRoundMultiplier"));
            c.GotoNext(i => i.MatchStfld<ShopHelper>("_currentPriceAdjustment"));
            c.GotoNext(i => i.MatchRet());
            c.GotoPrev(i => i.MatchLdarg0());
            c.Index--;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<ShopHelper>(OpCodes.Ldfld, "_currentPriceAdjustment");
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate((float priceAdjustment, Player player, NPC npc) =>
            {
                if (player.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(npc.type))
                {
                    priceAdjustment *= 1.2f;
                }
                return priceAdjustment;
            });
            c.Emit<ShopHelper>(OpCodes.Stfld, "_currentPriceAdjustment");
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);
        }
    }
}