using UnityEngine;

using Obvious.Soap;

namespace VRQuestion
{
    // 将答题反馈通过项目现有的Soap Notification事件展示
    // 即插即用：挂在QuestionPanel旁，配置事件资产，QuestionPanel会自动调用其反馈接口
    [RequireComponent(typeof(QuestionPanel))]
    public class SoapNotificationFeedback : MonoBehaviour, IQuestionFeedback
    {
        [SerializeField] private ScriptableEventNotification _notificationEvent;
        [SerializeField, TextArea(1, 2)] private string _correctMessage = "回答正确";
        [SerializeField, TextArea(1, 2)] private string _wrongMessage = "回答错误";
        [SerializeField, Min(0f)] private float _displayDuration = 2f;
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.3f;
        [SerializeField] private bool _broadcastOnPresent = false;
        [SerializeField, TextArea(1, 2)] private string _presentMessage = "请作答";

        private QuestionPanel _panel;

        private void Awake()
        {
            _panel = GetComponent<QuestionPanel>();
        }

        private void OnEnable() => _panel.RegisterFeedback(this);
        private void OnDisable() => _panel.UnregisterFeedback(this);

        public void OnQuestionPresented(QuestionData question)
        {
            if (_broadcastOnPresent && _notificationEvent != null)
                Raise(_presentMessage);
        }

        public void OnOptionSelected(AnswerOption option) { }
        public void OnOptionDeselected(AnswerOption option) { }
        public void OnBeforeSubmit(System.Collections.Generic.IReadOnlyList<AnswerOption> selected) { }

        public void OnAnswerSubmitted(AnswerResult result)
        {
            if (_notificationEvent == null) return;
            Raise(result.IsAllCorrect ? _correctMessage : _wrongMessage);
        }

        public void OnReset() { }

        private void Raise(string message)
        {
            var data = new NotificationData
            {
                Message = message,
                DisplayDuration = _displayDuration,
                FadeOutDuration = _fadeOutDuration
            };
            _notificationEvent.Raise(data);
        }
    }
}
