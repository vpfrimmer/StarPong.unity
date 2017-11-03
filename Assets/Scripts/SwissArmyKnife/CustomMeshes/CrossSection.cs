using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;


namespace SwissArmyKnife
{
	///=================================================================================================================
	///                                                                                                       <summary>
	///  CrossSection is a polyline consisting of Vertex objects. It's kind of a "2D CustomMesh" but still embedded in 3D space.   
    ///  It can be extracted from an existing Mesh or CustomMesh and be used to extrude a new CutomMesh (or parts of it).                   </summary>
	///
	///=================================================================================================================
	public class CrossSection 
	{
		#region Declarations and simple properties

        private List<Vertex> _vertices;
        private List<CrossSectionSegment> _segments;

        public List<Vertex> Vertices { get { return _vertices; } }
        public List<CrossSectionSegment> Segments { get { return _segments; } }

        private Transform _frameOfReference; // The transform in which's coordinates the vertex coordinates are expressed. null means "Global coordinates"

		#endregion
		#region Initialization
		//=================================================================================================================
		//
		// I N I T I A L I Z A T I O N 
		//
		//=================================================================================================================


		///-------------------------------------------------------                                                  <summary>
		/// Empty constructor                                                                        </summary>
		///-------------------------------------------------------
        public CrossSection()
        {
            _vertices = new List<Vertex>();
            _segments = new List<CrossSectionSegment>();
        }


        ///--------------------------------------------------                                                  <summary>
		/// Copy constructor                                                                        </summary>
		///-------------------------------------------------------
        public CrossSection(CrossSection crossSectionOriginal)
        {
            // Copy vertices
            _vertices = new List<Vertex>();
            for (int i = 0; i < crossSectionOriginal._vertices.Count; i++)
            {
                // Copy vertex
                Vertex originalVertex = crossSectionOriginal._vertices[i];
                Vertex vertex = new Vertex(originalVertex, _vertices.Count);
                _vertices.Add(vertex);

                // Original vertex know his copy
                crossSectionOriginal._vertices[i]._copy = vertex;
            }

            // Copy segments
            _segments = new List<CrossSectionSegment>();
            for (int i = 0; i < crossSectionOriginal._segments.Count; i += 3)
            {
                CrossSectionSegment originalSegment = crossSectionOriginal._segments[i];
                CrossSectionSegment segment = new CrossSectionSegment(originalSegment._A._copy, originalSegment._B._copy);
                _segments.Add(segment);
            }

            _frameOfReference = crossSectionOriginal._frameOfReference;
        }

        #endregion
        #region Extraction
        //=================================================================================================================
        //
        // E X T R A C T I O N
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Extract the border edges at one side of the mesh. borderDirection indicates which border.
        /// Example: To extract the border on the " negative x" side (points with minimal x), set borderDirection to (-1, 0, 0). 
        /// Note that the polyline contains original vertices.                                                 </summary>
        ///-------------------------------------------------------
        public CrossSection(CustomMesh customMesh, Vector3 borderDirection, GameObject referenceObject, float tolerancy = 0.001f)
        {
            // Create lists
            _vertices = new List<Vertex>();
            _segments = new List<CrossSectionSegment>();

            // Get where the border is
            float maxDotProduct = -Mathf.Infinity;
            foreach( Vertex vertex in customMesh.Vertices )
            {
                // Maximize the dot product of vertex position and borderDirection
                float dotProduct = Vector3.Dot(vertex._pos, borderDirection);
                maxDotProduct = Mathf.Max(dotProduct, maxDotProduct);
            }

            // Get the (original) vertices at this boder
            List<Vertex> meshVertices = new List<Vertex>();
            foreach (Vertex vertex in customMesh.Vertices)
            {
                float dotProduct = Vector3.Dot(vertex._pos, borderDirection);
                if (dotProduct > maxDotProduct - tolerancy )
                {
                    meshVertices.Add(vertex);
                    Vertex newVertex = new Vertex(vertex, _vertices.Count);
                    _vertices.Add(newVertex);
                    vertex._copy = newVertex;
                }
            }

            // Go through all triangles
            foreach (Triangle triangle in customMesh.Triangles)
            {
                bool containsA = meshVertices.Contains(triangle._A);
                bool containsB = meshVertices.Contains(triangle._B);
                bool containsC = meshVertices.Contains(triangle._C);

                // When _vertices contains EXACTLY TOO of these vertices, we add a segment.
                // (Why not three ? In this case we have a degenerate triangle or a triangle 
                // orthogonal to borderDirection. We don't want those in our cross section.) 
                if( containsA && containsB && !containsC )
                {
                    _segments.Add(new CrossSectionSegment(triangle._A._copy, triangle._B._copy));
                }
                else if (containsB && containsC && !containsA)
                {
                    _segments.Add(new CrossSectionSegment(triangle._B._copy, triangle._C._copy));
                }
                else if (containsC && containsA && !containsB)
                {
                    _segments.Add(new CrossSectionSegment(triangle._C._copy, triangle._A._copy));
                }

            }

            // All vertices are in local coordinates of referenceObject
            FrameOfReference = referenceObject.transform;
        }


