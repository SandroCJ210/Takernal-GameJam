#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionUIBuilder
{
    [MenuItem("Takernal/UI/Generar UI de Selección de Ingredientes")]
    public static void GenerateSelectionUI()
    {
        // 1. Encontrar o crear Canvas en la escena activa
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Si existe un EventSystem, bien; si no, crearlo
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. Destruir panel viejo si existía para reconstruir limpio
        Transform oldPanel = canvas.transform.Find("IngredientSelectionPanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // 3. Crear el Panel Principal de Fondo (Fullscreen Dark Overlay)
        GameObject panelObj = new GameObject("IngredientSelectionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(IngredientSelectionUI));
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.08f, 0.85f); // Fondo oscuro semitransparente

        IngredientSelectionUI selectionUI = panelObj.GetComponent<IngredientSelectionUI>();

        // 4. Crear Título Prominente
        GameObject titleObj = new GameObject("TitleLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -60);
        titleRect.sizeDelta = new Vector2(800, 60);

        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "¡ELIGE UN INGREDIENTE!";
        titleTMP.fontSize = 36;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.82f, 0.2f); // Dorado/Amarillo

        // 5. Crear Contenedor de Tarjetas (CardsContainer)
        GameObject containerObj = new GameObject("CardsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        containerObj.transform.SetParent(panelObj.transform, false);

        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, -20);
        containerRect.sizeDelta = new Vector2(900, 420);

        HorizontalLayoutGroup hlg = containerObj.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // 6. Crear las 3 Tarjetas
        IngredientCardUI[] cards = new IngredientCardUI[3];

        for (int i = 0; i < 3; i++)
        {
            cards[i] = CreateCard(containerObj.transform, i);
        }

        // 7. Conectar Referencias en IngredientSelectionUI
        SerializedObject uiSO = new SerializedObject(selectionUI);
        uiSO.FindProperty("selectionPanel").objectReferenceValue = panelObj;
        
        SerializedProperty cardsProp = uiSO.FindProperty("cards");
        cardsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
        }
        uiSO.ApplyModifiedProperties();

        // Desactivar el panel listo para el gameplay
        panelObj.SetActive(false);

        Undo.RegisterCreatedObjectUndo(panelObj, "Generar UI de Selección de Ingredientes");
        EditorUtility.SetDirty(panelObj);

        Debug.Log("[SelectionUIBuilder] ✅ ¡UI de Selección Elegante generada y conectada con éxito en el Canvas!");
    }

    private static IngredientCardUI CreateCard(Transform parent, int index)
    {
        // Fondo de la tarjeta (Botón interactivo completo)
        GameObject cardObj = new GameObject($"IngredientCard_{index}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(IngredientCardUI));
        cardObj.transform.SetParent(parent, false);

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(260, 380);

        Image cardBg = cardObj.GetComponent<Image>();
        cardBg.color = new Color(0.12f, 0.14f, 0.18f, 1f); // Gris oscuro elegante

        Button btn = cardObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        // Borde de rareza
        GameObject borderObj = new GameObject("RarityOutlineImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        borderObj.transform.SetParent(cardObj.transform, false);
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        Image borderImg = borderObj.GetComponent<Image>();
        borderImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);
        borderImg.type = Image.Type.Sliced;
        // Poner el borde por detrás para que no tape interacción
        borderObj.transform.SetAsFirstSibling();

        // Marco contenedor interno con Vertical Layout Group
        GameObject innerContent = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        innerContent.transform.SetParent(cardObj.transform, false);
        RectTransform innerRect = innerContent.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(12, 12);
        innerRect.offsetMax = new Vector2(-12, -12);

        VerticalLayoutGroup vlg = innerContent.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // 1. Icono del Ingrediente
        GameObject iconObj = new GameObject("IconImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(innerContent.transform, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(90, 90);
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.preserveAspect = true;

        // 2. Nombre del Ingrediente
        GameObject nameObj = new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(innerContent.transform, false);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(236, 32);
        TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
        nameTMP.text = "Nombre Ingrediente";
        nameTMP.fontSize = 20;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;

        // 3. Texto de Sabor / Ambientación
        GameObject flavorObj = new GameObject("FlavorLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        flavorObj.transform.SetParent(innerContent.transform, false);
        RectTransform flavorRect = flavorObj.GetComponent<RectTransform>();
        flavorRect.sizeDelta = new Vector2(236, 40);
        TextMeshProUGUI flavorTMP = flavorObj.GetComponent<TextMeshProUGUI>();
        flavorTMP.text = "\"Descripción corta del ingrediente\"";
        flavorTMP.fontSize = 12;
        flavorTMP.fontStyle = FontStyles.Italic;
        flavorTMP.alignment = TextAlignmentOptions.Center;
        flavorTMP.color = new Color(0.75f, 0.75f, 0.8f);

        // 4. Stats Modificados
        GameObject statsObj = new GameObject("StatsLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(innerContent.transform, false);
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.sizeDelta = new Vector2(236, 65);
        TextMeshProUGUI statsTMP = statsObj.GetComponent<TextMeshProUGUI>();
        statsTMP.text = "+ Stats";
        statsTMP.fontSize = 15;
        statsTMP.fontStyle = FontStyles.Bold;
        statsTMP.alignment = TextAlignmentOptions.Center;

        // 5. Habilidad
        GameObject abilityObj = new GameObject("AbilityLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        abilityObj.transform.SetParent(innerContent.transform, false);
        RectTransform abilityRect = abilityObj.GetComponent<RectTransform>();
        abilityRect.sizeDelta = new Vector2(236, 50);
        TextMeshProUGUI abilityTMP = abilityObj.GetComponent<TextMeshProUGUI>();
        abilityTMP.text = "Habilidad: Específica";
        abilityTMP.fontSize = 13;
        abilityTMP.alignment = TextAlignmentOptions.Center;
        abilityTMP.color = new Color(0.4f, 0.9f, 0.5f);

        // Conectar referencias en el script IngredientCardUI
        IngredientCardUI cardScript = cardObj.GetComponent<IngredientCardUI>();
        SerializedObject cardSO = new SerializedObject(cardScript);
        cardSO.FindProperty("iconImage").objectReferenceValue = iconImg;
        cardSO.FindProperty("nameText").objectReferenceValue = nameTMP;
        cardSO.FindProperty("flavorText").objectReferenceValue = flavorTMP;
        cardSO.FindProperty("statsText").objectReferenceValue = statsTMP;
        cardSO.FindProperty("abilityText").objectReferenceValue = abilityTMP;
        cardSO.FindProperty("rarityBorder").objectReferenceValue = borderImg;
        cardSO.FindProperty("cardButton").objectReferenceValue = btn;
        cardSO.ApplyModifiedProperties();

        return cardScript;
    }
}
#endif
