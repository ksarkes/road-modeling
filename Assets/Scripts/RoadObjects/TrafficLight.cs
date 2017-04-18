using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : Node
{
    List<Edge> positiveEdges;
    List<Edge> negativeEdges;

    private int stepsBetweenTurns = 100;
    private long lastSwitchTime = 0;

    private bool open = false;

    public void TrySwitch()
    {
        if (SimulationProcessor.Instance.currentTimeStep - lastSwitchTime > (long) Constants.TIME_STEPS_PER_FRAME * 100)
        {
            Switch();
            lastSwitchTime = SimulationProcessor.Instance.currentTimeStep;
        }
    }

    private void Switch()
    {
        open = !open;
    }

    public bool isOpen()
    {
       // return true;
        return open;
    }
}
