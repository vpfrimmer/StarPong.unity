using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SwissArmyKnife
{
    public class XZPolygon
    {
        public Vector3[] maPoints;
        private Rect mBoundingBoxXZ;

        public XZPolygon(List<Vector3> lPoints)
        {
            InitializeFrom(lPoints);
        }

        public XZPolygon(Vector3 vA, Vector3 vB, Vector3 vC)
        {
            List<Vector3> lPoints = new List<Vector3>();
            lPoints.Add(vA);
            lPoints.Add(vB);
            lPoints.Add(vC);
            InitializeFrom(lPoints);
        }

        private void InitializeFrom(List<Vector3> lPoints)
        {
            // Open polyline ?
            if (lPoints[0] != lPoints[lPoints.Count - 1])
            {
                // Close polyline
                lPoints.Add(lPoints[0]);
            }

            // Populate array
            maPoints = lPoints.ToArray();
            for (int i = 0; i < maPoints.Length; i++ )
            {
                maPoints[i].y = 0.0f;
            }

            ComputeBoundingBox();
        }

        public void ComputeBoundingBox()
        {
            // Initialize bounding box
            float fXMin = Mathf.Infinity;
            float fXMax = -Mathf.Infinity;
            float fZMin = Mathf.Infinity;
            float fZMax = -Mathf.Infinity;
            for (int i = 0; i < this.maPoints.Length; i++)
            {
                fXMin = Mathf.Min(fXMin, maPoints[i].x);
                fXMax = Mathf.Max(fXMax, maPoints[i].x);
                fZMin = Mathf.Min(fZMin, maPoints[i].z);
                fZMax = Mathf.Max(fZMax, maPoints[i].z);
            }
            mBoundingBoxXZ = new Rect(fXMin, fZMin, fXMax - fXMin, fZMax - fZMin);
        }

        public void DebugDraw(Color color)
        {

            // Display 
            for (int i = 0; i < maPoints.Length - 1; i++)
            {
                Vector3 vA = maPoints[i];
                Vector3 vB = maPoints[i + 1];
                vA.y = 0.0f;
                vB.y = 0.0f;

                Debug.DrawLine(vA, vB, color, 1.0f);
            }
        }

        public void DebugDraw(Color color1, Color color2)
        {

            // Display 
            for (int i = 0; i < maPoints.Length - 1; i++)
            {
                Vector3 vA = maPoints[i];
                Vector3 vB = maPoints[i + 1];
                vA.y = 0.0f;
                vB.y = 0.0f;

                float fLambda = (float)i / (maPoints.Length - 2);

                Debug.DrawLine(vA, vB, Color.Lerp(color1, color2, fLambda), 1.0f);
            }
        }

        // Method to test whether a point P is inside a polygon. 
        // (We ignore y and project everything to the XZ-plane.)
        public bool Contains(Vector3 vP)
        {
            // The algorithm is well-known. 
            // To test if P is inside the polygon, we count how many segments intersect the ray from P towards the left side.
            // The point is inside iff said number is odd.
            // Some rules are to respect : 
            // - Horizontal segments (segments parallel to the x axix) are ignored
            // - The "upper" point of a segment counts, the "lower" one doesn't (z-coordinate)
            // Note that the algorithm is relatiovely fast, as most cases can be decided with bounding box checks
            //
            //                                    _______________
            //                          _________/               |
            //                         /                         |
            //                        /             |\           |
            //-----------------------*--------------*-*----P     |
            //                      /               |  \         |
            //                     /___________     |   \________|
            //                                 |    |
            //                                 |    |
            //   z                             |____|
            //   |
            //   |
            //   *-----x

            // Before we start, make a simple bounding box check.
            if (!mBoundingBoxXZ.Contains(new Vector2(vP.x, vP.z)))
            {
                return false;
            }

            // We want to start the intersections
            int iIntersections = 0;

            //Debug.DrawLine( vP, vP - Vector3.right * 1000, Color.blue );

            // Run through all segments
            for (int i = 0; i < maPoints.Length - 1; i++)
            {
                //Debug.Log ("Checking segment "+i);
                Vector3 vA = maPoints[i];
                Vector3 vB = maPoints[i + 1];
                //Debug.DrawLine( vA, vB, Color.red );

                //         A
                //        /
                //       /
                //      /
                //     B

                // Make sure A is the "upper" point
                if (vA.z < vB.z)
                {
                    // Swap A and B
                    Vector3 vFoo = vA;
                    vA = vB;
                    vB = vFoo;
                }

                // Ignore horizontal segments
                if (vA.z == vB.z)
                {
                    //Debug.Log ("Horizontal");
                    // A---------B
                    //
                    continue;
                }

                // Some "bounding bow" tests:

                // (1) Ignore segments on the upper halfplane (_with_ border)
                if (vB.z >= vP.z)
                {
                    //Debug.Log ("Upper");
                    //         A
                    //        /
                    //       /
                    //      /
                    //     B
                    //
                    //  - - -  P is somewhere on this line - - -  
                    continue;
                }

                // (2) Ignore segments on the lower halfplane (_without_ border)
                if (vA.z < vP.z)
                {
                    //Debug.Log ("Lower");
                    //  - - -  P is somewhere on this line - - -  
                    //
                    //         A
                    //        /
                    //       /
                    //      /
                    //     B
                    continue;
                }

                // Now we can be sure that A is on the upper halfplane (with border)
                // and B on the lower halfplane (without border).
                // P is somewhere in-between - but where ?
                //
                //              A
                //             /
                //  - - -  P is somewhere on this line - - -  
                //           /
                //          B


                // (3) Ignore segments entirely on the right halfplane (with border)
                if (vA.x >= vP.x && vB.x >= vP.x)
                {
                    //Debug.Log ("Right");
                    //        :        A
                    //        :       /
                    // -------P      /
                    //        :     /
                    //        :    B
                    continue;
                }

                // (4) Always COUNT segments entirely on the left halfplane (with border)
                if (vA.x <= vP.x && vB.x <= vP.x)
                {
                    //Debug.Log ("Left +1");
                    //         A   :
                    //        /    :
                    // ------*-----P
                    //      /      :
                    //     B       :
                    iIntersections++;
                    continue;
                }

                // The remaining case is rather rare, it's when one of the points A, B is on the right, 
                // one is on the left, and we have to calculate whether P is actually left or right of the line.
                //         : A             :   A      
                //         :/              :  /          
                //         :          -----P Q    
                //        /:    or         :/                
                // ------Q-P               :    
                //      /  :              /:    
                //     B   :             B :    

                // Compute where the horizontal line through B cuts the segment. 0.0 = "at B", 0.5 = "in the middle", 1.0 = "at A".
                float fCutRatio = (vP.z - vB.z) / (vA.z - vB.z);

                // Compute the point X where the segment cuts the horizontal line
                Vector3 vQ = Vector3.Lerp(vB, vA, fCutRatio);

                //Debug.DrawLine ( vP, vQ, Color.yellow );

                // Is Q left or right of P?
                if (vQ.x < vP.x)
                {
                    // We have an intersection
                    iIntersections++;
                }
            }

            // Now we just have to check whether iIntersections is even or odd
            //Debug.Break();
            if (iIntersections % 2 == 1)
            {
                //Debug.Log ("Odd :)");
                // Odd number of intersections - inside
                return true;
            }
            else
            {
                //Debug.Log ("Even :(");
                // Even number of intersections (maybe none) - outside
                return false;
            }

        }

    }
}