        ///--------------------------------------------------                                                  <summary>
        /// Like CrossSection(CustomMesh customMesh, Vector3 borderDirection, float tolerancy),
        /// but from a Unity Mesh.                                                                                 </summary>
        ///-------------------------------------------------------
        public CrossSection(Mesh mesh, Vector3 globalBorderDirection, GameObject referenceObject, float tolerancy = 0.001f)
        {
            // Get local border direction
            Vector3 localBorderDirection = referenceObject.transform.InverseTransformVector(globalBorderDirection);

            tolerancy *= localBorderDirection.magnitude / globalBorderDirection.magnitude;

            localBorderDirection.Normalize();

            // Create lists
            _vertices = new List<Vertex>();
            _segments = new List<CrossSectionSegment>();

            // Get the original data
            Vector3[] meshVertices = mesh.vertices;
            int[] meshTriangles = mesh.triangles;

            // Get where the border is
            float maxDotProduct = -Mathf.Infinity;
            foreach (Vector3 vertex in meshVertices)
            {
                // Maximize the dot product of vertex position and borderDirection
                float dotProduct = Vector3.Dot(vertex, localBorderDirection);
                maxDotProduct = Mathf.Max(dotProduct, maxDotProduct);
            }

            // We need an array to quickly find a Vertex from a mesh vertex index
            Vertex[] verticesByMeshIndex = new Vertex[meshVertices.Length];

            // Get the vertices at this boder
            for (int i = 0; i < meshVertices.Length; i++ )
            {
                Vector3 vertexPoint = meshVertices[i];
                float dotProduct = Vector3.Dot(vertexPoint, localBorderDirection);
                if (dotProduct >= maxDotProduct - tolerancy)
                {
                    Vertex vertex = new Vertex(vertexPoint, _vertices.Count);
                    vertex._uv = mesh.uv[i];
                    _vertices.Add(vertex);
                    verticesByMeshIndex[i] = vertex;
                 }
            }

            // Note that now, verticesByMeshIndex contains mostly nulls - except for the indices of border vertices


            // Go through all triangles
            for( int i = 0; i < meshTriangles.Length; i += 3 /*because the array contains triplets of indices*/ ) 
            {
                Vertex A = verticesByMeshIndex[meshTriangles[i]];
                Vertex B = verticesByMeshIndex[meshTriangles[i+1]];
                Vertex C = verticesByMeshIndex[meshTriangles[i + 2]];

                bool containsA = (A != null);
                bool containsB = (B != null);
                bool containsC = (C != null);

                // When _vertices contains EXACTLY TOO of these vertices, we add a segment.
                // (Why not three ? In this case we have a degenerate triangle or a triangle 
                // orthogonal to borderDirection. We don't want those in our cross section.) 
                if (containsA && containsB && !containsC)
                {
                    _segments.Add(new CrossSectionSegment(A, B));
                }
                else if (containsB && containsC && !containsA)
                {
                    _segments.Add(new CrossSectionSegment(B, C));
                }
                else if (containsC && containsA && !containsB)
                {
                    _segments.Add(new CrossSectionSegment(C, A));
                }

            }

            // All vertices are in local coordinates of referenceObject
            FrameOfReference = referenceObject.transform;
        }




