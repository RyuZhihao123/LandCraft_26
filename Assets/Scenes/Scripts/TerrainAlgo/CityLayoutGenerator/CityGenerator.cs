using System.Collections.Generic;
using System.Threading;
using BlockGeneration;
using GraphModel;
using MeshGeneration;
using RoadGeneration;
using BlockDivision;
using Services;
using UnityEngine;

public class CityGenerator //: MonoBehaviour
{
    public static float m_ParkDensity = 0.7f;   //  默认：0.7


    public Graph roadGraph; //Graph which will be built, and then drawn
    public List<BlockNode> blockNodes; //Nodes of the Blocks
    public List<Block> blocks;
    public List<Block> thinnedBlocks;
    public List<Block> lots;    // a single building area.
    public System.Random rand;

    private List<Block> concaveBlocks;
    private List<Block> convexBlocks;
    private List<BlockMesh> blockMeshes;
    private List<BlockMesh> lotMeshes;
    private List<BoundingRectangle> boundingRectangles;
    private float blockHeight = 0.02f;

    [Header("Seed and Size")]
    public int mapSize = 300;
    public int seed = 30;


    [Header("Major Road generation")]
    public bool isGridMode = false;
    [Range(0, 20)]
    public int maxDegreeInCurves = 10;   // default = 10
    [Range(0.03f, 0.1f)]
    public float branchingProbability = 0.075f;

    [Header("Minor Road generation")]
    [Range(0.02f, 0.2f)]
    public float crossingDeletionProbability = 0.1f;

    [Header("Maximum Number of Roads")]
    public int maxMajorRoad = 2000;
    public int maxMinorRoad = 10000;

    [Header("Thickness of Roads")]
    [Range(0.1f, 2.5f)]
    public float majorThickness = 2.5f;
    [Range(0.1f, 2.5f)]
    public float minorThickness = 0.9f;

    [Header("Sidewalk generation")]
    [Range(0.1f, 1f)]
    public float sidewalkThickness = 0.5f;

    [Header("Building generation")]
    public float minBuildHeight = 2;  // 2
    public float maxBuildHeight = 15; // 15

    [Header("Gizmos")]
    public bool drawRoadNodes;
    public bool drawRoads = true;
    public bool drawBlockNodes;
    public bool drawBlocks = true;
    public bool drawThinnedBlocks;
    public bool drawConvexBlocks;
    public bool drawConcaveBlocks;
    public bool drawTriangulatedMeshes;
    public bool drawBoundingBoxes;
    public bool drawLots = true;
    

    //Event to call, when the generation is ready
    private bool genReady;
    private bool genDone;


