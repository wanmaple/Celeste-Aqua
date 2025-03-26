local SpikeHelper = require('helpers.spikes')

local SPIKE_VARIANTS = {
    "default",
    "outline",
    "cliffside",
    "reflection",
    "tentacles",
}

local spikeUp = SpikeHelper.createEntityHandler("Aqua/Unhookable Spike Up", "up", false, false, SPIKE_VARIANTS)
local spikeDown = SpikeHelper.createEntityHandler("Aqua/Unhookable Spike Down", "down", false, false, SPIKE_VARIANTS)
local spikeLeft = SpikeHelper.createEntityHandler("Aqua/Unhookable Spike Left", "left", false, false, SPIKE_VARIANTS)
local spikeRight = SpikeHelper.createEntityHandler("Aqua/Unhookable Spike Right", "right", false, false, SPIKE_VARIANTS)
local spikes = { spikeUp, spikeDown, spikeLeft, spikeRight, }
for i, spike in ipairs(spikes) do
    for _, placement in ipairs(spike.placements) do
        placement.data["color"] = "ffffff"
        placement.data["attach"] = false
        placement.data["block_up"] = i ~= 2
        placement.data["block_down"] = i ~= 1
        placement.data["block_left"] = i ~= 4
        placement.data["block_right"] = i ~= 3
    end
    spike.fieldInformation["color"] = { fieldType = "color", useAlpha = false, }
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