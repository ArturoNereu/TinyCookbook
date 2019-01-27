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


//
// Originally from https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/three/three-core.d.ts
//

declare namespace utmath {

/**
 * ( interface Vector&lt;T&gt; )
 *
 * Abstract interface of Vector2, Vector3 and Vector4.
 * Currently the members of Vector is NOT type safe because it accepts different typed vectors.
 * Those definitions will be changed when TypeScript innovates Generics to be type safe.
 *
 * @example
 * var v:THREE.Vector = new THREE.Vector3();
 * v.addVectors(new THREE.Vector2(0, 1), new THREE.Vector2(2, 3));    // invalid but compiled successfully
 */
interface Vector {
    setComponent(index: number, value: number): void;

    getComponent(index: number): number;

    /**
     * copy(v:T):T;
     */
    copy(v: this): this;

    /**
     * add(v:T):T;
     */
    add(v: Vector): Vector;

    /**
     * addVectors(a:T, b:T):T;
     */
    addVectors(a: Vector, b: Vector): Vector;

    /**
     * sub(v:T):T;
     */
    sub(v: Vector): Vector;

    /**
     * subVectors(a:T, b:T):T;
     */
    subVectors(a: Vector, b: Vector): Vector;

    /**
     * multiplyScalar(s:number):T;
     */
    multiplyScalar(s: number): Vector;

    /**
     * divideScalar(s:number):T;
     */
    divideScalar(s: number): Vector;

    /**
     * negate():T;
     */
    negate(): Vector;

    /**
     * dot(v:T):T;
     */
    dot(v: Vector): number;

    /**
     * lengthSq():number;
     */
    lengthSq(): number;

    /**
     * length():number;
     */
    length(): number;

    /**
     * normalize():T;
     */
    normalize(): Vector;

    /**
     * NOTE: Vector4 doesn't have the property.
     *
     * distanceTo(v:T):number;
     */
    distanceTo?(v: Vector): number;

    /**
     * NOTE: Vector4 doesn't have the property.
     *
     * distanceToSquared(v:T):number;
     */
    distanceToSquared?(v: Vector): number;

    /**
     * setLength(l:number):T;
     */
    setLength(l: number): Vector;

    /**
     * lerp(v:T, alpha:number):T;
     */
    lerp(v: Vector, alpha: number): Vector;

    /**
     * equals(v:T):boolean;
     */
    equals(v: Vector): boolean;

    /**
     * clone():T;
     */
    clone(): this;
}

/**
 * 2D vector.
 *
 * ( class Vector2 implements Vector<Vector2> )
 */
class Vector2 implements Vector {
    constructor(x?: number, y?: number);

    x: number;
    y: number;

    /**
     * Sets value of this vector.
     */
    set(x: number, y: number): Vector2;

    /**
     * Sets the x and y values of this vector both equal to scalar.
     */
    setScalar(scalar: number): Vector2;

    /**
     * Sets X component of this vector.
     */
    setX(x: number): Vector2;

    /**
     * Sets Y component of this vector.
     */
    setY(y: number): Vector2;

    /**
     * Sets a component of this vector.
     */
    setComponent(index: number, value: number): void;
    /**
     * Gets a component of this vector.
     */
    getComponent(index: number): number;

    /**
     * Clones this vector.
     */
    clone(): this;
    /**
     * Copies value of v to this vector.
     */
    copy(v: this): this;

    /**
     * Adds v to this vector.
     */
    add(v: Vector2): Vector2;
    /**
     * Adds the scalar value s to this vector's x and y values.
     */
    addScalar(s: number): Vector2;
    /**
     * Sets this vector to a + b.
     */
    addVectors(a: Vector2, b: Vector2): Vector2;
    /**
     * Adds the multiple of v and s to this vector.
     */
    addScaledVector(v: Vector2, s: number): Vector2;

    /**
     * Subtracts v from this vector.
    */
    sub(v: Vector2): Vector2;
    /**
     * Subtracts s from this vector's x and y components.
     */
    subScalar(s: number): Vector2;
    /**
     * Sets this vector to a - b.
     */
    subVectors(a: Vector2, b: Vector2): Vector2;

    /**
     * Multiplies this vector by v.
     */
    multiply(v: Vector2): Vector2;
    /**
     * Multiplies this vector by scalar s.
     */
    multiplyScalar(scalar: number): Vector2;

    /**
     * Divides this vector by v.
     */
    divide(v: Vector2): Vector2;
    /**
     * Divides this vector by scalar s.
     * Set vector to ( 0, 0 ) if s == 0.
     */
    divideScalar(s: number): Vector2;

    /**
     * Multiplies this vector (with an implicit 1 as the 3rd component) by m.
     */
    applyMatrix3(m: Matrix3): Vector2;

    /**
     * If this vector's x or y value is greater than v's x or y value, replace that value with the corresponding min value.
     */
    min(v: Vector2): Vector2;
    /**
     * If this vector's x or y value is less than v's x or y value, replace that value with the corresponding max value.
     */
    max(v: Vector2): Vector2;

    /**
     * If this vector's x or y value is greater than the max vector's x or y value, it is replaced by the corresponding value.
     * If this vector's x or y value is less than the min vector's x or y value, it is replaced by the corresponding value.
     * @param min the minimum x and y values.
     * @param max the maximum x and y values in the desired range.
     */
    clamp(min: Vector2, max: Vector2): Vector2;
    /**
     * If this vector's x or y values are greater than the max value, they are replaced by the max value.
     * If this vector's x or y values are less than the min value, they are replaced by the min value.
     * @param min the minimum value the components will be clamped to.
     * @param max the maximum value the components will be clamped to.
     */
    clampScalar(min: number, max: number): Vector2;
    /**
     * If this vector's length is greater than the max value, it is replaced by the max value.
     * If this vector's length is less than the min value, it is replaced by the min value.
     * @param min the minimum value the length will be clamped to.
     * @param max the maximum value the length will be clamped to.
     */
    clampLength(min: number, max: number): Vector2;

    /**
     * The components of the vector are rounded down to the nearest integer value.
     */
    floor(): Vector2;
    /**
     * The x and y components of the vector are rounded up to the nearest integer value.
     */
    ceil(): Vector2;
    /**
     * The components of the vector are rounded to the nearest integer value.
     */
    round(): Vector2;
    /**
     * The components of the vector are rounded towards zero (up if negative, down if positive) to an integer value.
     */
    roundToZero(): Vector2;

    /**
     * Inverts this vector.
     */
    negate(): Vector2;

    /**
     * Computes dot product of this vector and v.
     */
    dot(v: Vector2): number;

    /**
     * Computes squared length of this vector.
     */
    lengthSq(): number;

    /**
     * Computes length of this vector.
     */
    length(): number;

    /**
     * @deprecated Use {@link Vector2#manhattanLength .manhattanLength()} instead.
     */
    lengthManhattan(): number;

    /**
     * Normalizes this vector.
     */
    normalize(): Vector2;

    /**
     * computes the angle in radians with respect to the positive x-axis
     */
    angle(): number;

    /**
     * Computes distance of this vector to v.
     */
    distanceTo(v: Vector2): number;
    /**
     * Computes squared distance of this vector to v.
     */
    distanceToSquared(v: Vector2): number;
    /**
     * @deprecated Use {@link Vector2#manhattanDistanceTo .manhattanDistanceTo()} instead.
     */
    distanceToManhattan(v: Vector2): number;

    /**
     * Normalizes this vector and multiplies it by l.
     */
    setLength(length: number): Vector2;

    /**
     * Linearly interpolates between this vector and v, where alpha is the distance along the line - alpha = 0 will be this vector, and alpha = 1 will be v.
     * @param v vector to interpolate towards.
     * @param alpha interpolation factor in the closed interval [0, 1].
     */
    lerp(v: Vector2, alpha: number): Vector2;
    /**
     * Sets this vector to be the vector linearly interpolated between v1 and v2 where alpha is the distance along the line connecting the two vectors - alpha = 0 will be v1, and alpha = 1 will be v2.
     * @param v1 the starting vector.
     * @param v2 vector to interpolate towards.
     * @param alpha interpolation factor in the closed interval [0, 1].
     */
    lerpVectors(v1: Vector2, v2: Vector2, alpha: number): Vector2;

    /**
     * Checks for strict equality of this vector and v.
     */
    equals(v: Vector2): boolean;

    /**
     * Sets this vector's x value to be array[offset] and y value to be array[offset + 1].
     * @param array the source array.
     * @param offset (optional) offset into the array. Default is 0.
     */
    fromArray(array: number[], offset?: number): Vector2;
    /**
     * Returns an array [x, y], or copies x and y into the provided array.
     * @param array (optional) array to store the vector to. If this is not provided, a new array will be created.
     * @param offset (optional) optional offset into the array.
     */
    toArray(array?: number[], offset?: number): number[];

    /**
     * Rotates the vector around center by angle radians.
     * @param center the point around which to rotate.
     * @param angle the angle to rotate, in radians.
     */
    rotateAround(center: Vector2, angle: number): Vector2;

    /**
     * Computes the Manhattan length of this vector.
     *
     * @return {number}
     *
     * @see {@link http://en.wikipedia.org/wiki/Taxicab_geometry|Wikipedia: Taxicab Geometry}
     */
    manhattanLength(): number;

    /**
     * Computes the Manhattan length (distance) from this vector to the given vector v
     *
     * @param {Vector2} v
     *
     * @return {number}
     *
     * @see {@link http://en.wikipedia.org/wiki/Taxicab_geometry|Wikipedia: Taxicab Geometry}
     */
    manhattanDistanceTo(v: Vector2): number;
}

interface Vector2ComponentFieldDesc extends ut.ComponentFieldDesc {
    static readonly x: ut.ComponentFieldDesc;
    static readonly y: ut.ComponentFieldDesc;
}

/**
 * 3D vector.
 *
 * @example
 * var a = new THREE.Vector3( 1, 0, 0 );
 * var b = new THREE.Vector3( 0, 1, 0 );
 * var c = new THREE.Vector3();
 * c.crossVectors( a, b );
 *
 * @see <a href="https://github.com/mrdoob/three.js/blob/master/src/math/Vector3.js">src/math/Vector3.js</a>
 *
 * ( class Vector3 implements Vector<Vector3> )
 */
class Vector3 implements Vector {
    constructor(x?: number, y?: number, z?: number);

    x: number;
    y: number;
    z: number;

    /**
     * Sets value of this vector.
     */
    set(x: number, y: number, z: number): Vector3;

    /**
     * Sets all values of this vector.
     */
    setScalar(scalar: number): Vector3;

    /**
     * Sets x value of this vector.
     */
    setX(x: number): Vector3;

    /**
     * Sets y value of this vector.
     */
    setY(y: number): Vector3;

    /**
     * Sets z value of this vector.
     */
    setZ(z: number): Vector3;

    setComponent(index: number, value: number): void;
    getComponent(index: number): number;
    /**
     * Clones this vector.
     */
    clone(): this;
    /**
     * Copies value of v to this vector.
     */
    copy(v: this): this;

    /**
     * Adds v to this vector.
     */
    add(a: Vector3): Vector3;
    addScalar(s: number): Vector3;
    addScaledVector(v: Vector3, s: number): Vector3;

    /**
     * Sets this vector to a + b.
     */
    addVectors(a: Vector3, b: Vector3): Vector3;

    /**
     * Subtracts v from this vector.
     */
    sub(a: Vector3): Vector3;

    subScalar( s: number ): Vector3;

    /**
     * Sets this vector to a - b.
     */
    subVectors(a: Vector3, b: Vector3): Vector3;

    multiply(v: Vector3): Vector3;
    /**
     * Multiplies this vector by scalar s.
     */
    multiplyScalar(s: number): Vector3;
    multiplyVectors(a: Vector3, b: Vector3): Vector3;
    applyEuler(euler: Euler): Vector3;
    applyAxisAngle(axis: Vector3, angle: number): Vector3;
    applyMatrix3(m: Matrix3): Vector3;
    applyMatrix4(m: Matrix4): Vector3;
    applyQuaternion(q: Quaternion): Vector3;
    transformDirection(m: Matrix4): Vector3;
    divide(v: Vector3): Vector3;

    /**
     * Divides this vector by scalar s.
     * Set vector to ( 0, 0, 0 ) if s == 0.
     */
    divideScalar(s: number): Vector3;
    min(v: Vector3): Vector3;
    max(v: Vector3): Vector3;
    clamp(min: Vector3, max: Vector3): Vector3;
    clampScalar(min: number, max: number): Vector3;
    clampLength(min: number, max: number): Vector3;
    floor(): Vector3;
    ceil(): Vector3;
    round(): Vector3;
    roundToZero(): Vector3;

    /**
     * Inverts this vector.
     */
    negate(): Vector3;

    /**
     * Computes dot product of this vector and v.
     */
    dot(v: Vector3): number;

    /**
     * Computes squared length of this vector.
     */
    lengthSq(): number;

    /**
     * Computes length of this vector.
     */
    length(): number;

    /**
     * Computes Manhattan length of this vector.
     * http://en.wikipedia.org/wiki/Taxicab_geometry
     *
     * @deprecated Use {@link Vector3#manhattanLength .manhattanLength()} instead.
     */
    lengthManhattan(): number;

    /**
     * Computes the Manhattan length of this vector.
     *
     * @return {number}
     *
     * @see {@link http://en.wikipedia.org/wiki/Taxicab_geometry|Wikipedia: Taxicab Geometry}
     */
    manhattanLength(): number;

    /**
     * Computes the Manhattan length (distance) from this vector to the given vector v
     *
     * @param {Vector3} v
     *
     * @return {number}
     *
     * @see {@link http://en.wikipedia.org/wiki/Taxicab_geometry|Wikipedia: Taxicab Geometry}
     */
    manhattanDistanceTo(v: Vector3): number;

    /**
     * Normalizes this vector.
     */
    normalize(): Vector3;

    /**
     * Normalizes this vector and multiplies it by l.
     */
    setLength(l: number): Vector3;
    lerp(v: Vector3, alpha: number): Vector3;

    lerpVectors(v1: Vector3, v2: Vector3, alpha: number): Vector3;

    /**
     * Sets this vector to cross product of itself and v.
     */
    cross(a: Vector3): Vector3;

    /**
     * Sets this vector to cross product of a and b.
     */
    crossVectors(a: Vector3, b: Vector3): Vector3;
    projectOnVector(v: Vector3): Vector3;
    projectOnPlane(planeNormal: Vector3): Vector3;
    reflect(vector: Vector3): Vector3;
    angleTo(v: Vector3): number;

    /**
     * Computes distance of this vector to v.
     */
    distanceTo(v: Vector3): number;

    /**
     * Computes squared distance of this vector to v.
     */
    distanceToSquared(v: Vector3): number;

    /**
     * @deprecated Use {@link Vector3#manhattanDistanceTo .manhattanDistanceTo()} instead.
     */
    distanceToManhattan(v: Vector3): number;

    setFromMatrixPosition(m: Matrix4): Vector3;
    setFromMatrixScale(m: Matrix4): Vector3;
    setFromMatrixColumn(matrix: Matrix4, index: number): Vector3;

    /**
     * Checks for strict equality of this vector and v.
     */
    equals(v: Vector3): boolean;

    fromArray(xyz: number[], offset?: number): Vector3;
    toArray(xyz?: number[], offset?: number): number[];

    /**
     * @deprecated Use {@link Vector3#setFromMatrixPosition .setFromMatrixPosition()} instead.
     */
    getPositionFromMatrix(m: Matrix4): Vector3;

    /**
     * @deprecated Use {@link Vector3#setFromMatrixScale .setFromMatrixScale()} instead.
     */
    getScaleFromMatrix(m: Matrix4): Vector3;

    /**
     * @deprecated Use {@link Vector3#setFromMatrixColumn .setFromMatrixColumn()} instead.
     */
    getColumnFromMatrix(index: number, matrix: Matrix4): Vector3;
}

interface Vector3ComponentFieldDesc extends ut.ComponentFieldDesc {
    static readonly x: ut.ComponentFieldDesc;
    static readonly y: ut.ComponentFieldDesc;
    static readonly z: ut.ComponentFieldDesc;
}

/**
 * 4D vector.
 *
 * ( class Vector4 implements Vector<Vector4> )
 */
class Vector4 implements Vector {
    constructor(x?: number, y?: number, z?: number, w?: number);

    x: number;
    y: number;
    z: number;
    w: number;

    /**
     * Sets value of this vector.
     */
    set(x: number, y: number, z: number, w: number): Vector4;

    /**
     * Sets all values of this vector.
     */
    setScalar(scalar: number): Vector4;

    /**
     * Sets X component of this vector.
     */
    setX(x: number): Vector4;

    /**
     * Sets Y component of this vector.
     */
    setY(y: number): Vector4;

    /**
     * Sets Z component of this vector.
     */
    setZ(z: number): Vector4;

    /**
     * Sets w component of this vector.
     */
    setW(w: number): Vector4;

    setComponent(index: number, value: number): void;
    getComponent(index: number): number;
    /**
     * Clones this vector.
     */
    clone(): this;
    /**
     * Copies value of v to this vector.
     */
    copy(v: this): this;

    /**
     * Adds v to this vector.
     */
    add(v: Vector4): Vector4;
    addScalar(s: number): Vector4;

    /**
     * Sets this vector to a + b.
     */
    addVectors(a: Vector4, b: Vector4): Vector4;
    addScaledVector( v: Vector4, s: number ): Vector4;
    /**
     * Subtracts v from this vector.
     */
    sub(v: Vector4): Vector4;

    subScalar(s: number): Vector4;

    /**
     * Sets this vector to a - b.
     */
    subVectors(a: Vector4, b: Vector4): Vector4;

    /**
     * Multiplies this vector by scalar s.
     */
    multiplyScalar(s: number): Vector4;
    applyMatrix4(m: Matrix4): Vector4;

    /**
     * Divides this vector by scalar s.
     * Set vector to ( 0, 0, 0 ) if s == 0.
     */
    divideScalar(s: number): Vector4;

    /**
     * http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm
     * @param q is assumed to be normalized
     */
    setAxisAngleFromQuaternion(q: Quaternion): Vector4;

    /**
     * http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToAngle/index.htm
     * @param m assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)
     */
    setAxisAngleFromRotationMatrix(m: Matrix3): Vector4;

    min(v: Vector4): Vector4;
    max(v: Vector4): Vector4;
    clamp(min: Vector4, max: Vector4): Vector4;
    clampScalar(min: number, max: number): Vector4;
    floor(): Vector4;
    ceil(): Vector4;
    round(): Vector4;
    roundToZero(): Vector4;

    /**
     * Inverts this vector.
     */
    negate(): Vector4;

    /**
     * Computes dot product of this vector and v.
     */
    dot(v: Vector4): number;

    /**
     * Computes squared length of this vector.
     */
    lengthSq(): number;

    /**
     * Computes length of this vector.
     */
    length(): number;

    /**
     * @deprecated Use {@link Vector4#manhattanLength .manhattanLength()} instead.
     */
    lengthManhattan(): number;

    /**
     * Computes the Manhattan length of this vector.
     *
     * @return {number}
     *
     * @see {@link http://en.wikipedia.org/wiki/Taxicab_geometry|Wikipedia: Taxicab Geometry}
     */
    manhattanLength(): number;

    /**
     * Normalizes this vector.
     */
    normalize(): Vector4;
    /**
     * Normalizes this vector and multiplies it by l.
     */
    setLength(length: number): Vector4;

    /**
     * Linearly interpolate between this vector and v with alpha factor.
     */
    lerp(v: Vector4, alpha: number): Vector4;

    lerpVectors(v1: Vector4, v2: Vector4, alpha: number): Vector4;

    /**
     * Checks for strict equality of this vector and v.
     */
    equals(v: Vector4): boolean;

    fromArray(xyzw: number[], offset?: number): Vector4;

    toArray(xyzw?: number[], offset?: number): number[];
}

interface Vector4ComponentFieldDesc extends ut.ComponentFieldDesc {
    static readonly x: ut.ComponentFieldDesc;
    static readonly y: ut.ComponentFieldDesc;
    static readonly z: ut.ComponentFieldDesc;
    static readonly w: ut.ComponentFieldDesc;
}

/**
 * Implementation of a quaternion. This is used for rotating things without incurring in the dreaded gimbal lock issue, amongst other advantages.
 *
 * @example
 * var quaternion = new THREE.Quaternion();
 * quaternion.setFromAxisAngle( new THREE.Vector3( 0, 1, 0 ), Math.PI / 2 );
 * var vector = new THREE.Vector3( 1, 0, 0 );
 * vector.applyQuaternion( quaternion );
 */
class Quaternion {
    /**
     * @param x x coordinate
     * @param y y coordinate
     * @param z z coordinate
     * @param w w coordinate
     */
    constructor(x?: number, y?: number, z?: number, w?: number);

    x: number;
    y: number;
    z: number;
    w: number;

    /**
     * Sets values of this quaternion.
     */
    set(x: number, y: number, z: number, w: number): Quaternion;

    /**
     * Clones this quaternion.
     */
    clone(): this;

    /**
     * Copies values of q to this quaternion.
     */
    copy(q: this): this;

    /**
     * Sets this quaternion from rotation specified by Euler angles.
     */
    setFromEuler(euler: Euler, update?: boolean): Quaternion;

    /**
     * Sets this quaternion from rotation specified by axis and angle.
     * Adapted from http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaternion/index.htm.
     * Axis have to be normalized, angle is in radians.
     */
    setFromAxisAngle(axis: Vector3, angle: number): Quaternion;

    /**
     * Sets this quaternion from rotation component of m. Adapted from http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm.
     */
    setFromRotationMatrix(m: Matrix4): Quaternion;
    setFromUnitVectors(vFrom: Vector3, vTo: Vector3): Quaternion;
    /**
     * Inverts this quaternion.
     */
    inverse(): Quaternion;

    conjugate(): Quaternion;
    dot(v: Quaternion): number;
    lengthSq(): number;

    /**
     * Computes length of this quaternion.
     */
    length(): number;

    /**
     * Normalizes this quaternion.
     */
    normalize(): Quaternion;

    /**
     * Multiplies this quaternion by b.
     */
    multiply(q: Quaternion): Quaternion;
    premultiply(q: Quaternion): Quaternion;

    /**
     * Sets this quaternion to a x b
     * Adapted from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/code/index.htm.
     */
    multiplyQuaternions(a: Quaternion, b: Quaternion): Quaternion;


    slerp(qb: Quaternion, t: number): Quaternion;
    equals(v: Quaternion): boolean;
    fromArray(n: number[]): Quaternion;
    toArray(): number[];

    fromArray(xyzw: number[], offset?: number): Quaternion;
    toArray(xyzw?: number[], offset?: number): number[];

    onChange(callback: Function): Quaternion;
    onChangeCallback: Function;

    /**
     * Adapted from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/slerp/.
     */
    static slerp(qa: Quaternion, qb: Quaternion, qm: Quaternion, t: number): Quaternion;

    static slerpFlat(dst: number[], dstOffset: number, src0: number[], srcOffset: number, src1: number[], stcOffset1: number, t: number): Quaternion;

    /**
     * @deprecated Use {@link Vector#applyQuaternion vector.applyQuaternion( quaternion )} instead.
     */
    multiplyVector3(v: any): any;
}

interface QuaternionComponentFieldDesc extends ut.ComponentFieldDesc {
    static readonly x: ut.ComponentFieldDesc;
    static readonly y: ut.ComponentFieldDesc;
    static readonly z: ut.ComponentFieldDesc;
    static readonly w: ut.ComponentFieldDesc;
}

class Ray {
    constructor(origin?: Vector3, direction?: Vector3);

    origin: Vector3;
    direction: Vector3;

    set(origin: Vector3, direction: Vector3): Ray;
    clone(): this;
    copy(ray: this): this;
    at(t: number, target: Vector3): Vector3;
    lookAt(v: Vector3): Vector3;
    recast(t: number): Ray;
    closestPointToPoint(point: Vector3, target: Vector3): Vector3;
    distanceToPoint(point: Vector3): number;
    distanceSqToPoint(point: Vector3): number;
    distanceSqToSegment(v0: Vector3, v1: Vector3, optionalPointOnRay?: Vector3, optionalPointOnSegment?: Vector3): number;
    distanceToPlane(plane: Plane): number;
    intersectPlane(plane: Plane, target: Vector3): Vector3;
    intersectsPlane(plane: Plane): boolean;
    intersectTriangle(a: Vector3, b: Vector3, c: Vector3, backfaceCulling: boolean, target: Vector3): Vector3;
    applyMatrix4(matrix4: Matrix4): Ray;
    equals(ray: Ray): boolean;

    /**
     * @deprecated Use {@link Ray#intersectsBox .intersectsBox()} instead.
     */
    isIntersectionBox(b: any): any;

    /**
     * @deprecated Use {@link Ray#intersectsPlane .intersectsPlane()} instead.
     */
    isIntersectionPlane(p: any): any;

    /**
     * @deprecated Use {@link Ray#intersectsSphere .intersectsSphere()} instead.
     */
    isIntersectionSphere(s: any): any;
}

/**
 * ( interface Matrix&lt;T&gt; )
 */
interface Matrix {
    /**
     * Float32Array with matrix values.
     */
    elements: Float32Array;

    /**
     * identity():T;
     */
    identity(): Matrix;

    /**
     * copy(m:T):T;
     */
    copy(m: this): this;

    /**
     * multiplyScalar(s:number):T;
     */
    multiplyScalar(s: number): Matrix;

    determinant(): number;

    /**
     * getInverse(matrix:T, throwOnInvertible?:boolean):T;
     */
    getInverse(matrix: Matrix, throwOnInvertible?: boolean): Matrix;

    /**
     * transpose():T;
     */
    transpose(): Matrix;

    /**
     * clone():T;
     */
    clone(): this;
}

/**
 * ( class Matrix3 implements Matrix&lt;Matrix3&gt; )
 */
class Matrix3 implements Matrix {
    /**
     * Creates an identity matrix.
     */
    constructor();

    /**
     * Float32Array with matrix values.
     */
    elements: Float32Array;

    set(n11: number, n12: number, n13: number, n21: number, n22: number, n23: number, n31: number, n32: number, n33: number): Matrix3;
    identity(): Matrix3;
    clone(): this;
    copy(m: this): this;
    setFromMatrix4(m: Matrix4): Matrix3;

    multiplyScalar(s: number): Matrix3;
    determinant(): number;
    getInverse(matrix: Matrix3, throwOnDegenerate?: boolean): Matrix3;

    /**
     * Transposes this matrix in place.
     */
    transpose(): Matrix3;
    getNormalMatrix(matrix4: Matrix4): Matrix3;

    /**
     * Transposes this matrix into the supplied array r, and returns itself.
     */
    transposeIntoArray(r: number[]): number[];
    fromArray(array: number[], offset?: number): Matrix3;
    toArray(): number[];

    /**
     * Multiplies this matrix by m.
     */
    multiply(m: Matrix3): Matrix3;

    premultiply(m: Matrix3): Matrix3;

    /**
     * Sets this matrix to a x b.
     */
    multiplyMatrices(a: Matrix3, b: Matrix3): Matrix3;

