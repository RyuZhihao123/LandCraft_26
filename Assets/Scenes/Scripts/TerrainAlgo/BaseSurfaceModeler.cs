using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

// Attributes of a vertex.
public class LocalVertexInfo
{
    public float height;       // height of base terrain surface
    public TerrainType label;  // label of basic TerrainType;
    public Vector3 normal;
    public Vector3 tangent;

    // you may add any parameters over here:
    // e.g., tree_species, has_tree_or_not, building_category, etc.
    public float water_height_shift_factor = max_water_height_change_range;  // range [-max, +max] (-max:water, 0:margin,  +max:default value)
    public float city_height_shift_factor = max_city_height_change_range;    // range [-max, +max]
    
    public PlantType plant_type = PlantType._none;  // PLANT SPECIES
    public BuildingType building_type = BuildingType._none;  // BUILDING TYPES
    public bool isRoad = false;
    public bool isMountainArea = false; // 仅仅在读取的时候有用。

    // [1] water related: active when "label == water"
    public float height_of_water_surface;     // height of sea level.
    
    // GLOBAL STATIC PARAMETER SETTINGS;
    static public TerrainType default_terrain_type = TerrainType._FOREST;
    static public float snowline_height = 120.0f;
    static public int max_water_height_change_range = 2 * 6; // please set to 2*aru_suuji. default = 2 * 10 (16)
    static public int max_city_height_change_range = 2 * 11; // please set to 2*aru_suuji. 
}


// Attributes of the enter terrain.
public class GlobalTerrainInfo
{
    // (x, height, y)
    public List<Vector3> local_minimums = new List<Vector3>();
    public List<Vector3> local_maximums = new List<Vector3>();

    public Vector3 max_height_pos = Vector3.zero;
    public Vector3 min_height_pos = Vector3.zero;

    public void ClearAll()
    {
        local_maximums.Clear();
        local_minimums.Clear();
    }
};

// Terrain Modeler.
partial class BaseSurfaceModeler
{
    private GlobalParams global_params;

    // terrain data.
    public List<List<LocalVertexInfo>> m_vertexInfos = new List<List<LocalVertexInfo>>();  // all vertices.
    public GlobalTerrainInfo m_terrainInfos = new GlobalTerrainInfo();      // extra terrain info you may need. (plz update immediately after modifying terrain data everytime.)


    public BaseSurfaceModeler(GlobalParams global_params) 
    {
        this.global_params = global_params;
    }


    public void ClearAll()
    {   
        m_vertexInfos.Clear();
        m_terrainInfos.ClearAll();
    }



    public void InitBaseSurface(TerrainType defaultType)
    {
        // [lzh] note that, please don't do any chances to this function.
        // this function creates a simple initial plane (with all Y-coordinate set to 0.0f)
        var length = global_params.length;
        var width = global_params.width;

        this.ClearAll();

        LocalVertexInfo.default_terrain_type = defaultType;  // set up default terrain type;

        for (int x = 0; x < length; x++) 
        {
            var lineVertexInfos = new List<LocalVertexInfo>();
            for (int y = 0; y < width; y++) 
            {
                LocalVertexInfo vertexInfo = new LocalVertexInfo();
                vertexInfo.height = 0.0f;
                vertexInfo.label = defaultType;
                vertexInfo.normal = Vector3.up;   
                vertexInfo.height_of_water_surface = 0.0f;

                lineVertexInfos.Add(vertexInfo);
            }
            m_vertexInfos.Add(lineVertexInfos);
        }

        // Please call the following two functions immediately as long as you modify the terrain heights.
        this.CalculateNormalsOverEntireMap();
        this.UpdateTerrainInfo();
    }


    public void MakeCityAt(Vector2 center, BuildingType bType, float decay = 0.99999f, bool _override = true)
    {
        // first create water region using Lazy Flood Fill Algorithm.
        var positions = this.MakeLazyFloodFillDistributionAt(global_params, center, TerrainType._CITY, decay, _override);

        this.ConstructCityLayoutOverRegion(positions, bType);
        this.SmoothLocally(positions, TerrainType._CITY, 2);
    }

    public void MakeCityAtWithLayoutGen(CityGenerator generator, Vector2 center, BuildingType bType, float decay = 0.99999f, bool _override = true)
    {
        // first create water region using Lazy Flood Fill Algorithm.
        var positions = this.MakeLazyFloodFillDistributionAt(global_params, center, TerrainType._CITY, decay, _override);
        
        this.ConstructCityUsingLayoutGeneration(generator, positions, bType);
        this.SmoothLocally(positions, TerrainType._CITY, 2);
    }



