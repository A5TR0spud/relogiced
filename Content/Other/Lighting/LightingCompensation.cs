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

    private const float RLumen = 0.299f;
    private const float GLumen = 0.587f;
    private const float BLumen = 0.114f;
    
    //TODO: figure out how to stop unwanted flickering
    //TODO: figure out why blue is not being brightened

    public static Vector3 ProcessLight(Vector3 color)
    {
        Vector3 newColor = color * GetValue(color) / GetLuminance(color);
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
        Math.Max(
            color.X,
            Math.Max(color.Y, color.Z)
        );

    public static float GetValue(Vector3 color) =>
        Math.Max(
            color.X,
            Math.Max(color.Y, color.Z)
        );

    public static float GetLuminance(Vector3 color) =>
        (float)Math.Sqrt(
            RLumen * (color.X * color.X)
            + GLumen * (color.Y * color.Y)
            + BLumen * (color.Z * color.Z)
        );
}