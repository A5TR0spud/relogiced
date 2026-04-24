using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Relogiced.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.ConditionFillers;

public class EpitaphNPC : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.Epitaph;
    }

    public override void OnKill(NPC npc)
    {
        Player nearestHardcorePlayer = null;
        float dist = -1;
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.difficulty != PlayerDifficultyID.Hardcore) continue;
            float newDist = player.Center.DistanceSQ(npc.Center);
            if (nearestHardcorePlayer == null || dist < 0 || newDist < dist)
            {
                nearestHardcorePlayer = player;
                dist = newDist;
            }
        }

        if (dist < 0 || nearestHardcorePlayer == null) return;
        
        if (!Main.rand.NextBool(EpitaphPlayer.GetConsequentForEpitaphDrop(nearestHardcorePlayer))) return;
        Item toDrop = new Item(ModContent.ItemType<ForgottenEpitaph>());
        toDrop.noGrabDelay = 100;
        Item.NewItem(
            npc.GetSource_DropAsItem(),
            npc.Center,
            toDrop,
            noGrabDelay: true
        );
    }
}

public class EpitaphPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.Epitaph;
    }

    public override void Load()
    {
        On_Player.UpdateGraveyard += On_PlayerOnUpdateGraveyard;
    }

    public override void Unload()
    {
        On_Player.UpdateGraveyard -= On_PlayerOnUpdateGraveyard;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Main.netMode is not NetmodeID.SinglePlayer and not NetmodeID.Server)
            return;
        
        if (Player.difficulty != PlayerDifficultyID.Hardcore && !Main.rand.NextBool(GetConsequentForEpitaphDrop(Player))) return;
        Item toDrop = new Item(ModContent.ItemType<ForgottenEpitaph>());
        toDrop.noGrabDelay = 100;
        Item.NewItem(
            Player.GetSource_DropAsItem(),
            Player.Center,
            toDrop,
            noGrabDelay: true
        );
    }

    public static int GetConsequentForEpitaphDrop(Player player)
    {
        int consequent = 10;
        if (player.GetModPlayer<EpitaphPlayer>().PreviouslyAccursed)
        {
            consequent *= 2;
        }
        if (Main.expertMode)
        {
            consequent += 5;
        }
        if (Main.masterMode)
        {
            consequent += 10;
        }
        if (Main.bloodMoon)
        {
            consequent /= 2;
        }
        if (Main.hardMode)
        {
            consequent = (int)(consequent * 0.85f);
        }
        if (NPC.downedGolemBoss)
        {
            consequent += 3;
        }

        return consequent;
    }

    private void On_PlayerOnUpdateGraveyard(On_Player.orig_UpdateGraveyard orig, Player self, bool now)
    {
        if (self.GetModPlayer<EpitaphPlayer>().PreviouslyAccurseMe)
        {
            self.ZoneGraveyard = true;
        }
        if (Main.myPlayer == self.whoAmI && self.GetModPlayer<EpitaphPlayer>().PreviouslyAccurseMe)
            Main.SceneMetrics.GraveyardTileCount = Math.Max((SceneMetrics.GraveyardTileMin + SceneMetrics.GraveyardTileThreshold) / 2, Main.SceneMetrics.GraveyardTileCount);
        orig(self, now);
    }

    public bool AccursedThing = false;
    public bool PreviouslyAccursed = false;
    public bool AccurseMe = false;
    public bool PreviouslyAccurseMe = false;

    public override void ResetEffects()
    {
        PreviouslyAccursed = AccursedThing;
        AccursedThing = false;
        PreviouslyAccurseMe = AccurseMe;
        AccurseMe = false;
    }

    public override void PostUpdate()
    {
        //AccursedThing = Player.HasItemInInventoryOrOpenVoidBag(ModContent.ItemType<ForgottenEpitaph>());
        AccurseMe = AccursedThing;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            foreach (Player plr in Main.ActivePlayers)
            {
                if (plr.Center.Distance(Player.Center) < 16 * 20 && plr.GetModPlayer<EpitaphPlayer>().PreviouslyAccursed)
                {
                    AccurseMe = true;
                    break;
                }
            }
        }
        if (AccurseMe)
        {
            Player.luck -= 0.1f;
            Player.buffImmune[BuffID.Sunflower] = true;
        }
    }
}

public class Epitaph : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.Epitaph;
    }

    //public override LocalizedText Tooltip => Relogiced.Instance.GetLocalization("Items.Epitaph.Tooltip").WithFormatArgs(Main.LocalPlayer.name);

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerCountsAsItem[Type] = ModContent.ItemType<ForgottenEpitaph>();
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 50);
        Item.maxStack = 1;
    }

    public override bool CanRightClick()
    {
        return true;
    }
    
    public override bool ConsumeItem(Player player)
    {
        return false;
    }

    public override void RightClick(Player player)
    {
        RelogicedUtil.ChangeItemTypeFromRMB(Item, ModContent.ItemType<ForgottenEpitaph>());
    }
}

public class ForgottenEpitaph : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.Epitaph;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ModContent.ItemType<Epitaph>());
    }

    public override void UpdateInventory(Player player)
    {
        player.GetModPlayer<EpitaphPlayer>().AccursedThing = true;
    }

    public override void AddRecipes()
    {
        Recipe.Create(Type)
            .AddIngredient(ItemID.Marble, 50)
            .AddIngredient(ItemID.RedMoss, 5)
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft()
            .AddCondition(Condition.InGraveyard)
            .Register();
        Recipe.Create(Type)
            .AddIngredient(ItemID.Marble, 50)
            .AddIngredient(ItemID.BrownMoss, 5)
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft()
            .AddCondition(Condition.InGraveyard)
            .Register();
        Recipe.Create(Type)
            .AddIngredient(ItemID.Marble, 50)
            .AddIngredient(ItemID.GreenMoss, 5)
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft()
            .AddCondition(Condition.InGraveyard)
            .Register();
        Recipe.Create(Type)
            .AddIngredient(ItemID.Marble, 50)
            .AddIngredient(ItemID.BlueMoss, 5)
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft()
            .AddCondition(Condition.InGraveyard)
            .Register();
        Recipe.Create(Type)
            .AddIngredient(ItemID.Marble, 50)
            .AddIngredient(ItemID.PurpleMoss, 5)
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft()
            .AddCondition(Condition.InGraveyard)
            .Register();
    }

    public override bool CanRightClick()
    {
        return true;
    }
    
    public override bool ConsumeItem(Player player)
    {
        return false;
    }

    public override void RightClick(Player player)
    {
        RelogicedUtil.ChangeItemTypeFromRMB(Item, ModContent.ItemType<Epitaph>());
    }
}