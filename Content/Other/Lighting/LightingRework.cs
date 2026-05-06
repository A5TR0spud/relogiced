using System;
using System.CodeDom;
using System.Reflection;
using Microsoft.CodeAnalysis.FindSymbols;
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
	private const float CUTOFF = 0.0185f;
    
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
			c.EmitDelegate(() => Relogiced.ConfigClient.LightingRework);
			ILCursor c2 = new ILCursor(il);
			c2.GotoNext(i => i.MatchLdarg0());
			c2.GotoNext(i => i.MatchCall<LightMap>("BlurPass"));
			c.Emit(OpCodes.Brfalse_S, c2.Prev);
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
			c.EmitDelegate
			((LightMap self, ref Vector3[] colors, LightMaskMode[] mask,
				float decayAir, float decaySolid, Vector3 decayWater, Vector3 decayHoney,
				FastRandom random) =>
			{
				LightSource[] lights = new LightSource[colors.Length];
				int lightsCount = 0;
				Vector3[] colorsCopy = colors;
				
				GetLightsPass();
				DrawLightsPass();
				
				random.NextSeed();
				colors = colorsCopy;
				return;
				//TODO: avoid unstable raycasting. light with different approach?
				//TODO: light scattering?
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
							if (colorsCopy[i].X > 0 || colorsCopy[i].Y > 0 || colorsCopy[i].Z > 0)
							{
								r = Math.Max(colorsCopy[i].X, 0);
								g = Math.Max(colorsCopy[i].Y, 0);
								b = Math.Max(colorsCopy[i].Z, 0);
								int sameness = 0;
								int samenessT = 0;
								for (int samenessIdx = 0; samenessIdx < 4; samenessIdx++)
								{
									int t = i + IndexOf(samenessIdx switch
									{
										0 => -1,
										1 => 1,
										_ => 0
									}, samenessIdx switch
									{
										2 => -1,
										3 => 1,
										_ => 0
									});
									if (t < 0 || t >= colorsCopy.Length) continue;
									samenessT++;
									if (colorsCopy[t].X >= r && colorsCopy[t].Y >= g && colorsCopy[t].Z >= b)
										sameness++;
									if (sameness < samenessT) break;
								}
								if (sameness == samenessT) continue;
								Vector3 colo = new Vector3(r, g, b);
								lights[lightsCount++] = new LightSource
								{
									col = colo,
									bri = Value(colo),
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
							
							float radius = (float)Math.Log(CUTOFF / ls.bri, decayAir);
							Vector3 rayLight;
							Vector2 delta;
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
									colorsCopy[idx2] = Vector3.Max(colorsCopy[idx2], rayLight);
									rayLight *= trans;
								} while (Value(rayLight) > CUTOFF);
							}
						}
					}, (object)null);
				}
				
				Vector3 GetTransmission(int idx)
				{
					if (idx < 0 || idx >= colorsCopy.Length) return Vector3.Zero;
					Vector3 trans = Vector3.Zero;
					switch (mask[idx])
					{
						case LightMaskMode.Solid:
						{
							trans = new Vector3(decaySolid);
							break;
						}
						case LightMaskMode.Water:
						{
							float num = random.WithModifier((ulong)idx).Next(98, 100) / 100f;
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
				
				float Value(Vector3 color) => Math.Max(color.X, Math.Max(color.Y, color.Z));
				int IndexOf(int x, int y) => x * self.Height + y;
				Point PositionOf(int index) => new (index / self.Height, index % self.Height);
				float LengthSquaredOf(Point p) => p.X*p.X+p.Y*p.Y;
			});
			c.Emit(OpCodes.Ret);
	    }
	    catch
	    {
		    MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);
	    }
    }
}