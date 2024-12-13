local DrawableSprite = require("structs.drawable_sprite")
local Depths = require("consts.object_depths")

local TrapButton = {}

local directions = {
    "Up", "Down", "Left", "Right",
}

TrapButton.name = "Aqua/Trap Button"
TrapButton.depth = Depths.top
TrapButton.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    direction = {
        options = directions,
        editable = false,
    },
    group = {
        fieldType = "integer",
    },
}
TrapButton.placements = {
    {
        name = "Trap Button (Up)",
        data = {
            flag = "Trap1",
            color = "ff0000",
            direction = "Up",
        },
    },
    {
        name = "Trap Button (Down)",
        data = {
            flag = "Trap1",
            color = "ff0000",
            direction = "Down",
        },
    },
    {
        name = "Trap Button (Left)",
        data = {
            flag = "Trap1",
            color = "ff0000",
            direction = "Left",
        },
    },
    {
        name = "Trap Button (Right)",
        data = {
            flag = "Trap1",
            color = "ff0000",
            direction = "Right",
        },
    },
}

local BUTTON_TEXTURE = "objects/trap_button/button00"
local PEDESTAL_TEXTURE = "objects/trap_button/button_pedestal"

function TrapButton.sprite(room, entity)
    local button = DrawableSprite.fromTexture(BUTTON_TEXTURE, entity)
    button:setJustification(0.5, 0.0)
    button:setColor(entity.color)
    local pedestal = DrawableSprite.fromTexture(PEDESTAL_TEXTURE, entity)
    pedestal:setJustification(0.5, 0.0)    
    pedestal:setColor("ffffff")
    if entity.direction == "Down" then
        button.rotation = math.rad(180)
        pedestal.rotation = math.rad(180)
        pedestal:addPosition(0.0, -3.0)
    elseif entity.direction == "Left" then
        button.rotation = math.rad(-90)
        pedestal.rotation = math.rad(-90)
        pedestal:addPosition(3.0, 0.0)
    elseif entity.direction == "Right" then
        button.rotation = math.rad(90)
        pedestal.rotation = math.rad(90)
        pedestal:addPosition(-3.0, 0.0)
    else
        button.rotation = 0
        pedestal.rotation = 0
        pedestal:addPosition(0.0, 3.0)
    end
    return { button, pedestal, }
end

return TrapButton