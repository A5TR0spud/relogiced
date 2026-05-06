using System;
using System.IO;
using Relogiced.Content.RangedOverhaul.RodFromGodItem;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Relogiced.Other;

public class NetworkedPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        NetworkHelper.SyncNewPlayer();
    }
}

public class NetworkHelper : ModSystem
{
    private const ushort ID_SyncNewPlayer = 0;
    private const ushort ID_RFGCooldown = 1;

    private static ModPacket NewPacket(ushort id)
    {
        ModPacket packet = Relogiced.Instance.GetPacket();
        packet.Write(id);
        return packet;
    }

    internal static void SyncNewPlayer()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        ModPacket packet = NewPacket(ID_SyncNewPlayer);
        packet.Send();
    }

    private static void Handle_SyncNewPlayer(BinaryReader reader, int senderWhoAmI)
    {
        if (Main.netMode != NetmodeID.Server) return;
        //sync rods from god
        ModPacket myPacket = NewPacket(ID_RFGCooldown);
        myPacket.Write(RodFromGodCooldownSystem.RodFromGodCanBeUsed);
        myPacket.Send(toClient: senderWhoAmI);
    }

    public static void RodFromGod_PutOnCooldown()
    {
        Send_RFGCooldown(false);
    }
    
    public static void RodFromGod_CoolOff()
    {
        Send_RFGCooldown(true);
    }

    private static void Send_RFGCooldown(bool nowUsable, int ignoreClient = -1)
    {
        if (!RodFromGodItem.IsEnabled) return;
        RodFromGodCooldownSystem.SetState(nowUsable);
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        ModPacket myPacket = NewPacket(ID_RFGCooldown);
        myPacket.Write(nowUsable);
        myPacket.Send(ignoreClient: ignoreClient);
    }
    
    private static void Handle_RFGCooldown(BinaryReader reader, int senderWhoAmI)
    {
        bool nowUsable = reader.ReadBoolean();
        if (!RodFromGodItem.IsEnabled) return;
        RodFromGodCooldownSystem.SetState(nowUsable);
        if (Main.netMode == NetmodeID.Server)
            Send_RFGCooldown(nowUsable, senderWhoAmI);
    }

    internal static void HandlePacket(BinaryReader reader, int senderWhoAmI)
    {
        ushort msgID = reader.ReadUInt16();
        switch (msgID)
        {
            case ID_SyncNewPlayer:
                Handle_SyncNewPlayer(reader, senderWhoAmI);
                break;
            case ID_RFGCooldown:
                Handle_RFGCooldown(reader, senderWhoAmI);
                break;
        }
    }
}