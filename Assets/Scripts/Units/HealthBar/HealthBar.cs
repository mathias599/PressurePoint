using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour en Vector3.
using UnityEngine.Serialization; // Geeft toegang tot FormerlySerializedAs, zodat een hernoemd veld zijn waarde houdt.
using UnityEngine.InputSystem; // Geeft toegang tot het nieuwe Input System (toetsenbord).
using UnityEngine.UI; // Geeft toegang tot UI-elementen zoals Slider.

// Staat op de sliders "Healthbar" (held) en "Healthbar Ghoul" (vijand) in de scene Checkpoint, onder de tweede Canvas.
// Zoekt een held of vijand, laat de slider boven die unit zweven en vult hem met zijn health.
public class HealthBar : MonoBehaviour
{
    [Header("Welke unit")]
    [Tooltip("Aan = deze balk volgt een vijand. Uit = deze balk volgt een held.")]
    public bool followEnemy;
    [Tooltip("Vaste unit om te volgen. Leeg laten = het script zoekt zelf een held of vijand.")]
    public BaseUnit target;

    [Header("Weergave")]
    [Tooltip("De slider die de health toont. Leeg laten = de slider op dit object.")]
    public Slider slider;
    [Tooltip("Hoe hoog de balk boven de unit zweeft, in tegels.")]
    public float heightAboveUnit = 0.7f;

    [Header("Test")]
    [Tooltip("Aan = met de testtoets doe je schade aan de gevolgde unit.")]
    [FormerlySerializedAs("damageWithH")] public bool enableTestDamage = true;
    [Tooltip("Toets om testschade te doen.")]
    [SerializeField] private Key testDamageKey = Key.W;
    [Tooltip("Hoeveel schade de testtoets doet.")]
    [SerializeField, Min(0)] private int testDamageAmount = 3;

    private RectTransform rect; // Voor het verplaatsen van de balk.
    private Canvas canvas; // De Canvas waar de balk op staat.
    private CanvasGroup canvasGroup; // Voor het (on)zichtbaar maken van de hele balk.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        if (slider == null) slider = GetComponent<Slider>(); // Geen slider gekoppeld? Dan die op dit object gebruiken.
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Met een CanvasGroup maken we de balk (on)zichtbaar zonder onderdelen uit te zetten die in de editor zijn ingesteld.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void LateUpdate() // Elke frame, nadat alle Update-methodes klaar zijn (dus nadat units bewogen hebben).
    {
        if (target == null) // Units worden pas gespawnd als het spel start: blijven zoeken tot er een is.
        {
            if (followEnemy) target = FindAnyObjectByType<BaseEnemy>();
            else target = FindAnyObjectByType<BaseHero>();
        }

        bool hasTarget = target != null;
        SetVisible(hasTarget); // Alleen zichtbaar als er een unit is.
        if (!hasTarget) return;

        slider.maxValue = target.maxHealth; // Health in de slider zetten.
        slider.value = target.currentHealth;

        // Positie: van de wereld (boven de unit) naar het scherm, en dan naar de Canvas.
        Vector3 worldPos = target.transform.position + Vector3.up * heightAboveUnit; // Punt boven de unit.
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos); // Dat punt op het scherm.
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera; // Overlay-Canvas heeft geen camera nodig.
        RectTransform parent = (RectTransform)rect.parent;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPos, uiCam, out Vector3 uiPos))
        {
            rect.position = uiPos; // Balk naar die plek verplaatsen.
        }

        if (enableTestDamage && Keyboard.current != null && Keyboard.current[testDamageKey].wasPressedThisFrame) // Testtoets ingedrukt?
        {
            target.TakeDamage(testDamageAmount);
        }
    }

    private void SetVisible(bool visible) // Maakt de balk doorzichtig in plaats van hem uit te zetten (anders stopt LateUpdate).
    {
        canvasGroup.alpha = visible ? 1f : 0f;
    }
}
