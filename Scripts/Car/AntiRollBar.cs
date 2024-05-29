using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public WheelCollider frontLeftWheel;//左前
    public WheelCollider frontRightWheel;
    public WheelCollider backLeftWheel;
    public WheelCollider backRightWheel;

    public GameObject Tuning;

    private Rigidbody carRigidBody;
    public float antiRollFront = 15000f;
    public float antiRollBack = 15000f;
    

    // Start is called before the first frame update
    void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //使用时记得先将4个轮子的wheelcollider 参数调节到一样(前后可以不一样,但是一边的左右必须一样),不然编译不通过
        antiRollBack = 15000f + Tuning.GetComponent<SliderTuning>().spring[2];
        antiRollFront = 15000f + Tuning.GetComponent<SliderTuning>().spring[5];




        AntiRollProcess(frontLeftWheel, frontRightWheel, antiRollFront);
        AntiRollProcess(backLeftWheel, backRightWheel, antiRollBack);
        {
            //WheelHit hit = new WheelHit();

            //float travelFrontLeft = 1.0f;
            //float travelFrontRight = 1.0f;

            //bool groundedFrontLeft = frontLeftWheel.GetGroundHit(out hit);
            //if (groundedFrontLeft)
            //{
            //    travelFrontLeft = (-frontLeftWheel.transform.InverseTransformPoint(hit.point).y
            //                       - frontLeftWheel.radius) / frontLeftWheel.suspensionDistance;
            //}
            //else
            //{
            //    travelFrontLeft = 1f;
            //}


            //bool groundedFrontRight = frontRightWheel.GetGroundHit(out hit);
            //if (groundedFrontRight)
            //{
            //    travelFrontRight = (-frontRightWheel.transform.InverseTransformPoint(hit.point).y
            //                        - frontRightWheel.radius) / frontRightWheel.suspensionDistance;
            //}
            //else
            //{
            //    travelFrontRight = 1f;
            //}


            //var antiRollForce = (travelFrontLeft - travelFrontRight) * antiRollFront;
            //----------------------------------
            //if (groundedFrontLeft)
            //{
            //    carRigidBody.AddForceAtPosition(frontLeftWheel.transform.up * -antiRollForce//注意是否有负号
            //                                    , frontLeftWheel.transform.position);
            //}

            //if (groundedFrontRight)
            //{
            //    carRigidBody.AddForceAtPosition(frontRightWheel.transform.up * antiRollForce//注意是否  没有  负号
            //                                    , frontRightWheel.transform.position);
            //}
        }


    }
    public void AntiRollProcess(WheelCollider wheelLeft,WheelCollider wheelRight, float antiRollMultiplier)//第一个输入一定是左轮
    {
        
        var (travelLeft, groundedLeft) = PreProcess(wheelLeft);
        var (travelRight, groundedRight) = PreProcess(wheelRight);
        float antiRollForce = AntiRollForce(travelLeft, travelRight, antiRollMultiplier);
        PostProcessLeft(wheelLeft, groundedLeft, antiRollForce);
        PostProcessRight(wheelRight, groundedRight, antiRollForce);
        
    }

    public (float,bool) PreProcess(WheelCollider wheel)//前处理,返回 travel 值(用于计算 antirollforce)和是否压上一个物体
    {
        float travel = 1f;
        WheelHit hit = new WheelHit();
        bool grounded = wheel.GetGroundHit(out hit);
        if (grounded)
        {
            travel = (-wheel.transform.InverseTransformPoint(hit.point).y
                       - wheel.radius) / wheel.suspensionDistance;
            //Debug.Log(wheel.name + -wheel.transform.InverseTransformPoint(hit.point).y);
        }
        else
        {
            travel = 1;
        }
        return (travel,grounded);
    }

    public float AntiRollForce(float travel0, float travel1,float antiRollMultiplier)//前后任意一侧的计算,每次计算输入同一侧的PreProcess结果
    {
        float antiRollForce = (travel0 - travel1) * antiRollMultiplier;
        return antiRollForce;
    }


    public void PostProcessLeft(WheelCollider wheelLeft,bool grounded,float antiRollForce)//后处理,对轮子施加力
    {
        if (grounded)
        {
            carRigidBody.AddForceAtPosition(wheelLeft.transform.up * -antiRollForce
                                            , wheelLeft.transform.position);
        }
    }
    public void PostProcessRight(WheelCollider wheelRight, bool grounded, float antiRollForce)//后处理,对轮子施加力
    {
        if (grounded)
        {
            carRigidBody.AddForceAtPosition(wheelRight.transform.up * antiRollForce//和"后处理左(PostProcessLeft)"的区别是 "antiRollForce"前的系数是正的
                                            , wheelRight.transform.position);
        }
    }
}

//源代码
//WheelHit hit = new WheelHit();

//float travelFrontLeft = 1.0f;
//float travelFrontRight = 1.0f;

//bool groundedFrontLeft = frontLeftWheel.GetGroundHit(out hit);
//if (groundedFrontLeft)
//{
//    travelFrontLeft = (-frontLeftWheel.transform.InverseTransformPoint(hit.point).y
//                       - frontLeftWheel.radius) / frontLeftWheel.suspensionDistance;
//}
//else
//{
//    travelFrontLeft = 1f;
//}


//bool groundedFrontRight = frontRightWheel.GetGroundHit(out hit);
//if (groundedFrontRight)
//{
//    travelFrontRight = (-frontRightWheel.transform.InverseTransformPoint(hit.point).y
//                        - frontRightWheel.radius) / frontRightWheel.suspensionDistance;
//}
//else
//{
//    travelFrontRight = 1f;
//}


//var antiRollForce = (travelFrontLeft - travelFrontRight) * antiRollFront;
////----------------------------------
//if (groundedFrontLeft)
//{
//    carRigidBody.AddForceAtPosition(frontLeftWheel.transform.up * -antiRollForce//注意是否有负号
//                                    , frontLeftWheel.transform.position);
//}

//if (groundedFrontRight)
//{
//    carRigidBody.AddForceAtPosition(frontRightWheel.transform.up * antiRollForce//注意是否  没有  负号
//                                    , frontRightWheel.transform.position);
//}