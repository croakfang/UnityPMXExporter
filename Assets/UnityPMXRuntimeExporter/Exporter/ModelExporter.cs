using LibMMD.Material;
using LibMMD.Model;
using LibMMD.Reader;
using LibMMD.Writer;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LibMMD.Model.Morph;
using static LibMMD.Model.SkinningOperator;

namespace UnityPMXExporter
{
    public class ModelExporter
    {

        public static void ExportModel(GameObject target, string path, PMXModelConfig exportConfig = new PMXModelConfig(), RenderTextureReadWrite colorSpace = RenderTextureReadWrite.Default)
        {
            
            if(string.IsNullOrEmpty(exportConfig.Name) && string.IsNullOrEmpty(exportConfig.NameEn))
            {
                exportConfig = new PMXModelConfig(target);
            }

            var textures = TextureExporter.ExportAllTexture(Path.GetDirectoryName(path), target.gameObject, colorSpace);
            var model = ReadPMXModelFromGameObject(target, textures, exportConfig);

            FileStream fileStream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            var writeConfig = new ModelConfig() { GlobalToonPath = "Toon" };
            PMXWriter.Write(writer, model, writeConfig);

            writer.Close();
            fileStream.Close();

            Debug.Log($"PMX Save at {path}");
        }


        public static RawMMDModel ReadPMXModelFromGameObject(GameObject target, string[] textures, PMXModelConfig config)
        {
            RawMMDModel model = new RawMMDModel();

            model.Name = config.Name;
            model.NameEn = config.NameEn;
            model.Description = config.Description;
            model.DescriptionEn = config.DescriptionEn;

            //Rean Bones
            var rootBone = target.transform;
            List<Transform> bones = new List<Transform>(rootBone.GetComponentsInChildren<Transform>());

            //Read vertices And triangles
            List<Renderer> renderers = new List<Renderer>(target.GetComponentsInChildren<Renderer>());
            List<int> triangles = new List<int>();
            model.Vertices = ReadVerticesAndTriangles(renderers, bones, ref triangles, target.transform);
            model.TriangleIndexes = triangles.ToArray();

            //Read Texture reference
            foreach (var texture in textures)
            {
                model.TextureList.Add(new MMDTexture(texture));
            }

            model.Parts = ReadPartMaterials(renderers, model);
            model.Bones = ReadBones(bones);
            model.Morphs = ReadMorph(renderers);
            model.Rigidbodies = new MMDRigidBody[0];
            model.Joints = new MMDJoint[0];

            return model;
        }

        public static Vector3[] Vec4ToVec3(Vector4[] vector4s)
        {
            Vector3[] tmp = new Vector3[vector4s.Length];
            for (int i = 0; i < vector4s.Length; i++)
            {
                tmp[i] = new Vector3(vector4s[i].x, vector4s[i].y, vector4s[i].z);
            }
            return tmp;
        }

        public static Vector3[] CalDelta(Vector3[] ori, Vector3[] end)
        {
            Vector3[] tmp = new Vector3[ori.Length];
            for (int i = 0; i < ori.Length; i++)
            {
                tmp[i] = end[i] - ori[i];
            }
            return tmp;
        }

        private static Morph[] ReadMorph(List<Renderer> renderers)
        {
            List<Morph> morphs = new List<Morph>();
            int vertexOffset = 0;
            foreach (Renderer renderer in renderers)
            {
                Mesh mesh;
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    mesh = meshfilter.mesh;
                }
                else
                {
                    mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                }
                var vertexCount = mesh.vertexCount;

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    var deltaVertices = new Vector3[vertexCount];
                    var deltaNormals = new Vector3[vertexCount];
                    var deltaTangents = new Vector3[vertexCount];
                    mesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);

                    Morph morph = new Morph();
                    morph.Name = morph.NameEn = mesh.GetBlendShapeName(i);
                    morph.Type = MorphType.MorphTypeVertex;
                    var datas = new VertexMorphData[vertexCount];
                    for (int j = 0; j < vertexCount; j++)
                    {
                        var data = new VertexMorphData();
                        data.VertexIndex = vertexOffset + j;
                        data.Offset = deltaVertices[j];
                        datas[j] = data;
                    }
                    morph.MorphDatas = datas;
                    morph.Category = MorphCategory.MorphCatOther;
                    morphs.Add(morph);
                }

