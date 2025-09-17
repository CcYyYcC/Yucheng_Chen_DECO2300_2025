using System;
using System.Collections.Generic;
using UnityEngine;

public class ResetGroupOnPress : MonoBehaviour
{
    [Header("要复位的父级（其所有子物体都会被记录/复位）")]
    public GameObject groupRoot;

    [Header("选项")]
    [Tooltip("重置所有 Rigidbody 的速度（velocity & angularVelocity）")]
    public bool resetRigidbodyVelocity = true;
    [Tooltip("把 Rigidbody 的 Use Gravity / isKinematic 也恢复为初始值")]
    public bool restoreRigidbodyFlags = true;
    [Tooltip("恢复每个对象的激活状态（SetActive）到初始值")]
    public bool restoreActiveState = true;

    [Tooltip("记录/复位时是否包含【一开始是隐藏】的子物体")]
    public bool includeInactiveAtStart = true;

    [Serializable]
    private class NodeState
    {
        public Transform tf;
        public Vector3 localPos;
        public Quaternion localRot;
        public Vector3 localScale;
        public bool activeSelf;

        public bool hadRb;
        public bool rbUseGravity;
        public bool rbIsKinematic;
    }

    // 记录表：用 Transform 做 key 更稳（引用不变）
    private readonly Dictionary<Transform, NodeState> _states = new Dictionary<Transform, NodeState>();

    void Awake()
    {
        if (!groupRoot) groupRoot = this.gameObject;
        CaptureInitialStates();
    }

    /// <summary>
    /// 记录初始状态（进入场景/脚本启用时调用一次）
    /// </summary>
    [ContextMenu("Capture Initial States (Runtime)")]
    public void CaptureInitialStates()
    {
        _states.Clear();
        // 注意：includeInactive=true 才能抓到一开始被隐藏的节点
        var transforms = groupRoot.GetComponentsInChildren<Transform>(includeInactiveAtStart);
        foreach (var t in transforms)
        {
            var state = new NodeState
            {
                tf = t,
                localPos = t.localPosition,
                localRot = t.localRotation,
                localScale = t.localScale,
                activeSelf = t.gameObject.activeSelf
            };

            var rb = t.GetComponent<Rigidbody>();
            if (rb)
            {
                state.hadRb = true;
                state.rbUseGravity = rb.useGravity;
                state.rbIsKinematic = rb.isKinematic;
            }

            _states[t] = state;
        }
        // 可选：打印数量
        // Debug.Log($"[ResetGroupOnPress] Captured {_states.Count} nodes.");
    }

    /// <summary>
    /// 复位所有子物体到初始状态（在按钮 onPressed 里调用它）
    /// </summary>
    [ContextMenu("Reset All (Runtime)")]
    public void ResetAll()
    {
        // 先恢复激活状态（否则一些被隐藏的物体无法设置 Transform）
        if (restoreActiveState)
        {
            foreach (var kv in _states)
            {
                var s = kv.Value;
                if (s.tf && s.tf.gameObject.activeSelf != s.activeSelf)
                {
                    s.tf.gameObject.SetActive(s.activeSelf);
                }
            }
        }

        // 再恢复 Transform 与刚体状态
        foreach (var kv in _states)
        {
            var s = kv.Value;
            if (!s.tf) continue;

            // 还原局部变换
            s.tf.localPosition = s.localPos;
            s.tf.localRotation = s.localRot;
            s.tf.localScale    = s.localScale;

            // 还原 Rigidbody
            var rb = s.tf.GetComponent<Rigidbody>();
            if (rb)
            {
                if (resetRigidbodyVelocity)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                if (restoreRigidbodyFlags && s.hadRb)
                {
                    rb.useGravity  = s.rbUseGravity;
                    rb.isKinematic = s.rbIsKinematic;
                }
            }
        }
        // Debug.Log("[ResetGroupOnPress] Reset done.");
    }
}
