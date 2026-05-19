using UnityEngine;

namespace VRQuestion
{
    // 答题反馈配置 - 单一职责：仅描述反馈表现参数
    [CreateAssetMenu(menuName = "VR Question/Feedback Config", fileName = "feedback_config")]
    public class FeedbackConfig : ScriptableObject
    {
        [Header("Color Feedback")]
        public Color normalColor = new Color(1f, 1f, 1f, 1f);
        public Color hoverColor = new Color(0.85f, 0.92f, 1f, 1f);
        public Color selectedColor = new Color(0.4f, 0.7f, 1f, 1f);
        public Color correctColor = new Color(0.35f, 0.85f, 0.4f, 1f);
        public Color wrongColor = new Color(0.95f, 0.35f, 0.35f, 1f);
        public Color missedColor = new Color(1f, 0.85f, 0.3f, 1f);

        [Header("Audio Feedback")]
        public AudioClip selectClip;
        public AudioClip deselectClip;
        public AudioClip submitClip;
        public AudioClip correctClip;
        public AudioClip wrongClip;
        [Range(0f, 1f)] public float audioVolume = 1f;

        [Header("Reveal Settings")]
        public bool revealCorrectAnswers = true;
        public bool highlightMissedAnswers = true;
        public bool lockAfterSubmit = true;
        [Min(0f)] public float revealDelay = 0.1f;
        [Min(0f)] public float autoCloseDelay = 0f;

        [Header("Score")]
        public float fullScore = 100f;
        // 多选题部分正确得分比例：0 表示全对才得分，1 表示按比例线性给分
        [Range(0f, 1f)] public float partialScoreRatio = 0f;
    }
}
