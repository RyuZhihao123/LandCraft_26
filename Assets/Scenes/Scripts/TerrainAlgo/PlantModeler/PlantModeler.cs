using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum PlantType
{
    _none,
    _greenTrees,
    _redTrees,
    _pineTree,
    _cherryTrees,
    _parkTree
    // _bushes,
}

public class PlantModeler
{
    static Dictionary<PlantType, List<GameObject>> treeResources = new Dictionary<PlantType, List<GameObject>>();

    public static void InitPlantResources()
    {
        List<GameObject> greenTrees = new List<GameObject>();
        List<GameObject> redTrees = new List<GameObject>();
        List<GameObject> cherryTrees = new List<GameObject>();
        List<GameObject> pineTrees = new List<GameObject>();

        for (int i = 1; i <= 3; ++i)
            greenTrees.Add((GameObject)Resources.Load(string.Format("Models/Trees/GreenTrees/{0}/{0}", i)));

        for (int i = 1; i <= 2; ++i)
            redTrees.Add((GameObject)Resources.Load(string.Format("Models/Trees/RedTrees/{0}/{0}", i)));

        for (int i = 1; i <= 1; ++i)
            cherryTrees.Add((GameObject)Resources.Load(string.Format("Models/Trees/CherryTrees/{0}/{0}", i)));

        for (int i = 1; i <= 2; ++i)
            pineTrees.Add((GameObject)Resources.Load(string.Format("Models/Trees/PineTrees/{0}/{0}", i)));

        treeResources[PlantType._greenTrees] = greenTrees;
        treeResources[PlantType._redTrees] = redTrees;
        treeResources[PlantType._cherryTrees] = cherryTrees;
        treeResources[PlantType._pineTree] = pineTrees;


    }
    public static GameObject GetOnePlantModel(Vector3 position, PlantType plantType)
    {
        var resources = treeResources[plantType];

        int plantID = 0;

        if (resources.Count > 1)
        {
            plantID = Random.Range(0, resources.Count);   // count = 5: 0 1 2 3 4
        }

        GameObject a = GameObject.Instantiate(resources[plantID], position, Quaternion.identity);
        a.transform.localScale *= Random.Range(30.0f,45.0f);
        a.transform.localRotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        return a;
    }
}
