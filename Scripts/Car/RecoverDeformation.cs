using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverDeformation : MonoBehaviour
{
    private MeshFilter MeshFilter;
    private Vector3[] originVertices;
    private float length;
    //将这个脚本 加载在每个车身mesh 上
    // Start is called before the first frame update
    void Start()
    {

        MeshFilter = GetComponent<MeshFilter>();
        originVertices = MeshFilter.mesh.vertices;
        length = originVertices.Length;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (Input.GetKeyDown(KeyCode.R ))
        {
            
            //for (int i = 0; i < length; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        MeshFilter.mesh.vertices[i][j] = Mathf.Lerp(MeshFilter.mesh.vertices[i][j], originVertices[i][j], Time.deltaTime);
            //    }
                
            //}
            MeshFilter.mesh.vertices = originVertices;
        }
    }
}
