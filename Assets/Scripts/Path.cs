using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunnelAlgorithm
{
    [Serializable]
    public class Path
    {
        [SerializeField]
        private Vector3[] positions;
        public Vector3[] Positions { get { return positions; } private set { positions = value; } }

        [SerializeField]
        private Vector3[] normals;
        public Vector3[] Normals { get { return normals; } private set { normals = value; } }

        public void Clear()
        {
            if (positions != null)
                Array.Clear(positions, 0, positions.Length);

            if (normals != null)
                Array.Clear(normals, 0, normals.Length);
        }

        public Path(IEnumerable<Vector3> positions, IEnumerable<Vector3> normals)
        {
            Positions = positions.ToArray();
            Normals = normals.ToArray();
        }
    }
}
