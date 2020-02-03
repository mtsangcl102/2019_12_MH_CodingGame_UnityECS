using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public struct StatusComponent : IComponentData{
    // status 0 == nothing
    // status 1 == terrain
    // status 2 == character
    public float status;
    //public float3 position;
}
