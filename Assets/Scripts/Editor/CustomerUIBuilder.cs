#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUIBuilder
{
    [MenuItem("Takernal/UI/Generar UI de Clientes (HUD & Prefabs)")]
    public static void GenerateCustomerUI()
    {
        // 1. Crear directorio de Prefabs si no existe
        string prefabsDir = "Assets/Prefabs/UI";
        if (!Directory.Exists(prefabsDir))
        {
            Directory.CreateDirectory(prefabsDir);
            AssetDatabase.Refresh();
        }

        // 2. Construir Prefab de CustomerCard en memoria
        GameObject cardObj = new GameObject("CustomerCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(CustomerCardUI));
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(220, 110);
        cardRect.pivot = new Vector2(0, 1);

        Image cardBg = cardObj.GetComponent<Image>();
        cardBg.color = new Color(0.12f, 0.15f, 0.2f, 0.92f);

        CanvasGroup cardGroup = cardObj.GetComponent<CanvasGroup>();
        CustomerCardUI cardUI = cardObj.GetComponent<CustomerCardUI>();

        // ActiveContainer
        GameObject activeContainer = new GameObject("ActiveContainer", typeof(RectTransform));
        activeContainer.transform.SetParent(cardObj.transform, false);
        RectTransform activeRect = activeContainer.GetComponent<RectTransform>();
        activeRect.anchorMin = Vector2.zero;
        activeRect.anchorMax = Vector2.one;
        activeRect.sizeDelta = Vector2.zero;

        // Avatar Image
        GameObject avatarObj = new GameObject("AvatarImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        avatarObj.transform.SetParent(activeContainer.transform, false);
        RectTransform avatarRect = avatarObj.GetComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0, 1);
        avatarRect.anchorMax = new Vector2(0, 1);
        avatarRect.pivot = new Vector2(0.5f, 0.5f);
        avatarRect.anchoredPosition = new Vector2(30, -32);
        avatarRect.sizeDelta = new Vector2(44, 44);
        Image avatarImg = avatarObj.GetComponent<Image>();
        avatarImg.preserveAspect = true;

        // Name Text
        GameObject nameObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(activeContainer.transform, false);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(60, -16);
        nameRect.sizeDelta = new Vector2(-70, 22);
        TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 14;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;

        // Likes Text
        GameObject likesObj = new GameObject("LikesText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        likesObj.transform.SetParent(activeContainer.transform, false);
        RectTransform likesRect = likesObj.GetComponent<RectTransform>();
        likesRect.anchorMin = new Vector2(0, 1);
        likesRect.anchorMax = new Vector2(1, 1);
        likesRect.pivot = new Vector2(0, 1);
        likesRect.anchoredPosition = new Vector2(60, -38);
        likesRect.sizeDelta = new Vector2(-70, 18);
        TextMeshProUGUI likesText = likesObj.GetComponent<TextMeshProUGUI>();
        likesText.fontSize = 11;
        likesText.color = new Color(0.8f, 0.95f, 0.8f);

        // Dislikes Text
        GameObject dislikesObj = new GameObject("DislikesText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        dislikesObj.transform.SetParent(activeContainer.transform, false);
        RectTransform dislikesRect = dislikesObj.GetComponent<RectTransform>();
        dislikesRect.anchorMin = new Vector2(0, 1);
        dislikesRect.anchorMax = new Vector2(1, 1);
        dislikesRect.pivot = new Vector2(0, 1);
        dislikesRect.anchoredPosition = new Vector2(60, -56);
        dislikesRect.sizeDelta = new Vector2(-70, 18);
        TextMeshProUGUI dislikesText = dislikesObj.GetComponent<TextMeshProUGUI>();
        dislikesText.fontSize = 11;
        dislikesText.color = new Color(0.95f, 0.75f, 0.75f);

        // Patience Slider
        GameObject sliderObj = new GameObject("PatienceSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(activeContainer.transform, false);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0, 10);
        sliderRect.sizeDelta = new Vector2(-20, 12);
        Slider slider = sliderObj.GetComponent<Slider>();
        slider.interactable = false;

        // Slider Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.08f, 0.8f);

        // Slider Fill Area & Fill
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.3f);

        slider.fillRect = fillRect;

        // Conectar referencias en CustomerCardUI
        SerializedObject serializedCard = new SerializedObject(cardUI);
        serializedCard.FindProperty("avatarImage").objectReferenceValue = avatarImg;
        serializedCard.FindProperty("nameText").objectReferenceValue = nameText;
        serializedCard.FindProperty("likesText").objectReferenceValue = likesText;
        serializedCard.FindProperty("dislikesText").objectReferenceValue = dislikesText;
        serializedCard.FindProperty("patienceSlider").objectReferenceValue = slider;
        serializedCard.FindProperty("patienceFillImage").objectReferenceValue = fillImg;
        serializedCard.FindProperty("canvasGroup").objectReferenceValue = cardGroup;
        serializedCard.FindProperty("activeContainer").objectReferenceValue = activeContainer;
        serializedCard.ApplyModifiedProperties();

        // Guardar Prefab CustomerCard.prefab
        string cardPrefabPath = $"{prefabsDir}/CustomerCard.prefab";
        GameObject cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardObj, cardPrefabPath);
        Object.DestroyImmediate(cardObj);

        // 3. Crear HUD Panel en Escena y Prefab
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform oldHud = canvas.transform.Find("CustomersHUDPanel");
        if (oldHud != null)
        {
            Object.DestroyImmediate(oldHud.gameObject);
        }

        GameObject hudObj = new GameObject("CustomersHUDPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(CustomersHUDUI));
        hudObj.transform.SetParent(canvas.transform, false);

        RectTransform hudRect = hudObj.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(20, -20);
        hudRect.sizeDelta = new Vector2(750, 120);

        HorizontalLayoutGroup layout = hudObj.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 15f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        CustomersHUDUI hudUI = hudObj.GetComponent<CustomersHUDUI>();
        CustomerCardUI cardPrefabUI = cardPrefab != null ? cardPrefab.GetComponent<CustomerCardUI>() : null;

        SerializedObject serializedHud = new SerializedObject(hudUI);
        serializedHud.FindProperty("cardsContainer").objectReferenceValue = hudRect;
        serializedHud.FindProperty("cardPrefab").objectReferenceValue = cardPrefabUI;
        serializedHud.ApplyModifiedProperties();

        // Guardar Prefab CustomersHUDPanel.prefab
        string hudPrefabPath = $"{prefabsDir}/CustomersHUDPanel.prefab";
        PrefabUtility.SaveAsPrefabAsset(hudObj, hudPrefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[Takernal UI Builder] ¡UI de Clientes generada y guardada exitosamente en Assets/Prefabs/UI/!</color>");
    }
}
#endif
