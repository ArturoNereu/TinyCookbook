#ifndef UNION_TUPLE_H_
#define UNION_TUPLE_H_

//////////////////////////////////////////////////////////////////////////
/// Union with two types
template<typename A, typename B>
union UnionTuple
{
    typedef A first_type;
    typedef B second_type;

    A first;
    B second;
};

// A is specified last so it can be omitted and then deduced by the compiler from the argument
template<typename B, typename A>
inline UnionTuple<A, B>& AliasAs(A& a)
{
    CompileTimeAssert(sizeof(B) <= sizeof(A), "Aliasing type (A) with a larger one (B) is not allowed");
    return reinterpret_cast<UnionTuple<A, B>&>(a);
}

template<typename B, typename A>
inline UnionTuple<A, B> const& AliasAs(A const& a)
{
    CompileTimeAssert(sizeof(B) <= sizeof(A), "Aliasing type (A) with a larger one (B) is not allowed");
    return reinterpret_cast<UnionTuple<A, B> const&>(a);
}

// Use this metafunction together with UnionTuple<A, B>, for type B. It returns a type of same size
// that can be safely loaded into registers while data is in endianess-swapped form.
template<class T>
struct BitSafeTypeFor
{
    typedef T type;
};

template<>
struct BitSafeTypeFor<float>
{
    typedef UInt32 type;
};

template<>
struct BitSafeTypeFor<double>
{
    typedef UInt64 type;
};

#endif // UNION_TUPLE_H_
