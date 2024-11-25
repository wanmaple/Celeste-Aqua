local DrawableRectangle = require("structs.drawable_rectangle")
local Utils = require("utils")
local Entities = require("entities")

local AquaNPC = {}

AquaNPC.name = "Aqua/Custom NPC"
AquaNPC.depth = 100
AquaNPC.justification = {0.5, 1.0}
AquaNPC.nodeLimits = {0, 2}
AquaNPC.nodeLineRenderType = "line"
AquaNPC.fieldInformation = {
    spriteRate = {
        fieldType = "integer",
    },
    approachDistance = {
        fieldType = "integer",
    },
    indicatorOffsetX = {
        fieldType = "integer",
    },
    indicatorOffsetY = {
        fieldType = "integer",
    }
}

AquaNPC.placements = {
    {
        name = "Custom NPC",
        data = {
            sprite = "player/idle",
            spriteRate = 1,
            spriteName = "player",
            animationName = "idle",
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = 0,
        }
    },
    {
        name = "Theo",
        data = {
            sprite = "theo/theo",
            spriteRate = 10,
            spriteName = "theo",
            animationName = "idle",
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -5,
        }
    },
    {
        name = "Granny",
        data = {
            sprite = "oldlady/idle",
            spriteRate = 7,
            spriteName = "granny",
            animationName = "idle",
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -2
        }
    },
    {
        name = "Oshiro",
        data = {
            sprite = "oshiro/oshiro",
            spriteRate = 12,
            spriteName = "oshiro",
            animationName = "idle",
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -10,
        }
    },
    {
        name = "Badeline Boss",
        data = {
            sprite = "badelineBoss/boss",
            spriteRate = 17,
            spriteName = "badeline_boss",
            animationName = "idle",
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -5,
        }
    },
}

local borderColor = {1.0, 1.0, 1.0, 1.0}
local fillColor = {1.0, 1.0, 1.0, 0.8}

function AquaNPC.nodeSprite(room, entity, node, index)
    local rectangle = Utils.rectangle(node.x, node.y, 8, 8)
    return DrawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor):getDrawableSprite()
end

function AquaNPC.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1
    local scaleY = entity.flipY and -1 or 1

    return scaleX, scaleY
end

function AquaNPC.texture(room, entity)
    local spriteName = entity.sprite or ""

    if spriteName == "oshiro/oshiro" then
        return "characters/oshiro/oshiro31"
    end

    return string.format("characters/%s00", entity.sprite or "")
end

return AquaNPC
