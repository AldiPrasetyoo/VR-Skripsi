using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class PedestalDisplaySpawner : MonoBehaviour
    {
        [Header("Prefab yang akan di-spawn")]
        public GameObject prefabToSpawn;

        [Header("Titik posisi pajangan di pedestal")]
        public Transform pedestalPoint;

        [Header("Pajangan tetap di pedestal")]
        public bool keepDisplayLocked = true;

        [Header("Rotasi Pajangan")]
        public bool rotateDisplay = true;
        public float rotateSpeed = 30f;

        [Header("Batas Spawn")]
        public int maxSpawnCount = 1;
        public bool onlyAllowOneActiveObject = true;

        [Header("Grab Target")]
        public bool grabFromChild = false;

        private int spawnedCount;
        private GameObject currentSpawnedObject;

        private XRGrabInteractable displayGrab;
        private Rigidbody rb;
        private bool isSpawning;


        private void Awake()
        {
            displayGrab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            if (displayGrab == null)
            {
                Debug.LogError("PedestalDisplaySpawner harus dipasang di object pajangan yang punya XRGrabInteractable: " + gameObject.name);
                return;
            }

            displayGrab.selectEntered.AddListener(OnDisplayGrabbed);
        }


        private void Start()
        {
            SnapDisplayPosition();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }


        private void LateUpdate()
        {
            if (keepDisplayLocked)
            {
                SnapDisplayPosition();
            }

            if (rotateDisplay)
            {
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            }
        }


        private void OnDisplayGrabbed(SelectEnterEventArgs args)
        {
            if (isSpawning) return;


            if (prefabToSpawn == null)
            {
                Debug.LogWarning("Prefab To Spawn masih kosong.");
                return;
            }


            if (!CanSpawn())
            {
                Debug.Log("Spawn dibatasi. Tidak bisa mengambil item lagi.");

                ForceReleaseDisplay(args);
                SnapDisplayPosition();

                return;
            }


            StartCoroutine(SpawnPrefabAndGrab(args));
        }


        private bool CanSpawn()
        {
            if (spawnedCount >= maxSpawnCount)
            {
                return false;
            }


            if (onlyAllowOneActiveObject && currentSpawnedObject != null)
            {
                return false;
            }


            return true;
        }


        private IEnumerator SpawnPrefabAndGrab(SelectEnterEventArgs args)
        {
            isSpawning = true;


            IXRSelectInteractor interactor = args.interactorObject;
            XRInteractionManager manager = displayGrab.interactionManager;


            Component interactorComponent = interactor as Component;

            Transform handTransform = interactorComponent != null 
                ? interactorComponent.transform 
                : transform;


            ForceReleaseDisplay(args);
            SnapDisplayPosition();


            yield return null;


            GameObject spawnedObject = Instantiate(
                prefabToSpawn,
                handTransform.position,
                handTransform.rotation
            );


            currentSpawnedObject = spawnedObject;
            spawnedCount++;


            Rigidbody spawnedRb = spawnedObject.GetComponent<Rigidbody>();

            if (spawnedRb != null)
            {
                spawnedRb.linearVelocity = Vector3.zero;
                spawnedRb.angularVelocity = Vector3.zero;

                spawnedRb.isKinematic = false;
                spawnedRb.useGravity = false;
            }



            XRGrabInteractable spawnedGrab;


            // PILIH GRAB DARI ROOT ATAU CHILD
            if (grabFromChild)
            {
                spawnedGrab = spawnedObject.GetComponentInChildren<XRGrabInteractable>();
            }
            else
            {
                spawnedGrab = spawnedObject.GetComponent<XRGrabInteractable>();
            }



            yield return null;


            if (spawnedGrab != null && manager != null)
            {
                manager.SelectEnter(interactor, spawnedGrab);
            }
            else
            {
                Debug.LogWarning(
                    "Prefab hasil spawn tidak punya XRGrabInteractable di target yang dipilih."
                );
            }


            SnapDisplayPosition();


            isSpawning = false;
        }



        private void ForceReleaseDisplay(SelectEnterEventArgs args)
        {
            if (displayGrab == null) return;
            if (displayGrab.interactionManager == null) return;


            displayGrab.interactionManager.SelectExit(
                args.interactorObject,
                displayGrab
            );
        }



        private void SnapDisplayPosition()
        {
            if (pedestalPoint == null) return;


            transform.position = pedestalPoint.position;
        }



        public void ResetSpawnLimit()
        {
            spawnedCount = 0;
            currentSpawnedObject = null;
        }
    }
}