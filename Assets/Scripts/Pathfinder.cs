using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunnelAlgorithm
{
    struct Apex
    {
        // 左側もしくは右側の頂点配列のインデックス
        public int Index { get; private set; }
        // 位置
        public Vector3 Position { get; private set; }
        // 左側か右側のフラグ
        public bool IsLeft { get; private set; }

        public Apex(int index, Vector3 position, bool isLeft)
        {
            Index = index;
            Position = position;
            IsLeft = isLeft;
        }
    }

    public class Pathfinder
    {
        /// 3次元における左側の頂点配列
        private Vector3[] leftVertices3d;

        /// ３次元における右側の頂点配列
        private Vector3[] rightVertices3d;

        /// 2次元における左側の頂点配列
        private Vector3[] leftVertices2d;

        /// 2次元における右側の頂点配列
        private Vector3[] rightVertices2d;

        /// Funnelの先端
        private Apex apex;

        /// Funnelの右側の頂点リスト
        private List<int> leftIndices = new List<int>();

        /// Funnelの左側の頂点リスト
        private List<int> rightIndices = new List<int>();

        /// Funnelの先端リスト
        private List<Apex> apexes = new List<Apex>();

        public Path FindPath(IEnumerable<Triangle> triangles)
        {
            // 三角形の数が2つ未満の場合は計算できないので空のパスを返す
            if (triangles.Count() < 2)
                return null;

            // 与えられた三角形リストから３次元における左右の頂点配列を作成する
            if (!CreateVertices3D(triangles))
                return null;

            // 3次元における頂点配列を２次元における頂点配列にコピーする
            CopyVertices3DToVertices2D();

            // コピーした頂点配列を2次元に変換する
            ConvertTo2D();

            // XY平面上に変換する
            ConvertToXYPlane();

            // Funnelの先端を始点にセットする (右側も左側も同じなのでどっちを採用してももいい。ここでは左側を採用)
            apex = new Apex(0, leftVertices2d[0], true);

            // Funnelの左側の頂点リストに最初の頂点を追加する(先端から1つ進んだとこから開始する)
            leftIndices.Add(1);

            // Funnelの右側の頂点リストに最初の頂点を追加する(先端から1つ進んだとこから開始する)
            rightIndices.Add(1);

            // 終点にたどり着くまでFunnelに頂点を追加していく
            while (UpdateFunnel()) { }

            // 終点にたどり着いたら現在のFunnelの先端を追加する
            apexes.Add(apex);

            // 終点を追加する(ここも左、右側のどちらを採用してもOK)
            apexes.Add(new Apex(leftVertices2d.Length - 1, leftVertices2d.Last(), true));

            // 共有辺との交点()を求め、最終的なパスを計算する
            var path = MakePath();

            return path;
        }

        private bool CreateVertices3D(IEnumerable<Triangle> triangles)
        {
            // 三角形リストから共有辺のリストを作成する
            var commonEdges = new List<Edge>();
            foreach (var pair in triangles.MakePairs())
            {
                // 前後の三角形から共有辺を探す
                var commonEdge = pair.Left.FindCommonEdge(pair.Right);
                if (commonEdge == null)
                {
                    // 共有辺が見つからない場合は連結してないので失敗
                    return false;
                }

                commonEdges.Add(commonEdge);
            }

            // ３次元における頂点配列を初期化する。 長さは 始点 + 終点 + 共有辺の数 になる
            leftVertices3d = new Vector3[commonEdges.Count + 2];
            rightVertices3d = new Vector3[commonEdges.Count + 2];

            // 始点を求める。始点は最初の三角形の頂点の内、最初の共有辺に向かいあう頂点
            var startPoint = triangles.First().FindOppositeVertex(commonEdges.First()).Value;
            // 始点を両サイドの頂点配列に追加
            leftVertices3d[0] = rightVertices3d[0] = startPoint;

            // 共有辺の頂点を左、右に分けていく
            var i = 1;
            foreach (var commonEdge in commonEdges)
            {
                var nextOrigin = Vector3.zero;

                if (i == 1)
                {
                    leftVertices3d[i] = commonEdge.A;
                    rightVertices3d[i] = commonEdge.B;
                    nextOrigin = leftVertices3d[i];
                }
                else
                {
                    // 頂点配列の最後と一致するかを調べて一致する場合はそのサイドに配列に追加する。一致しないほうは逆サイドに追加する。　必ずどちらかに一致する
                    if (commonEdge.A.Equals(leftVertices3d[i - 1]) || commonEdge.B.Equals(rightVertices3d[i - 1]))
                    {
                        leftVertices3d[i] = commonEdge.A;
                        rightVertices3d[i] = commonEdge.B;

                        nextOrigin = rightVertices3d[i];
                    }
                    else
                    {
                        leftVertices3d[i] = commonEdge.B;
                        rightVertices3d[i] = commonEdge.A;

                        nextOrigin = leftVertices3d[i];
                    }
                }

                i++;
            }

            // 終点を求める。終点は最後の三角形の頂点の内、最後の共有辺に向かいある頂点
            var endPoint = triangles.Last().FindOppositeVertex(commonEdges.Last()).Value;
            // 終点を両サイドの頂点配列に追加
            leftVertices3d[leftVertices3d.Length - 1] = rightVertices3d[leftVertices3d.Length - 1] = endPoint;

            return true;
        }

        private void CopyVertices3DToVertices2D()
        {
            // 3次元における頂点配列をそのままコピーする
            // コピーしたものをあとで2次元に変換する
            leftVertices2d = leftVertices3d.Select(v => new Vector3(v.x, v.y, v.z)).ToArray();
            rightVertices2d = rightVertices3d.Select(v => new Vector3(v.x, v.y, v.z)).ToArray();
        }

        private void ConvertTo2D()
        {
            var origin = leftVertices2d[0];

            for (int i = 2, count = leftVertices2d.Length; i < count; i++)
            {
                // 回転対象の頂点をもとめる
                var isLeftEqual = leftVertices2d[i].Equals(leftVertices2d[i - 1]);
                var target = isLeftEqual ? rightVertices2d[i] : leftVertices2d[i];

                // origin - edge 平面の法線
                var originNormal = Vector3.Cross(leftVertices2d[i - 1] - rightVertices2d[i - 1], origin - leftVertices2d[i - 1]).normalized;

                // target - edge 平面の法線
                var targetNormal = Vector3.Cross(rightVertices2d[i - 1] - leftVertices2d[i - 1], target - rightVertices2d[i - 1]).normalized;

                // 2つの法線間の角度を求める
                var angle = MathUtility.SignedVectorAngle(originNormal, targetNormal, leftVertices2d[i - 1] - rightVertices2d[i - 1]);

                // edge を軸としてangleだけ回転するQuaternionを求める
                var rotation = Quaternion.AngleAxis(angle, rightVertices2d[i - 1] - leftVertices2d[i - 1]);

                // 回転軸に回転を適用して平行移動量を求める
                var translation = leftVertices2d[i - 1] - (rotation * leftVertices2d[i - 1]);

                for (int j = i; j < leftVertices2d.Length; j++)
                {
                    // 求めた変換パラメータを現在の頂点以降の頂点全てに適用する
                    // 適用する順番は回転->平行移動の順にすること

                    leftVertices2d[j] = rotation * leftVertices2d[j];
                    leftVertices2d[j] = leftVertices2d[j] + translation;

                    rightVertices2d[j] = rotation * rightVertices2d[j];
                    rightVertices2d[j] = rightVertices2d[j] + translation;
                }

                // 次の回転の起点を求める
                var nextOrigin = isLeftEqual ? rightVertices2d[i - 1] : leftVertices2d[i - 1];
                origin = nextOrigin;
            }
        }

        private void ConvertToXYPlane()
        {
            // 始点を原点に移動するパラメータを求める
            var origin = leftVertices2d[0];
            var translation = Vector3.zero - origin;
            var normal = Vector3.Cross(leftVertices2d[1] - origin, rightVertices2d[1] - origin).normalized;
            var rotation = Quaternion.FromToRotation(normal, new Vector3(0, 0, 1));

            // 全ての頂点に対してパラメータを適用する
            for (int i = 0; i < leftVertices2d.Length; i++)
            {
                leftVertices2d[i] = rotation * leftVertices2d[i];
                leftVertices2d[i] = leftVertices2d[i] + translation;
                leftVertices2d[i] = new Vector3(leftVertices2d[i].x, leftVertices2d[i].y);

                rightVertices2d[i] = rotation * rightVertices2d[i];
                rightVertices2d[i] = rightVertices2d[i] + translation;
                rightVertices2d[i] = new Vector3(rightVertices2d[i].x, rightVertices2d[i].y);
            }
        }

        private Path MakePath()
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();

            // 先端リストの前後でペアを作成してループで回す
            foreach (var pair in apexes.MakePairs())
            {
                // 現在の先端の頂点インデックスを開始インデックスとする
                var startIndex = pair.Left.Index;
                // 次の先端の頂点インデックスを終了インデックスとする
                var endIndex = pair.Right.Index;

                // 現在の先端のポジション(3次元)をパスに追加
                var startPoint = (pair.Left.IsLeft ? leftVertices3d : rightVertices3d)[pair.Left.Index];
                positions.Add(startPoint);

                // 現在の先端の法線を求める
                if (startIndex == 0)
                {
                    // 始点の場合
                    normals.Add(Vector3.Cross(rightVertices3d[1] - leftVertices3d[1], rightVertices3d[1] - leftVertices3d[0]).normalized);
                }
                else
                {
                    var opposite = rightVertices3d[startIndex].Equals(rightVertices3d[startIndex - 1]) ? leftVertices3d[startIndex - 1] : rightVertices3d[startIndex - 1];
                    normals.Add(Vector3.Cross(rightVertices3d[startIndex] - leftVertices3d[startIndex], rightVertices3d[startIndex] - opposite).normalized);
                }

                // 開始インデックスから終了インデックスまでの共有辺との交点を求める
                foreach (var i in Enumerable.Range(startIndex + 1, endIndex - startIndex))
                {
                    Vector3 intersection;
                    // 現在の先端 - 次の先端 の線分と 共有辺との交点を求める
                    if (MathUtility.SegmentSegmentIntersection(out intersection, pair.Left.Position, pair.Right.Position, leftVertices2d[i], rightVertices2d[i]))
                    {
                        // 後から3次元に変換できるように共有辺の左側の頂点から交点までの長さを割合として計算する
                        var lerp = Vector3.Distance(intersection, leftVertices2d[i]) / Vector3.Distance(rightVertices2d[i], leftVertices2d[i]);

                        // 各サイドの3次元における頂点を取得
                        var left3dpos = leftVertices3d[i];
                        var right3dpos = rightVertices3d[i];

                        // 先ほど求めた割合から3次元における交点を算出する
                        var position = Vector3.MoveTowards(left3dpos, right3dpos, Vector3.Distance(left3dpos, right3dpos) * lerp);

                        // 現在の面の法線を求める
                        var currentOpposite = rightVertices3d[i].Equals(rightVertices3d[i - 1]) ? leftVertices3d[i - 1] : rightVertices3d[i - 1];
                        var currentNormal = Vector3.Cross(rightVertices3d[i] - leftVertices3d[i], rightVertices3d[i] - currentOpposite);

                        // 次の面の法線を求める
                        var nextOpposite = rightVertices3d[i + 1].Equals(rightVertices3d[i]) ? leftVertices3d[i] : rightVertices3d[i];
                        var nextNormal = Vector3.Cross(rightVertices3d[i + 1] - leftVertices3d[i + 1], rightVertices3d[i + 1] - nextOpposite);

                        var normal = new Vector3((currentNormal.x + nextNormal.x) / 2.0f, (currentNormal.y + nextNormal.y) / 2.0f, (currentNormal.z + nextNormal.z) / 2.0f).normalized;

                        // パスに追加
                        positions.Add(position);
                        normals.Add(normal);
                    }
                }
            }

            // 3次元における終点をパスに追加
            var endPoint = leftVertices3d[leftVertices3d.Length - 1];
            positions.Add(endPoint);

            // 終点の法線を求める
            var endNormal = Vector3.Cross(leftVertices3d[leftVertices3d.Length - 2] - rightVertices3d[rightVertices3d.Length - 2], leftVertices3d[leftVertices3d.Length - 2] - leftVertices3d.Last()).normalized;
            normals.Add(endNormal);

            return MakePath(positions, normals);
        }

        private Path MakePath(IEnumerable<Vector3> positions, IEnumerable<Vector3> normals)
        {
            // 同じポジションでグループ化する
            var positionGroups = positions.SplitByEquality();

            // 法線を合成する
            var resultNormals = new List<Vector3>();
            int skipCount = 0;
            foreach (var g in positionGroups)
            {
                // ポジションリストに対応する法線リストを取得して合成する
                var resultNormal = MathUtility.Synthesize(normals.Skip(skipCount).Take(g.Count()));
                resultNormals.Add(resultNormal);
                skipCount += g.Count();
            }

            var resultPositions = positionGroups.Select(g => g.First());

            return new Path(resultPositions.Reverse<Vector3>(), resultNormals.Reverse<Vector3>());
        }

        private bool UpdateFunnel()
        {
            // Funnelの左側の頂点リストが終点にたどり着いたら終了 (右側も終点になるはずなので左側のチェックでよい)
            if (leftVertices2d.Length - 1 <= leftIndices.Last())
                return false;

            // Funnelの左側に頂点を追加する
            if (Push(leftVertices2d, rightVertices2d, ref leftIndices, ref rightIndices, true))
            {
                // 左側に追加できた場合は右側に追加する
                Push(rightVertices2d, leftVertices2d, ref rightIndices, ref leftIndices, false);
            }

            return true;
        }

        private bool Push(Vector3[] targets, Vector3[] opposites, ref List<int> targetIndices, ref List<int> oppositeIndices, bool isLeft)
        {
            // 進めた際にFunnnelの反対側の頂点リストを追い越さないかを調べる
            var crossedIndex = IsCrossedOppositeVertices(targets, opposites, targetIndices, oppositeIndices);
            if (crossedIndex > 0 && crossedIndex < targets.Count() - 1)
            {
                // 追い越した場合は現在のFunnelの先端を記録する
                apexes.Add(apex);

                // Funnelの先端を追い越されたほうの頂点にセットする
                apex = new Apex(crossedIndex, opposites[crossedIndex], !isLeft);

                // Funnelの両サイドのインデックスをセットする
                var nextIndex = apex.Index + 1;
                while (opposites.Length > nextIndex)
                {
                    // Funnelの先端と同じ座標にいる場合は座標が変わるまでインデックスを進める
                    if (!apex.Position.Equals(opposites[nextIndex]))
                        break;

                    nextIndex++;
                }

                targetIndices = new List<int>() { nextIndex };
                oppositeIndices = new List<int> { nextIndex };

                // 進めることができなかったのでFalseを返す
                return false;
            }

            // 進めることができる

            var next = targetIndices.Last() + 1;
            // 進めた場合Funnelが絞られるかを調べる
            if (IsTightened(targets, opposites, targetIndices, oppositeIndices))
            {
                // Funnelが絞られるので一度頂点リストをクリアする
                targetIndices.Clear();
            }

            // 新しい頂点として追加
            targetIndices.Add(next);

            // 進めることができたのでTrueを返す
            return true;
        }

        private bool IsTightened(Vector3[] targets, Vector3[] opposites, List<int> targetIndices, List<int> oppositeIndices)
        {
            // 先端から、進める側の最後の頂点に向かうベクトル
            var lastVec = (targets[targetIndices.Last() + 1] - apex.Position).normalized;
            // 先端から、進める側の最初の頂点に向かうベクトル
            var firstVec = (targets[targetIndices.First()] - apex.Position).normalized;
            // 先端から、反対側の最初の頂点に向かうベクトル
            var oppositeVec = (opposites[oppositeIndices.First()] - apex.Position).normalized;

            return Vector3.Dot(lastVec, oppositeVec) > Vector3.Dot(firstVec, oppositeVec);
        }

        private int IsCrossedOppositeVertices(Vector3[] targets, Vector3[] opposites, List<int> targetIndices, List<int> oppositeIndices)
        {
            // 先端から、進める側の最後の頂点に向かうベクトル
            var lastVec = targets[targetIndices.Last()] - apex.Position;
            // 先端から、進める側の追加する頂点に向かうベクトル
            var nextVec = targets[targetIndices.Last() + 1] - apex.Position;
            foreach (var i in oppositeIndices)
            {
                // 先端から、反対側のi番目の頂点に向かうベクトル
                var oppositeVec = opposites[i] - apex.Position;

                if (IsCrossed(oppositeVec, lastVec, nextVec))
                    return i;
            }

            return 0;
        }

        private bool IsCrossed(Vector3 target, Vector3 current, Vector3 next)
        {
            // 不正なベクトルの場合はダメ
            if (target == Vector3.zero || current == Vector3.zero || next == Vector3.zero)
                return false;

            // 更新前のベクトルと基準となるベクトルの外積をとる
            var currentCross = Vector3.Cross(target, current).normalized;
            // 更新後のベクトルと基準となるベクトルの外積をとる
            var nextCross = Vector3.Cross(target, next).normalized;

            var dot = Vector3.Dot(currentCross, nextCross);

            // 外積が反対方向なら追い越したことになる
            return dot <= 0;
        }
    }
}


