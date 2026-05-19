using System.Collections.Generic;
using UnityEngine;

namespace VRQuestion
{
    // 题目数据 ScriptableObject
    [CreateAssetMenu(menuName = "VR Question/Question Data", fileName = "question_data")]
    public class QuestionData : ScriptableObject
    {
        [SerializeField] private string _questionId;
        [SerializeField] private QuestionType _questionType = QuestionType.SingleChoice;
        [SerializeField, TextArea(2, 6)] private string _questionText;
        [SerializeField] private Sprite _questionImage;
        [SerializeField, TextArea(1, 4)] private string _explanation;
        [SerializeField, Min(0f)] private float _timeLimit = 0f;
        [SerializeField] private List<AnswerOption> _options = new List<AnswerOption>();

        public string QuestionId => _questionId;
        public QuestionType QuestionType => _questionType;
        public string QuestionText => _questionText;
        public Sprite QuestionImage => _questionImage;
        public string Explanation => _explanation;
        public float TimeLimit => _timeLimit;
        public IReadOnlyList<AnswerOption> Options => _options;

        // 校验配置合法性
        public bool Validate(out string error)
        {
            if (_options == null || _options.Count < 2)
            {
                error = $"题目 [{name}] 选项数量不足，至少需要 2 个";
                return false;
            }
            int correctCount = 0;
            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i].IsCorrect) correctCount++;
            }
            if (correctCount == 0)
            {
                error = $"题目 [{name}] 没有正确答案";
                return false;
            }
            if (_questionType == QuestionType.SingleChoice && correctCount > 1)
            {
                error = $"题目 [{name}] 是单选但配置了多个正确答案";
                return false;
            }
            error = null;
            return true;
        }
    }
}
