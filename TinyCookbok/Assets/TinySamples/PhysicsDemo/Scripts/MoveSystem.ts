namespace game {
    @ut.executeAfter(ut.Shared.UserCodeStart)
    @ut.executeBefore(ut.Shared.UserCodeEnd)
    @ut.requiredComponents(game.Move)
    @ut.requiredComponents(ut.Physics2D.Velocity2D)
    export class MoveSystem extends ut.ComponentSystem {
        OnUpdate(): void {
            this.world.forEach([ut.Entity, game.Move, ut.Physics2D.Velocity2D],
                (entity, move, velocity2d) => {
                    if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.A)) {
                        let setVelocity = new ut.Physics2D.SetVelocity2D;
                        setVelocity.velocity = new Vector2(-move.speed, velocity2d.velocity.y);
                        this.world.addComponentData(entity, setVelocity);
                    } else if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.D)) {
                        let setVelocity = new ut.Physics2D.SetVelocity2D;
                        setVelocity.velocity = new Vector2(move.speed, velocity2d.velocity.y);
                        this.world.addComponentData(entity, setVelocity);
                    }
                    if (ut.Runtime.Input.getKey(ut.Core2D.KeyCode.W) || ut.Runtime.Input.getMouseButton(0)) {
                        let impulse = new ut.Physics2D.AddImpulse2D;
                        impulse.impulse = new Vector2(0, move.upForce);
                        this.world.addComponentData(entity, impulse);
                    }
                });
        }
    }
}
