using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Serialization;


public class GlobalTerrainSettingQt
{
    public static TerrainStyle terrain_stype = TerrainStyle._DEFAULT;
    public static float m_lightIntensity = 53619.97f;
    public static float m_lightTemperature = 12942.0f;

    public static float m_globalTreeProbablity = 0.5f; // 二次

    public static string m_tree_species = "";
    public static string m_surfaceTexture_1 = "default_reg_tex1";
    public static string m_surfaceTexture_2 = "default_reg_tex2";
    public static string m_mountainTexture_1 = "default_reg_tex3";
    public static string m_mountainTexture_2 = "default_reg_tex4";

    public static string m_city_type = "";
    public static float m_bldgDensityFactor = 1.0f;
    public static float m_bldgHeightFactor = 1.0f;
    public static float m_bldgRoadCurveFactor = 1.0f;
}


[CreateAssetMenu(fileName = "GlobalParams", menuName = "ScriptableObjects/GlobalParams", order = 1)]
public class GlobalParams : ScriptableObject
{

    // for dataset generation
    public static bool enable_dataset_mode = false;
    public static int dataset_size = 200;

    public static bool loaded_predefined_label_map = false;
    public static bool loaded_predefined_height_map = false;

    public static bool showing_height_map = false;
    public bool show_height_map = false;



    // general settings
    [Header("Map Size")]
    public float global_scaling = 0.1f;
    public int length = 600;
    public int width = 600;

    [Header("Map Style")]
    public TerrainStyle terrain_style = TerrainStyle._DEFAULT;


    [Header("Predefine Map")]
    public Texture2D predefine_label_map;
    public Texture2D predefine_height_map;

    [Header("Terrain Gloabl Noise Configuration")]
    // public bool enable_noise_based_terrain = true;

    public TerrainNoiseType terrain_noise_type = TerrainNoiseType._STAMP;
    public int base_noise_seed = 112;
    public float base_noise_scale = 120f;
    public float base_noise_amplify = 120f;
    public int base_noise_octaves = 6;
    public float base_noise_persistence = 0.53f;
    public float base_noise_lacunarity = 2.43f;
    public Vector2 base_noise_offset = new Vector2(0f, 0f);
    public float gradient_trick_decay = 1.0f;
    public AnimationCurve base_noise_curve;
    //public float snow_line_rate = 0.96f;

    // public int DLA_random_seed = 114514;
    // public int DLA_process_level = 8;
    // public int DLA_node_number = 1; // 现在感觉没用
    // [Range(0.0f, 0.99f)]
    // public float DLA_occupantion_percent = 0.15f;

    [Header("[Texture] Mountain Layer Distribution")]
    [Range(0.0f, 1.0f)]
    public float mountainMinHeightLineRate = 0.5f;
    //[Range(0.0f, 1.001f)]
    //public float mountain_top_rate = 0.95f;
    [Range(0.0f, 1.001f)]
    public float mountain_middle_rate = 0.85f;
    [Range(0.0f, 1.001f)]
    public float mountain_bottom_rate = 0.6f;
    [Range(30.0f, 90.0f)]
    public float mountain_cliff_rate = 70.0f;
    public float mountain_texture_noise_scale = 20f;

    [Header("[Texture] Plain Layer Distribution")]
    [Range(0.0f, 1.001f)]
    public float PlainGrassRate = 0.45f;
    [Range(0.0001f, 0.5f)]
    public float PlainTransitionRadii = 0.3f;
    public float plain_texture_noise_scale = 20f;

    [Header("[Tree] Distribution and Density Configuration")]

    public float globalTreeExistRate = 0.2f;

    public float defaultTreeDensityThreshold = 0.9f;
    public float defaultTreeTransitionThreshold = 0.8f;
    public float defaultTreeWildThreshold = 0.01f;

    public float desertTreeDensityThreshold = 0.99f;
    public float desertTreeTransitionThreshold = 0.98f;
    public float desertTreeWildThreshold = 0.001f;

    public float parkTreeThreshold = 0.1f;

    public float treePerlinScale = 240f;
    public float treeHeightLine = 0.5f;

    [Range(0.0f, 1.0f)]
    public float zh_mntTree_MinHeightLine_Rate = 0.7f;
    [Range(0.0f, 1.0f)]
    public float zh_mntTree_reduction_rate = 0.9f;

    // public float default_tree_probability = 0.05f;
    // public float default_tree_wild_probability = 0.001f;
    // [Range(0.0f, 1.0f)]
    // public float default_tree_distribution_rate = 0.6f;
    // public float default_tree_distribution_perlin_scale = 50;
    //
    // public float desert_tree_probability = 0.004f;


    [Header("Plain Small Noise Configuration")]

    public bool apply_small_noise_globally = true;   // 使用在全部地区都应用小噪声。(甭管啥模式）
    public bool remove_small_noise_on_city = false;  // 移除城镇地区的小噪声。
    public int plain_noise_seed = 0;
    public float plain_noise_scale = 33f;
    public float plain_noise_amplify = 3f;
    public int plain_noise_octaves = 4;
    public float plain_noise_persistence = 0.5f;
    public float plain_noise_lacunarity = 2.4f;
    public Vector2 plain_noise_offset = new(0f, 0f);
    public AnimationCurve plain_noise_hill_curve;

    public int lake_num = 2;

