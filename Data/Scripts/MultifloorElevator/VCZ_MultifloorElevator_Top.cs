using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedRotor), false, "VCZ_Elevator_Rotor_Top")]
    public class MultifloorElevatorTop : MyGameLogicComponent
    {
        private IMyMotorAdvancedRotor ElevatorTop;
        public Cabin Cabin { get; set; }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            ElevatorTop = (IMyMotorAdvancedRotor)Entity;
        }

        public override void Close() => NeedsUpdate = MyEntityUpdateEnum.NONE;

        public override void UpdateOnceBeforeFrame() //I think this is the start function?
        {
            try
            {
                if (ElevatorSessionComp.IsDecisionMaker)
                {
                    Vector3I buttonsPosition = ElevatorTop.Position + (Vector3I)ElevatorTop.LocalMatrix.Up;
                    if (!ElevatorTop.CubeGrid.CanAddCube(buttonsPosition)) ElevatorTop.CubeGrid.RazeBlock(buttonsPosition);
                    Vector3D position = ElevatorTop.WorldMatrix.Translation + (ElevatorTop.WorldMatrix.Up * 2.5f);
                    var gridBuilder = Utilities.GridBuilder(ElevatorTop, new MyObjectBuilder_MedicalRoom(), "VCZ_Elevator_Buttons", position, ElevatorTop.WorldMatrix.Forward, ElevatorTop.WorldMatrix.Up);
                    (ElevatorTop.CubeGrid as MyCubeGrid).PasteBlocksToGrid(new List<MyObjectBuilder_CubeGrid> { gridBuilder }, 0, false, true);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }
        
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!ElevatorTop.IsFunctional)
                {
                    ElevatorTop.SlimBlock.IncreaseMountLevel(1f, ElevatorTop.Base.OwnerId);
                }

                if (!MyAPIGateway.Utilities.IsDedicated && ElevatorTop.CubeGrid.Physics != null && MyAPIGateway.Session.Player.Character != null && !MyAPIGateway.Session.Player.Character.IsDead)
                {
                    if (Cabin != null && Cabin.Block != null && !Cabin.Block.Closed)
                    {
                        if (ElevatorTop.IsAttached)
                        {
                            Cabin.UpdateMusic(MyAPIGateway.Session.Player.Character.WorldMatrix.Translation);
                            Cabin.UpdateChristmasLights();
                        }
                        else
                        {
                            //TODO Rotate elevator cabin to match instead of removing it
                            ElevatorTop.CubeGrid.RazeBlock(ElevatorTop.Position + (Vector3I)ElevatorTop.LocalMatrix.Up);
                            ElevatorTop.CubeGrid.RazeBlock(ElevatorTop.Position);
                        }
                    }
                    else
                    {
                        Cabin = new Cabin(ElevatorTop);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }
    }
}