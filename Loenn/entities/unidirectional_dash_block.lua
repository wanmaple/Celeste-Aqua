local FakeTilesHelper = require("helpers.fake_tiles")

local UnidirectionalDashBlock = {}

UnidirectionalDashBlock.name = "Aqua/Unidirectional Dash Block"
UnidirectionalDashBlock.depth = 0

UnidirectionalDashBlock.placements = {
    name = "Dash Block (Unidirectional)",
    data = {
        tiletype = FakeTilesHelper.getPlacementMaterial(),
        blendin = true,
        canDash = true,
        permanent = true,
        direction = "All",  -- All/Left/Right/Up/Down
        width = 8,
        height = 8,
    },
}

UnidirectionalDashBlock.sprite = FakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
UnidirectionalDashBlock.fieldInformation = FakeTilesHelper.getFieldInformation("tiletype")

return UnidirectionalDashBlock