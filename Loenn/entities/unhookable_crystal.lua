local DrawableSpriteStruct = require("structs.drawable_sprite")
local Utils = require("utils")

local spinnerConnectionDistanceSquared = 24 * 24
local dustEdgeColor = {1.0, 0.0, 0.0}

local FOREGROUND_TEXTURE = "danger/crystal/fg_white00"
local BACKGROUND_TEXTURE = "danger/crystal/bg_white00"

local UnhookableCrystal = {}
UnhookableCrystal.name = "Aqua/Unhookable Crystal"
UnhookableCrystal.texture = FOREGROUND_TEXTURE
UnhookableCrystal.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
}
UnhookableCrystal.placements = {
    name = "Unhookable Crystal",
    data = {
        attachToSolid = false,
        custom_foreground_texture = "",
        custom_background_texture = "",
        color = "05ecfb",
    },
}

local function getSpinnerSprite(entity, foreground)
    local color = entity.color
    local position = {
        x = entity.x,
        y = entity.y
    }

    local texture = foreground and FOREGROUND_TEXTURE or BACKGROUND_TEXTURE
    local sprite = DrawableSpriteStruct.fromTexture(texture, position)
    sprite:setColor(color)
    return sprite
end

local function getConnectionSprites(room, entity)
    local sprites = {}
    for _, target in ipairs(room.entities) do
        if target == entity then
            break
        end

        if (target._name == "spinner" or target._name == entity._name) and not target.dust and entity.attachToSolid == target.attachToSolid then
            if Utils.distanceSquared(entity.x, entity.y, target.x, target.y) < spinnerConnectionDistanceSquared then
                local connectorData = {
                    x = math.floor((entity.x + target.x) / 2),
                    y = math.floor((entity.y + target.y) / 2),
                    color = entity.color
                }
                local sprite = getSpinnerSprite(connectorData, false)
                sprite.depth = -8499
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function UnhookableCrystal.depth(room, entity)
    return -8500
end

function UnhookableCrystal.sprite(room, entity)
    local sprites = getConnectionSprites(room, entity)
    local mainSprite = getSpinnerSprite(entity, true)
    table.insert(sprites, mainSprite)
    return sprites
end

function UnhookableCrystal.selection(room, entity)
    return Utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return UnhookableCrystal