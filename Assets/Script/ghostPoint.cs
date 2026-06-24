using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using HPhysic;

public class LanGhostPreview : MonoBehaviour
{
    public Connector femaleConnector;

    [Header("Ghost Point")]
    public Transform ghostPoint;


    [Header("Detection")]
    public float detectRadius = 0.15f;


    private GameObject ghost;
    private GameObject marker;


    private XRGrabInteractable grabbedCable;

    private Renderer markerRenderer;


    Color normalMarker = Color.cyan;



    void Start()
    {

        if(femaleConnector == null)
            femaleConnector = GetComponent<Connector>();


        if(ghostPoint == null)
            ghostPoint = transform;

    }




    void Update()
    {

        grabbedCable = FindGrabbedCable();



        // TIDAK ADA KABEL DIPEGANG
        if(grabbedCable == null)
        {

            DestroyMarker();

            DestroyGhost();

            return;

        }



        // BARU BUAT MARKER SAAT GRAB
        CreateMarker();



        Connector male =
        grabbedCable.GetComponent<Connector>();


        if(male == null)
            return;




        if(femaleConnector.CanConnect(male))
        {

            ShowGhost(male);

            ChangeMarkerColor(male);

        }
        else
        {

            ResetMarkerColor();

            DestroyGhost();

        }




        if(femaleConnector.IsConnected)
        {

            DestroyMarker();

            DestroyGhost();

        }


    }







    XRGrabInteractable FindGrabbedCable()
    {


        XRGrabInteractable[] all =
        FindObjectsOfType<XRGrabInteractable>();



        foreach(var item in all)
        {


            // INI YANG BEDAKAN
            if(item.interactorsSelecting.Count == 0)
                continue;



            Connector c =
            item.GetComponent<Connector>();



            if(c == null)
                continue;



            if(c.ConnectionType != Connector.ConType.Male)
                continue;



            return item;

        }



        return null;

    }









    void CreateMarker()
    {

        if(marker != null)
            return;



        marker =
        GameObject.CreatePrimitive(
            PrimitiveType.Sphere
        );



        marker.name =
        "Grab_Marker";



        marker.transform.SetParent(
            ghostPoint
        );



        marker.transform.localPosition =
        new Vector3(0,0,0.08f);



        marker.transform.localScale =
        Vector3.one * 0.15f;



        Destroy(
            marker.GetComponent<Collider>()
        );



        markerRenderer =
        marker.GetComponent<Renderer>();



        markerRenderer.material.color =
        normalMarker;


    }







    void ChangeMarkerColor(Connector male)
    {


        if(markerRenderer == null)
            return;



        Renderer r =
        male.GetComponentInChildren<Renderer>();



        if(r != null)
        {

            markerRenderer.material.color =
            r.material.color;

        }


    }






    void ResetMarkerColor()
    {

        if(markerRenderer == null)
            return;


        markerRenderer.material.color =
        normalMarker;

    }







    void DestroyMarker()
    {

        if(marker != null)
        {

            Destroy(marker);

            marker = null;

            markerRenderer = null;

        }

    }









    void ShowGhost(Connector male)
    {

        if(ghost != null)
            return;



        ghost =
        Instantiate(
            male.gameObject
        );



        ghost.name =
        "GhostPreview";



        Connector gc =
        ghost.GetComponent<Connector>();



        if(gc != null)
        {

            ghost.transform.rotation =
            ghostPoint.rotation *
            gc.RotationOffset;



            Vector3 offset =
            gc.ConnectionPosition -
            ghost.transform.position;



            ghost.transform.position =
            ghostPoint.position -
            offset;

        }





        foreach(Collider c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;



        Rigidbody rb =
        ghost.GetComponent<Rigidbody>();


        if(rb != null)
        {

            rb.isKinematic = true;

            rb.useGravity = false;

        }





        foreach(MonoBehaviour m in ghost.GetComponents<MonoBehaviour>())
            m.enabled = false;


    }






    void DestroyGhost()
    {

        if(ghost != null)
        {

            Destroy(ghost);

            ghost = null;

        }

    }

}