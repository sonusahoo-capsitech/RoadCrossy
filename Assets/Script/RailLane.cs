using UnityEngine;

namespace Gamewise.crossyroad
{
public class RailLane : MonoBehaviour
{
    public float trainDelay;

    public void Init(float delay)
    {
        trainDelay = delay;
    }
}
}