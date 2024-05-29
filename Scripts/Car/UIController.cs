using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject Car;
    public GameObject[] UIObjects;
    private new Rigidbody rigidbody;

    private bool abs;
    private bool tc;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = Car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Abs_Tc();
        PointRotation(UIObjects[3], Car.GetComponent<CarController>().engineRPM, 1f, -90f, 180f);
        PointRotation(UIObjects[2], Length(rigidbody.velocity)*3.6f, 260f, -60f, 240f);
        Gear();
    }

    void Abs_Tc()
    {
        abs = Car.GetComponent<CarController>().absMultiplier <1f;
        tc = Car.GetComponent<CarController>().tcMultiplier < 1f;
        
        if (abs&&Length(rigidbody.velocity)>1f)
        {
            UIObjects[0].GetComponent<Image>().color = Color.red;
        }
        else
        {
            UIObjects[0].GetComponent<Image>().color = Color.white;
        }

        if (tc)
        {
            UIObjects[1].GetComponent<Image>().color = Color.red;
        }
        else
        {
            UIObjects[1].GetComponent<Image>().color = Color.white;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            UIObjects[5].GetComponent<Image>().color = Color.red;
        }
        else
        {
            UIObjects[5].GetComponent <Image>().color = Color.white;
        }


    }

    void PointRotation(GameObject point,float source,float sourceMax,float minAngle,float maxAngle)
    {
        float angle = (source / sourceMax)*(maxAngle - minAngle);       
        point.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 180f, minAngle + angle);
    }

    void Gear() 
    {
        float gear = Car.GetComponent<CarController>().gear;
        string string0 = gear.ToString("0");
        if (gear == 0)
        {
            string0 = "R";
        }      
        UIObjects[4].GetComponent<TextMeshProUGUI>().text = string0;
    }


    public float Length(Vector3 vector3)//3维向量 浮点, 取模
    {
        float length = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
        return length;
    }

}
