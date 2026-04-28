using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class ModVoodooPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    public List<int> KillableNPCs = [];

    public override void ResetEffects()
    {
        KillableNPCs.Clear();
    }
    
    public override void PostUpdate()
    {
        if (KillableNPCs.Contains(NPCID.Mechanic))
        {
            UpdateMechanicVoodoo();
        }
    }

    //TODO: mechanic voodoo doll
    private void UpdateMechanicVoodoo()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        Point ps = Player.TopLeft.ToTileCoordinates();
        Point pe = Player.BottomRight.ToTileCoordinates();
        //flicker lights
        if ((Player.miscCounter % 10 == 0 && Main.rand.NextBool(3)) || Main.rand.NextBool(30))
        {
            bool pref = Main.rand.NextBool();
            for (int i = -15; i <= 16; i++)
            {
                for (int j = -15; j <= 18; j++)
                {
                    Point newP = ps + new Point(i, j);
                    if (!WorldGen.InWorld(newP.X, newP.Y))
                        continue;
                    if (Main.rand.NextBool(50))
                        WorldGen.TryToggleLight(newP.X, newP.Y, Main.rand.NextBool() ? pref : null, false);
                }
            }
        }
        //trigger random junk
        if (Player.miscCounter % 300 == 153)
        {
            for (int i = -50; i <= 51; i++)
            {
                for (int j = -50; j <= 53; j++)
                {
                    //if (!Main.rand.NextBool(100))
                    //    continue;
                    Point newP = ps + new Point(i, j);
                    if (!WorldGen.InWorld(newP.X, newP.Y))
                        continue;
                    Tile t = Main.tile[newP.X, newP.Y];
                    bool? shouldTriggerTile = null;
                    if (!t.HasTile) continue;
                    if (t.TileType == TileID.Traps)
                        //dart traps
                    {
                        int dartType = t.TileFrameY / 18;
                        int dartDir = t.TileFrameX / 18;
                        int range = 50;
                        const int DART_TRAP = 0;
                        const int SUPER_TRAP = 1;
                        const int FLAME_TRAP = 2;
                        const int BALL_TRAP = 3;
                        const int SPEAR_TRAP = 4;
                        const int VENOM_TRAP = 5;
                        const byte UP = 0;
                        const byte LEFT = 1;
                        const byte RIGHT = 2;
                        const byte DOWN = 3;
                        byte dir = 0;
                        if (dartType is DART_TRAP or SUPER_TRAP or FLAME_TRAP or VENOM_TRAP)
                        {
                            dir = dartDir switch
                            {
                                0 => LEFT,
                                1 => RIGHT,
                                2 or 3 => UP,
                                _ => DOWN
                            };
                        }
                        else //ball, spear
                        {
                            dir = dartDir switch
                            {
                                0 or 1 => DOWN,
                                2 => UP,
                                3 => LEFT,
                                _ => RIGHT
                            };
                        }
                        if (dartType == BALL_TRAP)
                        {
                            range = dir switch
                            {
                                UP => 5,
                                LEFT or RIGHT => 10,
                                _ => 50
                            };
                        }
                        else if (dartType is FLAME_TRAP or SPEAR_TRAP)
                        {
                            range = 20;
                        }

                        bool matchingX = ps.X <= newP.X && newP.X <= pe.X;
                        bool matchingY = ps.Y <= newP.Y && newP.Y <= pe.Y;
                        int delta;
                        switch (dir)
                        {
                            //which way the trap is facing
                            case DOWN:
                                delta = ps.Y - newP.Y;
                                shouldTriggerTile = matchingX && delta >= 0 && delta <= range
                                    && Collision.CanHitLine(newP.ToWorldCoordinates(8, 24), 0, 0,
                                        newP.ToWorldCoordinates(8, 24 + 16 * delta), 0, 0);
                                break;
                            case UP:
                                delta = newP.Y - pe.Y;
                                shouldTriggerTile = matchingX && delta >= 0 && delta <= range
                                    && Collision.CanHitLine(newP.ToWorldCoordinates(8, -8), 0, 0,
                                        newP.ToWorldCoordinates(8, -8 - 16 * delta), 0, 0);
                                break;
                            case LEFT:
                                delta = newP.X - ps.X;
                                shouldTriggerTile = matchingY && delta >= 0 && delta <= range
                                    && Collision.CanHitLine(newP.ToWorldCoordinates(-8, 8), 0, 0,
                                        newP.ToWorldCoordinates(-8 - 16 * delta, 8), 0, 0);
                                break;
                            case RIGHT:
                                delta = pe.X - newP.X;
                                shouldTriggerTile = matchingY && delta >= 0 && delta <= range
                                    && Collision.CanHitLine(newP.ToWorldCoordinates(24, 8), 0, 0,
                                        newP.ToWorldCoordinates(24 + 16 * delta, 8), 0, 0);
                                break;
                        }
                    }
                    else if (TileDrawing.IsTileDangerous(newP.X, newP.Y, Player)
                        && TileLoader.PreHitWire(newP.X, newP.Y, t.TileType))
                    {
                        shouldTriggerTile = Main.rand.NextBool(5);
                    }
                    if (shouldTriggerTile ?? false)
                    {
                        //Wiring.SkipWire(newP.X, newP.Y);
                        //Wiring.HitSwitchAndSync(newP.X, newP.Y);
                        HACKING_THE_MAINFRAME = true;
                        Wiring.TripWire(newP.X, newP.Y, 1, 1);
                        //Wiring.HitWireSingle(newP.X, newP.Y);
                    }
                }
            }
        }
    }

    public static bool HACKING_THE_MAINFRAME = false;

    public override void Load()
    {
        IL_Wiring.TripWire += IL_WiringOnTripWire;
    }

    public override void Unload()
    {
        IL_Wiring.TripWire -= IL_WiringOnTripWire;
    }

    private void IL_WiringOnTripWire(ILContext il)
    {
        /*try
        {
            ILCursor c = new ILCursor(il);
            //c.Emit(OpCodes.Ldarg_0);
            //c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(() =>
            {
                return HACKING_THE_MAINFRAME;
            });
            c.Emit(OpCodes.br)
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);
        }*/
    }
    
    public override bool CanHitNPC(NPC target)
    {
        return base.CanHitNPC(target) || KillableNPCs.Contains(target.type);
    }
}