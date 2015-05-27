using UnityEngine;
using System.Collections;

public class TP_Controller : MonoBehaviour {

	public static CharacterController CharacterController;
	public static Animator Animator;
	public static TP_Controller Instance;
	
	void Awake () {
		CharacterController = GetComponent<CharacterController>();
		Animator = GetComponent<Animator>();
		Instance = this;
		TP_Camera.UseExistingOrCreateNewCamera();
	}

	void Update () {
		if(Camera.main == null)
			return;
			
		GetLocomotionInput ();
		HandleActionInput();
		TP_Motor.Instance.UpdateMotor();
	}
	
	//Take player input and create vector
	void GetLocomotionInput (){
		var deadZone = 0.1f;
		
		TP_Motor.Instance.VerticalVelocity = TP_Motor.Instance.MoveVector.y;
		TP_Motor.Instance.MoveVector = Vector3.zero;
		float vAxis = Input.GetAxis ("Vertical");
		float hAxis = Input.GetAxis ("Horizontal");
		
		if(vAxis > deadZone || vAxis < -deadZone){
			TP_Motor.Instance.MoveVector += new Vector3(0f,0f,vAxis);
		}
		
		if(hAxis > deadZone || hAxis < -deadZone){
			TP_Motor.Instance.MoveVector += new Vector3(hAxis,0f,0f);
		}
		
		TP_Animator.Instance.DetermineCurrentMoveDirection();
	}
	
	void HandleActionInput(){
		if(Input.GetButton("Jump")){
			Jump ();
		}
	}
	
	void Jump(){
		TP_Motor.Instance.Jump();
	}
	
}
