namespace game {

  @ut.executeBefore(ut.Shared.RenderingFence)
  @ut.requiredComponents(game.NumberTextRenderer)
  @ut.optionalComponents(ut.Core2D.Sprite2DRenderer, ut.Core2D.TransformLocalPosition)
  export class NumberTextRenderingSystem extends ut.ComponentSystem {

    /**
     * Workaround system to draw numbers since text was not supported at the time
     */
    OnUpdate(): void {
      this.world.forEach([game.NumberTextRenderer],
         (numbertextrenderer) => {
          let value = numbertextrenderer.value;
          let spacing = numbertextrenderer.spacing;
          let alignment = numbertextrenderer.alignment;
          let renderers = numbertextrenderer.renderers;
          let characters = numbertextrenderer.characters;

          // resolve each digit
          let digits = [
            value % 10,
            Math.floor(value / 10),
            Math.floor(value / 100),
            Math.floor(value / 1000)
          ];

          // @todo
          // assert(digits.length >= renderers.length);

          // computer the number of characters to draw based on the value
          let count = renderers.length;
          for (let i = renderers.length - 1; i >= 1; i--) {
            if (digits[i] != 0) {
              break;
            }
            count = i;
          }

          let width = count * spacing;

          for (let i = 0; i < renderers.length; ++i) {
            let renderer = renderers[i];
            let spriteRenderer = this.world.getComponentData(renderer, ut.Core2D.Sprite2DRenderer);
            let color = spriteRenderer.color;

            if (i < count) {
              // digit is used; show it and position it
              color.a = 1;

              spriteRenderer.sprite = characters[digits[i]];

              let position;
              if (alignment == game.TextAlignment.Center) {
                position = new Vector3(spacing * (count - i - 1) - (width - spacing) * 0.5, 0, 0);
              } else {
                position = new Vector3(spacing * -i, 0, 0);
              }
              this.world.setComponentData(renderer, new ut.Core2D.TransformLocalPosition(position));
            } else {
              // digit is unused; hide it
              color.a = 0;
            }

            spriteRenderer.color = color;
            this.world.setComponentData(renderer, spriteRenderer);
          }
        });
    }
  }
}
