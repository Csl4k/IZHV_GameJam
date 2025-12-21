using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISfxWiring : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource uiSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;
    [Range(0f, 1f)] public float hoverVolume = 1f;
    [Range(0f, 1f)] public float clickVolume = 1f;

    [Header("Scope")]
    public Transform root;           // If null, uses this transform
    public bool includeInactive = true;

    void Awake()
    {
        if (!root) root = transform;
        if (!uiSource) uiSource = GetComponent<AudioSource>();

        WireAllButtons();
    }

    [ContextMenu("Wire All Buttons Now")]
    public void WireAllButtons()
    {
        if (!root) root = transform;

        var buttons = root.GetComponentsInChildren<Button>(includeInactive);
        foreach (var btn in buttons)
        {
            if (!btn) continue;

            // Click sound
            btn.onClick.AddListener(PlayClick);

            // Hover sound (PointerEnter)
            AddPointerEnter(btn.gameObject);
        }
    }

    void AddPointerEnter(GameObject go)
    {
        var trigger = go.GetComponent<EventTrigger>();
        if (!trigger) trigger = go.AddComponent<EventTrigger>();
        if (trigger.triggers == null) trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

        foreach (var e in trigger.triggers)
            if (e.eventID == EventTriggerType.PointerEnter)
                return;

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => PlayHover());
        trigger.triggers.Add(entry);
    }

    public void PlayHover()
    {
        if (!uiSource || !hoverClip) return;
        uiSource.PlayOneShot(hoverClip, hoverVolume);
    }

    public void PlayClick()
    {
        if (!uiSource || !clickClip) return;
        uiSource.PlayOneShot(clickClip, clickVolume);
    }
}
