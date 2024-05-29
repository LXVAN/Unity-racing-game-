using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VehiclePhysics;
using UnityEngine.UI;

public class WheelColliderConfigure : MonoBehaviour
{
    //统一调整前轮和后轮的wheel Collider 参数
    public List<WheelCollider> backWheels;
    public List<WheelCollider> frontWheels;
    public GameObject hitInfomation;
    public float front_mass;
    public float front_radius;
    public float front_wheelDampingRate;
    public float front_suspensionDistance;
    public float front_forceAppPointDistance ;
    public Vector3 front_center ;
    public float front_spring;
    public float front_damper;
    public float front_targetPosition;
    public float front_ffExtremumSlip;
    public float front_ffExtremumValue;
    public float front_ffAsymptoteSlip;
    public float front_ffAsymptoteValue;
    public float front_ffStiffness;
    public float front_sfExtremumSlip;
    public float front_sfExtremumValue;
    public float front_sfAsymptoteSlip;
    public float front_sfAsymptoteValue;
    public float front_sfStiffness;
    [Header("-----------------------------------")]
    public float back_mass;
    public float back_radius;
    public float back_wheelDampingRate;
    public float back_suspensionDistance;
    public float back_forceAppPointDistance;
    public Vector3 back_center;
    public float back_spring;
    public float back_damper;
    public float back_targetPosition;
    public float back_ffExtremumSlip;
    public float back_ffExtremumValue;
    public float back_ffAsymptoteSlip;
    public float back_ffAsymptoteValue;
    public float back_ffStiffness;
    public float back_sfExtremumSlip;
    public float back_sfExtremumValue;
    public float back_sfAsymptoteSlip;
    public float back_sfAsymptoteValue;
    public float back_sfStiffness;


    public GameObject tuning;


    private float[] colBackffStiffness = { 1.5f,1.5f};
    private float[] colBacksfStiffness = { 1.5f, 1.5f };
    private float[] colFrontffStiffness = { 1.5f, 1.5f };
    private float[] colFrontsfStiffness = { 1.5f, 1.5f };


