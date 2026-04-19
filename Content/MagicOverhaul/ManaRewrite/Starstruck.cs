using System;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Relogiced.Content.MagicOverhaul.ManaRewrite;

public class Starstruck : ModBuff
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite;
    }

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<ManaRewritePlayer>().starstruck = true;
        player.statDefense -= 4;
        player.tipsy = true;
    }
}