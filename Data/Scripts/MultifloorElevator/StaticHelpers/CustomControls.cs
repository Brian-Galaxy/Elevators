using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public static class CustomControls
    {
        public static bool ElevatorControlsInited = false;
        public static bool FloorControlsInited = false;
        private static MultifloorElevator ElevatorGameLogic(IMyTerminalBlock b) => b.GameLogic.GetAs<MultifloorElevator>();
        private static MultifloorElevatorTop CabinGameLogic(IMyTerminalBlock b) => ElevatorGameLogic(b).Elevator.Block.Top.GameLogic.GetAs<MultifloorElevatorTop>();
        private static bool CanUpdateCabin(IMyTerminalBlock b) => !MyAPIGateway.Utilities.IsDedicated && ElevatorGameLogic(b).Elevator.Block.Top != null && CabinGameLogic(b).Cabin != null;

        public static void CreateControlsAndActions<T>(Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            if (ElevatorControlsInited) return;
            TerminalHelpers.AddSlider<T>("ElevatorSpeed", "Elevator Speed", b => ElevatorGameLogic(b).ElevatorSpeed, (b, v) => SetterFloat(b, v, "ElevatorSpeed"), 1, 20, visible);
            TerminalHelpers.AddSlider<T>("FloorOffset", "Floor Offset", b => ElevatorGameLogic(b).FloorOffset, (b, v) => SetterFloat(b, v, "FloorOffset"), -10, 90, visible);
            TerminalHelpers.AddSeparator<T>(visible);
            TerminalHelpers.AddLabel<T>("CabinMusicLabel", "Cabin Music Controls", visible);
            TerminalHelpers.AddCombobox<T>("MusicSelector", "Select Music Track", b => ElevatorGameLogic(b).MusicSelector, (b, v) => SetterLong(b, v, "MusicSelector"), list => MusicList(list), visible);
            TerminalHelpers.AddSlider<T>("MusicVolumeSlider", "Music Volume", b => ElevatorGameLogic(b).MusicVolume, (b, v) => SetterFloat(b, v, "MusicVolume"), 0, 50, visible);
            TerminalHelpers.AddCheckbox<T>("ShouldPlayDingSound", "Play 'Ding' sound when stopping", b => ElevatorGameLogic(b).ShouldPlayDingSound, (b, v) => SetterBool(b, v, "ShouldPlayDingSound"), visible);
            TerminalHelpers.AddSeparator<T>(visible);
            TerminalHelpers.AddLabel<T>("CabinLightLabel", "Cabin Light Controls", visible);
            TerminalHelpers.AddCombobox<T>("LightSelector", "Select Cabin Light Type", b => ElevatorGameLogic(b).LightSelector, (b, v) => SetterLong(b, v, "LightSelector"), list => LightList(list), visible);
            TerminalHelpers.AddColorEditor<T>("CabinLightColorControl", "Color", b => ElevatorGameLogic(b).CabinLightColor, (b, v) => SetterColor(b, v), visible);
            TerminalHelpers.AddSlider<T>("CabinLightRangeSlider", "Range", b => ElevatorGameLogic(b).CabinLightRange, (b, v) => SetterFloat(b, v, "CabinLightRange"), 2, 20, visible);
            TerminalHelpers.AddSlider<T>("CabinLightIntensitySlider", "Intensity", b => ElevatorGameLogic(b).CabinLightIntensity, (b, v) => SetterFloat(b, v, "CabinLightIntensity"), 0.5f, 10, visible);
            TerminalHelpers.AddCheckbox<T>("ShowChristmasLights", "Show Christmas Lights", b => ElevatorGameLogic(b).ShowChristmasLights, (b, v) => SetterBool(b, v, "ShowChristmasLights"), visible);
            TerminalHelpers.AddButton<T>("ResetButton", "Reset Cabin", b => ElevatorGameLogic(b).SendToFloor(10), visible, "Use in case you need to reset the cabin back to original position.");
            ElevatorControlsInited = true;
            TerminalHelpers.AddAction<T>("SendToFloor1", "Send To Floor 1", b => ElevatorGameLogic(b).SendToFloor(1), (b, v) => v.Append("F1"));
            TerminalHelpers.AddAction<T>("SendToFloor2", "Send To Floor 2", b => ElevatorGameLogic(b).SendToFloor(2), (b, v) => v.Append("F2"));
            TerminalHelpers.AddAction<T>("SendToFloor3", "Send To Floor 3", b => ElevatorGameLogic(b).SendToFloor(3), (b, v) => v.Append("F3"));
            TerminalHelpers.AddAction<T>("SendToFloor4", "Send To Floor 4", b => ElevatorGameLogic(b).SendToFloor(4), (b, v) => v.Append("F4"));
            TerminalHelpers.AddAction<T>("SendToFloor5", "Send To Floor 5", b => ElevatorGameLogic(b).SendToFloor(5), (b, v) => v.Append("F5"));
            TerminalHelpers.AddAction<T>("SendToFloor6", "Send To Floor 6", b => ElevatorGameLogic(b).SendToFloor(6), (b, v) => v.Append("F6"));
            TerminalHelpers.AddAction<T>("SendToFloor7", "Send To Floor 7", b => ElevatorGameLogic(b).SendToFloor(7), (b, v) => v.Append("F7"));
            TerminalHelpers.AddAction<T>("SendToFloor8", "Send To Floor 8", b => ElevatorGameLogic(b).SendToFloor(8), (b, v) => v.Append("F8"));
            TerminalHelpers.AddAction<T>("SendToFloor9", "Send To Floor 9", b => ElevatorGameLogic(b).SendToFloor(9), (b, v) => v.Append("F9"));
            TerminalHelpers.AddAction<T>("Stop", "Emergency Stop", b => ElevatorGameLogic(b).SendToFloor(0), (b, v) => v.Append("STOP"));
        }

        public static void CreateTerminalControls<T>(Func<IMyTerminalBlock, bool> visible) where T : IMyTerminalBlock
        {
            if (FloorControlsInited) return;
            TerminalHelpers.AddOnOff<T>("TimedDoorClose", "Close Door on Timer", b => b.GameLogic.GetAs<MultifloorElevatorFloors>().TimedDoorClose, (b, v) => SetterBool(b, v, b.CustomData), visible);
            FloorControlsInited = true;
        }

        private static void SetterBool(IMyTerminalBlock b, bool v, string keyWord)
        {
            if (b == null) return;
            if (b is IMyMotorAdvancedStator)
            {
                if (keyWord == "ShouldPlayDingSound") ElevatorGameLogic(b).ShouldPlayDingSound = v;
                if (keyWord == "ShowChristmasLights")
                {
                    ElevatorGameLogic(b).ShowChristmasLights = v;
                    if (!MyAPIGateway.Utilities.IsDedicated) ElevatorGameLogic(b).Elevator.Block.Top.GetSubpart("VCZ_Elevator_ChristmasLights").Render.Visible = v;
                }
                b.CustomData = b.CustomData.Replace($"{keyWord}[{ReadValue.GetBool(b.CustomData, keyWord)}]", $"{keyWord}[{v}]");
            }
            else if (b is IMyAdvancedDoor)
            {
                b.GameLogic.GetAs<MultifloorElevatorFloors>().TimedDoorClose = v;
                if (v && !keyWord.Contains("TimedDoorClose")) b.CustomData += "\nTimedDoorClose";
                if (!v && keyWord.Contains("TimedDoorClose")) b.CustomData = keyWord.Replace("\nTimedDoorClose", string.Empty);
            }
        }

        private static void SetterFloat(IMyTerminalBlock b, float v, string keyWord)
        {
            if (b == null) return;
            if (keyWord == "ElevatorSpeed" || keyWord == "FloorOffset" || keyWord == "MusicVolume")
            {
                if (keyWord == "ElevatorSpeed") ElevatorGameLogic(b).ElevatorSpeed = (int)v;
                if (keyWord == "FloorOffset")
                {
                    ElevatorGameLogic(b).FloorOffset = (int)v;
                    if (!MyAPIGateway.Utilities.IsDedicated) ElevatorGameLogic(b).Floors.UpdateFloorOffset((int)v);
                    if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.UpdateFloorOffset();
                }
                if (keyWord == "MusicVolume")
                {
                    ElevatorGameLogic(b).MusicVolume = (int)v;
                    if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.UpdateVolume();
                }
                b.CustomData = b.CustomData.Replace($"{keyWord}[{ReadValue.GetNumberInt(b.CustomData, keyWord)}]", $"{keyWord}[{(int)v}]");
            }
            if (keyWord == "CabinLightRange" || keyWord == "CabinLightIntensity")
            {
                if (keyWord == "CabinLightRange") ElevatorGameLogic(b).CabinLightRange = v;
                if (keyWord == "CabinLightIntensity") ElevatorGameLogic(b).CabinLightIntensity = v;
                b.CustomData = b.CustomData.Replace($"{keyWord}[{ReadValue.GetNumberFloat(b.CustomData, keyWord)}]", $"{keyWord}[{v}]");
                if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.UpdateLight();
            }
        }

        private static void SetterLong(IMyTerminalBlock b, long v, string keyWord)
        {
            if (b == null) return;
            if (keyWord == "MusicSelector")
            {
                ElevatorGameLogic(b).MusicSelector = (int)v;
                if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.SelectMusic((int)v);
            }

            if (keyWord == "LightSelector")
            {
                ElevatorGameLogic(b).LightSelector = (int)v;
                if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.UpdateLight();
            }
            b.CustomData = b.CustomData.Replace($"{keyWord}[{ReadValue.GetNumberInt(b.CustomData, keyWord)}]", $"{keyWord}[{v}]");
        }

        private static void SetterColor(IMyTerminalBlock b, Color v)
        {
            if (b == null) return;
            ElevatorGameLogic(b).CabinLightColor = v;
            b.CustomData = b.CustomData.Replace($"CabinLightRed[{ReadValue.GetNumberInt(b.CustomData, "CabinLightRed")}]", $"CabinLightRed[{v.R}]");
            b.CustomData = b.CustomData.Replace($"CabinLightGreen[{ReadValue.GetNumberInt(b.CustomData, "CabinLightGreen")}]", $"CabinLightGreen[{v.G}]");
            b.CustomData = b.CustomData.Replace($"CabinLightBlue[{ReadValue.GetNumberInt(b.CustomData, "CabinLightBlue")}]", $"CabinLightBlue[{v.B}]");
            if (CanUpdateCabin(b)) CabinGameLogic(b).Cabin.UpdateLight();
        }

        private static void MusicList(List<MyTerminalControlComboBoxItem> list)
        {
            list.Add(TerminalHelpers.AddComboboxItem(0, "No Music"));
            list.Add(TerminalHelpers.AddComboboxItem(1, "Elevator Music 1"));
            list.Add(TerminalHelpers.AddComboboxItem(2, "Elevator Music 2"));
            list.Add(TerminalHelpers.AddComboboxItem(3, "Mass Effect Elevator Music"));
            list.Add(TerminalHelpers.AddComboboxItem(4, "Left Bank Two"));
            list.Add(TerminalHelpers.AddComboboxItem(5, "Blues Brothers"));
            list.Add(TerminalHelpers.AddComboboxItem(6, "Psi-Ops Elevator Music"));
            list.Add(TerminalHelpers.AddComboboxItem(7, "Jingle Bells"));
        }

        private static void LightList(List<MyTerminalControlComboBoxItem> list)
        {
            list.Add(TerminalHelpers.AddComboboxItem(0, "No Light"));
            list.Add(TerminalHelpers.AddComboboxItem(1, "Standard light (No shadows)"));
            list.Add(TerminalHelpers.AddComboboxItem(2, "Spotlight (With shadows)"));
        }
    }
}