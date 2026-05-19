using System.Collections.Generic;
using UnityEngine;

namespace VRQuestion
{
    // 题库顺序播放控制器 - 自动按QuestionSet顺序推进
    [RequireComponent(typeof(QuestionPanel))]
    public class QuestionSetRunner : MonoBehaviour
    {
        [SerializeField] private QuestionSet _questionSet;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _advanceOnSubmit = true;
        [SerializeField, Min(0f)] private float _advanceDelay = 1.5f;
        [SerializeField] private bool _loop = false;

        public AnswerResultUnityEvent OnPerQuestionFinished;
        public UnityEngine.Events.UnityEvent OnAllFinished;

        public IReadOnlyList<AnswerResult> Results => _results;
        public int CurrentIndex => _currentIndex;
        public QuestionSet QuestionSet => _questionSet;

        private QuestionPanel _panel;
        private readonly List<QuestionData> _order = new List<QuestionData>();
        private readonly List<AnswerResult> _results = new List<AnswerResult>();
        private int _currentIndex = -1;
        private float _advanceTimer;
        private bool _waitingForAdvance;

        private void Awake()
        {
            _panel = GetComponent<QuestionPanel>();
            _panel.AnswerSubmitted += HandleAnswerSubmitted;
        }

        private void OnDestroy()
        {
            if (_panel != null) _panel.AnswerSubmitted -= HandleAnswerSubmitted;
        }

        private void Start()
        {
            if (_autoStart && _questionSet != null) StartRun(_questionSet);
        }

        private void Update()
        {
            if (!_waitingForAdvance) return;
            _advanceTimer -= Time.deltaTime;
            if (_advanceTimer <= 0f)
            {
                _waitingForAdvance = false;
                Next();
            }
        }

        public void StartRun(QuestionSet set)
        {
            _questionSet = set;
            _results.Clear();
            _order.Clear();
            for (int i = 0; i < set.Questions.Count; i++) _order.Add(set.Questions[i]);
            if (set.ShuffleQuestions) Shuffle(_order);

            _currentIndex = -1;
            Next();
        }

        public void Next()
        {
            _currentIndex++;
            if (_currentIndex >= _order.Count)
            {
                if (_loop)
                {
                    _currentIndex = 0;
                    _results.Clear();
                }
                else
                {
                    OnAllFinished?.Invoke();
                    return;
                }
            }
            _panel.gameObject.SetActive(true);
            _panel.Present(_order[_currentIndex]);
        }

        private void HandleAnswerSubmitted(AnswerResult result)
        {
            _results.Add(result);
            OnPerQuestionFinished?.Invoke(result);
            if (_advanceOnSubmit)
            {
                _waitingForAdvance = true;
                _advanceTimer = _advanceDelay;
            }
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
