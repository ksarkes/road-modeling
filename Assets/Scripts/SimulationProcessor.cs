using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationProcessor : MonoBehaviour
{
    public static SimulationProcessor Instance;

    private int timeStepsPerFrame = 3;
    public long currentTimeStep = 0;

    public GameObject linePrefab;
    public Transform linesParent;
    public Car carPrefab;
    public Transform carParent;

    private List<CarGenerator> generators = new List<CarGenerator>();
    private List<TrafficLight> lights = new List<TrafficLight>();

    public List<Node> nodes = new List<Node>();
    private HashSet<Car> cars = new HashSet<Car>();
    public Dictionary<int, Edge> edgesMap = new Dictionary<int, Edge>();
    public Dictionary<int, Car> carsMap = new Dictionary<int, Car>();

    public List<Node> way = new List<Node>();

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
        carsMap.Add(car.id, car);
    }

    public void OnCarDestroy(Car car)
    {
        cars.Remove(car);
        carsMap.Remove(car.id);
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

        foreach (var j in generators)
            j.TryGenerate();
    }

    private void Update()
    {
        for (int i = 0; i < timeStepsPerFrame; i++)
        {
            CalculateStep();
        }
    }

    private void CalculateStep()
    {
        currentTimeStep++;
        //foreach (var j in generators)
        //    j.TryGenerate();
        //foreach (var j in lights)
        //    j.TrySwitch();
        foreach (var j in cars)
            DoStep(j);
    }


    private void DoStep(Car car)
    {

        // Acceleration
        if (car.velocity < Constants.SPEED_LIMIT)
            car.velocity += 1;

        // Braking
        int brakingLength = IntPow(car.velocity, 2);
        int stepsLeft = car.velocity + brakingLength;

        var obsEgde = edgesMap[car.GetCurrentEdgeId()];
        int curEdgeNum = car.curEdgeNumInPath;
        int curCellNum = car.cellNum;

        bool hasObstacle = false;

        while (stepsLeft > 0)
        {
            stepsLeft--;
            curCellNum++;

            if (curCellNum == obsEgde.cells.Count)
            {
                curEdgeNum++;
                curCellNum = 1; // 1 ибо 0 = светофор, который только что проехали

                // Достигли конца пути и не нашли препятствий
                if (curEdgeNum == car.path.Count)
                    break;

                obsEgde = edgesMap[car.GetEdgeIdByPathNum(curEdgeNum)];
            }

            if (obsEgde.HasObstacle(curCellNum))
            {
                hasObstacle = true;
                break;
            }

        }

        if (hasObstacle)
            car.velocity -= stepsLeft + 1;

        // Move
        // Чистим за собой клетку
        edgesMap[car.GetCurrentEdgeId()].cells[car.cellNum] = Constants.NO_CAR;
        int cellsToMove = car.velocity;
        while (cellsToMove > 0)
        {
            cellsToMove--;
            car.cellNum++;
            if (car.cellNum == car.path.Count)
            {
                car.cellNum = 1;
                car.curEdgeNumInPath++;
                if (car.curEdgeNumInPath == car.path.Count)
                {
                    Destroy(car);
                }
            }
        }

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

    int IntPow(int x, uint pow)
    {
        int ret = 1;
        while (pow != 0)
        {
            if ((pow & 1) == 1)
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }
}
