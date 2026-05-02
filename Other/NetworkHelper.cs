using System;
using System.IO;
using Relogiced.Content.RangedOverhaul.Borealis;
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
    private const ushort ID_BorealisCooldown = 1;

    private static ModPacket NewPacket(ushort ID)
    {
        ModPacket packet = Relogiced.Instance.GetPacket();
        packet.Write(ID);
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
        //sync borealis
        ModPacket myPacket = NewPacket(ID_BorealisCooldown);
        myPacket.Write(BorealisCooldownSystem.BorealisCanBeUsed);
        myPacket.Send(toClient: senderWhoAmI);
    }

    public static void Borealis_PutOnCooldown()
    {
        Send_BorealisCooldown(false);
    }
    
    public static void Borealis_CoolOff()
    {
        Send_BorealisCooldown(true);
    }

    private static void Send_BorealisCooldown(bool nowUsable, int ignoreClient = -1)
    {
        BorealisCooldownSystem.SetState(nowUsable);
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        ModPacket myPacket = NewPacket(ID_BorealisCooldown);
        myPacket.Write(nowUsable);
        myPacket.Send(ignoreClient: ignoreClient);
    }
    
    private static void Handle_BorealisCooldown(BinaryReader reader, int senderWhoAmI)
    {
        bool nowUsable = reader.ReadBoolean();
        BorealisCooldownSystem.SetState(nowUsable);
        if (Main.netMode == NetmodeID.Server)
            Send_BorealisCooldown(nowUsable, senderWhoAmI);
    }

    internal static void HandlePacket(BinaryReader reader, int senderWhoAmI)
    {
        ushort msgID = reader.ReadUInt16();
        switch (msgID)
        {
            case ID_SyncNewPlayer:
                Handle_SyncNewPlayer(reader, senderWhoAmI);
                break;
            case ID_BorealisCooldown:
                Handle_BorealisCooldown(reader, senderWhoAmI);
                break;
        }
    }
}