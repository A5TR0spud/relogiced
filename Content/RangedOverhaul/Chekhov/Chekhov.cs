using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Relogiced.Other;
using Terraria;
using Terraria.Audio;
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
        Item.damage = 1860;
        Item.crit = 13; //13 + 4 = 17 ~ 16.66667 == 1/6
        Item.DamageType = DamageClass.Ranged;
        Item.useAmmo = AmmoID.Bullet;
        Item.shootSpeed = 10;
        Item.knockBack = 8;
        Item.useTime = 30;
        Item.value = Item.buyPrice(gold: 20);
        HasBeenNotified = Main.hardMode;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!Main.hardMode)
            return;
        RelogicedUtil.ReplaceTooltip(tooltips, "Items.Chekhov.TooltipActive");
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

    internal bool HasBeenNotified = false;

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(HasBeenNotified);
    }

    public override void NetReceive(BinaryReader reader)
    {
        HasBeenNotified = reader.ReadBoolean();
    }

    private const string NOTIFIED_KEY = "Relogiced/Chekhov:_hasBeenNotified";

    public override void SaveData(TagCompound tag)
    {
        if (HasBeenNotified)
            tag.Add(NOTIFIED_KEY, HasBeenNotified);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey(NOTIFIED_KEY))
            HasBeenNotified = tag.GetBool(NOTIFIED_KEY);
        else
            HasBeenNotified = Main.hardMode;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
        Vector2 origin, float scale)
    {
        if (Main.hardMode && !HasBeenNotified)
        {
            Item.newAndShiny = true;
            HasBeenNotified = true;
        }
        return true;
    }

    public override void UpdateInventory(Player player)
    {
        ChekhovPlayer plr = player.GetModPlayer<ChekhovPlayer>();
        if (plr.FirstChekhov == null)
        {
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
        if (Main.rand.NextBool(Main.hardMode ? 1000 : 400))
        {
            shop[nextSlot++] = ModContent.ItemType<Chekhov>();
        }
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.ArmsDealer && shop.Name == "Shop")
        {
            shop.Add(new Item(ModContent.ItemType<Chekhov>()) { shopCustomPrice = Item.buyPrice(gold: 30)},
                Condition.NotDownedQueenBee, Condition.NotDownedEowOrBoc, Condition.NotDownedGoblinArmy,
                Condition.LanternNight, Condition.PreHardmode);
            shop.Add(new Item(ModContent.ItemType<Chekhov>()) { shopCustomPrice = Item.buyPrice(gold: 50)},
                Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.TimeNight, Condition.MoonPhaseNew,
                Condition.PreHardmode);
            shop.Add(new Item(ModContent.ItemType<Chekhov>()) { shopCustomPrice = Item.buyPrice(gold: 50)},
                Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.TimeNight, Condition.MoonPhaseFull,
                Condition.PreHardmode);
            shop.Add(new Item(ModContent.ItemType<Chekhov>()) { shopCustomPrice = Item.buyPrice(platinum: 1, gold: 50)},
                Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.BloodMoon, Condition.Hardmode);
            shop.Add(new Item(ModContent.ItemType<Chekhov>()) { shopCustomPrice = Item.buyPrice(platinum: 1, gold: 50)},
                Condition.DownedEowOrBoc, Condition.DownedSkeletron, Condition.Eclipse, Condition.Hardmode);
        }
    }
}

public class ChekhovPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigRangedOverhaul.Chekhov;
    }

    public int ChekhovGunCounter = 0;
    private const string COUNTER_KEY = "Relogiced/ChekhovPlayer:ChekhovGunCounter";
    public Item FirstChekhov = null;
    private const int INTERVALS_PER_SHOT = 60 * 30 * 60 / 5; //leading term is approximate time to trigger, in minutes

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
        FirstChekhov = null;
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        if (FirstChekhov == null || FirstChekhov.IsAir)
            return;
        if (Main.hardMode && FirstChekhov.ModItem is Chekhov chk && !chk.HasBeenNotified)
        {
            FirstChekhov.newAndShiny = true;
            SoundEngine.PlaySound(SoundID.Grab);
            chk.HasBeenNotified = true;
        }
        if (!Main.hardMode)
            return;
        if (Player.miscCounter % 300 != 0) //5 seconds
            return;
        float threshold = 0.6321f * ChekhovGunCounter / (float)INTERVALS_PER_SHOT;
        threshold *= threshold;
        if (ChekhovGunCounter > 0 && Main.rand.Next(INTERVALS_PER_SHOT) < (int)(threshold * INTERVALS_PER_SHOT))
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