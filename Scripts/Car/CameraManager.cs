using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject focus;
    
    /*下面三个值应该基于车子速度进行一定调整,
     * 加速要有摄像机的加速感(摄像机可以适当向车子后位移),
     * 减速也要有摄像机的减速感(摄像机可以适当想车子前位移)
     * 
     * 同时还有后处理post-process,加速的时候可以让周围的颜色和图形渲染畸变一下,达到速度感的目的
     * 可以探究是否需要添加运动模糊
     * 
     * 
     */
    public float distance = 5.5f;//在z轴上摄像机距离车子的距离
    public float height = 1.5f;//在y轴上摄像机距离车子的距离
    public float dampening = 5f;//乘数,做了lerp之后,基于time.deltatime,决定相机接近车子的速度

    private Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pos = transform.position;
        //transform.position = Vector3.Lerp(transform.position, focus.transform.position + focus.transform.TransformDirection(new Vector3(0f, height, -distance)), Time.deltaTime * dampening);
        if (Input.GetKey(KeyCode.K))
        {
            //transform.position = Vector3.Lerp(transform.position, focus.transform.position + focus.transform.TransformDirection(new Vector3(0f, height, 0.25f * distance)), Time.deltaTime * dampening);
            //Debug.Log("K pressed!");
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, focus.transform.position + focus.transform.TransformDirection(new Vector3(0f, height, -distance)), Time.deltaTime * dampening);

        }


        transform.LookAt(focus.transform.position);
        transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y,pos.y,Time.deltaTime * dampening), transform.position.z);
    }

}
