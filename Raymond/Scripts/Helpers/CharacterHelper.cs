// Copyright (c) Mad Head Limited All Rights Reserved

using System;
using Unity.Mathematics;

namespace Helpers
{
    public static class CharacterHelper
    {
        public static float3 GetMoveVector( Direction direction )
        {
            switch ( direction )
            {
                case Direction.East: return new float3( 1f , 0f , 0f );
                case Direction.South: return new float3( 0f , 0f , -1f );
                case Direction.West: return new float3( -1f , 0f , 0f );
                case Direction.North: return new float3( 0f , 0f , 1f );
            }
            return float3.zero;
        }
    }
}