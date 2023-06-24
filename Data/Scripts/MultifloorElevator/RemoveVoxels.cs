using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedStator), false, new string[] { "VCZ_Elevator_Rotor", "VCZ_Elevator_Rotor_Base" })]
    public class ElevatorRotor : RemoveVoxels { }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_AdvancedDoor), false, new string[] { "VCZ_Elevator_Bottom", "VCZ_Elevator_Middle", "VCZ_Elevator_Middle_Double", "VCZ_Elevator_Top" })]
    public class ElevatorFloors : RemoveVoxels { }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, new string[] { "VCZ_Elevator_Filler", "VCZ_Elevator_FillerDouble" })]
    public class ElevatorFillers : RemoveVoxels { }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_AirVent), false, "VCZ_Elevator_Filler_Vent")]
    public class ElevatorFillerVent : RemoveVoxels { }

    public class RemoveVoxels : MyGameLogicComponent
    {
        private IMyTerminalBlock Block;
        private List<MyVoxelBase> VoxelsList = new List<MyVoxelBase>();
        private int Counter = 0;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME;
            Block = (IMyTerminalBlock)Entity;
        }

        public override void Close() => NeedsUpdate = MyEntityUpdateEnum.NONE;

        public override void UpdateBeforeSimulation10()
        {
            try
            {
                if (Block is IMyUpgradeModule && (Block.ShowInTerminal || Block.ShowInToolbarConfig))
                {
                    Block.ShowInTerminal = false;
                    Block.ShowInToolbarConfig = false;
                }
                if (!ElevatorSessionComp.IsDecisionMaker) return;
                if (Block.CubeGrid.IsStatic && Counter++ <= 20)
                {
                    if (PerformVoxelCut()) NeedsUpdate = MyEntityUpdateEnum.NONE;
                }
                else NeedsUpdate = MyEntityUpdateEnum.NONE;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private bool PerformVoxelCut()
        {
            VoxelsList.Clear();
            BoundingBoxD blockBB = new BoundingBoxD(Block.LocalAABB.Min * 1.3f, Block.LocalAABB.Max * 1.3f);
            MyGamePruningStructure.GetAllVoxelMapsInBox(ref blockBB, VoxelsList);
            if (VoxelsList.Count == 0 || GetItem2() <= 0.05f) return true;
            VoxelsList.ForEach(voxel => voxel.RequestVoxelOperationBox(blockBB, Block.WorldMatrix, 0, MyVoxelBase.OperationType.Cut));
            if (GetItem2() < 0.375f) return true;
            return false;
        }

        private float GetItem2()
        {
            if (VoxelsList.Count > 1) return VoxelsList[1].GetVoxelContentInBoundingBox_Fast(Block.Model.BoundingBox, Block.WorldMatrix).Item2;
            else return VoxelsList[0].GetVoxelContentInBoundingBox_Fast(Block.Model.BoundingBox, Block.WorldMatrix).Item2;
        }
    }
}