    [Header("Appearance")]
    public bool apply_grid_city_mode = false;
    [Range(0.0f, 1.001f)]
    public float RoadMaxHeightLineOnTheWild = 0.5f;
    public Color water_refracting_color = new Color(36 / 255.0f, 175 / 255.0f, 175 / 255.0f);
    public Color water_scattering_color = new Color(10 / 255.0f, 164 / 255.0f, 141 / 255.0f);

    [Range(0.0f, 5.001f)]
    public float water_depth = 1.5f;

    [Header("City & Lake Distribution Configuration")]
    public Vector2 city_floodfill_height_threshold = new Vector2(10f, 16f);
    public Vector2 lake_floodfill_height_threshold = new Vector2(10f, 14f);



    [Header("Details")]
    public bool enable_grass = true;

    [Header("Others")]
    // global paths
    [SerializeField]
    public PythonPathOption python_path_option;


    [Header("Night Light Setting")]
    public bool open_night_light = false;
    public float point_light_lower_min_intensity = 20000;
    public float point_light_lower_max_intensity = 80000;
    public float point_light_upper_min_intensity = 200000;
    public float point_light_upper_max_intensity = 300000;
    [Range(0.0f, 1.0f)] public float point_light_upper_prob_intensity = 0.3f;

    public float point_light_min_range = 8.0f;
    public float point_light_max_range = 20.0f;
    [Range(0.0f, 1.0f)] public float point_light_rate = 0.3f;

    private Dictionary<PythonPathOption, string> pythonPaths = new Dictionary<PythonPathOption, string>()
    {
        { PythonPathOption.lf_mac, "/Users/m1kann/DevTool/Anaconda/anaconda3/bin/python" },
        { PythonPathOption.lf_win, "D:/Anaconda/python.exe" },
        { PythonPathOption.lf_win_lab, "C:/Anaconda/python.exe" },
        { PythonPathOption.lzh, "D:/Program Files/Python/python.exe" },
        // ... more paths
    };

    public string path_python_win_exe
    {
        get { return pythonPaths[python_path_option]; }
    }

    // public string path_python_win_exe = "D:/Anaconda/python.exe";
    // static public string path python win exe = "D:/IDE/Anaconda/envs/UnityPlant/python. exe";

    public static string terrainStyleToString(TerrainStyle terrainStyle)
    {
        return terrainStyle switch
        {
            TerrainStyle._DEFAULT => "default",
            TerrainStyle._DESERT => "desert",
            TerrainStyle._SNOWLAND => "snowland",
            _ => ""
        };
    }
}

public enum PythonPathOption
{
    lf_mac,
    lf_win,
    lf_win_lab,
    lzh,
    // ... add more path
}


public enum TerrainStyle
{
    _DEFAULT,
    _DESERT,
    _SNOWLAND,
    _end
}

public enum TerrainNoiseType
{
    _STAMP,
    _FRAC_PERLIN,
    _GRADIENT,
    _DLA
}

public enum TerrainType   // current maximum is up to 50. (no need more at this moment.)
{
    _FOREST,    // 这个也可以里面加噪声来进行随机。无论是高度还是texture
    _CITY,
    _WATER,

    _GRASS,
    _SNOW,
    _end  // please do not use this 'end', which is only used for counting.
}



public class GlobalResources
{
    static public Dictionary<TerrainType, string> m_terrainTexs = new Dictionary<TerrainType, string>();

    static public (Texture2D atlas, Vector4[] uvs) m_terrainAtlas;

    static public void InitTerrainTextureAtlas()
    {
        for (int i = 0; i < (int)TerrainType._end; ++i)
        {
            string texturePath = string.Format("TerrainTextures/{0}_Albedo", System.Enum.GetName(typeof(TerrainType), i));
            m_terrainTexs.Add((TerrainType)i, texturePath);

            TCP_Client.SendMessage("A " + string.Format("[Load Texture] {0}, {1}, {2}", i, System.Enum.GetName(typeof(TerrainType), i), texturePath));

        }

        Texture2D[] array = new Texture2D[m_terrainTexs.Count];

        int count = 0;
        for (int key = 0; key < m_terrainTexs.Count; ++key)
        {
            array[count++] = Resources.Load<Texture2D>(m_terrainTexs[(TerrainType)key]);
        }

        m_terrainAtlas.atlas = new Texture2D(6 * 250, 6 * 250);
        Rect[] uvs = m_terrainAtlas.atlas.PackTextures(array, 0, 6 * 250);

        m_terrainAtlas.uvs = new Vector4[50];

        for (int i = 0; i < m_terrainTexs.Count; ++i)
        {
            m_terrainAtlas.uvs[i] = new Vector4(uvs[i].x, uvs[i].y, uvs[i].width, uvs[i].height);
            //float marginX = uvs[i].width * 0.001f;
            //float marginY = uvs[i].width * 0.001f;
            //m_terrainAtlas.uvs[i] = new Vector4(uvs[i].x + marginX, uvs[i].y+marginY, uvs[i].width-marginX, uvs[i].height-marginY);
        }

        //for (int i = 0; i < 50; ++i)
        //{
        //    Debug.Log(m_terrainAtlas.uvs[i].ToString());
        //}

        File.WriteAllBytes("./tmp_tex_atlas.png", m_terrainAtlas.atlas.EncodeToPNG());
    }


}

public class GlobalFunctions
{
    static public int ClipIndex(int i, int w)
    {
        if (i < 0) return -i;
        if (i >= w) return w - (i - w) - 1; // w=5, i=5 : (5-(5-5)-1=4), w=5, i=7: (5-(7-5)-1)=2)
        return i;
    }

}
