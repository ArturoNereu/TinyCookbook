# Particles Module

Tiny supports a subset of the particle workflows available in Unity. This subset is referred to as the Particles module. This document provides common use cases, examples, and a reference of Particles components.

[See this module's API documentation for more information](../api/Unity.Tiny.Particles.html)


## Use Cases and Examples

This section demonstrates how to use the Particles module through common use cases and usage examples. If a use case or an example is similar to a topic in the Unity User Manual, a link is provided to the appropriate topic.

### Simple Particle System

1. Create a [Sprite2D entity](./manual-module-core2d.md#simple-sprite-rendering). This will be your proto-particle.
2. Create an entity.
    1. Add a [ParticleEmitter](#particleemitter) component.
    2. Set the `particle` property to the Sprite2D entity.
    3. Add a [EmitterBoxSource](#emitterboxsource) component.

Modify the [ParticleEmitter](#particleemitter) or the [EmitterBoxSource](#emitterboxsource) properties to affect the emission (e.g. emission rate).

### Spawn particles with initial modifiers

After adding a particle system, it is easy to control the state of newly created particles. From the [ParticleEmitter](#particleemitter) component, you can add a related component using the `+` button.

1. Add a [EmitterInitialScale](#emitterinitialscale) component to set the scale of each particle on emission.
2. Add a [EmitterInitialRotation](#emitterinitialrotation) component to set the rotation around the z-axis of each particle on emission.
3. Add a [EmitterInitialVelocity](#emitterinitialvelocity) component to set the velocity (direction and speed) of each particle on emission.

For each of these components, it is possible to define the upper and lower bounds for the value; the actual value varies randomly between those bounds.

### Spawn particles with changes over time

After adding a particle system, it is easy to specify the particle's changes over time. From the [ParticleEmitter](#particleemitter) component, you can add a related component using the `+` button. Using a change over time component will most likely override any initial value component you provided.

#### Changing the color and alpha over time.
1. Create an empty entity and name it ColorOverTimeKeys.
    1. Add a component [KeyColor](#keycolor). The component holds the list of Color keyframes for the interpolation.
    2. Add two keys and set time 0.0 for the first one and 1.0 for the second one. Set different color/alpha values for both keys.
2. Create an empty entity and name it ColorOverTimeCurve.
    1. Add a component [LinearCurveColor](#linearcurvecolor).
    2. Drag and drop entity ColorOverTimeKeys into `keys` field.
3. Select an emitter entity.
    1. Add a [LifetimeColor](#lifetimecolor) component to the particle system entity in order to change the particle's color over time.
    2. Drag and drop ColorOverTimeCurve entity to `curve` field in [LifetimeColor](#lifetimecolor) component.

#### Changing the speed over time.
1. Create an empty entity and name it SpeedMultiplierOverTimeKeys.
    1. Add a component [KeyFloat](#keyfloat). The component holds the list of Float keyframes for the interpolation.
    2. Add two keys and set time 0.0 for the first one and 1.0 for the second one. Set `speed` to 5.0 for the first key and leave 0.0 fot the second.
2. Create an empty entity and name it SpeedMultiplierOverTimeCurve.
    1. Add a component [LinearCurveFloat](#linearcurvefloat).
    2. Drag and drop entity SpeedMultiplierOverTimeKeys into `keys` field.
3. Select an emitter entity.
    1. Add a [LifetimeSpeedMultiplier](#lifetimespeedmultiplier) component to the particle system entity in order to change the particle's speed over time.
    2. Drag and drop SpeedMultiplierOverTimeCurve entity to `curve` field in [LifetimeSpeedMultiplier](#lifetimespeedmultiplier) component.

## Components

Tiny is built around small, lightweight components. This way you only pay for what you need. This design usually creates more component types to represent data held in a single Unity component, which can be confusing at first for some developers.

> Use this table to associate familiar Unity concepts with Tiny.

| Unity Particle System Module | Particles Components                  |
| ---------------------------- | ------------------------------------- |
| Emission, Shape              | [EmitterBoxSource](#emitterboxsource), [EmitterCircleSource](#emittercirclesource), [EmitterConeSource](#emitterconesource) |
| Color Over Lifetime          | [LifetimeColor](#lifetimecolor)       |
| Rotation Over Lifetime       | [LifetimeAngularVelocity](#lifetimeangularvelocity) |
| Size Over Lifetime           | [LifetimeScale](#lifetimescale)       |
| Velocity Over Lifetime       | [LifetimeVelocity](#lifetimevelocity), [LifetimeSpeedMultiplier](#lifetimespeedmultiplier) |
| Particle System Main module  | [ParticleEmitter](#particleemitter)   |

| Unity Particle System Property    | Particles Components                              |
| --------------------------------- | ------------------------------------------------- |
| 3D Start Rotation, Start Rotation | [EmitterInitialRotation](#emitterinitialrotation), [EmitterInitialAngularVelocity][#emitterinitialangularvelocity] |
| Start Size                        | [EmitterInitialScale](#emitterinitialscale)       |
| Start Speed                       | [EmitterInitialVelocity](#emitterinitialvelocity) |

### EmitterBoxSource

* Requires: [ParticleEmitter](#particleemitter)
* Unity References: [Emission module](https://docs.unity3d.com/Manual/PartSysEmissionModule.html), [Shape module](https://docs.unity3d.com/Manual/PartSysShapeModule.html)

Change the Shape of emission to a Box with defined position and scale. Particles will be emitted from a random spot inside the box. The `rect` will be centered around the emitter's position.

> Defaults the emission Rate over Time (number of particles emitted per unit of time) to 10.

|Property|Description|
|--------|-----------|
|rect|The region of the entity to use as the box emitter source.  The position is set with the x and y values. The scaling is set with the width and height. Defaults width and height to 50.|
|attachToEmitter|Sets whether the emitted particle's transform will be a child of this emitter. <br> If true, the emission position is set as the entity's local position, and the particle will be added as a transform child. <br> If false, the emitter's world position will be added to the emission position, and that result is set as the local position.|

### EmitterCircleSource

* Requires: [ParticleEmitter](#particleemitter)

Change the Shape of emission to a Circle with defined radius and speed. Particles will be emitted from a random spot inside the circle with the random speed.

|Property|Description|
|--------|-----------|
|radius|Radius of the emitter|
|speed|Initial speed of the particles.|
|speedBasedOnRadius|If true, the initial speed is based on the initial particle's distance from the center of the emitter.|

### EmitterConeSource

* Requires: [ParticleEmitter](#particleemitter)

Change the Shape of emission to a cone with defined radius, angle, and speed. Particles will be emitted from a random spot inside the base of the cone with the random speed and angle.

|Property|Description|
|--------|-----------|
|radius|The radius of the base of the cone|
|angle|The angle of the cone|
|speed|Initial speed of the particles.|

### EmitterInitialRotation

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Particle System Main module](https://docs.unity3d.com/Manual/PartSysMainModule.html)

Sets the initial angle of each particle on emission.

> Since the particle system is only in 2D coordinates as of right now, the angle is around the z-axis.

|Property|Description|
|--------|-----------|
|angle|The initial rotation angle in **degrees** of each particle around the z-axis. If the start and end values are the same, the initial rotation is constant. Otherwise, the rotation is a random value between the start and end values.|

### EmitterInitialScale

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Particle System Main module](https://docs.unity3d.com/Manual/PartSysMainModule.html)

Sets the initial scale of each particle on emission.

|Property|Description|
|--------|-----------|
|scale|The initial scale of each particle. Defaults to 1. If the start and end values are the same, the initial scale is constant. Otherwise, the scale is a random value between the start and end values.|

### EmitterInitialVelocity

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Particle System Main module](https://docs.unity3d.com/Manual/PartSysMainModule.html)

Sets the initial velocity of each particle on emission. The length of the velocity vector stands for the initial speed of the particle.

|Property|Description|
|--------|-----------|
|velocity|The initial emission velocity of each particle from the chosen emission position.|

### LifetimeColor

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Color Over Lifetime module](https://docs.unity3d.com/Manual/PartSysColorOverLifeModule.html)

Specifies how a particle's color changes over its lifetime. It modifies the [Sprite2DRenderer](./manual-module-core2d.md#sprite2drenderer)'s color.

|Property|Description|
|--------|-----------|
|curve|A curve which defines the particle's color over its lifetime. In this case, the curve is a visual representation of a colour progression, which simply shows the main colours (which are called stops) and all the intermediate shades between them.|

### LifetimeAngularVelocity

* Requires: [ParticleEmitter](#particleemitter)

 Specifies how a particle's angular velocity changes over its lifetime. It modifies the [Transform](./manual-module-core2d.md#transform)'s local rotation.

> This component uses angular velocity in the editor and explicit rotation values in the runtime. In Unity, the rotation over lifetime indicates an angular velocity in degrees per second whereas in Tiny, the LifetimeRotation indicates the actual rotation value used at time x.

|Property|Description|
|--------|-----------|
|curve|A curve which defines the particle's angular velocity in **degrees** around the z-axis over its lifetime. In this case, the curve is a line graph that sets the angular velocity over time.|

### LifetimeScale

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Size Over Lifetime module](https://docs.unity3d.com/Manual/PartSysSizeOverLifeModule.html)

Specifies how a particle's scale changes over its lifetime. It modifies the [Transform](./manual-module-core2d.md#transform)'s scale with a uniform scale (on x, y, z axis).

|Property|Description|
|--------|-----------|
|curve|A curve which defines how the particle's scale changes over its lifetime. In this case, the curve is a line graph that sets the scale over time.|

### LifetimeVelocity

* Requires: [ParticleEmitter](#particleemitter)
* Unity Reference: [Velocity Over Lifetime module](https://docs.unity3d.com/Manual/PartSysVelOverLifeModule.html)

Specifies how a particle's velocity (direction and speed) changes over its lifetime.

|Property|Description|
|--------|-----------|
|curve|A curve which defines how the particle's velocity changes over its lifetime. The net velocity used for modyfying the particle's position is calculated by adding the initial velocity and the velocity over time.|

### BurstEmission

* Requires: [ParticleEmitter](#particleemitter)

An emitter with this component emits particles in bursts. A burst is a particle event where a number of particles are all emitted at the same time. A cycle is a single occurrence of a burst.

|Property|Description|
|--------|-----------|
|count|How many particles in every cycle.|
|interval|The interval between cycles, in seconds.|
|cycles|How many times to play the burst|

### ParticleEmitter

* Requires: [UTiny.Core2D](./manual-module-core2d.md), UTiny.Math
* Unity References: [Graphics.Particle Systems](https://docs.unity3d.com/Manual/ParticleSystems.html), [Particle System Main module](https://docs.unity3d.com/Manual/PartSysMainModule.html)

**This is the core particle emitter component.** When added to an entity, it becomes an emitter with given characteristics. The system continuously emits particles (loop). It is linked to the source data, the initial values and the lifetime values for particles. It contains global properties that affect the whole system (e.g. `lifetime` property). Various particle modifiers after initial emit can be added as component (e.g. [EmitterInitialScale](#emitterinitialscale)).

> The Unity `Prewarm`'s property is enabled if the prewarmPercent is equal or more than 0.5.

|Property|Description|
|--------|-----------|
|particle|The UTiny entity attached to the particle system and rendered in run-time. This proto-particle is used as a template.|
|maxParticles|The maximum number of particles in the system at once. If the limit is reached, some particles are removed. Defaults to 1000.|
|emitRate|The number of particles emitted per **second**. Defaults to 10.|
|lifetime|The lifetime of each particle in **seconds**. Defaults start (min) and end (max) to 5.|
|attachToEmitter|If true, then newly spawned particles will be children of the emitter.|

## Systems

### ParticleSystem

* Updates Before: Shared.InputFence, Shared.RenderingFence

The main entry system for particles. Spawns particles with given initial characteristics and updates particles' properties ([scale](#lifetimescale), [color](#lifetimecolor), [alpha](#lifetimealpha), [rotation](#lifetimerotation)) over time.

[See this module's API documentation for more information](../api/Unity.Tiny.Particles.html)