    /**
     * @deprecated Use {@link Vector3.applyMatrix3 vector.applyMatrix3( matrix )} instead.
     */
    multiplyVector3(vector: Vector3): any;

    /**
     * @deprecated This method has been removed completely.
     */
    multiplyVector3Array(a: any): any;
    getInverse(matrix: Matrix4, throwOnDegenerate?: boolean): Matrix3;

    /**
     * @deprecated Use {@link Matrix3#toArray .toArray()} instead.
     */
    flattenToArrayOffset(array: number[], offset: number): number[];
}

/**
 * A 4x4 Matrix.
 *
 * @example
 * // Simple rig for rotating around 3 axes
 * var m = new THREE.Matrix4();
 * var m1 = new THREE.Matrix4();
 * var m2 = new THREE.Matrix4();
 * var m3 = new THREE.Matrix4();
 * var alpha = 0;
 * var beta = Math.PI;
 * var gamma = Math.PI/2;
 * m1.makeRotationX( alpha );
 * m2.makeRotationY( beta );
 * m3.makeRotationZ( gamma );
 * m.multiplyMatrices( m1, m2 );
 * m.multiply( m3 );
 */
class Matrix4 implements Matrix {
    constructor();

    /**
     * Float32Array with matrix values.
     */
    elements: Float32Array;

    /**
     * Sets all fields of this matrix.
     */
    set(n11: number, n12: number, n13: number, n14: number, n21: number, n22: number, n23: number, n24: number, n31: number, n32: number, n33: number, n34: number, n41: number, n42: number, n43: number, n44: number): Matrix4;

    /**
     * Resets this matrix to identity.
     */
    identity(): Matrix4;
    clone(): this;
    copy(m: this): this;
    copyPosition(m: Matrix4): Matrix4;
    extractBasis( xAxis: Vector3, yAxis: Vector3, zAxis: Vector3): Matrix4;
    makeBasis( xAxis: Vector3, yAxis: Vector3, zAxis: Vector3): Matrix4;

    /**
     * Copies the rotation component of the supplied matrix m into this matrix rotation component.
     */
    extractRotation(m: Matrix4): Matrix4;
    makeRotationFromEuler(euler: Euler): Matrix4;
    makeRotationFromQuaternion(q: Quaternion): Matrix4;
    /**
     * Constructs a rotation matrix, looking from eye towards center with defined up vector.
     */
    lookAt(eye: Vector3, target: Vector3, up: Vector3): Matrix4;

    /**
     * Multiplies this matrix by m.
     */
    multiply(m: Matrix4): Matrix4;

    premultiply(m: Matrix4): Matrix4;

    /**
     * Sets this matrix to a x b.
     */
    multiplyMatrices(a: Matrix4, b: Matrix4): Matrix4;

    /**
     * Sets this matrix to a x b and stores the result into the flat array r.
     * r can be either a regular Array or a TypedArray.
     *
     * @deprecated This method has been removed completely.
     */
    multiplyToArray(a: Matrix4, b: Matrix4, r: number[]): Matrix4;

    /**
     * Multiplies this matrix by s.
     */
    multiplyScalar(s: number): Matrix4;

    /**
     * Computes determinant of this matrix.
     * Based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm
     */
    determinant(): number;

    /**
     * Transposes this matrix.
     */
    transpose(): Matrix4;

    /**
     * Sets the position component for this matrix from vector v.
     */
    setPosition(v: Vector3): Matrix4;

    /**
     * Sets this matrix to the inverse of matrix m.
     * Based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm.
     */
    getInverse(m: Matrix4, throwOnDegeneratee?: boolean): Matrix4;

    /**
     * Multiplies the columns of this matrix by vector v.
     */
    scale(v: Vector3): Matrix4;

    getMaxScaleOnAxis(): number;
    /**
     * Sets this matrix as translation transform.
     */
    makeTranslation(x: number, y: number, z: number): Matrix4;

    /**
     * Sets this matrix as rotation transform around x axis by theta radians.
     *
     * @param theta Rotation angle in radians.
     */
    makeRotationX(theta: number): Matrix4;

    /**
     * Sets this matrix as rotation transform around y axis by theta radians.
     *
     * @param theta Rotation angle in radians.
     */
    makeRotationY(theta: number): Matrix4;

    /**
     * Sets this matrix as rotation transform around z axis by theta radians.
     *
     * @param theta Rotation angle in radians.
     */
    makeRotationZ(theta: number): Matrix4;

    /**
     * Sets this matrix as rotation transform around axis by angle radians.
     * Based on http://www.gamedev.net/reference/articles/article1199.asp.
     *
     * @param axis Rotation axis.
     * @param theta Rotation angle in radians.
     */
    makeRotationAxis(axis: Vector3, angle: number): Matrix4;

    /**
     * Sets this matrix as scale transform.
     */
    makeScale(x: number, y: number, z: number): Matrix4;

    /**
     * Sets this matrix to the transformation composed of translation, rotation and scale.
     */
    compose(translation: Vector3, rotation: Quaternion, scale: Vector3): Matrix4;

    /**
     * Decomposes this matrix into the translation, rotation and scale components.
     * If parameters are not passed, new instances will be created.
     */
    decompose(translation?: Vector3, rotation?: Quaternion, scale?: Vector3): Object[]; // [Vector3, Quaternion, Vector3]

    /**
     * Creates a frustum matrix.
     */
    makePerspective(left: number, right: number, bottom: number, top: number, near: number, far: number): Matrix4;

    /**
     * Creates a perspective projection matrix.
     */
    makePerspective(fov: number, aspect: number, near: number, far: number): Matrix4;

    /**
     * Creates an orthographic projection matrix.
     */
    makeOrthographic(left: number, right: number, top: number, bottom: number, near: number, far: number): Matrix4;
    equals( matrix: Matrix4 ): boolean;
    fromArray(array: number[], offset?: number): Matrix4;
    toArray(): number[];

    /**
     * @deprecated Use {@link Matrix4#copyPosition .copyPosition()} instead.
     */
    extractPosition(m: Matrix4): Matrix4;

    /**
     * @deprecated Use {@link Matrix4#makeRotationFromQuaternion .makeRotationFromQuaternion()} instead.
     */
    setRotationFromQuaternion(q: Quaternion): Matrix4;

    /**
     * @deprecated Use {@link Vector3#applyMatrix4 vector.applyMatrix4( matrix )} instead.
     */
    multiplyVector3(v: any): any;

    /**
     * @deprecated Use {@link Vector4#applyMatrix4 vector.applyMatrix4( matrix )} instead.
     */
    multiplyVector4(v: any): any;

    /**
     * @deprecated This method has been removed completely.
     */
    multiplyVector3Array(array: number[]): number[];

    /**
     * @deprecated Use {@link Vector3#transformDirection Vector3.transformDirection( matrix )} instead.
     */
    rotateAxis(v: any): void;

    /**
     * @deprecated Use {@link Vector3#applyMatrix4 vector.applyMatrix4( matrix )} instead.
     */
    crossVector(v: any): void;

    /**
     * @deprecated Use {@link Matrix4#toArray .toArray()} instead.
     */
    flattenToArrayOffset(array: number[], offset: number): number[];
}

class Plane {
    constructor(normal?: Vector3, constant?: number);

    normal: Vector3;
    constant: number;

    set(normal: Vector3, constant: number): Plane;
    setComponents(x: number, y: number, z: number, w: number): Plane;
    setFromNormalAndCoplanarPoint(normal: Vector3, point: Vector3): Plane;
    setFromCoplanarPoints(a: Vector3, b: Vector3, c: Vector3): Plane;
    clone(): this;
    copy(plane: this): this;
    normalize(): Plane;
    negate(): Plane;
    distanceToPoint(point: Vector3): number;
    projectPoint(point: Vector3, target: Vector3): Vector3;
    orthoPoint(point: Vector3, target: Vector3): Vector3;
    coplanarPoint(target: Vector3): Vector3;
    applyMatrix4(matrix: Matrix4, optionalNormalMatrix?: Matrix3): Plane;
    translate(offset: Vector3): Plane;
    equals(plane: Plane): boolean;

    /**
     * @deprecated Use {@link Plane#intersectsLine .intersectsLine()} instead.
     */
    isIntersectionLine(l: any): any;
}

class Euler {
    constructor(x?: number, y?: number, z?: number, order?: string);

    x: number;
    y: number;
    z: number;
    order: string;
    onChangeCallback: Function;

    set(x: number, y: number, z: number, order?: string): Euler;
    clone(): this;
    copy(euler: this): this;
    setFromRotationMatrix(m: Matrix4, order?: string, update?: boolean): Euler;
    setFromQuaternion(q: Quaternion, order?: string, update?: boolean): Euler;
    setFromVector3( v: Vector3, order?: string ): Euler;
    reorder(newOrder: string): Euler;
    equals(euler: Euler): boolean;
    fromArray(xyzo: any[]): Euler;
    toArray(array?: number[], offset?: number): number[];
    toVector3(optionalResult?: Vector3): Vector3;
    onChange(callback: Function): void;

    static RotationOrders: string[];
    static DefaultOrder: string;
}

} // namespace utmath

declare namespace ut {
    import Vector2 = utmath.Vector2;
    import Vector3 = utmath.Vector3;
    import Vector4 = utmath.Vector4;
    import Quaternion = utmath.Quaternion;
    import Vector2ComponentFieldDesc = utmath.Vector2ComponentFieldDesc;
    import Vector3ComponentFieldDesc = utmath.Vector3ComponentFieldDesc;
    import Vector4ComponentFieldDesc = utmath.Vector4ComponentFieldDesc;
    import QuaternionComponentFieldDesc = utmath.QuaternionComponentFieldDesc;
    import Matrix3x3 = utmath.Matrix3;
    import Matrix4x4 = utmath.Matrix4;
    import Euler = utmath.Euler;
    import Plane = utmath.Plane;
}

import Vector2 = utmath.Vector2;
import Vector3 = utmath.Vector3;
import Vector4 = utmath.Vector4;
import Quaternion = utmath.Quaternion;
import Vector2ComponentFieldDesc = utmath.Vector2ComponentFieldDesc;
import Vector3ComponentFieldDesc = utmath.Vector3ComponentFieldDesc;
import Vector4ComponentFieldDesc = utmath.Vector4ComponentFieldDesc;
import QuaternionComponentFieldDesc = utmath.QuaternionComponentFieldDesc;
import Matrix3x3 = utmath.Matrix3;
import Matrix4x4 = utmath.Matrix4;
import Euler = utmath.Euler;
import Plane = utmath.Plane;
declare namespace ut.Math {
enum RotationOrder {
  XYZ = 0,
  XZY = 1,
  YZX = 2,
  YXZ = 3,
  ZXY = 4,
  ZYX = 5,
  Default = 4,
}
}
declare namespace ut.Math {

class Rect {
  constructor(x?: number, y?: number, width?: number, height?: number);
  x: number;
  y: number;
  width: number;
  height: number;
  static _size: number;
  static _fromPtr(p: number, v?: Rect): Rect;
  static _toPtr(p: number, v: Rect): void;
  static _tempHeapPtr(v: Rect): number;
}
interface RectComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
  static readonly width: ComponentFieldDesc;
  static readonly height: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class RectInt {
  constructor(x?: number, y?: number, width?: number, height?: number);
  x: number;
  y: number;
  width: number;
  height: number;
  static _size: number;
  static _fromPtr(p: number, v?: RectInt): RectInt;
  static _toPtr(p: number, v: RectInt): void;
  static _tempHeapPtr(v: RectInt): number;
}
interface RectIntComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
  static readonly width: ComponentFieldDesc;
  static readonly height: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Vector2 {
  constructor(x?: number, y?: number);
  x: number;
  y: number;
  static _size: number;
  static _fromPtr(p: number, v?: Vector2): Vector2;
  static _toPtr(p: number, v: Vector2): void;
  static _tempHeapPtr(v: Vector2): number;
}
interface Vector2ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Vector3 {
  constructor(x?: number, y?: number, z?: number);
  x: number;
  y: number;
  z: number;
  static _size: number;
  static _fromPtr(p: number, v?: Vector3): Vector3;
  static _toPtr(p: number, v: Vector3): void;
  static _tempHeapPtr(v: Vector3): number;
}
interface Vector3ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
  static readonly z: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Vector4 {
  constructor(x?: number, y?: number, z?: number, w?: number);
  x: number;
  y: number;
  z: number;
  w: number;
  static _size: number;
  static _fromPtr(p: number, v?: Vector4): Vector4;
  static _toPtr(p: number, v: Vector4): void;
  static _tempHeapPtr(v: Vector4): number;
}
interface Vector4ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
  static readonly z: ComponentFieldDesc;
  static readonly w: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Quaternion {
  constructor(x?: number, y?: number, z?: number, w?: number);
  x: number;
  y: number;
  z: number;
  w: number;
  static _size: number;
  static _fromPtr(p: number, v?: Quaternion): Quaternion;
  static _toPtr(p: number, v: Quaternion): void;
  static _tempHeapPtr(v: Quaternion): number;
}
interface QuaternionComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
  static readonly z: ComponentFieldDesc;
  static readonly w: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Matrix3x3 {
  constructor(m00?: number, m01?: number, m02?: number, m10?: number, m11?: number, m12?: number, m20?: number, m21?: number, m22?: number);
  m00: number;
  m01: number;
  m02: number;
  m10: number;
  m11: number;
  m12: number;
  m20: number;
  m21: number;
  m22: number;
  static _size: number;
  static _fromPtr(p: number, v?: Matrix3x3): Matrix3x3;
  static _toPtr(p: number, v: Matrix3x3): void;
  static _tempHeapPtr(v: Matrix3x3): number;
}
interface Matrix3x3ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly m00: ComponentFieldDesc;
  static readonly m01: ComponentFieldDesc;
  static readonly m02: ComponentFieldDesc;
  static readonly m10: ComponentFieldDesc;
  static readonly m11: ComponentFieldDesc;
  static readonly m12: ComponentFieldDesc;
  static readonly m20: ComponentFieldDesc;
  static readonly m21: ComponentFieldDesc;
  static readonly m22: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Matrix4x4 {
  constructor(m00?: number, m01?: number, m02?: number, m03?: number, m10?: number, m11?: number, m12?: number, m13?: number, m20?: number, m21?: number, m22?: number, m23?: number, m30?: number, m31?: number, m32?: number, m33?: number);
  m00: number;
  m01: number;
  m02: number;
  m03: number;
  m10: number;
  m11: number;
  m12: number;
  m13: number;
  m20: number;
  m21: number;
  m22: number;
  m23: number;
  m30: number;
  m31: number;
  m32: number;
  m33: number;
  static _size: number;
  static _fromPtr(p: number, v?: Matrix4x4): Matrix4x4;
  static _toPtr(p: number, v: Matrix4x4): void;
  static _tempHeapPtr(v: Matrix4x4): number;
}
interface Matrix4x4ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly m00: ComponentFieldDesc;
  static readonly m01: ComponentFieldDesc;
  static readonly m02: ComponentFieldDesc;
  static readonly m03: ComponentFieldDesc;
  static readonly m10: ComponentFieldDesc;
  static readonly m11: ComponentFieldDesc;
  static readonly m12: ComponentFieldDesc;
  static readonly m13: ComponentFieldDesc;
  static readonly m20: ComponentFieldDesc;
  static readonly m21: ComponentFieldDesc;
  static readonly m22: ComponentFieldDesc;
  static readonly m23: ComponentFieldDesc;
  static readonly m30: ComponentFieldDesc;
  static readonly m31: ComponentFieldDesc;
  static readonly m32: ComponentFieldDesc;
  static readonly m33: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class Range {
  constructor(start?: number, end?: number);
  start: number;
  end: number;
  static _size: number;
  static _fromPtr(p: number, v?: Range): Range;
  static _toPtr(p: number, v: Range): void;
  static _tempHeapPtr(v: Range): number;
}
interface RangeComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly start: ComponentFieldDesc;
  static readonly end: ComponentFieldDesc;
}

}
declare namespace ut.Math {

class RangeInt {
  constructor(start?: number, end?: number);
  start: number;
  end: number;
  static _size: number;
  static _fromPtr(p: number, v?: RangeInt): RangeInt;
  static _toPtr(p: number, v: RangeInt): void;
  static _tempHeapPtr(v: RangeInt): number;
}
interface RangeIntComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly start: ComponentFieldDesc;
  static readonly end: ComponentFieldDesc;
}

}

declare namespace Module {

}


declare namespace ut.Shared {
var InputFence: ut.System;
}
declare namespace ut.Shared {
var RenderingFence: ut.System;
}
declare namespace ut.Shared {
var PlatformRenderingFence: ut.System;
}
declare namespace ut.Shared {
var UserCodeStart: ut.System;
}
declare namespace ut.Shared {
var UserCodeEnd: ut.System;
}

declare namespace Module {

}


declare namespace ut.Core2D {
enum ImageStatus {
  Invalid = 0,
  Loaded = 1,
  Loading = 2,
  LoadError = 3,
}
}
declare namespace ut.Core2D {

/** Initialize an image from an asset file
    Once loading has started, the asset loading system will remove this component from the image*/
class Image2DLoadFromFile extends ut.Component {
  constructor(imageFile?: string, maskFile?: string);
  /** the image file/URL to load*/
  imageFile: string;
  /** An image to use as the mask. The red channel will be used as
      the mask; efficient compression can be used (e.g. a single channel PNG
      or paletted PNG8).*/
  maskFile: string;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Image2DLoadFromFile): Image2DLoadFromFile;
  static _toPtr(p: number, v: Image2DLoadFromFile): void;
  static _tempHeapPtr(v: Image2DLoadFromFile): number;
  static _dtorFn(v: Image2DLoadFromFile): void;
}

}
declare namespace ut.Core2D {

/** Tag component that needs to be next to be placed next to an Image2D component
    if it is intended to be used as a render to texture target.*/
class Image2DRenderToTexture extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Image2DRenderToTexture): Image2DRenderToTexture;
  static _toPtr(p: number, v: Image2DRenderToTexture): void;
  static _tempHeapPtr(v: Image2DRenderToTexture): number;
  static _dtorFn(v: Image2DRenderToTexture): void;
}

}
declare namespace ut.Core2D {

class Image2D extends ut.Component {
  constructor(sourceName?: string, pixelsToWorldUnits?: number, disableSmoothing?: boolean, imagePixelSize?: Vector2, hasAlpha?: boolean, status?: ImageStatus);
  /** the source of the image for debugging purposes
      To actually load an image attach any of the Image2DLoadFrom* components.*/
  sourceName: string;
  /** Conversion ratio of image pixels to world units, e.g.:
      The default value is 1. 
      This should not be used for scaling sprites.
      The intended use for this value is loading lower resultion
      images as stand-ins or replacements of high resolution assets on
      lower quality settings or for progressive loading.
      1 : 100 pixels = 100 world units
      1/4: 100 pixels = 25 world units*/
  pixelsToWorldUnits: number;
  /** Disable image bilinear filtering. 
      This is useful for pixel art assets. 
      Defaults to false.*/
  disableSmoothing: boolean;
  /** Image size in pixels 
      Set only after loading (status must be ImageStatus::Loaded)*/
  imagePixelSize: Vector2;
  /** Image contains any alpha values != 1
      Set only after loading (status must be ImageStatus::Loaded)*/
  hasAlpha: boolean;
  /** Load status of the image*/
  status: ImageStatus;
  
  static readonly pixelsToWorldUnits: ComponentFieldDesc;
  static readonly disableSmoothing: ComponentFieldDesc;
  static readonly imagePixelSize: Vector2ComponentFieldDesc;
  static readonly hasAlpha: ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Image2D): Image2D;
  static _toPtr(p: number, v: Image2D): void;
  static _tempHeapPtr(v: Image2D): number;
  static _dtorFn(v: Image2D): void;
}

}
declare namespace ut.Core2D {

/** A component that keeps a read only alpha mask of an Image2D for use in HitBox2D hit
    testing, when pixelAccurate hit testing is enabled. Add the Image2DAlphaMask component next to
    an Image2D component before loading the image.*/
class Image2DAlphaMask extends ut.Component {
  constructor(threshold?: number, pixelData?: number[]);
  /** Threshold value for when a bit is set or not, depending on the alpha value of the adjacent Image2D.
      Default value is .5.*/
  threshold: number;
  /** Array with alpha channel byte values, normalized to 0..1, same size as image.*/
  pixelData: number[];
  static readonly threshold: ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Image2DAlphaMask): Image2DAlphaMask;
  static _toPtr(p: number, v: Image2DAlphaMask): void;
  static _tempHeapPtr(v: Image2DAlphaMask): number;
  static _dtorFn(v: Image2DAlphaMask): void;
}

}

declare namespace Module {

}


declare namespace ut.Core2D {
/** Blending operation when drawing*/
enum BlendOp {
  /** Default. Normal alpha blending.*/
  Alpha = 0,
  /** Additive blending. Only brightens colors. Black is neutral and has no effect..*/
  Add = 1,
  /** Multiplicative blending. Only darken colors. White is neutral and has no effect.*/
  Multiply = 2,
  /** Multiplies the target by the source alpha.
      Only the source alpha channel is used.
      Drawing using this mode is useful when rendering to a textures to mask borders.*/
  MultiplyAlpha = 3,
}
}
declare namespace ut.Core2D {
enum LoopMode {
  /** The value is looped. It goes from the min to the max value. When the value reaches the max value, it starts
      from the beginning. It works in both directions.*/
  Loop = 0,
  /** The value is clamped between min and max value. If the value is equal or larger than max value, the caller
      should be notified about the end of the animation/sequence.*/
  Once = 1,
  /** The value goes between min and max back and forth.*/
  PingPong = 2,
  /** Same as PingPong, but performs only one cycle. If the value is equal or larger than max value, the caller
      should be notified about the end of the animation/sequence.*/
  PingPongOnce = 3,
  /** The value is clamped between min and max value.*/
  ClampForever = 4,
}
}
declare namespace ut.Core2D {

/** RGBA floating-point color.*/
class Color {
  constructor(r?: number, g?: number, b?: number, a?: number);
  /** Red value, range is [0..1]*/
  r: number;
  /** Green value, range is [0..1]*/
  g: number;
  /** Blue value, range is [0..1]*/
  b: number;
  /** Alpha value, range is [0..1]*/
  a: number;
  static _size: number;
  static _fromPtr(p: number, v?: Color): Color;
  static _toPtr(p: number, v: Color): void;
  static _tempHeapPtr(v: Color): number;
}
interface ColorComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly r: ComponentFieldDesc;
  static readonly g: ComponentFieldDesc;
  static readonly b: ComponentFieldDesc;
  static readonly a: ComponentFieldDesc;
}

}
declare namespace ut.Core2D {

/** Add this compoment next to a RectTransform component and a Text2DRenderer (for now)
    while adding a text in a rect transform*/
class RectTransformFinalSize extends ut.Component {
  constructor(size?: Vector2);
  /** Rect transform size of an entity.
      This value is updated by the SetRectTransformSizeSystem system*/
  size: Vector2;
  static readonly size: Vector2ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: RectTransformFinalSize): RectTransformFinalSize;
  static _toPtr(p: number, v: RectTransformFinalSize): void;
  static _tempHeapPtr(v: RectTransformFinalSize): number;
  static _dtorFn(v: RectTransformFinalSize): void;
}

}

declare namespace Module {

}


declare namespace ut.Core2D {
/** Describes potential states for the touch point.*/
enum TouchState {
  Began = 0,
  Moved = 1,
  Stationary = 2,
  Ended = 3,
  Canceled = 4,
}
}
declare namespace ut.Core2D {
enum DisplayOrientation {
  Horizontal = 0,
  Vertical = 1,
}
}
declare namespace ut.Core2D {
/** The rendering mode that DisplayInfo uses.*/
enum RenderMode {
  /** Selects a rendering mode automatically based on available modules and device support.*/
  Auto = 0,
  /** Forces DisplayInfo to use HTML5 canvas rendering.*/
  Canvas = 1,
  /** Forces DisplayInfo to use WebGL rendering.*/
  WebGL = 2,
}
}
declare namespace ut.Core2D {
/** Lists key codes that you can pass to methods such as GetKey, GetKeyDown, and GetKeyUp.*/
enum KeyCode {
  None = 0,
  Backspace = 8,
  Delete = 127,
  Tab = 9,
  Clear = 12,
  Return = 13,
  Pause = 19,
  Escape = 27,
  Space = 32,
  Keypad0 = 256,
  Keypad1 = 257,
  Keypad2 = 258,
  Keypad3 = 259,
  Keypad4 = 260,
  Keypad5 = 261,
  Keypad6 = 262,
  Keypad7 = 263,
  Keypad8 = 264,
  Keypad9 = 265,
  KeypadPeriod = 266,
  KeypadDivide = 267,
  KeypadMultiply = 268,
  KeypadMinus = 269,
  KeypadPlus = 270,
  KeypadEnter = 271,
  KeypadEquals = 272,
  UpArrow = 273,
  DownArrow = 274,
  RightArrow = 275,
  LeftArrow = 276,
  Insert = 277,
  Home = 278,
  End = 279,
  PageUp = 280,
  PageDown = 281,
  F1 = 282,
  F2 = 283,
  F3 = 284,
  F4 = 285,
  F5 = 286,
  F6 = 287,
  F7 = 288,
  F8 = 289,
  F9 = 290,
  F10 = 291,
  F11 = 292,
  F12 = 293,
  F13 = 294,
  F14 = 295,
  F15 = 296,
  Alpha0 = 48,
  Alpha1 = 49,
  Alpha2 = 50,
  Alpha3 = 51,
  Alpha4 = 52,
  Alpha5 = 53,
  Alpha6 = 54,
  Alpha7 = 55,
  Alpha8 = 56,
  Alpha9 = 57,
  Exclaim = 33,
  DoubleQuote = 34,
  Hash = 35,
  Dollar = 36,
  Ampersand = 38,
  Quote = 39,
  LeftParen = 40,
  RightParen = 41,
  Asterisk = 42,
  Plus = 43,
  Comma = 44,
  Minus = 45,
  Period = 46,
  Slash = 47,
  Colon = 58,
  Semicolon = 59,
  Less = 60,
  Equals = 61,
  Greater = 62,
  Question = 63,
  At = 64,
  LeftBracket = 91,
  Backslash = 92,
  RightBracket = 93,
  Caret = 94,
  Underscore = 95,
  BackQuote = 96,
  A = 97,
  B = 98,
  C = 99,
  D = 100,
  E = 101,
  F = 102,
  G = 103,
  H = 104,
  I = 105,
  J = 106,
  K = 107,
  L = 108,
  M = 109,
  N = 110,
  O = 111,
  P = 112,
  Q = 113,
  R = 114,
  S = 115,
  T = 116,
  U = 117,
  V = 118,
  W = 119,
  X = 120,
  Y = 121,
  Z = 122,
  Numlock = 300,
  CapsLock = 301,
  ScrollLock = 302,
  RightShift = 303,
  LeftShift = 304,
  RightControl = 305,
  LeftControl = 306,
  RightAlt = 307,
  LeftAlt = 308,
  LeftCommand = 310,
  LeftApple = 310,
  LeftWindows = 311,
  RightCommand = 309,
  RightApple = 309,
  RightWindows = 312,
  AltGr = 313,
  Help = 315,
  Print = 316,
  SysReq = 317,
  Break = 318,
  Menu = 319,
  Mouse0 = 323,
  Mouse1 = 324,
  Mouse2 = 325,
  Mouse3 = 326,
  Mouse4 = 327,
  Mouse5 = 328,
  Mouse6 = 329,
}
}
declare namespace ut.Core2D {
/** List options for clearing a camera's viewport before rendering. These are
    used by the Camera2D component.*/
enum CameraClearFlags {
  /** Do not clear. Use this when the camera renders to the entire screen,
      and in situations where multiple cameras render to the same screen area.*/
  Nothing = 0,
  /** Clears the viewport with a solid background color.*/
  SolidColor = 1,
}
}
declare namespace ut.Core2D {
/** Lists options for interpreting a camera's culling mask. These are used
    by the Camera2D component.*/
enum CameraCullingMode {
  /** Renders every renderable entity. Nothing is culled.*/
  None = 0,
  /** Renders only entities that have all of their components in cullingMask.*/
  All = 1,
  /** Renders only entities that have at least one of their components in cullingMask.
      This is similar to how Unity's cullingMask works.*/
  Any = 2,
  /** Renders all entities except for those that have the components in cullingMask set.*/
  Exclude = 3,
}
}
declare namespace ut.Core2D {

/** Stores the state for a single touch point.*/
class Touch {
  constructor(deltaX?: number, deltaY?: number, fingerId?: number, phase?: TouchState, x?: number, y?: number);
  /** Specifies the difference, in pixels, between the touch point's X coordinate
      in the current frame and the previous frame. This tells you how far the
      touch point has moved horizontally in the browser or application window.
      Positive values indicate rightward movement, and negative values leftward movement.*/
  deltaX: number;
  /** Specifies the difference, in pixels, between the touch point's Y coordinate
      in the current frame and the previous frame. This tells you how far the
      touch point has moved vertically in the browser or application window.
      Positive values indicate upward movement, and negative values downward movement.*/
  deltaY: number;
  /** A unique identifier for the finger used in a touch interaction.*/
  fingerId: number;
  /** Specifies the life cycle state of this touch. The TouchState
      enum defines the possible values*/
  phase: TouchState;
  /** Specifies the absolute X coordinate of the touch, in pixels on the browser
      or application window. A value of 0 corresponds to the leftmost edge of
      the window. The higher the value, the farther right the coordinate.*/
  x: number;
  /** Specifies the absolute Y coordinate of the touch, in pixels on the browser
      or application window. A value of 0 corresponds to the bottommost edge of
      the window. The higher the value, the farther up the coordinate.*/
  y: number;
  static _size: number;
  static _fromPtr(p: number, v?: Touch): Touch;
  static _toPtr(p: number, v: Touch): void;
  static _tempHeapPtr(v: Touch): number;
}
interface TouchComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly deltaX: ComponentFieldDesc;
  static readonly deltaY: ComponentFieldDesc;
  static readonly fingerId: ComponentFieldDesc;
  
