using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGenerator : Node {


    public int TimeBetweenGeneration = 200;
    private long LastGenerationTime = 0;

    public override void init()
    {
        base.init();
        LastGenerationTime = -TimeBetweenGeneration; 
    }

    public void TryGenerate()
    {
        if (SimulationProcessor.Instance.currentTimeStep - (long)LastGenerationTime >= TimeBetweenGeneration)
        {
            LastGenerationTime = SimulationProcessor.Instance.currentTimeStep;
            Generate();
        }
    }

    private void Generate()
    {
        var carPrefab = SimulationProcessor.Instance.carPrefab;
        var newCar = Instantiate(carPrefab);
        newCar.transform.parent = SimulationProcessor.Instance.carParent;
        newCar.transform.position = transform.position;
        
    }
}
