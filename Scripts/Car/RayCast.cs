using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    //����ű�����ÿ�� ���ӵ�mesh ��, �� ������һ��
    public float distance;//�����̥���ĵ�������ײ��ľ���
    public bool collision;//��������Ƿ���ײ
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //float val = tuning.GetComponent<SliderTuning>().spring[3];//ǰ ���ҳ���
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
