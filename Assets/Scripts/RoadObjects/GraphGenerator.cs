﻿using UnityEngine;
using UnityEditor;
using System;

public class GraphGenerator
{
    private int N = 10;
    private int M = 10;
    private int[,] init;
    private int[,] adjMatrix;

    private float generationMedian = 0.05f;

    public GraphGenerator(int N, int M)
    {
        this.N = N;
        this.M = M;
    }

    public void GenerateGraph(out int[,] posMatrix, out int[,] adjMatrix)
    {
        GenerateRandomInitMatrix();
        posMatrix = init;
        adjMatrix = this.adjMatrix;
        return;
    }

    private void GenerateRandomInitMatrix()
    {
        init = new int[M, N];
        System.Random rand = new System.Random();
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                var rnd = UnityEngine.Random.Range(0.0f, 1.0f);
                init[i, j] = rnd < generationMedian ? 1 : 0;
            }

        PrintMatrix(init);

        int[,] backup = new int[N, M];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                backup[i, j] = init[i, j];


        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                if (init[i, j] == 1)
                    TryCreateEdges(i, j, backup);
            }
        Debug.Log(init.ToString());


        PrintMatrix(init);

        var adjDim = AssignIds();

        PrintMatrix(init);

        CreateAdjMatrix(adjDim);

        PrintMatrix(init);

    }

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
                init[i, b] = 1;

            if (backup[i, b] == 0)
            {
                init[i, b] = -1;
                backup[i, b] = -1;
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
                init[i, b] = 1;

            if (backup[i, b] == 0)
            {
                init[i, b] = -1;
                backup[i, b] = -1;
            }

        }

        for (int i = b + 1; i < M; i++)
        {
            if (backup[a, i] == -1)
            {
                init[a, i] = 1;
                break;
            }
            if (i == M - 1)
                init[a, i] = 1;

            if (backup[a, i] == 0)
            {
                backup[a, i] = -1;
                init[a, i] = -1;
                break;
            }

        }

        for (int i = b - 1; i >= 0; i--)
        {
            if (backup[a, i] == -1)
            {
                init[a, i] = 1;
                break;
            }
            if (i == 0)
                init[a, i] = 1;

            if (backup[a, i] == 0)
            {
                backup[a, i] = -1;
                init[a, i] = -1;
                break;
            }
        }
    }

    private int AssignIds()
    {
        int maxId = 1;
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
            {
                if (init[i, j] == 1)
                {
                    init[i, j] = maxId;
                    maxId++;
                }

            }
        return maxId;
    }

    private void CreateAdjMatrix(int dim)
    {
        adjMatrix = new int[dim + 1, dim + 1];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < M; j++)
                if (init[i, j] > 0)
                    TryCreateAdjEdges(i, j);
    }

    private void TryCreateAdjEdges(int a, int b)
    {
        for (int i = a + 1; i < N; i++)
        {
            if (init[i, b] > 0)
            {
                var x = init[i, b];
                var y = init[a, b];
                adjMatrix[y, x] = System.Math.Abs(i - a);
                break;
            }
        }

        for (int i = a - 1; i >= 0; i--)
        {
            if (init[i, b] > 0)
            {
                var x = init[i, b];
                var y = init[a, b];
                adjMatrix[y, x] = System.Math.Abs(i - a);
                break;
            }
        }

        for (int i = b + 1; i < M; i++)
        {
            if (init[a, i] > 0)
            {
                var x = init[a, i];
                var y = init[a, b];
                try
                {
                    adjMatrix[y, x] = System.Math.Abs(i - b);
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                    Debug.LogErrorFormat(" :: X :: {0} :: Y :: {1} ::", x, y);
                    //Debug.LogError()
                }
            }
        }


        for (int i = b - 1; i >= 0; i--)
        {
            if (init[a, i] > 0)
            {
                var x = init[a, i];
                var y = init[a, b];
                adjMatrix[y, x] = System.Math.Abs(i - b);
                break;
            }
        }
    }

    private void PrintMatrix(int[,] matrix)
    {
        string obj = "";
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < M; j++)
                obj += " " + matrix[i, j].ToString();
            obj += '\n';
        }
        obj += '\n';
        Debug.Log(obj);
    }
}