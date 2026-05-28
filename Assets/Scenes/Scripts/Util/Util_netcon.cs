using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;


// Unity-Python
public class TCP_Client
{

    static private Socket socketSend;                   //???????????????????????????

    //????????
    static public void ConnectToServer()
    {
        try
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            socketSend.Connect(new IPEndPoint(ip, 7878));
            Debug.Log("Successfully Connect!");
        }
        catch { }
    }
    static public string SendMessage(string message)
    {
        if (socketSend.Connected == false)
            return "";
        // Send data
        {
            byte[] buffer = new byte[1024 * 8];
            buffer = Encoding.UTF8.GetBytes(message);
            socketSend.Send(buffer);
        }
        // Receive Data
        {
            byte[] buffer = new byte[1024 * 6];
            int len = socketSend.Receive(buffer);
            if (len == 0)
                return "";

            string recMes = Encoding.UTF8.GetString(buffer, 0, len);
            // Debug.Log("Receive Data:" + recMes);
            return recMes;
        }
    }
}


// Used to run python file
public class PythonHandler
{
    static public string result = ""; // the latest return msg from python server.

    static public void RunPythonScript(string python_exe, string script_path, string argvs)
    {
        var p = new System.Diagnostics.Process();

        string command = script_path + " " + argvs; // ???

        p.StartInfo.FileName = python_exe;
        p.StartInfo.Arguments = command;


        p.StartInfo.UseShellExecute = true;
        p.StartInfo.RedirectStandardOutput = false;
        p.StartInfo.RedirectStandardError = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.CreateNoWindow = false;

        p.Start();

        //p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Get_data);
        //p.WaitForExit();
        System.Threading.Thread.Sleep(1000);
    }

    private static void Get_data(object sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
    {
        //Debug.Log("Hi!!!!");
        if (!string.IsNullOrEmpty(eventArgs.Data))
        {
            result = eventArgs.Data;
        }
    }
}


// For Qt - Unity connection
public class TCP_Qt_Server
{
    private Thread thStartServer;   // thread object
    private string str_ip = "127.0.0.1";
    private string str_port = "7852";
    
    public MainScript m_main_obj = null;   
    // 这玩意儿是我们的terrain的主对象。
    // 因为要允许这个server访问terrain的主程序，来修改参数（森林的密度，城镇的密度，etc）。
    
    List<string> m_lines = new List<string>();  // 用不上，这个是文件中读取的一行一行。

    // [核心函数] 启动服务器监听。
    public void StartServerThread(MainScript mainObj)
    {
        // 创建了这个线程，并且指定线程要执行哪一个函数。
        thStartServer = new Thread(ClientListener);
        thStartServer.Start();

        m_main_obj = mainObj; // this指针。
    }



