using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Relogiced.Content.MagicOverhaul.ManaRewrite;

public class ManaRewritePlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite;
    }

    public override void Load()
    {
        On_Player.CheckMana_int_bool_bool += On_PlayerOnCheckMana_int_bool_bool;
        On_Player.CheckMana_Item_int_bool_bool += On_PlayerOnCheckMana_Item_int_bool_bool;
    }

    public override void Unload()
    {
        On_Player.CheckMana_int_bool_bool -= On_PlayerOnCheckMana_int_bool_bool;
        On_Player.CheckMana_Item_int_bool_bool -= On_PlayerOnCheckMana_Item_int_bool_bool;
    }

    private bool On_PlayerOnCheckMana_Item_int_bool_bool(On_Player.orig_CheckMana_Item_int_bool_bool orig, Player self, Item item, int amount, bool pay, bool blockQuickMana)
    {
        if (amount <= -1)
            amount = self.GetManaCost(item);
        if (amount > self.statManaMax2)
            return false;
        return orig(self, item, (int)(amount * (self.statMana - 1) / (float)self.statManaMax2 + 0.5f), true, true);
    }

    private bool On_PlayerOnCheckMana_int_bool_bool(On_Player.orig_CheckMana_int_bool_bool orig, Player self, int amount, bool pay, bool blockQuickMana)
    {
        if ((int)(amount * self.manaCost) > self.statManaMax2)
            return false;
        return orig(self, (int)(amount * (self.statMana - 1) / (float)self.statManaMax2 + 0.5f), true, true);
    }

    public float manaTimer = 0;
    
    public override void UpdateDead()
    {
        manaTimer = 0;
    }

    public override void OnRespawn()
    {
        manaTimer = 0;
    }

    public override void PostUpdate()
    {
        manaTimer += (Player.statManaMax2 - Player.statMana) / 10f;
        if (manaTimer >= 60)
        {
            if (Player.statMana < Player.statManaMax2)
                Player.statMana++;
            manaTimer -= 60;
        }
        Player.UpdateManaRegen();
        float forgiveness = Player.manaFlower ? 0.33f : 0.25f;
        float penalty = forgiveness + (1f - forgiveness) * Player.statMana / (float)Player.statManaMax2;
        Player.GetDamage(DamageClass.Magic) *= penalty;
    }
}

public class ManaRewriteItem : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigMagicOverhaul.ManaRewrite;
    }

    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        return item.healMana > 0 || IsManaFlower(item);
    }

    public static bool IsManaPotion(Item item) =>
        item.type is ItemID.ManaPotion or ItemID.GreaterManaPotion
            or ItemID.LesserManaPotion or ItemID.SuperManaPotion;

    public static bool IsManaFlower(Item item) =>
        item.type is ItemID.ManaFlower or ItemID.ManaCloak
            or ItemID.ArcaneFlower or ItemID.MagnetFlower;

    public override void SetDefaults(Item item)
    {
        if (item.healMana > 0)
        {
            item.buffTime = (int)(MaxManaBuff.BuffTimePerPotion * (item.healMana / 50f));
            item.healMana = 0;
            item.buffType = ModContent.BuffType<MaxManaBuff>();
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (IsManaFlower(item))
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (line.Name == "Tooltip1" && line.Mod == "Terraria")
                {
                    line.Text = ItemTooltip.FromLocalization(Mod.GetLocalization("Items.ManaFlowerTip")).GetLine(0);
                    break;
                }
            }
        }

        if (item.buffType == ModContent.BuffType<MaxManaBuff>())
        {
            ItemTooltip tip = ItemTooltip.FromLocalization(Mod.GetLocalization("Items.ManaPotionTip")
                .WithFormatArgs(MaxManaBuff.GetManaBuffFromDuration(item.buffTime)));
            int idx = tooltips.FindIndex(i => i.FullName == "Terraria/BuffTime");
            if (idx < 0) idx = tip.Lines;
            for (int i = 0; i < tip.Lines; i++)
            {
                TooltipLine line = new TooltipLine(Relogiced.Instance, "Tooltip" + i, tip.GetLine(i));
                tooltips.Insert(idx + i, line);
            }
        }
    }
}