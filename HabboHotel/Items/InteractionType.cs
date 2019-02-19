﻿namespace Oblivion.HabboHotel.Items
{
    public enum InteractionType
    {
        None,
        Gate,
        Postit,
        Moodlight,
        Trophy,
        Bed,
        Scoreboard,
        VendingMachine,
        Alert,
        OneWayGate,
        LoveShuffler,
        WiredScoreBoard,
        HabboWheel,
        Dice,
        Bottle,
        Hopper,
        Teleport,
        Pool,
        Roller,
        FootballGate,
        Petbed,
        Pet,
        IceSkates,
        NormalSkates,
        Lowpool,
        Haloweenpool,
        Football,
        FootballGoalGreen,
        FootballGoalYellow,
        FootballGoalBlue,
        FootballGoalRed,
        Footballcountergreen,
        Footballcounteryellow,
        Footballcounterblue,
        Footballcounterred,
        Banzaigateblue,
        Banzaigatered,
        Banzaigateyellow,
        Banzaigategreen,
        Banzaifloor,
        Banzaiscoreblue,
        Banzaiscorered,
        Banzaiscoreyellow,
        Banzaiscoregreen,
        Banzaicounter,
        Banzaitele,
        Banzaipuck,
        Banzaipyramid,
        Freezetimer,
        Freezeexit,
        Freezeredcounter,
        Freezebluecounter,
        Freezeyellowcounter,
        Freezegreencounter,
        FreezeYellowGate,
        FreezeRedGate,
        FreezeGreenGate,
        FreezeBlueGate,
        FreezeTileBlock,
        FreezeTile,
        Jukebox,
        MusicDisc,
        PuzzleBox,
        Toner,


        PressurePad,
        WalkSwitch,
        WfFloorSwitch1,
        WfFloorSwitch2,

        Gift,
        Background,
        Mannequin,
        GateVip,
        GuildItem,
        GuildGate,
        GuildForum,

        Tent,
        TentSmall,
        BadgeDisplay,
        Stacktool,
        Television,

        WiredEffect,
        WiredTrigger,
        WiredCondition,
        WiredCustom,
        Wallpaper,
        Floor,
        Landscape,

        Badge,
        CrackableEgg,
        Effect,
        Deal,

        HorseSaddle1,
        HorseSaddle2,
        HorseHairstyle,
        HorseBodyDye,
        HorseHairDye,
        GnomeBox,
        Bot,
        PurchasableClothing,
        PetBreedingBox,
        Arrow,
        Lovelock,
        MonsterplantSeed,
        Cannon,
        Counter,
        CameraPicture,
        FxProvider
    }


    public class InteractionTypes
    {
        public static InteractionType GetTypeFromString(string pType)
        {
            switch (pType.ToLower())
            {
                case "":
                case "default":
                    return InteractionType.None;
                case "gate":
                    return InteractionType.Gate;
                case "postit":
                    return InteractionType.Postit;
                case "dimmer":
                    return InteractionType.Moodlight;
                case "trophy":
                    return InteractionType.Trophy;
                case "bed":
                    return InteractionType.Bed;
                case "scoreboard":
                    return InteractionType.Scoreboard;
                case "vendingmachine":
                    return InteractionType.VendingMachine;
                case "alert":
                    return InteractionType.Alert;
                case "onewaygate":
                    return InteractionType.OneWayGate;
                case "loveshuffler":
                    return InteractionType.LoveShuffler;
                case "habbowheel":
                    return InteractionType.HabboWheel;
                case "dice":
                    return InteractionType.Dice;
                case "hopper":
                    return InteractionType.Hopper;
                case "bottle":
                    return InteractionType.Bottle;
                case "teleport":
                    return InteractionType.Teleport;
                case "pool":
                    return InteractionType.Pool;
                case "roller":
                    return InteractionType.Roller;
                case "fbgate":
                    return InteractionType.FootballGate;
                case "petbed":
                    return InteractionType.Petbed;
                case "pet":
                    return InteractionType.Pet;
                case "wired_score_board":
                    return InteractionType.WiredScoreBoard;
                case "iceskates":
                    return InteractionType.IceSkates;
                case "rollerskate":
                    return InteractionType.NormalSkates;
                case "lowpool":
                    return InteractionType.Lowpool;
                case "haloweenpool":
                    return InteractionType.Haloweenpool;
                case "ball":
                    return InteractionType.Football;

                case "green_goal":
                    return InteractionType.FootballGoalGreen;
                case "yellow_goal":
                    return InteractionType.FootballGoalYellow;
                case "red_goal":
                    return InteractionType.FootballGoalRed;
                case "blue_goal":
                    return InteractionType.FootballGoalBlue;

                case "green_score":
                    return InteractionType.Footballcountergreen;
                case "yellow_score":
                    return InteractionType.Footballcounteryellow;
                case "blue_score":
                    return InteractionType.Footballcounterblue;
                case "red_score":
                    return InteractionType.Footballcounterred;

                case "bb_blue_gate":
                    return InteractionType.Banzaigateblue;
                case "bb_red_gate":
                    return InteractionType.Banzaigatered;
                case "bb_yellow_gate":
                    return InteractionType.Banzaigateyellow;
                case "bb_green_gate":
                    return InteractionType.Banzaigategreen;
                case "bb_patch":
                    return InteractionType.Banzaifloor;

                case "bb_blue_score":
                    return InteractionType.Banzaiscoreblue;
                case "bb_red_score":
                    return InteractionType.Banzaiscorered;
                case "bb_yellow_score":
                    return InteractionType.Banzaiscoreyellow;
                case "bb_green_score":
                    return InteractionType.Banzaiscoregreen;

                case "banzaicounter":
                    return InteractionType.Banzaicounter;
                case "bb_teleport":
                    return InteractionType.Banzaitele;
                case "banzaipuck":
                    return InteractionType.Banzaipuck;
                case "bb_pyramid":
                    return InteractionType.Banzaipyramid;

                case "freezetimer":
                    return InteractionType.Freezetimer;
                case "freezeexit":
                    return InteractionType.Freezeexit;
                case "freezeredcounter":
                    return InteractionType.Freezeredcounter;
                case "freezebluecounter":
                    return InteractionType.Freezebluecounter;
                case "freezeyellowcounter":
                    return InteractionType.Freezeyellowcounter;
                case "freezegreencounter":
                    return InteractionType.Freezegreencounter;
                case "freezeyellowgate":
                    return InteractionType.FreezeYellowGate;
                case "freezeredgate":
                    return InteractionType.FreezeRedGate;
                case "freezegreengate":
                    return InteractionType.FreezeGreenGate;
                case "freezebluegate":
                    return InteractionType.FreezeBlueGate;
                case "freezetileblock":
                    return InteractionType.FreezeTileBlock;
                case "freezetile":
                    return InteractionType.FreezeTile;

                case "jukebox":
                    return InteractionType.Jukebox;

                case "musicdisc":
                    return InteractionType.MusicDisc;

                case "pressure_pad":
                    return InteractionType.PressurePad;
                case "wf_floor_switch1":
                    return InteractionType.WfFloorSwitch1;
                case "walk_switch":
                    return InteractionType.WalkSwitch;
                case "wf_floor_switch2":
                    return InteractionType.WfFloorSwitch2;
                case "puzzlebox":
                    return InteractionType.PuzzleBox;
                case "water":
                    return InteractionType.Pool;
                case "gift":
                    return InteractionType.Gift;
                case "background":
                    return InteractionType.Background;
                case "mannequin":
                    return InteractionType.Mannequin;
                case "vip_gate":
                    return InteractionType.GateVip;
                case "roombg":
                    return InteractionType.Toner;
                case "gld_item":
                    return InteractionType.GuildItem;
                case "gld_gate":
                    return InteractionType.GuildGate;
                case "guild_forum":
                    return InteractionType.GuildForum;
                case "tent":
                    return InteractionType.Tent;
                case "tent_small":
                    return InteractionType.TentSmall;

                case "badge_display":
                    return InteractionType.BadgeDisplay;
                case "stacktool":
                    return InteractionType.Stacktool;
                case "television":
                    return InteractionType.Television;
                case "wired_custom":
                    return InteractionType.WiredCustom;
                case "wired_effect":
                    return InteractionType.WiredEffect;
                case "wired_trigger":
                    return InteractionType.WiredTrigger;
                case "wired_condition":
                    return InteractionType.WiredCondition;
                case "floor":
                    return InteractionType.Floor;
                case "wallpaper":
                    return InteractionType.Wallpaper;
                case "landscape":
                    return InteractionType.Landscape;

                case "badge":
                    return InteractionType.Badge;

                case "crackable_egg":
                    return InteractionType.CrackableEgg;
                case "effect":
                    return InteractionType.Effect;
                case "deal":
                    return InteractionType.Deal;

                case "horse_saddle_1":
                    return InteractionType.HorseSaddle1;
                case "horse_saddle_2":
                    return InteractionType.HorseSaddle2;
                case "horse_hairstyle":
                    return InteractionType.HorseHairstyle;
                case "horse_body_dye":
                    return InteractionType.HorseBodyDye;
                case "horse_hair_dye":
                    return InteractionType.HorseHairDye;

                case "gnome_box":
                    return InteractionType.GnomeBox;
                case "bot":
                    return InteractionType.Bot;
                case "purchasable_clothing":
                    return InteractionType.PurchasableClothing;
                case "pet_breeding_box":
                    return InteractionType.PetBreedingBox;
                case "arrow":
                    return InteractionType.Arrow;
                case "lovelock":
                    return InteractionType.Lovelock;
                case "cannon":
                    return InteractionType.Cannon;
                case "counter":
                    return InteractionType.Counter;
                case "camera_picture":
                    return InteractionType.CameraPicture;
                case "fx_provider":
                    return InteractionType.FxProvider;
                default:
                {
                    //Logging.WriteLine("Unknown interaction type in parse code: " + pType, ConsoleColor.Yellow);
                    return InteractionType.None;
                }
            }
        }
    }
}