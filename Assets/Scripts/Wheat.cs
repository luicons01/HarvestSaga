using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Gestisce un singolo fascio di grano
/// </summary>
public class Wheat : MonoBehaviour
{
    /// <summary>
    /// Il Collider trigger che rappresenta il grano
    /// </summary>
    [HideInInspector]
    public Collider wheatCollider;

    /// <summary>
    /// Un vettore che punta diritto fuori dal grano
    /// </summary>
    public Vector3 WheatUpVector
    {
        get
        {
            return wheatCollider.transform.up;
        }
    }

    /// <summary>
    /// Il centro del collider del grano
    /// </summary>
    public Vector3 WheatCenterPosition
    {
        get
        {
            return wheatCollider.transform.position;
        }
    }

    private List<Material> wheatMaterials = new List<Material>();

    void Awake()
    {
        // Recupera il collider già presente sull'oggetto
        wheatCollider = GetComponent<Collider>();

        // Ottieni tutti i MeshRenderer dai figli
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Recupera i materiali da tutti i MeshRenderer trovati
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            wheatMaterials.Add(meshRenderer.material);
        }
    }

    /// <summary>
    /// Mietere il grano
    /// </summary>
    /// <returns>Il quantitativo di grano raccolto</returns>
    public int Harvest()
    {
        //Disattiva il grano ed il suo collider
        wheatCollider.gameObject.SetActive(false);
        gameObject.SetActive(false);
        return 1;
    }

    /// <summary>
    /// Resetta il grano per il training
    /// </summary>
    public void ResetWheat(){

        wheatCollider.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

}
