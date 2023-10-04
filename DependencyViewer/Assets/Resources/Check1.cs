using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check1 : MonoBehaviour
{
    public void Check()
    {
        Debug.Log("Check1");
        Check2.CheckStatic();
    }
}
