// the global heap
declare var HEAP32: any;
declare var HEAP16: any;
declare var HEAP8: any;

declare namespace ut {
    type ComponentTypeId = number;

    class Component {
    }

    enum ComponentFieldType {
        Vector3 = "ut.Math.Vector3",
        Vector2 = "ut.Math.Vector2",
        float = "float",
        Quaternion = "ut.Math.Quaternion",
        Color = "ut.Core2D.Color",
        Boolean = "bool",
        Int = "int32_t",
        Other = "other" // add other field types if they become tweenable or watchable
    }

    interface ComponentFieldDesc {
        static readonly $cid: number;
        static readonly $o: number; 
        static readonly $t: ComponentFieldType;
    }
    
    type CC<T> = ComponentClass<T>;
    type CSA<T> = ComponentClass<T> | ComponentSpecAdapter<T>;
    type CSABase = ComponentClassBase | ComponentSpec;
}
/// <reference path="../ts/Core.d.ts" />
/// <reference path="../../../artifacts/bindgem_output/bind-Core.d.ts" />
declare namespace ut {
    var NONE: Entity;
    class ComponentSpec {
        constructor(type: ComponentClassBase, access?: AccessMode);
        type: ComponentClassBase;
        access: AccessMode;
    }
    class ComponentSpecAdapter<T> extends ComponentSpec {
        constructor(type: ComponentClassBase, access: AccessMode);
    }
    interface ComponentClassBase {
        cid: number;
        _size: number;
        _view: any;
        _isSharedComp: boolean;
    }
    interface ComponentClass<T> extends ComponentClassBase {
        _fromPtr(p: number, v?: T): T;
        _toPtr(p: number, v: T): void;
        _tempHeapPtr(v: T): number;
        _dtorFn(v: T): void;
    }
    interface ComponentViewBase {
        $advance(): void;
    }
    function ReadOnly<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function ReadWrite<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function OptionalReadOnly<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function OptionalReadWrite<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function Optional<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function Subtractive<T>(type: ComponentClass<T>): ComponentSpecAdapter<T>;
    function getComponentType<T>(cdata: T): ComponentClass<T>;
    class ArchetypeChunkIterator extends ArchetypeChunkIteratorBase {
        addFilter(...specs: ComponentSpec[]): void;
        getBaseViewFor<T>(ctype: ComponentClass<T>): T | null;
        getSharedViewFor<T>(ctype: ComponentClass<T>): T | null;
    }
    class System extends SystemBase {
        static define(params: SystemJSParams): SystemJS;
    }
    interface SystemJSParams {
        name: string;
        update?: (() => void) | ((sched: Scheduler, world: World) => void);
        updatesBefore?: string[];
        updatesAfter?: string[];
    }
    class SystemJS {
        name: string;
        update: (() => void) | ((sched: Scheduler, world: World) => void);
        updatesBefore: string[];
        updatesAfter: string[];
        _sys: System | null;
        constructor(params: SystemJSParams);
        _realize(): System;
    }
}
declare namespace ut {
    type DataAdapter = World;
}
declare namespace ut {
    class EntityCommandBuffer extends EntityCommandBufferBase {
        private tmpEntity;
        createDeferredEntities(archetype: Archetype, count: number): Entity[];
        instantiateDeferredEntities(proto: Entity, count: number): Entity[];
        addComponent<T>(entity: Entity, ctype: ComponentClass<T> | T): void;
        addComponentData<T>(entity: Entity, cdata: T): void;
        removeComponent<T>(entity: Entity, ctype: ComponentClass<T>): void;
        setComponentData<T>(entity: Entity, cdata: T): void;
        addSharedComponentData<T>(entity: Entity, cdata: T): void;
        setSharedComponentData<T>(entity: Entity, cdata: T): void;
    }
}
declare namespace ut {
    class Scheduler extends SchedulerBase {
        constructor();
        schedule(system: System | SystemJS): void;
        schedule(name: string, systemFn: ((sched: Scheduler, world: WorldBase) => void)): void;
        remove(system: string | number): void;
        enable(system: string | number): void;
        disable(system: (string | number)): void;
        isEnabled(system: string | number): boolean;
        find(system: string | System | SystemJS): number;
    }
}
declare namespace ut {
    class World extends WorldBase {
        private tmpEntity;
        createEntities(archetype: Archetype, count: number): Entity[];
        instantiateEntities(proto: Entity, count: number): Entity[];
        destroyEntities(entities: Entity[]): void;
        hasComponent<T>(entity: Entity, ctype: ComponentClass<T>): boolean;
        addComponent<T>(entity: Entity, ctype: ComponentClass<T> | T): void;
        addComponentData<T>(entity: Entity, cdata: T): void;
        removeComponent<T>(entity: Entity, ctype: ComponentClass<T>): void;
        getComponentData<T>(entity: Entity, ctype: ComponentClass<T>): T;
        setComponentData<T>(entity: Entity, cdata: T): void;
        addSharedComponent<T>(entity: Entity, ctype: ComponentClass<T> | T): void;
        getSharedComponentData<T>(entity: Entity, ctype: ComponentClass<T>): T;
        setSharedComponentData<T>(entity: Entity, cdata: T): void;
        getConfigData<T>(ctype: ComponentClass<T>): T;
        setConfigData<T>(cdata: T): void;
        createArchetype(...ctypes: any[]): Archetype;
        allocChunkIterator(...specs: ComponentSpec[]): ArchetypeChunkIterator;
        forEach<A>(components: [CSA<A>], fn: ((a: A) => void)): void;
        forEach<A>(components: [CSA<A>], specs: ComponentSpec[], fn: ((a: A) => void)): void;
        forEach<A, B>(components: [CSA<A>, CSA<B>], fn: ((a: A, b: B) => void)): void;
        forEach<A, B>(components: [CSA<A>, CSA<B>], specs: ComponentSpec[], fn: ((a: A, b: B) => void)): void;
        forEach<A, B, C>(components: [CSA<A>, CSA<B>, CSA<C>], fn: ((a: A, b: B, c: C) => void)): void;
        forEach<A, B, C>(components: [CSA<A>, CSA<B>, CSA<C>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C) => void)): void;
        forEach<A, B, C, D>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>], fn: ((a: A, b: B, c: C, d: D) => void)): void;
        forEach<A, B, C, D>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C, d: D) => void)): void;
        forEach<A, B, C, D, E>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>], fn: ((a: A, b: B, c: C, d: D, e: E) => void)): void;
        forEach<A, B, C, D, E>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C, d: D, e: E) => void)): void;
        forEach<A, B, C, D, E, F>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F) => void)): void;
        forEach<A, B, C, D, E, F>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C, d: D, e: E, f: F) => void)): void;
        forEach<A, B, C, D, E, F, G>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>, CSA<G>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G) => void)): void;
        forEach<A, B, C, D, E, F, G>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>, CSA<G>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G) => void)): void;
        forEach<A, B, C, D, E, F, G, H>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>, CSA<G>, CSA<H>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G, h: H) => void)): void;
        forEach<A, B, C, D, E, F, G, H>(components: [CSA<A>, CSA<B>, CSA<C>, CSA<D>, CSA<E>, CSA<F>, CSA<G>, CSA<H>], specs: ComponentSpec[], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G, h: H) => void)): void;
        forEach(components: CSABase[], fn: ((...comps: any[]) => void)): void;
        forEach(components: CSABase[], specs: ComponentSpec[], fn: ((...comps: any[]) => void)): void;
        /** Gets the component data if it's already present on the given entity, or adds it, and returns it.
         * For performance, if you expect the component to be present on the entity you should call getComponentData directly.*/
        getOrAddComponentData<T extends Component>(entity: Entity, ctype: ComponentClass<T>): T;
        /** Sets the component data if it's already present on the given entity, or adds it.
         *  For performance, if you expect the component to be present on the entity you should call setComponentData directly.*/
        setOrAddComponentData<T extends Component>(entity: Entity, cdata: T): void;
        usingComponentData<A>(entity: Entity, components: [CC<A>], fn: ((a: A) => void)): void;
        usingComponentData<A, B>(entity: Entity, components: [CC<A>, CC<B>], fn: ((a: A, b: B) => void)): void;
        usingComponentData<A, B, C>(entity: Entity, components: [CC<A>, CC<B>, CC<C>], fn: ((a: A, b: B, c: C) => void)): void;
        usingComponentData<A, B, C, D>(entity: Entity, components: [CC<A>, CC<B>, CC<C>, CC<D>], fn: ((a: A, b: B, c: C, d: D) => void)): void;
        usingComponentData<A, B, C, D, E>(entity: Entity, components: [CC<A>, CC<B>, CC<C>, CC<D>, CC<E>], fn: ((a: A, b: B, c: C, d: D, e: E) => void)): void;
        usingComponentData<A, B, C, D, E, F>(entity: Entity, components: [CC<A>, CC<B>, CC<C>, CC<D>, CC<E>, CC<F>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F) => void)): void;
        usingComponentData<A, B, C, D, E, F, G>(entity: Entity, components: [CC<A>, CC<B>, CC<C>, CC<D>, CC<E>, CC<F>, CC<G>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G) => void)): void;
        usingComponentData<A, B, C, D, E, F, G, H>(entity: Entity, components: [CC<A>, CC<B>, CC<C>, CC<D>, CC<E>, CC<F>, CC<G>, CC<H>], fn: ((a: A, b: B, c: C, d: D, e: E, f: F, g: G, h: H) => void)): void;
        world(): World;
    }
}
declare namespace ut {
/** Describes how a component type is accessed during {@link World.ForEach} iteration.*/
enum AccessMode {
  /** The component type is read and/or written.*/
  ReadWrite = 0,
  /** The component type is read only.*/
  ReadOnly = 1,
  /** The component type must not be present on the entity.*/
  Subtractive = 2,
  /** At least one of the components flagged AnyOfReadWrite or AnyOfReadOnly
      must be present. If present, the component type is read and/or written.*/
  AnyOfReadWrite = 3,
  /** At least one of the components flagged AnyOfReadWrite or AnyOfReadOnly
      must be present. If present, the component type will be read only.*/
  AnyOfReadOnly = 4,
  /** The component is not required, but if present it is read and/or written.
      If not present, it is null.*/
  OptionalReadWrite = 5,
  /** The component is not required, but if present it is read only. If not
      present, it is null.*/
  OptionalReadOnly = 6,
}
}
declare namespace ut {

class LocaleResult {
  constructor(locales?: string[]);
  locales: string[];
  static _size: number;
  static _fromPtr(p: number, v?: LocaleResult): LocaleResult;
  static _toPtr(p: number, v: LocaleResult): void;
  static _tempHeapPtr(v: LocaleResult): number;
}
interface LocaleResultComponentFieldDesc extends ut.ComponentFieldDesc {
  
}

}
declare namespace ut {

/** A component filter specification consisting of a component type and an {@link ut.AccessMode}.*/
class ComponentSpecRaw {
  constructor(type?: ComponentTypeId, access?: AccessMode);
  type: ComponentTypeId;
  access: AccessMode;
  static _size: number;
  static _fromPtr(p: number, v?: ComponentSpecRaw): ComponentSpecRaw;
  static _toPtr(p: number, v: ComponentSpecRaw): void;
  static _tempHeapPtr(v: ComponentSpecRaw): number;
}
interface ComponentSpecRawComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
}

}
declare namespace ut {

/** An entity is a lightweight reference to a set of components. Entities are
    one of the foundations of the Entity Component System architecture, and
    are used in almost all operations.
    For operations that involve an entity, see {@link ut.World}.*/
class Entity extends ut.Component {
  constructor(index?: number, version?: number);
  /** The index into the entities array. A world cannot have two entities
      with the same index at the same time. The NONE entity has index=0 and version=0.
      
      You should not need to access the index directly except in specific cases
      such as making by-value copies of entities, or inspection during debugging.
      
      Note that entity index is re-used. This means that once an Entity with
      a specific index is destroyed, its index might be returned for a newly
      created entity. However, the combination of index + version is always unique.*/
  index: number;
  /** The version of this entity reference. The NONE entity has version=0 and index=0.
      You should not need to access the index directly except in specific cases
      such as inspection during debugging.
      Not that the combination of version + index is always unique.*/
  version: number;
  static readonly index: ComponentFieldDesc;
  static readonly version: ComponentFieldDesc;
  /** Check if this entity is NONE.
  @returns True if the entity is NONE, false otherwise.*/
  isNone(): boolean;

  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Entity): Entity;
  static _toPtr(p: number, v: Entity): void;
  static _tempHeapPtr(v: Entity): number;
  static _dtorFn(v: Entity): void;
}

}
declare namespace ut {

/** A component indicating that the entity should be treated as "disabled".
    Disabled entities do not show up during ForEach iteration unless the Disabled
    component is explicitly referenced in the ForEach component list.*/
class Disabled extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Disabled): Disabled;
  static _toPtr(p: number, v: Disabled): void;
  static _tempHeapPtr(v: Disabled): number;
  static _dtorFn(v: Disabled): void;
}

}
declare namespace ut {

/** A component added to an entity when all non system-state components have
    been removed, but some system state components are left.*/
class CleanupEntity extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: CleanupEntity): CleanupEntity;
  static _toPtr(p: number, v: CleanupEntity): void;
  static _tempHeapPtr(v: CleanupEntity): number;
  static _dtorFn(v: CleanupEntity): void;
}

}
declare namespace ut {

class EntityInformation extends ut.Component {
  constructor(name?: string);
  name: string;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EntityInformation): EntityInformation;
  static _toPtr(p: number, v: EntityInformation): void;
  static _tempHeapPtr(v: EntityInformation): number;
  static _dtorFn(v: EntityInformation): void;
}

}
declare namespace ut {
type EntityFn = (e: Entity) => void;
}
declare namespace ut {
type SystemFn = (s: ut.SchedulerBase, w: ut.WorldBase) => void;
}
declare namespace ut {
type CopyFn = (src: string, dst: string) => void;
}
declare namespace ut {
type DestructorFn = (ptr: string) => void;
}
declare namespace ut {
/** Base class for all systems. All systems implement this interface.*/
class SystemBase {
  /** Returns the name of this system.*/
  name(): string;
  /** Indicates that this system must update before the {@link name} system.
      Call UpdateBefore before this system is scheduled. Calling it after
      this system is scheduled causes an error.
      Note that a system can be scheduled before or after multiple other systems.*/
  updateBefore(name: string): void;
  /** Indicates that this system must update after the {@link name} system.
      Call UpdateBefore before this system is scheduled. Calling it after
      This system is scheduled causes an error.
      Note that a system can be scheduled before or after multiple other systems.*/
  updateAfter(name: string): void;
  /** Checks whether the system has been scheduled.
      Returns true if the system is currently scheduled.*/
  scheduled(): boolean;
}
}
declare namespace ut {
/** A Scheduler manages a set of systems and their dependencies. Typically you
    will only have a frame scheduler obtained from {@link ut.World}.
    Systems can be added, removed, enabled and disabled at any time.*/
class SchedulerBase {
  /** Returns the name of this scheduler.*/
  name(): string;
  /** Run this scheduler.*/
  run(): void;
  /** @returns The time elapsed since the last time this scheduler was run, in seconds.*/
  deltaTime(): number;
  /** @returns The time elapsed since this scheduler was first created, in seconds.*/
  now(): number;
  /** Removes the system named {@link name} from the scheduler.*/
  removeName(name: string): void;
  /** Removes the system with id {@link id} from the scheduler.*/
  removeId(id: number): void;
  /** Remove all systems from the scheduler.*/
  removeAll(): void;
  /** Enable the system named {@link name}.*/
  enableName(name: string): void;
  /** Enables the system with id {@link id}.*/
  enableId(id: number): void;
  /** Disables the system named {@link name}.*/
  disableName(name: string): void;
  /** Disables the system with id {@link id}.*/
  disableId(id: number): void;
  /** Returns whether the system named {@link name} is enabled.*/
  isEnabledName(name: string): boolean;
  /** Returns whether the system with id {@link id} is enabled.*/
  isEnabledId(id: number): boolean;
  scheduleSysInternal(sys: ut.SystemBase): number;
  scheduleFnInternal(name: string, sys: SystemFn): number;
  /** Returns the id of a given system.*/
  findSys(sys: ut.SystemBase): number;
  /** Returns the id of a system with the given {@link name}.*/
  findName(name: string): number;
  /** Pause the scheduler execution.*/
  pause(): void;
  /** Return whether the scheduler is paused or not.*/
  isPaused(): boolean;
  /** Step over to the start of the next iteration when scheduler is paused.*/
  step(): void;
  /** Resume the scheduler execution.*/
  resume(): void;
  /** Returns a string representation of the fully scheduled system order for debugging/testing purposes.*/
  stringify(): string;
}
}
declare namespace ut {
class CoreService {
  static createSystem(name: string, fn: SystemFn, runsAfter: string[], runsBefore: string[]): ut.SystemBase;
  static getCurrentLocale(): string;
  static getSupportedLocales(): LocaleResult;
}
}
declare namespace ut {
/** An Archetype is a set of component types. When creating multiple entities
    that all have the same components, it is faster to create an Archetype using
    {@link ut.World.CreateArchetype} and use that to create multiple entities.*/
class Archetype {
  getComponentTypeCount(): number;
}
}
declare namespace ut {
/** A low level entity iteration interface. Do not use it in user-generated code.*/
class ArchetypeChunkIteratorBase {
  next(): boolean;
  count(): number;
}
}
declare namespace ut {
/** A buffer for entity commands. You can use it to apply the commands to a {@link ut.World}.
    at a later time. You can also use it to apply the commands multiple times
    in order to perform a complex operation or build up a complex set of reusable entities.
    You should only need to create an EntityCommandBuffer manually in special
    use cases such as recording a command buffer in order to play it back multiple times.
    
    You can use an EntityCommandBuffer much as you would a {@link ut.World}, but the
    entities that {@link ut.EntityCommandBuffer.CreateDeferredEntity} and related methods return are
    "deferred" entities that are only valid to be passed to EntityCommandBuffer.
    After playback, you can obtain the real entities by passing a deferred entity to {@link ut.EntityCommandBuffer.TranslateDeferredEntity}.*/
class EntityCommandBufferBase {
  /** Disposes of an EntityCommandBuffer, frees buffers, and cleans up any
      component data.*/
  dispose(): void;
  playback(world: ut.WorldBase): void;
  /** Applies the commands in this buffer, and then immediately disposes of it.*/
  commit(world: ut.WorldBase): void;
  /** Adds a command to set the name of {@link entity} to this buffer.*/
  setEntityName(entity: Entity, name: string): void;
  /** Returns the real entity that corresponds to the entity for {@link deferred} after
      the most recent playback.*/
  translateDeferredEntity(deferred: Entity): Entity;
  /** Creates {@link count} entities of {@link archetype}.*/
  createEntities(archetype: Archetype, count: number): void;
  /** Creates one entity of {@link archetype}.*/
  createEntity(archetype: Archetype): void;
  /** Creates one entity of {@link archetype}, and returns its deferred entity handle.*/
  createDeferredEntity(archetype: Archetype): Entity;
  /** Instantiates (clones) {@link count} entities of source {@link entity}. The source entity may be real or deferred.*/
  instantiateEntities(entity: Entity, count: number): void;
  /** Instantiates (clones) one entity of source {@link entity}, and returns its deferred entity handle.
      The source entity may be real or deferred.*/
  instantiateDeferredEntity(entity: Entity): Entity;
  /** Destroys entity.*/
  destroyEntity(entity: Entity): void;
  /** Adds a component to {@link entity}. Raw form.*/
  addComponentRaw(entity: Entity, cid: ComponentTypeId): void;
  /** Removes a component from {@link entity}. Raw form.*/
  removeComponentRaw(entity: Entity, cid: ComponentTypeId): void;
}
}
declare namespace ut {
class WorldBase {
  debugCheckEntities(): void;
  setEntityName(entity: Entity, name: string): void;
  getEntityName(entity: Entity): string;
  /** Finds an entity by its name. If multiple entities have the same name,
      this function returns only one of them. Which one it returns may change
      from one GetEntityByName call to the next.
      
      This operation is slow. If you need direct access to entities, you
      can save {@link ut.Entity} references to them ahead of time and use
      the references directly.*/
  getEntityByName(name: string): Entity;
  /** Obtains the default frame scheduler for this world.*/
  scheduler(): ut.SchedulerBase;
  /** Gets the {@link ut.Entity} that contains configuration components.*/
  getConfigEntity(): Entity;
  /** Runs the default scheduler.*/
  run(): void;
  toJSON(): string;
  fromJSON(json: string): void;
  bufferDepth(): number;
  startBuffering(): void;
  stopBuffering(): void;
  /** Return the empty {@link ut.Archetype}, containing no components.*/
  emptyArchetype(): Archetype;
  /** Create an entity with {@link ut.Archetype}{@link archetype}.*/
  createEntity(archetype?: Archetype): Entity;
  /** Instantiate (clone) the source entity {@link proto}.
      This is a shallow copy of the source entity's components. Transform
      hierarchy children are ignored.*/
  instantiateEntity(proto: Entity): Entity;
  /** Destroy {@link entity}. If the entity has any {@link System State Components},
      all non-System State Components are removed, but the entity is not destroyed
      until all System State Component are deleted by the systems they belong to.*/
  destroyEntity(entity: Entity): void;
  /** Returns true if {@link entity} refers to a valid, live entity.*/
  exists(entity: Entity): boolean;
  /** Returns the {@link ut.Archetype} of {@link entity}.*/
  getArchetype(entity: Entity): Archetype;
  hasComponentRaw(entity: Entity, type: ComponentTypeId): boolean;
  addComponentRaw(entity: Entity, type: ComponentTypeId): void;
  removeComponentRaw(entity: Entity, type: ComponentTypeId): void;
}
}

