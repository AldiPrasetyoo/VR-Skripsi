using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class RadialSelection : MonoBehaviour
    {
        [Header("Input (XRI)")]
        public InputActionProperty spawnAction;

        [Header("Radial Settings")]
        [Range(2, 10)]
        public int numberOfRadialPart = 5;
        public GameObject radialPartPrefab;
        public Transform radialPartCanvas;
        public float angleBetweenRadialPart = 10f;

        [Header("Controller")]
        public Transform controllerTransform;
        public XRBaseInteractor interactor;
        public XRInteractionManager interactionManager;

        [Header("Spawn Settings")]
        public float spawnDistance = 1.5f;

        [System.Serializable]
        public class ItemData
        {
            public string itemName;

            [Header("UI")]
            public Sprite icon;

            [Header("Hierarchy (NO CLONE)")]
            public GameObject itemObject;

            [Header("Prefab (fallback)")]
            public GameObject prefab;
            public Transform scaleReference;

            [Header("Optional Component Copy")]
            public GameObject componentReference;

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

        void Update()
        {
            float input = spawnAction.action.ReadValue<float>();

            if (interactor != null && interactor.hasSelection)
                return;

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
            if (currentRadialPart < 0 || currentRadialPart >= items.Count)
            {
                radialPartCanvas.gameObject.SetActive(false);
                return;
            }

            ItemData item = items[currentRadialPart];

            // ❌ kalau habis
            if (!item.infinite && item.amount <= 0)
            {
                radialPartCanvas.gameObject.SetActive(false);
                return;
            }

            EquipItem(currentRadialPart);
            onPartSelected.Invoke(currentRadialPart);

            // 🔥 FIX LIMIT (PENTING)
            if (!item.infinite)
            {
                item.amount--;
            }

            radialPartCanvas.gameObject.SetActive(false);
        }

        public void GetSelectedRadialPart()
        {
            Vector3 direction = controllerTransform.forward;
            Vector3 projected = Vector3.ProjectOnPlane(direction, radialPartCanvas.forward);
            float angle = Vector3.SignedAngle(radialPartCanvas.up, projected, -radialPartCanvas.forward);

            if (angle < 0) angle += 360f;

            currentRadialPart = Mathf.FloorToInt(angle * numberOfRadialPart / 360f);

            for (int i = 0; i < spawnedParts.Count; i++)
            {
                Image img = spawnedParts[i].GetComponent<Image>();
                bool empty = i < items.Count && !items[i].infinite && items[i].amount <= 0;

                if (i == currentRadialPart)
                {
                    img.color = empty ? Color.gray : Color.red;
                    spawnedParts[i].transform.localScale = Vector3.one * 1.1f;
                }
                else
                {
                    img.color = empty ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
                    spawnedParts[i].transform.localScale = Vector3.one;
                }
            }
        }

        public void SpawnRadialPart()
        {
            radialPartCanvas.gameObject.SetActive(true);

            radialPartCanvas.position = controllerTransform.position + controllerTransform.forward * 0.3f;

            radialPartCanvas.rotation = Quaternion.LookRotation(
                radialPartCanvas.position - Camera.main.transform.position
            );

            foreach (var item in spawnedParts)
                Destroy(item);

            spawnedParts.Clear();

            for (int i = 0; i < numberOfRadialPart; i++)
            {
                float angle = -i * (360f / numberOfRadialPart) + angleBetweenRadialPart / 2f;

                GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
                part.transform.localPosition = Vector3.zero;
                part.transform.localEulerAngles = new Vector3(0, 0, angle);

                part.GetComponent<Image>().fillAmount =
                    (1f / numberOfRadialPart) - (angleBetweenRadialPart / 360f);

                // 🔥 ICON / TEXT
                if (i < items.Count)
                {
                    ItemData item = items[i];

                    Transform content = part.transform.Find("Content");

                    if (content != null)
                    {
                        Image icon = content.Find("Icon")?.GetComponent<Image>();
                        TMPro.TMP_Text text = content.Find("Text")?.GetComponent<TMPro.TMP_Text>();

                        if (item.icon != null)
                        {
                            if (icon != null)
                            {
                                icon.sprite = item.icon;
                                icon.enabled = true;
                                icon.preserveAspect = true;
                            }

                            if (text != null)
                                text.gameObject.SetActive(false);
                        }
                        else
                        {
                            if (icon != null)
                                icon.enabled = false;

                            if (text != null)
                            {
                                text.gameObject.SetActive(true);

                                if (item.infinite)
                                    text.text = item.itemName;
                                else
                                    text.text = item.itemName + " (" + item.amount + ")";
                            }
                        }
                    }
                }

                spawnedParts.Add(part);
            }
        }

        void EquipItem(int index)
        {
            ItemData item = items[index];

            GameObject obj = null;

            if (item.itemObject != null)
            {
                obj = item.itemObject;

                if (!obj.activeInHierarchy)
                    obj.SetActive(true);
            }
            else if (item.prefab != null)
            {
                obj = Instantiate(item.prefab);

                if (item.scaleReference != null)
                    obj.transform.localScale = item.scaleReference.localScale;

#if UNITY_EDITOR
                if (item.componentReference != null)
                {
                    foreach (var comp in item.componentReference.GetComponents<Component>())
                    {
                        if (comp is Transform) continue;
                        UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(obj);
                    }
                }
#endif
            }
            else return;

            obj.transform.position = controllerTransform.position + controllerTransform.forward * spawnDistance;
            obj.transform.rotation = controllerTransform.rotation;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            if (obj.GetComponent<Collider>() == null)
                obj.AddComponent<BoxCollider>();

            XRGrabInteractable grab = obj.GetComponent<XRGrabInteractable>();
            if (grab == null) grab = obj.AddComponent<XRGrabInteractable>();

            StartCoroutine(GrabNextFrame(grab));
        }

        System.Collections.IEnumerator GrabNextFrame(XRGrabInteractable grab)
        {
            yield return null;

            if (interactor != null && interactionManager != null)
            {
                interactionManager.SelectEnter(interactor, grab);
            }
        }
    }
}