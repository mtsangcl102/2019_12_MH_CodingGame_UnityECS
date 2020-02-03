// Copyright (c) Mad Head Limited All Rights Reserved

using System;
using Unity.Mathematics;

namespace Helpers
{
    public static class SceneHelper
    {
        public static bool IsCollide( float3 position1 , float3 position2 )
        {
            return Math.Abs( position1.x - position2.x ) < 1.0f &&
                   Math.Abs( position1.z - position2.z ) < 1.0f;
        }
    }
}