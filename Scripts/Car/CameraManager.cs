using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject focus;
    
    /*��������ֵӦ�û��ڳ����ٶȽ���һ������,
     * ����Ҫ��������ļ��ٸ�(����������ʵ����Ӻ�λ��),
     * ����ҲҪ��������ļ��ٸ�(����������ʵ��복��ǰλ��)
     * 
     * ͬʱ���к���post-process,���ٵ�ʱ���������Χ����ɫ��ͼ����Ⱦ����һ��,�ﵽ�ٶȸе�Ŀ��
     * ����̽���Ƿ���Ҫ����˶�ģ��
     * 
     * 
     */
    public float distance = 5.5f;//��z������������복�ӵľ���
    public float height = 1.5f;//��y������������복�ӵľ���
    public float dampening = 5f;//����,����lerp֮��,����time.deltatime,��������ӽ����ӵ��ٶ�

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
