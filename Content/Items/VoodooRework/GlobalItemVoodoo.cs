using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class GlobalItemVoodoo : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.VoodooRework;
    }

    private int GetValue(Item item)
    {
        int v = item.value;
        bool d = false;
        ItemLoader.ReforgePrice(item.Clone(), ref v, ref d);
        return v;
    }

    public override void PostReforge(Item item)
    {
        if (Main.LocalPlayer == null || !Main.LocalPlayer.active) return;
        if (!Main.LocalPlayer.GetModPlayer<ModVoodooPlayer>().KillableNPCs.Contains(NPCID.GoblinTinkerer)
            || !Main.reforgeItem.Equals(item))
            return;
        Item cpy = item.Clone();
        cpy.ResetPrefix();
        if (GetValue(item) > GetValue(cpy))
        {
            item.ResetPrefix();
            cpy.Prefix(-2);
            if (GetValue(cpy) <= GetValue(item))
                item.Prefix(cpy.prefix);
        }
    }
}