local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")
local Depths = require("consts.object_depths")
local FakeTilesHelper = require("helpers.fake_tiles")

local UnhookableTileController = {}

UnhookableTileController.name = "Aqua/Unhookable Tile Controller"
UnhookableTileController.fieldInformation = {}
UnhookableTileController.fieldInformation = FakeTilesHelper.addTileFieldInformation(UnhookableTileController.fieldInformation, "block_tile1")
UnhookableTileController.fieldInformation = FakeTilesHelper.addTileFieldInformation(UnhookableTileController.fieldInformation, "block_tile2")
UnhookableTileController.fieldInformation = FakeTilesHelper.addTileFieldInformation(UnhookableTileController.fieldInformation, "block_tile3")
UnhookableTileController.fieldInformation = FakeTilesHelper.addTileFieldInformation(UnhookableTileController.fieldInformation, "block_tile4")
UnhookableTileController.placements = {
    name = "Unhookable Tile Controller",
    data = {
        block_tile1 = FakeTilesHelper.getPlacementMaterial(),
        block_tile2 = FakeTilesHelper.getPlacementMaterial(),
        block_tile3 = FakeTilesHelper.getPlacementMaterial(),
        block_tile4 = FakeTilesHelper.getPlacementMaterial(),
        activate1 = true,
        activate2 = false,
        activate3 = false,
        activate4 = false,
        clear = false,
    },
}

function UnhookableTileController.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor, textColor = "0cfefe88", "ffffff", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    -- local textDrawable = DrawableText.fromText("Unhookable Tile Controller", x, y, width, height, nil, Triggers.triggerFontSize, textColor)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    -- table.insert(drawables, textDrawable)

    -- textDrawable.depth = Depths.triggers - 1

    return drawables
end

return UnhookableTileController