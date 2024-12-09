local DrawableSprite = require("structs.drawable_sprite")
local Depths = require("consts.object_depths")

local TieRod = {}

TieRod.name = "Aqua/Tie Rod"
TieRod.texture = "objects/tie_rod/tie_rod00"
TieRod.depth = Depths.top
TieRod.fieldInformation = {
    group = {
        fieldType = "integer",
    },
}
TieRod.placements = {
    name = "Tie Rod",
    data = {
        group = 0,
    }
}

return TieRod