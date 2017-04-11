using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
public class Node : MonoBehaviour
{

    public static int MaxID = 1;

    public int id;

    public Node()
    {
        MaxID++;
        id = MaxID;
    }

    public void Awake()
    {
        SimulationProcessor.Instance.RegisterNode(this);
    }

    public virtual void init()
    {

    }


    public List<Node> connectedNodes = new List<Node>();

}
