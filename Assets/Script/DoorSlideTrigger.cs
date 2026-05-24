using UnityEngine;
using System.Collections;

public class DoorSlideTrigger : MonoBehaviour
{
    [Header("Pintu Elevator")]
    public Transform doorLeft;
    public Transform doorRight;

    [Header("Jarak Buka Pintu di Sumbu X")]
    public float openDistance = 0.75f;

    [Header("Kecepatan")]
    public float speed = 3f;

    [Header("Jika arah buka kebalik, centang ini")]
    public bool reverseDirection = false;

    private Vector3 leftClosePos;
    private Vector3 rightClosePos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private Coroutine doorCoroutine;

    private void Start()
    {
        if (doorLeft == null || doorRight == null)
        {
            Debug.LogError("Door left atau Door right belum dimasukkan di Inspector!");
            return;
        }

        // Posisi awal di scene dianggap sebagai posisi TERTUTUP
        leftClosePos = doorLeft.localPosition;
        rightClosePos = doorRight.localPosition;

        // Hitung arah buka di sumbu X
        if (!reverseDirection)
        {
            leftOpenPos = leftClosePos + new Vector3(-openDistance, 0, 0);
            rightOpenPos = rightClosePos + new Vector3(openDistance, 0, 0);
        }
        else
        {
            leftOpenPos = leftClosePos + new Vector3(openDistance, 0, 0);
            rightOpenPos = rightClosePos + new Vector3(-openDistance, 0, 0);
        }

        Debug.Log("DoorSlideTrigger aktif. Posisi pintu tertutup sudah disimpan.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ada object masuk trigger: " + other.name + " | Tag: " + other.tag);

        if (IsPlayer(other))
        {
            Debug.Log("Player masuk trigger pintu. Pintu dibuka.");
            OpenDoor();
        }
        else
        {
            Debug.Log("Object masuk trigger, tapi bukan Player.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Ada object keluar trigger: " + other.name + " | Tag: " + other.tag);

        if (IsPlayer(other))
        {
            Debug.Log("Player keluar trigger pintu. Pintu ditutup.");
            CloseDoor();
        }
        else
        {
            Debug.Log("Object keluar trigger, tapi bukan Player.");
        }
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.transform.root.CompareTag("Player"))
            return true;

        if (other.GetComponentInParent<CharacterController>() != null)
            return true;

        return false;
    }

    public void OpenDoor()
    {
        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        doorCoroutine = StartCoroutine(MoveDoor(leftOpenPos, rightOpenPos));
    }

    public void CloseDoor()
    {
        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        doorCoroutine = StartCoroutine(MoveDoor(leftClosePos, rightClosePos));
    }

    private IEnumerator MoveDoor(Vector3 targetLeft, Vector3 targetRight)
    {
        Debug.Log("Pintu mulai bergerak.");

        while (
            Vector3.Distance(doorLeft.localPosition, targetLeft) > 0.01f ||
            Vector3.Distance(doorRight.localPosition, targetRight) > 0.01f
        )
        {
            doorLeft.localPosition = Vector3.MoveTowards(
                doorLeft.localPosition,
                targetLeft,
                speed * Time.deltaTime
            );

            doorRight.localPosition = Vector3.MoveTowards(
                doorRight.localPosition,
                targetRight,
                speed * Time.deltaTime
            );

            yield return null;
        }

        doorLeft.localPosition = targetLeft;
        doorRight.localPosition = targetRight;

        Debug.Log("Pintu selesai bergerak.");
    }
}