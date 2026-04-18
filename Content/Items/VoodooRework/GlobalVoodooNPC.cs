using System.Collections.Generic;
using Relogiced.Content.Items.VoodooRework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Relogiced.Common.GlobalNPCs;

class GlobalVoodooNPC : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        if (!Main.rand.NextBool(5)) return;
        if (Main.LocalPlayer.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(npc.type) ||
            (Main.LocalPlayer.killGuide && npc.type == NPCID.Guide) ||
            (Main.LocalPlayer.killClothier && npc.type == NPCID.Clothier))
        {
            chat = Mod.GetLocalization("Chats.VoodooUnhappy." + NPCID.Search.GetName(npc.netID)).Value;
        }
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Clothier && shop.Name == "Shop")
        {
            shop.Add(new NPCShop.Entry(
                ItemID.GuideVoodooDoll,
                Condition.NpcIsPresent(NPCID.Guide),
                Condition.DownedEowOrBoc
            ));
            shop.Add(new NPCShop.Entry(
                ModContent.ItemType<GoblinVoodooDoll_On>(),
                Condition.NpcIsPresent(NPCID.GoblinTinkerer),
                Condition.MoonPhasesNearNew
            ));
            shop.Add(new NPCShop.Entry(
                ModContent.ItemType<MechanicVoodooDoll_On>(),
                Condition.NpcIsPresent(NPCID.Mechanic),
                Condition.MoonPhasesNearNew
            ));
            shop.Add(new NPCShop.Entry(
                ItemID.ClothierVoodooDoll,
                Condition.HappyEnough
            ));
        }
    }
    
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        foreach (var rule in npcLoot.Get())
        {
            if (rule is not CommonDrop drop) continue;
            if (drop.itemId == ItemID.ClothierVoodooDoll)
            {
                drop.itemId = ModContent.ItemType<ClothierVoodooDoll_Off>();
                continue;
            }
            if (drop.itemId == ItemID.GuideVoodooDoll)
            {
                drop.itemId = ModContent.ItemType<GuideVoodooDoll_Off>();
            }
        }
    }
    
    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (player.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(npc.type))
        {
            return true;
        }
        return base.CanBeHitByItem(npc, player, item);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (projectile.friendly && projectile.TryGetOwner(out Player player) &&
            player.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(npc.type))
        {
            return true;
        }
        return base.CanBeHitByProjectile(npc, projectile);
    }
}