using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace FunnelAlgorithm
{
    public class PathCanvas : MonoBehaviour
    {
        [SerializeField]
        private List<Vector3> vertices = new List<Vector3>();
        public ReadOnlyCollection<Vector3> Vertices { get { return vertices.AsReadOnly(); } }

        [SerializeField]
        private List<Triangle> triangles = new List<Triangle>();
        public ReadOnlyCollection<Triangle> Triangles { get { return triangles.AsReadOnly(); } }

        [SerializeField]
        private Path path = null;
        public Path Path { get { return path; } private set { path = value; } }

        public Edge SelectedEdge { get; private set; }

        public void AddVertex(Vector3 point)
        {
            if (!TryAddVertex(point))
                return;

            vertices.Add(point);

            UpdatePath();
        }

        private bool TryAddVertex(Vector3 vertex)
        {
            if (vertices.Contains(vertex))
                return false;

            if (!triangles.Any())
            {
                if (vertices.Count == 2)
                {
                    triangles.Add(new Triangle(vertices.ElementAt(0), vertices.ElementAt(1), vertex));
                }

                return true;
            }

            return AddTriangle(vertex);
        }

        private bool AddTriangle(Vector3 vertex)
        {
            Edge targetEdge = Edge.Zero;
            if (triangles.Count == 1)
            {
                targetEdge = GetNearestEdge(triangles.Last().Edges, vertex);
            }
            else
            {
                var commonEdge = triangles.Last().FindCommonEdge(triangles.ElementAt(triangles.Count - 2));
                targetEdge = GetNearestEdge(triangles.Last().Edges.Where(e => !e.Equals(commonEdge)), vertex);
            }

            var newTriangle = new Triangle(targetEdge.A, vertex, targetEdge.B);

            triangles.Add(newTriangle);

            return true;
        }

        private static Edge GetNearestEdge(IEnumerable<Edge> edges, Vector3 point)
        {
            return edges.FindMin(e => Vector3.Distance(e.A, point) + Vector3.Distance(e.B, point));
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            Path.Clear();
        }

        public void Remove()
        {
            if (triangles.Any())
                triangles.RemoveAt(triangles.Count - 1);

            vertices.RemoveAt(vertices.Count - 1);

            UpdatePath();
        }

        private void UpdatePath()
        {
            var pathfinder = new Pathfinder();
            Path = pathfinder.FindPath(triangles);
        }
    }
}
