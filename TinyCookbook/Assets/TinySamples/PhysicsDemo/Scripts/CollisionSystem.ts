namespace game {
    @ut.executeAfter(ut.Shared.UserCodeStart)
    @ut.executeBefore(ut.Shared.UserCodeEnd)
    @ut.requiredComponents(ut.Physics2D.ColliderContacts)
    @ut.requiredComponents(ut.Core2D.Sprite2DRenderer)
    export class CollisionSystem extends ut.ComponentSystem {
        OnUpdate(): void {
            this.world.forEach([ut.Physics2D.ColliderContacts, ut.Core2D.Sprite2DRenderer],
                (collidercontacts, sprite2drenderer) => {
                    let contacts = collidercontacts.contacts;

                    if (contacts.length > 0) {
                        if (this.world.getEntityName(contacts[0]) == "LeftWall1") {
                            let color = new ut.Core2D.Color(1, 179 / 255, 179 / 255, 1);
                            sprite2drenderer.color = color;
                        }
                        else if (this.world.getEntityName(contacts[0]) == "Ball2") {
                            let color = new ut.Core2D.Color(129 / 255, 228 / 255, 129 / 255, 1);
                            sprite2drenderer.color = color;
                        }
                    }
                });
        }
    }
}
