
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Geometry;

public class ArrowVisualizator : MonoBehaviour
{
    // Gestor de InputSystem.
    InputMaster input;
    Vector3 currentMov;
    bool movementPressed;
    private void OnEnable() {input.Robot0.Enable();}
    private void OnDisable() {input.Robot0.Disable();}

    // Variables de la flecha.
    public GameObject flechaModel;
    public GameObject robot; // Para posicionar la flecha sobre el.
    public float velGiro = 200f; // Adjust the speed of rotation here
    public float velExpansion = 5f; // Adjust the speed of scaling here

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
        float targetScale = currentMov.z; // Target scale.
        // Escalado gradual del objeto en la coordenada Z hasta el valor máximo.
        flechaModel.transform.localScale = Vector3.Lerp(flechaModel.transform.localScale, new Vector3(0.2f, 0.2f, targetScale), Time.deltaTime * velExpansion);

        if (movementPressed)
        {
            flechaModel.SetActive(true);
            MovePrefab();
        }
        else
            flechaModel.SetActive(false);
    }

    void MovePrefab()
    {
        Vector3 posActualizada = robot.transform.position;
        posActualizada.y += 1;
        flechaModel.transform.position = posActualizada;

        Vector3 rotActualizada = robot.transform.rotation.eulerAngles;
        rotActualizada.y += currentMov.x*45;
        // Rotación gradual del objeto hacia la última dirección.
        flechaModel.transform.rotation = Quaternion.RotateTowards(flechaModel.transform.rotation, Quaternion.Euler(rotActualizada), Time.deltaTime * velGiro);
    }
}
