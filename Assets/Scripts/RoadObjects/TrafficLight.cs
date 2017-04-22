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

    public bool isJustNode = false;

    // Positions in init matrix
    public int i;
    public int j;

    public Transform openIndicator;

    private Quaternion positiveRotation;
    private Quaternion negativeRotation;


    private void Start()
    {
        positiveRotation = Quaternion.Euler(Vector3.zero);
        negativeRotation = Quaternion.Euler(0, 0, 90);
    }

    public void StartSwitchActions()
    {
        new System.Threading.Thread(() =>
        {
            while (true)
            {
                System.Threading.Thread.Sleep(5000);
                Switch();
            }
        }).Start();
    }

    public void Update()
    {
        if (open)
            openIndicator.rotation = negativeRotation;
        else
            openIndicator.rotation = positiveRotation;
    }

    [System.Obsolete]
    public void TrySwitch()
    {
        if (SimulationProcessor.Instance.currentTimeStep - lastSwitchTime > (long)Constants.TIME_STEPS_PER_FRAME * 300)
        {
            Switch();
            Debug.Log(SimulationProcessor.Instance.currentTimeStep - lastSwitchTime);
            lastSwitchTime = SimulationProcessor.Instance.currentTimeStep;
        }
    }

    private void Switch()
    {

        open = !open;
    }

    public bool isOpen(Node other)
    {
        if (isJustNode)
            return true;
        if ((int)other.transform.position.x == (int)transform.position.x)
            return open;
        else
            return !open;
    }
}
