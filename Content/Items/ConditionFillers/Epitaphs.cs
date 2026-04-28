using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Humanizer;
using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Relogiced.Content.Items.ConditionFillers;

public class EpitaphPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.ConditionFillers;
    }

    public override void Load()
    {
        On_Player.UpdateGraveyard += On_PlayerOnUpdateGraveyard;
        On_Player.DropTombstone += On_PlayerOnDropTombstone;
        On_NPC.DropTombstoneTownNPC += On_NPCOnDropTombstoneTownNPC;
    }

    private void On_PlayerOnDropTombstone(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, NetworkText deathText, int hitDirection)
    {
        if (self.difficulty == PlayerDifficultyID.Hardcore ||
            !ReplaceTombstoneWithEpitaph(self, self.GetModPlayer<EpitaphPlayer>().AccurseMe ? 30 : 0, deathText))
            orig(self, coinsOwned, deathText, hitDirection);
    }

    private void On_NPCOnDropTombstoneTownNPC(On_NPC.orig_DropTombstoneTownNPC orig, NPC self, NetworkText deathText)
    {
        if (!ReplaceTombstoneWithEpitaph(self, 10, deathText))
            orig(self, deathText);
    }

    public static bool ReplaceTombstoneWithEpitaph(Entity originator, int consequentDelta, NetworkText deathText)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return false;
        int consequent = 10 + consequentDelta;
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
        if (NPC.downedAncientCultist)
        {
            consequent += 3;
        }
        if (Main.hardMode)
        {
            consequent = (int)(consequent * 0.85f);
        }
        if (NPC.downedGolemBoss)
        {
            consequent += 3;
        }
        if (!Main.rand.NextBool(consequent)) return false;
        
        Item toDrop = new Item(ModContent.ItemType<Epitaph>());
        ((Epitaph)toDrop.ModItem).DeathMessage = deathText;
        toDrop.noGrabDelay = 100;
        Item.NewItem(
            originator.GetSource_Death(),
            originator.Center,
            toDrop,
            noGrabDelay: true
        );
        return true;
    }

    public override void Unload()
    {
        On_Player.UpdateGraveyard -= On_PlayerOnUpdateGraveyard;
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

public abstract class EpitaphBase : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.ConditionFillers;
    }
    
    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 50);
        Item.maxStack = 1;
    }

    public override ModItem Clone(Item newEntity)
    {
        if (newEntity.type == ModContent.ItemType<Epitaph>() || newEntity.type == ModContent.ItemType<ForgottenEpitaph>())
        {
            EpitaphBase e = null;
            if (newEntity.ModItem is EpitaphBase eepb)
                e = eepb;
            else
                e = (EpitaphBase)(new Item(newEntity.type).ModItem);
            e.DeathMessage = DeathMessage;
            e.Inscription = Inscription ?? DeathMessage.ToString();
        }
        return base.Clone(newEntity);
    }

    public NetworkText DeathMessage = NetworkText.Empty;
    
    public string Inscription = null;

    public override void NetReceive(BinaryReader reader)
    {
        DeathMessage = NetworkText.Deserialize(reader);
        Inscription = DeathMessage.ToString();
    }

    public override void NetSend(BinaryWriter writer)
    {
        if (DeathMessage == NetworkText.Empty && !string.IsNullOrEmpty(Inscription))
        {
            DeathMessage = NetworkText.FromLiteral(Inscription);
        }
        DeathMessage.Serialize(writer);
    }

    public override void SaveData(TagCompound tag)
    {
        if (string.IsNullOrEmpty(Inscription))
        {
            Inscription = DeathMessage.ToString();
        }
        if (string.IsNullOrEmpty(Inscription)) return;
        tag.Set("Epithet", Inscription);
    }

    public string GetInscription()
    {
        if (DeathMessage != null && DeathMessage != NetworkText.Empty && !string.IsNullOrEmpty(DeathMessage.ToString()))
        {
            return DeathMessage.ToString();
        }

        if (string.IsNullOrEmpty(Inscription))
        {
            return Relogiced.Instance.GetLocalization("Items.Epitaph.Epithet").Value;
        }

        return Inscription;
    }
    
    public override void LoadData(TagCompound tag)
    {
        Inscription = tag.ContainsKey("Epithet") ? tag.GetString("Epithet") : null;
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
        bool isALongForgottenMonument = Item.type == ModContent.ItemType<ForgottenEpitaph>();
        int itemTypeToBecome =
            isALongForgottenMonument ? ModContent.ItemType<Epitaph>() : ModContent.ItemType<ForgottenEpitaph>();
        string beforeI = Inscription;
        NetworkText beforeD = DeathMessage;
        if (RelogicedUtil.ChangeItemTypeFromRMB(Item, itemTypeToBecome) && Item.ModItem is EpitaphBase eepb)
        {
            eepb.Inscription = beforeI;
            eepb.DeathMessage = beforeD;
        }
    }
}

public class Epitaph : EpitaphBase
{
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        foreach (TooltipLine line in tooltips.Where(line => line.FullName.StartsWith("Terraria/Tooltip")))
        {
            line.Text = line.Text.FormatWith(GetInscription());
        }
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerCountsAsItem[Type] = ModContent.ItemType<ForgottenEpitaph>();
    }
}

public class ForgottenEpitaph : EpitaphBase
{
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
}