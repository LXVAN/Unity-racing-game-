using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    //这个脚本挂在每个 轮子的mesh 上, 即 最里面一层
    public float distance;//输出轮胎质心到光线碰撞点的距离
    public bool collision;//输出光线是否碰撞
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //float val = tuning.GetComponent<SliderTuning>().spring[3];//前 悬挂长度
        Ray ray = new Ray(transform.position,-transform.up);
        RaycastHit hitInfo;
        collision = Physics.Raycast(ray, out hitInfo, 10f);
        if (collision)
        {
            
            distance = hitInfo.distance;
            //float radius = 0.32f;
            //if (Mathf.Abs(distance - radius) > 0.05f)
            //{
                
            //    transform.localPosition = new Vector3(0f, -val, 0f);
                
            //}


        }

    }
}
