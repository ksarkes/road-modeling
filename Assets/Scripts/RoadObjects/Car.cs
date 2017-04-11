using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    public int id;
    const int NO_OBSTACLE = 0;
    const int TRAFIC_LIGHT_CLOSED = -1;
    private List<Edge> path;
    private int currentEdge;
    private int cellNum;
    // cell/s
    private int velocity = 0;

    private void Start()
    {
        SimulationProcessor.Instance.OnCarCreate(this);
    }

    private void OnDestroy()
    {
        SimulationProcessor.Instance.OnCarDestroy(this);
    }

    public void DoStep()
    {
        // Acceleration
        velocity += 1;

        // Braking
        int stepsLeft = velocity;
        var obsEgde = path[currentEdge];
        int curCell = cellNum;
        int loc = currentEdge;
        bool hasObstacle = false;
        while(stepsLeft > 0)
        {
            stepsLeft--;
            curCell++;
            if(obsEgde.cells.Count == curCell)
            {
                curCell = 0;
                loc++;
                if (loc == path.Count)
                    break;
            }

            if(obsEgde.cells[curCell] != NO_OBSTACLE)
            {
                hasObstacle = true;
                break;
            }

        }

        if (hasObstacle)
            velocity -= stepsLeft + 1;
        
        // move
        path[currentEdge].cells[cellNum] = NO_OBSTACLE;
        currentEdge = loc;
        cellNum = curCell;
        if (currentEdge == path.Count)
            Destroy(this);
        else
            path[currentEdge].cells[cellNum] = id; 


    }
}
