using System;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class VoodooRework : ILoadable
{
    public bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.Config_Reworks.VoodooRework;
    }

    public void Load(Mod mod)
    {
        TextureAssets.Item[ItemID.GuideVoodooDoll] = mod.Assets.Request<Texture2D>("Content/Items/VoodooRework/GuideVoodooDoll_On");
        TextureAssets.Item[ItemID.ClothierVoodooDoll] = mod.Assets.Request<Texture2D>("Content/Items/VoodooRework/ClothierVoodooDoll_On");
        IL_ShopHelper.ProcessMood += IL_ShopHelperOnProcessMood;
    }

    public void Unload()
    {
        RestoreItemSprite(ItemID.GuideVoodooDoll);
        RestoreItemSprite(ItemID.ClothierVoodooDoll);
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

    private static void RestoreItemSprite(int item)
    {
        TextureAssets.Item[item] = Main.Assets.Request<Texture2D>("Images/Item_" + item);
    }
}