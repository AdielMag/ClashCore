using System;
using System.Collections.Generic;
using System.Linq;

using App.SubDomains.Game.SubDomains.Environment.Scripts.Data;

using Cysharp.Threading.Tasks;
using Sylves;
using UnityEngine;
using Object = UnityEngine.Object;

namespace App.Scripts.Editor.EnvironmentCreator
{
    public class OrganicHexEnvironmentCreator : IDisposable
    {
        private GameObject gridParent;
        private List<GameObject> edgePool = new ();
        private GameObject edgeParent;

        // Data structure to represent a sub-face (cell) in the subdivided grid


        // Holds all sub-faces for selection/manipulation

        
        public async UniTask<SubFaceGrid> CreateGrid(float edgeWidth, int hexGridSize, float perturbBoundaryMagnitude, float perturbBoundarySmoothness, float perturbBoundaryInnerMagnitude)
        {
            CleanupPreviousGrid();
            gridParent = new GameObject("GeneratedGridParent");
            edgeParent = null;
            edgePool.Clear();
            var subFaceGrid = new SubFaceGrid();

            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(hexGridSize));
            MeshData meshData = triangleGrid.ToMeshData();
            await UpdateEdges(meshData, edgeWidth);

            meshData = meshData.RandomPairing();
            await UpdateEdges(meshData, edgeWidth);

            meshData = ConwayOperators.Ortho(meshData);
            await UpdateEdges(meshData, edgeWidth);

            meshData = meshData.Weld();
            await UpdateEdges(meshData, edgeWidth);

            PerturbBoundary(meshData, perturbBoundaryMagnitude, perturbBoundarySmoothness, perturbBoundaryInnerMagnitude);
            await UpdateEdges(meshData, edgeWidth);

            RelaxMesh(meshData, 10);
            await UpdateEdges(meshData, edgeWidth);

            SquarifyQuads(meshData);
            await UpdateEdges(meshData, edgeWidth);

            var result = await CreateDualGridOverlay(meshData);
            await CreateIntersectionFacesAndCleanup(
                subFaceGrid,
                result.dualEdgeParent,
                result.verts,
                result.faceIndices,
                result.faceCenters,
                result.boundaryEdgeToPoint,
                result.boundaryPoints
            );

            await UniTask.Delay(TimeSpan.FromSeconds(3));
            Object.DestroyImmediate(gridParent);
            return subFaceGrid;
        }

        private void CleanupPreviousGrid()
        {
            if (gridParent != null)
                Object.DestroyImmediate(gridParent);
        }