    public void ConstructCityLayout(bool _isGridMode)
    {
        roadGraph = new Graph(); //Graph which will be built, and then drawn
        blockNodes = new List<BlockNode>(); //Nodes of the Blocks
        blocks = new List<Block>();
        thinnedBlocks = new List<Block>();
        lots = new List<Block>();    // a single building area.

        isGridMode = _isGridMode;

        if (GlobalParams.enable_dataset_mode) {
            return;
        }
        
        
        this.Start();
        System.Diagnostics.Stopwatch mainSw = System.Diagnostics.Stopwatch.StartNew();
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        if(isGridMode == true)
        {
            maxDegreeInCurves = 0;
        }
        else
        {
            maxDegreeInCurves = 10;
        }

        //ROAD GENERATION
        MajorGenerator majorGen = new MajorGenerator(
            rand, mapSize, maxMajorRoad, (int)(maxDegreeInCurves*GlobalTerrainSettingQt.m_bldgRoadCurveFactor), branchingProbability, roadGraph, isGridMode);
        majorGen.Run();
        MinorGenerator minorGen = new MinorGenerator(
            rand, mapSize, maxMinorRoad, crossingDeletionProbability, roadGraph, majorGen.GetRoadSegments());
        minorGen.Run();



        //ROAD GENERATION TIME, ROAD COUNT
        sw.Stop();
        Debug.Log("Road generation time taken: " + sw.Elapsed.TotalMilliseconds + " ms");
        Debug.Log(majorGen.GetRoadSegments().Count + " major road generated");
        Debug.Log(minorGen.GetRoadSegments().Count + " minor road generated");

        //BLOCK GENERATION
        BlockGenerator blockGen = new BlockGenerator(roadGraph, mapSize, majorThickness, minorThickness, blockHeight);
        blockGen.Generate();
        blockNodes = blockGen.BlockNodes;
        blocks = blockGen.Blocks;
        Debug.Log(blockGen.Blocks.Count + " block generated");

        //SIDEWALK GENERATION
        blockGen.ThickenBlocks(sidewalkThickness);
        thinnedBlocks = blockGen.ThinnedBlocks;
        Debug.Log("Sidewalk generation completed");

        //BLOCK DIVISION
        sw = System.Diagnostics.Stopwatch.StartNew();

        BlockDivider blockDiv = new BlockDivider(rand, thinnedBlocks, lots);
        blockDiv.DivideBlocks();

        float heightFactor = GlobalTerrainSettingQt.m_bldgHeightFactor> 1.0f? GlobalTerrainSettingQt.m_bldgHeightFactor*30: GlobalTerrainSettingQt.m_bldgHeightFactor;
        float maxActualBldgHeight = maxBuildHeight * heightFactor;
        float minActualBldgHeight = minBuildHeight * heightFactor;
        minActualBldgHeight = minActualBldgHeight < 2.0f ? 2.0f : minActualBldgHeight;
        maxActualBldgHeight = maxActualBldgHeight < minBuildHeight ? minBuildHeight + 0.01f : maxActualBldgHeight;
        TCP_Client.SendMessage(string.Format("A BuildingHeightRange: {0} {1}", minBuildHeight, maxActualBldgHeight));
        blockDiv.SetBuildingHeights(minActualBldgHeight, maxActualBldgHeight, blockHeight, mapSize);
        boundingRectangles = blockDiv.BoundingRectangles;

        //LOT GENERATION TIME, LOT COUNT
        sw.Stop();
        Debug.Log("Lot generation time taken: " + sw.Elapsed.TotalMilliseconds + " ms");
        Debug.Log(lots.Count + " lot generated");

        ResizePositions();
    }

    void Start()
    {
        rand = new System.Random(seed);
        roadGraph = new Graph();
        lots = new List<Block>();

        //showcase - use
        //ThreadProc();
        //Thread t = new Thread(ThreadProc);
        //t.Start();
    }

    void Update()
    {
        if (genReady && !genDone) //This make sure, that this will be only called once
        {
            genDone = true;
            GenerateGameObjects();
        }
    }

    void ResizePositions()
    {
        var MajorNodes = roadGraph.MajorNodes;
        var MinorNodes = roadGraph.MinorNodes;


        for (int i = 0; i < MajorNodes.Count; ++i)
        {
            MajorNodes[i].X += 300.0f;
            MajorNodes[i].Y += 300.0f;
            //MajorNodes[i].X /= 2.0f;
            //MajorNodes[i].Y /= 2.0f;
        }
        for (int i = 0; i < MinorNodes.Count; ++i)
        {
            MinorNodes[i].X += 300.0f;
            MinorNodes[i].Y += 300.0f;

            //MinorNodes[i].X /= 2.0f;
            //MinorNodes[i].Y /= 2.0f;
        }

        foreach (var block in blocks)
        {
            var blockNodes = block.Nodes;

            for (int i = 0; i < blockNodes.Count; ++i)
            {
                blockNodes[i].X += 300.0f;
                blockNodes[i].Y += 300.0f;

                //blockNodes[i].X /= 2.0f;
                //blockNodes[i].Y /= 2.0f;
            }
        }

        foreach (var lot in lots)
        {
            var blockNodes = lot.Nodes;

            for (int i = 0; i < blockNodes.Count; ++i)
            {
                blockNodes[i].X += 300.0f;
                blockNodes[i].Y += 300.0f;

                //blockNodes[i].X /= 2.0f;
                //blockNodes[i].Y /= 2.0f;
            }
        }
    }

