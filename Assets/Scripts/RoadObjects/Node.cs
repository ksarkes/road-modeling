using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int id;

    public void Awake()
    {
        //SimulationProcessor.Instance.RegisterNode(this);
    }

    public virtual void init()
    {

    }


    public List<TrafficLight> connectedNodes = new List<TrafficLight>();

}