  static readonly x: ComponentFieldDesc;
  static readonly y: ComponentFieldDesc;
}

}
declare namespace ut.Core2D {

class DisplayInfo extends ut.Component {
  constructor(width?: number, height?: number, autoSizeToFrame?: boolean, frameWidth?: number, frameHeight?: number, screenWidth?: number, screenHeight?: number, screenDpiScale?: number, orientation?: DisplayOrientation, renderMode?: RenderMode, focused?: boolean, visible?: boolean);
  /** Specifies the output width, in pixels.*/
  width: number;
  /** Specifies the output height, in pixels.*/
  height: number;
  /** If set to true, the output automatically resizes to fill the frame
      (the browser or application window), and match the orientation.
      Changing output width or height manually has no effect.*/
  autoSizeToFrame: boolean;
  /** Specifies the frame width, in pixels. This is the width of the browser
      or application window.*/
  frameWidth: number;
  /** Specifies the frame height, in pixels. This is the height of the browser
      or application window.*/
  frameHeight: number;
  /** Specifies the device display (screen) width, in pixels.*/
  screenWidth: number;
  /** Specifies the device display (screen) height, in pixels.*/
  screenHeight: number;
  /** Specifies the scale of the device display (screen) DPI relative to.
      96 DPI. For example, a value of 2.0 yields 192 DPI (200% of 96).*/
  screenDpiScale: number;
  /** Specifies the device display (screen) orientation. Can be Horizontal
      or Vertical.*/
  orientation: DisplayOrientation;
  /** Forces DisplayInfo to use a specific renderer. This allows
      switching between WebGL and Canvas rendering in the HTML runtime.
      The RenderMode enum defines Possible renderers. The default is Auto.
      Switching renderers at runtime is usually possible, but may not be seamless.*/
  renderMode: RenderMode;
  /** Specifies whether the browser or application window has focus.*/
  focused: boolean;
  /** Specifies whether the browser or application window is currently visible
      on the screen/device display.*/
  visible: boolean;
  static readonly width: ComponentFieldDesc;
  static readonly height: ComponentFieldDesc;
  static readonly autoSizeToFrame: ComponentFieldDesc;
  static readonly frameWidth: ComponentFieldDesc;
  static readonly frameHeight: ComponentFieldDesc;
  static readonly screenWidth: ComponentFieldDesc;
  static readonly screenHeight: ComponentFieldDesc;
  static readonly screenDpiScale: ComponentFieldDesc;
  
  
  static readonly focused: ComponentFieldDesc;
  static readonly visible: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: DisplayInfo): DisplayInfo;
  static _toPtr(p: number, v: DisplayInfo): void;
  static _tempHeapPtr(v: DisplayInfo): number;
  static _dtorFn(v: DisplayInfo): void;
}

}
declare namespace ut.Core2D {

class Camera2D extends ut.Component {
  constructor(halfVerticalSize?: number, rect?: Rect, backgroundColor?: Color, clearFlags?: CameraClearFlags, depth?: number, cullingMode?: CameraCullingMode, cullingMask?: number[]);
  /** Specifies half of the vertical size in world units. The horizontal
      size is calculated from the output display size and aspect ratio.
      The Transform component's world position defines the camera origin's
      location, size, and rotation.*/
  halfVerticalSize: number;
  /** Specifies the coordinates of the viewport rectangle in the
      output frame that this camera should render to.*/
  rect: Rect;
  /** Specifies the background color this camera should render before rendering
      the scene when clearFlags is set to SolidColor.*/
  backgroundColor: Color;
  /** Specifies how to clear this camera's viewport before rendering.*/
  clearFlags: CameraClearFlags;
  /** The camera's render order relative to other cameras. Cameras with
      lower values render first. Cameras with higher values render overtop
      of cameras with lower vlaues.*/
  depth: number;
  /** Specifies which CameraCullingMode option this camera uses to
      interpret its cullingMask.*/
  cullingMode: CameraCullingMode;
  /** A component mask that specifies which components to consider when culling
      entities according to the cullingMode.*/
  cullingMask: number[];
  static readonly halfVerticalSize: ComponentFieldDesc;
  static readonly rect: RectComponentFieldDesc;
  static readonly backgroundColor: ColorComponentFieldDesc;
  
  static readonly depth: ComponentFieldDesc;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Camera2D): Camera2D;
  static _toPtr(p: number, v: Camera2D): void;
  static _tempHeapPtr(v: Camera2D): number;
  static _dtorFn(v: Camera2D): void;
}

}
declare namespace ut.Core2D {

class Camera2DRenderToTexture extends ut.Component {
  constructor(width?: number, height?: number, freeze?: boolean, target?: Entity);
  /** Width of the target render texture. Must be a power of two.*/
  width: number;
  /** Height of the target render texture. Must be a power of two.*/
  height: number;
  /** Freeze render to texture operation. Setting this flag to true skips re-rendering the texture.
      It is a lightweight way to temporarily disable render to texture for this camera without de-allocations.*/
  freeze: boolean;
  /** The target entity to render to. If NONE, this entity is used.
      The target entity must have an Image2D and an Image2DRenderToTexture component.*/
  target: Entity;
  static readonly width: ComponentFieldDesc;
  static readonly height: ComponentFieldDesc;
  static readonly freeze: ComponentFieldDesc;
  static readonly target: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Camera2DRenderToTexture): Camera2DRenderToTexture;
  static _toPtr(p: number, v: Camera2DRenderToTexture): void;
  static _tempHeapPtr(v: Camera2DRenderToTexture): number;
  static _dtorFn(v: Camera2DRenderToTexture): void;
}

}
declare namespace ut.Core2D {

/** Add this component to an entity with a Camera2D component to change the
    default sorting axis.*/
class Camera2DAxisSort extends ut.Component {
  constructor(axis?: Vector3);
  /** A sorting axis selector that uses the dot product of the camera-space
      position and this vector as a sorting value.
      The default z axis sorting uses (0,0,1).
      For negative y sorting, required for some 2D setups, use (0,-1,0).
      Isometric perspectives may require unusual combinations such as (1,1,0).*/
  axis: Vector3;
  static readonly axis: Vector3ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Camera2DAxisSort): Camera2DAxisSort;
  static _toPtr(p: number, v: Camera2DAxisSort): void;
  static _tempHeapPtr(v: Camera2DAxisSort): number;
  static _dtorFn(v: Camera2DAxisSort): void;
}

}
declare namespace ut.Core2D {

/** Add this component to an entity with a Camera2D component to specify the
    distances from the camera to its near and far clipping planes. The camera
    draws elements between the clipping planes and ignores elements outside of them.*/
class Camera2DClippingPlanes extends ut.Component {
  constructor(near?: number, far?: number);
  /** The distance to the near clipping plane. The camera does not draw anything
      closer than this distance.*/
  near: number;
  /** The distance to the far clipping plane. The camera does not draw anything
      beyond this distance.*/
  far: number;
  static readonly near: ComponentFieldDesc;
  static readonly far: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Camera2DClippingPlanes): Camera2DClippingPlanes;
  static _toPtr(p: number, v: Camera2DClippingPlanes): void;
  static _tempHeapPtr(v: Camera2DClippingPlanes): number;
  static _dtorFn(v: Camera2DClippingPlanes): void;
}

}
declare namespace ut.Core2D {

/** This component is required for an entity to be part of the transform hierarchy.
    Entities without this component are not rendered.
    A root entity's parent entity can be NONE.*/
class TransformNode extends ut.Component {
  constructor(parent?: Entity);
  parent: Entity;
  static readonly parent: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformNode): TransformNode;
  static _toPtr(p: number, v: TransformNode): void;
  static _tempHeapPtr(v: TransformNode): number;
  static _dtorFn(v: TransformNode): void;
}

}
declare namespace ut.Core2D {

/** This is an optional component that specifies a position transform.
    If it is not attached to the entity, no local translation is assumed.*/
class TransformLocalPosition extends ut.Component {
  constructor(position?: Vector3);
  position: Vector3;
  static readonly position: Vector3ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformLocalPosition): TransformLocalPosition;
  static _toPtr(p: number, v: TransformLocalPosition): void;
  static _tempHeapPtr(v: TransformLocalPosition): number;
  static _dtorFn(v: TransformLocalPosition): void;
}

}
declare namespace ut.Core2D {

/** This is an optional component that specifies a quaternion rotation transform.
    If it is not added to the entity, the unit quaternion rotation is assumed.*/
class TransformLocalRotation extends ut.Component {
  constructor(rotation?: Quaternion);
  rotation: Quaternion;
  static readonly rotation: QuaternionComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformLocalRotation): TransformLocalRotation;
  static _toPtr(p: number, v: TransformLocalRotation): void;
  static _tempHeapPtr(v: TransformLocalRotation): number;
  static _dtorFn(v: TransformLocalRotation): void;
}

}
declare namespace ut.Core2D {

/** This is an optional component that specifies a three-axis scale transform.
    It is optional. If it is not added to the entity, the unit scale (1,1,1) is assumed.*/
class TransformLocalScale extends ut.Component {
  constructor(scale?: Vector3);
  scale: Vector3;
  static readonly scale: Vector3ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformLocalScale): TransformLocalScale;
  static _toPtr(p: number, v: TransformLocalScale): void;
  static _tempHeapPtr(v: TransformLocalScale): number;
  static _dtorFn(v: TransformLocalScale): void;
}

}
declare namespace ut.Core2D {

/** The UpdateLocalTransformSystem system adds and updates this component, which
    provides direct access to the cached object space transform. The object
    transform is computed from the contents of the TransformLocalScale,
    TransformLocalRotation, and TransformLocalPosition components. This component
    is readable and writeable.*/
class TransformLocal extends ut.Component {
  constructor(matrix?: Matrix4x4);
  matrix: Matrix4x4;
  static readonly matrix: Matrix4x4ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformLocal): TransformLocal;
  static _toPtr(p: number, v: TransformLocal): void;
  static _tempHeapPtr(v: TransformLocal): number;
  static _dtorFn(v: TransformLocal): void;
}

}
declare namespace ut.Core2D {

/** The UpdateWorldTransformSystem system adds and updates this component, which
    provides direct access to the cached Object to World space transform. This
    component is readable and writeable.*/
class TransformObjectToWorld extends ut.Component {
  constructor(matrix?: Matrix4x4);
  matrix: Matrix4x4;
  static readonly matrix: Matrix4x4ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformObjectToWorld): TransformObjectToWorld;
  static _toPtr(p: number, v: TransformObjectToWorld): void;
  static _tempHeapPtr(v: TransformObjectToWorld): number;
  static _dtorFn(v: TransformObjectToWorld): void;
}

}
declare namespace ut.Core2D {

/** This is a flag component that marks the start of a sorting group in a hierarchy.
    You only need to add this component to the head node (entity) in a group, but
    that entity must also have a Transform component.
    
    Sorting groups allow you to sort the group head's descendants indepenently.
    Whatever sorting you apply to the overall hierarchy also happens locally
    within the group.*/
class SortingGroup extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: SortingGroup): SortingGroup;
  static _toPtr(p: number, v: SortingGroup): void;
  static _tempHeapPtr(v: SortingGroup): number;
  static _dtorFn(v: SortingGroup): void;
}

}
declare namespace ut.Core2D {

/** This is a flag component that marks an entity's transform as static. The
    entity you add it to must have a Transform component.
    If an entity transform is marked as static, both its object and world
    transforms are computed once and not updated again.
    You can remove the static transform marker at any time to resume transform
    computations.
    Note that the TransformStatic tag has no effect on uncached compute functions.*/
class TransformStatic extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TransformStatic): TransformStatic;
  static _toPtr(p: number, v: TransformStatic): void;
  static _tempHeapPtr(v: TransformStatic): number;
  static _dtorFn(v: TransformStatic): void;
}

}
declare namespace ut.Core2D {

/** This component tags a node with two additional values that allow sorting
    by layer and order. The default value for both is 0. The entity you add
    this component to must have a Transform component.
    
    Entities with a higher layer value overlay entities with a lower layer value.
    The order value defines how entities with the same layer value are ordered.
    As with layers, entities with a higher order value overlay those with a lower
    order value.
    
    Layer sorting happens before order sorting, and both happen before axis
    sorting. Axis sorting only happens when entitied shave the same layer and
    order values.
    
    LayerSorting values are not propagated through the hierarchy.*/
class LayerSorting extends ut.Component {
  constructor(layer?: number, order?: number);
  /** First, sort by layer.*/
  layer: number;
  /** If layer values are equal, sort by order.*/
  order: number;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LayerSorting): LayerSorting;
  static _toPtr(p: number, v: LayerSorting): void;
  static _tempHeapPtr(v: LayerSorting): number;
  static _dtorFn(v: LayerSorting): void;
}

}
declare namespace ut.Core2D {
class Input {
  /** Returns true if the key is currently held down.*/
  static getKey(key: KeyCode): boolean;
  /** Returns true if the key was pressed in the current frame.*/
  static getKeyDown(key: KeyCode): boolean;
  /** Returns true if the key was released in the current frame.*/
  static getKeyUp(key: KeyCode): boolean;
  /** Returns true if the mouse button is currently held down.*/
  static getMouseButton(button: number): boolean;
  /** Returns true if the mouse button was pressed in the current frame.*/
  static getMouseButtonDown(button: number): boolean;
  /** Returns true if the mouse button was released in the current frame.*/
  static getMouseButtonUp(button: number): boolean;
  /** Returns true if the current device produces mouse input.*/
  static isMousePresent(): boolean;
  /** Returns true if mouse events are emulated by other input devices, such as touch.*/
  static isMouseEmulated(): boolean;
  /** Returns true if the current device produces touch input responses.
      This value may not be accurate until a first touch occurs.*/
  static isTouchSupported(): boolean;
  /** Returns the number of currently active touches.*/
  static touchCount(): number;
  /** Retrieves information for a specific touch point. The index ranges
      from 0 to the value returned by TouchCount.*/
  static getTouch(index: number): Touch;
  /** Returns the input position in screen pixels. For touch input this is
      the first touch. For mouse input, it is the mouse position.*/
  static getInputPosition(): Vector2;
  /** Convenience function that returns the value from GetInputPosition transformed
      into world space. World space includes the camera transform of the camera
      closest to the input position.
      This is the same as TranslateScreenToWorld(world, GetInputPosition());
      In 2D setups, the world z coordinate is always set to 0.*/
  static getWorldInputPosition(world: ut.WorldBase): Vector3;
  /** Transforms Screen coordinates into World coordinates.
      World space includes the camera transform of the camera closest to
      the input coordinate.
      In 2D setups, the world z coordinate is always set to 0.*/
  static translateScreenToWorld(world: ut.WorldBase, screenCoord: Vector2): Vector3;
}
}
declare namespace ut.Core2D {
/** Provides a variety of convenience functions for transforms.*/
class TransformService {
  /** Finds a child entity of node with the given name. You can search deeper in the
      hierarchy to find a child of a child entity, a child of that child,
      and so on by separating the levels with forward slashes (/) in the name.
      
      For example Find(node, "a/b/c") is the same as Find(Find(Find(node,"a"),"b"),"c");
      
      Initial lookups can be slow, but are cached if successful.
      Failed lookups are not cached.*/
  static find(world: ut.WorldBase, node: Entity, name: string): Entity;
  /** Completely removes this node from the hierarchy, and sets the parent
      of all of its children to NONE.
      Avoid using this function in inner loops, where it can be very slow.*/
  static unlink(world: ut.WorldBase, node: Entity): void;
  /** Completely removes this node from the hierarchy, and sets the parent
      of all of its children to NONE.
      This function is like Unlink, but takes an explicit entity command buffer.
      Avoid using this function in inner loops, where it can be very slow.*/
  static unlinkDeferred(world: ut.WorldBase, ecb: ut.EntityCommandBufferBase, node: Entity): void;
  /** Sets the parent of all of node's child entities to NONE.
      Avoid using this function in inner loops, where it can be very slow.*/
  static removeAllChildren(world: ut.WorldBase, node: Entity): void;
  /** Sets the parent of all of node's child entities to NONE.
      This function is like RemoveAllChildren, but takes an explicit entity
      command buffer.
      Avoid using this function in inner loops, where it can be very slow.*/
  static removeAllChildrenDeferred(world: ut.WorldBase, ecb: ut.EntityCommandBufferBase, node: Entity): void;
  /** Returns the number of children that node has.
      Avoid using this function in inner loops, where it can be very slow.*/
  static countChildren(world: ut.WorldBase, node: Entity): number;
  /** Returns a child entity at the given index. If the
      index is out of range, returns Entity.NONE.
      Note that because the order of children can change at any time, a given
      index does not return the same child every time. In most cases, you
      can obtain better results by using the Find function or caching children
      locally.
      Avoid using this function in inner loops, where it can be very slow.*/
  static getChild(world: ut.WorldBase, node: Entity, index?: number): Entity;
  /** Destroys all of node's children recursively.
      The destroySelf parameter specifies whether to also destroy the entity
      along with its children.
      This function can be slow. Use it sparingly.*/
  static destroyTree(world: ut.WorldBase, node: Entity, destroySelf?: boolean): void;
  /** Destroys all of node's children recursively.
      The destroySelf parameter specifies whether to also destroy the entity
      along with its children.
      This function is like DestroyTree, but takes an explicit entity command buffer.
      This function can be slow. Use it sparingly.*/
  static destroyTreeDeferred(world: ut.WorldBase, ecb: ut.EntityCommandBufferBase, node: Entity, destroySelf?: boolean): void;
  /** Clones the entire hierarchy tree, including node.
      Returns the clone of node. All cloned descendants are in the new hierarchy.
      This function can be slow. Use it sparingly.*/
  static cloneTree(world: ut.WorldBase, node: Entity): Entity;
  /** Clones the entire hierarchy tree, including node.
      Returns the clone of node. All cloned descendants are in the new hierarchy.
      This function is like CloneTree, but takes an explicit entity command buffer.
      This function can be slow. Use it sparingly.*/
  static cloneTreeDeferred(world: ut.WorldBase, ecb: ut.EntityCommandBufferBase, node: Entity): Entity;
  /** Computes a new local matrix of the kind produced in the TransformLocal
      component.
      This function does not use TransformLocal, and can be quite resource
      intensive. Use it only if the cached result in TransformLocal is not available.*/
  static computeLocalMatrix(world: ut.WorldBase, node: Entity): Matrix4x4;
  /** Computes a new Object to World matrix of the kind produced in the TransformObjectToWorld
      component. This function does not use TransformObjectToWorld, and can be quite
      resource intensive. Use it only if the cached result in TransformLocal
      is not available.*/
  static computeWorldMatrix(world: ut.WorldBase, node: Entity): Matrix4x4;
  /** Computes a transform node's world position.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), and returns the position of that.
      
      This function can be quite resource intensive. When possible, read the
      TransformObjectToWorld component instead.*/
  static computeWorldPosition(world: ut.WorldBase, node: Entity): Vector3;
  /** Computes a transform node's world rotation.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), and returns the rotation of that.
      
      This function can be quite resource intensive. When possible, read the
      TransformObjectToWorld component instead.*/
  static computeWorldRotation(world: ut.WorldBase, node: Entity): Quaternion;
  /** Computes a transform node's world scale.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), and returns the scale of that.
      
      Because lossy scale does not include skew, it does not include enough
      information to recreate a full 4x4 matrix from rotation, position, and
      lossy scale in 3D.
      
      This function can be quite resource intensive. If possible, read the
      TransformObjectToWorld component instead.*/
  static computeWorldScaleLossy(world: ut.WorldBase, node: Entity): Vector3;
  /** Computes a transform node's world scale.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), and returns the scale of that.
      
      If the transform does not include skew, use computeWorldScaleLossy instead.
      
      This function can be quite resource intensive. If possible, read the
      TransformObjectToWorld component instead.*/
  static computeWorldScale(world: ut.WorldBase, node: Entity): Matrix3x3;
  /** Inverse-transforms a position by the TransformObjectToWorld matrix.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), then transforms by that.
      
      This function can be quite resource intensive. When possible, read the
      TransformObjectToWorld component instead.*/
  static localPositionFromWorldPosition(world: ut.WorldBase, node: Entity, position: Vector3): Vector3;
  /** Inverse-transforms a scale by the TransformObjectToWorld matrix.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), then transforms by that.
      
      This function can be quite resource intensive. When possible, read the
      TransformObjectToWorld component instead.*/
  static localScaleFromWorldScale(world: ut.WorldBase, node: Entity, scale: Vector3): Vector3;
  /** Inverse-transforms a rotation by the TransformObjectToWorld matrix.
      This function uses the computeWorldMatrix function to compute the world
      matrix (bypassing caching), then transforms by that.
      
      This function can be quite resource intensive. When possible, read the
      TransformObjectToWorld component instead.*/
  static localRotationFromWorldRotation(world: ut.WorldBase, node: Entity, rotation: Quaternion): Quaternion;
  /** Helper function for transforming from window to world coordinates.
      This only works if cameraEntity has a valid TransformObjectToWorld
      component. The windowPos (window position) and windowSize (window size) should be
      in the same coordinates. The Z coordinate is set to 0.*/
  static windowToWorld(world: ut.WorldBase, cameraEntity: Entity, windowPos: Vector2, windowSize: Vector2): Vector3;
  /** Helper function for transforming from world to window coordinates.
      This only works if the cameraEntity has a valid TransformObjectToWorld
      component. Returns windowPos (window position) and windowSize (window size) in
      the same coordinates.*/
  static worldToWindow(world: ut.WorldBase, cameraEntity: Entity, worldPos: Vector3, windowSize: Vector2): Vector2;
}
}
declare namespace ut.Core2D {
/** Collects and sorts entities by camera, and readies them for rendering.
    Rendering and hit testing do not consider changes made to entities after
    the display list is created.*/
var DisplayList: ut.System;
}
declare namespace ut.Core2D {
var UpdateLocalTransformSystem: ut.System;
}
declare namespace ut.Core2D {
var UpdateWorldTransformSystem: ut.System;
}

declare namespace Module {
function _ut_Core2D_Input_GetKey(key: any): any;
function _ut_Core2D_Input_GetKeyDown(key: any): any;
function _ut_Core2D_Input_GetKeyUp(key: any): any;
function _ut_Core2D_Input_GetMouseButton(button: any): any;
function _ut_Core2D_Input_GetMouseButtonDown(button: any): any;
function _ut_Core2D_Input_GetMouseButtonUp(button: any): any;
function _ut_Core2D_Input_IsMousePresent(): any;
function _ut_Core2D_Input_IsMouseEmulated(): any;
function _ut_Core2D_Input_IsTouchSupported(): any;
function _ut_Core2D_Input_TouchCount(): any;
function _ut_Core2D_Input_GetTouch(index: any): void;
function _ut_Core2D_Input_GetInputPosition(): void;
function _ut_Core2D_Input_GetWorldInputPosition(world: any): void;
function _ut_Core2D_Input_TranslateScreenToWorld(world: any, screenCoord: any): void;
function _ut_Core2D_TransformService_Find(world: any, node: any, name: any): void;
function _ut_Core2D_TransformService_Unlink(world: any, node: any): void;
function _ut_Core2D_TransformService_UnlinkDeferred(world: any, ecb: any, node: any): void;
function _ut_Core2D_TransformService_RemoveAllChildren(world: any, node: any): void;
function _ut_Core2D_TransformService_RemoveAllChildrenDeferred(world: any, ecb: any, node: any): void;
function _ut_Core2D_TransformService_CountChildren(world: any, node: any): any;
function _ut_Core2D_TransformService_GetChild(world: any, node: any, index: any): void;
function _ut_Core2D_TransformService_DestroyTree(world: any, node: any, destroySelf: any): void;
function _ut_Core2D_TransformService_DestroyTreeDeferred(world: any, ecb: any, node: any, destroySelf: any): void;
function _ut_Core2D_TransformService_CloneTree(world: any, node: any): void;
function _ut_Core2D_TransformService_CloneTreeDeferred(world: any, ecb: any, node: any): void;
function _ut_Core2D_TransformService_computeLocalMatrix(world: any, node: any): void;
function _ut_Core2D_TransformService_computeWorldMatrix(world: any, node: any): void;
function _ut_Core2D_TransformService_computeWorldPosition(world: any, node: any): void;
function _ut_Core2D_TransformService_computeWorldRotation(world: any, node: any): void;
function _ut_Core2D_TransformService_computeWorldScaleLossy(world: any, node: any): void;
function _ut_Core2D_TransformService_computeWorldScale(world: any, node: any): void;
function _ut_Core2D_TransformService_localPositionFromWorldPosition(world: any, node: any, position: any): void;
function _ut_Core2D_TransformService_localScaleFromWorldScale(world: any, node: any, scale: any): void;
function _ut_Core2D_TransformService_localRotationFromWorldRotation(world: any, node: any, rotation: any): void;
function _ut_Core2D_TransformService_windowToWorld(world: any, cameraEntity: any, windowPos: any, windowSize: any): void;
function _ut_Core2D_TransformService_worldToWindow(world: any, cameraEntity: any, worldPos: any, windowSize: any): void;

}


