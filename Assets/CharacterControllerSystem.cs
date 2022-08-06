using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

public class CharacterControllerSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        float inputY = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        var jobHandle = Entities
            .WithName("CharacterControllerSystem")
            .ForEach((ref PhysicsVelocity physics, ref Rotation rotation, ref CharacterData player) =>
            {
                if (inputZ == 0)
                    physics.Linear = float3.zero;
                else
                    physics.Linear += inputZ * deltaTime * player.speed * math.forward(rotation.Value);

                //physics.Angular += new float3(0, inputY * 0.1f, 0);
                rotation.Value = math.mul(math.normalize(rotation.Value), 
                            quaternion.AxisAngle(math.up(), deltaTime * inputY));
            })
            .Schedule(inputDeps);

        jobHandle.Complete();


        return inputDeps;
    }
}
