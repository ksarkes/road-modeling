﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge {

    public static int maxId = 1;

    public int id;

    public int cellsNum;

    public TrafficLight start;
    public TrafficLight finish;

    // элемент cells айдишник кара 
    public List<int> cells = new List<int>();

    public List<int> cellsModif = new List<int>();

    public Edge(TrafficLight start, TrafficLight finish)
    {
        this.start = start;
        this.finish = finish;

        id = maxId;
        maxId++;

        double len = (Vector3.Magnitude(new Vector3(start.transform.position.x - finish.transform.position.x,
            start.transform.position.y - finish.transform.position.y)))/ Constants.DST_MULT;

        cellsNum = (int) (Constants.METERS_PER_EDGE * len);

        for (int i = 0; i < cellsNum; i++)
            cells.Add(Constants.NO_CAR);

        for (int i = 0; i < cellsNum; i++)
        {
            if (start.id == 1 && finish.id == 2 && i > cellsNum / 2)
                cellsModif.Add(Constants.ALLOWED_SPEED_SIGN);
            else
                cellsModif.Add(Constants.NO_MODIF);
        }
    }

    public bool HasObstacle(int cellNum)
    {
        // cellNum == 0 никогда не происходит
        return cells[cellNum] != Constants.NO_CAR
            //|| cellNum == 0 && !start.isOpen(this)
            || cellNum == cells.Count - 1 && !finish.isOpen(start);
    }

    public bool HasObstacleUntil(int cellNum)
    {
        for (int i = 0; i <= cellNum; i++)
        {
            if (cells[i] != Constants.NO_CAR
            //|| i == 0 && !start.isOpen(this)
            || i == cells.Count - 1 && !finish.isOpen(start))
                return true;
        }
        return false;
    }

    public int SpeedLimit(int cellNum)
    {
        return cellsModif[cellNum];
    }
}
