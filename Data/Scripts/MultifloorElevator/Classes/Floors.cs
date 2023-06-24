using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Floors
    {
        private List<Floor> FloorsList = new List<Floor>();
        public Floor Floor(int indexMinusOne) => FloorsList[indexMinusOne - 1];
        public int Count => FloorsList.Count;
        public bool HasEnoughFloors => FloorsList.Count >= 2;
        public bool LastFloorIsTop { get; set; }
        public bool FirstFloorIsBottom { get; set; }
        public int ElevatorHeight { get; set; }
        public bool ElevatorReady => HasEnoughFloors && FirstFloorIsBottom && LastFloorIsTop;

        public void FindFloors(Elevator elevator)
        {
            try
            {
                if (elevator == null) return;
                FloorsList.Clear();
                Vector3I elevatorUp = (Vector3I)elevator.Block.LocalMatrix.Up;
                IMySlimBlock nextBlock = elevator.Block.CubeGrid.GetCubeBlock(elevator.Block.Position + (elevatorUp * -1));
                if (elevator.IsMidBlock)
                {
                    if (nextBlock != null && nextBlock.BlockDefinition.Id.SubtypeName.Contains("Elevator_Bottom"))
                    {
                        AssignFloor(elevator, -1, nextBlock.FatBlock as IMyAdvancedDoor);
                        FirstFloorIsBottom = true;
                    }
                    else FirstFloorIsBottom = false;
                }
                else FirstFloorIsBottom = true;
                for (int diff = 1; diff <= elevator.MaxHeight; diff++)
                {
                    nextBlock = elevator.Block.CubeGrid.GetCubeBlock(elevator.Block.Position + (elevatorUp * diff));
                    if (nextBlock == null || nextBlock.FatBlock == null) break;
                    ElevatorHeight = elevator.IsMidBlock ? diff + 2 : diff;
                    if (!(nextBlock.FatBlock is IMyAdvancedDoor)) continue;
                    if (nextBlock.BlockDefinition.Id.SubtypeName.Contains("Elevator_Middle"))
                    {
                        if (FloorsList.Count == 0 || nextBlock.FatBlock != Floor(FloorsList.Count).Block) AssignFloor(elevator, diff, nextBlock.FatBlock as IMyAdvancedDoor);
                        continue;
                    }
                    if (nextBlock.BlockDefinition.Id.SubtypeName.Contains("Elevator_Top")) AssignFloor(elevator, diff, nextBlock.FatBlock as IMyAdvancedDoor);
                    LastFloorIsTop = FloorsList.Count > 0 && Floor(FloorsList.Count).Block != null && Floor(FloorsList.Count).IsTopFloor;
                    if (FloorsList.Count == 9 || LastFloorIsTop) break;
                }
                if (elevator.CanUpdateCabin) elevator.Cabin.UpdateFloorsCount(FloorsList.Count);
                elevator.Block.RefreshCustomInfo();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine($"[ Error: {e.Message}\n{e.StackTrace} ]");
            }
        }

        private void AssignFloor(Elevator elevator, int diff, IMyAdvancedDoor block)
        {
            float distance = (diff * 2.5f) - 0.1f;
            if (elevator.IsMidBlock && FloorsList.Count == 0) distance = diff * 2.5f;
            if (block.BlockDefinition.SubtypeId == "VCZ_Elevator_Middle_Double") distance += 1.25f;
            FloorsList.Add(new Floor(block, distance, elevator, FloorsList.Count + 1));
        }

        public int GetCurrentFloor(float elevatorDisplacement, int currentFloor)
        {
            if (FloorsList.Exists(f => Math.Abs(f.Distance - elevatorDisplacement) <= 0.5f))
            {
                return FloorsList.Find(f => Math.Abs(f.Distance - elevatorDisplacement) <= 0.5f).ThisFloor;
            }
            else return currentFloor;
        }

        public void UpdateCurrentFloor(int newCurrentFloor) => FloorsList.ForEach(f => f.UpdateCurrentFloor(newCurrentFloor));

        public void UpdateTargetFloor(int newTargetFloor) => FloorsList.ForEach(f => f.UpdateTargetFloor(newTargetFloor));

        public void UpdateFloorOffset(int newOffset) => FloorsList.ForEach(f => f.UpdateFloorOffset(newOffset));

        public void UpdateElevatorEnabled(bool elevatorEnabled) => FloorsList.ForEach(f => f.UpdateElevatorEnabled(elevatorEnabled));
    }
}