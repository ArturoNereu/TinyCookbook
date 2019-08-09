#pragma once
#include "EntityTypes.h"
#include "bind-Unity_Entities_CPlusPlus.h"
#include <string>

namespace Unity {
namespace Entities {

namespace bindings {
struct EntityManagerDelegates {
    void (ENTITIES_CALL *AddComponentRaw)(void*, Entity e, int cid);
    void (ENTITIES_CALL *RemoveComponentRaw)(void*, Entity e, int cid);
    bool (ENTITIES_CALL *HasComponentRaw)(void*, Entity e, int cid);
    uint8_t* (ENTITIES_CALL *GetComponentDataPtrRawRO)(void*, Entity e, int cid);
    uint8_t* (ENTITIES_CALL *GetComponentDataPtrRawRW)(void*, Entity e, int cid);
    Entity (ENTITIES_CALL *CreateEntity)(void*, Archetype arch);
    void (ENTITIES_CALL *DestroyEntities)(void*, Entity* ents, int count);
    Archetype (ENTITIES_CALL *CreateArchetypeRaw)(void*, int* cids, int count);
    int (ENTITIES_CALL *TypeIndexForStableTypeHash)(uint64_t hash);
    void* (ENTITIES_CALL *GetBufferElementDataPtrRawRO)(void*, Entity e, int cid);
    void* (ENTITIES_CALL *GetBufferElementDataPtrRawRW)(void*, Entity e, int cid);
    int (ENTITIES_CALL *GetBufferElementDataLength)(void*, Entity e, int cid);
};

extern ENTITIES_EXPORT EntityManagerDelegates sEntityManagerDelegates;
}

class EntityManagerRaw {
public:
    EntityManagerRaw(void* emptr)
        : mEmPtr(emptr)
    {
    }

    void AddComponentRaw(Entity e, int cid)
    {
        bindings::sEntityManagerDelegates.AddComponentRaw(mEmPtr, e, cid);
    }

    void RemoveComponentRaw(Entity e, int cid)
    {
        bindings::sEntityManagerDelegates.RemoveComponentRaw(mEmPtr, e, cid);
    }

    bool HasComponentRaw(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.HasComponentRaw(mEmPtr, e, cid);
    }

    uint8_t* GetComponentDataPtrRawRO(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.GetComponentDataPtrRawRO(mEmPtr, e, cid);
    }

    uint8_t* GetComponentDataPtrRawRW(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.GetComponentDataPtrRawRW(mEmPtr, e, cid);
    }

    Entity CreateEntity(Archetype arch)
    {
        return bindings::sEntityManagerDelegates.CreateEntity(mEmPtr, arch);
    }

    void DestroyEntity(Entity e)
    {
        bindings::sEntityManagerDelegates.DestroyEntities(mEmPtr, &e, 1);
    }

    void DestroyEntities(Entity* ents, int count)
    {
        bindings::sEntityManagerDelegates.DestroyEntities(mEmPtr, ents, count);
    }

    Archetype CreateArchetypeRaw(int* cids, int count)
    {
        return bindings::sEntityManagerDelegates.CreateArchetypeRaw(mEmPtr, cids, count);
    }

    void* GetBufferElementDataPtrRawRO(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.GetBufferElementDataPtrRawRO(mEmPtr, e, cid);
    }

    void* GetBufferElementDataPtrRawRW(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.GetBufferElementDataPtrRawRW(mEmPtr, e, cid);
    }

    int GetBufferElementDataLength(Entity e, int cid)
    {
        return bindings::sEntityManagerDelegates.GetBufferElementDataLength(mEmPtr, e, cid);
    }

protected:
    void* mEmPtr;
};

class EntityManager : public EntityManagerRaw
{
public:
    EntityManager(void* emptr)
        : EntityManagerRaw(emptr)
    {
    }

    template<typename CT>
    void addComponent(Entity e)
    {
        AddComponentRaw(e, ComponentId<CT>());
    }

    template<typename CT>
    void removeComponent(Entity e)
    {
        RemoveComponentRaw(e, ComponentId<CT>());
    }

    template<typename CT>
    bool hasComponent(Entity e)
    {
        return HasComponentRaw(e, ComponentId<CT>());
    }

    template<typename CT>
    CT getComponentData(Entity e)
    {
        return *(CT*)GetComponentDataPtrRawRO(e, ComponentId<CT>());
    }

    template<typename CT>
    const CT* getComponentPtrConstUnsafe(Entity e)
    {
        return (const CT*)GetComponentDataPtrRawRO(e, ComponentId<CT>());
    }

    template<typename CT>
    CT* getComponentPtrUnsafe(Entity e)
    {
        return (CT*)GetComponentDataPtrRawRW(e, ComponentId<CT>());
    }


    template<typename... CTs>
    Archetype CreateArchetype()
    {
        const auto count = sizeof...(CTs);
        ComponentTypeId cids[] = { ComponentId<CTs>()... };
        return CreateArchetypeRaw(cids, count);
    }

    template<typename CT>
    const void* getBufferElementDataPtrConstUnsafe(Entity e)
    {
        return (const void*)GetBufferElementDataPtrRawRO(e, ComponentId<CT>());
    }

    template<typename CT>
    void* getBufferElementDataPtrUnsafe(Entity e)
    {
        return (void*)GetBufferElementDataPtrRawRW(e, ComponentId<CT>());
    }

    template<typename CT>
    int getBufferElementDataLength(Entity e)
    {
        return GetBufferElementDataLength(e, ComponentId<CT>());
    }

    std::string formatEntity(Entity e) {
        std::string str = "Entity Index: ";
        str += e.index;
        str += " Version: ";
        str += e.version;
        return str;
    }

    long long getHashCode(Entity e) {
        return (static_cast<long long> (e.index) << 32) + e.version;
    }
protected:
};

}
}
