using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LightingManager))]

public class CarController : MonoBehaviour
{
    public InputManager inputManager;
    public LightingManager lightingManager;//lm
    public WheelCollider[] wheelColliders;//�ϸ��� br,bl,fr,fl��������

    public List<GameObject> steeringWheels;
    public List<GameObject> wheelMeshes;//������Ⱦ��mesh
    public GameObject[] mids;
    public GameObject[] wheelAlignment;
    public AnimationCurve rpmToTorqueCurve;
    public AnimationCurve gearCurve;
    public float throttleCoefficient = 30000f;//motorTorque����
    public float brakeCoefficient = 60000f;//ɲ������
    public float brakeBias = 0.6f;//ǰ��ɲ����
    public float maxTurn = 30f;//���ת��Ƕ�
    public Transform CenterOfMass;//CM
    public Rigidbody rigidBody;//rb
    public float wheelRadius;//wheelcollider ����е�radius,������������ת��,ע���������ӵ�ת���ɳ��ٵó�,����Ӧ�����������ٽ��ж�������
    public float gear = 1;//��λ 0-6,����0Ϊ������
    private GameObject[] arrayWheelMeshes;//wheelMeshes ������
    private WheelCollider[] arrayBrakeWheels;// brakeWheels ������
    private WheelCollider[] arrayThrottleWheels;// throttleWheels ������

    public GameObject Tuning;//��������

    public float absMultiplier = 1f;
    public float tcMultiplier = 1f;

    private float frontLsdThrottleLockedRatio = 0f;
    private float frontLsdBrakeLockedRatio = 0f;
    private float backLsdThrottleLockedRatio = 0f;
    private float backLsdBrakeLockedRatio = 0f;

    private bool bool0 = false;
    void Start()
    {
        inputManager = GetComponent<InputManager>();
        rigidBody = GetComponent<Rigidbody>();
        //StartCoroutine(WaitForFIVESeconds());//��Ϸ��ʼ����Э�̣�Ϊ��ʱ������׼��
        //transform.position = new Vector3(679, 3.9f, 230);����������ͼ
    }

    private void Update()
    {
        GetLsdLockedRatioValue();
        brakeBias = 0.6f + Tuning.GetComponent<SliderTuning>().brakeBias;//��õ���������, slidertuning ����е�brakebiasֵ
        brakeCoefficient = 450000f + 10000f * Tuning.GetComponent<SliderTuning>().brakeForce;//brakeForce(1-2)*60000f

        ReloadCar();
        ResetCar();
        lightingManager.ToggleHeadLights(inputManager.lightSwitch);//����ǰ�յ�
        gear = Clamp(Gear(gear, inputManager.upArrow, inputManager.downArrow), 0, 6);


    }

    void FixedUpdate()//�ų�֡�ʲ��ȶ����˶���Ӱ��
    {

        torque = Throttle(rpmToTorqueCurve, gear, gearCurve);
        WheelMotorTorqueConfigure();//����,��λ,��ɲ,��ɲ

        NewCarBodyHeight();// CarBodyHeight();//����ǰ��߶ȵĵ���

        Esp(Tuning.GetComponent<SliderTuning>().esp[0] > 0, Tuning.GetComponent<SliderTuning>().esp[1] > 0);//������أ�����abs ��tc
        WheelRolling();//���г��ֹ������߼�����
        ResetCollisionPerformance();
        Debug.Log("vs code running!");
    }
    public void Esp(bool abs, bool tc)//��������ֵ�����Ƿ���abs����tc
    {
        WheelHit wheelHit = new WheelHit();
        for (int i = 0; i < 4; i++)
        {
            //��ʵ��abs
            wheelColliders[i].GetGroundHit(out wheelHit);
            //Collider collider = wheelHit.collider;
            float forwardSlip = Mathf.Abs(wheelHit.forwardSlip);
            float sideswaySlip = Mathf.Abs(wheelHit.sidewaysSlip);
            if (i == 2 || i == 3)//ʵ��abs
            {
                if ((forwardSlip > wheelColliders[i].forwardFriction.extremumSlip
                    || wheelColliders[i].sidewaysFriction.extremumSlip > 0.2) && abs && inputManager.brake > 0.5f)
                {
                    absMultiplier -= 0.01f;
                    Debug.Log("abs!");
                    absMultiplier = Clamp(absMultiplier, 0.8f, 1);
                    //rigidBody.AddForceAtPosition(-transform.up * 100000f, transform.position);
                }
                else
                {
                    absMultiplier = 1f;
                }
            }
            else//i=0����i=1,������
            {
                if ((forwardSlip > wheelColliders[i].forwardFriction.extremumSlip
                    || sideswaySlip > wheelColliders[i].sidewaysFriction.extremumSlip) && tc && inputManager.throttle > 0.5f)
                {
                    tcMultiplier -= 0.01f;
                    Debug.Log("tc!");
                    tcMultiplier = Clamp(tcMultiplier, 0.8f, 1);
                }
                else
                {
                    tcMultiplier = 1f;
                }
            }
        }
    }

