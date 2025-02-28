local DrawableNinePatch = require("structs.drawable_nine_patch")
local Utils = require("utils")

local AquaCrumblePlatform = {}

local DEFAULT_TEXTURES = {
    "default",
    "cliffside",
}

AquaCrumblePlatform.name = "Aqua/Aqua Crumble Platform"
AquaCrumblePlatform.depth = 0
AquaCrumblePlatform.fieldInformation = {
    platform_texture = {
        options = DEFAULT_TEXTURES,
    },
    crumble_particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
AquaCrumblePlatform.placements = {}

for _, texture in ipairs(DEFAULT_TEXTURES) do
    table.insert(AquaCrumblePlatform.placements, {
        name = string.format("Crumble Platform (%s)", texture),
        data = {
            width = 8,
            platform_texture = texture,
            outline_texture = "objects/crumbleBlock/outline",
            one_use = false,
            respawn_duration = 2.0,
            min_crumble_duration_on_top = 0.2,
            max_crumble_duration_on_top = 0.6,
            crumble_duration_on_side = 1.0,
            crumble_duration_attached = 1.0,
            crumble_particle_color = "847e87",
        }
    })
end

local NINE_PATCH_OPTIONS = {
    mode = "fill",
    fillMode = "repeat",
    border = 0,
}

function AquaCrumblePlatform.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = math.max(entity.width or 0, 8)
    local variant = entity.platform_texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local sprite = DrawableNinePatch.fromTexture(texture, NINE_PATCH_OPTIONS, x, y, width, 8)
    return sprite
end

function AquaCrumblePlatform.selection(room, entity)
    return Utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
end

return AquaCrumblePlatform