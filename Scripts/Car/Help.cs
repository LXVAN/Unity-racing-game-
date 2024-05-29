using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    public InputManager InputManager;
    public GameObject GameObject;
    private bool a = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.f1Pressed)
        {
            a = !a;
        }
        if (a)
        {
            GameObject.GetComponent<TextMesh>().text = "键位"+"\n" +
                                                       "Enter 重载车辆" + "\n" + 
                                                       "Q 车灯开关" + "\n" +
                                                       "↑↓ 档位切换" + "\n" +
                                                       "R 重置轮胎参数" + "\n" +
                                                       "Tab 重置车辆" + "\n" + 
                                                       "K 旋转视角" + "\n";
        }
        else
        {
            GameObject.GetComponent<TextMesh>().text = "Press F1 to check help";
        }
    }
}
