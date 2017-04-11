using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : Node {
    List<Edge> positiveEdges;
    List<Edge> negativeEdges;
    public int stepsBetweenTurns;
    private bool state = false;
    private long lastSwitchTime = 0;

    public void TrySwitch()
    {
        if (SimulationProcessor.Instance.currentTimeStep - lastSwitchTime > (long)stepsBetweenTurns)
        {
            Switch();
            lastSwitchTime = SimulationProcessor.Instance.currentTimeStep;
        }
    }
    private void Switch()
    {
        state = !state;
    }

}
