using Unity.Entities;

public struct Rotate : IComponentData
{
    public float speed;

    public static Rotate Default
    {
        get
        {
            var rotate = new Rotate
            {
                speed = 1.0f
            };

            return rotate;
        }
    }
}
