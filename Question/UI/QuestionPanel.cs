using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace VRQuestion
{
    // World Space 答题面板主控件
    // 即插即用：将本组件放到 Canvas (RenderMode = WorldSpace) 上，配置题目即可与AutoHand手部射线交互
    [DisallowMultipleComponent]
    [AddComponentMenu("VRQuestion/Question Panel")]
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class QuestionPanel : MonoBehaviour
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "题目"), LabelText("题目数据"), Tooltip("当前面板展示的题目；自动展示开启时 Start 即用此题")]
#endif
        private QuestionData _question;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "题目"), LabelText("反馈配置"), Tooltip("正确/错误音效、配色、解锁策略等参数集合")]
#endif
        private FeedbackConfig _feedbackConfig;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "题目"), LabelText("启动时自动展示"), Tooltip("启动时是否自动展示_question；否则等待外部调用Present()")]
#endif
        private bool _autoPresentOnStart = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "题目"), LabelText("随机打乱选项")]
#endif
        private bool _shuffleOptions = false;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("题干文本")]
#endif
        private TextMeshProUGUI _questionLabel;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("题干图片")]
#endif
        private Image _questionImage;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("解析文本")]
#endif
        private TextMeshProUGUI _explanationLabel;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("提示文本"), Tooltip("自动显示「单选题」/「多选题」")]
#endif
        private TextMeshProUGUI _hintLabel;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("倒计时文本")]
#endif
        private TextMeshProUGUI _timerLabel;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("选项容器"),
         Tooltip("ScrollRect.content：所有OptionButton的父节点，挂VerticalLayoutGroup+ContentSizeFitter即可从上到下依次生成")]
#endif
        private RectTransform _optionsContainer;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("选项滚动视图"),
         Tooltip("可选：选项滚动视图，切题时自动回到顶部并强制布局重建")]
#endif
        private ScrollRect _optionsScrollRect;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("选项预制体"), Required("必须指定 OptionButton 预制体")]
#endif
        private OptionButton _optionPrefab;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("提交按钮")]
#endif
        private Button _submitButton;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("重置按钮")]
#endif
        private Button _resetButton;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("关闭按钮")]
#endif
        private Button _closeButton;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("结果面板"), Tooltip("提交后激活，显示「回答正确/错误」")]
#endif
        private GameObject _resultPanel;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("结果文本")]
#endif
        private TextMeshProUGUI _resultText;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "UI 引用"), LabelText("音频源")]
#endif
        private AudioSource _audioSource;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "朝向"), LabelText("启用朝向相机"), Tooltip("是否让面板朝向玩家相机（VR头显）")]
#endif
        private bool _faceCameraEnabled = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "朝向"), LabelText("锁定 Y 轴"), EnableIf(nameof(_faceCameraEnabled)),
         Tooltip("仅绕Y轴旋转，保持上方向竖直；适合站立式答题")]
#endif
        private bool _faceCameraLockYAxis = false;

        [SerializeField, Min(0f)]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "朝向"), LabelText("跟随平滑速度"), EnableIf(nameof(_faceCameraEnabled)),
         Tooltip("0=瞬时跟随，>0使用Slerp平滑")]
#endif
        private float _faceCameraSmoothSpeed = 0f;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "Soap 事件"), LabelText("题目事件资产"),
         Tooltip("订阅此事件后，任意位置Raise(QuestionData)即可切题并FadeIn显示")]
#endif
        private ScriptableEventQuestionData _questionDataEvent;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "Soap 事件"), LabelText("CanvasGroup"),
         Tooltip("淡入淡出动画用的CanvasGroup；为空时跳过淡入淡出，直接切题")]
#endif
        private CanvasGroup _canvasGroup;

        [SerializeField, Min(0f)]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "Soap 事件"), LabelText("淡入时长(秒)")]
#endif
        private float _fadeInDuration = 0.25f;

        [SerializeField, Min(0f)]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "Soap 事件"), LabelText("淡出时长(秒)")]
#endif
        private float _fadeOutDuration = 0.25f;

        [SerializeField]