    // [核心函数] 监听函数。 这个函数是一个独立的线程。
    private void ClientListener()
    {
        const int bufferSize = 12000;
        TcpListener tlistener = new TcpListener(IPAddress.Parse(str_ip), int.Parse(str_port));

        tlistener.Start();   // 等待客户端链接。这个线程会Block掉。
        TCP_Client.SendMessage("A [QT Server] Thread has been started successfully!");

        TcpClient remoteClient = tlistener.AcceptTcpClient();//接收已连接的客户端,阻塞
        TCP_Client.SendMessage("A [QT Server] One connection is created!");

        NetworkStream streamToClient = remoteClient.GetStream();//获得来自客户端的流

        do
        {
            try
            {
                byte[] buffer = new byte[bufferSize];

                int byteRead = streamToClient.Read(buffer, 0, bufferSize);
                if (byteRead == 0)
                {
                    break;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, byteRead);
                TCP_Client.SendMessage(string.Format("A [QT Server] Received msg: {0} \n", msg));
                //TCP_Client.SendMessage(string.Format("A {0}", msg.Length));
                
                if (msg.Length < 3)
                    continue;

                string[] commands = msg.Split();  // 客户端发送的字符串切片。

                if (commands[0] == "A" && commands.Length >= 2)
                {
                    m_lines.Clear();  // 清除文件内容
                    IEnumerable<string> lines = File.ReadLines(commands[1]);

                    foreach (var line in lines)
                    {
                        if (line.Length < 2) continue;

                        this.m_lines.Add(line);
                    }

                    Debug.Log("[A] Receive Msg from Qt: " + msg);
                    // this.ConstructedTree();
                }

                // 重新生成地形
                if (commands[0] == "B" && commands.Length >= 2)
                {
                    Debug.Log("[B] Receive Msg from Qt: " + msg);
                    this.GenerateTerrain();
                }

                // 更新一些参数
                if (commands[0] == "C" && commands.Length >= 1)  
                {
                    // 植被密度改变
                    //if (commands[1] == "Plant")
                    //{
                    //    this.UpdatePlantDensity(float.Parse(commands[2]));
                    //}

                    //// 摄像机运动模式
                    //if (commands[1] == "Free")
                    //{
                    //    InteractionInfo.freeControl = true;
                    //} else if (commands[1] == "Fix")
                    //{
                    //    InteractionInfo.shouldResetView = true;
                    //    InteractionInfo.freeControl = false;
                    //}

                    //Debug.Log("[C] Receive Msg from Qt: " + msg);

                    m_lines.Clear();  // 清除文件内容
                    IEnumerable<string> lines = File.ReadLines(commands[1]);

                    foreach (var line in lines)
                    {
                        if (line.Length == 0) continue;

                        this.m_lines.Add(line);
                    }
                    this.UpdateSettings();
                }
                
                if (commands[0] == "D" && commands.Length >= 2)  // 远程控制鼠标交互操作：相机
                {
                    this.HandleUserInteraction(commands);
                }

                if (commands[0] == "E" && commands.Length >= 2)  // 清除
                {
                    // if (commands[1] == "Regenerate" && commands[2] == "Leaf")
                    //     isRegenerateLeaf = true;
                    // if (commands[1] == "Regenerate" && commands[2] == "Twig")
                    //     isRegenerateTwig = true;
                    // if (commands[1] == "Hide" && commands[2] == "Leaf")
                    //     isHideLeaf = true;
                    // if (commands[1] == "Hide" && commands[2] == "Twig")
                    //     isHideTwig = true;
                    // if (commands[1] == "Clear" && commands[2] == "Leaf")
                    //     isClearLeaf = true;
                    // if (commands[1] == "Clear" && commands[2] == "Twig")
                    //     isClearTwig = true;
                    
                    Debug.Log("[E] Receive Msg from Qt: " + msg);
                }

            }
            catch (System.Exception ex)
            {
                TCP_Client.SendMessage("A [QT Server] Client Exception occured: " + ex.Message);
                break;
            }
        }
        while (true);
    }

    public bool needGenerateTerrain = false;
    public bool needUpdatePlant = false;
    public float plantDensity = 0.0f;
    public void GenerateTerrain()
    {
        TCP_Client.SendMessage(string.Format("A Construction started!\n"));
        needGenerateTerrain = true;
    }
    
    public void UpdatePlantDensity(float density) {
        TCP_Client.SendMessage(string.Format("A Plant Density Change\n"));
        needUpdatePlant = true;
        plantDensity = density;
    }

    // // 读取skeleton的buffer文件
    // public bool isTreeCreated = false;
    // public Internode root = null;

    // public void ConstructedTree()
    // {
    //     // 告知网络后，之后执行调用Main来执行地形生成。
    //     TCP_Client.SendMessage(string.Format("A Construction started!\n"));
    //     isTreeCreated = true;
    // }
    //
    // public bool isPopulateTwigs = false;
    // public List<List<Vector2>> m_userDrawnFoliageLines = new List<List<Vector2>>();
    //
    // public void PopulateTwigs()
    // {
    //     
    //     TCP_Client.SendMessage(string.Format("A Foliage line Number = {0}/{1}", m_userDrawnFoliageLines.Count, m_lines.Count));
    //     isPopulateTwigs = true;
    // }

    public class InteractionInfo
    {
        public static bool enable = false;
        
        public static bool freeControl = true;
        public static bool shouldResetView = false;
        
        public static string interaction_type = "none";
        public static float interaction_dx = 0;
        public static float interaction_dy = 0;
        public static float interaction_zoom = 0;
    }

   

