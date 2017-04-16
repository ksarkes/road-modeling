﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
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


    public List<Node> connectedNodes = new List<Node>();

}
