using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class AABBCollisionSystem : JobComponentSystem {
    //ArchetypeChunkComponentType<AABBComponent> AABBQuery;
    EntityQuery query;
    EntityQueryDesc queryDesc;

    EntityQuery translationQuery;

    EntityCommandBufferSystem m_EndFrameBarrier;

    protected override void OnCreate() {
        m_EndFrameBarrier = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        queryDesc = new EntityQueryDesc() {
            All = new ComponentType[] { typeof(AABBComponent), typeof(Translation), typeof(MoveComponent) },
        };
        query = GetEntityQuery(queryDesc);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        //ArchetypeChunkComponentType<AABBComponent> collisionTypeRW = GetArchetypeChunkComponentType<AABBComponent>();

        NativeArray<AABBComponent> _colliders = query.ToComponentDataArray<AABBComponent>(Allocator.TempJob);
        NativeArray<Translation> _positions = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<MoveComponent> _moveComponents = query.ToComponentDataArray<MoveComponent>(Allocator.TempJob);


        var collisionJob = new CollisionJob {
            //chunkComponentType = collisionTypeRW,
            colliders = _colliders,
            positions = _positions,
            moveComponents = _moveComponents,
            commandBuffer = m_EndFrameBarrier.CreateCommandBuffer().ToConcurrent()
        };

        var collisionJobHandle = collisionJob.Schedule(query);
        collisionJobHandle.Complete();
        return collisionJobHandle;
    }

    [BurstCompile]
    public struct CollisionJob : IJobForEachWithEntity<AABBComponent, Translation, MoveComponent> {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<AABBComponent> colliders;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> positions;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<MoveComponent> moveComponents;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref AABBComponent aabbComponent, ref Translation translation, ref MoveComponent moveComponent) {
            for(int i = 0; i < colliders.Length; i++) {
                if(!moveComponent.isObstacle) {
                    if(colliders[i].status == 2) {
                        if(index != i) {
                            if(Intersect(translation, positions[i], aabbComponent.bound, colliders[i].bound)) {
                                commandBuffer.DestroyEntity(index, entity);
                            }
                        }

                    } else if(colliders[i].status == 1 && moveComponents[i].isObstacle) {
                        if(Intersect(translation, positions[i], aabbComponent.bound, colliders[i].bound)) {
                            moveComponent.isSuddenChange = true;
                        }
                    }
                }
            }
        }

        public bool Intersect(Translation box1, Translation box2, float box1Bound, float box2Bound) {
            //return math.abs(box1.Value.x - box2.Value.x) < (box1Bound + box2Bound) ||
                //math.abs(box1.Value.y - box2.Value.y) < (box1Bound + box2Bound) ||
                //math.abs(box1.Value.z - box2.Value.z) < (box1Bound + box2Bound);

            //AABB
            return (box1.Value.x - box1Bound <= box2.Value.x + box2Bound && box1.Value.x + box1Bound >= box2.Value.x - box2Bound) &&
            (box1.Value.y - box1Bound <= box2.Value.y + box2Bound && box1.Value.y + box1Bound >= box2.Value.y - box2Bound) &&
            (box1.Value.z - box1Bound <= box2.Value.z + box2Bound && box1.Value.z + box1Bound >= box2.Value.z - box2Bound);
        }
    }
    //[BurstCompile]
    //public struct CollisionJob : IJobChunk {
    //    public ArchetypeChunkComponentType<AABBComponent> chunkComponentType;
    //    [ReadOnly] public NativeArray<AABBComponent> colliders;

    //    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
    //            NativeArray<AABBComponent> entities = chunk.GetNativeArray<AABBComponent>(chunkComponentType);
    //        for(int j = chunkIndex + 1; j < entities.Length; j++) {
    //            if(entities[chunkIndex].status == 1) {
    //                Debug.Log(":BEFORE:::entities.Length:::" + entities.Length);

    //                Debug.Log(":BEFORE:::chunk:::" + chunk);
    //                Debug.Log(":BEFORE:::chunkIndex:::" + chunkIndex);
    //                Debug.Log(":BEFORE:::firstEntityIndex:::" + firstEntityIndex);
    //            }
    //            if(entities[chunkIndex].status == 2) { 
    //                Debug.Log(":AFTER:::status:::" + entities[chunkIndex].status);
    //                if(Intersect(entities[chunkIndex], entities[j])) {
    //                }
    //            }
    //        }
    //    }
    //public bool Intersect(AABBComponent box1, AABBComponent box2) {
    //    return (box1.min.x <= box2.max.x && box1.max.x >= box2.min.x) &&
    //    (box1.min.y <= box2.max.y && box1.max.y >= box2.min.y) &&
    //    (box1.min.z <= box2.max.z && box1.max.z >= box2.min.z);
    //}
    //}
}