        private class LineInfo
        {
            public Vector3 _start;
            public Vector3 _end;
            public int _multiplicity;

            public LineInfo( Vector3 from, Vector3 to )
            {
                _start = from;
                _end = to;
                _multiplicity = 1;
            }
        }

        public static CrossSection CommonBorderCrossSectionOf( GameObject referenceObject, params GameObject[] meshObjects )
        {
            CrossSection cs = new CrossSection();

            List<LineInfo> lines = new List<LineInfo>();

            // Go through all meshes
            foreach( GameObject meshObject in meshObjects )
            {
                Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;

                // Get the vertices and triangles
                Vector3[] meshVertices = mesh.vertices;
                int[] meshTriangles = mesh.triangles;

                // Put the vertices in local space of referenceObject
                for( int i = 0; i < meshVertices.Length; i++ )
                {
                    meshVertices[i] = referenceObject.transform.InverseTransformPoint(meshObject.transform.TransformPoint(meshVertices[i]));
                }

                // Go through all triangles
                for( int i = 0; i < meshTriangles.Length; i+=3 )
                {
                    // Go through the three sides : i+0->i+1, i+1->i+2,  i+2->i+0
                    for( int p = 0; p < 3; p++ )
                    {
                        int q = (p+1)%3;
                        Vector3 A = meshVertices[meshTriangles[i + p]];
                        Vector3 B = meshVertices[meshTriangles[i + q]];

                        // Check if line AB (or its inverse BA) is already in the list
                        bool found = false;
                        for(int l = 0; l < lines.Count && !found; l++  )
                        {
                             LineInfo line = lines[l];
                             if (IsNear(line._start, A) && IsNear(line._end, B))
                            {
                                // Line already there - increase multiplicity
                                line._multiplicity++;
                                found = true;
                            }
                             else if (IsNear(line._start, B) && IsNear(line._end, A))
                            {
                                // Inverse line already there - decrease multiplicity
                                line._multiplicity--;
                                found = true;

                                if (line._multiplicity == 0)
                                {
                                    // Line disappears
                                    lines.RemoveAt(l--);
                                }
                            }
                        }

                        if( !found )
                        {
                            // Add new line
                            lines.Add(new LineInfo(A, B));
                        }
                    }

                }

                // Now, lines contains all border lines, the rest has been eliminated.
               
                // Add segments corresponding to remaining lines
                foreach( LineInfo line in lines )
                {
                    Vertex A = cs.FindOrAddVertex(line._start);
                    Vertex B = cs.FindOrAddVertex(line._end);
                    cs.AddSegment(A, B);
                }
            }

            cs.FrameOfReference = referenceObject.transform;

            return cs;
        }

