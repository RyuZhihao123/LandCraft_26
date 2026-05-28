using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


/// <summary>
/// Distribution related functions
/// </summary>

partial class BaseSurfaceModeler
{
    // Apply "Lazy-Flood-Fill algorithm" to current terrain with a certain TerrainType.
    public List<Vector2> MakeLazyFloodFillDistributionAt(
        GlobalParams global_params,
        Vector2 center, TerrainType type, 
        float decay = 0.99999f,
        bool _override = true)
    {
        float chance = 100;
        Vector2Int[] offsets = new Vector2Int[4]
        {
            new Vector2Int(0, -1), new Vector2Int(0, 1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        var length = global_params.length;
        var width = global_params.width;

        bool[,] visited = new bool[length, width];

        for (int x = 0; x < length; x++)
        for (int y = 0; y < width; y++)
            visited[x, y] = false;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        List<Vector2> visited_pool = new List<Vector2>();
        queue.Enqueue(new Vector2Int((int)center.x, (int)center.y));

        while (queue.Count != 0)
        {
            var curPos = queue.Dequeue();
            visited[curPos.x, curPos.y] = true;
            
            if (_override == true || // 如果允许覆写
                (_override == false &&
                 m_vertexInfos[curPos.x][curPos.y].label ==
                 LocalVertexInfo.default_terrain_type)) // 不允许覆写，但是呢这个位置是没有被设置过的。
            {
                m_vertexInfos[curPos.x][curPos.y].label = type;
                visited_pool.Add(curPos);
            }

            if (chance >= Random.Range(1, 100))
            {
                for (int i = 0; i < 4; ++i)
                {
                    Vector2Int nextPos = curPos + offsets[i];

                    if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= length || nextPos.y >= width)
                        continue;
                    if (visited[nextPos.x, nextPos.y] == true)
                        continue;

                    // 噪声基础地形模式，要基于高度来蔓延
                    if (global_params.terrain_noise_type != TerrainNoiseType._STAMP)
                    {
                        float height = m_vertexInfos[curPos.x][curPos.y].height;

                        if ((height is > 14f) && type == TerrainType._CITY)
                            continue;

                        if (height > 13f && type == TerrainType._WATER)
                            continue;
                    }

                    visited[nextPos.x, nextPos.y] = true;
                    queue.Enqueue(nextPos);
                }
            }

            chance *= decay;
        }

        return visited_pool;
    }


    //public List<Vector2> MakeMountainAlongStroke(List<BaseAlgorithm.Line2D> stroke,
    //    float mountRange = 60.0f, float max_height = 70.0f, bool _override = true)
    //{
    //    var length = global_params.length;
    //    var width = global_params.width;

    //    List<Vector2> positions = new List<Vector2>();

    //    List<Rect> rects = new List<Rect>();
    //    List<float> peakHeights = new List<float>();

    //    for (int i = 0; i < stroke.Count; i++)
    //    {
    //        var rect = new Rect();
    //        rect.xMin = Mathf.Min(stroke[i].pt1.x, stroke[i].pt2.x) - mountRange;
    //        rect.yMin = Mathf.Min(stroke[i].pt1.y, stroke[i].pt2.y) - mountRange;
    //        rect.xMax = Mathf.Max(stroke[i].pt1.x, stroke[i].pt2.x) + mountRange;
    //        rect.yMax = Mathf.Max(stroke[i].pt1.y, stroke[i].pt2.y) + mountRange;
    //        rects.Add(rect);

    //        peakHeights.Add(Random.Range(0.7f * max_height, max_height));
    //    }

    //    for (int x = 0; x < length; ++x)
    //    {
    //        for (int y = 0; y < width; ++y)
    //        {
    //            Vector2 pt = new Vector2(x, y);

    //            if ((m_vertexInfos[x][y].label == TerrainType._CITY ||
    //                 m_vertexInfos[x][y].label == TerrainType._WATER))
    //                continue;

    //            float minDist = float.MaxValue;
    //            int strokeID = -1;
    //            for(int i=0; i<stroke.Count; ++i)  // check each line segment;
    //            {
    //                if (!rects[i].Contains(pt))
    //                    continue;

    //                float dist, param;
    //                (dist, param) = BaseAlgorithm.DistanceFromPointToLineSeg(pt, stroke[i].pt1, stroke[i].pt2);
    //                if (dist >= mountRange)
    //                    continue;

    //                if(dist < minDist)
    //                {
    //                    minDist = dist;
    //                    strokeID = i;
    //                }
    //            }

    //            if (strokeID == -1)
    //                continue;

    //            positions.Add(pt);
    //            float heightShift = mountRange - minDist;

    //            float edgeFactor1 = m_vertexInfos[x][y].city_height_shift_factor / (float) LocalVertexInfo.max_city_height_change_range;
    //            float edgeFactor2 = m_vertexInfos[x][y].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;
    //            float edgeFactor = Mathf.Min(edgeFactor1, edgeFactor2);

    //            m_vertexInfos[x][y].height += edgeFactor * peakHeights[strokeID] * heightShift / mountRange;

    //            if (m_vertexInfos[x][y].height > 0.9f * peakHeights[strokeID])
    //                m_vertexInfos[x][y].label = TerrainType._SNOW;
    //            else if (0.4f * peakHeights[strokeID] <= m_vertexInfos[x][y].height && m_vertexInfos[x][y].height <= 0.9f * peakHeights[strokeID])
    //            {
    //                m_vertexInfos[x][y].label = TerrainType._GRASS;

    //                // put some pine trees;
    //                if (Random.Range(0.0f, 1.0f) < 0.02f)
    //                    m_vertexInfos[x][y].plant_type = PlantType._pineTree;
    //            }
    //            else
    //                m_vertexInfos[x][y].label = LocalVertexInfo.default_terrain_type;
    //        }
    //    }

    //    return positions;
    //}


    public List<Vector2> MakeMountainAlongStroke(List<BaseAlgorithm.Line2D> stroke,
        float mountRange = 60.0f, float max_height = 70.0f, bool _override = true)
    {
        var length = global_params.length;
        var width = global_params.width;

        List<Vector2> positions = new List<Vector2>();

        List<Rect> rects = new List<Rect>();
        List<float> peakHeights = new List<float>();

        for (int i = 0; i < stroke.Count; i++)
        {
            var rect = new Rect();
            rect.xMin = Mathf.Min(stroke[i].pt1.x, stroke[i].pt2.x) - mountRange;
            rect.yMin = Mathf.Min(stroke[i].pt1.y, stroke[i].pt2.y) - mountRange;
            rect.xMax = Mathf.Max(stroke[i].pt1.x, stroke[i].pt2.x) + mountRange;
            rect.yMax = Mathf.Max(stroke[i].pt1.y, stroke[i].pt2.y) + mountRange;
            rects.Add(rect);

            peakHeights.Add(Random.Range(0.7f * max_height, max_height));
        }

        int mountRangeInt = (int)mountRange;
        for (int i = 0; i < stroke.Count; ++i)
        {
            int centerX = (int)stroke[i].pt1.x;
            int centerY = (int)stroke[i].pt1.y;

            Texture2D stamp = Resources.Load<Texture2D>("TerrainTextures/Stamps/1/4K Hills 4");

            for (int deltax = -mountRangeInt; deltax < +mountRangeInt; ++deltax)
            {
                for (int deltay = -mountRangeInt; deltay < +mountRangeInt; ++deltay)
                {
                    int x = centerX + deltax;
                    int y = centerY + deltay;
                    if (x < 0 || y < 0 || x >= length || y >= width)
                        continue;

                    if ((m_vertexInfos[x][y].label == TerrainType._CITY ||
                         m_vertexInfos[x][y].label == TerrainType._WATER))
                        continue;

                    positions.Add(new Vector2(x, y));


                    float heightFactor = 0.4f * stamp.GetPixel(
                        (int)((deltax + mountRange) / (2 * mountRange) * stamp.width),
                        (int)((deltay + mountRange) / (2 * mountRange) * stamp.height)).r;

                    float edgeFactor1 = m_vertexInfos[x][y].city_height_shift_factor /
                                        (float)LocalVertexInfo.max_city_height_change_range;
                    float edgeFactor2 = m_vertexInfos[x][y].water_height_shift_factor /
                                        (float)LocalVertexInfo.max_water_height_change_range;
                    float edgeFactor = Mathf.Min(edgeFactor1, edgeFactor2);

                    m_vertexInfos[x][y].height += edgeFactor * peakHeights[i] * heightFactor;

                    //m_vertexInfos[x][y].label = LocalVertexInfo.default_terrain_type;

                    if (m_vertexInfos[x][y].height > LocalVertexInfo.snowline_height)
                    {
                        m_vertexInfos[x][y].label = TerrainType._SNOW;
                        // m_vertexInfos[x][y].plant_type = PlantType._none;
                        m_vertexInfos[x][y].building_type = BuildingType._none;
                    }
                    else if (0.6f * LocalVertexInfo.snowline_height <= m_vertexInfos[x][y].height
                             && m_vertexInfos[x][y].height <= 0.8f * LocalVertexInfo.snowline_height)
                    {
                        m_vertexInfos[x][y].label = TerrainType._FOREST; // 这里从Grass改成FOREST，舍弃GRASS了
                        m_vertexInfos[x][y].building_type = BuildingType._none;

                        // put some pine trees;
                        if (Random.Range(0.0f, 1.0f) < 0.002f)
                            m_vertexInfos[x][y].plant_type = PlantType._pineTree;
                    }
                    else
                    {
                        m_vertexInfos[x][y].label = LocalVertexInfo.default_terrain_type;
                        m_vertexInfos[x][y].building_type = BuildingType._none;
                    }
                }
            }
        }

        return positions;
    }


    public void MakeNoiseBasedTerrainMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        // 这个可以指定生成高度图的类型
        // 1. _FRAC_PERLIN
        // 2. _GRADIENT
        // 3. _DLA
        List<List<float>> noiseMap = new List<List<float>>(length);
        for (int i = 0; i < length; i++)
        {
            noiseMap.Add(new List<float>(width));
        }

        switch (global_params.terrain_noise_type)
        {
            case TerrainNoiseType._FRAC_PERLIN:
                noiseMap = BaseSurfaceNoise.GeneratePerlinNoiseMap(global_params);
                break;
            case TerrainNoiseType._GRADIENT:
                noiseMap = BaseSurfaceNoise.GenerateGradientTrickNoiseMap(global_params);
                break;
            case TerrainNoiseType._DLA:
                // 目前不用这个DLA算法
                noiseMap = BaseSurfaceNoise.GenerateGradientTrickNoiseMap(global_params);
                break;
        }

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                m_vertexInfos[x][y].height += noiseMap[x][y];

                // m_vertexInfos[x][y].label = TerrainType._FOREST;
            }
        }
    }


    public void MakeMountainFromLayout()
    {
        var length = global_params.length;
        var width = global_params.width;

        bool[,] visited = new bool[length, width];

        Vector2Int[] offsets = new Vector2Int[8];
        offsets[0] = new Vector2Int(0, 1);
        offsets[1] = new Vector2Int(1, 0);
        offsets[2] = new Vector2Int(-1, 0);
        offsets[3] = new Vector2Int(0, -1);
        offsets[4] = new Vector2Int(1, 1);
        offsets[5] = new Vector2Int(-1, -1);
        offsets[6] = new Vector2Int(-1, 1);
        offsets[7] = new Vector2Int(1, -1);

        //Texture2D stamp = Resources.Load<Texture2D>("TerrainTextures/Stamps/Mesas/4k Mesa 4");
        Texture2D stamp = Resources.Load<Texture2D>(string.Format("TerrainTextures/Stamps/{0}/{1}", Random.Range(1, 2 + 1), Random.Range(1, 4 + 1)));


        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                if (visited[x, y] == false && m_vertexInfos[x][y].isMountainArea == true)
                {

                    List<Vector2Int> hillPts = new List<Vector2Int>();
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));

                    while (queue.Count != 0)
                    {
                        Vector2Int curPos = queue.Dequeue();

                        visited[curPos.x, curPos.y] = true;
                        hillPts.Add(curPos);

                        foreach (var offset in offsets)
                        {

                            Vector2Int nextPos = curPos + offset;

                            if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= length || nextPos.y >= width)
                                continue;
                            if (visited[nextPos.x, nextPos.y] == true)
                                continue;
                            if (m_vertexInfos[nextPos.x][nextPos.y].isMountainArea != true)
                                continue;
                            visited[nextPos.x, nextPos.y] = true;
                            queue.Enqueue(nextPos);
                        }
                    }
                    
                    if (hillPts.Count > 50 * 50)
                    {
                        // Bounding box
                        float minX = float.MaxValue,
                            minY = float.MaxValue,
                            maxX = float.MinValue,
                            maxY = float.MinValue;

                        foreach (var pt in hillPts)
                        {
                            minX = Mathf.Min(minX, pt.x);
                            minY = Mathf.Min(minY, pt.y);
                            maxX = Mathf.Max(maxX, pt.x);
                            maxY = Mathf.Max(maxY, pt.y);
                        }

                        float heightFactor = hillPts.Count / 300.0f;
                        heightFactor = Mathf.Min(50, heightFactor);

                        foreach (var pt in hillPts)
                        {
                            int x_in_stamp = (int)(stamp.width * (pt.x - minX) / (maxX - minX));
                            int y_in_stamp = (int)(stamp.height * (pt.y - minY) / (maxY - minY));
                            x_in_stamp = x_in_stamp < 0 ? 0 : x_in_stamp;
                            y_in_stamp = y_in_stamp < 0 ? 0 : y_in_stamp;
                            x_in_stamp = x_in_stamp >= stamp.width ? (stamp.width - 1) : x_in_stamp;
                            y_in_stamp = y_in_stamp >= stamp.height ? (stamp.height - 1) : y_in_stamp;



                            float edgeFactor1 = m_vertexInfos[x][y].city_height_shift_factor /
                                                (float)LocalVertexInfo.max_city_height_change_range;
                            float edgeFactor2 = m_vertexInfos[x][y].water_height_shift_factor /
                                                (float)LocalVertexInfo.max_water_height_change_range;
                            float edgeFactor = Mathf.Min(edgeFactor1, edgeFactor2);

                            //m_vertexInfos[x][y].height = edgeFactor * peakHeights[i] * heightFactor;

                            m_vertexInfos[(int)pt.x][(int)pt.y].height +=
                                edgeFactor * heightFactor * stamp.GetPixel(x_in_stamp, y_in_stamp).r;
                        }

                        // Æ½»¬Ò»ÏÂ¹ý¶È¼âÈñµÄµØ·½
                        for (int i = 0; i < 100; ++i)
                        {
                            foreach (var pt in hillPts)
                            {
                                float avg_delta_height = 0;
                                float avg_height = 0;
                                int count = 0;
                                float centerHeight = m_vertexInfos[pt.x][pt.y].height;
                                for (int delta_x = -1; delta_x <= 1; ++delta_x)
                                {
                                    for (int delta_y = -1; delta_y <= 1; ++delta_y)
                                    {
                                        var nextPos = pt + new Vector2Int(delta_x, delta_y);

                                        if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= length || nextPos.y >= width)
                                            continue;
                                        count++;
                                        avg_delta_height +=
                                            Mathf.Abs(m_vertexInfos[nextPos.x][nextPos.y].height - centerHeight);
                                        avg_height += m_vertexInfos[nextPos.x][nextPos.y].height;
                                    }
                                }

                                if (count == 0)
                                    continue;
                                avg_height /= count;
                                avg_delta_height /= count;

                                //if (avg_delta_height > 5)
                                {
                                    m_vertexInfos[pt.x][pt.y].height = avg_height;
                                }
                            }
                        } 

                        // ÔÙµü´úÒ»´ÎÏ¸½Ú
                        foreach (var pt in hillPts)
                        {
                            int x_in_stamp = (int)(stamp.width * (pt.x - minX) / (maxX - minX));
                            int y_in_stamp = (int)(stamp.height * (pt.y - minY) / (maxY - minY));
                            x_in_stamp = x_in_stamp < 0 ? 0 : x_in_stamp;
                            y_in_stamp = y_in_stamp < 0 ? 0 : y_in_stamp;
                            x_in_stamp = x_in_stamp >= stamp.width ? (stamp.width - 1) : x_in_stamp;
                            y_in_stamp = y_in_stamp >= stamp.height ? (stamp.height - 1) : y_in_stamp;



                            //m_vertexInfos[x][y].height = edgeFactor * peakHeights[i] * heightFactor;

                            m_vertexInfos[(int)pt.x][(int)pt.y].height +=
                                60.0f * stamp.GetPixel(x_in_stamp, y_in_stamp).r;

                            //// ¼ÓÉÏÒ»Ð©Ê÷°É
                            //if (m_vertexInfos[pt.x][pt.y].height > LocalVertexInfo.snowline_height)
                            //{
                            //    m_vertexInfos[pt.x][pt.y].label = TerrainType._SNOW;
                            //    m_vertexInfos[pt.x][pt.y].plant_type = PlantType._none;
                            //    m_vertexInfos[pt.x][pt.y].building_type = BuildingType._none;
                            //}
                            //else if (0.6f * LocalVertexInfo.snowline_height <= m_vertexInfos[pt.x][pt.y].height
                            //         && m_vertexInfos[pt.x][pt.y].height <= 0.8f * LocalVertexInfo.snowline_height)
                            //{
                            //    m_vertexInfos[pt.x][pt.y].label = TerrainType._GRASS;
                            //    m_vertexInfos[pt.x][pt.y].building_type = BuildingType._none;

                            //    // put some pine trees;
                            //    if (Random.Range(0.0f, 1.0f) < 0.02f)
                            //        m_vertexInfos[pt.x][pt.y].plant_type = PlantType._pineTree;
                            //}
                            //else
                            //{
                            //    m_vertexInfos[pt.x][pt.y].label = LocalVertexInfo.default_terrain_type;
                            //    m_vertexInfos[pt.x][pt.y].building_type = BuildingType._none;
                            //}
                        }



                    }
                    else
                    {

                    }

                }
            }
        }


    }


    public List<Vector2> MakeRiverAlongStroke(List<BaseAlgorithm.Line2D> stroke, float riverRange = 25.0f)
    {
        var length = global_params.length;
        var width = global_params.width;

        var positions = new List<Vector2>();
        var rects = new List<Rect>();

        for (int i = 0; i < stroke.Count; i++)
        {
            var rect = new Rect();
            rect.xMin = Mathf.Min(stroke[i].pt1.x, stroke[i].pt2.x) - riverRange + Random.Range(-3, 3);
            rect.yMin = Mathf.Min(stroke[i].pt1.y, stroke[i].pt2.y) - riverRange + Random.Range(-3, 3);
            rect.xMax = Mathf.Max(stroke[i].pt1.x, stroke[i].pt2.x) + riverRange + Random.Range(-3, 3);
            rect.yMax = Mathf.Max(stroke[i].pt1.y, stroke[i].pt2.y) + riverRange + Random.Range(-3, 3);
            rects.Add(rect);
        }

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                var pt = new Vector2(x, y);

                float minDist = float.MaxValue;
                int strokeID = -1;
                for (int i = 0; i < stroke.Count; ++i)
                {
                    if (!rects[i].Contains(pt))
                        continue;

                    float dist;
                    (dist, _) = BaseAlgorithm.DistanceFromPointToLineSeg(pt, stroke[i].pt1, stroke[i].pt2);
                    if (dist >= riverRange)
                        continue;

                    if (dist < minDist)
                    {
                        minDist = dist;
                        strokeID = i;
                    }
                }

                if (strokeID == -1)
                    continue;

                positions.Add(pt);
                m_vertexInfos[x][y].label = TerrainType._WATER;
            }
        }

        return positions;
    }


    public void ApplyPerlinNoise(List<Vector2> positions)
    {

        float shift_x = Random.Range(-2000.0f, 2000.0f);
        float shift_y = Random.Range(-2000.0f, 2000.0f);

        foreach (var pt in positions)
        {
            float height = 5.0f * Perlin.Noise(pt.x * 0.1f + shift_x, pt.y * 0.1f + shift_y);
            height += Perlin.Noise(pt.x * 0.02f + shift_x, pt.y * 0.02f + shift_y);
            m_vertexInfos[(int)pt.x][(int)pt.y].height += height;
        }
    }


    public void ConstructCityLayoutOverRegion(List<Vector2> positions, BuildingType bType = BuildingType._mansion)
    {
        // It must be with a "city" label first, then has possibility to have a building.
        int xmin, xmax, ymin, ymax;
        xmin = ymin = int.MaxValue;
        xmax = ymax = int.MinValue;

        var length = global_params.length;
        var width = global_params.width;
        bool[,] mask = new bool[length, width];

        for (int x = 0; x < length; ++x)
        for (int y = 0; y < width; ++y)
            mask[x, y] = false;

        for (int i = 0; i < positions.Count; ++i)
        {
            xmin = Mathf.Min(xmin, (int)positions[i].x);
            ymin = Mathf.Min(ymin, (int)positions[i].y);
            xmax = Mathf.Max(xmax, (int)positions[i].x);
            ymax = Mathf.Max(ymax, (int)positions[i].y);

            mask[(int)positions[i].x, (int)positions[i].y] = true;
        }


        int margin = 17;
        int road_scale = (int)(0.3f * margin);
        int block_xlen = 25;
        int block_ylen = 25;

        for (int y = ymin; y <= ymax; y += (margin + block_ylen))
        {
            for (int x = xmin; x <= xmax;)
            {
                for (int dx = 0; dx <= block_xlen; dx += 10)
                {
                    int x1 = x + dx, y1 = y, y2 = y + block_ylen;
                    if (x1 >= 0 && y1 >= 0 && x1 < length && y1 < width
                        && m_vertexInfos[x1][y1].label == TerrainType._CITY
                        && mask[x1, y1] == true)
                    {
                        if (Random.Range(0.0f, 1.0f) > 0.2f)
                        {
                            m_vertexInfos[x1][y1].building_type = Random.Range(0.0f, 1.0f) > 0.2
                                ? BuildingType._mansion
                                : BuildingType._skyscraper;
                            m_vertexInfos[x1][y1].plant_type = PlantType._none;
                        }
                        else
                        {
                            m_vertexInfos[x1][y1].building_type = BuildingType._none;
                            m_vertexInfos[x1][y1].plant_type = Random.Range(0.0f, 1.0f) > 0.3f
                                ? PlantType._greenTrees
                                : PlantType._redTrees;
                        }
                    }

                    if (x1 >= 0 && y2 >= 0 && x1 < length && y2 < width
                        && m_vertexInfos[x1][y2].label == TerrainType._CITY
                        && mask[x1, y2] == true)
                    {
                        if (Random.Range(0.0f, 1.0f) > 0.2f)
                        {
                            m_vertexInfos[x1][y2].building_type = Random.Range(0.0f, 1.0f) > 0.2
                                ? BuildingType._mansion
                                : BuildingType._skyscraper;
                            ;
                            m_vertexInfos[x1][y2].plant_type = PlantType._none;
                        }
                        else
                        {
                            m_vertexInfos[x1][y2].building_type = BuildingType._none;
                            m_vertexInfos[x1][y2].plant_type = Random.Range(0.0f, 1.0f) > 0.3f
                                ? PlantType._greenTrees
                                : PlantType._redTrees;
                        }
                    }
                }

                for (int dy = 0; dy <= block_ylen; dy += 10)
                {
                    int x1 = x, x2 = x + block_xlen, y1 = y + dy;
                    if (x1 >= 0 && y1 >= 0 && x1 < length && y1 < width
                        && m_vertexInfos[x1][y1].label == TerrainType._CITY
                        && mask[x1, y1] == true)
                    {
                        if (Random.Range(0.0f, 1.0f) > 0.2f)
                        {
                            m_vertexInfos[x1][y1].building_type = Random.Range(0.0f, 1.0f) > 0.2
                                ? BuildingType._mansion
                                : BuildingType._skyscraper;
                            ;
                            m_vertexInfos[x1][y1].plant_type = PlantType._none;
                        }
                        else
                        {
                            m_vertexInfos[x1][y1].building_type = BuildingType._none;
                            m_vertexInfos[x1][y1].plant_type = Random.Range(0.0f, 1.0f) > 0.3f
                                ? PlantType._greenTrees
                                : PlantType._redTrees;
                        }
                    }

                    if (x2 >= 0 && y1 >= 0 && x2 < length && y1 < width
                        && m_vertexInfos[x2][y1].label == TerrainType._CITY
                        && mask[x2, y1] == true)
                    {
                        if (Random.Range(0.0f, 1.0f) > 0.2f)
                        {
                            m_vertexInfos[x2][y1].building_type = Random.Range(0.0f, 1.0f) > 0.2
                                ? BuildingType._mansion
                                : BuildingType._skyscraper;
                            ;
                            m_vertexInfos[x2][y1].plant_type = PlantType._none;
                        }
                        else
                        {
                            m_vertexInfos[x2][y1].building_type = BuildingType._none;
                            m_vertexInfos[x2][y1].plant_type = Random.Range(0.0f, 1.0f) > 0.3f
                                ? PlantType._greenTrees
                                : PlantType._redTrees;
                        }
                    }
                }

                for (int dx = block_xlen + road_scale; dx <= (block_xlen + margin - road_scale); dx += 1)
                {
                    for (int dy = -road_scale; dy <= (block_ylen + margin - road_scale); dy += 1)
                    {
                        int x1 = x + dx, y1 = y + dy;
                        if (x1 >= 0 && y1 >= 0 && x1 < length && y1 < width
                            && m_vertexInfos[x1][y1].label == TerrainType._CITY
                            && mask[x1, y1] == true)
                            m_vertexInfos[x1][y1].isRoad = true;

                    }
                }

                for (int dy = block_ylen + road_scale; dy <= (block_ylen + margin - road_scale); dy += 1)
                {
                    for (int dx = -road_scale; dx <= (block_xlen + margin - road_scale); dx += 1)
                    {
                        int x1 = x + dx, y1 = y + dy;
                        if (x1 >= 0 && y1 >= 0 && x1 < length && y1 < width
                            && m_vertexInfos[x1][y1].label == TerrainType._CITY
                            && mask[x1, y1] == true)
                            m_vertexInfos[x1][y1].isRoad = true;
                    }
                }

                x += (margin + block_xlen);
                block_xlen = Random.Range(20, 50);
            }
        }
    }


    public void ConstructCityUsingLayoutGeneration(CityGenerator generator, List<Vector2> positions,
        BuildingType bType = BuildingType._mansion)
    {
        var length = global_params.length;
        var width = global_params.width;

        foreach (var roadEdge in generator.roadGraph.MajorEdges)
        {
            int minX = (int)Mathf.Min(roadEdge.NodeA.X, roadEdge.NodeB.X);
            int minY = (int)Mathf.Min(roadEdge.NodeA.Y, roadEdge.NodeB.Y);
            int maxX = (int)Mathf.Max(roadEdge.NodeA.X, roadEdge.NodeB.X);
            int maxY = (int)Mathf.Max(roadEdge.NodeA.Y, roadEdge.NodeB.Y);

            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if (x < 0 || y < 0 || x >= length || y >= width)
                        continue;
                    float dist;
                    (dist, _) = BaseAlgorithm.DistanceFromPointToLineSeg(new Vector2(x, y),
                        new Vector2(roadEdge.NodeA.X, roadEdge.NodeA.Y),
                        new Vector2(roadEdge.NodeB.X, roadEdge.NodeB.Y));
                    if (dist < 3)
                    {
                        m_vertexInfos[x][y].isRoad = true;
                    }
                }
            }
        }

        foreach (var roadEdge in generator.roadGraph.MinorEdges)
        {
            int minX = (int)Mathf.Min(roadEdge.NodeA.X, roadEdge.NodeB.X);
            int minY = (int)Mathf.Min(roadEdge.NodeA.Y, roadEdge.NodeB.Y);
            int maxX = (int)Mathf.Max(roadEdge.NodeA.X, roadEdge.NodeB.X);
            int maxY = (int)Mathf.Max(roadEdge.NodeA.Y, roadEdge.NodeB.Y);

            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if (x < 0 || y < 0 || x >= length || y >= width)
                        continue;
                    float dist;
                    (dist, _) = BaseAlgorithm.DistanceFromPointToLineSeg(new Vector2(x, y),
                        new Vector2(roadEdge.NodeA.X, roadEdge.NodeA.Y),
                        new Vector2(roadEdge.NodeB.X, roadEdge.NodeB.Y));
                    if (dist < 3)
                    {
                        m_vertexInfos[x][y].isRoad = true;
                    }
                }
            }
        }
    }


    public List<Vector2> SmoothLocally(List<Vector2> positions, TerrainType type, int range = 2)
    {
        var new_positions = new List<Vector2>();
        var length = global_params.length;
        var width = global_params.width;
        foreach (var curPt in positions)
        {
            int x = (int)curPt.x, z = (int)curPt.y;
            int count_unwater = 0, count_total = 0;
            var neighbor_unwater_Pts = new List<Vector2>();
            for (int i = -range; i <= range; i++)
            {
                for (int k = -range; k <= range; k++)
                {
                    int n_x = x + i, n_z = z + k;

                    if (x == n_x && z == n_z)
                        continue;

                    if (n_x < 0 || n_z < 0 || n_x >= length || n_z >= width)
                        continue;

                    TerrainType neighborType = m_vertexInfos[n_x][n_z].label;

                    if (neighborType != type) // 如果这个邻居点不是water类型
                    {
                        count_unwater++;
                        neighbor_unwater_Pts.Add(new Vector2(n_x, n_z));
                    }

                    count_total++;

                }
            }

            if (count_unwater / (float)count_total <= 0.5f)
            {
                foreach (var neighbor_pt in neighbor_unwater_Pts)
                    m_vertexInfos[(int)neighbor_pt.x][(int)neighbor_pt.y].label = type;
                new_positions.AddRange(neighbor_unwater_Pts);
            }
        }

        positions.AddRange(new_positions);

        return positions;
    }


    public void SmoothHeightLocally(List<Vector2> positions, int range = 3)
    {
        var length = global_params.length;
        var width = global_params.width;

        List<float> smoothedHeight = new List<float>();
        foreach (var curPt in positions)
        {
            int x = (int)curPt.x, z = (int)curPt.y;
            int count_total = 0;
            float avgHeight = 0;
            for (int i = -range; i <= range; i++)
            {
                for (int k = -range; k <= range; k++)
                {
                    int n_x = x + i, n_z = z + k;

                    if (x == n_x && z == n_z)
                        continue;

                    if (n_x < 0 || n_z < 0 || n_x >= length || n_z >= width)
                        continue;
                    avgHeight += m_vertexInfos[x][z].height;
                    count_total++;

                }
            }

            avgHeight /= count_total;

            smoothedHeight.Add(avgHeight);
        }

        for (int i = 0; i < positions.Count; ++i)
        {
            var curPt = positions[i];
            int x = (int)curPt.x, z = (int)curPt.y;
            m_vertexInfos[x][z].height = smoothedHeight[i];
        }

    }


    public void UpdateCityHeightShiftFactor()
    {
        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                var type = m_vertexInfos[x][y].label;
                if (type == TerrainType._CITY)
                    m_vertexInfos[x][y].city_height_shift_factor =
                        -LocalVertexInfo.max_city_height_change_range; // 设置为(-max)
                else
                    m_vertexInfos[x][y].city_height_shift_factor =
                        +LocalVertexInfo.max_city_height_change_range; // 设置为(+max)
            }
        }

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                var type = m_vertexInfos[x][y].label;

                if (type != TerrainType._CITY) // skip all non-city vertices;
                    continue;

                int t = LocalVertexInfo.max_city_height_change_range / 2; // range.

                for (int i = -t; i <= t; i++)
                {
                    for (int k = -t; k <= t; k++)
                    {
                        int n_x = x + i;
                        int n_y = y + k;

                        if (n_x < 0 || n_y < 0 || n_x >= length || n_y >= width)
                            continue;

                        TerrainType neighborType = m_vertexInfos[n_x][n_y].label;

                        if (neighborType != type) // 如果这个邻居点不是正在测试的类型（如water）
                        {
                            float dist = Mathf.Abs(i) + Mathf.Abs(k);
                            if (Mathf.Abs(m_vertexInfos[x][y].city_height_shift_factor) >
                                dist) // for current one (water);
                                m_vertexInfos[x][y].city_height_shift_factor = -dist;

                            if (m_vertexInfos[n_x][n_y].city_height_shift_factor > dist)
                                m_vertexInfos[n_x][n_y].city_height_shift_factor = dist;
                        }

                    }
                }
            }
        }
    }


    // 仅仅修改了水面高度的偏移量 water_height_shift_factor (权重 靠近水边)
    public void UpdateWaterHeightShiftFactor()
    {
        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                var type = m_vertexInfos[x][y].label;
                if (type == TerrainType._WATER)
                    m_vertexInfos[x][y].water_height_shift_factor =
                        -LocalVertexInfo.max_water_height_change_range; // 设置为(-max)
                else
                    m_vertexInfos[x][y].water_height_shift_factor =
                        +LocalVertexInfo.max_water_height_change_range; // 设置为(+max)
            }
        }

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                var type = m_vertexInfos[x][y].label;

                if (type != TerrainType._WATER) // skip all non-WATER vertices;
                    continue;

                int t = LocalVertexInfo.max_water_height_change_range / 2; // range.

                for (int i = -t; i <= t; i++)
                {
                    for (int k = -t; k <= t; k++)
                    {
                        int n_x = x + i;
                        int n_y = y + k;

                        if (n_x < 0 || n_y < 0 || n_x >= length || n_y >= width)
                            continue;

                        TerrainType neighborType = m_vertexInfos[n_x][n_y].label;

                        if (neighborType != type) // 如果这个邻居点不是正在测试的类型（如water）
                        {
                            // float dist = Mathf.Abs(i) + Mathf.Abs(k);
                            float dist = Mathf.Sqrt(i * i + k * k); // L2 distance
                            if (Mathf.Abs(m_vertexInfos[x][y].water_height_shift_factor) >
                                dist) // for current one (water);
                                m_vertexInfos[x][y].water_height_shift_factor = -dist;

                            if (m_vertexInfos[n_x][n_y].water_height_shift_factor > dist)
                                m_vertexInfos[n_x][n_y].water_height_shift_factor = dist;
                        }

                    }
                }
            }
        }
    }


    public List<BaseAlgorithm.Line2D> GetRandomStroke(int joint_num, float segment_len, int subsplit_num = 4)
    {
        var length = global_params.length;
        var width = global_params.width;

        var stroke = new List<BaseAlgorithm.Line2D>();

        Vector2 pt = new Vector2(Random.Range(0, length), Random.Range(0, width));
        Vector2 baseDir = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

        for (int i = 0; i < joint_num; ++i)
        {
            var lineSeg = new BaseAlgorithm.Line2D();
            lineSeg.pt1 = pt; // 1st pt


            pt += segment_len * baseDir;
            lineSeg.pt2 = pt;
            ; // 2nd pt
            stroke.Add(lineSeg);

            float deltaAngle = Random.Range(-2.0f, 2.0f); // rotate slightly
            baseDir = BaseAlgorithm.RotateVector2D(baseDir, deltaAngle).normalized;

        }

        return stroke;
    }


    public List<BaseAlgorithm.Line2D> GetRandomRiverStroke(int joint_num, float segment_len)
    {
        var length = global_params.length;
        var width = global_params.width;

        var stroke = new List<BaseAlgorithm.Line2D>();

        // 随机选取河流入口：Edge: 0: down, 1: left, 2: up, 3: right (左下角为原点，x轴向右，z轴向上)
        // 新增了初始高度阈值
        Vector2 pt;
        while (true)
        {
            var entryEdge = Random.Range(0, 4);
            pt = entryEdge % 2 == 0
                ? new Vector2(length * Random.Range(0.0f, 1.0f), (entryEdge / 2) * (width - 1)) // 上，下边
                : new Vector2((entryEdge / 2) * (length - 1), width * Random.Range(0.0f, 1.0f));

            // 初始出发点必须大于16
            if (m_vertexInfos[(int)pt.x][(int)pt.y].height < 16f)
            {
                break;
            }
        }

        var midPoint = new Vector2(length / 2.0f, width / 2.0f);

        float deltaAngle = Random.Range(-0.5f, 0.5f); // rotate slightly
        var baseDir = (midPoint - pt).normalized;
        baseDir = BaseAlgorithm.RotateVector2D(baseDir, deltaAngle).normalized;

        for (int i = 0; i < joint_num; ++i)
        {
            var lineSeg = new BaseAlgorithm.Line2D();
            lineSeg.pt1 = pt; // 1st pt
            
            pt += segment_len * baseDir;
            lineSeg.pt2 = pt;
            ; // 2nd pt
            stroke.Add(lineSeg);

            deltaAngle = Random.Range(-0.5f, 0.5f); // rotate slightly
            baseDir = BaseAlgorithm.RotateVector2D(baseDir, deltaAngle).normalized;
        }

        return stroke;
    }

    public void ShiftDownWaterRegion_OnlyStampUse()
    {
        // shift positions vertically.
        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                float water_factor = (m_vertexInfos[x][y].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range);
                float shift = (2 * 16) * water_factor * 0.2f; // negative value. (height=0) *0.5f
                //float shift = m_vertexInfos[x][y].water_height_shift_factor * 0.5f; // negative value. (height=0)

                m_vertexInfos[x][y].height_of_water_surface = m_vertexInfos[x][y].height;
                m_vertexInfos[x][y].height += shift + 0.1f;
            }
        }

        this.CalculateNormalsOverEntireMap();
        this.UpdateTerrainInfo();
    }


    public void ShiftDownWaterRegion(bool isOnlyUseLayoutMap=false)
    {
        // shift positions vertically.
        var length = global_params.length;
        var width = global_params.width;

        float factor = isOnlyUseLayoutMap ? 15.2f: 0.5f;

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                //float water_factor = (m_vertexInfos[x][y].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range);
                //float shift = (2 * 16) * water_factor * 0.2f; // negative value. (height=0) *0.5f
                float shift = m_vertexInfos[x][y].water_height_shift_factor * 0.5f; // negative value. (height=0)

                m_vertexInfos[x][y].height_of_water_surface = 0;    // 对于noise 直接设为 0
                m_vertexInfos[x][y].height += shift + 0.1f;
            }
        }

        this.CalculateNormalsOverEntireMap();
        this.UpdateTerrainInfo();
    }


    public void ApplyExistedLandCoverMap()
    {
        var landcover_map = global_params.predefine_label_map;
        var landcover_map_colors = landcover_map.GetPixels();
        var length = landcover_map.height;
        var width = landcover_map.width;

        var water_color = new Color(0.1f, 0.1f, 0.8f);
        var forest_color = new Color(0.1f, 0.8f, 0.3f);
        var city_color = new Color(0.5f, 0.2f, 0.2f);

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                int index = y * width + x;
                Color color = landcover_map_colors[index];
                m_vertexInfos[x][y].label = GetTerrainTypeFromColor(color);


                if (Mathf.Abs(color.r - 1.0f)< 0.1f && Mathf.Abs(color.g - 1.0f) < 0.1f && Mathf.Abs(color.b - 1.0f) < 0.1f)
                {
                    m_vertexInfos[x][y].isMountainArea = true;
                }

            }
        }
    }
    
    public void ApplyExistedHeightMap(bool reset_label=false)
    {
        var height_map = global_params.predefine_height_map;
        var height_map_colors = height_map.GetPixels();
        var length = height_map.height;
        var width = height_map.width;

        for (int x = 0; x < length; ++x)
        {
            for (int y = 0; y < width; ++y)
            {
                int index = y * width + x;
                Color height = height_map_colors[index];
                m_vertexInfos[x][y].height = height.r * global_params.base_noise_amplify;
                if (reset_label)
                {
                    m_vertexInfos[x][y].label = LocalVertexInfo.default_terrain_type;
                }
            }
        }
    }

    // 根据颜色返回最相近的color
    private TerrainType GetTerrainTypeFromColor(Color color)
    {
        // 这个是在SaveTerrainLabelMap中定义的label颜色及其对应的TerrainType
        var water_color = new Color(0.1f, 0.1f, 0.8f);
        var forest_color = new Color(0.1f, 0.8f, 0.3f);
        var city_color = new Color(0.5f, 0.2f, 0.2f);
        
        var distanceToWater = ColorDistance(color, water_color);
        var distanceToForest = ColorDistance(color, forest_color);
        var distanceToCity = ColorDistance(color, city_color);

        // 确定最小的距离
        var minDistance = Mathf.Min(distanceToWater, distanceToForest, distanceToCity);

        // 返回与传入颜色最接近的TerrainType
        if (minDistance == distanceToWater) {
            return TerrainType._WATER;
        } else if (minDistance == distanceToForest) {
            return TerrainType._FOREST;
        } else {
            return TerrainType._CITY;
        }
    }


    private static float ColorDistance(Color c1, Color c2)
    {
        return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
    }

}