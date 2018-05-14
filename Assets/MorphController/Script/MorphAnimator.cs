using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    [DisallowMultipleComponent]
    public class MorphAnimator : MonoBehaviour
    {
        public List<MorphAnimationClip> Clips = new List<MorphAnimationClip>();
        public int DefaultClipIndex = -1;
        public float Speed = 1;
        public UpdateType UpdateMode = UpdateType.Update;
        public bool IgnoreTimeScale = false;

        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private MorphAnimationClip _currentClip;
        private int _playIndex;
        private float _playLocation;
        private MorphDelegate.ExecuteNoArgu _updateAnimation;

        /// <summary>
        /// 当前的动画状态，Current Animation State
        /// </summary>
        public string CurrentState()
        {
            return _currentClip != null ? _currentClip.Name : "null";
        }

        /// <summary>
        /// 切换动画状态，Change animation state
        /// </summary>
        /// <param name="state">要切换到的动画剪辑的名称，Animation clip name</param>
        public void SwitchState(string state)
        {
            if (_skinnedMeshRenderer == null)
            {
                _updateAnimation = null;
                MorphDebug.LogError("物体丢失了 SkinnedMeshRenderer 组件！", gameObject);
                return;
            }

            MorphAnimationClip clip = Clips.Find((c) => { return c.Name == state; });

            if (clip != null)
            {
                SwitchClip(clip);
            }
        }

        private void SwitchClip(MorphAnimationClip clip)
        {
            _currentClip = clip;

            if (!_currentClip.Valid)
            {
                _updateAnimation = null;
                MorphDebug.LogError("动画剪辑 " + _currentClip.Name + " 是无效的！", gameObject);
                return;
            }

            if (!_currentClip.Eligible)
            {
                _updateAnimation = null;
                MorphDebug.LogError("动画剪辑 " + _currentClip.Name + " 无法播放，可能其并未拥有至少两个关键帧！", gameObject);
                return;
            }

            _playIndex = 0;
            _playLocation = 0;

            if (_currentClip.TransitionClip != -1 && Clips[_currentClip.TransitionClip].Valid && Clips[_currentClip.TransitionClip].Eligible)
            {
                _updateAnimation = UpdateAnimationTransition;
            }
            else
            {
                _updateAnimation = UpdateAnimationLoop;
            }

            EventCallBack(_currentClip.Keyframes[_playIndex]);
        }

        private void Start()
        {
            if (Clips.Count <= 0)
            {
                _updateAnimation = null;
                MorphDebug.LogError("当前不存在至少一个动画剪辑！", gameObject);
                return;
            }

            _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (_skinnedMeshRenderer == null)
            {
                _updateAnimation = null;
                MorphDebug.LogError("物体丢失了 SkinnedMeshRenderer 组件！", gameObject);
                return;
            }

            SwitchClip(Clips[DefaultClipIndex]);
        }

        private void Update()
        {
            if (UpdateMode == UpdateType.Update)
            {
                if (_updateAnimation != null)
                    _updateAnimation();
            }
        }

        private void FixedUpdate()
        {
            if (UpdateMode == UpdateType.FixedUpdate)
            {
                if (_updateAnimation != null)
                    _updateAnimation();
            }
        }

        private void UpdateAnimationLoop()
        {
            MorphAnimationKeyframe currentframe = _currentClip.Keyframes[_playIndex];
            MorphAnimationKeyframe lastframe;
            if (_playIndex + 1 >= _currentClip.Keyframes.Count)
                lastframe = _currentClip.Keyframes[0];
            else
                lastframe = _currentClip.Keyframes[_playIndex + 1];

            if (_playLocation <= currentframe.Time)
            {
                _playLocation += Speed * (IgnoreTimeScale ? Time.fixedDeltaTime : Time.deltaTime);
            }
            else
            {
                _playIndex += 1;
                _playLocation = 0f;

                if (_playIndex >= _currentClip.Keyframes.Count)
                {
                    _playIndex = 0;
                }

                EventCallBack(_currentClip.Keyframes[_playIndex]);
                return;
            }

            float location = _playLocation / currentframe.Time;
            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                _skinnedMeshRenderer.bones[i].localPosition = Vector3.Lerp(currentframe.Positions[i], lastframe.Positions[i], location);
                _skinnedMeshRenderer.bones[i].localRotation = Quaternion.Lerp(currentframe.Rotations[i], lastframe.Rotations[i], location);
                _skinnedMeshRenderer.bones[i].localScale = Vector3.Lerp(currentframe.Scales[i], lastframe.Scales[i], location);
            }
        }

        private void UpdateAnimationTransition()
        {
            MorphAnimationKeyframe currentframe = _currentClip.Keyframes[_playIndex];
            MorphAnimationKeyframe lastframe;
            if (_playIndex >= _currentClip.Keyframes.Count - 1)
                lastframe = Clips[_currentClip.TransitionClip].Keyframes[0];
            else
                lastframe = _currentClip.Keyframes[_playIndex + 1];

            if (_playLocation <= currentframe.Time)
            {
                _playLocation += Speed * (IgnoreTimeScale ? Time.fixedDeltaTime : Time.deltaTime);
            }
            else
            {
                _playIndex += 1;
                _playLocation = 0f;

                if (_playIndex >= _currentClip.Keyframes.Count)
                {
                    SwitchClip(Clips[_currentClip.TransitionClip]);
                    return;
                }

                EventCallBack(_currentClip.Keyframes[_playIndex]);
                return;
            }

            float location = _playLocation / currentframe.Time;
            for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
            {
                _skinnedMeshRenderer.bones[i].localPosition = Vector3.Lerp(currentframe.Positions[i], lastframe.Positions[i], location);
                _skinnedMeshRenderer.bones[i].localRotation = Quaternion.Lerp(currentframe.Rotations[i], lastframe.Rotations[i], location);
                _skinnedMeshRenderer.bones[i].localScale = Vector3.Lerp(currentframe.Scales[i], lastframe.Scales[i], location);
            }
        }

        private void EventCallBack(MorphAnimationKeyframe frame)
        {
            if (frame.EventCallBack.CallTarget && frame.EventCallBack.CallMethod != "<None>")
            {
                frame.EventCallBack.CallTarget.SendMessage(frame.EventCallBack.CallMethod);
            }
        }
    }
}
