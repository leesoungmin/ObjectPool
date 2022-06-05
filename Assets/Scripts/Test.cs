using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Cube box = ObjectPool.SpawnFromPool<Cube>("Cube", Vector3.zero);
            box.SetUp(Random.ColorHSV(0,1,0.5f,1,1,1));
        }
        else if(Input.GetKeyDown(KeyCode.J))
        {
            GameObject ball = ObjectPool.SpawnFromPool("Sphere", Vector3.zero);
        }
        
    }
}
