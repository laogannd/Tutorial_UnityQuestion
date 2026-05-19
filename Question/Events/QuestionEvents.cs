using UnityEngine.Events;

namespace VRQuestion
{
    // 答题流程事件 - 用于Inspector可视化连线
    [System.Serializable] public class QuestionUnityEvent : UnityEvent<QuestionData> { }
    [System.Serializable] public class OptionUnityEvent : UnityEvent<AnswerOption> { }
    [System.Serializable] public class AnswerResultUnityEvent : UnityEvent<AnswerResult> { }
}