    public float engineRPM = 0.1f;
    public float torque;
    public float Throttle(AnimationCurve rpmToTorqueCurve, float gear, AnimationCurve gearCurve)
    {
        /*�ɵ�rpm to torque����
        //��rpm to torque �����У�rpm �ڷ�Χ[0,0.8]��0-8000ת
        //���ڵĳ��Ӵ�Լ��Ч110ƥ
        gear = Clamp(gear, 0f, 6f);
        if (inputManager.wPressed)
        {
            engineRPM += (0.0025f / gear) - 0.0007f*(Length(rigidBody.velocity)*3.6f/500f);//��������
        }
        else if (inputManager.sPressed)
        {
            engineRPM -= 0.003f;//����ɲ��
        }
        else
        {
            engineRPM -= 0.0005f;//��Ȼ����
        }

        if (inputManager.upArrow&&gear>=1&&gear<=6)
        {
            engineRPM -= 0.2f;
        }
        else if (inputManager.downArrow&&gear>=1 && gear <= 6)
        {
            engineRPM += 0.2f;
        }

        engineRPM = Clamp(engineRPM, 0f, 0.8f);//�����ٸ��ˣ����߾���8000ת
        return rpmToTorqueCurve.Evaluate(engineRPM) * 1000f / gearCurve.Evaluate(gear / 10f);//��ת�ٶ�Ӧ��Ť�ط���
        */
        /*ͨ�����ټ�������ת�٣��ٵó�Ť�������������ϣ���Ȼǰ��������Ҫ���Ż��и�����ת��
        */
        if (gear <= 0)
        {
            gear = 1;
        }


        float speed = rigidBody.velocity.magnitude;//m/s
        if (inputManager.wPressed)
        {
            engineRPM = speed / (0.34f * gear * 50f) + 0.1f;
        }
        else if (inputManager.sPressed)
        {
            if (engineRPM > 0.1f)
            {
                engineRPM -= 0.003f;
            }
        }
        else
        {
            if (engineRPM > 0.1f)
            {
                engineRPM -= 0.0005f;
            }
        }
        engineRPM = Clamp(engineRPM, 0f, 0.8f);
        float torque = rpmToTorqueCurve.Evaluate(engineRPM) * 1000f / gearCurve.Evaluate(gear / 10f);
        if (engineRPM >= 0.78f && engineRPM <= 0.85f)
        {
            torque /= 30 / gear;//��ת�ٱƽ�����ʱ��Ҫ���ͣ��ͼ���Ť��������ٶ���Ȼ�Ͳ���������
        }

        return torque;


    }

    /*
    //private bool doTrue;
    //IEnumerator WaitForFIVESeconds()
    //    //׼����ʱ����
    //{ 
    //    yield return new WaitForSeconds(5);
    //    //������ʱ����
    //    doTrue = ReturnTrue();
    //}

    //public bool ReturnTrue()
    //{
    //    return true;
    //}
    */

