using System;
using UnityEngine;

namespace FunnelAlgorithm
{
    [Serializable]
    public class Edge : IEquatable<Edge>
    {
        public static readonly Edge Zero = new Edge(Vector3.zero, Vector3.zero);

        [SerializeField]
        private Vector3 a;
        public Vector3 A { get { return a; } private set { a = value; } }

        [SerializeField]
        private Vector3 b;
        public Vector3 B { get { return b; } private set { b = value; } }

        public Edge(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
                return false;

            return Equals((Edge)obj);
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }

        public bool Equals(Edge other)
        {
            return (other.A.Equals(A) && other.B.Equals(B)) || (other.A.Equals(B) && other.B.Equals(A));
        }
    }
}