declare namespace ut.Interpolation {

/** Contains float keyframes for Bezier interpolation.*/
class BezierCurveFloat extends ut.Component {
  constructor(times?: number[], values?: number[], outValues?: number[], inValues?: number[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points that lie on the curve.*/
  values: number[];
  /** Array of outValues for points on the curve.
      outValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The outValue
      determines the shape of the curve after its corresponding point.*/
  outValues: number[];
  /** Array of inValues for points on the curve.
      inValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The inValue
      determines the shape of the curve before its corresponding point.*/
  inValues: number[];
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BezierCurveFloat): BezierCurveFloat;
  static _toPtr(p: number, v: BezierCurveFloat): void;
  static _tempHeapPtr(v: BezierCurveFloat): number;
  static _dtorFn(v: BezierCurveFloat): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector2 keyframes for Bezier interpolation.*/
class BezierCurveVector2 extends ut.Component {
  constructor(times?: number[], values?: Vector2[], outValues?: Vector2[], inValues?: Vector2[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points that lie on the curve.*/
  values: Vector2[];
  /** Array of outValues for points on the curve.
      outValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The outValue
      determines the shape of the curve after its corresponding point.*/
  outValues: Vector2[];
  /** Array of inValues for points on the curve.
      inValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The inValue
      determines the shape of the curve before its corresponding point.*/
  inValues: Vector2[];
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BezierCurveVector2): BezierCurveVector2;
  static _toPtr(p: number, v: BezierCurveVector2): void;
  static _tempHeapPtr(v: BezierCurveVector2): number;
  static _dtorFn(v: BezierCurveVector2): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector3 keyframes for Bezier interpolation.*/
class BezierCurveVector3 extends ut.Component {
  constructor(times?: number[], values?: Vector3[], outValues?: Vector3[], inValues?: Vector3[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points that lie on the curve.*/
  values: Vector3[];
  /** Array of outValues for points on the curve.
      outValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The outValue
      determines the shape of the curve after its corresponding point.*/
  outValues: Vector3[];
  /** Array of inValues for points on the curve.
      inValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The inValue
      determines the shape of the curve before its corresponding point.*/
  inValues: Vector3[];
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BezierCurveVector3): BezierCurveVector3;
  static _toPtr(p: number, v: BezierCurveVector3): void;
  static _tempHeapPtr(v: BezierCurveVector3): number;
  static _dtorFn(v: BezierCurveVector3): void;
}

}
declare namespace ut.Interpolation {

/** Contains Quaternion keyframes for Bezier interpolation.*/
class BezierCurveQuaternion extends ut.Component {
  constructor(times?: number[], values?: Quaternion[], outValues?: Quaternion[], inValues?: Quaternion[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points that lie on the curve.*/
  values: Quaternion[];
  /** Array of outValues for points on the curve.
      outValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The outValue
      determines the shape of the curve after its corresponding point.*/
  outValues: Quaternion[];
  /** Array of inValues for points on the curve.
      inValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The inValue
      determines the shape of the curve before its corresponding point.*/
  inValues: Quaternion[];
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BezierCurveQuaternion): BezierCurveQuaternion;
  static _toPtr(p: number, v: BezierCurveQuaternion): void;
  static _tempHeapPtr(v: BezierCurveQuaternion): number;
  static _dtorFn(v: BezierCurveQuaternion): void;
}

}
declare namespace ut.Interpolation {

/** Contains Color keyframes for Bezier interpolation.*/
class BezierCurveColor extends ut.Component {
  constructor(times?: number[], values?: Color[], outValues?: Color[], inValues?: Color[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points that lie on the curve.*/
  values: Color[];
  /** Array of outValues for points on the curve.
      outValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The outValue
      determines the shape of the curve after its corresponding point.*/
  outValues: Color[];
  /** Array of inValues for points on the curve.
      inValues represent control points that do not lie on the curve, but
      correspond to control points that do lie on the curve. The inValue
      determines the shape of the curve before its corresponding point.*/
  inValues: Color[];
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BezierCurveColor): BezierCurveColor;
  static _toPtr(p: number, v: BezierCurveColor): void;
  static _tempHeapPtr(v: BezierCurveColor): number;
  static _dtorFn(v: BezierCurveColor): void;
}

}
declare namespace ut.Interpolation {

/** Contains float keyframes for linear interpolation.*/
class LinearCurveFloat extends ut.Component {
  constructor(times?: number[], values?: number[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: number[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LinearCurveFloat): LinearCurveFloat;
  static _toPtr(p: number, v: LinearCurveFloat): void;
  static _tempHeapPtr(v: LinearCurveFloat): number;
  static _dtorFn(v: LinearCurveFloat): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector2 keyframes for linear interpolation.*/
class LinearCurveVector2 extends ut.Component {
  constructor(times?: number[], values?: Vector2[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Vector2[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LinearCurveVector2): LinearCurveVector2;
  static _toPtr(p: number, v: LinearCurveVector2): void;
  static _tempHeapPtr(v: LinearCurveVector2): number;
  static _dtorFn(v: LinearCurveVector2): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector3 keyframes for linear interpolation.*/
class LinearCurveVector3 extends ut.Component {
  constructor(times?: number[], values?: Vector3[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Vector3[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LinearCurveVector3): LinearCurveVector3;
  static _toPtr(p: number, v: LinearCurveVector3): void;
  static _tempHeapPtr(v: LinearCurveVector3): number;
  static _dtorFn(v: LinearCurveVector3): void;
}

}
declare namespace ut.Interpolation {

/** Contains Quaternion keyframes for linear interpolation.*/
class LinearCurveQuaternion extends ut.Component {
  constructor(times?: number[], values?: Quaternion[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Quaternion[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LinearCurveQuaternion): LinearCurveQuaternion;
  static _toPtr(p: number, v: LinearCurveQuaternion): void;
  static _tempHeapPtr(v: LinearCurveQuaternion): number;
  static _dtorFn(v: LinearCurveQuaternion): void;
}

}
declare namespace ut.Interpolation {

/** Contains Color keyframes for linear interpolation.*/
class LinearCurveColor extends ut.Component {
  constructor(times?: number[], values?: Color[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Color[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LinearCurveColor): LinearCurveColor;
  static _toPtr(p: number, v: LinearCurveColor): void;
  static _tempHeapPtr(v: LinearCurveColor): number;
  static _dtorFn(v: LinearCurveColor): void;
}

}
declare namespace ut.Interpolation {

/** Contains float keyframes for step interpolation.*/
class StepCurveFloat extends ut.Component {
  constructor(times?: number[], values?: number[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: number[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: StepCurveFloat): StepCurveFloat;
  static _toPtr(p: number, v: StepCurveFloat): void;
  static _tempHeapPtr(v: StepCurveFloat): number;
  static _dtorFn(v: StepCurveFloat): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector2 keyframes for step interpolation.*/
class StepCurveVector2 extends ut.Component {
  constructor(times?: number[], values?: Vector2[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Vector2[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: StepCurveVector2): StepCurveVector2;
  static _toPtr(p: number, v: StepCurveVector2): void;
  static _tempHeapPtr(v: StepCurveVector2): number;
  static _dtorFn(v: StepCurveVector2): void;
}

}
declare namespace ut.Interpolation {

/** Contains Vector3 keyframes for step interpolation.*/
class StepCurveVector3 extends ut.Component {
  constructor(times?: number[], values?: Vector3[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Vector3[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: StepCurveVector3): StepCurveVector3;
  static _toPtr(p: number, v: StepCurveVector3): void;
  static _tempHeapPtr(v: StepCurveVector3): number;
  static _dtorFn(v: StepCurveVector3): void;
}

}
declare namespace ut.Interpolation {

/** Contains Quaternion keyframes for step interpolation.*/
class StepCurveQuaternion extends ut.Component {
  constructor(times?: number[], values?: Quaternion[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Quaternion[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: StepCurveQuaternion): StepCurveQuaternion;
  static _toPtr(p: number, v: StepCurveQuaternion): void;
  static _tempHeapPtr(v: StepCurveQuaternion): number;
  static _dtorFn(v: StepCurveQuaternion): void;
}

}
declare namespace ut.Interpolation {

/** Contains Color keyframes for step interpolation.*/
class StepCurveColor extends ut.Component {
  constructor(times?: number[], values?: Color[]);
  /** Array of keyframe times.*/
  times: number[];
  /** Array of curve control points.*/
  values: Color[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: StepCurveColor): StepCurveColor;
  static _toPtr(p: number, v: StepCurveColor): void;
  static _tempHeapPtr(v: StepCurveColor): number;
  static _dtorFn(v: StepCurveColor): void;
}

}
declare namespace ut.Interpolation {

/** Specifies how time wraps when evaluating the curve value. This component
    works with entities that have curve components such as BezierCurveFloat,
    LinearCurveVector3, and so on.*/
class CurveTimeLoopMode extends ut.Component {
  constructor(loopMode?: LoopMode);
  /** The type of the time wrapping.*/
  loopMode: LoopMode;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: CurveTimeLoopMode): CurveTimeLoopMode;
  static _toPtr(p: number, v: CurveTimeLoopMode): void;
  static _tempHeapPtr(v: CurveTimeLoopMode): number;
  static _dtorFn(v: CurveTimeLoopMode): void;
}

}
declare namespace ut.Interpolation {
/** Service for evaluating interpolation curves. It works with entities that
    have curve components such as BezierCurveFloat, LinearCurveVector3, and so on.*/
class InterpolationService {
  /** Evaluates the Float value of the curve component at the given time.
      curveEntity must contain a float curve component (BezierCurveFloat,
      LinearCurveFloat, StepCurveFloat and so on).
      To specify the type of time wrapping, add the CurveTimeWrapping
      component to curveEntity.*/
  static evaluateCurveFloat(world: ut.WorldBase, time: number, curveEntity: Entity): number;
  /** Evaluates the Vector2 value of the curve component at the given time.
      curveEntity must contain a Vector2 curve component. (BezierCurveVector2,
      LinearCurveVector2, StepCurveVector2 and so on)
      To specify the type of time wrapping, add the CurveTimeWrapping
      component to curveEntity.*/
  static evaluateCurveVector2(world: ut.WorldBase, time: number, curveEntity: Entity): Vector2;
  /** Evaluates the Vector3 value of the curve component at the given time.
      curveEntity must contain a Vector3 curve component. (BezierCurveVector3,
      LinearCurveVector3, StepCurveVector3 and so on)
      To specify the type of time wrapping, add the CurveTimeWrapping
      component to curveEntity.*/
  static evaluateCurveVector3(world: ut.WorldBase, time: number, curveEntity: Entity): Vector3;
  /** Evaluates the Quaternion value of the curve component at the given time.
      curveEntity must contain a Quaternion curve component. (BezierCurveQuaternion,
      LinearCurveQuaternion, StepCurveQuaternion and so on)
      To specify the type of time wrapping, add the CurveTimeWrapping
      component to curveEntity.*/
  static evaluateCurveQuaternion(world: ut.WorldBase, time: number, curveEntity: Entity): Quaternion;
  /** Evaluates the Color value of the curve component at the given time.
      curveEntity must contain a Color curve component. (BezierCurveColor,
      LinearCurveColor, StepCurveColor and so on)
      To specify the type of time wrapping, add the CurveTimeWrapping
      component to curveEntity.*/
  static evaluateCurveColor(world: ut.WorldBase, time: number, curveEntity: Entity): Color;
}
}

declare namespace Module {
function _ut_Interpolation_InterpolationService_EvaluateCurveFloat(world: any, time: any, curveEntity: any): any;
function _ut_Interpolation_InterpolationService_EvaluateCurveVector2(world: any, time: any, curveEntity: any): void;
function _ut_Interpolation_InterpolationService_EvaluateCurveVector3(world: any, time: any, curveEntity: any): void;
function _ut_Interpolation_InterpolationService_EvaluateCurveQuaternion(world: any, time: any, curveEntity: any): void;
function _ut_Interpolation_InterpolationService_EvaluateCurveColor(world: any, time: any, curveEntity: any): void;

}


declare namespace ut.Core2D {
/** Drawing mode used by Sprite2DRendererOptions.
    When a sprite size is manually set, the drawing mode specifies how the
    sprite fills the area.*/
enum DrawMode {
  /** Tiles the sprite continuously if the area is larger than the source sprite, or cuts it off it is smaller.*/
  ContinuousTiling = 0,
  /** Adaptively tiles the sprite. When the target area is smaller, the sprite is scaled down, like in Stretch mode. 
      If the area is larger a combination of scaling and tiling is used, that minimizes scaling but always tiles complete tiles.*/
  AdaptiveTiling = 1,
  /** Scale the sprite to fill the new area.*/
  Stretch = 2,
}
}
declare namespace ut.Core2D {

/** A component describing a sprite as a sub-region of an image. Specifies the source {@link Image2D} atlas
    and the region to use.*/
class Sprite2D extends ut.Component {
  constructor(image?: Entity, imageRegion?: Rect, pivot?: Vector2);
  /** The Entity on which to look for a {@link Image2D} component to use as the source image.
      If null, the {@link Image2D} is looked for on the same entity as the {@link Sprite2D}.*/
  image: Entity;
  /** The region of the source image to use as the sprite. The image is treated as a unit
      rectangle; thus this rectangle should use values in the range of 0..1. For example,
      to use the bottom left portion of the image, the rectangle should go from (0, 0) to
      (0.5, 0.5)*/
  imageRegion: Rect;
  /** The point in the sprite that is the sprite's center. Relative to the bottom-left corner
      of the sprite, in unit rectangle coordinates.*/
  pivot: Vector2;
  static readonly image: EntityComponentFieldDesc;
  static readonly imageRegion: RectComponentFieldDesc;
  static readonly pivot: Vector2ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2D): Sprite2D;
  static _toPtr(p: number, v: Sprite2D): void;
  static _tempHeapPtr(v: Sprite2D): number;
  static _dtorFn(v: Sprite2D): void;
}

}
declare namespace ut.Core2D {

/** A component describing a list of {@link Sprite2D} packed in an image atlas.*/
class SpriteAtlas extends ut.Component {
  constructor(sprites?: Entity[]);
  /** List of {@link Sprite2D} found in the {@link Image2D} atlas.*/
  sprites: Entity[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: SpriteAtlas): SpriteAtlas;
  static _toPtr(p: number, v: SpriteAtlas): void;
  static _tempHeapPtr(v: SpriteAtlas): number;
  static _dtorFn(v: SpriteAtlas): void;
}

}
declare namespace ut.Core2D {

/** A component for basic 2D sprite rendering. Specifies an {@link Sprite2D} to draw and rendering
    modifiers, such as a color tint.*/
class Sprite2DRenderer extends ut.Component {
  constructor(sprite?: Entity, color?: Color, blending?: BlendOp);
  /** The Entity on which to look for a {@link UTiny.Sprite2D Sprite2D} component to describe the sprite to render.
      If null, the {@link Sprite2D} is looked for on the same entity as the {@link Sprite2DRenderer}.*/
  sprite: Entity;
  /** A color tint to apply to the sprite image. For normal rendering, this should be opaque
      white (1, 1, 1, 1).*/
  color: Color;
  /** Blend op for rendering the sprite. The default and regular mode is Alpha.*/
  blending: BlendOp;
  static readonly sprite: EntityComponentFieldDesc;
  static readonly color: ColorComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DRenderer): Sprite2DRenderer;
  static _toPtr(p: number, v: Sprite2DRenderer): void;
  static _tempHeapPtr(v: Sprite2DRenderer): number;
  static _dtorFn(v: Sprite2DRenderer): void;
}

}
declare namespace ut.Core2D {

/** Modifier component. Add alongside a Sprite2DRenderer to override the
    world space size computation.
    Regular sprites have a base (untransformed) size that is computed as
    image asset size times pixelsToWorldUnits.
    When this component is added to an entity the base size is set explicitly,
    and the image is placed inside that region depending on the repeat mode.*/
class Sprite2DRendererOptions extends ut.Component {
  constructor(size?: Vector2, drawMode?: DrawMode);
  /** Sprite size in world units 
      This is used to override the computed natural sprite size.
      The natural size is Image2D.imagePixelSize * Image2D.pixelsToWorldUnits * Sprite2D.imageRegion.size.
      The new size is also used in hit testing.*/
  size: Vector2;
  /** Draw mode, defaults to ContinuousTiling.
      This mode specifies how the natural sized sprite is mapped into the new size.*/
  drawMode: DrawMode;
  static readonly size: Vector2ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DRendererOptions): Sprite2DRendererOptions;
  static _toPtr(p: number, v: Sprite2DRendererOptions): void;
  static _tempHeapPtr(v: Sprite2DRendererOptions): number;
  static _dtorFn(v: Sprite2DRendererOptions): void;
}

}
declare namespace ut.Core2D {

/** Modifier component. Add alongside a Sprite2D to add a border. 
    The border is used for sliced tiling modes*/
class Sprite2DBorder extends ut.Component {
  constructor(bottomLeft?: Vector2, topRight?: Vector2);
  /** Bottom left slice inset point, normalized [0..1] to sprite (not image)
      Defaults to (0,0) for no border.*/
  bottomLeft: Vector2;
  /** Top right slice inset point, normalized [0..1] to sprite (not image)
      Defaults to (1,1) for no border.*/
  topRight: Vector2;
  static readonly bottomLeft: Vector2ComponentFieldDesc;
  static readonly topRight: Vector2ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DBorder): Sprite2DBorder;
  static _toPtr(p: number, v: Sprite2DBorder): void;
  static _tempHeapPtr(v: Sprite2DBorder): number;
  static _dtorFn(v: Sprite2DBorder): void;
}

}
declare namespace ut.Core2D {

/** A component that describes a list of sprites for animation*/
class Sprite2DSequence extends ut.Component {
  constructor(sprites?: Entity[], frameRate?: number);
  /** Sprite entities, required to have Sprite2D component*/
  sprites: Entity[];
  /** base frame rate of the sequence, in frames per second*/
  frameRate: number;
  
  static readonly frameRate: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DSequence): Sprite2DSequence;
  static _toPtr(p: number, v: Sprite2DSequence): void;
  static _tempHeapPtr(v: Sprite2DSequence): number;
  static _dtorFn(v: Sprite2DSequence): void;
}

}
declare namespace ut.Core2D {

/** A component that is used by the SequencePlayer system to play
    play back a sequence of sprites when applied to an entity that also has
    the Sprite2DRenderer component.*/
class Sprite2DSequencePlayer extends ut.Component {
  constructor(sequence?: Entity, paused?: boolean, loop?: LoopMode, speed?: number, time?: number);
  /** Sequence entity, required to have Sprite2DSequence component*/
  sequence: Entity;
  /** Set to true to pause animation sequence. Set back to false to continue animation. Default false.*/
  paused: boolean;
  /** Sets the looping behavior of the animation. Defaults to Loop.
      - Loop: Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
      - Once Play the sequence once [A][B][C] then pause and set time to 0
      - PingPong: Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
      - PingPongOnce: Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
      - ClampForever: Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing.*/
  loop: LoopMode;
  /** Speed multiplier for playback, defaults to 1*/
  speed: number;
  /** Current time for playback, 0 to infinity*/
  time: number;
  static readonly sequence: EntityComponentFieldDesc;
  static readonly paused: ComponentFieldDesc;
  
  static readonly speed: ComponentFieldDesc;
  static readonly time: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DSequencePlayer): Sprite2DSequencePlayer;
  static _toPtr(p: number, v: Sprite2DSequencePlayer): void;
  static _tempHeapPtr(v: Sprite2DSequencePlayer): number;
  static _dtorFn(v: Sprite2DSequencePlayer): void;
}

}
declare namespace ut.Core2D {
/** An initialization only system required to initialize sprite rendering*/
var Sprite2DInitSystem: ut.System;
}
declare namespace ut.Core2D {
/** A system used to drive SequencePlayer components.
    Required to be scheduled in order for sequences to play.*/
var SequencePlayerSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.Particles {

/** Each spawned particle has this component attached to it automatically.*/
class Particle {
  constructor(time?: number, lifetime?: number);
  /** How long this particle has existed, in seconds. From 0.0 to lifetime.*/
  time: number;
  /** The maximum lifetime of this particle, in seconds.*/
  lifetime: number;
  static _size: number;
  static _fromPtr(p: number, v?: Particle): Particle;
  static _toPtr(p: number, v: Particle): void;
  static _tempHeapPtr(v: Particle): number;
}
interface ParticleComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly time: ComponentFieldDesc;
  static readonly lifetime: ComponentFieldDesc;
}

}
declare namespace ut.Particles {

/** The core particle emitter component. Adding this component to an entity
    turns the entity into an emitter with the specified characteristics.
    You can add other components (for example, EmitterInitialScale, EmitterConeSource,
    and so on) to the same entity after the initial emission to further control
    how particles are emitted.*/
class ParticleEmitter extends ut.Component {
  constructor(particle?: Entity, maxParticles?: number, emitRate?: number, lifetime?: Range, attachToEmitter?: boolean);
  particle: Entity;
  /** Maximum number of particles to emit.*/
  maxParticles: number;
  /** Number of particles per second to emit.*/
  emitRate: number;
  /** Lifetime of each particle, in seconds.*/
  lifetime: Range;
  /** Specifies whether the Transform of the emitted particles is a child
      of this emitter.
      
      If true, the emission position is set to the entity's local position,
      and the particle is added as a child transform.
      
      If false, the emitter's world position is added to the emission position,
      and that result set as the local position.*/
  attachToEmitter: boolean;
  static readonly particle: EntityComponentFieldDesc;
  
  static readonly emitRate: ComponentFieldDesc;
  static readonly lifetime: RangeComponentFieldDesc;
  static readonly attachToEmitter: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ParticleEmitter): ParticleEmitter;
  static _toPtr(p: number, v: ParticleEmitter): void;
  static _tempHeapPtr(v: ParticleEmitter): number;
  static _dtorFn(v: ParticleEmitter): void;
}

}
declare namespace ut.Particles {

class EmitterBoxSource extends ut.Component {
  constructor(rect?: Rect);
  /** Particles are emitted from a random spot inside this rectangle, with
      0,0 of the rect at the Emitter's position.*/
  rect: Rect;
  static readonly rect: RectComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterBoxSource): EmitterBoxSource;
  static _toPtr(p: number, v: EmitterBoxSource): void;
  static _tempHeapPtr(v: EmitterBoxSource): number;
  static _dtorFn(v: EmitterBoxSource): void;
}

}
declare namespace ut.Particles {

/** Spawns particles in a cone. Particles are emitted from the base of the cone,
    which is a circle on the X/Z plane. The angle and speed parameters define
    the initial particle velocity.*/
class EmitterConeSource extends ut.Component {
  constructor(radius?: number, angle?: number, speed?: number);
  /** The radius in which the particles are being spawned.*/
  radius: number;
  /** The angle of the cone.*/
  angle: number;
  /** The initial speed of the particles.*/
  speed: number;
  static readonly radius: ComponentFieldDesc;
  static readonly angle: ComponentFieldDesc;
  static readonly speed: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterConeSource): EmitterConeSource;
  static _toPtr(p: number, v: EmitterConeSource): void;
  static _tempHeapPtr(v: EmitterConeSource): number;
  static _dtorFn(v: EmitterConeSource): void;
}

}
declare namespace ut.Particles {

/** Spawns particles inside a circle on the X/Y plane.*/
class EmitterCircleSource extends ut.Component {
  constructor(radius?: Range, speed?: Range, speedBasedOnRadius?: boolean);
  /** The radius of the circle.*/
  radius: Range;
  /** The initial speed of the particles.*/
  speed: Range;
  /** If true, particles that spawn closer to the center of the circle
      have a lower speed.*/
  speedBasedOnRadius: boolean;
  static readonly radius: RangeComponentFieldDesc;
  static readonly speed: RangeComponentFieldDesc;
  static readonly speedBasedOnRadius: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterCircleSource): EmitterCircleSource;
  static _toPtr(p: number, v: EmitterCircleSource): void;
  static _tempHeapPtr(v: EmitterCircleSource): number;
  static _dtorFn(v: EmitterCircleSource): void;
}

}
declare namespace ut.Particles {

/** Multiplies the scale of the source particle by a random value in the range
    specified by scale.*/
class EmitterInitialScale extends ut.Component {
  constructor(scale?: Range);
  /** Min, max initial scale of each particle.*/
  scale: Range;
  static readonly scale: RangeComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterInitialScale): EmitterInitialScale;
  static _toPtr(p: number, v: EmitterInitialScale): void;
  static _tempHeapPtr(v: EmitterInitialScale): number;
  static _dtorFn(v: EmitterInitialScale): void;
}

}
declare namespace ut.Particles {

/** Sets the initial velocity for particles.*/
class EmitterInitialVelocity extends ut.Component {
  constructor(velocity?: Vector3);
  velocity: Vector3;
  static readonly velocity: Vector3ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterInitialVelocity): EmitterInitialVelocity;
  static _toPtr(p: number, v: EmitterInitialVelocity): void;
  static _tempHeapPtr(v: EmitterInitialVelocity): number;
  static _dtorFn(v: EmitterInitialVelocity): void;
}

}
declare namespace ut.Particles {

/** Sets the initial rotation on the Z axis for particles to a random value
    in the range specified by angle.*/
class EmitterInitialRotation extends ut.Component {
  constructor(angle?: Range);
  angle: Range;
  static readonly angle: RangeComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterInitialRotation): EmitterInitialRotation;
  static _toPtr(p: number, v: EmitterInitialRotation): void;
  static _tempHeapPtr(v: EmitterInitialRotation): number;
  static _dtorFn(v: EmitterInitialRotation): void;
}

}
declare namespace ut.Particles {

/** Sets the initial angular velocity on the Z axis for particles to a random
    value in the range specified by angularVelocity.*/
class EmitterInitialAngularVelocity extends ut.Component {
  constructor(angularVelocity?: Range);
  angularVelocity: Range;
  static readonly angularVelocity: RangeComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterInitialAngularVelocity): EmitterInitialAngularVelocity;
  static _toPtr(p: number, v: EmitterInitialAngularVelocity): void;
  static _tempHeapPtr(v: EmitterInitialAngularVelocity): number;
  static _dtorFn(v: EmitterInitialAngularVelocity): void;
}

}
declare namespace ut.Particles {

/** Sets the initial color of the particles by multiplying the color of the
    source particle by a random value obtained by sampling curve between time
    0.0 and 1.0.*/
class EmitterInitialColor extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveColor component.
      The color is choosen randomly by sampling the curve between time 0.0 and 1.0.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmitterInitialColor): EmitterInitialColor;
  static _toPtr(p: number, v: EmitterInitialColor): void;
  static _tempHeapPtr(v: EmitterInitialColor): number;
  static _dtorFn(v: EmitterInitialColor): void;
}

}
declare namespace ut.Particles {

/** Modifies the Sprite2DRenderer's color by multiplying it's initial color by
    curve. The value of curve at time 0.0 defines the particle's color at the
    beginning of its lifetime. The value at time 1.0 defines the particle's
    color at the end of its lifetime.*/
class LifetimeColor extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveColor component.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LifetimeColor): LifetimeColor;
  static _toPtr(p: number, v: LifetimeColor): void;
  static _tempHeapPtr(v: LifetimeColor): number;
  static _dtorFn(v: LifetimeColor): void;
}

}
declare namespace ut.Particles {

/** Modifies the Transform's scale (uniform x/y/z scaling) by curve. The value
    of curve at time 0.0 defines the particle's color at the beginning of its
    lifetime. The value at time 1.0 defines the particle's color at the end
    of its lifetime.*/
class LifetimeScale extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveFloat component.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LifetimeScale): LifetimeScale;
  static _toPtr(p: number, v: LifetimeScale): void;
  static _tempHeapPtr(v: LifetimeScale): number;
  static _dtorFn(v: LifetimeScale): void;
}

}
declare namespace ut.Particles {

/** The angular velocity over lifetime. The value of curve at time 0.0 defines
    the particle's angular velocity at the beginning of its lifetime. The value
    at time 1.0 defines the particle's angular velocity at the end of its lifetime.*/
class LifetimeAngularVelocity extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveFloat component.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LifetimeAngularVelocity): LifetimeAngularVelocity;
  static _toPtr(p: number, v: LifetimeAngularVelocity): void;
  static _tempHeapPtr(v: LifetimeAngularVelocity): number;
  static _dtorFn(v: LifetimeAngularVelocity): void;
}

}
declare namespace ut.Particles {

/** The velocity over lifetime. The value of curve at time 0.0 defines
    the particle's velocity at the beginning of its lifetime. The value
    at time 1.0 defines the particle's velocity at the end of its lifetime.*/
class LifetimeVelocity extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveVector3 component.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LifetimeVelocity): LifetimeVelocity;
  static _toPtr(p: number, v: LifetimeVelocity): void;
  static _tempHeapPtr(v: LifetimeVelocity): number;
  static _dtorFn(v: LifetimeVelocity): void;
}

}
declare namespace ut.Particles {

/** Speed multiplier over lifetime. The value of curve at time 0.0 defines the
    multiplier at the beginning of the particle's lifetime. The value at time
    1.0 defines the multiplier at the end of the particle's lifetime.*/
class LifetimeSpeedMultiplier extends ut.Component {
  constructor(curve?: Entity);
  /** Entity with the [Bezier|Linear|Step]CurveFloat component.*/
  curve: Entity;
  static readonly curve: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: LifetimeSpeedMultiplier): LifetimeSpeedMultiplier;
  static _toPtr(p: number, v: LifetimeSpeedMultiplier): void;
  static _tempHeapPtr(v: LifetimeSpeedMultiplier): number;
  static _dtorFn(v: LifetimeSpeedMultiplier): void;
}

}
declare namespace ut.Particles {

/** An emitter with this component emits particles in bursts. A burst is a particle
    event where a number of particles are all emitted at the same time. A cycle
    is a single occurrence of a burst.*/
class BurstEmission extends ut.Component {
  constructor(count?: Range, interval?: Range, cycles?: number);
  /** How many particles in every cycle.*/
  count: Range;
  /** The interval between cycles, in seconds.*/
  interval: Range;
  /** How many times to play the burst.*/
  cycles: number;
  static readonly count: RangeComponentFieldDesc;
  static readonly interval: RangeComponentFieldDesc;
  static readonly cycles: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BurstEmission): BurstEmission;
  static _toPtr(p: number, v: BurstEmission): void;
  static _tempHeapPtr(v: BurstEmission): number;
  static _dtorFn(v: BurstEmission): void;
}

}
declare namespace ut.Particles {
/** A system that updates all particles and emitters.*/
var ParticleSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.Audio {
/** An enum listing the possible states an audio clip can be in during loading.
    Used by the AudioClip component to track loading status.*/
enum AudioClipStatus {
  /** The clip is not loaded in memory.*/
  Unloaded = 0,
  /** The clip has begun loading but is not ready to begin playback.*/
  Loading = 1,
  /** The clip is fuly decoded, loaded in memory, and ready for playback.*/
  Loaded = 2,
  /** The clip cannot be loaded in memory.*/
  LoadError = 3,
}
}
declare namespace ut.Audio {

class AudioConfig extends ut.Component {
  constructor(initialized?: boolean, paused?: boolean, unlocked?: boolean);
  /** True if the audio context is initialized.
      
      After you export and launch the project, and the AudioSystem updates
      for the first time, the AudioConfig component attempts to initialize
      audio. If successful, it sets this value to true.
      
      Once audio is initialized successfully the AudioConfig component does
      not re-attempt to initialize it on subsequent AudioSystem updates.*/
  initialized: boolean;
  /** If true, pauses the audio context. Set this at any time to pause or
      resume audio.*/
  paused: boolean;
  /** True if the audio context is unlocked in the browser.
      Some browsers require a user interaction, for example a touch interaction
      or key input, to unlock the audio context. If the context is locked
      no audio plays.*/
  unlocked: boolean;
  static readonly initialized: ComponentFieldDesc;
  static readonly paused: ComponentFieldDesc;
  static readonly unlocked: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioConfig): AudioConfig;
  static _toPtr(p: number, v: AudioConfig): void;
  static _tempHeapPtr(v: AudioConfig): number;
  static _dtorFn(v: AudioConfig): void;
}

}
declare namespace ut.Audio {

/** An audio clip represents a single audio resource that can play back on
    one or more AudioSource components.
    
    If only one AudioSource component references the audio clip, you can attach the
    AudioClip component to the same entity as that AudioSource component.
    
    If multiple AudioSource components reference the audio clip, it's recommended
    that you add the AudioClip component to a separate entity.*/
class AudioClip extends ut.Component {
  constructor(source?: string, status?: AudioClipStatus);
  /** The clip's original audio file/URL/name. Unity generates this automatically.
      Attach an AudioClipLoadFromFile component to load this file into the clip.*/
  source: string;
  /** The audio clip load status. The AudioClipStatus enum defines the possible states.*/
  status: AudioClipStatus;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioClip): AudioClip;
  static _toPtr(p: number, v: AudioClip): void;
  static _tempHeapPtr(v: AudioClip): number;
  static _dtorFn(v: AudioClip): void;
}

}
declare namespace ut.Audio {

/** Attach this component to an entity with an AudioClip component to begin
    loading an audio clip. Loading is performed by the AudioSystem, which must
    be scheduled. Once loading is complete the AudioSystem removes the
    AudioClipLoadFromFile component.*/
class AudioClipLoadFromFile extends ut.Component {
  constructor(fileName?: string);
  /** Specifies the audio file/URL to load.*/
  fileName: string;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioClipLoadFromFile): AudioClipLoadFromFile;
  static _toPtr(p: number, v: AudioClipLoadFromFile): void;
  static _tempHeapPtr(v: AudioClipLoadFromFile): number;
  static _dtorFn(v: AudioClipLoadFromFile): void;
}

}
declare namespace ut.Audio {

/** Attach this component to an entity with an AudioSource component to start
    playback the next time the AudioSystem updates. Once playback starts, the
    AudioSystem removes this component.
    Attaching an AudioSourceStart component to an already playing source re-starts
    playback from the beginning.
    To stop a playing source, use the AudioSourceStop component.*/
class AudioSourceStart extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioSourceStart): AudioSourceStart;
  static _toPtr(p: number, v: AudioSourceStart): void;
  static _tempHeapPtr(v: AudioSourceStart): number;
  static _dtorFn(v: AudioSourceStart): void;
}

}
declare namespace ut.Audio {

/** Attach this component to an entity with an AudioSource component to stop
    playback the next time the AudioSystem updates. Once playback stops, the
    AudioSystem removes this component.
    Attaching an AudioSourceStop component to an already stopped source has no effect.
    To start playing a source, use the AudioSourceStart component.*/
class AudioSourceStop extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioSourceStop): AudioSourceStop;
  static _toPtr(p: number, v: AudioSourceStop): void;
  static _tempHeapPtr(v: AudioSourceStop): number;
  static _dtorFn(v: AudioSourceStop): void;
}

}
declare namespace ut.Audio {

/** An AudioSource component plays back one audio clip at a time.
    Multiple audio sources can play at the same time.
    To start playback use the AudioSourceStart component.
    To stop playback use the AudioSourceStop component.*/
class AudioSource extends ut.Component {
  constructor(clip?: Entity, volume?: number, loop?: boolean, playing?: boolean);
  /** Specifies the audio clip that plays when this source starts playing.
      The clip entity must have the AudioClip component attached.
      If you change the clip entity, the change only takes effect after
      you attach an AudioSourceStart component to start playback.*/
  clip: Entity;
  /** Specifies the audio clip's playback volume. Values can range from 0..1.
      If you change the volume, the change takes effect after
      you attach an AudioSourceStart component to start playback.*/
  volume: number;
  /** If true, replays the audio clip when it reaches end.
      If you change the loop state, the change takes effect after
      you attach an AudioSourceStart component to start playback.*/
  loop: boolean;
  /** True if the audio clip is currently playing.*/
  playing: boolean;
  static readonly clip: EntityComponentFieldDesc;
  static readonly volume: ComponentFieldDesc;
  static readonly loop: ComponentFieldDesc;
  static readonly playing: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AudioSource): AudioSource;
  static _toPtr(p: number, v: AudioSource): void;
  static _tempHeapPtr(v: AudioSource): number;
  static _dtorFn(v: AudioSource): void;
}

}
declare namespace ut.Audio {
/** System required for audio to function.*/
var AudioSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.Profiler {
/** Service available to send profiling data to a local or remote Unity Editor process.
    
    In the Tiny Project settings, enabling the "Auto-Connect Profiler" option will automatically
    setup non-Release builds to call {@link ut.Profiler.Profiler.StartProfiling} once the {@link ut.World} is created.*/
class Profiler {
  /** Start sending profiling data from the given {@link world}.*/
  static startProfiling(world: ut.WorldBase): void;
  /** Stop sending profiling data from the given {@link world}.*/
  static stopProfiling(world: ut.WorldBase): void;
}
}

declare namespace Module {
function _ut_Profiler_Profiler_StartProfiling(world: any): void;
function _ut_Profiler_Profiler_StopProfiling(world: any): void;

}


declare namespace ut.Core2D {

/** A component describing a 2d closed polygon shape*/
class Shape2D extends ut.Component {
  constructor(vertices?: Vector2[], indices?: number[]);
  /** The vertices of the shape. Shapes are limited to 64k vertices.*/
  vertices: Vector2[];
  /** Optional: Indices into the vertices of the shape.
      Every 3 indices form one triangle.
      Index values must be between 0 and vertices.size. 
      If indices has length zero, vertices are rendered as a triangle fan.*/
  indices: number[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Shape2D): Shape2D;
  static _toPtr(p: number, v: Shape2D): void;
  static _tempHeapPtr(v: Shape2D): number;
  static _dtorFn(v: Shape2D): void;
}

}
declare namespace ut.Core2D {

/** A component for basic 2D shape rendering. Specifies an {@link Shape2D} to draw and rendering
    modifiers, such as a color tint.*/
class Shape2DRenderer extends ut.Component {
  constructor(shape?: Entity, color?: Color, blending?: BlendOp);
  /** The Entity on which to look for a {@link UTiny.Shape2D Shape2D} component to describe the shape to render.
      If null, the {@link Shape2D} is looked for on the same entity as the {@link Shape2DRenderer}.*/
  shape: Entity;
  /** A color tint to apply to the sprite image. For normal rendering, this should be opaque
      white (1, 1, 1, 1).*/
  color: Color;
  /** Blend op for rendering the sprite. The default and regular mode is Alpha.*/
  blending: BlendOp;
  static readonly shape: EntityComponentFieldDesc;
  static readonly color: ColorComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Shape2DRenderer): Shape2DRenderer;
  static _toPtr(p: number, v: Shape2DRenderer): void;
  static _tempHeapPtr(v: Shape2DRenderer): number;
  static _dtorFn(v: Shape2DRenderer): void;
}

}
declare namespace ut.Core2D {
/** An initialization only system required to initialize shape rendering*/
var Shape2DInitSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.HitBox2D {

/** When added to an entity, this component describes overlap between that entity
    and another entity with a Sprite2DRendererHitBox2D HitBox2D component attached.*/
class HitBoxOverlap {
  constructor(otherEntity?: Entity, camera?: Entity);
  /** Specifies the "other" entity, which has a Sprite2DRendererHitBox2D
      or HitBox2D component attached.*/
  otherEntity: Entity;
  /** Specifies the camera that sees the overlap.*/
  camera: Entity;
  static _size: number;
  static _fromPtr(p: number, v?: HitBoxOverlap): HitBoxOverlap;
  static _toPtr(p: number, v: HitBoxOverlap): void;
  static _tempHeapPtr(v: HitBoxOverlap): number;
}
interface HitBoxOverlapComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly otherEntity: EntityComponentFieldDesc;
  static readonly camera: EntityComponentFieldDesc;
}

}
declare namespace ut.HitBox2D {

/** The HitBix2DService.hitTest function returns this structure.*/
class HitTestResult {
  constructor(entityHit?: Entity, uv?: Vector2);
  /** Specifies the hit entity, or NONE if no entity is hit.*/
  entityHit: Entity;
  /** Specifies normalized [0..1] coordinates for a hit location
      on a sprite. The coordinate system's origin is the lower-left corner.*/
  uv: Vector2;
  static _size: number;
  static _fromPtr(p: number, v?: HitTestResult): HitTestResult;
  static _toPtr(p: number, v: HitTestResult): void;
  static _tempHeapPtr(v: HitTestResult): number;
}
interface HitTestResultComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly entityHit: EntityComponentFieldDesc;
  static readonly uv: Vector2ComponentFieldDesc;
}

}
declare namespace ut.HitBox2D {

/** The HitBix2DService.rayCast function returns this structure.*/
class RayCastResult {
  constructor(entityHit?: Entity, t?: number);
  /** Specifies the hit entity, or NONE if no entity is hit.*/
  entityHit: Entity;
  /** Specifies a normalized [0..1] distance along a ray.
      The hit location is: hit = rayStart + (rayEnd-rayStart)*t;
      If the ray cast starts inside a hit box, t is the exit value.*/
  t: number;
  static _size: number;
  static _fromPtr(p: number, v?: RayCastResult): RayCastResult;
  static _toPtr(p: number, v: RayCastResult): void;
  static _tempHeapPtr(v: RayCastResult): number;
}
interface RayCastResultComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly entityHit: EntityComponentFieldDesc;
  static readonly t: ComponentFieldDesc;
}

}
declare namespace ut.HitBox2D {

/** The HitBix2DService.detailedOverlapInformation function returns this structure.*/
class DetailedHitBoxOverlap {
  constructor(vertices?: Vector2[]);
  /** Defines a 2D convex polygon outline of the intersection between two
      hit boxes. This can be rendered as a Shape2D component.*/
  vertices: Vector2[];
  static _size: number;
  static _fromPtr(p: number, v?: DetailedHitBoxOverlap): DetailedHitBoxOverlap;
  static _toPtr(p: number, v: DetailedHitBoxOverlap): void;
  static _tempHeapPtr(v: DetailedHitBoxOverlap): number;
}
interface DetailedHitBoxOverlapComponentFieldDesc extends ut.ComponentFieldDesc {
  
}

}
declare namespace ut.HitBox2D {

/** Describes a 2D hit box for simple picking.
    Hit boxes can be separate from sprite rendering, but should have a
    transform component.*/
class RectHitBox2D extends ut.Component {
  constructor(box?: Rect);
  /** Defines the hit-area rectangle used for picking and non-physics
      collision checks. Its pivot point is at coordinates 0,0.*/
  box: Rect;
  static readonly box: RectComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: RectHitBox2D): RectHitBox2D;
  static _toPtr(p: number, v: RectHitBox2D): void;
  static _tempHeapPtr(v: RectHitBox2D): number;
  static _dtorFn(v: RectHitBox2D): void;
}

}
declare namespace ut.HitBox2D {

/** Describes a 2D hit box for simple picking. This component
    behaves the same as a HitBox2D component, but a Sprite2DRenderer component
    defines its size.*/
class Sprite2DRendererHitBox2D extends ut.Component {
  constructor(pixelAccurate?: boolean);
  /** When true, the Sprite2DRendererHitBox2D component uses pixel-accurate
      hit testing from the sprite. Defaults to false.
      Pixel-accurate hit testing requires that the entity have both an Image2D
      component and an Image2DAlphaMask component attached before image loading.
      It ignores hits and overlap where the sprite Alpha is zero.
      Note that pixel-accurate hit testing is computationally intensive.*/
  pixelAccurate: boolean;
  static readonly pixelAccurate: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Sprite2DRendererHitBox2D): Sprite2DRendererHitBox2D;
  static _toPtr(p: number, v: Sprite2DRendererHitBox2D): void;
  static _tempHeapPtr(v: Sprite2DRendererHitBox2D): number;
  static _dtorFn(v: Sprite2DRendererHitBox2D): void;
}

}
declare namespace ut.HitBox2D {

/** The HitBox2DSystem adds this component to an entity in order to indicate
    how it overlaps other entities.*/
class HitBoxOverlapResults extends ut.Component {
  constructor(overlaps?: HitBoxOverlap[]);
  /** When the HitBoxOverlapResults component is added to an entity, this
      array lists the entity's overlaps.*/
  overlaps: HitBoxOverlap[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: HitBoxOverlapResults): HitBoxOverlapResults;
  static _toPtr(p: number, v: HitBoxOverlapResults): void;
  static _tempHeapPtr(v: HitBoxOverlapResults): number;
  static _dtorFn(v: HitBoxOverlapResults): void;
}

}
declare namespace ut.HitBox2D {
class HitBox2DService {
  /** Performs a hit test on all entities that have RectHitBox2D or Sprite2DRendererHitBox2D
      components. Returns the closest hit in z order at point, and outputs
      the UV Coordinates at the hit location. Returns an empty entity if
      there is no hit.
      Input point is in world space, but the z coordinate is ignored.*/
  static hitTest(w: ut.WorldBase, point: Vector3, camera: Entity): HitTestResult;
  /** Performs a 2D hit test along a ray and returns the first hit. Ignores
      the z coordinate.*/
  static rayCast(w: ut.WorldBase, rayStart: Vector3, rayEnd: Vector3, camera: Entity): RayCastResult;
  /** Gets the topmost camera at point, in world coordinates (ignores the
      z coordinate). This is useful to pass into hitTest if the camera is not known.*/
  static hitTestCamera(w: ut.WorldBase, point: Vector3): Entity;
  /** Gets detailed information about how two entities overlap, and computes
      the precise intersection area as a convex polygon with up to eight vertices.
      
      Warning: in cases where two hit areas are just barely touching, the
      system may report an overlap with an area of zero (zero vertices).*/
  static detailedOverlapInformation(w: ut.WorldBase, e: Entity, overlap: HitBoxOverlap): DetailedHitBoxOverlap;
}
}
declare namespace ut.HitBox2D {
/** This system tests for overlaps between all components under all cameras
    in the world. It adds HitBoxOverlapResults components to entities that
    overlap others, and removes them from entities that no longer overlap others.
    
    Overlaps are only detected between objects that overlap under the same camera.
    Note: DisplayListSystem must run before HitBox2DSystem produces results.*/
var HitBox2DSystem: ut.System;
}

