using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;
using Random = UnityEngine.Random;

partial class BaseSurfaceModeler
{
    // use this function to get the final vertex on 3D triangular geometry according to the given (x,y) coordinates of terrain height map.
    public Vector3 GetVector3FromIndex(int x, int y)
    {
        return new Vector3(x, m_vertexInfos[x][y].height, y);   // (x, height, y)
    }


    public List<Mesh> BuildBaseSurfaceMesh()
    {
        int mId = 0;
        int count = 0;
        List<Mesh> meshes = new List<Mesh>();
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        meshes.Add(new Mesh());

        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length - 1; x++)
        {
            for (int y = 0; y < width - 1; y++)
            {
                Vector3 v1 = GetVector3FromIndex(x + 0, y + 0);
                Vector3 v2 = GetVector3FromIndex(x + 0, y + 1);
                Vector3 v3 = GetVector3FromIndex(x + 1, y + 1);
                Vector3 v4 = GetVector3FromIndex(x + 1, y + 0);

                Vector3 n1 = this.m_vertexInfos[x + 0][y + 0].normal;
                Vector3 n2 = this.m_vertexInfos[x + 0][y + 1].normal;
                Vector3 n3 = this.m_vertexInfos[x + 1][y + 1].normal;
                Vector3 n4 = this.m_vertexInfos[x + 1][y + 0].normal;



                Vector2 t1 = new Vector2((x + 0) / (float)(length - 1), (y + 0) / (float)(width - 1));
                Vector2 t2 = new Vector2((x + 0) / (float)(length - 1), (y + 1) / (float)(width - 1));
                Vector2 t3 = new Vector2((x + 1) / (float)(length - 1), (y + 1) / (float)(width - 1));
                Vector2 t4 = new Vector2((x + 1) / (float)(length - 1), (y + 0) / (float)(width - 1));

                if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                {
                    meshes[mId].vertices = vecs[mId].ToArray();
                    meshes[mId].triangles = indices[mId].ToArray();
                    meshes[mId].normals = norms[mId].ToArray();
                    meshes[mId].uv = uvs[mId].ToArray();

                    vecs.Add(new List<Vector3>());
                    uvs.Add(new List<Vector2>());
                    norms.Add(new List<Vector3>());
                    indices.Add(new List<int>());
                    meshes.Add(new Mesh());

                    mId++;
                    count = 0;
                }

                // Create 2 triangles for this facelet.
                vecs[mId].Add(v1); vecs[mId].Add(v2); vecs[mId].Add(v3);
                vecs[mId].Add(v3); vecs[mId].Add(v4); vecs[mId].Add(v1);

                norms[mId].Add(n1); norms[mId].Add(n2); norms[mId].Add(n3);
                norms[mId].Add(n3); norms[mId].Add(n4); norms[mId].Add(n1);

                uvs[mId].Add(t1); uvs[mId].Add(t2); uvs[mId].Add(t3);
                uvs[mId].Add(t3); uvs[mId].Add(t4); uvs[mId].Add(t1);

                indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                count += 6;

            }
        }

        if (count > 0)  // The rest vertices are fed to one single mesh.
        {
            meshes[mId].vertices = vecs[mId].ToArray();
            meshes[mId].normals = norms[mId].ToArray();
            meshes[mId].uv = uvs[mId].ToArray();
            meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        return meshes;
    }


    public List<Mesh> BuildWaterMesh()
    {
        int mId = 0;
        int count = 0;
        List<Mesh> meshes = new List<Mesh>();
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        meshes.Add(new Mesh());

        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length - 1; x++)
        {
            for (int y = 0; y < width - 1; y++) {
                // m_vertexInfos[x][y].height_of_water_surface（原本的），现在y分量直接设为0
                //Vector3 v1 = new Vector3(x + 0, 0, y + 0);
                //Vector3 v2 = new Vector3(x + 0, 0, y + 1);
                //Vector3 v3 = new Vector3(x + 1, 0, y + 1);
                //Vector3 v4 = new Vector3(x + 1, 0, y + 0);
                Vector3 v1 = new Vector3(x + 0, m_vertexInfos[x][y].height_of_water_surface, y + 0);
                Vector3 v2 = new Vector3(x + 0, m_vertexInfos[x][y].height_of_water_surface, y + 1);
                Vector3 v3 = new Vector3(x + 1, m_vertexInfos[x][y].height_of_water_surface, y + 1);
                Vector3 v4 = new Vector3(x + 1, m_vertexInfos[x][y].height_of_water_surface, y + 0);

                if (m_vertexInfos[x][y].label != TerrainType._WATER && m_vertexInfos[x][y + 1].label != TerrainType._WATER
                                                                    && m_vertexInfos[x + 1][y + 1].label != TerrainType._WATER && m_vertexInfos[x + 1][y].label != TerrainType._WATER)
                {
                    if(m_vertexInfos[x][y].water_height_shift_factor >= 0.9* LocalVertexInfo.max_water_height_change_range)
                        continue;
                }
                

                Vector2 t1 = new Vector2(0.0f, 0.0f);
                Vector2 t2 = new Vector2(0.0f, 1.0f);
                Vector2 t3 = new Vector2(1.0f, 1.0f);
                Vector2 t4 = new Vector2(1.0f, 0.0f);

                if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                {
                    meshes[mId].vertices = vecs[mId].ToArray();
                    meshes[mId].triangles = indices[mId].ToArray();
                    meshes[mId].normals = norms[mId].ToArray();
                    meshes[mId].uv = uvs[mId].ToArray();

                    vecs.Add(new List<Vector3>());
                    uvs.Add(new List<Vector2>());
                    norms.Add(new List<Vector3>());
                    indices.Add(new List<int>());
                    meshes.Add(new Mesh());

                    mId++;
                    count = 0;
                }

                // Create 2 triangles for this facelet.
                vecs[mId].Add(v1); vecs[mId].Add(v2); vecs[mId].Add(v3);
                vecs[mId].Add(v3); vecs[mId].Add(v4); vecs[mId].Add(v1);

                norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);
                norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);

                uvs[mId].Add(t1); uvs[mId].Add(t2); uvs[mId].Add(t3);
                uvs[mId].Add(t3); uvs[mId].Add(t4); uvs[mId].Add(t1);

                indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                count += 6;
            }
        }

        if (count > 0)  // The rest vertices are fed to one single mesh.
        {
            meshes[mId].vertices = vecs[mId].ToArray();
            meshes[mId].normals = norms[mId].ToArray();
            meshes[mId].uv = uvs[mId].ToArray();
            meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        return meshes;
    }


    public List<Mesh> BuildRoadMesh()
    {
        float tex_scale = 10.0f;
        int mId = 0;
        int count = 0;
        List<Mesh> meshes = new List<Mesh>();
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        meshes.Add(new Mesh());

        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length - 1; x++)
        {
            for (int y = 0; y < width - 1; y++)
            {
                Vector3 height_offset = new Vector3(0, 0.001f, 0);
                Vector3 v1 = GetVector3FromIndex(x + 0, y + 0) + height_offset;
                Vector3 v2 = GetVector3FromIndex(x + 0, y + 1) + height_offset;
                Vector3 v3 = GetVector3FromIndex(x + 1, y + 1) + height_offset;
                Vector3 v4 = GetVector3FromIndex(x + 1, y + 0) + height_offset;


                int count_road_pixels = 0;
                if (m_vertexInfos[x][y].isRoad) count_road_pixels++;
                if (m_vertexInfos[x][y + 1].isRoad) count_road_pixels++;
                if (m_vertexInfos[x + 1][y + 1].isRoad) count_road_pixels++;
                if (m_vertexInfos[x + 1][y].isRoad) count_road_pixels++;

                if (count_road_pixels <= 1)
                    continue;

                //if (m_vertexInfos[x][y].isRoad == false && m_vertexInfos[x][y + 1].isRoad == false
                //    && m_vertexInfos[x + 1][y + 1].isRoad == false && m_vertexInfos[x + 1][y].isRoad == false)
                //    continue;


                Vector2 t1 = tex_scale * new Vector2(v1.x / length, v1.z / width);
                Vector2 t2 = tex_scale * new Vector2(v2.x / length, v2.z / width);
                Vector2 t3 = tex_scale * new Vector2(v3.x / length, v3.z / width);
                Vector2 t4 = tex_scale * new Vector2(v4.x / length, v4.z / width);

                if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                {
                    meshes[mId].vertices = vecs[mId].ToArray();
                    meshes[mId].triangles = indices[mId].ToArray();
                    meshes[mId].normals = norms[mId].ToArray();
                    meshes[mId].uv = uvs[mId].ToArray();

                    vecs.Add(new List<Vector3>());
                    uvs.Add(new List<Vector2>());
                    norms.Add(new List<Vector3>());
                    indices.Add(new List<int>());
                    meshes.Add(new Mesh());

                    mId++;
                    count = 0;
                }

                // Create 2 triangles for this facelet.
                vecs[mId].Add(v1); vecs[mId].Add(v2); vecs[mId].Add(v3);
                vecs[mId].Add(v3); vecs[mId].Add(v4); vecs[mId].Add(v1);

                norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);
                norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);

                uvs[mId].Add(t1); uvs[mId].Add(t2); uvs[mId].Add(t3);
                uvs[mId].Add(t3); uvs[mId].Add(t4); uvs[mId].Add(t1);

                indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                count += 6;

            }
        }

        if (count > 0)  // The rest vertices are fed to one single mesh.
        {
            meshes[mId].vertices = vecs[mId].ToArray();
            meshes[mId].normals = norms[mId].ToArray();
            meshes[mId].uv = uvs[mId].ToArray();
            meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        return meshes;
    }


    public float GetHeightByCentroidCoordinate(Vector3 P)
    {
        // https://zhuanlan.zhihu.com/p/110754637
        var length = global_params.length;
        var width = global_params.width;

        int map_x = (int)P.x, map_y = (int)P.z;

        map_x = map_x < 0 ? 0 : map_x;
        map_y = map_y < 0 ? 0 : map_y;
        map_x = (map_x >= length - 1) ? (length - 2) : map_x;
        map_y = (map_y >= width - 1) ? (width - 2) : map_y;

        int x1 = map_x;
        int x2 = map_x + 1;
        int y1 = map_y;
        int y2 = map_y + 1;
        float x = map_x + (P.x - (int)P.x);
        float y = map_y + (P.z - (int)P.z);
        float Q11 = m_vertexInfos[x1][y1].height;
        float Q12 = m_vertexInfos[x1][y2].height;
        float Q21 = m_vertexInfos[x2][y1].height;
        float Q22 = m_vertexInfos[x2][y2].height;

        float R1 = (x2 - x) / (float)(x2 - x1) * Q11 + (x - x1) / (float)(x2 - x1) * Q21;
        float R2 = (x2 - x) / (float)(x2 - x1) * Q12 + (x - x1) / (float)(x2 - x1) * Q22;

        float h = (y2 - y) / (float)(y2 - y1) * R1 + (y - y1) / (float)(y2 - y1) * R2;

        return h;

    }


    public List<Mesh> BuildRoadMesh(CityGenerator generator)
    {
        float tex_scale = 10.0f;
        int mId = 0;
        int count = 0;
        List<Mesh> meshes = new List<Mesh>();
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        meshes.Add(new Mesh());

        var length = global_params.length;
        var width = global_params.width;

        float width_majorRoad = 0.8f;
        float width_minorRoad = 0.4f;

        float lowest = m_terrainInfos.min_height_pos.y;
        float highest = m_terrainInfos.max_height_pos.y;
        float diff = highest - lowest;
        float mntRoad_LowlimitHeight = lowest + diff * global_params.RoadMaxHeightLineOnTheWild;


        foreach (var roadEdge in generator.roadGraph.MajorEdges)
        {
            //TCP_Client.SendMessage("A majorroad....");
            Vector3 nodeA = new Vector3(roadEdge.NodeA.X, 0.0f, roadEdge.NodeA.Y);
            Vector3 nodeB = new Vector3(roadEdge.NodeB.X, 0.0f, roadEdge.NodeB.Y);
            
            {
                int xa = (int)nodeA.x, ya = (int)nodeA.z, xb = (int)nodeB.x, yb = (int)nodeB.z;

                if (xa < 0 || ya < 0 || xa >= length || yb >= width)
                    continue;
                if (xb < 0 || yb < 0 || xb >= length || yb >= width)
                    continue;

                if (m_vertexInfos[xa][ya].label == TerrainType._WATER)
                    continue;
                if (m_vertexInfos[xb][yb].label == TerrainType._WATER)
                    continue;

                if (m_vertexInfos[xa][ya].label != TerrainType._CITY && m_vertexInfos[xa][ya].height > mntRoad_LowlimitHeight)
                    continue;
                if (m_vertexInfos[xa][ya].label != TerrainType._CITY && m_vertexInfos[xa][ya].height > mntRoad_LowlimitHeight)
                    continue;

                float water_factor_a = m_vertexInfos[xa][ya].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;
                float water_factor_b = m_vertexInfos[xb][yb].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;

                if (Mathf.Abs(water_factor_a) < 0.999f || Mathf.Abs(water_factor_b) < 0.999f)
                    continue;
            }


            Vector3 dir = (nodeB - nodeA);
            Vector3 dirNormed = dir.normalized;
            Vector3 radialDir = Vector3.Cross(dirNormed, Vector3.up).normalized;

            for (int i = -3; i < 3; ++i)
            {
                for (int k = 0; k < 10; ++k)
                {
                    Vector3 a0 = nodeA + width_majorRoad * (i + 0) * radialDir + (k + 0) / 10.0f * dir;
                    Vector3 a1 = nodeA + width_majorRoad * (i - 1) * radialDir + (k + 0) / 10.0f * dir;
                    Vector3 a2 = nodeA + width_majorRoad * (i - 1) * radialDir + (k + 1) / 10.0f * dir;
                    Vector3 a3 = nodeA + width_majorRoad * (i + 0) * radialDir + (k + 1) / 10.0f * dir;

                    float h0 = GetHeightByCentroidCoordinate(a0) + 0.5f;
                    float h1 = GetHeightByCentroidCoordinate(a1) + 0.5f;
                    float h2 = GetHeightByCentroidCoordinate(a2) + 0.5f;
                    float h3 = GetHeightByCentroidCoordinate(a3) + 0.5f;

                    a0 += new Vector3(0, h0, 0);
                    a1 += new Vector3(0, h1, 0);
                    a2 += new Vector3(0, h2, 0);
                    a3 += new Vector3(0, h3, 0);

                    Vector2 t0 = tex_scale * new Vector2(a0.x / length, a0.z / width);
                    Vector2 t1 = tex_scale * new Vector2(a1.x / length, a1.z / width);
                    Vector2 t2 = tex_scale * new Vector2(a2.x / length, a2.z / width);
                    Vector2 t3 = tex_scale * new Vector2(a3.x / length, a3.z / width);


                    if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                    {
                        meshes[mId].vertices = vecs[mId].ToArray();
                        meshes[mId].triangles = indices[mId].ToArray();
                        meshes[mId].normals = norms[mId].ToArray();
                        meshes[mId].uv = uvs[mId].ToArray();

                        vecs.Add(new List<Vector3>());
                        uvs.Add(new List<Vector2>());
                        norms.Add(new List<Vector3>());
                        indices.Add(new List<int>());
                        meshes.Add(new Mesh());

                        mId++;
                        count = 0;
                    }

                    // Create 2 triangles for this facelet.
                    vecs[mId].Add(a0); vecs[mId].Add(a2); vecs[mId].Add(a1);
                    vecs[mId].Add(a2); vecs[mId].Add(a0); vecs[mId].Add(a3);

                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);
                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);

                    uvs[mId].Add(t0); uvs[mId].Add(t2); uvs[mId].Add(t1);
                    uvs[mId].Add(t2); uvs[mId].Add(t0); uvs[mId].Add(t3);

                    indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                    indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                    count += 6;
                }
            }
        }


        foreach (var roadEdge in generator.roadGraph.MinorEdges)
        {
            //TCP_Client.SendMessage("A majorroad....");
            Vector3 nodeA = new Vector3(roadEdge.NodeA.X, 0.0f, roadEdge.NodeA.Y);
            Vector3 nodeB = new Vector3(roadEdge.NodeB.X, 0.0f, roadEdge.NodeB.Y);

            // �߽���
            {
                int xa = (int)nodeA.x, ya =(int)nodeA.z, xb = (int)nodeB.x, yb = (int)nodeB.z;

                if (xa < 0 || ya < 0 || xa >= length || yb >= width)
                    continue;
                if (xb < 0 || yb < 0 || xb >= length || yb >= width)
                    continue;

                if (m_vertexInfos[xa][ya].label != TerrainType._CITY)
                    continue;
                if (m_vertexInfos[xb][yb].label != TerrainType._CITY)
                    continue;


                float water_factor_a = m_vertexInfos[xa][ya].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;
                float water_factor_b = m_vertexInfos[xb][yb].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;

                if (Mathf.Abs(water_factor_a) < 0.99999f || Mathf.Abs(water_factor_b) < 0.99999f)
                    continue;
            }
       
            Vector3 dir = (nodeB - nodeA);
            Vector3 dirNormed = dir.normalized;
            Vector3 radialDir = Vector3.Cross(dirNormed, Vector3.up).normalized;

            for (int i = -3; i < 3; ++i)
            {
                for (int k = 0; k < 10; ++k)
                {
                    Vector3 a0 = nodeA + width_minorRoad * (i + 0) * radialDir + (k + 0) / 10.0f * dir;
                    Vector3 a1 = nodeA + width_minorRoad * (i - 1) * radialDir + (k + 0) / 10.0f * dir;
                    Vector3 a2 = nodeA + width_minorRoad * (i - 1) * radialDir + (k + 1) / 10.0f * dir;
                    Vector3 a3 = nodeA + width_minorRoad * (i + 0) * radialDir + (k + 1) / 10.0f * dir;

                    float h0 = GetHeightByCentroidCoordinate(a0) + 0.5f;
                    float h1 = GetHeightByCentroidCoordinate(a1) + 0.5f;
                    float h2 = GetHeightByCentroidCoordinate(a2) + 0.5f;
                    float h3 = GetHeightByCentroidCoordinate(a3) + 0.5f;

                    a0 += new Vector3(0, h0, 0);
                    a1 += new Vector3(0, h1, 0);
                    a2 += new Vector3(0, h2, 0);
                    a3 += new Vector3(0, h3, 0);

                    Vector2 t0 = tex_scale * new Vector2(a0.x / length, a0.z / width);
                    Vector2 t1 = tex_scale * new Vector2(a1.x / length, a1.z / width);
                    Vector2 t2 = tex_scale * new Vector2(a2.x / length, a2.z / width);
                    Vector2 t3 = tex_scale * new Vector2(a3.x / length, a3.z / width);


                    if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                    {
                        meshes[mId].vertices = vecs[mId].ToArray();
                        meshes[mId].triangles = indices[mId].ToArray();
                        meshes[mId].normals = norms[mId].ToArray();
                        meshes[mId].uv = uvs[mId].ToArray();

                        vecs.Add(new List<Vector3>());
                        uvs.Add(new List<Vector2>());
                        norms.Add(new List<Vector3>());
                        indices.Add(new List<int>());
                        meshes.Add(new Mesh());

                        mId++;
                        count = 0;
                    }

                    // Create 2 triangles for this facelet.
                    vecs[mId].Add(a0); vecs[mId].Add(a2); vecs[mId].Add(a1);
                    vecs[mId].Add(a2); vecs[mId].Add(a0); vecs[mId].Add(a3);

                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);
                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);

                    uvs[mId].Add(t0); uvs[mId].Add(t2); uvs[mId].Add(t1);
                    uvs[mId].Add(t2); uvs[mId].Add(t0); uvs[mId].Add(t3);

                    indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                    indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                    count += 6;
                }
            }
        }

        if (count > 0)  // The rest vertices are fed to one single mesh.
        {
            meshes[mId].vertices = vecs[mId].ToArray();
            meshes[mId].normals = norms[mId].ToArray();
            meshes[mId].uv = uvs[mId].ToArray();
            meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        TCP_Client.SendMessage(string.Format("A {0}, {1}", meshes.Count, count));

        return meshes;
    }


    public List<Mesh> BuildGreenAreas(CityGenerator generator, bool isGenWild = false)
    {
        int mId = 0;
        int count = 0;
        List<Mesh> meshes = new List<Mesh>();
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        meshes.Add(new Mesh());

        var length = global_params.length;
        var width = global_params.width;


        foreach (var block in generator.blocks)
        {
            if (block.IsPark == false)
                continue;

            List<Vector2> botNodes = new List<Vector2>();


            bool isWild = false;
            for (int i = 0; i < block.Nodes.Count; ++i)
            {

                botNodes.Add(new Vector2(block.Nodes[i].X, block.Nodes[i].Y));

                int x = (int)block.Nodes[i].X;
                int y = (int)block.Nodes[i].Y;

                if (x < 0 || y < 0 || x >= length - 1 || y >= width - 1)
                    continue;
                var label = m_vertexInfos[x][y].label;

                if (label != TerrainType._CITY)
                    isWild = true;
            }

            if (isWild == true && isGenWild == false)  // 当前是野外，并且目标是城镇内
                continue;
            if (isWild == false && isGenWild == true)  // 当前是城镇，并且目标是野外
                continue;

            float xMin, yMin, xMax, yMax;
            (xMin, xMax,yMin, yMax) = Util_Geo.GetBoundingBoxFromPolygon(botNodes);

            for (int x = (int)xMin; x < (int)xMax; x++) 
            {
                for (int y = (int)yMin; y < (int)yMax; y++) 
                {
                    if (x < 0 || y < 0 || x >= length - 1 || y >= width - 1)
                        continue;

                    if (m_vertexInfos[x][y].label == TerrainType._WATER)
                        continue;

                    if (m_vertexInfos[x][y].height > 50.0f && m_vertexInfos[x][y].label != TerrainType._CITY)
                        continue;

                    if (Util_Geo.IsPointInPolygon(botNodes, new Vector2(x, y)) == false)
                        continue;

                    if (Random.Range(0.0f, 1.0f) < global_params.parkTreeThreshold)
                    {
                        m_vertexInfos[x][y].plant_type = PlantType._parkTree;
                        m_functionalAreaTreePositions.Add(new Vector2Int(x, y));
                    }

                    Vector3 v1 = new Vector3(x + 0, m_vertexInfos[x][y].height + 0.4f, y + 0);
                    Vector3 v2 = new Vector3(x + 0, m_vertexInfos[x][y + 1].height + 0.4f, y + 1);
                    Vector3 v3 = new Vector3(x + 1, m_vertexInfos[x + 1][y + 1].height + 0.4f, y + 1);
                    Vector3 v4 = new Vector3(x + 1, m_vertexInfos[x + 1][y].height + 0.4f, y + 0);
                    
                    Vector2 t1 = new Vector2((x + 0) / (float)(length - 1), (y + 0) / (float)(width - 1));
                    Vector2 t2 = new Vector2((x + 0) / (float)(length - 1), (y + 1) / (float)(width - 1));
                    Vector2 t3 = new Vector2((x + 1) / (float)(length - 1), (y + 1) / (float)(width - 1));
                    Vector2 t4 = new Vector2((x + 1) / (float)(length - 1), (y + 0) / (float)(width - 1));

                    if (count > 55000)  // If vertex count is larger than this value, then start a new mesh.
                    {
                        meshes[mId].vertices = vecs[mId].ToArray();
                        meshes[mId].triangles = indices[mId].ToArray();
                        meshes[mId].normals = norms[mId].ToArray();
                        meshes[mId].uv = uvs[mId].ToArray();

                        vecs.Add(new List<Vector3>());
                        uvs.Add(new List<Vector2>());
                        norms.Add(new List<Vector3>());
                        indices.Add(new List<int>());
                        meshes.Add(new Mesh());

                        mId++;
                        count = 0;
                    }

                    // Create 2 triangles for this facelet.
                    vecs[mId].Add(v1); vecs[mId].Add(v2); vecs[mId].Add(v3);
                    vecs[mId].Add(v3); vecs[mId].Add(v4); vecs[mId].Add(v1);

                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);
                    norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up); norms[mId].Add(Vector3.up);

                    uvs[mId].Add(t1); uvs[mId].Add(t2); uvs[mId].Add(t3);
                    uvs[mId].Add(t3); uvs[mId].Add(t4); uvs[mId].Add(t1);

                    indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                    indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                    count += 6;
                }
            }
        }

        if (count > 0)  // The rest vertices are fed to one single mesh.
        {
            meshes[mId].vertices = vecs[mId].ToArray();
            meshes[mId].normals = norms[mId].ToArray();
            meshes[mId].uv = uvs[mId].ToArray();
            meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        TCP_Client.SendMessage(string.Format("A {0}, {1}", meshes.Count, count));

        return meshes;
    }

    
    public void CalculateNormalsOverEntireMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        for (int x = 0; x < length; x++)
        {

            for (int y = 0; y < width; y++)
            {
                //   v1
                //v2 v0 v3
                //   v4
                var v0 = GetVector3FromIndex(x, y);
                var v1 = GetVector3FromIndex(GlobalFunctions.ClipIndex(x - 1, length), y);
                var v2 = GetVector3FromIndex(x, GlobalFunctions.ClipIndex(y - 1, width));
                var v3 = GetVector3FromIndex(x, GlobalFunctions.ClipIndex(y + 1, width));
                var v4 = GetVector3FromIndex(GlobalFunctions.ClipIndex(x + 1, length), y);

                if (x == 0) v1 = Vector3.zero;
                if (y == 0) v2 = Vector3.zero;
                if (y == (width - 1)) v3 = Vector3.zero;
                if (x == (length - 1)) v4 = Vector3.zero;

                var n12 = Vector3.Cross(v1 - v0, v2 - v0);
                var n13 = Vector3.Cross(v3 - v0, v1 - v0);
                var n24 = Vector3.Cross(v2 - v0, v4 - v0);
                var n34 = Vector3.Cross(v4 - v0, v3 - v0);

                Vector3 normal = n12 + n13 + n24 + n34;

                m_vertexInfos[x][y].normal = -normal.normalized;  // noted, minus the computed normal..

            }
        }
    }

    // public Texture2D GetMaskImage()
    // {
    //     var length = global_params.length;
    //     var width = global_params.width;
    //
    //     Texture2D maskImage = new Texture2D(length, width, TextureFormat.RGBAFloat, false);
    //     Texture2D heightImage = new Texture2D(length, width, TextureFormat.RGBAFloat, false);
    //
    //     for (int x = 0; x < length; x++)
    //     {
    //         for (int y = 0; y < width; y++)
    //         {
    //             float type = ((float)m_vertexInfos[x][y].label) / (float)TerrainType._end;
    //             float height = (m_vertexInfos[x][y].height - m_terrainInfos.min_height_pos.y) / m_terrainInfos.max_height_pos.y;
    //                 
    //             Color mask_rgba = new Color(type, type, type);
    //             Color height_rgba = new Color(height, height, height);
    //             maskImage.SetPixel(x, y, mask_rgba);
    //             heightImage.SetPixel(x, y, height_rgba);
    //             //Debug.Log(type);
    //         }
    //     }
    //     maskImage.Apply();
    //     maskImage.filterMode = FilterMode.Point;
    //     File.WriteAllBytes("./tmp_tex_mask.png", maskImage.EncodeToPNG());
    //     File.WriteAllBytes("./tmp_height.png", heightImage.EncodeToPNG());
    //
    //     return maskImage;
    // }

    // �¸Ľ��ĵط���ֱ�Ӹ���label�����������ε�texture
    public Texture2D GetTerrainTexture() {
        float ratio = 5.0f;
        float precision = 1000000;
        int resolution = 8;
        
        var length = global_params.length;
        var width = global_params.width;
        var tempLength = length * resolution;
        var tempWidth = length * resolution;

        Texture2D terrainTexture = new Texture2D(tempLength, tempWidth, TextureFormat.RGBAFloat, false);
        Texture2D atlas = GlobalResources.m_terrainAtlas.atlas;
        
        for (int x = 0; x < length; x++) {
            for (int y = 0; y < width; y++) {
                Vector4 currentRect = GlobalResources.m_terrainAtlas.uvs[(int)m_vertexInfos[x][y].label];
                int atlasZ = (int)(currentRect.z * precision);
                int atlasW = (int)(currentRect.w * precision);
                
                for (int rx = 0; rx < resolution; rx++) {
                    for (int ry = 0; ry < resolution; ry++) {
                        int tempX = x * resolution + rx;
                        int tempY = y * resolution + ry;
                        
                        Vector2 real_uv = new Vector2(tempX / (float)(tempLength - 1), tempY / (float)(tempWidth - 1));
                        
                        real_uv.x = ((int)(ratio * real_uv.x * precision) % atlasZ) / precision;
                        real_uv.y = ((int)(ratio * real_uv.y * precision) % atlasW) / precision;

                        real_uv.x += currentRect.x;
                        real_uv.y += currentRect.y;

                        // ��һ�����ԣ�ʵ���ϻ����������ҵ�һ����ֵ������ƽ��
                        Color currentColor = atlas.GetPixelBilinear(real_uv.x, real_uv.y);
                        terrainTexture.SetPixel(tempX, tempY, currentColor);
                    }
                }
                
            }
        }
        
        terrainTexture.Apply();
        terrainTexture.filterMode = FilterMode.Bilinear;
        File.WriteAllBytes("./debug_terrainTexture.png", terrainTexture.EncodeToPNG());

        return terrainTexture;
    }

    public void SaveHeightShiftFactorMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        Texture2D image_water = new Texture2D(length, width, TextureFormat.RGBAFloat, false);
        Texture2D image_city = new Texture2D(length, width, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                {
                    float value = m_vertexInfos[x][y].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;

                    if (value > 0)
                        image_water.SetPixel(x, y, new Color(value, 0, 0)); // 0-�� 1-��
                    else
                        image_water.SetPixel(x, y, new Color(0, 0, -value)); // 0-�� 1-��

                }
                {
                    float value = m_vertexInfos[x][y].city_height_shift_factor / (float)LocalVertexInfo.max_city_height_change_range;

                    if (value > 0)
                        image_city.SetPixel(x, y, new Color(value, 0, 0)); // 0-�� 1-��
                    else
                        image_city.SetPixel(x, y, new Color(0, -value, 0)); // 0-�� 1-��
                }
            }
        }
        image_water.filterMode = FilterMode.Point;
        image_city.filterMode = FilterMode.Point;

        File.WriteAllBytes("./tmp_height_shift_factor_water_map.png", image_water.EncodeToPNG());
        File.WriteAllBytes("./tmp_height_shift_factor_city_map.png", image_city.EncodeToPNG());
    }
    
    //public Texture2D GetLayerMaskTexture() {
    //    var length = global_params.length;
    //    var width = global_params.width;

    //    Texture2D layerMask = new Texture2D(length, width, TextureFormat.RGBAFloat, false);
    //    Color[][] maskColors = new Color[length][];
    //    for (int index = 0; index < length; index++) {
    //        maskColors[index] = new Color[width];
    //    }

    //    for (int x = 0; x < length; x++) {
    //        for (int y = 0; y < width; y++) {
    //            TerrainType terrainType = m_vertexInfos[x][y].label;

    //            maskColors[x][y] = terrainType switch {
    //                TerrainType._FOREST => new Color(0.0f, 0.0f, 0.0f, 1.0f),
    //                TerrainType._CITY => new Color(1.0f, 0.0f, 0.0f, 0.0f),
    //                TerrainType._WATER or TerrainType._GRASS => new Color(0.0f, 1.0f, 0.0f, 0.0f),
    //                TerrainType._SNOW => new Color(0.0f, 0.0f, 1.0f, 0.0f),
    //                _ => new Color(1.0f, 0.0f, 0.0f, 0.0f)
    //            };
    //        }
    //    }
        
    //    // ģ����Ե����
    //    int r = 3;
    //    for (int x = 0; x < length; x++) {
    //        for (int y = 0; y < width; y++) {
    //            int pixelCount = 0;
    //            Color newColor = new Color(0, 0, 0, 0);
                
    //            for (int xi = x - r; xi <= x + r; xi++) {
    //                for (int yi = y - r; yi <= y + r; yi++) {
    //                    if (xi < 0 || yi < 0 || xi >= length || yi >= width) {
    //                        continue;
    //                    }

    //                    pixelCount++;
    //                    newColor += maskColors[xi][yi];
    //                }
    //            }

    //            maskColors[x][y] = newColor / pixelCount;
    //        }
    //    }

    //    for (int x = 0; x < length; x++) {
    //        for (int y = 0; y < width; y++) {
    //            layerMask.SetPixel(x, y, maskColors[x][y]);
    //        }
    //    }

    //    layerMask.Apply();
    //    layerMask.filterMode = FilterMode.Point;
    //    File.WriteAllBytes("./temp_layer_mask.png", layerMask.EncodeToPNG());
        
    //    return layerMask;
    //}

    public void SaveTerrainLabelMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        Texture2D image_label = new Texture2D(length, width, TextureFormat.RGBAFloat, false);

        Dictionary<TerrainType, Color> dic_colors = new Dictionary<TerrainType, Color>();
        //dic_colors[TerrainType._GRASS] = new Color(0.1f, 0.5f, 0.1f);
        //dic_colors[TerrainType._WATER] = new Color(0.1f, 0.1f, 0.8f);
        //dic_colors[TerrainType._FOREST] = new Color(0.1f, 0.8f, 0.3f);
        //dic_colors[TerrainType._SNOW] = new Color(0.8f, 0.8f, 0.8f);
        //dic_colors[TerrainType._CITY] = new Color(0.5f, 0.2f, 0.2f);
        //Color colorUndefined = new Color(0.0f, 0.0f, 0.0f);
        dic_colors[TerrainType._GRASS] = new Color(0.1f, 0.8f, 0.3f);
        dic_colors[TerrainType._WATER] = new Color(0.1f, 0.1f, 0.8f);
        dic_colors[TerrainType._FOREST] = new Color(0.1f, 0.8f, 0.3f);
        dic_colors[TerrainType._SNOW] = new Color(0.8f, 0.8f, 0.8f);
        dic_colors[TerrainType._CITY] = new Color(0.5f, 0.2f, 0.2f);
        Color colorUndefined = new Color(0.0f, 0.0f, 0.0f);

        // Color colorRoad = new Color(1.0f, 0.3f, 0.3f);

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                var terrainType = m_vertexInfos[x][y].label;

                if(dic_colors.ContainsKey(terrainType))
                {
                    //if (terrainType == TerrainType._CITY && m_vertexInfos[x][y].isRoad == true)
                    //    image_label.SetPixel(x, y, colorRoad);
                    //else
                    
                    // if height is larger than 30, we consider it is mountain area.
                    if (this.m_vertexInfos[x][y].height >= 40.0f && 
                        m_vertexInfos[x][y].label != TerrainType._WATER &&
                        m_vertexInfos[x][y].label != TerrainType._CITY)
                        // 山区是纯白的
                        image_label.SetPixel(x, y, new Color(255,255,255));
                    else
                        image_label.SetPixel(x, y, dic_colors[terrainType]);
                }
                else
                {
                    image_label.SetPixel(x, y, colorUndefined);
                }
            }
        }
        image_label.filterMode = FilterMode.Point;

        File.WriteAllBytes("./tmp_colored_mask_map.png", image_label.EncodeToPNG());
    }

    public void SaveHeightMap()
    {
        var length = global_params.length;
        var width = global_params.width;
        
        Texture2D heightImage = new Texture2D(length, width, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float height = (m_vertexInfos[x][y].height - m_terrainInfos.min_height_pos.y) / m_terrainInfos.max_height_pos.y;
                
                Color height_rgba = new Color(height, height, height);
                heightImage.SetPixel(x, y, height_rgba);
            }
        }
        
        File.WriteAllBytes("./tmp_height.png", heightImage.EncodeToPNG());
    }

    public void SaveHeightPlainUse()
    {
        var length = global_params.length;
        var width = global_params.width;

        Texture2D heightImage = new Texture2D(length, width, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float height = (m_vertexInfos[x][y].height - m_terrainInfos.min_height_pos.y) / m_terrainInfos.max_height_pos.y * 0.5f;

                Color height_rgba = new Color(height, height, height);
                heightImage.SetPixel(x, y, height_rgba);
            }
        }

        File.WriteAllBytes("./tmp_height_plane_use.png", heightImage.EncodeToPNG());
    }

    public void UpdateTerrainInfo()
    {
        var length = global_params.length;
        var width = global_params.width;

        float maxH = float.MinValue;
        float minH = float.MaxValue;


        int[,] offsets = new int[8, 2] {
            { -1,-1}, { 0,-1}, { 0,-1},
            { -1, 0},          { 1, 0},
            { -1, 1}, { 0, 1}, { 1, 1}
        };

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Vector3 pos = GetVector3FromIndex(x, y);
                if (maxH < pos.y)
                {
                    maxH = pos.y;
                    m_terrainInfos.max_height_pos = pos;
                }
                if (minH > pos.y)
                {
                    minH = pos.y;
                    m_terrainInfos.min_height_pos = pos;
                }

                bool isLocalMax = true, isLocalMin = true;

                for (int i = 0; i < 8; ++i)
                {
                    int neighbor_x = x + offsets[i, 0];
                    int neighbor_y = y + offsets[i, 1];

                    if (neighbor_x < 0 || neighbor_y < 0 || neighbor_x >= length || neighbor_y >= width)
                        continue;
                    Vector3 neighbor_pos = GetVector3FromIndex(neighbor_x, neighbor_y);
                    if (neighbor_pos.y < pos.y)
                        isLocalMin = false;

                    if (neighbor_pos.y > pos.y)
                        isLocalMax = false;

                    if (!isLocalMax && !isLocalMin)
                        break;
                }

                if (isLocalMin)
                    m_terrainInfos.local_minimums.Add(pos);
                if (isLocalMax)
                    m_terrainInfos.local_maximums.Add(pos);
            }
        }
    }
    
    private List<TerrainLayer> loadTerrainTextureLayer() {
        string subFolderName = GlobalParams.terrainStyleToString(global_params.terrain_style);

        TerrainLayer plainBaseLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/0");
        TerrainLayer plainTransLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/1");
        TerrainLayer plainSpecialLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/2");
        TerrainLayer mountainBottomLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/3");
        TerrainLayer mountainMidLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/4");
        TerrainLayer mountainCliffLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/5");
        TerrainLayer mountainTopLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/6");
        TerrainLayer cityLayer =
            Resources.Load<TerrainLayer>("TerrainTextures/TerrainLayers/" + subFolderName + "/7");
        
        List<TerrainLayer> layersList = new List<TerrainLayer> {
            plainBaseLayer, plainTransLayer, plainSpecialLayer, mountainBottomLayer,
            mountainMidLayer, mountainCliffLayer, mountainTopLayer, cityLayer
        };

        return layersList;
    }

    public void loadUserDefinedTextureLayer(int textureFlags)
    {
        TerrainData terrainData = Terrain.activeTerrain.terrainData;

        TerrainLayer[] terrainLayers = terrainData.terrainLayers;

        if((textureFlags & 0b1000) > 0)
        {
            terrainLayers[0] = Resources.Load<TerrainLayer>("_temp_Resources/0");
            terrainLayers[0].diffuseTexture = Resources.Load<Texture2D>("_temp_Resources/_New_Textures/" + GlobalTerrainSettingQt.m_surfaceTexture_1);
        }
        if ((textureFlags & 0b0100) > 0)
        {
            terrainLayers[1] = Resources.Load<TerrainLayer>("_temp_Resources/1");
            terrainLayers[1].diffuseTexture = Resources.Load<Texture2D>("_temp_Resources/_New_Textures/" + GlobalTerrainSettingQt.m_surfaceTexture_2);
        }
        if ((textureFlags & 0b0010) > 0)
        {
            terrainLayers[3] = Resources.Load<TerrainLayer>("_temp_Resources/3");
            terrainLayers[3].diffuseTexture = Resources.Load<Texture2D>("_temp_Resources/_New_Textures/" + GlobalTerrainSettingQt.m_mountainTexture_1);
        }
        if ((textureFlags & 0b0001) > 0)
        {
            terrainLayers[4] = Resources.Load<TerrainLayer>("_temp_Resources/4");
            terrainLayers[4].diffuseTexture = Resources.Load<Texture2D>("_temp_Resources/_New_Textures/" + GlobalTerrainSettingQt.m_mountainTexture_2);
        }

        Terrain.activeTerrain.terrainData.terrainLayers = terrainLayers;
    }
    
    // 用来获取float[,] 高度图
    public float[,] Get01HeightMapWithSmallHillInForest() {
        var length = global_params.length;
        var width = global_params.width;


        float[,] noiseMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数

        // 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        int countPixels = 0;
        float avgNoise = 0.0f;
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (m_vertexInfos[y][x].label == TerrainType._FOREST && global_params.terrain_noise_type == TerrainNoiseType._STAMP)
                {
                    noiseMap[x, y] = BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y, x), global_params);

                    avgNoise += noiseMap[x, y];
                    countPixels++;
                }
            }
        }
        avgNoise /= countPixels;

        float[,] heightMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数
        // 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (m_vertexInfos[y][x].label == TerrainType._FOREST && global_params.terrain_noise_type == TerrainNoiseType._STAMP)
                {
                    float city_margin_factor = m_vertexInfos[y][x].city_height_shift_factor / (float)LocalVertexInfo.max_city_height_change_range;
                    float water_margin_factor = m_vertexInfos[y][x].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;


                    // m_vertexInfos[y][x].height += (Mathf.PerlinNoise(y * 0.03f, x * 0.03f) - 0.5f)
                    //      * m_terrainInfos.max_height_pos.y * 0.11f;
                    m_vertexInfos[y][x].height += m_terrainInfos.max_height_pos.y * 0.4f
                        * global_params.plain_noise_amplify * city_margin_factor * water_margin_factor
                        * (noiseMap[x,y] - avgNoise);
                }

            }
        }

        this.UpdateTerrainInfo();

        var highestElevation = m_terrainInfos.max_height_pos.y;
        var lowestElevation = m_terrainInfos.min_height_pos.y;
        var diff = highestElevation - lowestElevation;

        TCP_Client.SendMessage(string.Format("A {0} {1}", highestElevation, lowestElevation));

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                heightMap[x, y] = (m_vertexInfos[y][x].height - lowestElevation) / diff;
            }
        }

                

        //float[,] heightMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数

        //// 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        //for (int x = 0; x < length; x++) {
        //    for (int y = 0; y < width; y++) {
        //        if (m_vertexInfos[y][x].label == TerrainType._FOREST && global_params.terrain_noise_type == TerrainNoiseType._STAMP) {
        //            // m_vertexInfos[y][x].height += (Mathf.PerlinNoise(y * 0.03f, x * 0.03f) - 0.5f)
        //            //      * m_terrainInfos.max_height_pos.y * 0.11f;
        //            m_vertexInfos[y][x].height += m_terrainInfos.max_height_pos.y * 0.09f*
        //                                          (BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y, x) ,global_params) - 0.6f);

        //            //TCP_Client.SendMessage(string.Format("A {0}", (BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y, x), global_params))));
        //        }

        //        heightMap[x, y] = (m_vertexInfos[y][x].height - lowestElevation) / diff;
        //    }
        //}

        return heightMap;
    }


    // 用来获取float[,] 高度图
    public float[,] Get01HeightMapWithSmallNoiseOnTheWild()
    {
        var length = global_params.length;
        var width = global_params.width;


        float[,] noiseMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数

        float random_noise_x = Random.Range(0, 10000.0f);
        float random_noise_y = Random.Range(0, 10000.0f);

        // 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        float max_int = float.MinValue;
        float min_int = float.MaxValue;
        int countPixels = 0;
        float avgNoise = 0.0f;
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (m_vertexInfos[y][x].label != TerrainType._WATER)
                {
                    // 如果没有应用城镇，那么城镇skip
                    if (global_params.remove_small_noise_on_city == true && m_vertexInfos[y][x].label == TerrainType._CITY)
                        continue;

                    noiseMap[x, y] = BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y + random_noise_y, x + random_noise_x), global_params);
                    avgNoise += noiseMap[x, y];
                    countPixels++;

                    max_int = max_int < noiseMap[x, y] ? noiseMap[x, y] : max_int;
                    min_int = min_int > noiseMap[x, y] ? noiseMap[x, y] : min_int;
                }
            }
        }

        avgNoise /= countPixels;

        //TCP_Client.SendMessage("A Added Local Noises (BaseSurfaceModeler_Geo)" + m_terrainInfos.max_height_pos.y.ToString() + " "+ global_params.plain_noise_amplify.ToString() +" " +avgNoise.ToString());
        //TCP_Client.SendMessage("A Added Local Noises (BaseSurfaceModeler_Geo)" + max_int.ToString() + " " + min_int.ToString());

        float[,] heightMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数
        // 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (m_vertexInfos[y][x].label != TerrainType._WATER)
                {
                    // 如果没有应用城镇，那么城镇skip
                    if (global_params.remove_small_noise_on_city == true && m_vertexInfos[y][x].label == TerrainType._CITY)
                        continue;

                   
                    float city_margin_factor = m_vertexInfos[y][x].city_height_shift_factor / (float)LocalVertexInfo.max_city_height_change_range;
                    float water_margin_factor = m_vertexInfos[y][x].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;


                    // m_vertexInfos[y][x].height += (Mathf.PerlinNoise(y * 0.03f, x * 0.03f) - 0.5f)
                    //      * m_terrainInfos.max_height_pos.y * 0.11f;
                    m_vertexInfos[y][x].height += m_terrainInfos.max_height_pos.y * 0.4f
                        * global_params.plain_noise_amplify * city_margin_factor * water_margin_factor
                        * (noiseMap[x, y] - avgNoise);
                }

            }
        }

        this.UpdateTerrainInfo();

        var highestElevation = m_terrainInfos.max_height_pos.y;
        var lowestElevation = m_terrainInfos.min_height_pos.y;
        var diff = highestElevation - lowestElevation;

        TCP_Client.SendMessage(string.Format("A {0} {1}", highestElevation, lowestElevation));

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                heightMap[x, y] = (m_vertexInfos[y][x].height - lowestElevation) / diff;
            }
        }

        //TCP_Client.SendMessage("A Added Local Noises (BaseSurfaceModeler_Geo)" + m_terrainInfos.max_height_pos.y.ToString());

        //float[,] heightMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数

        //// 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        //for (int x = 0; x < length; x++) {
        //    for (int y = 0; y < width; y++) {
        //        if (m_vertexInfos[y][x].label == TerrainType._FOREST && global_params.terrain_noise_type == TerrainNoiseType._STAMP) {
        //            // m_vertexInfos[y][x].height += (Mathf.PerlinNoise(y * 0.03f, x * 0.03f) - 0.5f)
        //            //      * m_terrainInfos.max_height_pos.y * 0.11f;
        //            m_vertexInfos[y][x].height += m_terrainInfos.max_height_pos.y * 0.09f*
        //                                          (BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y, x) ,global_params) - 0.6f);

        //            //TCP_Client.SendMessage(string.Format("A {0}", (BaseSurfaceNoise.GetPlainSmallNoise(new Vector2(y, x), global_params))));
        //        }

        //        heightMap[x, y] = (m_vertexInfos[y][x].height - lowestElevation) / diff;
        //    }
        //}

        return heightMap;
    }

    public float[,] Get01HeightMap() {
        var length = global_params.length;
        var width = global_params.width;
        
        float[,] heightMap = new float[length, width]; // 固定一下最大尺寸？后面感觉可以设置一下参数

        float lowestElevation = m_terrainInfos.min_height_pos.y;
        float highestElevation = m_terrainInfos.max_height_pos.y;
        float diff = highestElevation - lowestElevation;
        
        // 注意这里需要反向，因为built-in terrain的方向是跟本项目的terrain是90度旋转过后的
        for (int x = 0; x < length; x++) {
            for (int y = 0; y < width; y++) {
                heightMap[x, y] = (m_vertexInfos[y][x].height - lowestElevation) / diff;
            }
        }

        return heightMap;
    }


    private (float, float) GetTransitionFactor(float curValue, float pivotValue, float transition_radii)
    {
        float factor = (curValue - pivotValue) / transition_radii;  // -INF ~ +INF

        factor = Mathf.Max(-1, factor);
        factor = Mathf.Min(+1, factor); // refine to [-1, 1]. 0 is to mix uniformly.

        factor = (factor + 1) / 2.0f; // refine to [0,1]

        return (factor, 1 - factor);
    }


    private void ResetMaskVectorToZeros(float[] labels)
    {
        for (int i = 0; i < labels.Length; ++i)
            labels[i] = 0.0f;
    }

    public float[,,] GetTextureMaskMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        float[,,] maskMap = new float[length, width, 8];    // HDRP 最多支持8个

        /*
         * 0，1，2：平原基础纹理，平原过渡纹理，平原特殊纹理
         * 3，4，5，6：山区底部纹理，山区过渡纹理，山区悬崖纹理，山顶纹理
         * 7：城市纹理（目前先确定固定一套）
         */
        //float snowLine = m_terrainInfos.max_height_pos.y > LocalVertexInfo.snowline_height   // 平地的话snowline就是个默认值。
        //    ? m_terrainInfos.max_height_pos.y * global_params.snow_line_rate
        //    : LocalVertexInfo.snowline_height;

        float random_noise_shift_x = Random.Range(0, 10000.0f);
        float random_noise_shift_y = Random.Range(0, 10000.0f);

        /*highest
         * mnt_topHeightLine
         * mnt_middHeightLine
         * mnt_minHeightLine
         * lowest
        */
        float lowest = m_terrainInfos.min_height_pos.y;
        float highest = m_terrainInfos.max_height_pos.y;
        float mnt_botMinHeightLine = lowest + (highest - lowest) * global_params.mountainMinHeightLineRate;   // 被认为是山的最低高度
        float diff = highest - mnt_botMinHeightLine;  // 山的高度的范围数值
        float mnt_middMinHeightLine = mnt_botMinHeightLine + diff * global_params.mountain_bottom_rate;
        float mnt_topMinHeightLine = mnt_middMinHeightLine + diff * global_params.mountain_middle_rate;

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                // 这里也需要反向
                TerrainType type = m_vertexInfos[y][x].label;
                float height = m_vertexInfos[y][x].height;
                float[] textureIndexBasedHeight = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

                if (GlobalParams.loaded_predefined_height_map && !GlobalParams.loaded_predefined_label_map)
                {
                    textureIndexBasedHeight[2] = 1.0f;
                }
                else
                {
                    // 野外区域
                    // mountain height based random texture
                    // 分三层：
                    // 第一层：顶部 & 中部
                    // 第二层：中部 & 底部
                    // 第三层：底部 & 平原基础类型


                    if (height > mnt_botMinHeightLine)
                    {   // 这里10f是一个地面的基准高度
                        // float randPer = BaseSurfaceNoise.GetFbmPerlinNoise(new Vector2(x, y), 4,5f, 0.5f, 2.5f);
                        // float randPer = BaseSurfaceNoise.GetTextureSmallNoise(new Vector2(y, x),
                        //     0.33f, 3f, 6, 0.67f, 2.7f, new Vector2(0f, 0f));
                        float randPer = Mathf.PerlinNoise(y / global_params.mountain_texture_noise_scale + random_noise_shift_x / 2.0f,
                            x / global_params.mountain_texture_noise_scale + random_noise_shift_y / 2.0f);
                        // float randPer = Perlin.Fbm(new Vector2(x * 0.1f, y * 0.1f), 4);  // 这个不太行

                        //if (height > (mnt_botMinHeightLine + diff * global_params.mountain_top_rate))
                        //{               // 纯雪
                        //    textureIndexBasedHeight[6] = 1.0f;
                        //}
                        if (height > mnt_topMinHeightLine)   //[topMinHeight -> highest]  存在bug：topLineRate = 1.001 (雪还会出现)
                        {     // 雪+石头交叉
                              //textureIndexBasedHeight = randPer < 0.3f ? 6 : 4;
                            float factor = (height - mnt_topMinHeightLine) / (highest - mnt_topMinHeightLine);
                            textureIndexBasedHeight[4] = 1 - factor;
                            textureIndexBasedHeight[6] = factor;
                        }
                        else if (height > mnt_middMinHeightLine)    //[middMinHeight -> topMinHeight]
                        {
                            //textureIndexBasedHeight = randPer < 0.15f ? 1 : (randPer < 0.55f ? 3 : 4);
                            float factor = (height - mnt_middMinHeightLine) / (mnt_topMinHeightLine-mnt_middMinHeightLine);
                            textureIndexBasedHeight[3] = 1 - factor;
                            textureIndexBasedHeight[4] = factor;
                        }
                        else   //[botMinHeight -> middMinHeight]
                        {
                            //textureIndexBasedHeight = randPer < 0.25f ? 0 : (randPer < 0.67f ? 0 : 1);
                            float factor = (height - mnt_botMinHeightLine) / (diff * global_params.mountain_bottom_rate);
                            textureIndexBasedHeight[0] = 1-factor;
                            textureIndexBasedHeight[1] = factor;
                        }

                        // normal based texture:  （悬崖）
                        // 计算normal角度
                        float dotProductNormal = Vector3.Dot(m_vertexInfos[y][x].normal.normalized, Vector3.up);
                        float absAngle = Mathf.Abs(Mathf.Acos(dotProductNormal) * Mathf.Rad2Deg);

                        if (absAngle > global_params.mountain_cliff_rate)
                        {
                            ResetMaskVectorToZeros(textureIndexBasedHeight);
                            textureIndexBasedHeight[5] = 1.0f;
                        }
                    }
                    else
                    {
                        // float randPer = BaseSurfaceNoise.GetFbmPerlinNoise(new Vector2(x, y), 5,20f, 0.4f, 2.3f);
                        // float randPer = BaseSurfaceNoise.GetHillSmallNoise(new Vector2(y, x), global_params);
                        float randPer = Mathf.PerlinNoise(y / global_params.plain_texture_noise_scale + random_noise_shift_x, x / global_params.plain_texture_noise_scale + random_noise_shift_y);
                        // float randPer = Perlin.Fbm(new Vector2(x * 0.05f, y * 0.05f), 4);    // 这个不太行

                        if (type is TerrainType._FOREST or TerrainType._GRASS or TerrainType._WATER)
                        {

                            // [现在单纯应用了噪声，然后那个外面控制比例的系数 其实没有涌上来]

                            if (type is TerrainType._FOREST or TerrainType._GRASS or TerrainType._WATER)
                            {
                                var (fac1, fac2) = GetTransitionFactor(randPer, global_params.PlainGrassRate, global_params.PlainTransitionRadii);

                                textureIndexBasedHeight[0] = fac1;
                                textureIndexBasedHeight[1] = fac2;
                            }

                        }
                    }

                    // 城市
                    if (type == TerrainType._CITY)
                    {
                        ResetMaskVectorToZeros(textureIndexBasedHeight);
                        textureIndexBasedHeight[7] = 1.0f;    // pavement
                    }
                }

                for (int i = 0; i < textureIndexBasedHeight.Length; ++i)
                    maskMap[x, y, i] = textureIndexBasedHeight[i];
            }
        }

        return maskMap;
    }


    //public float[,,] GetTextureMaskMap() {
    //    var length = global_params.length;
    //    var width = global_params.width;

    //    float[,,] maskMap = new float[length, width, 8];    // HDRP 最多支持8个

    //    /*
    //     * 0，1，2：平原基础纹理，平原过渡纹理，平原特殊纹理
    //     * 3，4，5，6：山区底部纹理，山区过渡纹理，山区悬崖纹理，山顶纹理
    //     * 7：城市纹理（目前先确定固定一套）
    //     */
    //    //float snowLine = m_terrainInfos.max_height_pos.y > LocalVertexInfo.snowline_height   // 平地的话snowline就是个默认值。
    //    //    ? m_terrainInfos.max_height_pos.y * global_params.snow_line_rate
    //    //    : LocalVertexInfo.snowline_height;

    //    float random_noise_shift_x = Random.Range(0, 10000.0f);
    //    float random_noise_shift_y = Random.Range(0, 10000.0f);
    //    for (int x = 0; x < length; x++) {
    //        for (int y = 0; y < width; y++)
    //        {
    //            // 这里也需要反向
    //            TerrainType type = m_vertexInfos[y][x].label;
    //            float height = m_vertexInfos[y][x].height;
    //            int textureIndexBasedHeight = 0;

    //            if (GlobalParams.loaded_predefined_height_map && !GlobalParams.loaded_predefined_label_map)
    //            {
    //                textureIndexBasedHeight = 2;
    //            }
    //            else
    //            {
    //                // 野外区域
    //                // mountain height based random texture
    //                // 分三层：
    //                // 第一层：顶部 & 中部
    //                // 第二层：中部 & 底部
    //                // 第三层：底部 & 平原基础类型

    //                float lowest = m_terrainInfos.min_height_pos.y;
    //                float highest = m_terrainInfos.max_height_pos.y;

    //                float mnt_minHeightLine = lowest + (highest - lowest) * global_params.mountainMinHeightLineRate;
    //                float diff = highest - mnt_minHeightLine;

    //                if (height > mnt_minHeightLine)
    //                {   // 这里10f是一个地面的基准高度
    //                    // float randPer = BaseSurfaceNoise.GetFbmPerlinNoise(new Vector2(x, y), 4,5f, 0.5f, 2.5f);
    //                    // float randPer = BaseSurfaceNoise.GetTextureSmallNoise(new Vector2(y, x),
    //                    //     0.33f, 3f, 6, 0.67f, 2.7f, new Vector2(0f, 0f));
    //                    float randPer = Mathf.PerlinNoise(y / global_params.mountain_texture_noise_scale + random_noise_shift_x/2.0f,
    //                        x / global_params.mountain_texture_noise_scale + random_noise_shift_y/2.0f);
    //                    // float randPer = Perlin.Fbm(new Vector2(x * 0.1f, y * 0.1f), 4);  // 这个不太行

    //                    if (height > (mnt_minHeightLine + diff*global_params.mountain_top_rate))
    //                    {               // 纯雪
    //                        textureIndexBasedHeight = 6;
    //                    }
    //                    else if (height > (mnt_minHeightLine + diff * global_params.mountain_middle_rate))
    //                    {     // 雪+石头交叉
    //                        textureIndexBasedHeight = randPer < 0.3f ? 6 : 4;
    //                    }
    //                    else if (height > (mnt_minHeightLine + diff * global_params.mountain_bottom_rate))
    //                    {
    //                        textureIndexBasedHeight = randPer < 0.15f ? 1 : (randPer < 0.55f ? 3 : 4);
    //                    }
    //                    else
    //                    {
    //                        // textureIndexBasedHeight = randPer < 0.51f ? 6 : 3;
    //                        textureIndexBasedHeight = randPer < 0.25f ? 0 : (randPer < 0.67f ? 0 : 1);
    //                    }

    //                    // normal based texture:  （悬崖）
    //                    // 计算normal角度
    //                    float dotProductNormal = Vector3.Dot(m_vertexInfos[y][x].normal.normalized, Vector3.up);
    //                    float absAngle = Mathf.Abs(Mathf.Acos(dotProductNormal) * Mathf.Rad2Deg);

    //                    if (absAngle > global_params.mountain_cliff_rate)
    //                    {
    //                        textureIndexBasedHeight = 5;
    //                    }
    //                }
    //                else 
    //                {
    //                    // float randPer = BaseSurfaceNoise.GetFbmPerlinNoise(new Vector2(x, y), 5,20f, 0.4f, 2.3f);
    //                    // float randPer = BaseSurfaceNoise.GetHillSmallNoise(new Vector2(y, x), global_params);
    //                    float randPer = Mathf.PerlinNoise(y / global_params.plain_texture_noise_scale + random_noise_shift_x, x / global_params.plain_texture_noise_scale + random_noise_shift_y);
    //                    // float randPer = Perlin.Fbm(new Vector2(x * 0.05f, y * 0.05f), 4);    // 这个不太行

    //                    if (type is TerrainType._FOREST or TerrainType._GRASS or TerrainType._WATER) {
    //                        if (randPer < global_params.PlainGrassRate) {
    //                            textureIndexBasedHeight = 0; //  plain base  草
    //                        } else if (randPer < global_params.PlainSoilRate) {
    //                            textureIndexBasedHeight = 1; // plain transition    根 泥土
    //                        } else {
    //                            textureIndexBasedHeight = 2; // plain special   砂石
    //                        }
    //                    }
    //                }

    //                // 城市
    //                if (type == TerrainType._CITY) {
    //                    textureIndexBasedHeight = 7;    // pavement
    //                }

    //                //float water_factor = m_vertexInfos[y][x].water_height_shift_factor / (float)LocalVertexInfo.max_water_height_change_range;
    //                //if (type == TerrainType._WATER || Mathf.Abs(water_factor)<0.8f) {
    //                //    textureIndexBasedHeight = 1;    // plain transition
    //                //}
    //            }

    //            maskMap[x, y, textureIndexBasedHeight] = 1.0f;
    //        }
    //    }

    //    return maskMap;
    //}

    public float[,,] GetHeightTextureMaskMap()
    {
        var length = global_params.length;
        var width = global_params.width;

        float[,,] maskMap = new float[length, width, 8];    // HDRP 最多支持8个

        /*
         * 0，1，2：平原基础纹理，平原过渡纹理，平原特殊纹理
         * 3，4，5，6：山区底部纹理，山区过渡纹理，山区悬崖纹理，山顶纹理
         * 7：城市纹理（目前先确定固定一套）
         */
        for (int x = 0; x < length; x++) {
            for (int y = 0; y < width; y++)
            {
                maskMap[x, y, 2] = 1.0f;
            }
        }

        return maskMap;
    }

    List<int> m_old_ParkTreeInstanceIDs = new List<int>();
    public void setupTerrainDetails(TerrainData terrainData, bool isOnlyRedoParkTrees = false) 
    {
        /*
        // HDRP 对detail map支持不好，改用tree paints
        // // detail map, 树、草、石头等
        // terrainData.SetDetailResolution(1024, 16);
        // terrainData.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
        //
        // GameObject testGrassPrefab = Resources.Load<GameObject>("TerrainTextures/TerrainDetails/Grasses/Wild Grass_LOD2");
        // DetailPrototype test_grass_prot = new DetailPrototype {
        //     prototype = testGrassPrefab,
        //     usePrototypeMesh = true,
        //     renderMode = DetailRenderMode.VertexLit,    // HDRP必须是这个
        //     alignToGround = 0.60f,
        //     minWidth = 0.4f,
        //     maxWidth = 2.0f,
        //     minHeight = 0.4f,
        //     maxHeight = 3.0f,
        //     noiseSeed = 114514,
        //     noiseSpread = 2.66f,
        //     useInstancing = true,
        //     useDensityScaling = true,
        // };
        //
        // List<DetailPrototype> detailList = new List<DetailPrototype>(terrainData.detailPrototypes) { test_grass_prot };
        // terrainData.detailPrototypes = detailList.ToArray();
        //
        // int meshIndex = detailList.Count - 1;  // The index of our new mesh prototype
        // int[,] detailMap = new int[global_params.length, global_params.width];
        // for (int y = 0; y < global_params.width; y++) {
        //     for (int x = 0; x < global_params.length; x++) {
        //         if (m_vertexInfos[y][x].label == TerrainType._GRASS) {
        //             detailMap[y, x] = 0;
        //             // detailMap[y, x] = Random.Range(0.0f, 1.0f) < 0.4f ? 2 : 1;  // Randomly places the meshes, change as needed
        //         } else if (m_vertexInfos[y][x].label == TerrainType._FOREST) {
        //             detailMap[y, x] = 0;
        //             // detailMap[y, x] = Mathf.PerlinNoise(y * 0.15f, x * 0.15f) < 0.1f ? 3 : 0 ;
        //         }
        //     }
        // }
        //
        // terrainData.SetDetailLayer(0, 0, meshIndex, detailMap);
        */

        string subFolderName = GlobalParams.terrainStyleToString(global_params.terrain_style);

        if(GlobalTerrainSettingQt.m_tree_species != "")
            subFolderName = GlobalTerrainSettingQt.m_tree_species;

        TCP_Client.SendMessage("A Change Texture: " + subFolderName);



        GameObject grass0 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/grasses/0");
        GameObject grass1 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/grasses/1");
        GameObject grass2 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/grasses/2");
        
        GameObject others0 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/others/0");
        GameObject others1 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/others/1");
        GameObject others2 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/others/2");

        GameObject mountainTree0 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/mountain_tree/0");
        GameObject mountainTree1 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/mountain_tree/1");
        GameObject mountainTree2 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/mountain_tree/2");

        //GameObject plainTree0 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/plain_tree/0");
        //GameObject plainTree1 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/plain_tree/1");
        //GameObject plainTree2 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/plain_tree/2");
        //GameObject plainTree3 = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/plain_tree/3");

        //TCP_Client.SendMessage("A CountTree:" + plainTreeFiles.Count.ToString());

        TreePrototype grassPrototype0 = new TreePrototype {prefab = grass0};
        TreePrototype grassPrototype1 = new TreePrototype {prefab = grass1};
        TreePrototype grassPrototype2 = new TreePrototype {prefab = grass2};
        
        TreePrototype othersPrototype0 = new TreePrototype {prefab = others0};
        TreePrototype othersPrototype1 = new TreePrototype {prefab = others1};
        TreePrototype othersPrototype2 = new TreePrototype {prefab = others2};
        
        TreePrototype mountainTreePrototype0 = new TreePrototype {prefab = mountainTree0};
        TreePrototype mountainTreePrototype1 = new TreePrototype {prefab = mountainTree1};
        TreePrototype mountainTreePrototype2 = new TreePrototype {prefab = mountainTree2};

        // nomral plain tree (green)
        string plainTreeDirectory = Application.dataPath + "/Resources/" + "TerrainDetails/" + subFolderName + "/trees/plain_tree";
        int countPlainTreeFiles = BaseAlgorithm.QueryAllFilesUnderDirectory(plainTreeDirectory, ".prefab").Count;

        // special plain tree (red)
        string redTreeDirectory = Application.dataPath + "/Resources/" + "TerrainDetails/" + subFolderName + "/trees/special_tree";
        int countRedTreeFiles = 0;
        //if (global_params.terrain_style == TerrainStyle._DEFAULT)
        countRedTreeFiles = BaseAlgorithm.QueryAllFilesUnderDirectory(redTreeDirectory, ".prefab").Count;
        /*
         * 0, 1, 2 : grasses
         * 3, 4, 5 : others
         * * 6, 7, 8 : mountain trees
         * 9 to the end : plain trees & special trees.
         */
        List <TreePrototype> treePrototypes = new List<TreePrototype>() 
        {
            grassPrototype0, grassPrototype1, grassPrototype2,
            othersPrototype0, othersPrototype1, othersPrototype2,
            mountainTreePrototype0, mountainTreePrototype1, mountainTreePrototype2,
            //plainTreePrototype0, plainTreePrototype1, plainTreePrototype2, plainTreePrototype3,
        };

        for (int i = 0; i < countPlainTreeFiles; ++i)
        {
            GameObject plainTreeObj = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/plain_tree/" + i.ToString());
            TreePrototype plainTreePrototype = new TreePrototype { prefab = plainTreeObj };
            treePrototypes.Add(plainTreePrototype);
        }

        for (int i = 0; i < countRedTreeFiles; ++i)
        {
            GameObject plainTreeObj = Resources.Load<GameObject>("TerrainDetails/" + subFolderName + "/trees/special_tree/" + i.ToString());
            TreePrototype plainTreePrototype = new TreePrototype { prefab = plainTreeObj };
            treePrototypes.Add(plainTreePrototype);
        }

        
        terrainData.treePrototypes = treePrototypes.ToArray();

        List<TreeInstance> treeInstances = new List<TreeInstance>(terrainData.treeInstances);


        if(isOnlyRedoParkTrees == true &&m_old_ParkTreeInstanceIDs.Count !=0)  // 仅当
        {
            
            m_old_ParkTreeInstanceIDs.Sort((x, y) => -x.CompareTo(y));
            //string str = treeInstances.Count.ToString() +"| ";
            for (int i = 0; i < m_old_ParkTreeInstanceIDs.Count; ++i)
            {
                treeInstances.RemoveAt(m_old_ParkTreeInstanceIDs[i]);
                //str += m_old_ParkTreeInstanceIDs[i].ToString()+" ";
            }
            //Debug.Log(str);
        }
        m_old_ParkTreeInstanceIDs.Clear();

        // place tree and grass...
        var length = global_params.length;
        var width = global_params.width;

        float lowest = m_terrainInfos.min_height_pos.y;
        float highest = m_terrainInfos.max_height_pos.y;
        float diff = highest - lowest;
        float mntTree_LowlimitHeight= lowest + diff*global_params.zh_mntTree_MinHeightLine_Rate;

        for (int y = 0; y < width; y++) 
        {
            for (int x = 0; x < length; x++) 
            {
                // normal based placement:
                // 计算normal角度
                float dotProductNormal = Vector3.Dot(m_vertexInfos[y][x].normal.normalized, Vector3.up);
                float absAngle = Mathf.Abs(Mathf.Acos(dotProductNormal) * Mathf.Rad2Deg);
                    
                if (absAngle > 65.0f) 
                {
                    continue;
                }

                if (Random.Range(0.0f, 1.0f) > GlobalTerrainSettingQt.m_globalTreeProbablity)  // Qt的控制影响在这里。
                    continue;

                // 高度越高，角度越抖，越接近0，越不容易长植被
                //float probability = (1.0f - absAngle / 90.0f) * (1.0f - currentHeight / diff);
                //if (Random.Range(0.0f, 1.0f) > probability) 
                //{
                //    continue;
                //}
                float currentHeight = m_vertexInfos[y][x].height;
                currentHeight = Mathf.Max(mntTree_LowlimitHeight, currentHeight);  // 当前curHeight必须大于等于mntTree_lowLimit。

                float tree_HeightProb = global_params.zh_mntTree_reduction_rate * (1.0f-(currentHeight - lowest) / (diff)); // [1.0f, -> 0.0f]
                if (Random.Range(0.0f, 1.0f) < tree_HeightProb) // probablity越大，山峰处树越多。
                {
                    continue;
                }

                float probability = (1.0f - absAngle / 90.0f);


                if (Random.Range(0.0f, 1.0f) > probability) // 按照坡度来决定，probablity越大，树越不生成。
                {
                    continue;
                }



                TerrainType terrainType = m_vertexInfos[y][x].label;
                PlantType plantType = m_vertexInfos[y][x].plant_type;

                float plant_tree_scale = 1.0f;
                if (global_params.terrain_style == TerrainStyle._SNOWLAND)
                    plant_tree_scale *= 1.3f;
                // 功能区的树木
                if (plantType == PlantType._parkTree)
                {
                    int typeIndex = Random.Range(9, 9 + countPlainTreeFiles);
       
                    TreeInstance treeInstance = new TreeInstance
                    {
                        position = new Vector3(y / 1025f,
                            (m_vertexInfos[y][x].height - lowest) / diff,
                            x / 1025f),
                        prototypeIndex = typeIndex,
                        widthScale = Random.Range(0.3f, 0.5f) * plant_tree_scale,
                        heightScale = Random.Range(0.3f, 0.5f) * plant_tree_scale,
                        rotation = Random.Range(0.0f, Mathf.PI),
                        color = Color.white, // 或许可以用于季节变化对植被对改变？
                        lightmapColor = Color.white
                    };

                    treeInstances.Add(treeInstance);
                    m_old_ParkTreeInstanceIDs.Add(treeInstances.Count - 1);
                }

                if(isOnlyRedoParkTrees == true)
                {
                    continue;
                }

                // 户外森林区域。
                if (global_params.enable_grass) 
                {
                    // 第一个if分支可以删除了，因为没用到GRASS
                    if (terrainType == TerrainType._GRASS && plantType == PlantType._none) 
                    {
                        TreeInstance grassInstance = new TreeInstance 
                        {
                            position = new Vector3(y / 1025f, 
                                (m_vertexInfos[y][x].height - lowest) / diff, 
                                x / 1025f),
                            prototypeIndex = 
                                Random.Range(0.0f, 1.0f) < 0.7f ? Random.Range(0, 3) : Random.Range(3, 6),
                            widthScale = Random.Range(0.2f, 0.6f) * plant_tree_scale,
                            heightScale = Random.Range(0.2f, 0.6f) * plant_tree_scale,
                            rotation = Random.Range(0.0f, Mathf.PI),
                            color = Color.white, // 或许可以用于季节变化对植被颜色改变？
                            lightmapColor = Color.white
                        };
                    
                        treeInstances.Add(grassInstance);
                    } 
                    else if (terrainType == TerrainType._FOREST && plantType == PlantType._none) 
                    {
                        if ((Mathf.PerlinNoise(y * 0.05f, x * 0.05f) < 0.6f && Random.Range(0.0f, 1.0f) < 0.8f) || 
                            Random.Range(0.0f, 1.0f) < 0.08f) 
                        {
                            TreeInstance grassInstance = new TreeInstance 
                            {
                                position = new Vector3(y / 1025f, 
                                    (m_vertexInfos[y][x].height - lowest) / diff, 
                                    x / 1025f),
                                prototypeIndex = 
                                    Random.Range(0.0f, 1.0f) < 0.8f ? Random.Range(0, 3) : Random.Range(3, 6),
                                widthScale = Random.Range(0.2f, 0.6f) * plant_tree_scale,
                                heightScale = Random.Range(0.2f, 0.6f) * plant_tree_scale,
                                rotation = Random.Range(0.0f, Mathf.PI),
                                color = Color.white, // 或许可以用于季节变化对植被对改变？
                                lightmapColor = Color.white
                            };
                    
                            treeInstances.Add(grassInstance);
                        }
                    }
                }

                // Trees
                if (plantType != PlantType._none) 
                {
                    int typeIndex;
                    // 以后只分山上的树和平原树，以及功能区的树
                    // red 还有 Green Tree 的 Type 现在都没用的

                    if (plantType == PlantType._pineTree)
                    {
                        typeIndex = Random.Range(6, 9);
                    }
                    else if (plantType == PlantType._parkTree)
                    {
                        typeIndex = Random.Range(9, 9 + countPlainTreeFiles);
                    }
                    else
                    {
                        // 给一点点概率特殊树种
                        if (Random.Range(0.0f, 1.0f) < 0.97f)
                        {
                            typeIndex = Random.Range(9, 9 + countPlainTreeFiles); // 3 (9, 10, 11) 9+3 = 12
                        }
                        else
                        {
                            typeIndex = Random.Range(9 + countPlainTreeFiles, treePrototypes.Count);
                        }
                    }
                    //else
                    //    typeIndex = Random.Range(9, treePrototypes.Count);
                    //typeIndex = plantType == PlantType._pineTree ?
                    //    Random.Range(6, 9) :
                    //    Random.Range(9, treePrototypes.Count);

                    // debug: typeIndex = 10;

                    if(plantType != PlantType._parkTree)
                    {
                        TreeInstance treeInstance = new TreeInstance
                        {
                            position = new Vector3(y / 1025f,
                            (m_vertexInfos[y][x].height - lowest) / diff,
                            x / 1025f),
                            prototypeIndex = typeIndex,
                            widthScale = Random.Range(0.3f, 0.5f) * plant_tree_scale,
                            heightScale = Random.Range(0.3f, 0.5f) * plant_tree_scale,
                            rotation = Random.Range(0.0f, Mathf.PI),
                            color = Color.white, // 或许可以用于季节变化对植被对改变？
                            lightmapColor = Color.white
                        };

                        treeInstances.Add(treeInstance);
                    }
                }
            }
        }

        terrainData.treeInstances = treeInstances.ToArray();
        terrainData.RefreshPrototypes();
    }

    
    
    
}