    public void MakeLakeAt(Vector2 center, float decay = 0.99999f, bool _override = true)
    {
        // first create water region using Lazy Flood Fill Algorithm.
        var positions = this.MakeLazyFloodFillDistributionAt(global_params, center, TerrainType._WATER, decay, _override);

        // smooth locally
        this.SmoothLocally(positions, TerrainType._WATER, 2);  // only perform smoothing to the vertices in the list "positions".
    }
    
    public void MakeRiverAt(List<BaseAlgorithm.Line2D> stroke, float riverRange=25.0f) {
        this.MakeRiverAlongStroke(stroke, riverRange:25.0f);
        // this.SmoothLocally(positions, TerrainType._WATER, 2);
    }
    
    public void MakeMountainAt(List<BaseAlgorithm.Line2D> stroke)
    {
        // 
        var positions = this.MakeMountainAlongStroke(stroke, Random.Range(60, 120), Random.Range(70, 130), _override: false);
        //this.SmoothHeightLocally(positions, 3);

    }

    public void MakeNoiseBasedTerrainAt() {
        this.MakeNoiseBasedTerrainMap();
    }

    public void DistributeTreesOnForest()
    {
        float globalTreeExistRate = global_params.globalTreeExistRate;
           
        float densityThreshold = global_params.defaultTreeDensityThreshold;
        float transitionThreshold = global_params.defaultTreeTransitionThreshold;
        float wildThreshold = global_params.defaultTreeWildThreshold;

        if (global_params.terrain_style == TerrainStyle._DESERT)
        {
            densityThreshold = global_params.desertTreeDensityThreshold;
            transitionThreshold = global_params.desertTreeTransitionThreshold;
            wildThreshold = global_params.desertTreeWildThreshold;
        }
        
        
        var length = global_params.length;
        var width = global_params.width;

        float noise_seed_x = Random.Range(0.0f, 100000.0f);
        float noise_seed_y = Random.Range(0.0f, 100000.0f);
        for (int x = 0; x < length; x++) // 0~250
        {
            for (int y = 0; y < width; y++)
            {
                m_vertexInfos[x][y].plant_type = PlantType._none;  // 重置

                if (m_vertexInfos[x][y].label == TerrainType._FOREST && 
                    m_vertexInfos[x][y].plant_type != PlantType._parkTree && 
                    globalTreeExistRate > Random.Range(0.0f, 1.0f)) // 控制全局树的密度
                {
                    float perlinValue = Mathf.PerlinNoise(y / global_params.treePerlinScale + noise_seed_x,
                        x / global_params.treePerlinScale + noise_seed_y);
                    
                    // 森林区域树的逻辑：
                    // 按照perlin的大小来逐步下降概率！
                    // 所以要有一个逐步过渡的过程
                    
                    // 根据perlin决定是否有树
                    if (perlinValue > densityThreshold)
                    {
                        // Perlin值高，群聚区
                        m_vertexInfos[x][y].plant_type = PlantType._greenTrees;
                    }
                    else if (perlinValue > transitionThreshold)
                    {
                        // 边缘区，过渡区，使用随机数决定是否放置树
                        if (Random.Range(0.0f, 1.0f) < perlinValue) // 使用perlinValue作为概率
                        {
                            m_vertexInfos[x][y].plant_type = PlantType._greenTrees;
                        }
                    }
                    else
                    {
                        // 在低密度区域放置零星的树
                        if (Random.Range(0.0f, 1.0f) < wildThreshold * perlinValue)
                        {
                            m_vertexInfos[x][y].plant_type = PlantType._greenTrees;
                        }
                    }
                }
            }
        }
    }

    // TODO:
    // here to implement more interfaces for diverse terrain types;
    public void GenerateForest(GameObject parentObj)
    {
        var length = global_params.length;
        var width = global_params.width;

        GameObject forestObj = new GameObject("Forest");


        for (int x = 0; x < length; x++) // 0~250
        {
            for (int y = 0; y < width; y++) 
            {
                PlantType plantType = m_vertexInfos[x][y].plant_type;
                if (plantType == PlantType._none || m_vertexInfos[x][y].label == TerrainType._WATER) 
                    continue;
                 
                Vector3 position = GetVector3FromIndex(x, y);
                GameObject pobj = PlantModeler.GetOnePlantModel(position, plantType);
                pobj.transform.parent = forestObj.transform;
            }
        }

        forestObj.transform.parent = parentObj.transform;
        TCP_Client.SendMessage("A [DONE] GenerateForest");
    }


