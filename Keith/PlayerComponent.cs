using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

public struct PlayerComponent : IComponentData
{
    public int direction;
    //public fixed bool heightMap[10000];
}

// public struct WorldComponent : ISharedComponentData,  IEquatable<WorldComponent>
// {
//     public bool isPlayer;
//     //public int[,] heightMap;
//     public bool Equals(WorldComponent other)
//     {
//         return isPlayer == other.isPlayer;
//     }
//     public override int GetHashCode()
//     {
//         int hash = 0;
//         //if (!ReferenceEquals(heightMap, null)) hash ^= heightMap.GetHashCode();
//         hash ^= isPlayer.GetHashCode();
//         return hash;
//     }
// }