        private async UniTask UpdateEdges(MeshData meshData, float edgeWidth)
        {
            if (edgeParent == null)
            {
                edgeParent = new GameObject("GridEdges");
                edgeParent.transform.SetParent(gridParent.transform);
            }
            var vertices = meshData.vertices;
            var edgeSet = new HashSet<(int, int)>();
            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                foreach (var face in Sylves.MeshUtils.GetFaces(meshData, submesh))
                {
                    int count = face.Count;
                    for (int j = 0; j < count; j++)
                    {
                        int a = face[j];
                        int b = face[(j + 1) % count];
                        var edge = a < b ? (a, b) : (b, a);
                        edgeSet.Add(edge);
                    }
                }
            }
            int edgeCount = 0;
            foreach (var (a, b) in edgeSet)
            {
                GameObject edgeObj = GetOrCreateEdgeObject(edgeCount);
                var start = vertices[a];
                var end = vertices[b];
                var dir = end - start;
                edgeObj.transform.position = (start + end) / 2f;
                edgeObj.transform.up = dir.normalized;
                edgeObj.transform.localScale = new Vector3(edgeWidth, dir.magnitude / 2f, 0.05f);
                // Set color to white for original edges
                var renderer = edgeObj.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial.color = Color.white;
                edgeCount++;
                await UniTask.Delay(TimeSpan.FromMilliseconds(0.01f));
            }
            DisableUnusedEdges(edgeCount);
        }

        private GameObject GetOrCreateEdgeObject(int index)
        {
            if (index < edgePool.Count)
            {
                edgePool[index].SetActive(true);
                return edgePool[index];
            }
            var edgeObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            edgeObj.transform.SetParent(edgeParent.transform);
            Object.DestroyImmediate(edgeObj.GetComponent<Collider>());
            edgePool.Add(edgeObj);
            return edgeObj;
        }

        private void DisableUnusedEdges(int usedCount)
        {
            for (int i = usedCount; i < edgePool.Count; i++)
                edgePool[i].SetActive(false);
        }

        private static HashSet<int> FindBoundaryVertices(MeshData meshData)
        {
            var edgeCount = new Dictionary<(int, int), int>();
            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                var indices = meshData.GetIndices(submesh);
                var topology = meshData.GetTopology(submesh);
                int step = topology == Sylves.MeshTopology.Quads ? 4 : 3;
                for (int i = 0; i < indices.Length; i += step)
                {
                    for (int j = 0; j < step; j++)
                    {
                        int a = indices[i + j];
                        int b = indices[i + (j + 1) % step];
                        var edge = a < b ? (a, b) : (b, a);
                        if (!edgeCount.ContainsKey(edge)) edgeCount[edge] = 0;
                        edgeCount[edge]++;
                    }
                }
            }
            var boundaryVerts = new HashSet<int>();
            foreach (var kvp in edgeCount)
            {
                if (kvp.Value == 1)
                {
                    boundaryVerts.Add(kvp.Key.Item1);
                    boundaryVerts.Add(kvp.Key.Item2);
                }
            }
            return boundaryVerts;
        }

        private static void RelaxMesh(MeshData meshData, int iterations = 1)
        {
            var boundary = FindBoundaryVertices(meshData);
            var verts = meshData.vertices;
            for (int iter = 0; iter < iterations; iter++)
            {
                var newVerts = (Vector3[])verts.Clone();
                for (int i = 0; i < verts.Length; i++)
                {
                    if (boundary.Contains(i)) continue;
                    var sum = Vector3.zero;
                    int count = 0;
                    for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
                    {
                        var indices = meshData.GetIndices(submesh);
                        var topology = meshData.GetTopology(submesh);
                        int step = topology == Sylves.MeshTopology.Quads ? 4 : 3;
                        for (int k = 0; k < indices.Length; k += step)
                        {
                            for (int j = 0; j < step; j++)
                            {
                                if (indices[k + j] == i)
                                {
                                    int prev = indices[k + (j + step - 1) % step];
                                    int next = indices[k + (j + 1) % step];
                                    sum += verts[prev];
                                    sum += verts[next];
                                    count += 2;
                                }
                            }
                        }
                    }
                    if (count > 0)
                        newVerts[i] = (verts[i] + sum / count) / 2f;
                }
                verts = newVerts;
            }
            meshData.vertices = verts;
        }

        private static void SquarifyQuads(MeshData meshData)
        {
            var verts = meshData.vertices;
            var boundary = FindBoundaryVertices(meshData);
            var forces = new Vector3[verts.Length];
            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                if (meshData.GetTopology(submesh) != Sylves.MeshTopology.Quads) continue;
                var indices = meshData.GetIndices(submesh);
                for (int i = 0; i < indices.Length; i += 4)
                {
                    int[] quad = { indices[i], indices[i+1], indices[i+2], indices[i+3] };
                    Vector3 center = (verts[quad[0]] + verts[quad[1]] + verts[quad[2]] + verts[quad[3]]) / 4f;
                    Vector3 force = Vector3.zero;
                    for (int j = 0; j < 4; j++)
                    {
                        force += verts[quad[j]] - center;
                        force = new Vector3(force.y, -force.x, 0f);
                    }
                    Vector3 rotatedForce = force;
                    for (int j = 0; j < 4; j++)
                    {
                        if (!boundary.Contains(quad[j]))
                            forces[quad[j]] += (center + rotatedForce - verts[quad[j]]) * 0.5f;
                        rotatedForce = new Vector3(rotatedForce.y, -rotatedForce.x, 0f);
                    }
                }
            }
            for (int i = 0; i < verts.Length; i++)
                verts[i] += forces[i];
            meshData.vertices = verts;
        }

        private static void PerturbBoundary(MeshData meshData, float boundaryMagnitude, float smoothness, float interiorMagnitude)
        {
            var boundary = FindBoundaryVertices(meshData);
            var verts = meshData.vertices;
            var rand = new System.Random();
            for (int i = 0; i < verts.Length; i++)
            {
                float angle = (float)(rand.NextDouble() * Math.PI * 2);
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                float offset = (float)rand.NextDouble() * (boundary.Contains(i) ? boundaryMagnitude : interiorMagnitude);
                verts[i] += dir * offset;
            }
            var edgeCount = new Dictionary<(int, int), int>();
            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                var indices = meshData.GetIndices(submesh);
                var topology = meshData.GetTopology(submesh);
                int step = topology == Sylves.MeshTopology.Quads ? 4 : 3;
                for (int i = 0; i < indices.Length; i += step)
                {
                    for (int j = 0; j < step; j++)
                    {
                        int a = indices[i + j];
                        int b = indices[i + (j + 1) % step];
                        var edge = a < b ? (a, b) : (b, a);
                        if (!edgeCount.ContainsKey(edge)) edgeCount[edge] = 0;
                        edgeCount[edge]++;
                    }
                }
            }
            var boundaryEdges = new List<(int, int)>();
            foreach (var kvp in edgeCount)
                if (kvp.Value == 1)
                    boundaryEdges.Add(kvp.Key);
            var adjacency = new Dictionary<int, List<int>>();
            foreach (var (a, b) in boundaryEdges)
            {
                if (!adjacency.ContainsKey(a)) adjacency[a] = new List<int>();
                if (!adjacency.ContainsKey(b)) adjacency[b] = new List<int>();
                adjacency[a].Add(b);
                adjacency[b].Add(a);
            }
            var newVerts = (Vector3[])verts.Clone();
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 sum = verts[i];
                int count = 1;
                if (adjacency.ContainsKey(i) && adjacency[i].Count > 0)
                {
                    foreach (var n in adjacency[i])
                    {
                        sum += verts[n];
                        count++;
                    }
                }
                else
                {
                    for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
                    {
                        var indices = meshData.GetIndices(submesh);
                        var topology = meshData.GetTopology(submesh);
                        int step = topology == Sylves.MeshTopology.Quads ? 4 : 3;
                        for (int k = 0; k < indices.Length; k += step)
                        {
                            for (int j = 0; j < step; j++)
                            {
                                if (indices[k + j] == i)
                                {
                                    for (int m = 0; m < step; m++)
                                    {
                                        if (m != j)
                                        {
                                            sum += verts[indices[k + m]];
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                newVerts[i] = Vector3.Lerp(verts[i], sum / count, Mathf.Clamp01(smoothness));
            }
            meshData.vertices = newVerts;
        }

        private async UniTask<(GameObject dualEdgeParent, Vector3[] verts, List<List<int>> faceIndices, List<Vector3> faceCenters, Dictionary<(int, int), Vector3> boundaryEdgeToPoint, List<Vector3> boundaryPoints)> CreateDualGridOverlay(MeshData meshData)
        {
            var verts = meshData.vertices;
            var dualEdgeParent = new GameObject("DualGridEdges");
            dualEdgeParent.transform.SetParent(gridParent.transform);

            var faceCenters = new List<Vector3>();
            var faceIndices = new List<List<int>>();

            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                foreach (var face in MeshUtils.GetFaces(meshData, submesh))
                {
                    Vector3 center = Vector3.zero;
                    foreach (var idx in face)
                        center += verts[idx];
                    center /= face.Count;
                    faceCenters.Add(center);
                    faceIndices.Add(new List<int>(face));
                }
            }

            var edgeToFace = new Dictionary<(int, int), List<int>>();
            for (int f = 0; f < faceIndices.Count; f++)
            {
                var face = faceIndices[f];
                int count = face.Count;
                for (int i = 0; i < count; i++)
                {
                    int a = face[i];
                    int b = face[(i + 1) % count];
                    var edge = a < b ? (a, b) : (b, a);
                    if (!edgeToFace.ContainsKey(edge)) edgeToFace[edge] = new List<int>();
                    edgeToFace[edge].Add(f);
                }
            }

            var boundaryPoints = new List<Vector3>();
            var boundaryEdges = new List<(int, int, int)>();
            var boundaryEdgeToPoint = new Dictionary<(int, int), Vector3>();

            foreach (var kvp in edgeToFace)
            {
                var edge = kvp.Key;
                var faces = kvp.Value;
                if (faces.Count == 1)
                {
                    int a = edge.Item1;
                    int b = edge.Item2;
                    int faceIdx = faces[0];
                    Vector3 va = verts[a];
                    Vector3 vb = verts[b];
                    Vector3 mid = (va + vb) / 2f;
                    Vector3 faceCenter = faceCenters[faceIdx];
                    Vector3 edgeDir = (vb - va).normalized;
                    Vector3 toCenter = (faceCenter - mid).normalized;
                    Vector3 normal = Vector3.Cross(edgeDir, Vector3.forward).normalized;
                    if (Vector3.Dot(normal, toCenter) > 0) normal = -normal;
                    Vector3 boundaryPoint = mid + normal * 0.1f;
                    boundaryPoints.Add(boundaryPoint);
                    boundaryEdgeToPoint[(a < b ? (a, b) : (b, a))] = boundaryPoint;
                }
            }

            if (boundaryPoints.Count > 2)
            {
                Vector3 centroid = boundaryPoints.Aggregate(Vector3.zero, (acc, p) => acc + p) / boundaryPoints.Count;
                boundaryPoints.Sort((p1, p2) =>
                {
                    float a1 = Mathf.Atan2(p1.y - centroid.y, p1.x - centroid.x);
                    float a2 = Mathf.Atan2(p2.y - centroid.y, p2.x - centroid.x);
                    return a1.CompareTo(a2);
                });
            }

            return (dualEdgeParent, verts, faceIndices, faceCenters, boundaryEdgeToPoint, boundaryPoints);
        }
        
        private async UniTask CreateIntersectionFacesAndCleanup(
            SubFaceGrid subFaceGrid,
            GameObject dualEdgeParent,
            Vector3[] verts,
            List<List<int>> faceIndices,
            List<Vector3> faceCenters,
            Dictionary<(int, int), Vector3> boundaryEdgeToPoint,
            List<Vector3> boundaryPoints)
        {
            Object.DestroyImmediate(dualEdgeParent);
            var facesParent = new GameObject("IntersectionFaces");
            facesParent.transform.SetParent(gridParent.transform);
            await SubdivideAllFaces(subFaceGrid, facesParent, verts, faceIndices, faceCenters);

            if (boundaryPoints.Count > 2)
            {
                // Subdivide boundary ring by treating it as a polygon
                Vector3 boundaryCenter = boundaryPoints.Aggregate(Vector3.zero, (acc, p) => acc + p) / boundaryPoints.Count;
                var edgeMidpoints = new List<Vector3>();
                for (int i = 0; i < boundaryPoints.Count; i++)
                {
                    Vector3 p1 = boundaryPoints[i];
                    Vector3 p2 = boundaryPoints[(i + 1) % boundaryPoints.Count];
                    edgeMidpoints.Add((p1 + p2) * 0.5f);
                }

                for (int i = 0; i < boundaryPoints.Count; i++)
                {
                    Vector3 corner = boundaryPoints[i];
                    Vector3 prevMid = edgeMidpoints[(i - 1 + edgeMidpoints.Count) % edgeMidpoints.Count];
                    Vector3 nextMid = edgeMidpoints[i];

                    await CreateSubFace(
                        subFaceGrid,
                        facesParent,
                        new[] { boundaryCenter, prevMid, corner, nextMid },
                        faceIndex: -1,
                        subFaceIndex: i
                    );
                }
            }

            if (edgeParent != null)
            {
                Object.DestroyImmediate(edgeParent);
                edgeParent = null;
                edgePool.Clear();
            }
        }

        private static List<Vector3> GetOrderedBoundaryPolygon(MeshData meshData)
        {
            // Step 1: Build edge counts to find boundary edges
            var edgeCounts = new Dictionary<(int, int), int>();
            for (int submesh = 0; submesh < meshData.subMeshCount; submesh++)
            {
                var indices = meshData.GetIndices(submesh);
                var topology = meshData.GetTopology(submesh);
                int step = topology == Sylves.MeshTopology.Quads ? 4 : 3;
                for (int i = 0; i < indices.Length; i += step)
                {
                    for (int j = 0; j < step; j++)
                    {
                        int a = indices[i + j];
                        int b = indices[i + (j + 1) % step];
                        var edge = a < b ? (a, b) : (b, a);
                        if (!edgeCounts.ContainsKey(edge)) edgeCounts[edge] = 0;
                        edgeCounts[edge]++;
                    }
                }
            }
            // Step 2: Extract all boundary edges (appear once)
            var boundaryEdges = edgeCounts.Where(kvp => kvp.Value == 1).Select(kvp => kvp.Key).ToList();

            // Step 3: Build adjacency map for boundary vertices
            var nexts = new Dictionary<int, int>();
            var prevs = new Dictionary<int, int>();
            foreach (var (a, b) in boundaryEdges)
            {
                if (!nexts.ContainsKey(a)) nexts[a] = b;
                if (!prevs.ContainsKey(b)) prevs[b] = a;
            }
            // Step 4: Find start vertex (vertex with only one connection)
            int start = boundaryEdges[0].Item1;
            var boundary = new List<int>();
            boundary.Add(start);
            int curr = start;
            // Step 5: Walk along the boundary loop
            while (true)
            {
                if (!nexts.ContainsKey(curr)) break;
                int next = nexts[curr];
                if (next == start) break;
                boundary.Add(next);
                curr = next;
            }
            // Step 6: Build ordered list of positions
            var verts = meshData.vertices;
            var orderedPoints = new List<Vector3>();
            foreach (var idx in boundary)
                orderedPoints.Add(verts[idx]);
            return orderedPoints;
        }
        
        private async UniTask SubdivideAllFaces(
            SubFaceGrid subFaceGrid,
            GameObject facesParent,
            Vector3[] verts,
            List<List<int>> faceIndices,
            List<Vector3> faceCenters)
        {
            for (int f = 0; f < faceIndices.Count; f++)
                await SubdivideFace(subFaceGrid, facesParent, verts, faceIndices[f], faceCenters[f], f);
        }

        private async UniTask SubdivideFace(
            SubFaceGrid subFaceGrid,
            GameObject facesParent,
            Vector3[] verts,
            List<int> face,
            Vector3 cellCenter,
            int faceIndex)
        {
            int count = face.Count;
            if (count < 3) return;
            var edgeMidpoints = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector3 v1 = verts[face[i]];
                Vector3 v2 = verts[face[(i + 1) % count]];
                edgeMidpoints[i] = (v1 + v2) * 0.5f;
            }
            for (int i = 0; i < count; i++)
            {
                Vector3 vCorner = verts[face[i]];
                Vector3 vEdgePrev = edgeMidpoints[(i - 1 + count) % count];
                Vector3 vEdgeNext = edgeMidpoints[i];
                if (count == 3)
                    await CreateSubFace(subFaceGrid, facesParent, new[] { cellCenter, vEdgePrev, vCorner }, faceIndex, i);
                else
                    await CreateSubFace(subFaceGrid, facesParent, new[] { cellCenter, vEdgePrev, vCorner, vEdgeNext }, faceIndex, i);
            }
        }

        private async UniTask CreateSubFace(
            SubFaceGrid subFaceGrid,
            GameObject facesParent,
            Vector3[] subFaceVerts,
            int faceIndex,
            int subFaceIndex)
        {
            // Add sub-face data to the grid for selection/manipulation
            subFaceGrid.cells.Add(new SubFaceCell((Vector3[])subFaceVerts.Clone(), faceIndex, subFaceIndex));
            // ...existing code...
            var mesh = new Mesh();
            mesh.vertices = subFaceVerts;
            if (subFaceVerts.Length == 3)
                mesh.triangles = new int[] { 0, 1, 2 };
            else
                mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var go = new GameObject($"SubFace_{faceIndex}_{subFaceIndex}");
            go.transform.SetParent(facesParent.transform);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
            mr.material = mat;
            await UniTask.Delay(TimeSpan.FromMilliseconds(0.01f));
        }

        public void Dispose()
        {
            Object.DestroyImmediate(gridParent);
        }
    }
}
