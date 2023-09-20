
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autodestruccion : MonoBehaviour
{
    public float tiempoDeVida = 0.1f; // Time en segundos.

    private void Start()
    {
        // Invoke el método Destruye pasado el TiempoDeVida.
        Invoke("Destruye", tiempoDeVida);
    }

    private void Destruye() {Destroy(gameObject);} // Elimina el GameObject al que está asignado este script.
}
