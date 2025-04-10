/*****************************************************************************
// File Name : ParryScript.cs
// Author : Nicholas Williams
// Creation Date : March 27, 2025
//
// Brief Description : This is a script that detects collision when punching and calls the correct methods in
the player controller depending on what is hit.
*****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryScript : MonoBehaviour
{
    /// <summary>
    /// Detect what is punched and call the corresponding methods
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //Bounce up if crystal is airborne
        //gives an error, but it doesnt affect gameplay

        //If jump crystal is punched
        if (other.gameObject.tag == "Jump Crystal")
        {
            Destroy(other.gameObject);
            FindObjectOfType<PlayerController>().Parry();
            FindObjectOfType<PlayerController>().AddJumps();
            if (other.GetComponent<CrystalScript>().airborne)
                FindObjectOfType<PlayerController>().BounceParry();
        }

        //If dash crystal is punched
        else if (other.gameObject.tag == "Dash Crystal")
        {
            Destroy(other.gameObject);
            FindObjectOfType<PlayerController>().Parry();
            FindObjectOfType<PlayerController>().AddDashes();
            if (other.GetComponent<CrystalScript>().airborne)
                FindObjectOfType<PlayerController>().BounceParry();
        }

        //If ground is punched
        //(see GravityParry() for further explanation)
        else if (other.gameObject.tag == "Ground")
            FindObjectOfType<PlayerController>().GravityParry();
    }
}
