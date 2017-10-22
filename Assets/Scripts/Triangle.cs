using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunnelAlgorithm
{
    [Serializable]
    public class Triangle
    {
        [SerializeField]
        private Vector3 a;
        public Vector3 A { get { return a; } private set { a = value; } }

        [SerializeField]
        private Vector3 b;
        public Vector3 B { get { return b; } private set { b = value; } }

        [SerializeField]
        private Vector3 c;
        public Vector3 C { get { return c; } private set { c = value; } }

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                yield return A;
                yield return B;
                yield return C;
            }
        }

        [SerializeField]
        private Edge ab;
        public Edge AB { get { return ab; } private set { ab = value; } }
        [SerializeField]
        private Edge bc;
        public Edge BC { get { return bc; } private set { bc = value; } }
        [SerializeField]
        private Edge ca;
        public Edge CA { get { return ca; } private set { ca = value; } }

        public IEnumerable<Edge> Edges
        {
            get
            {
                yield return AB;
                yield return BC;
                yield return CA;
            }
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
            AB = new Edge(a, b);
            BC = new Edge(b, c);
            CA = new Edge(c, a);
        }

        public Edge FindCommonEdge(Triangle other)
        {
            if (other.Edges.Contains(AB))
                return AB;
            else if (other.Edges.Contains(BC))
                return BC;
            else if (other.Edges.Contains(CA))
                return CA;

            return null;
        }

        public Vector3? FindOppositeVertex(Edge edge)
        {
            if (edge.Equals(AB))
                return C;
            else if (edge.Equals(BC))
                return A;
            else if (edge.Equals(CA))
                return B;

            return null;
        }

        public bool IsEncompass(Vector3 position)
        {
            var BP = position - B;
            var CP = position - C;
            var AP = position - A;

            var cross1 = Vector3.Cross(B - A, BP).normalized;
            var cross2 = Vector3.Cross(C - B, CP).normalized;
            var cross3 = Vector3.Cross(A - C, AP).normalized;

            return cross1.Equals(cross2) && cross2.Equals(cross3);
        }
    }
}

