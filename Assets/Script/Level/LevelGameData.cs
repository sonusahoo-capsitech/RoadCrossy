using System;
using System.Collections.Generic;



namespace Gamewise.crossyroad
{
    [Serializable]
    public class LevelData
    {
        public int levelId;
        public int gridWidth;
        public int timeLimit;
        public List<LaneData> lanes;
    }
}