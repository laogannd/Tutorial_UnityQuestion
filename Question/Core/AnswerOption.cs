using System;
using UnityEngine;

namespace VRQuestion
{
    // 单个选项数据
    [Serializable]
    public class AnswerOption
    {
        [SerializeField] private string _id;
        [SerializeField, TextArea(1, 4)] private string _content;
        [SerializeField] private bool _isCorrect;
        [SerializeField] private Sprite _icon;

        public string Id => _id;
        public string Content => _content;
        public bool IsCorrect => _isCorrect;
        public Sprite Icon => _icon;

        public AnswerOption() { }

        public AnswerOption(string id, string content, bool isCorrect, Sprite icon = null)
        {
            _id = id;
            _content = content;
            _isCorrect = isCorrect;
            _icon = icon;
        }
    }
}
