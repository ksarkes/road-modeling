using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : Node
{
    public List<Edge> positiveEdges = new List<Edge>();
    public List<Edge> negativeEdges = new List<Edge>();

    private int stepsBetweenTurns = 200;
    private long lastSwitchTime = 0;

    public bool open = false;

    public void TrySwitch()
    {
        if (SimulationProcessor.Instance.currentTimeStep - lastSwitchTime > (long) Constants.TIME_STEPS_PER_FRAME * 300)
        {
            Switch();
            lastSwitchTime = SimulationProcessor.Instance.currentTimeStep;
        }
    }

    private void Switch()
    {
        open = !open;
    }

    public bool isOpen(Node other)
    {
        if ((int)other.transform.position.x == (int)transform.position.x)
            return open;
        else
            return !open;
    }
}
