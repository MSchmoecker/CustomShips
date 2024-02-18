using System;
using UnityEngine;

namespace CustomShips.Pieces {
    public class DynamicHull : MonoBehaviour {
        public int segments = 2;
        public int splits = 1;
        public float width = 0.2f;
        public float offsetY = -0.1f;
        public Rect uvRect = new Rect(0.54f, 0.04f, 0.08f, 0.1f);

        private AnimationCurve curve = new AnimationCurve();
        private Hull hull;

        private void Awake() {
            hull = GetComponent<Hull>();

            hull.OnChange += () => {
                UpdateCurve();

                if (hull.leftRib && hull.rightRib) {
                    RegenerateMesh(hull.leftRib.size + 0.1f, hull.rightRib.size + 0.1f, 0.9f, 0, 0);
                } else if (hull.leftRib) {
                    RegenerateMesh(hull.leftRib.size + 0.1f, 0.1f, 0.9f, 0, 0.4f);
                } else if (hull.rightRib) {
                    RegenerateMesh(0.1f, hull.rightRib.size + 0.1f, 0.9f, 0.4f, 0);
                }
            };
        }

        private void UpdateCurve() {
            curve.ClearKeys();
            curve.AddKey(-1f, hull.leftRib ? hull.leftRib.size : 0.1f);
            curve.AddKey(1f, hull.rightRib ? hull.rightRib.size : 0.1f);

            if (hull.preLeftRib) {
                curve.AddKey(-3f, hull.preLeftRib.size);
            } else {
                if (hull.leftRib) {
                    curve.AddKey(-3f, 0.1f);
                } else if (hull.rightRib) {
                    curve.AddKey(-3f, -hull.rightRib.size);
                } else {
                    curve.AddKey(-3f, 0f);
                }
            }

            if (hull.preRightRib) {
                curve.AddKey(3f, hull.preRightRib.size);
            } else {
                if (hull.rightRib) {
                    curve.AddKey(3f, 0.1f);
                } else if (hull.leftRib) {
                    curve.AddKey(3f, -hull.leftRib.size);
                } else {
                    curve.AddKey(3f, 0f);
                }
            }
        }

        private void MakeTriangle(int index, int[] triangles, int a, int b, int c) {
            if (index > triangles.Length - 3) {
                Debug.Log("Index " + index + " out of bounds " + triangles.Length);
                return;
            }

            triangles[index] = a;
            triangles[index + 1] = b;
            triangles[index + 2] = c;
        }

        private void MakeFace(ref int index, int[] triangles, int a, int b, int c, int d) {
            MakeTriangle(index, triangles, a, c, b);
            MakeTriangle(index + 3, triangles, b, c, d);
            index += 6;
        }

        private int GetVertice(int segment, int split, bool top) {
            return segment * (splits + 2) * 2 + split * 2 + (top ? 0 : 1);
        }

        public void RegenerateMesh(float left, float right, float height, float startLeft, float startRight) {
            Vector3[] vertices = new Vector3[(segments + 1) * (splits + 2) * 4];
            int[] triangles = new int[(segments + 1) * 4 * 6 * (splits + 1) + 12 * (splits + 1)];
            Vector2[] uv = new Vector2[vertices.Length];

            int triangle = 0;

            // start cap
            for (int split = 0; split <= splits; split++) {
                MakeFace(ref triangle, triangles,
                    GetVertice(0, split, false),
                    GetVertice(0, split + 1, false),
                    GetVertice(0, split, true),
                    GetVertice(0, split + 1, true)
                );
            }

            // end cap
            for (int split = 0; split <= splits; split++) {
                MakeFace(ref triangle, triangles,
                    GetVertice(segments, split, false),
                    GetVertice(segments, split, true),
                    GetVertice(segments, split + 1, false),
                    GetVertice(segments, split + 1, true)
                );
            }

            for (int segment = 0; segment < segments + 1; segment++) {
                float t = (float)segment / segments;
                float halfWidth = width / 2f;

                float heightCurve = 0.2f * t + 0.6f * Mathf.Pow(t, 3) + 0.4f * Mathf.Pow(t, 10);
                float segmentHeight = (height - halfWidth) * heightCurve;
                float angle = Mathf.Lerp(0, Mathf.PI / 2f, segmentHeight);

                float sinHalf = Mathf.Sin(angle) * halfWidth;
                float cosHalf = Mathf.Cos(angle) * halfWidth;

                for (int split = 0; split <= splits + 1; split++) {
                    float splitT = (float)split / (splits + 1);
                    float startY = startLeft * (1f - splitT) + startRight * splitT;

                    float x = -curve.Evaluate(splitT * 2f - 1f);
                    float y = Mathf.Max(0, segmentHeight - startY) + startY;
                    float z = Mathf.Lerp(-1f, 1f, splitT);

                    vertices[GetVertice(segment, split, true)] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                    vertices[GetVertice(segment, split, false)] = new Vector3(x * t - sinHalf, y - cosHalf + offsetY, z);

                    if (segment < segments && split < splits + 1) {
                        // top
                        MakeFace(ref triangle, triangles,
                            GetVertice(segment + 0, split + 0, true),
                            GetVertice(segment + 0, split + 1, true),
                            GetVertice(segment + 1, split + 0, true),
                            GetVertice(segment + 1, split + 1, true)
                        );

                        // bottom
                        MakeFace(ref triangle, triangles,
                            GetVertice(segment + 0, split + 0, false),
                            GetVertice(segment + 1, split + 0, false),
                            GetVertice(segment + 0, split + 1, false),
                            GetVertice(segment + 1, split + 1, false)
                        );

                        // right
                        if (split == 0) {
                            MakeFace(ref triangle, triangles,
                                GetVertice(segment + 1, split + 0, false),
                                GetVertice(segment + 0, split + 0, false),
                                GetVertice(segment + 1, split + 0, true),
                                GetVertice(segment + 0, split + 0, true)
                            );
                        }

                        // left
                        if (split == splits) {
                            MakeFace(ref triangle, triangles,
                                GetVertice(segment + 0, split + 1, false),
                                GetVertice(segment + 1, split + 1, false),
                                GetVertice(segment + 0, split + 1, true),
                                GetVertice(segment + 1, split + 1, true)
                            );
                        }
                    }

                    float uvX = Mathf.LerpUnclamped(uvRect.xMin, uvRect.xMax, (float)split / (splits + 1));
                    float uvY = Mathf.LerpUnclamped(uvRect.yMin, uvRect.yMax, t);

                    uv[GetVertice(segment, split, true)] = new Vector2(uvX, uvY + uvRect.height / 10f);
                    uv[GetVertice(segment, split, false)] = new Vector2(uvX, uvY);
                }
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
                mainCollider.sharedMesh = GenerateBottomCollider(left + 0.1f, right + 0.1f, 0.1f, 0.2f);
            }

            if (hull.sideCollider) {
                hull.sideCollider.sharedMesh = GenerateSideCollider(left, right, 0f, height, 0.2f);
            }

            if (hull.watermask) {
                hull.watermask.mesh = GenerateWatermask(left, right, height);
            }
        }

