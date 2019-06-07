#include <stdio.h>
#include <stdint.h>
#include <memory>

#include "EntityWrappers.h"

using namespace Unity::Entities;

const Entity Entity::NONE = Entity{0, 0};

bindings::EntityManagerDelegates bindings::sEntityManagerDelegates = { 0 };

/*static*/ int ENTITIES_CALL
TypeManager::TypeIndexForStableTypeHash(uint64_t hash)
{
    return bindings::sEntityManagerDelegates.TypeIndexForStableTypeHash(hash);
}

extern "C" ENTITIES_EXPORT
void ENTITIES_CALL 
SetEntityManagerDelegates(bindings::EntityManagerDelegates delegates)
{
    bindings::sEntityManagerDelegates = delegates;
}

extern "C" ENTITIES_EXPORT
EntityManager* ENTITIES_CALL 
MakeEntityManagerWrapper(void* emptr)
{
    return new EntityManager(emptr);
}
