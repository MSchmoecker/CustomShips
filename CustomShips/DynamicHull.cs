using System;
using UnityEngine;

namespace CustomShips {
    public class DynamicHull : MonoBehaviour {
        public int segments = 2;
        public float width = 0.2f;
        public float offsetY = -0.1f;
        public Rect uvRect = new Rect(0.54f, 0.04f, 0.08f, 0.1f);

        private Hull hull;

        private void Awake() {
            hull = GetComponent<Hull>();

            hull.OnChange += () => {
                if (hull.leftRib && hull.rightRib) {
                    RegenerateMesh(hull.leftRib.size + 0.1f, hull.rightRib.size + 0.1f, 0.9f, 0, 0);
                } else if (hull.leftRib) {
                    RegenerateMesh(hull.leftRib.size + 0.1f, 0.1f, 0.9f, 0, 0.4f);
                } else if (hull.rightRib) {
                    RegenerateMesh(0.1f, hull.rightRib.size + 0.1f, 0.9f, 0.4f, 0);
                }
            };
        }

        private void MakeTriangle(int index, int[] triangles, int a, int b, int c) {
            triangles[index] = a;
            triangles[index + 1] = b;
            triangles[index + 2] = c;
        }

        public void RegenerateMesh(float left, float right, float height, float startLeft, float startRight) {
            Vector3[] vertices = new Vector3[(segments + 1) * 4];
            int[] triangles = new int[(segments + 1) * 24 + 12];
            Vector2[] uv = new Vector2[vertices.Length];

            // start cap
            MakeTriangle(0, triangles, 0, 2, 1);
            MakeTriangle(3, triangles, 1, 2, 3);

            // end cap
            MakeTriangle(6, triangles, vertices.Length - 4, vertices.Length - 3, vertices.Length - 2);
            MakeTriangle(9, triangles, vertices.Length - 3, vertices.Length - 1, vertices.Length - 2);

            for (int segment = 0; segment < segments + 1; segment++) {
                float t = (float)segment / segments;
                float tScaled = t; // Mathf.Pow(t, 0.5f);
                float halfWidth = width / 2f;

                float curve = 0.2f * tScaled + 0.6f * Mathf.Pow(tScaled, 3) + 0.4f * Mathf.Pow(tScaled, 10);
                float segmentHeight = (height - halfWidth) * curve;
                float angle = Mathf.Lerp(0, Mathf.PI / 2f, segmentHeight);
                float lengthLeft = Mathf.Lerp(0, -left, tScaled);
                float lengthRight = Mathf.Lerp(0, -right, tScaled);

                float sinHalf = Mathf.Sin(angle) * halfWidth;
                float cosHalf = Mathf.Cos(angle) * halfWidth;
                float heightLeft = Mathf.Max(0, segmentHeight - startLeft) + startLeft;
                float heightRight = Mathf.Max(0, segmentHeight - startRight) + startRight;

                vertices[segment * 4 + 0] = new Vector3(lengthLeft - sinHalf, heightLeft - cosHalf + offsetY, -1f);
                vertices[segment * 4 + 1] = new Vector3(lengthRight - sinHalf, heightRight - cosHalf + offsetY, 1f);
                vertices[segment * 4 + 2] = new Vector3(lengthLeft + sinHalf, heightLeft + cosHalf + offsetY, -1f);
                vertices[segment * 4 + 3] = new Vector3(lengthRight + sinHalf, heightRight + cosHalf + offsetY, 1f);

                if (segment < segments) {
                    // top
                    MakeTriangle(12 + segment * 24, triangles, segment * 4 + 2, (segment + 1) * 4 + 2, segment * 4 + 3);
                    MakeTriangle(15 + segment * 24, triangles, segment * 4 + 3, (segment + 1) * 4 + 2, (segment + 1) * 4 + 3);

                    // bottom
                    MakeTriangle(18 + segment * 24, triangles, segment * 4 + 0, segment * 4 + 1, (segment + 1) * 4 + 0);
                    MakeTriangle(21 + segment * 24, triangles, (segment + 1) * 4 + 0, segment * 4 + 1, (segment + 1) * 4 + 1);

                    // right
                    MakeTriangle(24 + segment * 24, triangles, segment * 4 + 0, (segment + 1) * 4 + 0, segment * 4 + 2);
                    MakeTriangle(27 + segment * 24, triangles, (segment + 1) * 4 + 0, (segment + 1) * 4 + 2, segment * 4 + 2);

                    // left
                    MakeTriangle(30 + segment * 24, triangles, segment * 4 + 1, segment * 4 + 3, (segment + 1) * 4 + 1);
                    MakeTriangle(33 + segment * 24, triangles, (segment + 1) * 4 + 1, segment * 4 + 3, (segment + 1) * 4 + 3);
                }

                float uvY = Mathf.LerpUnclamped(uvRect.yMin, uvRect.yMax, tScaled);

                uv[segment * 4 + 0] = new Vector2(uvRect.xMin, uvY + uvRect.height / 10f);
                uv[segment * 4 + 1] = new Vector2(uvRect.xMax, uvY + uvRect.height / 10f);
                uv[segment * 4 + 2] = new Vector2(uvRect.xMin, uvY);
                uv[segment * 4 + 3] = new Vector2(uvRect.xMax, uvY);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;

            if (!hull) {
                hull = GetComponent<Hull>();
            }

            if (hull.mainCollider is MeshCollider mainCollider) {
                mainCollider.sharedMesh = mesh;
            }
        }
    }
}
