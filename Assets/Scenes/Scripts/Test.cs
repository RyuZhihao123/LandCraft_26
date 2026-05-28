using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; ++i)
        {
            int pre_id = (i - 1) % 4;
            pre_id = pre_id < 0 ? 4 - pre_id : pre_id;

            Debug.Log(string.Format("{0} {1} {2} {3}", pre_id, i, (i + 1) % 4, (i + 2) % 4));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
