using System.Collections.Generic;

namespace VRQuestion
{
    // 反馈策略接口 - 实现此接口可注入自定义反馈逻辑（视觉、音效、震动、Soap事件等）
    public interface IQuestionFeedback
    {
        // 题目展示时
        void OnQuestionPresented(QuestionData question);
        // 选项被选中
        void OnOptionSelected(AnswerOption option);
        // 选项被取消选择（多选时）
        void OnOptionDeselected(AnswerOption option);
        // 提交答案前
        void OnBeforeSubmit(IReadOnlyList<AnswerOption> selected);
        // 提交答案后，反馈结果
        void OnAnswerSubmitted(AnswerResult result);
        // 关闭/重置
        void OnReset();
    }
}
