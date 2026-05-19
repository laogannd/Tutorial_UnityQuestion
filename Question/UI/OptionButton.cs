using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRQuestion
{
    // 单个选项按钮 - 实现Unity EventSystem标准接口，自动兼容AutoHand的HandCanvasPointer
    // 强制带LayoutElement，便于在ScrollRect.content的VerticalLayoutGroup下从上到下正确撑开
    [RequireComponent(typeof(Graphic))]
    [RequireComponent(typeof(LayoutElement))]
    public class OptionButton : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Graphic _background;
        [SerializeField] private TextMeshProUGUI _contentLabel;
        [SerializeField] private TextMeshProUGUI _indexLabel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _selectionMark;
        [SerializeField] private GameObject _correctIndicator;
        [SerializeField] private GameObject _wrongIndicator;
        [SerializeField] private GameObject _missedIndicator;

        // 仅供QuestionPanel订阅，外部不直接订阅按钮事件
        public event Action<OptionButton> Clicked;
        public event Action<OptionButton> PointerEntered;
        public event Action<OptionButton> PointerExited;

        public AnswerOption Option { get; private set; }
        public int Index { get; private set; }
        public bool IsSelected { get; private set; }
        public bool Interactable { get; private set; } = true;

        private FeedbackConfig _feedback;
        private OptionVisualState _currentState = OptionVisualState.Normal;

        // 由QuestionPanel在生成选项时调用
        public void Bind(AnswerOption option, int index, FeedbackConfig feedback)
        {
            Option = option;
            Index = index;
            _feedback = feedback;
            IsSelected = false;
            Interactable = true;

            if (_contentLabel != null) _contentLabel.text = option.Content;
            if (_indexLabel != null) _indexLabel.text = IndexToLetter(index);
            if (_iconImage != null)
            {
                bool hasIcon = option.Icon != null;
                _iconImage.gameObject.SetActive(hasIcon);
                if (hasIcon) _iconImage.sprite = option.Icon;
            }

            if (_selectionMark != null) _selectionMark.gameObject.SetActive(false);
            if (_correctIndicator != null) _correctIndicator.SetActive(false);
            if (_wrongIndicator != null) _wrongIndicator.SetActive(false);
            if (_missedIndicator != null) _missedIndicator.SetActive(false);

            ApplyState(OptionVisualState.Normal);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (_selectionMark != null) _selectionMark.gameObject.SetActive(selected);
            ApplyState(selected ? OptionVisualState.Selected : OptionVisualState.Normal);
        }

        public void SetInteractable(bool interactable)
        {
            Interactable = interactable;
            if (!interactable && _currentState == OptionVisualState.Hover)
                ApplyState(IsSelected ? OptionVisualState.Selected : OptionVisualState.Normal);
        }

        public void ApplyState(OptionVisualState state)
        {
            _currentState = state;
            if (_background == null || _feedback == null) return;

            switch (state)
            {
                case OptionVisualState.Normal:
                    _background.color = _feedback.normalColor;
                    break;
                case OptionVisualState.Hover:
                    _background.color = _feedback.hoverColor;
                    break;
                case OptionVisualState.Selected:
                    _background.color = _feedback.selectedColor;
                    break;
                case OptionVisualState.Correct:
                    _background.color = _feedback.correctColor;
                    if (_correctIndicator != null) _correctIndicator.SetActive(true);
                    break;
                case OptionVisualState.Wrong:
                    _background.color = _feedback.wrongColor;
                    if (_wrongIndicator != null) _wrongIndicator.SetActive(true);
                    break;
                case OptionVisualState.Missed:
                    _background.color = _feedback.missedColor;
                    if (_missedIndicator != null) _missedIndicator.SetActive(true);
                    break;
                case OptionVisualState.Disabled:
                    var c = _feedback.normalColor;
                    c.a *= 0.5f;
                    _background.color = c;
                    break;
            }
        }

        // === AutoHand 通过 AutoInputModule 调用以下接口 ===
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Interactable) return;
            Clicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Interactable) return;
            PointerEntered?.Invoke(this);
            if (!IsSelected) ApplyState(OptionVisualState.Hover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Interactable) return;
            PointerExited?.Invoke(this);
            if (!IsSelected) ApplyState(OptionVisualState.Normal);
        }

        // A B C D...选项编号
        private static string IndexToLetter(int index)
        {
            if (index < 0) return string.Empty;
            if (index < 26) return ((char)('A' + index)).ToString();
            return (index + 1).ToString();
        }
    }
}
