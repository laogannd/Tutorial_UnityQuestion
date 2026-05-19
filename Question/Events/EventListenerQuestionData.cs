using Obvious.Soap;
using UnityEngine;
using UnityEngine.Events;

namespace VRQuestion
{
    // 与项目内EventListenerGameObject对齐风格的QuestionData事件监听器
    // 用于Inspector可视化绑定：收到事件 → 在UnityEvent中触发任意Component方法
    [AddComponentMenu("Soap/EventListeners/EventListenerQuestionData")]
    public class EventListenerQuestionData : EventListenerGeneric<QuestionData>
    {
        [SerializeField] private EventResponse[] _eventResponses = null;
        protected override EventResponse<QuestionData>[] EventResponses => _eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<QuestionData>
        {
            [SerializeField] private ScriptableEventQuestionData _scriptableEvent = null;
            public override ScriptableEvent<QuestionData> ScriptableEvent => _scriptableEvent;

            [SerializeField] private QuestionDataUnityEvent _response = null;
            public override UnityEvent<QuestionData> Response => _response;
        }

        [System.Serializable]
        public class QuestionDataUnityEvent : UnityEvent<QuestionData>
        {
        }
    }
}
