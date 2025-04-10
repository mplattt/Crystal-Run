/*****************************************************************************
// File Name : MonkeyBarScript.cs
// Author : Nicholas Williams
// Creation Date : March 31, 2025
//
// Brief Description : This is a script that swings the player when they collide with a monkey bar.
*****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyBarScript : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Rigidbody rb;
    [SerializeField] private float swingSpeed;

    /// <summary>
    /// Initializes the rigidbody
    /// </summary>
    private void Start()
    {
        rb = player.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Call swing enum when hitting a monkey bar
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Monkey Bar")
        {
            StartCoroutine(Swing());
        }
    }


    private IEnumerator dashSwing()
    {
        FindObjectOfType<PlayerController>().resetDash = false;
        yield return new WaitForSeconds(.4f);
        FindObjectOfType<PlayerController>().resetDash = true;
    }
    /// <summary>
    /// Manipulated vertical forces to makae it seem like the player is swinging
    /// </summary>
    /// <returns></returns>
    private IEnumerator Swing()
    {
        StartCoroutine(dashSwing());
        rb.velocity = new Vector3(rb.velocity.x, -8f, rb.velocity.z);
        while (rb.velocity.y < 4f)
        {
            rb.AddForce(0, swingSpeed, 0);
            yield return new WaitForSeconds(.1f);
        }
    }
}
