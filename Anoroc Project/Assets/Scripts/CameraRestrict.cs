using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraRestrict : MonoBehaviour
{
    public Transform player;
    public Tilemap world;

    public Camera cam;

    private Vector3 offset;


    public float speed = 1;

    private float aspectAfterSetup;
    //public float size;

    private Vector2 min;
    private Vector2 max;
    public Vector2 worldMinOffset = new Vector2();
    public Vector2 worldMaxOffset = new Vector2();
    // Start is called before the first frame update
    void Update()
    {
        Debug.Assert(player != null, "Player must be set!");
        Debug.Assert(world != null, "World must be set!");
        Debug.Assert(cam.orthographic, "Camera must be orthographic!");

        Vector3 newPosition = player.position + offset;
        newPosition.z = -10;

        newPosition.x = Mathf.Clamp(newPosition.x, min.x, max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, min.y, max.y);

        transform.position = Vector3.Lerp(transform.position, newPosition, speed);
    }

    private void Start()
    {
        world.CompressBounds();
        aspectAfterSetup = cam.aspect;

        var height = 2 * cam.orthographicSize;
        var width = height * aspectAfterSetup;


        min = worldMinOffset + new Vector2(
            world.origin.x + (width / 2),
            world.origin.y + (height / 2)
        );

        max = worldMaxOffset + new Vector2(
            world.origin.x + world.size.x - (width / 2),
            world.origin.y + world.size.y - (height / 2) 
        );
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCam(duration, magnitude));
    }


    IEnumerator ShakeCam(float duration, float magnitude)
    {
        Vector2 pos = cam.transform.localPosition;
        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            if (Time.timeScale == 0) continue;

            cam.transform.localPosition = new Vector2(
                Random.Range(-1, 1) * magnitude,
                Random.Range(-1, 1) * magnitude
            );

            yield return null;
        }

        cam.transform.localPosition = pos;
    }
}
