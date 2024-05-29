//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.SceneManagement;
//using UnityEngine;

//public class test : MonoBehaviour
//{
//    WheelHit wheelHit;
//    WheelCollider wheelCollider;
//    Vector3 pointPos;



//    Vector3[] vertices = new Vector3[6];
//    public float length = 1f;
//    public float width = 1f;
    
//    MeshFilter meshFilter;
//    Mesh mesh;
//    void Start()
//    {


//        meshFilter = GetComponent<MeshFilter>();
//        mesh = new Mesh();
        
//        /*
//        mesh.vertices = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0) };
//        mesh.uv = new Vector2[3] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
//        mesh.triangles = new int[3] { 0, 2, 1 };
//        */
        
//    }
//    void Update()
//    {
//        vertices[0] = new Vector3(-width, -length, 0);
//        vertices[1] = new Vector3( width,  length, 0);
//        vertices[2] = new Vector3( width, -length, 0);
//        vertices[3] = new Vector3(-width, -length, 0);
//        vertices[4] = new Vector3(-width,  length, 0);
//        vertices[5] = new Vector3( width,  length, 0);

//        mesh.vertices = vertices;

//        mesh.uv = new Vector2[6];
//        /*
//        {
//            new Vector2(-1, -1),
//            new Vector2(1, 1),
//            new Vector2(1, -1),
//            new Vector2(-1, -1),
//            new Vector2(-1, 1),
//            new Vector2(1, 1)
//        };
//        */
        
//        for (int i = 0; i < 6; i++)
//        {
//            mesh.uv[i] = new Vector2(vertices[i].x, vertices[i].y);
//        }
        

//        mesh.triangles = new int[] { 3, 4, 1, 5, 2, 0 };
//        meshFilter.mesh = mesh;




//    }
//}
