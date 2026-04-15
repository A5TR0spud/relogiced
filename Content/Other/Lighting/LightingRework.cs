using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Threading;
using Terraria.Graphics.Light;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Relogiced.Content.Other.Lighting;

public class LightingRework : ModSystem
{
	private const float Cutoff = 0.0185f;
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigClient.LightingRework;
    }
    
    public override void Load()
    {
	    IL_LightMap.Blur += IL_LightMapOnBlur;
    }

    public override void Unload()
    {
	    IL_LightMap.Blur -= IL_LightMapOnBlur;
    }

    struct LightSource
    {
	    public Point pos = Point.Zero;
	    public Vector3 col = Vector3.Zero;
	    public float bri = 0f;
	    public int idx = 0;

	    public LightSource()
	    {
	    }
    }

    private void IL_LightMapOnBlur(ILContext il)
    {
	    try
	    {
			ILCursor c = new ILCursor(il);
			c.GotoNext(i => i.MatchLdarg0());
			ILCursor c2 = new ILCursor(il);
			c2.GotoNext(MoveType.After, i => i.MatchRet());
			c2.Index--;
			c.RemoveRange(c2.Index - c.Index);
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Ldflda, "_colors");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Ldfld, "_mask");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Call, "get_LightDecayThroughAir");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Call, "get_LightDecayThroughSolid");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Call, "get_LightDecayThroughWater");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Call, "get_LightDecayThroughHoney");
			c.Emit(OpCodes.Ldarg_0);
			c.Emit<LightMap>(OpCodes.Ldfld, "_random");
			c.EmitDelegate((LightMap self, ref Vector3[] _colors, LightMaskMode[] _mask,
				float decayAir, float decaySolid, Vector3 decayWater, Vector3 decayHoney,
				FastRandom _random) =>
			{
				LightSource[] lights = new LightSource[_colors.Length];
				int lightsCount = 0;
				Vector3[] colors = _colors;
				
				GetLightsPass();
				DrawLightsPass();
				
				_random.NextSeed();
				_colors = colors;
				return;

				void GetLightsPass()
				{
					lightsCount = 0;
					//FastParallel.For(0, self.Width * self.Height, (ParallelForAction)delegate(int start, int end, object context)
					int start = 0;
					int end = self.Width * self.Height;
					{
						float r, g, b;
						for (int i = start; i < end; i++)
						{
							if (colors[i].X > 0 || colors[i].Y > 0 || colors[i].Z > 0)
							{
								r = Math.Max(colors[i].X, 0);
								g = Math.Max(colors[i].Y, 0);
								b = Math.Max(colors[i].Z, 0);
								int sameness = 0;
								int samenessT = 0;
								for (int _c = 0; _c < 4; _c++)
								{
									int t = i + IndexOf(_c switch
									{
										0 => -1,
										1 => 1,
										_ => 0
									}, _c switch
									{
										2 => -1,
										3 => 1,
										_ => 0
									});
									if (t < 0 || t >= colors.Length) continue;
									samenessT++;
									if (colors[t].X >= r && colors[t].Y >= g && colors[t].Z >= b)
										sameness++;
									if (sameness < samenessT) break;
								}
								if (sameness == samenessT) continue;
								Vector3 colo = new Vector3(r, g, b);
								lights[lightsCount++] = new LightSource
								{
									col = colo,
									pos = PositionOf(i),
									bri = value(colo),
									idx = i
								};
							}
						}
					}//, (object)null);
				}
				
				void DrawLightsPass()
				{
					FastParallel.For(0, lightsCount, (ParallelForAction)delegate(int start,
						int end, object context)
					{
						for (int i = start; i < end; i++)
						{
							LightSource ls = lights[i];
							
							float radius = (float)Math.Log(Cutoff / ls.bri, decayAir);
							Vector3 rayLight;
							Vector2 delta;
							/*int rI = (int)Math.Ceiling(radius);
							Point target;
							for (int j = 0; j <= 4 * rI; j++)
							{
								int u = j % (2 * rI) - rI;
								int v = (j + rI) % (2 * rI) - rI;
								int idx = ls.idx + IndexOf(u, v);
								if (idx < 0 || idx >= colors.Length) continue;
								target = new Point(u, v);
								delta = Point.Zero;
								rayLight = ls.col;
							}*/
							float circumference = 2f * radius * MathHelper.Pi;
							int circumsteps = (int)Math.Round(circumference);
							float circumstep = MathHelper.TwoPi / circumsteps;
							float rot;
							for (int j = 0; j < circumsteps; j ++)
							{
								delta = Vector2.Zero;
								rayLight = ls.col;
								rot = j * circumstep;
								do
								{
									delta.X += (float)Math.Cos(rot);
									delta.Y += (float)Math.Sin(rot);
									int idx2 = ls.idx + IndexOf((int)delta.X, (int)delta.Y);
									Vector3 trans = GetTransmission(idx2);
									if (trans == Vector3.Zero) break;
									colors[idx2] = vec3max(colors[idx2], rayLight);
									rayLight *= trans;
								} while (value(rayLight) > Cutoff);
							}
						}
					}, (object)null);
				}
				
				Vector3 GetTransmission(int idx)
				{
					if (idx < 0 || idx >= colors.Length) return Vector3.Zero;
					Vector3 trans = Vector3.Zero;
					switch (_mask[idx])
					{
						case LightMaskMode.Solid:
						{
							trans = new Vector3(decaySolid);
							break;
						}
						case LightMaskMode.Water:
						{
							float num = (float)_random.WithModifier((ulong)idx).Next(98, 100) / 100f;
							trans = num * decayWater;
							break;
						}
						case LightMaskMode.Honey:
						{
							trans = decayHoney;
							break;
						}
						default:
						case LightMaskMode.None:
						{
							trans = new Vector3(decayAir);
							break;
						}
					}

					return trans;
				}

				Vector3 vec3max(Vector3 vec1, Vector3 vec2) => new (
					Math.Max(vec1.X, vec2.X),
					Math.Max(vec1.Y, vec2.Y),
					Math.Max(vec1.Z, vec2.Z)
				);
				float value(Vector3 color) => Math.Max(color.X, Math.Max(color.Y, color.Z));
				int IndexOf(int x, int y) => x * self.Height + y;
				Point PositionOf(int index) => new (index / self.Height, index % self.Height);
				float LengthSquaredOf(Point p) => p.X*p.X+p.Y*p.Y;
			});
	    }
	    catch
	    {
		    MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);
	    }
    }
}