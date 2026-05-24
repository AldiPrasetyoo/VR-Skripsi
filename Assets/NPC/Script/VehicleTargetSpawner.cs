using UnityEngine;

public class VehicleTargetSpawner : MonoBehaviour
{
    public string[] spawnDirection;
    public GameObject targetPrefab;

    GameObject upTarget;
    GameObject downTarget;
    GameObject leftTarget;
    GameObject rightTarget;

    GameObject nextUpTarget;
    GameObject nextDownTarget;
    GameObject nextLeftTarget;
    GameObject nextRightTarget;

    public GameObject redlightBlockPrefab;

    GameObject upRedLight;
    GameObject downRedLight;
    GameObject leftRedLight;
    GameObject rightRedLight;

    float timerRedLight = 0;
    float maxTimerRedLight = 10;
    int indexGreenLight = 0;

    GameObject[] redLightBlock;

    void Start()
    {
        // ===============================
        // SPAWN TARGET DI JALUR SEBELAHNYA
        // ===============================

        for (int i = 0; i < spawnDirection.Length; i++)
        {
            if (spawnDirection[i] == "up")
            {
                // Sebelumnya X = 4, sekarang dipindah ke X = -4
                upTarget = Instantiate(targetPrefab, transform.position + new Vector3(-4, 0, 4), Quaternion.identity);
                nextUpTarget = Instantiate(targetPrefab, transform.position + new Vector3(4, 0, 12), Quaternion.identity);
            }

            if (spawnDirection[i] == "down")
            {
                // Sebelumnya X = -4, sekarang dipindah ke X = 4
                downTarget = Instantiate(targetPrefab, transform.position + new Vector3(4, 0, -4), Quaternion.identity);
                nextDownTarget = Instantiate(targetPrefab, transform.position + new Vector3(-4, 0, -12), Quaternion.identity);
            }

            if (spawnDirection[i] == "left")
            {
                // Sebelumnya Z = 4, sekarang dipindah ke Z = -4
                leftTarget = Instantiate(targetPrefab, transform.position + new Vector3(-4, 0, -4), Quaternion.identity);
                nextLeftTarget = Instantiate(targetPrefab, transform.position + new Vector3(-12, 0, 4), Quaternion.identity);
            }

            if (spawnDirection[i] == "right")
            {
                // Sebelumnya Z = -4, sekarang dipindah ke Z = 4
                rightTarget = Instantiate(targetPrefab, transform.position + new Vector3(4, 0, 4), Quaternion.identity);
                nextRightTarget = Instantiate(targetPrefab, transform.position + new Vector3(12, 0, -4), Quaternion.identity);
            }
        }

        // ===============================
        // HUBUNGKAN TARGET KE NEXT TARGET
        // ===============================

        if (upTarget != null)
        {
            SetNextTargets(upTarget, nextDownTarget, nextLeftTarget, nextRightTarget);
        }

        if (downTarget != null)
        {
            SetNextTargets(downTarget, nextUpTarget, nextLeftTarget, nextRightTarget);
        }

        if (leftTarget != null)
        {
            SetNextTargets(leftTarget, nextUpTarget, nextDownTarget, nextRightTarget);
        }

        if (rightTarget != null)
        {
            SetNextTargets(rightTarget, nextUpTarget, nextDownTarget, nextLeftTarget);
        }

        // ===============================
        // SPAWN RED LIGHT DI JALUR SEBELAHNYA
        // ===============================

        redLightBlock = new GameObject[spawnDirection.Length];

        for (int i = 0; i < spawnDirection.Length; i++)
        {
            if (spawnDirection[i] == "up")
            {
                // Sebelumnya X = 4, sekarang X = -4
                upRedLight = Instantiate(redlightBlockPrefab, transform.position + new Vector3(-4, 0, 8), Quaternion.identity);
                redLightBlock[i] = upRedLight;
            }

            if (spawnDirection[i] == "down")
            {
                // Sebelumnya X = -4, sekarang X = 4
                downRedLight = Instantiate(redlightBlockPrefab, transform.position + new Vector3(4, 0, -8), Quaternion.identity);
                redLightBlock[i] = downRedLight;
            }

            if (spawnDirection[i] == "left")
            {
                // Sebelumnya Z = 4, sekarang Z = -4
                leftRedLight = Instantiate(redlightBlockPrefab, transform.position + new Vector3(-8, 0, -4), Quaternion.identity);
                leftRedLight.transform.eulerAngles = new Vector3(0, 90, 0);
                redLightBlock[i] = leftRedLight;
            }

            if (spawnDirection[i] == "right")
            {
                // Sebelumnya Z = -4, sekarang Z = 4
                rightRedLight = Instantiate(redlightBlockPrefab, transform.position + new Vector3(8, 0, 4), Quaternion.identity);
                rightRedLight.transform.eulerAngles = new Vector3(0, 90, 0);
                redLightBlock[i] = rightRedLight;
            }
        }

        // Awal-awal semua red light aktif dulu, kecuali index pertama
        for (int i = 0; i < redLightBlock.Length; i++)
        {
            if (redLightBlock[i] != null)
            {
                redLightBlock[i].SetActive(i != indexGreenLight);
            }
        }
    }

    void Update()
    {
        timerRedLight -= Time.deltaTime;

        if (timerRedLight < 0)
        {
            timerRedLight = maxTimerRedLight;

            indexGreenLight++;

            if (indexGreenLight >= spawnDirection.Length)
            {
                indexGreenLight = 0;
            }

            for (int i = 0; i < spawnDirection.Length; i++)
            {
                if (redLightBlock[i] == null) continue;

                if (i == indexGreenLight)
                {
                    redLightBlock[i].SetActive(false);
                }
                else
                {
                    redLightBlock[i].SetActive(true);
                }
            }
        }
    }

    void SetNextTargets(GameObject targetObject, params GameObject[] nextObjects)
    {
        VehicleTarget target = targetObject.GetComponent<VehicleTarget>();

        int count = 0;

        for (int i = 0; i < nextObjects.Length; i++)
        {
            if (nextObjects[i] != null)
            {
                count++;
            }
        }

        target.nextTarget = new VehicleTarget[count];

        int index = 0;

        for (int i = 0; i < nextObjects.Length; i++)
        {
            if (nextObjects[i] != null)
            {
                target.nextTarget[index] = nextObjects[i].GetComponent<VehicleTarget>();
                index++;
            }
        }
    }
}