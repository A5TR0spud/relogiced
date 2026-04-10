using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Other;

public class BuffStationSavePlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.Config_Reworks.BuffStationsSave;
    }

    public override void Load()
    {
        IL_Player.UpdateDead += IL_PlayerOnUpdateDead;
    }

    public override void Unload()
    {
        IL_Player.UpdateDead -= IL_PlayerOnUpdateDead;
    }

    private void IL_PlayerOnUpdateDead(ILContext il)
    {
        try
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchLdsfld<Main>("persistentBuff"));
            cursor.GotoNext(i => i.MatchLdarg0());
            cursor.GotoNext(i => i.MatchLdfld<Player>("buffType"));
            cursor.GotoNext(i => i.MatchLdloc2());
            cursor.GotoNext(i => i.MatchLdelemI4());
            cursor.GotoNext(i => i.MatchLdelemU1());
            cursor.Index++;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 2);
            cursor.EmitDelegate((bool b, Player player, int i) =>
                b || BuffID.Sets.TimeLeftDoesNotDecrease[player.buffType[i]]
            );
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Relogiced.Instance, il);
        }
    }
}