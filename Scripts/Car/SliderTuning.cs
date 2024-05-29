using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderTuning : MonoBehaviour
{
    public GameObject self;//物体自己
    public GameObject Car;
    public WheelColliderConfigure wheelColliderConfigue;//wheelcolliderconfigure
    private bool f3Input = false;
    public GameObject backGround;

    public GameObject brakeBiasSlider;//slider
    public float brakeBias;//

    public GameObject brakeForceSlider;
    public float brakeForce;

    public GameObject[] Alignment;
    public float[] alignment = { 0f,0f,0f,0f};//记得在unity 的界面设置4个element

    public GameObject[] Springs;//前后车身高度(suspension distance), spring,anti-rollbar
    public float[] spring;

    public GameObject[] Esp;
    public float[] esp;

    public GameObject[] Lsd;
    public float[] lsd;

    public GameObject[] Damper;
    public float[] damper;

    void Start()
    {
        
    }
    private void Awake()
    {
        
    }

    void Update()
    {
        Text0();
        BackGround(f3Input);


        
        //可以正常读取slider 中的数值
        //每个要调整的值的格式建议     值 = 值 + slider 值,这样有个初始值,不需要打开f3进行事件激活
        //需要归位的时候,将slider 值设置为0 就行
        //但是要注意 slider值 的上下限,不要超过正常值
        //在getslidervalue 中设置默认值,让textmesh pro 中的显示的值正常显示他的范围
        brakeBias = brakeBiasSlider.GetComponent<GetSliderValue>().sliderValue;
        SliderActive(f3Input, brakeBiasSlider);
        
        brakeForce = brakeForceSlider.GetComponent<GetSliderValue>().sliderValue;
        SliderActive(f3Input, brakeForceSlider);

        WheelAlignment();//前后轮束角和倾角

        SliderArrayValue(Springs,spring);

        SliderArrayValue(Esp, esp);
        
        SliderArrayValue(Lsd, lsd);

        SliderArrayValue(Damper, damper);
    }
    public void SliderArrayValue(GameObject[] objectArray, float[] valueArray)
        //自动读取 tuning 中的 物体组和参数组,并且自动开启或关闭对应的滑块
    {
        for (int i = 0; i < objectArray.Length; i++)
        {
            valueArray[i] = objectArray[i].GetComponent<GetSliderValue>().sliderValue;
            SliderActive(f3Input, objectArray[i]);
        }
    }



    public void WheelAlignment()
    {
        for (int i = 0; i < Alignment.Length; i++)
        {
            //if (i == 0)//back toe
            //{
            //    alignment[0] = Alignment[0].GetComponent<GetSliderValue>().sliderValue;
            //    SliderActive(f3Input, Alignment[0]);
            //}
            //if (i == 1)//back Camber
            //{
            //    alignment[1] = Alignment[1].GetComponent<GetSliderValue>().sliderValue;
            //    SliderActive(f3Input, Alignment[1]);
            //}
            //if (i == 2)//front toe
            //{ 
            //    alignment[2] = Alignment[2].GetComponent<GetSliderValue>().sliderValue;
            //    SliderActive(f3Input, Alignment[2]);
            //}
            //if (i == 3)//front Camber
            //{
            //    alignment[3] = Alignment[3].GetComponent<GetSliderValue>().sliderValue;
            //    SliderActive(f3Input, Alignment[3]);
            //}
            alignment[i] = Alignment[i].GetComponent<GetSliderValue>().sliderValue;
            SliderActive(f3Input, Alignment[i]);
        }
    }

    void BackGround(bool f3Input)
    {
        bool  a = f3Input;
        if (Input.GetKeyDown(KeyCode.F3))
        { 
            a= !a;
        }

        if (a)
        {
            backGround.GetComponent<Image>().color = new Color(0, 0, 0, 0.34f);
        }
        else
        {
            backGround.GetComponent<Image>().color = new Color(0,0,0,0);
        }
    }



    public virtual float InitializeValue(GameObject slider)//获取不断更新的slider值
    {
        return slider.GetComponent<Slider>().value;
    }
    public void Text0()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            f3Input = !f3Input;
        }
        if (f3Input)
        {
            self.GetComponent<TextMesh>().text = "Car Tuning";
            //设置可交互和设置启用禁用物体可以用循环
            
            //设置可交互和设置启用禁用物体可以用循环

        }
        else
        {
            self.GetComponent<TextMesh>().text = "Press F3 To Tune The Car!";
            
        }
    }

    public void SliderActive(bool a , GameObject slider)
    {
        if (a)
        {
            slider.GetComponent<Slider>().interactable = true;
            slider.SetActive(true);
        }
        else
        {
            slider.GetComponent<Slider>().interactable = false;
            slider.SetActive(false);
        }
    }
   

}
