local DrawableSprite = require("structs.drawable_sprite")

local AquaRefill = {}

AquaRefill.name = "Aqua/Aqua Refill"
AquaRefill.depth = -100
AquaRefill.placements = {
    name = "Refill",
    data = {
        twoDash = false,
        oneUse = false,
        hookable = true,
    },
}

function AquaRefill.texture(room, entity)
    if entity.hookable then
        return entity.twoDash and "objects/refill_two/idle00" or "objects/refill_one/idle00"
    end
    return entity.twoDash and "objects/refillTwo/idle00" or "objects/refill/idle00"
end

return AquaRefill