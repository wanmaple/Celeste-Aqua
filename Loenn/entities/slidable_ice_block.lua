local DrawableNinePatch = require("structs.drawable_nine_patch")
local Depths = require("consts.object_depths")

local SlidableIceBlock = {}
SlidableIceBlock.name = "Aqua/Slidable Ice Block"
SlidableIceBlock.depth = 8995
SlidableIceBlock.placements = {
    name = "Slidable Ice Block",
    data = {
        width = 24,
        height = 24,
        hook_smooth = 2.5,
    },
}

return SlidableIceBlock