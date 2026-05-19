using System.Collections.Generic;
using UnityEngine;

namespace VRQuestion
{
    // 触觉反馈 - 通过AutoHand手柄震动反馈答题结果
    // 兼容：仅依赖反射级查询，未引用AutoHand即编译通过
    [RequireComponent(typeof(QuestionPanel))]
    public class HapticFeedback : MonoBehaviour, IQuestionFeedback
    {
        [Range(0f, 1f)] public float selectAmplitude = 0.2f;
        [Range(0f, 1f)] public float selectDuration = 0.05f;
        [Range(0f, 1f)] public float correctAmplitude = 0.6f;
        [Range(0f, 1f)] public float correctDuration = 0.2f;
        [Range(0f, 1f)] public float wrongAmplitude = 1f;
        [Range(0f, 1f)] public float wrongDuration = 0.3f;

        // 由外部（VR Rig 或 AutoHand 适配脚本）注入
        public System.Action<float, float> HapticImpulse;

        private QuestionPanel _panel;

        private void Awake() => _panel = GetComponent<QuestionPanel>();
        private void OnEnable() => _panel.RegisterFeedback(this);
        private void OnDisable() => _panel.UnregisterFeedback(this);

        public void OnQuestionPresented(QuestionData question) { }
        public void OnOptionSelected(AnswerOption option) => HapticImpulse?.Invoke(selectAmplitude, selectDuration);
        public void OnOptionDeselected(AnswerOption option) => HapticImpulse?.Invoke(selectAmplitude, selectDuration);
        public void OnBeforeSubmit(IReadOnlyList<AnswerOption> selected) { }

        public void OnAnswerSubmitted(AnswerResult result)
        {
            if (result.IsAllCorrect) HapticImpulse?.Invoke(correctAmplitude, correctDuration);
            else HapticImpulse?.Invoke(wrongAmplitude, wrongDuration);
        }

        public void OnReset() { }
    }
}
