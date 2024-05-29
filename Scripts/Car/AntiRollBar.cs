using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public WheelCollider frontLeftWheel;//��ǰ
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
        //ʹ��ʱ�ǵ��Ƚ�4�����ӵ�wheelcollider �������ڵ�һ��(ǰ����Բ�һ��,����һ�ߵ����ұ���һ��),��Ȼ���벻ͨ��
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
            //    carRigidBody.AddForceAtPosition(frontLeftWheel.transform.up * -antiRollForce//ע���Ƿ��и���
            //                                    , frontLeftWheel.transform.position);
            //}

            //if (groundedFrontRight)
            //{
            //    carRigidBody.AddForceAtPosition(frontRightWheel.transform.up * antiRollForce//ע���Ƿ�  û��  ����
            //                                    , frontRightWheel.transform.position);
            //}
        }


    }
    public void AntiRollProcess(WheelCollider wheelLeft,WheelCollider wheelRight, float antiRollMultiplier)//��һ������һ��������
    {
        
        var (travelLeft, groundedLeft) = PreProcess(wheelLeft);
        var (travelRight, groundedRight) = PreProcess(wheelRight);
        float antiRollForce = AntiRollForce(travelLeft, travelRight, antiRollMultiplier);
        PostProcessLeft(wheelLeft, groundedLeft, antiRollForce);
        PostProcessRight(wheelRight, groundedRight, antiRollForce);
        
    }

    public (float,bool) PreProcess(WheelCollider wheel)//ǰ����,���� travel ֵ(���ڼ��� antirollforce)���Ƿ�ѹ��һ������
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

    public float AntiRollForce(float travel0, float travel1,float antiRollMultiplier)//ǰ������һ��ļ���,ÿ�μ�������ͬһ���PreProcess���
    {
        float antiRollForce = (travel0 - travel1) * antiRollMultiplier;
        return antiRollForce;
    }


    public void PostProcessLeft(WheelCollider wheelLeft,bool grounded,float antiRollForce)//����,������ʩ����
    {
        if (grounded)
        {
            carRigidBody.AddForceAtPosition(wheelLeft.transform.up * -antiRollForce
                                            , wheelLeft.transform.position);
        }
    }
    public void PostProcessRight(WheelCollider wheelRight, bool grounded, float antiRollForce)//����,������ʩ����
    {
        if (grounded)
        {
            carRigidBody.AddForceAtPosition(wheelRight.transform.up * antiRollForce//��"������(PostProcessLeft)"�������� "antiRollForce"ǰ��ϵ��������
                                            , wheelRight.transform.position);
        }
    }
}

//Դ����
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
//    carRigidBody.AddForceAtPosition(frontLeftWheel.transform.up * -antiRollForce//ע���Ƿ��и���
//                                    , frontLeftWheel.transform.position);
//}

//if (groundedFrontRight)
//{
//    carRigidBody.AddForceAtPosition(frontRightWheel.transform.up * antiRollForce//ע���Ƿ�  û��  ����
//                                    , frontRightWheel.transform.position);
//}