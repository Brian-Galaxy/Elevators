using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;

namespace Vicizlat.MultifloorElevator
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ElevatorSessionComp : MySessionComponentBase
    {
        public static bool IsDecisionMaker => MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.MultiplayerActive || MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated;
        public override void LoadData()
        {
            try
            {
                Logging.Instance.WriteLine("<<< Debug Log Started >>>");
                Communication.RegisterHandlers();
                MyAPIGateway.TerminalControls.CustomActionGetter += HideActions;
                MyAPIGateway.TerminalControls.CustomControlGetter += HideControls;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        protected override void UnloadData()
        {
            try
            {
                MyAPIGateway.TerminalControls.CustomControlGetter -= HideControls;
                MyAPIGateway.TerminalControls.CustomActionGetter -= HideActions;
                Communication.UnregisterHandlers();
                Logging.Instance.WriteLine("<<< Debug Log Closed >>>");
                Logging.Instance.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private void HideActions(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            try
            {
                if (block != null && IDs.Elevator.Contains(block.BlockDefinition.SubtypeId))
                {
                    actions.Where(a => IDs.ElevatorAction.Contains(a.Id)).ToList().ForEach(a => a.Enabled = b => b.GameLogic.GetAs<MultifloorElevator>() == null);
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteException(e.Message, e.StackTrace);
            }
        }

        private void HideControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            try
            {
                if (block == null) return;
                if (IDs.Elevator.Contains(block.BlockDefinition.SubtypeId))
                {
                    controls.Where(c => IDs.ElevatorControl.Contains(c.Id)).ToList().ForEach(c => c.Visible = b => b.GameLogic.GetAs<MultifloorElevator>() == null);
                }
                if (IDs.Floor.Contains(block.BlockDefinition.SubtypeId))
                {
                    controls.Where(c => IDs.FloorControl.Contains(c.Id)).ToList().ForEach(c => c.Visible = b => b.GameLogic.GetAs<MultifloorElevatorFloors>() == null);
                    if (controls.Last(control => control.Id == "Open") != controls.First(control => control.Id == "Open"))
                    {
                        controls.Last(control => control.Id == "Open").Visible = b => !(b is IMyAdvancedDoor);
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