    //public void GenerateBaseSurface(GameObject parentOject)
    //{
    //    GameObject baseSurfaceObj = new GameObject("BaseSurface");
    //    // first, create triangular mesh for base terrain surface.
    //    var meshes = this.BuildBaseSurfaceMesh();

    //    Texture2D maskImage = this.GetMaskImage();
        
    //    // Texture2D terrainTexture = this.GetTerrainTexture();
    //    Texture2D vertexColorTexture = this.GetLayerMaskTexture();

    //    for (int i = 0; i < meshes.Count; ++i)
    //    {
    //        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

    //        obj.name = "Block" + i.ToString();

    //        obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
    //        // obj.GetComponent<Renderer>().material = (Material)Resources.Load("Shaders/MatBaseSurface");
    //        // obj.GetComponent<Renderer>().material.SetVectorArray("_MaskRects", GlobalResources.m_terrainAtlas.uvs); // 对应label信息来取texture
    //        // obj.GetComponent<Renderer>().material.SetFloat("_TexCount", (float)TerrainType._end);
    //        // obj.GetComponent<Renderer>().material.SetTexture("_MaskTex", maskImage);    // 用来保存label信息
    //        // obj.GetComponent<Renderer>().material.SetTexture("_MainTex", GlobalResources.m_terrainAtlas.atlas);

    //        obj.GetComponent<Renderer>().material = (Material)Resources.Load("Shaders/LayerLitMaterial");
    //        obj.GetComponent<Renderer>().material.SetTexture("_LayerMaskMap", vertexColorTexture);
    //        // obj.GetComponent<Renderer>().material.SetTexture("_BaseColorMap", terrainTexture);
    //        obj.GetComponent<MeshFilter>().mesh = meshes[i];

    //        obj.transform.parent = baseSurfaceObj.transform;
    //    }

    //    baseSurfaceObj.transform.parent = parentOject.transform;

    //    TCP_Client.SendMessage("A [DONE] GenerateBaseSurface");
    //}

    
    // 使用Unity built-in terrain创建地形
    public void GenerateBuiltInSurface(GameObject parentObject) 
    {
        GameObject baseSurfaceObj = new GameObject("BaseSurface");


        // base surface
        Terrain terrain = baseSurfaceObj.AddComponent<Terrain>();
        TerrainCollider terrainCollider = baseSurfaceObj.AddComponent<TerrainCollider>();
        TerrainData terrainData = new TerrainData();
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
        // this.GetMaskImage();    // 生成图片用
        terrainData.heightmapResolution = 1025;



        // 得到地形的高度图0~1
        // alpha后面设置heightMap，因为要制造一点起伏
        float[,] heightMap = null;
        if (global_params.apply_small_noise_globally == true)
            heightMap = this.Get01HeightMapWithSmallNoiseOnTheWild();
        else
            heightMap = this.Get01HeightMap();
        float diffElevation = m_terrainInfos.max_height_pos.y - m_terrainInfos.min_height_pos.y;
        // 设置高度图
        terrainData.SetHeights(0, 0, heightMap);


        
        // 这个diffElevation是 地形高度的scale 因子
        terrainData.size = new Vector3(1025.0f, diffElevation, 1025.0f) * global_params.global_scaling;

        // material load
        Material matTerrain = Resources.Load<Material>("TerrainTextures/TerrainLayers/MatTerrain");
        terrain.materialTemplate = matTerrain;

        // 加载texture资源
        terrainData.terrainLayers = loadTerrainTextureLayer().ToArray();

        // alpha map and height map
        // 这个一定要注意heightMap和alphaMap的size之间的对应，而不是resolution之间的对应
        terrainData.alphamapResolution = 1024;
        
        // alphaMap就是对应着地形的texture的分布
        float[,,] alphaMap = this.GetTextureMaskMap();
        terrainData.SetAlphamaps(0, 0, alphaMap);




        // details: tree, grasses, flowers, stone(? maybe later)...
        // setupTerrainDetails(terrainData);
        Terrain.activeTerrain.terrainData = terrainData;
        Terrain.activeTerrain.Flush();

        baseSurfaceObj.transform.position = Vector3.up * m_terrainInfos.min_height_pos.y;
        baseSurfaceObj.transform.parent = parentObject.transform;

        TCP_Client.SendMessage("A [DONE] Build Terrain Surface");
        Debug.Log($"Highest Elevation: {m_terrainInfos.max_height_pos.y}");
        Debug.Log($"Lowest Elevation: {m_terrainInfos.min_height_pos.y}");
        
    }

