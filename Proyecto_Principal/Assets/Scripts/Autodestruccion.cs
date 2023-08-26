using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autodestruccion : MonoBehaviour
{
    public float TiempoDeVida = 0.1f; // Time en segundos

    private void Start()
    {
        // Invoke el métood Destruye pasado el TiempoDeVida
        Invoke("Destruye", TiempoDeVida);
    }

    private void Destruye() {Destroy(gameObject);} // Elimina el GameObject al que está asignado este script
}
