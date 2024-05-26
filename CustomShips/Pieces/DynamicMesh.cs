using System;
using UnityEngine;

namespace CustomShips.Pieces {
    public class DynamicMesh : MonoBehaviour {
        public int segments = 2;
        public int splits = 1;
        public float width = 0.2f;
        public float height = 1f;
        public float sideWidthInceaseOffset = 0f;

        public bool rotateMesh = true;
        public bool useCurve = true;

        public Vector3 meshOffset;
        public Vector3 colliderOffset;
        public Rect uvRect = new Rect(0.54f, 0.04f, 0.08f, 0.1f);

        private AnimationCurve curve = new AnimationCurve();
        private Hull hull;

        private float sideWidthIncease = 10f;

        private void Awake() {
            hull = GetComponentInParent<Hull>();

            hull.OnChange += () => {
                UpdateCurve();

                bool generate = false;
                float left = 2f;
                float right = 2f;
                float startLeft = 0f;
                float startRight = 0f;
                float relY = 0f;

                if (hull.leftRib && hull.rightRib) {
                    generate = true;
                    left = hull.leftRib.size + 0.1f;
                    right = hull.rightRib.size + 0.1f;
                    startLeft = 0;
                    startRight = 0;
                    relY = Mathf.Min(transform.position.y - hull.leftRib.transform.position.y, transform.position.y - hull.rightRib.transform.position.y);
                } else if (hull.leftRib) {
                    generate = true;
                    left = hull.leftRib.size + 0.1f;
                    right = -0.1f;
                    startLeft = 0;
                    startRight = 0.4f;
                    relY = transform.position.y - hull.leftRib.transform.position.y;
                } else if (hull.rightRib) {
                    generate = true;
                    left = -0.1f;
                    right = hull.rightRib.size + 0.1f;
                    startLeft = 0.4f;
                    startRight = 0;
                    relY = transform.position.y - hull.rightRib.transform.position.y;
                }

                if (generate) {
                    RegenerateMesh(left, right, Mathf.Abs(relY) < 0.1f ? startLeft : 0f, Mathf.Abs(relY) < 0.1f ? startRight : 0f, relY);
                }
            };
        }

