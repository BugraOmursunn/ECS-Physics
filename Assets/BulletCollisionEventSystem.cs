using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class BulletCollisionEventSystem : JobComponentSystem
{
    BuildPhysicsWorld physWorld;
    StepPhysicsWorld stepWorld;

    protected override void OnCreate()
    {
        physWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    struct CollisionEventJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<BulletData> BulletGroup;
        public ComponentDataFromEntity<DestroyNowData> DestroyNowGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

            bool isTargetA = DestroyNowGroup.Exists(entityA);
            bool isTargetB = DestroyNowGroup.Exists(entityB);

            bool isBulletA = BulletGroup.Exists(entityA);
            bool isBulletB = BulletGroup.Exists(entityB);

            if (isBulletA && isTargetB)
            {
                var destroyComponent = DestroyNowGroup[entityB];
                destroyComponent.shouldDestroy = true;
                DestroyNowGroup[entityB] = destroyComponent;
            }

            if (isBulletB && isTargetA)
            {
                var destroyComponent = DestroyNowGroup[entityA];
                destroyComponent.shouldDestroy = true;
                DestroyNowGroup[entityA] = destroyComponent;
            }

        }
    
    
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new CollisionEventJob
        {
            BulletGroup = GetComponentDataFromEntity<BulletData>(),
            DestroyNowGroup = GetComponentDataFromEntity<DestroyNowData>()
        }.Schedule(stepWorld.Simulation, ref physWorld.PhysicsWorld, inputDeps);

        jobHandle.Complete();
        return jobHandle;
    }
}
