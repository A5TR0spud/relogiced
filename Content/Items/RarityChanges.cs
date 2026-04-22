using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items;

public class RarityChanges : GlobalItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.RarityChanges;
    }

    public override void SetDefaults(Item item)
    {
        if (ItemID.Sets.IsLavaImmuneRegardlessOfRarity[item.type] &&
            item.rare is ItemRarityID.White or ItemRarityID.Gray)
        {
            item.rare = ItemRarityID.Blue;
            return;
        }

        if (item.type is
            ItemID.LihzahrdAltar or
            ItemID.LihzahrdBanner or
            ItemID.LihzahrdBathtub or
            ItemID.LihzahrdBed or
            ItemID.LihzahrdBookcase or
            ItemID.LihzahrdBrick or
            ItemID.LihzahrdBrickWall or
            ItemID.LihzahrdCandelabra or
            ItemID.LihzahrdCandle or
            ItemID.LihzahrdChair or
            ItemID.LihzahrdChandelier or
            ItemID.LihzahrdChest or
            ItemID.LihzahrdClock or
            ItemID.Fake_LihzahrdChest or
            ItemID.LihzahrdPowerCell or
            ItemID.LihzahrdDoor or
            ItemID.LihzahrdDresser or
            ItemID.LihzahrdFurnace or
            ItemID.LihzahrdGuardianStatue or
            ItemID.LihzahrdLamp or
            ItemID.LihzahrdLantern or
            ItemID.LihzahrdPiano or
            ItemID.LihzahrdPlatform or
            ItemID.LihzahrdPressurePlate or
            ItemID.LihzahrdSink or
            ItemID.LihzahrdSofa or
            ItemID.LihzahrdStatue or
            ItemID.LihzahrdTable or
            ItemID.LihzahrdWallUnsafe or
            ItemID.LihzahrdWatcherStatue or
            ItemID.LihzahrdWorkBench or
            ItemID.ToiletLihzhard
            )
        {
            item.rare = ItemRarityID.Lime;
            return;
        }

        if (item.type is
            ItemID.Meteorite or
            ItemID.AshWood or
            ItemID.AshWoodWall or
            ItemID.AshWoodWorkbench or
            ItemID.AshWoodBathtub or
            ItemID.AshWoodBed or
            ItemID.AshWoodBookcase or
            ItemID.AshWoodCandelabra or
            ItemID.AshWoodCandle or
            ItemID.AshWoodChair or
            ItemID.AshWoodChandelier or
            ItemID.AshWoodChest or
            ItemID.AshWoodClock or
            ItemID.AshWoodDoor or
            ItemID.AshWoodDresser or
            ItemID.AshWoodLamp or
            ItemID.AshWoodLantern or
            ItemID.AshWoodPiano or
            ItemID.AshWoodPlatform or
            ItemID.AshWoodSink or
            ItemID.AshWoodSofa or
            ItemID.AshWoodTable or
            ItemID.AshWoodToilet or
            ItemID.AshWoodHelmet or
            ItemID.AshWoodBreastplate or
            ItemID.AshWoodGreaves or
            ItemID.AshWoodBow or
            ItemID.AshWoodHammer or
            ItemID.AshWoodSword or
            ItemID.Fake_AshWoodChest or
            ItemID.AshWoodFence or
            ItemID.AshGrassSeeds or
            ItemID.Obsidian or
            ItemID.ObsidianBackEcho or
            ItemID.ObsidianBathtub or
            ItemID.ObsidianBed or
            ItemID.ObsidianBookcase or
            ItemID.ObsidianBrick or
            ItemID.ObsidianBrickWall or
            ItemID.ObsidianCandelabra or
            ItemID.ObsidianCandle or
            ItemID.ObsidianChair or
            ItemID.ObsidianChandelier or
            ItemID.ObsidianChest or
            ItemID.ObsidianClock or
            ItemID.ObsidianDoor or
            ItemID.ObsidianDresser or
            ItemID.ObsidianLamp or
            ItemID.ObsidianLantern or
            ItemID.ObsidianPiano or
            ItemID.ObsidianPlatform or
            ItemID.ObsidianSink or
            ItemID.ObsidianSofa or
            ItemID.ObsidianTable or
            ItemID.ObsidianVase or
            ItemID.ObsidianWorkBench or
            ItemID.ToiletObsidian or
            ItemID.AncientObsidianBrick or
            ItemID.AncientObsidianBrickWall or
            ItemID.Fake_ObsidianChest
        )
        {
            item.rare = ItemRarityID.Blue;
            return;
        }
        
        if (item.type is
            ItemID.CursedFlare or
            ItemID.LivingCursedFireBlock or
            ItemID.CursedTorch or
            ItemID.LivingIchorBlock or
            ItemID.IchorTorch or
            ItemID.CandyCaneHook or
            ItemID.BundleofBalloons or
            ItemID.HorseshoeBundle)
        {
            item.rare = ItemRarityID.Orange;
            return;
        }

        if (item.type is
            ItemID.Sundial or
            ItemID.Moondial)
        {
            item.rare = ItemRarityID.Pink;
            return;
        }

        if (item.type is
            ItemID.ShimmerMonolith or
            ItemID.ShimmerArrow or
            ItemID.ShimmerFlare)
            //TODO or shimmer gun light purple 1.4.5 tmod
            //TODO or infused fertilizer light purple 1.4.5 tmod
        {
            item.rare = ItemRarityID.LightPurple;
            return;
        }

        if (item.type is
            ItemID.RainbowWings or //empress wings
            ItemID.HallowBossDye or
            ItemID.SparkleGuitar or
            ItemID.RainbowCursor or
            ItemID.EmpressBlade or
            ItemID.FairyQueenMask)
        {
            item.rare = ItemRarityID.Yellow;
            return;
        }

        if (item.type is
            ItemID.VoidMonolith or
            ItemID.VortexMonolith or
            ItemID.SolarMonolith or
            ItemID.NebulaMonolith or
            ItemID.StardustMonolith or
            ItemID.TeleportationPylonVictory)
        {
            item.rare = ItemRarityID.Red;
            return;
        }
    }
}