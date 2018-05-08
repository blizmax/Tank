using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeSample : MonoBehaviour {

	// Use this for initialization
	void Start () {
        iTween.ShakePosition(gameObject, iTween.Hash("x", 2, "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
