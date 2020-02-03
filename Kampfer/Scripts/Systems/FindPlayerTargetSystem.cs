using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodingGame
{
    public class FindPlayerTargetSystem : JobComponentSystem
    {
        private struct BlockEntityWithPosition
        {
            public int TranslationX;
            public int TranslationY;
        }
        
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }

        [RequireComponentTag(typeof(PlayerComponent))]
        [ExcludeComponent(typeof(TargetTranslationComponent))]
        private struct FindTargetJob : IJobForEachWithEntity<StartTranslationComponent>
        {
            [DeallocateOnJobCompletion] [ReadOnly] 
            public NativeArray<BlockEntityWithPosition> BlockArray;
            [ReadOnly] 
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public Random Rng;

            public void Execute(Entity entity, int index, [ReadOnly] ref StartTranslationComponent startTranslationComponent)
            {
                var closet1 = new int2(startTranslationComponent.X, 99);
                var closet2 = new int2(99, startTranslationComponent.Y);
                var closet3 = new int2(startTranslationComponent.X, 0);
                var closet4 = new int2(0, startTranslationComponent.Y);
                
                for (var i = 0; i < BlockArray.Length; i++)
                {
                    var block = BlockArray[i];
                    if (block.TranslationX == startTranslationComponent.X)
                    {
                        
                        if (block.TranslationY - 1 >= startTranslationComponent.Y && block.TranslationY-1 <= closet1.y)
                        {
                            closet1.y = block.TranslationY-1 ;
                        }
                        else if (block.TranslationY +1 <= startTranslationComponent.Y && block.TranslationY+1 >= closet3.y)
                        {
                            closet3.y = block.TranslationY +1;
                        }
                    }
                    else if (block.TranslationY == startTranslationComponent.Y)
                    {
                        if (block.TranslationX -1 >= startTranslationComponent.X && block.TranslationX-1  <= closet2.x)
                        {
                            closet2.x = block.TranslationX -1;
                        }
                        else if (block.TranslationX +1 <= startTranslationComponent.X && block.TranslationX+1  >= closet4.x)
                        {
                            closet4.x = block.TranslationX +1;
                        }
                    }
                }

                var startIndex = Rng.NextInt(0, 4);
                var upDown = Rng.NextInt(0, 2);

                var closet = new int2(startTranslationComponent.X, startTranslationComponent.Y);
                var direction = float2.zero;
                
                
                for (var i = 0; i < 4; i++)
                {
                    var thisIndex = startIndex;
                    thisIndex = (upDown == 0) ? thisIndex + i : thisIndex - i;
                    thisIndex = thisIndex < 0 ? 3 : thisIndex;
                    thisIndex = thisIndex > 3 ? 0 : thisIndex;
                    
                    int2 thisCloset;
                    float2 thisDirection;

                    switch (thisIndex)
                    {
                        case 0:
                            thisCloset = closet1;
                            thisDirection = new float2(0, 1);
                            break;
                        case 1:
                            thisCloset = closet2;
                            thisDirection = new float2(1, 0);
                            break;
                        case 2:
                            thisCloset = closet3;
                            thisDirection = new float2(0, -1);
                            break;
                        default:
                            thisCloset = closet4;
                            thisDirection = new float2(-1, 0);
                            break;
                    }

                    if (math.abs(math.distance(thisCloset, new int2(startTranslationComponent.X, startTranslationComponent.Y))) >= 1)
                    {
                        closet = thisCloset;
                        direction = thisDirection;
                        break;
                    }
                }

                if ((math.abs(direction.x) + math.abs(direction.y)) > 0f)
                {
                    EntityCommandBuffer.AddComponent(index, entity, new TargetTranslationComponent(){X = closet.x, Y = closet.y, Direction = direction} );
                    EntityCommandBuffer.RemoveComponent( index, entity, ComponentType.ReadOnly<StartTranslationComponent>());
                }

            }
        }



        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var  rng = new Random();
            rng.InitState((uint) System.DateTime.UtcNow.Ticks);
            
            var blockEntityQuery = GetEntityQuery(typeof(BlockComponent), ComponentType.ReadOnly<ObstacleComponent>());
            var blockComponentArray = blockEntityQuery.ToComponentDataArray<BlockComponent>(Allocator.TempJob);

            var blockArray = new NativeArray<BlockEntityWithPosition>(blockComponentArray.Length, Allocator.TempJob);

            for (var i = 0; i < blockComponentArray.Length; i++)
            {
                blockArray[i] = new BlockEntityWithPosition
                {
                    TranslationX = blockComponentArray[i].X,
                    TranslationY = blockComponentArray[i].Y,
                };
            }
            
            blockComponentArray.Dispose();

            var findTargetJob = new FindTargetJob
            {
                BlockArray = blockArray,
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Rng = rng,
            };
            
            var findTargetJobHandle = findTargetJob.Schedule(this, inputDeps);
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(findTargetJobHandle);

            return findTargetJobHandle;
        }
    }
}