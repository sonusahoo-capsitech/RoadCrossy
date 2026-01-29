using System;



namespace Gamewise.crossyroad
{
    [Serializable]
    public class LaneData
    {
        public string type;

        // Road
        public float carSpeed;
        public float spawnRate;

        // River
        public float logSpeed;
        public int gap;

        // Rail
        public float trainDelay;
    }
}
