using UnityEngine;


public class DoorEnterScene : MonoBehaviour
{

    public AutoDoorTrigger door;

    [Header("Control")]
    public bool doorActive = false;



    private void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("Player"))
        {

            door.GoNextScene();

        }

    }


        public void ActivateDoor()
    {

        doorActive = true;


        Debug.Log("Pintu sudah aktif");

    }
}