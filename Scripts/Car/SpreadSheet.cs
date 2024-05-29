using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpreadSheet : MonoBehaviour
{
    public GameObject GameObject;//�Լ�
    public GameObject Car;//��
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
            MySpreadSheet();//������ǰ�ٶ�,������}
        }
        else
        {
            GameObject.GetComponent<TextMesh>().text = b;
        }
        

    }
    public void MySpreadSheet()
    {


        if (Random.Range(0, 2) == 1)//Լ ÿ��֡ˢ��һ��
        {



            string gear = GetGear();//��õ�λ
            Vector3 velocity = Car.GetComponent<Rigidbody>().velocity;//��ó����ٶ�
            int speed = (int)(Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z));
            GameObject.GetComponent<TextMesh>().text = speed.ToString("0.00") + "m/s" + "\n" +
                                                       (3.6 * speed).ToString("0.00") + "Km/h" + "\n"
                                                       +"Gear=" + gear+"\n";

            foreach (WheelCollider wheel in Wheels)//���ÿ�����ӵĸ�����Ϣ
            {
                float motorTorque = wheel.motorTorque;
                float steerAngle = wheel.steerAngle;
                float brakeTorque = wheel.brakeTorque;
                float rpm = wheel.rpm;//ԭ���ݵĵ�λ��  ÿ���Ӷ���ת
                float v = rpm * (wheel.radius * 2 * Mathf.PI) * 60 / 1000;
                object forwardFriction = wheel.forwardFriction;
                /*���Ҫ��ȡforwardFriction
                    ֱ��ʹ��
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
                                                            "rpm="+rpm.ToString("0.00") + "ÿ����" + "\n" +
                                                            "���ټ����ٶ�=" + v.ToString("0.00") + "\n" +
                                                            (-wheel.transform.InverseTransformPoint(hit.point).y).ToString("0.00") + "\n"  
                                                            ;
            }



        }
    }
    public string GetGear()
    {
        string gear = (Car.GetComponent<CarController>().gear).ToString();//��õ�λ
        if (Car.GetComponent<CarController>().gear == 0)
        {
            gear = "R";
        }
        return gear;
    }
}
