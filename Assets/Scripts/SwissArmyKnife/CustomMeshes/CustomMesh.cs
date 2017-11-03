using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SwissArmyKnife
{
    // A class allowing creating custom meshes without the stress of keeping track of indices.

    // Use it as follows:
    // - Create a new CustomMesh
    // - Add vertices with AddVertex(...)
    // - Add triangles with Addtriangle or AddQuadrangle, referencing the vertices
    // - Use AttachMeshTo to create the actual MeshFilter and attach it to a game object
    // - If you want a mesh collider, use AddMeshCollider

    public enum HowToComputeUV
    {
        CS1u_to_u_and_CS2v_to_v,
        CS1u_to_u_and_CS2u_to_v
    }

    public class CustomMesh
    {
        protected List<Vertex> _vertices;
        protected List<Triangle> _triangles;

        public CustomMesh()
        {
            _vertices = new List<Vertex>();
            _triangles = new List<Triangle>();
        }

        // Constructor to clone existing mesh
        public CustomMesh(MeshFilter meshFilterOriginal)
        {
            // Retrieve data from original mesh
            Vector3[] aVertices = meshFilterOriginal.mesh.vertices;
            Vector2[] aUV = meshFilterOriginal.mesh.uv;
            Vector4[] aTangents = meshFilterOriginal.mesh.tangents;
            int[] aTriangles = meshFilterOriginal.mesh.triangles;

            // Copy vertices
            _vertices = new List<Vertex>();
            for (int i = 0; i < aVertices.Length; i++)
            {
                Vertex vertex = new Vertex(_vertices.Count);
                vertex._pos = aVertices[i];
                vertex._uv = aUV[i];
                vertex.Tangent = aTangents[i];
#if UNITY_EDITOR
                vertex.DEBUG_INFO = "Vertex from original mesh";
#endif
                _vertices.Add(vertex);
            }

            // Copy triangles
            _triangles = new List<Triangle>();
            for (int i = 0; i < aTriangles.Length; i += 3)
            {
                Vertex A = _vertices[aTriangles[i]];
                Vertex B = _vertices[aTriangles[i + 1]];
                Vertex C = _vertices[aTriangles[i + 2]];
                AddTriangle(A, B, C);
            }
        }

        // Constructor to clone existing custom mesh
        public CustomMesh(CustomMesh customMeshOriginal)
        {
            // Copy vertices
            _vertices = new List<Vertex>();
            for (int i = 0; i < customMeshOriginal._vertices.Count; i++)
            {
                Vertex vertex = new Vertex(_vertices.Count);
                vertex._pos = customMeshOriginal._vertices[i]._pos;
                vertex._uv = customMeshOriginal._vertices[i]._uv;
                vertex.Tangent = customMeshOriginal._vertices[i].Tangent;
                _vertices.Add(vertex);
            }

            // Copy triangles
            _triangles = new List<Triangle>();
            for (int i = 0; i < customMeshOriginal._triangles.Count; i++)
            {
                Triangle originalTriangle = customMeshOriginal._triangles[i];
                Vertex A = _vertices[originalTriangle._A._index];
                Vertex B = _vertices[originalTriangle._B._index];
                Vertex C = _vertices[originalTriangle._C._index];
                AddTriangle(A, B, C);
            }
        }

        // Constructor to clone existing custom mesh, but maybe flipped and with offset
        public CustomMesh(CustomMesh customMeshOriginal, Vector3 offset, bool flipped = false) 
        {
            // Copy vertices
            _vertices = new List<Vertex>();
            for (int i = 0; i < customMeshOriginal._vertices.Count; i++)
            {
                Vertex vertex = new Vertex(_vertices.Count);
                vertex._pos = customMeshOriginal._vertices[i]._pos + offset;
                vertex._uv = customMeshOriginal._vertices[i]._uv;
                _vertices.Add(vertex);
                Debug.Assert(customMeshOriginal._vertices[i]._index == i);
            }

            // Copy triangles
            _triangles = new List<Triangle>();
            for (int i = 0; i < customMeshOriginal._triangles.Count; i++)
            {
                Triangle originalTriangle = customMeshOriginal._triangles[i];
                Vertex A = _vertices[originalTriangle._A._index];
                Vertex B = _vertices[originalTriangle._B._index];
                Vertex C = _vertices[originalTriangle._C._index];
                AddTriangle(A, B, C, flipped);
            }
        }



        public Vertex FindVertexUnder(Vector3 vPosition)
        {
            for (int v = _vertices.Count - 1; v >= 0; v--)
            {
                Vertex vertex = _vertices[v];
                if (vertex.IsUnder(vPosition))
                {
                    return vertex;
                }
            }
            return null;
        }

        public Vertex AddVertex()
        {
            Vertex vertex = new Vertex(_vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }


        public Vertex AddVertex(Vector3 vPosition)
        {
            Vertex vertex = new Vertex(vPosition, _vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }

        public Vertex AddVertex(Vertex original)
        {
            Vertex vertex = new Vertex(original._pos, _vertices.Count);
            _vertices.Add(vertex);
            return vertex;
        }

        public virtual Triangle AddTriangle(Vertex A, Vertex B, Vertex C, bool bInverted = false)
        {
            if (bInverted)
            {
                return AddTriangle(A, C, B);
            }
            Triangle triangle = new Triangle(A, B, C, _triangles.Count);
            _triangles.Add(triangle);
            return triangle;
        }


        public void AddQuadrangle(Vertex A, Vertex B, Vertex C, Vertex D, bool bInverted = false)
        {
            AddTriangle(A, B, C, bInverted);
            AddTriangle(A, C, D, bInverted);
        }


        public void RemoveTriangle(Triangle triangle)
        {
            triangle._A._listAdjacentTriangles.Remove(triangle);
            triangle._B._listAdjacentTriangles.Remove(triangle);
            triangle._C._listAdjacentTriangles.Remove(triangle);
            _triangles.Remove(triangle);
        }

        public virtual void RemoveTriangle(int t)
        {
             _triangles.RemoveAt(t);
        }


        public List<Vertex> Vertices
        {
            get
            {
                return _vertices;
            }
        }

        public List<Triangle> Triangles
        {
            get
            {
                return _triangles;
            }
        }


        public void AttachMeshTo(GameObject gObject, Material material)
        {
            // Create components
            if (gObject.GetComponent<MeshFilter>() == null)
            {
                gObject.AddComponent<MeshFilter>();
            }
            if (gObject.GetComponent<MeshRenderer>() == null)
            {
                gObject.AddComponent<MeshRenderer>();
            }
            if (gObject.GetComponent<MeshCollider>() == null)
            {
                gObject.AddComponent<MeshCollider>();
            }

            // Create arrays
            Vector3[] aVertices = new Vector3[_vertices.Count];
            Color[] aVertexColors = new Color[_vertices.Count];
            Vector2[] aUV = new Vector2[_vertices.Count];
            int[] aTriangles = new int[_triangles.Count * 3];
            Vector4[] aTangents = new Vector4[_vertices.Count];

            // Populate arrays
            for (int v = 0; v < _vertices.Count; v++)
            {
                aVertices[v] = _vertices[v]._pos;
                aVertexColors[v] = _vertices[v]._color;
                aUV[v] = _vertices[v]._uv;
                aTangents[v] = _vertices[v].Tangent;
            }
            for (int t = 0; t < _triangles.Count; t++)
            {
                Triangle triangle = _triangles[t];
                aTriangles[3 * t + 0] = triangle._A._index;
                aTriangles[3 * t + 1] = triangle._B._index;
                aTriangles[3 * t + 2] = triangle._C._index;
            }

            // Assign arrays
            MeshFilter meshfilter = gObject.GetComponent<MeshFilter>();
            meshfilter.mesh.triangles = new int[0]; // Otherwise the next line may cause an error "not enough vertices"
            meshfilter.mesh.vertices = aVertices;
            meshfilter.mesh.colors = aVertexColors;
            meshfilter.mesh.uv = aUV;
            meshfilter.mesh.triangles = aTriangles;
            meshfilter.mesh.RecalculateNormals();
            meshfilter.mesh.RecalculateBounds();
            meshfilter.mesh.tangents = aTangents;
            var o_255_12_636289890268226818 = meshfilter.mesh;

            gObject.GetComponent<MeshRenderer>().material = material;
        }

        public void AddMeshCollider(GameObject gObject)
        {
            if (gObject.GetComponent<MeshCollider>() == null)
            {
                gObject.AddComponent<MeshCollider>();
            }

            gObject.GetComponent<MeshCollider>().sharedMesh = null;
            gObject.GetComponent<MeshCollider>().sharedMesh = gObject.GetComponent<MeshFilter>().mesh;
        }


        //=================================================================================================================
        //
        // E X T R U S I O N
        //
        //=================================================================================================================

        ///--------------------------------------------------                                                  <summary>
        /// Extrude a cross section along a vector.
        /// The extruded form follows the cross section shape.
        ///        ____
        /// __--''    ''--____--''   _
        /// |      ____          |   | thicknessVector
        /// __--''    ''--____--''   V
        /// 
        /// Exemple: Extrude terrain border one meter downwards.     
        /// In this case, thicknessVector = -Vector3.up.                                                       </summary>
        ///-------------------------------------------------------
        public void ExtrudeConstantThickness(CrossSection crossSection, Vector3 thicknessVector, bool isFlipped = false)
        {
            // Create array of vertices
            Vertex[,] verticesGrid = new Vertex[crossSection.Vertices.Count, 2];
            for (int i = 0; i < crossSection.Vertices.Count; i++)
            {
                // Create vertex on crossSection
                Vertex A = new Vertex(crossSection.Vertices[i]._pos, _vertices.Count);
                verticesGrid[i, 0] = A;
                _vertices.Add(A);

                //TODO: uv

                // Create second shifted vertex
                Vertex B = new Vertex(crossSection.Vertices[i]._pos + thicknessVector, _vertices.Count);
                verticesGrid[i, 1] = B;
                _vertices.Add(B);


                //TODO: uv

            }

            // Create quads
            foreach (CrossSectionSegment segment in crossSection.Segments)
            {
                // Get the vertices corresponding tho the end points
                Vertex P = verticesGrid[segment._A._index, 0];
                Vertex Q = verticesGrid[segment._A._index, 1];
                Vertex R = verticesGrid[segment._B._index, 0];
                Vertex S = verticesGrid[segment._B._index, 1];

                // Create a quad
                AddQuadrangle(P, Q, S, R, isFlipped);
            }

        }

        ///--------------------------------------------------                                                  <summary>
        /// Extrude a cross section by projection onto a plane.
        /// The plane is defined by { x (dot) planeNormal = planeValue }
        ///       ____
        /// __--''    ''--____--''
        /// |                    |  
        /// ______________________
        /// 
        /// Exemple: Extrude terrain border downwards up to a depth of -5.      
        /// In thies case, planeNormal = Vector3.up and  planeValue = -5.                                    </summary>
        ///-------------------------------------------------------
        public void ExtrudeTowardsPlane(CrossSection crossSection, Vector3 planeNormal, float planeValue, bool isFlipped = false)
        {
            // just to be sure
            planeNormal.Normalize();

            // Create array of vertices
            Vertex[,] verticesGrid = new Vertex[crossSection.Vertices.Count, 2];
            for (int i = 0; i < crossSection.Vertices.Count; i++)
            {
                // Create vertex on crossSection
                Vertex A = new Vertex(crossSection.Vertices[i]._pos, _vertices.Count);
                verticesGrid[i, 0] = A;
                _vertices.Add(A);

                //TODO: uv

                // Create second shifted vertex
                Vector3 posB = A._pos + planeNormal * (planeValue - Vector3.Dot(A._pos, planeNormal));
                Vertex B = new Vertex(posB, _vertices.Count);
                verticesGrid[i, 1] = B;
                _vertices.Add(B);

            }

            // Create quads
            foreach (CrossSectionSegment segment in crossSection.Segments)
            {
                // Get the vertices corresponding tho the end points
                Vertex P = verticesGrid[segment._A._index, 0];
                Vertex Q = verticesGrid[segment._A._index, 1];
                Vertex R = verticesGrid[segment._B._index, 0];
                Vertex S = verticesGrid[segment._B._index, 1];

                // Create a quad
                AddQuadrangle(P, Q, S, R, isFlipped);
            }

        }


        ///--------------------------------------------------                                                  <summary>
        /// Extrude a cross section along another cross section.
        /// Exemple: Extrude road profile along terrain border.                                                 
        /// ReferencePoint is the "origin" of the operation, usually the start point of crossSection2
        /// (or even of both cross sections).                                                                   </summary>
        ///-------------------------------------------------------
        public void ExtrudeAlongCrossSection(CrossSection crossSection1, CrossSection crossSection2, Vector3 referencePoint, bool isFlipped = false )
        {
            if( crossSection1.FrameOfReference != crossSection2.FrameOfReference)
            {
                Debug.LogWarning("Cross sections have different reference frames.");
            }

            bool CS1_is_along_u_axis = !crossSection1.IsRatherAlongVAxis();
            bool CS2_is_along_u_axis = !crossSection2.IsRatherAlongVAxis();

            // Create array of vertices
            Vertex[,] verticesGrid = new Vertex[crossSection1.Vertices.Count, crossSection2.Vertices.Count];
            for( int i = 0; i < crossSection1.Vertices.Count; i++ )
            {
                for (int j = 0; j < crossSection2.Vertices.Count; j++)
                {
                    Vector3 position = crossSection1.Vertices[i]._pos + crossSection2.Vertices[j]._pos - referencePoint;
                    Vertex vertex = new Vertex(position, _vertices.Count);


                    if (CS1_is_along_u_axis)
                    {
                        if (CS2_is_along_u_axis)
                        {
                            vertex._uv = new Vector2(crossSection1.Vertices[i]._uv.x, crossSection2.Vertices[j]._uv.x);
                        }
                        else
                        {
                            vertex._uv = new Vector2(crossSection1.Vertices[i]._uv.x, crossSection2.Vertices[j]._uv.y);
                        }
                    }
                    else
                    {
                        if (CS2_is_along_u_axis)
                        {
                            vertex._uv = new Vector2(crossSection1.Vertices[i]._uv.y, crossSection2.Vertices[j]._uv.x);
                        }
                        else
                        {
                            vertex._uv = new Vector2(crossSection1.Vertices[i]._uv.y, crossSection2.Vertices[j]._uv.y);
                        }
                    }
       

                    verticesGrid[i, j] = vertex;
                    _vertices.Add(vertex);
                }
            }

            // Create quads
            int count = 0;
            foreach (CrossSectionSegment segment1 in crossSection1.Segments)
            {
                foreach (CrossSectionSegment segment2 in crossSection2.Segments)
                {
                    // Get the vertices corresponding tho the end points
                    Vertex A = verticesGrid[segment1._A._index, segment2._A._index];
                    Vertex B = verticesGrid[segment1._A._index, segment2._B._index];
                    Vertex C = verticesGrid[segment1._B._index, segment2._A._index];
                    Vertex D = verticesGrid[segment1._B._index, segment2._B._index];

                    // Create tangent
                    Vector3 tangent3 = (B._pos - A._pos).normalized;
                    Vector4 tangent4 = tangent3;
                    tangent4.w = 1.0f;
                    A.Tangent = B.Tangent = C.Tangent = D.Tangent = tangent4;

                    // Create a quad
                    AddQuadrangle(A, B, D, C, isFlipped);
                    count++;
                }
            }
        }

    }

}