declare namespace Module {
function _ut_Entity_IsNone(selfPtr: any): any;
function _ut_System_shRelease(self: number): void;
function _ut_System_Name(selfPtr: any): void;
function _ut_System_UpdateBefore(selfPtr: any, name: any): void;
function _ut_System_UpdateAfter(selfPtr: any, name: any): void;
function _ut_System_Scheduled(selfPtr: any): any;
function _ut_Scheduler_shRelease(self: number): void;
function _ut_Scheduler_Name(selfPtr: any): void;
function _ut_Scheduler_Run(selfPtr: any): void;
function _ut_Scheduler_DeltaTime(selfPtr: any): any;
function _ut_Scheduler_Now(selfPtr: any): any;
function _ut_Scheduler_RemoveName(selfPtr: any, name: any): void;
function _ut_Scheduler_RemoveId(selfPtr: any, id: any): void;
function _ut_Scheduler_RemoveAll(selfPtr: any): void;
function _ut_Scheduler_EnableName(selfPtr: any, name: any): void;
function _ut_Scheduler_EnableId(selfPtr: any, id: any): void;
function _ut_Scheduler_DisableName(selfPtr: any, name: any): void;
function _ut_Scheduler_DisableId(selfPtr: any, id: any): void;
function _ut_Scheduler_IsEnabledName(selfPtr: any, name: any): any;
function _ut_Scheduler_IsEnabledId(selfPtr: any, id: any): any;
function _ut_Scheduler_ScheduleSysInternal(selfPtr: any, sys: any): any;
function _ut_Scheduler_ScheduleFnInternal(selfPtr: any, name: any, sys: any): any;
function _ut_Scheduler_FindSys(selfPtr: any, sys: any): any;
function _ut_Scheduler_FindName(selfPtr: any, name: any): any;
function _ut_Scheduler_Pause(selfPtr: any): void;
function _ut_Scheduler_IsPaused(selfPtr: any): any;
function _ut_Scheduler_Step(selfPtr: any): void;
function _ut_Scheduler_Resume(selfPtr: any): void;
function _ut_Scheduler_Stringify(selfPtr: any): void;
function _ut_CoreService_CreateSystem(name: any, fn: any, runsAfter: any, runsBefore: any): any;
function _ut_CoreService_GetCurrentLocale(): void;
function _ut_CoreService_GetSupportedLocales(): void;
function _ut_Archetype_shRelease(self: number): void;
function _ut_Archetype_GetComponentTypeCount(selfPtr: any): any;
function _ut_ArchetypeChunkIterator_shRelease(self: number): void;
function _ut_ArchetypeChunkIterator_AddFilter(selfPtr: any, spec: any, count: any): void;
function _ut_ArchetypeChunkIterator_Next(selfPtr: any): any;
function _ut_ArchetypeChunkIterator_Count(selfPtr: any): any;
function _ut_ArchetypeChunkIterator_RawData(selfPtr: any, type: any): any;
function _ut_ArchetypeChunkIterator_RawDataByIndex(selfPtr: any, index: any): any;
function _ut_ArchetypeChunkIterator_RawSharedData(selfPtr: any, type: any): any;
function _ut_ArchetypeChunkIterator_RawSharedDataByIndex(selfPtr: any, index: any): any;
function _ut_ArchetypeChunkIterator_GetTypeOrSharedComponentIndicesAndSizesInOrder(selfPtr: any, cids: any, indices: any, sizes: any, count: any): void;
function _ut_ArchetypeChunkIterator_GetTypeOrSharedComponentIndicesAndSizesInOrderForSpecs(selfPtr: any, specs: any, indices: any, sizes: any, count: any): void;
function _ut_ArchetypeChunkIterator_GetBasePointersAndSizesInOrder(selfPtr: any, cids: any, data: any, sizes: any, count: any): void;
function _ut_EntityCommandBuffer_EntityCommandBuffer(): number;
function _ut_EntityCommandBuffer_shRelease(self: number): void;
function _ut_EntityCommandBuffer_Dispose(selfPtr: any): void;
function _ut_EntityCommandBuffer_Playback(selfPtr: any, world: any): void;
function _ut_EntityCommandBuffer_Commit(selfPtr: any, world: any): void;
function _ut_EntityCommandBuffer_SetEntityName(selfPtr: any, entity: any, name: any): void;
function _ut_EntityCommandBuffer_TranslateDeferredEntity(selfPtr: any, deferred: any): void;
function _ut_EntityCommandBuffer_CreateEntities(selfPtr: any, archetype: any, count: any): void;
function _ut_EntityCommandBuffer_CreateEntity(selfPtr: any, archetype: any): void;
function _ut_EntityCommandBuffer_CreateDeferredEntities(selfPtr: any, archetype: any, entities: any, count: any): void;
function _ut_EntityCommandBuffer_CreateDeferredEntity(selfPtr: any, archetype: any): void;
function _ut_EntityCommandBuffer_InstantiateEntities(selfPtr: any, entity: any, count: any): void;
function _ut_EntityCommandBuffer_InstantiateDeferredEntities(selfPtr: any, entity: any, newEntities: any, count: any): void;
function _ut_EntityCommandBuffer_InstantiateDeferredEntity(selfPtr: any, entity: any): void;
function _ut_EntityCommandBuffer_DestroyEntities(selfPtr: any, ents: any, count: any): void;
function _ut_EntityCommandBuffer_DestroyEntity(selfPtr: any, entity: any): void;
function _ut_EntityCommandBuffer_AddComponentRaw(selfPtr: any, entity: any, cid: any): void;
function _ut_EntityCommandBuffer_RemoveComponentRaw(selfPtr: any, entity: any, cid: any): void;
function _ut_EntityCommandBuffer_AddComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_EntityCommandBuffer_SetComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_EntityCommandBuffer_AddSharedComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_EntityCommandBuffer_SetSharedComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_World(): number;
function _ut_World_shRelease(self: number): void;
function _ut_World_DebugCheckEntities(selfPtr: any): void;
function _ut_World_SetEntityName(selfPtr: any, entity: any, name: any): void;
function _ut_World_GetEntityName(selfPtr: any, entity: any): void;
function _ut_World_GetEntityByName(selfPtr: any, name: any): void;
function _ut_World_Scheduler(selfPtr: any): any;
function _ut_World_GetConfigEntity(selfPtr: any): void;
function _ut_World_Run(selfPtr: any): void;
function _ut_World_toJSON(selfPtr: any): void;
function _ut_World_fromJSON(selfPtr: any, json: any): void;
function _ut_World_BufferDepth(selfPtr: any): any;
function _ut_World_StartBuffering(selfPtr: any): void;
function _ut_World_StopBuffering(selfPtr: any): void;
function _ut_World_GetConfigDataRaw(selfPtr: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_SetConfigDataRaw(selfPtr: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_CreateArchetype(selfPtr: any, types: any, count: any): any;
function _ut_World_EmptyArchetype(selfPtr: any): any;
function _ut_World_CreateEntity(selfPtr: any, archetype: any): void;
function _ut_World_CreateEntities(selfPtr: any, archetype: any, entities: any, count: any): void;
function _ut_World_InstantiateEntity(selfPtr: any, proto: any): void;
function _ut_World_InstantiateEntities(selfPtr: any, proto: any, entities: any, count: any): void;
function _ut_World_DestroyEntity(selfPtr: any, entity: any): void;
function _ut_World_DestroyEntities(selfPtr: any, entities: any, count: any): void;
function _ut_World_Exists(selfPtr: any, entity: any): any;
function _ut_World_GetArchetype(selfPtr: any, entity: any): any;
function _ut_World_AllocChunkIterator(selfPtr: any, specs: any, count: any): any;
function _ut_World_HasComponentRaw(selfPtr: any, entity: any, type: any): any;
function _ut_World_AddComponentRaw(selfPtr: any, entity: any, type: any): void;
function _ut_World_RemoveComponentRaw(selfPtr: any, entity: any, type: any): void;
function _ut_World_AddComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_GetComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_SetComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_SetSharedComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_AddSharedComponentRaw(selfPtr: any, entity: any, type: any): void;
function _ut_World_AddSharedComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;
function _ut_World_GetSharedComponentDataRaw(selfPtr: any, entity: any, type: any, data: any, dataSizeBytes: any): void;

}