    public void HandleUserInteraction(string[] commands)
    {
        if (commands[1] == "Rotate" && commands.Length == 4)
        {
            InteractionInfo.interaction_type = commands[1];

            InteractionInfo.interaction_dx = float.Parse(commands[2]);
            InteractionInfo.interaction_dy = float.Parse(commands[3]);
        }

        if (commands[1] == "Zoom" && commands.Length == 3)
        {
            InteractionInfo.interaction_type = commands[1];
            InteractionInfo.interaction_zoom = float.Parse(commands[2]);
        }
    }

    // public bool isUpdateLeafTextureSetting = false;
    // public bool isUpdateBranchRadiusFactor = false;
    // public bool isUpdateTwigFactor = false;
    // public bool isUpdateLeafFactor = false;

    public bool isUpdateLightSetting = false;
    public int isUpdateTexturePaths = 0b0000;
    public bool isTreeDensityChanged = false;
    public bool isUpdateCitySettings = false;

    public void UpdateSettings()
    {
        foreach (var line in m_lines)
        {
            TCP_Client.SendMessage("A param: " + line);
        }

        if (GlobalTerrainSettingQt.m_globalTreeProbablity != float.Parse(m_lines[1]))  // 树木的密度
        {
            GlobalTerrainSettingQt.m_globalTreeProbablity = float.Parse(m_lines[1]);
            this.isTreeDensityChanged = true;
        }

        if (GlobalTerrainSettingQt.m_tree_species != m_lines[2])  // 树木的密度
        {
            GlobalTerrainSettingQt.m_tree_species = m_lines[2];
            this.isTreeDensityChanged = true;
        }

        if (GlobalTerrainSettingQt.m_lightIntensity != float.Parse(m_lines[3]))  // 变换光照的强度
        {
            GlobalTerrainSettingQt.m_lightIntensity = float.Parse(m_lines[3]);

            this.isUpdateLightSetting = true;
        }

        if (GlobalTerrainSettingQt.m_lightTemperature != float.Parse(m_lines[4]))  // 变换光照的温度
        {
            GlobalTerrainSettingQt.m_lightTemperature = float.Parse(m_lines[4]);

            this.isUpdateLightSetting = true;
        }

        if (GlobalTerrainSettingQt.m_surfaceTexture_1 != m_lines[6])  // 变换纹理1
        {
            GlobalTerrainSettingQt.m_surfaceTexture_1 = m_lines[6];
            this.isUpdateTexturePaths = this.isUpdateTexturePaths | 0b1000;
        }
        if (GlobalTerrainSettingQt.m_surfaceTexture_2 != m_lines[7])  // 变换纹理2
        {
            GlobalTerrainSettingQt.m_surfaceTexture_2 = m_lines[7];
            this.isUpdateTexturePaths = this.isUpdateTexturePaths | 0b0100;
        }
        if (GlobalTerrainSettingQt.m_mountainTexture_1 != m_lines[8])  // 变换纹理3
        {
            GlobalTerrainSettingQt.m_mountainTexture_1 = m_lines[8];
            this.isUpdateTexturePaths = this.isUpdateTexturePaths | 0b0010;
        }
        if (GlobalTerrainSettingQt.m_mountainTexture_2 != m_lines[9])  // 变换纹理4
        {
            GlobalTerrainSettingQt.m_mountainTexture_2 = m_lines[9];
            this.isUpdateTexturePaths = this.isUpdateTexturePaths | 0b0001;
        }

        if (GlobalTerrainSettingQt.m_city_type != m_lines[10])  // 城镇类型
        {
            GlobalTerrainSettingQt.m_city_type = m_lines[10];
        }

        if (GlobalTerrainSettingQt.m_bldgDensityFactor != float.Parse(m_lines[11]))  // 城镇密度系数
        {
            GlobalTerrainSettingQt.m_bldgDensityFactor = float.Parse(m_lines[11]);
            this.isUpdateCitySettings = true;
        }

        if (GlobalTerrainSettingQt.m_bldgHeightFactor != float.Parse(m_lines[12]))  // 城镇高度系数
        {
            GlobalTerrainSettingQt.m_bldgHeightFactor = float.Parse(m_lines[12]);
            this.isUpdateCitySettings = true;
        }
        if (GlobalTerrainSettingQt.m_bldgRoadCurveFactor != float.Parse(m_lines[13]))  // 城镇高度系数
        {
            GlobalTerrainSettingQt.m_bldgRoadCurveFactor = float.Parse(m_lines[13]);
            this.isUpdateCitySettings = true;
        }

        //if ("back-texture" != m_lines[1])  // 变换树干纹理 1
        //{

        //}


        //if (GlobalParams.p_radius_factor != float.Parse(m_lines[2]))   // 变换枝干半径缩放系数 2
        //{
        //    GlobalParams.p_radius_factor = float.Parse(m_lines[2]);

        //    this.isUpdateBranchRadiusFactor = true;
        //}

        //if (GlobalSketchSetting.p_twig_density != float.Parse(m_lines[3]))   // 变换Twig密度 3
        //{
        //    GlobalSketchSetting.p_twig_density = float.Parse(m_lines[3]);

        //    this.isUpdateTwigFactor = true;
        //}
    }
    // public void UpdateSettings()
    // {
    //     foreach(var line in m_lines)
    //     {
    //         TCP_Client.SendMessage("A param: " + line);
    //     }
    //     if(GlobalSketchSetting.m_default_leaf_textures != m_lines[0])  // 变换树叶纹理 0
    //     {
    //         GlobalSketchSetting.m_default_leaf_textures = m_lines[0];
    //
    //         this.isUpdateLeafTextureSetting = true;
    //     }
    //
    //     if ("back-texture" != m_lines[1])  // 变换树干纹理 1
    //     {
    //         
    //     }
    //
    //
    //     if (GlobalParams.p_radius_factor != float.Parse(m_lines[2]))   // 变换枝干半径缩放系数 2
    //     {
    //         GlobalParams.p_radius_factor = float.Parse(m_lines[2]);
    //
    //         this.isUpdateBranchRadiusFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_twig_density != float.Parse(m_lines[3]))   // 变换Twig密度 3
    //     {
    //         GlobalSketchSetting.p_twig_density = float.Parse(m_lines[3]);
    //
    //         this.isUpdateTwigFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_twig_base_len != float.Parse(m_lines[4]))   // 变换twig的基础长度 4
    //     {
    //         GlobalSketchSetting.p_twig_base_len = float.Parse(m_lines[4]);
    //
    //         this.isUpdateTwigFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_twig_gravity_factor != float.Parse(m_lines[5])
    //         || GlobalSketchSetting.p_twig_base_dir_factor != float.Parse(m_lines[6]))   // 变换twig的重力和基础方向系数 5 6
    //     {
    //         GlobalSketchSetting.p_twig_gravity_factor = float.Parse(m_lines[5]);
    //         GlobalSketchSetting.p_twig_base_dir_factor = float.Parse(m_lines[6]);
    //         this.isUpdateTwigFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_leaf_density != float.Parse(m_lines[7]))   // 变换叶片的密度 7
    //     {
    //         GlobalSketchSetting.p_leaf_density = float.Parse(m_lines[7]);
    //
    //         this.isUpdateLeafFactor = true;
    //     }
    //     if (GlobalSketchSetting.p_leaf_size != float.Parse(m_lines[8]))   // 变换叶片的尺寸 8
    //     {
    //         GlobalSketchSetting.p_leaf_size = float.Parse(m_lines[8]);
    //
    //         this.isUpdateLeafFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_leaf_is_crossed != bool.Parse(m_lines[9]))   // 变换叶片是否交叉 9
    //     {
    //         GlobalSketchSetting.p_leaf_is_crossed = bool.Parse(m_lines[9]);
    //
    //         this.isUpdateLeafFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_leaf_gravity_factor != float.Parse(m_lines[10]))   // 叶片的重力方向权重 10
    //     {
    //         GlobalSketchSetting.p_leaf_gravity_factor = float.Parse(m_lines[10]);
    //
    //         this.isUpdateLeafFactor = true;
    //     }
    //
    //     if (GlobalSketchSetting.p_leaf_base_dir_factor != float.Parse(m_lines[11]))   // 叶片的基础方向权重 11
    //     {
    //         GlobalSketchSetting.p_leaf_base_dir_factor = float.Parse(m_lines[11]);
    //
    //         this.isUpdateLeafFactor = true;
    //     }
    // }


    public void Abort()
    {
        thStartServer.Abort();
    }


}

