local DrawableSprite = require("structs.drawable_sprite")

local GravityBooster = {}

local GRAVITY_TYPES = {
    "Normal",
    "Inverted",
    "Toggle",
}

local HIGHLIGHT_COLORS = { "007cff", "dc1828", "ca41f5", }

local INTERNAL_SKINS = {
    "Aqua_BoosterOrange",
    "Aqua_BoosterPurple",
}

GravityBooster.name = "Aqua/Gravity Booster"
GravityBooster.depth = -8500
GravityBooster.fieldInformation = {
    sprite = {
        options = INTERNAL_SKINS,
        editable = true,
    },
    gravity_type = {
        options = GRAVITY_TYPES,
        editable = false,
    },
}
GravityBooster.placements = {}
for _, gravityType in ipairs(GRAVITY_TYPES) do
    table.insert(GravityBooster.placements, {
        name = string.format("Gravity Booster (Orange, %s)", gravityType),
        data = {
            red = false,
            hookable = true,
            sprite = "Aqua_BoosterOrange",
            gravity_type = gravityType,
        },
    })
    table.insert(GravityBooster.placements, {
        name = string.format("Gravity Booster (Purple, %s)", gravityType),
        data = {
            red = true,
            hookable = true,
            sprite = "Aqua_BoosterPurple",
            gravity_type = gravityType,
        },
    })
end

function GravityBooster.sprite(room, entity)
    local bubbleTexture = nil
    if entity.hookable then
        bubbleTexture = entity.red and "objects/booster_purple/booster_red00" or "objects/booster_orange/booster00"
    else
        bubbleTexture = entity.red and "objects/booster/boosterRed00" or "objects/booster/booster00"
    end
    local overlayTexture = "objects/GravityHelper/gravityBooster/" .. (entity.gravity_type == "Toggle" and "overlayToggle01" or entity.gravity_type == "Inverted" and "overlayInvert00" or "overlayNormal00")
    local bubble = DrawableSprite.fromTexture(bubbleTexture, entity)
    local overlay = DrawableSprite.fromTexture(overlayTexture, entity)
    local sprites = { bubble, overlay, }
    
    local function createRippleSprite(scaleY)
        local rippleSprite = DrawableSprite.fromTexture("objects/GravityHelper/ripple03", entity)
        local offset = scaleY < 0 and 5 or -5
        rippleSprite:addPosition(0, offset)
        rippleSprite:setColor(HIGHLIGHT_COLORS[entity.gravity_type])
        rippleSprite:setScale(1, scaleY)
        return rippleSprite
    end

    if entity.gravity_type == "Normal" or entity.gravity_type == "Toggle" then
        table.insert(sprites, createRippleSprite(-1.0))
    end
    if entity.gravity_type == "Inverted" or entity.gravity_type == "Toggle" then
        table.insert(sprites, createRippleSprite(1.0))
    end
    return sprites
end

return GravityBooster