    // Update is called once per frame
    void FixedUpdate()
    {

        front_spring = 4000f + tuning.GetComponent<SliderTuning>().spring[4];
        back_spring = 4000f + tuning.GetComponent<SliderTuning>().spring[1];

        front_suspensionDistance =0.1f + tuning.GetComponent<SliderTuning>().spring[3];
        back_suspensionDistance =0.1f +  tuning.GetComponent<SliderTuning>().spring[0];

        front_damper = 6000f +  tuning.GetComponent<SliderTuning>().damper[0];
        back_damper = 6000f + tuning.GetComponent<SliderTuning>().damper[1];

        SetDefaultCoefficiency();

        //AdjustStiffnessByColliders();
        
        FrictionCurveStiffness("GRASS",0.1f, 0.1f);

        /*
         *将后轮的sideway friction stiffness调到0.5 可以有效解决推头
         *将damper 调高一点可以没那么颠
         *damper 的值一定不能超过 spring的值，不然会出错
        */

        //front_ffStiffness = 1.0f+ 0.5f*GetComponent<CarController>().collisionPerformance[2];


        //foreach (WheelCollider wheel in frontWheels)//设置后轮的wheel Collider参数
        for (int i = 0;i<2;i++)
        {
            frontWheels[i].mass = front_mass;
            frontWheels[i].radius = front_radius;
            frontWheels[i].wheelDampingRate = front_wheelDampingRate;
            frontWheels[i].suspensionDistance = front_suspensionDistance;
            frontWheels[i].forceAppPointDistance = front_forceAppPointDistance;
            frontWheels[i].center = front_center;          
            //-------------------Suspension Spring-------------------
            // 这些参数属于struct,如果要调整的话,需要新建一个struct,
            // 在新建的struct里面的参数进行调节,最后assign 给原来struct中的参数
            JointSpring suspensionSpring = frontWheels[i].suspensionSpring;           
            suspensionSpring.spring = front_spring;
            suspensionSpring.damper = front_damper;
            suspensionSpring.targetPosition = front_targetPosition;
            frontWheels[i].suspensionSpring = suspensionSpring;
            //-------------------Forward Friction--------------------            
            WheelFrictionCurve ff_friction = frontWheels[i].forwardFriction;
            ff_friction.extremumSlip = front_ffExtremumSlip;
            ff_friction.extremumValue = front_ffExtremumValue;
            ff_friction.asymptoteSlip = front_ffAsymptoteSlip;
            ff_friction.asymptoteValue = front_ffAsymptoteValue;
            //ff_friction.stiffness = Mathf.Clamp (front_ffStiffness - GetComponent<CarController>().collisionPerformance[i+2],0.01f,2f);//这四个语句会覆盖草地修改stiffness的值
            frontWheels[i].forwardFriction = ff_friction;            
            //------------------Sideways Friction--------------------
            WheelFrictionCurve sf_friction = frontWheels[i].sidewaysFriction;
            sf_friction.extremumSlip = front_sfExtremumSlip;
            sf_friction.extremumValue = front_sfExtremumValue;
            sf_friction.asymptoteSlip = front_sfAsymptoteSlip;
            sf_friction.asymptoteValue = front_sfAsymptoteValue;
            //sf_friction.stiffness = Mathf.Clamp(front_sfStiffness - GetComponent<CarController>().collisionPerformance[i + 2], 0.01f, 2f);
            frontWheels[i].sidewaysFriction = sf_friction;
        }
        //foreach (WheelCollider wheel in backWheels)//设置前轮的wheel Collider参数
        for (int i = 0;i<2;i++)
        {
            backWheels[i].mass = back_mass;
            backWheels[i].radius = back_radius;
            backWheels[i].wheelDampingRate = back_wheelDampingRate;
            backWheels[i].suspensionDistance = back_suspensionDistance;
            backWheels[i].forceAppPointDistance = back_forceAppPointDistance;
            backWheels[i].center = back_center;           
            JointSpring suspensionSpring = backWheels[i].suspensionSpring;
            suspensionSpring.spring = back_spring;
            suspensionSpring.damper = back_damper;
            suspensionSpring.targetPosition = back_targetPosition;
            backWheels[i].suspensionSpring = suspensionSpring;
            //-------------------Forward Friction--------------------            
            WheelFrictionCurve ff_friction = backWheels[i].forwardFriction;
            ff_friction.extremumSlip = back_ffExtremumSlip;
            ff_friction.extremumValue = back_ffExtremumValue;
            ff_friction.asymptoteSlip = back_ffAsymptoteSlip;
            ff_friction.asymptoteValue = back_ffAsymptoteValue;
            //ff_friction.stiffness = Mathf.Clamp(back_ffStiffness - GetComponent<CarController>().collisionPerformance[i],0.01f,2f);
            backWheels[i].forwardFriction = ff_friction;
            //------------------Sideways Friction--------------------
            WheelFrictionCurve sf_friction = backWheels[i].sidewaysFriction;
            sf_friction.extremumSlip = back_sfExtremumSlip;
            sf_friction.extremumValue = back_sfExtremumValue;
            sf_friction.asymptoteSlip = back_sfAsymptoteSlip;
            sf_friction.asymptoteValue = back_sfAsymptoteValue;
           // sf_friction.stiffness = Mathf.Clamp(back_sfStiffness - GetComponent<CarController>().collisionPerformance[i], 0.01f, 2f);
            backWheels[i].sidewaysFriction = sf_friction;
        }
    }
    public void FrictionCurveStiffness(string colliderName,float colFFStiffness,float colSFStiffness)
        //为所有轮子调整轮胎摩擦力曲线中的stiffness 
        //第一个: 包含在colliderName[]数组中的特征值，第二个接触后要调整的stiffness值，第三个同第二个
    {
        string[] colliderNames = hitInfomation.GetComponent<GetGroundHitInformation>().colliderName;
        float[] colBackffStiffness = { 1.5f, 1.5f };
        float[] colBacksfStiffness = { 1.5f, 1.5f };
        float[] colFrontffStiffness = { 1.5f, 1.5f };
        float[] colFrontsfStiffness = { 1.5f, 1.5f };

        for(int i = 0;i<2;i++)
        {
            if (colliderNames[i].Contains(colliderName))//对后轮
            {
                colBackffStiffness[i] = colFFStiffness;
                colBacksfStiffness[i] = colSFStiffness;
            }
            else
            {
                colBackffStiffness[i] = 1.5f;
                colBacksfStiffness[i] = 1.5f;
            }
            //设置后轮的ff 摩擦曲线和sf摩擦曲线
            WheelFrictionCurve backffFriction = new WheelFrictionCurve();
            backffFriction.stiffness = colBackffStiffness[i];
            backWheels[i].forwardFriction = backffFriction;
            WheelFrictionCurve backsfFriction = new WheelFrictionCurve();
            backsfFriction.stiffness = colBacksfStiffness[i];
            backWheels[i].sidewaysFriction = backsfFriction;


            if (colliderNames[i + 2].Contains(colliderName))//对前轮
            {
                colFrontffStiffness[i] = colFFStiffness;
                colFrontsfStiffness[i] = colSFStiffness;
            }
            else
            {
                colBackffStiffness[i] = 1.5f;
                colBacksfStiffness[i] = 1.5f;
            }

            WheelFrictionCurve frontffFriction = new WheelFrictionCurve();
            frontffFriction.stiffness = colFrontffStiffness[i];
            frontWheels[i].forwardFriction = frontffFriction;
            WheelFrictionCurve frontsfFriction = new WheelFrictionCurve();
            frontsfFriction.stiffness = colFrontsfStiffness[i];
            frontWheels[i].sidewaysFriction = frontsfFriction;




        }












    }

