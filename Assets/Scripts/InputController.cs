using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour 
{

	public Button buttonA;
	public Button buttonB;
	public Button buttonStart;
	public Button buttonSelect;
	public Axis xAxis1;
	public Axis yAxis1;

	void Start () 
	{
		buttonA = new Button ("A Button");
		buttonB = new Button("B Button");
		buttonStart = new Button("Start");
		buttonSelect = new Button("Select");
		xAxis1 = new Axis ("D-Pad Horizontal");
		yAxis1 = new Axis ("D-Pad Vertical");
	}

	void Update () 
	{
		buttonA.UpdateButton ();
		buttonB.UpdateButton();
		buttonStart.UpdateButton();
		buttonSelect.UpdateButton();
		xAxis1.UpdateAxis ();
		yAxis1.UpdateAxis ();
	}
}

public class Button 
{
	public string buttonName;
	public bool isPressed = false;
	public bool isHeld = false;
	public bool isReleased = false;
	public float durration = 0;

	public Button(string buttonName) 
	{
		this.buttonName = buttonName;
	}

	public void UpdateButton() 
	{
		isReleased = false;

		if (Input.GetButton (buttonName)) 
		{
			isPressed = (durration == 0) ? true : false;
			isHeld = true;
			durration += 1;
		} 
		else if(isHeld && !Input.GetButton (buttonName)) 
		{
			durration = 0;
			isPressed = false;
			isHeld = false;
			isReleased = true;
		}

		if (durration > 1) 
		{
			isPressed = false;
		}
	}
}

public class Axis 
{
	public string axisName;
	public bool isPressed = false;
	public bool isHeld = false;
	public bool isReleased = false;
	public float durration = 0;
	public float direction = 0;

	public Axis(string axisName) 
	{
		this.axisName = axisName;
	}

	public void UpdateAxis() 
	{
		isReleased = false;

		if (Mathf.Abs(Input.GetAxisRaw (axisName)) > 0.3) 
		{
			isPressed = (durration == 0) ? true : false;
			isHeld = true;
			durration += 1;
			direction = Mathf.Sign (Input.GetAxisRaw (axisName));
		} 
		else if(isHeld && !(Mathf.Abs(Input.GetAxisRaw (axisName)) > 0.3))  
		{
			durration = 0;
			direction = 0;
			isPressed = false;
			isHeld = false;
			isReleased = true;
		}

		if (durration > 1) 
		{
			isPressed = false;
		}
	}
}
