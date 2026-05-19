using Obvious.Soap;
using UnityEngine;

namespace VRQuestion
{
    // 全局QuestionData事件资产 - 与项目内ScriptableEventNotification相同模式
    // 任意位置Raise此事件即可让所有订阅的QuestionPanel切题
    [CreateAssetMenu(menuName = "Soap/ScriptableEvents/QuestionData", fileName = "scriptable_event_question_data")]
    public class ScriptableEventQuestionData : ScriptableEvent<QuestionData>
    {
    }
}
