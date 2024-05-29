using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEvent : MonoBehaviour
{

    public event EventHandler <OnSpacePressedEventArgs> OnSpacePressed;
    public class OnSpacePressedEventArgs : EventArgs
    {
        public int spaceCount;
    }
    private int spaceCount;


    // Start is called before the first frame update
    void Start()
    {
        //OnspacePressed += Testing_OnSpacePressed;

    }

    // Update is called once per frame
    void Update()
    {
        Running();
    }
    

   
    //private static void Testing_OnSpacePressed(object sender,EventArgs e)
    //{
    //    Debug.Log("Space pressed!");
    //}


    private void Running()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceCount++;
            if (OnSpacePressed != null) OnSpacePressed(this, new OnSpacePressedEventArgs { spaceCount = spaceCount});
        }
    }

}
