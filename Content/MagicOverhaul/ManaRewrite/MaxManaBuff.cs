using System;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.ManaRewrite;

public class MaxManaBuff : ModBuff
{
    public const int BuffTimePerPotion = 60 * 60 * 5;

    private Asset<Texture2D> texSheet;

    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite;
    }

    public override void Load()
    {
        texSheet = RelogicedUtil.GetAsset("Content/MagicOverhaul/ManaRewrite/MaxManaBuff_Sheet");
    }

    public override void Unload()
    {
        texSheet = null;
    }

    public static int GetManaBuffFromDuration(int time)
    {
        return 20 * ((time - 1) / BuffTimePerPotion + 1);
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statManaMax2 += GetManaBuffFromDuration(player.buffTime[buffIndex]);
        player.manaCost *= 0.9f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
    {
        Player player = Main.LocalPlayer;
        int power = (player.buffTime[player.FindBuffIndex(Type)] - 1) / BuffTimePerPotion;
        power = Math.Clamp(power, 0, 5);
        drawParams.Texture = texSheet.Value;
        drawParams.SourceRectangle = new Rectangle(0, 34 * power, 32, 32);
        return true;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        Player player = Main.LocalPlayer;
        int power = GetManaBuffFromDuration(player.buffTime[player.FindBuffIndex(Type)]);
        tip = tip.FormatWith(power);
    }
}