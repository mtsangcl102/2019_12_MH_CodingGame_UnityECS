using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

public class MoverSystem : JobComponentSystem{
//public class MoverSystem : ComponentSystem {
    EntityCommandBufferSystem m_EndFrameBarrier;

    protected override void OnCreate() {
        // Cache the EndFrameBarrier in a field, so we don't have to get it every frame
        m_EndFrameBarrier = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var job = new MoveJob {
            randomSeed = new Unity.Mathematics.Random((uint) UnityEngine.Random.Range(1, 100000)),
            deltaTime = Time.deltaTime,
            commandBuffer = m_EndFrameBarrier.CreateCommandBuffer()
        }.ScheduleSingle(this, inputDeps);

        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EndFrameBarrier.AddJobHandleForProducer(job);

        return job;
    }
    [BurstCompile]
    public struct MoveJob : IJobForEachWithEntity<MoveComponent, Translation> {
        public float deltaTime;
        public Unity.Mathematics.Random randomSeed;
        private int _directionVal;
        private float _displacement;
        private int _steps;
        private float _moveSteps;
        public EntityCommandBuffer commandBuffer;

        public void Execute(Entity entity, int i, ref MoveComponent moveComponent, ref Translation translation) {
            _displacement = 0f;
            _steps = 0;
            _moveSteps = 0f;
            if(moveComponent.isSuddenChange) {
                _directionVal = randomSeed.NextInt(0, 4);
                moveComponent.direction = (MoveComponent.DIRECTION) _directionVal;
                moveComponent.isSuddenChange = false;
            }
            _displacement = moveComponent.moveSpeed * deltaTime;

            _steps = math.max(1, (int) math.round(math.length(_displacement) / 0.05f));
            _moveSteps = _displacement / _steps;
            for(int s = 0; s < _steps; s++) {
                switch(moveComponent.direction) {
                    case MoveComponent.DIRECTION.RIGHT: //right
                    translation.Value.x += _moveSteps;
                    break;
                    case MoveComponent.DIRECTION.LEFT: //left
                    translation.Value.x -= _moveSteps;
                    break;
                    case MoveComponent.DIRECTION.UP: //up
                    translation.Value.z += _moveSteps;
                    break;
                    case MoveComponent.DIRECTION.DOWN: //down
                    translation.Value.z -= _moveSteps;
                    break;
                    default:
                    break;
                }
            }

            if(translation.Value.x > 100f) {
                _directionVal = 1;
                moveComponent.direction = (MoveComponent.DIRECTION) _directionVal;
            }
            if(translation.Value.x < 0f) {
                _directionVal = 0;
                moveComponent.direction = (MoveComponent.DIRECTION) _directionVal;
            }

            if(translation.Value.z > 100f) {
                _directionVal = 3;
                moveComponent.direction = (MoveComponent.DIRECTION) _directionVal;
            }
            if(translation.Value.z < 0f) {
                _directionVal = 2;
                moveComponent.direction = (MoveComponent.DIRECTION) _directionVal;
            }
        }

    }
}
