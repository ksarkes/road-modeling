using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{

    public int id;

    public List<Edge> path;
    public Node start;
    
    public int curEdgeNumInPath = 0;
    public int cellNum = 0;

    public bool toRemove = false;

    // cell/s
    public int velocity = 0;

    private void Start()
    {
        path = SimulationProcessor.Instance.GetCarPath(start);
        SimulationProcessor.Instance.OnCarCreate(this);
    }

    private void OnDestroy()
    {
        SimulationProcessor.Instance.OnCarDestroy(this);
    }

    public int GetCurrentEdgeId()
    {
        return path[curEdgeNumInPath].id;
    }

    public int GetEdgeIdByPathNum(int num)
    {

        return path[num].id;
    }
    
}
