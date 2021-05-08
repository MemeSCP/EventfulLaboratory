using System;

namespace EventfulLaboratory
{
    public static class Constant
    {
        public const string PLUGIN_NAME = "EventfulLaboratory";
        public const string VERSION = "v1.0.0";
        public const string SHORTFORM = "eventLab";

        public const string SHELTER_NAME = "EZ_Shelter";

        public const string FOUR_NINE_CHAMBER = "HCZ_049";
        public const string SEVEN_NINE_CHAMBER = "HCZ_079";
        public const string HCZ_ELEV_A_DOOR = "HCZ_ChkpA";
        public const string HCZ_ELEV_B_DOOR = "HCZ_ChkpB";
        public const string HCZ_106_DOOR = "HCZ_106";

        public const string ECZ_GATE = "CHECKPOINT_EZ_HCZ";

        public const string PEANUT_CHAMBER_WELCOME = "Welcome to the Peanut Chamber. Prepare your allergies.";
        public const string PEANUT_CHAMBER_DCLASS_WARN = "Prepare your necks! It's snapping time.";
        public const string PEANUT_CHAMBER_173_WARN = "You know what to do.";
        public const string PEANUT_CHAMBER_DBOY_WIN = "You win! Congrats!";
        public const string PEANUT_CHAMBER_DBOY_ELLAMINATE = "Kill the last one!";
        public const string PEANUT_CHAMBER_END = "Game end. Thanks for playing!";
        
        public const string THAWED_EFFECT_API_NAME = "thawed";
        
        public static readonly string[] PEANUT_VIRUS_DOOR_BLOCK = {HCZ_ELEV_A_DOOR, HCZ_ELEV_B_DOOR, HCZ_106_DOOR};
    }
}