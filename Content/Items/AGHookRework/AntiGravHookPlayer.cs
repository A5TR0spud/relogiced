using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Hookers;
public class AntiGravHookPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.Config_Reworks.AntiGravityHookRework;
    }

    public override void Load()
    {
        IL_Player.GetGrapplingForces += AntiGravityHookHook;
    }

    public override void Unload()
    {
        IL_Player.GetGrapplingForces -= AntiGravityHookHook;
    }

    private void AntiGravityHookHook(ILContext il)
    {
        try
        {
            //Anti Gravity Hook movement change
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchLdloc(10));
            cursor.GotoNext(i => i.MatchLdfld<Terraria.Projectile>("type"));
            cursor.GotoNext(i => i.MatchLdcI4(446));
            //^ Locating "if (projectile.type == 446)" (anti-gravity hook)
            cursor.Index++; // Move cursor to after that check rather than inside it
            cursor.Index++; // Move cursor to inside if statement, past "bne.un IL_0191"
            ILCursor cursor2 = new ILCursor(il);
            cursor2.Index = cursor.Index;
            cursor2.GotoNext(i => i.MatchLdloc(10));
            cursor2.GotoNext(i => i.MatchLdfld<Terraria.Projectile>("type"));
            cursor2.GotoNext(i => i.MatchLdcI4(652));
            cursor2.Index -= 3;
            cursor.RemoveRange(cursor2.Index - cursor.Index);
            cursor.Emit(OpCodes.Ldarg_0); //push Player onto stack
            cursor.Emit(OpCodes.Ldloc, 10); //push Projectile onto stack
            cursor.EmitDelegate<Func<Player, Projectile, Vector2>>((Player player, Projectile proj) =>
            {
                Vector2 control = new Vector2(player.controlRight.ToInt() - player.controlLeft.ToInt(), (player.controlDown.ToInt() - player.controlUp.ToInt()) * player.gravDir);
                control = control.SafeNormalize(Vector2.Zero);
                control *= 4f;
                Vector2 target = player.Center - proj.Center + control;
                if (target.Length() > 750)
                {
                    proj.ai[0] = 1f;
                    return target;
                }
                int closeID = proj.identity;
                //it sucks to place a for loop in a for loop but i don't have many options and even though this is n^2, n is 3.
                for (int i = 0; i < player.grapCount; i++)
                {
                    Projectile iHook = Main.projectile[player.grappling[i]];
                    if (iHook.type != proj.type) continue;
                    if ((player.Center - iHook.Center).LengthSquared() < (player.Center - Main.projectile[closeID].Center).LengthSquared())
                    {
                        closeID = iHook.identity;
                    }
                }
                float prox = (Main.projectile[closeID].Center - proj.Center).Length() + 250f;
                Vector2 closePoint = player.Center - Main.projectile[closeID].Center;
                if ((closePoint + control).LengthSquared() > 250 * 250)
                {
                    Vector2 controlDir = Utils.SafeNormalize(new Vector2(player.controlRight.ToInt() - player.controlLeft.ToInt(), (player.controlDown.ToInt() - player.controlUp.ToInt()) * player.gravDir), Vector2.Zero);
                    target = Main.projectile[closeID].Center - proj.Center + closePoint.ToRotation().AngleTowards(controlDir.ToRotation(), 4f / 250f).ToRotationVector2() * 250f;
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 dest = proj.Center + 250f * (2f * Main.GlobalTimeWrappedHourly + 2f / 3f * (float)i * (float)Math.PI).ToRotationVector2();
                    bool flag = false;
                    for (int j = 0; j < player.grapCount; j++)
                    {
                        Projectile iHook = Main.projectile[player.grappling[j]];
                        if (iHook.type != proj.type) continue;
                        if (iHook.identity == proj.identity) continue;
                        if ((dest - iHook.Center).Length() <= 251)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag) continue;
                    Dust.NewDustPerfect(dest, DustID.Vortex, Vector2.Zero).noGravity = true;
                }
                return target;
            });
            cursor.Emit(OpCodes.Stloc, 12);

            cursor.Emit(OpCodes.Ldloc, 0); //push num
            cursor.Emit(OpCodes.Ldloc, 12); //push "vector"
            cursor.Emit<Vector2>(OpCodes.Ldfld, "X");
            cursor.Emit(OpCodes.Add);
            cursor.Emit(OpCodes.Stloc, 0);

            cursor.Emit(OpCodes.Ldloc, 1); //push num2
            cursor.Emit(OpCodes.Ldloc, 12); //push "vector"
            cursor.Emit<Vector2>(OpCodes.Ldfld, "Y");
            cursor.Emit(OpCodes.Add);
            cursor.Emit(OpCodes.Stloc, 1);
        }
        catch (Exception)
        {
            // If there are any failures with the IL editing, this method will dump the IL to Logs/ILDumps/{Mod Name}/{Method Name}.txt
            MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);

            // If the mod cannot run without the IL hook, throw an exception instead. The exception will call DumpIL internally
            // throw new ILPatchFailureException(ModContent.GetInstance<ExampleMod>(), il, e);
        }
    }
}