using UnityEngine;

namespace Gamewise.crossyroad
{
    public class RiverLane : MonoBehaviour
    {
        public float logSpeed;
        public int gap;

        public void Init(float speed, int g)
        {
            logSpeed = speed;
            gap = g;
        }
    }
}