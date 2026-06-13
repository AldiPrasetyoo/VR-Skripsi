using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PTPSimulationPanel : MonoBehaviour
{
    public enum AntennaRole
    {
        None,
        Station,
        AP
    }

    [Header("Role Antenna 1")]
    public Button antenna1NoneButton;
    public Button antenna1StationButton;
    public Button antenna1APButton;

    [Header("Role Antenna 2")]
    public Button antenna2NoneButton;
    public Button antenna2StationButton;
    public Button antenna2APButton;

    [Header("Icon Tower")]
    public Image antenna1Image;
    public Image antenna2Image;

    public Sprite towerNoneSprite;
    public Sprite towerStationSprite;
    public Sprite towerAPSprite;

    [Header("Titik Animasi Data")]
    public GameObject dataDotPrefab;
    public RectTransform dataDotParent;

    public RectTransform antenna1Point;
    public RectTransform antenna2Point;

    [Header("Panah Arah Data")]
    public RectTransform arrowRect;
    public Image arrowImage;

    [Header("Pengaturan Animasi")]
    public int dotCount = 10;
    public float dotSpeed = 220f;
    public float dotSpacing = 55f;

    [Header("Jarak Panah Dari Dot Depan")]
    public float arrowForwardOffset = 80f;

    [Range(0f, 1f)]
    public float backDotAlpha = 0.25f;

    [Range(0f, 1f)]
    public float frontDotAlpha = 1f;

    [Header("Status UI")]
    public TMP_Text statusText;
    public TMP_Text ruleText;
    public TMP_Text rolePairText;

    [Header("Warna")]
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.red;
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = Color.green;

    private AntennaRole antenna1Role = AntennaRole.None;
    private AntennaRole antenna2Role = AntennaRole.None;

    private RectTransform[] dots;
    private Image[] dotImages;

    private bool linkActive;
    private float moveValue;

    private Vector2 currentStartPoint;
    private Vector2 currentEndPoint;

    private void Start()
    {
        if (antenna1NoneButton != null)
            antenna1NoneButton.onClick.AddListener(() => SetAntenna1Role(AntennaRole.None));

        if (antenna1StationButton != null)
            antenna1StationButton.onClick.AddListener(() => SetAntenna1Role(AntennaRole.Station));

        if (antenna1APButton != null)
            antenna1APButton.onClick.AddListener(() => SetAntenna1Role(AntennaRole.AP));

        if (antenna2NoneButton != null)
            antenna2NoneButton.onClick.AddListener(() => SetAntenna2Role(AntennaRole.None));

        if (antenna2StationButton != null)
            antenna2StationButton.onClick.AddListener(() => SetAntenna2Role(AntennaRole.Station));

        if (antenna2APButton != null)
            antenna2APButton.onClick.AddListener(() => SetAntenna2Role(AntennaRole.AP));

        CreateDataDots();
        UpdateSimulation();
    }

    private void Update()
    {
        if (linkActive)
        {
            AnimateDataTransfer();
        }
    }

    private void SetAntenna1Role(AntennaRole role)
    {
        antenna1Role = role;
        UpdateSimulation();
    }

    private void SetAntenna2Role(AntennaRole role)
    {
        antenna2Role = role;
        UpdateSimulation();
    }

    private void UpdateSimulation()
    {
        linkActive = IsLinkActive();

        SetDirectionByRole();
        UpdateTowerIcon();
        UpdateButtonColor();
        UpdateStatusText();
        UpdateRolePairText();
        UpdateDataVisual();

        moveValue = 0f;
    }

    private bool IsLinkActive()
    {
        if (antenna1Role == AntennaRole.None) return false;
        if (antenna2Role == AntennaRole.None) return false;

        if (antenna1Role == AntennaRole.AP && antenna2Role == AntennaRole.Station)
            return true;

        if (antenna1Role == AntennaRole.Station && antenna2Role == AntennaRole.AP)
            return true;

        return false;
    }

    private void SetDirectionByRole()
    {
        if (antenna1Point == null || antenna2Point == null) return;

        if (antenna1Role == AntennaRole.AP && antenna2Role == AntennaRole.Station)
        {
            currentStartPoint = antenna1Point.anchoredPosition;
            currentEndPoint = antenna2Point.anchoredPosition;
        }
        else if (antenna1Role == AntennaRole.Station && antenna2Role == AntennaRole.AP)
        {
            currentStartPoint = antenna2Point.anchoredPosition;
            currentEndPoint = antenna1Point.anchoredPosition;
        }
        else
        {
            currentStartPoint = antenna1Point.anchoredPosition;
            currentEndPoint = antenna2Point.anchoredPosition;
        }
    }

    private void CreateDataDots()
    {
        dots = new RectTransform[dotCount];
        dotImages = new Image[dotCount];

        if (dataDotPrefab == null || dataDotParent == null)
        {
            Debug.LogWarning("Data Dot Prefab atau Data Dot Parent belum diisi.");
            return;
        }

        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dataDotPrefab, dataDotParent);

            RectTransform rect = dot.GetComponent<RectTransform>();
            Image img = dot.GetComponent<Image>();

            dots[i] = rect;
            dotImages[i] = img;

            dot.SetActive(false);
        }

        if (arrowRect != null)
            arrowRect.gameObject.SetActive(false);
    }

    private void AnimateDataTransfer()
    {
        if (dots == null) return;

        float distance = Vector2.Distance(currentStartPoint, currentEndPoint);

        if (distance <= 1f) return;

        moveValue += dotSpeed * Time.deltaTime;

        if (moveValue > distance)
            moveValue = 0f;

        Vector2 direction = (currentEndPoint - currentStartPoint).normalized;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;

            float dotDistance = moveValue - (i * dotSpacing);

            while (dotDistance < 0)
                dotDistance += distance;

            float t = dotDistance / distance;

            dots[i].anchoredPosition = Vector2.Lerp(currentStartPoint, currentEndPoint, t);

            float alpha;

            if (dotCount <= 1)
                alpha = frontDotAlpha;
            else
                alpha = Mathf.Lerp(frontDotAlpha, backDotAlpha, (float)i / (dotCount - 1));

            if (dotImages[i] != null)
            {
                Color c = activeColor;
                c.a = alpha;
                dotImages[i].color = c;
            }
        }

        UpdateArrow(direction, distance);
    }

    private void UpdateArrow(Vector2 direction, float distance)
    {
        if (arrowRect == null) return;

        arrowRect.gameObject.SetActive(true);

        float arrowMoveValue = moveValue + arrowForwardOffset;

        while (arrowMoveValue > distance)
            arrowMoveValue -= distance;

        float t = arrowMoveValue / distance;
        t = Mathf.Clamp01(t);

        arrowRect.anchoredPosition = Vector2.Lerp(currentStartPoint, currentEndPoint, t);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        if (arrowImage != null)
        {
            Color c = activeColor;
            c.a = 1f;
            arrowImage.color = c;
        }
    }

    private void UpdateDataVisual()
    {
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null)
                    dots[i].gameObject.SetActive(linkActive);
            }
        }

        if (arrowRect != null)
            arrowRect.gameObject.SetActive(linkActive);
    }

    private void UpdateTowerIcon()
    {
        if (antenna1Image != null)
            antenna1Image.sprite = GetSpriteByRole(antenna1Role);

        if (antenna2Image != null)
            antenna2Image.sprite = GetSpriteByRole(antenna2Role);
    }

    private Sprite GetSpriteByRole(AntennaRole role)
    {
        if (role == AntennaRole.Station)
            return towerStationSprite;

        if (role == AntennaRole.AP)
            return towerAPSprite;

        return towerNoneSprite;
    }

    private void UpdateButtonColor()
    {
        SetButtonColor(antenna1NoneButton, antenna1Role == AntennaRole.None);
        SetButtonColor(antenna1StationButton, antenna1Role == AntennaRole.Station);
        SetButtonColor(antenna1APButton, antenna1Role == AntennaRole.AP);

        SetButtonColor(antenna2NoneButton, antenna2Role == AntennaRole.None);
        SetButtonColor(antenna2StationButton, antenna2Role == AntennaRole.Station);
        SetButtonColor(antenna2APButton, antenna2Role == AntennaRole.AP);
    }

    private void SetButtonColor(Button button, bool selected)
    {
        if (button == null) return;

        Image img = button.GetComponent<Image>();

        if (img != null)
            img.color = selected ? selectedButtonColor : normalButtonColor;
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            if (linkActive)
            {
                statusText.text = "LINK AKTIF";
                statusText.color = activeColor;
            }
            else
            {
                statusText.text = "NO LINK";
                statusText.color = inactiveColor;
            }
        }

        if (ruleText != null)
        {
            ruleText.text = "Aturan: Link aktif hanya jika satu antena Station dan satu antena AP.";
        }
    }

    private void UpdateRolePairText()
    {
        if (rolePairText == null) return;

        rolePairText.text = GetRoleName(antenna1Role) + " - " + GetRoleName(antenna2Role);

        rolePairText.color = linkActive ? activeColor : inactiveColor;
    }

    private string GetRoleName(AntennaRole role)
    {
        if (role == AntennaRole.Station)
            return "Station";

        if (role == AntennaRole.AP)
            return "Access Point";

        return "None";
    }
}