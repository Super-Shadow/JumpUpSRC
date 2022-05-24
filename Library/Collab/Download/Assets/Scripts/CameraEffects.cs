using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Script for some effects such as flashing the screen
 * and causing it to shake
*/

public class CameraEffects : MonoBehaviour
{
    private CameraController Cam { get => GetComponent<CameraController>(); }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    public IEnumerator Flash(float duration, float speed)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Cam.flashImage.color = new Color(255, 255, 255, Cam.flashImage.color.a + speed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        while (elapsed > 0)
        {
            Cam.flashImage.color = new Color(255, 255, 255, Cam.flashImage.color.a - speed);
            elapsed -= Time.deltaTime;
            yield return null;
        }
    }
}
