using System;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.Entity;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Elevator
    {
        private float TargetDisplacement;
        private float CurrentElevatorSpeed;
        private float DistanceAccelerating;
        private float StartingDistance;
        private bool Deccelerating;
        private bool oldIsPowered;
        private bool oldCabinReady;
        private readonly MySoundPair Ding = new MySoundPair("ElevatorDing");

        public IMyMotorAdvancedStator Block { get; private set; }
        public Cabin Cabin => Block.Top.GameLogic.GetAs<MultifloorElevatorTop>().Cabin;
        public long Id { get; private set; }
        public bool IsMidBlock { get; private set; }
        public float AccelerationTime { get; private set; }
        public int MoveModifier { get; private set; }
        public bool IsPowered { get; private set; }
        public bool JustGotPower { get; private set; }
        public bool CabinReady { get; private set; }
        public bool JustGotCabin { get; private set; }
        public int MaxHeight { get; private set; }
        public float GetDisplacement => -Block.Displacement;
        public MatrixD CabinMatrix => Block.Top.WorldMatrix;
        public Vector3D CabinPosition => CabinMatrix.Translation + (CabinMatrix.Up * 2.5f);
        public bool CanUpdateCabin => !MyAPIGateway.Utilities.IsDedicated && Block.Top != null && Cabin != null;

        public Elevator(IMyMotorAdvancedStator elevator)
        {
            if (elevator == null) return;
            Block = elevator;
            Id = elevator.EntityId;
            IsMidBlock = elevator.BlockDefinition.SubtypeId == "VCZ_Elevator_Rotor";
            MaxHeight = (int)(MyDefinitionManager.Static.GetCubeBlockDefinition(Block.BlockDefinition) as MyMotorAdvancedStatorDefinition).SafetyDetach;
            InitialSetup();
        }

        public void MoveElevator(float elevatorSpeed)
        {
            float currentDistance = Math.Abs(TargetDisplacement - GetDisplacement);
            if (Block.Enabled && TargetDisplacement != GetDisplacement)
            {
                if (CurrentElevatorSpeed <= elevatorSpeed && !Deccelerating)
                {
                    CurrentElevatorSpeed += 0.1f;
                    AccelerationTime++;
                    if (CurrentElevatorSpeed >= elevatorSpeed || currentDistance >= StartingDistance / 2) DistanceAccelerating = StartingDistance - currentDistance;
                }
                if (currentDistance <= DistanceAccelerating)
                {
                    Deccelerating = true;
                    if (AccelerationTime > 1) CurrentElevatorSpeed -= 0.1f;
                    AccelerationTime--;
                }
                if (currentDistance < CurrentElevatorSpeed / 60) Block.Displacement += currentDistance * MoveModifier;
                else Block.Displacement += (CurrentElevatorSpeed / 60) * MoveModifier;
            }
        }

        public void CheckCabin()
        {
            CabinReady = Block.IsAttached;
            if (CabinReady != oldCabinReady) Block.RefreshCustomInfo();
            if (!CabinReady && ElevatorSessionComp.IsDecisionMaker) Block.ApplyAction("Add Top Part");
            JustGotCabin = CabinReady != oldCabinReady && CabinReady;
            oldCabinReady = CabinReady;
        }

        public void CheckPower()
        {
            IsPowered = Block.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, 0.10f);
            if (IsPowered != oldIsPowered) Block.RefreshCustomInfo();
            JustGotPower = IsPowered != oldIsPowered && IsPowered;
            oldIsPowered = IsPowered;
        }

        public void ResetMoveElevatorValues()
        {
            CurrentElevatorSpeed = 0;
            Deccelerating = false;
            AccelerationTime = 0;
            DistanceAccelerating = 0;
        }

        public void SetTargetDisplacement(float targetDisplacement) => TargetDisplacement = targetDisplacement;

        public void SetStartDistAndMoveModifier()
        {
            StartingDistance = Math.Abs(TargetDisplacement - GetDisplacement);
            MoveModifier = TargetDisplacement > GetDisplacement ? -1 : 1;
        }

        public void PlayDingSound()
        {
            if (MyAPIGateway.Utilities.IsDedicated || MyAPIGateway.Session.Player.Character == null) return;
            var Dinger = new MyEntity3DSoundEmitter(Block.Top as MyEntity, dopplerScaler: 0) { CustomMaxDistance = 5f };
            Dinger.SetPosition(CabinPosition);
            Dinger.PlaySound(Ding, alwaysHearOnRealistic: true);
        }

        private void InitialSetup()
        {
            Block.GetProperty("ShareInertiaTensor").AsBool().SetValue(Block, false);
            Block.BrakingTorque = 100000000;
            Block.TargetVelocityRPM = 0;
            Block.Torque = 100000000;
            Block.RotorLock = true;
        }
    }
}