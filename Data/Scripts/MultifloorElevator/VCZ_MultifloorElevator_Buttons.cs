using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MedicalRoom), false, "VCZ_Elevator_Buttons")]
    public class MultifloorElevatorButtons : MyGameLogicComponent
    {
        private IMyMedicalRoom ElevatorButtons;
        private IMyMotorAdvancedRotor ElevatorTop;
        private MultifloorElevator ElevatorGameLogic;
        private bool ButtonsSetToBlack;
        private bool SetButtonsRed;
        private bool PlayerPresentAndAlive => MyAPIGateway.Session.Player.Character != null && !MyAPIGateway.Session.Player.Character.IsDead;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            ElevatorButtons = (IMyMedicalRoom)Entity;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public override void Close() => NeedsUpdate = MyEntityUpdateEnum.NONE;

        public override void UpdateOnceBeforeFrame() => ConfigureUpdates();

        public override void UpdateBeforeSimulation10() => ConfigureUpdates();

        private void ConfigureUpdates()
        {
            try
            {
                IMySlimBlock block = ElevatorButtons.CubeGrid.GetCubeBlock(ElevatorButtons.Position + (Vector3I)ElevatorButtons.LocalMatrix.Down);
                if (block != null && block.FatBlock is IMyMotorAdvancedRotor)
                {
                    ElevatorTop = block.FatBlock as IMyMotorAdvancedRotor;
                    ElevatorGameLogic = ElevatorTop.Base.GameLogic.GetAs<MultifloorElevator>();
                    NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
                    NeedsUpdate ^= MyEntityUpdateEnum.EACH_10TH_FRAME;
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
                if (ElevatorSessionComp.IsDecisionMaker && (ElevatorTop == null || ElevatorTop.Closed)) ElevatorButtons.CubeGrid.RazeBlock(ElevatorButtons.Position);
                if (!MyAPIGateway.Utilities.IsDedicated && PlayerPresentAndAlive && ElevatorButtons.CubeGrid.Physics != null)
                {
                    if (!ButtonsSetToBlack && !SetButtonsRed)
                    {
                        for (int i = 1; i <= 9; i++) ElevatorTop.SetEmissiveParts($"LookingAtLightFloor{i}", Color.Black, 0f);
                        ButtonsSetToBlack = true;
                    }
                    if (ElevatorGameLogic.Floors.Count > 0 && MyAPIGateway.Gui.ActiveGamePlayScreen == null) UpdateButtons(MyAPIGateway.Session.Player.Character);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            try
            {
                if (ElevatorSessionComp.IsDecisionMaker)
                {
                    if (ElevatorButtons.Enabled) ElevatorButtons.Enabled = false;
                    if (ElevatorButtons.ShowInTerminal) ElevatorButtons.ShowInTerminal = false;
                    if (ElevatorButtons.ShowInToolbarConfig) ElevatorButtons.ShowInToolbarConfig = false;
                    if (!ElevatorButtons.IsFunctional) ElevatorButtons.SlimBlock.IncreaseMountLevel(1f, ElevatorTop.Base.OwnerId);
                }
                if (!MyAPIGateway.Utilities.IsDedicated && PlayerPresentAndAlive && ElevatorButtons.CubeGrid.Physics != null)
                {
                    if (ElevatorGameLogic.ElevatorEnabled) SetButtonsRed = false;
                    else SetButtonsRed = true;
                    if (SetButtonsRed && ButtonsSetToBlack)
                    {
                        for (int i = 1; i <= ElevatorGameLogic.Floors.Count; i++) ElevatorTop.SetEmissiveParts($"LookingAtLightFloor{i}", Color.Red, 1f);
                        ButtonsSetToBlack = false;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private void UpdateButtons(IMyCharacter player)
        {
            if (!SetButtonsRed && player.Components.Get<MyCharacterDetectorComponent>()?.UseObject == null) return;
            if (player.Components.Get<MyCharacterDetectorComponent>().DetectedEntity != ElevatorButtons) return;
            IMyModelDummy dummy = player.Components.Get<MyCharacterDetectorComponent>().UseObject.Dummy as IMyModelDummy;
            bool UseDummy = MyAPIGateway.Input.IsGameControlReleased(MyControlsSpace.USE) || MyAPIGateway.Input.IsGameControlReleased(MyControlsSpace.PRIMARY_TOOL_ACTION);
            if (!"detector_block_010|detector_block_020|detector_block_030|detector_block_040".Contains(dummy.Name))
            {
                for (int i = 1; i <= ElevatorGameLogic.Floors.Count; i++)
                {
                    if (dummy.Name == $"detector_block_00{i}" || dummy.Name == $"detector_block_01{i}" || dummy.Name == $"detector_block_02{i}" || dummy.Name == $"detector_block_03{i}")
                    {
                        ElevatorTop.SetEmissiveParts($"LookingAtLightFloor{i}", Color.Orange, 1f);
                        ButtonsSetToBlack = false;
                        if (UseDummy) ElevatorGameLogic.SendToFloor(i);
                        break;
                    }
                }
            }
            else if (UseDummy) ElevatorGameLogic.SendToFloor(0);
        }
    }
}