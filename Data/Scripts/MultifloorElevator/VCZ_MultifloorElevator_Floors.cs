using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Vicizlat.MultifloorElevator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_AdvancedDoor), false, new string[] { "VCZ_Elevator_Bottom", "VCZ_Elevator_Middle", "VCZ_Elevator_Middle_Double", "VCZ_Elevator_Top" })]
    public class MultifloorElevatorFloors : MyGameLogicComponent
    {
        private IMyAdvancedDoor ElevatorFloor;
        public Floor Floor { get; set; }
        public bool TimedDoorClose { get; set; }
        private bool PlayerPresentAndAlive => MyAPIGateway.Session.Player.Character != null && !MyAPIGateway.Session.Player.Character.IsDead;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            ElevatorFloor = (IMyAdvancedDoor)Entity;
            TimedDoorClose = ElevatorFloor.CustomData.Contains("TimedDoorClose");
            if (!CustomControls.FloorControlsInited) CustomControls.CreateTerminalControls<IMyAdvancedDoor>(b => b.GameLogic.GetAs<MultifloorElevatorFloors>() != null);
        }

        public override void Close() => NeedsUpdate = MyEntityUpdateEnum.NONE;

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (ElevatorFloor.CubeGrid.Physics != null && ElevatorFloor.IsFunctional && Floor != null && Floor.Block == ElevatorFloor)
                {
                    if (ElevatorSessionComp.IsDecisionMaker) Floor.ManageDoorOpenClose(TimedDoorClose);
                    if (!MyAPIGateway.Utilities.IsDedicated && PlayerPresentAndAlive) Floor.HandlePlayerInput(MyAPIGateway.Session.Player.Character.Components);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }
    }
}