using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Relogiced.Content.MagicOverhaul.Hardmode.LostWispLantern;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Relogiced.Content.MagicOverhaul.Hardmode.HauntedBrazier;

public class HauntedBrazierTile : ModTile
{
	public override bool IsLoadingEnabled(Mod mod)
	{
		return Relogiced.ConfigMagicOverhaul.HardmodeWeapons;
	}

	public override void Load()
	{
		IL_SceneMetrics.ExportTileCountsToMain += IL_SceneMetricsOnExportTileCountsToMain;
	}

	public override void Unload()
	{
		IL_SceneMetrics.ExportTileCountsToMain -= IL_SceneMetricsOnExportTileCountsToMain;
	}

	private void IL_SceneMetricsOnExportTileCountsToMain(ILContext il)
	{
		try
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(i => i.MatchLdarg0());
            cursor.GotoNext(i => i.MatchLdarg0());
            cursor.GotoNext(i => i.MatchLdfld<SceneMetrics>("_tileCounts"));
            cursor.GotoNext(i => i.MatchLdcI4(85));
            cursor.GotoNext(i => i.MatchLdelemI4());
            cursor.GotoNext(i => i.MatchCall<SceneMetrics>("set_GraveyardTileCount"));
            //matched to: this.GraveyardTileCount = this._tileCounts[85];
            cursor.Index++;
            //inject: this.GraveyardTileCount += this._tileCounts[ModContent.TileType<HauntedBrazierTile>()];
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<SceneMetrics>(OpCodes.Call, "get_GraveyardTileCount");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<SceneMetrics>(OpCodes.Ldfld, "_tileCounts");
            //the following does not work because it is evaluated before it is injected:
            //cursor.Emit(OpCodes.Ldc_I4, ModContent.TileType<HauntedBrazierTile>());
            //instead, emit a delegate to calculate it at runtime
            cursor.EmitDelegate<Func<int>>(() => Type);
            //ldelem i4 gets a 4-byte integer from an array at the given index
            //the index is what the delegate emits, i.e., the TileID of HauntedBrazierTile
            cursor.Emit(OpCodes.Ldelem_I4);
            //gravetiles counts individual tiles, i.e., each tombstone increases graveTiles by 4.
            cursor.EmitDelegate<Func<int, int, int>>((graveTiles, hauntedBraziers) =>
	            graveTiles + 4 * hauntedBraziers / 6
	        );
            cursor.Emit<SceneMetrics>(OpCodes.Call, "set_GraveyardTileCount");
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Relogiced>(), il);
        }
	}

	//private static Asset<Texture2D> flameTexture;
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        VanillaFallbackOnModDeletion = TileID.BloodMoonMonolith;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.CoordinatePadding = 2;
        //TileObjectData.newTile.DrawYOffset = -4;
        TileObjectData.addTile(Type);
        
        //flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
        AddMapEntry(new Color(80, 80, 80), Relogiced.Instance.GetLocalization("Items.HauntedBrazier.DisplayName"));

        HitSound = SoundID.Tink;
        DustType = DustID.Lead;
    }
    
    private static readonly Vector3 LightColor1 = new Vector3(0.31f, 0.902f, 0.486f);
    private static readonly Vector3 LightColor2 = new Vector3(0f, 0.604f, 0.369f);
    
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
	    float lerp = (Main.DiscoR + Main.DiscoG) / 512f;
	    Vector3 c = LightColor1 * lerp + LightColor2 * (1f - lerp);
	    c *= Math.Max(Main.rand.NextFloat(0.95f, 1.025f), Main.rand.NextFloat(0.95f, 1.025f));
		r = c.X;
		g = c.Y;
		b = c.Z;
	}

    public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
	    if (tileFrameY != 0) return;
		Vector2 referencePoint = new Point(i, j).ToWorldCoordinates(4 - tileFrameX, 6);
		
		ulong seed = Main.TileFrameSeed ^ (ulong)((long)(j + tileFrameX) << 32 | (long)(uint)i); // Don't remove any casts.
		bool makeDust = Utils.RandomInt(ref seed, 6) == 0 || Utils.RandomInt(ref seed, 6) == 0;
		if (!visible || !makeDust) {
			return;
		}

		Dust d = Dust.NewDustDirect(
			referencePoint,
			24, 4, DustID.CoralTorch);
		d.velocity *= 0.1f;
		d.noGravity = true;
		d.velocity.Y -= 2;
		d.position -= d.scale * new Vector2(3);
		d.fadeIn = 0.3f;
		d.noLightEmittence = true;
	}

	public override void RandomUpdate(int i, int j)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		Tile tile = Main.tile[i, j];
		if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
			return;
		if (SeekProjectileTicker.SeekProjectiles.Count >= 5)
			return;
		bool playerIsNearby = false;
		Vector2 pos = new Point(i, j).ToWorldCoordinates(16, 0);
		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Center.DistanceSQ(pos) < 2560000)
				//checks for players within 100 tiles
			{
				playerIsNearby = true;
				break;
			}
		}
		if (!playerIsNearby) return;
		foreach (Projectile flame in SeekProjectileTicker.SeekProjectiles)
		{
			if (flame.DistanceSQ(pos) < 16 * 16)
			{
				return;
			}
		}
		Projectile.NewProjectile(
			new EntitySource_TileUpdate(i, j),
			pos,
			Vector2.Zero,
			ModContent.ProjectileType<LostWisp>(), 40, 0);
	}
}