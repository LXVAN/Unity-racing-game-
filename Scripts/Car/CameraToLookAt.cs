using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToLookAt : MonoBehaviour
{
    //按下调整视角的几个键后,调整空物体的位置

    // Update is called once per frame
    public GameObject Car;
    public float dampening;
    private Quaternion rot ;
    private void Start()
    {
        rot = transform.rotation;
        
    }
    


    void Update()
    {
        Vector3 direction = Car.transform.forward;
        Vector3 up = Car.transform.up;

        //transform.position = new Vector3(direction.x,direction.y+0.4f,direction.z)  + Car.transform.position;
        transform.position = new Vector3(Car.transform.position.x,
                                         Mathf.Lerp( Car.transform.position.y,transform.position.y,Time.deltaTime*dampening),
                                          Car.transform.position.z);//
            
        if (Input.GetKeyDown(KeyCode.K))
        {
            transform.RotateAround(transform.position, up, 90);
            
        }

        
        
        
        
    }
        
}
