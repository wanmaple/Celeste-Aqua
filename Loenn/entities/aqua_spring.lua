local DrawableSprite = require("structs.drawable_sprite")

local AquaSpring = {}

local ORIENTATIONS = {
    "Up", "Down", "Left", "Right",
}
local ROTATIONS = {
    0.0, math.pi, -math.pi * 0.5, math.pi * 0.5
}

AquaSpring.name = "Aqua/Aqua Spring"
AquaSpring.depth = -8501
AquaSpring.justification = {0.5, 1.0}
AquaSpring.texture = "objects/spring/00"
AquaSpring.fieldInformation = {
    orientation = {
        options = ORIENTATIONS,
        editable = false,
    }
}
AquaSpring.placements = {}

for _, orientation in ipairs(ORIENTATIONS) do
    table.insert(AquaSpring.placements, {
        name = string.format("Spring (%s)", orientation),
        data = {
            playerCanUse = true,
            orientation = orientation,
        },
    })
end

function AquaSpring.rotation(room, entity)
    if entity.orientation == "Up" then
        return 0.0
    elseif entity.orientation == "Down" then
        return math.pi
    elseif entity.orientation == "Left" then
        return -math.pi * 0.5
    elseif entity.orientation == "Right" then
        return math.pi * 0.5
    end
    return 0.0
end

return AquaSpring