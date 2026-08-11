using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimationLoop : MonoBehaviour
{
    [Header("Frames & Speed")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float pauseBetweenLoops = 0.5f;

    private Image image;
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isPausing = false;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        currentFrameIndex = 0;
        timer = 0f;
        isPausing = false;
        UpdateSprite();
    }

    private void Update()
    {
        if (frames == null || frames.Length <= 1) return;

        timer += Time.unscaledDeltaTime;

        if (isPausing)
        {
            if (timer >= pauseBetweenLoops)
            {
                isPausing = false;
                timer = 0f;
                currentFrameIndex = 0;
                UpdateSprite();
            }
            return;
        }

        float timePerFrame = 1f / Mathf.Max(0.01f, frameRate);
        if (timer >= timePerFrame)
        {
            timer -= timePerFrame;
            currentFrameIndex++;

            if (currentFrameIndex >= frames.Length)
            {
                if (loop)
                {
                    if (pauseBetweenLoops > 0f)
                    {
                        isPausing = true;
                        timer = 0f;
                    }
                    else
                    {
                        currentFrameIndex = 0;
                        UpdateSprite();
                    }
                }
                else
                {
                    currentFrameIndex = frames.Length - 1;
                }
            }
            else
            {
                UpdateSprite();
            }
        }
    }

    private void UpdateSprite()
    {
        if (image != null && frames != null && frames.Length > 0 && currentFrameIndex < frames.Length)
        {
            if (frames[currentFrameIndex] != null)
            {
                image.sprite = frames[currentFrameIndex];
            }
        }
    }

    public void SetFrames(Sprite[] newFrames, float newFrameRate = 8f)
    {
        frames = newFrames;
        frameRate = newFrameRate;
        currentFrameIndex = 0;
        timer = 0f;
        isPausing = false;
        UpdateSprite();
    }
}
