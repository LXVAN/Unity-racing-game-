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
            GameObject.GetComponent<TextMesh>().text = "��λ"+"\n" +
                                                       "Enter ���س���" + "\n" + 
                                                       "Q ���ƿ���" + "\n" +
                                                       "���� ��λ�л�" + "\n" +
                                                       "R ������̥����" + "\n" +
                                                       "Tab ���ó���" + "\n" + 
                                                       "K ��ת�ӽ�" + "\n";
        }
        else
        {
            GameObject.GetComponent<TextMesh>().text = "Press F1 to check help";
        }
    }
}
