using System.Collections.Generic;

namespace VRQuestion
{
    // 答题结果
    public class AnswerResult
    {
        public QuestionData Question { get; }
        public IReadOnlyList<AnswerOption> SelectedOptions { get; }
        public IReadOnlyList<AnswerOption> CorrectOptions { get; }
        public IReadOnlyList<AnswerOption> MissedOptions { get; }
        public IReadOnlyList<AnswerOption> WrongOptions { get; }
        public bool IsAllCorrect { get; }
        public bool HasAnySelection { get; }
        public float Score { get; }
        public float ElapsedSeconds { get; }

        public AnswerResult(
            QuestionData question,
            IReadOnlyList<AnswerOption> selectedOptions,
            IReadOnlyList<AnswerOption> correctOptions,
            IReadOnlyList<AnswerOption> missedOptions,
            IReadOnlyList<AnswerOption> wrongOptions,
            bool isAllCorrect,
            float score,
            float elapsedSeconds)
        {
            Question = question;
            SelectedOptions = selectedOptions;
            CorrectOptions = correctOptions;
            MissedOptions = missedOptions;
            WrongOptions = wrongOptions;
            IsAllCorrect = isAllCorrect;
            HasAnySelection = selectedOptions.Count > 0;
            Score = score;
            ElapsedSeconds = elapsedSeconds;
        }
    }
}
