#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUIBuilder
{
    [MenuItem("Takernal/UI/Generar UI de Entrega de Platillos (DeliveryPanel)")]
    public static void GenerateDeliveryUI()
    {
        string prefabsDir = "Assets/Prefabs/UI";
        if (!Directory.Exists(prefabsDir))
        {
            Directory.CreateDirectory(prefabsDir);
            AssetDatabase.Refresh();
        }

        // -------------------------------------------------------------
        // 1. Crear Prefab de DeliveryDishItem
        // -------------------------------------------------------------
        GameObject dishObj = new GameObject("DeliveryDishItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(DeliveryDishItemUI), typeof(DraggableDishUI));
        RectTransform dishRect = dishObj.GetComponent<RectTransform>();
        dishRect.sizeDelta = new Vector2(280, 60);

        Image dishBg = dishObj.GetComponent<Image>();
        dishBg.color = new Color(0.16f, 0.20f, 0.26f, 0.95f);

        Button dishBtn = dishObj.GetComponent<Button>();

        // Border de Selección
        GameObject borderObj = new GameObject("SelectionBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        borderObj.transform.SetParent(dishObj.transform, false);
        RectTransform bRect = borderObj.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.sizeDelta = Vector2.zero;
        Image bImg = borderObj.GetComponent<Image>();
        bImg.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        borderObj.SetActive(false);

        // Icon Image
        GameObject iconObj = new GameObject("IconImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(dishObj.transform, false);
        RectTransform iRect = iconObj.GetComponent<RectTransform>();
        iRect.anchorMin = new Vector2(0.02f, 0.1f);
        iRect.anchorMax = new Vector2(0.18f, 0.9f);
        iRect.sizeDelta = Vector2.zero;
        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.preserveAspect = true;

        // Name Text
        GameObject nameObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(dishObj.transform, false);
        RectTransform nRect = nameObj.GetComponent<RectTransform>();
        nRect.anchorMin = new Vector2(0.22f, 0.5f);
        nRect.anchorMax = new Vector2(0.98f, 0.95f);
        nRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 15;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;

        // Tags Text
        GameObject tagsObj = new GameObject("TagsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        tagsObj.transform.SetParent(dishObj.transform, false);
        RectTransform tRect = tagsObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.22f, 0.05f);
        tRect.anchorMax = new Vector2(0.98f, 0.5f);
        tRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI tagsText = tagsObj.GetComponent<TextMeshProUGUI>();
        tagsText.fontSize = 12;
        tagsText.color = new Color(0.75f, 0.8f, 0.9f);

        DeliveryDishItemUI dishUI = dishObj.GetComponent<DeliveryDishItemUI>();
        SerializedObject serDish = new SerializedObject(dishUI);
        serDish.FindProperty("iconImage").objectReferenceValue = iconImg;
        serDish.FindProperty("nameText").objectReferenceValue = nameText;
        serDish.FindProperty("tagsText").objectReferenceValue = tagsText;
        serDish.FindProperty("selectionBorder").objectReferenceValue = bImg;
        serDish.FindProperty("selectButton").objectReferenceValue = dishBtn;
        serDish.ApplyModifiedProperties();

        string dishPrefabPath = $"{prefabsDir}/DeliveryDishItem.prefab";
        GameObject dishPrefab = PrefabUtility.SaveAsPrefabAsset(dishObj, dishPrefabPath);
        Object.DestroyImmediate(dishObj);

        // -------------------------------------------------------------
        // 2. Crear Prefab de DeliveryCustomerItem
        // -------------------------------------------------------------
        GameObject custObj = new GameObject("DeliveryCustomerItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(DeliveryCustomerItemUI), typeof(DropCustomerTargetUI));
        RectTransform custRect = custObj.GetComponent<RectTransform>();
        custRect.sizeDelta = new Vector2(280, 75);

        Image custBg = custObj.GetComponent<Image>();
        custBg.color = new Color(0.22f, 0.18f, 0.25f, 0.95f);

        Button custBtn = custObj.GetComponent<Button>();

        // Border de Selección
        GameObject cBorderObj = new GameObject("SelectionBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cBorderObj.transform.SetParent(custObj.transform, false);
        RectTransform cbRect = cBorderObj.GetComponent<RectTransform>();
        cbRect.anchorMin = Vector2.zero;
        cbRect.anchorMax = Vector2.one;
        cbRect.sizeDelta = Vector2.zero;
        Image cbImg = cBorderObj.GetComponent<Image>();
        cbImg.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        cBorderObj.SetActive(false);

        // Avatar Image
        GameObject avObj = new GameObject("AvatarImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        avObj.transform.SetParent(custObj.transform, false);
        RectTransform aRect = avObj.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0.02f, 0.15f);
        aRect.anchorMax = new Vector2(0.18f, 0.85f);
        aRect.sizeDelta = Vector2.zero;
        Image avImg = avObj.GetComponent<Image>();
        avImg.preserveAspect = true;

        // Customer Name Text
        GameObject cNameObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        cNameObj.transform.SetParent(custObj.transform, false);
        RectTransform cnRect = cNameObj.GetComponent<RectTransform>();
        cnRect.anchorMin = new Vector2(0.22f, 0.65f);
        cnRect.anchorMax = new Vector2(0.98f, 0.98f);
        cnRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI cNameText = cNameObj.GetComponent<TextMeshProUGUI>();
        cNameText.fontSize = 14;
        cNameText.fontStyle = FontStyles.Bold;
        cNameText.color = Color.white;

        // Likes Text
        GameObject likesObj = new GameObject("LikesText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        likesObj.transform.SetParent(custObj.transform, false);
        RectTransform lRect = likesObj.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.22f, 0.35f);
        lRect.anchorMax = new Vector2(0.98f, 0.65f);
        lRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI likesText = likesObj.GetComponent<TextMeshProUGUI>();
        likesText.fontSize = 11;

        // Dislikes Text
        GameObject dislikesObj = new GameObject("DislikesText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        dislikesObj.transform.SetParent(custObj.transform, false);
        RectTransform dRect = dislikesObj.GetComponent<RectTransform>();
        dRect.anchorMin = new Vector2(0.22f, 0.05f);
        dRect.anchorMax = new Vector2(0.98f, 0.35f);
        dRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI dislikesText = dislikesObj.GetComponent<TextMeshProUGUI>();
        dislikesText.fontSize = 11;

        DeliveryCustomerItemUI custUI = custObj.GetComponent<DeliveryCustomerItemUI>();
        SerializedObject serCust = new SerializedObject(custUI);
        serCust.FindProperty("avatarImage").objectReferenceValue = avImg;
        serCust.FindProperty("nameText").objectReferenceValue = cNameText;
        serCust.FindProperty("likesText").objectReferenceValue = likesText;
        serCust.FindProperty("dislikesText").objectReferenceValue = dislikesText;
        serCust.FindProperty("selectionBorder").objectReferenceValue = cbImg;
        serCust.FindProperty("selectButton").objectReferenceValue = custBtn;
        serCust.ApplyModifiedProperties();

        string custPrefabPath = $"{prefabsDir}/DeliveryCustomerItem.prefab";
        GameObject custPrefab = PrefabUtility.SaveAsPrefabAsset(custObj, custPrefabPath);
        Object.DestroyImmediate(custObj);

        // -------------------------------------------------------------
        // 3. Buscar/Crear Canvas en la Escena
        // -------------------------------------------------------------
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

        // Limpiar panel anterior si ya existía en la escena
        Transform oldPanel = canvas.transform.Find("DeliveryPanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel.gameObject);
        }

        // -------------------------------------------------------------
        // 4. Crear GameObject DeliveryPanel dentro del Canvas
        // -------------------------------------------------------------
        GameObject panelObj = new GameObject("DeliveryPanel", typeof(RectTransform), typeof(Image), typeof(DeliveryPanelUI));
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = panelObj.GetComponent<Image>();
        panelBg.color = new Color(0.05f, 0.06f, 0.09f, 0.93f);

        DeliveryPanelUI panelUI = panelObj.GetComponent<DeliveryPanelUI>();

        // Objeto Marco Principal (MainBox)
        GameObject mainBox = new GameObject("MainBox", typeof(RectTransform), typeof(Image));
        mainBox.transform.SetParent(panelObj.transform, false);
        RectTransform mainRect = mainBox.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.1f, 0.08f);
        mainRect.anchorMax = new Vector2(0.9f, 0.92f);
        mainRect.sizeDelta = Vector2.zero;
        Image boxBg = mainBox.GetComponent<Image>();
        boxBg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

        // Header / Title Text
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(mainBox.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.88f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "🍽️ ENTREGAR PLATILLO (Presiona TAB para salir)";
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.9f, 0.4f);

        // Left Container (Dishes)
        GameObject leftPanel = new GameObject("DishesPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
        leftPanel.transform.SetParent(mainBox.transform, false);
        RectTransform leftRect = leftPanel.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.03f, 0.25f);
        leftRect.anchorMax = new Vector2(0.48f, 0.86f);
        leftRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup leftLayout = leftPanel.GetComponent<VerticalLayoutGroup>();
        leftLayout.spacing = 10;
        leftLayout.childControlWidth = true;
        leftLayout.childControlHeight = false;

        GameObject dishesTitle = new GameObject("HeaderDishes", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        dishesTitle.transform.SetParent(leftPanel.transform, false);
        TextMeshProUGUI dtText = dishesTitle.GetComponent<TextMeshProUGUI>();
        dtText.text = "<b>MIS PLATILLOS:</b> (Haz clic o arrastra)";
        dtText.fontSize = 16;
        dtText.color = new Color(0.4f, 0.8f, 1f);

        GameObject dishesContainer = new GameObject("DishesContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        dishesContainer.transform.SetParent(leftPanel.transform, false);
        VerticalLayoutGroup dcLayout = dishesContainer.GetComponent<VerticalLayoutGroup>();
        dcLayout.spacing = 8;
        dcLayout.childControlWidth = true;
        dcLayout.childControlHeight = false;

        // Right Container (Customers)
        GameObject rightPanel = new GameObject("CustomersPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
        rightPanel.transform.SetParent(mainBox.transform, false);
        RectTransform rightRect = rightPanel.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.52f, 0.25f);
        rightRect.anchorMax = new Vector2(0.97f, 0.86f);
        rightRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup rightLayout = rightPanel.GetComponent<VerticalLayoutGroup>();
        rightLayout.spacing = 10;
        rightLayout.childControlWidth = true;
        rightLayout.childControlHeight = false;

        GameObject custTitle = new GameObject("HeaderCustomers", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        custTitle.transform.SetParent(rightPanel.transform, false);
        TextMeshProUGUI ctText = custTitle.GetComponent<TextMeshProUGUI>();
        ctText.text = "<b>CLIENTES ACTIVOS:</b> (Suelta el platillo aquí)";
        ctText.fontSize = 16;
        ctText.color = new Color(1f, 0.8f, 0.4f);

        GameObject customersContainer = new GameObject("CustomersContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        customersContainer.transform.SetParent(rightPanel.transform, false);
        VerticalLayoutGroup ccLayout = customersContainer.GetComponent<VerticalLayoutGroup>();
        ccLayout.spacing = 8;
        ccLayout.childControlWidth = true;
        ccLayout.childControlHeight = false;

        // Bottom Footer Panel
        GameObject bottomPanel = new GameObject("BottomPanel", typeof(RectTransform));
        bottomPanel.transform.SetParent(mainBox.transform, false);
        RectTransform botRect = bottomPanel.GetComponent<RectTransform>();
        botRect.anchorMin = new Vector2(0.03f, 0.02f);
        botRect.anchorMax = new Vector2(0.97f, 0.22f);
        botRect.sizeDelta = Vector2.zero;

        // Preview Text
        GameObject previewObj = new GameObject("PreviewText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        previewObj.transform.SetParent(bottomPanel.transform, false);
        RectTransform prevRect = previewObj.GetComponent<RectTransform>();
        prevRect.anchorMin = new Vector2(0f, 0.45f);
        prevRect.anchorMax = new Vector2(0.68f, 1f);
        prevRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI previewText = previewObj.GetComponent<TextMeshProUGUI>();
        previewText.fontSize = 15;
        previewText.color = Color.white;
        previewText.text = "Selecciona un platillo y un cliente (o arrastra el platillo hacia el cliente).";

        // Feedback Text
        GameObject feedbackObj = new GameObject("FeedbackText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        feedbackObj.transform.SetParent(bottomPanel.transform, false);
        RectTransform feedRect = feedbackObj.GetComponent<RectTransform>();
        feedRect.anchorMin = new Vector2(0f, 0f);
        feedRect.anchorMax = new Vector2(0.68f, 0.45f);
        feedRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI feedbackText = feedbackObj.GetComponent<TextMeshProUGUI>();
        feedbackText.fontSize = 14;
        feedbackText.text = "";

        // Button Deliver
        GameObject btnDelObj = new GameObject("ButtonDeliver", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnDelObj.transform.SetParent(bottomPanel.transform, false);
        RectTransform bdRect = btnDelObj.GetComponent<RectTransform>();
        bdRect.anchorMin = new Vector2(0.70f, 0.2f);
        bdRect.anchorMax = new Vector2(0.84f, 0.8f);
        bdRect.sizeDelta = Vector2.zero;
        Image bdImg = btnDelObj.GetComponent<Image>();
        bdImg.color = new Color(0.2f, 0.7f, 0.35f);
        Button btnDeliver = btnDelObj.GetComponent<Button>();

        GameObject txtDelObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtDelObj.transform.SetParent(btnDelObj.transform, false);
        RectTransform tdRect = txtDelObj.GetComponent<RectTransform>();
        tdRect.anchorMin = Vector2.zero;
        tdRect.anchorMax = Vector2.one;
        tdRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI txtDel = txtDelObj.GetComponent<TextMeshProUGUI>();
        txtDel.text = "ENTREGAR";
        txtDel.alignment = TextAlignmentOptions.Center;
        txtDel.fontSize = 15;
        txtDel.fontStyle = FontStyles.Bold;
        txtDel.color = Color.white;

        // Button Close
        GameObject btnCloseObj = new GameObject("ButtonClose", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnCloseObj.transform.SetParent(bottomPanel.transform, false);
        RectTransform bcRect = btnCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.86f, 0.2f);
        bcRect.anchorMax = new Vector2(1f, 0.8f);
        bcRect.sizeDelta = Vector2.zero;
        Image bcImg = btnCloseObj.GetComponent<Image>();
        bcImg.color = new Color(0.75f, 0.22f, 0.22f);
        Button btnClose = btnCloseObj.GetComponent<Button>();

        GameObject txtCloseObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtCloseObj.transform.SetParent(btnCloseObj.transform, false);
        RectTransform tcRect = txtCloseObj.GetComponent<RectTransform>();
        tcRect.anchorMin = Vector2.zero;
        tcRect.anchorMax = Vector2.one;
        tcRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI txtClose = txtCloseObj.GetComponent<TextMeshProUGUI>();
        txtClose.text = "CERRAR";
        txtClose.alignment = TextAlignmentOptions.Center;
        txtClose.fontSize = 15;
        txtClose.fontStyle = FontStyles.Bold;
        txtClose.color = Color.white;

        // Conectar referencias en DeliveryPanelUI
        SerializedObject serPanel = new SerializedObject(panelUI);
        serPanel.FindProperty("panelRoot").objectReferenceValue = panelObj;
        serPanel.FindProperty("dishesContainer").objectReferenceValue = dishesContainer.transform;
        serPanel.FindProperty("customersContainer").objectReferenceValue = customersContainer.transform;
        serPanel.FindProperty("dishItemPrefab").objectReferenceValue = dishPrefab.GetComponent<DeliveryDishItemUI>();
        serPanel.FindProperty("customerItemPrefab").objectReferenceValue = custPrefab.GetComponent<DeliveryCustomerItemUI>();
        serPanel.FindProperty("previewText").objectReferenceValue = previewText;
        serPanel.FindProperty("feedbackText").objectReferenceValue = feedbackText;
        serPanel.FindProperty("deliverButton").objectReferenceValue = btnDeliver;
        serPanel.FindProperty("closeButton").objectReferenceValue = btnClose;

        DummyPlayer dummyPlayer = Object.FindFirstObjectByType<DummyPlayer>();
        if (dummyPlayer != null)
        {
            serPanel.FindProperty("player").objectReferenceValue = dummyPlayer;
        }

        serPanel.ApplyModifiedProperties();

        // -------------------------------------------------------------
        // 5. Guardar Prefab DeliveryPanel.prefab
        // -------------------------------------------------------------
        string panelPrefabPath = $"{prefabsDir}/DeliveryPanel.prefab";
        PrefabUtility.SaveAsPrefabAsset(panelObj, panelPrefabPath);

        // Desactivar el panel inicialmente en escena
        panelObj.SetActive(false);

        // -------------------------------------------------------------
        // 6. Asegurar Managers en la Escena
        // -------------------------------------------------------------
        if (Object.FindFirstObjectByType<DeliveryManager>() == null)
        {
            GameObject dmObj = new GameObject("[DeliveryManager]", typeof(DeliveryManager));
            Undo.RegisterCreatedObjectUndo(dmObj, "Create DeliveryManager");
        }

        if (Object.FindFirstObjectByType<UpgradeManager>() == null)
        {
            GameObject umObj = new GameObject("[UpgradeManager]", typeof(UpgradeManager));
            Undo.RegisterCreatedObjectUndo(umObj, "Create UpgradeManager");
        }

        if (Object.FindFirstObjectByType<DeliveryInputController>() == null)
        {
            GameObject icObj = new GameObject("[DeliveryInputController]", typeof(DeliveryInputController));
            Undo.RegisterCreatedObjectUndo(icObj, "Create DeliveryInputController");
        }

        // Cargar platillos en DummyPlayer si está vacío
        if (dummyPlayer != null && (dummyPlayer.availableDishes == null || dummyPlayer.availableDishes.Count == 0))
        {
            if (dummyPlayer.availableDishes == null) dummyPlayer.availableDishes = new System.Collections.Generic.List<DishData>();

            string[] dishGuids = AssetDatabase.FindAssets("t:DishData");
            foreach (string g in dishGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                DishData d = AssetDatabase.LoadAssetAtPath<DishData>(path);
                if (d != null && !dummyPlayer.availableDishes.Contains(d))
                {
                    dummyPlayer.availableDishes.Add(d);
                }
            }
            EditorUtility.SetDirty(dummyPlayer);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[Takernal UI Builder] ¡DeliveryPanel creado exitosamente en la escena y guardado en Assets/Prefabs/UI/DeliveryPanel.prefab!</color>");
    }
}
#endif
