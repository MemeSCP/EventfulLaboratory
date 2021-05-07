using System;

namespace EventfulLaboratory
{
    public class Constant
    {
        public static readonly String PLUGIN_NAME = "EventfulLaboratory";
        public static readonly String VERSION = "v1.0.0";
        public static readonly String SHORTFORM = "eventLab";

        public static readonly String SHELTER_NAME = "EZ_Shelter";

        public static readonly String FOUR_NINE_CHAMBER = "HCZ_049";
        public static readonly String SEVEN_NINE_CHAMBER = "HCZ_079";
        public static readonly String HCZ_ELEV_A_DOOR = "HCZ_ChkpA";
        public static readonly String HCZ_ELEV_B_DOOR = "HCZ_ChkpB";
        public static readonly String HCZ_106_DOOR = "HCZ_106";

        public static readonly String ECZ_GATE = "CHECKPOINT_EZ_HCZ";

        public static readonly String PEANUT_CHAMBER_WELCOME = "Welcome to the Peanut Chamber. Prepare your allergies.";
        public static readonly String PEANUT_CHAMBER_DCLASS_WARN = "Prepare your necks! It's snapping time.";
        public static readonly String PEANUT_CHAMBER_173_WARN = "You know what to do.";
        public static readonly String PEANUT_CHAMBER_DBOY_WIN = "You win! Congrats!";
        public static readonly String PEANUT_CHAMBER_DBOY_ELLAMINATE = "Kill the last one!";
        public static readonly String PEANUT_CHAMBER_END = "Game end. Thanks for playing!";

        public static readonly String[] PEANUT_VIRUS_DOOR_BLOCK = {HCZ_ELEV_A_DOOR, HCZ_ELEV_B_DOOR, HCZ_106_DOOR};

        public static readonly String THAWED_EFFECT_API_NAME = "thawed";
    }
}