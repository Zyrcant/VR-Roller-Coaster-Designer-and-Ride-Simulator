using System.Collections.Generic;
using UnityEngine;
public class Spline : MonoBehaviour
{
    public List<GameObject> points = new List<GameObject>();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

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
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = points[index-1].transform.position;
        Vector3 p1 = points[index].transform.position;
        Vector3 p2 = points[index+1].transform.position;
        Vector3 p3 = points[index+2].transform.position;

        //The start position of the line
        Vector3 lastPos = p1;

        for (int i = 1; i <= 200; i++)
        {
            //Which t position are we at?
            float t = i * 0.005f;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRom(t, p0, p1, p2, p3);

            //Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);

            //Save this pos so we can draw the next line segment
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