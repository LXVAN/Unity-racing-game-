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
    public WheelCollider[] wheelColliders;//严格按照 br,bl,fr,fl排序填入

    public List<GameObject> steeringWheels;
    public List<GameObject> wheelMeshes;//轮子渲染的mesh
    public GameObject[] mids;
    public GameObject[] wheelAlignment;
    public AnimationCurve rpmToTorqueCurve;
    public AnimationCurve gearCurve;
    public float throttleCoefficient = 30000f;//motorTorque力度
    public float brakeCoefficient = 60000f;//刹车力度
    public float brakeBias = 0.6f;//前后刹车比
    public float maxTurn = 30f;//最大转向角度
    public Transform CenterOfMass;//CM
    public Rigidbody rigidBody;//rb
    public float wheelRadius;//wheelcollider 组件中的radius,用来计算轮子转速,注意这里轮子的转速由车速得出,正常应该由自身轮速进行动画驱动
    public float gear = 1;//档位 0-6,其中0为倒车档
    private GameObject[] arrayWheelMeshes;//wheelMeshes 的数组
    private WheelCollider[] arrayBrakeWheels;// brakeWheels 的数组
    private WheelCollider[] arrayThrottleWheels;// throttleWheels 的数组

    public GameObject Tuning;//调车物体

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
        //StartCoroutine(WaitForFIVESeconds());//游戏开始启动协程，为延时函数做准备
        //transform.position = new Vector3(679, 3.9f, 230);测试拉力地图
    }

    private void Update()
    {
        GetLsdLockedRatioValue();
        brakeBias = 0.6f + Tuning.GetComponent<SliderTuning>().brakeBias;//获得调车物体中, slidertuning 组件中的brakebias值
        brakeCoefficient = 450000f + 10000f * Tuning.GetComponent<SliderTuning>().brakeForce;//brakeForce(1-2)*60000f

        ReloadCar();
        ResetCar();
        lightingManager.ToggleHeadLights(inputManager.lightSwitch);//开关前照灯
        gear = Clamp(Gear(gear, inputManager.upArrow, inputManager.downArrow), 0, 6);


    }

    void FixedUpdate()//排除帧率不稳定对运动的影响
    {

        torque = Throttle(rpmToTorqueCurve, gear, gearCurve);
        WheelMotorTorqueConfigure();//油门,档位,手刹,脚刹

        NewCarBodyHeight();// CarBodyHeight();//车身前后高度的调节

        Esp(Tuning.GetComponent<SliderTuning>().esp[0] > 0, Tuning.GetComponent<SliderTuning>().esp[1] > 0);//车辆电控，包含abs 和tc
        WheelRolling();//所有车轮滚动的逻辑函数
        ResetCollisionPerformance();
        Debug.Log("vs code running!");
    }
    public void Esp(bool abs, bool tc)//两个布尔值决定是否开启abs或者tc
    {
        WheelHit wheelHit = new WheelHit();
        for (int i = 0; i < 4; i++)
        {
            //先实现abs
            wheelColliders[i].GetGroundHit(out wheelHit);
            //Collider collider = wheelHit.collider;
            float forwardSlip = Mathf.Abs(wheelHit.forwardSlip);
            float sideswaySlip = Mathf.Abs(wheelHit.sidewaysSlip);
            if (i == 2 || i == 3)//实现abs
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
            else//i=0或者i=1,即后轮
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
        /*旧的rpm to torque计算
        //在rpm to torque 曲线中，rpm 在范围[0,0.8]即0-8000转
        //现在的车子大约等效110匹
        gear = Clamp(gear, 0f, 6f);
        if (inputManager.wPressed)
        {
            engineRPM += (0.0025f / gear) - 0.0007f*(Length(rigidBody.velocity)*3.6f/500f);//踩下油门
        }
        else if (inputManager.sPressed)
        {
            engineRPM -= 0.003f;//踩下刹车
        }
        else
        {
            engineRPM -= 0.0005f;//自然滑行
        }

        if (inputManager.upArrow&&gear>=1&&gear<=6)
        {
            engineRPM -= 0.2f;
        }
        else if (inputManager.downArrow&&gear>=1 && gear <= 6)
        {
            engineRPM += 0.2f;
        }

        engineRPM = Clamp(engineRPM, 0f, 0.8f);//不能再高了，红线就是8000转
        return rpmToTorqueCurve.Evaluate(engineRPM) * 1000f / gearCurve.Evaluate(gear / 10f);//将转速对应的扭矩返回
        */
        /*通过车速计算引擎转速，再得出扭矩作用在轮子上，当然前提是汽车要打着火，有个基础转速
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
            torque /= 30 / gear;//当转速逼近红线时，要断油，就减少扭矩输出，速度自然就不会上升了
        }

        return torque;


    }

    /*
    //private bool doTrue;
    //IEnumerator WaitForFIVESeconds()
    //    //准备延时函数
    //{ 
    //    yield return new WaitForSeconds(5);
    //    //启用延时函数
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
        //--------------------------以下是旧的函数
        //使用getworldpose 函数来读取 wheelcollider 组件在世界中的位置
        //将这个位置赋予给车轮mesh,即可实现车辆高度随悬挂动态的调整
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
        //---------------有关车轮转动逻辑------------------------------------------
        /*车轮由三层组成,最外面的是wheelcollider,中间是 间接层,最里面是 渲染面
         * 整个转动逻辑为 :  从里到外: 滚动(渲染面)→四轮定位(间接层)→转向(wheelcollider)
         * 这样保证不会出现问题
         * 不把alignment放在 wheelcollider上的原因是: wheelcollider 的旋转不会随着附着组件的转动而变化
        */
        for (int i = 0; i < 2; i++)//新的轮子转向
        {
            wheelColliders[i + 2].steerAngle = maxTurn * inputManager.steer;
            wheelColliders[i + 2].transform.localEulerAngles = new Vector3(0f, inputManager.steer * maxTurn, 0f);

        }


        for (int i = 0; i < 4; i++)// 所有轮子正确转动
        {
            float rpm = wheelColliders[i].rpm;
            if (rpm == float.NaN)
            //为了实现差速器而做的补偿
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



        for (int i = 0; i < 4; i++)//前后轮的倾角与束脚的设置
        {
            //束角从上往下看,toe in 是内八,toe out 是外八,转动y轴
            //倾角从前往后看,负数是/\,正数是\/,转动z轴
            if (i == 0 || i == 1)//后轮
            {
                GameObject mesh = mids[i];
                Vector3 up = mesh.transform.up;//局部坐标系的y轴
                Vector3 forward = mesh.transform.forward;//局部坐标系的z轴
                float backToe = Tuning.GetComponent<SliderTuning>().alignment[0];
                float backCamber = Tuning.GetComponent<SliderTuning>().alignment[1];//倾角               
                mesh.transform.localEulerAngles = new Vector3(0f, backToe * Mathf.Pow(-1, i), backCamber * Mathf.Pow(-1, i));

            }
            else//前轮
            {

                GameObject mesh = mids[i];
                Vector3 up = mesh.transform.up;
                Vector3 forward = mesh.transform.forward;
                float frontToe = Tuning.GetComponent<SliderTuning>().alignment[2];
                float frontCamber = Tuning.GetComponent<SliderTuning>().alignment[3];
                mesh.transform.localEulerAngles = new Vector3(0f, frontToe * Mathf.Pow(-1, i), frontCamber * Mathf.Pow(-1, i));//i = 2, pow等于1,i = 3,pow = -1

            }
        }
        //---------------------------------------------------------
    }

    public void CarBodyHeight()
    {
        //这个函数不需要了,使用 wheel collider 中的 函数 get world pos 可以完美解决车轮和车身间隙问题


        //使用前,先把四个轮子的mesh 添加 raycast (我写的csharp,不是unity 自带的)组件
        //这个函数并不完美,使用起来轮子任然会有穿模地面的现象
        /*出现这个现象的原因是,当只有一边的suspension distance 被调节时
         * 假设是前轮的 suspension distance 增加时,车子会以后轮为支点进行旋转
         * 此时并不能单纯的将前轮的mesh 向下移动,应该是斜着,车身前侧一点再下降,
         * 不过这个旋转的角度不好计算,所以没有这么做,只是单纯的将车轮mesh 向下移动
         * 
         */
        for (int i = 0; i < 4; i++)
        {
            if (i == 0 || i == 1)//后轮
            {
                float backSuspensionDistance = Tuning.GetComponent<SliderTuning>().spring[0];//后轮的 悬挂高度
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
            else//前轮,i=2或3
            {
                float frontSuspensionDistance = Tuning.GetComponent<SliderTuning>().spring[3];//前轮的 悬挂高度           
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
    //管理油门,档位,脚刹,手刹
    {

        float frontRPMSum = Mathf.Abs(wheelColliders[2].rpm) + Math.Abs(wheelColliders[3].rpm);
        float backRPMSum = Mathf.Abs(wheelColliders[0].rpm) + Math.Abs(wheelColliders[1].rpm);

        if (frontRPMSum == 0f || frontRPMSum == float.NaN)
        {
            frontRPMSum = 1f;//不做这两个if判断，容易产生NaN值，从而使游戏报错
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




            float brakeLSDMultiplier_;//= ((frontRPMSum - Mathf.Abs(wheelColliders[i + 2].rpm)) / frontRPMSum);//前轮
            wheelColliders[i + 2].brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * brakeBias * absMultiplier;//前轮刹车
            brakeLSDMultiplier_ = LsdMultiplier(frontLsdBrakeLockedRatio, Mathf.Abs(wheelColliders[i + 2].rpm), frontRPMSum, "brake");
            //wheelColliders[i + 2].brakeTorque *= brakeLSDMultiplier_ * 2f;
            wheelColliders[i + 2].brakeTorque = Mathf.Abs(wheelColliders[i + 2].brakeTorque);



            if (inputManager.brake > 0)
            //后轮刹车,不要去掉这个判断，如果去掉，0会一直覆盖手刹的值，直到s和空格按下，手刹才会有值
            {
                float brakeLSDMultiplier__;//= ((backRPMSum - Mathf.Abs(wheelColliders[i].rpm)) / backRPMSum);               
                wheelColliders[i].brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * (1 - brakeBias) * absMultiplier;
                brakeLSDMultiplier__ = LsdMultiplier(backLsdBrakeLockedRatio, Mathf.Abs(wheelColliders[i].rpm), backRPMSum, "brake");
                //wheelColliders[i].brakeTorque *= brakeLSDMultiplier__ * 2f;
                wheelColliders[i].brakeTorque = Mathf.Abs(wheelColliders[i ].brakeTorque);


            }



        }
        /*关于档位
        * 1.扭矩和功率
        *   一般来说，发动机转速越高，发动机产生的功率越大；
        *   扭矩不一定随着转速越高而越大，通常是一个转速区间，这个区间能产生最大的扭矩
        * 
        * 2.升档与降档
        *   最低档位时，发动机转速达到红线（红线时功率一定），此时发动机输出功率由两部分（p = p0+p1）组成：
        *   维持汽车速度（p0 = f0v, f0 为维持当前速度的发动机牵引力（基本等于阻力），多了加速，少了减速）
        *   让汽车加速（p1 = f1v，f1为多出来的牵引力，前面有f0维持速度，此时可以用f1来加速）
        *   
        *   最高档位时，发动机转速达到红线，此时发动机输出功率：
        *   由于汽车功率一定，功率p = fv， 功率不变，
        *   档位最大时，由于变速箱的存在，发动机发出的扭矩被缩小，所以提供的牵引力最小
        *   此时f 等于 汽车受到的阻力（很大一部分）和牵引力（很小一部分），
        *   让汽车加速多出来的牵引力无限接近于0，此时汽车加速度也就越来越小，汽车的速度趋近于最高速度
        *   
        * 3.在unity 中实现 档位控制逻辑
        *   速度低，档位低，汽车受到的阻力小（反映在rigidbody组件上就是Drag 和 angular Drag的值比较小）
        *   速度低，档位高，阻力小，但是转速上不去，所以发动机释放的功率也小，那么牵引力也很小，加速就很慢
        *   
        *   速度高，档位高，阻力要变大一点，牵引力也要变小点（motorTorque变小点）
        *   速度高，不允许降低档，同时要发送警告
        * 
        * 4.关于升档，降档影响的发动机转速
        *   升档，转速应该下降一部分
        *   降档，转速应该上升一部分
        * 
        * 
        * 
        */


        /*关于差速器
         * 最开始的汽车，驱动轮上共用一根杆子，车辆在转弯时，内侧轮子走过的路径比外侧少，但是由于引擎转速一致，传动到轮胎上，轮胎转速一致
         * 这就导致，外侧轮子要被“拖着”转动一定距离；内侧轮子由于“拖着”外侧轮子转动，自身会受到反作用力;
         * 这样，两个轮子的滑移率都容易慢慢增大，从而使摩擦力降低，影响车辆性能
         * 
         * 差速器，可以允许内外轮子存在转速差，这样就没有轮子会被拖动了
         * 
         * 当车辆一侧行驶到比较滑的路面时，一侧轮子在空转，并且分走了发动机的大部分扭矩，导致另一侧的轮子没有太多扭矩进行旋转，导致车辆容易原地打滑
         * 这时，限滑差速器（LSD）就排上用场
         * 当两侧转速差到达 多少 时，LSD 就锁死，将发动机的扭矩从空转的轮子分配给另一边没有打滑的轮子，从而不让车辆在原地打滑
         * 
         * 在普通弯道，需要抓地过弯时，最好是开放的差速器
         * 但是应对度数较高的弯道时，开放的差速器有可能使在弯中的车子动力出不来，因为动力基本都分配在外轮了
         * 此时，需要部分锁死，来将动力分配一部分给内轮，但是需要注意，锁死率高了容易甩，低了容易没有动力（虽然更稳）
         * 
         * 朝着抓地的方向看
         * 让轮胎始终工作在最佳滑移率区间是最理想的
         * 没有加入差速器和扭矩分配前，两个轮子扭矩相等，过弯时必定存在内外轮转速差
         * 加入差速器后和扭矩分配后，两个轮子随转速差分配不同扭矩
         * 此时出弯容易失去一点动力，轮胎滑移率始终小于最佳滑移率（即轮胎抓地力没有用尽）
         * 此时需要一点锁死，将外轮的扭矩分配一点给里面的轮子，让内外两个轮胎的滑移率都达到最佳滑移率
         * 
         * 
         * 
         * 
         * 
         * 关于处理刹车力矩的差速器：
         * 大前提仍然是希望轮子工作在最佳的滑移率区间
         * 刹车lsd 锁死时：
         * 直线全力刹车，内外侧轮子几乎不会出现转速差，将车辆在弯中刹车抽象出来
         * 就是一侧抓地，一侧不抓地，抓地的一侧会受到更多的摩擦力，所以车子会以抓地一侧的轮子为点，进行旋转
         * 刹车lsd 完全打开时：
         * 直线全力刹车，滑的一侧会比不滑一侧先抱死，即滑的一侧轮速会比不滑一侧的转速下降的快很多
         * 那么刹车力矩可以分配给转速少的一侧，即抱死的一侧
         * 不滑的一侧只能分配到很少的刹车力矩，防止车辆失控
         * 
         * 带入弯中， 如果锁死，容易推头；
         * 不锁死，不容易推，但是制动力表现会差一点（虽然也会推，但是推的没有那么厉害）
         * 
         * 
         * 
         */


        /*以下是旧的函数
            //
            //foreach (WheelCollider wheel in throttleWheels)//施加动力 motorTorque 和手刹(后轮)
            //{
            //    wheel.ConfigureVehicleSubsteps(1f, 5, 100);
            //    if (inputManager.handBrake)//判断是否手刹
            //    {
            //        wheel.brakeTorque = strenghCoefficient * Time.deltaTime * 2;
            //        //Debug.Log("space pressed");  
            //    }
            //    else
            //    {
            //        wheel.brakeTorque = 0;


            //        if (gear > 0)//档位判断
            //        {
            //            if (gear == 1)
            //            {
            //                wheel.motorTorque = strenghCoefficient * Time.deltaTime * inputManager.throttle;//1-6为前进
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
            //            wheel.motorTorque = -strenghCoefficient * Time.deltaTime * inputManager.throttle;//0 为倒车档,后退
            //        }

            //    }

            //}
            //foreach (WheelCollider wheel in brakeWheels)//刹车前轮
            //{
            //    wheel.ConfigureVehicleSubsteps(1f, 5, 100);
            //    wheel.brakeTorque = brakeCoefficient * Time.deltaTime * inputManager.brake * brakeBias;
            //    lightingManager.BrakeLight(inputManager.sPressed);
            //}
            //foreach (WheelCollider wheel in throttleWheels)//添加后轮脚刹
            //{
            //    //注意这个地方不能直接赋值,不然会覆盖掉手刹的数值
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


    public float Length(Vector3 vector3)//3维向量 浮点, 取模
    {
        float length = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
        return length;
    }

    public virtual void ReloadCar()//重新加载车辆至当前位置上方
    {
        if (inputManager.enterPressed)//
        {
            transform.rotation = Quaternion.identity;
            transform.position = transform.position + new Vector3(0, 2, 0);//将当前车辆位置往上加2m
            rigidBody.velocity = new Vector3(0f, 0f, 0f);
        }
    }
    public virtual void ResetCar()//将车子送回到世界坐标原点
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3(0, 2, 0);
            rigidBody.velocity = new Vector3(0f, 0f, 0f);
        }
    }
    public float Clamp(float input, float downLimit, float upLimit)//Clamp 函数,将一个变化的值钳制在[downLimit,upLimit]之内
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
    public float Gear(float gear, bool gearUp, bool gearDown)//档位本身,升档键,降档键
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
    public void Shift(WheelCollider wheel, float gear, float limitGear, float limitSpeed, float RPMSum)//档位变更,速度限制||轮子(wheel collider), 当前档位,要限制速度的档位,限制的速度
    {       //在面对 motortorque 较大的时候,不能直接限制速度(会出现升档后,提速依然很快的现象),
            //应该要减少一点motortorque,让速度慢慢提升(可以让速度掉一点下来造成扭矩不够用的情况,但是要适量,最好还是减少到一个能慢慢加速的情况)
            //一般低档位,轮速会比车速快,因为torque产生的力已经比轮胎最大摩擦力还要大,造成打滑;高档位时,轮胎摩擦力一般比较富余,此时轮速应尽量匹配车速
        if (gear == limitGear)
        {
            /*
            //这个是旧的
            //wheel.motorTorque = throttleCoefficient * Time.deltaTime * inputManager.throttle * tcMultiplier;


            //这个是新的，先加入完全开放的差速器，让轮子由它们的转速自由分配扭矩,
            //这样，当其中一边的轮子打滑时，打滑的轮子会分得绝大多数的扭矩，而没有打滑的轮子会分得很少的扭矩
            //然后再加入差速锁，这样打滑的轮子分配的扭矩会少一点，从而将更多的扭矩分配至没有打滑的轮子

            //试试从滑移率入手，使用车速反向计算轮速rpm
            */


            //通过转速差来分配扭矩
            float rpm = Mathf.Abs(wheel.rpm);
            float LsdThrottleMultiplier = rpm / RPMSum;

            LsdThrottleMultiplier = LsdMultiplier(backLsdThrottleLockedRatio, rpm, RPMSum, "throttle");
            wheel.motorTorque = 2 * inputManager.throttle * 2 * torque * LsdThrottleMultiplier * tcMultiplier;//实现完全开放的差速器

            Debug.Log("throttle differential working");

            //if (Length(rigidBody.velocity) * 3.6f >= limitSpeed)
            ////这个是限速
            //{
            //    rigidBody.velocity = rigidBody.velocity.normalized * limitSpeed / 3.6f;
            //}
        }
    }

    private float LsdMultiplier(float LockedRatio, float rpm, float RPMSum, string throttle_or_brake)
    //LockedRatio填入 四个锁死率的其中之一，rpm==当前车轮的转速，rpmsum 这一侧车轮的总转速， side 中填入 throttle 或者 brake
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
                    //乘 0.5 的原因是，当lockratio =1 时，扭矩会因为差速器完全锁死而全部被分配至没有打滑的轮子上，这在现实中是不可能发生的
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
    //获得由 tuning 物体中 slidertuning 脚本传递而来的4个锁死率
    {
        frontLsdBrakeLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[0];
        frontLsdThrottleLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[1];
        backLsdBrakeLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[2];
        backLsdThrottleLockedRatio = Tuning.GetComponent<SliderTuning>().lsd[3];
    }

    public List<float> collisionPerformance = new List<float> { 1, 1, 1, 1, 1, 1 };
    /*
    public virtual List<float> PerformanceAfterCollision()
    //汽车因发生碰撞而产生的性能损失
    {
        float max = 25000f * Mathf.Sqrt(2);
        Vector2 vector2 = new Vector2(collisionDIr.x, collisionDIr.z);
        if (collisionDIr.x > 0)
        {
            if (collisionDIr.z > 0)
            {
                Debug.Log("右前方");
                collisionPerformance[2] += vector2.magnitude / max;
                //在这个里面设置没用，要到WheelColliderConfigure里面去设置
                //WheelFrictionCurve wheelFrictionCurve = wheelColliders[2].forwardFriction;
                //wheelFrictionCurve.stiffness *= 0.1f;                      //*= 1-collisionPerformance[2];

                //wheelColliders[2].forwardFriction = wheelFrictionCurve;

            }
            else if (collisionDIr.z < 0)
            {
                //collisionPerformance[0] = 
                //Debug.Log("右后方");

                collisionPerformance[0] += vector2.magnitude / max;

            }
        }
        else if (collisionDIr.x < 0)
        {
            if (collisionDIr.z > 0)
            {
                //Debug.Log("左前方");
                collisionPerformance[3] += vector2.magnitude / max;


            }
            else if (collisionDIr.z < 0)
            {
                //Debug.Log("左后方");
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

        collisionDIr = collision.impulse;//获得冲量的方向
        collisionDIr = -transform.InverseTransformDirection(collisionDIr);
        //将冲量的方向变换到车子的局部坐标，加上负号表示，指那个方向      
        //Debug.Log(collisionDIr);
        float max = 25000f * Mathf.Sqrt(2);

        Vector2 vector2 = new Vector2(collisionDIr.x, collisionDIr.z);
        if (collisionDIr.z > 0 && collisionDIr.x > 0)
        {
            //Debug.Log("右前方");
            collisionPerformance[2] += vector2.magnitude / max;
            //在这个里面设置没用，要到WheelColliderConfigure里面去设置
            //WheelFrictionCurve wheelFrictionCurve = wheelColliders[2].forwardFriction;
            //wheelFrictionCurve.stiffness *= 0.1f;                      //*= 1-collisionPerformance[2];

            //wheelColliders[2].forwardFriction = wheelFrictionCurve;

        }
        if (collisionDIr.z < 0 && collisionDIr.x > 0)
        {
            //collisionPerformance[0] = 
            //Debug.Log("右后方");

            collisionPerformance[0] += vector2.magnitude / max;

        }

        if (collisionDIr.x < 0 && collisionDIr.z > 0)
        {
            //Debug.Log("左前方");
            collisionPerformance[3] += vector2.magnitude / max;
        }

        if (collisionDIr.z < 0 && collisionDIr.x < 0)
        {
            //Debug.Log("左后方");
            collisionPerformance[1] += vector2.magnitude / max;
        }


    }
}
        


    



