using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MorphController
{
    [CustomEditor(typeof(SkinnedMeshRenderer))]
    public class MorphSkinnedMeshEditor : Editor
    {
        public static bool IsOpeningAnimator = false;
        
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private MeshCollider _meshCollider;
        private MorphAnimationData _morphAnimationData;
        private MorphAnimator _morphAnimator;

        private Mesh _mesh;
        private MorphEditType _editType;
        private MorphEditTool _editTool;
        private MorphBone _currentMorphBone;
        private MorphVertex _currentMorphVertex;
        private bool _showBaseSetting;
        private bool _showMorphController;
        private bool _isReName;
        private string _newName;
        private bool _isEditBoneRange;
        private bool _showBoneInfo;

        private float _minBoneIconSize = 0.01f;
        private float _maxBoneIconSize = 1f;
        private float _minVertexIconSize = 0.001f;
        private float _maxVertexIconSize = 0.1f;

        private void OnEnable()
        {
            _skinnedMeshRenderer = target as SkinnedMeshRenderer;
            _meshCollider = _skinnedMeshRenderer.transform.GetComponent<MeshCollider>();
            _morphAnimationData = _skinnedMeshRenderer.transform.GetComponent<MorphAnimationData>();
            _morphAnimator = _skinnedMeshRenderer.transform.GetComponent<MorphAnimator>();

            _mesh = _skinnedMeshRenderer.sharedMesh;
            _editType = MorphEditType.Bone;
            _editTool = MorphEditTool.Move;
            _currentMorphBone = null;
            _currentMorphVertex = null;
            _showBaseSetting = false;
            _showMorphController = true;
            _isReName = false;
            _newName = "";
            _isEditBoneRange = true;
            _showBoneInfo = true;

            if (_morphAnimationData && _morphAnimationData.Identity)
            {
                Rebuild();
                ApplyWeights();
            }
        }

        public override void OnInspectorGUI()
        {
            BaseSettingGUI();

            if (Application.isPlaying)
            {
                return;
            }

            if (_morphAnimationData && _morphAnimationData.Identity)
            {
                MorphControllerGUI();

                SceneView.RepaintAll();
            }
            else
            {
                GenerateGUI();
            }
        }
        private void BaseSettingGUI()
        {
            GUILayout.BeginHorizontal("MeTransitionHead");
            GUILayout.Space(12);
            _showBaseSetting = EditorGUILayout.Foldout(_showBaseSetting, "Base Setting", true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_showBaseSetting)
            {
                base.OnInspectorGUI();
            }
        }
        private void MorphControllerGUI()
        {
            MorphEditorTool.SetGUIBackgroundColor(Color.white);
            MorphEditorTool.SetGUIColor(Color.white);
            MorphEditorTool.SetGUIContentColor(Color.white);

            GUILayout.BeginHorizontal("MeTransitionHead");
            GUILayout.Space(12);
            _showMorphController = EditorGUILayout.Foldout(_showMorphController, "Morph Controller", true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_showMorphController)
            {
                #region Opening Animator
                if (IsOpeningAnimator)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Close Animator Window"))
                    {
                        MorphAnimatorWindow window = EditorWindow.GetWindow<MorphAnimatorWindow>();
                        window.Close();
                        IsOpeningAnimator = false;
                    }
                    GUILayout.EndHorizontal();
                    return;
                }
                #endregion

                #region EditType
                GUILayout.BeginHorizontal();
                bool boneType = (_editType == MorphEditType.Bone);
                if (GUILayout.Toggle(boneType, "Bone", "LargeButtonLeft") != boneType)
                {
                    _editType = MorphEditType.Bone;
                }
                bool vertexType = (_editType == MorphEditType.Vertex);
                if (GUILayout.Toggle(vertexType, "Vertex", "LargeButtonRight") != vertexType)
                {
                    _editType = MorphEditType.Vertex;
                }
                GUILayout.EndHorizontal();
                #endregion

                #region Bone
                if (_editType == MorphEditType.Bone)
                {
                    #region Title
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Bone Number " + _skinnedMeshRenderer.bones.Length);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Bone Icon Size", GUILayout.Width(100));
                    _morphAnimationData.BoneIconSize = EditorGUILayout.Slider(_morphAnimationData.BoneIconSize, _minBoneIconSize, _maxBoneIconSize);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Tool
                    GUILayout.BeginHorizontal();
                    bool none = (_editTool == MorphEditTool.None);
                    if (GUILayout.Toggle(none, MorphStyle.IconGUIContent("ViewToolMove"), "ButtonLeft") != none)
                    {
                        _editTool = MorphEditTool.None;
                    }
                    bool move = (_editTool == MorphEditTool.Move);
                    if (GUILayout.Toggle(move, MorphStyle.IconGUIContent("MoveTool"), "ButtonMid") != move)
                    {
                        _editTool = MorphEditTool.Move;
                    }
                    bool rotate = (_editTool == MorphEditTool.Rotate);
                    if (GUILayout.Toggle(rotate, MorphStyle.IconGUIContent("RotateTool"), "ButtonMid") != rotate)
                    {
                        _editTool = MorphEditTool.Rotate;
                    }
                    bool scale = (_editTool == MorphEditTool.Scale);
                    if (GUILayout.Toggle(scale, MorphStyle.IconGUIContent("ScaleTool"), "ButtonMid") != scale)
                    {
                        _editTool = MorphEditTool.Scale;
                    }
                    bool transform = (_editTool == MorphEditTool.Transform);
                    if (GUILayout.Toggle(transform, MorphStyle.IconGUIContent("TransformTool"), "ButtonRight") != transform)
                    {
                        _editTool = MorphEditTool.Transform;
                    }
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Operation
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Create", "ButtonLeft"))
                    {
                        CreateBone(null);
                        ResetBindposes();
                    }
                    MorphEditorTool.SetGUIEnabled(_currentMorphBone);
                    if (GUILayout.Button("CreateSub", "ButtonMid"))
                    {
                        CreateBone(_currentMorphBone);
                        ResetBindposes();
                    }
                    if (GUILayout.Button("ReName", "ButtonMid"))
                    {
                        _isReName = !_isReName;
                        if (_isReName)
                        {
                            _newName = _currentMorphBone.name;
                        }
                    }
                    if (GUILayout.Button("Delete", "ButtonRight"))
                    {
                        if (_currentMorphBone.transform.childCount > 0)
                        {
                            MorphDebug.LogError("请先删除该骨骼的子骨骼！", _skinnedMeshRenderer.gameObject);
                            return;
                        }
                        if (_skinnedMeshRenderer.bones.Length == 1)
                        {
                            MorphDebug.LogError("必须拥有一条以上的骨骼！", _skinnedMeshRenderer.gameObject);
                            return;
                        }
                        if (EditorUtility.DisplayDialog("Prompt", "Whether to remove bone '" + _currentMorphBone.name + "'?This is unrecoverable.", "Sure", "Cancel"))
                        {
                            DeleteBone(_currentMorphBone);
                            _currentMorphBone = null;
                            ResetBindposes();
                            ApplyWeights();
                        }
                    }
                    MorphEditorTool.SetGUIEnabled(true);
                    GUILayout.EndHorizontal();
                    #endregion

                    #region ReName
                    if (_isReName && _currentMorphBone)
                    {
                        GUILayout.BeginHorizontal();
                        _newName = GUILayout.TextField(_newName);
                        if (GUILayout.Button("Sure", "MiniButtonLeft", GUILayout.Width(50)))
                        {
                            if (MorphEditorTool.BoneNameIsAllow(_skinnedMeshRenderer.bones, _newName))
                            {
                                _currentMorphBone.name = _newName;
                                _isReName = false;
                            }
                            else
                            {
                                MorphDebug.LogError("输入的骨骼名字不符合规定或者存在重名！", _skinnedMeshRenderer.gameObject);
                            }
                        }
                        if (GUILayout.Button("Cancel", "MiniButtonRight", GUILayout.Width(50)))
                        {
                            _isReName = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region Bone
                    GUILayout.BeginVertical("Box");
                    if (_skinnedMeshRenderer.bones != null)
                    {
                        PreviewBoneChildGUI(_skinnedMeshRenderer.rootBone, 0);
                        MorphEditorTool.SetGUIBackgroundColor(Color.white);
                    }
                    GUILayout.EndVertical();
                    #endregion

                    #region Restore To Bindposes & Reset Bindposes
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Restore To Bindposes", "ButtonLeft"))
                    {
                        RestoreToBindposes();
                    }
                    if (GUILayout.Button("Reset Bindposes", "ButtonRight"))
                    {
                        if (EditorUtility.DisplayDialog("Prompt", "Whether to reset bindposes?", "Sure", "Cancel"))
                        {
                            ResetBindposes();
                        }
                    }
                    GUILayout.EndHorizontal();
                    #endregion
                }
                #endregion

                #region Vertex
                else if (_editType == MorphEditType.Vertex)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Vertex Number " + _mesh.vertexCount);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Vertex Icon Size", GUILayout.Width(100));
                    _morphAnimationData.VertexIconSize = EditorGUILayout.Slider(_morphAnimationData.VertexIconSize, _minVertexIconSize, _maxVertexIconSize);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Click the left mouse to select vertex in this model!", MessageType.Info);
                    GUILayout.EndHorizontal();
                }
                #endregion

                #region Open Animator
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Animator Window"))
                {
                    if (!_morphAnimator)
                    {
                        _morphAnimator = _skinnedMeshRenderer.transform.GetComponent<MorphAnimator>();
                        if (!_morphAnimator)
                        {
                            _morphAnimator = _skinnedMeshRenderer.gameObject.AddComponent<MorphAnimator>();
                        }
                    }

                    MorphAnimatorWindow window = EditorWindow.GetWindow<MorphAnimatorWindow>("MorphAnimator");
                    window.Init(_skinnedMeshRenderer, _morphAnimationData, _morphAnimator);
                    window.Show();
                    IsOpeningAnimator = true;

                    _currentMorphBone = null;
                    _currentMorphVertex = null;
                }
                GUILayout.EndHorizontal();
                #endregion
            }
        }
        private void GenerateGUI()
        {
            GUILayout.BeginHorizontal();
            MorphEditorTool.SetGUIBackgroundColor(Color.cyan);
            if (GUILayout.Button("Generate Morph Controller"))
            {
                if (_skinnedMeshRenderer.bones == null || _skinnedMeshRenderer.bones.Length <= 0)
                {
                    GenerateMorphController();
                }
                else
                {
                    OverrideMorphController();
                }
            }
            MorphEditorTool.SetGUIBackgroundColor(Color.white);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (_mesh)
            {
                if (_mesh.vertexCount > 1000)
                {
                    EditorGUILayout.HelpBox("This mesh's vertices number is too much! Generate will be very slow！", MessageType.Warning);
                }
            }
            GUILayout.EndHorizontal();
        }
        private void PreviewBoneChildGUI(Transform boneTransform, int indentLevel)
        {
            GUILayout.Space(2);

            MorphBone bone = boneTransform.GetComponent<MorphBone>();
            MorphEditorTool.SetGUIBackgroundColor(bone ? Color.white : Color.red);

            if (bone)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20 * indentLevel);
                if (boneTransform.childCount > 0)
                {
                    if (GUILayout.Button("", bone.IsShowInInspector ? "OL Minus" : "OL Plus", GUILayout.Width(20)))
                    {
                        bone.IsShowInInspector = !bone.IsShowInInspector;
                    }
                }
                else
                {
                    GUILayout.Space(20);
                }

                bool currentBone = (_currentMorphBone == bone);
                if (GUILayout.Toggle(currentBone, boneTransform.name, "PreButton") != currentBone)
                {
                    _currentMorphBone = bone;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (bone.IsShowInInspector)
                {
                    for (int i = 0; i < boneTransform.childCount; i++)
                    {
                        PreviewBoneChildGUI(boneTransform.GetChild(i), indentLevel + 1);
                    }
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20 * indentLevel);
                GUILayout.Space(20);
                if (GUILayout.Button(boneTransform.name, "PreButton"))
                {
                    _currentMorphBone = null;
                }
                if (boneTransform.childCount <= 0)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("X", "PreButton", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        DestroyImmediate(boneTransform.gameObject);
                        return;
                    }
                    GUILayout.Label("Remove unused bone");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                for (int i = 0; i < boneTransform.childCount; i++)
                {
                    PreviewBoneChildGUI(boneTransform.GetChild(i), indentLevel + 1);
                }
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_morphAnimationData && _morphAnimationData.Identity)
            {
                MorphHandles.DrawMorphBone(_skinnedMeshRenderer.rootBone, _currentMorphBone ? _currentMorphBone.transform : null, _morphAnimationData.BoneIconSize);

                #region Opening Animator
                if (IsOpeningAnimator)
                {
                    return;
                }
                #endregion

                #region Bone
                if (_editType == MorphEditType.Bone)
                {
                    if (_currentMorphBone != null)
                    {
                        #region Tool
                        MorphHandles.SetHandlesColor(Color.white);
                        switch (_editTool)
                        {
                            case MorphEditTool.Move:
                                _currentMorphBone.transform.position = MorphHandles.PositionHandle(_currentMorphBone.transform.position);
                                break;
                            case MorphEditTool.Rotate:
                                _currentMorphBone.transform.rotation = MorphHandles.RotationHandle(_currentMorphBone.transform.rotation, _currentMorphBone.transform.position);
                                break;
                            case MorphEditTool.Scale:
                                _currentMorphBone.transform.localScale = MorphHandles.ScaleHandle(_currentMorphBone.transform.localScale, _currentMorphBone.transform.position);
                                break;
                            case MorphEditTool.Transform:
                                _currentMorphBone.transform.position = MorphHandles.PositionHandle(_currentMorphBone.transform.position);
                                _currentMorphBone.transform.rotation = MorphHandles.RotationHandle(_currentMorphBone.transform.rotation, _currentMorphBone.transform.position);
                                _currentMorphBone.transform.localScale = MorphHandles.ScaleHandle(_currentMorphBone.transform.localScale, _currentMorphBone.transform.position);
                                break;
                        }
                        #endregion

                        #region GUI
                        try
                        {
                            Handles.BeginGUI();
                            GUILayout.BeginArea(new Rect(10, 10, 200, 95), new GUIStyle("HelpBox"));

                            GUILayout.Label("Bone:" + _currentMorphBone.name, "PreLabel");

                            GUILayout.BeginHorizontal();
                            if (GUILayout.Toggle(_isEditBoneRange, "EditRange", "PreButton") != _isEditBoneRange)
                            {
                                Event.current.type = EventType.Used;
                                _isEditBoneRange = !_isEditBoneRange;
                                SceneView.RepaintAll();
                            }
                            if (GUILayout.Toggle(_showBoneInfo, "WeightInfo", "PreButton") != _showBoneInfo)
                            {
                                Event.current.type = EventType.Used;
                                _showBoneInfo = !_showBoneInfo;
                                SceneView.RepaintAll();
                            }
                            GUILayout.EndHorizontal();

                            _currentMorphBone.InternalRange = EditorGUILayout.FloatField("Internal Range：", _currentMorphBone.InternalRange);
                            if (_currentMorphBone.InternalRange < 0)
                                _currentMorphBone.InternalRange = 0;

                            _currentMorphBone.ExternalRange = EditorGUILayout.FloatField("External Range：", _currentMorphBone.ExternalRange);
                            if (_currentMorphBone.ExternalRange < 0)
                                _currentMorphBone.ExternalRange = 0;
                            if (_currentMorphBone.ExternalRange < _currentMorphBone.InternalRange)
                                _currentMorphBone.ExternalRange = _currentMorphBone.InternalRange;

                            if (GUILayout.Button("Apply Range", "PreButton"))
                            {
                                if (EditorUtility.DisplayDialog("Prompt", "Whether to apply this bone's range setting?", "Sure", "Cancel"))
                                {
                                    Event.current.type = EventType.Used;
                                    ApplyRange();
                                    ApplyWeights();
                                    SceneView.RepaintAll();
                                }
                            }

                            GUILayout.EndArea();
                            Handles.EndGUI();
                        }
                        catch
                        { }
                        #endregion

                        #region Range
                        MorphHandles.SetHandlesColor(Color.cyan);
                        if (_isEditBoneRange)
                        {
                            _currentMorphBone.InternalRange = Handles.RadiusHandle(Quaternion.identity, _currentMorphBone.transform.position, _currentMorphBone.InternalRange);
                        }
                        for (int i = 0; i < _currentMorphBone.InternalRangeVertexs.Count; i++)
                        {
                            MorphVertex inVertex = _morphAnimationData.Vertexs[_currentMorphBone.InternalRangeVertexs[i]];
                            MorphHandles.DrawMorphVertex(inVertex.Vertex, _morphAnimationData.VertexIconSize);

                            if (_showBoneInfo)
                            {
                                float weight = inVertex.VertexWeight.GetWeight(_currentMorphBone.transform);
                                Handles.Label(inVertex.Vertex, weight.ToString());
                            }
                        }

                        MorphHandles.SetHandlesColor(Color.yellow);
                        if (_isEditBoneRange)
                        {
                            _currentMorphBone.ExternalRange = Handles.RadiusHandle(Quaternion.identity, _currentMorphBone.transform.position, _currentMorphBone.ExternalRange);
                        }
                        for (int i = 0; i < _currentMorphBone.ExternalRangeVertexs.Count; i++)
                        {
                            MorphVertex exVertex = _morphAnimationData.Vertexs[_currentMorphBone.ExternalRangeVertexs[i]];
                            MorphHandles.DrawMorphVertex(exVertex.Vertex, _morphAnimationData.VertexIconSize);

                            if (_showBoneInfo)
                            {
                                float weight = exVertex.VertexWeight.GetWeight(_currentMorphBone.transform);
                                Handles.Label(exVertex.Vertex, weight.ToString());
                            }
                        }

                        MorphHandles.SetHandlesColor(Color.red);
                        for (int i = 0; i < _currentMorphBone.ErrorVertexs.Count; i++)
                        {
                            MorphVertex errVertex = _morphAnimationData.Vertexs[_currentMorphBone.ErrorVertexs[i]];
                            MorphHandles.DrawMorphVertex(errVertex.Vertex, _morphAnimationData.VertexIconSize);

                            if (_showBoneInfo)
                            {
                                float weight = errVertex.VertexWeight.GetWeight(_currentMorphBone.transform);
                                Handles.Label(errVertex.Vertex, weight.ToString());
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region Vertex
                else if (_editType == MorphEditType.Vertex)
                {
                    if (_currentMorphVertex != null)
                    {
                        MorphHandles.SetHandlesColor(Color.cyan);
                        MorphHandles.DrawMorphVertex(_currentMorphVertex.Vertex, _morphAnimationData.VertexIconSize);

                        #region GUI
                        try
                        {
                            Handles.BeginGUI();
                            GUILayout.BeginArea(new Rect(10, 10, 200, 170), new GUIStyle("HelpBox"));

                            MorphWeight weight = _currentMorphVertex.VertexWeight;
                            Vector3 vertex = _skinnedMeshRenderer.transform.worldToLocalMatrix.MultiplyPoint3x4(_currentMorphVertex.Vertex);
                            GUILayout.Label("Vertex:" + vertex, "PreLabel");
                            
                            if (_skinnedMeshRenderer.bones.Length > 0)
                            {
                                string boneName = (weight.bone0 ? weight.bone0.name : "<None>");
                                if (GUILayout.Button("bone1：" + boneName, "PreDropDown", GUILayout.Width(150)))
                                {
                                    Event.current.type = EventType.Used;
                                    GenericMenu gm = new GenericMenu();
                                    for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                                    {
                                        int s = i;
                                        gm.AddItem(new GUIContent(_skinnedMeshRenderer.bones[s].name), _skinnedMeshRenderer.bones[s].name == boneName, delegate ()
                                        {
                                            weight.bone0 = _skinnedMeshRenderer.bones[s];
                                            ApplyWeights();
                                        });
                                    }
                                    gm.ShowAsContext();
                                }
                                float w0 = weight.weight0;
                                weight.weight0 = EditorGUILayout.FloatField("weight：", weight.weight0);
                                if (weight.weight0 < 0)
                                    weight.weight0 = 0;
                                if (weight.weight0 > 1)
                                    weight.weight0 = 1;

                                boneName = (weight.bone1 ? weight.bone1.name : "<None>");
                                if (GUILayout.Button("bone2：" + boneName, "PreDropDown", GUILayout.Width(150)))
                                {
                                    Event.current.type = EventType.Used;
                                    GenericMenu gm = new GenericMenu();
                                    for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                                    {
                                        int s = i;
                                        gm.AddItem(new GUIContent(_skinnedMeshRenderer.bones[s].name), _skinnedMeshRenderer.bones[s].name == boneName, delegate ()
                                        {
                                            weight.bone1 = _skinnedMeshRenderer.bones[s];
                                            ApplyWeights();
                                        });
                                    }
                                    gm.ShowAsContext();
                                }
                                float w1 = weight.weight1;
                                weight.weight1 = EditorGUILayout.FloatField("weight：", weight.weight1);
                                if (weight.weight1 < 0)
                                    weight.weight1 = 0;
                                if (weight.weight1 > 1)
                                    weight.weight1 = 1;

                                boneName = (weight.bone2 ? weight.bone2.name : "<None>");
                                if (GUILayout.Button("bone3：" + boneName, "PreDropDown", GUILayout.Width(150)))
                                {
                                    Event.current.type = EventType.Used;
                                    GenericMenu gm = new GenericMenu();
                                    for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                                    {
                                        int s = i;
                                        gm.AddItem(new GUIContent(_skinnedMeshRenderer.bones[s].name), _skinnedMeshRenderer.bones[s].name == boneName, delegate ()
                                        {
                                            weight.bone2 = _skinnedMeshRenderer.bones[s];
                                            ApplyWeights();
                                        });
                                    }
                                    gm.ShowAsContext();
                                }
                                float w2 = weight.weight2;
                                weight.weight2 = EditorGUILayout.FloatField("weight：", weight.weight2);
                                if (weight.weight2 < 0)
                                    weight.weight2 = 0;
                                if (weight.weight2 > 1)
                                    weight.weight2 = 1;

                                boneName = (weight.bone3 ? weight.bone3.name : "<None>");
                                if (GUILayout.Button("bone4：" + boneName, "PreDropDown", GUILayout.Width(150)))
                                {
                                    Event.current.type = EventType.Used;
                                    GenericMenu gm = new GenericMenu();
                                    for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                                    {
                                        int s = i;
                                        gm.AddItem(new GUIContent(_skinnedMeshRenderer.bones[s].name), _skinnedMeshRenderer.bones[s].name == boneName, delegate ()
                                        {
                                            weight.bone3 = _skinnedMeshRenderer.bones[s];
                                            ApplyWeights();
                                        });
                                    }
                                    gm.ShowAsContext();
                                }
                                float w3 = weight.weight3;
                                weight.weight3 = EditorGUILayout.FloatField("weight：", weight.weight3);
                                if (weight.weight3 < 0)
                                    weight.weight3 = 0;
                                if (weight.weight3 > 1)
                                    weight.weight3 = 1;


                                if (!Mathf.Approximately(w0, weight.weight0))
                                {
                                    Event.current.type = EventType.Used;
                                    weight.AstrictWeightsToOneExclude0();
                                    ApplyWeights();
                                }
                                else if (!Mathf.Approximately(w1, weight.weight1))
                                {
                                    Event.current.type = EventType.Used;
                                    weight.AstrictWeightsToOneExclude1();
                                    ApplyWeights();
                                }
                                else if (!Mathf.Approximately(w2, weight.weight2))
                                {
                                    Event.current.type = EventType.Used;
                                    weight.AstrictWeightsToOneExclude2();
                                    ApplyWeights();
                                }
                                else if (!Mathf.Approximately(w3, weight.weight3))
                                {
                                    Event.current.type = EventType.Used;
                                    weight.AstrictWeightsToOneExclude3();
                                    ApplyWeights();
                                }
                            }
                            else
                            {
                                GUILayout.Label("The number of bones is 0！", "PreLabel");
                            }

                            GUILayout.EndArea();
                            Handles.EndGUI();
                        }
                        catch
                        { }
                        #endregion
                    }
                    if (Event.current.button == 0 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
                        {
                            if (hit.triangleIndex >= 0 && hit.triangleIndex < _morphAnimationData.Triangles.Count)
                            {
                                _currentMorphVertex = _morphAnimationData.GetVertexByClick(_morphAnimationData.Triangles[hit.triangleIndex], hit.point);
                                Event.current.type = EventType.Used;
                            }
                        }
                    }
                }
                #endregion

                ChangeHandleTool();
            }
        }

        private void OnDestroy()
        {
            if (IsOpeningAnimator)
            {
                return;
            }

            if (_morphAnimationData && _morphAnimationData.Identity)
            {
                RestoreToBindposes();
            }
        }

        #region Auxiliary
        private void ChangeHandleTool()
        {
            if (Tools.current == Tool.View)
            {
                _editTool = MorphEditTool.None;
            }
            else if (Tools.current == Tool.Move)
            {
                _editTool = MorphEditTool.Move;
            }
            else if (Tools.current == Tool.Rotate)
            {
                _editTool = MorphEditTool.Rotate;
            }
            else if (Tools.current == Tool.Scale)
            {
                _editTool = MorphEditTool.Scale;
            }
            else if (Tools.current == Tool.Transform)
            {
                _editTool = MorphEditTool.Transform;
            }

            if (Tools.current != Tool.None)
            {
                Tools.current = Tool.None;
            }
        }
        private void GenerateMorphController()
        {
            if (!_mesh)
            {
                MorphDebug.LogError("SkinnedMeshRenderer组件丢失了Mesh数据！", _skinnedMeshRenderer.gameObject);
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Save Morph Mesh", Application.dataPath, _mesh.name + "(Morph)", "asset");
            if (path.Length != 0)
            {
                Collider[] cols = _skinnedMeshRenderer.GetComponents<Collider>();
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i].enabled = false;
                }

                string subPath = path.Substring(0, path.IndexOf("Asset"));
                path = path.Replace(subPath, "");
                Mesh mesh = Instantiate(_mesh);
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();

                _mesh = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) as Mesh;

                //生成蒙皮网格组件
                _skinnedMeshRenderer.sharedMesh = _mesh;
                _skinnedMeshRenderer.rootBone = _skinnedMeshRenderer.transform;
                _skinnedMeshRenderer.enabled = true;
                GameObject boneRoot = new GameObject("BoneRoot");
                boneRoot.AddComponent<MorphBone>();
                Transform[] bones = new Transform[1];
                bones[0] = boneRoot.transform;
                bones[0].SetParent(_skinnedMeshRenderer.rootBone);
                bones[0].localPosition = Vector3.zero;
                bones[0].localRotation = Quaternion.identity;
                _skinnedMeshRenderer.bones = bones;

                //生成网格碰撞器
                if (!_meshCollider)
                {
                    _meshCollider = _skinnedMeshRenderer.transform.gameObject.AddComponent<MeshCollider>();
                }
                _meshCollider.sharedMesh = _mesh;
                _meshCollider.enabled = true;

                //生成变形动画数据组件
                if (!_morphAnimationData)
                {
                    _morphAnimationData = _skinnedMeshRenderer.transform.gameObject.AddComponent<MorphAnimationData>();
                }
                _morphAnimationData.Identity = true;
                _morphAnimationData.Vertexs.Clear();
                _morphAnimationData.Triangles.Clear();
                //处理顶点
                List<int> repetitionVertices = new List<int>();
                for (int i = 0; i < _mesh.vertices.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose vertices（" + i + "/" + _mesh.vertices.Length + "）......", 1.0f / _mesh.vertices.Length * i);

                    if (repetitionVertices.Contains(i))
                        continue;

                    List<int> verticesGroup = new List<int>();
                    verticesGroup.Add(i);

                    for (int j = i + 1; j < _mesh.vertices.Length; j++)
                    {
                        if (_mesh.vertices[i] == _mesh.vertices[j])
                        {
                            verticesGroup.Add(j);
                            repetitionVertices.Add(j);
                        }
                    }

                    _morphAnimationData.Vertexs.Add(new MorphVertex(_skinnedMeshRenderer.transform.localToWorldMatrix.MultiplyPoint3x4(_mesh.vertices[i]), verticesGroup));
                }
                //处理三角面
                List<int> allTriangles = new List<int>(_mesh.triangles);
                for (int i = 0; (i + 2) < allTriangles.Count; i += 3)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose triangles（" + i + "/" + allTriangles.Count + "）......", 1.0f / allTriangles.Count * i);

                    int mv1 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i]);
                    int mv2 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 1]);
                    int mv3 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 2]);
                    MorphTriangle mt = new MorphTriangle(mv1, mv2, mv3);
                    _morphAnimationData.Triangles.Add(mt);
                }

                EditorUtility.ClearProgressBar();

                if (_skinnedMeshRenderer.GetComponent<MeshFilter>())
                {
                    DestroyImmediate(_skinnedMeshRenderer.GetComponent<MeshFilter>());
                }
                if (_skinnedMeshRenderer.GetComponent<MeshRenderer>())
                {
                    DestroyImmediate(_skinnedMeshRenderer.GetComponent<MeshRenderer>());
                }

                _skinnedMeshRenderer.transform.parent = null;

                Rebuild();
                ResetBindposes();
            }
        }
        private void OverrideMorphController()
        {
            if (!_mesh)
            {
                MorphDebug.LogError("SkinnedMeshRenderer组件丢失了Mesh数据！", _skinnedMeshRenderer.gameObject);
                return;
            }
            if (!_skinnedMeshRenderer.rootBone)
            {
                MorphDebug.LogError("SkinnedMeshRenderer组件rootBone属性不能为空！", _skinnedMeshRenderer.gameObject);
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Morph Mesh", Application.dataPath, _mesh.name + "(Morph)", "asset");
            if (path.Length != 0)
            {
                Collider[] cols = _skinnedMeshRenderer.GetComponents<Collider>();
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i].enabled = false;
                }

                string subPath = path.Substring(0, path.IndexOf("Asset"));
                path = path.Replace(subPath, "");
                Mesh mesh = Instantiate(_mesh);
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();

                _mesh = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) as Mesh;

                //生成蒙皮网格组件
                _skinnedMeshRenderer.rootBone.SetParent(_skinnedMeshRenderer.transform);
                _skinnedMeshRenderer.sharedMesh = _mesh;
                for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                {
                    MorphBone mb = _skinnedMeshRenderer.bones[i].GetComponent<MorphBone>();
                    if (!mb)
                    {
                        _skinnedMeshRenderer.bones[i].gameObject.AddComponent<MorphBone>();
                    }
                }
                _skinnedMeshRenderer.enabled = true;                

                //生成网格碰撞器
                if (!_meshCollider)
                {
                    _meshCollider = _skinnedMeshRenderer.transform.gameObject.AddComponent<MeshCollider>();
                }
                _meshCollider.sharedMesh = _mesh;
                _meshCollider.enabled = true;

                //生成变形动画数据组件
                if (!_morphAnimationData)
                {
                    _morphAnimationData = _skinnedMeshRenderer.transform.gameObject.AddComponent<MorphAnimationData>();
                }
                _morphAnimationData.Identity = true;
                _morphAnimationData.Vertexs.Clear();
                _morphAnimationData.Triangles.Clear();
                //处理顶点
                List<int> repetitionVertices = new List<int>();
                for (int i = 0; i < _mesh.vertices.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose vertices（" + i + "/" + _mesh.vertices.Length + "）......", 1.0f / _mesh.vertices.Length * i);

                    if (repetitionVertices.Contains(i))
                        continue;

                    List<int> verticesGroup = new List<int>();
                    verticesGroup.Add(i);

                    for (int j = i + 1; j < _mesh.vertices.Length; j++)
                    {
                        if (_mesh.vertices[i] == _mesh.vertices[j])
                        {
                            verticesGroup.Add(j);
                            repetitionVertices.Add(j);
                        }
                    }

                    MorphVertex vertex = new MorphVertex(_skinnedMeshRenderer.transform.localToWorldMatrix.MultiplyPoint3x4(_mesh.vertices[i]), verticesGroup);
                    vertex.VertexWeight = new MorphWeight(_skinnedMeshRenderer.bones, _mesh.boneWeights[i]);
                    _morphAnimationData.Vertexs.Add(vertex);
                }
                //处理三角面
                List<int> allTriangles = new List<int>(_mesh.triangles);
                for (int i = 0; (i + 2) < allTriangles.Count; i += 3)
                {
                    EditorUtility.DisplayProgressBar("Please wait", "Dispose triangles（" + i + "/" + allTriangles.Count + "）......", 1.0f / allTriangles.Count * i);

                    int mv1 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i]);
                    int mv2 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 1]);
                    int mv3 = _morphAnimationData.GetVertexIndexByIndex(allTriangles[i + 2]);
                    MorphTriangle mt = new MorphTriangle(mv1, mv2, mv3);
                    _morphAnimationData.Triangles.Add(mt);
                }

                EditorUtility.ClearProgressBar();

                if (_skinnedMeshRenderer.GetComponent<MeshFilter>())
                {
                    DestroyImmediate(_skinnedMeshRenderer.GetComponent<MeshFilter>());
                }
                if (_skinnedMeshRenderer.GetComponent<MeshRenderer>())
                {
                    DestroyImmediate(_skinnedMeshRenderer.GetComponent<MeshRenderer>());
                }

                _skinnedMeshRenderer.transform.parent = null;

                Rebuild();
                ResetBindposes();
            }
        }
        private void CreateBone(MorphBone parentBone)
        {
            GameObject bone = new GameObject("NewBone" + _skinnedMeshRenderer.bones.Length);
            MorphBone mb = bone.AddComponent<MorphBone>();
            mb.hideFlags = HideFlags.HideInInspector;
            bone.hideFlags = HideFlags.HideInHierarchy;
            bone.transform.SetParent(parentBone ? parentBone.transform : _skinnedMeshRenderer.rootBone);
            bone.transform.localPosition = Vector3.zero;
            bone.transform.localRotation = Quaternion.identity;
            List<Transform> bonesList = _skinnedMeshRenderer.bones.ToList();
            bonesList.Add(bone.transform);
            _skinnedMeshRenderer.bones = bonesList.ToArray();
        }
        private void DeleteBone(MorphBone bone)
        {
            List<Transform> bonesList = _skinnedMeshRenderer.bones.ToList();
            bonesList.Remove(bone.transform);
            DestroyImmediate(bone.gameObject);
            _skinnedMeshRenderer.bones = bonesList.ToArray();
        }
        private void Rebuild()
        {
            if (_morphAnimationData)
            {
                _morphAnimationData.hideFlags = HideFlags.HideInInspector;
            }

            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                _skinnedMeshRenderer.bones[i].hideFlags = HideFlags.HideInHierarchy;
                MorphBone bone = _skinnedMeshRenderer.bones[i].GetComponent<MorphBone>();
                if (bone)
                {
                    bone.hideFlags = HideFlags.HideInInspector;
                }
            }
        }
        private void ApplyRange()
        {
            _currentMorphBone.InternalRangeVertexs.Clear();
            _currentMorphBone.ExternalRangeVertexs.Clear();
            _currentMorphBone.ErrorVertexs.Clear();

            for (int i = 0; i < _morphAnimationData.Vertexs.Count; i++)
            {
                MorphVertex vertex = _morphAnimationData.Vertexs[i];
                MorphWeight weight = vertex.VertexWeight;

                float distance = Vector3.Distance(vertex.Vertex, _currentMorphBone.transform.position);
                if (distance <= _currentMorphBone.InternalRange)
                {
                    weight.bone0 = _currentMorphBone.transform;
                    weight.bone1 = _currentMorphBone.transform;
                    weight.bone2 = _currentMorphBone.transform;
                    weight.bone3 = _currentMorphBone.transform;
                    weight.weight0 = 1;
                    weight.weight1 = 0;
                    weight.weight2 = 0;
                    weight.weight3 = 0;
                    _currentMorphBone.InternalRangeVertexs.Add(i);
                }
                else if (distance > _currentMorphBone.InternalRange && distance <= _currentMorphBone.ExternalRange)
                {
                    if (weight.bone0 == _currentMorphBone.transform)
                    {
                        weight.weight0 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (weight.bone1 == _currentMorphBone.transform)
                    {
                        weight.weight1 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (weight.bone2 == _currentMorphBone.transform)
                    {
                        weight.weight2 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (weight.bone3 == _currentMorphBone.transform)
                    {
                        weight.weight3 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (!weight.bone0)
                    {
                        weight.bone0 = _currentMorphBone.transform;
                        weight.weight0 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (!weight.bone1)
                    {
                        weight.bone1 = _currentMorphBone.transform;
                        weight.weight1 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (!weight.bone2)
                    {
                        weight.bone2 = _currentMorphBone.transform;
                        weight.weight2 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else if (!weight.bone3)
                    {
                        weight.bone3 = _currentMorphBone.transform;
                        weight.weight3 = 1 - distance / _currentMorphBone.ExternalRange;
                        weight.AstrictWeightsToOne();
                        _currentMorphBone.ExternalRangeVertexs.Add(i);
                    }
                    else
                    {
                        _currentMorphBone.ErrorVertexs.Add(i);
                    }
                }
            }
        }
        private void ApplyWeights()
        {
            BoneWeight[] weights = new BoneWeight[_mesh.vertexCount];
            List<Transform> bones = _skinnedMeshRenderer.bones.ToList();
            for (int i = 0; i < _morphAnimationData.Vertexs.Count; i++)
            {
                BoneWeight weight = _morphAnimationData.Vertexs[i].VertexWeight.ToBoneWeight(bones);
                for (int j = 0; j < _morphAnimationData.Vertexs[i].VertexIndexs.Count; j++)
                {
                    int index = _morphAnimationData.Vertexs[i].VertexIndexs[j];
                    weights[index] = weight;
                }
            }

            _mesh.boneWeights = weights;
            _mesh.RecalculateNormals();
            _skinnedMeshRenderer.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
        }
        private void RestoreToBindposes()
        {
            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                _skinnedMeshRenderer.bones[i].localPosition = _morphAnimationData.BindposesPosition[i];
                _skinnedMeshRenderer.bones[i].localRotation = _morphAnimationData.BindposesRotation[i];
                _skinnedMeshRenderer.bones[i].localScale = _morphAnimationData.BindposesScale[i];
            }
        }
        private void ResetBindposes()
        {
            _morphAnimationData.BindposesPosition.Clear();
            _morphAnimationData.BindposesRotation.Clear();
            _morphAnimationData.BindposesScale.Clear();

            Matrix4x4[] bindposes = new Matrix4x4[_skinnedMeshRenderer.bones.Length];
            for (int i = 0; i < bindposes.Length; i++)
            {
                bindposes[i] = _skinnedMeshRenderer.bones[i].worldToLocalMatrix * _skinnedMeshRenderer.transform.localToWorldMatrix;
                _morphAnimationData.BindposesPosition.Add(_skinnedMeshRenderer.bones[i].localPosition);
                _morphAnimationData.BindposesRotation.Add(_skinnedMeshRenderer.bones[i].localRotation);
                _morphAnimationData.BindposesScale.Add(_skinnedMeshRenderer.bones[i].localScale);
            }

            _mesh.bindposes = bindposes;
            _mesh.RecalculateNormals();
            _skinnedMeshRenderer.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
        }
        #endregion
    }
}
