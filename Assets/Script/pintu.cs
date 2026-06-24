using UnityEngine;


public class AutoDoorTrigger : MonoBehaviour
{

    [Header("Door")]
    public Transform door;


    [Header("Rotation")]
    public Vector3 openRotation;

    public float openSpeed = 3f;



    [Header("Control")]
    public bool doorActive = false;



    [Header("Scene")]
    public string nextScene;



    private Quaternion closeRot;
    private Quaternion openRot;


    private bool opened = false;



    void Start()
    {

        closeRot = door.localRotation;


        openRot =
        closeRot *
        Quaternion.Euler(openRotation);


    }





    void Update()
    {


        if(opened)
        {

            door.localRotation =
            Quaternion.Lerp(
                door.localRotation,
                openRot,
                Time.deltaTime * openSpeed
            );

        }


    }





    public void ActivateDoor()
    {

        doorActive = true;


        Debug.Log("Pintu sudah aktif");

    }






    private void OnTriggerEnter(Collider other)
    {

        if(!doorActive)
        {
            Debug.Log("Pintu masih terkunci");
            return;
        }



        if(other.CompareTag("Player"))
        {

            opened = true;


            Debug.Log("Pintu terbuka");

        }


    }






    private void OnTriggerExit(Collider other)
    {

        if(!doorActive)
            return;



        if(other.CompareTag("Player"))
        {

            opened = false;


            Debug.Log("Pintu menutup");

        }


    }





    public void GoNextScene()
    {

        if(!doorActive)
            return;



        VRSceneTransition.Instance.LoadScene(nextScene);


    }



}