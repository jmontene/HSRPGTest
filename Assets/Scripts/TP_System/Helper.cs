using UnityEngine;

public static class Helper{

	public struct ClipPlanePoints{
		public Vector3 UpperLeft;
		public Vector3 UpperRight;
		public Vector3 LowerLeft;
		public Vector3 LowerRight;
	}

	public static float ClampAngle(float angle, float min, float max){
		do{
			if(angle < -360) angle += 360;
			if(angle > 360) angle -= 360;
		}while (angle < -360 || angle > 360);
		
		return Mathf.Clamp (angle,min,max);
	}
	
	public static ClipPlanePoints ClipPlaneAtNear(Vector3 pos){
		var clipPlanePoints = new ClipPlanePoints();
		
		if(Camera.main == null) return clipPlanePoints;
		
		var transform = Camera.main.transform;
		var halfFOV = (Camera.main.fieldOfView / 2) * Mathf.Deg2Rad;
		var aspect = Camera.main.aspect;
		var distance = Camera.main.nearClipPlane;
		var height = distance * Mathf.Tan(halfFOV);
		var width = height * aspect;
		var right = transform.right * width;
		var up = transform.up * height;
		var forward = transform.forward * distance;
		
		clipPlanePoints.LowerRight = pos + right - up + forward;
		clipPlanePoints.LowerLeft = pos - right - up + forward;
		clipPlanePoints.UpperRight = pos + right + up + forward;
		clipPlanePoints.UpperLeft = pos - right + up + forward;
		
		return clipPlanePoints;
	}
	
}

