using Relogiced.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.VoodooRework;

public class GuideVoodooDoll_Off : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.GuideVoodooDoll);
    }

    public override int AssociatedNPC() => 0;

    public override int OtherVariant() => ItemID.GuideVoodooDoll;
}
public class ClothierVoodooDoll_Off : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ClothierVoodooDoll);
    }

    public override int AssociatedNPC() => 0;

    public override int OtherVariant() => ItemID.ClothierVoodooDoll;
}
public class MechanicVoodooDoll_On : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ClothierVoodooDoll);
    }
    public override int AssociatedNPC() => NPCID.Mechanic;
    public override int OtherVariant() => ModContent.ItemType<MechanicVoodooDoll_Off>();
}
public class MechanicVoodooDoll_Off : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(OtherVariant());
    }
    public override int AssociatedNPC() => 0;
    public override int OtherVariant() => ModContent.ItemType<MechanicVoodooDoll_On>();
}
public class GoblinVoodooDoll_On : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ClothierVoodooDoll);
    }
    public override int AssociatedNPC() => NPCID.GoblinTinkerer;
    public override int OtherVariant() => ModContent.ItemType<GoblinVoodooDoll_Off>();
}
public class GoblinVoodooDoll_Off : VoodooDollItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(OtherVariant());
    }
    public override int AssociatedNPC() => 0;
    public override int OtherVariant() => ModContent.ItemType<GoblinVoodooDoll_On>();
}