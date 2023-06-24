using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Lights;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Cabin
    {
        public IMyMotorAdvancedRotor Block { get; set; }
        public MultifloorElevator ElevatorGameLogic { get; set; }
        private MyLight Light { get; set; }
        public MyEntity3DSoundEmitter SoundEmitter { get; set; }
        private MySoundPair ElevatorMusic { get; set; }
        private int frame = -1;

        public Cabin(IMyMotorAdvancedRotor cabin)
        {
            if (cabin == null) return;
            Block = cabin;
            ElevatorGameLogic = Block.Base.GameLogic.GetAs<MultifloorElevator>();
            SoundEmitter = new MyEntity3DSoundEmitter(Block as MyEntity, dopplerScaler: 0);
            ElevatorMusic = new MySoundPair($"ElevatorMusic{ElevatorGameLogic.MusicSelector}");
            InitLight(new Dictionary<string, IMyModelDummy>());
            UpdateVolume();
            UpdateFloorOffset();
            ElevatorGameLogic.Floors.FindFloors(ElevatorGameLogic.Elevator);
            Block.GetSubpart("VCZ_Elevator_ChristmasLights").Render.Visible = ElevatorGameLogic.ShowChristmasLights;
        }

        private void InitLight(Dictionary<string, IMyModelDummy> Dummies)
        {
            int dummiesCount = Block.Model.GetDummies(Dummies);
            for (int i = 1; i <= dummiesCount; i++)
            {
                if (!Dummies.ContainsKey($"CabinLight_{i}")) continue;
                IMyModelDummy lightDummy = Dummies.GetValueOrDefault($"CabinLight_{i}");
                Light = MyLights.AddLight();
                Light.Start(lightDummy.Name);
                Light.Falloff = 1f;
                Light.ReflectorFalloff = 1f;
                Light.ReflectorConeDegrees = 150; // projected light angle in degrees, max 179.
                Light.ParentID = Block.CubeGrid.Render.GetRenderObjectID();
                Light.Position = Vector3D.Transform(Vector3D.Transform(lightDummy.Matrix.Translation, Block.WorldMatrix), Block.CubeGrid.WorldMatrixInvScaled);
                Light.ReflectorDirection = Vector3.TransformNormal(Vector3.TransformNormal(lightDummy.Matrix.Forward, Block.WorldMatrix), Block.CubeGrid.WorldMatrixInvScaled);
                Light.ReflectorTexture = @"Textures\Lights\reflector.dds"; // NOTE: for textures inside your mod you need to use: Utils.GetModTextureFullPath(@"Textures\someFile.dds");
            }
            UpdateLight();
        }

        public void UpdateMusic(Vector3D playerPosition)
        {
            if (ElevatorGameLogic.MusicSelector > 0 && Vector3D.Distance(ElevatorGameLogic.CabinPosition, playerPosition) < SoundEmitter.CustomMaxDistance)
            {
                if (!SoundEmitter.IsPlaying) SoundEmitter.PlaySound(ElevatorMusic, stopPrevious: true, alwaysHearOnRealistic: true);
                SoundEmitter.SetPosition(ElevatorGameLogic.CabinPosition);
                SoundEmitter.Update();
            }
            else if (SoundEmitter.IsPlaying) SoundEmitter.StopSound(true);
        }

        public void SelectMusic(int musicSelector)
        {
            if (musicSelector > 0)
            {
                ElevatorMusic = new MySoundPair($"ElevatorMusic{musicSelector}");
                SoundEmitter.PlaySound(ElevatorMusic, stopPrevious: true, alwaysHearOnRealistic: true);
            }
            else SoundEmitter.StopSound(true);
        }

        public void UpdateVolume()
        {
            SoundEmitter.VolumeMultiplier = (float)ElevatorGameLogic.MusicVolume / 10;
            SoundEmitter.CustomMaxDistance = SoundEmitter.VolumeMultiplier >= 1 ? SoundEmitter.VolumeMultiplier * 3 : 3;
        }

        public void UpdateCurrentFloor(int currentFloor, int targetFloor)
        {
            SetDigit.UpdateFloorDisplay(Block, "CurrentFloorDisplay", currentFloor, Color.Orange, ElevatorGameLogic.FloorOffset);
            if (currentFloor == targetFloor) SetDigit.UpdateFloorDisplay(Block, $"LightFloor{currentFloor}", currentFloor, Color.White, ElevatorGameLogic.FloorOffset);
        }

        public void UpdateTargetFloor(int currentFloor, int targetFloor)
        {
            if (targetFloor == currentFloor) SetDigit.UpdateFloorDisplay(Block, $"LightFloor{targetFloor}", targetFloor, Color.White, ElevatorGameLogic.FloorOffset);
            else SetDigit.UpdateFloorDisplay(Block, $"LightFloor{targetFloor}", targetFloor, Color.Orange, ElevatorGameLogic.FloorOffset);
        }

        public void UpdateFloorsCount(int floorsCount)
        {
            for (int i = 1; i <= 9; i++)
            {
                SetDigit.UpdateFloorDisplay(Block, $"LightFloor{i}", i, Color.White, ElevatorGameLogic.FloorOffset, floorsCount);
                Block.SetEmissiveParts($"LookingAtLightFloor{i}", Color.Black, 0f);
            }
        }

        public void UpdateFloorOffset()
        {
            UpdateFloorsCount(ElevatorGameLogic.Floors.Count);
            UpdateCurrentFloor(ElevatorGameLogic.CurrentFloor, ElevatorGameLogic.TargetFloor);
            UpdateTargetFloor(ElevatorGameLogic.CurrentFloor, ElevatorGameLogic.TargetFloor);
        }

        public void UpdateLight()
        {
            if (ElevatorGameLogic.LightSelector > 0 && ElevatorGameLogic.ElevatorEnabled)
            {
                Block.SetEmissiveParts("Light", ElevatorGameLogic.CabinLightColor, 0.01f);
                Light.LightOn = true;
                Light.Color = ElevatorGameLogic.CabinLightColor;
                Light.Range = ElevatorGameLogic.LightSelector == 2 ? ElevatorGameLogic.CabinLightRange / 10 : ElevatorGameLogic.CabinLightRange;
                Light.Intensity = ElevatorGameLogic.CabinLightIntensity;
                Light.ReflectorOn = ElevatorGameLogic.LightSelector == 2;
                Light.ReflectorColor = ElevatorGameLogic.CabinLightColor;
                Light.ReflectorRange = ElevatorGameLogic.CabinLightRange;
                Light.ReflectorIntensity = ElevatorGameLogic.CabinLightIntensity;
            }
            else
            {
                Block.SetEmissiveParts("Light", Color.DarkGray, 0.00f);
                Light.LightOn = false;
                Light.ReflectorOn = false;
            }
            Light.UpdateLight();
        }

        public void UpdateChristmasLights()
        {
            if (ElevatorGameLogic.ShowChristmasLights)
            {
                if (frame++ == 839) frame = 0;
                if (frame == 0 || frame == 540) Block.SetEmissivePartsForSubparts("LightSequence1", Color.White, 1f);
                if (frame == 60 || frame == 480) Block.SetEmissivePartsForSubparts("LightSequence2", Color.Green, 1f);
                if (frame == 120 || frame == 420) Block.SetEmissivePartsForSubparts("LightSequence3", Color.Red, 1f);
                if (frame == 180 || frame == 360 || frame == 600 || frame == 780)
                {
                    for (int i = 1; i <= 3; i++) Block.SetEmissivePartsForSubparts("LightSequence" + i, Color.Transparent, 0f);
                }
                if (frame == 240 || frame == 300 || frame == 660 || frame == 720)
                {
                    for (int i = 1; i <= 3; i++) Block.SetEmissivePartsForSubparts("LightSequence" + i, Color.Yellow, 1f);
                }
            }
        }
    }
}