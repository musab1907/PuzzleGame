using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public float speed;
    public FloatingJoystick floatingJoystick;
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
            floatingJoystick = mazeGenerator.floatingJoystick;
        }
        else
        {
            Debug.LogError("MazeGenerator sahnede bulunamadı!");
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(floatingJoystick.Horizontal, floatingJoystick.Vertical);

        if (input.magnitude > 0.1f)
        {
            rb.drag = 0f; // Aktif kontrol varsa, direnç olmasın
            Vector3 moveDir = new Vector3(input.x, 0, input.y);
            rb.AddForce(moveDir.normalized * speed, ForceMode.Force);
        }
        else
        {
            rb.drag = 3f; // Elini çektiysen doğal yavaşlama olsun
            Debug.Log("Çektii" + rb.drag);
            
        }
    }
}