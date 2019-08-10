using Unity.Entities;

public struct Dinosaur : IComponentData
{
    public DinosaurState dinosaurState;
    public float attackTime;
    public float timeSinceAttack;
}
