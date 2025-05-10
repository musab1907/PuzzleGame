using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public float speed;
    public DynamicJoystick dynamicJoystick;
    public Rigidbody rb;
    public MazeGenerator mazeGenerator;

    private void Start()
    {
        rb.drag = 3f;
        rb.angularDrag = 2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (mazeGenerator == null)
        {
            mazeGenerator = FindObjectOfType<MazeGenerator>();
        }

        if (mazeGenerator != null)
        {
            dynamicJoystick = mazeGenerator.dynamicJoystick;
        }
        else
        {
            Debug.LogError("MazeGenerator sahnede bulunamadı!");
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(dynamicJoystick.Horizontal, dynamicJoystick.Vertical);

        if (input.magnitude > 0.1f)
        {
            Vector3 moveDir = new Vector3(input.x, 0, input.y);
            rb.AddForce(moveDir.normalized * speed, ForceMode.Force);
        }
        // Joystick bırakıldığında hiçbir şey yapma; sürtünme doğal yavaşlatacak
    }
}