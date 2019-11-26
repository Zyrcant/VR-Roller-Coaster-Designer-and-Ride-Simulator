using System;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    public const int numSegments = 200;
    public List<GameObject> points = new List<GameObject>();
    //TODO: interpolate distance using these curvepoints
    public List<Vector3> curvePoints = new List<Vector3>();
    public List<double> curvePointsDist = new List<double>();

    public void RedrawSpline()
    {
        //destroys each line and redraws
        foreach (Transform child in GameObject.Find("Lines").transform) {
            Destroy(child.gameObject);
        }

        //Draw the Catmull-Rom spline between the points
        if (points.Count >= 2) {
            for (int i = 0; i < points.Count - 1; i++) {
                CalculateCatmullRom(i);
            }
            CalculateQuaternions();
        }
    }

    public void RedrawSplineForAdd(GameObject target)
    {
        int index = points.IndexOf(target);

        if (points.Count >= 2) {
            for (int i = -3; i < -1; i++) {
                if (index + i > -1 && index + i < points.Count - 1) {
                    // CalculateCatmullRom(index + i);
                    RecalculateCatmullRom(index + i);
                }
            }

            CalculateCatmullRom(index - 1);
            // CalculateDistance();
            CalculateQuaternions();
        }
    }

    // Calculates which lines need to be redrawn for the spline 
    public void RedrawSplineForModify(GameObject target)
    {
        int index = points.IndexOf(target);
        points[index].transform.position = target.transform.position;

        //Draw the Catmull-Rom spline between the points
        if (points.Count >= 2) {
            for (int i = -2; i <= 2; i++) {
                if (index + i > -1 && index + i < points.Count - 1)
                    RecalculateCatmullRom(index + i);
            }
            // CalculateDistance();
            CalculateQuaternions();
        }
    }

    // Recalculates Catmull Rom for deletion of Object
    public void RedrawSplineForDelete(GameObject target)
    {
        int index = points.IndexOf(target);
        

        points.Remove(target);
        int lastIndex = 0;
        if (points.Count >= 2) {
            for (int i = -2; i <= 2; i++) {
                if (index + i > -1 && index + i < points.Count - 1)
                    RecalculateCatmullRom(index + i);
            }
            CalculateQuaternions();
        }

        if (points.Count >= 1) {
            Transform lines = GameObject.Find("Lines").transform;
            lastIndex = Math.Min(points.Count - index - 1, 2);
            for (int x = ((index + lastIndex) * (numSegments)); x < ((index + lastIndex + 1) * (numSegments)); x++) {
                DestroyImmediate(lines.GetChild((index + lastIndex) * (numSegments)).gameObject);
            }
            for (int x = ((index + lastIndex) * (numSegments)); x <= ((index + lastIndex + 1) * (numSegments)); x++) {
                curvePoints.Remove(curvePoints[((index + lastIndex) * (numSegments))]);
            }
        }
        // CalculateDistance();
    }

    void CalculateCatmullRom(int index)
    {
        Vector3 p0, p1, p2, p3;

        p1 = points[index].transform.position;
        p2 = points[index + 1].transform.position;

        if (index > 0)
            p0 = points[index - 1].transform.position;
        else
            p0 = p1 * 2 - p2;

        if (index < points.Count - 2)
            p3 = points[index + 2].transform.position;
        else
            p3 = p2 * 2 - p1;

        Vector3 lastPos = p1;

        curvePoints.Add(lastPos);
        for (int i = 1; i <= numSegments; i++) {
            float t = i / (float) numSegments;
            Vector3 newPos = GetCatmullRom(t, p0, p1, p2, p3);

            //Draw this line segment
            LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
            line.gameObject.name = "Line " + index + ": " + (i - 1);
            line.transform.parent = GameObject.Find("Lines").transform;
            line.startWidth = 0.025f;
            line.endWidth = 0.025f;
            line.material = Resources.Load("LineMaterial", typeof(Material)) as Material;
            line.material.color = Color.blue;
            line.startColor = Color.blue;
            line.endColor = Color.blue;

            line.SetPosition(0, lastPos);
            line.SetPosition(1, newPos);

            //add to curve points list
            curvePoints.Add(newPos);
            lastPos = newPos;
        }
    }

    void RecalculateCatmullRom(int index)
    {
        // Debug.Log("Index: " + index);
        Vector3 p0, p1, p2, p3;

        p1 = points[index].transform.position;
        p2 = points[index + 1].transform.position;

        if (index > 0)
            p0 = points[index - 1].transform.position;
        else
            p0 = p1 * 2 - p2;

        if (index < points.Count - 2)
            p3 = points[index + 2].transform.position;
        else
            p3 = p2 * 2 - p1;

        Vector3 lastPos = p1;

        if(index * numSegments < curvePoints.Count)
            curvePoints[index * numSegments] = lastPos;
        else
            curvePoints.Add(lastPos);

        GameObject lines = GameObject.Find("Lines");

        for (int i = 1; i <= numSegments; i++) {
            float t = i / (float) numSegments;
            Vector3 newPos = GetCatmullRom(t, p0, p1, p2, p3);

            LineRenderer line = lines.transform.GetChild(index * numSegments + i - 1).gameObject.GetComponent<LineRenderer>();
            line.gameObject.name = "Line " + index + ": " + (i - 1);
            line.material.color = Color.blue;
            line.SetPosition(0, lastPos);
            line.SetPosition(1, newPos);

            //add to curve points list
            curvePoints[index * numSegments + i] = newPos;
            lastPos = newPos;
        }
    }

    Vector3 GetCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;
        Vector3 pos = 0.5f * (a + (b * t) + (t * t * c) + (t * t * t * d));

        return pos;
    }

    void CalculateQuaternions() {
        for(int i = 0; i < points.Count-1; i++) {
            if(i == 0)
                points[i].transform.LookAt(points[i+1].transform);
            else
                points[i].transform.rotation = Quaternion.LookRotation(points[i+1].transform.position - points[i-1].transform.position, Vector3.up);
        }
        points[points.Count-1].transform.rotation = points[points.Count-2].transform.rotation;
    }

    public void CalculateDistance()
    {
        curvePointsDist.Clear();
        double cumulativeDistance = 0;
        curvePointsDist.Add(0.0f);
        for (int i = 1; i <= curvePoints.Count - 1; i++)
        {
            cumulativeDistance += Vector3.Distance(curvePoints[i], curvePoints[i-1]);
            curvePointsDist.Add(cumulativeDistance);
        }
    }

    public void RecalculateDistance(int index) {

    }

    public int indexOfDist(double dist)
    {
        int index = 0;
        while (index < curvePointsDist.Count && curvePointsDist[index++] < dist) ;
        return index-1;
    }
}