declare namespace Module {
function _ut_HitBox2D_HitBox2DService_hitTest(w: any, point: any, camera: any): void;
function _ut_HitBox2D_HitBox2DService_rayCast(w: any, rayStart: any, rayEnd: any, camera: any): void;
function _ut_HitBox2D_HitBox2DService_hitTestCamera(w: any, point: any): void;
function _ut_HitBox2D_HitBox2DService_detailedOverlapInformation(w: any, e: any, overlap: any): void;

}


declare namespace ut.HTML {
var AssetLoader: ut.System;
}

declare namespace Module {

}


declare namespace ut.Text {
enum FontName {
  SansSerif = 0,
  Serif = 1,
  Monospace = 2,
}
}
declare namespace ut.Text {

/** Use CharacterInfo to create glyph metrics for a bitmap font.*/
class CharacterInfo {
  constructor(value?: number, advance?: number, bearingX?: number, bearingY?: number, width?: number, height?: number, characterRegion?: Rect);
  /** UTF32 character value of the glyph.*/
  value: number;
  /** The horizontal distance, in pixels, from this character's origin to
      the next character's origin.*/
  advance: number;
  /** The horizontal distance in pixels from this glyph's origin to the beginning
      of the glyph image.*/
  bearingX: number;
  /** The vertical distance in pixels from the baseline to the glyph's ymax
      (top of the glyph bounding box).*/
  bearingY: number;
  /** The width of the glyph image.*/
  width: number;
  /** The height of the glyph image.*/
  height: number;
  /** The glyph's uv coordinates in the texture atlas. x, y is bottom left.*/
  characterRegion: Rect;
  static _size: number;
  static _fromPtr(p: number, v?: CharacterInfo): CharacterInfo;
  static _toPtr(p: number, v: CharacterInfo): void;
  static _tempHeapPtr(v: CharacterInfo): number;
}
interface CharacterInfoComponentFieldDesc extends ut.ComponentFieldDesc {
  
  static readonly advance: ComponentFieldDesc;
  static readonly bearingX: ComponentFieldDesc;
  static readonly bearingY: ComponentFieldDesc;
  static readonly width: ComponentFieldDesc;
  static readonly height: ComponentFieldDesc;
  static readonly characterRegion: RectComponentFieldDesc;
}

}
declare namespace ut.Text {

/** Add this component to a Font entity to specify a bitmap font.*/
class BitmapFont extends ut.Component {
  constructor(textureAtlas?: Entity, data?: CharacterInfo[], size?: number, ascent?: number, descent?: number);
  /** The entity on which to look for an {@link Image2D} component to use.*/
  textureAtlas: Entity;
  /** Lists CharacterInfo data (glyphs).*/
  data: CharacterInfo[];
  /** The Font size in World Units.*/
  size: number;
  /** The distance from the baseline to the font's ascent line.*/
  ascent: number;
  /** Distance from the baseline to the font's descent line.*/
  descent: number;
  static readonly textureAtlas: EntityComponentFieldDesc;
  
  static readonly size: ComponentFieldDesc;
  static readonly ascent: ComponentFieldDesc;
  static readonly descent: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: BitmapFont): BitmapFont;
  static _toPtr(p: number, v: BitmapFont): void;
  static _tempHeapPtr(v: BitmapFont): number;
  static _dtorFn(v: BitmapFont): void;
}

}
declare namespace ut.Text {

/** Add this component to a font entity to specify a native font.*/
class NativeFont extends ut.Component {
  constructor(name?: FontName, worldUnitsToPt?: number);
  /** {@link FontName} generic names.*/
  name: FontName;
  /** Multiplier for converting World units to Points. Fonts are rendered
      in points (pt).*/
  worldUnitsToPt: number;
  
  static readonly worldUnitsToPt: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: NativeFont): NativeFont;
  static _toPtr(p: number, v: NativeFont): void;
  static _tempHeapPtr(v: NativeFont): number;
  static _dtorFn(v: NativeFont): void;
}

}
declare namespace ut.Text {

/** Add this component to an entity with a Text2DRenderer component to specify
    a native font.*/
class Text2DStyleNativeFont extends ut.Component {
  constructor(font?: Entity, italic?: boolean, weight?: number);
  /** The Font entity on which to look for a {@link NativeFont} component to use.*/
  font: Entity;
  /** If true, adds the italic attribute to the text.*/
  italic: boolean;
  /** Sets the font weight. A value of 400 is normal weight. Higher values
      make characters thicker. Lower values make them thinner.*/
  weight: number;
  static readonly font: EntityComponentFieldDesc;
  static readonly italic: ComponentFieldDesc;
  static readonly weight: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Text2DStyleNativeFont): Text2DStyleNativeFont;
  static _toPtr(p: number, v: Text2DStyleNativeFont): void;
  static _tempHeapPtr(v: Text2DStyleNativeFont): number;
  static _dtorFn(v: Text2DStyleNativeFont): void;
}

}
declare namespace ut.Text {

/** Add this component to an entity with a Text2DRenderer component to specify
    a bitmap font.*/
class Text2DStyleBitmapFont extends ut.Component {
  constructor(font?: Entity);
  /** The Font entity on which to look for a {@link BitmapFont} component to use.*/
  font: Entity;
  static readonly font: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Text2DStyleBitmapFont): Text2DStyleBitmapFont;
  static _toPtr(p: number, v: Text2DStyleBitmapFont): void;
  static _tempHeapPtr(v: Text2DStyleBitmapFont): number;
  static _dtorFn(v: Text2DStyleBitmapFont): void;
}

}
declare namespace ut.Text {

/** Add this component to an entity with a Text2DRenderer component to specify
    the font style.*/
class Text2DStyle extends ut.Component {
  constructor(color?: Color, size?: number);
  /** The text {@link Color}*/
  color: Color;
  /** The Font size in World Units;*/
  size: number;
  static readonly color: ColorComponentFieldDesc;
  static readonly size: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Text2DStyle): Text2DStyle;
  static _toPtr(p: number, v: Text2DStyle): void;
  static _tempHeapPtr(v: Text2DStyle): number;
  static _dtorFn(v: Text2DStyle): void;
}

}
declare namespace ut.Text {

/** Add this component to a text entity to specify the string text, the pivot,
    and a blending operation.
    
    If you want to set the text inside of a RectTransform component, you must
    add both the RectTransform and a RectTransformFinalSize component to the
    entity with the Text2DRenderer.
    
    If you want to auto-fit the text inside the RectTransform component, you
    must also add a Text2DAutoFit component, and make sure to schedule the
    SetRectTransformSizeSystem.*/
class Text2DRenderer extends ut.Component {
  constructor(style?: Entity, text?: string, pivot?: Vector2, blending?: BlendOp);
  /** The entity on which to look for a {@link Text2DStyle} component to
      use as the Text style.*/
  style: Entity;
  /** The text to display.*/
  text: string;
  /** The center point in the text, relative to the text's bottom-left corner,
      in unit rectangle coordinates*/
  pivot: Vector2;
  /** {@link BlendOp} for rendering text. The default and regular mode is Alpha.*/
  blending: BlendOp;
  static readonly style: EntityComponentFieldDesc;
  