        private Mesh GenerateWatermask(float left, float right, float height) {
            Vector3[] vertices = new Vector3[4];
            int[] triangles = new int[6];

            vertices[0] = new Vector3(0, height, -1f);
            vertices[1] = new Vector3(0, height, 1f);
            vertices[2] = new Vector3(-left, height, -1f);
            vertices[3] = new Vector3(-right, height, 1f);

            MakeTriangle(0, triangles, 0, 2, 1);
            MakeTriangle(3, triangles, 1, 2, 3);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh GenerateBottomCollider(float left, float right, float offset, float colliderWidth) {
            Vector3[] vertices = new Vector3[8];
            int[] triangles = new int[12];

            vertices[0] = new Vector3(0, offset + colliderWidth / 2f, -1f);
            vertices[1] = new Vector3(0, offset + colliderWidth / 2f, 1f);
            vertices[2] = new Vector3(-left, offset + colliderWidth / 2f, -1f);
            vertices[3] = new Vector3(-right, offset + colliderWidth / 2f, 1f);
            vertices[4] = new Vector3(0, offset - colliderWidth / 2f, -1f);
            vertices[5] = new Vector3(0, offset - colliderWidth / 2f, 1f);
            vertices[6] = new Vector3(-left, offset - colliderWidth / 2f, -1f);
            vertices[7] = new Vector3(-right, offset - colliderWidth / 2f, 1f);

            MakeTriangle(0, triangles, 0, 2, 1);
            MakeTriangle(3, triangles, 1, 2, 3);
            MakeTriangle(6, triangles, 3 + 0, 3 + 2, 3 + 1);
            MakeTriangle(9, triangles, 3 + 1, 3 + 2, 3 + 3);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh GenerateSideCollider(float left, float right, float startHeight, float endHeight, float colliderWidth) {
            Vector3[] vertices = new Vector3[8];
            int[] triangles = new int[12];

            vertices[0] = new Vector3(-left + colliderWidth / 2f, startHeight, -1f);
            vertices[1] = new Vector3(-right + colliderWidth / 2f, startHeight, 1f);
            vertices[2] = new Vector3(-left + colliderWidth / 2f, endHeight, -1f);
            vertices[3] = new Vector3(-right + colliderWidth / 2f, endHeight, 1f);
            vertices[4] = new Vector3(-left - colliderWidth / 2f, startHeight, -1f);
            vertices[5] = new Vector3(-right - colliderWidth / 2f, startHeight, 1f);
            vertices[6] = new Vector3(-left - colliderWidth / 2f, endHeight, -1f);
            vertices[7] = new Vector3(-right - colliderWidth / 2f, endHeight, 1f);

            MakeTriangle(0, triangles, 0, 2, 1);
            MakeTriangle(3, triangles, 1, 2, 3);
            MakeTriangle(6, triangles, 3 + 0, 3 + 2, 3 + 1);
            MakeTriangle(9, triangles, 3 + 1, 3 + 2, 3 + 3);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
