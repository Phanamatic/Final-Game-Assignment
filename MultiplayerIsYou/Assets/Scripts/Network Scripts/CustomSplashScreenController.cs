using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomSplashScreenController : MonoBehaviour
{
    [Header("Images for Initial Animation")]
    public Image image1;
    public Image image2;

    [Header("Loading Dots")]
    public Image dot4;
    public Image dot5;
    public Image dot6;

    [Header("Loading Background")]
    public Image loadingBackground; // Optional: Background image for loading

    [Header("Loading Dots Sprites")]
    public Sprite[] loadingDotsSprites; // Assign sprites for dots 4, 5, 6

    [Header("Animation Timings")]
    public float fadeInTime = 2f;
    public float holdTime = 1f;
    public float fadeOutTime = 2f;
    public float loadingMinTime = 2f;
    public float loadingMaxTime = 6f;

    void Start()
    {
        // Ensure all images are initially inactive
        InitializeImages();

        StartCoroutine(PlaySplashSequence());
    }

    void InitializeImages()
    {
        if (image1 != null) image1.gameObject.SetActive(false);
        if (image2 != null) image2.gameObject.SetActive(false);
        if (dot4 != null) dot4.gameObject.SetActive(false);
        if (dot5 != null) dot5.gameObject.SetActive(false);
        if (dot6 != null) dot6.gameObject.SetActive(false);
        if (loadingBackground != null) loadingBackground.gameObject.SetActive(false);
    }

    IEnumerator PlaySplashSequence()
    {
        // Initial Fade-In and Zoom for Image1 and Image2
        if (image1 != null)
        {
            image1.gameObject.SetActive(true);
            yield return StartCoroutine(FadeAndZoomIn(image1, new Vector3(1.2f, 1.2f, 1f), fadeInTime));
        }

        if (image2 != null)
        {
            image2.gameObject.SetActive(true);
            yield return StartCoroutine(FadeAndZoomIn(image2, new Vector3(1.2f, 1.2f, 1f), fadeInTime));
        }

        // Hold for specified time
        yield return new WaitForSeconds(holdTime);

        // Fade-Out and Zoom-Out for Image1 and Image2
        if (image1 != null)
        {
            yield return StartCoroutine(FadeAndZoomOut(image1, new Vector3(0.8f, 0.8f, 1f), fadeOutTime));
            image1.gameObject.SetActive(false);
        }

        if (image2 != null)
        {
            yield return StartCoroutine(FadeAndZoomOut(image2, new Vector3(0.8f, 0.8f, 1f), fadeOutTime));
            image2.gameObject.SetActive(false);
        }

        // Start Loading Simulation
        yield return StartCoroutine(LoadingSequence());

        // Transition to Menu Scene
        SceneManager.LoadScene("menu_scene");
    }

    IEnumerator FadeAndZoomIn(Image img, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Color color = img.color;
        Vector3 initialScale = img.rectTransform.localScale;
        img.rectTransform.localScale = Vector3.zero;
        color.a = 0f;
        img.color = color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            img.rectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            color.a = Mathf.Lerp(0f, 1f, t);
            img.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        img.rectTransform.localScale = targetScale;
        color.a = 1f;
        img.color = color;
    }

    IEnumerator FadeAndZoomOut(Image img, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Color color = img.color;
        Vector3 initialScale = img.rectTransform.localScale;
        img.rectTransform.localScale = initialScale;
        color.a = 1f;
        img.color = color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            img.rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            color.a = Mathf.Lerp(1f, 0f, t);
            img.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        img.rectTransform.localScale = targetScale;
        color.a = 0f;
        img.color = color;
    }

    IEnumerator LoadingSequence()
    {
        // Optionally, activate Loading Background
        if (loadingBackground != null)
        {
            loadingBackground.gameObject.SetActive(true);
            yield return StartCoroutine(FadeIn(loadingBackground, 1f));
        }

        // Activate and Animate Loading Dots
        StartCoroutine(AnimateLoadingDots());

        // Wait for random loading time between 2 and 6 seconds
        float randomLoadTime = Random.Range(loadingMinTime, loadingMaxTime);
        yield return new WaitForSeconds(randomLoadTime);

        // Deactivate Loading Dots and Background
        if (dot4 != null) dot4.gameObject.SetActive(false);
        if (dot5 != null) dot5.gameObject.SetActive(false);
        if (dot6 != null) dot6.gameObject.SetActive(false);
        if (loadingBackground != null)
        {
            yield return StartCoroutine(FadeOut(loadingBackground, 1f));
            loadingBackground.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeIn(Image img, float duration)
    {
        float elapsed = 0f;
        Color color = img.color;
        color.a = 0f;
        img.color = color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            color.a = Mathf.Lerp(0f, 1f, t);
            img.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        color.a = 1f;
        img.color = color;
    }

    IEnumerator FadeOut(Image img, float duration)
    {
        float elapsed = 0f;
        Color color = img.color;
        color.a = 1f;
        img.color = color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            color.a = Mathf.Lerp(1f, 0f, t);
            img.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        color.a = 0f;
        img.color = color;
    }

    IEnumerator AnimateLoadingDots()
    {
        // Ensure loadingDotsSprites is not empty
        if (loadingDotsSprites == null || loadingDotsSprites.Length == 0)
        {
            Debug.LogError("LoadingDotsSprites array is empty or not assigned!");
            yield break; // Exit the coroutine to prevent errors
        }

        // Activate Dots
        if (dot4 != null) dot4.gameObject.SetActive(true);
        if (dot5 != null) dot5.gameObject.SetActive(true);
        if (dot6 != null) dot6.gameObject.SetActive(true);

        while (true)
        {
            // Animate Dot4
            if (dot4 != null && loadingDotsSprites.Length > 0)
            {
                int index = Random.Range(0, loadingDotsSprites.Length);
                dot4.sprite = loadingDotsSprites[index];
            }
            yield return new WaitForSeconds(0.5f);

            // Animate Dot5
            if (dot5 != null && loadingDotsSprites.Length > 0)
            {
                int index = Random.Range(0, loadingDotsSprites.Length);
                dot5.sprite = loadingDotsSprites[index];
            }
            yield return new WaitForSeconds(0.5f);

            // Animate Dot6
            if (dot6 != null && loadingDotsSprites.Length > 0)
            {
                int index = Random.Range(0, loadingDotsSprites.Length);
                dot6.sprite = loadingDotsSprites[index];
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
