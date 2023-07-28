
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeMovementExample : MonoBehaviour
{
    public float speed = 5f;

    InputMaster input;
    Vector3 currentMov = Vector3.zero;
    bool movementPressed;

    private void OnEnable() {input.Robot0.Enable();}
    private void OnDisable() {input.Robot0.Disable();}

    private void Awake()
    {
        input = new InputMaster();
        input.Robot0.Movimiento.performed += ctx => {
            currentMov = ctx.ReadValue<Vector3>();
            movementPressed = currentMov.x != 0 || currentMov.z != 0;
            };
    }

    void Update()
    {
        handleMovement();
    }

    void handleMovement()
    {
        Vector3 movimiento = new Vector3(currentMov.x * speed * Time.deltaTime, 0, currentMov.z * speed * Time.deltaTime);
        transform.position = transform.position + movimiento;
        Debug.Log(transform.position);
    }
}
