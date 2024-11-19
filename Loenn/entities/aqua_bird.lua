local AquaBird = {}

AquaBird.name = "Aqua/Aqua Bird"
AquaBird.depth = -1000000
AquaBird.nodeLineRenderType = "line"
AquaBird.justification = { 0.5, 1.0 }
AquaBird.texture = "characters/bird/crow00"
AquaBird.nodeLimits = { 0, -1 }
AquaBird.fieldInformation = {
    startupIndex = {
        fieldType = "integer",
    }
}
AquaBird.placements = {
    name = "Tutorial Bird",
    data = {
        birdId = "",
        controls = "UpRight,+,mod:Aqua/ThrowHook;HOLD,Grab,+,Left,Right,tinyarrow,Jump",
        dialogs = "THROW_HOOK;SWING_JUMP",
        startupIndex = 0,
        triggerOnce = true,
        faceLeft = true,
        caw = true,
        onlyOnce = false,
    },
}

function AquaBird.scale(room, entity)
    return AquaBird.placements.data.faceLeft and -1 or 1, 1
end

return AquaBird