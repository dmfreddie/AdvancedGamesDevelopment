﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour {

	float deltaTime = 0.0f;
	Text fpsText;
	
	void Start()
	{
		fpsText = GetComponent<Text>();
	}

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		fpsText.text = text;

		if(Input.GetKeyDown(KeyCode.F6))
			fpsText.enabled = !fpsText.enabled;
	}
}