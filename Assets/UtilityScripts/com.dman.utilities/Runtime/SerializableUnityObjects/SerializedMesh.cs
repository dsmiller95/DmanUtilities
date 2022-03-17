using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializedMesh
    {
        float[] vertexes;
        float[] normals;
        SerializedSubmesh[] submeshDescriptors;
        int[] indexes;
        float[] uvs;

        [Serializable]
        class SerializedSubmesh
        {
            public int indexStart;
            public int indexCount;

            public SerializedSubmesh(SubMeshDescriptor subMesh)
            {
                indexStart = subMesh.indexStart;
                indexCount = subMesh.indexCount;
            }

            public SubMeshDescriptor ToDescriptor()
            {
                return new SubMeshDescriptor(indexStart, indexCount);
            }
        }

        public SerializedMesh(Mesh mesh)
        {
            vertexes = ToFloatArray(mesh.vertices);
            normals = ToFloatArray(mesh.normals);

            submeshDescriptors = new SerializedSubmesh[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var subMesh = mesh.GetSubMesh(i);
                submeshDescriptors[i] = new SerializedSubmesh(subMesh);
            }
            indexes = mesh.triangles;

            uvs = new float[mesh.vertexCount * 2];
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                var uv = mesh.uv[i];
                uvs[i * 2] = uv.x;
                uvs[i * 2 + 1] = uv.y;
            }
        }

        private static float[] ToFloatArray(Vector3[] source)
        {
            var result = new float[source.Length * 3];
            for (int i = 0; i < source.Length; i++)
            {
                var sourceVector = source[i];
                result[i * 3] = sourceVector.x;
                result[i * 3 + 1] = sourceVector.y;
                result[i * 3 + 2] = sourceVector.z;
            }
            return result;
        }

        private static Vector3[] FromFloatArray(float[] source)
        {
            var result = new Vector3[source.Length / 3];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Vector3(
                        source[i * 3],
                        source[i * 3 + 1],
                        source[i * 3 + 2]
                    );
            }
            return result;
        }

        public Mesh GetDeserialized()
        {
            var result = new Mesh();

            var vertexCount = vertexes.Length / 3;
            var vertexHydrated = FromFloatArray(vertexes);
            var normalsHydrated = FromFloatArray(normals);


            var subMeshHydrated = new SubMeshDescriptor[submeshDescriptors.Length];
            for (int i = 0; i < submeshDescriptors.Length; i++)
            {
                subMeshHydrated[i] = submeshDescriptors[i].ToDescriptor();
            }


            var hydratedUvs = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                hydratedUvs[i] = new Vector2(
                        uvs[i * 2],
                        uvs[i * 2 + 1]
                    );
            }

            result.SetVertices(vertexHydrated);
            result.SetNormals(normalsHydrated);
            result.uv = hydratedUvs;
            result.triangles = indexes;
            result.SetSubMeshes(subMeshHydrated);

            result.RecalculateBounds();

            return result;
        }
    }
}
