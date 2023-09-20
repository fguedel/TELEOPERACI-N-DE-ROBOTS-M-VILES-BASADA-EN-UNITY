
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
    public GameObject indicadorMov;
    public GameObject indicadorGiro;
    public GameObject robot; // Para posicionar la flecha sobre el.
    public float velGiro = 200f; // Adjust the speed of rotation here
    public float velCrecer = 5f; // Adjust the speed of scaling here
    Quaternion targetRotation; // Cache the target rotation
    Vector3 originalScale; // Cache the original scale

    // Generador de puntos.
    public GameObject generadorPuntos;

    private void Awake()
    {
        // Sustituye el GameObject por las instancias de los indicadores.
        indicadorMov = Instantiate(indicadorMov, Vector3.zero, Quaternion.identity);
        indicadorGiro = Instantiate(indicadorGiro, Vector3.zero, Quaternion.identity);

        originalScale = indicadorMov.transform.localScale;

        input = new InputMaster();
        input.Robot0.Movimiento.performed += ctx => {
            currentMov = ctx.ReadValue<Vector3>();
            movementPressed = currentMov.x != 0 || currentMov.z != 0;
            };

        if (movementPressed)
            targetRotation = Quaternion.Euler(robot.transform.rotation.eulerAngles + Vector3.up * currentMov.x * 45);
    }

    void Update()
    {
        if (movementPressed)
        {
            indicadorMov.SetActive(true);
            ActualizaFlecha();
            IndicaRotacion();
        }
        else
        {
            indicadorMov.SetActive(false);
            indicadorGiro.SetActive(false);
            indicadorMov.transform.localScale = originalScale;
        }
    }

    void ActualizaFlecha()
    {
        // Posiciona la flecha sobre el robot.
        Vector3 posActualizada = robot.transform.position + Vector3.up * 1;
        // Orienta la flecha según el robot y si se ordena giro la rota ligeramente.
        Vector3 rotActualizada = robot.transform.rotation.eulerAngles;
        rotActualizada.y += currentMov.x*45;
        // Cuando se ordene avanzar, la flecha se estirará en la dirección deseada.
        float targetScale = currentMov.z; // Target scale.

        // Posicionamiento inmediato sobre el robot.
        indicadorMov.transform.position = posActualizada;
        // Rotación gradual del objeto hacia la última dirección.
        indicadorMov.transform.rotation = Quaternion.RotateTowards(indicadorMov.transform.rotation, Quaternion.Euler(rotActualizada), Time.deltaTime * velGiro);
        // Escalado gradual del objeto en la coordenada Z hasta el valor máximo.
        indicadorMov.transform.localScale = Vector3.Lerp(indicadorMov.transform.localScale, new Vector3(0.2f, 0.2f, targetScale + 0.2f), Time.deltaTime * velCrecer);
    }

    void IndicaRotacion()
    {
        // Posiciona las flechas rotatorias sobre el robot.
        Vector3 posActualizada = robot.transform.position + Vector3.up * 0.7f;
        indicadorGiro.transform.position = posActualizada;

        // Si se ha pulsado giro (se considera +-0.1 como ruido), el disco rota en ese sentido.
        if(Mathf.Abs(currentMov.x) > 0.1f)
        {
            indicadorGiro.SetActive(true);
            // Rotación gradual del indicador de giro.
            indicadorGiro.transform.Rotate(Vector3.up, Time.deltaTime * velGiro * currentMov.x);
        }
        else
        {
            indicadorGiro.SetActive(false);
        }
    }
}
