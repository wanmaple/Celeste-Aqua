local DrawableSprite = require("structs.drawable_sprite")
local Depths = require("consts.object_depths")

local TieRod = {}

TieRod.name = "Aqua/Tie Rod"
TieRod.depth = Depths.top
TieRod.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    }
}
TieRod.placements = {
    name = "Tie Rod",
    data = {
        flag = "Rod1",
        color = "ff0000",
    }
}

local TEXTURE_BOTTOM = "objects/tie_rod/tie_rod00"
local TEXTURE_TOP = "objects/tie_rod/tie_rod_top00"

function TieRod.sprite(room, entity)
    local bottom = DrawableSprite.fromTexture(TEXTURE_BOTTOM, entity)
    bottom:setColor("ffffff")
    local top = DrawableSprite.fromTexture(TEXTURE_TOP, entity)
    top:setColor(entity.color or "ff0000")
    return { bottom, top, }
end

return TieRod