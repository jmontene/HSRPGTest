using UnityEngine;
using System.Collections;

public class TP_Animator : MonoBehaviour {
	
	public enum Direction{
		Stationary, Forward, Backward, Left, Right,
		LeftForward, RightForward, LeftBackward, RightBackward
	}
	
	public enum CharacterState{
		Idle, Running, WalkingBackwards, StrafingLeft, StrafingRight,
		Jumping, Falling, Landing, Climbing, Sliding, Using, Dead,
		ActionLocked
	}

	public static TP_Animator Instance;
	
	public Direction MoveDirection {get; set;}
	public CharacterState State {get; set;}

	void Awake() {
		Instance = this;
	}
	
	void Update () {
		UpdateAnimator();
	}
	
	public void DetermineCurrentMoveDirection(){
		var forward = TP_Motor.Instance.MoveVector.z > 0;
		var backward = TP_Motor.Instance.MoveVector.z < 0;
		var left = TP_Motor.Instance.MoveVector.x < 0;
		var right = TP_Motor.Instance.MoveVector.x > 0;
		
		if(forward){
			if(left) MoveDirection = Direction.LeftForward;
			else if (right) MoveDirection = Direction.RightForward;
			else MoveDirection = Direction.Forward;
		}else if(backward){
			if(left) MoveDirection = Direction.LeftBackward;
			else if(right) MoveDirection = Direction.RightBackward;
			else MoveDirection = Direction.Backward;
		}else if(left) MoveDirection = Direction.Left;
		else if(right) MoveDirection = Direction.Right;
		else MoveDirection = Direction.Stationary;
		
		TP_Controller.Animator.SetInteger("Direction",(int) MoveDirection);
	}
	
	void UpdateAnimator(){
		TP_Controller.Animator.SetFloat ("VerticalVelocity",TP_Motor.Instance.VerticalVelocity);
		TP_Controller.Animator.SetBool ("Grounded",TP_Controller.CharacterController.isGrounded);
	}
}
