using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_Geo
{
    public static bool IsPointInPolygon(List<Vector2> polygon, Vector2 testPoint)
    {
        bool result = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y ||
                polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
            {
                if (polygon[i].x + (testPoint.y - polygon[i].y) /
                   (polygon[j].y - polygon[i].y) *
                   (polygon[j].x - polygon[i].x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static (float, float, float, float) GetBoundingBoxFromPolygon(List<Vector2> polygon)
    {
        Rect rect = new Rect();

        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float ymin = float.MaxValue;
        float ymax = float.MinValue;

        for (int i = 0; i < polygon.Count; i++)
        {
            xmin = Mathf.Min(polygon[i].x, xmin);
            ymin = Mathf.Min(polygon[i].y, ymin);

            xmax = Mathf.Max(polygon[i].x, xmax);
            ymax = Mathf.Max(polygon[i].y, ymax);
        }

        rect.xMin = xmin;
        rect.yMin = ymin;
        rect.xMax = xmax;
        rect.yMax = ymax;

        return (xmin, xmax, ymin, ymax);
    }
}
