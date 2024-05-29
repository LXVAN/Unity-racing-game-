using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SliderTuning : MonoBehaviour
{
    public GameObject self;//�����Լ�
    public GameObject Car;
    public WheelColliderConfigure wheelColliderConfigue;//wheelcolliderconfigure
    private bool f3Input = false;
    public GameObject backGround;

    public GameObject brakeBiasSlider;//slider
    public float brakeBias;//

    public GameObject brakeForceSlider;
    public float brakeForce;

    public GameObject[] Alignment;
    public float[] alignment = { 0f,0f,0f,0f};//�ǵ���unity �Ľ�������4��element

    public GameObject[] Springs;//ǰ����߶�(suspension distance), spring,anti-rollbar
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


        
        //����������ȡslider �е���ֵ
        //ÿ��Ҫ������ֵ�ĸ�ʽ����     ֵ = ֵ + slider ֵ,�����и���ʼֵ,����Ҫ��f3�����¼�����
        //��Ҫ��λ��ʱ��,��slider ֵ����Ϊ0 ����
        //����Ҫע�� sliderֵ ��������,��Ҫ��������ֵ
        //��getslidervalue ������Ĭ��ֵ,��textmesh pro �е���ʾ��ֵ������ʾ���ķ�Χ
        brakeBias = brakeBiasSlider.GetComponent<GetSliderValue>().sliderValue;
        SliderActive(f3Input, brakeBiasSlider);
        
        brakeForce = brakeForceSlider.GetComponent<GetSliderValue>().sliderValue;
        SliderActive(f3Input, brakeForceSlider);

        WheelAlignment();//ǰ�������Ǻ����

        SliderArrayValue(Springs,spring);

        SliderArrayValue(Esp, esp);
        
        SliderArrayValue(Lsd, lsd);

        SliderArrayValue(Damper, damper);
    }
    public void SliderArrayValue(GameObject[] objectArray, float[] valueArray)
        //�Զ���ȡ tuning �е� ������Ͳ�����,�����Զ�������رն�Ӧ�Ļ���
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



    public virtual float InitializeValue(GameObject slider)//��ȡ���ϸ��µ�sliderֵ
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
            //���ÿɽ������������ý������������ѭ��
            
            //���ÿɽ������������ý������������ѭ��

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
