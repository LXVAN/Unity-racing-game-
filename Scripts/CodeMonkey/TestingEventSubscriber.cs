using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestingEventSubscriber : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TestingEvent testingEvent = GetComponent<TestingEvent>();
        testingEvent.OnSpacePressed += TestingEvent_OnspacePressed;
    }

    private void TestingEvent_OnspacePressed(object sender, TestingEvent.OnSpacePressedEventArgs e)
    {
        Debug.Log("space pressed!" + e.spaceCount);
        TestingEvent testingEvent = GetComponent<TestingEvent>();
        testingEvent.OnSpacePressed -= TestingEvent_OnspacePressed;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