  static readonly pivot: Vector2ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Text2DRenderer): Text2DRenderer;
  static _toPtr(p: number, v: Text2DRenderer): void;
  static _tempHeapPtr(v: Text2DRenderer): number;
  static _dtorFn(v: Text2DRenderer): void;
}

}
declare namespace ut.Text {

/** When added to an entity with a Text2DRenderer component, auto-fits the text
    within a RectTransform.
    
    For Text2DAutofit to work, you must also: add a RectTransformFinalSize
    component to the entity, and schedule the SetRectTransformSizeSystem in
    the UILayout module.
    
    The RectTransformFinalSize component enables the SetRectTransformSizeSystem
    to get the correct auto-fit size at runtime.*/
class Text2DAutoFit extends ut.Component {
  constructor(minSize?: number, maxSize?: number);
  /** The minimum font size. If the font size computed at runtime to fit
      in the RectTransform is below this value, the text does not render.*/
  minSize: number;
  /** The maximum font size.*/
  maxSize: number;
  static readonly minSize: ComponentFieldDesc;
  static readonly maxSize: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Text2DAutoFit): Text2DAutoFit;
  static _toPtr(p: number, v: Text2DAutoFit): void;
  static _tempHeapPtr(v: Text2DAutoFit): number;
  static _dtorFn(v: Text2DAutoFit): void;
}

}
declare namespace ut.Text {
class TextService {
  /** Helper function that creates a native font entity and returns it.
      @returns The native font entity.*/
  static createNativeFont(world: ut.WorldBase, fontName: FontName, worldUnitsToPt: number): Entity;
  /** Helper function that creates a text renderer entity and assigns a native
      font to it.
      @returns The text entity.*/
  static createTextWithNativeFont(world: ut.WorldBase, fontEntity: Entity, text: NativeString, size: number, italic: boolean, weight: number, color: Color, blending: BlendOp, pivot: Vector2): Entity;
  /** Helper function that creates a bitmap font entity and returns it.
      @returns The bitmap font entity.*/
  static createBitmapFont(world: ut.WorldBase, textureAtlasImg: Entity, size: number, ascent: number, descent: number): Entity;
  /** Helper function that adds a glyph to a bitmap font.*/
  static addGlyph(world: ut.WorldBase, fontEntity: Entity, utf32CharacterValue: number, bearingX: number, bearingY: number, advance: number, width: number, height: number, characterRegion: Rect): void;
  /** Helper function that creates a text renderer entity and assigns a bitmap
      font to it.
      @returns The text entity*/
  static createTextWithBitmapFont(world: ut.WorldBase, fontEntity: Entity, text: NativeString, size: number, color: Color, blending: BlendOp, pivot: Vector2): Entity;
}
}
declare namespace ut.Text {
/** An initialization-only system required to initialize text rendering.*/
var Text2DInitSystem: ut.System;
}

declare namespace Module {
function _ut_Text_TextService_createNativeFont(world: any, fontName: any, worldUnitsToPt: any): void;
function _ut_Text_TextService_CreateTextWithNativeFont(world: any, fontEntity: any, text: any, size: any, italic: any, weight: any, color: any, blending: any, pivot: any): void;
function _ut_Text_TextService_CreateBitmapFont(world: any, textureAtlasImg: any, size: any, ascent: any, descent: any): void;
function _ut_Text_TextService_AddGlyph(world: any, fontEntity: any, utf32CharacterValue: any, bearingX: any, bearingY: any, advance: any, width: any, height: any, characterRegion: any): void;
function _ut_Text_TextService_CreateTextWithBitmapFont(world: any, fontEntity: any, text: any, size: any, color: any, blending: any, pivot: any): void;

}


declare namespace ut.UILayout {
/** Determines how UI elements in the Canvas are scaled.*/
enum UIScaleMode {
  /** In Constant Pixel Size mode, you specify UI element positions and sizes
      in pixels on the screen.
      
      In practical terms, this means that elements retain the same size,
      in pixels, regardless of screen size.*/
  ConstantPixelSize = 0,
  /** In Scale With Screen Size mode, the Canvas has a specified reference
      resolution and you specify UI element positions and sizes in pixels
      within that resolution.
      
      If the screen resolution is different from the the Canvas resolution,
      the Canvas scales up or down, as needed, to fit the screen.
      
      In practical terms, this means that UI elements get bigger as the screen
      gets bigger, and vice versa.*/
  ScaleWithScreenSize = 1,
}
}
declare namespace ut.UILayout {

/** Defines the position, size, anchor and pivot information for a rectangle.*/
class RectTransform extends ut.Component {
  constructor(anchorMin?: Vector2, anchorMax?: Vector2, sizeDelta?: Vector2, anchoredPosition?: Vector2, pivot?: Vector2);
  /** The normalized position in the parent RectTransform that the lower
      left corner of the rectangle is anchored to.*/
  anchorMin: Vector2;
  /** The normalized position in the parent RectTransform that the upper
      right corner of the rectangle is anchored to.*/
  anchorMax: Vector2;
  /** The size of this RectTransform relative to the distances between the
      anchors.
      If the anchors are together, sizeDelta is the same as size. If the
      anchors are in each of the four corners of the parent, the sizeDelta
      is how much bigger or smaller the rectangle is compared to its parent.*/
  sizeDelta: Vector2;
  /** The position of this RectTransform's pivot of relative to the anchor
      reference point.*/
  anchoredPosition: Vector2;
  /** The normalized position in this RectTransform that it rotates around.*/
  pivot: Vector2;
  static readonly anchorMin: Vector2ComponentFieldDesc;
  static readonly anchorMax: Vector2ComponentFieldDesc;
  static readonly sizeDelta: Vector2ComponentFieldDesc;
  static readonly anchoredPosition: Vector2ComponentFieldDesc;
  static readonly pivot: Vector2ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: RectTransform): RectTransform;
  static _toPtr(p: number, v: RectTransform): void;
  static _tempHeapPtr(v: RectTransform): number;
  static _dtorFn(v: RectTransform): void;
}

}
declare namespace ut.UILayout {

/** Root component for UI elements (entities with RectTransform and Transform components).*/
class UICanvas extends ut.Component {
  constructor(camera?: Entity, uiScaleMode?: UIScaleMode, referenceResolution?: Vector2, matchWidthOrHeight?: number);
  /** The entity with the Camera2D component used for layouting calculations.*/
  camera: Entity;
  /** How UI elements in the Canvas are scaled.*/
  uiScaleMode: UIScaleMode;
  /** The resolution the UI layout is designed for (in pixels).*/
  referenceResolution: Vector2;
  /** Scales the Canvas to match the width or height of the reference
      resolution, or a combination of width and height.
      
      A value of 0 scales the Canvas according to the difference between the
      current screen resolution width and the reference resolution width.
      
      A value of 1 scales Canvas according to the difference between the
      current screen resolution height and the reference resolution height.
      
      Values between 0 and 1 scale the Canvas based on a combination of
      the relative width and height.*/
  matchWidthOrHeight: number;
  static readonly camera: EntityComponentFieldDesc;
  
  static readonly referenceResolution: Vector2ComponentFieldDesc;
  static readonly matchWidthOrHeight: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: UICanvas): UICanvas;
  static _toPtr(p: number, v: UICanvas): void;
  static _tempHeapPtr(v: UICanvas): number;
  static _dtorFn(v: UICanvas): void;
}

}
declare namespace ut.UILayout {
/** Utility class that facilitates common layouting operations.*/
class UILayoutService {
  /** Gets the dimensions of a RectTransform component.
      This method assumes that entity has both a Transform component and a
      RectTransform component.*/
  static getRectTransformSizeOfEntity(world: ut.WorldBase, entity: Entity): Vector2;
  /** Returns the size of the childTransform's parent. If the childTransform
      doesn't have a parent, returns the screen size.*/
  static getRectTransformSizeOfParent(world: ut.WorldBase, entity: Entity): Vector2;
  /** Calculates the dimensions of a RectTransform component using the supplied
      values.*/
  static computeRectTransformSize(anchorMin: Vector2, anchorMax: Vector2, sizeDelta: Vector2, parentSize: Vector2): Vector2;
}
}
declare namespace ut.UILayout {
/** Sets the desired scale for entities with the UICanvas component. The scale
    value is calculated based on the UICanvas.uiScaleMode field.*/
var UICanvasSystem: ut.System;
}
declare namespace ut.UILayout {
/** Lays out elements with both a RectTransform and Transform component by
    setting their Transform.localPosition. The local position depends on the
    RectTransform component.*/
var UILayoutSystem: ut.System;
}
declare namespace ut.UILayout {
/** SetSprite2DSizeSystem works on entities that have Transform, RectTransform,
    Sprite2DRenderer, and Sprite2DRendererOptions components.
    It automatically updates the Sprite2DRendererOptions.size property
    based on the size of the RectTransform.*/
var SetSprite2DSizeSystem: ut.System;
}
declare namespace ut.UILayout {
var SetRectTransformSizeSystem: ut.System;
}

declare namespace Module {
function _ut_UILayout_UILayoutService_GetRectTransformSizeOfEntity(world: any, entity: any): void;
function _ut_UILayout_UILayoutService_GetRectTransformSizeOfParent(world: any, entity: any): void;
function _ut_UILayout_UILayoutService_ComputeRectTransformSize(anchorMin: any, anchorMax: any, sizeDelta: any, parentSize: any): void;

}


declare namespace ut.Tilemap2D {
/** An enum that describes the different collider types for tiles.*/
enum TileColliderType {
  /** No collider.*/
  None = 0,
  /** Sprite collider. Uses the sprite outline as the collider shape for the tile.*/
  Sprite = 1,
  /** Grid collider. Uses the grid layout boundary outline as the collider
      shape for the tile.*/
  Grid = 2,
}
}
declare namespace ut.Tilemap2D {

/** A structure that describes data per tile instance in a tilemap.*/
class TileData {
  constructor(position?: Vector2, tile?: Entity);
  /** Tile position.*/
  position: Vector2;
  /** The tile to display. Must point to an entity with the Tile component on it.*/
  tile: Entity;
  static _size: number;
  static _fromPtr(p: number, v?: TileData): TileData;
  static _toPtr(p: number, v: TileData): void;
  static _tempHeapPtr(v: TileData): number;
}
interface TileDataComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly position: Vector2ComponentFieldDesc;
  static readonly tile: EntityComponentFieldDesc;
}

}
declare namespace ut.Tilemap2D {

/** A component that describes tiles in a grid layout.*/
class Tilemap extends ut.Component {
  constructor(anchor?: Vector3, position?: Vector3, rotation?: Quaternion, scale?: Vector3, cellSize?: Vector3, cellGap?: Vector3, tiles?: TileData[]);
  /** The anchor point of each tile within its grid cell.*/
  anchor: Vector3;
  /** The position of each tile in its grid cell relative to anchor.*/
  position: Vector3;
  /** The rotation of each tile in its grid cell relative to anchor*/
  rotation: Quaternion;
  /** The scale of each tile in its grid cell relative to anchor.*/
  scale: Vector3;
  /** The size of grid cells, in pixels.*/
  cellSize: Vector3;
  /** The gap between grid cells, in pixels.*/
  cellGap: Vector3;
  /** The list of tiles to draw.*/
  tiles: TileData[];
  static readonly anchor: Vector3ComponentFieldDesc;
  static readonly position: Vector3ComponentFieldDesc;
  static readonly rotation: QuaternionComponentFieldDesc;
  static readonly scale: Vector3ComponentFieldDesc;
  static readonly cellSize: Vector3ComponentFieldDesc;
  static readonly cellGap: Vector3ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Tilemap): Tilemap;
  static _toPtr(p: number, v: Tilemap): void;
  static _tempHeapPtr(v: Tilemap): number;
  static _dtorFn(v: Tilemap): void;
}

}
declare namespace ut.Tilemap2D {

/** A component describing properties of a tile used by tilemaps.*/
class Tile extends ut.Component {
  constructor(color?: Color, sprite?: Entity, colliderType?: TileColliderType);
  /** The Color used to tint the material.*/
  color: Color;
  /** The Sprite entity to draw.*/
  sprite: Entity;
  /** Collider type, as defined in the TileColliderType enum.
      Options are: None, Sprite, Grid. Defaults to None.*/
  colliderType: TileColliderType;
  static readonly color: ColorComponentFieldDesc;
  static readonly sprite: EntityComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Tile): Tile;
  static _toPtr(p: number, v: Tile): void;
  static _tempHeapPtr(v: Tile): number;
  static _dtorFn(v: Tile): void;
}

}
declare namespace ut.Tilemap2D {

/** A component for tilemap rendering. Specifies an {@link Tilemap} to draw,
    as well as and rendering modifiers, such as a color tint.*/
class TilemapRenderer extends ut.Component {
  constructor(tilemap?: Entity, color?: Color, blending?: BlendOp);
  /** Specifies the entity with the {@link UTiny.Tilemap Tilemap} component that describes
      the shape to render.
      If null, looks for the {@link Tilemap} on the same entity as the {@link Tilemap2DRenderer}.*/
  tilemap: Entity;
  /** A color tint to apply to the sprite image. For normal rendering, this should be opaque
      white (1, 1, 1, 1).*/
  color: Color;
  /** Blending mode for rendering the sprite. The default and regular mode is Alpha.*/
  blending: BlendOp;
  static readonly tilemap: EntityComponentFieldDesc;
  static readonly color: ColorComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TilemapRenderer): TilemapRenderer;
  static _toPtr(p: number, v: TilemapRenderer): void;
  static _tempHeapPtr(v: TilemapRenderer): number;
  static _dtorFn(v: TilemapRenderer): void;
}

}
declare namespace ut.Tilemap2D {

/** Flag component that you apply to the same entity as a TileMap component
    to indicate that the tilemap needs re-chunking.
    Add this component whenever a tile changes.*/
class TilemapRechunk extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TilemapRechunk): TilemapRechunk;
  static _toPtr(p: number, v: TilemapRechunk): void;
  static _tempHeapPtr(v: TilemapRechunk): number;
  static _dtorFn(v: TilemapRechunk): void;
}

}
declare namespace ut.Tilemap2D {
/** System that chunks tilemaps and prepares them for rendering.
    This system must run before the DisplayList system and before the map renders.
    It will perform dirty checking on the TileMap, and create private components
    on the entity with the TileMap component.*/
var TilemapChunkingSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.HTML {
/** This system does some pre-process work to compute text measurements before
    rendering the text. You must schedule it in order to render text in Canvas or WebGL.
    
    If you are using a Text2DAutofit component, this system also computes the
    text size required to auto-fit the text.
    
    When using the Text2DAutofit component, schedule this system before the
    RenderingFence system and after the SetRectTransformSizeSystem.*/
var TextMeasurement: ut.System;
}

declare namespace Module {

}



declare namespace Module {

}



declare namespace Module {

}


declare namespace ut.Rendering {
var RendererGLWebGL: ut.System;
}

declare namespace Module {

}


declare namespace ut.Rendering {
var RendererCanvas: ut.System;
}

declare namespace Module {

}


// this is an object defined in GeminiHTML.js
declare namespace ut.Runtime {
    var Service: typeof ut.HTML.HTMLService;
    var Renderer: ut.System;
    var InputHandler: ut.System;
    var Input: typeof ut.Core2D.Input;
}
declare namespace ut.HTML {
class HTMLService {
  static run(world: ut.WorldBase): boolean;
  static oneWorld(): ut.WorldBase;
  static promptText(message: string, defaultText?: string): string;
  static create(world: ut.WorldBase): void;
  static destroy(): void;
}
}
declare namespace ut.HTML {
var InputHandler: ut.System;
}
declare namespace ut.HTML {
var RendererHTMLSwitchable: ut.System;
}

declare namespace Module {
function _ut_HTML_HTMLService_Run(world: any): any;
function _ut_HTML_HTMLService_OneWorld(): any;
function _ut_HTML_HTMLService_PromptText(message: any, defaultText: any): void;
function _ut_HTML_HTMLService_Create(world: any): void;
function _ut_HTML_HTMLService_Destroy(): void;

}


declare namespace ut.PlayableAd {
    var Service: PlayableAdService;

    enum AdState {
        Loading,
        Hidden,
        Default,
        Expanded
    }

    type ForceOrientationState = "portrait" | "landscape" | "none";

    type PlacementType = "inline" | "interstitial";

    interface OrientationProperties {
        public allowOrientationChange: boolean;
        public forceOrientation: ForceOrientationState;
    }

    class AdAnalytics {
        sendFirstInteraction();
        sendWin();
        sendFail();
        sendTutorialComplete();
        sendCTAClickedEndCard();
        sendCTAClickedGameplay();
        sendProgressionComplete();
    }

    class PlayableAdService {
        sendAnalyticsEvent(event: string, data: object);
        complete();
        getConfiguration(): object;
        getState(): AdState;
        openURL(url: string);
        openStore();
        close();
        getOrientationProperties(): OrientationProperties;
        setOrientationProperties(props: OrientationProperties);
        getPlacementType(): PlacementType;
        analytics(): AdAnalytics;
    }
}declare namespace ut.PlayableAd {
/** Describes the current state of the playable ad.*/
enum AdState {
  /** The playable is hidden.*/
  Hidden = 0,
  /** The playable is in the default fixed position.*/
  Default = 1,
  /** The playable is still initializing and not yet ready.*/
  Loading = 2,
  /** The playable is expanded, and occupies a larger screen area.*/
  Expanded = 3,
}
}
declare namespace ut.PlayableAd {

/** Specifies how the playable's orientation should be handled.*/
class OrientationProperties {
  constructor(allowOrientationChange?: boolean, forceOrientation?: string);
  /** Determines whether the playable's orientation should be allowed to change.*/
  allowOrientationChange: boolean;
  /** Forces the playable to a specific orientation.*/
  forceOrientation: string;
  static _size: number;
  static _fromPtr(p: number, v?: OrientationProperties): OrientationProperties;
  static _toPtr(p: number, v: OrientationProperties): void;
  static _tempHeapPtr(v: OrientationProperties): number;
}
interface OrientationPropertiesComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly allowOrientationChange: ComponentFieldDesc;
  
}

}
declare namespace ut.PlayableAd {

/** Specifies the store URLs to open if a user interacts with the end card of
    the playable.*/
class PlayableAdInfo extends ut.Component {
  constructor(googlePlayStoreUrl?: string, appStoreUrl?: string);
  /** Determines the Google Play store URL of the advertised product
      on the Android platform.*/
  googlePlayStoreUrl: string;
  /** Determines the Apple App store URL of the advertised product
      on the iOS platform.*/
  appStoreUrl: string;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: PlayableAdInfo): PlayableAdInfo;
  static _toPtr(p: number, v: PlayableAdInfo): void;
  static _tempHeapPtr(v: PlayableAdInfo): number;
  static _dtorFn(v: PlayableAdInfo): void;
}

}

declare namespace Module {

}


declare namespace ut.Animation {

/** Binds an animation curve to a specific property. The property and the curve
    must be of the same type. For example, to animate a Vector3 property,
    you must use a StepCurveVector3, LinearCurveVector3, or BezierCurveVector3
    component.*/
class AnimationPlayerDesc {
  constructor(componentId?: ComponentTypeId, propertyOffset?: number, curve?: Entity, entityPath?: string);
  /** The component that contains the property to be animated.*/
  componentId: ComponentTypeId;
  /** The offset in memory of the property to be animated.*/
  propertyOffset: number;
  /** The curve to bind to the property.*/
  curve: Entity;
  /** The entity to which the component containing the property to be animated
      is attached.*/
  entityPath: string;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDesc): AnimationPlayerDesc;
  static _toPtr(p: number, v: AnimationPlayerDesc): void;
  static _tempHeapPtr(v: AnimationPlayerDesc): number;
}
interface AnimationPlayerDescComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
  static readonly curve: EntityComponentFieldDesc;
  
}

}
declare namespace ut.Animation {

/** A type-safe wrapper for Float properties.*/
class AnimationPlayerDescFloat {
  constructor(desc?: AnimationPlayerDesc);
  desc: AnimationPlayerDesc;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDescFloat): AnimationPlayerDescFloat;
  static _toPtr(p: number, v: AnimationPlayerDescFloat): void;
  static _tempHeapPtr(v: AnimationPlayerDescFloat): number;
}
interface AnimationPlayerDescFloatComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly desc: AnimationPlayerDescComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** A type-safe wrapper for Vector2 properties.*/
class AnimationPlayerDescVector2 {
  constructor(desc?: AnimationPlayerDesc);
  desc: AnimationPlayerDesc;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDescVector2): AnimationPlayerDescVector2;
  static _toPtr(p: number, v: AnimationPlayerDescVector2): void;
  static _tempHeapPtr(v: AnimationPlayerDescVector2): number;
}
interface AnimationPlayerDescVector2ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly desc: AnimationPlayerDescComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** A type-safe wrapper for Vector3 properties.*/
class AnimationPlayerDescVector3 {
  constructor(desc?: AnimationPlayerDesc);
  desc: AnimationPlayerDesc;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDescVector3): AnimationPlayerDescVector3;
  static _toPtr(p: number, v: AnimationPlayerDescVector3): void;
  static _tempHeapPtr(v: AnimationPlayerDescVector3): number;
}
interface AnimationPlayerDescVector3ComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly desc: AnimationPlayerDescComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** A type-safe wrapper for Quaternion properties.*/
class AnimationPlayerDescQuaternion {
  constructor(desc?: AnimationPlayerDesc);
  desc: AnimationPlayerDesc;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDescQuaternion): AnimationPlayerDescQuaternion;
  static _toPtr(p: number, v: AnimationPlayerDescQuaternion): void;
  static _tempHeapPtr(v: AnimationPlayerDescQuaternion): number;
}
interface AnimationPlayerDescQuaternionComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly desc: AnimationPlayerDescComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** A type-safe wrapper for Color properties.*/
class AnimationPlayerDescColor {
  constructor(desc?: AnimationPlayerDesc);
  desc: AnimationPlayerDesc;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationPlayerDescColor): AnimationPlayerDescColor;
  static _toPtr(p: number, v: AnimationPlayerDescColor): void;
  static _tempHeapPtr(v: AnimationPlayerDescColor): number;
}
interface AnimationPlayerDescColorComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly desc: AnimationPlayerDescComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** Animation clip info.*/
class AnimationClipInfo {
  constructor(startTime?: number, endTime?: number);
  /** The animation start time, in seconds.*/
  startTime: number;
  /** The animation end time, in seconds.*/
  endTime: number;
  static _size: number;
  static _fromPtr(p: number, v?: AnimationClipInfo): AnimationClipInfo;
  static _toPtr(p: number, v: AnimationClipInfo): void;
  static _tempHeapPtr(v: AnimationClipInfo): number;
}
interface AnimationClipInfoComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly startTime: ComponentFieldDesc;
  static readonly endTime: ComponentFieldDesc;
}

}
declare namespace ut.Animation {

/** Holds the file name of the animation to load. An AnimationClip component
    is added to Entities with this component automatically.*/
class AnimationClipSource extends ut.Component {
  constructor(file?: string);
  /** The file name of the animation to load.*/
  file: string;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AnimationClipSource): AnimationClipSource;
  static _toPtr(p: number, v: AnimationClipSource): void;
  static _tempHeapPtr(v: AnimationClipSource): number;
  static _dtorFn(v: AnimationClipSource): void;
}

}
declare namespace ut.Animation {

/** Defines an animation clip (array of curves assigned to properties of different types).*/
class AnimationClip extends ut.Component {
  constructor(animationPlayerDescFloat?: AnimationPlayerDescFloat[], animationPlayerDescVector2?: AnimationPlayerDescVector2[], animationPlayerDescVector3?: AnimationPlayerDescVector3[], animationPlayerDescQuaternion?: AnimationPlayerDescQuaternion[], animationPlayerDescColor?: AnimationPlayerDescColor[]);
  animationPlayerDescFloat: AnimationPlayerDescFloat[];
  animationPlayerDescVector2: AnimationPlayerDescVector2[];
  animationPlayerDescVector3: AnimationPlayerDescVector3[];
  animationPlayerDescQuaternion: AnimationPlayerDescQuaternion[];
  animationPlayerDescColor: AnimationPlayerDescColor[];
  
  
  
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AnimationClip): AnimationClip;
  static _toPtr(p: number, v: AnimationClip): void;
  static _tempHeapPtr(v: AnimationClip): number;
  static _dtorFn(v: AnimationClip): void;
}

}
declare namespace ut.Animation {

/** Binds an animation clip to an animated entity and holds the animation state.
    This component is required for the animation to take place. The animation
    starts immediately after this component is added to the entity.*/
class AnimationClipPlayer extends ut.Component {
  constructor(animationClip?: Entity, time?: number, speed?: number, paused?: boolean, loopMode?: LoopMode);
  /** The entity with the AnimationClip component.*/
  animationClip: Entity;
  /** The current time of the animation, in seconds. This value starts from
      0.0 and changes every frame based on the animation speed.*/
  time: number;
  /** The playback speed of the animation. 1 is normal speed. Negative values
      cause the animation to play backwards.*/
  speed: number;
  /** If true, animation playback stops, and the time value does not change.*/
  paused: boolean;
  /** The looping mode for the entire animation.*/
  loopMode: LoopMode;
  static readonly animationClip: EntityComponentFieldDesc;
  static readonly time: ComponentFieldDesc;
  static readonly speed: ComponentFieldDesc;
  static readonly paused: ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AnimationClipPlayer): AnimationClipPlayer;
  static _toPtr(p: number, v: AnimationClipPlayer): void;
  static _tempHeapPtr(v: AnimationClipPlayer): number;
  static _dtorFn(v: AnimationClipPlayer): void;
}

}
declare namespace ut.Animation {

/** Specifies that animation results should be stored directly in the target entity.
    This component requires that an AnimationClipPlayer component be attached
    to the same entity as this component.*/
class AnimationTarget extends ut.Component {
  constructor(target?: Entity);
  /** The entity whose components are animated.*/
  target: Entity;
  static readonly target: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: AnimationTarget): AnimationTarget;
  static _toPtr(p: number, v: AnimationTarget): void;
  static _tempHeapPtr(v: AnimationTarget): number;
  static _dtorFn(v: AnimationTarget): void;
}

}
declare namespace ut.Animation {
/** Helper methods for managing animations.*/
class AnimationService {
  /** Gets animation clip info (start and end time) for the animationClip entity.*/
  static getAnimationClipInfo(world: ut.WorldBase, animationClip: Entity): AnimationClipInfo;
  /** Creates a new entity with the AnimationClipPlayer component and sets
      it up to run immediately.*/
  static createAnimationClipPlayer(world: ut.WorldBase, target: Entity, animationClip: Entity, speed: number, loopMode: LoopMode, newEntityName: string): Entity;
  /** Creates the context required for animation blending.*/
  static createAnimationBlenderContext(world: ut.WorldBase, target: Entity, name: string): Entity;
  /** Blends the currently playing animation with the new animation. If no
      animation was running before, the new animation is blended with the
      entity's current property values.*/
  static blendAnimation(world: ut.WorldBase, context: Entity, animationClip: Entity, transitionSpeed: number, animationSpeed: number, loopMode: LoopMode): Entity;
  /** Destroys the animation blender context and associated entities.*/
  static destroyAnimationBlenderContext(world: ut.WorldBase, entity: Entity): void;
}
}
declare namespace ut.Animation {
/** System that works on entities that have an AnimationClipSource component.
    It loads animation clips from file and stores the results in AnimationClip
    components, which it adds to the entities automatically.*/
var AnimationClipSourceSystem: ut.System;
}
declare namespace ut.Animation {
/** Evaluates animation clips (defined in an AnimationClip component) according
    to the current state of the AnimationClipPlayer component. Evaluated values
    for animated properties can be applied directly on the target entity or
    stored internally.*/
var AnimationClipPlayerSystem: ut.System;
}
declare namespace ut.Animation {
/** System used internally for blending animations.*/
var AnimationBlenderSystem: ut.System;
}
declare namespace ut.Animation {
/** Applies property values from the AnimationResult on the target entity.*/
var AnimationResultApplierSystem: ut.System;
}

