using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Content.Items.ShopChanges;

public class ShopChangesNPC : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return Relogiced.ConfigAssorted.ShopChanges;
    }

    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.ArmsDealer && shop.Name == "Shop")
        {
            shop.Add(ItemID.Handgun, Condition.DownedSkeletron, Condition.BloodMoonOrHardmode);
            shop.Add(ItemID.ClockworkAssaultRifle, Condition.Hardmode, Condition.EclipseOrBloodMoon);
            shop.Add(ItemID.ClockworkAssaultRifle, Condition.NpcIsPresent(NPCID.Steampunker));
            shop.InsertAfter(ItemID.TungstenBullet, ItemID.MoonlordBullet, Condition.DownedMoonLord);
            shop.InsertAfter(ItemID.TungstenBullet, ItemID.ChlorophyteBullet, Condition.DownedPlantera);
            shop.InsertAfter(ItemID.TungstenBullet, ItemID.CrystalBullet, Condition.DownedQueenSlime);
            shop.InsertAfter(ItemID.TungstenBullet, ItemID.CursedBullet, Condition.CorruptWorld, Condition.Hardmode, Condition.EclipseOrBloodMoon);
            shop.InsertAfter(ItemID.TungstenBullet, ItemID.IchorBullet, Condition.CrimsonWorld, Condition.Hardmode, Condition.EclipseOrBloodMoon);
            shop.InsertAfter(ItemID.UnholyArrow, ItemID.MoonlordArrow, Condition.DownedMoonLord);
            return;
        }
        
        if (shop.NpcType == NPCID.Clothier && shop.Name == "Shop")
        {
            if (shop.TryGetEntry(ItemID.FamiliarWig, out NPCShop.Entry entry))
                entry.Disable();
            shop.Add(ItemID.RedHat, Condition.HappyEnough);
            shop.Add(ItemID.GoldenKey, Condition.DownedSkeletron, Condition.BloodMoonOrHardmode);
            shop.Add(ItemID.Nazar, Condition.DownedSkeletron, Condition.BloodMoonOrHardmode, Condition.NotDownedSkeletronPrime);
            shop.Add(ItemID.CountercurseMantra, Condition.DownedSkeletronPrime);
            return;
        }

        if (shop.NpcType == NPCID.Cyborg && shop.Name == "Shop")
        {
            //DisableEntry(shop, ItemID.RocketI);
            //DisableEntry(shop, ItemID.RocketII);
            DisableEntry(shop, ItemID.RocketIII);
            DisableEntry(shop, ItemID.RocketIV);
            DisableEntry(shop, ItemID.DryRocket);
            //DisableEntry(shop, ItemID.ProximityMineLauncher);
            DisableEntry(shop, ItemID.ClusterRocketI);
            DisableEntry(shop, ItemID.ClusterRocketII);
            return;
        }

        if (shop.NpcType == NPCID.Demolitionist && shop.Name == "Shop")
        {
            shop.Add(ItemID.AleThrowingGlove, Condition.NpcIsPresent(NPCID.DD2Bartender), Condition.PlayerCarriesItem(ItemID.Ale));
            shop.InsertAfter(ItemID.Grenade, ItemID.PartyGirlGrenade, Condition.BirthdayParty);
            shop.Add(ItemID.ScarabBomb, Condition.InDesert);
            shop.Add(ItemID.ScarabBomb, Condition.InUndergroundDesert);
            return;
        }

        if (shop.NpcType == NPCID.DyeTrader && shop.Name == "Shop")
        {
            shop.Add(ItemID.DyeTradersScimitar, Condition.HappyEnough);
            return;
        }

        if (shop.NpcType == NPCID.GoblinTinkerer && shop.Name == "Shop")
        {
            shop.Add(ItemID.WeatherRadio, Condition.NpcIsPresent(NPCID.Angler), Condition.AnglerQuestsFinishedOver(8), Condition.InRain);
            shop.Add(ItemID.Sextant, Condition.NpcIsPresent(NPCID.Angler), Condition.AnglerQuestsFinishedOver(10), Condition.MoonPhaseFull);
            shop.Add(ItemID.DepthMeter, Condition.InBelowSurface, Condition.MoonPhasesQuarter0, Condition.NotDownedSkeletron);
            shop.Add(ItemID.TallyCounter, Condition.InBelowSurface, Condition.MoonPhasesQuarter0, Condition.DownedSkeletron);
            shop.Add(ItemID.Compass, Condition.InBelowSurface, Condition.MoonPhasesQuarter1, Condition.NotDownedMechBossAny);
            shop.Add(ItemID.GPS, Condition.InBelowSurface, Condition.MoonPhasesQuarter1, Condition.DownedMechBossAny);
            shop.Add(ItemID.Radar, Condition.InBelowSurface, Condition.MoonPhasesQuarter2, Condition.NotDownedMechBossAny);
            shop.Add(ItemID.REK, Condition.InBelowSurface, Condition.MoonPhasesQuarter2, Condition.DownedMechBossAny);
            shop.Add(ItemID.DPSMeter, Condition.InBelowSurface, Condition.MoonPhasesQuarter3, Condition.PreHardmode);
            shop.Add(ItemID.GoblinTech, Condition.InBelowSurface, Condition.MoonPhasesQuarter3, Condition.Hardmode);
            shop.Add(ItemID.LifeformAnalyzer, Condition.InBelowSurface, Condition.BestiaryFilledPercent(30));
            return;
        }

        /*if (shop.NpcType == NPCID.Nurse && shop.Name == "Shop")
        {
            shop.Add(ItemID.AdhesiveBandage, Condition.BloodMoonOrHardmode);
            shop.Add(ItemID.Vitamins, Condition.Hardmode);
            return;
        }*/

        if (shop.NpcType == NPCID.Painter && shop.Name == "Shop")
        {
            shop.InsertAfter(ItemID.PaintScraper, ItemID.PainterPaintballGun, Condition.HappyEnough);
            return;
        }

        if (shop.NpcType == NPCID.PartyGirl && shop.Name == "Shop")
        {
            shop.InsertAfter(ItemID.PartyGirlGrenade, ItemID.PartyGirlGrenade, Condition.HappyEnough);
            return;
        }

        if (shop.NpcType == NPCID.Pirate && shop.Name == "Shop")
        {
            shop.Add(ItemID.FishermansGuide, Condition.NpcIsPresent(NPCID.Angler), Condition.AnglerQuestsFinishedOver(5));
            shop.Add(ItemID.ClockworkAssaultRifle, Condition.Hardmode, Condition.MoonPhases37);
            shop.Add(ItemID.Cutlass);
            shop.Add(ItemID.PirateStaff, Condition.HappyEnough, Condition.DownedPirates, Condition.MoonPhasesOdd);
            shop.Add(ItemID.CoinGun, Condition.HappyEnough, Condition.DownedPirates, Condition.MoonPhasesEven);
            shop.Add(ItemID.DiscountCard, Condition.HappyEnough, Condition.DownedPirates, Condition.MoonPhasesQuarter0);
            shop.Add(ItemID.LuckyCoin, Condition.HappyEnough, Condition.DownedPirates, Condition.MoonPhasesQuarter1);
            shop.Add(ItemID.GoldRing, Condition.HappyEnough, Condition.DownedPirates, Condition.MoonPhasesQuarter2);
            return;
        }

        if (shop.NpcType == NPCID.Princess && shop.Name == "Shop")
        {
            shop.InsertAfter(ItemID.RoyalScepter, ItemID.PrincessWeapon, Condition.HappyEnough);
            shop.InsertAfter(ItemID.SlimeStaff, ItemID.SlimeStaff,
                Condition.NotRemixWorld, Condition.NotZenithWorld,
                Condition.DownedKingSlime, Condition.BirthdayParty);
            shop.InsertAfter(ItemID.HeartLantern, ItemID.HeartLantern,
                Condition.NotRemixWorld, Condition.NotZenithWorld,
                Condition.DownedEowOrBoc, Condition.BirthdayParty);
            shop.InsertAfter(ItemID.FlaskofParty, ItemID.FlaskofParty,
                Condition.NotRemixWorld, Condition.NotZenithWorld,
                Condition.NpcIsPresent(NPCID.PartyGirl), Condition.BirthdayParty);
            shop.InsertAfter(ItemID.SandstorminaBottle, ItemID.SandstorminaBottle,
                Condition.NotRemixWorld, Condition.NotZenithWorld,
                Condition.InDesert, Condition.BirthdayParty);
            shop.InsertAfter(ItemID.Terragrim, ItemID.Terragrim,
                Condition.NotRemixWorld, Condition.NotZenithWorld,
                Condition.BloodMoonOrHardmode, Condition.BirthdayParty);
            return;
        }

        if (shop.NpcType == NPCID.Mechanic && shop.Name == "Shop")
        {
            shop.Add(ItemID.CombatWrench, Condition.HappyEnough);
            return;
        }

        if (shop.NpcType == NPCID.Merchant && shop.Name == "Shop")
        {
            shop.InsertAfter(ItemID.ManaPotion, ItemID.ArcheryPotion, Condition.MoonPhasesEven);
            shop.InsertAfter(ItemID.ManaPotion, ItemID.BuilderPotion, Condition.MoonPhasesOdd);
            shop.InsertAfter(ItemID.ManaPotion, ItemID.PotionOfReturn, Condition.LanternNight, Condition.DownedEowOrBoc);
            shop.InsertAfter(ItemID.ManaPotion, ItemID.RecallPotion, Condition.HappyWindyDay);
            return;
        }

        if (shop.NpcType == NPCID.SkeletonMerchant && shop.Name == "Shop")
        {
            shop.Add(ItemID.Vitamins, Condition.Hardmode, Condition.MoonPhases37);
            shop.Add(ItemID.PocketMirror, Condition.Hardmode, Condition.InMarble);
            shop.Add(ItemID.HandWarmer, Condition.Hardmode, Condition.InSnow);
            shop.Add(ItemID.LavaCharm, Condition.BloodMoonOrHardmode, Condition.InUnderworldHeight);
            shop.Add(ItemID.LuckyHorseshoe, Condition.LanternNight);
            return;
        }

        if (shop.NpcType == NPCID.Stylist && shop.Name == "Shop")
        {
            shop.InsertBefore(ItemID.WilsonBeardShort, ItemID.FamiliarWig);
            shop.InsertAfter(ItemID.WilsonBeardShort, ItemID.FruitJuice, Condition.HappyEnough);
            shop.InsertAfter(ItemID.WilsonBeardShort, ItemID.CoffeeCup, Condition.HappyEnough);
            shop.InsertAfter(ItemID.WilsonBeardShort, ItemID.Teacup, Condition.HappyEnough);
            shop.Add(ItemID.StylistKilLaKillScissorsIWish, Condition.HappyEnough);
            shop.Add(ItemID.ArmorPolish, Condition.Hardmode, Condition.EclipseOrBloodMoon);
            shop.Add(ItemID.PocketMirror, Condition.Hardmode, Condition.InMarble);
            return;
        }

        if (shop.NpcType == NPCID.Truffle && shop.Name == "Shop")
        {
            shop.Add(ItemID.RocketLauncher, Condition.DownedPlantera, Condition.BloodMoon);
            shop.Add(ItemID.RocketI, Condition.DownedPlantera);
            shop.Add(ItemID.RocketII, Condition.DownedPlantera, Condition.BloodMoon);
            shop.Add(ItemID.RocketIII, Condition.DownedPlantera, Condition.NightOrEclipse);
            shop.Add(ItemID.RocketIV, Condition.DownedPlantera, Condition.Eclipse);
            shop.Add(ItemID.DryRocket, Condition.DownedMechBossAll);
            shop.Add(ItemID.ClusterRocketI, Condition.DownedPlantera, Condition.DownedMartians);
            shop.Add(ItemID.ClusterRocketII, Condition.DownedPlantera, Condition.DownedMartians, Condition.EclipseOrBloodMoon);
            return;
        }

        if (shop.NpcType == NPCID.WitchDoctor && shop.Name == "Shop")
        {
            shop.Add(ItemID.Bezoar, Condition.InJungle);
            return;
        }

        if (shop.NpcType == NPCID.Wizard && shop.Name == "Shop")
        {
            shop.Add(ItemID.PixieDust, Condition.DownedQueenSlime, Condition.TimeDay, Condition.InHallow);
            shop.Add(ItemID.CrystalShard, Condition.DownedQueenSlime, Condition.TimeNight);
            shop.Add(ItemID.ReleaseLantern, Condition.LanternNight);
            shop.Add(ItemID.SoulofLight, Condition.InHallow, Condition.TimeDay, Condition.InBelowSurface);
            shop.Add(ItemID.SoulofNight, Condition.NotInHallow, Condition.NightOrEclipse, Condition.InBelowSurface);
            shop.Add(ItemID.SoulofFlight, Condition.InSpace, Condition.DownedQueenSlime);
            shop.Add(ItemID.TrifoldMap, Condition.DownedClown);
            //shop.Add(ItemID.SoulofFright, Condition.EclipseOrBloodMoon, Condition.DownedMechBossAll);
            //shop.Add(ItemID.SoulofMight, Condition.TimeNight, Condition.NotEclipseAndNotBloodMoon, Condition.DownedMechBossAll);
            //shop.Add(ItemID.SoulofSight, Condition.TimeDay, Condition.NotEclipseAndNotBloodMoon, Condition.DownedMechBossAll);
            return;
        }
    }

    private static void DisableEntry(NPCShop shop, int item)
    {
        if (shop.TryGetEntry(item, out NPCShop.Entry e)) e.Disable();
    }
}