using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationProcessor : MonoBehaviour
{

    public GameObject linePrefab;
    public Transform linesParent;
    public List<Node> nodes = new List<Node>();
    private List<CarGenerator> generators = new List<CarGenerator>();
    private List<TrafficLight> lights = new List<TrafficLight>();
    private HashSet<Car> cars = new HashSet<Car>();
    public static SimulationProcessor Instance;
    private int TimeStepsPerFrame = 3;
    public Car carPrefab;
    public Transform carParent;

    public List<Node> way = new List<Node>();

    public long currentTimeStep = 0;

    private SimulationProcessor()
    {
        Instance = this;
    }

    public void RegisterNode(Node node)
    {
        nodes.Add(node);
    }

    public void OnCarCreate(Car car)
    {
        cars.Add(car);
    }

    public void OnCarDestroy(Car car)
    {
        cars.Remove(car);
    }

    private void DrawLine(Vector3 position1, Vector3 position2, float alpha = 1.0f, bool curLayer = true)
    {
        Vector3 differenceVector = position1 - position2;
        var newLine = (GameObject)Instantiate(linePrefab);
        var lr = newLine.GetComponent<LineRenderer>();
        position1.z = 1;
        position2.z = 1;
        List<Vector3> positions = new List<Vector3>() { position1, position2 };
        lr.SetPositions(positions.ToArray());
        if (linesParent != null)
            newLine.transform.parent = linesParent;
    }

    void Start()
    {
        foreach (var start in nodes)
        {
            start.init();
            foreach (var finish in start.connectedNodes)
                DrawLine(start.transform.position, finish.transform.position);
            if (start is TrafficLight)
                lights.Add(start as TrafficLight);
            else if (start is CarGenerator)
                generators.Add(start as CarGenerator);
        }
    }

    private void Update()
    {
        for (int i = 0; i < TimeStepsPerFrame; i++)
        {
            CalculateStep();
        }
    }

    private void CalculateStep()
    {
        currentTimeStep++;
        foreach (var j in generators)
            j.TryGenerate();
        foreach (var j in lights)
            j.TrySwitch();
        foreach (var j in cars)
            j.DoStep();
    }

    public List<Node> GetCarPath(Node start)
    {
        //Node finish = generators[0];
        //if (start == finish)
        //    finish = generators[1];

        return way;


    }
    //public List<Node> DFS(Node start, Node target)
    //{


    //    /*
    //    Stack<Node> path = new Stack<Node>();
    //    Stack<Node> observedNodes = new Stack<Node>();
    //    observedNodes.Push(start);

    //    // List<Node> result = new List<Node>();

    //    while (observedNodes.Any())
    //    {
    //        var node = observedNodes.Pop();

    //        if (node == target)
    //            return path;

    //        foreach (var child in node.connectedNodes)
    //        {
    //            if (!observedNodes.Contains(child))
    //                observedNodes.Push(child);

    //        }
    //    }

    //    Stack<Node<K, V>> stack = new Stack<Node<K, V>>();

    //    while (stack.Any())
    //    {
    //        var node = stack.Pop();

    //        if (node.key == key)
    //        {
    //            return node.value;
    //        }
    //        foreach (var child in node.children)
    //        {
    //            stack.Push(child);
    //        }
    //    } */
    //}   

}
