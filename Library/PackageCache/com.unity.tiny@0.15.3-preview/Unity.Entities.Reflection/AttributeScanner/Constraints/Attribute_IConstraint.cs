namespace Unity.Entities.Reflection
{
    /// <summary>
    /// Allows you to specify a type as a constraint query with one argument.
    /// </summary>
    /// <typeparam name="T">The type of the query's argument.</typeparam>
    public interface IConstraint<in T>
    {
        bool SatisfiesConstraint(T t);
    }

    /// <summary>
    /// Allows you to specify a type as a constraint query with two arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the query's first argument.</typeparam>
    /// <typeparam name="T2">The type of the query's second argument.</typeparam>
    public interface IConstraint<in T1, in T2>
    {
        bool SatisfiesConstraint(T1 t1, T2 t2);
    }
}
