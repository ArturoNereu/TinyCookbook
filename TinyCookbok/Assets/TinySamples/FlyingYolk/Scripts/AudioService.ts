namespace game {
  export class AudioService {

    /**
     * Helper method to play an audio clip by entity name
     */
    static playAudioSourceByName(world: ut.World, name: string) {
      let entity = world.getEntityByName(name);
      
      if (entity.isNone()) {
        console.error("game.AudioService.playAudioSourceByName: No entity with the name '" + name + "' was found")
        return;
      }

      AudioService.playAudioSource(world, entity);
    }

    /**
     * Helper method to play an audio clip
     */
    static playAudioSource(world: ut.World, entity: ut.Entity) {

      if (!world.hasComponent(entity, ut.Audio.AudioSource)) {
        console.error("game.AudioService.playAudioSource: Entity does not have an AudioSource component")
        return;
      }

      if (!world.hasComponent(entity, ut.Audio.AudioSourceStart))
        world.addComponent(entity, ut.Audio.AudioSourceStart);
    }
  }
}