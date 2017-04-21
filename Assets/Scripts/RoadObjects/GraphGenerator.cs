using UnityEngine;
using System;

public class GraphGenerator
{
    private int N = 10;
    private int M = 10;
    private int[,] init;
    private int[,] adjMatrix;
    private int[,] idsMatrix;
    private int[,] backup;

    private float generationMedian = 0.05f;

    public GraphGenerator(int N, int M)
    {
        this.N = N;
        this.M = M;
    }

    public void GenerateGraph(out int[,] posMatrix, out int[,] adjMatrix, out int[,] init)
    {
        GenerateRandomInitMatrix();
        posMatrix = idsMatrix;
        adjMatrix = this.adjMatrix;
        init = this.init;
        PrintMatrix(idsMatrix);
        return;
    }

    private void GenerateRandomInitMatrix()
    {
        init = new int[N, M];
        idsMatrix = new int[N, M];
        //init = new int[,] 
        //{ 
        //    {0, 0, 0, 0, 0, 0},
        //    { 0,1}
        //};

        /*
        backup = new int[N, M];
        System.Random rand = new System.Random();
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                //var rnd = UnityEngine.Random.Range(0.0f, 1.0f);
                //init[i, j] = rnd < generationMedian ? 1 : 0;
                if (i == j || i % 5 == 0)
                    init[i, j] = 1;
                else
                    init[i, j] = 0;

                //if (i == j && i == 2 || i == 0 && j == 0)
                //    init[i, j] = 1;
                //else
                //    init[i, j] = 0;
            }

        PrintMatrix(init);

        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                backup[i, j] = init[i, j];


        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                if (backup[i, j] == 1)
                    TryCreateEdges(i, j, backup);
            }

        PrintMatrix(init);
        */

        init = ReadMatrix();
        var adjDim = AssignIds();

        PrintMatrix(init);

        CreateAdjMatrix(adjDim);

        PrintMatrix(adjMatrix);
        PrintMatrix(init);

    }

    private int AssignIds()
    {
        int maxId = 1;
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                if (init[i, j] > 0)
                {
                    idsMatrix[i, j] = maxId;
                    maxId++;
                }
                else
                    idsMatrix[i, j] = -1;
            }
        return maxId;
    }

    private void CreateAdjMatrix(int dim)
    {
        adjMatrix = new int[dim + 1, dim + 1];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                if (idsMatrix[i, j] > 0)
                    TryCreateAdjEdges(i, j);
    }

    private void TryCreateAdjEdges(int a, int b)
    {
        for (int i = a + 1; i < N; i++)
        {
            if (idsMatrix[i, b] > 0)
            {
                var x = idsMatrix[i, b];
                var y = idsMatrix[a, b];

                if (CheckSpecConditions(a, b, i, b))
                    break;

                adjMatrix[y, x] = 1;
                break;
            }
        }

        for (int i = a - 1; i >= 0; i--)
        {
            if (idsMatrix[i, b] > 0)
            {
                var x = idsMatrix[i, b];
                var y = idsMatrix[a, b];

                if (CheckSpecConditions(a, b, i, b))
                    break;

                adjMatrix[y, x] = 1;
                break;
            }
        }

        for (int i = b + 1; i < M; i++)
        {
            if (idsMatrix[a, i] > 0)
            {
                var x = idsMatrix[a, i];
                var y = idsMatrix[a, b];

                if (CheckSpecConditions(a, b, a, i))
                    break;
            
                adjMatrix[y, x] = 1;
                break;
            }
        }


        for (int i = b - 1; i >= 0; i--)
        {
            if (idsMatrix[a, i] > 0)
            {
                var x = idsMatrix[a, i];
                var y = idsMatrix[a, b];

                if (CheckSpecConditions(a, b, a, i))
                    break;

                adjMatrix[y, x] = 1;
                break;
            }
        }
    }

    // Return whether should break edge
    private bool CheckSpecConditions(int i1, int j1, int i2, int j2)
    {
        if (i1 == 2 && i2 == 2 && 
            (j1 == 8 && j2 == 9 || j1 == 9 && j2 == 8))
            return true;

        if (i1 == 2 && i2 == 2 &&
            (j1 == 4 && j2 == 5 || j1 == 5 && j2 == 4))
            return true;

        // Одностороннее внизу Красной
        if (i1 == 0 && i2 == 0 && j1 > 5 && j2 > 5 && j2 < j1)
            return true;
        // Одностороннее наверху Красной
        if (i1 == 0 && i2 == 0 && j1 == 1 && j2 == 2)
            return true;
        // Одностороннее на Андропова
        if (i1 < 3 && i2 < 3 && i1 < i2 && j1 == 5 && j2 == 5)
            return true;
        // Одностороннее на Дзержинского
        if (i1 < 3 && i2 < 3 && i1 > i2 && j1 == 6 && j2 == 6)
            return true;
        // Одностороннее внизу Свердлова
        if (i1 == 2 && i2 == 2 && j1 >= 5 && j2 >= 5 && j1 <= 7 && j2 <= 7 && j2 < j1)
            return true;
        if (i1 == 2 && i2 == 2 && j1 >= 5 && j1 == 7 && j2 == 8)
            return true;
        // Одностороннее на Горького
        if (i1 == 3 && i2 == 3 && j1 < j2)
            return true;
        else
            return false;
    }

    /*
private void TryCreateEdges(int a, int b, int[,] backup)
{
   for (int i = a + 1; i < N; i++)
   {
       if (backup[i, b] == -1)
       {
           init[i, b] = 1;
           break;
       }

       if (i == N - 1)
       {
           init[i, b] = 1;
           break;
       }

       if (backup[i, b] == 0)
       {
           init[i, b] = -1;
           //backup[i, b] = -1;
       }

   }

   for (int i = a - 1; i >= 0; i--)
   {
       if (backup[i, b] == -1)
       {
           init[i, b] = 1;
           break;
       }

       if (i == 0)
       {
           init[i, b] = 1;
           break;
       }

       if (backup[i, b] == 0)
       {
           init[i, b] = -1;
           backup[i, b] = -1;
       }

   }

   for (int j = b + 1; j < M; j++)
   {
       if (backup[a, j] == -1)
       {
           init[a, j] = 1;
           break;
       }

       if (j == M - 1)
       {
           init[a, j] = 1;
           break;
       }

       if (backup[a, j] == 0)
       {
           backup[a, j] = -1;
           init[a, j] = -1;
       }

   }

   for (int j = b - 1; j >= 0; j--)
   {
       if (backup[a, j] == -1)
       {
           init[a, j] = 1;
           break;
       }

       if (j == 0)
       {
           init[a, j] = 1;
           break;
       }

       if (backup[a, j] == 0)
       {
           backup[a, j] = -1;
           init[a, j] = -1;
       }
   }
}
*/

    private int[,] ReadMatrix()
    {
        String input = System.IO.File.ReadAllText("input.txt");
        int i = 0, j = 0;
        int[,] result = new int[N, M];
        foreach (var row in input.Split('\n'))
        {
            j = 0;
            foreach (var col in row.Trim().Split(' '))
            {
                result[i, j] = int.Parse(col.Trim());
                j++;
            }
            i++;
        }
        return result;
    }

    public static void PrintMatrix(int[,] matrix)
    {
        string obj = "";
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
                obj += " " + matrix[i, j].ToString();
            obj += '\n';
        }
        obj += '\n';
        Debug.Log(obj);
    }
}