    private void ThreadProc()
    {

        if (isGridMode == true)
        {
            maxDegreeInCurves = 0;
        }
        else
        {
            maxDegreeInCurves = 10;
        }

        System.Diagnostics.Stopwatch mainSw = System.Diagnostics.Stopwatch.StartNew();
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        //ROAD GENERATION
        MajorGenerator majorGen = new MajorGenerator(
            rand, mapSize, maxMajorRoad, maxDegreeInCurves, branchingProbability, roadGraph, isGridMode);
        majorGen.Run();
        MinorGenerator minorGen = new MinorGenerator(
            rand, mapSize, maxMinorRoad, crossingDeletionProbability, roadGraph, majorGen.GetRoadSegments());
        minorGen.Run();


        //ROAD GENERATION TIME, ROAD COUNT
        sw.Stop();
        Debug.Log("Road generation time taken: " + sw.Elapsed.TotalMilliseconds + " ms");
        Debug.Log(majorGen.GetRoadSegments().Count + " major road generated");
        Debug.Log(minorGen.GetRoadSegments().Count + " minor road generated");

        //BLOCK GENERATION
        BlockGenerator blockGen = new BlockGenerator(roadGraph, mapSize, majorThickness, minorThickness, blockHeight);
        blockGen.Generate();
        blockNodes = blockGen.BlockNodes;
        blocks = blockGen.Blocks;
        Debug.Log(blockGen.Blocks.Count + " block generated");

        //SIDEWALK GENERATION
        blockGen.ThickenBlocks(sidewalkThickness);
        thinnedBlocks = blockGen.ThinnedBlocks;
        Debug.Log("Sidewalk generation completed");

        //BLOCK DIVISION
        sw = System.Diagnostics.Stopwatch.StartNew();

        BlockDivider blockDiv = new BlockDivider(rand, thinnedBlocks, lots);
        blockDiv.DivideBlocks();
        blockDiv.SetBuildingHeights(minBuildHeight, maxBuildHeight, blockHeight, mapSize);
        boundingRectangles = blockDiv.BoundingRectangles;

        //LOT GENERATION TIME, LOT COUNT
        sw.Stop();
        Debug.Log("Lot generation time taken: " + sw.Elapsed.TotalMilliseconds + " ms");
        Debug.Log(lots.Count + " lot generated");


        ResizePositions();
        //BLOCK MESH GENERATION
        MeshGenerator blockMeshGen = new MeshGenerator(blocks, blockHeight);
        blockMeshGen.GenerateMeshes();
        blockMeshes = blockMeshGen.BlockMeshes;

        //LOT MESH GENERATION
        MeshGenerator lotMeshGen = new MeshGenerator(lots, blockHeight + blockHeight / 3);
        lotMeshGen.GenerateMeshes();

        convexBlocks = lotMeshGen.ConvexBlocks;
        concaveBlocks = lotMeshGen.ConcaveBlocks;
        lotMeshes = lotMeshGen.BlockMeshes;

        mainSw.Stop();
        Debug.Log("City generation time taken: " + mainSw.Elapsed.TotalMilliseconds + " ms");

        genReady = true;
    }

