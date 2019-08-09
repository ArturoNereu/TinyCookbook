using System;
using System.Collections.Generic;

namespace Unity.Entities.Reflection
{
    public static class Constraints
    {
        /// <summary>
        /// Filters a source using a constraint.
        /// </summary>
        /// <param name="source">The source to filter.</param>
        /// <param name="constraint">The constraint to satisfy.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="T">The type that is filtered.</typeparam>
        /// <typeparam name="TConstraint">The constraint type.</typeparam>
        /// <returns>Instances that match the <see cref="IConstraint{T}"/>.</returns>
        public static IEnumerable<T> WithConstraint<T, TConstraint>(
            this IEnumerable<T> source,
            TConstraint constraint,
            Action<T> onMismatchCallback = null)
            where TConstraint : IConstraint<T>
        {
            if (null == onMismatchCallback)
            {
                return source.WithConstraintImpl(constraint);
            }
            return source.WithConstraintImpl(constraint, onMismatchCallback);
        }

        /// <summary>
        /// Filters a source using a constraint that can take one additional argument. For example, when using a `SubClassOf` constraint, you can pass the parent class as an additional argument.
        /// </summary>
        /// <param name="source">The source to filter.</param>
        /// <param name="constraint">The constraint to satisfy.</param>
        /// <param name="argument">The argument passed to the constraint.</param>
        /// <param name="onMismatchCallback">A callback called when a mismatch is detected.</param>
        /// <typeparam name="T">The type that is filtered.</typeparam>
        /// <typeparam name="TConstraint">The constraint type.</typeparam>
        /// <typeparam name="TArgument">The type of argument passed to the constraint.</typeparam>
        /// <returns>Instances that match the <see cref="IConstraint{T, TArgument}"/>.</returns>
        public static IEnumerable<T> WithConstraint<T, TConstraint, TArgument>(
            this IEnumerable<T> source,
            TConstraint constraint,
            TArgument argument,
            Action<T, TArgument> onMismatchCallback = null)
            where TConstraint : IConstraint<T, TArgument>
        {
            if (null == onMismatchCallback)
            {
                return source.WithConstraintImpl(constraint, argument);
            }
            return source.WithConstraintImpl(constraint, argument, onMismatchCallback);
        }

        private static IEnumerable<T> WithConstraintImpl<T, TConstraint>(
            this IEnumerable<T> source,
            TConstraint constraint)
            where TConstraint : IConstraint<T>
        {
            foreach (var attribute in source)
            {
                if (constraint.SatisfiesConstraint(attribute))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<T> WithConstraintImpl<T, TConstraint>(
            this IEnumerable<T> source,
            TConstraint constraint,
            Action<T> onMismatchCallback)
            where TConstraint : IConstraint<T>
        {
            foreach (var attribute in source)
            {
                if (constraint.SatisfiesConstraint(attribute))
                {
                    yield return attribute;
                }
                else
                {
                    onMismatchCallback.Invoke(attribute);
                }
            }
        }

        private static IEnumerable<T> WithConstraintImpl<T, TConstraint, TArgument>(
            this IEnumerable<T> source,
            TConstraint constraint, TArgument argument)
            where TConstraint : IConstraint<T, TArgument>
        {
            foreach (var attribute in source)
            {
                if (constraint.SatisfiesConstraint(attribute, argument))
                {
                    yield return attribute;
                }
            }
        }

        private static IEnumerable<T> WithConstraintImpl<T, TConstraint, TArgument>(
            this IEnumerable<T> source,
            TConstraint constraint, TArgument argument,
            Action<T, TArgument> onMismatchCallback)
            where TConstraint : IConstraint<T, TArgument>
        {
            foreach (var attribute in source)
            {
                if (constraint.SatisfiesConstraint(attribute, argument))
                {
                    yield return attribute;
                }
                else
                {
                    onMismatchCallback.Invoke(attribute, argument);
                }
            }
        }
    }
}
