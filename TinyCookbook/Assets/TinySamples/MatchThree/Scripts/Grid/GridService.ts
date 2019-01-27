
namespace game {

    export class GridService {

        static getGridConfiguration(world: ut.World): game.GridConfiguration {
            return world.getConfigData(game.GridConfiguration);
        }

        static clear(world: ut.World, grid: GridConfiguration) {
            grid.IsGridCreated = false;

            grid.CellEntities = new Array(grid.Width * grid.Height);
            for (let i = 0; i < grid.CellEntities.length; i++) {
                grid.CellEntities[i] = new ut.Entity;
            }

            grid.GemEntities = new Array(grid.Width * grid.Height);
            for (let i = 0; i < grid.GemEntities.length; i++) {
                grid.GemEntities[i] = new ut.Entity;
            }

            world.setConfigData(grid);
        };

        static isGridFrozen(world: ut.World): boolean {
            return (this.getGridConfiguration(world).FrozenGridTimer > 0);
        }

        static getCellAt(world: ut.World, grid: GridConfiguration, x: number, y: number): game.Cell {
            let hashCode = this.getCellHashCode(grid, x, y);
            if (world.exists(grid.CellEntities[hashCode])) {
                return world.getComponentData(grid.CellEntities[hashCode], game.Cell);
            }
            else {
                return null;
            }
        }

        static getCellHashCode(grid: GridConfiguration, x: number, y: number): number {
            return x * (grid.Width + 1) + y;
        }

        static getPositionFromCellHashCode(grid: GridConfiguration, hashCode: number): ut.Math.Vector2 {
            let width = (grid.Width + 1);
            return new Vector2(Math.floor(hashCode / width), hashCode % width);
        }

        static getCellWorldPosition(grid: GridConfiguration, cell: game.Cell): ut.Math.Vector3 {
            return this.getGridToWorldPosition(grid, cell.X, cell.Y);
        }

        static getGridToWorldPosition(grid: GridConfiguration, x: number, y: number): ut.Math.Vector3 {
            let cellSize = grid.CellDimension;
            let gridWidth = cellSize * grid.Width;
            let gridHeight = cellSize * grid.Height;
            let position = new Vector3(
                x * cellSize - gridWidth / 2 + cellSize / 2,
                grid.GridDefaultPositionY + grid.GridOffsetPositionY + y * cellSize - gridHeight / 2 + cellSize / 2,
                0);

            return position;
        }

        static createGridCells(world: ut.World, grid: GridConfiguration) {
            this.clear(world, grid);

            for (let i = 0; i < grid.Width; i++) {
                for (let j = 0; j < grid.Height; j++) {
                    let cell = this.getCellAt(world, grid, i, j);
                    if (cell == null) {
                        let entity = this.createCell(grid, world, i, j);
                        cell = world.getComponentData(entity, game.Cell);

                        let transformLocalPosition = world.getComponentData(entity, ut.Core2D.TransformLocalPosition);
                        let position = this.getCellWorldPosition(grid, cell);
                        transformLocalPosition.position.x = position.x;
                        transformLocalPosition.position.y = position.y;
                        world.setComponentData(entity, transformLocalPosition);

                        let spriteRendererOptions = world.getComponentData(entity, ut.Core2D.Sprite2DRendererOptions);
                        spriteRendererOptions.size = new Vector2(cell.Size, cell.Size);
                        world.setComponentData(entity, spriteRendererOptions);

                        let spriteName = "Center";
                        if (i == 0 && j == 0) {
                            spriteName = "BottomLeft";
                        }
                        else if (i == grid.Width - 1 && j == 0) {
                            spriteName = "BottomRight";
                        }
                        else if (i == grid.Width - 1 && j == grid.Height - 1) {
                            spriteName = "TopRight";
                        }
                        else if (i == 0 && j == grid.Height - 1) {
                            spriteName = "TopLeft";
                        }
                        else if (j == 0) {
                            spriteName = "Bottom";
                        }
                        else if (i == grid.Width - 1) {
                            spriteName = "Right";
                        }

                        let path = "assets/sprites/Cells/" + spriteName;
                        let spriteRenderer = world.getComponentData(entity, ut.Core2D.Sprite2DRenderer);
                        spriteRenderer.sprite = world.getEntityByName(path);
                        world.setComponentData(entity, spriteRenderer);
                    }
                }
            }

            world.setConfigData(grid);
        }

        static createCell(grid: GridConfiguration, world: ut.World, x: number, y: number): ut.Entity {
            let cellEntity = ut.EntityGroup.instantiate(world, "game.Cell")[0];

            let cell = world.getComponentData(cellEntity, game.Cell);
            cell.X = x;
            cell.Y = y;
            world.setComponentData(cellEntity, cell);
            
            grid.CellEntities[this.getCellHashCode(grid, x, y)] = cellEntity;
        
            return cellEntity;
        };
    }
}
