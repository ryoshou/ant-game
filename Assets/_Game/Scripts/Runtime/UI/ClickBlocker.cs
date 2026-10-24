using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    /// <summary>Swallows clicks so they don't bubble to a parent's "tap outside to close" button.</summary>
    public sealed class ClickBlocker : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) { }
    }
}
