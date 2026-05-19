using Autohand;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRQuestion
{
    // 将AutoHand手指物理碰触桥接到Unity EventSystem的Click/Enter/Exit事件
    // 放在任何带IPointerClickHandler的UI元素上即可支持手指点触
    [DisallowMultipleComponent]
    [AddComponentMenu("VRQuestion/UI Poke Bridge")]
    public class UIPokeBridge : MonoBehaviour
    {
        [Header("Collider")]
        [SerializeField, Tooltip("BoxCollider Z轴厚度(米)")]
        private float _colliderDepth = 0.008f;

        [SerializeField, Tooltip("Collider中心相对Canvas表面的前移距离(米)")]
        private float _colliderForwardOffset = 0.004f;

        [Header("Interaction")]
        [SerializeField, Tooltip("两次点触之间的冷却时间(秒)，防止手指抖动重复触发")]
        private float _pokeCooldown = 0.4f;

        [SerializeField, Tooltip("是否桥接Hover反馈(IPointerEnter/Exit)")]
        private bool _enableHoverBridge = true;

        [Header("Haptics")]
        [SerializeField, Range(0f, 1f)]
        private float _pokeHapticAmplitude = 0.3f;

        [SerializeField, Range(0f, 0.5f)]
        private float _pokeHapticDuration = 0.05f;

        [SerializeField, Range(0f, 1f)]
        private float _hoverHapticAmplitude = 0.1f;

        [SerializeField, Range(0f, 0.5f)]
        private float _hoverHapticDuration = 0.02f;

        private RectTransform _rect;
        private BoxCollider _boxCollider;
        private HandTouchEvent _touchEvent;
        private float _lastPokeTime = -999f;
        private bool _isHovered;
        private Vector2 _lastSyncedSize;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            EnsureComponents();
            SyncColliderSize();
        }

        private void OnEnable()
        {
            _touchEvent.HandStartTouchEvent += OnHandTouch;
            _touchEvent.HandStopTouchEvent += OnHandUntouch;
        }

        private void OnDisable()
        {
            _touchEvent.HandStartTouchEvent -= OnHandTouch;
            _touchEvent.HandStopTouchEvent -= OnHandUntouch;
            if (_isHovered) ClearHover();
        }

        private void LateUpdate()
        {
            Vector2 currentSize = _rect.rect.size;
            if (currentSize != _lastSyncedSize)
                SyncColliderSize();
        }

        private void OnHandTouch(Hand hand)
        {
            if (_enableHoverBridge && !_isHovered)
            {
                _isHovered = true;
                BridgePointerEnter();
                PlayHaptic(hand, _hoverHapticAmplitude, _hoverHapticDuration);
            }

            if (Time.unscaledTime - _lastPokeTime < _pokeCooldown) return;
            _lastPokeTime = Time.unscaledTime;

            BridgePointerClick();
            PlayHaptic(hand, _pokeHapticAmplitude, _pokeHapticDuration);
        }

        private void OnHandUntouch(Hand hand)
        {
            if (!_isHovered) return;
            ClearHover();
        }

        private void BridgePointerClick()
        {
            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }

        private void BridgePointerEnter()
        {
            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.pointerEnterHandler);
        }

        private void BridgePointerExit()
        {
            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.pointerExitHandler);
        }

        private void ClearHover()
        {
            _isHovered = false;
            BridgePointerExit();
        }

        private void PlayHaptic(Hand hand, float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;
            hand.PlayHapticVibration(duration, amplitude);
        }

        private void EnsureComponents()
        {
            _boxCollider = GetComponent<BoxCollider>();
            if (_boxCollider == null)
                _boxCollider = gameObject.AddComponent<BoxCollider>();
            _boxCollider.isTrigger = false;

            _touchEvent = GetComponent<HandTouchEvent>();
            if (_touchEvent == null)
                _touchEvent = gameObject.AddComponent<HandTouchEvent>();
            _touchEvent.oneHanded = false;
        }

        private void SyncColliderSize()
        {
            Rect r = _rect.rect;
            _lastSyncedSize = r.size;
            _boxCollider.size = new Vector3(r.width, r.height, _colliderDepth);
            _boxCollider.center = new Vector3(r.center.x, r.center.y, -_colliderForwardOffset);
        }
    }
}