    public void GenerateWaterRegion(GameObject parentObject)
    {
        var meshes = this.BuildWaterMesh();

        
        GameObject lakeObj = new GameObject("Lake");

        for (int i = 0; i < meshes.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            WaterSurface waterSurface = obj.AddComponent<WaterSurface>();

            obj.name = "Block" + i.ToString();
            waterSurface.surfaceType = WaterSurfaceType.River;
            waterSurface.geometryType = WaterGeometryType.CustomMesh;
            waterSurface.ripplesWindSpeed = 7.0f;
            waterSurface.largeWindSpeed = 7.0f;
            waterSurface.absorptionDistance = 2.3f;// 1.5f;
            waterSurface.ambientScattering = 0.35f;
            waterSurface.maxRefractionDistance = 0.5f;
            waterSurface.directLightTipScattering = 0.6f;
            waterSurface.heightScattering = 0.2f;
            waterSurface.timeMultiplier = 0.0f;
            //waterSurface.heightScattering
            //waterSurface.SetFloat("_AmplitudeDimmer", desiredAmplitudeDimmerValue);
            //waterSurface.refractionColor = new Color(62 / 255.0f, 200 / 255.0f, 200 / 255.0f); // deeper

            waterSurface.refractionColor = global_params.water_refracting_color; // deeper
            waterSurface.scatteringColor = global_params.water_scattering_color;
            //waterSurface.refractionColor = new Color(36 / 255.0f, 175 / 255.0f, 175 / 255.0f); // deeper
            //waterSurface.scatteringColor = new Color(10 / 255.0f, 164 / 255.0f, 141 / 255.0f);
            waterSurface.mesh = meshes[i];

            TCP_Client.SendMessage("A 水面: " +  meshes[i].vertices.Length.ToString());
            if (global_params.terrain_noise_type == TerrainNoiseType._STAMP)
            {
                obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            }
            else
            {
                obj.transform.position = new Vector3(0.0f, 10.2f, 0.0f);
            }
            
            obj.transform.localScale = new Vector3(1, 1, 1);

            obj.transform.parent = lakeObj.transform;
        }

        lakeObj.transform.parent = parentObject.transform;
        TCP_Client.SendMessage("A [DONE] GenerateWaterRegion");
    }

    
    public void GenerateRoadRegion(GameObject parentObject, CityGenerator generator)
    {
        var meshes = this.BuildRoadMesh(generator);

        GameObject lakeObj = new GameObject("RoadMaps");

        for (int i = 0; i < meshes.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            obj.name = "Road" + i.ToString();

            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.GetComponent<Renderer>().material = (Material)Resources.Load("Models/Roads/MatRoadMapHDRP");
            obj.GetComponent<MeshFilter>().mesh = meshes[i];

            obj.transform.parent = lakeObj.transform;
        }

        lakeObj.transform.parent = parentObject.transform;

        lakeObj.transform.localScale = new Vector3(1, 1, 1);

        TCP_Client.SendMessage("A [DONE] GenerateRoadmaps");
    }


    //public void GenerateBuildings(GameObject parentObj)
    //{
    //    var length = global_params.length;
    //    var width = global_params.width;

    //    GameObject buildingComplexObj = new GameObject("BldgComplex");

    //    BuildingModeler buildingModeler = new BuildingModeler();

    //    for (int x = 0; x < length; x++) // 0~250
    //    {
    //        for (int y = 0; y < width; y++)
    //        {
    //            if(m_vertexInfos[x][y].building_type != BuildingType._none && m_vertexInfos[x][y].label != TerrainType._WATER)
    //            {
    //                Vector3 position = GetVector3FromIndex(x, y);
    //                var buildingType = m_vertexInfos[x][y].building_type;
    //                GameObject a = buildingModeler.GetOneBldgInstance(Vector3.zero, isUsingDecorations: false, type: buildingType);
    //                a.transform.position = position;

    //                //对于建筑物可能需要一些不一样的缩放方案
    //                if (buildingType == BuildingType._skyscraper)
    //                    a.transform.localScale *= Random.Range(1.5f, 2.0f);
    //                else
    //                    a.transform.localScale *= Random.Range(2.0f, 3.0f);
    
    //                a.transform.parent = buildingComplexObj.transform;
    //            }

    //        }
    //    }

