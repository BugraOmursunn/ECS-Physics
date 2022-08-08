using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class BoxTriggerEventSystem : JobComponentSystem
{
    BuildPhysicsWorld physWorld;
    StepPhysicsWorld stepWorld;

    protected override void OnCreate()
    {
        physWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new BoxTriggerJob
        {
            BoxTriggerDataGroup = GetComponentDataFromEntity<BoxTriggerData>(),
            PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>()
        }.Schedule(stepWorld.Simulation, ref physWorld.PhysicsWorld, inputDeps);

        return jobHandle;
    }

    struct BoxTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<BoxTriggerData> BoxTriggerDataGroup;
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            bool isBodyATrigger = BoxTriggerDataGroup.Exists(entityA);
            bool isBodyBTrigger = BoxTriggerDataGroup.Exists(entityB);

            if (isBodyATrigger && isBodyBTrigger) return;

            bool isBodyADynamic = PhysicsVelocityGroup.Exists(entityA);
            bool isBodyBDynamic = PhysicsVelocityGroup.Exists(entityB);

            if ((isBodyATrigger && !isBodyBDynamic) ||
                (isBodyBTrigger && !isBodyADynamic)) return;

            var triggerEntity = isBodyATrigger ? entityA : entityB;
            var dynamicEntity = isBodyATrigger ? entityB : entityA;

            var component = PhysicsVelocityGroup[dynamicEntity];
            component.Linear += new float3(0, 1000, 0);
            PhysicsVelocityGroup[dynamicEntity] = component;

        }

    }



}
