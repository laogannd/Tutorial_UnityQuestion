using UnityEngine;

namespace VRQuestion
{
    // 让面板朝向VR头显（与VRNotificationPanel思路一致）
    // 可选项：放在面板上即生效，不影响AutoHand交互
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField] private bool _lockYAxis = false;
        [SerializeField, Min(0f)] private float _smoothSpeed = 0f;

        private Transform _cameraTransform;

        private void Awake()
        {
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            Vector3 forward = _cameraTransform.rotation * Vector3.forward;
            Vector3 up = _cameraTransform.rotation * Vector3.up;
            if (_lockYAxis)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f) return;
                up = Vector3.up;
            }

            Quaternion target = Quaternion.LookRotation(forward, up);
            if (_smoothSpeed <= 0f) transform.rotation = target;
            else transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _smoothSpeed);
        }
    }
}