    //    buildingComplexObj.transform.parent = parentObj.transform;
    //    TCP_Client.SendMessage("A [DONE] GenerateBuidlings");
    //}


    public void GenerateBuildings(CityGenerator generator, GameObject parentObj)
    {
        var length = global_params.length;
        var width = global_params.width;

        GameObject buildingComplexObj = new GameObject("BldgComplex");
        BuildingModelerBlockVersion buildingModeler = new BuildingModelerBlockVersion();

        TCP_Client.SendMessage("A building:" + generator.lots.Count.ToString());

        foreach (var buildingBlock in generator.lots)  // default: generator.blocks
        {
            if (buildingBlock.IsPark == true)  // 如果是公园，那么不要生成的哈。
                continue;


            // 边缘检测 (isWithinCityRegion) 以及 河道检测（isNearWater）
            {
                bool isWithinCityRegion = true;
                bool isNearWater = false;

                for (int i = 0; i < buildingBlock.Nodes.Count; ++i)
                {
                    int x = (int)buildingBlock.Nodes[i].X;
                    int y = (int)buildingBlock.Nodes[i].Y;

                    if (x < 0 || y < 0 || x >= length || y >= width)
                        continue;

                    if (m_vertexInfos[x][y].label != TerrainType._CITY)
                        isWithinCityRegion = false;

                    if (isWithinCityRegion == false || isNearWater == true)
                        break;

                    float water_factor = m_vertexInfos[x][y].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;

                    if (Mathf.Abs(water_factor) < 0.3f)
                        isNearWater = true;

                }

                if (isWithinCityRegion == false || isNearWater == true)
                    continue;
            }

            GameObject buildingObj;
            Vector3 buildingCenterPos;

            BuildingType bldgType = Random.Range(0.0f, 1.0f) > 0.2f ? BuildingType._mansion : BuildingType._skyscraper;

            float actual_height_factor = GlobalTerrainSettingQt.m_bldgHeightFactor;

            if (actual_height_factor > 1.9f)
                actual_height_factor *= 2.0f; // 1.5
            (buildingObj, buildingCenterPos) = buildingModeler.GetOneBldgInstance(buildingBlock, Vector3.zero, type: bldgType, bldgHeightFactor: actual_height_factor, isUsingDecorations: false);


            if (buildingCenterPos.x < 0 || 
                buildingCenterPos.y < 0 || 
                buildingCenterPos.x >= length || 
                buildingCenterPos.y >= width)
                //m_vertexInfos[(int)buildingCenterPos.x][(int)buildingCenterPos.y].height < 5.0f)
            {
                GameObject.Destroy(buildingObj);
                continue;
            }

            buildingCenterPos.y = m_vertexInfos[(int)buildingCenterPos.x][(int)buildingCenterPos.z].height;
            buildingObj.transform.position = buildingCenterPos;


            buildingObj.transform.parent = buildingComplexObj.transform;
        }

        buildingComplexObj.transform.parent = parentObj.transform;
        buildingComplexObj.transform.localScale = new Vector3(1, 1, 1);
        TCP_Client.SendMessage("A [DONE] GenerateBuidlings");
    }

    public void GenerateNightLightingInCity(GameObject buildingComplexObj, GameObject gameObject)
    {
        GameObject lightComplexObj = new GameObject("LightComplex");
        BuildingModelerBlockVersion buildingModeler = new BuildingModelerBlockVersion();

        for (int bid = 0; bid < buildingComplexObj.transform.childCount; bid++) 
        {
            ////////////////////////////
            bool hasPointLight = (Random.Range(0.0f, 1.0f) < global_params.point_light_rate);

            Transform bldgTransform = buildingComplexObj.transform.GetChild(bid);
            if (hasPointLight)
            {
                GameObject bldgLightObj = buildingModeler.GetOnePointLight(Vector3.zero, global_params);
                bldgLightObj.transform.position = bldgTransform.position;
                bldgLightObj.transform.parent = lightComplexObj.transform;
            }
            ////////////////////////////////
        }

        lightComplexObj.transform.parent = gameObject.transform;
        lightComplexObj.transform.position = new Vector3(0, 0, 0);
        lightComplexObj.transform.localScale = new Vector3(1, 1, 1);
    }


    //public void GenerateBuildings(CityGenerator generator, GameObject parentObj)
    //{
    //    var length = global_params.length;
    //    var width = global_params.width;

    //    GameObject buildingComplexObj = new GameObject("BldgComplex");

