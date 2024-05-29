using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float throttle;
    public float brake;
    public float steer;

    public bool enterPressed;//大回车按下
    public bool lightSwitch;//前照灯灯光开关
    public bool f1Pressed;
    public bool handBrake;
    public bool wPressed;
    public bool sPressed;
    public bool upArrow;
    public bool downArrow;
    float left = 0;
    float right = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //getkeydown 为按下
        //getkey 为长按
        //Input.GetAxis("Vertical");



        //steer += -Input.GetAxis("Horizontal")*0.02f ;//加负号前是 左正右负,加负号车子才能正常转弯,前轮转弯动作才能正常动作
        //if(steer > 0 )
        //{
        //    steer -= 0.005f;
        //}
        //if(steer < 0 )
        //{
        //    steer += 0.005f;
        //}

        steer = -Input.GetAxis("Horizontal") ;




        //steer = Mathf.Clamp(steer, -1, 1);






        lightSwitch = Input.GetKeyDown(KeyCode.Q);
        enterPressed = Input.GetKeyDown(KeyCode.Return);
        f1Pressed = Input.GetKeyDown(KeyCode.F1);
        handBrake = Input.GetKey(KeyCode.Space);
        wPressed = Input.GetKey(KeyCode.W);
        sPressed = Input.GetKey(KeyCode.S); 
        throttle = LinearInput(wPressed,throttle,1);
        brake = LinearInput(sPressed,brake,1);
        upArrow = Input.GetKeyDown(KeyCode.UpArrow);
        downArrow = Input.GetKeyDown(KeyCode.DownArrow);
        
    }

    



    public float LinearInput(bool keyPressed,float itself,float multiplier)
    {
        /* 线性输入,默认每一帧加0.1,最大1,不按直接置零
         * 
         * keyPressed按下的键,multiplier乘上的系数(必须大于0,表示变化快慢)
         */
        if (keyPressed & itself > 0)
        {
            itself += 0.1f * multiplier;
            if (itself >= 1f)
            {
                itself = 1f;
            }
        }
        else if (keyPressed & itself == 0)
        {
            itself += 0.1f * multiplier;
        }
        else
        {
            itself -= 0.01f;
        }
        itself = Mathf.Clamp(itself, 0f, 1f);


        return itself;
    }


}
