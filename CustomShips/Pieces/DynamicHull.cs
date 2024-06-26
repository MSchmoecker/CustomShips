﻿using System;
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
            float preLeft = 0f;
            float left = hull.leftRib ? hull.leftRib.size : 0.1f;
            float right = hull.rightRib ? hull.rightRib.size : 0.1f;
            float preRight = 0f;

            if (hull.leftRib) {
                if (hull.leftRib.leftRib) {
                    preLeft = hull.leftRib.leftRib.size;
                } else if (hull.rightRib) {
                    preLeft = Mathf.Min(hull.leftRib.size, hull.rightRib.size) -
                              Mathf.Clamp(Mathf.Abs(hull.leftRib.size - hull.rightRib.size), 0.1f, 0.6f);
                } else {
                    preLeft = 0.1f;
                }
            } else if (hull.rightRib) {
                preLeft = -hull.rightRib.size * 2f;
            }

            if (hull.rightRib) {
                if (hull.rightRib.rightRib) {
                    preRight = hull.rightRib.rightRib.size;
                } else if (hull.leftRib) {
                    preRight = Mathf.Min(hull.leftRib.size, hull.rightRib.size) -
                               Mathf.Clamp(Mathf.Abs(hull.leftRib.size - hull.rightRib.size), 0.1f, 0.6f);
                } else {
                    preRight = 0.1f;
                }
            } else if (hull.leftRib) {
                preRight = -hull.leftRib.size * 2f;
            }

            curve.ClearKeys();
            curve.AddKey(-3f, preLeft);
            curve.AddKey(-1f, left);
            curve.AddKey(1f, right);
            curve.AddKey(3f, preRight);
        }

        public void UpdateCurve(float preLeft, float left, float right, float preRight) {
            curve.ClearKeys();
            curve.AddKey(-3f, preLeft);
            curve.AddKey(-1f, left);
            curve.AddKey(1f, right);
            curve.AddKey(3f, preRight);
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

        private int GetTopVertice(int segment, int split) {
            return segment * (splits + 2) * 2 + split * 2;
        }

        private int GetBottomVertice(int segment, int split) {
            return segment * (splits + 2) * 2 + split * 2 + 1;
        }

        public void RegenerateMesh(float left, float right, float height, float startLeft, float startRight) {
            int topVertices = (segments + 1) * (splits + 2);
            int bottomVertices = (segments + 1) * (splits + 2);
            int frontVertices = (splits + 2) * 4;
            int backVertices = (splits + 2) * 4;
            int rightVertices = (segments + 1) * 4;
            int leftVertices = (segments + 1) * 4;

            int vertY = topVertices + bottomVertices;
            int vertX = vertY + frontVertices + backVertices;

            Vector3[] vertices = new Vector3[
                topVertices + bottomVertices +
                frontVertices + backVertices +
                rightVertices + leftVertices
            ];

            int[] triangles = new int[
                segments * (splits + 1) * 6 * 2 +
                (splits + 1) * 6 * 2 +
                segments * 6 * 2
            ];

            Vector2[] uv = new Vector2[vertices.Length];

            int triangle = 0;

            // front
            for (int split = 0; split <= splits; split++) {
                MakeFace(ref triangle, triangles,
                    vertY + (split + 0) * 2 + 1,
                    vertY + (split + 1) * 2 + 1,
                    vertY + (split + 0) * 2 + 0,
                    vertY + (split + 1) * 2 + 0
                );
            }

            // back
            for (int split = 0; split <= splits; split++) {
                MakeFace(ref triangle, triangles,
                    vertY + frontVertices + (split + 0) * 2 + 0,
                    vertY + frontVertices + (split + 1) * 2 + 0,
                    vertY + frontVertices + (split + 0) * 2 + 1,
                    vertY + frontVertices + (split + 1) * 2 + 1
                );
            }

            for (int segment = 0; segment <= segments; segment++) {
                float t = (float)segment / segments;
                float halfWidth = width / 2f;

                float heightCurve = 0.2f * t + 0.6f * Mathf.Pow(t, 3) + 0.4f * Mathf.Pow(t, 10);
                float segmentHeight = (height - halfWidth) * heightCurve;
                float angle = Mathf.Lerp(0, Mathf.PI / 2f, segmentHeight);

                float sinHalf = Mathf.Sin(angle) * halfWidth;
                float cosHalf = Mathf.Cos(angle) * halfWidth;

                if (segment < segments) {
                    // right
                    MakeFace(ref triangle, triangles,
                        vertX + (segment + 0) * 2 + 0,
                        vertX + (segment + 1) * 2 + 0,
                        vertX + (segment + 0) * 2 + 1,
                        vertX + (segment + 1) * 2 + 1
                    );

                    // left
                    MakeFace(ref triangle, triangles,
                        vertX + rightVertices + (segment + 0) * 2 + 1,
                        vertX + rightVertices + (segment + 1) * 2 + 1,
                        vertX + rightVertices + (segment + 0) * 2 + 0,
                        vertX + rightVertices + (segment + 1) * 2 + 0
                    );
                }

                for (int split = 0; split <= splits + 1; split++) {
                    float splitT = (float)split / (splits + 1);
                    float startY = startLeft * (1f - splitT) + startRight * splitT;

                    float x = -curve.Evaluate(splitT * 2f - 1f) - width / 4f;
                    float y = Mathf.Max(0, segmentHeight - startY) + startY;
                    float z = Mathf.Lerp(-1f, 1f, splitT);

                    vertices[GetTopVertice(segment, split)] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                    vertices[GetBottomVertice(segment, split)] = new Vector3(x * t - sinHalf, y - cosHalf + offsetY, z);

                    if (segment < segments && split < splits + 1) {
                        // top
                        MakeFace(ref triangle, triangles,
                            GetTopVertice(segment + 0, split + 0),
                            GetTopVertice(segment + 0, split + 1),
                            GetTopVertice(segment + 1, split + 0),
                            GetTopVertice(segment + 1, split + 1)
                        );

                        // bottom
                        MakeFace(ref triangle, triangles,
                            GetBottomVertice(segment + 0, split + 0),
                            GetBottomVertice(segment + 1, split + 0),
                            GetBottomVertice(segment + 0, split + 1),
                            GetBottomVertice(segment + 1, split + 1)
                        );
                    }

                    float uvX = Mathf.LerpUnclamped(uvRect.xMin, uvRect.xMax, (float)split / (splits + 1));
                    float uvY = Mathf.LerpUnclamped(uvRect.yMin, uvRect.yMax, t);

                    uv[GetTopVertice(segment, split)] = new Vector2(uvX, uvY + uvRect.height / 10f);
                    uv[GetBottomVertice(segment, split)] = new Vector2(uvX, uvY);

                    if (segment == 0) {
                        // front
                        vertices[vertY + split * 2 + 0] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                        vertices[vertY + split * 2 + 1] = new Vector3(x * t + sinHalf, y - cosHalf + offsetY, z);

                        uv[vertY + split * 2 + 0] = new Vector2(uvX, uvY + uvRect.height / 10f);
                        uv[vertY + split * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (segment == segments) {
                        // back
                        vertices[vertY + frontVertices + split * 2 + 0] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                        vertices[vertY + frontVertices + split * 2 + 1] = new Vector3(x * t - sinHalf, y - cosHalf + offsetY, z);

                        uv[vertY + frontVertices + split * 2 + 0] = new Vector2(uvX, uvY + uvRect.height / 10f);
                        uv[vertY + frontVertices + split * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (split == 0) {
                        // right
                        vertices[vertX + segment * 2 + 0] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                        vertices[vertX + segment * 2 + 1] = new Vector3(x * t - sinHalf, y - cosHalf + offsetY, z);

                        uv[vertX + segment * 2 + 0] = new Vector2(uvX + uvRect.height / 10f, uvY);
                        uv[vertX + segment * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (split == splits + 1) {
                        // left
                        vertices[vertX + rightVertices + segment * 2 + 0] = new Vector3(x * t + sinHalf, y + cosHalf + offsetY, z);
                        vertices[vertX + rightVertices + segment * 2 + 1] = new Vector3(x * t - sinHalf, y - cosHalf + offsetY, z);

                        uv[vertX + rightVertices + segment * 2 + 0] = new Vector2(uvX + uvRect.height / 10f, uvY);
                        uv[vertX + rightVertices + segment * 2 + 1] = new Vector2(uvX, uvY);
                    }
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
