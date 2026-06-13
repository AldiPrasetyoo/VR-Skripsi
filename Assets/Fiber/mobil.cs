using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class PodMovement : MonoBehaviour
{
    [Header("Tujuan Mobil")]
    public Transform endPoint;

    [Header("Kecepatan")]
    public float speed = 5f;

    [Header("Passenger / XR Origin")]
    private GameObject passenger;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    [Header("Shake / Guncangan")]
    public Transform visualMobil;
    public float shakeDuration = 1.5f;
    public float shakeMagnitude = 0.08f;

    public Vector3 originalVisualLocalPos;

    [Header("Teleport Setelah Sampai")]
    public bool teleportSetelahSampai = true;
    public TeleportationProvider teleportProvider;
    public Transform titikTeleportSetelahSampai;
    public float delayTeleportSetelahSampai = 2f;

    [Header("Object Dimatikan Setelah Sampai")]
    public GameObject[] objectDimatikanSetelahSampai;

    [Header("Object Dinyalakan Setelah Sampai")]
    public GameObject[] objectDinyalakanSetelahSampai;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isShaking = false;
    private bool canMove = false;
    private bool arrived = false;
    private bool teleportRoutineStarted = false;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Start()
    {
        if (visualMobil == null)
        {
            visualMobil = transform;
        }

        originalVisualLocalPos = visualMobil.localPosition;
    }

    public void ResetPod()
    {
        StopAllCoroutines();

        transform.position = startPosition;
        transform.rotation = startRotation;

        passenger = null;

        canMove = false;
        arrived = false;
        isShaking = false;
        teleportRoutineStarted = false;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        if (visualMobil != null)
        {
            visualMobil.localPosition = originalVisualLocalPos;
        }

        Debug.Log("Mobil di-reset ke posisi awal");
    }

    public void SetPassenger(GameObject obj)
    {
        passenger = obj;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("XR Origin mulai ikut mobil");
    }

    public void StartMove()
    {
        canMove = true;
        arrived = false;
        teleportRoutineStarted = false;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        Debug.Log("Mobil mulai jalan");
    }

    private void Update()
    {
        if (!canMove) return;
        if (arrived) return;
        if (endPoint == null) return;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        transform.position = Vector3.MoveTowards(
            transform.position,
            endPoint.position,
            speed * Time.deltaTime
        );

        Vector3 direction = endPoint.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        IkutkanPassenger();

        if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
        {
            arrived = true;
            canMove = false;

            Debug.Log("Mobil sampai tujuan");

            if (!teleportRoutineStarted)
            {
                teleportRoutineStarted = true;
                StartCoroutine(TeleportSetelahSampai());
            }
        }
    }

    private void IkutkanPassenger()
    {
        if (!canMove) return;
        if (passenger == null) return;

        Vector3 deltaPos = transform.position - lastPosition;
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);

        CharacterController cc = passenger.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        passenger.transform.position += deltaPos;

        Vector3 offset = passenger.transform.position - transform.position;
        offset = deltaRot * offset;
        passenger.transform.position = transform.position + offset;

        passenger.transform.rotation = deltaRot * passenger.transform.rotation;

        if (cc != null)
            cc.enabled = true;
    }

    private IEnumerator TeleportSetelahSampai()
    {
        yield return new WaitForSeconds(delayTeleportSetelahSampai);

        // Pastikan mobil berhenti
        canMove = false;
        arrived = true;

        // Simpan XR Origin dulu
        GameObject xrYangDiteleport = passenger;

        // LEPAS XR Origin dari sistem ikut mobil sebelum teleport
        passenger = null;

        MatikanObjectSetelahSampai();
        NyalakanObjectSetelahSampai();

        if (!teleportSetelahSampai)
        {
            Debug.Log("Teleport setelah sampai dimatikan");
            yield break;
        }

        if (teleportProvider == null)
        {
            Debug.LogWarning("Teleport Provider belum dimasukkan!");
            yield break;
        }

        if (titikTeleportSetelahSampai == null)
        {
            Debug.LogWarning("Titik teleport setelah sampai belum dimasukkan!");
            yield break;
        }

        if (!teleportProvider.enabled)
            teleportProvider.enabled = true;

        CharacterController cc = null;

        if (xrYangDiteleport != null)
        {
            cc = xrYangDiteleport.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;
        }

        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = titikTeleportSetelahSampai.position,
            destinationRotation = titikTeleportSetelahSampai.rotation
        };

        teleportProvider.QueueTeleportRequest(request);

        yield return new WaitForSeconds(0.2f);

        if (cc != null)
            cc.enabled = true;

        Debug.Log("XR Origin dilepas dari mobil lalu diteleport ke tempat baru");
    }

    private void MatikanObjectSetelahSampai()
    {
        for (int i = 0; i < objectDimatikanSetelahSampai.Length; i++)
        {
            if (objectDimatikanSetelahSampai[i] != null)
                objectDimatikanSetelahSampai[i].SetActive(false);
        }
    }

    private void NyalakanObjectSetelahSampai()
    {
        for (int i = 0; i < objectDinyalakanSetelahSampai.Length; i++)
        {
            if (objectDinyalakanSetelahSampai[i] != null)
                objectDinyalakanSetelahSampai[i].SetActive(true);
        }
    }

    public void StartShake()
    {
        if (!isShaking)
            StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;

        float elapsed = 0f;

        Vector3 lastVisualOffset = Vector3.zero;
        Vector3 lastPassengerOffset = Vector3.zero;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            Vector3 newOffset = new Vector3(x, y, 0f);

            if (visualMobil != null)
            {
                visualMobil.localPosition = visualMobil.localPosition - lastVisualOffset + newOffset;
                lastVisualOffset = newOffset;
            }

            if (passenger != null)
            {
                CharacterController cc = passenger.GetComponent<CharacterController>();

                if (cc != null)
                    cc.enabled = false;

                passenger.transform.position = passenger.transform.position - lastPassengerOffset + newOffset;

                if (cc != null)
                    cc.enabled = true;

                lastPassengerOffset = newOffset;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (visualMobil != null)
        {
            visualMobil.localPosition = visualMobil.localPosition - lastVisualOffset;
        }

        if (passenger != null)
        {
            CharacterController cc = passenger.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            passenger.transform.position = passenger.transform.position - lastPassengerOffset;

            if (cc != null)
                cc.enabled = true;
        }

        isShaking = false;
    }
}