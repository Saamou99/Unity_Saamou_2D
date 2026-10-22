using UnityEngine;
using UnityEngine.UI;

public class ScreenColorController : MonoBehaviour
{
    [Header("UI Elementer")]
    public Image backgroundImage;
    public Slider brightnessSlider;
    public CanvasGroup sliderCanvasGroup;

    [Header("De 3 Natte-venlige Farver")]
    public Color[] nightColors = new Color[3]
    {
        new Color(1.00f, 0.55f, 0.16f), // 1. Varm Rav / Stearinlys
        new Color(0.90f, 0.33f, 0.24f), // 2. Blød Solnedgangs-Koral
        new Color(1.00f, 0.71f, 0.37f)  // 3. Gylden Fersken / Varm Okker
    };

    [Header("Flow Animation Indstillinger")]
    [Tooltip("Hvor mange sekunder det tager at glide fra én farve til den næste")]
    public float transitionDuration = 20f;

    [Range(0.15f, 1f)]
    [Tooltip("Mindste lysstyrke så skærmen aldrig bliver helt slukket")]
    public float minBrightness = 0.5f;

    [Header("Slider Animation")]
    public float idleFadeTime = 2f; // Sekunder før slideren dæmpes automatisk

    private int currentColorIndex = 0;
    private int nextColorIndex = 1;
    private float transitionTimer = 0f;

    private float targetBrightness = 1f;
    private float smoothBrightness = 1f;
    private float lastInteractionTime;

    void Start()
    {
        if (brightnessSlider != null)
        {
            targetBrightness = brightnessSlider.value;
            smoothBrightness = targetBrightness;

            // Lytter efter når brugeren trækker i slideren
            brightnessSlider.onValueChanged.AddListener(OnSliderMoved);
        }

        lastInteractionTime = Time.time;
    }

    void OnSliderMoved(float value)
    {
        targetBrightness = value;
        lastInteractionTime = Time.time; // Vækker slider-animationen
    }

    void Update()
    {
        // Registrer også hvis man rører skærmen for at vække slideren
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            lastInteractionTime = Time.time;
        }

        AnimateColorFlow();
        AnimateSliderUI();
    }

    void AnimateColorFlow()
    {
        if (nightColors.Length < 2) return;

        // Tæl tiden op for overgangen mellem de 3 farver
        transitionTimer += Time.deltaTime / transitionDuration;

        // SmoothStep giver en moderne, organisk og blød start/slut på hvert farveskift
        float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionTimer);

        // Bland den nuværende farve med den næste farve
        Color baseColor = Color.Lerp(nightColors[currentColorIndex], nightColors[nextColorIndex], smoothProgress);

        // Gør selve lysstyrke-justeringen silkeblød, når man trækker i slideren
        smoothBrightness = Mathf.Lerp(smoothBrightness, targetBrightness, Time.deltaTime * 8f);
        float finalBrightness = Mathf.Lerp(minBrightness, 1f, smoothBrightness);

        // Påfør farve og lysstyrke på skærmen
        backgroundImage.color = baseColor * finalBrightness;
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);

        // Skift videre til næste farve i rækken (0 -> 1 -> 2 -> 0)
        if (transitionTimer >= 1f)
        {
            transitionTimer = 0f;
            currentColorIndex = nextColorIndex;
            nextColorIndex = (nextColorIndex + 1) % nightColors.Length;
        }
    }

    void AnimateSliderUI()
    {
        if (brightnessSlider == null) return;

        bool isActive = (Time.time - lastInteractionTime) < idleFadeTime;

        // 1. Fade-animation (tydelig ved brug, dæmpet i mørket ved inaktivitet)
        if (sliderCanvasGroup != null)
        {
            float targetAlpha = isActive ? 0.95f : 0.2f;
            sliderCanvasGroup.alpha = Mathf.Lerp(sliderCanvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
        }

        // 2. Blød størrelses-animation (vokser en smule når man bruger den)
        Vector3 targetScale = isActive ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
        brightnessSlider.transform.localScale = Vector3.Lerp(brightnessSlider.transform.localScale, targetScale, Time.deltaTime * 6f);
    }
}