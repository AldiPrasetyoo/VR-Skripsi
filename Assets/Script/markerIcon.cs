using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MinimapTargetIndicator : MonoBehaviour
{
    [System.Serializable]
    public class TargetUI
    {
        public string namaTarget;

        [Header("Target Gedung di World")]
        public Transform worldTarget;

        [Header("Pakai Titik Tengah Visual Gedung")]
        public bool useRendererBoundsCenter = true;

        [Header("Offset Tambahan Kalau Masih Kurang Pas")]
        public Vector3 extraOffset = Vector3.zero;

        [Header("Panah Saat Jauh / Keluar Minimap")]
        public RectTransform farArrow;

        [HideInInspector] public TMP_Text nearText;
        [HideInInspector] public TMP_Text farText;
    }

    [Header("Referensi")]
    public Camera minimapCamera;
    public RectTransform minimapRect;

    [Header("Pengaturan Area Minimap")]
    public float viewportMargin = 0.08f;
    public float edgePadding = 20f;

    [Header("Pengaturan Text Otomatis")]
    public bool buatTextOtomatis = true;
    public int fontSize = 12;
    public Color textColor = Color.white;

    [Header("Ukuran Text")]
    public Vector2 nearTextSize = new Vector2(140f, 28f);
    public Vector2 farTextSize = new Vector2(140f, 28f);

    [Header("Posisi Text")]
    public float farTextDownOffset = 16f;

    [Header("Koreksi Rotasi Panah")]
    [Tooltip("Kalau sprite panah default menghadap kanan, isi 0. Kalau menghadap atas, isi -90.")]
    public float arrowRotationOffset = 0f;

    [Header("Daftar Target")]
    public List<TargetUI> targets = new List<TargetUI>();

    private void Start()
    {
        if (buatTextOtomatis)
        {
            BuatSemuaTextOtomatis();
        }
    }

    private void LateUpdate()
    {
        if (minimapCamera == null || minimapRect == null)
            return;

        foreach (TargetUI target in targets)
        {
            if (target.worldTarget == null)
                continue;

            Vector3 targetWorldPosition = GetTargetCenter(target);
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(targetWorldPosition);

            bool insideMinimap =
                viewportPos.z > 0 &&
                viewportPos.x > viewportMargin &&
                viewportPos.x < 1f - viewportMargin &&
                viewportPos.y > viewportMargin &&
                viewportPos.y < 1f - viewportMargin;

            Vector2 uiPos = ViewportToMinimapPosition(viewportPos);

            if (insideMinimap)
            {
                ShowNearName(target, uiPos);
            }
            else
            {
                ShowFarArrow(target, uiPos, viewportPos);
            }
        }
    }

    private void BuatSemuaTextOtomatis()
    {
        foreach (TargetUI target in targets)
        {
            Transform parent = minimapRect;

            if (target.farArrow != null)
            {
                parent = target.farArrow.parent;
            }

            if (target.nearText == null)
            {
                target.nearText = BuatTextTMP(
                    parent,
                    target.namaTarget + "_NearText",
                    target.namaTarget,
                    nearTextSize
                );
            }

            if (target.farText == null)
            {
                target.farText = BuatTextTMP(
                    parent,
                    target.namaTarget + "_FarText",
                    target.namaTarget,
                    farTextSize
                );
            }
        }
    }

    private TMP_Text BuatTextTMP(Transform parent, string objectName, string isiText, Vector2 size)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.text = isiText;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;

        textObject.SetActive(false);

        return tmp;
    }

    private Vector3 GetTargetCenter(TargetUI target)
    {
        if (!target.useRendererBoundsCenter)
        {
            return target.worldTarget.position + target.extraOffset;
        }

        Renderer[] renderers = target.worldTarget.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return target.worldTarget.position + target.extraOffset;
        }

        Bounds combinedBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        return combinedBounds.center + target.extraOffset;
    }

    private void ShowNearName(TargetUI target, Vector2 uiPos)
    {
        if (target.nearText != null)
        {
            target.nearText.gameObject.SetActive(true);
            target.nearText.text = target.namaTarget;

            RectTransform textRect = target.nearText.GetComponent<RectTransform>();
            textRect.anchoredPosition = uiPos;
            textRect.localRotation = Quaternion.identity;
        }

        if (target.farArrow != null)
        {
            target.farArrow.gameObject.SetActive(false);
        }

        if (target.farText != null)
        {
            target.farText.gameObject.SetActive(false);
        }
    }

    private void ShowFarArrow(TargetUI target, Vector2 uiPos, Vector3 viewportPos)
    {
        if (target.nearText != null)
        {
            target.nearText.gameObject.SetActive(false);
        }

        if (target.farArrow == null)
            return;

        target.farArrow.gameObject.SetActive(true);

        Vector2 dir = uiPos;

        if (viewportPos.z < 0)
        {
            dir = -dir;
        }

        if (dir.sqrMagnitude < 0.001f)
        {
            dir = Vector2.up;
        }

        Vector2 edgePos = GetEdgePosition(dir.normalized);

        target.farArrow.anchoredPosition = edgePos;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + arrowRotationOffset;
        target.farArrow.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (target.farText != null)
        {
            target.farText.gameObject.SetActive(true);
            target.farText.text = target.namaTarget;

            RectTransform textRect = target.farText.GetComponent<RectTransform>();

            textRect.localRotation = Quaternion.identity;

            // INI BAGIAN PENTING:
            // Text selalu di bawah arrow, tidak peduli arrow muter ke mana.
            textRect.anchoredPosition = edgePos + new Vector2(0f, -farTextDownOffset);
        }
    }

    private Vector2 ViewportToMinimapPosition(Vector3 viewportPos)
    {
        Rect rect = minimapRect.rect;

        float x = Mathf.Lerp(rect.xMin, rect.xMax, viewportPos.x);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, viewportPos.y);

        return new Vector2(x, y);
    }

    private Vector2 GetEdgePosition(Vector2 dir)
    {
        Vector2 halfSize = minimapRect.rect.size * 0.5f;
        halfSize -= new Vector2(edgePadding, edgePadding);

        float scaleX = Mathf.Abs(dir.x) > 0.001f
            ? halfSize.x / Mathf.Abs(dir.x)
            : float.MaxValue;

        float scaleY = Mathf.Abs(dir.y) > 0.001f
            ? halfSize.y / Mathf.Abs(dir.y)
            : float.MaxValue;

        float scale = Mathf.Min(scaleX, scaleY);

        return dir * scale;
    }
}