local FakeTilesHelper = require("helpers.fake_tiles")

local UnidirectionalDashBlock = {}

UnidirectionalDashBlock.name = "Aqua/Unidirectional Dash Block"
UnidirectionalDashBlock.depth = 0

UnidirectionalDashBlock.placements = {
    name = "Unidirectional Dash Block",
    data = {
        tiletype = FakeTilesHelper.getPlacementMaterial(),
        blendin = true,
        canDash = true,
        permanent = true,
        width = 8,
        height = 8,
        direction = "All",  -- All/Left/Right/Up/Down
    },
}

UnidirectionalDashBlock.sprite = FakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")
UnidirectionalDashBlock.fieldInformation = FakeTilesHelper.getFieldInformation("tiletype")

return UnidirectionalDashBlock