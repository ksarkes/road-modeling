using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{

    public int id;

    public List<Edge> path;
    
    public int curEdgeNumInPath = 0;
    public int cellNum = 0;

    // cell/s
    public int velocity = 0;

    private void Start()
    {
        SimulationProcessor.Instance.OnCarCreate(this);
        path = SimulationProcessor.Instance.GetCarPath(null);
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
