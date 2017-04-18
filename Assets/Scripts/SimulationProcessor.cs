using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationProcessor : MonoBehaviour
{
    public static SimulationProcessor Instance;
    public long currentTimeStep = 0;
    public int N = 15;
    public int M = 15;
    public int carsNum = 200;

    public List<Node> nodes = new List<Node>();
    public List<Edge> edges = new List<Edge>();
    private HashSet<Car> cars = new HashSet<Car>();
    public Dictionary<int, Edge> edgesMap = new Dictionary<int, Edge>();
    public Dictionary<int, Car> carsMap = new Dictionary<int, Car>();
    private Dictionary<int, List<Edge>> nodeIncEdges = new Dictionary<int, List<Edge>>();

    public List<Node> way = new List<Node>();

    [Header("Parents")]
    public Transform linesParent;
    public Transform carParent;
    public Transform lightsParent;

    [Header("PREFABS")]
    [SerializeField]
    private TrafficLight trafficLightPrefab;
    [SerializeField]
    private CarGenerator generatorPrefab;
    public Car carPrefab;
    public GameObject linePrefab;

    private List<Edge> edgeway;
    private List<CarGenerator> generators = new List<CarGenerator>();
    private List<TrafficLight> lights = new List<TrafficLight>();

    private int maxCarId = 0;

    private int[,] posMatrix;
    private int[,] adjMatrix;

    public Dictionary<int, TrafficLight> lightsMap = new Dictionary<int, TrafficLight>();

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
        maxCarId++;
        car.id = maxCarId;
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

    private void GenerateGraph()
    {
        var generator = new GraphGenerator(N, M);
        generator.GenerateGraph(out posMatrix, out adjMatrix);
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                if (posMatrix[i, j] > 0)
                    CreateTrafficLight(j, i, posMatrix[i, j]);
    }

    private TrafficLight CreateTrafficLight(int x, int y, int id)
    {
        TrafficLight newLight;
        newLight = Instantiate(trafficLightPrefab);
        //trafficLightPrefab.transform.position = new Vector3(x - posMatrix.GetLength(1) / 2, y - posMatrix.GetLength(0) / 2);
        newLight.transform.position = new Vector3(x * Constants.DST_MULT, -y * Constants.DST_MULT);
        newLight.transform.parent = lightsParent;
        newLight.id = id;
        lightsMap.Add(newLight.id, newLight);
        return newLight;
    }

    private void ConnectNodes()
    {
        foreach (var i in lightsMap.Values)
        {
            for (int j = 1; j < adjMatrix.GetLength(1); j++)
            {

                if (adjMatrix[i.id, j] > 0)
                {
                    i.connectedNodes.Add(lightsMap[j]);
                    var edge = new Edge(i, lightsMap[j]);
                    edgesMap.Add(edge.id, edge);

                    if (!nodeIncEdges.ContainsKey(i.id))
                        nodeIncEdges.Add(i.id, new List<Edge>());
                    nodeIncEdges[i.id].Add(edge);

                    //edge = new Edge(lightsMap[j], i);
                    //edgesMap.Add(edge.id, edge);
                }
            }
        }
    }

    private void DrawLines()
    {
        foreach (var i in lightsMap.Values)
        {
            foreach (var j in i.connectedNodes)
            {
                DrawLine(i.transform.position, j.transform.position);
            }
        }
    }

    void Start()
    {
        GenerateGraph();
        ConnectNodes();

        GraphGenerator.PrintMatrix(adjMatrix);
        DrawLines();

        for (int i = 0; i < carsNum; i++)
        {
            var start = GetRandomNode();
            // TODO: синхронизовать GetRandomNode и GetRandomEdge
            System.Threading.Thread.Sleep(10);
            var newCar = (Car)Instantiate(carPrefab);

            newCar.transform.parent = carParent;
            newCar.start = start;
            newCar.transform.position = start.transform.position;
        }
    }

    private long LastGenerationTime = 0;

    private void Update()
    {
        //if (currentTimeStep - LastGenerationTime == 5)
        //{
        //    var newCar = Instantiate(carPrefab);
        //    newCar.transform.parent = carParent;
        //    newCar.transform.position = transform.position;

        //    cars.Add(newCar);
        //}
        for (int i = 0; i < Constants.TIME_STEPS_PER_FRAME; i++)
        {
            CalculateStep();
        }

        foreach (var j in lightsMap.Values)
            j.TrySwitch();
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

        if (car.toRemove)
            return;

        // Acceleration
        if (car.velocity < Constants.SPEED_LIMIT)
            car.velocity += 1;
        else
            car.velocity = Constants.SPEED_LIMIT;

        //System.Random rand = new System.Random();
        //int r = Constants.SPEED_LIMIT / 3 / rand.Next(1, Constants.SPEED_LIMIT + 1);
        //car.velocity += r;

        // Braking
        //int brakingLength = IntPow(car.velocity, 2);
        int stepsLeft = car.velocity * 3;//+ brakingLength;

        var obsEgde = edgesMap[car.GetCurrentEdgeId()];
        int curEdgeNum = car.curEdgeNumInPath;
        int curCellNum = car.cellNum + Constants.HALF_CAR_SIZE;

        bool hasObstacle = false;

        while (stepsLeft > 0)
        {
            stepsLeft--;
            curCellNum++;

            if (curCellNum >= obsEgde.cells.Count)
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
            car.velocity -= ((stepsLeft)/3);

        // Move
        // Чистим за собой клетку
        edgesMap[car.GetCurrentEdgeId()].cells[car.cellNum] = Constants.NO_CAR;

        for (int i = car.cellNum; i <= car.cellNum + Constants.HALF_CAR_SIZE && i < edgesMap[car.GetCurrentEdgeId()].cells.Count; i++)
            edgesMap[car.GetCurrentEdgeId()].cells[i] = Constants.NO_CAR;

        for (int i = car.cellNum; i >= car.cellNum - Constants.HALF_CAR_SIZE && i >= 0; i--)
            edgesMap[car.GetCurrentEdgeId()].cells[i] = Constants.NO_CAR;

        int cellsToMove = car.velocity;
        while (cellsToMove > 0)
        {
            cellsToMove--;
            car.cellNum++;
            if (car.cellNum >= car.path[car.curEdgeNumInPath].cellsNum)
            {
                car.cellNum = 1;
                car.curEdgeNumInPath++;
                if (car.curEdgeNumInPath >= car.path.Count)
                {
                    Destroy(car.gameObject);
                    car.toRemove = true;
                    return;
                }
            }
        }

        var curEdgeId = car.GetCurrentEdgeId();
        //edgesMap[curEdgeId].cells[car.cellNum] = Constants.CAR_OBSTACLE;

        for (int i = car.cellNum; i < car.cellNum + Constants.HALF_CAR_SIZE && i < edgesMap[curEdgeId].cells.Count; i++)
            edgesMap[curEdgeId].cells[i] = Constants.CAR_OBSTACLE;

        for (int i = car.cellNum; i >= car.cellNum + Constants.HALF_CAR_SIZE && i >= 0; i--)
            edgesMap[curEdgeId].cells[i] = Constants.CAR_OBSTACLE;

        var newpos = Vector3.Lerp(edgesMap[curEdgeId].start.transform.position,
            edgesMap[curEdgeId].finish.transform.position,
             (float)car.cellNum / (float)edgesMap[curEdgeId].cells.Count);

        if (edgesMap[curEdgeId].finish.transform.position.y != edgesMap[curEdgeId].start.transform.position.y)
        {
            if (edgesMap[curEdgeId].finish.transform.position.y > edgesMap[curEdgeId].start.transform.position.y)
                newpos.x += Constants.CAR_OFFSET;
            else
                newpos.x -= Constants.CAR_OFFSET;
        }else if (edgesMap[curEdgeId].finish.transform.position.x != edgesMap[curEdgeId].start.transform.position.x)
        {
            if (edgesMap[curEdgeId].finish.transform.position.x > edgesMap[curEdgeId].start.transform.position.x)
                newpos.y -= Constants.CAR_OFFSET;
            else
                newpos.y += Constants.CAR_OFFSET;
        }
        car.transform.position = newpos;
        var angle = Vector3.Angle(new Vector3(0, 1) - new Vector3(0, 0),
            edgesMap[curEdgeId].finish.transform.position - edgesMap[curEdgeId].start.transform.position);
        car.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90.0f));
    }

    public List<Edge> GetCarPath(Node start)
    {
        List<Edge> path = new List<Edge>();
        Dictionary<int, bool> visited = new Dictionary<int, bool>();
        Node cur = start;
        visited.Add(start.id, true);

        while (true)
        {
            List<Edge> inceds = new List<Edge>();

            foreach (var edge in nodeIncEdges[cur.id])
            {
                if (!visited.ContainsKey(edge.finish.id) || !visited[edge.finish.id])
                    inceds.Add(edge);
            }

            if (inceds.Count == 0)
                break;

            Edge newEdge = GetRandomEdge(inceds);
            visited.Add(newEdge.finish.id, true);
            path.Add(newEdge);
            cur = newEdge.finish;
        }

        return path;
    }

    public Node GetRandomNode()
    {
        List<int> keyList = new List<int>(lightsMap.Keys);
        System.Random rand = new System.Random();
        int randomKey = keyList[rand.Next(keyList.Count)];
        return lightsMap[randomKey];
    }

    private Edge GetRandomEdge(List<Edge> dic)
    {
        System.Random rand = new System.Random();
        int randomKey = rand.Next(dic.Count);
        return dic[randomKey];
    }

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
