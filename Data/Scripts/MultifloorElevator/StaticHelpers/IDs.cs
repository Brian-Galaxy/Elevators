namespace Vicizlat.MultifloorElevator
{
    public static class IDs
    {
        public static string[] Elevator =
        {
            "VCZ_Elevator_Rotor",
            "VCZ_Elevator_Rotor_Base"
        };
        public static string[] ElevatorControl =
        {
            "OnOff",
            "CustomData",
            "SafetyDetach",
            "ShareInertiaTensor",
            "Add Top Part",
            "Add Small Top Part",
            "Reverse",
            "Detach",
            "Attach",
            "RotorLock",
            "Torque",
            "BrakingTorque",
            "Velocity",
            "LowerLimit",
            "UpperLimit",
            "Displacement"
        };
        public static string[] ElevatorAction =
        {
            "OnOff",
            "OnOff_On",
            "OnOff_Off",
            "IncreaseSafetyDetach",
            "DecreaseSafetyDetach",
            "ShareInertiaTensor",
            //"Add Top Part",
            "Add Small Top Part",
            "Reverse",
            "Detach",
            "Attach",
            "RotorLock",
            "IncreaseTorque",
            "DecreaseTorque",
            "IncreaseBrakingTorque",
            "DecreaseBrakingTorque",
            "IncreaseVelocity",
            "DecreaseVelocity",
            "ResetVelocity",
            "IncreaseLowerLimit",
            "DecreaseLowerLimit",
            "IncreaseUpperLimit",
            "DecreaseUpperLimit",
            "IncreaseDisplacement",
            "DecreaseDisplacement"
        };
        public static string[] Floor =
        {
            "VCZ_Elevator_Bottom",
            "VCZ_Elevator_Middle",
            "VCZ_Elevator_Middle_Double",
            "VCZ_Elevator_Top"
        };
        public static string[] FloorControl =
        {
            "OnOff",
            "ShowInToolbarConfig",
            "CustomData",
            "Open",
            "AnyoneCanUse"
        };
        public static string[] ValidElevatorBlock =
        {
            //"VCZ_Elevator_Rotor",
            //"VCZ_Elevator_Rotor_Base",
            "VCZ_Elevator_Bottom",
            "VCZ_Elevator_Middle",
            "VCZ_Elevator_Middle_Double",
            "VCZ_Elevator_Top",
            "VCZ_Elevator_Filler",
            "VCZ_Elevator_FillerDouble",
            "VCZ_Elevator_Filler_Vent"
        };
    }
}