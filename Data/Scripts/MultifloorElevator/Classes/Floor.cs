using Sandbox.Game;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Floor
    {
        private Elevator Elevator { get; set; }
        public IMyAdvancedDoor Block { get; set; }
        private MultifloorElevator ElevatorGameLogic { get; set; }
        public float Distance { get; set; }
        public int ThisFloor { get; set; }
        public bool TimedDoorClose { get; set; }
        public bool IsOpened => Block.OpenRatio == 2.2f;
        public bool IsClosed => Block.OpenRatio == 0;
        private int OpenedTimer { get; set; }
        public bool IsCurrentFloor => ThisFloor == ElevatorGameLogic.CurrentFloor;
        public bool IsTopFloor => Block.BlockDefinition.SubtypeId == "VCZ_Elevator_Top";
        public bool IsBottomFloor => Block.BlockDefinition.SubtypeId == "VCZ_Elevator_Bottom";
        private int FloorOffset => ElevatorGameLogic.FloorOffset;
        private bool ShouldLock => Elevator.Block.Enabled || !ElevatorGameLogic.ElevatorEnabled || !IsCurrentFloor;

        public Floor(IMyAdvancedDoor floor, float dist, Elevator elevator, int floorNum)
        {
            if (floor != null && elevator != null)
            {
                Block = floor;
                Distance = dist;
                ThisFloor = floorNum;
                Elevator = elevator;
                OpenedTimer = 300;
                ElevatorGameLogic = Elevator.Block.GameLogic.GetAs<MultifloorElevator>();
                Block.GameLogic.GetAs<MultifloorElevatorFloors>().Floor = this;
                if (!MyAPIGateway.Utilities.IsDedicated)
                {
                    UpdateCurrentFloor(ElevatorGameLogic.CurrentFloor);
                    UpdateTargetFloor(ElevatorGameLogic.TargetFloor);
                    UpdateFloorOffset(ElevatorGameLogic.FloorOffset);
                }
                Block.ShowInToolbarConfig = false;
            }
        }

        public void UpdateElevatorEnabled(bool elevatorEnabled) => Block.SetEmissiveParts("CallBtn", elevatorEnabled ? Color.Black : Color.Red, elevatorEnabled ? 0 : 1);

        public void UpdateCurrentFloor(int newCurrentFloor)
        {
            SetDigit.UpdateFloorDisplay(Block, "CurrentFloorDisplay", newCurrentFloor, Color.Orange, FloorOffset);
            if (newCurrentFloor == ElevatorGameLogic.TargetFloor) SetTargetBlack();
        }

        public void UpdateTargetFloor(int newTargetFloor)
        {
            if (newTargetFloor == ThisFloor) Block.SetEmissiveParts("CallBtn", Color.Orange, 1f);
            if (newTargetFloor == ElevatorGameLogic.CurrentFloor) SetTargetBlack();
            if (newTargetFloor > ElevatorGameLogic.CurrentFloor) SetTargetAndArrow("TargetFloorUpDisplay", "TargetFloorDownDisplay", "MovingUpArrow", "MovingDownArrow");
            if (newTargetFloor < ElevatorGameLogic.CurrentFloor) SetTargetAndArrow("TargetFloorDownDisplay", "TargetFloorUpDisplay", "MovingDownArrow", "MovingUpArrow");
        }

        public void UpdateFloorOffset(int newOffset)
        {
            SetDigit.UpdateFloorDisplay(Block, "CurrentFloorDisplay", ElevatorGameLogic.CurrentFloor, Color.Orange, newOffset);
            UpdateTargetFloor(ElevatorGameLogic.TargetFloor);
            SetDigit.UpdateFloorDisplay(Block, "ThisFloorDisplay", ThisFloor, Color.Orange, newOffset);
        }

        private void SetTargetAndArrow(string targetColor, string targetBlack, string arrowColor, string arrowBlack)
        {
            SetDigit.UpdateFloorDisplay(Block, targetColor, ElevatorGameLogic.TargetFloor, Color.Orange, FloorOffset);
            SetDigit.UpdateFloorDisplay(Block, targetBlack, -1, Color.Black, FloorOffset);
            Block.SetEmissiveParts(arrowColor, Color.Orange, 1f);
            Block.SetEmissiveParts(arrowBlack, Color.Black, 0f);
        }

        private void SetTargetBlack()
        {
            Block.SetEmissiveParts("CallBtn", Color.Black, 0f);
            Block.SetEmissiveParts("MovingUpArrow", Color.Black, 0f);
            Block.SetEmissiveParts("MovingDownArrow", Color.Black, 0f);
            SetDigit.UpdateFloorDisplay(Block, "TargetFloorUpDisplay", -1, Color.Black, FloorOffset);
            SetDigit.UpdateFloorDisplay(Block, "TargetFloorDownDisplay", -1, Color.Black, FloorOffset);
        }

        public void ManageDoorOpenClose(bool timedDoorClose)
        {
            if (timedDoorClose)
            {
                if (!IsClosed && OpenedTimer <= 0)
                {
                    CloseDoor();
                    OpenedTimer = 300;
                }
                if (IsOpened && OpenedTimer > 0) OpenedTimer--;
            }
            else
            {
                if (!IsClosed && ShouldLock) CloseDoor();
            }
            Block.Enabled = !(Block.Enabled && IsClosed && ShouldLock);
        }

        public void CloseDoor()
        {
            if (ElevatorSessionComp.IsDecisionMaker)
            {
                Block.Enabled = true;
                Block.CloseDoor();
            }
        }

        public void OpenDoor()
        {
            if (ElevatorSessionComp.IsDecisionMaker)
            {
                Block.Enabled = true;
                Block.OpenDoor();
            }
        }

        public void HandlePlayerInput(MyEntityComponentContainer characterCompContainer)
        {
            if (!IsClosed || !ElevatorGameLogic.ElevatorEnabled || MyAPIGateway.Gui.ActiveGamePlayScreen != null) return;
            if (characterCompContainer.Get<MyCharacterDetectorComponent>()?.UseObject == null || characterCompContainer.Get<MyCharacterDetectorComponent>().DetectedEntity != Block) return;
            bool UseDummy = MyAPIGateway.Input.IsGameControlReleased(MyControlsSpace.USE) || MyAPIGateway.Input.IsGameControlReleased(MyControlsSpace.PRIMARY_TOOL_ACTION);
            if (UseDummy) ElevatorGameLogic.SendToFloor(ThisFloor);
        }
    }
}