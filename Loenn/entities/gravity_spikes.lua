local SpikeHelper = require('helpers.spikes')

local SPIKE_VARIANTS = {
    "default",
    "outline",
}

local DISABLE_TYPES = {
    "outline_disable",
}

local GRAVITY_OPTIONS = {
    "Normal",
    "Inverted",
}

local spikeUp = SpikeHelper.createEntityHandler("Aqua/Gravity Spike Up", "up", false, false, SPIKE_VARIANTS)
local spikeDown = SpikeHelper.createEntityHandler("Aqua/Gravity Spike Down", "down", false, false, SPIKE_VARIANTS)
local spikeLeft = SpikeHelper.createEntityHandler("Aqua/Gravity Spike Left", "left", false, false, SPIKE_VARIANTS)
local spikeRight = SpikeHelper.createEntityHandler("Aqua/Gravity Spike Right", "right", false, false, SPIKE_VARIANTS)
local spikes = { spikeUp, spikeDown, spikeLeft, spikeRight, }
for i, spike in ipairs(spikes) do
    for _, placement in ipairs(spike.placements) do
        for k, v in pairs(placement.data) do
            print("##############", k)
        end
        placement.data["color"] = "ffffff"
        placement.data["gravity"] = "Normal"
        placement.data["disable_type"] = "outline_disable"
    end
    spike.fieldInformation["color"] = { fieldType = "color", useAlpha = false, }
    spike.fieldInformation["gravity"] = { options = GRAVITY_OPTIONS, editable = false, }
    spike.fieldInformation["disable_type"] = { options = DISABLE_TYPES, editable = true, }
    local oldSpriteFunc = spike.sprite
    spike.sprite = function(room, entity) 
        local sprites = oldSpriteFunc(room, entity)
        for _, sprite in ipairs(sprites) do
            local color = entity.color or "ffffff"
            sprite:setColor(color)
        end
        return sprites
    end
end

return spikes