declare namespace Module {
function _ut_Animation_AnimationService_getAnimationClipInfo(world: any, animationClip: any): void;
function _ut_Animation_AnimationService_createAnimationClipPlayer(world: any, target: any, animationClip: any, speed: any, loopMode: any, newEntityName: any): void;
function _ut_Animation_AnimationService_createAnimationBlenderContext(world: any, target: any, name: any): void;
function _ut_Animation_AnimationService_blendAnimation(world: any, context: any, animationClip: any, transitionSpeed: any, animationSpeed: any, loopMode: any): void;
function _ut_Animation_AnimationService_destroyAnimationBlenderContext(world: any, entity: any): void;

}


declare namespace ut.Tweens.TweenService {
    function addTween(world: ut.World, entity: ut.Entity, field: any,
        startval: ut.Math.Vector3,
        targetval: ut.Math.Vector3,
        duration: number,
        timeOffset: number,
        loopmode: ut.Core2D.LoopMode,
        func: ut.Tweens.TweenFunc,
        destroyWhenDone: boolean);
    function addTween(world: ut.World, entity: ut.Entity, field: any,
        startval: ut.Math.Vector2,
        targetval: ut.Math.Vector2,
        duration: number,
        timeOffset: number,
        loopmode: ut.Core2D.LoopMode,
        func: ut.Tweens.TweenFunc,
        destroyWhenDone: boolean);
    function addTween(world: ut.World, entity: ut.Entity, field: any,
        startval: ut.Math.Quaternion,
        targetval: ut.Math.Quaternion,
        duration: number,
        timeOffset: number,
        loopmode: ut.Core2D.LoopMode,
        func: ut.Tweens.TweenFunc,
        destroyWhenDone: boolean);
    function addTween(world: ut.World, entity: ut.Entity, field: any,
        startval: ut.Core2D.Color,
        targetval: ut.Core2D.Color,
        duration: number,
        timeOffset: number,
        loopmode: ut.Core2D.LoopMode,
        func: ut.Tweens.TweenFunc,
        destroyWhenDone: boolean);
    function addTween(world: ut.World, entity: ut.Entity, field: any,
        startval: number,
        targetval: number,
        duration: number,
        timeOffset: number,
        loopmode: ut.Core2D.LoopMode,
        func: ut.Tweens.TweenFunc,
        destroyWhenDone: boolean);

    function setValue(world: ut.World, entity: ut.Entity, field: any, value: ut.Math.Vector3);
    function setValue(world: ut.World, entity: ut.Entity, field: any, value: ut.Math.Vector2);
    function setValue(world: ut.World, entity: ut.Entity, field: any, value: ut.Math.Quaternion);
    function setValue(world: ut.World, entity: ut.Entity, field: any, value: ut.Core2D.Color);
    function setValue(world: ut.World, entity: ut.Entity, field: any, value: number);
}
declare namespace ut.Tweens {
/** Enum that lists tweening functions that you can pass into addTween functions
    on TweenService.*/
enum TweenFunc {
  External = 0,
  Linear = 1,
  Hardstep = 2,
  Smoothstep = 3,
  Cosine = 4,
  InQuad = 5,
  OutQuad = 6,
  InOutQuad = 7,
  InCubic = 8,
  OutCubic = 9,
  InOutCubic = 10,
  InQuart = 11,
  OutQuart = 12,
  InOutQuart = 13,
  InQuint = 14,
  OutQuint = 15,
  InOutQuint = 16,
  InBack = 17,
  OutBack = 18,
  InOutBack = 19,
  InBounce = 20,
  OutBounce = 21,
  InOutBounce = 22,
  InCircle = 23,
  OutCircle = 24,
  InOutCircle = 25,
  InExponential = 26,
  OutExponential = 27,
  InOutExponential = 28,
}
}
declare namespace ut.Tweens {

/** Structure that describes a single tween. It is passed into the addTween
    functions on TweenService.*/
class TweenDesc {
  constructor(cid?: ComponentTypeId, offset?: number, duration?: number, func?: TweenFunc, loop?: LoopMode, destroyWhenDone?: boolean, t?: number);
  /** Target: target component.*/
  cid: ComponentTypeId;
  /** Target: offset in the component.
      This is the field offset into the component memory, in bytes.
      This offset is set automatically when using JS or TS generic addTween functions.
      Internally, it is retrieved from the ComponentType.FieldName.$o
      prototype value. For example TransformLocalPosition.position.y.$o has
      an offset of 4: The second float value inside the TransformLocalPosition
      component.*/
  offset: number;
  /** Duration of tween, in seconds.*/
  duration: number;
  /** The tweening function to use, as defined in the TweenFunc enum.*/
  func: TweenFunc;
  /** The looping behavior to use.*/
  loop: LoopMode;
  /** If true, destroys the tweening entity (not the target entity) when
      the tweening operation ends.*/
  destroyWhenDone: boolean;
  /** Current time of tween, in seconds. Negative if the tween has not started.
      Setting a negative value delays the start of the tween.
      You can chain multiple tweens together by having them start at different offsets.*/
  t: number;
  static _size: number;
  static _fromPtr(p: number, v?: TweenDesc): TweenDesc;
  static _toPtr(p: number, v: TweenDesc): void;
  static _tempHeapPtr(v: TweenDesc): number;
}
interface TweenDescComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
  static readonly duration: ComponentFieldDesc;
  
  
  static readonly destroyWhenDone: ComponentFieldDesc;
  static readonly t: ComponentFieldDesc;
}

}
declare namespace ut.Tweens {

/** An active tween. The addTween functions on TweenService create an entity with
    the TweenComponent on it.
    Once the tween is created, you can use callbacks in the Watcher module to
    watch for the current state of the tweening operation.
    
    Most of this component's fields are copied from TweenDesc when you use
    TweenService::addTween to add a new tween.
    
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenComponent extends ut.Component {
  constructor(target?: Entity, cid?: ComponentTypeId, offset?: number, duration?: number, func?: TweenFunc, loop?: LoopMode, destroyWhenDone?: boolean, t?: number, started?: boolean, ended?: boolean, loopCount?: number);
  /** Target: target entity.*/
  target: Entity;
  /** Target: target component.*/
  cid: ComponentTypeId;
  /** Target: offset in the component.
      This is the field offset into the component memory, in bytes.
      This offset is set automatically when using JS or TS generic addTween functions.
      Internally, it is retrieved from the ComponentType.FieldName.$o
      prototype value. For example TransformLocalPosition.position.y.$o has
      an offset of 4: The second float value inside the TransformLocalPosition
      component.*/
  offset: number;
  /** Duration of tween, in seconds.*/
  duration: number;
  /** The tweening function to use, as defined in the TweenFunc enum.*/
  func: TweenFunc;
  /** The looping behavior to use.*/
  loop: LoopMode;
  /** If true, destroys the tweening entity (not the target entity) when
      the tweening operation ends.*/
  destroyWhenDone: boolean;
  /** Current time of tween, in seconds. Negative if the tween has not started.*/
  t: number;
  /** True if the tween has started playing (t>0)
      This can be watched using the Watcher module.*/
  started: boolean;
  /** True if the tween has stopped playing (t>duration).
      Never true if looping is continuous.
      You can use the Watcher module to watch this, and trigger a calback
      when the tween is finished playing.*/
  ended: boolean;
  /** Number of times the tween has looped.
      You can use the Watcher module to watch this, and trigger a calback
      every loop.*/
  loopCount: number;
  static readonly target: EntityComponentFieldDesc;
  
  
  static readonly duration: ComponentFieldDesc;
  
  
  static readonly destroyWhenDone: ComponentFieldDesc;
  static readonly t: ComponentFieldDesc;
  static readonly started: ComponentFieldDesc;
  static readonly ended: ComponentFieldDesc;
  static readonly loopCount: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenComponent): TweenComponent;
  static _toPtr(p: number, v: TweenComponent): void;
  static _tempHeapPtr(v: TweenComponent): number;
  static _dtorFn(v: TweenComponent): void;
}

}
declare namespace ut.Tweens {

/** An active tween of a float value. The addTween functions on TweenService
    create an entity with the type-specific Tween component and the generic
    TweenComponent.
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenFloat extends ut.Component {
  constructor(start?: number, end?: number);
  start: number;
  end: number;
  static readonly start: ComponentFieldDesc;
  static readonly end: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenFloat): TweenFloat;
  static _toPtr(p: number, v: TweenFloat): void;
  static _tempHeapPtr(v: TweenFloat): number;
  static _dtorFn(v: TweenFloat): void;
}

}
declare namespace ut.Tweens {

/** An active tween of a Vector2 value. The addTween functions on TweenService
    create an entity with the type-specific Tween component and the generic
    TweenComponent.
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenVector2 extends ut.Component {
  constructor(start?: Vector2, end?: Vector2);
  start: Vector2;
  end: Vector2;
  static readonly start: Vector2ComponentFieldDesc;
  static readonly end: Vector2ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenVector2): TweenVector2;
  static _toPtr(p: number, v: TweenVector2): void;
  static _tempHeapPtr(v: TweenVector2): number;
  static _dtorFn(v: TweenVector2): void;
}

}
declare namespace ut.Tweens {

/** An active tween of a Vector3 value. The addTween functions on TweenService
    create an entity with the type-specific Tween component and the generic
    TweenComponent.
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenVector3 extends ut.Component {
  constructor(start?: Vector3, end?: Vector3);
  start: Vector3;
  end: Vector3;
  static readonly start: Vector3ComponentFieldDesc;
  static readonly end: Vector3ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenVector3): TweenVector3;
  static _toPtr(p: number, v: TweenVector3): void;
  static _tempHeapPtr(v: TweenVector3): number;
  static _dtorFn(v: TweenVector3): void;
}

}
declare namespace ut.Tweens {

/** An active tween of a Quaternion value. The addTween functions on TweenService
    create an entity with the type-specific Tween component and the generic
    TweenComponent.
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenQuaternion extends ut.Component {
  constructor(start?: Quaternion, end?: Quaternion);
  start: Quaternion;
  end: Quaternion;
  static readonly start: QuaternionComponentFieldDesc;
  static readonly end: QuaternionComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenQuaternion): TweenQuaternion;
  static _toPtr(p: number, v: TweenQuaternion): void;
  static _tempHeapPtr(v: TweenQuaternion): number;
  static _dtorFn(v: TweenQuaternion): void;
}

}
declare namespace ut.Tweens {

/** An active tween of a Color value. The addTween functions on TweenService
    create an entity with the type-specific Tween component and the generic
    You can dynamically change values inside the TweenComponent at any time.*/
class TweenColor extends ut.Component {
  constructor(start?: Color, end?: Color);
  start: Color;
  end: Color;
  static readonly start: ColorComponentFieldDesc;
  static readonly end: ColorComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: TweenColor): TweenColor;
  static _toPtr(p: number, v: TweenColor): void;
  static _tempHeapPtr(v: TweenColor): number;
  static _dtorFn(v: TweenColor): void;
}

}
declare namespace ut.Tweens {
class TweenService {
  /** Creates a new tweening entity with a TweenFloat component and a TweenComponent component.
      This a helper function that does the following:
      e = createEntity
      addComponent(e,TweenFloat)
      addComponent(e,TweenComponent)
      It then populates the two components with the values passed as parameters.
      Returns the new tweening entity, e.
      From a JS or TS script, use TweenService.addTween without a type.*/
  static addTweenFloat(world: ut.WorldBase, e: Entity, start: number, end: number, parameters: TweenDesc): Entity;
  /** Creates a new tweening entity with a TweenVector2 component and a TweenComponent component.
      This a helper function that does the following:
      e = createEntity
      addComponent(e,TweenVector2)
      addComponent(e,TweenComponent)
      It then populates the two components with the values passed as parameters.
      Returns the new tweening entity, e.
      From a JS or TS script, use TweenService.addTween without a type.*/
  static addTweenVector2(world: ut.WorldBase, e: Entity, start: Vector2, end: Vector2, parameters: TweenDesc): Entity;
  /** Creates a new tweening entity with the TweenVector3 component and a TweenComponent component.
      This a helper function that does the following:
      e = createEntity
      addComponent(e,TweenVector3)
      addComponent(e,TweenComponent)
      It then populates the two components with the values passed as parameters.
      Returns the new tweening entity, e.
      From a JS or TS script, use TweenService.addTween without a type.*/
  static addTweenVector3(world: ut.WorldBase, e: Entity, start: Vector3, end: Vector3, parameters: TweenDesc): Entity;
  /** Creates a new tweening entity with the TweenQuaternion component and a TweenComponent component.
      This a helper function that does the following:
      e = createEntity
      addComponent(e,TweenQuaternion)
      addComponent(e,TweenComponent)
      It then populates the two components with the values passed as parameters.
      Returns the new tweening entity, e.
      From a JS or TS script, use TweenService.addTween without a type.*/
  static addTweenQuaternion(world: ut.WorldBase, e: Entity, start: Quaternion, end: Quaternion, parameters: TweenDesc): Entity;
  /** Creates a new tweening entity with the TweenColor component and a TweenComponent component.
      This a helper function that does the following:
      e = createEntity
      addComponent(e,TweenColor)
      addComponent(e,TweenComponent)
      It then populates the two components with the values passed as parameters.
      Returns the new tweening entity, e.
      From a JS or TS script, use TweenService.addTween without a type.*/
  static addTweenColor(world: ut.WorldBase, e: Entity, start: Color, end: Color, parameters: TweenDesc): Entity;
  /** Helper function that removes all tween entities that reference target entity e.*/
  static removeAllTweens(world: ut.WorldBase, e: Entity): void;
  /** Helper function that removes all tween entities in the world.
      You can use it to clean up after a level.*/
  static removeAllTweensInWorld(world: ut.WorldBase): void;
  /** Helper function that removes all tween entities that have stopped playing.
      You can use it to clean up after a level or transition sequence.*/
  static removeAllEndedTweens(world: ut.WorldBase): void;
  /** Sets a tweenable value.
      This is a shorthand helper function that writes the component data.
      temp = getComponentData(e,cid)
      temp[offset] = value
      setComponentData(e,temp)
      From a JS or TS script, use TweenService.setValue without a type.*/
  static setValueQuaternion(world: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, value: Quaternion): void;
  /** Sets a tweenable value.
      This is a shorthand helper function that writes the component data.
      temp = getComponentData(e,cid)
      temp[offset] = value
      setComponentData(e,temp)
      From a JS or TS script, use TweenService.setValue without a type.*/
  static setValueVector3(world: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, value: Vector3): void;
  /** Sets a tweenable value.
      This is a shorthand helper function that writes the component data.
      temp = getComponentData(e,cid)
      temp[offset] = value
      setComponentData(e,temp)
      From a JS or TS script, use TweenService.setValue without a type.*/
  static setValueVector2(world: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, value: Vector2): void;
  /** Sets a tweenable value.
      This is a shorthand helper function that writes the component data.
      temp = getComponentData(e,cid)
      temp[offset] = value
      setComponentData(e,temp)
      From a JS or TS script, use TweenService.setValue without a type.*/
  static setValueColor(world: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, value: Color): void;
  /** Sets a tweenable value.
      This is a shorthand helper function that writes the component data.
      temp = getComponentData(e,cid)
      temp[offset] = value
      setComponentData(e,temp)
      From a JS or TS script, use TweenService.setValue without a type.*/
  static setValueFloat(world: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, value: number): void;
}
}
declare namespace ut.Tweens {
/** System that must be scheduled to drive all tweens.*/
var TweenSystem: ut.System;
}

declare namespace Module {
function _ut_Tweens_TweenService_addTweenFloat(world: any, e: any, start: any, end: any, parameters: any): void;
function _ut_Tweens_TweenService_addTweenVector2(world: any, e: any, start: any, end: any, parameters: any): void;
function _ut_Tweens_TweenService_addTweenVector3(world: any, e: any, start: any, end: any, parameters: any): void;
function _ut_Tweens_TweenService_addTweenQuaternion(world: any, e: any, start: any, end: any, parameters: any): void;
function _ut_Tweens_TweenService_addTweenColor(world: any, e: any, start: any, end: any, parameters: any): void;
function _ut_Tweens_TweenService_removeAllTweens(world: any, e: any): void;
function _ut_Tweens_TweenService_removeAllTweensInWorld(world: any): void;
function _ut_Tweens_TweenService_removeAllEndedTweens(world: any): void;
function _ut_Tweens_TweenService_setValueQuaternion(world: any, e: any, cid: any, offset: any, value: any): void;
function _ut_Tweens_TweenService_setValueVector3(world: any, e: any, cid: any, offset: any, value: any): void;
function _ut_Tweens_TweenService_setValueVector2(world: any, e: any, cid: any, offset: any, value: any): void;
function _ut_Tweens_TweenService_setValueColor(world: any, e: any, cid: any, offset: any, value: any): void;
function _ut_Tweens_TweenService_setValueFloat(world: any, e: any, cid: any, offset: any, value: any): void;

}


declare namespace ut.Video {
/** Lists the possible states a video clip can be in during loading.
    Used by the VideoClip component to display loading status.*/
enum VideoClipLoadingStatus {
  /** The is not loaded.*/
  Unloaded = 0,
  /** The clip has begun loading but is not ready to begin playback.*/
  Loading = 1,
  /** The clip is fully decoded, loaded in memory, and ready for playback.*/
  Loaded = 2,
  /** The clip could not be loaded.*/
  LoadError = 3,
}
}
declare namespace ut.Video {

/** Attach this component to a clip entity to specify a video source (media file) to play.*/
class VideoClip extends ut.Component {
  constructor(src?: string, status?: VideoClipLoadingStatus);
  /** The path to media source file. This can be a URL if the file is remote, or
      it can be the path of the source file in the project.
      HTML media types supported: MP4, WebM, Ogg.*/
  src: string;
  /** Video clip loading status. Set it back to Unloaded when updating src*/
  status: VideoClipLoadingStatus;
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: VideoClip): VideoClip;
  static _toPtr(p: number, v: VideoClip): void;
  static _tempHeapPtr(v: VideoClip): number;
  static _dtorFn(v: VideoClip): void;
}

}
declare namespace ut.Video {

/** Use this component as a video player.
    This component is required to play a video clip, and to set options such
    as whether or not to display the video player controls, whether to loop the
    video, and so on.*/
class VideoPlayer extends ut.Component {
  constructor(clip?: Entity, controls?: boolean, loop?: boolean, currentTime?: number);
  /** The clip entity. Attach a {@link VideoClip} to it to play a video.
      The video is muted by default.*/
  clip: Entity;
  /** If true, video controls (Play/Pause/FullScreen/Volume) are displayed.*/
  controls: boolean;
  /** If true, the player automatically seeks back to the start of the
      video after reaching the end.
      This attribute is ignored if the entity has a VideoPlayerAutoDeleteOnEnd component.*/
  loop: boolean;
  /** Current playback time, in seconds*/
  currentTime: number;
  static readonly clip: EntityComponentFieldDesc;
  static readonly controls: ComponentFieldDesc;
  static readonly loop: ComponentFieldDesc;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: VideoPlayer): VideoPlayer;
  static _toPtr(p: number, v: VideoPlayer): void;
  static _tempHeapPtr(v: VideoPlayer): number;
  static _dtorFn(v: VideoPlayer): void;
}

}
declare namespace ut.Video {

/** Add this component to an entity with a VideoPlayer component to auto-delete
    a video once it reaches the end.*/
class VideoPlayerAutoDeleteOnEnd extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: VideoPlayerAutoDeleteOnEnd): VideoPlayerAutoDeleteOnEnd;
  static _toPtr(p: number, v: VideoPlayerAutoDeleteOnEnd): void;
  static _tempHeapPtr(v: VideoPlayerAutoDeleteOnEnd): number;
  static _dtorFn(v: VideoPlayerAutoDeleteOnEnd): void;
}

}
declare namespace ut.Video {
/** Video system required to add, play and remove a video.
    When an entity has VideoClip and VideoPLayer components, this system loads
    the video, and adds add a video element once the video is loaded. It also
    plays back the video. If the entity has a VideoPlayerAutotDeleteOnEnd component
    this system deletes the video after playback.*/
var VideoSystem: ut.System;
}

declare namespace Module {

}


declare namespace ut.Watchers {

type WatchChangedBoolFunc =   (e: ut.Entity, cid: number, offset: number, prevValue: boolean, newValue: boolean, watchId: number) => void;
type WatchChangedNumberFunc = (e: ut.Entity, cid: number, offset: number, prevValue: number, newValue: number, watchId: number) => void;

class WatchGroup extends WatchGroupBase {
    watchChanged(world: ut.World, entity: ut.Entity, field: any, func: WatchChangedBoolFunc);
    watchChanged(world: ut.World, entity: ut.Entity, field: any, func: WatchChangedNumberFunc);
    watchChangedMask(world: ut.World, mask: Array<number>, field: any, func: WatchChangedBoolFunc);
    watchChangedMask(world: ut.World, mask: Array<number>, field: any, func: WatchChangedNumberFunc);
}

}
declare namespace ut.Watchers {

/** Structure for describing an entity mask.*/
class EntityMask {
  constructor(all?: number[], any?: number[], sub?: number[]);
  /** List of component ids where all of the components must be set*/
  all: number[];
  /** List of component ids where at least one of the components must be set*/
  any: number[];
  /** List of component ids that must not be set*/
  sub: number[];
  static _size: number;
  static _fromPtr(p: number, v?: EntityMask): EntityMask;
  static _toPtr(p: number, v: EntityMask): void;
  static _tempHeapPtr(v: EntityMask): number;
}
interface EntityMaskComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
  
}

}
declare namespace ut.Watchers {
type WatchComponentFn = (e: Entity, cid: ComponentTypeId, watch: number) => void;
}
declare namespace ut.Watchers {
type WatchValueBoolFn = (e: Entity, cid: ComponentTypeId, offset: number, prev: boolean, value: boolean, watch: number) => void;
}
declare namespace ut.Watchers {
type WatchValueIntFn = (e: Entity, cid: ComponentTypeId, offset: number, prev: number, value: number, watch: number) => void;
}
declare namespace ut.Watchers {
type WatchValueFloatFn = (e: Entity, cid: ComponentTypeId, offset: number, prev: number, value: number, watch: number) => void;
}
declare namespace ut.Watchers {
/** Watch groups are standalone classes that group multiple watched values.
    A watch group must be scheduled as a system in order to trigger delegates/callbacks.
    
    Callbacks have the same restrictions as code running in systems. They are
    called from the watching system, just like code called from any other system.
    They are executed when the watching system runs, and not immediately when
    a value changes.*/
class WatchGroupBase {
  /** Sets a name for this watch group*/
  setName(name: string): void;
  /** Gets the name of this watch group*/
  name(): string;
  /** Removes a watcher with a specific id.
      Ids are returned by the various Watch* functions.*/
  removeWatcher(id: number): void;
  /** Removes all watchers that watch the component type cid*/
  removeAllWatchers(cid: ComponentTypeId): void;
  /** Creates a new system that references this watch group.
      A watch group can be scheduled as multiple systems, which enables advanced
      ordering of the watching system.
      You should use either Schedule or CreateSystem, not both.
      If you use both the watching system is scheduled twice, and every callback
      is called twice.*/
  createSystem(): ut.SystemBase;
  /** Shortcut for creating a system and scheduling it using s.schedule(this.CreateSystem());
      You should use either Schedule or CreateSystem, not both.
      If you use both the watching system is scheduled twice, and every callback
      is called twice.*/
  schedule(s: ut.SchedulerBase): void;
  /** Shortcut for removing the system scheduled with Schedule.*/
  unSchedule(s: ut.SchedulerBase): void;
  /** Watches entity e for the addition of component type cid.
      When the specified component is added, callback f is called.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchAdded(w: ut.WorldBase, e: Entity, cid: ComponentTypeId, f: WatchComponentFn): number;
  /** Watches all entities that match mask for the addition of component
      type cid. When the component is added, callback f is called for each
      entity that it was added to.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchAddedMask(w: ut.WorldBase, mask: EntityMask, cid: ComponentTypeId, f: WatchComponentFn): number;
  /** Watches entity e for the removal of component type cid.
      When the component is removed, callback f is called.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchRemoved(w: ut.WorldBase, e: Entity, cid: ComponentTypeId, f: WatchComponentFn): number;
  /** Watches all entities that match mask for the removal of component type cid.
      When the component is removed, the callback f is called for each entity
      that it was removed from.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchRemovedMask(w: ut.WorldBase, mask: EntityMask, cid: ComponentTypeId, f: WatchComponentFn): number;
  /** Watches entity e for changes to a bool at offset, on a component with id cid.
      When the value is changed, f is called.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedBool(w: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, f: WatchValueBoolFn): number;
  /** Watches all entities that match mask for changes to a bool at offset,
      on component cid.
      When the value is changed, f is called for each individual entity
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedBoolMask(w: ut.WorldBase, mask: EntityMask, cid: ComponentTypeId, offset: number, f: WatchValueBoolFn): number;
  /** Watches entity e for changes to an int at offset, on a component with id cid.
      When the value is changed, f is called.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedInt(w: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, f: WatchValueIntFn): number;
  /** Watches all entities that match mask for changes to an int at offset,
      on component cid.
      When the value is changed, f is called for each individual entity
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedIntMask(w: ut.WorldBase, mask: EntityMask, cid: ComponentTypeId, offset: number, f: WatchValueIntFn): number;
  /** Watches entity e for changes to a float at offset, on a component with id cid.
      When the value is changed, f is called.
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedFloat(w: ut.WorldBase, e: Entity, cid: ComponentTypeId, offset: number, f: WatchValueFloatFn): number;
  /** Watches all entities that match mask for changes to a float at offset,
      on component cid.
      When the value is changed, f is called for each individual entity
      Returns an id for the watcher that can be used in RemoveWatcher.
      
      Note that all watching callbacks are called when the watching system
      runs, not when the value is changed.*/
  watchChangedFloatMask(w: ut.WorldBase, mask: EntityMask, cid: ComponentTypeId, offset: number, f: WatchValueFloatFn): number;
}
}

