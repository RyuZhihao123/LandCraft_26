using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;



public class OSMLoader : MonoBehaviour
{
    public class OSMNode
    {
        public string id = "";
        public float x = 0;
        public float y = 0;

        public OSMNode() { }
        public OSMNode(string id, float lat, float lon) { this.id = id; this.x = lat; this.y = lon;}
    }

    public class OSMWay
    {
        public string type = "";
        public List<OSMNode> nodes = new List<OSMNode>();
    }

    Dictionary<string, OSMNode> osmNodes = new Dictionary<string, OSMNode>();

    List<OSMWay> osmWays = new List<OSMWay>();

    public void LoadOSMFile(string filename)
    {
        XmlDocument doc = new XmlDocument();
        XmlTextReader reader = new XmlTextReader(filename);
        doc.Load(reader);

        XmlNodeList xmlNodeList = doc.GetElementsByTagName("node");    //根据元素名称获取元素list
        
 
        foreach (XmlNode xmlNode in xmlNodeList)
        {
            string id = xmlNode.Attributes["id"].Value;
            float x = float.Parse(xmlNode.Attributes["lat"].Value);
            float y = float.Parse(xmlNode.Attributes["lon"].Value);

            print(string.Format("{0} {1} {2}", id, x, y));

            OSMNode osmNode = new OSMNode(id, x, y);
            osmNodes[id] = osmNode;
        }


        XmlNodeList xmlWayList = doc.GetElementsByTagName("way");    //根据元素名称获取元素list

        int count = 0;
        GameObject objParent = new GameObject();

        foreach (XmlNode xmlWay in xmlWayList)  //
        {
            OSMWay osmWay = new OSMWay();
            XmlNodeList children = xmlWay.ChildNodes;


            if (count > 50)
                break;
            count++;
            foreach (XmlNode xmlWayNode in children)
            {
                if (xmlWayNode.Attributes[0].Name == "ref")//根据元素属性筛选出node元素
                {
                    string nodeID = xmlWayNode.Attributes["ref"].InnerText;

                    OSMNode node = osmNodes[nodeID];

                    osmWay.nodes.Add(node);
                }

                    //print(xmlWayNode.Attributes[0].Name);
            }
            print("Way Children: " + children.Count.ToString()+" "+osmWay.nodes.Count.ToString());
            GameObject obj = new GameObject();
            obj.AddComponent<LineRenderer>();
            LineRenderer line = obj.GetComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.5f;
            line.positionCount = osmWay.nodes.Count;
      
            for(int i=0; i<osmWay.nodes.Count; ++i)
            {
                line.SetPosition(i, new Vector3(osmWay.nodes[i].x, 0, osmWay.nodes[i].y));
            }
            obj.transform.parent = objParent.transform;


        }
    }



    // Start is called before the first frame update
    void Start()
    {
        this.LoadOSMFile("C:/Users/liuzh/Downloads/map.osm");
        ////读入xml文件
        //XmlTextReader reader = new XmlTextReader("C:/Users/liuzh/Downloads/map.osm");
        //doc.Load(reader);

        //XmlNodeList elemList = doc.GetElementsByTagName("node");          //根据元素名称获取元素list
        ////获取元素的子元素们
        //foreach (XmlNode node in elemList)
        //{
        //    XmlNodeList children = node.ChildNodes;
        //    print(node.Attributes[0].Name +" " +node.Attributes[0].Value);
        //    print(children.Count);
        //}


        ////获取元素的某一属性名称
        //string attrName = elemList[0].Attributes[0].Name;
        ////获取元素的某一属性值
        //string idStr = elemList[0].Attributes["id"].InnerText;
        ////把string转为int
        //int id = int.Parse(idStr);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
