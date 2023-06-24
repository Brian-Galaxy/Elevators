using System;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedStator), false, new string[] { "VCZ_Elevator_Rotor", "VCZ_Elevator_Rotor_Base" })]
    public class MultifloorElevator : MyGameLogicComponent
    {
        private IMyMotorAdvancedStator Elevator_Block;
        private bool ShouldMoveElevator = false;
        private bool ElevatorStarted = true;
        public Elevator Elevator { get; private set; }
        public Passengers Passengers { get; private set; } = new Passengers();
        public Floors Floors { get; private set; } = new Floors();
        public int CurrentFloor { get; set; } = 0;
        public int TargetFloor { get; set; } = 0;
        public float ElevatorSpeed { get; set; } = 10;
        public int FloorOffset { get; set; } = 0;
        public int MusicSelector { get; set; } = 1;
        public int MusicVolume { get; set; } = 10;
        public bool ShouldPlayDingSound { get; set; } = true;
        public int LightSelector { get; set; } = 1;
        public Color CabinLightColor { get; set; } = new Color(255, 255, 255);
        public float CabinLightRange { get; set; } = 3;
        public float CabinLightIntensity { get; set; } = 2;
        public bool ShowChristmasLights { get; set; } = false;
        private MatrixD CabinMatrix => Elevator.CabinMatrix;
        public Vector3D CabinPosition => Elevator.CabinPosition;
        public bool ElevatorEnabled { get; set; }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            Elevator_Block = Entity as IMyMotorAdvancedStator;
            Elevator = new Elevator(Elevator_Block);
            Elevator_Block.AppendingCustomInfo += AppendingCustomInfo;
            Elevator_Block.CubeGrid.OnBlockAdded += OnBlockAddedOrRemoved;
            Elevator_Block.CubeGrid.OnBlockRemoved += OnBlockAddedOrRemoved;
        }

        public override void Close()
        {
            Elevator_Block.AppendingCustomInfo -= AppendingCustomInfo;
            Elevator_Block.CubeGrid.OnBlockAdded -= OnBlockAddedOrRemoved;
            Elevator_Block.CubeGrid.OnBlockRemoved -= OnBlockAddedOrRemoved;
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                LoadTerminalControlSettings();
                if (!CustomControls.ElevatorControlsInited) CustomControls.CreateControlsAndActions<IMyMotorAdvancedStator>(b => b.GameLogic.GetAs<MultifloorElevator>() != null);
                Floors.FindFloors(Elevator);
                NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private void OnBlockAddedOrRemoved(IMySlimBlock block)
        {
            if (IDs.ValidElevatorBlock.Contains(block.BlockDefinition.Id.SubtypeName)) Floors.FindFloors(Elevator);
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!GridIsRotating() && ElevatorEnabled)
                {
                    if (CurrentFloor == 0 || (ShouldMoveElevator && Elevator_Block.Enabled && ElevatorStarted)) GetCurrentFloor(Floors.GetCurrentFloor(Elevator.GetDisplacement, CurrentFloor));
                    if (ShouldMoveElevator && !MyAPIGateway.Utilities.IsDedicated && Passengers.Count > 0) Passengers.UpdateOffset(Elevator.Id);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private bool GridIsRotating()
        {
            if (!Elevator_Block.CubeGrid.IsStatic && Elevator_Block.CubeGrid.Physics.AngularVelocity.AbsMax() >= 1.5f)
            {
                if (!ElevatorStarted && !ShouldMoveElevator && Elevator_Block.Top != null) Passengers.GetPassengers(CabinMatrix, CabinPosition);
                if (Passengers.Count <= 0)
                {
                    Elevator_Block.Detach();
                    return true;
                }
            }
            return false;
        }

        private void GetCurrentFloor(int findNewCurrentFloor)
        {
            if (findNewCurrentFloor != CurrentFloor) SetNewCurrentFloor(findNewCurrentFloor);
            if (CurrentFloor == 0)
            {
                SetNewCurrentFloor(1);
                SetNewTargetFloor(1);
                StartElevator();
            }
            if (TargetFloor == 0) SetNewTargetFloor(CurrentFloor);
        }

        private void SetNewCurrentFloor(int newCurrentFloor)
        {
            CurrentFloor = newCurrentFloor;
            if (Elevator.CanUpdateCabin) Elevator.Cabin.UpdateCurrentFloor(CurrentFloor, TargetFloor);
            if (!MyAPIGateway.Utilities.IsDedicated) Floors.UpdateCurrentFloor(newCurrentFloor);
        }

        private void SetNewTargetFloor(int newTargetFloor)
        {
            TargetFloor = newTargetFloor;
            Elevator.SetTargetDisplacement(Floors.Floor(newTargetFloor).Distance);
            ShouldMoveElevator = true;
            if (Elevator.CanUpdateCabin) Elevator.Cabin.UpdateTargetFloor(CurrentFloor, newTargetFloor);
            if (!MyAPIGateway.Utilities.IsDedicated) Floors.UpdateTargetFloor(newTargetFloor);
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (ElevatorEnabled)
                {
                    if (ShouldMoveElevator && TargetFloor > 0 && Elevator.GetDisplacement != Floors.Floor(TargetFloor).Distance)
                    {
                        if (CurrentFloor <= Floors.Count && Floors.Floor(CurrentFloor).Block != null)
                        {
                            if (Floors.Floor(CurrentFloor).IsClosed && !ElevatorStarted) StartElevator();
                        }
                        else StartElevator();
                    }
                    if (ShouldMoveElevator && ElevatorSessionComp.IsDecisionMaker)
                    {
                        if (ElevatorStarted) Elevator.MoveElevator(ElevatorSpeed);
                        if (Passengers.Count > 0) Passengers.MoveEntity(CabinPosition);
                    }
                    if (ElevatorStarted && TargetFloor > 0 && Elevator.GetDisplacement == Floors.Floor(TargetFloor).Distance) SendToFloor(0);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private void StartElevator()
        {
            ElevatorStarted = true;
            if (ElevatorSessionComp.IsDecisionMaker) Elevator_Block.Enabled = true;
            ElevatorSpeed = ReadValue.GetNumberFloat(Elevator_Block.CustomData, "ElevatorSpeed", 10);
            if (ShouldMoveElevator) Passengers.GetPassengers(CabinMatrix, CabinPosition);
            Elevator.SetStartDistAndMoveModifier();
        }

        public override void UpdateBeforeSimulation10()
        {
            try
            {
                if (Elevator_Block.CubeGrid.Physics != null && Elevator_Block.IsFunctional && !GridIsRotating())
                {
                    if (Floors.ElevatorReady)
                    {
                        Elevator.CheckCabin();
                        Elevator.CheckPower();
                    }
                    if (Elevator.JustGotPower || Elevator.JustGotCabin) StartElevator();
                }
                bool elevatorFunctional = Elevator_Block.CubeGrid.Physics != null && Elevator_Block.IsFunctional && Elevator_Block.IsAttached;
                bool elevatorEnabled = Elevator.IsPowered && Floors.ElevatorReady && Elevator.CabinReady && elevatorFunctional && Elevator_Block.HasLocalPlayerAccess();
                if (elevatorEnabled != ElevatorEnabled)
                {
                    ElevatorEnabled = elevatorEnabled;
                    if (Elevator.CanUpdateCabin) Elevator.Cabin.UpdateLight();
                    if (!MyAPIGateway.Utilities.IsDedicated) Floors.UpdateElevatorEnabled(elevatorEnabled);
                    Elevator_Block.RefreshCustomInfo();
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        public void SendToFloor(int i)
        {
            if (ShouldMoveElevator) Communication.RequestFloor(0, Elevator.Id);
            Communication.RequestFloor(i, Elevator.Id);
        }

        public void SetTargetFloor(int selectedFloor)
        {
            if (!ElevatorEnabled || selectedFloor > Floors.Count) return;
            if (selectedFloor != CurrentFloor) Floors.Floor(CurrentFloor).CloseDoor();
            Logging.Instance.WriteLine($"Setting Target Floor: {selectedFloor}");
            SetNewTargetFloor(selectedFloor);
        }

        public void StopElevator(bool reset = false)
        {
            ElevatorStarted = false;
            if (ElevatorSessionComp.IsDecisionMaker) Elevator_Block.Enabled = false;
            Elevator.ResetMoveElevatorValues();
            if (Passengers.Count > 0) Passengers.Clear();
            if (reset)
            {
                Elevator_Block.Detach();
                Elevator_Block.Displacement = 0;
                CurrentFloor = 0;
                return;
            }
            if (ShouldMoveElevator)
            {
                Logging.Instance.WriteLine("Target Floor Reached: Stopping Elevator");
                if (ShouldPlayDingSound) Elevator.PlayDingSound();
                ShouldMoveElevator = false;
            }
            if (CurrentFloor == TargetFloor && Floors.Count > 0) Floors.Floor(CurrentFloor).OpenDoor();
        }

        public void SetNewPassengerOffset(byte[] data)
        {
            Passenger passenger = Passengers.FindByEntityId(BitConverter.ToInt64(data, 5));
            if (passenger == null) return;
            Vector3D futureOffset = passenger.Оffset;
            if (BitConverter.ToBoolean(data, 1)) futureOffset += passenger.Entity.LocalMatrix.Forward * 0.03f;
            if (BitConverter.ToBoolean(data, 2)) futureOffset += passenger.Entity.LocalMatrix.Backward * 0.03f;
            if (BitConverter.ToBoolean(data, 3)) futureOffset += passenger.Entity.LocalMatrix.Left * 0.03f;
            if (BitConverter.ToBoolean(data, 4)) futureOffset += passenger.Entity.LocalMatrix.Right * 0.03f;
            if (Vector3D.Distance(futureOffset, new Vector3D(0)) <= 1.45) passenger.Оffset = futureOffset;
        }

        private void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
            stringBuilder.AppendLine($"\n[> Elevator Status <]: {(ElevatorEnabled ? "Ready!" : "Not working!")}");
            stringBuilder.AppendLine($"[> Cabin Status <]: {(Elevator.CabinReady ? "Ready!" : "Not Found!")}");
            stringBuilder.AppendLine($"Floors Count: {Floors.Count}");
            stringBuilder.AppendLine($"Elevator Height: {Floors.ElevatorHeight} blocks");
            if (!ElevatorEnabled)
            {
                stringBuilder.AppendLine("\n[ Errors ]:");
                if (!Elevator.IsPowered) stringBuilder.AppendLine("Not enough power!");
                if (!Floors.ElevatorReady) stringBuilder.AppendLine("Floors not finished!");
                if (!Floors.HasEnoughFloors) stringBuilder.AppendLine("Less than 2 floors!");
                if (!Floors.FirstFloorIsBottom) stringBuilder.AppendLine("Wrong or no block for first floor!");
                if (!Floors.LastFloorIsTop)
                {
                    stringBuilder.AppendLine("Top floor not found or \n elevator's path is blocked!");
                    if (Floors.Count == 9) stringBuilder.AppendLine("Max floors (9) without Top floor!");
                    int maxHeight = Elevator.IsMidBlock ? Elevator.MaxHeight + 2 : Elevator.MaxHeight;
                    if (Floors.ElevatorHeight >= maxHeight) stringBuilder.AppendLine($"Max height ({maxHeight}) without Top floor!");
                }
            }
            stringBuilder.AppendLine("\nRequired power: 100kW");
        }

        private void LoadTerminalControlSettings()
        {
            ElevatorSpeed = ReadValue.GetNumberInt(Elevator_Block.CustomData, "ElevatorSpeed", 10);
            if (!Elevator_Block.CustomData.Contains("ElevatorSpeed")) Elevator_Block.CustomData += $"\nElevatorSpeed[{ElevatorSpeed}]";
            FloorOffset = ReadValue.GetNumberInt(Elevator_Block.CustomData, "FloorOffset", 0);
            if (!Elevator_Block.CustomData.Contains("FloorOffset")) Elevator_Block.CustomData += $"\nFloorOffset[{FloorOffset}]";
            MusicSelector = ReadValue.GetNumberInt(Elevator_Block.CustomData, "MusicSelector", 1);
            if (!Elevator_Block.CustomData.Contains("MusicSelector")) Elevator_Block.CustomData += $"\nMusicSelector[{MusicSelector}]";
            MusicVolume = ReadValue.GetNumberInt(Elevator_Block.CustomData, "MusicVolume", 10);
            if (!Elevator_Block.CustomData.Contains("MusicVolume")) Elevator_Block.CustomData += $"\nMusicVolume[{MusicVolume}]";
            ShouldPlayDingSound = ReadValue.GetBool(Elevator_Block.CustomData, "ShouldPlayDingSound", true);
            if (!Elevator_Block.CustomData.Contains("ShouldPlayDingSound")) Elevator_Block.CustomData += $"\nShouldPlayDingSound[{ShouldPlayDingSound}]";
            LightSelector = ReadValue.GetNumberInt(Elevator_Block.CustomData, "LightSelector", 2);
            if (!Elevator_Block.CustomData.Contains("LightSelector")) Elevator_Block.CustomData += $"\nLightSelector[{LightSelector}]";
            byte R = (byte)ReadValue.GetNumberInt(Elevator_Block.CustomData, "CabinLightRed", 255);
            if (!Elevator_Block.CustomData.Contains("CabinLightRed")) Elevator_Block.CustomData += $"\nCabinLightRed[{CabinLightColor.R}]";
            byte G = (byte)ReadValue.GetNumberInt(Elevator_Block.CustomData, "CabinLightGreen", 255);
            if (!Elevator_Block.CustomData.Contains("CabinLightGreen")) Elevator_Block.CustomData += $"\nCabinLightGreen[{CabinLightColor.G}]";
            byte B = (byte)ReadValue.GetNumberInt(Elevator_Block.CustomData, "CabinLightBlue", 255);
            if (!Elevator_Block.CustomData.Contains("CabinLightBlue")) Elevator_Block.CustomData += $"\nCabinLightBlue[{CabinLightColor.B}]";
            CabinLightColor = new Color(R, G, B);
            CabinLightRange = ReadValue.GetNumberFloat(Elevator_Block.CustomData, "CabinLightRange", 3);
            if (!Elevator_Block.CustomData.Contains("CabinLightRange")) Elevator_Block.CustomData += $"\nCabinLightRange[{CabinLightRange}]";
            CabinLightIntensity = ReadValue.GetNumberFloat(Elevator_Block.CustomData, "CabinLightIntensity", 2);
            if (!Elevator_Block.CustomData.Contains("CabinLightIntensity")) Elevator_Block.CustomData += $"\nCabinLightIntensity[{CabinLightIntensity}]";
            ShowChristmasLights = ReadValue.GetBool(Elevator_Block.CustomData, "ShowChristmasLights");
            if (!Elevator_Block.CustomData.Contains("ShowChristmasLights")) Elevator_Block.CustomData += $"\nShowChristmasLights[{ShowChristmasLights}]";
        }
    }
}