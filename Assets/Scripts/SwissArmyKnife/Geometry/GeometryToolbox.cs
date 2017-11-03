using UnityEngine;
using System.Collections;

namespace SwissArmyKnife
{
    // Class for geometric utility functions
    public class GeometryToolbox
    {
        // 2D intersection algorithm. The important point is that the resulting point is (in 3D) on the segment AB, even if the line PQ is above or below 
        // The algorithm assumes that the segment actually intersects the line.
        public static Vector3 CutSegmentByLine(Vector3 vA, Vector3 vB, Vector3 vP, Vector3 vQ, out float fCutRatio)
        {
            Vector3 vNormalToLine = new Vector3(vQ.z - vP.z, 0.0f, vP.x - vQ.x).normalized;
            float fDistanceAToLine = Vector3.Dot(vNormalToLine, vA - vP);
            float fDistanceBToLine = Vector3.Dot(vNormalToLine, vB - vP);
            fCutRatio = 0.0f; // for the error cases

            // Check for problems
            if (vA == vB)
            {
                // Degenerate segment
                Debug.LogError("vA == vB");
                Debug.DrawLine(vA, vA + Vector3.up, Color.red);
                Debug.DrawLine(vP, vQ, Color.yellow);
                Debug.Break();
                return Vector3.zero;
            }
            else if (vP == vQ)
            {
                // Degenerate line
                Debug.LogError("vP == vQ");
                Debug.DrawLine(vA, vB, Color.red);
                Debug.DrawLine(vP, vP + Vector3.up, Color.yellow);
                Debug.Break();
                return Vector3.zero;
            }
            else if (fDistanceAToLine * fDistanceBToLine > 0.0001f)
            {
                // Same sign, not 0 : Both points are on the same side of the line
                Debug.LogError("CutSegmmentByLine : Line doesn't cut segment " + fDistanceAToLine + "," + fDistanceBToLine);
                Debug.DrawLine(vA, vB, Color.red);
                Debug.DrawLine(vP, vQ, Color.yellow);
                Debug.Break();
                return Vector3.zero;
            }
            else if (Mathf.Abs(fDistanceAToLine) < 0.0001f && Mathf.Abs(fDistanceBToLine) < 0.0001f)
            {
                // Both values are identical
                Debug.LogError("CutSegmmentByLine : Segment is on line");
                Debug.DrawLine(vA, vB, Color.red);
                Debug.DrawLine(vP, vQ, Color.yellow);
                Debug.Break();
                return Vector3.Lerp(vA, vB, 0.5f);
            }

            // Forget sign
            fDistanceAToLine = Mathf.Abs(fDistanceAToLine);
            fDistanceBToLine = Mathf.Abs(fDistanceBToLine);

            // Use the distances to interpolate between A and B
            fCutRatio = fDistanceAToLine / (fDistanceAToLine + fDistanceBToLine);
            return Vector3.Lerp(vA, vB, fCutRatio);

        }



        // Method to test if a point P is over an (open) segment AB
        public static bool IsOverSegment(Vector3 vP, Vector3 vA, Vector3 vB)
        {
            Vector3 vAB = vB - vA;
            vAB.y = 0.0f;

            Vector3 vAP = vP - vA;
            vAP.y = 0.0f;

            // Check determinant
            if (Mathf.Abs(vAB.x * vAP.z - vAB.z * vAP.x) > 0.00001f)
            {
                // Determinant != 0 (with error): not alligned
                return false;
            }

            // Now the points are alligned
            if (Vector3.Dot(vAB, vAP) <= 0)
            {
                // P is in the wrong diretcion
                return false;
            }

            if (vAB.magnitude <= vAP.magnitude)
            {
                // P is too far away
                return false;
            }

            return true;
        }

        public static bool SegmentsIntersectXZ(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
        {
            return FasterLineSegmentIntersection(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(q1.x, q1.z), new Vector2(q2.x, q2.z));
        }

        // Method copied from http://www.stefanbader.ch/faster-line-segment-intersection-for-unity3dc/
        private static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            Vector2 a = p2 - p1;
            Vector2 b = q1 - q2;
            Vector2 c = p1 - q1;

            float alphaNumerator = b.y * c.x - b.x * c.y;
            float alphaDenominator = a.y * b.x - a.x * b.y;
            float betaNumerator = a.x * c.y - a.y * c.x;
            float betaDenominator = alphaDenominator; /*2013/07/05, fix by Deniz*/

            bool doIntersect = true;

            if (alphaDenominator == 0 || betaDenominator == 0)
            {
                doIntersect = false;
            }
            else
            {

                if (alphaDenominator > 0)
                {
                    if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
                {
                    doIntersect = false;
                }

                if (doIntersect && betaDenominator > 0)
                {
                    if (betaNumerator < 0 || betaNumerator > betaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (betaNumerator > 0 || betaNumerator < betaDenominator)
                {
                    doIntersect = false;
                }
            }

            return doIntersect;
        }





    }
}