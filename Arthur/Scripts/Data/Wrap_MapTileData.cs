using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

[Serializable]
public struct Dat_MapTileData : IComponentData
{
    public int id;
    public bool isObstacle;
}

class Wrap_MapTileData : ComponentDataProxy<Dat_MapTileData> { }
