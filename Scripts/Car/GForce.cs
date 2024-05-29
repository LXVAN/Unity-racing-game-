using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

public class GForce : MonoBehaviour
{
    public GameObject Car;
    public GameObject gForceSphere;
    private Vector3 velocity;
    private Vector3 pos;//gforcesphere ԭ����λ��
    // Start is called before the first frame update
    void Start()
    {
        pos = gForceSphere.transform.position;   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 acceleration = (Car.GetComponent<Rigidbody>().velocity - velocity)/Time.deltaTime;
        velocity = Car.GetComponent<Rigidbody>().velocity;
        float cosTheta0 = Vector3.Dot(velocity.normalized, new Vector3(0f, 0f, 1f));//���� �ٶȺ� (0,0,1)������ֵ
        float theta0  = Mathf.Acos(cosTheta0);//����Ƕ�,�Ƕ���,�����������       
        Vector2 dir = new Vector2(acceleration.x, acceleration.z);        
        dir = new Vector2(dir.x * Mathf.Cos(theta0) - dir.y * Mathf.Sin(theta0), dir.x * Mathf.Sin(theta0) + dir.y * Mathf.Cos(theta0));        
        gForceSphere.transform.localPosition = new Vector3(dir.x, 0f, dir.y)*0.75f ;
        

        
        
    }
    public float Length(Vector3 vector3)//3ά���� ����, ȡģ
    {
        float length = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
        return length;
    }
}
