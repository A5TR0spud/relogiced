using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class ModVoodooPlayer : ModPlayer
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }
/*
    public override void Load()
    {
        On_Player.GetItemExpectedPrice += On_PlayerOnGetItemExpectedPrice;
    }

    public override void Unload()
    {
        On_Player.GetItemExpectedPrice -= On_PlayerOnGetItemExpectedPrice;
    }

    private void On_PlayerOnGetItemExpectedPrice(On_Player.orig_GetItemExpectedPrice orig, Player self, Item item, out long calcForSelling, out long calcForBuying)
    {
        orig.Invoke(self, item, out calcForSelling, out calcForBuying);
        if (self.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(self.TalkNPC.type))
        {
            if (!item.buyOnce)
                calcForBuying = (int)((float)calcForBuying * 1.2f);
            if (item.isAShopItem)
                calcForSelling = (int)((float)calcForSelling * 0.7f);
            Main.shopSellbackHelper.Remove(item);
        }
    }
    */
    public List<int> KillableNPCs = [];

    override public void ResetEffects()
    {
        KillableNPCs.Clear();
    }
    
    public override void PostUpdateEquips()
    {
        for (int i = 0; i < Player.inventory.Length; i++)
        {
            Item test = Player.inventory[i];
            DueDiligence(test);
        }
        if (Player.useVoidBag())
        {
            for (int i = 0; i < Player.bank4.item.Length; i++)
            {
                Item test = Player.bank4.item[i];
                DueDiligence(test);
            }
        }
    }

    private void DueDiligence(Item item)
    {
        if (item.IsAir) return;
        if (item.type == ItemID.GuideVoodooDoll)
        {
            Player.killGuide = true;
            return;
        }
        if (item.type == ItemID.ClothierVoodooDoll)
        {
            Player.killClothier = true;
            return;
        }
        if (item.ModItem is VoodooDollItem voodooDoll &&
            voodooDoll.AssociatedNPC() != 0 &&
            !KillableNPCs.Contains(voodooDoll.AssociatedNPC()))
        {
            KillableNPCs.Add(voodooDoll.AssociatedNPC());
        }
    }

    public override bool CanHitNPC(NPC target)
    {
        return base.CanHitNPC(target) || KillableNPCs.Contains(target.type);
    }
}