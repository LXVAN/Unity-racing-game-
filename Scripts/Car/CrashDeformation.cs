using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CrashDeformation : MonoBehaviour
{
    [Range(0, 10)]
    public float deformRadius = 0.2f;
    [Range(0, 10)]
    public float maxDeform = 0.1f;
    [Range(0, 1)]
    public float damageFalloff = 1;
    [Range(0, 10)]
    public float damageMultiplier = 1;
    [Range(0, 100000)]
    public float minDamage = 1;
    public GameObject Rigidbody;//Ҫ�α��gameobject �е�rigidbody
    public GameObject[] meshes;//Ҫ���εĵ�����mesh ���� ����
    


    private MeshFilter filter;
    private Rigidbody physics;
    private MeshCollider coll;
    private Vector3[] startingVerticies;
    private Vector3[] meshVerticies;
    private Vector3[] originMeshVerticies; //���ʼ��vertices������
    private List<Vector3[]> list;
    private ArrayList arrays;
    void Start()
    {
        //filter = GetComponent<MeshFilter>();
        //physics = GetComponent<Rigidbody>();
        physics = Rigidbody.GetComponent<Rigidbody>();
           
    }
    

    void OnCollisionEnter(Collision collision)
    {
        
        float collisionPower = collision.impulse.magnitude;

        if (collisionPower > minDamage)
        {

            foreach (GameObject mesh in meshes)
                //����ط���meshes������һ�������棬���ǳ��Ӳ�ͬ�������ɵ�������Ĵ󼯺ϣ�ÿ��mesh�ж��кܶ��������
            {
                filter = mesh.GetComponent<MeshFilter>();
                startingVerticies = filter.mesh.vertices;//unity�е�mesh.vertices�൱��houdini �е�points
                meshVerticies = filter.mesh.vertices;
                if (mesh.GetComponent<MeshCollider>())
                    coll = mesh.GetComponent<MeshCollider>();
                foreach (ContactPoint point in collision.contacts)
                {
                    for (int i = 0; i < meshVerticies.Length; i++)
                    {
                        Vector3 vertexPosition = meshVerticies[i];//��Щλ�þ����ھֲ�����ϵ��
                        Vector3 pointPosition = transform.InverseTransformPoint(point.point);
                        float distanceFromCollision = Vector3.Distance(vertexPosition, pointPosition);
                        float distanceFromOriginal = Vector3.Distance(startingVerticies[i], vertexPosition);

                        if (distanceFromCollision < deformRadius && distanceFromOriginal < maxDeform) // If within collision radius and within max deform
                        {
                            float falloff = 1 - (distanceFromCollision / deformRadius) * damageFalloff;

                            float xDeform = pointPosition.x * falloff;
                            float yDeform = pointPosition.y * falloff;
                            float zDeform = pointPosition.z * falloff;

                            xDeform = Mathf.Clamp(xDeform, 0, maxDeform);
                            yDeform = Mathf.Clamp(yDeform, 0, maxDeform);
                            zDeform = Mathf.Clamp(zDeform, 0, maxDeform);

                            Vector3 deform = new Vector3(xDeform, yDeform, zDeform);
                            meshVerticies[i] -= deform * damageMultiplier;
                        }
                    }
                }
                UpdateMeshVerticies();
            }
            
        }
    }

    void UpdateMeshVerticies()
    {
        
        filter.mesh.vertices = meshVerticies;
        //coll.sharedMesh = filter.mesh;//�������л���ʧ�ǳ��������
        
    }
}