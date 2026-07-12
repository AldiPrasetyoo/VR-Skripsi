using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


namespace HPhysic
{

public class QuickSpawnItem : MonoBehaviour
{


    [Header("Input XRI")]
    public InputActionProperty spawnAction;



    [Header("Controller")]
    public Transform controllerTransform;

    public XRBaseInteractor interactor;

    public XRInteractionManager interactionManager;



    [Header("Spawn")]
    public float spawnDistance = 1.5f;




    [Header("Item yang keluar")]
    public GameObject itemPrefab;




    private bool pressed = false;




    void Update()
    {

        float input =
        spawnAction.action.ReadValue<float>();



        if(input > 0.5f && !pressed)
        {

            pressed = true;


            SpawnItem();


        }




        if(input <= 0.5f)
        {

            pressed = false;

        }



    }







    void SpawnItem()
    {


        GameObject obj =
        Instantiate(
            itemPrefab
        );



        obj.transform.position =
        controllerTransform.position +
        controllerTransform.forward *
        spawnDistance;



        obj.transform.rotation =
        controllerTransform.rotation;







        // cari grab START dulu
        XRGrabInteractable grab = null;



        Transform start =
        obj.transform.Find("Start");



        if(start != null)
        {

            grab =
            start.GetComponent<XRGrabInteractable>();

        }





        // kalau tidak ada start
        if(grab == null)
        {

            grab =
            obj.GetComponent<XRGrabInteractable>();

        }







        if(grab != null)
        {

            StartCoroutine(
                GrabNextFrame(grab)
            );


        }



    }







    System.Collections.IEnumerator GrabNextFrame(
    XRGrabInteractable grab
)
{
    yield return null;

    if (interactionManager == null || interactor == null || grab == null)
        yield break;

    if (interactor is IXRSelectInteractor selectInteractor &&
        grab is IXRSelectInteractable selectInteractable)
    {
        interactionManager.SelectEnter(
            selectInteractor,
            selectInteractable
        );
    }
}



}


}