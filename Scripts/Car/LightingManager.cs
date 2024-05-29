using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;
using Unity.VisualScripting;


public class LightingManager : MonoBehaviour
{
    public List<Light> lights;
    public List<GameObject> brakeLights; 
    private bool a = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public virtual void ToggleHeadLights(bool switch_)
    {
        
        if (switch_)
        {
            a = !a;//��һ��L ��,���ֵ��"��"һ��
        }
        if (a)
        {
            LightIntensity(1);
        }
        else 
        {
            LightIntensity(0);
        }
    }
    public void LightIntensity(float intensity)//����ǰ�յƵƹ�����
    {
        foreach (Light light in lights)
        {
            light.intensity = intensity;
        }
    }

    public virtual void BrakeLight(bool toggle)//ɲ��������
    {
        //�ǵý�emission color �����global illumination ����Ϊ realtime,��Ȼ�ƹⲻ��仯
        foreach (GameObject brakelight in brakeLights)
        {
            if (toggle)
            {
                brakelight.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.3f,0.1f,0.1f));
                
            }
            else
            {
                brakelight.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
            }
        }
    }



}
