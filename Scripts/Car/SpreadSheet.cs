using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpreadSheet : MonoBehaviour
{
    public GameObject GameObject;//自己
    public GameObject Car;//车
    public List<WheelCollider> Wheels;
    private bool a = false;
    private string b;
    private void Start()
    {
        b = GameObject.GetComponent<TextMesh>().text;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F2))
        {
            a = !a;
        }
        if (a)
        {
            MySpreadSheet();//车辆当前速度,非轮速}
        }
        else
        {
            GameObject.GetComponent<TextMesh>().text = b;
        }
        

    }
    public void MySpreadSheet()
    {


        if (Random.Range(0, 2) == 1)//约 每两帧刷新一次
        {



            string gear = GetGear();//获得档位
            Vector3 velocity = Car.GetComponent<Rigidbody>().velocity;//获得车辆速度
            int speed = (int)(Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z));
            GameObject.GetComponent<TextMesh>().text = speed.ToString("0.00") + "m/s" + "\n" +
                                                       (3.6 * speed).ToString("0.00") + "Km/h" + "\n"
                                                       +"Gear=" + gear+"\n";

            foreach (WheelCollider wheel in Wheels)//获得每个轮子的各种信息
            {
                float motorTorque = wheel.motorTorque;
                float steerAngle = wheel.steerAngle;
                float brakeTorque = wheel.brakeTorque;
                float rpm = wheel.rpm;//原数据的单位是  每分钟多少转
                float v = rpm * (wheel.radius * 2 * Mathf.PI) * 60 / 1000;
                object forwardFriction = wheel.forwardFriction;
                /*如果要读取forwardFriction
                    直接使用
                    float asySlip = wheel.forwardFriction.asymptoteSlip;                
                 
                 */

               
                float asySlip = wheel.forwardFriction.asymptoteSlip;
                object sidewaysFriction = wheel.sidewaysFriction;

                WheelHit hit = new WheelHit();
                bool grounded = wheel.GetGroundHit(out hit);





                GameObject.GetComponent<TextMesh>().text +=  
                                                            "wheelName="+wheel + "\n" + 
                                                            "motorTorque="+motorTorque.ToString("0.00") + "\n" +
                                                            "breakTorque="+brakeTorque.ToString("0.00") + "\n" +
                                                            "rpm="+rpm.ToString("0.00") + "每分钟" + "\n" +
                                                            "轮速计算速度=" + v.ToString("0.00") + "\n" +
                                                            (-wheel.transform.InverseTransformPoint(hit.point).y).ToString("0.00") + "\n"  
                                                            ;
            }



        }
    }
    public string GetGear()
    {
        string gear = (Car.GetComponent<CarController>().gear).ToString();//获得档位
        if (Car.GetComponent<CarController>().gear == 0)
        {
            gear = "R";
        }
        return gear;
    }
}
