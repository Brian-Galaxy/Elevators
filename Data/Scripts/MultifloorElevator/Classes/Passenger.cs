using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using VRage.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Passenger
    {
        public IMyEntity Entity { get; set; }
        public Vector3D Оffset { get; set; }

        public Passenger(IMyEntity entity, Vector3D offset)
        {
            if (entity == null) return;
            Entity = entity;
            Оffset = offset;
        }

        public void SetOffset(long elevatorId)
        {
            if (Entity != null && Entity.EntityId == MyAPIGateway.Session.LocalHumanPlayer.Character.EntityId)
            {
                byte[] data = new byte[21];
                data[0] = 1;
                BitConverter.GetBytes(MyAPIGateway.Input.IsGameControlPressed(MyControlsSpace.FORWARD)).CopyTo(data, 1);
                BitConverter.GetBytes(MyAPIGateway.Input.IsGameControlPressed(MyControlsSpace.BACKWARD)).CopyTo(data, 2);
                BitConverter.GetBytes(MyAPIGateway.Input.IsGameControlPressed(MyControlsSpace.STRAFE_LEFT)).CopyTo(data, 3);
                BitConverter.GetBytes(MyAPIGateway.Input.IsGameControlPressed(MyControlsSpace.STRAFE_RIGHT)).CopyTo(data, 4);
                BitConverter.GetBytes(Entity.EntityId).CopyTo(data, 5);
                BitConverter.GetBytes(elevatorId).CopyTo(data, 13);
                MyAPIGateway.Utilities.InvokeOnGameThread(Communication.SendToServer(data));
            }
        }

        public void SetPosition(Vector3D cabinPosition)
        {
            if (Entity == null) return;
            Entity.PositionComp.SetPosition(cabinPosition + Оffset);
            Entity.Physics.ClearSpeed();
        }
    }
}