    private void GenerateGameObjects()
    {
        var separator = new GameObject();
        separator.name = "===========";

        //Make RoadPlane
        var roadPlane = new GameObject();
        roadPlane.name = "Road Plane";
        roadPlane.AddComponent<MeshFilter>();
        roadPlane.AddComponent<MeshRenderer>();
        roadPlane.GetComponent<MeshFilter>().mesh = MeshCreateService.GenerateRoadMesh(mapSize);

        Material roadMaterial = Resources.Load<Material>("MatCityGenerator/MatRoad");
        roadPlane.GetComponent<MeshRenderer>().material = roadMaterial;

        //Make Blocks
        var blockContainer = new GameObject();
        blockContainer.name = "Block Container";

        Material blockMaterial = Resources.Load<Material>("MatCityGenerator/MatBlock");
        Material parkMaterial = Resources.Load<Material>("MatCityGenerator/MatGreen");

        for (int i = 0; i < blockMeshes.Count; i++)
        {
            var block = new GameObject();
            block.name = "Block" + i.ToString();
            block.transform.parent = blockContainer.transform;
            block.AddComponent<MeshFilter>();
            block.AddComponent<MeshRenderer>();
            block.GetComponent<MeshFilter>().mesh = MeshCreateService.GenerateBlockMesh(blockMeshes[i]);

            if (blockMeshes[i].Block.IsPark) block.GetComponent<MeshRenderer>().material = parkMaterial;
            else block.GetComponent<MeshRenderer>().material = blockMaterial;
        }

        //Make Lots
        var lotContainer = new GameObject();
        lotContainer.name = "Lot Container";

        Material lotMaterial = Resources.Load<Material>("MatCityGenerator/MatLot");

        for (int i = 0; i < lotMeshes.Count; i++)
        {
            var lot = new GameObject();
            lot.name = "Lot" + i.ToString();
            lot.transform.parent = lotContainer.transform;
            lot.AddComponent<MeshFilter>();
            lot.AddComponent<MeshRenderer>();
            lot.GetComponent<MeshFilter>().mesh = MeshCreateService.GenerateBlockMesh(lotMeshes[i]);

            if (lotMeshes[i].Block.IsPark) lot.GetComponent<MeshRenderer>().material = parkMaterial;
            else lot.GetComponent<MeshRenderer>().material = lotMaterial;
        }
    }

    private void OnDrawGizmos()
    {
        if (roadGraph == null)
        {
            return;
        }

        if (drawRoads)
        {
            GizmoService.DrawEdges(roadGraph.MajorEdges, Color.white);
            GizmoService.DrawEdges(roadGraph.MinorEdges, Color.black);
        }

        if (drawRoadNodes)
        {
            GizmoService.DrawNodes(roadGraph.MajorNodes, Color.white, 2f);
            GizmoService.DrawNodes(roadGraph.MinorNodes, Color.black, 1f);
        }

        if (drawBlockNodes)
        {
            GizmoService.DrawBlockNodes(blockNodes, Color.red, 0.4f);
        }

        if (drawBlocks)
        {
            GizmoService.DrawBlocks(blocks, new Color(0.7f, 0.4f, 0.4f));
        }

        if (drawThinnedBlocks)
        {
            GizmoService.DrawBlocks(thinnedBlocks, new Color(0.7f, 0.4f, 0.4f));
        }

        if (drawConvexBlocks && genDone)
        {
            GizmoService.DrawBlocks(convexBlocks, new Color(0.2f, 0.7f, 0.7f));
        }
        if (drawConcaveBlocks && genDone)
        {
            GizmoService.DrawBlocks(concaveBlocks, new Color(0.2f, 0.7f, 0.2f));
        }
        if (drawTriangulatedMeshes && genDone)
        {
            GizmoService.DrawBlockMeshes(blockMeshes, new Color(.8f, .8f, .8f));
        }

        if (drawBoundingBoxes && genDone)
        {
            List<Edge> cutEdges = new List<Edge>();

            foreach (var boundingBox in boundingRectangles)
            {
                GizmoService.DrawEdges(boundingBox.Edges, Color.white);
                cutEdges.Add(boundingBox.GetCutEdge());
            }

            GizmoService.DrawEdges(cutEdges, Color.yellow);
        }

        if (drawLots)
        {
            GizmoService.DrawBlocks(lots, new Color(0.2f, 0.7f, 0.7f));
        }
    }
}