declare namespace Module {
function _ut_Watchers_WatchGroup_WatchGroup(): number;
function _ut_Watchers_WatchGroup_shRelease(self: number): void;
function _ut_Watchers_WatchGroup_SetName(selfPtr: any, name: any): void;
function _ut_Watchers_WatchGroup_Name(selfPtr: any): void;
function _ut_Watchers_WatchGroup_RemoveWatcher(selfPtr: any, id: any): void;
function _ut_Watchers_WatchGroup_RemoveAllWatchers(selfPtr: any, cid: any): void;
function _ut_Watchers_WatchGroup_CreateSystem(selfPtr: any): any;
function _ut_Watchers_WatchGroup_Schedule(selfPtr: any, s: any): void;
function _ut_Watchers_WatchGroup_UnSchedule(selfPtr: any, s: any): void;
function _ut_Watchers_WatchGroup_WatchAdded(selfPtr: any, w: any, e: any, cid: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchAddedMask(selfPtr: any, w: any, mask: any, cid: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchRemoved(selfPtr: any, w: any, e: any, cid: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchRemovedMask(selfPtr: any, w: any, mask: any, cid: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedBool(selfPtr: any, w: any, e: any, cid: any, offset: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedBoolMask(selfPtr: any, w: any, mask: any, cid: any, offset: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedInt(selfPtr: any, w: any, e: any, cid: any, offset: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedIntMask(selfPtr: any, w: any, mask: any, cid: any, offset: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedFloat(selfPtr: any, w: any, e: any, cid: any, offset: any, f: any): any;
function _ut_Watchers_WatchGroup_WatchChangedFloatMask(selfPtr: any, w: any, mask: any, cid: any, offset: any, f: any): any;

}


declare namespace ut.UIControls {

/** Captures the interaction between the mouse/touch and the UI control.
    This component requires the RectTransform component, from the UILayout module.*/
class MouseInteraction extends ut.Component {
  constructor(down?: boolean, over?: boolean, clicked?: boolean);
  /** True if the mouse button is pressed and the press started when the
      cursor was over the UI control.*/
  down: boolean;
  /** True if the cursor is inside the bounds of the UI control.*/
  over: boolean;
  /** True if the UI control is clicked. A click consists of a mouse-down
      action and a corresponding mouse-up action while the cursor is inside
      the control's bounds.*/
  clicked: boolean;
  static readonly down: ComponentFieldDesc;
  static readonly over: ComponentFieldDesc;
  static readonly clicked: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: MouseInteraction): MouseInteraction;
  static _toPtr(p: number, v: MouseInteraction): void;
  static _tempHeapPtr(v: MouseInteraction): number;
  static _dtorFn(v: MouseInteraction): void;
}

}
declare namespace ut.UIControls {

/** Component for UI buttons.*/
class Button extends ut.Component {
  constructor(sprite2DRenderer?: Entity, transition?: Entity);
  /** Reference to an entity with a Sprite2DRenderer component that represents
      the button's default state. Mouse/touch interaction, captured by the
      MouseInteraction component, swaps or modifies the sprite based on the
      type of transition you apply.
      If this is set to NONE, it assumes that the underlying entity (the one
      that the Button component is attached to) also has a Sprite2DRenderer
      component, and uses that.*/
  sprite2DRenderer: Entity;
  /** Reference to an entity that defines visual transitions based on mouse/
      touch interaction captured by the MouseInteraction component. For example,
      A SpriteTransition or ColorTintTransition component.*/
  transition: Entity;
  static readonly sprite2DRenderer: EntityComponentFieldDesc;
  static readonly transition: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Button): Button;
  static _toPtr(p: number, v: Button): void;
  static _tempHeapPtr(v: Button): number;
  static _dtorFn(v: Button): void;
}

}
declare namespace ut.UIControls {

/** Component for toggle buttons.*/
class Toggle extends ut.Component {
  constructor(isOn?: boolean, sprite2DRenderer?: Entity, transition?: Entity, transitionChecked?: Entity);
  /** True if the toggle is on (for example, checked).*/
  isOn: boolean;
  /** Reference to an entity with a Sprite2DRenderer component that represents
      the button's toggle's state. Mouse/touch interaction, captured by the
      MouseInteraction component, swaps or modifies the sprite based on the
      type of transitions you apply.
      If this is set to NONE, it assumes that the underlying entity (the one
      that the Toggle component is attached to) also has a Sprite2DRenderer
      component, and uses that.*/
  sprite2DRenderer: Entity;
  /** Reference to an entity that defines visual transitions based on mouse/
      touch interaction captured by the MouseInteraction component. For example,
      A SpriteTransition or ColorTintTransition component.
      Used when isOn is true.*/
  transition: Entity;
  /** Reference to an entity that defines visual transitions based on mouse/
      touch interaction captured by the MouseInteraction component. For example,
      A SpriteTransition or ColorTintTransition component.
      Used when isOn is false.*/
  transitionChecked: Entity;
  static readonly isOn: ComponentFieldDesc;
  static readonly sprite2DRenderer: EntityComponentFieldDesc;
  static readonly transition: EntityComponentFieldDesc;
  static readonly transitionChecked: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: Toggle): Toggle;
  static _toPtr(p: number, v: Toggle): void;
  static _tempHeapPtr(v: Toggle): number;
  static _dtorFn(v: Toggle): void;
}

}
declare namespace ut.UIControls {

/** Applies standard sprite-swap effect on controls that have a MouseInteraction
    component.*/
class SpriteTransition extends ut.Component {
  constructor(normal?: Entity, hover?: Entity, pressed?: Entity, disabled?: Entity);
  /** The sprite used when MouseInteraction.down = false and MouseInteraction.over = false.*/
  normal: Entity;
  /** The sprite used when MouseInteraction.down = false and MouseInteraction.over = false.*/
  hover: Entity;
  /** The sprite used when MouseInteraction.down = true.*/
  pressed: Entity;
  /** The sprite used when the entity has an InactiveUIElement component.*/
  disabled: Entity;
  static readonly normal: EntityComponentFieldDesc;
  static readonly hover: EntityComponentFieldDesc;
  static readonly pressed: EntityComponentFieldDesc;
  static readonly disabled: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: SpriteTransition): SpriteTransition;
  static _toPtr(p: number, v: SpriteTransition): void;
  static _tempHeapPtr(v: SpriteTransition): number;
  static _dtorFn(v: SpriteTransition): void;
}

}
declare namespace ut.UIControls {

/** Applies a standard color-tint effect on controls that have a MouseInteraction
    component.*/
class ColorTintTransition extends ut.Component {
  constructor(normal?: Color, hover?: Color, pressed?: Color, disabled?: Color);
  /** The color used when MouseInteraction.down = false and MouseInteraction.over = false.*/
  normal: Color;
  /** The color used when MouseInteraction.down = false and MouseInteraction.over = false.*/
  hover: Color;
  /** The color used when MouseInteraction.down = true.*/
  pressed: Color;
  /** The color used when the entity has InactiveUIElement component.*/
  disabled: Color;
  static readonly normal: ColorComponentFieldDesc;
  static readonly hover: ColorComponentFieldDesc;
  static readonly pressed: ColorComponentFieldDesc;
  static readonly disabled: ColorComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ColorTintTransition): ColorTintTransition;
  static _toPtr(p: number, v: ColorTintTransition): void;
  static _tempHeapPtr(v: ColorTintTransition): number;
  static _dtorFn(v: ColorTintTransition): void;
}

}
declare namespace ut.UIControls {

/** Disables the UI control and resets the MouseInteraction component.*/
class InactiveUIControl extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: InactiveUIControl): InactiveUIControl;
  static _toPtr(p: number, v: InactiveUIControl): void;
  static _tempHeapPtr(v: InactiveUIControl): number;
  static _dtorFn(v: InactiveUIControl): void;
}

}
declare namespace ut.UIControls {
type UIControlEventFn = (e: Entity) => void;
}
declare namespace ut.UIControls {
/** Util class for UI controls.*/
class UIControlsService {
  /** Adds the callback that is called when the control is clicked.*/
  static addOnClickCallback(world: ut.WorldBase, uiControl: Entity, f: UIControlEventFn): void;
  /** Adds the callback that is called when the control is pressed.*/
  static addOnDownCallback(world: ut.WorldBase, uiControl: Entity, f: UIControlEventFn): void;
  /** Adds the callback that is called when the control is released.*/
  static addOnUpCallback(world: ut.WorldBase, uiControl: Entity, f: UIControlEventFn): void;
  /** Adds the callback that is called when the mouse cursor enters the control's bounds.*/
  static addOnEnterCallback(world: ut.WorldBase, uiControl: Entity, f: UIControlEventFn): void;
  /** Adds the callback that is called when the mouse cursor leaves the control's bounds.*/
  static addOnLeaveCallback(world: ut.WorldBase, uiControl: Entity, f: UIControlEventFn): void;
}
}
declare namespace ut.UIControls {
/** Updates MouseInteraction component based on the current state and position
    of the pointer.*/
var MouseInteractionSystem: ut.System;
}
declare namespace ut.UIControls {
/** Changes the value of Toggle.isOn from true to false and
    from false to true every time the Toggle control is clicked.*/
var ToggleCheckedSystem: ut.System;
}
declare namespace ut.UIControls {
/** Updates internal components related to UI controls.*/
var UIControlsSystem: ut.System;
}
declare namespace ut.UIControls {
/** Updates the appearance of the button based on the pointer interaction
    (pointer over, pointer down, etc).*/
var ButtonSystem: ut.System;
}
declare namespace ut.UIControls {
/** Updates the appearance of the toggle control based on the pointer
    interaction (pointer over, pointer down, etc).*/
var ToggleSystem: ut.System;
}

declare namespace Module {
function _ut_UIControls_UIControlsService_addOnClickCallback(world: any, uiControl: any, f: any): void;
function _ut_UIControls_UIControlsService_addOnDownCallback(world: any, uiControl: any, f: any): void;
function _ut_UIControls_UIControlsService_addOnUpCallback(world: any, uiControl: any, f: any): void;
function _ut_UIControls_UIControlsService_addOnEnterCallback(world: any, uiControl: any, f: any): void;
function _ut_UIControls_UIControlsService_addOnLeaveCallback(world: any, uiControl: any, f: any): void;

}


declare namespace ut.PointQuery {

/** Structure returned by queryNClosestPoints in PointQueryService.*/
class PointQueryResults {
  constructor(distances?: number[], ids?: Entity[]);
  /** Lists the distances to the points returned by the query.*/
  distances: number[];
  /** Lists the ids of the points returned by the query.*/
  ids: Entity[];
  static _size: number;
  static _fromPtr(p: number, v?: PointQueryResults): PointQueryResults;
  static _toPtr(p: number, v: PointQueryResults): void;
  static _tempHeapPtr(v: PointQueryResults): number;
}
interface PointQueryResultsComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
}

}
declare namespace ut.PointQuery {

/** Component used to mark an entity created by createPointQueryStruct in PointQueryService.*/
class PointQueryStructTag extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: PointQueryStructTag): PointQueryStructTag;
  static _toPtr(p: number, v: PointQueryStructTag): void;
  static _tempHeapPtr(v: PointQueryStructTag): number;
  static _dtorFn(v: PointQueryStructTag): void;
}

}
declare namespace ut.PointQuery {
class PointQueryService {
  /** Creates a new entity that has a point query structure, and can be passed
      to other functions in this service.
      
      The created entity has an internal hidden component, and an external
      PointQueryStructTag component.
      
      To free the allocated memory used by the structure, destroy the entity.*/
  static createPointQueryStruct(world: ut.WorldBase): Entity;
  /** Prepares and resets a point query structure.
      You can use this to clear a query structure, or optimize allocations
      when the number of items is known ahead of time.
      
  @param eQuery Entity created via createPointQueryStruct
  @param numExpectedPoints Number of points expected. This is an
      optional hint for performance and does not need to match exactly.*/
  static resetPointQueryStruct(world: ut.WorldBase, eQuery: Entity, numExpectedPoints: number): void;
  /** Adds a single point to the query structure. The point id is returned
      when the point is found.
      The id Entity does not have to be a valid entity, and can be treated
      as an integer id.
      The only disallowed id is the NONE entity, which is used internally.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static addPointToQueryStruct(world: ut.WorldBase, eQuery: Entity, point: Vector3, id: Entity): void;
  /** Explicitly build the query structure.
      Normally the query structure is built the first time the query runs,
      but you can call this function to help with timing or debugging.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static buildPointQueryStruct(world: ut.WorldBase, eQuery: Entity): void;
  /** Query for the closest point to point in ps.
      Closeness is based on Euclidean distance. This query only considers
      points inside the hull around the point, which is described by maxDist (exclusive)
      and minDist (inclusive).
      
      Returns the id of the closest point as specified when it was added.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static queryClosestPoint(world: ut.WorldBase, eQuery: Entity, point: Vector3, maxDist: number, minDist: number): Entity;
  /** Query for the n closest points to point in ps.
      Closeness is based on Euclidean distance. This query only considers
      points inside the hull around the point, which is described by maxDist (exclusive)
      and minDist (inclusive).
      Returns a list of ids and distances of the closest points, sorted by
      most distant first.
      This is slower than single-point queries. For best results, n should
      not be too large.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static queryNClosestPoints(world: ut.WorldBase, eQuery: Entity, point: Vector3, maxDist: number, minDist: number, n: number): PointQueryResults;
  /** Reference query for testing only. It yields the same results as queryClosestPoint,
      but takes much longer to execute. Avoid using it.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static queryClosestPointRef(world: ut.WorldBase, eQuery: Entity, point: Vector3, maxDist: number, minDist: number): Entity;
  /** Reference query for testing only. It yields the same results as queryNClosestPoints,
      but takes much longer to execute. Avoid using it.
      
  @param eQuery Entity created via createPointQueryStruct*/
  static queryNClosestPointsRef(world: ut.WorldBase, eQuery: Entity, point: Vector3, maxDist: number, minDist: number, n: number): PointQueryResults;
}
}

declare namespace Module {
function _ut_PointQuery_PointQueryService_createPointQueryStruct(world: any): void;
function _ut_PointQuery_PointQueryService_resetPointQueryStruct(world: any, eQuery: any, numExpectedPoints: any): void;
function _ut_PointQuery_PointQueryService_addPointToQueryStruct(world: any, eQuery: any, point: any, id: any): void;
function _ut_PointQuery_PointQueryService_buildPointQueryStruct(world: any, eQuery: any): void;
function _ut_PointQuery_PointQueryService_queryClosestPoint(world: any, eQuery: any, point: any, maxDist: any, minDist: any): void;
function _ut_PointQuery_PointQueryService_queryNClosestPoints(world: any, eQuery: any, point: any, maxDist: any, minDist: any, n: any): void;
function _ut_PointQuery_PointQueryService_queryClosestPointRef(world: any, eQuery: any, point: any, maxDist: any, minDist: any): void;
function _ut_PointQuery_PointQueryService_queryNClosestPointsRef(world: any, eQuery: any, point: any, maxDist: any, minDist: any, n: any): void;

}


declare namespace ut.TestHelper {

class StructWithArray {
  constructor(ints?: number[]);
  ints: number[];
  static _size: number;
  static _fromPtr(p: number, v?: StructWithArray): StructWithArray;
  static _toPtr(p: number, v: StructWithArray): void;
  static _tempHeapPtr(v: StructWithArray): number;
}
interface StructWithArrayComponentFieldDesc extends ut.ComponentFieldDesc {
  
}

}
declare namespace ut.TestHelper {

class StructWithArrayOfVectors {
  constructor(vs?: Vector3[]);
  vs: Vector3[];
  static _size: number;
  static _fromPtr(p: number, v?: StructWithArrayOfVectors): StructWithArrayOfVectors;
  static _toPtr(p: number, v: StructWithArrayOfVectors): void;
  static _tempHeapPtr(v: StructWithArrayOfVectors): number;
}
interface StructWithArrayOfVectorsComponentFieldDesc extends ut.ComponentFieldDesc {
  
}

}
declare namespace ut.TestHelper {

class StructWithString {
  constructor(s?: string, sv?: string[]);
  s: string;
  sv: string[];
  static _size: number;
  static _fromPtr(p: number, v?: StructWithString): StructWithString;
  static _toPtr(p: number, v: StructWithString): void;
  static _tempHeapPtr(v: StructWithString): number;
}
interface StructWithStringComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
}

}
declare namespace ut.TestHelper {

class MyInnerStruct {
  constructor(x?: number);
  x: number;
  static _size: number;
  static _fromPtr(p: number, v?: MyInnerStruct): MyInnerStruct;
  static _toPtr(p: number, v: MyInnerStruct): void;
  static _tempHeapPtr(v: MyInnerStruct): number;
}
interface MyInnerStructComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly x: ComponentFieldDesc;
}

}
declare namespace ut.TestHelper {

class MyStruct {
  constructor(ivalue?: number, fvalue?: number, bvalue?: boolean, inner?: MyInnerStruct);
  ivalue: number;
  fvalue: number;
  bvalue: boolean;
  inner: MyInnerStruct;
  static _size: number;
  static _fromPtr(p: number, v?: MyStruct): MyStruct;
  static _toPtr(p: number, v: MyStruct): void;
  static _tempHeapPtr(v: MyStruct): number;
}
interface MyStructComponentFieldDesc extends ut.ComponentFieldDesc {
  static readonly ivalue: ComponentFieldDesc;
  static readonly fvalue: ComponentFieldDesc;
  static readonly bvalue: ComponentFieldDesc;
  static readonly inner: MyInnerStructComponentFieldDesc;
}

}
declare namespace ut.TestHelper {

class ComponentWithInt extends ut.Component {
  constructor(value?: number);
  value: number;
  static readonly value: ComponentFieldDesc;
  addOne(): number;

  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithInt): ComponentWithInt;
  static _toPtr(p: number, v: ComponentWithInt): void;
  static _tempHeapPtr(v: ComponentWithInt): number;
  static _dtorFn(v: ComponentWithInt): void;
}

}
declare namespace ut.TestHelper {

class SharedComponentWithInt extends ut.Component {
  constructor(a?: number);
  a: number;
  static readonly a: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: SharedComponentWithInt): SharedComponentWithInt;
  static _toPtr(p: number, v: SharedComponentWithInt): void;
  static _tempHeapPtr(v: SharedComponentWithInt): number;
  static _dtorFn(v: SharedComponentWithInt): void;
}

}
declare namespace ut.TestHelper {

class ComponentASystemState extends ut.Component {
  constructor(val?: number);
  val: number;
  static readonly val: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentASystemState): ComponentASystemState;
  static _toPtr(p: number, v: ComponentASystemState): void;
  static _tempHeapPtr(v: ComponentASystemState): number;
  static _dtorFn(v: ComponentASystemState): void;
}

}
declare namespace ut.TestHelper {

class ComponentWithArray extends ut.Component {
  constructor(ints?: number[]);
  ints: number[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithArray): ComponentWithArray;
  static _toPtr(p: number, v: ComponentWithArray): void;
  static _tempHeapPtr(v: ComponentWithArray): number;
  static _dtorFn(v: ComponentWithArray): void;
}

}
declare namespace ut.TestHelper {

class ComponentWithArrayOfVectors extends ut.Component {
  constructor(vs?: Vector3[]);
  vs: Vector3[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithArrayOfVectors): ComponentWithArrayOfVectors;
  static _toPtr(p: number, v: ComponentWithArrayOfVectors): void;
  static _tempHeapPtr(v: ComponentWithArrayOfVectors): number;
  static _dtorFn(v: ComponentWithArrayOfVectors): void;
}

}
declare namespace ut.TestHelper {

class ComponentWithString extends ut.Component {
  constructor(s?: string);
  s: string;
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithString): ComponentWithString;
  static _toPtr(p: number, v: ComponentWithString): void;
  static _tempHeapPtr(v: ComponentWithString): number;
  static _dtorFn(v: ComponentWithString): void;
}

}
declare namespace ut.TestHelper {

class ComponentWithStringArray extends ut.Component {
  constructor(sv?: string[]);
  sv: string[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithStringArray): ComponentWithStringArray;
  static _toPtr(p: number, v: ComponentWithStringArray): void;
  static _tempHeapPtr(v: ComponentWithStringArray): number;
  static _dtorFn(v: ComponentWithStringArray): void;
}

}
declare namespace ut.TestHelper {

class ComponentWithStructs extends ut.Component {
  constructor(a?: MyStruct, b?: MyStruct);
  a: MyStruct;
  b: MyStruct;
  static readonly a: MyStructComponentFieldDesc;
  static readonly b: MyStructComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithStructs): ComponentWithStructs;
  static _toPtr(p: number, v: ComponentWithStructs): void;
  static _tempHeapPtr(v: ComponentWithStructs): number;
  static _dtorFn(v: ComponentWithStructs): void;
}

}
declare namespace ut.TestHelper {
class TestHelperService {
  static addIntsInComponent(w: ut.WorldBase, e: Entity): number;
  static addIntsInStruct(s: StructWithArray): number;
  static addVectorsInComponent(w: ut.WorldBase, e: Entity): Vector3;
  static addVectorsInStruct(s: StructWithArrayOfVectors): Vector3;
  static stringReturn(): string;
  static stringArg(s: string): number;
  static stringArrayArg(sv: string[]): number;
  static stringInStruct(s: StructWithString): number;
  static stringInComponent(w: ut.WorldBase, e: Entity): number;
  static stringArrayInComponent(w: ut.WorldBase, e: Entity): number;
}
}
declare namespace ut.TestHelper {
class Bar {
  memberFunctionOnBar(): number;
}
}
declare namespace ut.TestHelper {
class Foo {
  createBar(i: number): Bar;
  funcThatTakesBar(bar: Bar): number;
}
}

declare namespace Module {
function _ut_TestHelper_ComponentWithInt_AddOne(selfPtr: any): any;
function _ut_TestHelper_TestHelperService_addIntsInComponent(w: any, e: any): any;
function _ut_TestHelper_TestHelperService_addIntsInStruct(s: any): any;
function _ut_TestHelper_TestHelperService_addVectorsInComponent(w: any, e: any): void;
function _ut_TestHelper_TestHelperService_addVectorsInStruct(s: any): void;
function _ut_TestHelper_TestHelperService_stringReturn(): void;
function _ut_TestHelper_TestHelperService_stringArg(s: any): any;
function _ut_TestHelper_TestHelperService_stringArrayArg(sv: any): any;
function _ut_TestHelper_TestHelperService_stringInStruct(s: any): any;
function _ut_TestHelper_TestHelperService_stringInComponent(w: any, e: any): any;
function _ut_TestHelper_TestHelperService_stringArrayInComponent(w: any, e: any): any;
function _ut_TestHelper_Bar_shRelease(self: number): void;
function _ut_TestHelper_Bar_memberFunctionOnBar(selfPtr: any): any;
function _ut_TestHelper_Foo_Foo(): number;
function _ut_TestHelper_Foo_shRelease(self: number): void;
function _ut_TestHelper_Foo_createBar(selfPtr: any, i: any): any;
function _ut_TestHelper_Foo_funcThatTakesBar(selfPtr: any, bar: any): any;

}


declare namespace ut.JSTestHelper {

class StructWithString {
  constructor(s?: string, sv?: string[]);
  s: string;
  sv: string[];
  static _size: number;
  static _fromPtr(p: number, v?: StructWithString): StructWithString;
  static _toPtr(p: number, v: StructWithString): void;
  static _tempHeapPtr(v: StructWithString): number;
}
interface StructWithStringComponentFieldDesc extends ut.ComponentFieldDesc {
  
  
}

}
declare namespace ut.JSTestHelper {

class EmptyComponent extends ut.Component {
  constructor();
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: EmptyComponent): EmptyComponent;
  static _toPtr(p: number, v: EmptyComponent): void;
  static _tempHeapPtr(v: EmptyComponent): number;
  static _dtorFn(v: EmptyComponent): void;
}

}
declare namespace ut.JSTestHelper {

class ComponentWithString extends ut.Component {
  constructor(s?: string, sv?: string[]);
  s: string;
  sv: string[];
  
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithString): ComponentWithString;
  static _toPtr(p: number, v: ComponentWithString): void;
  static _tempHeapPtr(v: ComponentWithString): number;
  static _dtorFn(v: ComponentWithString): void;
}

}
declare namespace ut.JSTestHelper {

class ComponentWithEntity extends ut.Component {
  constructor(a?: number, e0?: Entity, b?: number, e1?: Entity);
  a: number;
  e0: Entity;
  b: number;
  e1: Entity;
  static readonly a: ComponentFieldDesc;
  static readonly e0: EntityComponentFieldDesc;
  
  static readonly e1: EntityComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithEntity): ComponentWithEntity;
  static _toPtr(p: number, v: ComponentWithEntity): void;
  static _tempHeapPtr(v: ComponentWithEntity): number;
  static _dtorFn(v: ComponentWithEntity): void;
}

}
declare namespace ut.JSTestHelper {

class ComponentWithEntityArray extends ut.Component {
  constructor(ents?: Entity[]);
  ents: Entity[];
  
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentWithEntityArray): ComponentWithEntityArray;
  static _toPtr(p: number, v: ComponentWithEntityArray): void;
  static _tempHeapPtr(v: ComponentWithEntityArray): number;
  static _dtorFn(v: ComponentWithEntityArray): void;
}

}
declare namespace ut.JSTestHelper {

class SharedComponentWithInt extends ut.Component {
  constructor(a?: number);
  a: number;
  static readonly a: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: SharedComponentWithInt): SharedComponentWithInt;
  static _toPtr(p: number, v: SharedComponentWithInt): void;
  static _tempHeapPtr(v: SharedComponentWithInt): number;
  static _dtorFn(v: SharedComponentWithInt): void;
}

}
declare namespace ut.JSTestHelper {

class ComponentASystemState extends ut.Component {
  constructor(val?: number);
  val: number;
  static readonly val: ComponentFieldDesc;
  static readonly cid: number;
  static readonly _view: any;
  static readonly _isSharedComp: boolean;

  static _size: number;
  static _fromPtr(p: number, v?: ComponentASystemState): ComponentASystemState;
  static _toPtr(p: number, v: ComponentASystemState): void;
  static _tempHeapPtr(v: ComponentASystemState): number;
  static _dtorFn(v: ComponentASystemState): void;
}

}
declare namespace ut.JSTestHelper {
var Fence1: ut.SystemJS;
}
declare namespace ut.JSTestHelper {
var Fence2: ut.SystemJS;
}
declare namespace ut.JSTestHelper {
var TestSystemA: ut.SystemJS;
}
declare namespace ut.JSTestHelper {
var TestSystemB: ut.SystemJS;
}
declare namespace ut.JSTestHelper {
var TestSystemC: ut.SystemJS;
}

declare namespace Module {

}


