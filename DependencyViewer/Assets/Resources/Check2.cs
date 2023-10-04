using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Check2 : MonoBehaviour
{
    public Check1 check1;
    public void Check()
    {
        Debug.Log("Check2");
        check1.Check();
    }
}
