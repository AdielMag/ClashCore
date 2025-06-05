using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace UnityEngine.InputSystem.OnScreen
{
    /// <summary>
    /// Mobile‑style “floating” joystick consisting of two UI Images:
    /// a *background* (range ring) and a *handle* (thumb).
    /// 
    /// ▸ The background snaps to the first press inside <see cref="dynamicOriginRange"/> (dynamic origin).
    /// ▸ When the pointer moves beyond <see cref="movementRange"/>, the entire joystick drags along,
    ///   so the handle never leaves its circular boundary.
    /// ▸ Sends a normalised <c>Vector2</c> value to <see cref="controlPathInternal"/>, just like
    ///   <c>OnScreenStick</c> – perfect drop‑in for existing Input Actions.
    /// </summary>
    [AddComponentMenu("Input/Dynamic On‑Screen Joystick")]
    public class DynamicOnScreenJoystick : OnScreenControl,
                                           IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        /* ────────────────────────────── CONFIG ───────────────────────────── */
        [Header("Joystick Graphics")]
        [Tooltip("Background Image that shows the movement range circle.")]
        [SerializeField] private RectTransform m_Background;

        [Tooltip("Handle Image that represents the thumb / knob.")]
        [SerializeField] private RectTransform m_Handle;

        [Header("Behaviour")]
        [Tooltip("Radius (in px) the handle can move from the background centre before clamping & dragging.")]
        [Min(1)]
        [SerializeField] private float m_MovementRange = 75f;

        [Tooltip("Tap acceptance radius (in px) around the prefab’s original position.")]
        [Min(0)]
        [SerializeField] private float m_DynamicOriginRange = 150f;

        [InputControl(layout = "Vector2")]
        [Tooltip("Control path to receive the stick Vector2, e.g. <Gamepad>/leftStick.")]
        [SerializeField] private string m_ControlPath = "<Gamepad>/leftStick";

        /* ───────────────────────────── STATE ─────────────────────────────── */
        private Vector2 m_Origin;           // current centre of background (updates when dragged)
        private Vector2 m_InitialPos;       // prefab position in scene (designer‑time)
        private RectTransform m_CanvasRect; // cached parent canvas rect

        /* ──────────────────────── MONOBEHAVIOUR ─────────────────────────── */
        private void Awake()
        {
            if (m_Background == null)
                m_Background = (RectTransform)transform; // fallback
            if (m_Handle == null)
                m_Handle = m_Background.GetChild(0) as RectTransform; // assume first child

            m_CanvasRect = m_Background.parent.GetComponentInParent<RectTransform>();
        }

        private void Start()
        {
            m_InitialPos = m_Background.anchoredPosition;
            m_Origin = m_InitialPos;
            ResetHandle();
        }

        /* ───────────────────────────── INPUT ────────────────────────────── */
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!ScreenToLocal(eventData, out var localPos))
                return;

            // Accept press only inside dynamic origin range (optional safety / multi‑touch).
            if ((localPos - m_InitialPos).sqrMagnitude > m_DynamicOriginRange * m_DynamicOriginRange)
                return;

            m_Origin = localPos;
            m_Background.anchoredPosition = localPos; // jump under finger
            UpdateJoystick(localPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!ScreenToLocal(eventData, out var localPos))
                return;

            UpdateJoystick(localPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset visuals & send zero.
            m_Origin = m_InitialPos;
            m_Background.anchoredPosition = m_InitialPos;
            ResetHandle();
            SendValueToControl(Vector2.zero);
        }

        /* ──────────────────────────── LOGIC ─────────────────────────────── */
        private void UpdateJoystick(Vector2 current)
        {
            var delta = current - m_Origin;
            var mag   = delta.magnitude;

            // Drag whole joystick if finger leaves ring.
            if (mag > m_MovementRange)
            {
                var overshoot = delta - delta.normalized * m_MovementRange;
                m_Origin     += overshoot;               // shift origin
                m_Background.anchoredPosition = m_Origin;
                delta        = delta.normalized * m_MovementRange; // clamp handle
            }

            m_Handle.anchoredPosition = delta;          // handle moves inside background
            SendValueToControl(delta / m_MovementRange);
        }

        private void ResetHandle() => m_Handle.anchoredPosition = Vector2.zero;

        /* ─────────────────────────── HELPERS ────────────────────────────── */
        private bool ScreenToLocal(PointerEventData eventData, out Vector2 localPos)
        {
            localPos = default;
            if (m_CanvasRect == null)
                return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_CanvasRect, eventData.position, eventData.pressEventCamera, out localPos);
        }

        /* ─────────────────────── OnScreenControl API ─────────────────────── */
        protected override string controlPathInternal { get => m_ControlPath; set => m_ControlPath = value; }

        /* ─────────────────────────── GIZMOS (Editor) ─────────────────────── */
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var bg = m_Background ?? (RectTransform)transform;
            var parent = bg.parent as RectTransform;
            if (parent == null) return;

            Gizmos.matrix = parent.localToWorldMatrix;
            var centre    = Application.isPlaying ? m_Origin : bg.anchoredPosition;

            DrawCircle(centre, m_MovementRange, new Color32(84, 173, 219, 160));               // movement range
            DrawCircle(m_InitialPos, m_DynamicOriginRange, new Color32(158, 84, 219, 100));     // origin range
        }

        private static void DrawCircle(Vector2 centre, float radius, Color col)
        {
            Gizmos.color = col;
            const int SEG = 32;
            Vector3 prev = centre + Vector2.right * radius;
            for (int i = 1; i <= SEG; ++i)
            {
                float ang = i / (float)SEG * Mathf.PI * 2f;
                Vector3 next = centre + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
                Gizmos.DrawLine(prev, next); prev = next;
            }
        }
#endif
    }
}
