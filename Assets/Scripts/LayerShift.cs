using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerShift : MonoBehaviour
{

    // The target position for the camera, and player to move
    [SerializeField] private int PrimaryID;
    [SerializeField] private int SecondaryID;
    [SerializeField] private bool oneTime;

    // If triggered, compare tag, if player then execute transition
    // If one-time transition, then remove event.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gameMan = GameManager.Inst;
        if (!collision.CompareTag("Player")) 
            return;
        // For now this should work, 
        // could provide implementation later for horizontal camera transition in which case
        // we would record the actual side of the trigger box the player entered from
        // then pick the corrosponding opposing destination.

        StartCoroutine(collision.transform.position.y > transform.position.y
            ? gameMan.GameCamera.LayerTransition(gameMan.CameraPos(PrimaryID).position)
            : gameMan.GameCamera.LayerTransition(gameMan.CameraPos(SecondaryID).position));

        if (oneTime)
        {
            Destroy(this.gameObject, 2f);
        }
    }
}
