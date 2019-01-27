namespace game {

  @ut.executeAfter(ut.Shared.UserCodeStart)
  @ut.executeBefore(ut.Shared.UserCodeEnd)
  export class AudioInputSystem extends ut.ComponentSystem {

    OnUpdate(): void {

      // On iPhone, you need to interact at least once to play audio clips
      if (ut.Runtime.Input.getMouseButtonDown(0)) {
        this.playClip("Walk");
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.T)) {
        this.playClip("TreeWind");
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.B)) {
        this.playClip("Breeze");
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.W)) {
        this.playClip("Walk");
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.S)) {
        // Stop all AudioSources except BackgroundMusic
        this.world.forEach([ut.Entity, ut.Audio.AudioSource], (entity, audioSource) => {
          let name = this.world.getEntityName(entity);
          if (name != "BackgroundMusic") {
            this.stopClip(name);
          }
        });
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.P)) {
        // Pause all AudioSources
        this.world.forEach([ut.Audio.AudioConfig], (audioConfig) => {
          audioConfig.paused = true;
        });
      }

      if (ut.Runtime.Input.getKeyDown(ut.Core2D.KeyCode.R)) {
        // Resume all AudioSources
        this.world.forEach([ut.Audio.AudioConfig], (audioConfig) => {
          audioConfig.paused = false;
        });
      }
    }

    // Play an AudioClip
    private playClip(audioSourceEntityName: string): void {
      let audioSourceEntity = this.world.getEntityByName(audioSourceEntityName);
      if (!this.world.hasComponent(audioSourceEntity, ut.Audio.AudioSourceStart)) {
        this.world.addComponent(audioSourceEntity, ut.Audio.AudioSourceStart);
      }
    }

    // Stop an AudioClip
    private stopClip(audioSourceEntityName: string): void {
      let audioSourceEntity = this.world.getEntityByName(audioSourceEntityName);
      if (!this.world.hasComponent(audioSourceEntity, ut.Audio.AudioSourceStop)) {
        this.world.addComponent(audioSourceEntity, ut.Audio.AudioSourceStop);
      }
    }

  }

}