        #endregion
        #region Vertices
        //=================================================================================================================
        //
        // V E R T I C E S
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Add uninitialized vertex                                                                     </summary>
        ///-------------------------------------------------------
        public Vertex AddVertex()
        {
            Vertex vertex = new Vertex(_vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }

        ///--------------------------------------------------                                                  <summary>
        /// Add vertex at a position                                                                    </summary>
        ///-------------------------------------------------------
        public Vertex AddVertex(Vector3 position)
        {
            Vertex vertex = new Vertex(position, _vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }

        ///--------------------------------------------------                                                  <summary>
        /// Find vertex at a position, if any                                                                    </summary>
        ///-------------------------------------------------------
        public Vertex FindVertex(Vector3 position)
        {
            foreach( Vertex vertex in _vertices )
            {
                if( IsNear(vertex._pos, position) )
                {
                    return vertex;
                }
            }
            return null;
        }

        ///--------------------------------------------------                                                  <summary>
        /// Find vertex at a position, if any                                                                    </summary>
        ///-------------------------------------------------------
        public Vertex FindOrAddVertex(Vector3 position)
        {
            Vertex vertex = FindVertex(position);
            if( vertex == null )
            {
                vertex = AddVertex(position);
            }
            return vertex;
        }

        ///--------------------------------------------------                                                  <summary>
        /// Add vertex at another vertex' position                                                               </summary>
        ///-------------------------------------------------------
        public Vertex AddVertex(Vertex original)
        {
            Vertex vertex = new Vertex(original._pos, _vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }

        #endregion
        #region Segments
        //=================================================================================================================
        //
        // S E G M E N T S
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Link two vertices by a segment                                                               </summary>
        ///-------------------------------------------------------
        public virtual CrossSectionSegment AddSegment(Vertex A, Vertex B, bool bInverted = false)
        {
            if (bInverted)
            {
                return AddSegment(B, A);
            }
            CrossSectionSegment segment = new CrossSectionSegment(A, B);
            _segments.Add(segment);
            return segment;
        }

        #endregion
        #region Polylines
        //=================================================================================================================
        //
        // S E G M E N T S
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Transforms the cross section into one or more polylines (in global coordinates).
        /// (One for each connected component.)
        /// Closed cross sections become polylines where the end point equals the start point.                 </summary>
        ///-------------------------------------------------------
        public List<List<Vector3>> ToPolylines()
        {
            // We start with a list of all unhandled segments
            List<CrossSectionSegment> unhandledSegments = new List<CrossSectionSegment>(_segments);

            // Remove degenerated segments
            for (int i = 0; i < unhandledSegments.Count; i++)
            {
                CrossSectionSegment segment = unhandledSegments[i];
                if (IsNear(segment._A._pos, segment._B._pos))
                {
                    // That's a teeny tiny segment we should just ignore
                    unhandledSegments.RemoveAt(i--);
                }
            }

            // A1 list to collect finished polylines
            List<List<Vector3>> allFinishedPolylines = new List<List<Vector3>>();

            // Now we do the following :
            // 0) as long as unhandled segments are left: 
            //      1) Initialize polylineUnderConstruction with any unhandled segment
            //      2) Try to "grow" it by adding a matching unhandled segment at start or end
            //      3) Rince and repeat until we can't find a matching segment
            //      4) Add polylineUnderConstruction to allFinishedPolylines
            //      5) Repeat at 0)

            // Are we done yet ?
            while (unhandledSegments.Count > 0)
            {
                // Initialize new polyline
                List<Vector3> polylineUnderConstruction = new List<Vector3>();

                // Get first unhandled segment
                CrossSectionSegment startSegment = unhandledSegments[0];
                unhandledSegments.RemoveAt(0); // <-- guarantees that the outer loop finishes, btw

                // Add it to our polyline
                Vector3 startPoint = _frameOfReference.TransformPoint( startSegment._A._pos );
                Vector3 endPoint = _frameOfReference.TransformPoint( startSegment._B._pos );
                polylineUnderConstruction.Add(startPoint);
                polylineUnderConstruction.Add(endPoint);

                // Now check the unhandled segments to find matching segments
                for (int i = 0; i < unhandledSegments.Count; i++ )
                {
                    CrossSectionSegment segment = unhandledSegments[i];
                    Vector3 A = _frameOfReference.TransformPoint( segment._A._pos );
                    Vector3 B = _frameOfReference.TransformPoint( segment._B._pos );
                    if(  IsNear(endPoint, A ) )
                    {
                        // Append segment at end
                        endPoint = B;
                        polylineUnderConstruction.Add(endPoint);

                        // Remove from list
                        unhandledSegments.RemoveAt(i);  // <-- guarantees that the inner loop finishes

                        // Reverify all segments from the begining
                        i = -1; // will be pluplussed to 0
                    }
                    else if (IsNear(B, startPoint ))
                    {
                        // Append segment at start
                        startPoint = A;
                        polylineUnderConstruction.Insert(0, startPoint);

                        // Remove from list
                        unhandledSegments.RemoveAt(i);  // <-- guarantees that the inner loop finishes

                        // Reverify all segments from the begining
                        i = -1;  // will be pluplussed to 0
                    }
    
                }
                // We reached the end of the loop, which means we can't grow the polyline anymore.

                // Reverse polyline (it seems to be in the wrong direction)
                polylineUnderConstruction.Reverse();

                // Add to list
                allFinishedPolylines.Add(polylineUnderConstruction);
            }

            // We are done here.
            return allFinishedPolylines;
        }


        #endregion
        #region Frames of reference
        //=================================================================================================================
        //
        // F R A M E  O F   R E F E R E N C E
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Allows initializing the frame of reference without actually changing the coordinates.
        /// Call this when you know that the vertex coordinates correspond already to this frame.           </summary>
        ///-------------------------------------------------------
        public Transform FrameOfReference
        {
            get
            {
                return _frameOfReference;
            }

            set
            {
                _frameOfReference = value;
            }

        }

        ///--------------------------------------------------                                                  <summary>
        /// Alows changing the frame of reference (and the hence the vertex coordinates).
        /// Call this when you want to change the vertex coordinates.                              </summary>
        ///-------------------------------------------------------
        public void ChangeFrameOfReferenceTo(Transform newFrameOfReference)
        {
            for( int i = 0; i < _vertices.Count; i++ )
            {
                // Were we in some local coordinates ?
                if( _frameOfReference != null )
                {
                    // Change to global coordinates
                    _vertices[i]._pos = _frameOfReference.TransformPoint(_vertices[i]._pos);
                }

                // Do we want to change to local (and not global) coordinates ?
                if (newFrameOfReference != null)
                {
                    // Change to new local coordinates
                    _vertices[i]._pos = newFrameOfReference.InverseTransformPoint(_vertices[i]._pos);
                }

            }

            // Don't fornet to upfdate _frameOfReference
            _frameOfReference = newFrameOfReference;
        }

        #endregion
        #region Miscellanous
        //=================================================================================================================
        //
        // M I S C E L L A N O U S
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Determines if the cross section is horizontal-ish or vertical_ish in the UV square                        </summary>
        ///-------------------------------------------------------
        public bool IsRatherAlongVAxis()
        {
            float minU = Mathf.Infinity;
            float maxU = -Mathf.Infinity;
            float minV = Mathf.Infinity;
            float maxV = -Mathf.Infinity;

            foreach( Vertex vertex in _vertices )
            {
                minU = Mathf.Min(minU, vertex._uv.x);
                maxU = Mathf.Max(maxU, vertex._uv.x);
                minV = Mathf.Min(minV, vertex._uv.y);
                maxV = Mathf.Max(maxV, vertex._uv.y);
            }

            return (maxV - minV) > (maxU - minU);
        }


        ///--------------------------------------------------                                                  <summary>
        /// Displays the cross section in editor                                                           </summary>
        ///-------------------------------------------------------
        public void DebugDisplay( Color color, float duration )
        {
            foreach( CrossSectionSegment segment in _segments )
            {
                Vector3 A = _frameOfReference.TransformPoint(segment._A._pos);
                Vector3 B = _frameOfReference.TransformPoint(segment._B._pos);
                Debug.DrawLine(A, B, color, duration);
                Debug.DrawLine(A, Vector3.zero, color, duration);
            }
        }


        ///--------------------------------------------------                                                  <summary>
        /// Says if two vertices are really near                                                               </summary>
        ///-------------------------------------------------------
        private static bool IsNear(Vector3 A, Vector3 B)
        {
            return (B - A).magnitude < 0.001f;
        }

		#endregion
	}
}

