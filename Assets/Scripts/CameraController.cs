using System; 
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

/* Script for handling camera movement, as well as a pass through
 * for calling effects such as screen shake, flashing ect.
*/

public class CameraController : MonoBehaviour
{
    public CameraEffects effects;
    public Image flashImage;

    protected bool transitioning;
    protected PlayerController player;

    private Camera camera;
    private bool waiting;
    private bool playerCamera;

    private float biggestY = Single.NegativeInfinity;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
            playerCamera = true;
        effects = GetComponent<CameraEffects>();
        camera = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (!playerCamera) 
            return;
        // Player above camera || Player below camera 
        if (player.transform.position.y < transform.position.y -
            camera.orthographicSize || player.transform.position.y > transform.position.y +
            camera.orthographicSize)
        {
            StartCoroutine(MoveCamera(player.transform.position.y));
        }
    }

    IEnumerator MoveCamera(float playerY)
    {
        // Player above camera 
        if (playerY > transform.position.y + camera.orthographicSize && !waiting)
        {
            if (transform.position.y + camera.orthographicSize > biggestY)
            {
                biggestY = transform.position.y + camera.orthographicSize+1;
                player.IncreaseScore(100);
            }
            waiting = true;
            yield return StartCoroutine(LayerTransition(new Vector3(transform.position.x, transform.position.y + camera.orthographicSize*2, transform.position.z)));
            yield return new WaitForSeconds(0.01f);
            waiting = false;
        }

        // Player below camera 
        if (playerY < transform.position.y - camera.orthographicSize && !waiting)
        {
            waiting = true;
            yield return StartCoroutine(LayerTransition(new Vector3(transform.position.x, transform.position.y - camera.orthographicSize*2, transform.position.z)));
            yield return new WaitForSeconds(0.01f);
            waiting = false;
        }
    }

    // Handling Camera Movement for Transitioning Layers
    // 1. This method is called from a trigger script,
    // 2. Transitioning is true, lerp towards location,
    // 3. Transitioning is false, re-enable player controller and physics,
    //    reinstate player momentum

    public IEnumerator LayerTransition(Vector3 cameraTarget, float transitionTime = 0.75f)
    {
        var currentPos = transform.position;
        var duration = 0f;
        while(duration < transitionTime)
        { 
            var t = duration / transitionTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // “ease out” with sinerp
            transform.position = Vector3.Lerp(currentPos, cameraTarget, t);
            duration += Time.deltaTime;


            yield return null;
        }

        transform.position = cameraTarget;
    }

}
