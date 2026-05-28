using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainAdapter
{
    private GlobalParams global_params;
    BaseSurfaceModeler m_baseSurfaceModeler;   // core terrain data. 
    GameObject finalGameObject = null;

    Terrain temp_terrain = null;

    public TerrainAdapter(GlobalParams global_params) {
        this.global_params = global_params;
        m_baseSurfaceModeler = new BaseSurfaceModeler(global_params);
    }

    public void GenTerrainMap(CityGenerator generator) 
    {
        m_baseSurfaceModeler.ClearAll();

        // you should always call this function in the beginning.
        m_baseSurfaceModeler.InitBaseSurface(defaultType: TerrainType._FOREST);

        // 是否有提前定义一些 land cover map
        GlobalParams.loaded_predefined_height_map = false;
        GlobalParams.loaded_predefined_label_map = false;
        // 仅提供label map
        if (global_params.predefine_label_map != null && global_params.predefine_height_map == null)
        {
            var temp_length = global_params.predefine_label_map.height;
            var temp_width = global_params.predefine_label_map.width;

            if (temp_length == global_params.length && temp_width == global_params.width)
            {
                m_baseSurfaceModeler.ApplyExistedLandCoverMap();
                
                // // 试试能不能生成一些地形~
                // {   // base terrain
                //     m_baseSurfaceModeler.MakeNoiseBasedTerrainAt();
                //
                //     m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                //     m_baseSurfaceModeler.UpdateTerrainInfo();
                // }
                
                m_baseSurfaceModeler.UpdateWaterHeightShiftFactor(); // after making lakes, we need to shift this region vertically.
                m_baseSurfaceModeler.ShiftDownWaterRegion(isOnlyUseLayoutMap:true);
                
                m_baseSurfaceModeler.UpdateCityHeightShiftFactor();


                m_baseSurfaceModeler.MakeMountainFromLayout();
     

                m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                m_baseSurfaceModeler.UpdateTerrainInfo();
                
                m_baseSurfaceModeler.DistributeTreesOnForest();
                
                m_baseSurfaceModeler.SaveHeightShiftFactorMap();
                
                TCP_Client.SendMessage("A [DONE] Apply LandCover Map!");
                GlobalParams.loaded_predefined_label_map = true;
            }
            else
            {
                TCP_Client.SendMessage("A [DONE] !!!!!!!!!!WARNING! Predefined LandCover Map is Not Compatible!!!!!!!!!!");
            }
        }
        // 只提供height
        else if (global_params.predefine_label_map == null && global_params.predefine_height_map != null)
        {
            var temp_length = global_params.predefine_height_map.height;
            var temp_width = global_params.predefine_height_map.width;

            if (temp_length == global_params.length && temp_width == global_params.width)
            {
                m_baseSurfaceModeler.ApplyExistedHeightMap(reset_label:true);
                
                m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                m_baseSurfaceModeler.UpdateTerrainInfo();
                
                // m_baseSurfaceModeler.DistributeTreesOnForest();

                TCP_Client.SendMessage("A [DONE] Apply Height Map!");
                GlobalParams.loaded_predefined_height_map = true;
            }
            else
            {
                TCP_Client.SendMessage("A [DONE] !!!!!!!!!!WARNING! Predefined LandCover Map is Not Compatible!!!!!!!!!!");
            }
        }
        // 同时提供label和height map
        else if (global_params.predefine_label_map != null && global_params.predefine_height_map != null)
        {
            var temp_label_height = global_params.predefine_label_map.height;
            var temp_label_width = global_params.predefine_label_map.width;
            var temp_height_height = global_params.predefine_height_map.height;
            var temp_height_width = global_params.predefine_height_map.width;
            
            if (temp_label_height == global_params.length && 
                temp_label_width == global_params.width &&
                temp_height_height == global_params.length &&
                temp_height_width == global_params.width)
            {
                m_baseSurfaceModeler.ApplyExistedHeightMap(reset_label:false);
                m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                m_baseSurfaceModeler.UpdateTerrainInfo();
                
                m_baseSurfaceModeler.ApplyExistedLandCoverMap();
                
                m_baseSurfaceModeler.DistributeTreesOnForest();
                TCP_Client.SendMessage("A [DONE] Apply Land Cover Map!");
                TCP_Client.SendMessage("A [DONE] Apply Height Map!");
                GlobalParams.loaded_predefined_label_map = true;
                GlobalParams.loaded_predefined_height_map = true;
            }
            else
            {
                TCP_Client.SendMessage("A [DONE] !!!!!!!!!!WARNING! Predefined Maps are Not Compatible!!!!!!!!!!");
            }
        }
        

        // 用自动生成的逻辑
        // v1: 印章算法生成地形
        if (!GlobalParams.loaded_predefined_label_map && !GlobalParams.loaded_predefined_height_map && global_params.terrain_noise_type == TerrainNoiseType._STAMP) 
        {
            // create lakes;
            {
                for (int i = 0; i < global_params.lake_num; ++i) 
                {
                    Vector2 center = new Vector2(Random.Range(0, global_params.length), Random.Range(0, global_params.width));
                    m_baseSurfaceModeler.MakeLakeAt(center, decay: 0.99997f, _override: false);
                }
            }
            TCP_Client.SendMessage("A [DONE] Create Lake.");

            // create rivers;
            {
                var stroke = m_baseSurfaceModeler.GetRandomRiverStroke(300, 10);
                m_baseSurfaceModeler.MakeRiverAt(stroke, riverRange:17.0f);

                // after making lakes, we need to shift this region vertically.
                m_baseSurfaceModeler.UpdateWaterHeightShiftFactor();   // 更新一下water_shift_factor，即每个点到水边的距离（权重）
                m_baseSurfaceModeler.ShiftDownWaterRegion_OnlyStampUse();   // 应用water_shift_factor，修改了一版本顶点的高度height。
            }
            TCP_Client.SendMessage("A [DONE] Create River.");

            // create cities;
            {
                TCP_Client.SendMessage("A [DONE] Create City layout Done.");

                for (int i = 0; i < 2; ++i) 
                {
                    Vector2 center = new Vector2(Random.Range(0, global_params.length), Random.Range(0, global_params.width));
                    BuildingType bType = Random.Range(0.0f, 1.0f) > 0.5f ? BuildingType._mansion : BuildingType._skyscraper;
                    m_baseSurfaceModeler.MakeCityAtWithLayoutGen(generator, center, bType, decay: 0.99999f, _override: false); // distribution manner (0,1,2,3,4).
                }
                m_baseSurfaceModeler.UpdateCityHeightShiftFactor();   // 仅仅更新了每个点的max_city_height_change_range。
            }
            TCP_Client.SendMessage("A [DONE] Create City.");

            // create mountains.
            {
                var stoke = m_baseSurfaceModeler.GetRandomStroke(Random.Range(18, 30), Random.Range(45, 90));
                m_baseSurfaceModeler.MakeMountainAt(stoke); // you need to implement this.

                m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                m_baseSurfaceModeler.UpdateTerrainInfo();
            }
            TCP_Client.SendMessage("A [DONE] Create Mountain.");

            // distribute trees on forest region
            {
                m_baseSurfaceModeler.DistributeTreesOnForest();
            }
            TCP_Client.SendMessage("A [DONE] Distribute Trees.");

            // ....
            // ....

            // savings
            //m_baseSurfaceModeler.SaveTerrainLabelMap();
            m_baseSurfaceModeler.SaveHeightShiftFactorMap();

            TCP_Client.SendMessage("A [DONE] GenTerrainMap-----Non_Noise_Version-----");
        }
        // v2: 随机生成整片区域的地形
        //      1. Fractal Perlin Noise
        //      2. Gradient Trick
        else if (!GlobalParams.loaded_predefined_label_map && !GlobalParams.loaded_predefined_height_map && global_params.terrain_noise_type != TerrainNoiseType._STAMP)
        {
            {   // base terrain
                m_baseSurfaceModeler.MakeNoiseBasedTerrainAt();

                m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
                m_baseSurfaceModeler.UpdateTerrainInfo();
            }
            TCP_Client.SendMessage("A [DONE] Generate Base Noise Terrain");

            {   // city distribution
                TCP_Client.SendMessage("A [DONE] Create City layout Done.");
            
                var randomCityNumber = Random.Range(1, 3);
                for (int i = 0; i < randomCityNumber; ++i)
                {
                    Vector2Int center = new Vector2Int();
                    bool foundCenter = false;
                    // while (true) 
                    // {
                    //     center = new Vector2Int(Random.Range(0, global_params.length), Random.Range(0, global_params.width));
                    //     float height = m_baseSurfaceModeler.m_vertexInfos[center.x][center.y].height;
                    //     if (height is < 16f and > 9f) 
                    //         break;
                    // }
              
                    // 尝试100次，如果还没找到就不生成城市了
                    for (int count = 0; count < 100; count++)
                    {
                        center = new Vector2Int(Random.Range(0, global_params.length), Random.Range(0, global_params.width));
                        float height = m_baseSurfaceModeler.m_vertexInfos[center.x][center.y].height;
                        if (height is < 16f and > 10f)
                        {
                            foundCenter = true;
                            break;
                        }
                    }

                    if (foundCenter)
                    {
                        BuildingType bType = Random.Range(0.0f, 1.0f) > 0.5f ? BuildingType._mansion : BuildingType._skyscraper;
                        m_baseSurfaceModeler.MakeCityAtWithLayoutGen(generator, (Vector2)center, bType, decay: 0.99999f, _override: false); // distribution manner (0,1,2,3,4).
                    }
                }
            
                m_baseSurfaceModeler.UpdateCityHeightShiftFactor();
            }
            TCP_Client.SendMessage("A [DONE] Create City.");
            
            // water regions
            {   // lake
                for (int i = 0; i < global_params.lake_num; ++i) 
                {
                    Vector2Int center;
                    while (true) 
                    {
                        center = new Vector2Int(Random.Range(0, global_params.length), Random.Range(0, global_params.width));
                        float height = m_baseSurfaceModeler.m_vertexInfos[center.x][center.y].height;
                        if (height < 16f) 
                            break;
                    }
            
                    m_baseSurfaceModeler.MakeLakeAt(center, decay: 0.99997f, _override: false);
                }
                
                m_baseSurfaceModeler.UpdateWaterHeightShiftFactor(); // after making lakes, we need to shift this region vertically.
                m_baseSurfaceModeler.ShiftDownWaterRegion();
            }
            TCP_Client.SendMessage("A [DONE] Create Lake Done.");
            
            // create rivers;
            {
                var stroke = m_baseSurfaceModeler.GetRandomRiverStroke(300, 10);
                m_baseSurfaceModeler.MakeRiverAt(stroke);
            
                m_baseSurfaceModeler.UpdateWaterHeightShiftFactor(); // after making lakes, we need to shift this region vertically.
                m_baseSurfaceModeler.ShiftDownWaterRegion();
            }
            TCP_Client.SendMessage("A [DONE] Create River.");
            
            m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
            m_baseSurfaceModeler.UpdateTerrainInfo();
            
            // distribute trees on forest region
            {
                m_baseSurfaceModeler.DistributeTreesOnForest();
            }
            TCP_Client.SendMessage("A [DONE] Distribute Trees.");
            
            m_baseSurfaceModeler.m_terrainInfos.min_height_pos = new Vector3(0.0f, -10.0f, 0.0f);
            
            m_baseSurfaceModeler.SaveHeightShiftFactorMap();
            
            Debug.Log("Real Scale Highest Evaluation : " + m_baseSurfaceModeler.m_terrainInfos.max_height_pos.y);
            Debug.Log("Real Scale Lowest Evaluation : " + m_baseSurfaceModeler.m_terrainInfos.min_height_pos.y);
        }
    }

    public void BuildFinalMesh(CityGenerator generator)
    {
        // destroy previously constructed gameobject.
        if (finalGameObject != null)
            UnityEngine.Object.Destroy(finalGameObject);

        // dataset generation mode
        if (GlobalParams.enable_dataset_mode) {
            m_baseSurfaceModeler.SaveTerrainLabelMap();
            m_baseSurfaceModeler.Get01HeightMapWithSmallHillInForest(); // increase the plain with perlin noise
            m_baseSurfaceModeler.SaveHeightMap();    // 生成图片用

            return;
        }

        finalGameObject = new GameObject("BaseTerrain");
        // then, create meshes for other terrain components.
        // m_baseSufaceModeler.GenerateBaseSurface(finalGameObject);

        // 为了让功能区先把树种类定义好，这里放在最后
        m_baseSurfaceModeler.GenerateBuiltInSurface(finalGameObject);

        // 如果为了加速可以暂时注释掉这些。
        // 如果提前仅仅定义了高度图，也不需要这些，用纯色
        // 如果提前定义了label图，则需要
        if (!GlobalParams.loaded_predefined_height_map || GlobalParams.loaded_predefined_label_map)
        {
            m_baseSurfaceModeler.GenerateFunctionalAreas(generator, finalGameObject);

            // 摆放树木、草、石头等object
            GameObject temp_terrainObject = GameObject.Find("BaseSurface");
            temp_terrain = temp_terrainObject.GetComponent<Terrain>();
           
            m_baseSurfaceModeler.setupTerrainDetails(temp_terrain.terrainData);
            
            m_baseSurfaceModeler.GenerateBuildings(generator, finalGameObject);
            m_baseSurfaceModeler.GenerateWaterRegion(finalGameObject);
            m_baseSurfaceModeler.GenerateRoadRegion(finalGameObject, generator);
            // m_baseSurfaceModeler.GenerateForest(finalGameObject);
        }

        finalGameObject.transform.localScale *= global_params.global_scaling; // for acquiring better rendering quality.

        // savings.
        m_baseSurfaceModeler.SaveHeightMap();    // 生成高度图用
        m_baseSurfaceModeler.SaveHeightPlainUse();    // 生成高度图用
        m_baseSurfaceModeler.SaveTerrainLabelMap(); // 生成mask color 图用

        TCP_Client.SendMessage(string.Format("A -------------------Terrain Info-----------------------:\n"));
        TCP_Client.SendMessage(string.Format("A Lowest={0}, Highest={1}",
           m_baseSurfaceModeler.m_terrainInfos.min_height_pos.y, m_baseSurfaceModeler.m_terrainInfos.max_height_pos.y));
        TCP_Client.SendMessage("A F1:WATER | F2: TREE | F3: TEXTURE | F4: ROAD | F5: REDO CITY |");

    }

    private float GetDeltaBetweenColors(Color a, Color b)
    {
        float res = Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        return res;
    }

    public void LoadFromExistingLayout(string filename)
    {
        m_baseSurfaceModeler.ClearAll();

        // you should always call this function in the beginning.
        m_baseSurfaceModeler.InitBaseSurface(defaultType: TerrainType._FOREST);
        
        {
            byte[] fileData = System.IO.File.ReadAllBytes("C:/Users/liuzh/Downloads/100_Zelda level/lzh_terrain_data/box_dataset/4.png");
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            TCP_Client.SendMessage(string.Format("A Image size: {0} {1}", tex.width, tex.height));

            //tex.Reinitialize(global_params.length, global_params.width);

            Dictionary<TerrainType, Color> dic_colors = new Dictionary<TerrainType, Color>();
            dic_colors[TerrainType._GRASS] = new Color(0.1f, 0.5f, 0.1f);
            dic_colors[TerrainType._WATER] = new Color(0.1f, 0.1f, 0.8f);
            dic_colors[TerrainType._FOREST] = new Color(0.1f, 0.8f, 0.3f);
            dic_colors[TerrainType._SNOW] = new Color(0.8f, 0.8f, 0.8f);
            dic_colors[TerrainType._CITY] = new Color(0.5f, 0.2f, 0.2f);

            for (int x = 0; x < global_params.length; x++)
            {
                for (int y = 0; y < global_params.width; y++)
                {
                    var pixel = tex.GetPixel(x, y);
                    //TCP_Client.SendMessage(string.Format("A {0} {1} {2}", pixel.r, pixel.g, pixel.b));
                    TerrainType closestType = TerrainType._FOREST;
                    float minColorDelta = float.MaxValue;

                    float detlaWater = GetDeltaBetweenColors(pixel, dic_colors[TerrainType._WATER]);
                    float deltaSnow = GetDeltaBetweenColors(pixel, dic_colors[TerrainType._SNOW]);
                    float detlaCity = GetDeltaBetweenColors(pixel, dic_colors[TerrainType._CITY]);
                    if (detlaWater < minColorDelta && detlaWater < 0.2f)
                    {
                        minColorDelta = detlaWater;
                        closestType = TerrainType._WATER;
                    }
                    if (deltaSnow < minColorDelta && deltaSnow < 0.2f)
                    {
                        minColorDelta = deltaSnow;
                        closestType = TerrainType._SNOW;
                    }
                    if (detlaCity < minColorDelta && detlaCity < 0.2f)
                    {
                        minColorDelta = detlaCity;
                        closestType = TerrainType._CITY;
                    }

                    m_baseSurfaceModeler.m_vertexInfos[x][y].label = closestType;
                }
            }
        }


        {
            m_baseSurfaceModeler.UpdateWaterHeightShiftFactor();  // after making lakes, we need to shift this region vertically.
            m_baseSurfaceModeler.ShiftDownWaterRegion();
            m_baseSurfaceModeler.UpdateCityHeightShiftFactor();

            m_baseSurfaceModeler.MakeMountainFromLayout();
            m_baseSurfaceModeler.CalculateNormalsOverEntireMap();
            m_baseSurfaceModeler.UpdateTerrainInfo();
            
            m_baseSurfaceModeler.DistributeTreesOnForest();
        }
    }

    public void RandomizeTerrainParameters()
    {   
        global_params.length = Random.Range(600, 601);
        global_params.width = Random.Range(600, 601);

        // global_params.bs_freq = Random.Range(0.005f, 0.04f);
        // global_params.bs_amplitude = Random.Range(40.0f, 100.0f);
        
    }

    public bool ChangeTerrainForHeightShow()
    {
        GameObject temp_lakeObject = GameObject.Find("Lake");
        GameObject temp_roadObject = GameObject.Find("RoadMaps");
        GameObject temp_functionAreaObject = GameObject.Find("FuncAreas");
        GameObject temp_BldgsObject = GameObject.Find("BldgComplex");
        temp_lakeObject.SetActive(false);
        temp_roadObject.SetActive(false);
        temp_functionAreaObject.SetActive(false);
        temp_BldgsObject.SetActive(false);
        
        GameObject temp_terrainObject = GameObject.Find("BaseSurface");
        if (temp_terrainObject == null) return false;

        temp_terrain = temp_terrainObject.GetComponent<Terrain>();

        float[,,] heightShowAlphaMap = m_baseSurfaceModeler.GetHeightTextureMaskMap();
        TerrainData temp_terrain_data = temp_terrain.terrainData;
        temp_terrain_data.SetAlphamaps(0, 0, heightShowAlphaMap);

        Terrain.activeTerrain.terrainData = temp_terrain.terrainData;
        Terrain.activeTerrain.Flush();

        List<TreeInstance> temp_treeInstance = temp_terrain_data.treeInstances.ToList();
        for (int i = 0; i < temp_treeInstance.Count; i++)
        {
            TreeInstance tree = temp_treeInstance[i];
            tree.heightScale = 0.0f; // 设置为透明
            tree.widthScale = 0.0f;
            temp_treeInstance[i] = tree;
        }

        temp_terrain_data.treeInstances = temp_treeInstance.ToArray();
        temp_terrain_data.RefreshPrototypes();
        
        return true;
    }

    public void ResetWaterMesh()
    {

        GameObject rootObject = GameObject.Find("Lake");



        foreach (Transform child in rootObject.transform)
        {
            GameObject obj = child.gameObject;
            var waterSurface = obj.GetComponent<UnityEngine.Rendering.HighDefinition.WaterSurface>();


            waterSurface.refractionColor = global_params.water_refracting_color; // deeper
            waterSurface.scatteringColor = global_params.water_scattering_color;
            waterSurface.absorptionDistance = global_params.water_depth;
            waterSurface.timeMultiplier = 0.0f;
        }
        
    }

    public void ResetTrees(CityGenerator generator)
    {
        temp_terrain.terrainData.treeInstances = new TreeInstance[0];
        m_baseSurfaceModeler.DistributeTreesOnForest();   // 按照分布，给野外加上森林。

        GameObject obj = GameObject.Find("BaseTerrain");
        GameObject childObject = GameObject.Find("FuncAreas");

        UnityEngine.Object.Destroy(childObject);
        m_baseSurfaceModeler.GenerateFunctionalAreas(generator, obj);   // 给城镇内的功能区加上森林。



        m_baseSurfaceModeler.setupTerrainDetails(temp_terrain.terrainData);  // 实际生成森林的内容。
    }

    public void ResetSurfaceTextures()
    {
        // alphaMap就是对应着地形的texture的分布
        float[,,] alphaMap = m_baseSurfaceModeler.GetTextureMaskMap();
        this.temp_terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
    }

    public void ResetRoadMesh(CityGenerator generator)
    {
        GameObject childObject = GameObject.Find("RoadMaps");
        UnityEngine.Object.Destroy(childObject);
        m_baseSurfaceModeler.GenerateRoadRegion(finalGameObject, generator);
    }

    public void ResetCityStuff(CityGenerator generator)
    {
        GameObject childObjectRoadmaps = GameObject.Find("RoadMaps");
        UnityEngine.Object.Destroy(childObjectRoadmaps);
        m_baseSurfaceModeler.GenerateRoadRegion(finalGameObject, generator);

        GameObject childObjectBldg = GameObject.Find("BldgComplex");
        UnityEngine.Object.Destroy(childObjectBldg);
        m_baseSurfaceModeler.GenerateBuildings(generator, finalGameObject);

        GameObject obj = GameObject.Find("BaseTerrain");
        GameObject childObject = GameObject.Find("FuncAreas");
        UnityEngine.Object.Destroy(childObject);
        m_baseSurfaceModeler.GenerateFunctionalAreas(generator, obj);   // 给城镇内的功能区加上森林。
        m_baseSurfaceModeler.setupTerrainDetails(temp_terrain.terrainData, isOnlyRedoParkTrees:true);  // 实际生成森林的内容。
        temp_terrain.Flush();
    }

    public void ActivateNightLighting()
    {
        global_params.open_night_light = !global_params.open_night_light;

        if(global_params.open_night_light == true)
        {
            GameObject childObjectBldg = GameObject.Find("BldgComplex");
            m_baseSurfaceModeler.GenerateNightLightingInCity(childObjectBldg, finalGameObject);
            GameObject childObjectLight = GameObject.Find("LightComplex");
            childObjectLight.transform.localScale /= global_params.global_scaling;
        }
        else
        {
            GameObject childObjectLight = GameObject.Find("LightComplex");
            childObjectLight.SetActive(false);
            UnityEngine.Object.Destroy(childObjectLight);
        }
    }

    public void RegenerateTexturesUnderQtControl(int textureFlags)
    {
        m_baseSurfaceModeler.loadUserDefinedTextureLayer(textureFlags);
    }

    public void RegenerateTreesUnderQtControl()
    {
        temp_terrain.terrainData.treeInstances = new TreeInstance[0];
        m_baseSurfaceModeler.setupTerrainDetails(temp_terrain.terrainData);  // 实际生成森林的内容。
        temp_terrain.Flush();
    }
}
