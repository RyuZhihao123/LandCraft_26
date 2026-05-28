using UnityEngine;

public class TiledTerrainGenerator : MonoBehaviour
{
    [Header("Heightmap Settings")]
    public Texture2D heightmap;
    public float heightMultiplier = 10f;
    public float scale = 1f;

    [Header("Tile Settings")]
    public int tileSize = 64;

    [Header("Material")]
    public Material terrainMaterial;

    private int mapWidth;
    private int mapHeight;

    void Start()
    {
        if (heightmap == null || terrainMaterial == null)
        {
            Debug.LogError("Missing heightmap or terrain material.");
            return;
        }

        mapWidth = heightmap.width;
        mapHeight = heightmap.height;

        GenerateTiledTerrain();
    }

    void GenerateTiledTerrain()
    {
        int tilesX = (mapWidth - 1) / tileSize;
        int tilesY = (mapHeight - 1) / tileSize;

        for (int ty = 0; ty < tilesY; ty++)
        {
            for (int tx = 0; tx < tilesX; tx++)
            {
                int startX = tx * tileSize;
                int startY = ty * tileSize;
                CreateTile(startX, startY, tileSize + 1, tileSize + 1, $"Tile_{tx}_{ty}");
            }
        }
    }

    void CreateTile(int startX, int startY, int width, int height, string name)
    {
        Vector3[] vertices = new Vector3[width * height];
        Vector2[] uvs = new Vector2[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = x + y * width;

                // Clamp to avoid overflow
                int sampleX = Mathf.Min(startX + x, mapWidth - 1);
                int sampleY = Mathf.Min(startY + y, mapHeight - 1);

                float gray = heightmap.GetPixel(sampleX, sampleY).grayscale;
                float worldX = (startX + x) * scale;
                float worldZ = (startY + y) * scale;
                float worldY = gray * heightMultiplier;

                vertices[i] = new Vector3(worldX, worldY, worldZ);

                // Global UV based on full terrain
                uvs[i] = new Vector2((float)(startX + x) / (mapWidth - 1), (float)(startY + y) / (mapHeight - 1));
            }
        }

        int triIndex = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int i = x + y * width;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + width;
                triangles[triIndex++] = i + width + 1;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + width + 1;
                triangles[triIndex++] = i + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GameObject tile = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        tile.transform.parent = this.transform;

        tile.GetComponent<MeshFilter>().mesh = mesh;
        tile.GetComponent<MeshRenderer>().material = terrainMaterial;
    }
}
