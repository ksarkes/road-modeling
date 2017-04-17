using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge {

    public static int maxId = 1;

    public int id;

    public TrafficLight start;
    public TrafficLight finish;

    // элемент cells айдишник кара 
    public List<int> cells = new List<int>();

    public Edge()
    {
        for (int i = 0; i < Constants.DIV_CELLS_NUM; i++)
        {
            cells.Add(Constants.NO_CAR);
        }
    }

    public Edge(TrafficLight start, TrafficLight finish)
    {
        this.start = start;
        this.finish = finish;

        id = maxId;
        maxId++;

        for (int i = 0; i < Constants.DIV_CELLS_NUM; i++)
        {
            cells.Add(Constants.NO_CAR);
        }

    }

    public bool HasObstacle(int cellNum)
    {
        return cells[cellNum] != Constants.NO_CAR
            || cellNum == 0 && !start.isOpen()
            || cellNum == cells.Count - 1 && !finish.isOpen(); 
    }
}
