using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Staat op het "Healthbar" Slider-object in TestScene (onder de tweede Canvas).
// Zoekt de hero op, laat de slider boven zijn hoofd zweven en vult hem met zijn health.
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public BaseUnit target;              // leeg laten: dan zoekt hij zelf de hero
    public float heightAboveUnit = 0.7f; // hoe hoog boven de unit (in world units, 1 tile = 1)

    [Header("Test")]
    public bool damageWithH = true;      // druk op H in Play mode voor 1 schade

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Met een CanvasGroup maken we de hele balk (on)zichtbaar,
        // zonder onderdelen aan/uit te zetten die jij zelf in de editor hebt ingesteld.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        // De hero wordt pas gespawnd als het spel start, dus we zoeken hem tot we hem hebben.
        if (target == null)
        {
            target = FindAnyObjectByType<BaseHero>();
        }

        bool hasTarget = target != null;
        SetVisible(hasTarget);
        if (!hasTarget) return;

        // Health -> slider
        slider.maxValue = target.maxHealth;
        slider.value = target.currentHealth;

        // Positie: van de wereld (boven de unit) naar het scherm, en dan naar de canvas
        Vector3 worldPos = target.transform.position + Vector3.up * heightAboveUnit;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransform parent = (RectTransform)rect.parent;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPos, uiCam, out Vector3 uiPos))
        {
            rect.position = uiPos;
        }

        // Testen: H = 1 schade
        if (damageWithH && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            target.TakeDamage(1);
        }
    }

    // Verberg de balk door hem doorzichtig te maken (niet uitzetten, anders stopt LateUpdate)
    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
    }
}
