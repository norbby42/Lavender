using Silk.NET.Assimp;
using System;
using System.Collections.Generic;

namespace Lavender.RuntimeImporter
{
    // Assimp allows to import OBJ, FBX, DAE and more!
    // But for now we stick to OBJ

    public static class AssimpAPI
    {
        public static Assimp? assimp;

        public static void Innit()
        {
            assimp = Assimp.GetApi();
        }

        public static UnityEngine.Mesh? LoadSingleMeshOBJ(string path)
        {
            UnityEngine.Mesh[]? meshes = ImportOBJ(path);

            if(meshes.Length > 0)
            {
                return meshes[0];
            }
            else
            {
                return null;
            }
        }

        public static UnityEngine.Mesh[]? ImportOBJ(string objPath)
        {
            if (assimp == null) return null;

            try
            {
                List<UnityEngine.Mesh> importedMeshes = new List<UnityEngine.Mesh>();

                unsafe
                {
                    Scene* scene = assimp.ImportFile(objPath, (uint)PostProcessSteps.Triangulate);

                    if(scene == null || scene->MRootNode == null)
                    {
                        LavenderLog.Error($"Failed to load OBJ with Assimp. Path: {objPath}");
                    }

                    for(uint i = 0; i < scene->MNumMeshes; i++)
                    {
                        Mesh* mesh = scene->MMeshes[i];

                        UnityEngine.Mesh? umesh = CreateUnityMesh(mesh);
                        if (umesh != null)
                        {
                            importedMeshes.Add(umesh);
                        }
                    }

                    // Cleanup
                    assimp.ReleaseImport(scene);
                }

                return importedMeshes.ToArray();
            }
            catch(Exception e)
            {
                LavenderLog.Error($"[Assimp] {e}");
                return null;
            }
        }

        public static unsafe UnityEngine.Mesh? CreateUnityMesh(Mesh* mesh)
        {
            int vertexCount = (int)mesh->MNumVertices;
            int faceCount = (int)mesh->MNumFaces;

            UnityEngine.Vector3[] vertices = new UnityEngine.Vector3[vertexCount];
            int[] triangles = new int[faceCount * 3];

            for (int i = 0; i < vertexCount; i++)
            {
                var v = mesh->MVertices[i];
                vertices[i] = new UnityEngine.Vector3(v.X, v.Y, v.Z);
            }

            for (int i = 0; i < faceCount; i++)
            {
                var face = mesh->MFaces[i];
                if (face.MNumIndices == 3)
                {
                    triangles[i * 3 + 0] = (int)face.MIndices[0];
                    triangles[i * 3 + 1] = (int)face.MIndices[1];
                    triangles[i * 3 + 2] = (int)face.MIndices[2];
                }
            }

            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            unityMesh.RecalculateNormals();

            return unityMesh;
        }
    }
}
