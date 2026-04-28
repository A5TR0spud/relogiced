using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Relogiced.Content.RangedOverhaul.Chekhov;

public class Chekhov : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.LightRed;
        Item.damage = 1000;
        Item.crit = 13; //13 + 4 = 17 ~ 16.66667 == 1/6
        Item.DamageType = DamageClass.Ranged;
        Item.useAmmo = AmmoID.Bullet;
        Item.shootSpeed = 10;
        Item.knockBack = 8;
        Item.useTime = 30;
        Item.value = Item.sellPrice(gold: 2, silver: 50);
    }

    public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
    {
        reforgePrice = Item.buyPrice(gold: 20) * 3;
        return false;
    }

    public override bool CanUseItem(Player player)
    {
        return false;
    }

    public override bool NeedsAmmo(Player player)
    {
        return false;
    }

    public override void UpdateInventory(Player player)
    {
        ChekhovPlayer plr = player.GetModPlayer<ChekhovPlayer>();
        if (!plr.HasChekhov)
        {
            plr.HasChekhov = true;
            plr.FirstChekhov = Item;
        }
    }
}

public class ChekhovShopHelper : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }

    public override void SetupTravelShop(int[] shop, ref int nextSlot)
    {
        if (nextSlot >= shop.Length)
            return;
        if (Main.GetMoonPhase() == MoonPhase.Full && !Main.hardMode && Main.rand.NextBool(400))
        {
            shop[nextSlot++] = ModContent.ItemType<Chekhov>();
        }
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.ArmsDealer && shop.Name == "Shop")
        {
            shop.Add(ModContent.ItemType<Chekhov>(), Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.MoonPhaseNew, Condition.PreHardmode);
            shop.Add(ModContent.ItemType<Chekhov>(), Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.MoonPhaseFull, Condition.PreHardmode);
            shop.Add(ModContent.ItemType<Chekhov>(), Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.EclipseOrBloodMoon, Condition.Hardmode);
        }
    }
}

public class ChekhovPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }

    public bool HasChekhov = false;
    public int ChekhovGunCounter = 0;
    private const string COUNTER_KEY = "ChekhovGunCounter";
    public Item FirstChekhov = null;
    private const int INTERVALS_PER_SHOT = 45 * 30 * 60 / 5; //leading term is approximate time to trigger, in minutes

    public override void SaveData(TagCompound tag)
    {
        if (ChekhovGunCounter != 0)
            tag.Add(COUNTER_KEY, ChekhovGunCounter);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey(COUNTER_KEY))
            ChekhovGunCounter = tag.GetAsInt(COUNTER_KEY);
    }

    public override void ResetEffects()
    {
        HasChekhov = false;
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        if (!HasChekhov)
            return;
        if (!Main.hardMode)
            return;
        if (Player.miscCounter % 300 != 0) //5 seconds
            return;
        int consequent = 2 * INTERVALS_PER_SHOT - ChekhovGunCounter;
        if (ChekhovGunCounter > 0 && (consequent < 1 || Main.rand.NextBool(consequent)))
        {
            if (TryChekhov())
            {
                ChekhovGunCounter -= INTERVALS_PER_SHOT;
            }
            else
            {
                ChekhovGunCounter -= INTERVALS_PER_SHOT / 2;
            }
        }
        ChekhovGunCounter += (int)(30 * 30f / FirstChekhov.useTime * Player.GetWeaponAttackSpeed(FirstChekhov));
    }

    private bool TryChekhov()
    {
        NPC nearest = PlotDevice.GetChekhovTarget(Player.Center, Player);

        if (nearest != null && Player.PickAmmo(FirstChekhov,
                out int projToCopyExtraUpdatesOf,
                out float speed,
                out int damage,
                out float kb,
                out int ammoItemID))
        {
            Projectile proj = Projectile.NewProjectileDirect(
                Player.GetSource_ItemUse_WithPotentialAmmo(FirstChekhov, ammoItemID),
                Player.Center,
                speed * (nearest.Center - Player.Center).SafeNormalize(-Vector2.UnitY),
                ModContent.ProjectileType<PlotDevice>(),
                damage,
                kb,
                Player.whoAmI,
                ai0: nearest.whoAmI + 1,
                ai2: projToCopyExtraUpdatesOf
            );
            proj.CritChance += Player.GetWeaponCrit(FirstChekhov);
            proj.ArmorPenetration += Player.GetWeaponArmorPenetration(FirstChekhov);
            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Top,
                new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2),
                ModContent.ProjectileType<ChekhovVisualizer>(),
                0,
                0
            );
            return true;
        }

        return false;
    }
}