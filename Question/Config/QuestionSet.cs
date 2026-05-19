using System.Collections.Generic;
using UnityEngine;

namespace VRQuestion
{
    // 题目集合（试卷 / 题库）
    [CreateAssetMenu(menuName = "VR Question/Question Set", fileName = "question_set")]
    public class QuestionSet : ScriptableObject
    {
        [SerializeField] private string _setId;
        [SerializeField] private string _setName;
        [SerializeField, TextArea(1, 4)] private string _description;
        [SerializeField] private bool _shuffleQuestions = false;
        [SerializeField] private bool _shuffleOptions = false;
        [SerializeField] private List<QuestionData> _questions = new List<QuestionData>();

        public string SetId => _setId;
        public string SetName => _setName;
        public string Description => _description;
        public bool ShuffleQuestions => _shuffleQuestions;
        public bool ShuffleOptions => _shuffleOptions;
        public IReadOnlyList<QuestionData> Questions => _questions;
        public int Count => _questions.Count;
    }
}
