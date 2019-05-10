using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointScript : MonoBehaviour {

    public Material color;
    public GameObject WhatToSpawn;
    public Transform SpawnLocation;
    public bool AlreadyInUse;
    public float NetWorkID;

	void Start ()
    {
        AlreadyInUse = false;
        SpawnLocation = transform;
        NetWorkID = 0;
	}
}
