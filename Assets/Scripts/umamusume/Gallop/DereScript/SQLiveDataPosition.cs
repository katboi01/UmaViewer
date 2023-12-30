public class SQLiveDataPosition
{
    public int liveDataID;
    public int positionNum;

    public int charaPosition1;
    public int dressPosition1;
    public int charaPosition2;
    public int dressPosition2;
    public int charaPosition3;
    public int dressPosition3;
    public int charaPosition4;
    public int dressPosition4;
    public int charaPosition5;
    public int dressPosition5;

    public int[] CharaPositions
    {
        get
        {
            if (positionNum > 0)
            {
                int[] array = new int[5];
                array[0] = charaPosition4;
                array[1] = charaPosition2;
                array[2] = charaPosition1;
                array[3] = charaPosition3;
                array[4] = charaPosition5;
                return array;
            }
            else
            {
                return new int[0];
            }
        }
    }
    public int[] DressPositions
    {
        get
        {
            if (positionNum > 0)
            {
                int[] array = new int[5];
                array[0] = dressPosition4;
                array[1] = dressPosition2;
                array[2] = dressPosition1;
                array[3] = dressPosition3;
                array[4] = dressPosition5;
                return array;
            }
            else
            {
                return new int[0];
            }
        }
    }


    static public SQLiveDataPosition GetChihiro201604()
    {
        SQLiveDataPosition data = new SQLiveDataPosition();
        data.liveDataID = 901;
        data.positionNum = 1;
        data.charaPosition1 = 20;
        data.dressPosition1 = 900001;

        return data;
    }

    static public SQLiveDataPosition GetPutiderera201704()
    {
        SQLiveDataPosition data = new SQLiveDataPosition();
        data.liveDataID = 902;
        data.positionNum = 3;
        data.charaPosition1 = 48;
        data.dressPosition1 = 900009;
        data.charaPosition2 = 49;
        data.dressPosition2 = 900011;
        data.charaPosition3 = 50;
        data.dressPosition3 = 900013;

        return data;
    }

    static public SQLiveDataPosition GetKirarinLobo201804()
    {
        SQLiveDataPosition data = new SQLiveDataPosition();
        data.liveDataID = 903;
        data.positionNum = 4;
        data.charaPosition1 = 83;
        data.dressPosition1 = 900019;
        data.charaPosition2 = 83;
        data.dressPosition2 = 900021;
        data.charaPosition3 = 271;
        data.dressPosition3 = 300467;//3002,3602
        data.charaPosition4 = 134;
        data.dressPosition4 = 100479;//1002,1602

        return data;
    }

    static public SQLiveDataPosition GetNono201904()
    {
        SQLiveDataPosition data = new SQLiveDataPosition();
        data.liveDataID = 907;
        data.positionNum = 3;
        data.charaPosition1 = 226;
        data.dressPosition1 = 200331;
        data.charaPosition2 = 162;
        data.dressPosition2 = 100607;
        data.charaPosition3 = 256;
        data.dressPosition3 = 300577;

        return data;
    }
}

