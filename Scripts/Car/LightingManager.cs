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
            a = !a;//按一下L 键,这个值就"反"一次
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
    public void LightIntensity(float intensity)//调整前照灯灯光亮度
    {
        foreach (Light light in lights)
        {
            light.intensity = intensity;
        }
    }

    public virtual void BrakeLight(bool toggle)//刹车灯设置
    {
        //记得将emission color 下面的global illumination 调整为 realtime,不然灯光不会变化
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