    public void NewCarBodyHeight()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 vector;
            Quaternion quaternion;
            wheelColliders[i].GetWorldPose(out vector, out quaternion);
            wheelMeshes[i].transform.position = vector;
        }
        /*
        //--------------------------�����Ǿɵĺ���
        //ʹ��getworldpose ��������ȡ wheelcollider ����������е�λ��
        //�����λ�ø��������mesh,����ʵ�ֳ����߶������Ҷ�̬�ĵ���
        //for (int i = 0; i < 4; i++)
        //{
        //    if (i == 0 || i == 1)
        //    {

        //        Vector3 vector0;
        //        Quaternion quaternion;
        //        arrayThrottleWheels[i].GetWorldPose(out vector0, out quaternion);
        //        arrayWheelMeshes[i].transform.position = vector0;
        //    }
        //    else
        //    {
        //        Vector3 vector0;
        //        Quaternion quaternion;
        //        arrayBrakeWheels[i - 2].GetWorldPose(out vector0, out quaternion);
        //        arrayWheelMeshes[i].transform.position = vector0;
        //    }
        //}
        */
    }

    public void WheelRolling()
    {
        //---------------�йس���ת���߼�------------------------------------------
        /*�������������,���������wheelcollider,�м��� ��Ӳ�,�������� ��Ⱦ��
         * ����ת���߼�Ϊ :  ���ﵽ��: ����(��Ⱦ��)�����ֶ�λ(��Ӳ�)��ת��(wheelcollider)
         * ������֤�����������
         * ����alignment���� wheelcollider�ϵ�ԭ����: wheelcollider ����ת�������Ÿ��������ת�����仯
        */
        for (int i = 0; i < 2; i++)//�µ�����ת��
        {
            wheelColliders[i + 2].steerAngle = maxTurn * inputManager.steer;
            wheelColliders[i + 2].transform.localEulerAngles = new Vector3(0f, inputManager.steer * maxTurn, 0f);

        }


        for (int i = 0; i < 4; i++)// ����������ȷת��
        {
            float rpm = wheelColliders[i].rpm;
            if (rpm == float.NaN)
            //Ϊ��ʵ�ֲ����������Ĳ���
            {
                rpm = 0f;
            }
            Vector3 wheelRot = new Vector3(360 * rpm * Time.deltaTime / 60, 0f, 0f);
            if ((wheelRot.x == float.NaN) || (wheelRot.y == float.NaN) || (wheelRot.z == float.NaN) || (wheelRot.x == float.PositiveInfinity) || (wheelRot.x == float.NegativeInfinity))
            {
                wheelRot = new Vector3(0f, 0f, 0f);
                Debug.Log("set wheelRot to vec3.zero");
            }



            wheelMeshes[i].transform.Rotate(wheelRot);

        }



        for (int i = 0; i < 4; i++)//ǰ���ֵ���������ŵ�����
        {
            //���Ǵ������¿�,toe in ���ڰ�,toe out �����,ת��y��
            //��Ǵ�ǰ����,������/\,������\/,ת��z��
            if (i == 0 || i == 1)//����
            {
                GameObject mesh = mids[i];
                Vector3 up = mesh.transform.up;//�ֲ�����ϵ��y��
                Vector3 forward = mesh.transform.forward;//�ֲ�����ϵ��z��
                float backToe = Tuning.GetComponent<SliderTuning>().alignment[0];
                float backCamber = Tuning.GetComponent<SliderTuning>().alignment[1];//���               
                mesh.transform.localEulerAngles = new Vector3(0f, backToe * Mathf.Pow(-1, i), backCamber * Mathf.Pow(-1, i));

            }
            else//ǰ��
            {

                GameObject mesh = mids[i];
                Vector3 up = mesh.transform.up;
                Vector3 forward = mesh.transform.forward;
                float frontToe = Tuning.GetComponent<SliderTuning>().alignment[2];
                float frontCamber = Tuning.GetComponent<SliderTuning>().alignment[3];
                mesh.transform.localEulerAngles = new Vector3(0f, frontToe * Mathf.Pow(-1, i), frontCamber * Mathf.Pow(-1, i));//i = 2, pow����1,i = 3,pow = -1

            }
        }
        //---------------------------------------------------------
    }

    public void CarBodyHeight()
    {
        //�����������Ҫ��,ʹ�� wheel collider �е� ���� get world pos ��������������ֺͳ����϶����


        //ʹ��ǰ,�Ȱ��ĸ����ӵ�mesh ��� raycast (��д��csharp,����unity �Դ���)���
        //���������������,ʹ������������Ȼ���д�ģ���������
        /*������������ԭ����,��ֻ��һ�ߵ�suspension distance ������ʱ
         * ������ǰ�ֵ� suspension distance ����ʱ,���ӻ��Ժ���Ϊ֧�������ת
         * ��ʱ�����ܵ����Ľ�ǰ�ֵ�mesh �����ƶ�,Ӧ����б��,����ǰ��һ�����½�,
         * ���������ת�ĽǶȲ��ü���,����û����ô��,ֻ�ǵ����Ľ�����mesh �����ƶ�
         * 
         */
        for (int i = 0; i < 4; i++)
        {
            if (i == 0 || i == 1)//����
            {
                float backSuspensionDistance = Tuning.GetComponent<SliderTuning>().spring[0];//���ֵ� ���Ҹ߶�
                if (wheelMeshes[i].GetComponent<RayCast>().collision)
                {
                    float distance = wheelMeshes[i].GetComponent<RayCast>().distance;
                    float radius = arrayThrottleWheels[i].radius;
                    if (distance - radius > 0.05f)
                    {
                        wheelMeshes[i].transform.localPosition = new Vector3(0, -backSuspensionDistance, 0);
                    }
                }
            }
            else//ǰ��,i=2��3
            {
                float frontSuspensionDistance = Tuning.GetComponent<SliderTuning>().spring[3];//ǰ�ֵ� ���Ҹ߶�           
                if (wheelMeshes[i].GetComponent<RayCast>().collision)
                {
                    float distance = wheelMeshes[i].GetComponent<RayCast>().distance;
                    float radius = arrayBrakeWheels[i - 2].radius;
                    if (distance - radius > 0.05f)
                    {
                        wheelMeshes[i].transform.localPosition = new Vector3(0, -frontSuspensionDistance, 0);
                    }
                }
            }
        }
    }

    public void WheelMotorTorqueConfigure()
    //��������,��λ,��ɲ,��ɲ
    {

        float frontRPMSum = Mathf.Abs(wheelColliders[2].rpm) + Math.Abs(wheelColliders[3].rpm);
        float backRPMSum = Mathf.Abs(wheelColliders[0].rpm) + Math.Abs(wheelColliders[1].rpm);

        if (frontRPMSum == 0f || frontRPMSum == float.NaN)
        {
            frontRPMSum = 1f;//����������if�жϣ����ײ���NaNֵ���Ӷ�ʹ��Ϸ����
        }
        if (backRPMSum == 0f || backRPMSum == float.NaN)
        {
            backRPMSum = 1f;
        }


        for (int i = 0; i < 2; i++)
        {
            if (inputManager.handBrake)
            {
                wheelColliders[i].brakeTorque = throttleCoefficient * Time.deltaTime * 3;
            }
            else
            {
                wheelColliders[i].brakeTorque = 0;
                if (gear > 0)
                {

                    Shift(wheelColliders[i], gear, 1, 75f, backRPMSum);
                    Shift(wheelColliders[i], gear, 2, 110f, backRPMSum);
                    Shift(wheelColliders[i], gear, 3, 160f, backRPMSum);
                    Shift(wheelColliders[i], gear, 4, 200f, backRPMSum);
                    Shift(wheelColliders[i], gear, 5, 220f, backRPMSum);
                    Shift(wheelColliders[i], gear, 6, 300f, backRPMSum);
                }
                else
                {
                    //wheelColliders[i].motorTorque = -throttleCoefficient * Time.deltaTime * inputManager.throttle;
                    wheelColliders[i].motorTorque = -inputManager.throttle * torque;
                }

            }


            lightingManager.BrakeLight(inputManager.sPressed);




            float brakeLSDMultiplier_;//= ((frontRPMSum - Mathf.Abs(wheelColliders[i + 2].rpm)) / frontRPMSum);//ǰ��
            wheelColliders[i + 2].brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * brakeBias * absMultiplier;//ǰ��ɲ��
            brakeLSDMultiplier_ = LsdMultiplier(frontLsdBrakeLockedRatio, Mathf.Abs(wheelColliders[i + 2].rpm), frontRPMSum, "brake");
            //wheelColliders[i + 2].brakeTorque *= brakeLSDMultiplier_ * 2f;
            wheelColliders[i + 2].brakeTorque = Mathf.Abs(wheelColliders[i + 2].brakeTorque);



            if (inputManager.brake > 0)
            //����ɲ��,��Ҫȥ������жϣ����ȥ����0��һֱ������ɲ��ֵ��ֱ��s�Ϳո��£���ɲ�Ż���ֵ
            {
                float brakeLSDMultiplier__;//= ((backRPMSum - Mathf.Abs(wheelColliders[i].rpm)) / backRPMSum);               
                wheelColliders[i].brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * (1 - brakeBias) * absMultiplier;
                brakeLSDMultiplier__ = LsdMultiplier(backLsdBrakeLockedRatio, Mathf.Abs(wheelColliders[i].rpm), backRPMSum, "brake");
                //wheelColliders[i].brakeTorque *= brakeLSDMultiplier__ * 2f;
                wheelColliders[i].brakeTorque = Mathf.Abs(wheelColliders[i ].brakeTorque);


            }



        }
        /*���ڵ�λ
        * 1.Ť�غ͹���
        *   һ����˵��������ת��Խ�ߣ������������Ĺ���Խ��
        *   Ť�ز�һ������ת��Խ�߶�Խ��ͨ����һ��ת�����䣬��������ܲ�������Ť��
        * 
        * 2.�����뽵��
        *   ��͵�λʱ��������ת�ٴﵽ���ߣ�����ʱ����һ��������ʱ��������������������֣�p = p0+p1����ɣ�
        *   ά�������ٶȣ�p0 = f0v, f0 Ϊά�ֵ�ǰ�ٶȵķ�����ǣ�������������������������˼��٣����˼��٣�
        *   ���������٣�p1 = f1v��f1Ϊ�������ǣ������ǰ����f0ά���ٶȣ���ʱ������f1�����٣�
        *   
        *   ��ߵ�λʱ��������ת�ٴﵽ���ߣ���ʱ������������ʣ�
        *   ������������һ��������p = fv�� ���ʲ��䣬
        *   ��λ���ʱ�����ڱ�����Ĵ��ڣ�������������Ť�ر���С�������ṩ��ǣ������С
        *   ��ʱf ���� �����ܵ����������ܴ�һ���֣���ǣ��������Сһ���֣���
        *   ���������ٶ������ǣ�������޽ӽ���0����ʱ�������ٶ�Ҳ��Խ��ԽС���������ٶ�����������ٶ�
        *   
        * 3.��unity ��ʵ�� ��λ�����߼�
        *   �ٶȵͣ���λ�ͣ������ܵ�������С����ӳ��rigidbody����Ͼ���Drag �� angular Drag��ֵ�Ƚ�С��
        *   �ٶȵͣ���λ�ߣ�����С������ת���ϲ�ȥ�����Է������ͷŵĹ���ҲС����ôǣ����Ҳ��С�����پͺ���
        *   
        *   �ٶȸߣ���λ�ߣ�����Ҫ���һ�㣬ǣ����ҲҪ��С�㣨motorTorque��С�㣩
        *   �ٶȸߣ��������͵���ͬʱҪ���;���
        * 
        * 4.��������������Ӱ��ķ�����ת��
        *   ������ת��Ӧ���½�һ����
        *   ������ת��Ӧ������һ����
        * 
        * 
        * 
        */


        /*���ڲ�����
         * �ʼ���������������Ϲ���һ�����ӣ�������ת��ʱ���ڲ������߹���·��������٣�������������ת��һ�£���������̥�ϣ���̥ת��һ��
         * ��͵��£��������Ҫ�������š�ת��һ�����룻�ڲ��������ڡ����š��������ת����������ܵ���������;
         * �������������ӵĻ����ʶ������������󣬴Ӷ�ʹĦ�������ͣ�Ӱ�쳵������
         * 
         * �����������������������Ӵ���ת�ٲ������û�����ӻᱻ�϶���
         * 
         * ������һ����ʻ���Ƚϻ���·��ʱ��һ�������ڿ�ת�����ҷ����˷������Ĵ󲿷�Ť�أ�������һ�������û��̫��Ť�ؽ�����ת�����³�������ԭ�ش�
         * ��ʱ���޻���������LSD���������ó�
         * ������ת�ٲ�� ���� ʱ��LSD ������������������Ť�شӿ�ת�����ӷ������һ��û�д򻬵����ӣ��Ӷ����ó�����ԭ�ش�
         * 
         * ����ͨ�������Ҫץ�ع���ʱ������ǿ��ŵĲ�����
         * ����Ӧ�Զ����ϸߵ����ʱ�����ŵĲ������п���ʹ�����еĳ��Ӷ�������������Ϊ����������������������
         * ��ʱ����Ҫ����������������������һ���ָ����֣�������Ҫע�⣬�����ʸ�������˦����������û�ж�������Ȼ���ȣ�
         * 
         * ����ץ�صķ���
         * ����̥ʼ�չ�������ѻ������������������
         * û�м����������Ť�ط���ǰ����������Ť����ȣ�����ʱ�ض�����������ת�ٲ�
         * ������������Ť�ط��������������ת�ٲ���䲻ͬŤ��
         * ��ʱ��������ʧȥһ�㶯������̥������ʼ��С����ѻ����ʣ�����̥ץ����û���þ���
         * ��ʱ��Ҫһ�������������ֵ�Ť�ط���һ�����������ӣ�������������̥�Ļ����ʶ��ﵽ��ѻ�����
         * 
         * 
         * 
         * 
         * 
         * ���ڴ���ɲ�����صĲ�������
         * ��ǰ����Ȼ��ϣ�����ӹ�������ѵĻ���������
         * ɲ��lsd ����ʱ��
         * ֱ��ȫ��ɲ������������Ӽ����������ת�ٲ������������ɲ���������
         * ����һ��ץ�أ�һ�಻ץ�أ�ץ�ص�һ����ܵ������Ħ���������Գ��ӻ���ץ��һ�������Ϊ�㣬������ת
         * ɲ��lsd ��ȫ��ʱ��
         * ֱ��ȫ��ɲ��������һ���Ȳ���һ���ȱ�����������һ�����ٻ�Ȳ���һ���ת���½��Ŀ�ܶ�
         * ��ôɲ�����ؿ��Է����ת���ٵ�һ�࣬��������һ��
         * ������һ��ֻ�ܷ��䵽���ٵ�ɲ�����أ���ֹ����ʧ��
         * 
         * �������У� ���������������ͷ��
         * ���������������ƣ������ƶ������ֻ��һ�㣨��ȻҲ���ƣ������Ƶ�û����ô������
         * 
         * 
         * 
         */


        /*�����Ǿɵĺ���
            //
            //foreach (WheelCollider wheel in throttleWheels)//ʩ�Ӷ��� motorTorque ����ɲ(����)
            //{
            //    wheel.ConfigureVehicleSubsteps(1f, 5, 100);
            //    if (inputManager.handBrake)//�ж��Ƿ���ɲ
            //    {
            //        wheel.brakeTorque = strenghCoefficient * Time.deltaTime * 2;
            //        //Debug.Log("space pressed");  
            //    }
            //    else
            //    {
            //        wheel.brakeTorque = 0;


            //        if (gear > 0)//��λ�ж�
            //        {
            //            if (gear == 1)
            //            {
            //                wheel.motorTorque = strenghCoefficient * Time.deltaTime * inputManager.throttle;//1-6Ϊǰ��
            //                if (Length(rigidBody.velocity) * 3.6f >= 60f)
            //                {
            //                    rigidBody.velocity = rigidBody.velocity.normalized * 60f / 3.6f;
            //                }
            //            }
            //            Shift(wheel, gear, 2, 80);
            //            Shift(wheel, gear, 3, 100);
            //            Shift(wheel, gear, 4, 120);
            //            Shift(wheel, gear, 5, 160);
            //            Shift(wheel, gear, 6, 200);

            //        }
            //        else
            //        {
            //            wheel.motorTorque = -strenghCoefficient * Time.deltaTime * inputManager.throttle;//0 Ϊ������,����
            //        }

            //    }

            //}
            //foreach (WheelCollider wheel in brakeWheels)//ɲ��ǰ��
            //{
            //    wheel.ConfigureVehicleSubsteps(1f, 5, 100);
            //    wheel.brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * brakeBias;
            //    lightingManager.BrakeLight(inputManager.sPressed);
            //}
            //foreach (WheelCollider wheel in throttleWheels)//��Ӻ��ֽ�ɲ
            //{
            //    //ע������ط�����ֱ�Ӹ�ֵ,��Ȼ�Ḳ�ǵ���ɲ����ֵ
            //    if (inputManager.brake > 0)
            //    {
            //        wheel.ConfigureVehicleSubsteps(1f, 5, 100);
            //        wheel.brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * (1 - brakeBias);
            //    }
            //    else
            //    {

            //    }
            //}
        */
    }


    public float Length(Vector3 vector3)//3ά���� ����, ȡģ
    {
        float length = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
        return length;
    }

    public virtual void ReloadCar()//���¼��س�������ǰλ���Ϸ�
    {
        if (inputManager.enterPressed)//
        {
            transform.rotation = Quaternion.identity;
            transform.position = transform.position + new Vector3(0, 2, 0);//����ǰ����λ�����ϼ�2m
            rigidBody.velocity = new Vector3(0f, 0f, 0f);
        }
    }
    public virtual void ResetCar()//�������ͻص���������ԭ��
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3(0, 2, 0);
            rigidBody.velocity = new Vector3(0f, 0f, 0f);
        }
    }
    public float Clamp(float input, float downLimit, float upLimit)//Clamp ����,��һ���仯��ֵǯ����[downLimit,upLimit]֮��
    {
        float output;
        if (input < downLimit)
        {
            output = downLimit;
        }
        else if (input > upLimit)
        {
            output = upLimit;
        }
        else
        {
            output = input;
        }
        return output;
    }
    public float Gear(float gear, bool gearUp, bool gearDown)//��λ����,������,������
    {
        if (gearUp)
        {
            gear = gear + 1;
        }
        if (gearDown)
        {
            gear = gear - 1;
        }
        return gear;

    }
    public void Shift(WheelCollider wheel, float gear, float limitGear, float limitSpeed, float RPMSum)//��λ���,�ٶ�����||����(wheel collider), ��ǰ��λ,Ҫ�����ٶȵĵ�λ,���Ƶ��ٶ�
    {       //����� motortorque �ϴ��ʱ��,����ֱ�������ٶ�(�����������,������Ȼ�ܿ������),
            //Ӧ��Ҫ����һ��motortorque,���ٶ���������(�������ٶȵ�һ���������Ť�ز����õ����,����Ҫ����,��û��Ǽ��ٵ�һ�����������ٵ����)
            //һ��͵�λ,���ٻ�ȳ��ٿ�,��Ϊtorque���������Ѿ�����̥���Ħ������Ҫ��,��ɴ�;�ߵ�λʱ,��̥Ħ����һ��Ƚϸ���,��ʱ����Ӧ����ƥ�䳵��
        if (gear == limitGear)
        {
            /*
            //����Ǿɵ�
            //wheel.motorTorque = throttleCoefficient * Time.deltaTime * inputManager.throttle * tcMultiplier;


            //������µģ��ȼ�����ȫ���ŵĲ������������������ǵ�ת�����ɷ���Ť��,
            //������������һ�ߵ����Ӵ�ʱ���򻬵����ӻ�ֵþ��������Ť�أ���û�д򻬵����ӻ�ֵú��ٵ�Ť��
            //Ȼ���ټ���������������򻬵����ӷ����Ť�ػ���һ�㣬�Ӷ��������Ť�ط�����û�д򻬵�����

            //���Դӻ��������֣�ʹ�ó��ٷ����������rpm
            */


            //ͨ��ת�ٲ�������Ť��
            float rpm = Mathf.Abs(wheel.rpm);
            float LsdThrottleMultiplier = rpm / RPMSum;

            LsdThrottleMultiplier = LsdMultiplier(backLsdThrottleLockedRatio, rpm, RPMSum, "throttle");
            wheel.motorTorque = 2 * inputManager.throttle * 2 * torque * LsdThrottleMultiplier * tcMultiplier;//ʵ����ȫ���ŵĲ�����

            Debug.Log("throttle differential working");

            //if (Length(rigidBody.velocity) * 3.6f >= limitSpeed)
            ////���������
            //{
            //    rigidBody.velocity = rigidBody.velocity.normalized * limitSpeed / 3.6f;
            //}
        }
    }

    private float LsdMultiplier(float LockedRatio, float rpm, float RPMSum, string throttle_or_brake)
    //LockedRatio���� �ĸ������ʵ�����֮һ��rpm==��ǰ���ֵ�ת�٣�rpmsum ��һ�೵�ֵ���ת�٣� side ������ throttle ���� brake
    {
        rpm = Mathf.Abs(rpm);
        RPMSum = Mathf.Abs(RPMSum);
        float LsdMultiplier = 1f;

        if (throttle_or_brake.Contains("throttle") || throttle_or_brake.Contains("Throttle") || throttle_or_brake.Contains("THROTTLE"))
        {
            LsdMultiplier = rpm / RPMSum;
            if (LockedRatio > 0f)
            {

                float anotherRpm = RPMSum - rpm;
                if (rpm > anotherRpm)
                {
                    LsdMultiplier = LsdMultiplier - LsdMultiplier * LockedRatio * 0.5f;
                    //�� 0.5 ��ԭ���ǣ���lockratio =1 ʱ��Ť�ػ���Ϊ��������ȫ������ȫ����������û�д򻬵������ϣ�������ʵ���ǲ����ܷ�����
                }
                else//rpm<=anotherRpm
                {
                    LsdMultiplier = LsdMultiplier + 0.5f * LockedRatio * (RPMSum - rpm) / RPMSum;
                }


            }
            else
            {
                LsdMultiplier = rpm / RPMSum;
            }

        }
        if (throttle_or_brake.Contains("brake") || throttle_or_brake.Contains("Brake") || throttle_or_brake.Contains("BRAKE"))
        {
            LsdMultiplier = (RPMSum - rpm) / RPMSum;
            if (LockedRatio > 0f)
            {
                Debug.Log("detected back locked ratio >0!!!");
                float another = RPMSum - rpm;
                if (rpm > another)
                {
                    LsdMultiplier = LsdMultiplier + (rpm / RPMSum) * LockedRatio;
                }
                else
                {
                    LsdMultiplier = LsdMultiplier - LockedRatio * LsdMultiplier;
                }
            }
            else
            {
                LsdMultiplier = (RPMSum - rpm) / RPMSum;
            }

        }
        return LsdMultiplier;
    }
    private void GetLsdLockedRatioValue()
    //����� tuning ������ slidertuning �ű����ݶ�����4��������
    {
        frontLsdBrakeLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[0];
        frontLsdThrottleLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[1];
        backLsdBrakeLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[2];
        backLsdThrottleLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[3];
    }

    public List<float> collisionPerformance = new List<float> { 1, 1, 1, 1, 1, 1 };
    /*
    public virtual List<float> PerformanceAfterCollision()
    //����������ײ��������������ʧ
    {
        float max = 25000f * Mathf.Sqrt(2);
        Vector2 vector2 = new Vector2(collisionDIr.x, collisionDIr.z);
        if (collisionDIr.x > 0)
        {
            if (collisionDIr.z > 0)
            {
                Debug.Log("��ǰ��");
                collisionPerformance[2] += vector2.magnitude / max;
                //�������������û�ã�Ҫ��WheelColliderConfigure����ȥ����
                //WheelFrictionCurve wheelFrictionCurve = wheelColliders[2].forwardFriction;
                //wheelFrictionCurve.stiffness *= 0.1f;                      //*= 1-collisionPerformance[2];

                //wheelColliders[2].forwardFriction = wheelFrictionCurve;

            }
            else if (collisionDIr.z < 0)
            {
                //collisionPerformance[0] = 
                //Debug.Log("�Һ�");

                collisionPerformance[0] += vector2.magnitude / max;

            }
        }
        else if (collisionDIr.x < 0)
        {
            if (collisionDIr.z > 0)
            {
                //Debug.Log("��ǰ��");
                collisionPerformance[3] += vector2.magnitude / max;


            }
            else if (collisionDIr.z < 0)
            {
                //Debug.Log("���");
                collisionPerformance[1] += vector2.magnitude / max;
            }
        }
        return collisionPerformance;
    }*/
    private Vector3 collisionDIr;

    void ResetCollisionPerformance()
    {
        if (Input.GetKeyDown(KeyCode.R))
        { 
            for(int i = 0;i<collisionPerformance.Count;i++)
            {
                collisionPerformance[i] = 0f;
            }
            Debug.Log("reset list collision performance");
        }
    }
    public void OnCollisionEnter(Collision collision)
    {

        collisionDIr = collision.impulse;//��ó����ķ���
        collisionDIr = -transform.InverseTransformDirection(collisionDIr);
        //�������ķ���任�����ӵľֲ����꣬���ϸ��ű�ʾ��ָ�Ǹ�����      
        //Debug.Log(collisionDIr);
        float max = 25000f * Mathf.Sqrt(2);

        Vector2 vector2 = new Vector2(collisionDIr.x, collisionDIr.z);
        if (collisionDIr.z > 0 && collisionDIr.x > 0)
        {
            //Debug.Log("��ǰ��");
            collisionPerformance[2] += vector2.magnitude / max;
            //�������������û�ã�Ҫ��WheelColliderConfigure����ȥ����
            //WheelFrictionCurve wheelFrictionCurve = wheelColliders[2].forwardFriction;
            //wheelFrictionCurve.stiffness *= 0.1f;                      //*= 1-collisionPerformance[2];

            //wheelColliders[2].forwardFriction = wheelFrictionCurve;

        }
        if (collisionDIr.z < 0 && collisionDIr.x > 0)
        {
            //collisionPerformance[0] = 
            //Debug.Log("�Һ�");

            collisionPerformance[0] += vector2.magnitude / max;

        }

        if (collisionDIr.x < 0 && collisionDIr.z > 0)
        {
            //Debug.Log("��ǰ��");
            collisionPerformance[3] += vector2.magnitude / max;
        }

        if (collisionDIr.z < 0 && collisionDIr.x < 0)
        {
            //Debug.Log("���");
            collisionPerformance[1] += vector2.magnitude / max;
        }


    }
}
        


    



