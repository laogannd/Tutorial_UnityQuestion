using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace VRQuestion
{
    // QuestionData 事件发射器 - 与 NotificationEventInvoker 同模式
    // 挂任意 GameObject，配置事件资产 + 题目资产，由 UnityEvent / 代码触发 Raise
    [AddComponentMenu("VRQuestion/Question Data Event Invoker")]
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class QuestionDataEventInvoker : MonoBehaviour
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "配置"), Required("必须引用 ScriptableEventQuestionData 资产"), LabelText("题目事件资产"),
         Tooltip("Raise 时调用此事件，所有订阅者会收到 _question")]
#endif
        private ScriptableEventQuestionData _event;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "配置"), LabelText("默认题目"),
         Tooltip("Raise() 时发送的题目；可在运行时通过 SetQuestion 切换")]
#endif
        private QuestionData _question;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("事件是否就绪")]
        private bool DebugEventReady => _event != null;

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("当前题目标题")]
        private string DebugQuestionTitle => _question == null ? "<空>" : _question.QuestionText;
#endif

        // 通过 UnityEvent 静态绑定调用，发送 Inspector 中配置好的题目
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "调试"), Button("Raise 默认题目", ButtonSizes.Medium), GUIColor(0.6f, 0.9f, 1f),
         EnableIf(nameof(CanRaiseDefault))]
#endif
        public void Raise() => _event.Raise(_question);

        // 通过 UnityEvent 动态绑定调用，使用传入题目覆盖 Inspector 配置
        public void RaiseWithQuestion(QuestionData question) => _event.Raise(question);

        // 提供运行时切换默认题目的入口
        public void SetQuestion(QuestionData question) => _question = question;

#if ODIN_INSPECTOR
        private bool CanRaiseDefault => _event != null && _question != null;
#endif
    }
}