    //    BuildingModelerBlockVersion buildingModeler = new BuildingModelerBlockVersion();

    //    foreach (var buildingBlock in generator.lots)
    //    {
    //        if (buildingBlock.IsPark == true)  // 如果是公园，那么不要生成的哈。
    //            continue;

    //        // 边缘检测
    //        {
    //            bool isWithinCityRegion = true;
    //            for (int i = 0; i < buildingBlock.Nodes.Count; ++i)
    //            {
    //                int x = (int)buildingBlock.Nodes[i].X;
    //                int y = (int)buildingBlock.Nodes[i].Y;

    //                if (x < 0 || y < 0 || x >= length || y >= width)
    //                    continue;

    //                if (m_vertexInfos[x][y].label != TerrainType._CITY)
    //                    isWithinCityRegion = false;
    //            }

    //            if (isWithinCityRegion == false)
    //                continue;
    //        }

    //        GameObject buildingObj;
    //        Vector3 buildingCenterPos;

    //        BuildingType bldgType = Random.Range(0.0f, 1.0f) > 0.2f ? BuildingType._mansion : BuildingType._skyscraper;
    //        (buildingObj, buildingCenterPos) = buildingModeler.GetOneBldgInstance(buildingBlock, Vector3.zero, type: bldgType, isUsingDecorations: false);

    //        if (buildingCenterPos.x < 0 || buildingCenterPos.y < 0 || buildingCenterPos.x >= length || buildingCenterPos.y >= width)
    //        {
    //            GameObject.Destroy(buildingObj);
    //            continue;
    //        }

    //        buildingCenterPos.y = m_vertexInfos[(int)buildingCenterPos.x][(int)buildingCenterPos.z].height;
    //        buildingObj.transform.position = buildingCenterPos;

    //        buildingObj.transform.parent = buildingComplexObj.transform;
    //    }

    //    buildingComplexObj.transform.parent = parentObj.transform;
    //    TCP_Client.SendMessage("A [DONE] GenerateBuidlings");
    //}


    private List<Vector2Int> m_functionalAreaTreePositions = new List<Vector2Int>();

    public void GenerateFunctionalAreas(CityGenerator generator, GameObject parentObj)
    {
        var length = global_params.length;
        var width = global_params.width;

        // 首先重制过去可能的功能区的树木
        for (int i = 0; i < m_functionalAreaTreePositions.Count; ++i)
        {
            var pos = m_functionalAreaTreePositions[i];
            m_vertexInfos[pos.x][pos.y].plant_type = PlantType._none;
        }
        m_functionalAreaTreePositions.Clear();


        GameObject funcAreaObj= new GameObject("FuncAreas");
        // 城镇内功能区。
        GameObject funcAreaObjTown = new GameObject("FuncAreas_town");
        var meshes_town = this.BuildGreenAreas(generator, isGenWild: false);

        for (int i = 0; i < meshes_town.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);


            obj.name = "FuncArea" + i.ToString();
            obj.GetComponent<MeshRenderer>().material = 
                (Material)Resources.Load("TerrainTextures/TerrainLayers/MatFunctionArea");
            obj.GetComponent<MeshFilter>().mesh = meshes_town[i];

            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.localScale = new Vector3(1, 1, 1);

            obj.transform.parent = funcAreaObjTown.transform;
        }

        funcAreaObjTown.transform.parent = funcAreaObj.transform;
        funcAreaObjTown.transform.localScale = new Vector3(1, 1, 1);

        // 城镇外功能区。
        GameObject funcAreaObjWild = new GameObject("FuncAreas_wild");
        var meshes_wild = this.BuildGreenAreas(generator, isGenWild: true);

        for (int i = 0; i < meshes_wild.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);


            obj.name = "FuncArea" + i.ToString();
            obj.GetComponent<MeshRenderer>().material =
                (Material)Resources.Load("TerrainTextures/TerrainLayers/MatFunctionArea");
            obj.GetComponent<MeshFilter>().mesh = meshes_wild[i];

            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.localScale = new Vector3(1, 1, 1);

            obj.transform.parent = funcAreaObjWild.transform;
        }

        funcAreaObjWild.transform.parent = funcAreaObj.transform;
        funcAreaObjWild.transform.localScale = new Vector3(1, 1, 1);

        funcAreaObj.transform.parent = parentObj.transform;
        funcAreaObj.transform.localScale = new Vector3(1, 1, 1);
        TCP_Client.SendMessage("A [DONE] GenerateWaterRegion");


    }

    
    
    
}










