using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunnelAlgorithm
{
    static class MathUtility
    {
        public static Vector3 Synthesize(IEnumerable<Vector3> vectors)
        {
            if (!vectors.Any())
                return Vector3.zero;

            return Synthesize(vectors.ToArray());
        }

        public static Vector3 Synthesize(params Vector3[] vectors)
        {
            var x = vectors.First().x;
            var y = vectors.First().y;
            var z = vectors.First().z;

            foreach (var v in vectors.Skip(1))
            {
                x += v.x;
                y += v.y;
                z += v.z;
            }

            return new Vector3(x, y, z) / vectors.Count();
        }

        public static float DistanceToLine(Ray ray, Vector3 point, bool clamp = false)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        public static bool SegmentSegmentIntersection(
            out Vector3 intersectionPoint,
            Vector3 segment1Start,
            Vector3 segment1End,
            Vector3 segment2Start,
            Vector3 segment2End)
        {
            Vector3 direction1 = segment1End - segment1Start;
            Vector3 direction2 = segment2End - segment2Start;

            bool isIntersect = LineLineIntersection(out intersectionPoint, segment1Start, direction1, segment2Start, direction2);

            if (isIntersect)
            {
                var left1 = (intersectionPoint - segment1Start).normalized;
                var right1 = (intersectionPoint - segment1End).normalized;

                if (left1 == right1)
                {
                    isIntersect = false;
                }

                if (isIntersect)
                {
                    var left2 = (intersectionPoint - segment2Start).normalized;
                    var right2 = (intersectionPoint - segment2End).normalized;
                    if (left2 == right2)
                    {
                        isIntersect = false;
                    }
                }
            }

            return isIntersect;
        }

        public static bool LineLineIntersection(
            out Vector3 intersection,
            Vector3 linePoint1,
            Vector3 lineVec1,
            Vector3 linePoint2,
            Vector3 lineVec2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            if (Mathf.Abs(planarFactor) < 0.000001f && crossVec1and2.sqrMagnitude > 0.000000f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }

        public static Vector3 Round(this Vector3 self, int significantFigures)
        {
            var x = (float)Math.Round(self.x, significantFigures);
            var y = (float)Math.Round(self.y, significantFigures);
            var z = (float)Math.Round(self.z, significantFigures);

            return new Vector3(x, y, z);
        }

        public static float SignedVectorAngle(Vector3 vec1, Vector3 vec2, Vector3 normal)
        {
            Vector3 perpVector;
            float angle;

            perpVector = Vector3.Cross(normal, vec1);

            angle = Vector3.Angle(vec1, vec2);
            angle *= Mathf.Sign(Vector3.Dot(perpVector, vec2));

            return angle;
        }
    }
}
