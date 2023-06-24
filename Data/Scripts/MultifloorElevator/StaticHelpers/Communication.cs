using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Vicizlat.MultifloorElevator
{
    public static class Communication
    {
        private static ushort NETWORK_ID => 9190;   //MaxValue = 65535

        public static void RegisterHandlers()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(NETWORK_ID, MessageHandler);
            Logging.Instance.WriteLine("Register Message Handler");
        }

        public static void UnregisterHandlers()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(NETWORK_ID, MessageHandler);
            Logging.Instance.WriteLine("Unregister Message Handler");
        }

        public static void RequestFloor(int selectedFloor, long elevatorId)
        {
            byte[] data = new byte[sizeof(long) + 2];
            data[0] = 0;
            data[1] = (byte)selectedFloor;
            BitConverter.GetBytes(elevatorId).CopyTo(data, 2);
            MyAPIGateway.Utilities.InvokeOnGameThread(SendToServer(data));
        }

        private static void MessageHandler(byte[] data)
        {
            try
            {
                IMyEntity Elevator;
                if (data[0] == 0)
                {
                    if (!MyAPIGateway.Entities.TryGetEntityById(BitConverter.ToInt64(data, 2), out Elevator)) return;
                    if (Elevator.GameLogic.GetAs<MultifloorElevator>() == null) return;
                    switch (data[1])
                    {
                        case 0: Elevator.GameLogic.GetAs<MultifloorElevator>().StopElevator(); break;
                        case 10: Elevator.GameLogic.GetAs<MultifloorElevator>().StopElevator(true); break;
                        default: Elevator.GameLogic.GetAs<MultifloorElevator>().SetTargetFloor(data[1]); break;
                    }
                }
                if (data[0] == 1)
                {
                    if (!MyAPIGateway.Entities.TryGetEntityById(BitConverter.ToInt64(data, 13), out Elevator)) return;
                    if (Elevator.GameLogic.GetAs<MultifloorElevator>() == null) return;
                    Elevator.GameLogic.GetAs<MultifloorElevator>().SetNewPassengerOffset(data.Take(13).ToArray());
                }
                if (MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated) SendToClients(data, MyAPIGateway.Multiplayer.ServerId);
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine($"Error during message handle: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void SendToClients(byte[] data, ulong sender)
        {
            List<IMyPlayer> PlayersList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(PlayersList);
            foreach (IMyPlayer player in PlayersList)
            {
                if (player == null || player.Character == null || player.Character.IsDead) continue;
                bool shouldGetMessage = player.SteamUserId != MyAPIGateway.Multiplayer.MyId && player.SteamUserId != sender;
                if (shouldGetMessage) MyAPIGateway.Utilities.InvokeOnGameThread(SendToClient(data, player.SteamUserId));
            }
            PlayersList.Clear();
        }

        public static Action SendToServer(byte[] data) => () => MyAPIGateway.Multiplayer.SendMessageToServer(NETWORK_ID, data);

        private static Action SendToClient(byte[] data, ulong playerId) => () => MyAPIGateway.Multiplayer.SendMessageTo(NETWORK_ID, data, playerId);
    }
}