    /*
    private void AdjustStiffnessByColliders()
    {
        string[] colliderNames = hitInfomation.GetComponent<GetGroundHitInformation>().colliderName;
        //获得GroundHit物体中返回的接触物体
        //0 br,1bl,2fr,3fl

        for (int i = 0; i < 2; i++)//后轮，i = 0为右边，i = 1为左边
        //根据轮子接触的物体，调整轮子的摩擦力参数
        {
            if (colliderNames[i].Contains("PITLANE") || colliderNames[i].Contains("TRACKMAIN"))
            {

            }
            if (colliderNames[i].Contains("GRASS"))
            {
                colBackffStiffness[i] = 0.2f;
                colBacksfStiffness[i] = 0.2f;
            }
            else
            {
                colBackffStiffness[i] = 1.5f;
                colBacksfStiffness[i] = 1.5f;
            }
        }
        for (int i = 0; i < 2; i++)//前轮
        {
            if (colliderNames[i + 2].Contains("GRASS"))
            {
                colFrontffStiffness[i] = 0.2f;
                colFrontsfStiffness[i] = 0.2f;
            }
            else
            {
                colFrontffStiffness[i] = 1.5f;
                colFrontsfStiffness[i] = 1.5f;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            WheelFrictionCurve ffFriction = new WheelFrictionCurve();
            ffFriction.stiffness = colBackffStiffness[i];
            backWheels[i].forwardFriction = ffFriction;
            WheelFrictionCurve sfFriction = new WheelFrictionCurve();
            sfFriction.stiffness = colBacksfStiffness[i];
            backWheels[i].sidewaysFriction = sfFriction;
        }
        for (int i = 0; i < 2; i++)
        {
            WheelFrictionCurve ffFriction = new WheelFrictionCurve();
            ffFriction.stiffness = colFrontffStiffness[i];
            frontWheels[i].forwardFriction = ffFriction;
            WheelFrictionCurve sfFriction = new WheelFrictionCurve();
            sfFriction.stiffness = colFrontsfStiffness[i];
            frontWheels[i].sidewaysFriction = sfFriction;
        }
    }
    */


