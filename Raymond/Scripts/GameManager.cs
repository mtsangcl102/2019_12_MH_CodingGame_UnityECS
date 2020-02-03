using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

public class GameManager : MonoBehaviour
{
    public Mesh Mesh;
    public Material Material;
    public Material[] Materials;

    public Mesh CharacterMesh;
    public Material[] CharacterMaterials;

    public int CharacterLimit;
    
    public int ObstaclePercentage = 5;

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }
}