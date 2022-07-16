using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializedMesh
    {
        [Serializable]
        private class SingleMeshDataSerialized
        {
            SerializedSubmesh[] subMeshes;

            SerializedVertexAttributeDescriptor[] vertexAttributes;
            byte[] vertexes;
            int vertexCount;

            IndexFormat indexFormat;
            UInt16[] indexes;


            public SingleMeshDataSerialized(MeshData source, VertexAttributeDescriptor[] vertexAttributes)
            {
                subMeshes = new SerializedSubmesh[source.subMeshCount];
                for (int i = 0; i < subMeshes.Length; i++)
                {
                    subMeshes[i] = new SerializedSubmesh(source.GetSubMesh(i));
                }

                if (source.vertexBufferCount != 1)
                {
                    Debug.LogWarning("warning: mesh data has more than one vertex buffer count. this is unsupported when serializing");
                }

                this.vertexCount = source.vertexCount;
                vertexes = source.GetVertexData<byte>(0).ToArray();
                this.vertexAttributes = new SerializedVertexAttributeDescriptor[vertexAttributes.Length];
                for (int i = 0; i < this.vertexAttributes.Length; i++)
                {
                    this.vertexAttributes[i] = new SerializedVertexAttributeDescriptor(vertexAttributes[i]);
                }

                this.indexFormat = source.indexFormat;
                if (this.indexFormat == IndexFormat.UInt16)
                {
                    var tmpData = source.GetIndexData<UInt16>();
                    indexes = tmpData.ToArray();
                }
                else if (this.indexFormat == IndexFormat.UInt32)
                {
                    var tmpData = source.GetIndexData<UInt32>().Reinterpret<UInt16>(sizeof(UInt32));
                    indexes = tmpData.ToArray();
                }
                else
                {
                    throw new Exception($"unrecognized index format: {this.indexFormat}");
                }

            }

            public void WriteMeshData(MeshData target)
            {
                {
                    var newVertexAttributes = new VertexAttributeDescriptor[this.vertexAttributes.Length];
                    for (int i = 0; i < newVertexAttributes.Length; i++)
                    {
                        newVertexAttributes[i] = vertexAttributes[i].ToDescriptor();
                    }
                    target.SetVertexBufferParams(this.vertexCount, newVertexAttributes);
                    using var tmpVertextes = new NativeArray<byte>(vertexes, Allocator.TempJob);
                    var targetVertexes = target.GetVertexData<byte>(0);
                    targetVertexes.CopyFrom(tmpVertextes);
                }

                {
                    var trueLength = indexFormat == IndexFormat.UInt16 ? indexes.Length : indexes.Length / 2;
                    target.SetIndexBufferParams(trueLength, indexFormat);
                    using var tmpIndexes = new NativeArray<UInt16>(indexes, Allocator.TempJob);
                    if (indexFormat == IndexFormat.UInt16)
                    {
                        var targetIndexes = target.GetIndexData<UInt16>();
                        targetIndexes.CopyFrom(tmpIndexes);
                    }
                    else
                    {
                        var targetIndexes = target.GetIndexData<UInt32>();
                        targetIndexes.CopyFrom(tmpIndexes.Reinterpret<UInt32>(sizeof(UInt16)));
                    }
                }

                {
                    target.subMeshCount = subMeshes.Length;
                    int i;
                    for (i = 0; i < subMeshes.Length - 1; i++)
                    {
                        target.SetSubMesh(i, subMeshes[i].ToDescriptor(), MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);
                    }
                    target.SetSubMesh(i, subMeshes[i].ToDescriptor(), MeshUpdateFlags.Default);
                }
            }
        }

        SingleMeshDataSerialized serializedMesh;

        [Serializable]
        struct SerializedSubmesh
        {
            public int indexStart;
            public int indexCount;
            public int baseVertex;
            public MeshTopology topology;

            public SerializedSubmesh(SubMeshDescriptor subMesh)
            {
                indexStart = subMesh.indexStart;
                indexCount = subMesh.indexCount;
                baseVertex = subMesh.baseVertex;
                topology = subMesh.topology;
            }

            public SubMeshDescriptor ToDescriptor()
            {
                var newSub = new SubMeshDescriptor(indexStart, indexCount, topology);
                newSub.baseVertex = baseVertex;
                return newSub;
            }
        }

        [Serializable]
        struct SerializedVertexAttributeDescriptor
        {
            public VertexAttribute attribute;
            public VertexAttributeFormat format;
            public int dimension;
            public int stream;

            public SerializedVertexAttributeDescriptor(VertexAttributeDescriptor descriptor)
            {
                attribute = descriptor.attribute;
                format = descriptor.format;
                dimension = descriptor.dimension;
                stream = descriptor.stream;
            }

            public VertexAttributeDescriptor ToDescriptor()
            {
                return new VertexAttributeDescriptor(attribute, format, dimension, stream);
            }
        }

        public SerializedMesh(Mesh mesh)
        {
            UnityEngine.Profiling.Profiler.BeginSample("serializing mesh");

            var meshData = Mesh.AcquireReadOnlyMeshData(mesh);
            if (meshData.Length != 1)
            {
                Debug.Log("serializing multiple meshes is not supported");
            }
            serializedMesh = new SingleMeshDataSerialized(meshData[0], mesh.GetVertexAttributes());
            meshData.Dispose();

            UnityEngine.Profiling.Profiler.EndSample();
        }

        public Mesh GetDeserialized()
        {
            var meshBuilder = Mesh.AllocateWritableMeshData(1);
            var meshTarget = meshBuilder[0];
            serializedMesh.WriteMeshData(meshTarget);


            var result = new Mesh();

            Mesh.ApplyAndDisposeWritableMeshData(meshBuilder, result, MeshUpdateFlags.Default);

            return result;
        }
    }
}