    private void SetDefaultCoefficiency()
    {
        if (Input.GetKeyDown(KeyCode.R))//按下R 让所有参数返回默认值
        {

            //这一组中的stiffness =1.5时, 轮速差不多匹配上车速了,只有一点点偏差
            front_mass = 20f;
            //front_radius = 0.34f;
            front_wheelDampingRate = 0.25f;
            front_suspensionDistance = 0.1f;
            front_forceAppPointDistance = 0f;
            front_center = new Vector3(0f, 0f, 0f);
            front_spring = 4000f;
            front_damper = 6000f;
            front_targetPosition = 0.5f;
            front_ffExtremumSlip = 0.4f;
            front_ffExtremumValue = 1f;
            front_ffAsymptoteSlip = 0.8f;
            front_ffAsymptoteValue = 0.5f;
            front_ffStiffness = 1.5f;
            front_sfExtremumSlip = 0.4f;
            front_sfExtremumValue = 1f;
            front_sfAsymptoteSlip = 0.8f;
            front_sfAsymptoteValue = 0.5f;
            front_sfStiffness = 1.5f;
            //----------------
            back_mass = 20f;
            //back_radius = 0.34f;
            back_wheelDampingRate = 0.25f;
            back_suspensionDistance = 0.1f;
            back_forceAppPointDistance = 0f;
            back_center = new Vector3(0f, 0f, 0f);
            back_spring = 4000f;
            back_damper = 6000f;
            back_targetPosition = 0.5f;
            back_ffExtremumSlip = 0.4f;
            back_ffExtremumValue = 1f;
            back_ffAsymptoteSlip = 0.8f;
            back_ffAsymptoteValue = 0.5f;
            back_ffStiffness = 1.5f;
            back_sfExtremumSlip = 0.4f;
            back_sfExtremumValue = 1f;
            back_sfAsymptoteSlip = 0.8f;
            back_sfAsymptoteValue = 0.5f;
            back_sfStiffness = 1.5f;













            /*
            //最开始的参数,rpm 读取非常抽象
            front_mass = 1f;
            front_radius = 0.34f;
            front_wheelDampingRate = 0.25f;
            front_suspensionDistance = 0.1f;
            front_forceAppPointDistance = 0f;
            front_center = new Vector3 (0f, 0f, 0f);
            front_spring = 4000f;
            front_damper = 2000f;
            front_targetPosition = 0.5f;
            front_ffExtremumSlip = 1f;
            front_ffExtremumValue = 1f;
            front_ffAsymptoteSlip = 1f;
            front_ffAsymptoteValue = 1f;
            front_ffStiffness = 5f;
            front_sfExtremumSlip = 1f;
            front_sfExtremumValue = 1f;
            front_sfAsymptoteSlip = 1f;
            front_sfAsymptoteValue = 1f;
            front_sfStiffness = 5f;
            //----------------
            back_mass = 1f;
            back_radius = 0.34f;
            back_wheelDampingRate = 0.25f;
            back_suspensionDistance = 0.1f;
            back_forceAppPointDistance = 0f;
            back_center = new Vector3(0f, 0f, 0f);
            back_spring = 4000f;
            back_damper = 2000f;
            back_targetPosition = 0.5f;
            back_ffExtremumSlip = 1f;
            back_ffExtremumValue = 1f;
            back_ffAsymptoteSlip = 1f;
            back_ffAsymptoteValue = 1f;
            back_ffStiffness = 5f;
            back_sfExtremumSlip = 1f;
            back_sfExtremumValue = 1f;
            back_sfAsymptoteSlip = 1f;
            back_sfAsymptoteValue = 1f;
            back_sfStiffness = 5f;
            */
        }
    }


    public virtual float SliderValue(Slider slider)
    {
        float value = slider.value;

        return value;
    }
}
