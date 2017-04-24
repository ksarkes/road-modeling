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

    private float averageVelocity = 0;
    private int fullSpeedStep = 0;
    private float fullSpeedAll = 0;
    private float fullSpeedAllTime = 0;

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

    [SerializeField]
    private Text averageSpeedLabel;

    [SerializeField]
    private Text fullAverageSpeedLabel;
    
    [SerializeField]
    private Text timer;

    [SerializeField]
    private GameObject speedSignPrefab;

    private List<Edge> edgeway;
    private List<CarGenerator> generators = new List<CarGenerator>();
    private List<TrafficLight> lights = new List<TrafficLight>();

    private int maxCarId = 0;
    private long lastSpeedCalcStep = 0;

    private int[,] idsMatrix;
    private int[,] adjMatrix;
    private int[,] initMatrix;

    public Dictionary<int, TrafficLight> lightsMap = new Dictionary<int, TrafficLight>();

    private System.Random nodeRand = new System.Random();
    private System.Random pathRand = new System.Random();

    private SimulationProcessor()
    {
        Instance = this;
    }

    public void RegisterNode(Node node)
    {
        nodes.Add(node);
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
        generator.GenerateGraph(out idsMatrix, out adjMatrix, out initMatrix);
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                if (idsMatrix[i, j] > 0)
                    CreateTrafficLight(i, j, idsMatrix[i, j]);
    }

    private TrafficLight CreateTrafficLight(int i, int j, int id)
    {
        int x = j;
        int y = i;
        TrafficLight newLight;
        newLight = Instantiate(trafficLightPrefab);
        //trafficLightPrefab.transform.position = new Vector3(x - posMatrix.GetLength(1) / 2, y - posMatrix.GetLength(0) / 2);
        newLight.transform.position = new Vector3(x * Constants.DST_MULT, -y * Constants.DST_MULT);

        // Энгельса
        if (i == 2 && j == 4)
        {
            newLight.transform.position += new Vector3(1, 0);
        }
        if (j == 4)
            newLight.transform.position -= new Vector3(1, 0);

        newLight.transform.parent = lightsParent;
        newLight.id = id;
        newLight.i = i;
        newLight.j = j;

        if (initMatrix[i, j] == 2)
        {
            newLight.isJustNode = true;
            newLight.GetComponent<SpriteRenderer>().enabled = false;
            newLight.openIndicator.GetComponent<SpriteRenderer>().enabled = false;
        }
        newLight.StartSwitchActions();
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

                    //if ((int)i.transform.position.x == (int)lightsMap[j].transform.position.x)
                    //    i.positiveEdges.Add(edge);
                    //else
                    //    i.negativeEdges.Add(edge);

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
        time = 0;

        GenerateGraph();
        ConnectNodes();
        DrawSpeedSigns();

        GraphGenerator.PrintMatrix(adjMatrix);
        DrawLines();

        for (int i = 0; i < carsNum; i++)
        {
            var start = GetStartNode(i);
            var newCar = (Car)Instantiate(carPrefab);

            maxCarId++;
            newCar.id = maxCarId;
            newCar.transform.parent = carParent;
            newCar.start = start;
            newCar.transform.position = start.transform.position;
            cars.Add(newCar);
            carsMap.Add(newCar.id, newCar);
        }
        StartCoroutine(speedChangeCoroutine());
    }

    private void DrawSpeedSigns()
    {
        foreach (var i in edgesMap)
        {
            var e = i.Value;
            for (int j = 0; j < e.cellsNum; j++)
            {
                // Знаки ограничения на Анохина, Ленина (на половине отрезка)
                if ((e.start.id == 15 && e.finish.id == 16 ||
                    e.start.id == 16 && e.finish.id == 15 ||
                    e.start.id == 11 && e.finish.id == 27 ||
                    e.start.id == 27 && e.finish.id == 11)
                    && j == e.cellsNum / 2)
                {
                    var sign = Instantiate(speedSignPrefab);
                    sign.transform.GetComponentInChildren<TextMesh>().text = "40";
                    sign.transform.position = Vector3.Lerp(e.start.transform.position, e.finish.transform.position, (float)j / (float)e.cellsNum);
                }
                // Кусок Красной
                else if ((e.start.id == 4 && e.finish.id == 5 ||
                    e.start.id == 5 && e.finish.id == 4) && j == 0)
                {
                    var sign = Instantiate(speedSignPrefab);
                    sign.transform.GetComponentInChildren<TextMesh>().text = "40";
                    sign.transform.position = new Vector3(e.start.transform.position.x, e.start.transform.position.y + 0.2f, e.start.transform.position.z);

                }
            }
        }
    }

    private long LastGenerationTime = 0;

    private float time;

    private void Update()
    {
        //if (currentTimeStep - LastGenerationTime == 5)
        //{
        //    var newCar = Instantiate(carPrefab);
        //    newCar.transform.parent = carParent;
        //    newCar.transform.position = transform.position;

        //    cars.Add(newCar);
        //}   timer -= Time.deltaTime;
        if (cars.Count != 0)
        {
            time += Time.deltaTime;
            timer.text = "Time: " + Math.Round(time, 2);
        }

        for (int i = 0; i < Constants.TIME_STEPS_PER_FRAME; i++)
        {
            CalculateStep();
        }

        //foreach (var j in lightsMap.Values)
        //    j.TrySwitch();

    }

    private int secondsPassed = 0;
    private IEnumerator speedChangeCoroutine()
    {
        fullSpeedAllTime = 0;
        yield return null;
        while (true)
        {

            if (fullSpeedStep == 0)
                StopAllCoroutines();
            secondsPassed++;
            fullSpeedAllTime += averageVelocity;
            averageSpeedLabel.text = "Current average speed: " + ((int)(averageVelocity * 12)).ToString();
            fullAverageSpeedLabel.text = "Total average speed: " + ((int)((fullSpeedAllTime / secondsPassed) * 12)).ToString();
            yield return new WaitForSeconds(1f);
            fullSpeedAll = 0f;
            lastSpeedCalcStep = currentTimeStep;
        }
    }

    private void CalculateStep()
    {
        currentTimeStep++;
        //foreach (var j in generators)
        //    j.TryGenerate();
        //foreach (var j in lights)
        //    j.TrySwitch();
        fullSpeedStep = 0;
        foreach (var j in cars)
        {
            Accelerate(j);
            Brake(j);
            Move(j);
        }
        fullSpeedAll += (float)fullSpeedStep / cars.Count;
        averageVelocity = fullSpeedAll / (currentTimeStep - lastSpeedCalcStep);

    }

    private void Accelerate(Car car)
    {
        if (car.toRemove)
            return;


        // Acceleration
        if (car.velocity < Constants.SPEED_LIMIT)
            car.velocity += 1;
        else
            car.velocity = Constants.SPEED_LIMIT;

        // Костыль для мгновенного сброса скорости до дозволенной знаком
        int allowedSpeed = edgesMap[car.GetCurrentEdgeId()].SpeedLimit(car.cellNum);
        if (allowedSpeed != Constants.NO_MODIF)
            car.velocity = allowedSpeed;

        //System.Random rand = new System.Random();
        //int r = Constants.SPEED_LIMIT / 3 / rand.Next(1, Constants.SPEED_LIMIT + 1);
        //car.velocity += r;
    }

    private void Brake(Car car)
    {
        // Braking
        //int brakingLength = IntPow(car.velocity, 2);
        int V_BRAKE = 200;
        int stepsLeft = car.velocity * 5; //+ V_BRAKE;//+ brakingLength;

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
                curCellNum = 2 + 2 * Constants.HALF_CAR_SIZE; // 1, ибо 0 = светофор, который только что проехали

                // Достигли конца пути и не нашли препятствий
                if (curEdgeNum == car.path.Count)
                    break;

                obsEgde = edgesMap[car.GetEdgeIdByPathNum(curEdgeNum)];

                if (obsEgde.HasObstacleUntil(curCellNum))
                {
                    hasObstacle = true;
                    break;
                }
            }

            if (obsEgde.HasObstacle(curCellNum))
            {
                hasObstacle = true;
                break;
            }

        }

        if (hasObstacle)
            //car.velocity -= ((stepsLeft) / 11);
            //car.velocity -= V_BRAKE + 1;
            car.velocity = 0;
    }

    private void Move(Car car)
    {
        fullSpeedStep += car.velocity;
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
                car.cellNum = 1; //+ Constants.HALF_CAR_SIZE;
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

        // Вертим корыто так, чтобы ехало рядом с ребром по правильной полосе
        var newpos = Vector3.Lerp(edgesMap[curEdgeId].start.transform.position,
            edgesMap[curEdgeId].finish.transform.position,
             (float)car.cellNum / (float)edgesMap[curEdgeId].cells.Count);

        if (edgesMap[curEdgeId].finish.transform.position.y != edgesMap[curEdgeId].start.transform.position.y)
        {
            if (edgesMap[curEdgeId].finish.transform.position.y > edgesMap[curEdgeId].start.transform.position.y)
                newpos.x += Constants.CAR_OFFSET;
            else
                newpos.x -= Constants.CAR_OFFSET;
        }
        else if (edgesMap[curEdgeId].finish.transform.position.x != edgesMap[curEdgeId].start.transform.position.x)
        {
            if (edgesMap[curEdgeId].finish.transform.position.x > edgesMap[curEdgeId].start.transform.position.x)
                newpos.y -= Constants.CAR_OFFSET;
            else
                newpos.y += Constants.CAR_OFFSET;
        }
        car.transform.position = newpos;
        var angle = Vector3.Angle(new Vector3(0, 1) - new Vector3(0, 0),
            edgesMap[curEdgeId].finish.transform.position - edgesMap[curEdgeId].start.transform.position);

        // Подпорка для Энгельса
        if (edgesMap[curEdgeId].start.i == 1 && edgesMap[curEdgeId].start.j == 4
            && edgesMap[curEdgeId].finish.i == 2 && edgesMap[curEdgeId].finish.j == 4)
            car.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 45.0f));
        else
            car.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90.0f));
    }

    public List<Edge> GetCarPath(int carId, Node start)
    {
        List<Edge> path = new List<Edge>();
        Dictionary<int, bool> visited = new Dictionary<int, bool>();
        Node cur = start;
        visited.Add(start.id, true);

        int i = 1;
        while (true)
        {
            List<Edge> inceds = new List<Edge>();

            foreach (var edge in nodeIncEdges[cur.id])
            {
                if (!visited.ContainsKey(edge.finish.id) || !visited[edge.finish.id])
                {
                    inceds.Add(edge);
                    // Для Ленина вероятность больше
                    if (edge.finish.i == 1)
                    {
                        inceds.Add(edge);
                        inceds.Add(edge);
                    }
                }
            }

            if (inceds.Count == 0)
                break;

            Edge newEdge = inceds[pathRand.Next(0, inceds.Count)];
            visited.Add(newEdge.finish.id, true);
            path.Add(newEdge);
            cur = newEdge.finish;
        }

        return path;
    }

    public Node GetStartNode(int i)
    {
        //List<int> keyList = new List<int>(lightsMap.Keys);
        //int key = keyList[i % lightsMap.Count];
        List<int> startPoints = new List<int>();
        startPoints.Add(10);
        startPoints.Add(10);
        startPoints.Add(10);
        startPoints.Add(10);
        startPoints.Add(2);
        startPoints.Add(2);
        startPoints.Add(2);
        startPoints.Add(5);
        startPoints.Add(5);
        startPoints.Add(5);
        startPoints.Add(22);
        startPoints.Add(24);
        startPoints.Add(25);
        startPoints.Add(27);
        startPoints.Add(28);
        startPoints.Add(28);
        startPoints.Add(29);
        startPoints.Add(20);
        return lightsMap[startPoints[nodeRand.Next(0, startPoints.Count)]];
        //return lightsMap[startPoints[i % startPoints.Count]];
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
