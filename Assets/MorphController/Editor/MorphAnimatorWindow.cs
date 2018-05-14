using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MorphController
{
    public class MorphAnimatorWindow : EditorWindow
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private MorphAnimationData _morphAnimationData;
        private MorphAnimator _morphAnimator;

        private MorphAnimationClip _currentClip;
        private MorphAnimationKeyframe _currentKeyframe;
        private Transform _currentBone;
        private Transform _currentBoneParent;
        private float _keepingDistance;

        private bool _isRecord;
        private bool _isPreview;
        private int _previewIndex;
        private float _previewLocation;
        private bool _isRenameClip;
        private string _newNameClip;
        private Vector2 _keyframeView;
        private int _clipSizeWidth;
        private int _clipSizeHeight;
        private Vector2 _moveCenter;
        private int _keyframePropertyViewWidth;
        private int _keyframePropertyViewHeight;
        private Vector2 _keyframePropertyView;
        private string _boneNameFiltrate;
        private int _clipPropertyViewWidth;
        private int _clipPropertyViewHeight;

        public void Init(SkinnedMeshRenderer skinnedMesh, MorphAnimationData data, MorphAnimator animator)
        {
            _skinnedMeshRenderer = skinnedMesh;
            _morphAnimationData = data;
            _morphAnimator = animator;

            _currentClip = null;
            _currentKeyframe = null;
            _currentBone = null;
            _currentBoneParent = null;
            _keepingDistance = 0;

            _isRecord = false;
            _isPreview = false;
            _previewIndex = 0;
            _previewLocation = 0;
            _isRenameClip = false;
            _newNameClip = "";
            _keyframeView = Vector2.zero;
            _clipSizeWidth = 150;
            _clipSizeHeight = 30;
            _moveCenter = Vector2.zero;
            _keyframePropertyViewWidth = 0;
            _keyframePropertyViewHeight = 0;
            _keyframePropertyView = Vector2.zero;
            _boneNameFiltrate = "";
            _clipPropertyViewWidth = 0;
            _clipPropertyViewHeight = 0;

            ReviewClips();
        }

        private void Update()
        {
            AutoClose();
            ResetBone();
            LengthKeeping();
            Recording();
            PreviewAnimation();
        }

        private void OnGUI()
        {
            RightMenuGUI();
            MoveViewGUI();

            MorphEditorTool.SetGUIEnabled(!_isPreview);
            MorphEditorTool.SetGUIBackgroundColor(_isRecord ? Color.red : Color.white);

            TitleGUI();

            MorphEditorTool.SetGUIBackgroundColor(Color.white);

            KeyframeGUI();
            
            GUILayout.FlexibleSpace();

            ClipsGUI();
            ClipPropertyGUI();
            KeyframePropertyGUI();
        }
        private void RightMenuGUI()
        {
            if (_isPreview)
            {
                return;
            }

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                GenericMenu gm = new GenericMenu();
                Vector2 mousePos = Event.current.mousePosition;
                gm.AddItem(new GUIContent("Create Clip"), false, delegate ()
                {
                    CreateClip(mousePos);
                    Repaint();
                });
                gm.AddItem(new GUIContent("Clear Invalid Clip"), false, delegate ()
                {
                    if (EditorUtility.DisplayDialog("Prompt", "Whether to clear invalid clips?This is unrecoverable.", "Sure", "Cancel"))
                    {
                        ClearInvalidClip();
                        Repaint();
                    }
                });
                for (int i = 0; i < _morphAnimator.Clips.Count; i++)
                {
                    if (_morphAnimator.Clips[i].Valid)
                    {
                        int si = i;
                        gm.AddItem(new GUIContent("Find Clip/" + _morphAnimator.Clips[si].Name), _currentClip == _morphAnimator.Clips[si], delegate ()
                        {
                            SelectClip(_morphAnimator.Clips[si]);
                            FindCurrentClip();
                        });
                    }
                }
                gm.ShowAsContext();
            }
        }
        private void MoveViewGUI()
        {
            if (_isPreview)
            {
                return;
            }

            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    _moveCenter = Event.current.mousePosition;
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 direction = (Event.current.mousePosition - _moveCenter);
                    for (int i = 0; i < _morphAnimator.Clips.Count; i++)
                    {
                        _morphAnimator.Clips[i].Anchor += direction;
                    }
                    _moveCenter = Event.current.mousePosition;
                    Repaint();
                }
                Event.current.type = EventType.Used;
            }
        }
        private void TitleGUI()
        {
            GUILayout.BeginHorizontal("Toolbar");
            if (GUILayout.Button(_skinnedMeshRenderer.transform.name, "Toolbarbutton", GUILayout.Width(100)))
            {
                Selection.activeGameObject = _skinnedMeshRenderer.gameObject;
            }
            if (GUILayout.Button(MorphStyle.IconGUIContent("Animation.Record"), "Toolbarbutton"))
            {
                _isRecord = !_isRecord;
            }
            if (GUILayout.Button(MorphStyle.IconGUIContent("Animation.PrevKey"), "Toolbarbutton"))
            {
                if (_currentKeyframe != null)
                {
                    int index = _currentClip.Keyframes.IndexOf(_currentKeyframe);
                    index -= 1;
                    if (index < 0)
                    {
                        index = _currentClip.Keyframes.Count - 1;
                    }
                    SelectKeyframe(_currentClip.Keyframes[index]);
                }
            }

            MorphEditorTool.SetGUIEnabled(true);
            if (GUILayout.Toggle(_isPreview, MorphStyle.IconGUIContent("Animation.Play"), "Toolbarbutton") != _isPreview)
            {
                _isPreview = !_isPreview;
                if (_isPreview)
                {
                    _previewIndex = 0;
                    _previewLocation = 0;
                    if (_currentClip == null || !_currentClip.Eligible)
                    {
                        MorphDebug.LogError("无法预览动画！当前未选中动画剪辑或选中的剪辑关键帧数小于2！", _skinnedMeshRenderer.gameObject);
                        _isPreview = false;
                    }
                }
            }
            MorphEditorTool.SetGUIEnabled(!_isPreview);

            if (GUILayout.Button(MorphStyle.IconGUIContent("Animation.NextKey"), "Toolbarbutton"))
            {
                if (_currentKeyframe != null)
                {
                    int index = _currentClip.Keyframes.IndexOf(_currentKeyframe);
                    index += 1;
                    if (index >= _currentClip.Keyframes.Count)
                    {
                        index = 0;
                    }
                    SelectKeyframe(_currentClip.Keyframes[index]);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create Clip", "Toolbarbutton"))
            {
                CreateClip(new Vector2(position.width / 2, position.height / 2));
            }
            if (_currentClip != null)
            {
                if (GUILayout.Button("Rename Clip", "Toolbarbutton"))
                {
                    _isRenameClip = !_isRenameClip;
                    if (_isRenameClip)
                    {
                        _newNameClip = _currentClip.Name;
                    }
                }
                if (GUILayout.Button("Delete Clip", "Toolbarbutton"))
                {
                    if (EditorUtility.DisplayDialog("Prompt", "Whether to delete clip '" + _currentClip.Name + "'?This is unrecoverable.", "Sure", "Cancel"))
                    {
                        DeleteClip(_currentClip);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        private void KeyframeGUI()
        {
            if (_currentClip != null)
            {
                GUILayout.BeginHorizontal("Toolbar");
                if (GUILayout.Button(_currentClip.Name, "ToolbarPopup", GUILayout.Width(100)))
                {
                    GenericMenu gm = new GenericMenu();
                    for (int i = 0; i < _morphAnimator.Clips.Count; i++)
                    {
                        MorphAnimationClip clip = _morphAnimator.Clips[i];
                        if (clip.Valid)
                        {
                            gm.AddItem(new GUIContent(clip.Name), _currentClip == clip, () =>
                            {
                                SelectClip(clip);
                            });
                        }
                    }
                    gm.ShowAsContext();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Keyframe", "Toolbarbutton"))
                {
                    CreateKeyframe();
                }
                if (_currentKeyframe != null)
                {
                    if (GUILayout.Button("Clone Keyframe", "Toolbarbutton"))
                    {
                        CloneKeyframe();
                    }
                    if (GUILayout.Button("Delete Keyframe", "Toolbarbutton"))
                    {
                        if (EditorUtility.DisplayDialog("Prompt", "Whether to delete current keyframe?This is unrecoverable.", "Sure", "Cancel"))
                        {
                            DeleteKeyframe();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                _keyframeView = GUILayout.BeginScrollView(_keyframeView);
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("Keyframes:");
                if (_currentClip.Keyframes.Count <= 0)
                {
                    GUILayout.Label("Please add a keyframe!");
                }
                else
                {
                    for (int i = 0; i < _currentClip.Keyframes.Count; i++)
                    {
                        bool value = (_currentKeyframe == _currentClip.Keyframes[i]);
                        MorphEditorTool.SetGUIColor(_currentClip.Keyframes[i].EventCallBack.CallTarget ? Color.cyan : Color.white);
                        if (GUILayout.Toggle(value, (i + 1).ToString(), "PreButton") != value)
                        {
                            SelectKeyframe(_currentClip.Keyframes[i]);
                        }
                    }
                    MorphEditorTool.SetGUIColor(Color.white);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Please select a clip!");
                GUILayout.EndHorizontal();
            }
        }
        private void ClipsGUI()
        {
            string currentStyle = "";
            string otherStyle = "";
            for (int i = 0; i < _morphAnimator.Clips.Count; i++)
            {
                if (_currentClip != _morphAnimator.Clips[i])
                {
                    MorphAnimationClip clip = _morphAnimator.Clips[i];

                    if (!_isPreview)
                    {
                        MorphEditorTool.SetGUIEnabled(clip.Valid);
                    }

                    Rect rect = new Rect(clip.Anchor, new Vector2(_clipSizeWidth, _clipSizeHeight));
                    otherStyle = ((_morphAnimator.DefaultClipIndex == i) ? "flow node 5" : "flow node 0");
                    if (GUI.Button(rect, clip.Name, otherStyle))
                    {
                        SelectClip(clip);
                        currentStyle = otherStyle + " on";
                    }
                }
                else
                {
                    currentStyle = ((_morphAnimator.DefaultClipIndex == i) ? "flow node 5 on" : "flow node 0 on");
                }
            }
            if (_currentClip != null)
            {
                if (!_isPreview)
                {
                    MorphEditorTool.SetGUIEnabled(_currentClip.Valid);
                }

                Rect rect = new Rect(_currentClip.Anchor, new Vector2(_clipSizeWidth, _clipSizeHeight));
                if (GUI.RepeatButton(rect, _currentClip.Name, currentStyle))
                {
                    _currentClip.Anchor = Event.current.mousePosition - new Vector2(_clipSizeWidth / 2, _clipSizeHeight / 2);
                    Repaint();
                }
            }

            if (!_isPreview)
            {
                MorphEditorTool.SetGUIEnabled(true);
            }

            for (int i = 0; i < _morphAnimator.Clips.Count; i++)
            {
                if (_morphAnimator.Clips[i].TransitionClip != -1)
                {
                    MorphAnimationClip clip = _morphAnimator.Clips[i];
                    Vector2 clipVec = new Vector2(clip.Anchor.x + _clipSizeWidth, clip.Anchor.y + _clipSizeHeight / 2);
                    MorphAnimationClip transitionClip = _morphAnimator.Clips[clip.TransitionClip];
                    Vector2 transitionVec = new Vector2(transitionClip.Anchor.x, transitionClip.Anchor.y + _clipSizeHeight / 2);
                    MorphHandles.DrawTransition(clipVec, transitionVec);
                }
            }
        }
        private void ClipPropertyGUI()
        {
            if (_currentClip != null)
            {
                _clipPropertyViewWidth = (int)position.width - 204;

                Rect rect = new Rect(_clipPropertyViewWidth, 85, 200, _clipPropertyViewHeight);
                GUI.BeginGroup(rect, new GUIStyle("box"));

                _clipPropertyViewHeight = 4;

                if (_isRenameClip)
                {
                    _newNameClip = GUI.TextField(new Rect(4, _clipPropertyViewHeight, 100, 16), _newNameClip);
                    if (GUI.Button(new Rect(108, _clipPropertyViewHeight, 40, 16), "Sure", "MiniButtonLeft"))
                    {
                        if (MorphEditorTool.ClipNameIsAllow(_morphAnimator.Clips, _newNameClip))
                        {
                            _currentClip.Name = _newNameClip;
                            _newNameClip = "";
                            _isRenameClip = false;
                        }
                        else
                        {
                            MorphDebug.LogError("输入的剪辑名字不符合规定或者存在重名！", _skinnedMeshRenderer.gameObject);
                        }
                    }
                    if (GUI.Button(new Rect(148, _clipPropertyViewHeight, 48, 16), "Cancel", "MiniButtonRight"))
                    {
                        _isRenameClip = false;
                    }
                    _clipPropertyViewHeight += 20;
                }

                GUI.Label(new Rect(4, _clipPropertyViewHeight, 192, 16), _currentClip.Name, "PreLabel");
                _clipPropertyViewHeight += 20;

                if (GUI.Button(new Rect(4, _clipPropertyViewHeight, 192, 16), "Set Default Clip"))
                {
                    int index = _morphAnimator.Clips.IndexOf(_currentClip);
                    _morphAnimator.DefaultClipIndex = index;
                }
                _clipPropertyViewHeight += 20;

                GUI.Label(new Rect(4, _clipPropertyViewHeight, 192, 16), "Transition: " + (_currentClip.TransitionClip != -1 ? _morphAnimator.Clips[_currentClip.TransitionClip].Name : "<None>"));
                _clipPropertyViewHeight += 20;

                if (GUI.Button(new Rect(4, _clipPropertyViewHeight, 192, 16), "Make Transition"))
                {
                    GenericMenu gm = new GenericMenu();
                    for (int i = 0; i < _morphAnimator.Clips.Count; i++)
                    {
                        if (_morphAnimator.Clips[i] != _currentClip)
                        {
                            int si = i;
                            gm.AddItem(new GUIContent(_morphAnimator.Clips[si].Name), si == _currentClip.TransitionClip, () =>
                            {
                                _currentClip.TransitionClip = si;
                            });
                        }
                    }
                    gm.ShowAsContext();
                }
                _clipPropertyViewHeight += 20;
                
                GUI.EndGroup();
            }
        }
        private void KeyframePropertyGUI()
        {
            if (_currentKeyframe != null)
            {
                Rect viewRect = new Rect(_keyframePropertyViewWidth, 85, 200, position.height - 70);
                Rect maxRect = new Rect(_keyframePropertyViewWidth, 85, 200, _keyframePropertyViewHeight);

                _keyframePropertyView = GUI.BeginScrollView(viewRect, _keyframePropertyView, maxRect);
                GUI.BeginGroup(maxRect, new GUIStyle("box"));

                _keyframePropertyViewWidth = 4;
                _keyframePropertyViewHeight = 4;

                GUI.Label(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 35, 16), "Time:");
                _currentKeyframe.Time = EditorGUI.FloatField(new Rect(43, _keyframePropertyViewHeight, 153, 16), _currentKeyframe.Time);
                _keyframePropertyViewHeight += 20;

                MorphEditorTool.SetGUIColor(_currentKeyframe.EventCallBack.CallTarget ? Color.white : Color.gray);
                GUI.Label(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 85, 16), "Event Target:");
                _currentKeyframe.EventCallBack.CallTarget = EditorGUI.ObjectField(new Rect(93, _keyframePropertyViewHeight, 103, 16), _currentKeyframe.EventCallBack.CallTarget, typeof(GameObject), true) as GameObject;
                _keyframePropertyViewHeight += 20;

                GUI.Label(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 85, 16), "Event Method:");
                if (GUI.Button(new Rect(93, _keyframePropertyViewHeight, 103, 16), _currentKeyframe.EventCallBack.CallMethod, "MiniPopup"))
                {
                    if (_currentKeyframe.EventCallBack.CallTarget)
                    {
                        GenericMenu gm = new GenericMenu();
                        Component[] cps = _currentKeyframe.EventCallBack.CallTarget.GetComponents<Component>();
                        for (int j = 0; j < cps.Length; j++)
                        {
                            Type type = cps[j].GetType();
                            MethodInfo[] mis = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                            for (int n = 0; n < mis.Length; n++)
                            {
                                string methodName = mis[n].Name;
                                if (!methodName.StartsWith("set_") && !methodName.StartsWith("get_"))
                                {
                                    gm.AddItem(new GUIContent(type.Name + "/" + methodName), methodName == _currentKeyframe.EventCallBack.CallMethod, () =>
                                    {
                                        _currentKeyframe.EventCallBack.CallMethod = methodName;
                                    });
                                }
                            }
                        }
                        gm.ShowAsContext();
                    }
                }
                _keyframePropertyViewHeight += 20;
                MorphEditorTool.SetGUIColor(Color.white);

                GUI.Label(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 192, 16), "Bone List", "PreLabel");
                _keyframePropertyViewHeight += 20;

                GUI.Label(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 35, 16), "Find:");
                _boneNameFiltrate = GUI.TextField(new Rect(43, _keyframePropertyViewHeight, 137, 16), _boneNameFiltrate, "SearchTextField");
                if (GUI.Button(new Rect(180, _keyframePropertyViewHeight, 16, 16), "", _boneNameFiltrate == "" ? "SearchCancelButtonEmpty" : "SearchCancelButton"))
                {
                    _boneNameFiltrate = "";
                }
                _keyframePropertyViewHeight += 20;

                for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                {
                    if (_skinnedMeshRenderer.bones[i].name.Contains(_boneNameFiltrate))
                    {
                        bool value = (_currentBone == _skinnedMeshRenderer.bones[i]);
                        if (GUI.Toggle(new Rect(_keyframePropertyViewWidth, _keyframePropertyViewHeight, 192, 16), value, _skinnedMeshRenderer.bones[i].name, "PreButton") != value)
                        {
                            _currentBone = _skinnedMeshRenderer.bones[i];
                            _currentBoneParent = null;
                            Selection.activeGameObject = _currentBone.gameObject;
                            Tools.current = Tool.Move;

                            if (_currentBone != _skinnedMeshRenderer.rootBone && _currentBone.parent && _currentBone.parent != _skinnedMeshRenderer.rootBone)
                            {
                                _currentBoneParent = _currentBone.parent;
                                _keepingDistance = Vector3.Distance(_currentBone.position, _currentBoneParent.position);
                            }
                        }
                        _keyframePropertyViewHeight += 20;
                    }
                }

                GUI.EndGroup();
                GUI.EndScrollView();
            }
        }

        private void OnDestroy()
        {
            RestoreToBindposes();
            MorphSkinnedMeshEditor.IsOpeningAnimator = false;
        }

        #region Update
        private void AutoClose()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
            {
                Close();
            }
        }
        private void ResetBone()
        {
            if (_currentBone)
            {
                if (_currentBone.gameObject != Selection.activeGameObject)
                {
                    _currentBone = null;
                }
            }
        }
        private void LengthKeeping()
        {
            if (!_isPreview && _currentBone != null && _currentBoneParent != null)
            {
                float distance = Vector3.Distance(_currentBone.position, _currentBoneParent.position);
                if (!Mathf.Approximately(_keepingDistance, distance))
                {
                    Vector3 direction = (_currentBone.position - _currentBoneParent.position).normalized;
                    _currentBone.position = _currentBoneParent.position + direction * _keepingDistance;
                }
            }
        }
        private void Recording()
        {
            if (_isPreview)
            {
                _isRecord = false;
            }

            if (_isRecord && _currentKeyframe != null && _currentBone != null)
            {
                for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                {
                    _currentKeyframe.Positions[i] = _skinnedMeshRenderer.bones[i].localPosition;
                    _currentKeyframe.Rotations[i] = _skinnedMeshRenderer.bones[i].localRotation;
                    _currentKeyframe.Scales[i] = _skinnedMeshRenderer.bones[i].localScale;
                }
            }
        }
        private void PreviewAnimation()
        {
            if (_isPreview)
            {
                MorphAnimationKeyframe currentframe = _currentClip.Keyframes[_previewIndex];
                MorphAnimationKeyframe lastframe;
                if (_previewIndex + 1 >= _currentClip.Keyframes.Count)
                    lastframe = _currentClip.Keyframes[0];
                else
                    lastframe = _currentClip.Keyframes[_previewIndex + 1];

                if (_previewLocation <= currentframe.Time)
                {
                    _previewLocation += Time.deltaTime;
                }
                else
                {
                    _previewIndex += 1;
                    _previewLocation = 0f;

                    if (_previewIndex >= _currentClip.Keyframes.Count)
                    {
                        _previewIndex = 0;
                    }
                    return;
                }

                float location = _previewLocation / currentframe.Time;
                for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                {
                    _skinnedMeshRenderer.bones[i].localPosition = Vector3.Lerp(currentframe.Positions[i], lastframe.Positions[i], location);
                    _skinnedMeshRenderer.bones[i].localRotation = Quaternion.Lerp(currentframe.Rotations[i], lastframe.Rotations[i], location);
                    _skinnedMeshRenderer.bones[i].localScale = Vector3.Lerp(currentframe.Scales[i], lastframe.Scales[i], location);
                }
            }
        }
        #endregion

        #region Auxiliary
        private void CreateClip(Vector2 anchor)
        {
            MorphAnimationClip clip = new MorphAnimationClip("NewClip" + _morphAnimator.Clips.Count, _skinnedMeshRenderer.bones.Length, anchor);
            _morphAnimator.Clips.Add(clip);

            if (_morphAnimator.DefaultClipIndex == -1)
            {
                _morphAnimator.DefaultClipIndex = 0;
            }
        }
        private void ClearInvalidClip()
        {
            for (int i = 0; i < _morphAnimator.Clips.Count; i++)
            {
                if (!_morphAnimator.Clips[i].Valid)
                {
                    DeleteClip(_morphAnimator.Clips[i]);
                    i--;
                }
            }
        }
        private void DeleteClip(MorphAnimationClip clip)
        {
            int index = _morphAnimator.Clips.IndexOf(clip);
            if (_morphAnimator.DefaultClipIndex >= index)
            {
                _morphAnimator.DefaultClipIndex -= 1;
            }
            
            _morphAnimator.Clips.Remove(clip);
            _currentClip = null;
            _currentKeyframe = null;

            if (_morphAnimator.DefaultClipIndex == -1 && _morphAnimator.Clips.Count > 0)
            {
                _morphAnimator.DefaultClipIndex = 0;
            }

            for (int i = 0; i < _morphAnimator.Clips.Count; i++)
            {
                if (_morphAnimator.Clips[i].TransitionClip == index)
                {
                    _morphAnimator.Clips[i].TransitionClip = -1;
                }
                else if (_morphAnimator.Clips[i].TransitionClip > index)
                {
                    _morphAnimator.Clips[i].TransitionClip -= 1;
                }
            }
        }
        private void SelectClip(MorphAnimationClip clip)
        {
            _currentClip = clip;
            _currentKeyframe = null;
        }
        private void FindCurrentClip()
        {
            if (_currentClip != null)
            {
                _currentClip.Anchor = new Vector2(position.width / 2, position.height / 2);
            }
        }

        private void CreateKeyframe()
        {
            _currentClip.Keyframes.Add(new MorphAnimationKeyframe(_morphAnimationData));

            if (_currentClip.Keyframes.Count >= 2)
            {
                _currentClip.Eligible = true;
            }
        }
        private void CloneKeyframe()
        {
            MorphAnimationKeyframe frame = new MorphAnimationKeyframe(_morphAnimationData);
            frame.CopyBy(_currentKeyframe);
            _currentClip.Keyframes.Add(frame);
            _currentKeyframe = frame;

            if (_currentClip.Keyframes.Count >= 2)
            {
                _currentClip.Eligible = true;
            }
        }
        private void DeleteKeyframe()
        {
            _currentClip.Keyframes.Remove(_currentKeyframe);
            _currentKeyframe = null;

            if (_currentClip.Keyframes.Count < 2)
            {
                _currentClip.Eligible = false;
            }
        }
        private void SelectKeyframe(MorphAnimationKeyframe frame)
        {
            _currentKeyframe = frame;

            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                _skinnedMeshRenderer.bones[i].localPosition = frame.Positions[i];
                _skinnedMeshRenderer.bones[i].localRotation = frame.Rotations[i];
                _skinnedMeshRenderer.bones[i].localScale = frame.Scales[i];
            }
        }

        private void RestoreToBindposes()
        {
            if (_skinnedMeshRenderer)
            {
                for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
                {
                    _skinnedMeshRenderer.bones[i].localPosition = _morphAnimationData.BindposesPosition[i];
                    _skinnedMeshRenderer.bones[i].localRotation = _morphAnimationData.BindposesRotation[i];
                    _skinnedMeshRenderer.bones[i].localScale = _morphAnimationData.BindposesScale[i];
                }
            }
        }
        private void ReviewClips()
        {
            for (int i = 0; i < _morphAnimator.Clips.Count; i++)
            {
                if (_morphAnimator.Clips[i].BoneNumber == _skinnedMeshRenderer.bones.Length)
                {
                    _morphAnimator.Clips[i].Valid = true;
                }
                else
                {
                    _morphAnimator.Clips[i].Valid = false;
                    MorphDebug.LogError(_morphAnimator.Clips[i].Name + " 无效，骨骼数量不与网格匹配！");
                }
            }
        }
        #endregion
    }
}