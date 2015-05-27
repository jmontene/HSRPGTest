using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour {

	public static TP_Camera Instance;
	public Transform TargetLookAt;
	
	public float Distance = 5f;
	public float DistanceMin = 3f;
	public float DistanceMax = 10f;
	public float X_mouseSensitivity = 5f;
	public float Y_mouseSensitivity = 5f;
	public float MouseWheel_sensitivity = 5f;
	public float Y_MinLimit = -40f;
	public float Y_MaxLimit = 80f;
	public float DistanceSmooth = 0.05f;
	public float DistanceResumeSmooth = 1f;
	public float XSmooth = 0.05f;
	public float YSmooth = 0.05f;
	public float OcclusionDistanceStep = 0.5f;
	public int MaxOcclusionChecks = 10;
	
	private float startDistance = 0f;
	private float mouseX = 0f;
	private float mouseY = 0f;
	private float desiredDistance = 0f;
	private float velDistance = 0f;
	private float velX = 0f;
	private float velY = 0f;
	private float velZ = 0f;
	private Vector3 desiredPosition = Vector3.zero;
	private Vector3 position = Vector3.zero;
	private float preOccludedDistance = 0f;
	private float distanceSmooth = 0f;
	
	void Awake(){
		Instance = this;
	}
	
	void Start(){
		Distance = Mathf.Clamp (Distance,DistanceMin,DistanceMax);
		startDistance = Distance;
		Reset ();
	}

	void LateUpdate(){
		if(TargetLookAt == null)
			return;
	
		HandlePlayerInput();
		
		var count = 0;
		do{
			CalculateDesiredPosition();
			++count;
			
		}while(CheckIfOccluded (count));
		
		UpdatePosition();
	}
	
	void HandlePlayerInput(){
		var deadZone = 0.01f;
		mouseX += Input.GetAxis ("Mouse X") * X_mouseSensitivity;
		mouseY -= Input.GetAxis ("Mouse Y") * Y_mouseSensitivity;
		mouseY = Helper.ClampAngle(mouseY,Y_MinLimit,Y_MaxLimit);
		
		float mouseWheel = Input.GetAxis ("Mouse ScrollWheel");
		if(mouseWheel < -deadZone || mouseWheel > deadZone){
			mouseWheel *= MouseWheel_sensitivity;
			desiredDistance = Mathf.Clamp (Distance-mouseWheel,DistanceMin,DistanceMax);
			preOccludedDistance = desiredDistance;
			distanceSmooth = DistanceSmooth;
		}
	}

	void CalculateDesiredPosition(){
		ResetDesiredDistance();
		Distance = Mathf.SmoothDamp(Distance,desiredDistance,ref velDistance,distanceSmooth);
		desiredPosition = CalculatePosition(mouseY,mouseX,Distance);
	}
	
	Vector3 CalculatePosition(float rotationX,float rotationY,float distance){
		Vector3 direction = new Vector3(0f,0f,-distance);
		Quaternion rotation = Quaternion.Euler(rotationX,rotationY,0f);
		return (TargetLookAt.position + (rotation * direction));
	}
	
	bool CheckIfOccluded(int count){
		var isOccluded = false;
		var nearestDistance = CheckCameraPoints (TargetLookAt.position,desiredPosition);
		if(nearestDistance != -1){
			if(count < MaxOcclusionChecks){
				Distance -= OcclusionDistanceStep;
				isOccluded = true;
				//Hardcoded. Personal tests required
				if(Distance < 0.25f) Distance = 0.25f;
			}else{
				Distance = nearestDistance - Camera.main.nearClipPlane;
			}
			desiredDistance = Distance;
			distanceSmooth = DistanceResumeSmooth;
		}
		
		return isOccluded;
	}
	
	float CheckCameraPoints(Vector3 from, Vector3 to){
		var nearestDistance = -1f;
		RaycastHit hitInfo;
		
		Helper.ClipPlanePoints clipPlanePoints = Helper.ClipPlaneAtNear(to);
		
		//Draw lines in editor to make it easier to visualize
		Debug.DrawLine(from,to + transform.forward * -Camera.main.nearClipPlane, Color.red);
		Debug.DrawLine(from,clipPlanePoints.UpperLeft);
		Debug.DrawLine(from,clipPlanePoints.UpperRight);
		Debug.DrawLine(from,clipPlanePoints.LowerLeft);
		Debug.DrawLine(from,clipPlanePoints.LowerRight);
		
		Debug.DrawLine (clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight);
		Debug.DrawLine (clipPlanePoints.UpperRight, clipPlanePoints.LowerRight);
		Debug.DrawLine (clipPlanePoints.LowerRight, clipPlanePoints.LowerLeft);
		Debug.DrawLine (clipPlanePoints.LowerLeft, clipPlanePoints.UpperLeft);
		
		if(Physics.Linecast(from, clipPlanePoints.UpperLeft, out hitInfo) && hitInfo.collider.tag != "Player")
			nearestDistance = hitInfo.distance;
		if(Physics.Linecast(from, clipPlanePoints.LowerLeft, out hitInfo) && hitInfo.collider.tag != "Player")
			if(hitInfo.distance < nearestDistance || nearestDistance == -1) nearestDistance = hitInfo.distance;
		if(Physics.Linecast(from, clipPlanePoints.UpperRight, out hitInfo) && hitInfo.collider.tag != "Player")
			if(hitInfo.distance < nearestDistance || nearestDistance == -1) nearestDistance = hitInfo.distance;
		if(Physics.Linecast(from, clipPlanePoints.LowerRight, out hitInfo) && hitInfo.collider.tag != "Player")
			if(hitInfo.distance < nearestDistance || nearestDistance == -1) nearestDistance = hitInfo.distance;
		if(Physics.Linecast(from, to + transform.forward * -Camera.main.nearClipPlane, out hitInfo) && hitInfo.collider.tag != "Player")
			if(hitInfo.distance < nearestDistance || nearestDistance == -1) nearestDistance = hitInfo.distance;
		
		return nearestDistance;
	}
	
	void ResetDesiredDistance(){
		if(desiredDistance < preOccludedDistance){
			var pos = CalculatePosition(mouseY, mouseX, preOccludedDistance);
			var nearestDistance = CheckCameraPoints(TargetLookAt.position,pos);
			if(nearestDistance == -1 || nearestDistance > preOccludedDistance){
				desiredDistance = preOccludedDistance;
			}
		}
	}
	
	void UpdatePosition(){
		position = transform.position;
		position.x = Mathf.SmoothDamp(position.x,desiredPosition.x,ref velX, XSmooth);
		position.y = Mathf.SmoothDamp(position.y,desiredPosition.y,ref velY, YSmooth);
		position.z = Mathf.SmoothDamp(position.z,desiredPosition.z,ref velZ, XSmooth);
		transform.position = position;
		transform.LookAt (TargetLookAt);
	}
	
	public void Reset(){
		mouseX = 0f;
		mouseY = 10f;
		Distance = startDistance;
		desiredDistance = Distance;
		preOccludedDistance = Distance;
	}
	
	public static void UseExistingOrCreateNewCamera(){
		GameObject tempCamera;
		GameObject targetLookAt;
		TP_Camera myCamera;
		
		if(Camera.main != null) tempCamera = Camera.main.gameObject;
		else{
			tempCamera = new GameObject("Main Camera");
			tempCamera.AddComponent <Camera>();
			tempCamera.tag = "MainCamera";
		}
		
		tempCamera.AddComponent <TP_Camera>();
		myCamera = tempCamera.GetComponent<TP_Camera>();
		
		targetLookAt = GameObject.Find ("targetLookAt") as GameObject;
		if(targetLookAt == null){
			targetLookAt = new GameObject("targetLookAt");
			targetLookAt.transform.position = Vector3.zero;
		}
		
		myCamera.TargetLookAt = targetLookAt.transform; 
	}
}
