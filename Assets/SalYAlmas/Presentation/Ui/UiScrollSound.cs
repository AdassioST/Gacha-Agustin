using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SalYAlmas.Ui
{
    /// <summary>Papel por distancia recorrida con gesto o rueda; los saltos de navegación quedan mudos.</summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UiScrollSound : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        private ScrollRect _scroll;
        private Vector2 _last;
        private float _distance;
        private bool _userMotion, _dragging;

        private void Awake() => _scroll = GetComponent<ScrollRect>();
        private void OnEnable() => ResetMotion();
        private void OnDisable() => ResetMotion();

        public void ResetMotion()
        {
            _distance = 0f;
            _userMotion = _dragging = false;
            if (_scroll != null && _scroll.content != null) _last = _scroll.content.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) _userMotion = _dragging = true; }
        public void OnEndDrag(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) _dragging = false; }
        public void OnScroll(PointerEventData e) { if (e.scrollDelta.sqrMagnitude > 0f) _userMotion = true; }

        private void LateUpdate()
        {
            if (_scroll.content == null) return;
            var position = _scroll.content.anchoredPosition;
            float delta = Vector2.Distance(position, _last);
            _last = position;
            if (!_userMotion) return;
            _distance += delta;
            if (_distance >= 28f) { UiSfx.Scroll(); _distance = 0f; }
            if (!_dragging && delta < 0.1f && _scroll.velocity.sqrMagnitude < 1f) ResetMotion();
        }
    }
}
