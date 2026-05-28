using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEditor.PackageManager;

public class MainScript : MonoBehaviour
{
    // load params data
    public GlobalParams global_params;
    
    // the main terrain object. (Everything starts here)
    TerrainAdapter m_terrainAdapter;
    CityGenerator cityGenerator = new CityGenerator();

    TCP_Qt_Server m_tcp_qt_server;

    // external game objects:
    public Light m_globalLight = null;

    void Start()
    {
        // set up python server. now you could print msg through "TCP_Client.SendMessage("A "+"your content!")" or performing external calculations.
        //PythonHandler.RunPythonScript(global_params.path_python_win_exe, Application.dataPath+ "/Scenes/Scripts/Util/Python_Win_Server.py", "");
        //TCP_Client.ConnectToServer();

        m_tcp_qt_server = new TCP_Qt_Server();
        m_tcp_qt_server.StartServerThread(this);

        if (GlobalParams.enable_dataset_mode) 
        {
            GenerateDatasets();
            return;
        }

        // hey, build up your own terrains!
        m_terrainAdapter = new TerrainAdapter(global_params);
        cityGenerator = new CityGenerator();
        cityGenerator.ConstructCityLayout(global_params.apply_grid_city_mode);
        m_terrainAdapter.GenTerrainMap(cityGenerator);
        m_terrainAdapter.BuildFinalMesh(cityGenerator);
    }




    void Update()
    {
        UpdateUserInteraction();


        if (global_params.show_height_map == true)
        {
            if (GlobalParams.showing_height_map == false)
            {
                m_terrainAdapter.ChangeTerrainForHeightShow();
                GlobalParams.showing_height_map = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.F1)) // ¸üÐÂË®
        {
            m_terrainAdapter.ResetWaterMesh();
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            m_terrainAdapter.ResetTrees(cityGenerator);
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {
            m_terrainAdapter.ResetSurfaceTextures();
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            m_terrainAdapter.ResetRoadMesh(cityGenerator);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            cityGenerator.ConstructCityLayout(global_params.apply_grid_city_mode);
            m_terrainAdapter.ResetCityStuff(cityGenerator);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            m_terrainAdapter.ActivateNightLighting();
        }

        // else (global_params.show_height_map == false)
        // {
        //     if (GlobalParams.showing_height_map == true)
        //     {
        //         m_terrainAdapter.RecoverTerrainForNormal();
        //         GlobalParams.showing_height_map = false;
        //     }
        // }
    }

    // create dataset
    private void GenerateDatasets() 
    {
        int n = GlobalParams.dataset_size;
        m_terrainAdapter = new TerrainAdapter(global_params);
        CityGenerator cityGenerator = new CityGenerator();

        TCP_Client.SendMessage("A Start Dataset Generation - Size:" + n);

        for (int i = 0; i < n; i++) 
        {
            cityGenerator.ConstructCityLayout(global_params.apply_grid_city_mode);
            m_terrainAdapter.GenTerrainMap(cityGenerator);
            m_terrainAdapter.BuildFinalMesh(cityGenerator);
            MoveAndRenameFiles(i+1);
            TCP_Client.SendMessage("A Done:" + (i+1).ToString());
        }
        
    }

    private void MoveAndRenameFiles(int number) 
    {
        string originalHeightMapPath = "./tmp_height.png";
        string originalMaskMapPath = "./tmp_colored_mask_map.png";
        
        string targetHeightDirectory = "../datasets/heights/";
        string targetMaskDirectory = "../datasets/masks/";
        
        string targetHeightMapPath = Path.Combine(targetHeightDirectory, number.ToString() + ".png");
        string targetMaskMapPath = Path.Combine(targetMaskDirectory, number.ToString() + ".png");
        
        Directory.CreateDirectory(targetHeightDirectory);
        Directory.CreateDirectory(targetMaskDirectory);
        
        MoveFile(originalHeightMapPath, targetHeightMapPath);
        MoveFile(originalMaskMapPath, targetMaskMapPath);
    }
    
    private static void MoveFile(string originalPath, string targetPath) 
    {
        if (File.Exists(originalPath)) 
        {
            if (File.Exists(targetPath))
                File.Delete(targetPath);

            File.Move(originalPath, targetPath);
        }
        else
        {
            UnityEngine.Debug.LogError("File not found: " + originalPath);
        }
    }

    private void OnDisable()
    {
        GlobalParams.loaded_predefined_label_map = false;
        GlobalParams.loaded_predefined_height_map = false;
        GlobalParams.showing_height_map = false;
    }

    private void UpdateUserInteraction()
    {
        if(m_tcp_qt_server.isUpdateLightSetting == true)
        {
            m_globalLight.colorTemperature = GlobalTerrainSettingQt.m_lightTemperature;
            m_globalLight.intensity = GlobalTerrainSettingQt.m_lightIntensity;
            m_tcp_qt_server.isUpdateLightSetting = false;
        }

        if(m_tcp_qt_server.isUpdateTexturePaths > 0)
        {
            m_terrainAdapter.RegenerateTexturesUnderQtControl(m_tcp_qt_server.isUpdateTexturePaths);

            m_tcp_qt_server.isUpdateTexturePaths = 0b0000;
        }

        if (m_tcp_qt_server.isTreeDensityChanged == true)
        {
            m_terrainAdapter.RegenerateTreesUnderQtControl();

            m_tcp_qt_server.isTreeDensityChanged = false;
        }

        if (m_tcp_qt_server.isUpdateCitySettings == true)
        {
            cityGenerator.ConstructCityLayout(global_params.apply_grid_city_mode);
            m_terrainAdapter.ResetCityStuff(cityGenerator);

            m_tcp_qt_server.isUpdateCitySettings = false;
        }
    }
}
