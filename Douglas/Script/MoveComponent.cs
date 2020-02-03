using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct MoveComponent : IComponentData {
    public enum DIRECTION { 
        RIGHT = 0,
        LEFT = 1,
        UP = 2,
        DOWN = 3
    }
    public float moveSpeed;
    public DIRECTION direction;
    public bool isSuddenChange;
    public bool isObstacle;
}