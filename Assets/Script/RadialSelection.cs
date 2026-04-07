using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;

public class RadialSelection : MonoBehaviour
{
    [Header("Input (XRI)")]
    public InputActionProperty spawnAction;

    [Header("Radial Settings")]
    [Range(2, 10)]
    public int numberOfRadialPart = 5;
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public float angleBetweenRadialPart = 10;

    [Header("Controller")]
    public Transform controllerTransform;

    [Header("XR")]
    public XRBaseInteractor interactor;
    public XRInteractionManager interactionManager;

    [Header("Spawn Settings")]
    public float spawnDistance = 1.5f;

    [System.Serializable]
    public class ItemData
    {
        public string itemName;

        [Header("Grab (di tangan)")]
        public GameObject itemObject; // hierarchy template

        [Header("Spawn ke world")]
        public GameObject prefab; // opsional prefab
        public Transform scaleReference;

        [Header("Optional Template")]
        public GameObject templateObject; // properties akan dicopy ke prefab

        [Header("Inventory")]
        public int amount;
        public bool infinite;
    }

    [Header("Inventory")]
    public List<ItemData> items;

    public UnityEvent<int> onPartSelected;

    private List<GameObject> spawnedParts = new List<GameObject>();
    private int currentRadialPart = -1;
    private bool isHolding = false;

    private int currentIndex = -1;
    private XRGrabInteractable currentGrab;

    void Update()
    {
        float input = spawnAction.action.ReadValue<float>();

        if (input > 0.5f && !isHolding)
        {
            isHolding = true;
            SpawnRadialPart();
        }

        if (input > 0.5f && isHolding)
        {
            GetSelectedRadialPart();
        }

        if (input <= 0.5f && isHolding)
        {
            isHolding = false;
            HideAndTriggerSelectedPart();
        }
    }

    public void HideAndTriggerSelectedPart()
    {
        if (currentRadialPart >= 0)
        {
            EquipItem(currentRadialPart);
            onPartSelected.Invoke(currentRadialPart);
        }

        radialPartCanvas.gameObject.SetActive(false);
    }

    public void GetSelectedRadialPart()
    {
        Vector3 direction = controllerTransform.forward;
        Vector3 projected = Vector3.ProjectOnPlane(direction, radialPartCanvas.forward);

        float angle = Vector3.SignedAngle(radialPartCanvas.up, projected, -radialPartCanvas.forward);
        if (angle < 0) angle += 360;

        currentRadialPart = Mathf.FloorToInt(angle * numberOfRadialPart / 360f);

        for (int i = 0; i < spawnedParts.Count; i++)
        {
            Image img = spawnedParts[i].GetComponent<Image>();
            bool empty = (i < items.Count && !items[i].infinite && items[i].amount <= 0);

            if (i == currentRadialPart)
            {
                img.color = empty ? Color.gray : Color.red;
                spawnedParts[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                img.color = empty ? new Color(0.5f,0.5f,0.5f) : Color.white;
                spawnedParts[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SpawnRadialPart()
    {
        radialPartCanvas.gameObject.SetActive(true);
        radialPartCanvas.position = controllerTransform.position + controllerTransform.forward * 0.3f;
        radialPartCanvas.rotation = Quaternion.LookRotation(radialPartCanvas.position - Camera.main.transform.position);

        foreach (var item in spawnedParts) Destroy(item);
        spawnedParts.Clear();

        for (int i = 0; i < numberOfRadialPart; i++)
        {
            float angle = -i * (360f / numberOfRadialPart) + angleBetweenRadialPart / 2;
            GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
            part.transform.localPosition = Vector3.zero;
            part.transform.localEulerAngles = new Vector3(0, 0, angle);
            part.GetComponent<Image>().fillAmount = (1f / numberOfRadialPart) - (angleBetweenRadialPart / 360f);
            spawnedParts.Add(part);
        }
    }

    void EquipItem(int index)
    {
        if (index >= items.Count) return;

        ItemData item = items[index];
        if (index == 0 || (!item.infinite && item.amount <= 0)) return;

        GameObject obj = null;

        // 🔹 clone hierarchy jika ada
        if (item.itemObject != null)
        {
            obj = Instantiate(item.itemObject);
        }
        else if (item.prefab != null)
        {
            obj = Instantiate(item.prefab);

            // 🔹 copy component dari templateObject jika ada
            if (item.templateObject != null)
            {
                CopyComponents(item.templateObject, obj);
            }

            // scale tetap pakai scaleReference jika ada
            if (item.scaleReference != null)
                obj.transform.localScale = item.scaleReference.localScale;

            if (obj.GetComponent<XRGrabInteractable>() == null)
                obj.AddComponent<XRGrabInteractable>();
        }
        else
        {
            Debug.LogWarning("Item tidak punya object atau prefab!");
            return;
        }

        obj.transform.position = controllerTransform.position + controllerTransform.forward * spawnDistance;
        obj.transform.rotation = controllerTransform.rotation;

        XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
        if (grab != null && interactor != null && interactionManager != null)
        {
            interactionManager.SelectEnter(interactor, grab);
            currentGrab = grab;
            currentIndex = index;
        }
    }

    public void UseCurrentItem()
    {
        if (currentIndex < 0 || currentIndex >= items.Count) return;

        ItemData item = items[currentIndex];

        if (currentGrab != null && interactor != null && interactionManager != null)
        {
            interactionManager.SelectExit(interactor, currentGrab);
            currentGrab = null;
            currentIndex = -1;
        }

        if (item.prefab != null)
        {
            GameObject obj = Instantiate(item.prefab);
            obj.transform.position = controllerTransform.position + controllerTransform.forward * 2f;
            obj.transform.rotation = Quaternion.identity;

            if (item.templateObject != null)
            {
                CopyComponents(item.templateObject, obj);
            }

            if (item.scaleReference != null)
                obj.transform.localScale = item.scaleReference.localScale;

            if (obj.GetComponent<XRGrabInteractable>() == null)
                obj.AddComponent<XRGrabInteractable>();
        }

        if (!item.infinite)
        {
            item.amount--;
            if (item.amount <= 0) item.amount = 0;
        }
    }

    // 🔹 Utility: copy semua komponen dari source ke target (kecuali Transform)
    void CopyComponents(GameObject source, GameObject target)
    {
        Component[] components = source.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp is Transform) continue; // jangan copy transform
            UnityEditorInternal.ComponentUtility.CopyComponent(comp);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
        }
    }
}