
namespace game {

    /**
     * Make a tiled background that moves and repeats itself endlessly.
     */
    export class AnimateTilingBackgroundSystem extends ut.ComponentSystem {
        
        OnUpdate():void {

            let deltaTime = this.scheduler.deltaTime();
            this.world.forEach([ut.Entity, game.TilingBackground], (entity, tilingBackground) => {
                if (this.world.hasComponent(entity, ut.UILayout.RectTransform)) {
                    let rectTransform = this.world.getComponentData(entity, ut.UILayout.RectTransform);
                    let position = rectTransform.anchoredPosition;
                    position = this.movePosition(position, deltaTime, tilingBackground);
                    rectTransform.anchoredPosition = position;
                    this.world.setComponentData(entity, rectTransform);
                }
                else if (this.world.hasComponent(entity, ut.Core2D.TransformLocalPosition)) {
                    let transformPosition = this.world.getComponentData(entity, ut.Core2D.TransformLocalPosition);
                    let position = new Vector2(transformPosition.position.x, transformPosition.position.y);
                    position = this.movePosition(position, deltaTime, tilingBackground);
                    transformPosition.position.x = position.x;
                    transformPosition.position.y = position.y;
                    this.world.setComponentData(entity, transformPosition);
                }
            });
        }

        movePosition(position: Vector2, deltaTime: number, tilingBackground: TilingBackground): Vector2 {

            position.x += tilingBackground.SpeedX * deltaTime;
            position.y += tilingBackground.SpeedY * deltaTime;

            if (tilingBackground.SpeedX != 0 && position.x < -tilingBackground.TileSize) {
                position.x += tilingBackground.TileSize;
            }
            else if (tilingBackground.SpeedX != 0 && position.x > tilingBackground.TileSize) {
                position.x -= tilingBackground.TileSize;
            }

            if (tilingBackground.SpeedY != 0 && position.y < -tilingBackground.TileSize) {
                position.y += tilingBackground.TileSize;
            }
            else if (tilingBackground.SpeedY != 0 && position.y > tilingBackground.TileSize) {
                position.y -= tilingBackground.TileSize;
            }

            return position;
        }
    }
}