#if ODIN_INSPECTOR
        [TabGroup("Tabs", "Soap 事件"), LabelText("事件触发时自动激活"),
         Tooltip("收到事件时若面板已关闭，自动SetActive(true)再展示")]
#endif
        private bool _autoActivateOnEvent = true;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("题目展示")]
#endif
        public QuestionUnityEvent OnQuestionPresented;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("选项被选中")]
#endif
        public OptionUnityEvent OnOptionSelected;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("选项被取消")]
#endif
        public OptionUnityEvent OnOptionDeselected;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("已提交答案")]
#endif
        public AnswerResultUnityEvent OnAnswerSubmitted;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("答对")]
#endif
        public AnswerResultUnityEvent OnAnswerCorrect;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("答错")]
#endif
        public AnswerResultUnityEvent OnAnswerWrong;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("面板关闭")]
#endif
        public UnityEngine.Events.UnityEvent OnPanelClosed;

#if ODIN_INSPECTOR
        [TabGroup("Tabs", "事件"), LabelText("超时")]
#endif
        public UnityEngine.Events.UnityEvent OnTimeOut;

#if ODIN_INSPECTOR
        // ========== 调试 Tab ==========
        // 运行时只读状态显示 + 一键测试按钮，仅在 Play Mode 真正可用
        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("当前题目"), PropertyOrder(0)]
        private string DebugQuestionTitle => _question == null ? "<空>" : _question.QuestionText;

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("题型"), PropertyOrder(1)]
        private string DebugQuestionType => _question == null
            ? "-"
            : (_question.QuestionType == QuestionType.SingleChoice ? "单选" : "多选");

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("已提交"), PropertyOrder(2)]
        private bool DebugIsSubmitted => IsSubmitted;

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("已锁定"), PropertyOrder(3)]
        private bool DebugIsLocked => IsLocked;

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("活动选项数"), PropertyOrder(4)]
        private int DebugActiveOptionCount => _activeOptions.Count;

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("已选中数量"), PropertyOrder(5)]
        private int DebugSelectedCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < _activeOptions.Count; i++)
                    if (_activeOptions[i].IsSelected) c++;
                return c;
            }
        }

        [TabGroup("Tabs", "调试"), ShowInInspector, ReadOnly, LabelText("已耗时(秒)"), PropertyOrder(6)]
        private float DebugElapsed => Application.isPlaying && _question != null ? Time.time - _startTime : 0f;

        [TabGroup("Tabs", "调试"), PropertySpace(SpaceBefore = 6), PropertyOrder(10)]
        [Button("展示默认题目", ButtonSizes.Medium), GUIColor(0.6f, 0.9f, 1f), EnableIf(nameof(DebugCanPresent))]
        private void DebugPresent()
        {
            if (_question == null) { Debug.LogWarning("[QuestionPanel] _question 为空，无法展示"); return; }
            Present(_question);
        }

        [TabGroup("Tabs", "调试"), PropertyOrder(11)]
        [Button("淡入展示默认题目", ButtonSizes.Medium), EnableIf(nameof(DebugCanPresent))]
        private void DebugPresentWithFade()
        {
            if (_question == null) { Debug.LogWarning("[QuestionPanel] _question 为空，无法展示"); return; }
            PresentWithFade(_question);
        }

        [TabGroup("Tabs", "调试"), PropertyOrder(12)]
        [Button("提交当前作答"), EnableIf(nameof(DebugCanSubmit))]
        private void DebugSubmit() => Submit();

        [TabGroup("Tabs", "调试"), PropertyOrder(13)]
        [Button("重置面板"), EnableIf(nameof(DebugCanPresent))]
        private void DebugReset() => ResetPanel();

        [TabGroup("Tabs", "调试"), PropertyOrder(14)]
        [Button("关闭面板"), GUIColor(1f, 0.7f, 0.7f)]
        private void DebugClose() => Close();

        private bool DebugCanPresent => Application.isPlaying;
        private bool DebugCanSubmit => Application.isPlaying && !IsSubmitted && _activeOptions.Count > 0;
