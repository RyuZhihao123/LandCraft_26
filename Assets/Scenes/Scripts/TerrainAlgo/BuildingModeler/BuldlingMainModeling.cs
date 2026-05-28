using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuldlingMainModeling : MonoBehaviour
{
    GameObject bldgObj = null;
    //BuildingModeler m_buildingModeler;
    void Start()
    {
        //m_buildingModeler = new BuildingModeler();
        // bldgObj = m_buildingModeler.GetOneBldgInstance(Vector3.zero);

        //GameObject defaultBldg = (GameObject)Resources.Load("Models/Buildings/defaultBldg");

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if (bldgObj != null)
                Destroy(bldgObj);

            //bldgObj = m_buildingModeler.GetOneBldgInstance(Vector3.zero);
        }
    }
}
