﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Security.Permissions;
using UnityEngine;

public class Rider : MonoBehaviour {

	public bool ridingMode = false;

	public double distance = 0.0;


	public Spline spline;

	public int global_index = 0;

    //vive space
    public CommonSpace space;

    public double gravity = 0.40;
    
    public double velocity = 0.02;
	public double minVelocity = 0.2;
    
    public bool scaled = false;
    public float scale = 0.6f;

    public int last_point_index = 0;

	public Quaternion offset;
	public Vector3 positionOffset;

	public Quaternion originalRotation;
	public Vector3 originalPosition;
	public Vector3 originalScale;

	private GameObject tempObject;

    void Awake() {
		spline = GameObject.Find("Spline").GetComponent<Spline>();
	}

	// FixedUpdate is called every Time.fixedDeltaTime
	void FixedUpdate () {
		if(ridingMode) {
			// Consts to reduce built in access
			Transform spaceTransform = space.transform;
			GameObject gameObj  = gameObject;
			
			// Scale Vive Input for coaster
            if (!scaled){
				originalScale = new Vector3(1.0f, 1.0f, 1.0f);
				originalPosition = new Vector3(0.0f,0.0f,0.0f) + spaceTransform.position;
				originalRotation = Quaternion.identity * spaceTransform.rotation;
//				spaceTransform.Translate(-positionOffset.x, -positionOffset.y, -positionOffset.z);
	            spaceTransform.rotation = Quaternion.identity;
	            spaceTransform.Rotate(-offset.eulerAngles.x,-offset.eulerAngles.y,-offset.eulerAngles.z);
	            spaceTransform.localScale = new Vector3(1f,1f,1f) * scale;
                scaled = true;
            }
//			if(distance == 0.0) {
//				spline.CalculateDistance();
//			}
			if(last_point_index == spline.curvePoints.Count - 1)
				return;
			// 2 Point calcs
			Vector3 lastpoint, nextpoint;
			if(last_point_index > spline.curvePoints.Count)
				last_point_index = spline.curvePoints.Count-1;
			lastpoint = spline.curvePoints[last_point_index];
			if (last_point_index + 1 >= spline.curvePoints.Count) {
				// ridingMode = false;
				last_point_index = spline.curvePoints.Count - 2;
			}
			nextpoint = spline.curvePoints[last_point_index + 1];
			double y_diff = lastpoint.y - nextpoint.y;
			Vector2 lastpointxz, nextpointxz;
			lastpointxz = new Vector2(lastpoint.x, lastpoint.z);
			nextpointxz = new Vector2(nextpoint.x, nextpoint.z);
			double xz_dist = Vector3.Distance(lastpointxz, nextpointxz);
			
			// 3 point calcs
			Vector3 prevpoint;
			if (last_point_index == 0)
				last_point_index = 1;
			prevpoint = spline.curvePoints[last_point_index - 1];
			double y_diff3 = prevpoint.y - nextpoint.y;
			Vector2 prevpointxz = new Vector2(prevpoint.x, prevpoint.z);
			double xz_dist3 = Vector3.Distance(prevpointxz, nextpointxz);
			
			float delta_time = Time.fixedDeltaTime;
			double angle = Math.Atan2(y_diff, xz_dist);
			// double angle = Math.Atan2(y_diff3, xz_dist3);

			// Acceleration physics
			double acceleration = (angle / 1.57) * gravity;
			if(velocity < minVelocity)
				velocity = minVelocity;
			distance += (velocity + (acceleration * delta_time / 2)) * delta_time;
			velocity += acceleration * delta_time;
			if(velocity < minVelocity)
				velocity = minVelocity;
			
			// Movement
			int index = spline.indexOfDist(distance);
			// int index = global_index++;
			last_point_index = index;
			Vector3 indexed_curve_point = spline.curvePoints[index];
			gameObj.transform.position = indexed_curve_point;
			Quaternion slerp;
			if(index/200 < spline.points.Count - 1)
				slerp = Quaternion.Slerp(spline.points[index/200].transform.rotation, spline.points[index/200+1].transform.rotation, ((index%200)+1f)/200.0f);
			else
				slerp = spline.points[index/200].transform.rotation;
			if (index + 1 < spline.curvePoints.Count)
				slerp = Quaternion.LookRotation(spline.curvePoints[index + 1] - spline.curvePoints[index], Vector3.up);
			else {
				slerp = gameObj.transform.rotation;
			}
			Quaternion diff = slerp * Quaternion.Inverse(gameObj.transform.rotation); 
//			Quaternion temp = Quaternion.FromToRotation(gameObj.transform.rotation, Quaternion.Slerp(spline.points[index/200].transform.rotation, spline.points[index/200+1].transform.rotation, ((index%200)+1f)/200.0f));
			// Quaternion relative = gameObj.transform.rotation * Quaternion.Inverse(slerp);
			gameObj.transform.rotation = slerp;

			// Move Vive input
			spaceTransform.position = gameObj.transform.position + new Vector3(0.0f, scale, 0.0f);
			spaceTransform.rotation = diff * spaceTransform.rotation;
			spaceTransform.Translate(-positionOffset.x*scale, -positionOffset.y*scale, -positionOffset.z*scale);
//			spaceTransform.rotation = gameObj.transform.rotation;

			
                    // Transform eye = spaceTransform.Find("Vive Camera (eye)");
			
                    // // Get current head heading in scene (y-only, to avoid tilting the floor)
                    // float offsetXAngle = eye.rotation.eulerAngles.x;
                    // float offsetAngle = eye.rotation.eulerAngles.y;
                    // float offsetZAngle = eye.rotation.eulerAngles.z;
                    // // Now rotate CameraRig in opposite direction to compensate
                    // space.transform.rotation = gameObj.transform.rotation;
                    // space.transform.Rotate(-offsetXAngle, -offsetAngle, -offsetZAngle);
		}
		else if(scaled) {
			scaled = false;
			Transform spaceTransform = space.transform;
			spaceTransform.localScale = originalScale;
			spaceTransform.position = originalPosition;
			spaceTransform.rotation = originalRotation;
			last_point_index = 0;
			distance = 0.0;
			velocity = 0.02;
		}
	}

}
