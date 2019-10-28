using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rider : MonoBehaviour {

	public bool moving = false;

	public float distance = 0.0f;

	public float time = 0.0f;

	public float total_time = 10.0f;

	public Spline spline;

	public int global_index = 0;

	public Camera cam;

	void Awake() {
		spline = GameObject.Find("Spline").GetComponent<Spline>();
	}

	// Update is called once per frame
	void Update () {
		if(moving) {
			if(distance == 0.0f) {
				spline.CalculateDistance();
			}
			float delta_time = 0.1f/6;
			time += delta_time;
			distance = (time/total_time) * 1.0f;
			// int index = spline.indexOfDist(distance);
			int index = global_index++;
			GameObject indexed_control_point = spline.points[index/200];
			Vector3 indexed_curve_point = spline.curvePoints[index];
			gameObject.transform.position = indexed_curve_point;

			gameObject.transform.rotation = Quaternion.Slerp(spline.points[index/200].transform.rotation, spline.points[index/200+1].transform.rotation, ((index%200)+1f)/200.0f);
			cam.transform.position = gameObject.transform.position;
			cam.transform.rotation = gameObject.transform.rotation;
		}
	}

}
