using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    /// <summary>Raises <see cref="Held"/> once when this UI element is pressed for <see cref="HoldSeconds"/>.</summary>
    public sealed class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private float pressedAt = -1f;

        public float HoldSeconds { get; set; } = 2f;

        public event Action Held;

        public void OnPointerDown(PointerEventData eventData) => pressedAt = Time.unscaledTime;

        public void OnPointerUp(PointerEventData eventData) => pressedAt = -1f;

        public void OnPointerExit(PointerEventData eventData) => pressedAt = -1f;

        private void Update()
        {
            if (pressedAt < 0f || Time.unscaledTime - pressedAt < HoldSeconds) return;
            pressedAt = -1f;
            Held?.Invoke();
        }
    }
}
