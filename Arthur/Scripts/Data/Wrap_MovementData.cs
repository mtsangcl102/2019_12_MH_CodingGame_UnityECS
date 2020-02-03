using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
public struct Dat_MovementData : IComponentData
{
    
    public float2 dir;
    public float speed;
}

class Wrap_MovementData : ComponentDataProxy<Dat_MovementData> { }
