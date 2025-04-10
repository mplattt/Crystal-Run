/*****************************************************************************
// File Name : FallingObstacleScript.cs
// Author : Nicholas Williams
// Creation Date : March 31, 2025
//
// Brief Description : Script for environmental hazards, specifically objects that fall and kill the player
when walking underneath.
*****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingObstacleScript : MonoBehaviour
{
    private Transform initialPoint;
    private Rigidbody rb;

    /// <summary>
    /// Sets initial point, rigidbody, and freezes position
    /// </summary>
    void Start()
    {
        initialPoint = gameObject.transform;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePosition;
    }

    /// <summary>
    /// Resets to original position - not currently used
    /// </summary>
    private void Reset()
    {
        gameObject.transform.position = initialPoint.position;
        Start();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Drops the object
    /// </summary>
    private void Collapse()
    {
        rb.constraints = RigidbodyConstraints.None;
    }

    /// <summary>
    /// The object is disabled
    /// </summary>
    private void _Disable()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Calls collapse method when player walks underneath
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            Collapse();
    }

    /// <summary>
    /// Calls disable if it hits the ground, and kills the player if it hits the player
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
            _Disable();
        else if (collision.gameObject.tag == "Player")
            FindObjectOfType<PlayerController>().Kill();
    }
}
