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

	void Awake() {
		spline = GameObject.Find("Spline").GetComponent<Spline>();
	}

	// Update is called once per frame
	void Update () {
		if(moving) {
			float delta_time = 0.1f/6;
			time += delta_time;
			distance = (time/total_time) * 1.0f;
			int index = spline.indexOfDist(distance);
			index = global_index++;
			Debug.Log("Index: " + index + " :: " + (index/200));
			GameObject indexed_control_point = spline.points[index/200];
			Vector3 indexed_curve_point = spline.curvePoints[index];
			gameObject.transform.position = indexed_curve_point;

			gameObject.transform.rotation = Quaternion.Slerp(spline.points[index/200*200].transform.rotation, spline.points[index/200*200+1].transform.rotation, (time/total_time)/spline.points.Count);

		}
	}

}