                vertexOffset += mesh.vertexCount;
            }
            return morphs.ToArray();
        }

        private static Bone[] ReadBones(List<Transform> bonelist)
        {
            List<Bone> pmxbones = new List<Bone>();
            foreach (var bone in bonelist)
            {
                Bone pmxbone = new Bone();
                pmxbone.Name = pmxbone.NameEn = bone.name;
                pmxbone.Position = bone.position;
                pmxbone.ParentIndex = bonelist.IndexOf(bone.parent);
                pmxbone.TransformLevel = 0;
                pmxbone.Visible = true;
                pmxbone.Movable = true;
                pmxbone.Rotatable = true;
                pmxbone.Controllable = true;
                pmxbone.ChildBoneVal = new Bone.ChildBone()
                {
                    ChildUseId = true,
                    Index = (bone.childCount > 0 ? bonelist.IndexOf(bone.GetChild(0)) : -1)
                };
                pmxbones.Add(pmxbone);
            }
            return pmxbones.ToArray();
        }

        private static Part[] ReadPartMaterials(List<Renderer> renderers, RawMMDModel model)
        {
            List<Part> parts = new List<Part>();
            int baseShift = 0;

            foreach (Renderer renderer in renderers)
            {
                Mesh mesh;
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    mesh = meshfilter.mesh;
                }
                else if (((SkinnedMeshRenderer)renderer).sharedMesh.isReadable)
                {
                    mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                }
                else
                {
                    mesh = new Mesh();
                    ((SkinnedMeshRenderer)renderer).BakeMesh(mesh);
                }

                var materials = new List<Material>(renderer.sharedMaterials);
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    var material = (i < materials.Count ? materials[i] : materials[materials.Count - 1]);
                    var part = new Part();
                    var mat = new MMDMaterial();
                    part.Material = mat;
                    mat.Name = mat.NameEn = material.name.Replace(" (Instance)", "");
                    mat.DiffuseColor = Color.white;
                    mat.SpecularColor = Color.clear;
                    mat.AmbientColor = Color.white * 0.5f;
                    mat.Shiness = 5;
                    mat.CastSelfShadow = true;
                    mat.DrawGroundShadow = true;
                    mat.DrawSelfShadow = true;
                    mat.EdgeColor = Color.black;
                    mat.EdgeSize = 0.4f;
                    var propNames = material.GetTexturePropertyNames();
                    if (propNames.Length > 0)
                    {
                        var main_tex = material.GetTexture(propNames[0]);
                        var tex = model.TextureList.Find(t => t.TexturePath.Contains($"/{main_tex.name}.png"));
                        if (tex != null)
                        {
                            mat.Texture = tex;
                        }
                        else if (model.TextureList.Count > 0)
                        {
                            mat.Texture = model.TextureList[0];
                        }
                    }
                    mat.MetaInfo = "";
                    part.BaseShift = baseShift + mesh.GetSubMesh(i).indexStart;
                    part.TriangleIndexNum = mesh.GetSubMesh(i).indexCount;
                    parts.Add(part);
                }
                baseShift += mesh.triangles.Length;
            }
            return parts.ToArray();
        }

        private static Vertex[] ReadVerticesAndTriangles(List<Renderer> renderers, List<Transform> bones, ref List<int> triangleList, Transform root)
        {
            List<Vertex> verticesList = new List<Vertex>();
            int vertexOffset = 0;

            foreach (Renderer renderer in renderers)
            {
                if (renderer is MeshRenderer mr)
                {
                    var meshfilter = mr.GetComponent<MeshFilter>();
                    //Cache this data to avoid copying arrays.
                    var mesh = meshfilter.mesh;
                    var vertices = mesh.vertices;
                    var normals = mesh.normals;
                    var uv = mesh.uv;
                    var uv1 = mesh.uv2;
                    var uv2 = mesh.uv3;
                    var colors = mesh.colors;
                    var triangles = mesh.triangles;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vertex vertex = new Vertex();
                        vertex.Coordinate = renderer.transform.TransformPoint(vertices[i]);
                        vertex.Normal = normals[i];
                        vertex.UvCoordinate = uv[i];

                        vertex.ExtraUvCoordinate = new Vector4[3]
                        {
                        uv1.Length > 0 ? uv1[i] : Vector2.zero,
                        uv2.Length > 0 ? uv2[i] : Vector2.zero,
                        colors.Length > 0 ? colors[i] : Color.clear
                        };

                        vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef1 };
                        vertex.SkinningOperator.Param = new Bdef1() { BoneId = bones.IndexOf(renderer.transform) };
                        vertex.EdgeScale = 1;
                        verticesList.Add(vertex);
                    }


                    foreach (var triangle in mesh.triangles)
                    {
                        triangleList.Add(triangle + vertexOffset);
                    }
                    vertexOffset += vertices.Length;
                }
                else if (renderer is SkinnedMeshRenderer smr)
                {
                    Mesh mesh = new Mesh();
                    if (smr.sharedMesh.isReadable)
                    {
                        mesh = smr.sharedMesh;
                    }
                    else
                    {
                        smr.BakeMesh(mesh);
                    }

                    var vertices = mesh.vertices;
                    var normals = mesh.normals;
                    var uv = mesh.uv;
                    var uv1 = mesh.uv2;
                    var uv2 = mesh.uv3;
                    var colors = mesh.colors;
                    var skinbone = smr.bones;
                    var skinbones = smr.bones;
                    var boneCounts = smr.sharedMesh.GetBonesPerVertex();
                    var weights = smr.sharedMesh.boneWeights;
                    var bakemesh = new Mesh();
                    smr.BakeMesh(bakemesh, true);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vertex vertex = new Vertex();
                        vertex.Coordinate = root.InverseTransformPoint(smr.transform.TransformPoint(bakemesh.vertices[i]));
                        vertex.Normal = normals[i];
                        vertex.UvCoordinate = new Vector2(uv[i].x, 1 - uv[i].y);
                        vertex.ExtraUvCoordinate = new Vector4[3]
                        {
                        uv1.Length > 0 ? new Vector2(uv1[i].x, 1 - uv1[i].y) : Vector2.zero,
                        uv2.Length > 0 ? new Vector2(uv2[i].x, 1 - uv2[i].y) : Vector2.zero,
                        colors.Length > 0 ? colors[i] : Color.clear
                        };

                        var boneWeight = weights[i];
                        var boneCount = boneCounts[i];

                        switch (boneCount)
                        {
                            case 0:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef1 };
                                vertex.SkinningOperator.Param = new Bdef1() { BoneId = GetBoneIndex(bones, renderer.transform) };
                                break;

                            default:
                            case 1:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef1 };
                                vertex.SkinningOperator.Param = new Bdef1() { BoneId = GetBoneIndex(bones, skinbone[boneWeight.boneIndex0]) };
                                break;

                            case 2:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef2 };
                                vertex.SkinningOperator.Param = new Bdef2()
                                {
                                    BoneId = new int[]{
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex0]),
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex1]),
                                },
                                    BoneWeight = boneWeight.weight0
                                };
                                break;

                            case 3:
                            case 4:
                                vertex.SkinningOperator = new SkinningOperator() { Type = SkinningType.SkinningBdef4 };
                                vertex.SkinningOperator.Param = new Bdef4()
                                {
                                    BoneId = new int[]{
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex0]),
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex1]),
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex2]),
                                    GetBoneIndex(bones, skinbone[boneWeight.boneIndex3]),
                                },
                                    BoneWeight = new float[]
                                    {
                                    boneWeight.weight0,
                                    boneWeight.weight1,
                                    boneWeight.weight2,
                                    boneWeight.weight3
                                    }
                                };
                                break;
                        }
                        vertex.EdgeScale = 1;
                        verticesList.Add(vertex);
                    }

                    foreach (var triangle in mesh.triangles)
                    {
                        triangleList.Add(triangle + vertexOffset);
                    }
                    vertexOffset += vertices.Length;
                }
            }
            return verticesList.ToArray();
        }

        private static int GetBoneIndex(List<Transform> bones, Transform bone)
        {
            return bones.Contains(bone) ? bones.IndexOf(bone) : 0;
        }
    }

    public struct PMXModelConfig
    {
        public string Name;
        public string NameEn;
        public string Description;
        public string DescriptionEn;

        public PMXModelConfig(GameObject gameObject)
        {
            Name = NameEn = gameObject.name;
            Description = DescriptionEn = gameObject.name;
        }
    }
}
