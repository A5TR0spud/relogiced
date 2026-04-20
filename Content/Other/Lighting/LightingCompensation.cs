using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace Relogiced.Content.Other.Lighting;

public class LightingCompensation : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigClient.LightingCompensation;
    }

    public override void Load()
    {
        On_LegacyLighting.AddLight += On_LegacyLightingOnAddLight;
        On_LightingEngine.AddLight += On_LightingEngineOnAddLight;
    }

    public override void Unload()
    {
        On_LegacyLighting.AddLight -= On_LegacyLightingOnAddLight;
        On_LightingEngine.AddLight -= On_LightingEngineOnAddLight;
    }

    private void On_LegacyLightingOnAddLight(On_LegacyLighting.orig_AddLight orig, LegacyLighting self, int x, int y, Vector3 color)
    {
        if (Terraria.Lighting.Mode is LightMode.Color)
            color = ProcessLight(color);
        orig(self, x, y, color);
    }
    
    private void On_LightingEngineOnAddLight(On_LightingEngine.orig_AddLight orig, LightingEngine self, int x, int y, Vector3 color)
    {
        if (Terraria.Lighting.Mode is LightMode.Color)
            color = ProcessLight(color);
        orig(self, x, y, color);
    }

    private const float R_LUMEN = 0.299f;
    private const float G_LUMEN = 0.587f;
    private const float B_LUMEN = 0.114f;

    public static Vector3 ProcessLight(Vector3 color)
    {
        float lumen = GetLuminance(color);
        if (lumen <= 0) return color;
        color *= 0.707106781187f;
        Vector3 newColor = color * GetValue(color) / lumen;
        Vector3 blownOutColor = Clamp(newColor);
        Vector3 overflow = newColor - blownOutColor;
        if (overflow.X > 0)
        {
            blownOutColor.Y += 0.5f * overflow.X;
            blownOutColor.Z += 0.5f * overflow.X;
        }
        if (overflow.Y > 0)
        {
            blownOutColor.X += 0.5f * overflow.Y;
            blownOutColor.Z += 0.5f * overflow.Y;
        }
        if (overflow.Z > 0)
        {
            blownOutColor.Y += 0.5f * overflow.Z;
            blownOutColor.X += 0.5f * overflow.Z;
        }

        return blownOutColor;
    }

    public static Vector3 Clamp(Vector3 color)
    {
        color.X = Math.Clamp(color.X, 0, 1);
        color.Y = Math.Clamp(color.Y, 0, 1);
        color.Z = Math.Clamp(color.Z, 0, 1);
        return color;
    }

    public static float GetSaturation(Vector3 color) =>
        GetValue(color) - GetDimmest(color);

    public static float GetDimmest(Vector3 color) =>
        Math.Min(
            color.X,
            Math.Min(color.Y, color.Z)
        );

    public static float GetValue(Vector3 color) =>
        Math.Max(
            color.X,
            Math.Max(color.Y, color.Z)
        );

    public static float GetLuminance(Vector3 color) =>
        (float)Math.Sqrt(
            R_LUMEN * (color.X * color.X)
            + G_LUMEN * (color.Y * color.Y)
            + B_LUMEN * (color.Z * color.Z)
        );
}

public class LightingCompTile : GlobalTile
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigClient.LightingCompensation;
    }

    public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
    {
        Vector3 col = new Vector3(r, g, b);
        if (Main.tileLighted[type] && LightingCompensation.GetValue(col) > 0)
        {
            col = LightingCompensation.ProcessLight(col);
            r = col.X;
            g = col.Y;
            b = col.Z;
        }
    }
}