using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGroundHitInformation : MonoBehaviour
{
    public WheelCollider[] wheelColliders;
    private string text;//显示在text mesh 中的文本
    private bool a = false;
    public string[] colliderName = { "","","",""};
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        WheelColliderHitMaterial();
        

        if (Input.GetKeyDown(KeyCode.F4))
        {
            a = !a;
        }
        if (a)
        {
            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                GroundHitInformation();
            }
            //GetComponent<TextMesh>().text = text;
        }
        else
        {
            GetComponent<TextMesh>().text = "Press F4 to Check HitInfo!";
        }
    }

    private void WheelColliderHitMaterial()
    //使用wheelcollider 获得 轮子接触的物体名字
    {
        WheelHit hit = new WheelHit();
        for (int i = 0; i < 4; i++)
        {
            if (wheelColliders[i].GetGroundHit(out hit))
            {
                Collider collider = hit.collider;
                colliderName[i] = collider.name;
            }
        }
    }
    private void GroundHitInformation()
    {
        GetComponent<TextMesh>().text = "";
        WheelHit wheelHit = new WheelHit();//
        for (int i = 0; i < wheelColliders.Length ; i++)
        {
            if (wheelColliders[i].GetGroundHit(out wheelHit))//判断wheelcollider 是否接触地面
            {
                Collider collider = wheelHit.collider;
                float force = wheelHit.force;
                Vector3 forwardDir = wheelHit.forwardDir;
                float forwardSlip = wheelHit.forwardSlip;
                //Vector3 normal = wheelHit.normal;
                //Vector3 point = wheelHit.point;
                Vector3 sidewaysDir = wheelHit.sidewaysDir;
                float sidewaysSlip = wheelHit.sidewaysSlip;
                float stiffness = wheelColliders[i].forwardFriction.stiffness;
                
                GetComponent<TextMesh>().text += "wheelCollidierName = " + wheelColliders[i].name + "\n" +
                                                 "Ground = " + collider + "\n" +
                                                 "force = " + force.ToString("0.00") + "\n" +
                                                  
                                                 "forwardSlip = " + forwardSlip.ToString("0.00") + "\n" +
                                                 //normal.ToString("0.00") + "/n" +
                                                 "stiffness = " + stiffness.ToString("0.00") + "\n" +
                                                 
                                                 "sidewaysSlip = " + sidewaysSlip.ToString("0.00") + "\n"+"\n";


            }
        }
    }

        
}
