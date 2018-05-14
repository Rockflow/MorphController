using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MorphController
{
    [CustomEditor(typeof(MeshFilter))]
    public class MorphMeshFilterEditor : Editor
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private MeshCollider _meshCollider;
        private MorphAnimationData _morphAnimationData;

        private void OnEnable()
        {
            _meshFilter = target as MeshFilter;
            _meshRenderer = _meshFilter.transform.GetComponent<MeshRenderer>();
            _skinnedMeshRenderer = _meshFilter.transform.GetComponent<SkinnedMeshRenderer>();
            _meshCollider = _meshFilter.transform.GetComponent<MeshCollider>();
            _morphAnimationData = _meshFilter.transform.GetComponent<MorphAnimationData>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                return;
            }

            GenerateGUI();
        }
        private void GenerateGUI()
        {
            GUILayout.BeginHorizontal();
            MorphEditorTool.SetGUIBackgroundColor(Color.cyan);
            if (GUILayout.Button("Generate Morph Controller"))
            {
                GenerateMorphController();
            }
            MorphEditorTool.SetGUIBackgroundColor(Color.white);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (_meshFilter && _meshFilter.sharedMesh)
            {
                if (_meshFilter.sharedMesh.vertexCount > 1000)
                {
                    EditorGUILayout.HelpBox("This mesh's vertices number is too much! Generate will be very slow！", MessageType.Warning);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void GenerateMorphController()
        {
            if (!_meshFilter.sharedMesh)
            {
                MorphDebug.LogError("MeshFilter组件丢失了Mesh数据！", _meshFilter.gameObject);
                return;
            }
            if (!_meshRenderer)
            {
                MorphDebug.LogError("GameObject丢失了组件MeshRenderer！", _meshFilter.gameObject);
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Morph Mesh", Application.dataPath, _meshFilter.sharedMesh.name + "(Morph)", "asset");
            if (path.Length != 0)
            {
                Collider[] cols = _meshFilter.GetComponents<Collider>();
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i].enabled = false;
                }

                string subPath = path.Substring(0, path.IndexOf("Asset"));
                path = path.Replace(subPath, "");
                Mesh mesh = Instantiate(_meshFilter.sharedMesh);
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();

                Mesh meshAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) as Mesh;

                //生成蒙皮网格组件，并创建根骨骼
                if (!_skinnedMeshRenderer)
                {
                    _skinnedMeshRenderer = _meshFilter.transform.gameObject.AddComponent<SkinnedMeshRenderer>();
                }
                _skinnedMeshRenderer.sharedMesh = meshAsset;
                _skinnedMeshRenderer.rootBone = _meshFilter.transform;
                _skinnedMeshRenderer.sharedMaterials = _meshRenderer.sharedMaterials;
                _skinnedMeshRenderer.enabled = true;
                
                GameObject boneRoot = new GameObject("BoneRoot");
                boneRoot.hideFlags = HideFlags.HideInHierarchy;
                MorphBone mb = boneRoot.AddComponent<MorphBone>();
                mb.hideFlags = HideFlags.HideInInspector;
                Transform[] bones = new Transform[1];
                Matrix4x4[] bindposes = new Matrix4x4[1];
                bones[0] = boneRoot.transform;
                bones[0].SetParent(_skinnedMeshRenderer.rootBone);
                bones[0].localPosition = Vector3.zero;
                bones[0].localRotation = Quaternion.identity;
                bindposes[0] = bones[0].worldToLocalMatrix * _skinnedMeshRenderer.transform.localToWorldMatrix;
                _skinnedMeshRenderer.bones = bones;
                _skinnedMeshRenderer.sharedMesh.bindposes = bindposes;

                //生成网格碰撞器
                if (!_meshCollider)
                {
                    _meshCollider = _meshFilter.transform.gameObject.AddComponent<MeshCollider>();
                }
                _meshCollider.sharedMesh = meshAsset;
                _meshCollider.enabled = true;

                //生成变形动画数据组件
                if (!_morphAnimationData)
                {
                    _morphAnimationData = _meshFilter.transform.gameObject.AddComponent<MorphAnimationData>();
                }
                _morphAnimationData.Identity = true;
                _morphAnimationData.hideFlags = HideFlags.HideInInspector;
                _morphAnimationData.Vertexs.Clear();
                _morphAnimationData.Triangles.Clear();
                //处理顶点
                List<int> repetitionVertices = new List<int>();
                for (int i = 0; i < meshAsset.vertices.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose vertices（" + i + "/" + meshAsset.vertices.Length + "）......", 1.0f / meshAsset.vertices.Length * i);

                    if (repetitionVertices.Contains(i))
                        continue;

                    List<int> verticesGroup = new List<int>();
                    verticesGroup.Add(i);

                    for (int j = i + 1; j < meshAsset.vertices.Length; j++)
                    {
                        if (meshAsset.vertices[i] == meshAsset.vertices[j])
                        {
                            verticesGroup.Add(j);
                            repetitionVertices.Add(j);
                        }
                    }
                    _morphAnimationData.Vertexs.Add(new MorphVertex(_meshFilter.transform.localToWorldMatrix.MultiplyPoint3x4(meshAsset.vertices[i]), verticesGroup));
                }
                //处理三角面
                List<int> allTriangles = new List<int>(meshAsset.triangles);
                for (int i = 0; (i + 2) < allTriangles.Count; i += 3)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose triangles（" + i + "/" + allTriangles.Count + "）......", 1.0f / allTriangles.Count * i);

                    int mv1 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i]);
                    int mv2 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 1]);
                    int mv3 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 2]);
                    MorphTriangle mt = new MorphTriangle(mv1, mv2, mv3);
                    _morphAnimationData.Triangles.Add(mt);
                }
                _morphAnimationData.BindposesPosition.Add(boneRoot.transform.localPosition);
                _morphAnimationData.BindposesRotation.Add(boneRoot.transform.localRotation);
                _morphAnimationData.BindposesScale.Add(boneRoot.transform.localScale);

                EditorUtility.ClearProgressBar();

                DestroyImmediate(_meshFilter);
                DestroyImmediate(_meshRenderer);

                _skinnedMeshRenderer.transform.parent = null;
            }
        }
    }
}
