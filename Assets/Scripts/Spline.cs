using System.Collections.Generic;
using UnityEngine;
public class Spline : MonoBehaviour
{
    public List<GameObject> points = new List<GameObject>();
    //TODO: interpolate distance using these curvepoints
    public List<Vector3> curvePoints = new List<Vector3>();

    public void RedrawSpline()
    {
        //destroys each line and redraws
        foreach (Transform child in GameObject.Find("Lines").transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Draw the Catmull-Rom spline between the points
        if (points.Count >= 4)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0 || i == points.Count - 2 || i == points.Count - 1)
                {
                    continue;
                }
                CalculateCatmullRom(i);
            }
        }
    }
    void CalculateCatmullRom(int index)
    {
        Vector3 p0 = points[index-1].transform.position;
        Vector3 p1 = points[index].transform.position;
        Vector3 p2 = points[index+1].transform.position;
        Vector3 p3 = points[index+2].transform.position;
        
        Vector3 lastPos = p1;
        curvePoints.Add(lastPos);
        for (int i = 1; i <= 200; i++)
        {
            float t = i * 0.005f;
            Vector3 newPos = GetCatmullRom(t, p0, p1, p2, p3);

            //Draw this line segment
            LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
            line.transform.parent = GameObject.Find("Lines").transform;
            line.startWidth = 0.025f;
            line.endWidth = 0.025f;
            line.startColor = Color.red;
            line.endColor = Color.red;
            line.SetPosition(0, lastPos);
            line.SetPosition(1, newPos);

            //add to curve points list
            curvePoints.Add(newPos);
            lastPos = newPos;
        }
    }

    Vector3 GetCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

}