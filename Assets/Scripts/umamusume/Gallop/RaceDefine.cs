namespace Gallop
{
    public static class RaceDefine
    {
        public enum PopularityMark
        {
            Left = 0,
            Center = 1,
            Right = 2
        }

        public enum RaceType
        {
            None = 0,
            PvP = 1,
            Tutorial = 2,
            Story = 3,
            StoryCondition = 4,
            Champions = 5,
            Single = 6,
            SingleModeScenarioTeamRace = 7,
            RoomMatch = 8,
            Practice = 9,
            Daily = 10,
            TeamBuilding = 11,
            Legend = 12,
            ChallengeMatch = 13,
            TeamStadium = 14,
            Heroes = 16
        }

        public enum RaceGroup
        {
            None = 0,
            Common = 1,
            Daily = 2,
            TrainingPractice = 3,
            StoryCondition = 4,
            TeamBuilding = 5,
            Practice = 6,
            Single = 7,
            Legend = 8,
            TeamStadium = 9,
            SingleModeScenarioTeamRace = 10,
            Champions = 11,
            RoomMatch = 12,
            ChallengeMatch = 13,
            Heroes = 14,
            CustomG1 = 61
        }

        public enum CourseDistanceType
        {
            Short = 1,
            Mile = 2,
            Middle = 3,
            Long = 4
        }

        public enum TeamRaceNumberType
        {
            Short = 1,
            Mile = 2,
            Middle = 3,
            Long = 4,
            Dirt = 5
        }

        public enum RaceState
        {
            Null = -1,
            Init = 0,
            MemberList = 1,
            GateIn = 2,
            WaitStartDash = 3,
            Race = 4,
            OverRun = 5,
            OverRunResult = 6,
            WinningCircleInit = 7,
            WinningCircle = 8,
            End = 9
        }

        public enum RaceNpcType
        {
            Normal = 0,
            Boss = 3,
            User = 11,
            UserOther = 20,
            Ghost = 21,
            Follow = 22
        }

        public enum Grade
        {
            None = 0,
            Grade1 = 100,
            Grade2 = 200,
            Grade3 = 300,
            Grade4 = 400,
            Grade5 = 500,
            Grade6 = 600,
            Grade7 = 700,
            Grade8 = 800,
            Grade9 = 900,
            SpecialGrade = 1000,
            G1 = 100,
            G2 = 200,
            G3 = 300,
            Open = 400,
            U_1600 = 500,
            U_1000 = 600,
            PreOpen = 700,
            NoWin = 800,
            NewHorses = 900,
            Legend = 100,
            Orion = 200,
            Cassiopeia = 300,
            Aries = 400,
            Varugo = 500,
            Consolation = 800,
            Beginner = 900,
            GUR = 1000,
            Min = 900
        }

        public enum Difficulty
        {
            Easy = 1,
            Normal = 2,
            Hard = 3,
            VeryHard = 4,
            Extreme = 5
        }

        public enum StraightFrontType
        {
            Null = 0,
            Front = 1,
            AcrossFront = 2
        }

        public enum GroundType
        {
            Turf = 1,
            Dirt = 2,
            Max = 2
        }

        public enum GroundCondition
        {
            Good = 1,
            Soft = 2,
            Hard = 3,
            Bad = 4
        }

        public enum Weather
        {
            None = 0,
            Sunny = 1,
            Cloudy = 2,
            Rainy = 3,
            Snow = 4,
            Max = 5,
            Min = 0
        }

        public enum Time
        {
            None = 0,
            Morning = 1,
            Daytime = 2,
            Evening = 3,
            Night = 4,
            Max = 5,
            Min = 0
        }

        public enum CourseAround
        {
            None = 1,
            Inner = 2,
            Outer = 3,
            OuterToInner = 4
        }

        public enum Rotation
        {
            Right = 1,
            Left = 2,
            StraightRight = 3,
            StraightLeft = 4
        }

        public enum Audience
        {
            Few = 1,
            Normal = 2,
            SoldOut = 3
        }

        public enum Category
        {
            Course = 0,
            Gate = 1,
            TurfGoal = 2,
            DirtGoal = 3,
            TurfGoalFlower = 4,
            DirtGoalFlower = 5,
            Tree = 6,
            Audience = 7,
            Flag = 8,
            Sky = 9,
            Car = 10
        }

        public enum GoalGateType
        {
            Common = 0
        }

        public enum RunningStyle
        {
            None = 0,
            Nige = 1,
            Senko = 2,
            Sashi = 3,
            Oikomi = 4
        }

        public enum RunningStyleEx
        {
            None = 0,
            Oonige = 1
        }

        public enum ProperExpCategory
        {
            None = 0,
            Ground = 1,
            RunningStyle = 2,
            Distance = 3
        }

        public enum ProperGrade
        {
            Null = 0,
            G = 1,
            F = 2,
            E = 3,
            D = 4,
            C = 5,
            B = 6,
            A = 7,
            S = 8
        }

        public enum ProperType
        {
            Distance = 1,
            Ground = 2,
            RunningStyle = 3
        }

        public enum DefeatType
        {
            Null = 0,
            Win = 1,
            Lose = 2,
            RunningStyleMany = 3,
            Temptaion = 4,
            GutsOrder = 5,
            Stamina = 6,
            LastSpurtFalse = 7,
            LastSpurtTargetSpeedDec = 8,
            PassiveSkillNum = 9,
            BlockFrontTime = 10,
            Speed = 11,
            ProperDistance = 12,
            ProperGround = 13,
            Motivation = 14
        }

        public enum HorseLength
        {
            Nose = 0,
            Head = 1,
            Neck = 2,
            Half = 3,
            ThreeQuarters = 4,
            One = 5,
            OneAndOneQuarters = 6,
            OneAndHalf = 7,
            OneAndThreeQuarters = 8,
            Two = 9,
            TwoAndHalf = 10,
            Three = 11,
            ThreeAndHalf = 12,
            Four = 13,
            Five = 14,
            Six = 15,
            Seven = 16,
            Eight = 17,
            Nine = 18,
            Ten = 19,
            OverTen = 20
        }

        public enum CoursePathType
        {
            NormalRace = 0,
            StoryRace = 1
        }

        public enum BGMControlMode
        {
            Null = 0,
            Normal = 1
        }

        public enum RaceBGMConditionType
        {
            Null = -1,
            Heroes1stStage = 1,
            HeroesFinalStage = 10
        }

        public enum SlopeType
        {
            Null = 0,
            Up = 1,
            Down = 2
        }

        public enum QualityType
        {
            Quality3D_Light = 0,
            Quality3D = 1,
            Quality3D_Rich = 2,
            Quality_Unknown = -1
        }

        public enum CutInPlayMode
        {
            LongOnceADay = 0,
            Long = 1,
            Short = 2
        }

        public enum Motivation
        {
            None = 0,
            Min = 1,
            Low = 2,
            Middle = 3,
            High = 4,
            Max = 5
        }

        public enum UnlockFlag
        {
            Status = 1,
            ConservePower = 2
        }

        public enum PositionKeepMode
        {
            Null = 0,
            SpeedUp = 1,
            Overtake = 2,
            PaseUp = 3,
            PaseDown = 4,
            PaseUpEx = 5
        }

        public enum EventCameraCategory
        {
            Jikkyo = 1,
            SkillCutIn = 2,
            Temp01 = 3,
            Goal = 4
        }

        public enum HorseRelativePos
        {
            Null = 0,
            Left = 1,
            Right = 2
        }

        public enum TurfVisionType
        {
            URA = 1,
            NAU = 2,
            Stand = 3
        }

        public enum ResultBoardConditionType
        {
            Turf_None = 1,
            Turf_Dirt = 2,
            Dirt_None = 3,
            Dirt_Turf = 4
        }

        public enum EventType
        {
            None = 0,
            LegendRace = 1
        }

        public enum RaceTrackFootSmokeColorType
        {
            LightMap = 1,
            LightProbeOnlyNight = 2
        }

        public enum SEPriority
        {
            ResidentCrowd = 1,
            GroupFoot = 2,
            HighFoot = 3,
            CourseEnvironment = 4,
            EventCrowd = 5,
            StoryRaceVoice = 6,
            CourseGateOpen = 7,
            StoryRaceTimeline = 8,
            Score = 9,
            SkillCutIn = 10,
            MessagePlate = 11,
            Skill = 13,
            Foot = 15,
            Cloth = 20
        }

        public enum CharaColorType
        {
            Null = 0,
            Black = 1
        }

        public enum MainStoryShowSkillType
        {
            Null = 0,
            Show = 1,
            ShowGimmickMatch = 2,
            ShowGimmickUnmatch = 3
        }

        public enum LastSpurtEvaluateValue
        {
            False = 0,
            True = 1,
            TrueExceedNeedMaxHp = 2,
            Max = 2
        }

        public const int FINAL_CORNER_END_ORDER_NULL = -1;
        public const int HORSE_INDEX_NULL = -1;
        public const int TEAM_STADIUM_MEMBER_MAX = 3;
        public const int POPULARITY_ICON_MAX = 5;
        public const int SCREEN_VERTICAL = 0;
        public const int SCREEN_LANDSCAPE = 1;
        public const string POPULAR_ICON_NAME = "utx_ico_racepopularity_{0:D2}";
        public const int RANK_1ST = 1;
        public const int RANK_2ND = 2;
        public const int RANK_3RD = 3;
        public const int RANK_4TH = 4;
        public const int RANK_5TH = 5;
        public const int RANK_6TH = 6;
        public const int COURSE_SECTION_NUM = 24;
        public const float HORSE_LENGTH_DISTANCE_ONE = 2.5f;
        public const float HorseLength_One = 1;
        public const float HorseLength_Three = 3;
        public const float HorseDistance2LaneCoef = 18;
        public const float HorseLane2DistanceCoef = 0.055555556f;
        public const float HorseLane_Quarters = 0.013888889f;
        public const float HorseLane_Harf = 0.027777778f;
        public const float HorseLane_ThreeQuarters = 0.041666668f;
        public const float HorseLane_One = 0.055555556f;
        public const float HorseLane_OneAndOneQuarters = 0.06944445f;
        public const float HorseLane_OneAndHarf = 0.083333336f;
        public const float HorseLane_OneAndThreeQuarters = 0.097222224f;
        public const float HorseLane_Two = 0.11111111f;
        public const float HorseLane_Three = 0.16666667f;
        public const float HorseLane_Four = 0.22222222f;
        public const float HorseLane_Five = 0.2777778f;
        public const float HorseLane_Six = 0.33333334f;
        public const float HorseLane_Seven = 0.3888889f;
        public const float HorseLane_Eight = 0.44444445f;
        public const float HorseLane_Nine = 0.5f;
        public const float HorseLane_Ten = 0.5555556f;
        public const int RACE_HORSE_MAX = 18;
        public const float COURSE_SCALE = 0.625f;
        public const float COURSE_WIDTH = 11.25f;
        public const float ONE_FURONG_DISTANCE = 200;
        public const float ONE_FURONG_DISTANCE_INV = 0.005f;
        public const int POST_NUMBER_MIN = 1;
        public const int POST_NUMBER_MAX = 8;
        public const int COMPETE_GROUP_NULL = -1;
    }
}