#endif

        // C# 事件 - 给代码订阅者用
        public event Action<QuestionData> QuestionPresented;
        public event Action<AnswerOption> OptionSelected;
        public event Action<AnswerOption> OptionDeselected;
        public event Action<AnswerResult> AnswerSubmitted;
        public event Action PanelClosed;

        public QuestionData CurrentQuestion => _question;
        public IReadOnlyList<OptionButton> ActiveOptions => _activeOptions;
        public bool IsSubmitted { get; private set; }
        public bool IsLocked { get; private set; }
        public bool FaceCameraEnabled => _faceCameraEnabled;

        private readonly List<OptionButton> _activeOptions = new List<OptionButton>();
        private readonly List<OptionButton> _optionPool = new List<OptionButton>();
        private readonly List<AnswerOption> _selectedBuffer = new List<AnswerOption>();
        private readonly List<IQuestionFeedback> _feedbackHandlers = new List<IQuestionFeedback>();

        private float _startTime;
        private Coroutine _timerRoutine;
        private Coroutine _autoCloseRoutine;
        private Coroutine _fadeRoutine;
        private Transform _cameraTransform;

        private void Awake()
        {
            if (_submitButton != null) _submitButton.onClick.AddListener(Submit);
            if (_resetButton != null) _resetButton.onClick.AddListener(ResetPanel);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            if (_cameraTransform == null && Camera.main != null) _cameraTransform = Camera.main.transform;
            if (_questionDataEvent != null) _questionDataEvent.OnRaised += HandleQuestionDataEvent;
        }

        private void OnDisable()
        {
            if (_questionDataEvent != null) _questionDataEvent.OnRaised -= HandleQuestionDataEvent;
        }

        private void Start()
        {
            if (_autoPresentOnStart && _question != null)
                Present(_question);
        }

        private void OnDestroy()
        {
            if (_submitButton != null) _submitButton.onClick.RemoveListener(Submit);
            if (_resetButton != null) _resetButton.onClick.RemoveListener(ResetPanel);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
        }

        private void LateUpdate()
        {
            if (!_faceCameraEnabled || _cameraTransform == null) return;

            Vector3 forward = _cameraTransform.rotation * Vector3.forward;
            Vector3 up = _cameraTransform.rotation * Vector3.up;
            if (_faceCameraLockYAxis)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f) return;
                up = Vector3.up;
            }

            Quaternion target = Quaternion.LookRotation(forward, up);
            if (_faceCameraSmoothSpeed <= 0f) transform.rotation = target;
            else transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _faceCameraSmoothSpeed);
        }

        // 运行时开关朝向相机
        public void SetFaceCameraEnabled(bool enabled) => _faceCameraEnabled = enabled;

        // 注册自定义反馈处理器（运行时扩展）
        public void RegisterFeedback(IQuestionFeedback feedback)
        {
            if (feedback != null && !_feedbackHandlers.Contains(feedback))
                _feedbackHandlers.Add(feedback);
        }

        public void UnregisterFeedback(IQuestionFeedback feedback)
        {
            if (feedback != null) _feedbackHandlers.Remove(feedback);
        }

        // Soap事件入口：FadeOut → 切题 → FadeIn
        private void HandleQuestionDataEvent(QuestionData question)
        {
            if (question == null) return;
            if (_autoActivateOnEvent && !gameObject.activeSelf) gameObject.SetActive(true);
            PresentWithFade(question);
        }

        // 带淡入淡出的展示（外部也可直接调用）
        public void PresentWithFade(QuestionData question)
        {
            if (question == null)
            {
                Debug.LogError("[QuestionPanel] PresentWithFade 传入的 question 为空");
                return;
            }
            if (_canvasGroup == null)
            {
                Present(question);
                return;
            }
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeSwitchRoutine(question));
        }

        private IEnumerator FadeSwitchRoutine(QuestionData question)
        {
            // 首次展示（无旧题目）或已透明时跳过淡出
            bool needFadeOut = _question != null && _canvasGroup.alpha > 0.001f && _fadeOutDuration > 0f;
            if (needFadeOut) yield return Fade(_canvasGroup.alpha, 0f, _fadeOutDuration);
            else _canvasGroup.alpha = 0f;

            Present(question);

            if (_fadeInDuration > 0f) yield return Fade(0f, 1f, _fadeInDuration);
            else _canvasGroup.alpha = 1f;

            _fadeRoutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
        }

        // 展示题目（同步切题，不带淡入淡出）
        public void Present(QuestionData question)
        {
            if (question == null)
            {
                Debug.LogError("[QuestionPanel] Present 传入的 question 为空");
                return;
            }
            if (!question.Validate(out var error))
            {
                Debug.LogError($"[QuestionPanel] {error}");
                return;
            }

            _question = question;
            IsSubmitted = false;
            IsLocked = false;
            _selectedBuffer.Clear();

            if (_autoCloseRoutine != null) { StopCoroutine(_autoCloseRoutine); _autoCloseRoutine = null; }
            if (_timerRoutine != null) { StopCoroutine(_timerRoutine); _timerRoutine = null; }
            if (_resultPanel != null) _resultPanel.SetActive(false);

            if (_questionLabel != null) _questionLabel.text = question.QuestionText;
            if (_questionImage != null)
            {
                bool hasImage = question.QuestionImage != null;
                _questionImage.gameObject.SetActive(hasImage);
                if (hasImage) _questionImage.sprite = question.QuestionImage;
            }
            if (_explanationLabel != null)
            {
                _explanationLabel.gameObject.SetActive(false);
                _explanationLabel.text = question.Explanation;
            }
            if (_hintLabel != null)
            {
                _hintLabel.text = question.QuestionType == QuestionType.SingleChoice
                    ? "单选题"
                    : "多选题";
            }

            BuildOptions(question);
            RefreshOptionsLayout();

            UpdateSubmitButtonState();

            _startTime = Time.time;
            if (question.TimeLimit > 0f)
                _timerRoutine = StartCoroutine(TimerRoutine(question.TimeLimit));
            else if (_timerLabel != null)
                _timerLabel.gameObject.SetActive(false);

            OnQuestionPresented?.Invoke(question);
            QuestionPresented?.Invoke(question);
            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnQuestionPresented(question);
        }

        // 用对象池构建选项
        private void BuildOptions(QuestionData question)
        {
            // 释放旧选项回池
            for (int i = 0; i < _activeOptions.Count; i++)
            {
                var btn = _activeOptions[i];
                btn.Clicked -= HandleOptionClicked;
                btn.gameObject.SetActive(false);
            }
            _activeOptions.Clear();

            var ordered = new List<AnswerOption>(question.Options);
            if (_shuffleOptions) Shuffle(ordered);

            for (int i = 0; i < ordered.Count; i++)
            {
                var btn = AcquireOptionButton();
                btn.transform.SetSiblingIndex(i);
                btn.Bind(ordered[i], i, _feedbackConfig);
                btn.Clicked += HandleOptionClicked;
                btn.gameObject.SetActive(true);
                _activeOptions.Add(btn);
            }
        }

        // 强制Layout重建并滚到顶 - 解决Content在切题后选项位置错乱或不刷新尺寸的问题
        private void RefreshOptionsLayout()
        {
            if (_optionsContainer != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_optionsContainer);
            if (_optionsScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _optionsScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private OptionButton AcquireOptionButton()
        {
            for (int i = 0; i < _optionPool.Count; i++)
            {
                if (!_optionPool[i].gameObject.activeSelf)
                    return _optionPool[i];
            }
            var instance = Instantiate(_optionPrefab, _optionsContainer);
            _optionPool.Add(instance);
            return instance;
        }

        private void HandleOptionClicked(OptionButton button)
        {
            if (IsLocked) return;

            if (_question.QuestionType == QuestionType.SingleChoice)
            {
                // 单选：取消其他，选中当前
                for (int i = 0; i < _activeOptions.Count; i++)
                {
                    var other = _activeOptions[i];
                    if (other == button) continue;
                    if (other.IsSelected)
                    {
                        other.SetSelected(false);
                        InvokeDeselected(other.Option);
                    }
                }
                if (!button.IsSelected)
                {
                    button.SetSelected(true);
                    InvokeSelected(button.Option);
                }
            }
            else
            {
                // 多选：切换
                bool nextSelected = !button.IsSelected;
                button.SetSelected(nextSelected);
                if (nextSelected) InvokeSelected(button.Option);
                else InvokeDeselected(button.Option);
            }

            UpdateSubmitButtonState();
            if (_feedbackConfig != null)
                PlayClip(button.IsSelected ? _feedbackConfig.selectClip : _feedbackConfig.deselectClip);
        }

        private void InvokeSelected(AnswerOption option)
        {
            OnOptionSelected?.Invoke(option);
            OptionSelected?.Invoke(option);
            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnOptionSelected(option);
        }

        private void InvokeDeselected(AnswerOption option)
        {
            OnOptionDeselected?.Invoke(option);
            OptionDeselected?.Invoke(option);
            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnOptionDeselected(option);
        }

        private void UpdateSubmitButtonState()
        {
            if (_submitButton == null) return;
            int selectedCount = 0;
            for (int i = 0; i < _activeOptions.Count; i++)
                if (_activeOptions[i].IsSelected) selectedCount++;
            _submitButton.interactable = !IsLocked && selectedCount > 0;
        }

        // 提交答案
        public void Submit()
        {
            if (IsSubmitted || _question == null) return;

            _selectedBuffer.Clear();
            for (int i = 0; i < _activeOptions.Count; i++)
                if (_activeOptions[i].IsSelected)
                    _selectedBuffer.Add(_activeOptions[i].Option);

            if (_selectedBuffer.Count == 0) return;

            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnBeforeSubmit(_selectedBuffer);

            var result = BuildResult(_selectedBuffer);
            IsSubmitted = true;

            if (_feedbackConfig != null) PlayClip(_feedbackConfig.submitClip);

            if (_feedbackConfig != null && _feedbackConfig.lockAfterSubmit)
                LockAllOptions();

            if (_timerRoutine != null) { StopCoroutine(_timerRoutine); _timerRoutine = null; }

            float delay = _feedbackConfig != null ? _feedbackConfig.revealDelay : 0f;
            if (delay > 0f) StartCoroutine(RevealAfterDelay(result, delay));
            else RevealResult(result);
        }

        private IEnumerator RevealAfterDelay(AnswerResult result, float delay)
        {
            yield return new WaitForSeconds(delay);
            RevealResult(result);
        }

        private void RevealResult(AnswerResult result)
        {
            if (_feedbackConfig == null || _feedbackConfig.revealCorrectAnswers)
            {
                for (int i = 0; i < _activeOptions.Count; i++)
                {
                    var btn = _activeOptions[i];
                    var opt = btn.Option;
                    if (btn.IsSelected && opt.IsCorrect) btn.ApplyState(OptionVisualState.Correct);
                    else if (btn.IsSelected && !opt.IsCorrect) btn.ApplyState(OptionVisualState.Wrong);
                    else if (!btn.IsSelected && opt.IsCorrect &&
                             (_feedbackConfig == null || _feedbackConfig.highlightMissedAnswers))
                        btn.ApplyState(OptionVisualState.Missed);
                }
            }

            if (_explanationLabel != null && !string.IsNullOrEmpty(_question.Explanation))
                _explanationLabel.gameObject.SetActive(true);

            if (_resultPanel != null) _resultPanel.SetActive(true);
            if (_resultText != null)
                _resultText.text = result.IsAllCorrect ? "回答正确" : "回答错误";

            if (_feedbackConfig != null)
                PlayClip(result.IsAllCorrect ? _feedbackConfig.correctClip : _feedbackConfig.wrongClip);

            OnAnswerSubmitted?.Invoke(result);
            AnswerSubmitted?.Invoke(result);
            if (result.IsAllCorrect) OnAnswerCorrect?.Invoke(result);
            else OnAnswerWrong?.Invoke(result);

            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnAnswerSubmitted(result);

            if (_feedbackConfig != null && _feedbackConfig.autoCloseDelay > 0f)
                _autoCloseRoutine = StartCoroutine(AutoCloseRoutine(_feedbackConfig.autoCloseDelay));
        }

        private IEnumerator AutoCloseRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Close();
        }

        private AnswerResult BuildResult(List<AnswerOption> selected)
        {
            var correct = new List<AnswerOption>();
            var missed = new List<AnswerOption>();
            var wrong = new List<AnswerOption>();
            var allCorrect = new List<AnswerOption>();

            var options = _question.Options;
            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                bool isSelected = selected.Contains(opt);
                if (opt.IsCorrect) allCorrect.Add(opt);
                if (isSelected && opt.IsCorrect) correct.Add(opt);
                else if (isSelected && !opt.IsCorrect) wrong.Add(opt);
                else if (!isSelected && opt.IsCorrect) missed.Add(opt);
            }

            bool isAllCorrect = wrong.Count == 0 && missed.Count == 0;
            float score = CalculateScore(correct.Count, wrong.Count, allCorrect.Count, isAllCorrect);
            float elapsed = Time.time - _startTime;

            return new AnswerResult(_question, selected, allCorrect, missed, wrong, isAllCorrect, score, elapsed);
        }

        private float CalculateScore(int correctCount, int wrongCount, int totalCorrect, bool isAllCorrect)
        {
            if (_feedbackConfig == null) return isAllCorrect ? 100f : 0f;
            float full = _feedbackConfig.fullScore;
            if (isAllCorrect) return full;
            if (wrongCount > 0) return 0f;
            float ratio = _feedbackConfig.partialScoreRatio;
            if (ratio <= 0f || totalCorrect == 0) return 0f;
            return full * ratio * (correctCount / (float)totalCorrect);
        }

        private void LockAllOptions()
        {
            IsLocked = true;
            for (int i = 0; i < _activeOptions.Count; i++)
                _activeOptions[i].SetInteractable(false);
            if (_submitButton != null) _submitButton.interactable = false;
        }

        // 重置面板（保留当前题目，清空作答）
        public void ResetPanel()
        {
            if (_question == null) return;
            for (int i = 0; i < _feedbackHandlers.Count; i++)
                _feedbackHandlers[i].OnReset();
            Present(_question);
        }

        // 关闭面板
        public void Close()
        {
            if (_autoCloseRoutine != null) { StopCoroutine(_autoCloseRoutine); _autoCloseRoutine = null; }
            if (_timerRoutine != null) { StopCoroutine(_timerRoutine); _timerRoutine = null; }
            if (_fadeRoutine != null) { StopCoroutine(_fadeRoutine); _fadeRoutine = null; }
            OnPanelClosed?.Invoke();
            PanelClosed?.Invoke();
            gameObject.SetActive(false);
        }

        private IEnumerator TimerRoutine(float limit)
        {
            float remaining = limit;
            int lastShown = -1;
            if (_timerLabel != null) _timerLabel.gameObject.SetActive(true);
            while (remaining > 0f && !IsSubmitted)
            {
                int seconds = Mathf.CeilToInt(remaining);
                if (seconds != lastShown && _timerLabel != null)
                {
                    _timerLabel.text = seconds.ToString();
                    lastShown = seconds;
                }
                yield return null;
                remaining -= Time.deltaTime;
            }
            if (!IsSubmitted)
            {
                OnTimeOut?.Invoke();
                // 超时强制提交（即便没选项）
                bool hasSelection = false;
                for (int i = 0; i < _activeOptions.Count; i++)
                    if (_activeOptions[i].IsSelected) { hasSelection = true; break; }
                if (hasSelection) Submit();
                else
                {
                    var result = BuildResult(_selectedBuffer);
                    RevealResult(result);
                    IsSubmitted = true;
                    if (_feedbackConfig != null && _feedbackConfig.lockAfterSubmit) LockAllOptions();
                }
            }
            _timerRoutine = null;
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            float volume = _feedbackConfig != null ? _feedbackConfig.audioVolume : 1f;
            _audioSource.PlayOneShot(clip, volume);
        }

        // Fisher-Yates 洗牌（避免 Linq 分配）
        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
