using UnityEngine;
using System.Collections;

namespace SwissArmyKnife
{
    public class Triangle
    {
        public Vertex _A;
        public Vertex _B;
        public Vertex _C;
        public int _index;

        private float _xMin;
        private float _xMax;
        private float _zMin;
        private float _zmax;

        private Vector3 _XZNormalA;
        private Vector3 _XZNormalB;
        private Vector3 _XZNormalC;


        public int CornersWithInsideFlag
        {
            get
            {
                return (_A._isInside ? 1 : 0) + (_B._isInside ? 1 : 0) + (_C._isInside ? 1 : 0);
            }
        } 

        public Triangle(Vertex A, Vertex B, Vertex C, int i)
        {
            _A = A;
            _B = B;
            _C = C;
            _index = i;
            _xMin = Mathf.Min(_A._pos.x, _B._pos.x, _C._pos.x);
            _xMax = Mathf.Max(_A._pos.x, _B._pos.x, _C._pos.x);
            _zMin = Mathf.Min(_A._pos.z, _B._pos.z, _C._pos.z);
            _zmax = Mathf.Max(_A._pos.z, _B._pos.z, _C._pos.z);

            _XZNormalA = new Vector3(_A._pos.z - _C._pos.z, 0.0f, _C._pos.x - _A._pos.x);
            _XZNormalB = new Vector3(_B._pos.z - _A._pos.z, 0.0f, _A._pos.x - _B._pos.x);
            _XZNormalC = new Vector3(_C._pos.z - _B._pos.z, 0.0f, _B._pos.x - _C._pos.x);
        }

        // For quick bounding box checks
        public bool IsFarFromXZ(Vector3 vPoint)
        {
            return (vPoint.x < _xMin - 0.0001f || vPoint.x > _xMax + 0.0001f || vPoint.z < _zMin - 0.0001f || vPoint.z > _zmax + 0.0001f);
        }

        public bool BorderContainsXZ(Vector3 vPoint, out Vertex P, out Vertex Q)
        {
            // Check AB
            if (GeometryToolbox.IsOverSegment(vPoint, _A._pos, _B._pos))
            {
                P = _A;
                Q = _B;
                return true;
            }

            // Check AC
            if (GeometryToolbox.IsOverSegment(vPoint, _A._pos, _C._pos))
            {
                P = _A;
                Q = _C;
                return true;
            }


            // Check BC
            if (GeometryToolbox.IsOverSegment(vPoint, _B._pos, _C._pos))
            {
                P = _B;
                Q = _C;
                return true;
            }

            P = null;
            Q = null;
            return false;

        }


        public bool ContainsXZ(Vector3 vPoint)
        {
            // Point is inside iff it's in the three half-planes
            return (Vector3.Dot(vPoint - _A._pos, _XZNormalA) > 0
                   && Vector3.Dot(vPoint - _B._pos, _XZNormalB) > 0
                   && Vector3.Dot(vPoint - _C._pos, _XZNormalC) > 0);
        }

        public bool IntersectsXZ(Vector3 vP, Vector3 vQ)
        {
            Vector3 vA = _A._pos;
            Vector3 vB = _B._pos;
            Vector3 vC = _C._pos;

            return (GeometryToolbox.SegmentsIntersectXZ(vP, vQ, vA, vB)
                    || GeometryToolbox.SegmentsIntersectXZ(vP, vQ, vA, vC)
                    || GeometryToolbox.SegmentsIntersectXZ(vP, vQ, vB, vC));
        }

        public bool HasCorner(Vertex P)
        {
            return (P == _A || P == _B || P == _C);

        }

        public Vertex GetThirdCorner(Vertex notThisOne, Vertex notThisOneEither)
        {
            if (notThisOne == _A)
            {
                if (notThisOneEither == _B)
                {
                    return _C;
                }
                else
                {
                    return _B;
                }
            }
            else if (notThisOne == _B)
            {
                if (notThisOneEither == _A)
                {
                    return _C;
                }
                else
                {
                    return _A;
                }
            }
            else // notThisOne == mC
            {
                if (notThisOneEither == _B)
                {
                    return _A;
                }
                else
                {
                    return _B;
                }
            }

        }

        public Triangle GetNeighbourTriangleBehind(Vertex P, Vertex Q)
        {
            foreach (Triangle triangle in P._listAdjacentTriangles)
            {
                if (triangle != this && triangle.HasCorner(Q))
                {
                    return triangle;
                }
            }

            return null;
        }

        public Vector3 Normal
        {
            get
            {
                return Vector3.Cross(_B._pos - _A._pos, _C._pos - _A._pos).normalized;
            }
        }
    }
}
