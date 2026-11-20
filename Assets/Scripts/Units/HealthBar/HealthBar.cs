using UnityEngine; // Geeft toegang tot Unity-functionaliteiten zoals MonoBehaviour, Transform en Vector3.
using UnityEngine.InputSystem; // Geeft toegang tot het nieuwe Input System om bijvoorbeeld toetsenbordinput te detecteren.
using UnityEngine.UI; // Geeft toegang tot UI-elementen zoals Slider.


// Staat op de Slider-objecten "Healthbar" en "Healthbar Ghoul" in TestScene (onder de tweede Canvas).
// Zoekt een held of vijand, laat de slider boven die unit zweven en vult hem met zijn health.
public class HealthBar : MonoBehaviour // Maakt de klasse HealthBar aan. MonoBehaviour zorgt ervoor dat het script op een GameObject kan staan.
{
    public Slider slider; // Verwijst naar de Slider die de levenspunten visueel weergeeft.
    public BaseUnit target; // Bewaart welke unit gevolgd wordt. Als deze leeg is, zoekt het script zelf een unit.
    public bool followEnemy; // Bepaalt of de healthbar een vijand volgt. True = vijand, false = held.
    public float heightAboveUnit = 0.7f; // Bepaalt hoeveel hoger de healthbar boven de unit staat, in world units (1 tegel = 1).

    [Header("Test")] // Toont een aparte titel 'Test' in de Unity Inspector.
    public bool damageWithH = true; // Bepaalt of de H-toets tijdens het testen 1 schade doet aan de gevolgde unit.

    private RectTransform rect; // Bewaart de RectTransform van de healthbar. Hiermee kunnen we de positie van de UI aanpassen.
    private Canvas canvas; // Bewaart de Canvas waarop de healthbar staat.
    private CanvasGroup canvasGroup; // Bewaart de CanvasGroup waarmee we de zichtbaarheid van de healthbar regelen.

    private void Awake() // Wordt automatisch uitgevoerd wanneer het GameObject wordt geladen.
    {
        if (slider == null) slider = GetComponent<Slider>(); // Controleert of er nog geen Slider gekoppeld is. Zo niet, zoekt hij de Slider op hetzelfde GameObject.
        rect = GetComponent<RectTransform>(); // Haalt de RectTransform van dit GameObject op om de positie te kunnen aanpassen.
        canvas = GetComponentInParent<Canvas>(); // Zoekt de Canvas waartoe dit GameObject behoort.

        // Met een CanvasGroup maken we de hele balk (on)zichtbaar,
        // zonder onderdelen aan of uit te zetten die in de editor zijn ingesteld.
        canvasGroup = GetComponent<CanvasGroup>(); // Controleert of er al een CanvasGroup op dit GameObject staat.
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>(); // Als er nog geen CanvasGroup bestaat, voegt Unity er automatisch een toe.
    }

    private void LateUpdate() // Wordt elke frame uitgevoerd nadat alle normale Update-methodes zijn uitgevoerd.
    {
        // Units worden pas gespawnd als het spel start, dus we blijven zoeken tot we er een hebben.
        if (target == null) // Controleert of er nog geen unit gekoppeld is aan target.
        {
            if (followEnemy) target = FindAnyObjectByType<BaseEnemy>(); // Als followEnemy true is, zoekt Unity naar een object met de component BaseEnemy.
            else target = FindAnyObjectByType<BaseHero>(); // Anders zoekt Unity naar een object met de component BaseHero.
        }

        bool hasTarget = target != null; // Controleert of er een unit gevonden is en bewaart het resultaat als true of false.
        SetVisible(hasTarget); // Maakt de healthbar zichtbaar als er een unit gevonden is en onzichtbaar als dat niet zo is.
        if (!hasTarget) return; // Als er geen unit gevonden is, stopt de methode onmiddellijk. Het uitroepteken betekent NOT.

        // Health -> slider
        slider.maxValue = target.maxHealth; // Stelt de maximale waarde van de Slider gelijk aan de maximale levenspunten van de unit.
        slider.value = target.currentHealth; // Stelt de huidige waarde van de Slider gelijk aan de huidige levenspunten van de unit.

        // Positie: van de wereld (boven de unit) naar het scherm, en dan naar de canvas
        Vector3 worldPos = target.transform.position + Vector3.up * heightAboveUnit; // Berekent een positie boven de unit in de spelwereld. Vector3.up betekent omhoog op de Y-as.
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos); // Zet de wereldpositie om naar een positie op het scherm met behulp van de hoofdcamera.
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera; // Controleert het type Canvas. Bij ScreenSpaceOverlay is geen UI-camera nodig, anders wordt de camera van de Canvas gebruikt.
        RectTransform parent = (RectTransform)rect.parent; // Haalt de RectTransform van het bovenliggende GameObject op.
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPos, uiCam, out Vector3 uiPos)) // Zet de schermpositie om naar een wereldpositie op de Canvas en bewaart die in uiPos.
        {
            rect.position = uiPos; // Verplaatst de healthbar naar de berekende positie boven de unit.
        }

        // Testen: H = 1 schade
        if (damageWithH && Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame) // Controleert of testen ingeschakeld zijn, het toetsenbord beschikbaar is en de H-toets deze frame ingedrukt werd.
        {
            target.TakeDamage(3); // Roept TakeDamage aan en brengt 1 schade toe aan de unit.
        }
    }

    // Verberg de balk door hem doorzichtig te maken (niet uitzetten, anders stopt LateUpdate)
    private void SetVisible(bool visible) // Methode die de healthbar zichtbaar of onzichtbaar maakt. visible is true of false.
    {
        canvasGroup.alpha = visible ? 1f : 0f; // Zet de doorzichtigheid op 1 (volledig zichtbaar) of 0 (volledig onzichtbaar).
    }
}