        private void UpdateCurve() {
            float preLeft = 0f;
            float left = hull.leftRib ? hull.leftRib.size : -0.1f;
            float right = hull.rightRib ? hull.rightRib.size : -0.1f;
            float preRight = 0f;

            if (hull.leftRib) {
                if (hull.leftRib.leftRib) {
                    preLeft = hull.leftRib.leftRib.size;
                } else if (hull.rightRib) {
                    preLeft = Mathf.Min(hull.leftRib.size, hull.rightRib.size) -
                              Mathf.Clamp(Mathf.Abs(hull.leftRib.size - hull.rightRib.size), 0.1f, 0.6f);
                } else {
                    preLeft = -0.1f;
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
                    preRight = -0.1f;
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

        public void RegenerateMesh(float left, float right, float startLeft, float startRight, float relY) {
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

                float heightCurve = 0.2f * t + 0.6f * Mathf.Pow(t, 3) + 0.2f * Mathf.Pow(t, 10);
                float segmentHeight = (height - meshOffset.y) * heightCurve;
                float angle = Mathf.Lerp(0, Mathf.PI / 2f, segmentHeight);

                if (!rotateMesh) {
                    angle = Mathf.PI / 2f;
                }

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

                    float x = -curve.Evaluate(splitT * 2f - 1f) - width;
                    float y = Mathf.Max(0, segmentHeight - startY) + startY;
                    float z = Mathf.Lerp(-1f, 1f, splitT);

                    Vector3 top;
                    Vector3 bottom;

                    float sideT;

                    if (useCurve) {
                        sideT = ((relY + sideWidthInceaseOffset) * t) / sideWidthIncease;
                        top = new Vector3(x * (t + sideT) + sinHalf, y + cosHalf, z) + meshOffset;
                        bottom = new Vector3(x * (t + sideT) - sinHalf, y - cosHalf, z) + meshOffset;
                    } else {
                        sideT = (relY - 1f + height * t) / sideWidthIncease;
                        top = new Vector3(x * (1f + sideT) + sinHalf, y + cosHalf, z) + meshOffset;
                        bottom = new Vector3(x * (1f + sideT) - sinHalf, y - cosHalf, z) + meshOffset;
                    }

                    vertices[GetTopVertice(segment, split)] = top;
                    vertices[GetBottomVertice(segment, split)] = bottom;

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
                        vertices[vertY + split * 2 + 0] = top;
                        vertices[vertY + split * 2 + 1] = bottom;

                        uv[vertY + split * 2 + 0] = new Vector2(uvX, uvY + uvRect.height / 10f);
                        uv[vertY + split * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (segment == segments) {
                        // back
                        vertices[vertY + frontVertices + split * 2 + 0] = top;
                        vertices[vertY + frontVertices + split * 2 + 1] = bottom;

                        uv[vertY + frontVertices + split * 2 + 0] = new Vector2(uvX, uvY + uvRect.height / 10f);
                        uv[vertY + frontVertices + split * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (split == 0) {
                        // right
                        vertices[vertX + segment * 2 + 0] = top;
                        vertices[vertX + segment * 2 + 1] = bottom;

                        uv[vertX + segment * 2 + 0] = new Vector2(uvX + uvRect.height / 10f, uvY);
                        uv[vertX + segment * 2 + 1] = new Vector2(uvX, uvY);
                    }

                    if (split == splits + 1) {
                        // left
                        vertices[vertX + rightVertices + segment * 2 + 0] = top;
                        vertices[vertX + rightVertices + segment * 2 + 1] = bottom;

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
                mainCollider.sharedMesh = GenerateBottomCollider(left + 0.1f, right + 0.1f, colliderOffset);
            }

            if (hull.sideCollider) {
                hull.sideCollider.sharedMesh = GenerateSideCollider(left, right, 0f, height);
            }

            if (hull.watermask) {
                hull.watermask.mesh = GenerateWatermask(left, right);
            }
        }

        private Mesh GenerateWatermask(float left, float right) {
            Vector3[] vertices = new Vector3[8];
            int[] triangles = new int[18];
            int triangle = 0;

            vertices[0] = new Vector3(0f, height, -1f);
            vertices[1] = new Vector3(0f, height, -0.333f);
            vertices[2] = new Vector3(0f, height, 0.333f);
            vertices[3] = new Vector3(0f, height, 1f);
            vertices[4] = new Vector3(-left - width / 2f, height, -1f);
            vertices[5] = new Vector3(-curve.Evaluate(-0.333f) - width, height, -0.333f);
            vertices[6] = new Vector3(-curve.Evaluate(0.333f) - width, height, 0.333f);
            vertices[7] = new Vector3(-right - width / 2f, height, 1f);

            MakeFace(ref triangle, triangles, 0, 1, 4, 5);
            MakeFace(ref triangle, triangles, 1, 2, 5, 6);
            MakeFace(ref triangle, triangles, 2, 3, 6, 7);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh GenerateBottomCollider(float left, float right, Vector3 offset) {
            Vector3[] vertices = new Vector3[8];
            int[] triangles = new int[12];
            int triangle = 0;

            vertices[0] = new Vector3(0, +width / 2f, -1f) + offset;
            vertices[1] = new Vector3(0, width / 2f, 1f) + offset;
            vertices[2] = new Vector3(-left, width / 2f, -1f) + offset;
            vertices[3] = new Vector3(-right, width / 2f, 1f) + offset;
            vertices[4] = new Vector3(0, -width / 2f, -1f) + offset;
            vertices[5] = new Vector3(0, -width / 2f, 1f) + offset;
            vertices[6] = new Vector3(-left, -width / 2f, -1f) + offset;
            vertices[7] = new Vector3(-right, -width / 2f, 1f) + offset;

            MakeFace(ref triangle, triangles, 0, 1, 2, 3);
            MakeFace(ref triangle, triangles, 4, 5, 6, 7);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh GenerateSideCollider(float left, float right, float startHeight, float endHeight) {
            Vector3[] vertices = new Vector3[8];
            int[] triangles = new int[12];
            int triangle = 0;

            vertices[0] = new Vector3(-left + width / 2f, startHeight, -1f);
            vertices[1] = new Vector3(-right + width / 2f, startHeight, 1f);
            vertices[2] = new Vector3(-left + width / 2f, endHeight, -1f);
            vertices[3] = new Vector3(-right + width / 2f, endHeight, 1f);
            vertices[4] = new Vector3(-left - width / 2f, startHeight, -1f);
            vertices[5] = new Vector3(-right - width / 2f, startHeight, 1f);
            vertices[6] = new Vector3(-left - width / 2f, endHeight, -1f);
            vertices[7] = new Vector3(-right - width / 2f, endHeight, 1f);

            MakeFace(ref triangle, triangles, 0, 1, 2, 3);
            MakeFace(ref triangle, triangles, 4, 5, 6, 7);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
