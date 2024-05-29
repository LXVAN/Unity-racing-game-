using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceGizemos : MonoBehaviour
{
    public GameObject Object;
    
    public Vector3 sourceCenter;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(sourceCenter, GetComponent<Solver>().initScale);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

}
